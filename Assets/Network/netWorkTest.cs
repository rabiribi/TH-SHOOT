using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCoder;
public class netWorkTest : MonoBehaviour
{
	public static bool isConnected;
	public bool isServer;
	public string Serverip;
	public static int Serverport=33491;
	public static int Clientport=33490;
	public static NetFileTSever.NFTServer server;
	public static NetFileTClient.NFTClient client;
	public void Start()
	{
		if (isServer){
			server = new NetFileTSever.NFTServer();
			client = new NetFileTClient.NFTClient();
			server.run(Serverport);
		}
		else {
			server = new NetFileTSever.NFTServer();
			client = new NetFileTClient.NFTClient();
			server.run(Clientport);
			client.ConnectToIp(Serverip,Serverport);
			while(client.Transmission(EnCoder.StartMessage())!=0){
				Debug.Log("retry");
			}
			netWorkTest.isConnected = true;
		}
		
	}
}
