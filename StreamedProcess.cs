using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

public class StreamedProcess {
	public Process process = null;
	StreamWriter messageStream;

	public int index = -1;
	public int GUID = -1;

	public string execPath = "";
	public string execArgs = "";
	public string workingPath = "";

	private bool applicationIsExiting = false;

	public void Execute(string path, string args, string workingpath) {
		execPath = path.Trim();
		execArgs = args.Trim();
		workingPath = workingpath.Trim();
		startProcess();
	}

	// https://stackoverflow.com/questions/12640943/run-process-start-on-a-background-thread
	// https://stackoverflow.com/questions/611094/async-process-start-and-wait-for-it-to-finish

	public void StdIn(string msg) {
		// incase the user is a clever bunny and wants to put in multiple lines, let's help her
		foreach ( string line in msg.Split('\n') ) {
			stdInStream.WriteLine(line.Trim());
			stdInStream.Flush(); // let's flush just in case
		}
	}

	public delegate void StreamedProcessMsgHandler(StreamedProcess process, string message);

	public delegate void StreamedProcessExitHandler(StreamedProcess process, EventArgs eventArgs);

	public StreamedProcessMsgHandler StdOut;
	public StreamedProcessMsgHandler StdErr;
	public StreamedProcessExitHandler ProcessExited;

	private StreamWriter stdInStream;
	private StreamReader stdOutStream;
	private StreamReader stdErrStream;

	void startProcess() {
		try {
			process = new Process();
			process.StartInfo.FileName = execPath;
			if ( execArgs.Length > 0 ) {
				process.StartInfo.Arguments = execArgs;
			}
			if ( workingPath.Length > 0 ) {
				process.StartInfo.WorkingDirectory = workingPath;
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.StandardOutputEncoding = Encoding.ASCII;
			process.StartInfo.StandardErrorEncoding = Encoding.ASCII;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;

			process.Exited += processExited;

			process.Start();

			stdInStream = new StreamWriter(process.StandardInput.BaseStream, Encoding.ASCII);
			//stdInStream.AutoFlush = true;

			stdOutStream = new StreamReader(process.StandardOutput.BaseStream, Encoding.ASCII);
			stdOutThread = new Thread(stdOutReader);
			stdOutThread.Start();

			stdErrStream = new StreamReader(process.StandardError.BaseStream, Encoding.ASCII);
			stdErrThread = new Thread(stdErrReader);
			stdErrThread.Start();

			if ( GUID == -1 ) {
				Debug.Log("Successfully launched app #" + index + ": " + execPath);
			}
		} catch ( Exception e ) {
			Debug.LogError("Unable to launch app #" + index + ": " + execPath + " : " + e.Message);
		}
	}

	public void RestartProcess() {
		if ( process != null && !process.HasExited ) {
			process.Kill();
			process.Close();
			process.Dispose();
			process = null;
		} else {
			startProcess();
		}
	}

	private readonly Queue<EventArgs> ProcessExitedQueue = new Queue<EventArgs>();
	private readonly Queue<string> StdOutQueue = new Queue<string>();
	private readonly Queue<string> StdErrQueue = new Queue<string>();

	private Thread stdOutThread;
	private Thread stdErrThread;

	void stdOutReader() {
		// if you're reading this, something is probably broken? 
		// make sure your client process flushes.. python at least needed a manual flush

		StringBuilder stringout = new StringBuilder();
		while ( !applicationIsExiting ) {
			while ( stdOutStream.Peek() > -1 ) {
				char c = (char)stdOutStream.Read();
				if ( c == '\n' ) {
					string line = stringout.ToString();
					if ( StdOut != null ) {
						lock ( StdOutQueue ) {
							StdOutQueue.Enqueue(line);
						}
						stringout = new StringBuilder();
					} else {
						Debug.Log("Unhandled StdOut: " + line);
					}
				} else {
					stringout.Append(c);
				}
			}
			Thread.Sleep(100);
		}
	}

	void stdErrReader() {
		StringBuilder stringout = new StringBuilder();
		while ( !applicationIsExiting ) {
			while ( stdErrStream.Peek() > -1 ) {
				char c = (char)stdErrStream.Read();
				if ( c == '\n' ) {
					string line = stringout.ToString();
					if ( StdErr != null ) {
						lock ( StdErrQueue ) {
							StdErrQueue.Enqueue(line);
						}
						stringout = new StringBuilder();
					} else {
						Debug.LogWarning("Unhandled StdErr: " + line);
					}
				} else {
					stringout.Append(c);
				}
			}
			Thread.Sleep(100);
		}
	}

	public void Flush() { // called from main thread to flush output queues
		lock ( StdOutQueue ) {
			while ( StdOutQueue.Count > 0 ) { // get it all out
				//if ( StdOutQueue.Count > 0 ) { // spread it around
				StdOut(this, StdOutQueue.Dequeue());
			}
		}
		lock ( StdErrQueue ) {
			while ( StdErrQueue.Count > 0 ) { // get it all out
				//if ( StdErrQueue.Count > 0 ) { // spread it around
				StdErr(this, StdErrQueue.Dequeue());
			}
		}
		lock ( ProcessExitedQueue ) {
			while ( ProcessExitedQueue.Count > 0 ) { // get it all out
				//if ( ProcessExitedQueue.Count > 0 ) { // spread it around
				ProcessExited(this, ProcessExitedQueue.Dequeue());
			}
		}
	}

	void processExited(object sender, EventArgs eventArgs) {
		if ( ProcessExited != null ) {
			//ProcessExited(this, eventArgs);
			//UnityMainThreadDispatcher.Instance().Enqueue(() => ProcessExited(this, eventArgs));

			lock ( ProcessExitedQueue ) {
				ProcessExitedQueue.Enqueue(eventArgs);
			}
		} else {
			Debug.Log("Unhandled ProcessExit: " + eventArgs);
		}
		process = null;
	}

	public void Kill() {
		applicationIsExiting = true;
		//process.CancelOutputRead();
		//process.CancelErrorRead();
		stdOutThread.Abort();
		stdErrThread.Abort();
		process.Kill();
	}

}
