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
    public class User:IDisposable
    {
        public Socket userSock;
        public byte[] sBuff;
        public byte[] rBuff;
        public bool isInterrupt;
        public Thread thread;
        public Thread rThread;
        private Queue<byte[]> packetQueue;
        private int uid;
        private Dictionary<string, string> myInfo;
        public bool isConnect;
        public User(Socket _sock, int _uid)
        {
            isInterrupt = false;
            userSock = _sock;
            uid = _uid;
            sBuff = new byte[256];
            rBuff = new byte[256];
            packetQueue = new Queue<byte[]>();
            myInfo = new Dictionary<string, string>();
            ThreadStart threadStart = new ThreadStart(NewConnect);
            thread = new Thread(threadStart);
            thread.Start();
            thread.Join();
            thread.Interrupt();
            ThreadStart threadStartReceive = new ThreadStart(CallReceive);
            rThread = new Thread(threadStartReceive);
            rThread.Start();
            rThread.Join();
            rThread.Interrupt();
            Dispose();
        }
        public void NewConnect()
        {
            try
            {
                MakeCreatePacket();
                Receive();
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
            }
        }
        public void CallReceive()
        {
            while (userSock.Connected)
            {
                if(packetQueue.Count > 0)
                {
                    SaveUserInfo info = new SaveUserInfo();
                    byte[] data = new byte[256];
                    byte[] _packet = new byte[2];
                    data = packetQueue.Dequeue();
                    Array.Copy(data, 0, _packet, 0, _packet.Length);
                    short type = BitConverter.ToInt16(_packet);
                    switch (type)
                    {
                        case (short)ePACKETTYPE.REGISTINFO:
                            {
                                byte[] name = new byte[10];
                                byte[] id = new byte[30];
                                byte[] pw = new byte[40];
                                byte[] email = new byte[60];
                                Array.Copy(data, 2, name, 0, name.Length);
                                Array.Copy(data, 12, id, 0, id.Length);
                                Array.Copy(data, 42, pw, 0, pw.Length);
                                Array.Copy(data, 82, email, 0, email.Length);
                                info.NICKNAME = Encoding.Default.GetString(name).Trim('\0');
                                info.ID = Encoding.Default.GetString(id).Trim('\0');
                                info.PW = Encoding.Default.GetString(pw).Trim('\0');
                                info.EMAIL = Encoding.Default.GetString(email).Trim('\0');
                                UserInfoToJson.SaveInfo(info);
                            }
                            break;
                        case (short)ePACKETTYPE.LOGININFO:
                            {
                                List<SaveUserInfo> _list = new List<SaveUserInfo>();
                                byte[] id = new byte[30];
                                byte[] pw = new byte[40];
                                Array.Copy(data, 2, id, 0, id.Length);
                                Array.Copy(data, 32, pw, 0, pw.Length);
                                info.ID = Encoding.UTF8.GetString(id).Trim('\0');
                                info.PW = Encoding.UTF8.GetString(pw).Trim('\0');
                                foreach(var item in UserInfoToJson.infoList)
                                {
                                    _list.Add(item);
                                }
                                int _success = -1;
                                foreach (var item in _list)
                                {
                                    if (info.ID == item.ID)
                                    {
                                        if (info.PW == item.PW)
                                        {
                                            myInfo.Add("ID", item.ID);
                                            myInfo.Add("PW", item.PW);
                                            myInfo.Add("Name", item.NICKNAME);
                                            myInfo.Add("Email", item.EMAIL);
                                            Debug.Log("로그인 성공");
                                            _success = 0;
                                            break;
                                        }
                                        else
                                        {
                                            _success = 1;
                                            Debug.Log("비밀번호 틀림");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        _success = 2;
                                        Debug.Log("아이디 없음");
                                    }
                                }
                                MakeLoginPacket(_success);
                            }
                            break;
                        case (short)ePACKETTYPE.EXIT:
                            {
                                isInterrupt = true;
                                packetQueue.Clear();
                            }
                            break;
                        case (short)ePACKETTYPE.NONE:
                            {
                                Dispose();
                            }
                            break;
                        default:
                            // 종료할 때 0이 넘어오는 현상 project에서 해결필요
                            Debug.Log(packetQueue.Count);
                            break;
                    }
                }
                Thread.Sleep(10);
            }
        }
        public void Receive()
        {
            try
            {
                userSock.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, userSock);
            }
            catch (Exception e)
            {
                Debug.Log($"연결이 종료된 상태({e.Message})");
            }
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
            byte[] data = new byte[256];
            Array.Copy(rBuff, data, rBuff.Length);
            Array.Clear(rBuff, 0, rBuff.Length);
            packetQueue.Enqueue(data);
            Receive();
        }
        public void SendCallBack(IAsyncResult ar)
        {
            byte[] data = new byte[256];
            Array.Copy(sBuff, 0, data, 0, sBuff.Length);
            Array.Clear(sBuff, 0, sBuff.Length);
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
            Send();
        }
        public void MakeLoginPacket(int isSuccess)
        {
            LOGININFO login;
            byte[] _packetType = new byte[2];
            byte[] _isSuccess = new byte[4];
            login.ePacketType = ePACKETTYPE.LOGININFO;
            login.isSuccess = isSuccess;
            _packetType = BitConverter.GetBytes((short)login.ePacketType);
            _isSuccess = BitConverter.GetBytes((int)login.isSuccess);
            Array.Copy(_packetType, 0, sBuff, 0, _packetType.Length);
            Array.Copy(_isSuccess, 0, sBuff, 2, _isSuccess.Length);
            if (isSuccess == 0)
            {
                login.id = myInfo["ID"];
                login.pw = myInfo["PW"];
                login.name = myInfo["Name"];
                login.email = myInfo["Email"];
                byte[] _id = new byte[30];
                byte[] _pw = new byte[40];
                byte[] _name = new byte[20];
                byte[] _email = new byte[60];
                _id = Encoding.UTF8.GetBytes(login.id);
                _pw = Encoding.UTF8.GetBytes(login.pw);
                _name = Encoding.UTF8.GetBytes(login.name);
                _email = Encoding.UTF8.GetBytes(login.email);
                Array.Copy(_id, 0, sBuff, 6, _id.Length);
                Array.Copy(_pw, 0, sBuff, 36, _pw.Length);
                Array.Copy(_name, 0, sBuff, 76, _name.Length);
                Array.Copy(_email, 0, sBuff, 96, _email.Length);
            }
            Send();
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
            Send();
        }

        public void Dispose()
        {
            try
            {
                packetQueue.Clear();
                userSock.Shutdown(SocketShutdown.Both);
                userSock.Close();
                isConnect = false;
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}