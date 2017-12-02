using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exampleProcessPoll : MonoBehaviour {

	private StreamedProcessPool proc;
	
	public float callInterval = 0.1f;
	// Use this for initialization
	void Start () {
		proc = GetComponent<StreamedProcessPool>();
		proc.StdOut = StdOut;
		StartCoroutine(sendPoll());
	}
	
	
	IEnumerator sendPoll() {
		while ( true ) {
			yield return new WaitForSeconds(callInterval);

			float outval = Random.Range(1f, 100f);
			Debug.Log("stdin " + outval);
			proc.StdIn("" + outval);
		}
	}

	void StdOut(StreamedProcess proc, string message) {
		Debug.Log(proc.index + " stdout " + message);
	}

}
