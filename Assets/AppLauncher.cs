using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
		//Debug.Log("a");
		while ( true ) {
			//Debug.Log("b");
			yield return new WaitForSeconds(1);
			
			float outval = Random.Range(1f, 100f);
			Debug.Log("stdin " + outval);
			process.StandardInput.WriteLine("" + outval);
			process.StandardInput.Flush();
			
		}
	}

	void StartProcess() {
		try {
			process = new Process();
			process.StartInfo.FileName = execPath;
			process.StartInfo.Arguments = execArgs;
			
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			//process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			//process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			
			process.OutputDataReceived += DataReceived;
			process.ErrorDataReceived += ErrorReceived;
			
			process.Start();
			
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.StandardInput.AutoFlush = true;

			Debug.Log("Successfully launched app");
			//process.WaitForExit();
		} catch ( Exception e ) {
			Debug.LogError("Unable to launch app: " + e.Message);
		}
	}

	void DataReceived(object sender, DataReceivedEventArgs eventArgs) {
		Debug.Log("stdout " + eventArgs.Data);
		//Debug.Log("stdout " + process.StandardOutput.ReadToEnd());
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
