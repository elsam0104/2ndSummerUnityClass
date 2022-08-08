using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectAttrType
{
    None = -1, //세팅이 안 되어 있음
    Normal,
    Attack,
    Damage,
    Etc
}
public class EffectAttr
{
    public int code = 0;    //int값으로 크기를 주겠다 = 배열
    public EffectAttrType effectAttrType = EffectAttrType.None;

    public GameObject effectObj = null;

    public string effectObjName = string.Empty;
    public string effectObjPath = string.Empty;
    public string effectFullPath = string.Empty;

    public EffectAttr() { }


    //사전 로딩 기능
    public void effectPreLoad()
    {
        this.effectFullPath = effectObjPath + effectObjName;
        if(this.effectFullPath != string.Empty && this.effectObj == null)
        {
            this.effectObj = ResourceManager.Load(effectFullPath) as GameObject;
        }
    }
    //프리로드 한 이펙트 삭제
    //gc는 일정 시간마다 참조가 없는 것들을 가져감. 따라서 null로 설정하면 지워짐
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
