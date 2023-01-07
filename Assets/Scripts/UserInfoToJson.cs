using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;
using System.Text;

public static class UserInfoToJson
{
    static string pathJsonDirectory;
    static string pathJson;
    public static List<SaveUserInfo> infoList = new List<SaveUserInfo>();
    public static void SetPath()
    {
        pathJsonDirectory = Path.Combine(Application.dataPath, "/Documents/BSServer/");
        pathJson = Path.Combine(Application.dataPath, "/Documents/BSServer/UserInfo.json");
        Directory.CreateDirectory(pathJsonDirectory);
        LoadInfo();
    }
    public static void SaveInfo(SaveUserInfo _info)
    {
        infoList.Add(_info);
        string jsonData = JsonUtility.ToJson(new Serialization<SaveUserInfo>(infoList));
        try {
            FileStream writeStream = new FileStream(pathJson, FileMode.Create);
            byte[] writeData = Encoding.UTF8.GetBytes(jsonData);
            writeStream.Write(writeData, 0, writeData.Length);
            writeStream.Close();
            Debug.Log("저장");
        }
        catch(Exception e)
        {
            Debug.Log("실패" + e.Message);
        }
        
    }
    public static void LoadInfo()
    {
        try
        {
            FileStream readStream = new FileStream(pathJson, FileMode.Open);
            byte[] data = new byte[readStream.Length];
            readStream.Read(data, 0, data.Length);
            readStream.Close();
            JSONNode root = JSON.Parse(Encoding.UTF8.GetString(data))[0];
            for (int i = 0; i < root.Count; i++)
            {
                SaveUserInfo origin = new SaveUserInfo();
                origin.NICKNAME = root[i]["nickName"].Value;
                origin.ID = root[i]["id"].Value;
                origin.PW = root[i]["pw"].Value;
                origin.EMAIL = root[i]["email"].Value;
                infoList.Add(origin);
            }
        }
        catch (Exception e)
        {
            Debug.Log("셋" + e.Message);
        }
    }
}
