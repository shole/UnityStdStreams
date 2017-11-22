using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;

public class AppLauncher : MonoBehaviour {
	Process process = null;
	StreamWriter messageStream;

	void StartProcess() {
		try {
			process = new Process();
			process.EnableRaisingEvents = false;
			process.StartInfo.FileName = Application.dataPath + "/path/to/The.app/Contents/MacOS/The";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			process.OutputDataReceived += DataReceived;
			process.ErrorDataReceived += ErrorReceived;
			process.Start();
			process.BeginOutputReadLine();
			messageStream = process.StandardInput;

			UnityEngine.Debug.Log("Successfully launched app");
		} catch ( Exception e ) {
			UnityEngine.Debug.LogError("Unable to launch app: " + e.Message);
		}
	}

	void DataReceived(object sender, DataReceivedEventArgs eventArgs) {
		// Handle it
	}

	void ErrorReceived(object sender, DataReceivedEventArgs eventArgs) {
		UnityEngine.Debug.LogError(eventArgs.Data);
	}

	void OnApplicationQuit() {
		if ( process != null && !process.HasExited ) {
			process.Kill();
		}
	}
}
