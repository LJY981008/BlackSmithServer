using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class UserInfoToJson
{
    static string pathJson;
    static string jsonName = "userInfo.json";
    static List<string> infoList = new List<string>();
    public static void SetPath()
    {
        pathJson = Path.Combine(Application.dataPath, "/Documents/BSServer/");
    }
    public static void SaveInfo(List<string> _infoList)
    {
        infoList = _infoList;
        SaveUserInfo info = new SaveUserInfo(infoList);
        string jsonData = JsonUtility.ToJson(new Serialization<SaveUserInfo>(info));
        try {
            File.WriteAllText(pathJson + jsonName, jsonData);
        }catch(Exception e)
        {
            Directory.CreateDirectory(pathJson);
            File.WriteAllText(pathJson + jsonName, jsonData);
        }
        Debug.Log("¿˙¿Â");
    }
    public static List<string> LoadInfo()
    {
        try
        {
            SaveUserInfo info = new SaveUserInfo();
            List<string> infoList = new List<string>();
            string loadJson = File.ReadAllText(pathJson + jsonName);
            info = JsonUtility.FromJson<Serialization<SaveUserInfo>>(loadJson).toReturn();
            infoList.Add(info.NICKNAME);
            infoList.Add(info.ID);
            infoList.Add(info.PW);
            infoList.Add(info.EMAIL);
            return infoList;
        }catch(Exception e)
        {
            return null;
        }
    }
}
