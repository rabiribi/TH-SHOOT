using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CQueue {
    public class ClQueue {
        private struct box {
            public byte[] Message;
            public int index;
            public int proiority;
        }
        private static Queue<box> queue = new Queue<box>();
        private const uint maxindex = 1000;
        private static bool isRun;
        private static Thread RunThread;
        public static void ReadyForSend(byte[] Message) {
            box temp = new box();
            temp.Message = Message;
            queue.Enqueue(temp);
            if(!isRun){
                RunThread = new Thread(new ThreadStart(run));
                RunThread.Start();
            }
        }
        private static void run() {
            isRun = true;
            box temp;
            while(queue.Count>0){
                if(queue.Count>300){
                    Debug.Log("Queue"+queue.Count);
                    temp = queue.Dequeue();
                }
                else {
                    temp = queue.Dequeue();
                    try {
                        NetWorkScript.client.Transmission(temp.Message);
                    }
                    catch {
                        Debug.Log("Transmis Error.");
                    }
                }
            }
            isRun = false;
        }
    }

}