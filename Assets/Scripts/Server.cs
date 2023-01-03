using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Game.Packet;

/// <summary>
/// 패킷타입 기본, 캐릭터 선택
/// </summary>

delegate void Do();
public delegate void DoMove(int id, float x, float y, float z);
public class DelegateWrap
{
    public CHARMOVE moveInfo;
    public DoMove doMove;
}
public class Server : MonoBehaviour
{
    Socket listenSock;
    Socket otherPeer;
    string IP;
    int port;
    byte[] sBuff;
    byte[] rBuff;
    GameObject player;
    GameObject otherPlayer;
    Do doCreate;
    Queue<byte[]> packetQueue;
    Dictionary<int, DelegateWrap> doDic;
    Vector3 endPos;
    private void Awake()
    {
        IP = "172.30.1.25";
        port = 8082;
        sBuff = new byte[128];
        rBuff = new byte[128];
        doCreate = null;
        packetQueue = new Queue<byte[]>();
        doDic = new Dictionary<int, DelegateWrap>();
        endPos = -Vector3.one;
    }
    private void Start()
    {
        listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(IP), port);
        listenSock.Bind(ip);
        listenSock.Listen(10);
        listenSock.BeginAccept(AddPeer, null);
    }
    
    private void Update()
    {
        MousePick();
        if (doCreate != null)
            doCreate();
        if (packetQueue.Count > 0)
        {
            byte[] data = packetQueue.Dequeue();
            // 패킷 타입(2) + 패킷 내용(126)
            byte[] _Packet = new byte[2];
            Array.Copy(data, 0, _Packet, 0, 2);
            short packetType = BitConverter.ToInt16(_Packet, 0);
            switch ((int)packetType)
            {
                case (int)ePACKETTYPE.PEERINFO:
                    {

                    }
                    break;
                case (int)ePACKETTYPE.CHARSELECT:
                    {
                        // 내용 파싱
                        Debug.Log("셀렉");
                    }
                    break;
                case (int)ePACKETTYPE.CHARMOVE:
                    {
                        byte[] _uid = new byte[4];
                        byte[] _xPos = new byte[4];
                        byte[] _yPos = new byte[4];
                        byte[] _zPos = new byte[4];
                        Array.Copy(data, 2, _uid, 0, _uid.Length);
                        Array.Copy(data, 6, _xPos, 0, _xPos.Length);
                        Array.Copy(data, 10, _yPos, 0, _yPos.Length);
                        Array.Copy(data, 14, _zPos, 0, _zPos.Length);
                        int uid = BitConverter.ToInt32(_uid, 0);
                        float xPos = BitConverter.ToSingle(_xPos);
                        float yPos = BitConverter.ToSingle(_yPos);
                        float zPos = BitConverter.ToSingle(_zPos);
                        if (doDic.ContainsKey(uid))
                        {
                            DelegateWrap actionValue;
                            doDic.TryGetValue(uid, out actionValue);
                            actionValue.moveInfo.xPos = xPos;
                            actionValue.moveInfo.yPos = yPos;
                            actionValue.moveInfo.zPos = zPos;
                        }
                        else
                        {
                            DelegateWrap doWarp = new DelegateWrap();
                            doWarp.moveInfo.uid = uid;
                            doWarp.moveInfo.xPos = xPos;
                            doWarp.moveInfo.yPos = yPos;
                            doWarp.moveInfo.zPos = zPos;
                            doWarp.doMove = MovePlayer;
                            doDic.Add(uid, doWarp);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        // start에 코루틴이나 쓰레드로 작동하게 변경 
        foreach (KeyValuePair<int, DelegateWrap> dic in doDic)
        {
            dic.Value.doMove(  dic.Value.moveInfo.uid,
                               dic.Value.moveInfo.xPos,
                               dic.Value.moveInfo.yPos,
                               dic.Value.moveInfo.zPos);
        }
    }
    /// <summary>
    /// 피어 접속
    /// </summary>
    /// <param name="ar"></param>
    public void AddPeer(IAsyncResult ar)
    {
        Socket otherPeer = listenSock.EndAccept(ar);
        Debug.Log(otherPeer.RemoteEndPoint + "님이 접속했습니다.");
        doCreate += CreateGameObject;
        // 저장할 버퍼, 시작 위치, 패킷의 길이, 플래그, 콜백함수, 매개변수
        otherPeer.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, otherPeer);
    }
    /// <summary>
    /// Receive의 콜백함수
    /// </summary>
    public void ReceiveCallBack(IAsyncResult ar)
    {
        otherPeer = (Socket)ar.AsyncState;
        // queue에 enqueue
        byte[] data = new byte[128];
        Array.Copy(rBuff, data, rBuff.Length);
        Array.Clear(rBuff, 0, rBuff.Length);
        packetQueue.Enqueue(data);
        otherPeer.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, otherPeer);
    }
    public void SendCallBack(IAsyncResult ar)
    {
        otherPeer = (Socket)ar.AsyncState;
        byte[] data = new byte[128];
        Array.Copy(sBuff, 0, data, 0, sBuff.Length);
        Array.Clear(sBuff, 0, sBuff.Length);
        packetQueue.Enqueue(data);
        otherPeer.BeginReceive(rBuff, 0, rBuff.Length, SocketFlags.None, ReceiveCallBack, otherPeer);

    }
    /// <summary>
    /// 접속 시 오브젝트 생성
    /// </summary>
    public void CreateGameObject()
    {
        GameObject tmp = Resources.Load<GameObject>("Cube");
        if (tmp != null)
        {
            player = Instantiate(tmp);
            otherPlayer = Instantiate(tmp);
        }
        doCreate -= CreateGameObject;

    }
    public void MousePick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                MakeMovePacket(hit.point);
                endPos = hit.point;
                otherPeer.BeginSend(sBuff, 0, sBuff.Length, SocketFlags.None, SendCallBack, otherPeer);
            }
        }
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
    
    public void MovePlayer(int _uid, float x, float y, float z)
    {
        Vector3 dest = new Vector3(x, y, z);
        if (dest != -Vector3.one)
        {
            if (_uid == 10)
            {
                otherPlayer.transform.position = Vector3.MoveTowards(otherPlayer.transform.position, dest, Time.deltaTime * 2.4f);
            }
            else if (_uid == 1)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, dest, Time.deltaTime * 2.4f);
            }
        }
    }
}
