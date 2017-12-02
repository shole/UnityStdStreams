using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Collections;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class StreamedProcessPool : MonoBehaviour {

	private StreamedProcess[] processes;
	private bool[] processBusy;

	[Header("Process")]
	public string execPath = "";
	public string execArgs = "";

	[Header("Process pool")]
	public int processCount = 1;
	private List<string> StdInQueue = new List<string>();
	public int StdInMaxQueueLength = 100;
	public bool overflowDiscardsOldest = true;

	[Header("Status")]
	public int currentQueueLength = 0;

	// Use this for initialization
	void Start() {
		processes = new StreamedProcess[processCount];
		processBusy = new bool[processCount];
		for ( int i = 0; i < processes.Length; i++ ) {
			processes[i] = new StreamedProcess();
			processes[i].index = i;
			processes[i].Execute(execPath, execArgs);
			processes[i].StdOut = StdOut;
		}
		StartCoroutine(sendPoll());
	}

	public void StdIn(string msg) {
		for ( int i = 0; i < processes.Length; i++ ) {
			if ( !processBusy[i] ) { // found a free process, send message and return 
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
		currentQueueLength = StdInQueue.Count;
	}

	public float callInterval = 0.1f;
	IEnumerator sendPoll() {
		//Debug.Log("a");
		while ( true ) {
			//Debug.Log("b");
			//yield return new WaitForSeconds(1);
			yield return new WaitForSeconds(callInterval);

			float outval = Random.Range(1f, 100f);
			Debug.Log("stdin " + outval);
			StdIn("" + outval);
		}
	}

	void StdOut(StreamedProcess proc, string message) {
		
		Debug.Log(proc.index + " stdout " + message);
		processBusy[proc.index] = false;

		sendFromStdInQueue();
	}

	void sendFromStdInQueue() {
		if ( StdInQueue.Count > 0 ) {
			string message = StdInQueue[0];
			StdInQueue.RemoveAt(0);
			StdIn(message);
		}
		currentQueueLength = StdInQueue.Count;
	}

	void OnApplicationQuit() {
		for ( int i = 0; i < processes.Length; i++ ) {
			if ( processes[i] != null && processes[i].process != null && !processes[i].process.HasExited ) {
				processes[i].Kill();
			}
		}
	}
}
