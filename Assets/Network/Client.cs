using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using CloFunCC;
using NetFileTSever;
using UnityEngine;

namespace NetFileTClient
{
    public class NFTClient
    {
        struct ClientConfig
        {
            public IPAddress Address;
            public IPAddress LocalAddress;
            public int Port;
            public int BufferSize;
            public string filepath;
            public string serverpath;
            public long FileLength;
        };

        struct ClientState
        {
            public bool Connected;
            public bool IPReady;
            public bool PortReady;
            public bool login;
            public bool user;
            public bool pass;
            public bool list;
            public bool download;
            public bool upload;
            public bool exit;
            public bool transmission;
        };
        //
        private Socket HandSocket;
        private ClientConfig Config;
        private ClientState state;
        FileStream fp;
        private string MessageTemp;
        private long tempLength;
        Thread Transmis;
        public NFTClient()
        {
            Config.filepath = @"file";
            Config.serverpath = "";
            checkfilepath();
            Config.BufferSize = 2048;
            HandSocket = new Socket(AddressFamily.InterNetwork , SocketType.Stream, ProtocolType.Tcp);
            // default port
            Config.Port = 33491;
            // default adress (ipv4) (local host)
            Config.Address = IPAddress.Parse("127.0.0.1");
        }

        ~NFTClient()
        {
            HandSocket.Close();
        }

        public void ConnectToIp()
        {
            HandSocket.Connect(Config.Address, Config.Port);
            state.Connected = true;
        }

        public void ConnectToIp(string ipString)
        {
            Config.Address = IPAddress.Parse(ipString);
            ConnectToIp();
        }

        public void ConnectToIp(string ipString, string portString)
        {
            if(int.TryParse(portString, out Config.Port))
            {//unused need refine if use.
                Config.Address = IPAddress.Parse(ipString);
                ConnectToIp();
            }
            else throw new Exception();
        }
        public void ConnectToIp(string ipString, int port)
        {
            Config.Address = IPAddress.Parse(ipString);
            Config.Port = port;
            ConnectToIp();
        }
        static void ArgumentHelp()
        {
            Console.Write("=============" + '\n' +
                          "Argument List" + '\n' +
                          "=============" + '\n' +
                          "-c / connect <adress> : try to connect to server adressed by <adress>" + '\n' +
                          "disconnect : close the connect with server" + '\n' +
                          "-p / port <port> : config the port server is listening. Default port is recammanded." + '\n' +
                          "-v / version : show this release's version." + '\n' +
                          "-h / help : show help information." + '\n' +
                          "-e / exit : quit this program." + '\n' +
                          "user <username> : try to login with <username>." + '\n' +
                          "pass <password> : try to login with <password>." + '\n' +
                          "--debug : try to send a debug message." + '\n' +
                          "shutdown : shutdown remote server." + '\n' +
                          "sign <username> <password> : sign up on server." + '\n' +
                          "rspas <password> : reset password as <password>." + '\n' +
                          "ls : show all file and directory under path." + '\n' +
                          "dir <directory> : try to create new directory." + '\n' +
                          "cd <directory> : try to get in <direstory>." + '\n' +
                          "del / delete <directory> : try to delete <directory>." + '\n' +
                          "-r / remove <file> : try to remove <file>." + '\n' +
                          "-d / download <file> : download <file> from server." + '\n' +
                          "-u / upload <file> : upload local <file> to server." + '\n' +
                          "Supload <file1 file2 ...>: upload local <files> to server." + '\n' +
                          "-------------" + '\n');
        }

        public void run(string[] args)
        {
            string tpath;
            while (true)
            {
                CommandInput(args);
                if (state.exit) break;
                tpath = state.Connected ? Config.Address.ToString():"";
                Console.Write(tpath + ">" + Config.serverpath + " ");
                args = Console.ReadLine().Split(' ');
            }
        }
        private void CommandInput(string[] args)
        {
            if (args.Length == 0)
            {
                CloFunc.Errorinfo("Can't run without any arguments.");
                ArgumentHelp();
            }
            else try
            {
                switch(args[0])
                {
                    case "-c":
                    case "connect":
                        {
                            if(args.Length == 1) Console.WriteLine("<adress> expected.");
                            else if (state.Connected)
                            {
                                CloFunc.Errorinfo("Server Already Connected.");
                            }
                            else
                            {
                                try
                                {
                                    ConnectToIp(args[1]);
                                    state.Connected = true;
                                    state.IPReady = true;
                                    Console.WriteLine("Connect Sucess.");
                                }
                                catch (Exception e)
                                {
                                    CloFunc.Errorinfo("Connect Fail." + '\n' + e.ToString());
                                }
                                if (args.Length > 2)
                                {
                                    CommandInput(CloFunc.slice(args,2,args.Length-1));
                                }
                            }
                            break;
                        }
                    case "-p":
                    case "port":
                    {
                        if (args.Length == 1) Console.WriteLine("<port> expected.");
                        else if (state.Connected)
                        {
                            CloFunc.Errorinfo("Can't change port while connecting.");
                        }
                        else
                        {
                            try
                            {
                                Config.Port = int.Parse(args[1]);
                                state.PortReady = true;
                            }
                            catch
                            {
                                CloFunc.Errorinfo("ERROR:Port should be int number.");
                            }
                        }
                        if (args.Length > 2) {
                            CommandInput(CloFunc.slice(args,2,args.Length-1));
                        }
                        break;
                    }
                    case "-h":
                    case "help":
                    case "?": {
                        ArgumentHelp();
                        break;
                    }
                    case "-v":
                    case "version": {
                        Console.WriteLine("NetFileTransportG <version 0.1>");
                        break;
                    }
                    case "--debug": {
                        DebugMission();
                        break;
                    }
                    case "user": {
                        if (!state.Connected) CloFunc.Errorinfo("Please Connect to Server First");
                        else if (args.Length == 1) Console.WriteLine("<username> expected.");
                        else if (state.login||state.user)
                        {
                            CloFunc.Errorinfo("Already login or input user.");
                        }
                        else
                        {
                            try
                            {
                                TextMessage(HandSocket, NFTServer.ServerControl.USER, args[1]);
                                ReceiveMessage();
                                state.user = true;//label
                            }
                            catch
                            {
                                CloFunc.Errorinfo("ERROR: Command User Fail.");
                            }
                            
                        }
                        if (args.Length > 2) {
                            CommandInput(CloFunc.slice(args,2,args.Length-1));
                        }
                        break;
                    }
                    case "pass": {
                        if (args.Length == 1) Console.WriteLine("<password> expected.");
                        else if (state.login||!state.user)
                        {
                            CloFunc.Errorinfo("Already login or need username.");
                        }
                        else
                        {
                            try
                            {
                                //Console.WriteLine(args[1].GetHashCode());
                                TextMessage(HandSocket, NFTServer.ServerControl.PASS, args[1].GetHashCode().GetHashCode().ToString());
                                state.pass = true;
                                ReceiveMessage();
                                if (state.pass) {
                                    state.login = true;
                                }
                            }
                            catch
                            {
                                CloFunc.Errorinfo("ERROR: Command Pass Fail.");
                            }
                            
                        }
                        if (args.Length > 2) {
                            CommandInput(CloFunc.slice(args,2,args.Length-1));
                        }
                        break;
                    }
                    case "ls": {
                        TextMessage(HandSocket, NFTServer.ServerControl.LIST, "Show me list.");
                        state.list = true;
                        MessageTemp = "";
                        while (state.list) {
                            ReceiveMessage();
                        }
                        break;
                    }
                    case "dir": {
                        if(args.Length == 1) Console.WriteLine("<directory> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                TextMessage(HandSocket, NFTServer.ServerControl.CDIR, args[1]);
                                ReceiveMessage();
                            }
                            catch
                            {
                                CloFunc.Errorinfo("Create Directory Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "cd" : {
                        if(args.Length == 1) Console.WriteLine("<directory> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                TextMessage(HandSocket, NFTServer.ServerControl.CDCD, args[1]);
                                ReceiveMessage();
                            }
                            catch
                            {
                                CloFunc.Errorinfo("GetIn Directory Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "del":
                    case "delete": {
                        if(args.Length == 1) Console.WriteLine("<directory> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                TextMessage(HandSocket, NFTServer.ServerControl.DDEL, args[1]);
                                //state.deldir = true;
                                ReceiveMessage();
                            }
                            catch
                            {
                                CloFunc.Errorinfo("Delete Directory Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "-r":
                    case "remove" : {
                        if(args.Length == 1) Console.WriteLine("<file> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                TextMessage(HandSocket, NFTServer.ServerControl.FDEL, args[1]);
                                //state.deldir = true;
                                ReceiveMessage();
                            }
                            catch
                            {
                                CloFunc.Errorinfo("Delete Directory Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "-d" :
                    case "download" : {
                        if(args.Length == 1) Console.WriteLine("<file> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                DownloadProcess(args[1]);
                                
                            }
                            catch
                            {
                                CloFunc.Errorinfo("DownLoad File Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "-u" :
                    case "upload" : {
                        if(args.Length == 1) Console.WriteLine("<file> expected.");
                        else if (!state.Connected)
                        {
                            CloFunc.Errorinfo("None Server Connected.");
                        }
                        else
                        {
                            try
                            {
                                UploadProcess(args[1]);
                                
                            }
                            catch
                            {
                                CloFunc.Errorinfo("UpLoad File Fail.");
                            }
                        }
                        if (args.Length > 2)
                            {
                                CommandInput(CloFunc.slice(args,2,args.Length-1));
                            }
                        break;
                    }
                    case "sign" : {
                        if (!state.Connected) CloFunc.Errorinfo("None Server Connected.");
                        else if (args.Length < 3) {
                            CloFunc.Errorinfo("<username> or <password> required");
                        }
                        else {
                            string username = args[1];
                            string password = args[2].GetHashCode().ToString();
                            string tempmessage = username + ' ' + password;
                            TextMessage(HandSocket, NFTServer.ServerControl.SIGN, tempmessage);
                            ReceiveMessage();
                        }
                        if (args.Length > 3) {
                            CommandInput(CloFunc.slice(args,2,args.Length-1));
                        }
                        break;
                    }
                    case "rspass" : {
                        if (!state.Connected) CloFunc.Errorinfo("None Server Connected.");
                        else if (args.Length == 1) {
                            CloFunc.Errorinfo("<password> required");
                        }
                        else {
                            string password = args[1].GetHashCode().ToString();
                            TextMessage(HandSocket, NFTServer.ServerControl.RPAS, password);
                            ReceiveMessage();
                        }
                        if (args.Length > 2) {
                            CommandInput(CloFunc.slice(args,2,args.Length-1));
                        }
                        break;
                    }
                    case "shutdown": {
                        if (!state.Connected) CloFunc.Errorinfo("None Server Connected.");
                        else if(args.Length == 1) {
                            TextMessage(HandSocket, NFTServer.ServerControl.SHUT, "SHUTDOWN");
                            state.Connected = false;
                        }
                        else {
                            CommandInput(CloFunc.slice(args,1,args.Length-1));
                            TextMessage(HandSocket, NFTServer.ServerControl.SHUT, "SHUTDOWN");
                            state.Connected = false;
                        }
                        break;
                    }
                    case "-e":
                    case "exit": {
                        if(args.Length == 1) ExitProcess();
                        else {
                            CommandInput(CloFunc.slice(args,1,args.Length-1));
                            ExitProcess();
                        }
                        break;
                    }
                    case "disconnect": {
                        if (!state.Connected) CloFunc.Errorinfo("None Servers Connected.");
                        else {
                            DisconnectProcess();
                            if (args.Length > 2) {
                            CommandInput(CloFunc.slice(args,1,args.Length-1));
                            }
                        }
                        break;
                    }
                    case "Supload": {
                        if (!state.Connected) CloFunc.Errorinfo("None Server Connected.");
                        else {
                            foreach (string filename in args.Skip(1).ToArray()) {
                                try
                                {
                                    UploadProcess(filename);
                                }
                                catch
                                {
                                    CloFunc.Errorinfo("UpLoad File Fail.");
                                }
                            }
                        }
                        break;
                    }
                    default: {
                        CloFunc.Errorinfo("Unexpected arguments");
                        ConsoleMessage("Use \"help\" to get arguments help.");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CloFunc.Errorinfo("Unexpected arguments' error.");
                Console.WriteLine(e.Message);
            }
        }
        private void DisconnectProcess() {
            if (!state.Connected) {
                CloFunc.Errorinfo("YOU SHOULD CONNECT TO SERVER FIRST.");
            }
            else {
                HandSocket.Close();
                HandSocket = NewSocket();
                state.Connected = false;
                state.login = false;
                state.user = false;
                state.pass = false;
            }
        }
        private void UploadProcess(string filename) {
            string filepath = Config.filepath + @"\" + filename;
            if (!File.Exists(filepath)) {
                ConsoleMessage("File Does Not Exist.");
            }
            // not exists or "Y"
            else {
                fp = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                TextMessage(HandSocket, NFTServer.ServerControl.ULFL, filename);
                state.upload = true;
                ReceiveMessage();
            }
        }
        private void DownloadProcess(string filename) {
            string filepath = Config.filepath + @"\" + filename;
            if (File.Exists(filepath)) {
                ConsoleMessage("FileName Already Exists. Do you want to rewrite it? (y / n)");
                string temp = Console.ReadLine();
                if (temp == "y") {
                    ////////
                    ConsoleMessage("ReWriting...");
                }
                else {
                    //temp == "n" or anything.
                    //state.download = false;
                    ConsoleMessage("Download Cancel.");
                    return;//exit.
                }
            }
            // not exists or "Y"
            fp = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            TextMessage(HandSocket, NFTServer.ServerControl.DLFL, filename);
            state.download = true;
            while (state.download) {
                ReceiveMessage();
            }
            //////////////////////////
            //ReceiveMessage();
            //////////////////////////
        }
        private Socket NewSocket() {
            return new Socket(AddressFamily.InterNetwork , SocketType.Stream, ProtocolType.Tcp);
        }
        private void ExitProcess() {
            if (state.Connected) {
                HandSocket.Close();
            }
            Console.WriteLine("Now Exiting...");
            state.exit = true;
        }
        private void ReceiveMessage() {
            byte[] buffer = new byte[2048];
            //HandSocket.Receive(buffer);
            int MessageCount;
            NFTServer.ServerControl Control;
            MessageCount = HandSocket.Receive(buffer);
            if ( MessageCount == 0)
            {
                //Thread exit
            }
            else if(!Enum.IsDefined(typeof(NFTServer.ServerControl), (int)buffer[0]))
            {
                CloFunc.Errorinfo("Error: Undefined Control Head.");
            }
            else
            {
                Control = (NFTServer.ServerControl)buffer[0];
                //Message = Encoding.Default.GetString(buffer, 1, MessageCount - 1);
                MessageHandle(Control, buffer, MessageCount-1);
            }
        }
        private void MessageHandle(NFTServer.ServerControl Control, byte[] Message, int MessageCount) {
            switch(Control) {
                case NFTServer.ServerControl.DBUG: {
                    Console.WriteLine(Encoding.Default.GetString(Message, 1, MessageCount));
                    break;
                }
                case NFTServer.ServerControl.ERRO: {
                    ErrorHandle();
                    CloFunc.Errorinfo(ByteEncoding(Message, MessageCount));
                    
                    break;
                }
                case NFTServer.ServerControl.TEXT: {
                    ConsoleMessage(ByteEncoding(Message, MessageCount));
                    break;
                }
                case NFTServer.ServerControl.LIST: {
                    MessageTemp += ByteEncoding(Message, MessageCount);
                    break;
                }
                case NFTServer.ServerControl.CDCD: {
                    Config.serverpath = ByteEncoding(Message, MessageCount);
                    break;
                }
                case NFTServer.ServerControl.LEND: {
                    state.list = false;
                    ConsoleMessage(MessageTemp);
                    MessageTemp = "";
                    break;
                }
                case NFTServer.ServerControl.DDEL: {
                    //state.deldir = false;
                    ConsoleMessage(ByteEncoding(Message, MessageCount));
                    break;
                }
                case NFTServer.ServerControl.DLFL: {
                    if (state.download) {
                        string filename = ByteEncoding(Message, MessageCount);
                        TextMessage(HandSocket, NFTServer.ServerControl.FSTT, filename);
                    }
                    break;
                }
                case NFTServer.ServerControl.ULFL: {
                    if (state.upload) {
                        string filename = ByteEncoding(Message, MessageCount);
                        Config.FileLength = fp.Length;
                        tempLength = 0;
                        TextMessage(HandSocket, NFTServer.ServerControl.FSTT, fp.Length.ToString());
                        ////////////////////////////////////////
                        long count = (Config.FileLength + 2046) / 2047;
                        byte[] messagebuff = new byte[2047];
                        byte[] finalmessage = new byte[(int)(fp.Length%2047)];
                        Console.Write("Process:{0} / {1}", tempLength, Config.FileLength);
                        //Console.CursorVisible = false;
                        for (long i=0; i < count ; ++i) {
                            if (i!=count-1) {
                                tempLength += 2047;
                                fp.Read(messagebuff,0,2047);
                                ByteMessage(NFTServer.ServerControl.FILE, messagebuff);
                            }
                            else {
                                tempLength += finalmessage.Length;
                                fp.Read(finalmessage,0,finalmessage.Length);
                                ByteMessage(NFTServer.ServerControl.FEND, finalmessage);
                            }
                            //Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("Process:{0} / {1}", tempLength, Config.FileLength);
                        }
                        //Console.SetCursorPosition(0, Console.CursorTop);
                        Console.WriteLine("Process:{0} / {1}", tempLength, Config.FileLength);
                        //Console.CursorVisible = true;
                        //TextMessage(HandSocket, NFTServer.ServerControl.FEND, "FL END");
                        state.upload = false;
                        fp.Close();
                    }
                    break;
                }
                case NFTServer.ServerControl.FSTT: {
                    if (state.download) {
                        Config.FileLength = long.Parse(ByteEncoding(Message, MessageCount));
                        //Console.CursorVisible = false;
                        tempLength = 0;
                        Console.Write("Process:{0} / {1}", tempLength, Config.FileLength);
                    }
                    else if (state.upload) {
                        ///////
                    }
                    ///////////
                    break;
                }
                case NFTServer.ServerControl.FILE: {
                    if (state.download) {
                        fp.Write(Message,1,MessageCount);
                        //Console.SetCursorPosition(0, Console.CursorTop);
                        tempLength += MessageCount;
                        //////////////////////////////////////
                        if (tempLength == Config.FileLength) {
                            state.download = false;
                        }
                        //////////////////////////////////////
                        Console.Write("Process:{0} / {1}", tempLength, Config.FileLength);
                    }
                    break;
                }
                case NFTServer.ServerControl.FEND: {
                    if (state.download) {
                        fp.Write(Message, 1, MessageCount);
                        tempLength += MessageCount;
                        fp.Close();
                        state.download = false;
                        //Console.SetCursorPosition(0, Console.CursorTop);
                        //tempLength += MessageCount;
                        Console.WriteLine("Process:{0} / {1}", tempLength, Config.FileLength);
                        //Console.CursorVisible = true;
                    }
                    break;
                }
                default: {
                    CloFunc.Errorinfo("Unknown Control Head");
                    break;
                }
            }
        }
        private void ErrorHandle() {
            if (!state.login&&state.user&&state.pass) {
                state.user = false;//login -- password or username error
                state.pass = false;
                //CloFunc.Errorinfo("Password or Username fales.");
            }
            if (state.list) {
                state.list = false;
            }
            if (state.download) {
                fp.Close();
                string filename = fp.Name;
                try {
                    File.Delete(filename);
                }
                catch {
                    Console.WriteLine("Can't Delete >{0}.",filename);
                }
                state.download = false;
            }
            if (state.upload) {
                fp.Close();
                state.upload = false;
            }
        }
        private void ConsoleMessage(string Message) {
            Console.WriteLine(Message);
        }
        private string ByteEncoding(byte[] Message, int MessageCount) {
            return Encoding.Default.GetString(Message, 1, MessageCount);
        }
        private void DebugMission() {
            TextMessage(HandSocket, NFTServer.ServerControl.DBUG, "Debug Message.");
            ReceiveMessage();
        }
        public void Close() {
            DisconnectProcess();
        }
        public int AsynTransmis(byte[] Message) {
            if(!state.Connected)return -2;
            if(state.transmission)return -1;
            Transmis = new Thread(new ParameterizedThreadStart(AsynTransmission));
            Transmis.Start(Message);
            //end
            return 0;
        }
        private void AsynTransmission(object obj) {
            byte[] Message = obj as byte[];
            //package
            ////////////////////////////////////////
            state.transmission = true;
            long count = (Message.Length + 2046) / 2047;
            byte[] messagebuff = new byte[2047];
            byte[] finalmessage = new byte[(int)(Message.Length%2047)];
            for (long i=0; i < count ; ++i) {
                if (i!=count-1) {
                    messagebuff = Message.Skip((int)i*2047).Take(2047).ToArray();
                    ByteMessage(NFTServer.ServerControl.BYTE, messagebuff);
                }
                else {
                    finalmessage = Message.Skip((int)i*2047).Take(finalmessage.Length).ToArray();
                    ByteMessage(NFTServer.ServerControl.BEND, finalmessage);
                }
            }
            state.transmission = false;
        }
        public int Transmission(byte[] Message) {
            if(!state.Connected)return -2;
            if(state.transmission)return -1;
            //package
            ////////////////////////////////////////
            state.transmission = true;
            long count = (Message.Length + 2046) / 2047;
            byte[] messagebuff = new byte[2047];
            byte[] finalmessage = new byte[(int)(Message.Length%2047)];
            for (long i=0; i < count ; ++i) {
                if (i!=count-1) {
                    messagebuff = Message.Skip((int)i*2047).Take(2047).ToArray();
                    ByteMessage(NFTServer.ServerControl.BYTE, messagebuff);
                }
                else {
                    finalmessage = Message.Skip((int)i*2047).Take(finalmessage.Length).ToArray();
                    ByteMessage(NFTServer.ServerControl.BEND, finalmessage);
                }
            }
            state.transmission = false;
            return 0;
        }
        private void ByteMessage(NFTServer.ServerControl Control, byte[] Message) {
            byte[] textbuffer = new byte[1 + Message.Length];
            textbuffer[0] = (byte)Control;
            Message.CopyTo(textbuffer, 1);
            HandSocket.Send(textbuffer);
        }
        private void TextMessage(Socket MessageSocket, NFTServer.ServerControl Control, string Message) {
            //these code is based on "ErrorMessage" function
            byte[] text = System.Text.Encoding.Default.GetBytes(Message);
            byte[] textbuffer = new byte[1 + text.Length];
            textbuffer[0] = (byte)Control;
            text.CopyTo(textbuffer, 1);
            MessageSocket.Send(textbuffer);
        }
        private void checkfilepath() {
            if (!Directory.Exists(Config.filepath)) {
                try
                {
                    Directory.CreateDirectory(Config.filepath);
                    Console.WriteLine("filepath created.");
                }
                catch
                {
                    CloFunc.Errorinfo("Filepath Create Crash.");
                }
            }
            else {
                Console.WriteLine("Filepath Exists...OK.");
            }
        }
    }
}