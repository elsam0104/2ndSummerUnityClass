using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEditor;

public class EditorLib : MonoBehaviour
{
    //1. 파일경로를 찾아주는 기능
    //2. enum structure
    //3. layout
    //  1)top
    //  2)list
    //  3)bottom

    //1. 파일경로를 찾아주는 기능
    public static string getAssetPath(UnityEngine.Object _attr)
    {
        string retStrPath = string.Empty;

        //Assets/Resources/
        retStrPath = AssetDatabase.GetAssetPath(_attr);

        string[] tmpStrPath = retStrPath.Split("/");
        bool flagRes = false;
        for (int i = 0; i < tmpStrPath.Length; i++)
        {
            if (!flagRes)
            {
                if (tmpStrPath[i] == "Resources") //find resources folder
                {
                    flagRes = true;
                    retStrPath = string.Empty;
                }
            }
            else
            {
                retStrPath += tmpStrPath[i] + "/";
            }
        }
        return retStrPath;
    }
    //2. enum structure
    public static void makeEnumClass(string enumName, StringBuilder enumData)
    {
        string _filePathTemple = "Assets/Resources/Editor/EnumClassTemplate.txt";

        string contentClassTemplate = File.ReadAllText(_filePathTemple);

        contentClassTemplate = contentClassTemplate.Replace("$CLASSNUM$", enumName);
        contentClassTemplate = contentClassTemplate.Replace("$DATAINFO$", enumData.ToString());

        string tempFilePathTemplate = "Assets/Resources/EnumClass";
        if (Directory.Exists(tempFilePathTemplate) == false)
        {
            Directory.CreateDirectory(tempFilePathTemplate);
        }

        string retFilePathTemplate = tempFilePathTemplate + enumName + ".cs";

        if (File.Exists(retFilePathTemplate))
        {
            File.Delete(retFilePathTemplate);
        }
        File.WriteAllText(retFilePathTemplate, contentClassTemplate);
    }

    //3. layout
    //  1)top
    //  2)list
    //  3)bottom
    public static void setTopLayer(DefaultData data, ref int nowidx, ref UnityEngine.Object objLayer, int sizeWidth)
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Constructor", GUILayout.Width(sizeWidth)))
            {
                data.constuctorData("NewData");
                nowidx = data.getDataCnt() - 1;
                objLayer = null;
            }
            if (GUILayout.Button("Defulicate", GUILayout.Width(sizeWidth)))
            {
                data.defulicateData(nowidx);
                objLayer = null;
                nowidx = data.getDataCnt() - 1;
            }
            if (data.getDataCnt() > 1)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(sizeWidth)))
                {
                    objLayer = null;
                    data.deleteData(nowidx);
                }
            }
            if (nowidx > data.getDataCnt() - 1) //인덱스 더 못 늘리게 방지
            {
                nowidx = data.getDataCnt() - 1;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void setListLayer(ref Vector2 posScroll, DefaultData data, ref int nowidx, ref UnityEngine.Object objectLayer, int sizeWidth)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(sizeWidth));
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical("box");
            {
                posScroll = EditorGUILayout.BeginScrollView(posScroll);
                {
                    if(data.getDataCnt()>0)
                    {
                        int lastidx = nowidx;
                        nowidx = GUILayout.SelectionGrid(nowidx, data.getDataIdxList(true), 1);
                        if(lastidx != nowidx) //변경되었다
                        {
                            objectLayer = null;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }
}