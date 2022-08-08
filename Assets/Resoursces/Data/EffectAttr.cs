using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectAttrType
{
    None = -1, //������ �� �Ǿ� ����
    Normal,
    Attack,
    Damage,
    Etc
}
public class EffectAttr
{
    public int code = 0;    //int������ ũ�⸦ �ְڴ� = �迭
    public EffectAttrType effectAttrType = EffectAttrType.None;

    public GameObject effectObj = null;

    public string effectObjName = string.Empty;
    public string effectObjPath = string.Empty;
    public string effectFullPath = string.Empty;

    public EffectAttr() { }


    //���� �ε� ���
    public void effectPreLoad()
    {
        this.effectFullPath = effectObjPath + effectObjName;
        if(this.effectFullPath != string.Empty && this.effectObj == null)
        {
            this.effectObj = ResourceManager.Load(effectFullPath) as GameObject;
        }
    }
    //�����ε� �� ����Ʈ ����
    //gc�� ���� �ð����� ������ ���� �͵��� ������. ���� null�� �����ϸ� ������
    public void deleteEffect()
    {
        if(this.effectObj !=null)
        {
            this.effectObj = null;
        }
    }

    public GameObject Instantiate(Vector3 pos)
    {
        if(this.effectObj== null)
        {
            this.effectPreLoad();
        }

        if(this.effectObj != null)
        {
            GameObject retEffectObj = GameObject.Instantiate(effectObj,pos,Quaternion.identity);
            return retEffectObj;
        }
        
        return null;
    }
}
