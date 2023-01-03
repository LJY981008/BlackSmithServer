using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Packet
{
    public enum ePACKETTYPE
    {
        NONE,
        PEERINFO = 1000,
        CHARSELECT,
        CHARMOVE
    }
    public struct PEERINFO
    {
        public ePACKETTYPE ePacketType;
        public int uid;
        public string name;
        public short charType;
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