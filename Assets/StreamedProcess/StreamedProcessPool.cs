using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StreamedProcessPool : MonoBehaviour {

	private StreamedProcess[] processes;

	[Header("Process")]
	public string execPath = "";
	public string execArgs = "";
	public string workingPath = "";

	[Header("Process pool")]
	public int processCount = 1;

	public int maxQueueLength = 100;
	public bool queueOverflowOldest = true;

	struct queueItem {
		public int GUID; // generated identifier returned to query process on start and completion
		public string message; // querystring
	}

	private List<queueItem> StdInQueue = new List<queueItem>();

	[Header("Status")]
	public int queriesSent = 0;
	public int currentQueueLength = 0;
	public int currentBusyProcesses = 0;
	public bool[] processBusy;
	
	// this is different from process one by confirmation
	public delegate bool StreamedProcessMsgHandler(StreamedProcess proc, string message);

	// attach handler to this for StdOut/StdErr messages
	public StreamedProcessMsgHandler StdOut; // StdOut(StreamedProcess proc, string message)
	public StreamedProcessMsgHandler StdErr; // StdErr(StreamedProcess proc, string message)

	private bool applicationIsExiting = false;
	
	void Start() {
		processes = new StreamedProcess[processCount];
		processBusy = new bool[processCount];
		for ( int i = 0; i < processes.Length; i++ ) {
			processes[i] = new StreamedProcess();
			processes[i].index = i;
			processes[i].Execute(execPath, execArgs,workingPath);
			processes[i].StdOut = stdOut;
			processes[i].StdErr = stdErr;
			processes[i].ProcessExited = processRestart; // we're a service so restart any process who quits
			processBusy[i] = true; // wait for consent!
		}
	}

	void processRestart(StreamedProcess process, EventArgs eventArgs) {
		if ( applicationIsExiting ) return; // don't resurrect after apocalypse
		process.RestartProcess();
		processBusy[process.index] = true; // wait for consent!
	}

	// call this to send StdIn messages
	public int StdIn(string msg) {
		queueItem item = new queueItem();
		item.GUID = int.MinValue + (++queriesSent); // keeping it simple
		item.message = msg;

		for ( int i = 0; i < processes.Length; i++ ) { // find an idle process to call
			if ( !processBusy[i] ) {
				processes[i].StdIn(item.message);
				processes[i].GUID = item.GUID;
				processBusy[i] = true;

				return item.GUID;
			}
		}

		// no idle process found, add it on the queue
		if ( StdInQueue.Count < maxQueueLength ) {
			StdInQueue.Add(item);
		} else { // queue full, get rid of something
			if ( queueOverflowOldest ) {
				StdInQueue.RemoveAt(0);
				StdInQueue.Add(item);
			} // else, just don't add the new one
		}
		updateStats();
		return item.GUID;
	}

	void updateStats() {
		currentBusyProcesses = processBusy.Where(v => v).Count();
		currentQueueLength = StdInQueue.Count;
	}

	void stdOut(StreamedProcess proc, string message) {
		bool complete = false;
		if ( StdOut != null ) {
			complete = StdOut(proc, message); // external caller callback hookup
		} else {
			Debug.Log("Unhandled StdOut #" + proc.index + ": " + message);
			complete = true;
		}

		if ( complete ) { // if callback said we're ready for more input
			processBusy[proc.index] = false;
			sendFromStdInQueue();
		}
	}

	void stdErr(StreamedProcess proc, string message) {
		bool complete = false;
		if ( StdErr != null ) {
			complete = StdErr(proc, message); // external caller callback hookup
		} else {
			Debug.LogWarning("Unhandled StdErr #" + proc.index + ": " + message);
			complete = true;
		}

		if ( complete ) { // if callback said we're ready for more input
			processBusy[proc.index] = false;
			sendFromStdInQueue();
		}
	}

	void sendFromStdInQueue() { // call next msg in queue
		if ( StdInQueue.Count > 0 ) {
			queueItem item = StdInQueue[0];
			for ( int i = 0; i < processes.Length; i++ ) { // find an idle process to call
				if ( !processBusy[i] ) {
					processes[i].StdIn(item.message);
					processes[i].GUID = item.GUID;
					processBusy[i] = true;
				}
			}
			StdInQueue.RemoveAt(0);
		}
		updateStats();
	}

	void OnApplicationQuit() { // murder all children on exit
		applicationIsExiting = true;
		for ( int i = 0; i < processes.Length; i++ ) {
			if ( processes[i] != null && processes[i].process != null && !processes[i].process.HasExited ) {
				processes[i].Kill();
			}
		}
	}
}
