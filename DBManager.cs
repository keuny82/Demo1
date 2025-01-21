using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;


namespace Demo1
{
    public enum DBTaskType
    {
        SignUp,
        Login,
    }
    public class DBTask
    {
        public DBTaskType type { get; set; }
        public TaskCompletionSource<bool> result { get; set; }
        public string PlayerID { get; set; }
        public string Password { get; set; }
    }
    class DBManager
    {
        private readonly string connectionString;
        private Thread dbThread;
        private bool running;
        private ConcurrentQueue<DBTask> taskQueue = new ConcurrentQueue<DBTask>();

        public DBManager(string connectStr)
        {
            connectionString = connectStr;
            running = true;
            dbThread = new Thread(Run);
            dbThread.Start();
        }

        private void Run()
        {
            while(running)
            {
                while(taskQueue.TryDequeue(out var task))
                {
                    switch(task.type)
                    {
                        case DBTaskType.SignUp : task.result.SetResult(SavePlayerLogin(task.PlayerID, task.Password)); break;
                        case DBTaskType.Login : task.result.SetResult(GetPlayerLoginInfo(task.PlayerID, task.Password)); break;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            running = false;
            dbThread.Join();
        }

        public async Task<bool> QueueSaveUserLoginInfoAsync(string playerID, string password)
        {
            var task = new DBTask { PlayerID = playerID, Password = password, type = DBTaskType.SignUp, result = new TaskCompletionSource<bool>() };
            taskQueue.Enqueue(task);
            return await task.result.Task;
        }

        public async Task<bool> QueueGetPlayerLoginAsync(string playerID, string password)
        {
            var task = new DBTask { PlayerID = playerID, Password = password, type = DBTaskType.Login, result = new TaskCompletionSource<bool>() };
            taskQueue.Enqueue(task);
            return await task.result.Task;
        }

        public bool GetPlayerLoginInfo(string playerID, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT COUNT(*) FROM t_PlayerInfo WHERE playerID = @playerID AND password = @password";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerID", playerID);
                        cmd.Parameters.AddWithValue("@password", password);
                        int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                        return cnt > 0;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"DB exception : {ex.Message}");
                return false;
            }
        }

        public bool SavePlayerLogin(string playerID, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "INSERT INTO t_PlayerInfo (playerID, password) VALUES (@playerID, @password)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerID", playerID);
                        cmd.Parameters.AddWithValue("@password", password);

                        int cnt = cmd.ExecuteNonQuery();
                        return cnt == 1;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"DB exception : {ex.Message}");
                return false;
            }
        }
    }
}
