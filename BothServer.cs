using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

public class BothServer : MonoBehaviour
{

	//UDP
	#region UDP
	//UDP
	private static int localPort;
	public GameObject cubeAct;
	// prefs
	private string IP;  // define in init
	public int port;  // define in init
	Vector3 pos;
	public bool ShouldUpdate;


	// "connection" things
	IPEndPoint remoteEndPoint;
	UdpClient client;

	// gui
	//string strMessage = "";



    #endregion


    //TCP
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient;
	#endregion

	// Use this for initialization
	void Start()
	{
		//TCP
        #region
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();
		#endregion



		//UDP
		#region
		init();
		#endregion
	}

	// Update is called once per frame
	void Update()
	{
		/*
		//TCP
		#region
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SendMessage();
		}
		#endregion
		*/

		//UDP
		#region
		if (pos != cubeAct.transform.position)
		{
			Info newInfo = new Info();
			newInfo.name = this.name;
			// vector3 cannot be set because it is not default value type
			newInfo.localPosX = cubeAct.transform.position.x;
			newInfo.localPosY = cubeAct.transform.position.y;
			newInfo.localPosZ = cubeAct.transform.position.z;
			string json = JsonConvert.SerializeObject(newInfo);
			sendString(json + "\n");
			pos = cubeAct.transform.position;
			ShouldUpdate = true;
		}
		else
		{
			SendMessage();
		}

		#endregion
	}

	//TCP
    #region
    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
	{
		try
		{
			// Create listener on localhost port 8052. 			
			tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8051);
			tcpListener.Start();
			Debug.Log("Server is listening");
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				using (connectedTcpClient = tcpListener.AcceptTcpClient())
				{
					// Get a stream object for reading 					
					using (NetworkStream stream = connectedTcpClient.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary. 						
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 							
							string clientMessage = Encoding.ASCII.GetString(incommingData);
							Debug.Log("client message received as: " + clientMessage);
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (connectedTcpClient == null)
		{
			return;
		}

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream();
			if (stream.CanWrite)
			{
				string serverMessage = "This is a message from your server.";
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("Server sent his message - should be received by client");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	void OnApplicationQuit()
	{

		tcpListenerThread.Abort();

	}

	#endregion


	//UDP
	#region
	public void init()
	{
		// Endpunkt definieren, von dem die Nachrichten gesendet werden.
		print("UDPSend.init()");

		// define
		IP = "127.0.0.1";
		port = 8051;

		// ----------------------------
		// Senden
		// ----------------------------
		remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
		client = new UdpClient();

		// status
		print("Sending to " + IP + " : " + port);
		print("Testing: nc -lu " + IP + " : " + port);

	}

	// inputFromConsole
	private void inputFromConsole()
	{
		try
		{
			string text;
			do
			{
				text = Console.ReadLine();

				// Den Text zum Remote-Client senden.
				if (text != "")
				{

					// Daten mit der UTF8-Kodierung in das Binärformat kodieren.
					byte[] data = Encoding.UTF8.GetBytes(text);

					// Den Text zum Remote-Client senden.
					client.Send(data, data.Length, remoteEndPoint);
				}
			} while (text != "");
		}
		catch (Exception err)
		{
			print(err.ToString());
		}

	}

	// sendData
	private void sendString(string message)
	{
		try
		{
			//if (message != "")
			//{

			// Daten mit der UTF8-Kodierung in das Binärformat kodieren.
			byte[] data = Encoding.UTF8.GetBytes(message);

			// Den message zum Remote-Client senden.
			client.Send(data, data.Length, remoteEndPoint);
			//}
		}
		catch (Exception err)
		{
			print(err.ToString());
		}
	}


	// endless test
	private void sendEndless(string testStr)
	{
		do
		{
			sendString(testStr);


		}
		while (true);

	}
	#endregion

	void OnDestroy()
	{
		tcpListenerThread.Abort();
	}

    private void OnDisable()
    {
		tcpListenerThread.Abort();
	}

}
