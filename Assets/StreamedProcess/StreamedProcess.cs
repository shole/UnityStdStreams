using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class StreamedProcess {
	public Process process = null;
	StreamWriter messageStream;

	public int index = -1;

	public string execPath = "";
	public string execArgs = "";

	public void Execute(string path, string args) {
		execPath = path;
		execArgs = args;
		startProcess();
	}

	// https://stackoverflow.com/questions/12640943/run-process-start-on-a-background-thread
	// https://stackoverflow.com/questions/611094/async-process-start-and-wait-for-it-to-finish

	public void StdIn(string msg) {
		// incase the user is a clever bunny and wants to put in multiple lines, let's help her
		foreach ( string line in msg.Split('\n') ) {
			process.StandardInput.WriteLine(line);
			process.StandardInput.Flush(); // let's flush just in case
		}
	}

	public delegate void StreamedProcessMsgHandler(StreamedProcess process, string message);

	public StreamedProcessMsgHandler StdOut;
	public StreamedProcessMsgHandler StdErr;

	void startProcess() {
		try {
			process = new Process();
			process.StartInfo.FileName = execPath;
			if ( execArgs.Length > 0 ) {
				process.StartInfo.Arguments = execArgs;
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			//process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			//process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;

			process.OutputDataReceived += dataReceived;
			process.ErrorDataReceived += errorReceived;

			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.StandardInput.AutoFlush = true;

			Debug.Log("Successfully launched app #" + index + ": " + execPath);
		} catch ( Exception e ) {
			Debug.LogError("Unable to launch app #" + index + ": " + execPath + " : " + e.Message);
		}
	}

	void dataReceived(object sender, DataReceivedEventArgs eventArgs) {
		// if you're reading this, something is probably broken? 
		// make sure your client process flushes.. python at least needed a manual flush
		if ( StdOut != null ) {
			StdOut(this, eventArgs.Data);
		} else {
			Debug.Log("Unhandled StdOut: " + eventArgs.Data);
		}
	}

	void errorReceived(object sender, DataReceivedEventArgs eventArgs) {
		if ( StdErr != null ) {
			StdErr(this, eventArgs.Data);
		} else {
			Debug.Log("Unhandled StdErr: " + eventArgs.Data);
		}
	}

	public void Kill() {
		process.CancelOutputRead();
		process.CancelErrorRead();
		process.Kill();
	}

}
