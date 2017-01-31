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
    public List<int> Scores = new List<int>();

    public SaveData()
    {
    }

    public SaveData (SerializationInfo info, StreamingContext context)
    {
        Scores = (List<int>)info.GetValue("Scores", typeof(List<int>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Scores", Scores, typeof(List<int>));
    }
}

public class SerializationManager : MonoBehaviour
{
    private static SerializationManager instance;

    public static SerializationManager Instance
    {
        get { return instance; }
    }    

    private string filePath = "SaveData.sf";

    public void Save()
    {        
        SaveData data = new SaveData();
        data.Scores = PlayerManager.Instance.HighScores;

        using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new VersionDeserializerBinder();
            formatter.Serialize(stream, data);            
        }
    }

    public void Load()
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        SaveData data = new SaveData();
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new VersionDeserializerBinder();
            data = formatter.Deserialize(stream) as SaveData;
        }

        if (data != null)
        {
            PlayerManager.Instance.HighScores = data.Scores;
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
}
