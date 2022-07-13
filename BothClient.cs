using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Net;
using Newtonsoft.Json;
public class BothClient : MonoBehaviour
{
	//TCP
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion

	//UDP
	#region
	public Vector3 newPos;

	// receiving Thread
	Thread receiveThread;

	// udpclient object
	UdpClient client;

	// public
	// public string IP = "127.0.0.1"; default local
	public int port; // define > init

	// infos
	public string lastReceivedUDPPacket = "";
	public string allReceivedUDPPackets = ""; // clean up this from time to time!


	// start from shell

	#endregion

	// Use this for initialization 	
	void Start()
	{
		//TCP
        #region
        ConnectToTcpServer();
		#endregion


		//UDP
		#region
		init();
		#endregion
	}
    // Update is called once per frame
    void Update()
	{
        //TCP
        #region
        if (Input.GetKeyDown(KeyCode.Space))
		{
			SendMessage();
		}
        #endregion

        //UDP
        #region

		#endregion
	}

	//UDP
	#region
	private void init()
	{
		// Endpunkt definieren, von dem die Nachrichten gesendet werden.
		print("UDPSend.init()");

		// define port
		port = 8051;

		// status
		print("Sending to 127.0.0.1 : " + port);
		print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


		// ----------------------------
		// Abhören
		// ----------------------------
		// Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
		// Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}

	// receive thread
	private void ReceiveData()
	{

		client = new UdpClient(port);
		while (true)
		{

			try
			{
				// Bytes empfangen.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref anyIP);

				// Bytes mit der UTF8-Kodierung in das Textformat kodieren.
				string text = Encoding.UTF8.GetString(data);

				// Den abgerufenen Text anzeigen.
				print(">> " + text);



				Info receiveInfo = JsonConvert.DeserializeObject<Info>(text);


				newPos = new Vector3(receiveInfo.localPosX, receiveInfo.localPosY + 3.0f, receiveInfo.localPosZ);
				print("Pos>> " + newPos);



				// latest UDPpacket
				lastReceivedUDPPacket = text;

				// ....
				allReceivedUDPPackets = allReceivedUDPPackets + text;

			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}

	// getLatestUDPPacket
	// cleans up the rest
	public string getLatestUDPPacket()
	{
		allReceivedUDPPackets = "";
		return lastReceivedUDPPacket;
	}

	#endregion


	//TCP
	#region
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient("localhost", 8051);
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = "This is a message from one of your clients.";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	void OnApplicationQuit()
	{
		clientReceiveThread.Abort();
	}

	void OnDestroy()
	{
		clientReceiveThread.Abort();
	}

	private void OnDisable()
	{
		clientReceiveThread.Abort();
	}
	#endregion


}
