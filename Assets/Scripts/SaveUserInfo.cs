using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;
[Serializable]
public class SaveUserInfo
{
    [SerializeField] string nickName;
    [SerializeField] string id;
    [SerializeField] string pw;
    [SerializeField] string email;
    public string NICKNAME
    {
        get { return nickName; }
        set { nickName = value; }
    }
    public string ID
    {
        get { return id; }
        set { id = value; }
    }
    public string PW
    {
        get { return pw; }
        set { pw = value; }
    }
    public string EMAIL 
    {
        get { return email; }
        set { email = value; }
    }
    public SaveUserInfo() { }
    public SaveUserInfo(List<string> _info)
    {
        nickName = _info[0];
        id = _info[1];
        pw = _info[2];
        email = _info[3];
    }
}
[Serializable]
public class Serialization<T>
{
    [SerializeField] List<T> _t;
    public List<T> toReturn() { return _t; }
    public Serialization(List<T> _tmp)
    {
        _t = _tmp;
    }
}