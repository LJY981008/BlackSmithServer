using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Packet
{
    public enum ePACKETTYPE
    {
        NONE,
        USERINFO = 1000,
        CHARSELECT,
        CHARMOVE
    }
    public struct USERINFO
    {
        public ePACKETTYPE ePacketType;
        public int uid;
        public string name;
    }
    public struct CHARMOVE
    {
        public ePACKETTYPE ePacketType;
        public int uid;
        public float xPos;
        public float yPos;
        public float zPos;
    }
    
}