using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class EffectEditor : EditorWindow
{
    public int sizeLargeWidth = 400;
    public int sizeMiddleWidth = 200;

    private int nowCode = 0;
    private Vector2 posScroll_1 = Vector2.zero;
    private Vector2 posScroll_2 = Vector2.zero;

    private GameObject effectObj = null;

    private static EffectXMLData effectXMLData;

    [MenuItem("tool/EffectEditor")]
    static void init()
    {
        effectXMLData = ScriptableObject.CreateInstance<EffectXMLData>();
        effectXMLData.LoadData();

        EffectEditor window = GetWindow<EffectEditor>(false, "Effect Editor");
        window.Show();
    }
    private void OnGUI()
    {
        if (effectXMLData == null) return;
        EditorGUILayout.BeginVertical();
        {
            //top
            UnityEngine.Object editObj = effectObj;
            EditorLib.setTopLayer(effectXMLData, ref nowCode, ref editObj, this.sizeLargeWidth);
            effectObj = (GameObject)editObj;

            //middle
            EditorGUILayout.BeginHorizontal();
            {
                //list
                EditorLib.setListLayer(ref posScroll_1, effectXMLData, ref nowCode, ref editObj, this.sizeLargeWidth);
                effectObj = editObj as GameObject;
                //contents
                //컨텐츠는 다 다르기 때문에 여기서 만들어 놓음
                EditorGUILayout.BeginVertical();
                {
                    posScroll_2 = EditorGUILayout.BeginScrollView(this.posScroll_2);
                    {
                        if (effectXMLData.getDataCnt() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                EditorGUILayout.LabelField("코드", nowCode.ToString(), GUILayout.Width(sizeLargeWidth));
                                effectXMLData.idx[nowCode] = EditorGUILayout.TextField(
                                    "고유코드", effectXMLData.idx[nowCode], GUILayout.Width(sizeLargeWidth * 1.5f
                                    ));

                                effectXMLData.effectAttrs[nowCode].effectAttrType = (EffectAttrType)EditorGUILayout.EnumPopup(
                                    "이펙트 타입", effectXMLData.effectAttrs[nowCode].effectAttrType, GUILayout.Width(sizeLargeWidth
                                    ));

                                EditorGUILayout.Separator();

                                if (effectObj == null && effectXMLData.effectAttrs[nowCode].effectObjName != string.Empty)
                                {
                                    effectXMLData.effectAttrs[nowCode].effectPreLoad();
                                    effectObj = (GameObject)ResourceManager.Load(
                                        effectXMLData.effectAttrs[nowCode].effectObjPath + effectXMLData.effectAttrs[nowCode].effectObjName
                                        );
                                }

                                effectObj = (GameObject)EditorGUILayout.ObjectField(
                                    "이펙트", this.effectObj, typeof(GameObject), false, GUILayout.Width(sizeLargeWidth * 1.5f)
                                    );

                                string _tmpEffectObjPath = string.Empty;
                                string _tmpEffectObjName = string.Empty;
                                if (effectObj != null)
                                {
                                    _tmpEffectObjName = effectObj.name;
                                    _tmpEffectObjPath = EditorLib.getAssetPath(this.effectObj);
                                }
                                effectXMLData.effectAttrs[nowCode].effectObjPath = _tmpEffectObjPath;
                                effectXMLData.effectAttrs[nowCode].effectObjName = _tmpEffectObjName;
                                EditorGUILayout.Separator();
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        //bottom
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("ReLoad"))
            {
                effectXMLData = CreateInstance<EffectXMLData>();
                effectXMLData.LoadData();
                nowCode = 0;
                this.effectObj = null;
            }
            if (GUILayout.Button("Save"))
            {
                EffectEditor.effectXMLData.writeXMLData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void CreateEnumStructure()
    {
        string enumName = "EffectList";
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();

        for (int i = 0; i < effectXMLData.idx.Length; i++)
        {
            builder.AppendLine("           " + effectXMLData.idx[i] + "=" + i + ",");
        }
        EditorLib.makeEnumClass(enumName, builder);
    }
}
