using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
	public class EventManager : MonoSingletion<EventManager>, IManager
	{
		public  delegate void GameEvent<Sender, Param>(Sender sender, Param param=null) where Sender:class where Param:class;
		private Dictionary<GameEventType, GameEvent<object, object>> GameEvents=new Dictionary<GameEventType, GameEvent<object, object>>();
		public void Initial()
		{
		}

		public void PostEvent<Sender, Param>(GameEventType eventType,Sender sender, Param param=null) where Sender : class where Param :class
		{
			if (!GameEvents.ContainsKey(eventType))
			{
				return;
			}
			if (GameEvents[eventType] != null)
			{
				GameEvents[eventType](sender, param);
			}
		}

		public void AddListener<Sender, Param>(GameEventType eventType, GameEvent<Sender,Param> gameEvent)where Sender : class where Param : class
		{
			if (!GameEvents.ContainsKey(eventType))
			{
				GameEvents.Add(eventType,null);
			}
			GameEvent<object, object> theEvent = gameEvent as GameEvent<object, object>;
			GameEvents[eventType] += theEvent;
		}
	}
}
