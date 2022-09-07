using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;

[System.Serializable]
public class ServerIp
{
	public string ipAddress;
	public string port;
}

public class UnityHttpListener : MonoBehaviour
{

	private HttpListener listener;
	private Thread listenerThread;

	void Start()
	{
		listener = new HttpListener();
		listener.Prefixes.Add("http://localhost:4444/");
		listener.Prefixes.Add("http://127.0.0.1:4444/");
		listener.Prefixes.Add("http://*:4444/");
		listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		listener.Start();

		listenerThread = new Thread(startListener);
		listenerThread.Start();
		Debug.Log("Server Started");
	}

	void Update()
	{
	}

	private void startListener()
	{
		while (true)
		{
			var result = listener.BeginGetContext(ListenerCallback, listener);
			result.AsyncWaitHandle.WaitOne();
		}
	}

	private void ListenerCallback(IAsyncResult result)
	{
		var context = listener.EndGetContext(result);

		if (context.Request.QueryString.AllKeys.Length > 0)
			foreach (var key in context.Request.QueryString.AllKeys)
			{
				Debug.Log("Key: " + key + ", Value: " + context.Request.QueryString.GetValues(key)[0]);
			}

		if (context.Request.HttpMethod == "POST")
		{
			//Thread.Sleep(1000);
			var data_text = new StreamReader(context.Request.InputStream,
								context.Request.ContentEncoding).ReadToEnd();
			
			ServerIp serverIp = JsonUtility.FromJson<ServerIp>(data_text);
			NetworkManager.instance.ipAddress = serverIp.ipAddress;
			NetworkManager.instance.port = serverIp.port;
			
			Debug.Log("server IP:" + NetworkManager.instance.ipAddress);
			Debug.Log("server port:" + NetworkManager.instance.port);


		}

		context.Response.Close();
	}

}