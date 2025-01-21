using System;
using System.Text;
using System.IO;
using ProtoBuf;

namespace Demo1
{
    public enum PacketID : ushort
    {
        SIGN_UP_REQUEST,
        SIGN_UP_RESPONSE,
        LOGIN_REQUEST,
        LOGIN_RESPONSE,
        SET_MYINFO_REQUEST,
        SET_MYINFO_RESPONSE,
        ROOM_CREATE_REQUEST,
        ROOM_CREATE_RESPONSE,
        ROOM_JOIN_REQUEST,
        ROOM_JOIN_RESPONSE,
        ROOM_READY_REQUEST,
        ROOM_READY_RESPONSE,
        ROOM_LEAVE_REQUEST,
        ROOM_LEAVE_RESPONSE,
        CHAT_MESSAGE_SEND,
        CHAT_MESSAGE_RECV,
        ROOM_NOTI_MESSAGE,
        GAME_PACKET_MIN,
        GAME_PACKET_START,
        GAME_PACKET_MAX,
    }

    public interface IPacket
    {
        ushort PacketSize { get; }
        ushort PacketId { get; }
        byte[] Body { get; }

        byte[] ToByteArray();
        void FromByteArray(byte[] data);
    }

    [ProtoContract]
    public class SignUpRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.SIGN_UP_REQUEST;
        [ProtoMember(1)]
        public string PlayerID { get; set; }
        [ProtoMember(2)]
        public string PlayerPW { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<SignUpRequestPacket>(ms);
                PlayerID = packet.PlayerID;
                PlayerPW = packet.PlayerPW;
            }
        }
    }

    [ProtoContract]
    public class SignUpResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.SIGN_UP_RESPONSE;
        [ProtoMember(1)]
        public bool SignUpSuccess { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<SignUpResponsePacket>(ms);
                SignUpSuccess = packet.SignUpSuccess;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class LoginRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.LOGIN_REQUEST;
        [ProtoMember(1)]
        public string PlayerID { get; set; }
        [ProtoMember(2)]
        public string PlayerPW { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<LoginRequestPacket>(ms);
                PlayerID = packet.PlayerID;
                PlayerPW = packet.PlayerPW;
            }
        }
    }

    [ProtoContract]
    public class LoginResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.LOGIN_RESPONSE;
        [ProtoMember(1)]
        public bool LoginSuccess { get; set; }
        [ProtoMember(2)]
        public string PlayerName { get; set; }
        [ProtoMember(3)]
        public string Message { get; set; }
        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<LoginResponsePacket>(ms);
                LoginSuccess = packet.LoginSuccess;
                PlayerName = packet.PlayerName;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class SetMyInfoRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.SET_MYINFO_REQUEST;
        [ProtoMember(1)]
        public string PlayerName { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<SetMyInfoRequestPacket>(ms);
                PlayerName = packet.PlayerName;
            }
        }
    }

    [ProtoContract]
    public class SetMyInfoResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.SET_MYINFO_RESPONSE;
        [ProtoMember(1)]
        public string Message { get; set; }
        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<SetMyInfoResponsePacket>(ms);
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class CreateRoomRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_CREATE_REQUEST;

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<CreateRoomRequestPacket>(ms);
            }
        }
    }

    [ProtoContract]
    public class CreateRoomResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_CREATE_RESPONSE;
        [ProtoMember(1)]
        public int RoomId { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<CreateRoomResponsePacket>(ms);
                RoomId = packet.RoomId;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class JoinRoomRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_JOIN_REQUEST;
        [ProtoMember(1)]
        public int RoomId { get; set; }
        [ProtoMember(2)]
        public string PlayerName { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<JoinRoomRequestPacket>(ms);
                RoomId = packet.RoomId;
                PlayerName = packet.PlayerName;
            }
        }
    }

    [ProtoContract]
    public class JoinRoomResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_JOIN_RESPONSE;
        [ProtoMember(1)]
        public int RoomId { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<JoinRoomResponsePacket>(ms);
                RoomId = packet.RoomId;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class LeaveRoomRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_LEAVE_REQUEST;
        [ProtoMember(1)]
        public int RoomId { get; set; }
        [ProtoMember(2)]
        public string PlayerName { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<LeaveRoomRequestPacket>(ms);
                RoomId = packet.RoomId;
                PlayerName = packet.PlayerName;
            }
        }
    }

    [ProtoContract]
    public class LeaveRoomResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_LEAVE_RESPONSE;
        [ProtoMember(1)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<LeaveRoomResponsePacket>(ms);
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class ChatMessageSendPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.CHAT_MESSAGE_SEND;
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<ChatMessageSendPacket>(ms);
                Name = packet.Name;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class ChatMessageRecvPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.CHAT_MESSAGE_RECV;
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<ChatMessageRecvPacket>(ms);
                Name = packet.Name;
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class PlayerReadyRequestPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_READY_REQUEST;
        [ProtoMember(1)]
        public bool Ready { get; set; }
        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<PlayerReadyRequestPacket>(ms);
                Ready = packet.Ready;
            }
        }
    }

    [ProtoContract]
    public class PlayerReadyResponsePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_READY_RESPONSE;
        [ProtoMember(1)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<PlayerReadyResponsePacket>(ms);
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class NotiMessagePacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.ROOM_NOTI_MESSAGE;
        [ProtoMember(1)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<NotiMessagePacket>(ms);
                Message = packet.Message;
            }
        }
    }

    [ProtoContract]
    public class GameStartPacket : IPacket
    {
        [ProtoIgnore]
        public ushort PacketSize => (ushort)(4 + Body.Length);
        [ProtoIgnore]
        public ushort PacketId => (ushort)PacketID.GAME_PACKET_START;
        [ProtoMember(1)]
        public string Message { get; set; }

        public byte[] Body
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ToByteArray()
        {
            Byte[] buffer = new byte[PacketSize];
            Buffer.BlockCopy(BitConverter.GetBytes(PacketSize), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PacketId), 0, buffer, 2, 2);
            Buffer.BlockCopy(Body, 0, buffer, 4, Body.Length);
            return buffer;
        }

        public void FromByteArray(byte[] data)
        {
            using (var ms = new MemoryStream(data, 4, data.Length - 4))
            {
                var packet = Serializer.Deserialize<GameStartPacket>(ms);
                Message = packet.Message;
            }
        }
    }
}