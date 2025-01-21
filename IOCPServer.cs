using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using ProtoBuf;

namespace Demo1
{
    public class BufferManager
    {
        private byte[] buffer;
        private Stack<int> freeIndexPool;
        private int currentIndex;
        private int bufferSize;
        private int numBuffers;

        public BufferManager(int totalBufferSize, int bufferSize)
        {
            this.buffer = new byte[totalBufferSize];
            this.freeIndexPool = new Stack<int>();
            this.currentIndex = 0;
            this.bufferSize = bufferSize;
            this.numBuffers = totalBufferSize / bufferSize;
        }

        public void InitBuffer()
        {
            for(int i = 0; i < this.numBuffers; ++i)
            {
                this.freeIndexPool.Push(i * this.bufferSize);
            }
        }

        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (this.freeIndexPool.Count > 0)
            {
                args.SetBuffer(this.buffer, this.freeIndexPool.Pop(), this.bufferSize);
                return true;
            }
            else
            {
                if ((numBuffers - bufferSize) < currentIndex)
                {
                    return false;
                }
                args.SetBuffer(buffer, currentIndex, bufferSize);
                currentIndex += bufferSize;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            this.freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }

    public class SocketAsyncEventArgsPool
    {
        private ConcurrentStack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            pool = new ConcurrentStack<SocketAsyncEventArgs>(new SocketAsyncEventArgs[capacity]);
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            pool.Push(item);
        }

        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs item;
            pool.TryPop(out item);
            return item;
        }

        public int Count => pool.Count;
    }


    class IOCPServer
    {
        private Socket listenSocket;
        private IPEndPoint localEndPoint;
        private BufferManager bufferManager;
        private SocketAsyncEventArgsPool readWritePool;

        private int maxConnections;
        private readonly ManualResetEvent allDone = new ManualResetEvent(false);

        private DBManager dbManager;
        private GameCardTableLoader CardDic = new GameCardTableLoader();
        private ConcurrentDictionary<Socket, Player> players = new ConcurrentDictionary<Socket, Player>();
        private RoomManager roomManager = new RoomManager();

        public IOCPServer(IPAddress ipAddress, int port, string connectionStr)
        {
            localEndPoint = new IPEndPoint(ipAddress, port);
            dbManager = new DBManager(connectionStr);
            maxConnections = Environment.ProcessorCount * 2;
            bufferManager = new BufferManager(1024 * maxConnections, 1024);
            readWritePool = new SocketAsyncEventArgsPool(maxConnections);
        }
        public void StartServer()
        {
            bufferManager.InitBuffer();

            for (int i = 0; i < maxConnections; ++i)
            {
                SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                bufferManager.SetBuffer(readWriteEventArg);
                readWritePool.Push(readWriteEventArg);
            }

            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(100);

            Console.WriteLine("Waiting for a connection...");

            for(int i = 0; i < maxConnections; ++i)
            {
                StartAccept(null);
            }
        }

        public void Stop()
        {
            listenSocket.Close();
            allDone.Set();
            dbManager.Stop();
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if(acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            allDone.Reset();

            try
            {
                bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"Socket exception : {ex.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unexpected exception : {ex.Message}");
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessAccept(e);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"AcceptCompleted exception : {ex.Message}");
            }
            finally
            {
                allDone.Set();
            }
        }

       private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            Console.WriteLine($"Client connect!!");

            Player newplayer = new Player("", s);
            players[s] = newplayer;
            SocketAsyncEventArgs readEventArgs = readWritePool.Pop();
            readEventArgs.UserToken = s;

            StartReceive(readEventArgs);
            StartAccept(e);
        }

        private void StartReceive(SocketAsyncEventArgs readEventArgs)
        {
            Socket socket = (Socket)readEventArgs.UserToken;

            try
            {
                bool willRaiseEvent = socket.ReceiveAsync(readEventArgs);
                if(!willRaiseEvent)
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"Socket exception : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception : {ex.Message}");
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch(e.LastOperation)
            {
                case SocketAsyncOperation.Receive: ReceiveCompleted(sender, e); break;
                case SocketAsyncOperation.Send: SendCompleted(sender, e); break;
                default: throw new ArgumentException("The last operation completed on the socket was not a receive or send.");
            }
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessReceive(e);
            }         
            catch(Exception ex)
            {
                Console.WriteLine($"ReceiveCompleted exception : {ex.Message}");
            }
            finally
            {
                allDone.Set();
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Socket s = (Socket)e.UserToken;

            if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] data = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);

                if (!players.TryGetValue(s, out Player player))
                    return;

                try
                {
                    IPacket requestPacket = DeserializePacket(data);
                    //Task<IPacket> responseTask = HandleRequestAsync(player, requestPacket);
                    //responseTask.ContinueWith(task =>
                    Task.Run(async () =>
                    {
                        IPacket responsePacket = await HandleRequestAsync(player, requestPacket);

                        if (null != responsePacket)
                        {
                            byte[] responseBytes = responsePacket.ToByteArray();
                            e.SetBuffer(e.Offset, responseBytes.Length);
                            Buffer.BlockCopy(responseBytes, 0, e.Buffer, e.Offset, responseBytes.Length);
                            StartSend(e);
                        }
                        else
                            StartReceive(e);
                        //}, TaskScheduler.FromCurrentSynchronizationContext());
                    });
                }
                catch(IOException ex)
                {
                    Console.WriteLine($"IO Exception : {ex.Message}");
                    s.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Processing Exception : {ex.Message}");
                    s.Close();
                }
            }
            else
            {
                Console.WriteLine("Client Disconnect!!");
                s.Close();
                readWritePool.Push(e);
            }
        }

        private void StartSend(SocketAsyncEventArgs sendEventArgs)
        {
            Socket socket = (Socket)sendEventArgs.UserToken;

            try
            {
                bool willRaiseEvent = socket.SendAsync(sendEventArgs);
                if(!willRaiseEvent)
                {
                    ProcessSend(sendEventArgs);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Exception : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Exception : {ex.Message}");                
            }
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessSend(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendCompleted Exception : {ex.Message}");
            }
            finally
            {
                allDone.Set();
            }
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            Socket s = (Socket)e.UserToken;

            if(e.SocketError == SocketError.Success)
            {
                StartReceive(e);
            }
            else
            {
                Console.WriteLine("Send Fail!!");
                s.Close();
                readWritePool.Push(e);
            }
        }

        private IPacket DeserializePacket(byte[] data)
        {
            ushort packetId = BitConverter.ToUInt16(data, 2);
            IPacket packet;

            switch((PacketID)packetId)
            {
                case PacketID.SIGN_UP_REQUEST: packet = new SignUpRequestPacket(); break;
                case PacketID.LOGIN_REQUEST: packet = new LoginRequestPacket(); break;
                case PacketID.SET_MYINFO_REQUEST: packet = new SetMyInfoRequestPacket(); break;
                case PacketID.ROOM_CREATE_REQUEST: packet = new CreateRoomRequestPacket(); break;
                case PacketID.ROOM_JOIN_REQUEST: packet = new JoinRoomRequestPacket(); break;
                case PacketID.ROOM_LEAVE_REQUEST: packet = new LeaveRoomRequestPacket(); break;
                case PacketID.CHAT_MESSAGE_SEND: packet = new ChatMessageSendPacket(); break;
                case PacketID.ROOM_READY_REQUEST: packet = new PlayerReadyRequestPacket(); break;
                default: throw new InvalidOperationException("Unknown Packet ID!!");
            }
            packet.FromByteArray(data);
            return packet;
        }
        
        private async Task<IPacket> HandleRequestAsync(Player player, IPacket requestPacket)
        {
            switch((PacketID)requestPacket.PacketId)
            {
                case PacketID.SIGN_UP_REQUEST:
                    var signupRequest = (SignUpRequestPacket)requestPacket;
                    {
                        bool signupSuccess = await dbManager.QueueSaveUserLoginInfoAsync(signupRequest.PlayerID, signupRequest.PlayerPW);
                        var signupResponse = new SignUpResponsePacket
                        {
                            SignUpSuccess = signupSuccess,
                            Message = signupSuccess ? "Sign Up Success!!!" : "Sign Up Fail!!!"
                        };
                        Console.WriteLine(signupResponse.Message);
                        return signupResponse;
                    }
                case PacketID.LOGIN_REQUEST:
                    var loginRequest = (LoginRequestPacket)requestPacket;
                    {
                        bool loginSuccess = await dbManager.QueueGetPlayerLoginAsync(loginRequest.PlayerID, loginRequest.PlayerPW);
                        var loginResponse = new LoginResponsePacket
                        {
                            LoginSuccess = loginSuccess,
                            Message = loginSuccess ? "Login Success!!" : "Invalid user ID or Password!!"
                        };
                        Console.WriteLine(loginResponse.Message);
                        return loginResponse;
                    }
                case PacketID.SET_MYINFO_REQUEST:
                    var setMyInfoRequest = (SetMyInfoRequestPacket)requestPacket;
                    {
                        player.Name = setMyInfoRequest.PlayerName;
                        var setMyInfoResponse = new SetMyInfoResponsePacket()
                        {
                            Message = String.Format("Set MyInfo Success!! Name : {0}", player.Name)
                        };
                        Console.WriteLine(setMyInfoResponse.Message);
                        return setMyInfoResponse;
                    }
                case PacketID.ROOM_CREATE_REQUEST:
                    var createRoomRequest = (CreateRoomRequestPacket)requestPacket;
                    {
                        var createRoomResponse = new CreateRoomResponsePacket();
                        if (false == player.IsSetMyInfo())
                        {
                            createRoomResponse.Message = "Set Up Your Name First!";
                            return createRoomResponse;
                        }
                        
                        if (player.GetCurrentRoomID() > 0)
                        {
                            createRoomResponse.RoomId = 0;
                            createRoomResponse.Message = "Already join room";
                            return createRoomResponse;
                        }
                        
                        Room newRoom = roomManager.CreateRoom(player);
                        player.SetCurrentRoomID(newRoom.RoomID);
                        //roomManager.AddPlayerToRoom(newRoom.RoomID, player);
                        createRoomResponse.RoomId = newRoom.RoomID;
                        createRoomResponse.Message = String.Format("RoomNo {0} Create", newRoom.RoomID);
                        Console.WriteLine($"TotalRoomCount : {roomManager.GetCurrentRoomCount()}");
                        return createRoomResponse;
                    }
                case PacketID.ROOM_JOIN_REQUEST:
                    var joinRoomRequest = (JoinRoomRequestPacket)requestPacket;
                    {
                        var joinRoomResponse = new JoinRoomResponsePacket();
                        if (false == player.IsSetMyInfo())
                        {
                            joinRoomResponse.Message = "Set Up Your Name First!";
                            return joinRoomResponse;
                        }

                        if (player.GetCurrentRoomID() > 0)
                        {
                            joinRoomResponse.Message = "Already join room";
                            return joinRoomResponse;
                        }
                        Room findRoom = roomManager.GetRoom(joinRoomRequest.RoomId);
                        if (null == findRoom)
                        {
                            joinRoomResponse.Message = "Don't find room";
                            return joinRoomResponse;
                        }
                        if(findRoom.DestroyRoom)
                        {
                            joinRoomResponse.Message = "This Room will be deleted!!";
                            return joinRoomResponse;
                        }
                        if(findRoom.GetCurrentPlayerCount() >= 2)
                        {
                            joinRoomResponse.Message = "This Room is full!!";
                            return joinRoomResponse;
                        }
                        if(findRoom.GetGamePhase() > GAME_PHASE.GAME_PHASE_READY)
                        {
                            joinRoomResponse.Message = "Game has already started!!";
                            return joinRoomResponse;
                        }
                        player.Name = joinRoomRequest.PlayerName;
                        findRoom.AddPlayer(player);
                        player.SetCurrentRoomID(findRoom.RoomID);
                        joinRoomResponse.RoomId = findRoom.RoomID;
                        joinRoomResponse.Message = String.Format("RoomNo {0} Join Success", findRoom.RoomID);
                        Console.WriteLine($"TotalPlayerCount : {findRoom.GetCurrentPlayerCount()}");

                        IPacket noti = new NotiMessagePacket
                        {
                            Message = String.Format("{0} entered the Room!", player.Name)
                        };
                        findRoom.BroadcastPacket(noti);
                        return joinRoomResponse;
                    }
                case PacketID.ROOM_LEAVE_REQUEST: 
                    var leaveRoomRequest = (LeaveRoomRequestPacket)requestPacket;
                    {
                        var leaveRoomResponse = new LeaveRoomResponsePacket();

                        Room findRoom = roomManager.GetRoom(leaveRoomRequest.RoomId);
                        if (null == findRoom)
                        {
                            leaveRoomResponse.Message = "Don't find room";
                            return leaveRoomResponse;
                        }
                        roomManager.LeavePlayerToRoom(leaveRoomRequest.RoomId, player);
                        player.SetCurrentRoomID(0);
                        leaveRoomResponse.Message = String.Format("RoomNo {0} Leave Success", leaveRoomRequest.RoomId);
                        Console.WriteLine($"TotalRoomCount : {roomManager.GetCurrentRoomCount()}");
                        IPacket noti = new NotiMessagePacket
                        {
                            Message = String.Format("{0} left the Room!", player.Name)
                        };
                        findRoom.BroadcastPacket(noti);
                        return leaveRoomResponse;
                        
                    }
                case PacketID.CHAT_MESSAGE_SEND:
                    var chatMessageSend = (ChatMessageSendPacket)requestPacket;
                    {
                        IPacket chatMessageRecvPacket = new ChatMessageRecvPacket
                        { 
                            Name = player.Name,
                            Message = chatMessageSend.Message
                        };
                        Room findRoom = roomManager.GetRoom(player.GetCurrentRoomID());
                        if (null == findRoom)
                        {
                            Console.WriteLine("Don't find room!!!");
                            return null;
                        }
                        //BroadcastMessage(player.GetCurrentRoomID(), player, chatMessageRecvPacket);
                        findRoom.BroadcastPacket(chatMessageRecvPacket);
                        //return chatMessageRecvPacket;
                        return null;
                    }
                case PacketID.ROOM_READY_REQUEST:
                    var readyRequest = (PlayerReadyRequestPacket)requestPacket;
                    {
                        Room findRoom = roomManager.GetRoom(player.GetCurrentRoomID());
                        if (null == findRoom)
                        {
                            Console.WriteLine("Don't find room!!!");
                            return null;
                        }
                        player.SetReady(readyRequest.Ready);
                        IPacket readyResponsePacket = new PlayerReadyResponsePacket
                        {
                            Message = readyRequest.Ready == true ? String.Format("{0} is Ready Complete!", player.Name) : String.Format("{0} is Ready Cancel!", player.Name)
                        };
                        findRoom.BroadcastPacket(readyResponsePacket);
                        //return readyResponsePacket;
                        return null;
                    }
                default: throw new InvalidOperationException("Unknown Request Type !!!");
            }
        }

        private void BroadcastMessage(int roomID, Player player, IPacket sendPacket)
        {
            var room = roomManager.GetRoom(roomID);
            if (null == room)
                return;
            
            byte[] responseBytes = sendPacket.ToByteArray();

            foreach(var recver in room.Players)
            {
                if (recver.Name == player.Name)
                    continue;

                var args = new SocketAsyncEventArgs();
                args.SetBuffer(responseBytes, 0, responseBytes.Length);
                args.UserToken = recver.GetSocket();
                args.Completed += (sender, e) => { readWritePool.Push(e); };

                recver.GetSocket().SendAsync(args);
            }
        }

        public RoomManager GetRoomManager() { return roomManager; }
    }
}

