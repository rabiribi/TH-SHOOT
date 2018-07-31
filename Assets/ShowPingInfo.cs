using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CQueue;
using CCoder;
using CloFunCC;
using UnityEngine;
using UnityEngine.UI;

public class ShowPingInfo : MonoBehaviour {
	public bool isShow;
	public Text text;
	private static DateTime Timetemp;
	private static double ping;//ms
	private static Timer timer;
	private static Rect PingRect = new Rect(10,10,50,20);
	// Use this for initialization
	void Start () {
		timer = new Timer(new TimerCallback(SendPingMessage),null,0,1000);
	}
	
	// Update is called once per frame
	void Update () {
		if(isShow) text.text = CloFunc.Round(ping).ToString();
	}
	// void OnGUI () {
	// 	if(isShow) GUI.TextArea(PingRect,((int)ping).ToString(),0,GUIStyle.none);
	// }
	private void SendPingMessage(object state) {
		if(NetWorkScript.isConnected&&isShow){
			Timetemp = CloFunc.GetTimeNow();
			ClQueue.ReadyForSend(EnCoder.PingMessage());
		}
	}
	public static void CalculatePing() {
		ping = (CloFunc.GetTimeNow()-Timetemp).TotalMilliseconds;
	}
}
