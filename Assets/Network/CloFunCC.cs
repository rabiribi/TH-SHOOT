using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace CloFunCC
{
    
    public static class CloFunc
    {
        public static void Errorinfo(string message)
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            //Console.ResetColor();
        }
        public static string HostIP()
        {
            //Console.WriteLine(Dns.GetHostAddresses(Dns.GetHostName())[0].ToString());
            return Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
        }

        public static string[] slice(string[] args, int start, int end)
        {
            int length = end - start + 1;
            string[] a = new string[length];
            Array.ConstrainedCopy(args, start, a, 0, length);
            return a;
        }
        public static byte[] Bytecat(byte[] Dest, byte[] Source){
            byte[] temp = new byte[Dest.Length+Source.Length];
            Dest.CopyTo(temp, 0);
            Source.CopyTo(temp, Dest.Length);
            return temp;
        }
        public static void Filelog(string path, string loginfo)
        {
            try
            {
                StreamWriter fp = new StreamWriter(path, true);
                fp.WriteLine(loginfo);
                fp.Close();
            }
            catch
            {
                Errorinfo("log Write Fail");
            }
            
        }
        public static IPAddress getAddress(Socket s)
        {
            return ((IPEndPoint)s.RemoteEndPoint).Address;
        }
        public static DateTime GetTimeNow() {
            return DateTime.Now;
        }
        public static int Round(double a) {
            return (int)Math.Round(a,MidpointRounding.AwayFromZero);
        }
    }
}