using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Timers;

namespace Demo1
{
    class Room
    {
        private Game game = new Game();
        private List<Player> players = new List<Player>();
        private int roomID;

        //private Dictionary<string, Socket> playerSockets = new Dictionary<string, Socket>();

        private DateTime deleteReserveTime;
        private bool bDestroyRoom;

        public Room(int ID)
        {
            roomID = ID;
            bDestroyRoom = false;
        }

        public int RoomID => roomID;
        public bool DestroyRoom => bDestroyRoom;
        public List<Player> Players => players;

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            game.AddPlayer(player);
        }

        public bool RemovePlayer(Player player)
        {
            if(Players.Contains(player))
            {
                Players.Remove(player);
            }

            if (players.Count <= 0)
            {
                bDestroyRoom = true;
                deleteReserveTime = DateTime.Now.AddSeconds(10);
            }

            return true;
        }

        public int GetCurrentPlayerCount()
        {
            return players.Count;
        }

        public DateTime GetDeleteReserveTime()
        {
            return deleteReserveTime;
        }
        
        public GAME_PHASE GetGamePhase()
        {
            return game.GetCurrentPhase();
        }
        public void Update()
        {
            if(Players.Count == 2)
            {
                CheckGameStart();
            }
        }

        public void CheckGameStart()
        {
            int readyCompleteCount = 0;
            foreach(var player in Players)
            {
                if (player.GetReady())
                    ++readyCompleteCount;
            }

            if (readyCompleteCount < 2)
                return;

            if (game.GetCurrentPhase() == GAME_PHASE.GAME_PHASE_READY)
            {
                IPacket sendPacket = new GameStartPacket
                {
                    Message = String.Format("Game start soon!!")
                };
                BroadcastPacket(sendPacket);
            }

            game.StartGame();
        }

        public void BroadcastPacket(IPacket packet)
        {
            byte[] responseBytes = packet.ToByteArray();

            foreach (var recver in Players)
            {
                //if(player != null && player.Name == recver.Name)
                //    continue;

                var args = new SocketAsyncEventArgs();
                args.SetBuffer(responseBytes, 0, responseBytes.Length);
                args.UserToken = recver.GetSocket();
                args.Completed += (sender, e) => {};

                recver.GetSocket().SendAsync(args);
            }
        }
    }
    class RoomManager
    {
        private Dictionary<int, Room> rooms = new Dictionary<int, Room>();
        private int nextRoomID = 1;
        private Thread tickThread;
        private bool isRunning = false;

        public RoomManager()
        {
            StartTickThread();
        }
        public int GetCurrentRoomCount()
        {
            return rooms.Count;
        }

        public Room CreateRoom(Player player)
        {
            if (rooms.ContainsKey(nextRoomID))
                return null;

            Room room = new Room(nextRoomID);
            rooms.Add(nextRoomID, room);
            room.AddPlayer(player);
            nextRoomID++;
            return room;
        }
        public Room GetRoom(int roomID)
        {
            rooms.TryGetValue(roomID, out Room room);
            return room;
        }
        public void AddPlayerToRoom(int roomID, Player player)
        {
            Room room = GetRoom(roomID);
            room?.AddPlayer(player);
        }

        public void LeavePlayerToRoom(int roomID, Player player)
        {
            rooms.TryGetValue(roomID, out Room room);
            if(null != room)
            {
                room.RemovePlayer(player);
            }
        }
        
        private void StartTickThread()
        {
            isRunning = true;
            tickThread = new Thread(new ThreadStart(Tick));
            tickThread.Start();
        }

        private void Tick()
        {
            while (isRunning)
            {
                foreach (var room in rooms.Values)
                {
                    room.Update();
                    if (true == room.DestroyRoom)
                    {
                        if (room.GetDeleteReserveTime() < DateTime.Now)
                            rooms.Remove(room.RoomID);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            isRunning = false;
            tickThread.Join();
        }
    }
}
