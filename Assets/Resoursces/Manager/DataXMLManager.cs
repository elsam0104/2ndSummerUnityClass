using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataXMLManager : MonoBehaviour
{
    private static EffectXMLData effectXMLData = null;
    private void Start()
    {
        if(effectXMLData ==null)
        {
            effectXMLData = ScriptableObject.CreateInstance<EffectXMLData>();
            effectXMLData.LoadData();
        }
    }
    public static EffectXMLData EffectData()
    {
        effectXMLData = ScriptableObject.CreateInstance<EffectXMLData>();
        effectXMLData.LoadData();
        return effectXMLData;
    }
}
