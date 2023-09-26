using System.Collections.Generic;
using UnityEngine;

public class ExtensionMethods
{
    public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (value < inMin || value > inMax)
        {
            //Debug.LogWarning($"Value ({value}) not between inMin ({inMin}) and inMax ({inMax}), clamping value");
            value = Mathf.Clamp(value, inMin, inMax);
        }
        
        return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
    }
    
    public static List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }
}
