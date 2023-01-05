using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Game.Packet;
namespace ConnectServer.User
{
    public delegate void DelegateTmp();
    public delegate void DelegateMove(int id, float x, float y, float z);
    public delegate void DelegateInfo();
    public class DelegateMoveClass
    {
        public CHARMOVE moveInfo;
        public DelegateMove delMove;
    }
    public class DelegateCreateClass
    {
        public USERINFO userInfo;
        public DelegateInfo delInfo;
    }
    public class User
    {
        public Socket userSock;
        public byte[] sBuff;
        public byte[] rBuff;
        public bool isInterrupt;
        public Thread thread;
        public Thread rThread;
        private Queue<byte[]> packetQueue;
        private int uid;
        private Dictionary<string, string> info;
        public User(Socket _sock, int _uid)
        {
            isInterrupt = false;
            userSock = _sock;
            uid = _uid;
            sBuff = new byte[128];
            rBuff = new byte[128];
            packetQueue = new Queue<byte[]>();
            info = new Dictionary<string, string>();
            ThreadStart threadStart = new ThreadStart(NewConnect);
            thread = new Thread(threadStart);
            thread.Start();
            ThreadStart threadStartReceive = new ThreadStart(CallReceive);
            rThread = new Thread(threadStartReceive);
            rThread.Start();
        }
        public void NewConnect()
        {
            try
            {
                MakeCreatePacket();
                Send();
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
            }
        }
        public void CallReceive()
        {
            while (!isInterrupt)
            {
                if(packetQueue.Count > 0)
                {
                    byte[] data = new byte[128];
                    byte[] _packet = new byte[2];
                    data = packetQueue.Dequeue();
                    Array.Copy(data, 0, _packet, 0, _packet.Length);
                    short type = BitConverter.ToInt16(_packet);
                    switch ((int)type)
                    {
                        case (int)ePACKETTYPE.REGISTINFO:
                            {
                                info.Clear();
                                byte[] name = new byte[10];
                                byte[] id = new byte[30];
                                byte[] pw = new byte[40];
                                byte[] email = new byte[60];
                                Array.Copy(data, 2, name, 0, name.Length);
                                Array.Copy(data, 12, id, 0, name.Length);
                                Array.Copy(data, 32, pw, 0, name.Length);
                                Array.Copy(data, 72, email, 0, name.Length);
                                info.Add("name", Encoding.Default.GetString(name));
                                info.Add("id", Encoding.Default.GetString(id));
                                info.Add("pw", Encoding.Default.GetString(pw));
                                info.Add("email", Encoding.Default.GetString(email));
                                foreach (KeyValuePair<string, string> tmp in info)
                                {
                                    Debug.Log(tmp.Key + " : " + tmp.Value);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                Thread.Sleep(10);
            }
        }
        public void Receive()
        {
            userSock.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, userSock);
        }
        public void Send()
        {
            userSock.BeginSend(sBuff, 0, sBuff.Length, SocketFlags.None, SendCallBack, userSock);
        }
        public void ClearSendBuff()
        {
            Array.Clear(sBuff, 0, sBuff.Length);
        }
        public void ClearReceiveBuff()
        {
            Array.Clear(rBuff, 0, rBuff.Length);
        }
        public void Close()
        {
            try
            {
                userSock.Shutdown(SocketShutdown.Both);
                userSock.Close();
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
            }
        }

        public void ReceiveCallBack(IAsyncResult ar)
        {
            // queue에 enqueue
            byte[] data = new byte[128];
            Array.Copy(rBuff, data, rBuff.Length);
            Array.Clear(rBuff, 0, rBuff.Length);
            packetQueue.Enqueue(data);
            userSock.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, userSock);
        }
        public void SendCallBack(IAsyncResult ar)
        {
            byte[] data = new byte[128];
            Array.Copy(sBuff, 0, data, 0, sBuff.Length);
            Array.Clear(sBuff, 0, sBuff.Length);
            packetQueue.Enqueue(data);
            userSock.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, userSock);

        }

        public void MakeCreatePacket()
        {
            USERINFO userInfo;
            userInfo.ePacketType = ePACKETTYPE.USERINFO;
            userInfo.uid = uid;
            byte[] _packetType = BitConverter.GetBytes((short)userInfo.ePacketType);
            byte[] _uid = BitConverter.GetBytes((int)userInfo.uid);
            Array.Copy(_packetType, 0, sBuff, 0, _packetType.Length);
            Array.Copy(_uid, 0, sBuff, 2, _uid.Length);
        }

        /// <summary>
        /// Move패킷생성
        /// </summary>
        /// <param name="_pos">이동할 위치</param>
        public void MakeMovePacket(Vector3 _pos)
        {
            CHARMOVE charMove;
            charMove.uid = 001;
            charMove.xPos = _pos.x;
            charMove.yPos = _pos.y;
            charMove.zPos = _pos.z;
            byte[] packetType = BitConverter.GetBytes((short)ePACKETTYPE.CHARMOVE);
            byte[] uid = BitConverter.GetBytes((int)1);
            byte[] xPos = BitConverter.GetBytes(charMove.xPos);
            byte[] yPos = BitConverter.GetBytes(charMove.yPos);
            byte[] zPos = BitConverter.GetBytes(charMove.zPos);
            Array.Copy(packetType, 0, sBuff, 0, packetType.Length);
            Array.Copy(uid, 0, sBuff, 2, uid.Length);
            Array.Copy(xPos, 0, sBuff, 6, xPos.Length);
            Array.Copy(yPos, 0, sBuff, 10, yPos.Length);
            Array.Copy(zPos, 0, sBuff, 14, zPos.Length);
        }
    }

}