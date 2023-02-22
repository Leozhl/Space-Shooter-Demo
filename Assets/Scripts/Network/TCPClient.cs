using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using ProtoBuf;
using msg;

public class TCPClient : MonoBehaviour
{
    private Socket mServerSocket;
    private Thread mReceiveThread;
    [HideInInspector]
    public bool Connected = false;

    private static Dictionary<MessageType, Delegate> mReceiveMsgFuncDic = new Dictionary<MessageType, Delegate>();
    private static Dictionary<MessageType, Type> mReceiveMsgTypeDic = new Dictionary<MessageType, Type>();
    private static List<Tuple<MessageType, object>> mUpdateMsgList = new List<Tuple<MessageType, object>>();
    private static List<Tuple<MessageType, object>> mFixedUpdateMsgList = new List<Tuple<MessageType, object>>();

    public void StartConnect(string serverAddress, int port)
    {
        try
        {
            mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mServerSocket.Connect(serverAddress, port);

            mReceiveThread = new Thread(new ThreadStart(Receiver));
            mReceiveThread.IsBackground = true;
            Connected = true;

            Debug.Log("Connection success");
        }
        catch (Exception e)
        {
            Debug.Log("Connection error: " + e);
        }
    }

    public void StartReceive()
    {
        if (mReceiveThread == null || !Connected) return;

        if (mReceiveThread.IsAlive)
        {
            //Debug.Log("Thread already start!");
        }
        else
        {
            mReceiveThread.Start();
        }
    }

    public void StopReceive()
    {
        Connected = false;
        if (mReceiveThread != null)
        {
            mReceiveThread.Abort();
            mReceiveThread.Join();
            mServerSocket.Close();
        }
    }

    private byte[] SerialzeProtobuf<T>(T t)
    {
        System.IO.MemoryStream stream = new System.IO.MemoryStream();
        try
        {
            Serializer.Serialize(stream, t);
        }
        catch (Exception)
        {
            Debug.Log("protobuf序列化失败!");
            return null;
        }
        return stream.ToArray();
    }

    private T DeSerialzeProtobuf<T>(byte[] bytes)
    {
        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
        try
        {
            return Serializer.Deserialize<T>(stream);
        }
        catch
        {
            Debug.Log("protobuf反序列化失败!");
            return default;
        }
    }

    private object DeSerialzeProtobuf(Type t, byte[] bytes)
    {
        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
        try
        {
            return Serializer.Deserialize(t, stream);
        }
        catch
        {
            Debug.Log("protobuf反序列化失败!");
            return null;
        }
    }

    public void SendMessage<T>(MessageType msgType, T message)
    {
        MsgType mt = new MsgType();
        mt.id = GameManager.Instance.ID;
        mt.msgType = msgType;
        List<byte> buffer = new List<byte>();
        byte[] tmp = SerialzeProtobuf(mt);
        buffer.Add((byte)tmp.Length);
        buffer.AddRange(tmp);
        tmp = SerialzeProtobuf(message);
        buffer.Add((byte)tmp.Length);
        buffer.AddRange(tmp);
        //Debug.Log("Send " + msgType + " " + buffer.ToArray().Length);
        //Debug.Log(byteListToString(buffer));
        if (Connected && mServerSocket.Connected) mServerSocket.Send(buffer.ToArray());
    }

    private string byteListToString (List<byte> list)
    {
        string result = "";
        foreach (byte i in list)
        {
            result += i.ToString() + " ";
        }
        return result;
    }

    public void RegReceiveMessage<T>(MessageType msgType, Action<T> onReceiveMessage)
    {
        if (mReceiveMsgFuncDic.ContainsKey(msgType))
        {
            //Debug.Log("This message type already registered!");
            return;
        }
        lock(mReceiveMsgTypeDic)
        {
            mReceiveMsgFuncDic[msgType] = onReceiveMessage;
            mReceiveMsgTypeDic[msgType] = Activator.CreateInstance<T>().GetType();
        }
    }

    private void Receiver()
    {
        //Debug.Log("Thread start");
        while (Connected)
        {
            byte[] buffer = new byte[256];
            try 
            {
                mServerSocket.Receive(buffer, 1, SocketFlags.None);

                int length = buffer[0];
                mServerSocket.Receive(buffer, length, SocketFlags.None);
                MsgType receiveType = DeSerialzeProtobuf<MsgType>(buffer.Take(length).ToArray());
                if (receiveType == default) continue;
                //Debug.Log(receiveType.msgType);

                mServerSocket.Receive(buffer, 1, SocketFlags.None);
                length = buffer[0];
                mServerSocket.Receive(buffer, length, SocketFlags.None);

                Type t;
                lock (mReceiveMsgTypeDic)
                {
                    t = mReceiveMsgTypeDic[receiveType.msgType];
                }
                object receiveMsg = DeSerialzeProtobuf(t, buffer.Take(length).ToArray());
                if (receiveMsg == null) continue;
                if (receiveType.msgType == MessageType.PlayerMove)
                {
                    lock (mFixedUpdateMsgList)
                    {
                        mFixedUpdateMsgList.Add(new Tuple<MessageType, object>(receiveType.msgType, receiveMsg));
                    }
                }
                else
                {
                    lock (mUpdateMsgList)
                    {
                        mUpdateMsgList.Add(new Tuple<MessageType, object>(receiveType.msgType, receiveMsg));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Thread exception: " + e);
                Connected = false;
            }
        }
    }

    private void Update()
    {
        lock (mUpdateMsgList)
        {
            foreach (Tuple<MessageType, object> msgTuple in mUpdateMsgList)
            {
                Type t;
                lock (mReceiveMsgTypeDic)
                {
                    t= mReceiveMsgTypeDic[msgTuple.Item1];
                }
                mReceiveMsgFuncDic[msgTuple.Item1].DynamicInvoke(Convert.ChangeType(msgTuple.Item2, t));
            }
            mUpdateMsgList.Clear();
        }
    }

    private void FixedUpdate()
    {
        lock (mFixedUpdateMsgList)
        {
            foreach (Tuple<MessageType, object> msgTuple in mFixedUpdateMsgList)
            {
                Type t;
                lock (mReceiveMsgTypeDic)
                {
                    t = mReceiveMsgTypeDic[msgTuple.Item1];
                }
                mReceiveMsgFuncDic[msgTuple.Item1].DynamicInvoke(Convert.ChangeType(msgTuple.Item2, t));
            }
            mFixedUpdateMsgList.Clear();
        }
    }
}
