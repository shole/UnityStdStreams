using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exampleProcessPoll : MonoBehaviour {

	private StreamedProcessPool proc;

	public float callInterval = 0.1f;
	public string processReadyMsg = "READY";

	void Start() {
		proc = GetComponent<StreamedProcessPool>();
		proc.StdOut = StdOut;
		//proc.StdErr = StdErr; // output handlers are optional - if not used, they're echoed to console
		
		StartCoroutine(sendPoll());
	}

	IEnumerator sendPoll() { // simple poller
		while ( true ) {
			yield return new WaitForSeconds(callInterval);

			float outval = Random.Range(1f, 100f);
			Debug.Log("stdin " + outval);
			proc.StdIn("" + outval);
		}
	}

	// Standard Output message handler - return true when we think process is ready to take more input
	bool StdOut(StreamedProcess proc, string message) {
		if ( message.Trim() == processReadyMsg ) { //  process has sent message that it's ready to receive new input
			// we don't need to do anything with this - just return true
			return true;
		}
		
		// do something with received data
		Debug.Log(proc.index + " stdout " + message);
		return false; // this was not the last line of data
	}
	/*
	// if you are just expecting single line responses, it's ok for every line to return true
	bool StdOut(StreamedProcess proc, string message) {
		Debug.Log(proc.index + " stdout " + message);
		return true;
	}
	*/

}
