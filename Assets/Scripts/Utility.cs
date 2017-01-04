using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
}