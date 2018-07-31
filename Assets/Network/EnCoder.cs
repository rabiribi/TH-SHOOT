using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CQueue;
using UnityEngine;

namespace CCoder{
public static class EnCoder{
	
	static public byte[] StartMessage(){
		return HeadMessage(DeCoder.Coder.STAT);
	}
	static public byte[] MoveMessage(Vector2 input,Vector3 position,float speed){
		byte[] message = new byte[21];
		message[0] = (byte)DeCoder.Coder.MOVE;
		BitConverter.GetBytes(input.x).CopyTo(message,1);
		BitConverter.GetBytes(input.y).CopyTo(message,5);
		BitConverter.GetBytes(position.x).CopyTo(message,9);
		BitConverter.GetBytes(position.y).CopyTo(message,13);
		BitConverter.GetBytes(speed).CopyTo(message,17);
		return message;
	}
	static public byte[] PingMessage(){
		return HeadMessage(DeCoder.Coder.PING);
	}
	static public byte[] RpinMessage(){
		return HeadMessage(DeCoder.Coder.RPIN);
	}
	static public byte[] HeadMessage(DeCoder.Coder coder){
		byte[] message = new byte[1];
		message[0] = (byte)coder;
		return message;
	}
}
public static class DeCoder{
	public enum Coder{
		STAT, //start
		MOVE, //move
		PING, //ping
		RPIN, //return ping
	}
	public static void DeCodeMessage(byte[] Message){
		Coder coder = (Coder)Message[0];
		Debug.Log("Decode Message:"+(int)coder);
		MessageHandle(coder, Message.Skip(1).ToArray());
	}
	private static void MessageHandle(Coder coder, byte[] Message){
		switch(coder){
			case Coder.STAT:{
				NetWorkScript.client.ConnectToIp(NetWorkScript.server.GetClientAddress(),NetWorkScript.Clientport);
				Debug.Log("Connected!");
				NetWorkScript.isConnected = true;
				break;
			}
			case Coder.MOVE:{
				player2.inputAxis = new Vector2(BitConverter.ToSingle(Message,0),BitConverter.ToSingle(Message,4));
				player2.Position = new Vector2(BitConverter.ToSingle(Message,8),BitConverter.ToSingle(Message,12));
				player2.moveSpeed = BitConverter.ToSingle(Message,16);
				break;
			}
			case Coder.PING:{
				ClQueue.ReadyForSend(EnCoder.RpinMessage());
				break;
			}
			case Coder.RPIN:{
				ShowPingInfo.CalculatePing();
				break;
			}
		}
	}
}
}