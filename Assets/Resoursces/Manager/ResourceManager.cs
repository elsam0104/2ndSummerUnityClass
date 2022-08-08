using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System;

public class ResourceManager
{
    public static UnityObject Load(string path)
    {
        return Resources.Load(path);
    }
    public static GameObject LoadAndInstantiate(string path)
    {
        UnityObject obj = Load(path);
        if (obj == null)
        {
            return null;
        }
        return (GameObject)GameObject.Instantiate(obj); // ������ ����. ����ȯ�� ����? return ���� ����?
        //return GameObject.Instantiate(obj) as GameObject;
    }
}
