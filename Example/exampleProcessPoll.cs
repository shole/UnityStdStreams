using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class exampleProcessPoll : MonoBehaviour {

	private StreamedProcessPool proc;

	public float callInterval = 0.1f;
	public string processReadyMsg = "READY";

	Dictionary<int, double> callRegistry = new Dictionary<int, double>();

	private ParticleSystem ps;
	
	void Start() {
		ps = GameObject.FindObjectOfType<ParticleSystem>();
		proc = GetComponent<StreamedProcessPool>();
		proc.StdOut = StdOut;
		//proc.StdErr = StdErr; // output handlers are optional - if not used, they're echoed to console

		StartCoroutine(sendPoll());
	}

	IEnumerator sendPoll() { // simple poller
		while ( true ) {
			yield return new WaitForSeconds(callInterval);

			float outval = (int)Random.Range(1f, 10000f);
			Debug.Log("stdin " + outval);
			int id = proc.StdIn("" + outval);
			callRegistry.Add(id, DateTime.Now.Ticks); // safe some data with call id
		}
	}

	// Standard Output message handler - return true when we think process is ready to take more input
	bool StdOut(StreamedProcess proc, string message) {
		if ( message.Trim() == processReadyMsg.Trim() ) { //  process has sent message that it's ready to receive new input
			// we don't need to do anything with this - just return true
			//Debug.Log(proc.index+" ready");
			return true;
		}

		// do something with received data
		double endTime = DateTime.Now.Ticks;
		double startTime = callRegistry[proc.GUID]; // you can use the GUID to link sends to receives
		callRegistry.Remove(proc.GUID);

		Debug.Log(proc.index + " stdout " + message + ", took " + ((endTime - startTime)/TimeSpan.TicksPerSecond) + "s");
		
		ParticleSystem.EmitParams p=new ParticleSystem.EmitParams();
		
		p.position=new Vector3(
			Random.value*2-1,
			Random.value*2,
			Random.value*2-1
		);
		p.startColor=new Color(
			Random.value,
			Random.value,
			Random.value
		);
		p.startSize3D= new Vector3(
			               Random.value,
			               Random.value,
			               Random.value
			               )
		               * 0.1f;
		
		ps.Emit(p,1);
		
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
