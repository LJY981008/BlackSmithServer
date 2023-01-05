using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using ConnectServer.User;
/// <summary>
/// 패킷타입 기본, 캐릭터 선택
/// </summary>


public class Server : MonoBehaviour
{

    private Socket listenSock;
    private Socket otherPeer;
    private GameObject player;
    private GameObject otherPlayer;
    private Queue<byte[]> packetQueue;
    private Thread thread;
    private Dictionary<int ,User> userList;

    private byte[] sBuff;
    private byte[] rBuff;
    private int port;
    private int idCount;
    private string IP;
    private bool isInterrupt;
    
    private void Awake()
    {
        IP = "172.30.1.25";
        port = 8082;
        idCount = 0;
        sBuff = new byte[128];
        rBuff = new byte[128];
        isInterrupt = false;
        userList = new Dictionary<int, User>();
    }
    private void Start()
    {
        listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(IP), port);
        listenSock.Bind(ip);
        ThreadStart threadStart = new ThreadStart(NewConnect);
        thread = new Thread(threadStart);
        thread.Start();
    }
    
    private void Update()
    {
        
    }
    public void NewConnect()
    {
        while (!isInterrupt)
        {
            listenSock.Listen(10);
            listenSock.BeginAccept(AddPeer, null);
            Thread.Sleep(10);
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
        User user = new User(otherPeer,idCount);
        userList.Add(idCount, user);
        idCount++;
    }
    private void OnDestroy()
    {
        isInterrupt = true;
        
        foreach(KeyValuePair<int, User> user in userList)
        {
            user.Value.isInterrupt = true;
            user.Value.Close();
        }
    }
}
