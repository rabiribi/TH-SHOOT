using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using CloFunCC;
using UnityEngine;
using CCoder;

namespace NetFileTSever
{
    public class NFTServer
    {
        public enum ServerControl
        {
            USER,//user name
            PASS,//password
            ERRO,//error message
            DBUG,//for debug/test
            TEXT, //string message
            LIST, //LS command
            CDCD, //CD command
            FILE, //FILE STREAM
            LEND, //LIST END
            CDIR, //Create new dir
            DDEL, //Delete Directory
            FDEL, //Delete File
            ULFL, //Upload file
            DLFL, //Download file
            FSTT, //FILE START
            FEND, //FILE END
            SHUT, //SHUTDOWN SERVER //////
            SIGN, //sign up
            RPAS, //reset password
            BYTE, //just byte package
            BEND  //byte package end
        };
        enum UserGroup {
            Guest,
            Root,
            General,
            Stranger
        };
        struct ServerConfig
        {
            public IPAddress LocalAddress;
            public int LPort;
            public int SocketNum;
            public int bufferSize;
            public string logpath;
            public string userpath;
            public string filepath;
            public bool SHUTDOWN;
        };
        struct ServerState
        {
            //public bool Connected;
            public bool Listen;
            public bool login;
            public bool user;
            public bool pass;
            public bool Download;
            public bool Upload;
            public bool exit;
            public bool transmission;
        };
        private class ThreadStruct
        {
            public Thread MessageThread;
            public Socket ThreadSocket;
            public ServerState ThreadState;
            public UserGroup usergroup;
            public string username;
            public int password;
            public string filepath;
            public FileStream fp;
            public byte[] ByteBuff;
        };
        //const int ControlSize;
        Socket ListenSocket;
        Socket FileSocket;
        ServerConfig Config;
        //ServerState State;
        private Dictionary<IPAddress, ThreadStruct> Dict;
        Thread MessageThread;
        Thread ListenThread;
        //CCoder.DeCoder ServerDecoder;
        public NFTServer()
        {
            Config.logpath = @"log\log.txt";
            Config.userpath = @"log\user.txt";
            Config.filepath = @"file";
            checklogpath();
            checkuserpath();
            checkfilepath();
            Config.LPort = 33491;
            Config.SocketNum = 10;
            Config.bufferSize = 2048;
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //ListenSocket.Blocking = false;//non-blocking mode
            Dict = new Dictionary<IPAddress, ThreadStruct>();
            //Config.LocalAddress = IPAddress.Parse(CloFunc.HostIP());
        }
        public void run()
        {
            //Console.WriteLine(Config.LocalAddress.AddressFamily);
            IPEndPoint ListenPoint = new IPEndPoint(IPAddress.Any, Config.LPort);
            ListenSocket.Bind(ListenPoint);
            //int LocalAddress
            Config.LocalAddress = ((IPEndPoint)ListenSocket.LocalEndPoint).Address;
            Console.WriteLine("Local IP : " + Config.LocalAddress);
            ListenSocket.Listen(Config.SocketNum);
            ///////thread way
            //ServerListen();
            ListenThread = new Thread(new ThreadStart(ServerListen));
            ListenThread.Start();
            ///////
            Console.WriteLine("Exiting...OK.");
        }
        public void run(int port)
        {
            Config.LPort = port;
            this.run();
            Debug.Log("Server EXIT");
        }
        public string GetClientAddress(){
            Debug.Log(((IPEndPoint)FileSocket.RemoteEndPoint).Address.ToString());
            return ((IPEndPoint)FileSocket.RemoteEndPoint).Address.ToString();
        }
        // private void AsynServerListen(IAsyncResult ar){
        //     ListenSocket = (Socket) ar.AsyncState;
        //     FileSocket = ListenSocket.EndAccept(ar);
        // }
        private void ServerListen()
        {
            //State.Listen = true;
            Console.WriteLine("Waiting...");
            while(!Config.SHUTDOWN)
            {
                try {
                //ListenSocket.BeginAccept(new AsyncCallback(AsynServerListen),ListenSocket);
                FileSocket = ListenSocket.Accept();
                //////////////!!!!!!!!!!!!!!!
                //Config.SHUTDOWN = true;
                Debug.Log("Server Accept");
                //////////////!!!!!!!!!!!!!!!
                }
                catch (Exception e) {
                    if (Config.SHUTDOWN) break;
                    else Console.WriteLine(e.ToString());
                }
                try
                {
                    ThreadStruct AS = new ThreadStruct();
                    IPAddress Address = getAddress(FileSocket);
                    Dict.Add(((IPEndPoint)FileSocket.RemoteEndPoint).Address, AS);
                    //Dict.Add(((IPEndPoint)FileSocket.RemoteEndPoint).Address,FileSocket);
                    Dict[Address].ThreadSocket = FileSocket;
                    Dict[Address].ThreadState.Listen = true;
                    Dict[Address].usergroup = UserGroup.Stranger;
                    Dict[Address].filepath = "";
                    log("Client" + " " + FileSocket.RemoteEndPoint + " " + "connected.");
                    MessageThread = new Thread( new ParameterizedThreadStart(MessageTransport));
                    Dict[Address].MessageThread = MessageThread;
                    MessageThread.Start(FileSocket);//FileSocket
                }
                catch(Exception e)
                {
                    CloFunc.Errorinfo("Listening Fail :\n" + e.ToString());
                    //same ip adress try to connect twice is illegal
                }
                
            }
        }
        private void MessageTransport(object obj)
        {
            Socket MessageSocket = obj as Socket;
            IPAddress Address = getAddress(MessageSocket);
            IPEndPoint IPinfo = (IPEndPoint)MessageSocket.RemoteEndPoint;
            byte[] buffer = new byte[2048];
            int MessageCount;
            ServerControl Control;
            ////////////
            //MessageSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            ////////////
            //string Message;
            while(!Dict[Address].ThreadState.exit)
            {
                //////
                //Console.WriteLine("Waiting for message.");
                //////
                try {
                    MessageCount = MessageSocket.Receive(buffer);
                    //Debug.Log("Server Receive");
                }
                catch {
                    ////////
                    break;
                }
                if ( MessageCount == 0)
                {
                    //Thread exit
                    break;
                }
                if(!Enum.IsDefined(typeof(ServerControl), (int)buffer[0]))
                {
                    log("Error: " + MessageSocket.RemoteEndPoint + " Undefined Control Head.");
                }
                else
                {
                    Control = (ServerControl)buffer[0];
                    //Message = Encoding.Default.GetString(buffer, 1, MessageCount - 1);
                    if(AuthorityCheck(Control, Dict[getAddress(MessageSocket)].usergroup)) {
                        /////////////
                        //Console.WriteLine("messagesocket start");
                        /////////////
                        try {
                            MessageHandle(MessageSocket, Control, buffer, MessageCount-1);
                        }
                        catch (Exception e) {
                            CloFunc.Errorinfo("MessageHandle Error :\n" + e.ToString());
                        }
                    }
                    else {
                        ErrorMessage(MessageSocket, "Request Denied.");
                    }
                }
            }
            log("Client" + " " + IPinfo + " " + "disconnected.");
            ////////////////////////////
            Dict[Address].ThreadSocket.Close();
            Dict.Remove(Address);
            ////////////////////////////
        }
        private bool AuthorityCheck(ServerControl Control, UserGroup usergroup) {
            if (usergroup == UserGroup.Root) return true;
            else if (usergroup == UserGroup.Stranger) {
                ServerControl[] A = {
                    ServerControl.USER,
                    ServerControl.PASS,
                    ServerControl.BYTE,
                    ServerControl.BEND
                };
                return -1 != Array.IndexOf(A, Control);//if "control" is not in "A" , then "indexof" will return -1.
            }
            else if (usergroup == UserGroup.Guest) {
                ServerControl[] A = {
                    ServerControl.USER,
                    ServerControl.PASS,
                    ServerControl.DBUG,
                    ServerControl.CDCD,
                    ServerControl.LIST
                };
                return -1 != Array.IndexOf(A, Control);
            }
            else if (usergroup == UserGroup.General) {
                ServerControl[] A = {
                    ServerControl.USER,
                    ServerControl.PASS,
                    ServerControl.DBUG,
                    ServerControl.DLFL,
                    ServerControl.ULFL,
                    ServerControl.FSTT,
                    ServerControl.FILE,
                    ServerControl.FEND,
                    ServerControl.CDCD,
                    ServerControl.LIST,
                    ServerControl.RPAS
                };
                return -1 != Array.IndexOf(A, Control);
            }
            return false;
        }
        private void MessageHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount)
        {
            ///////
            //Debug.Log("MessageHandle");
            ///////
            IPAddress Address = getAddress(MessageSocket);
            switch(Control)
            {
                case ServerControl.USER:{
                    if(Dict[Address].ThreadState.login||Dict[Address].ThreadState.user||Dict[Address].ThreadState.pass)
                    {
                        if(Dict[Address].ThreadState.login)ErrorMessage(MessageSocket, "Already login.");
                        else if(Dict[Address].ThreadState.user||Dict[Address].ThreadState.pass)ErrorMessage(MessageSocket, "Don't push user twice.");
                    }
                    else
                    {
                        Dict[Address].username = System.Text.Encoding.Default.GetString(Message, 1, MessageCount);
                        Dict[Address].ThreadState.user = true;
                        TextMessage(MessageSocket, ServerControl.TEXT, "Please input pass <password>");
                    }
                    break;
                }
                case ServerControl.PASS:{
                    if(Dict[Address].ThreadState.login){
                        ErrorMessage(MessageSocket,"Already login.");
                    }
                    else if(!Dict[Address].ThreadState.user){
                        ErrorMessage(MessageSocket,"Need User information.");
                    }
                    else if(Dict[Address].ThreadState.pass){
                        ErrorMessage(MessageSocket,"Don't push pass twice.");
                    }
                    else {
                        Dict[Address].password = int.Parse(Encoding.Default.GetString(Message, 1, MessageCount));//if encoding is not int , then prase will throw error.
                        Dict[Address].ThreadState.pass = true;
                        //login message
                        Dict[Address].usergroup = login(Dict[Address].username, Dict[Address].password);
                        if (Dict[Address].usergroup != UserGroup.Stranger) {
                            Dict[Address].ThreadState.login = true;
                            TextMessage(MessageSocket, ServerControl.TEXT, "Login Sucessed.");
                            log(Address + " : " + Dict[Address].username + " sign in.");
                        }
                        else {
                            Dict[Address].ThreadState.user = false;
                            Dict[Address].ThreadState.pass = false;
                            ErrorMessage(MessageSocket, "Username / Password Wrong.");
                        }
                    }
                    break;
                }
                case ServerControl.DBUG:{
                    TextMessage(MessageSocket, ServerControl.DBUG, "DEBUG Message.");
                    break;
                }
                case ServerControl.LIST:{
                    ListHandle(MessageSocket, Control, Message, Address);
                    break;
                }
                case ServerControl.CDIR: {
                    CdirHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.CDCD: {
                    CdHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.DDEL: {
                    DdelHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.FDEL: {
                    FdelHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.DLFL: {
                    DlflHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.ULFL: {
                    UlflHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.FSTT: {
                    if (Dict[Address].ThreadState.Download) FsttHandle(MessageSocket, Control, Message, MessageCount, Address);
                    else if (Dict[Address].ThreadState.Upload) FsttHandle4UF(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.FILE: {
                    if (Dict[Address].ThreadState.Upload) FileHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.FEND: {
                    if (Dict[Address].ThreadState.Upload) FendHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.SHUT: {
                    SHUTHandle();
                    break;
                }
                case ServerControl.SIGN: {
                    SignHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.RPAS: {
                    RpasHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.BYTE: {
                    ByteHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                case ServerControl.BEND: {
                    BendHandle(MessageSocket, Control, Message, MessageCount, Address);
                    break;
                }
                default: {
                    ErrorMessage(MessageSocket, "Unknown Request.");
                    break;
                }
            }
        }
        private void ThrowByteBuff(IPAddress Address) {
            //Throw ByteBuff Here
            //Debug.Log("ThrowBuff");
	        //Debug.Log(System.Text.Encoding.Default.GetString(Dict[Address].ByteBuff));
            DeCoder.DeCodeMessage(Dict[Address].ByteBuff);
	        //Throw ByteBuff End
        }
        private void BendHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                if(Dict[Address].ThreadState.transmission){
                    Dict[Address].ByteBuff = CloFunc.Bytecat(Dict[Address].ByteBuff, Message.Skip(1).ToArray());
                    Dict[Address].ThreadState.transmission = false;
                    ThrowByteBuff(Address);
                }
                else {
                    //single byte[]
                    //Debug.Log("?//?");
                    Dict[Address].ByteBuff = Message.Skip(1).ToArray();
                    ThrowByteBuff(Address);
                }
            }
            catch {
                //CloFunc.Errorinfo("Unexpected Byte Fail.");
            }
        }
        private void ByteHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                if(Dict[Address].ThreadState.transmission){
                    Dict[Address].ByteBuff = CloFunc.Bytecat(Dict[Address].ByteBuff, Message.Skip(1).ToArray());
                }
                else {
                    Dict[Address].ThreadState.transmission = true;
                    Dict[Address].ByteBuff = Message.Skip(1).ToArray();
                }
            }
            catch {
                //CloFunc.Errorinfo("Unexpected Byte Fail.");
            }
        }
        private void RpasHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                string ThisMessage = ByteEncoding(Message, MessageCount);
                StreamReader logincp = new StreamReader(Config.userpath);//login check process
                int tempindex = 0;
                string stemp = logincp.ReadLine();
                string[] temp = stemp.Split(' ');//split string with whitespace.
                stemp = logincp.ReadLine();
                while(stemp != null){
                    ++tempindex;
                    temp = stemp.Split(' ');
                    if(temp[0] == Dict[Address].username) {
                        break;
                    }
                    stemp = logincp.ReadLine();
                }
                logincp.Close();
                string[] Alllines = File.ReadAllLines(Config.userpath);
                Alllines[tempindex] = Dict[Address].username + ' ' + ThisMessage;
                File.WriteAllLines(Config.userpath, Alllines);
                TextMessage(MessageSocket, ServerControl.TEXT, "ResetPassword Success.");
            }
            catch {
                //
                CloFunc.Errorinfo("Unexpected RPAS Fail.");
            }
        }
        private void SignHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                string ThisMessage = ByteEncoding(Message, MessageCount);
                string[] MessageArr = ThisMessage.Split(' ').ToArray();
                if(IsUserExists(MessageArr[0])) {
                    ErrorMessage(MessageSocket, "USER EXISTS.");
                }
                else {
                    FileStream fp = File.Open(Config.userpath, FileMode.Append, FileAccess.Write);
                    fp.Write(Message, 1, MessageCount);
                    fp.Close();
                    TextMessage(MessageSocket, ServerControl.TEXT, "Sign Up Success.");
                }
            }
            catch {
                //
                CloFunc.Errorinfo("Unexpected SIGN Fail.");
            }
        }
        private void FendHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                Dict[Address].fp.Write(Message,1,MessageCount);
                log(Address +" > "+ Dict[Address].fp.Name + " Upload Over.");
                Dict[Address].fp.Close();
                Dict[Address].ThreadState.Upload = false;
            }
            catch {
                CloFunc.Errorinfo("Unexpected FILE Fail.");
            }
        }
        private void FileHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            try {
                Dict[Address].fp.Write(Message, 1, MessageCount);
            }
            catch {
                CloFunc.Errorinfo("Unexpected FILE Fail.");
            }
        }
        private void FsttHandle4UF(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
        }
        private void FsttHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Split('\\').ToArray().Length > 1) ErrorMessage(MessageSocket, "Do not contain directory in <file>.");
            else try {
                if(!File.Exists(path)) {
                    ErrorMessage(MessageSocket, "File does not Exist.");
                }
                else {
                    try {
                        FileStream fp = new FileStream(path, FileMode.Open, FileAccess.Read);//, FileShare.Read);
                        byte[] messagebuff = new byte[2047];
                        TextMessage(MessageSocket, ServerControl.FSTT, fp.Length.ToString());
                        long count = (fp.Length + 2046) / 2047 ;
                        byte[] finalmessage = new byte[(int)(fp.Length%2047)];
                        for (long i=0; i < count ; ++i) {
                            if (i+1 != count) {
                                fp.Read(messagebuff,0,2047);
                                ByteMessage(MessageSocket, ServerControl.FILE, messagebuff);
                            }
                            else {
                                fp.Read(finalmessage, 0, finalmessage.Length);
                                ByteMessage(MessageSocket, ServerControl.FEND, finalmessage);
                            }
                        }
                        /////////ByteMessage & TextMessage may be cat by Socket or FTP///////
                        /////////cause an unrepairable bug
                        /////////so add an [meaningless] operation here to avoid bug
                        log(Address +" < "+ path + " Download Over.");
                        ////////////////////////////////////////
                        //TextMessage(MessageSocket, ServerControl.FEND, "DL END");
                        Dict[Address].ThreadState.Download = false;
                        fp.Close();
                    }
                    catch {
                        ErrorMessage(MessageSocket, "Unexpected Error.");
                    }
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected FSTT Fail:\n" + e.ToString());
            }
        }
        private void UlflHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Split('\\').ToArray().Length > 1) ErrorMessage(MessageSocket, "Do not contain directory in <file>.");
            else try {
                if(File.Exists(path)) {
                    ErrorMessage(MessageSocket, "File Already Exist.");
                }
                else {
                    try {
                        Dict[Address].fp = File.Open(path, FileMode.CreateNew, FileAccess.Write);
                        TextMessage(MessageSocket, ServerControl.ULFL, tpath);
                        Dict[Address].ThreadState.Upload = true;
                    }
                    catch {
                        ErrorMessage(MessageSocket, "Unexpected Error.");
                    }
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected DLFL Fail:\n" + e.ToString());
            }
        }
        private void DlflHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Split('\\').ToArray().Length > 1) ErrorMessage(MessageSocket, "Do not contain directory in <file>.");
            else try {
                if(!File.Exists(path)) {
                    ErrorMessage(MessageSocket, "File does not Exist.");
                }
                else {
                    try {
                        TextMessage(MessageSocket, ServerControl.DLFL, tpath);
                        Dict[Address].ThreadState.Download = true;
                    }
                    catch {
                        ErrorMessage(MessageSocket, "Unexpected Error.");
                    }
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected DLFL Fail:\n" + e.ToString());
            }
        }
        public void Close() {
            SHUTHandle();
        }
        private void SHUTHandle() {
            Config.SHUTDOWN = true;
            Console.WriteLine("Try Exiting...");
            
            foreach(var item in Dict) {
                item.Value.ThreadState.exit = true;
            }
            try {
                FileSocket.Close();
                ListenSocket.Close();
            }
            catch (Exception e) {
                CloFunc.Errorinfo(e.ToString());
            }
        }
        private void FdelHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Split('\\').ToArray().Length > 1) ErrorMessage(MessageSocket, "Do not contain directory in <file>.");
            else try {
                if(!File.Exists(path)) {
                    ErrorMessage(MessageSocket, "File does not Exist.");
                }
                else {
                    try {
                        File.Delete(path);
                        TextMessage(MessageSocket, ServerControl.TEXT, "File Deleted.");
                    }
                    catch {
                        ErrorMessage(MessageSocket, "Can't delete <File>. Please make sure it's not occupied.");
                    }
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected FDEL Fail:\n" + e.ToString());
            }
        }
        private void DdelHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Replace(".", String.Empty) == "") ErrorMessage(MessageSocket, "Directory does not Exist.");
            else try {
                if(!Directory.Exists(path)) {
                    ErrorMessage(MessageSocket, "Directory does not Exist.");
                }
                else {
                    try {
                        Directory.Delete(path);
                        TextMessage(MessageSocket, ServerControl.TEXT, "Directory Deleted.");
                    }
                    catch {
                        ErrorMessage(MessageSocket, "Can't delete <directory>. Please make sure there is no file under this directory.");
                    }
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected DDEL Fail:\n" + e.ToString());
            }
        }
        private void CdHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string dopathex;
            if (path.Replace(".", String.Empty) == "") {
                if (path == "..") {
                    if (Dict[Address].filepath == "" || Dict[Address].filepath == null) {
                        ErrorMessage(MessageSocket, "Already at root menu.");
                    }
                    else {
                        string[] pathlist = Dict[Address].filepath.Split('\\');
                        string pathtemp = "";
                        for (int i = 0; i < pathlist.Length - 1; ++i) {
                            if (i != pathlist.Length - 2) pathtemp += pathlist[i] + @"\";
                            else pathtemp += pathlist[i];
                        }
                        Dict[Address].filepath = pathtemp;
                        TextMessage(MessageSocket, ServerControl.CDCD, Dict[Address].filepath);
                    }
                }
                else {
                    /////////////////////////////
                    //Directory like "." mean itself
                    //".." mean parent directory
                    //And "...", "....", ".....", and so on All MEAN parent directory
                    //SO ALL THOSE THINGS IS UNSAFE!!!!!!!
                    /////////////////////////////
                    ErrorMessage(MessageSocket, "Directory does not Exist.");
                }
            }
            else {
                if (Dict[Address].filepath == "") dopathex = Config.filepath + @"\" + path;
                else dopathex = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
                try {
                    if(!Directory.Exists(dopathex)) {
                        ErrorMessage(MessageSocket, "Directory does not Exist.");
                    }
                    else {
                        if (Dict[Address].filepath == "" || path == "") Dict[Address].filepath += path;
                        else Dict[Address].filepath += @"\" + path;
                        TextMessage(MessageSocket, ServerControl.CDCD, Dict[Address].filepath);
                    }
                }
                catch (Exception e) {
                    CloFunc.Errorinfo("UnExpected CDIR Fail: " + e.ToString());
                }
            }
        }
        private void CdirHandle(Socket MessageSocket, ServerControl Control, byte[] Message, int MessageCount, IPAddress Address) {
            string path = ByteEncoding(Message, MessageCount);
            string tpath = path;
            path = Config.filepath + @"\" + Dict[Address].filepath + @"\" + path;
            if (tpath.Replace(".", String.Empty) == "") ErrorMessage(MessageSocket, "Illegal <directory>");
            else try {
                if(Directory.Exists(path)) {
                    ErrorMessage(MessageSocket, "Directory Alread Exists.");
                }
                else {
                    Directory.CreateDirectory(path);
                    TextMessage(MessageSocket, ServerControl.TEXT, "Directory Created.");
                }
            }
            catch (Exception e) {
                CloFunc.Errorinfo("UnExpected CDIR Fail: " + e.ToString());
            }
        }
        private void ListHandle(Socket MessageSocket, ServerControl Control, byte[] Message, IPAddress Address) {
            DirectoryInfo info = new DirectoryInfo(Config.filepath + @"\" + Dict[Address].filepath);
            DirectoryInfo[] infolist = info.GetDirectories();
            FileInfo[] Finfolist = info.GetFiles();
            string infoMessage = "";
            //add dir info
            for (int i = 0; i < infolist.Length; ++i) {
                infoMessage += infolist[i].Name;
                if (i+1 != infolist.Length) infoMessage += "\t";
            }
            //add file info
            for (int i = 0; i < Finfolist.Length; ++i) {
                if (i==0 && infolist.Length==0) infoMessage += Finfolist[i].Name;
                else infoMessage += "\t" + Finfolist[i].Name;
            }
            byte[] listMessage = Encoding.Default.GetBytes(infoMessage);
            int count = (listMessage.Length + (2048-2)) / (2048-1) ;//ceil
            byte[] smessage;
            for (int i = 0; i < count; ++i) {
                if (i+1 != count) {
                    smessage = listMessage.Take(2047).ToArray();
                    listMessage = listMessage.Skip(2047).ToArray();
                }
                else smessage = listMessage;
                ByteMessage(MessageSocket, ServerControl.LIST, smessage);
            }
            ///may crash.
            TextMessage(MessageSocket, ServerControl.LEND, "OVER.");
        }
        private bool IsUserExists (string username) {
            if (username == "Guest" || username == "Stranger") return true;
            StreamReader logincp = new StreamReader(Config.userpath);//login check process
            string stemp = logincp.ReadLine();
            string[] temp = stemp.Split(' ');//split string with whitespace.
            stemp = logincp.ReadLine();
            while(stemp != null){
                temp = stemp.Split(' ');
                if(temp[0] == username) {
                    logincp.Close();
                    return true;
                }
                stemp = logincp.ReadLine();
            }
            logincp.Close();
            return false;
        }
        private UserGroup login (string username, int password)
        {
            if (username == "Guest") return UserGroup.Guest;
            StreamReader logincp = new StreamReader(Config.userpath);//login check process
            string stemp = logincp.ReadLine();
            string[] temp = stemp.Split(' ');//split string with whitespace.
            stemp = logincp.ReadLine();
            while(stemp != null){
                temp = stemp.Split(' ');
                if(temp[0] == username && Convert.ToInt32(temp[1]) == password.GetHashCode()) {
                    logincp.Close();
                    return username == "Root" ? UserGroup.Root : UserGroup.General;
                }
                stemp = logincp.ReadLine();
            }
            logincp.Close();
            return UserGroup.Stranger;
        }
        private void ErrorMessage(Socket MessageSocket, string Message)
        {
            byte[] text = System.Text.Encoding.Default.GetBytes(Message);
            byte[] errorbuffer = new byte[1 + text.Length];
            errorbuffer[0] = (byte)ServerControl.ERRO;
            text.CopyTo(errorbuffer, 1);
            MessageSocket.Send(errorbuffer);
        }
        private string ByteEncoding(byte[] Message, int MessageCount) {
            return Encoding.Default.GetString(Message, 1, MessageCount);
        }
        private void ByteMessage(Socket MessageSocket, ServerControl Control, byte[] Message) {
            byte[] textbuffer = new byte[1 + Message.Length];
            textbuffer[0] = (byte)Control;
            Message.CopyTo(textbuffer, 1);
            MessageSocket.Send(textbuffer);
        }
        private void TextMessage(Socket MessageSocket, ServerControl Control, string Message) {
            //these code is based on "ErrorMessage" function
            byte[] text = System.Text.Encoding.Default.GetBytes(Message);
            byte[] textbuffer = new byte[1 + text.Length];
            textbuffer[0] = (byte)Control;
            text.CopyTo(textbuffer, 1);
            MessageSocket.Send(textbuffer);
        }
        private void log(string loginfo)
        {
            loginfo = TimeCode(loginfo);
            Console.WriteLine(loginfo);
            CloFunc.Filelog(Config.logpath,loginfo);
        }
        private string TimeCode(string loginfo)
        {
            return '[' + System.DateTime.Now.ToString("HH:mm:ss") + ']' + ' ' + loginfo;
        }
        private IPAddress getAddress(Socket s) {
            return CloFunc.getAddress(s);
        }
        private void checklogpath()
        {
            //check if log file exists.
            if( !File.Exists(Config.logpath))
            {
                //if log file does not exist, create one.
                //!!! Exists bool may crash.
                Console.WriteLine("Log Unexists.");
                string[] pathtemp = Config.logpath.Split('\\');
                string ptemp = "";
                for (int i = 0; i < pathtemp.Length - 1; i++)
                {
                    ptemp = ptemp + pathtemp[i];
                }
                if( !File.Exists(ptemp))
                {
                    try
                    {
                        Directory.CreateDirectory(ptemp);
                    }
                    catch
                    {
                        CloFunc.Errorinfo("Path Create Crash.");
                    }
                }
                try
                {
                    File.Create(Config.logpath).Close();
                    Console.WriteLine("log path created.");
                }
                catch
                {
                    CloFunc.Errorinfo("log File Create Crash.");
                }
                
            }
            else
            {
                //log
                Console.WriteLine("Log Exists...OK.");
                
            }
        }
        private void checkuserpath()
        {
            //these code is based on function=>checklogpath()
            //check if userlog file exists.
            if( !File.Exists(Config.userpath))
            {
                //if userlog file does not exist, create one.
                //!!! Exists bool may crash.
                Console.WriteLine("Userlog Unexists.");
                string[] pathtemp = Config.userpath.Split('\\');
                string ptemp = "";
                for (int i = 0; i < pathtemp.Length - 1; i++)
                {
                    ptemp = ptemp + pathtemp[i];
                }
                if( !File.Exists(ptemp))
                {
                    try
                    {
                        Directory.CreateDirectory(ptemp);
                    }
                    catch
                    {
                        CloFunc.Errorinfo("Path Create Crash.");
                    }
                }
                try
                {
                    File.Create(Config.userpath).Close();
                    Console.WriteLine("Userlog path created.");
                }
                catch
                {
                    CloFunc.Errorinfo("Userlog File Create Crash.");
                }
                
                //int Root user
                try {
                    StreamWriter fp = new StreamWriter(Config.userpath);
                    fp.WriteLine("#Username #password");
                    fp.WriteLine("Root " + "root".GetHashCode());
                    fp.Close();
                }
                catch {
                    CloFunc.Errorinfo("Userlog int fail.");
                }
                
            }
            else
            {
                //log
                Console.WriteLine("UserLog Exists...OK.");
                
            }
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
