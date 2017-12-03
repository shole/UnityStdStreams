using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StreamedProcessPool : MonoBehaviour {

	private StreamedProcess[] processes;
	private bool[] processBusy;

	[Header("Process")]
	public string execPath = "";
	public string execArgs = "";
	public bool waitForReady = true;
	public string readyForInput = "READY";

	[Header("Process pool")]
	public int processCount = 1;
	private List<string> StdInQueue = new List<string>();
	public int StdInMaxQueueLength = 100;
	public bool overflowDiscardsOldest = true;

	[Header("Status")]
	public int currentBusyProcesses = 0;
	public int currentQueueLength = 0;

	// attach handler to this for StdOut/StdErr messages
	public StreamedProcess.StreamedProcessMsgHandler StdOut; // StdOut(StreamedProcess proc, string message)
	public StreamedProcess.StreamedProcessMsgHandler StdErr; // StdErr(StreamedProcess proc, string message)

	void Start() {
		processes = new StreamedProcess[processCount];
		processBusy = new bool[processCount];
		for ( int i = 0; i < processes.Length; i++ ) {
			processes[i] = new StreamedProcess();
			processes[i].index = i;
			processes[i].Execute(execPath, execArgs);
			processes[i].StdOut = stdOut;
			processes[i].StdErr = stdErr;
			if ( readyForInput != "" ) {
				processBusy[i] = true; // wait for consent!
			}
		}
	}

	// call this to send StdIn messages
	public void StdIn(string msg) {
		for ( int i = 0; i < processes.Length; i++ ) { // find an idle process to call
			if ( !processBusy[i] ) {
				processes[i].StdIn(msg);
				processBusy[i] = true;
				return;
			}
		}
		// no idle process found, add it on the queue
		if ( StdInQueue.Count < StdInMaxQueueLength ) {
			StdInQueue.Add(msg);
		} else { // queue full, get rid of something
			if ( overflowDiscardsOldest ) {
				StdInQueue.RemoveAt(0);
				StdInQueue.Add(msg);
			} // else, just don't add the new one
		}
		updateStats();
	}

	void updateStats() {
		currentBusyProcesses = processBusy.Where(v => v).Count();
		currentQueueLength = StdInQueue.Count;
	}

	void stdOut(StreamedProcess proc, string message) {
		if ( waitForReady && message.Trim() == readyForInput.Trim() ) { // ready for input.. 
			processBusy[proc.index] = false;
			sendFromStdInQueue();
		} else if ( StdOut != null ) { // message not about input, output instead
			StdOut(proc, message); // external caller callback hookup
		} else {
			Debug.Log("Unhandled StdOut #" + proc.index + ": " + message);
		}
		if ( !waitForReady ) {
			processBusy[proc.index] = false;
			sendFromStdInQueue();
		}
	}

	void stdErr(StreamedProcess proc, string message) {
		if ( StdErr != null ) {
			StdErr(proc, message); // external caller callback hookup
		} else {
			Debug.Log("Unhandled StdErr #" + proc.index + ": " + message);
		}

		sendFromStdInQueue();
	}

	void sendFromStdInQueue() { // call next msg in queue
		if ( StdInQueue.Count > 0 ) {
			string message = StdInQueue[0];
			StdInQueue.RemoveAt(0);
			StdIn(message);
		}
		updateStats();
	}

	void OnApplicationQuit() { // murder all children on exit
		for ( int i = 0; i < processes.Length; i++ ) {
			if ( processes[i] != null && processes[i].process != null && !processes[i].process.HasExited ) {
				processes[i].Kill();
			}
		}
	}
}
