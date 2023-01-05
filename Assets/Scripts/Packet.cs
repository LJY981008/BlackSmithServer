using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Packet
{
    public enum ePACKETTYPE
    {
        NONE,
        USERINFO = 1000,
        LOGININFO,
        REGISTINFO,
        CHARMOVE,
        EXIT
    }
    public struct USERINFO
    {
        public ePACKETTYPE ePacketType;
        public int uid;
    }
    public struct LOGININFO
    {
        public ePACKETTYPE ePacketType;
        public int uid;
    }
    public struct REGISTINFO
    {
        public ePACKETTYPE ePacketType;
        public string name;
        public string id;
        public string pw;
        public string email;
    }
    public struct CHARMOVE
    {
        public ePACKETTYPE ePacketType;
        public int uid;
        public float xPos;
        public float yPos;
        public float zPos;
    }
    public struct EXIT
    {
        public ePACKETTYPE ePacketType;
        public int uid;
    }
}