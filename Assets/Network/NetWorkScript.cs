using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using CCoder;
public class NetWorkScript: MonoBehaviour
{
	public static bool isConnected;
	//public bool isServer;
	public static string Serverip;
	public static int Serverport=33491;
	public static int Clientport=33490;
	public static NetFileTSever.NFTServer server = new NetFileTSever.NFTServer();
	public static NetFileTClient.NFTClient client = new NetFileTClient.NFTClient();
	public void ServerStart() {
		server.Close();
		client.Close();
		server = new NetFileTSever.NFTServer();
		client = new NetFileTClient.NFTClient();
		server.run(Serverport);
		Debug.Log("ServerStart");
	}
	public void ClientStart() {
		server.Close();
		client.Close();
		server = new NetFileTSever.NFTServer();
		client = new NetFileTClient.NFTClient();
		server.run(Clientport);
		client.ConnectToIp(Serverip,Serverport);
		while(client.Transmission(EnCoder.StartMessage())!=0){
			Debug.Log("retry");
		}
		NetWorkScript.isConnected = true;
	}
	public void TryClient() {
		IPAddress temp;
		Debug.Log("TRY");
		if(IPAddress.TryParse(Serverip,out temp)){
			Debug.Log(temp);
			ClientStart();
		}
		else {
			Debug.Log("Unright Address");
		}
	}
	public void CloseAll() {
		server.Close();
		client.Close();
	}
}
