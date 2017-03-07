using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using UnityEngine;

[Serializable]
public class SaveData : ISerializable
{
    public List<string> UnlockedItems = new List<string>();
    public List<int> Scores = new List<int>();
    public int UniversalScore = 0;
    public Achievments Achievments = new Achievments();
    public string PlayerName;
    public int MaxLevel = 0;

    public SaveData()
    {
    }

    public SaveData (SerializationInfo info, StreamingContext context)
    {
        try
        {
            Scores = (List<int>)info.GetValue("Scores", typeof(List<int>));
        } catch {}

        try
        {
            UnlockedItems = (List<string>)info.GetValue("UnlockedItems", typeof(List<string>));
        } catch {}

        try
        {
            UniversalScore = info.GetInt32("UniversalScore");
        } catch {}

        try
        {
            Achievments = (Achievments)info.GetValue("Achievments", typeof(Achievments));
        } catch {}

        try
        {
            PlayerName = info.GetString("PlayerName");
        } catch {}

        try
        {
            MaxLevel = info.GetInt32("MaxLevel");
        } catch {}
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Scores", Scores, typeof(List<int>));
        info.AddValue("UnlockedItems", UnlockedItems, typeof(List<string>));
        info.AddValue("UniversalScore", UniversalScore);
        info.AddValue("Achievments", Achievments);
        info.AddValue("PlayerName", PlayerName);
        info.AddValue("MaxLevel", MaxLevel);
    }
}

public class SerializationManager : MonoBehaviour
{
    private static SerializationManager instance;

    public static SerializationManager Instance
    {
        get { return instance; }
    }    

    private string FilePath
    {
        get
        {
            return Application.persistentDataPath + "/SaveData.sf";
        }
    }

    public void Save()
    {
        try
        {
            SaveData data = new SaveData();
            data.Scores = PlayerManager.Instance.HighScores;
            data.UnlockedItems = PlayerManager.Instance.UnlockedItems;
            data.UniversalScore = PlayerManager.Instance.UniversalScore;
            data.Achievments = PlayerManager.Instance.Achievments;
            data.PlayerName = PlayerManager.Instance.PlayerName;
            data.MaxLevel = PlayerManager.Instance.MaxLevel;

            using (Stream stream = File.Open(FilePath, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new VersionDeserializerBinder();
                formatter.Serialize(stream, data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            SaveData data = new SaveData();
            using (Stream stream = File.Open(FilePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new VersionDeserializerBinder();
                data = formatter.Deserialize(stream) as SaveData;
            }

            if (data != null)
            {
                PlayerManager.Instance.HighScores = data.Scores;
                PlayerManager.Instance.UnlockedItems = data.UnlockedItems;
                PlayerManager.Instance.UniversalScore = data.UniversalScore;
                PlayerManager.Instance.Achievments = data.Achievments;
                PlayerManager.Instance.PlayerName = data.PlayerName;
                PlayerManager.Instance.MaxLevel = data.MaxLevel;
            }
        }        
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public sealed class VersionDeserializerBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (!string.IsNullOrEmpty(assemblyName) && ! string.IsNullOrEmpty(typeName))
            {
                assemblyName = Assembly.GetExecutingAssembly().FullName;
                Type typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
                return typeToDeserialize;
            }

            return null;
        }
    }

    // Use this for initialization
    void Start () {
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DeleteProgress()
    {
        PlayerManager.Instance.DeleteProgress();
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        this.Save();
    }
}
