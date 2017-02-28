using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public static class ExtensionFunctions
{
    private static System.Random random = new System.Random();

    public static T GetRandom<T>(this IList<T> list) 
    {
        if (list.Count == 0)
        {
            return default(T);
        }
            
        int rand = random.Next(0, list.Count);
        return list[rand];
    }

    public static bool IsNear(this Vector3 me, Vector3 other, float within = 0.1f) 
    {
        return Math.Abs(Vector3.Distance(me, other)) <= within;
    }

    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> list)
    {
        foreach (T item in list)
        {
            if (!hashSet.Contains(item))
            {
                hashSet.Add(item);
            }
        }
    }

    public static string Encrypt(this string input, string key)
    {
        byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
        byte[] keyArray;

        SHA1CryptoServiceProvider hash = new SHA1CryptoServiceProvider();
        keyArray = hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        byte[] trimmedBytes = new byte[24];
        Buffer.BlockCopy(keyArray, 0, trimmedBytes, 0, 20);
        keyArray = trimmedBytes;

        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = keyArray;
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }
}

public static class GameUtils
{
    public static FloatyText GenerateFloatyTextAt(string text, float x, float y, FloatyText template, GameObject parent = null, Color? color = null)
    {
        FloatyText newFloatyText = GameObject.Instantiate(template);
        if (parent != null)
        {
            newFloatyText.transform.parent = parent.transform;
        }

        newFloatyText.SetText(text, color);
        newFloatyText.transform.localPosition = new Vector3(x, y, 100);
        return newFloatyText;
    }

    public static void LogToDebug(string message)
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.LoggingEnabled)
        {
            //Debug.Log(message);
        }
    }

    /// <summary>
    /// Change value by amount and provides capped bounds.
    /// </summary>
    /// <returns>True if value changes, false otherwise.</returns>
    public static bool CappedIncrement(ref int value, int amount, int? lowerBound = null, int? upperBound = null)
    {
        int initValue = value;
        value += amount;
        if (lowerBound != null) value = Math.Max(lowerBound.Value, value);
        if (upperBound != null) value = Math.Min(upperBound.Value, value);
        return initValue != value;
    }

    public static string GetGemLogStats(Gem gem)
    {
        return string.Format("[{0},{1} ({2})]", gem.GridX, gem.GridY, gem.GemId);
    }

    public static void LogCoordConcat(string label, List<Gem> gems)
    {
        //Debug.Log(label + ": " + string.Join("; ", gems.Select(gem => GetGemLogStats(gem)).ToArray()));
    }
}