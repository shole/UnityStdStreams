using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class AppLauncher : MonoBehaviour {
	Process process = null;
	StreamWriter messageStream;

	public string execPath = "";
	public string execArgs = "";

	void Start() {
		StartProcess();
		StartCoroutine(sendPoll());
	}

	// https://stackoverflow.com/questions/12640943/run-process-start-on-a-background-thread
	// https://stackoverflow.com/questions/611094/async-process-start-and-wait-for-it-to-finish
	
	IEnumerator sendPoll() {
		Debug.Log("a");
		while ( true ) {
			Debug.Log("b");
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			Debug.Log("c");
			float outval = Random.Range(1f, 100f);
			Debug.Log("stdin " + outval);
			messageStream.WriteLine("" + outval);
			//messageStream.Flush();
		}
	}

	void StartProcess() {
		try {
			process = new Process();
			process.EnableRaisingEvents = false;
			process.StartInfo.FileName = execPath;
			process.StartInfo.Arguments = execArgs;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			//process.StartInfo.CreateNoWindow = true;
			process.OutputDataReceived += DataReceived;
			process.ErrorDataReceived += ErrorReceived;
			process.Start();
			process.BeginOutputReadLine();
			messageStream = process.StandardInput;
			messageStream.AutoFlush = true;

			Debug.Log("Successfully launched app");
		} catch ( Exception e ) {
			Debug.LogError("Unable to launch app: " + e.Message);
		}
	}

	void DataReceived(object sender, DataReceivedEventArgs eventArgs) {
		Debug.Log("stdout " + eventArgs.Data);
	}

	void ErrorReceived(object sender, DataReceivedEventArgs eventArgs) {
		Debug.LogError("stderr " + eventArgs.Data);
	}

	void OnApplicationQuit() {
		if ( process != null && !process.HasExited ) {
			process.Kill();
		}
	}
}
