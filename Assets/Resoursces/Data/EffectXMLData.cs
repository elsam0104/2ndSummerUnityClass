using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System;
using System.IO;
public class EffectXMLData : DefaultData
{
    public EffectAttr[] effectAttrs = new EffectAttr[0];

    public string effectAttrPath = "Resources/Prefab/Effects";

    private string xmlPath = "";
    private string xmlName = "effectXmlData.xml";
    private string dataPath = "Data/effectData";

    private const string EFFECT = "effect";
    private const string DEFAULT = "default";
    public void writeXMLData()
    {
        //try catch + 오류 강제로 삭제
        using (XmlTextWriter xmlinfo = new XmlTextWriter(xmlPath + xmlName, System.Text.Encoding.Unicode))
        {
            xmlinfo.WriteStartDocument();

            xmlinfo.WriteStartElement(EFFECT);
            xmlinfo.WriteElementString("length", getDataCnt().ToString());
            for (int i = 0; i < this.idx.Length; i++)
            {
                EffectAttr attr = this.effectAttrs[i];
                xmlinfo.WriteStartElement(DEFAULT);
                xmlinfo.WriteElementString("code", i.ToString());
                xmlinfo.WriteElementString("idx", this.idx[i]);
                xmlinfo.WriteElementString("effectAttrType", attr.effectAttrType.ToString());
                xmlinfo.WriteElementString("effectObjName", attr.effectObjName.ToString());
                xmlinfo.WriteElementString("effectObjPath", attr.effectObjPath.ToString());
                xmlinfo.WriteEndElement();
            }
            xmlinfo.WriteEndElement();
            xmlinfo.WriteEndDocument();
        }
    }
    public void LoadData()
    {
        this.xmlPath = Application.dataPath + dataPath;
        TextAsset dataAsset = (TextAsset)ResourceManager.Load(xmlPath);
        if (dataAsset == null || dataAsset.text == null)
        {
            this.constuctorData("not thing!");
            return;
        }

        using (XmlTextReader xmlinfo = new XmlTextReader(new StringReader(dataAsset.text)))
        {
            int nowCode = 0;
            while (xmlinfo.Read()) //도큐먼트 읽고 있는 동안
            {
                if (xmlinfo.IsStartElement())
                {
                    switch (xmlinfo.Name)
                    {
                        case "length":
                            int length = int.Parse(xmlinfo.ReadString());
                            this.idx = new string[length];
                            this.effectAttrs = new EffectAttr[length];
                            break;
                        case "code":
                            int code = int.Parse(xmlinfo.ReadString());
                            this.effectAttrs[nowCode] = new EffectAttr();
                            this.effectAttrs[nowCode].code = nowCode;
                            break;
                        case "idx":
                            this.idx[nowCode] = xmlinfo.ReadString();
                            break;
                        case "effectAttrType":
                            this.effectAttrs[nowCode].effectAttrType
                                = (EffectAttrType)Enum.Parse(typeof(EffectAttrType), xmlinfo.ReadString());
                            break;
                        case "effectObjName":
                            this.effectAttrs[nowCode].effectObjName = xmlinfo.ReadString();
                            break;
                        case "effectObjPath":
                            this.effectAttrs[nowCode].effectObjPath = xmlinfo.ReadString();
                            break;
                    }
                }
            }
        }
    }
    public static T[] Add<T>(T[] idx,T _name)
    {
        ArrayList _tmpList = new ArrayList();

        foreach (T _val in idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(_name);
        return (T[])_tmpList.ToArray(typeof(T));
    }
    public override int constuctorData(string _dataidx)
    {
        if(this.idx == null)
        {
            this.idx = new string[] { name };
            this.effectAttrs = new EffectAttr[]
            {
                new EffectAttr()
            };

        }else
        {
            Add<string>(this.idx, _dataidx);
            //Add<EffectAttr>(this.effectAttrs, _dataidx);

            //ArrayList _tmpList = new ArrayList();
            //foreach (EffectAttr _val in this.effectAttrs)
            //{
            //    _tmpList.Add(_val);
            //}
            //_tmpList.Add(_dataidx);
            //this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
        }
        return getDataCnt();
    }

    public override void deleteData(int _pid)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(_pid);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        if(this.idx.Length == 0)
        {
            this.idx = null;
        }

        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(_pid);
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
    }

    //게임 종료될 때 정리
    public void ClearData()
    {
        foreach(EffectAttr attr in this.effectAttrs)
        {
            attr.deleteEffect();
        }
        this.effectAttrs = null;
        this.idx = null;
    }

    public override void defullcateData(int _pid)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(this.idx[_pid]);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(GetCopy(_pid));
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr));
    }

    public EffectAttr GetCopy(int _pid)
    {
        if(_pid <0||_pid>=this.effectAttrs.Length)
        {
            return null; 
        }
        EffectAttr beforeAttr = this.effectAttrs[_pid];
        EffectAttr attr = new EffectAttr();
        attr.effectFullPath = beforeAttr.effectFullPath;
        attr.effectObjName = beforeAttr.effectObjName;
        attr.effectAttrType = beforeAttr.effectAttrType;
        attr.effectObjPath = beforeAttr.effectObjPath;

        attr.code = this.effectAttrs.Length;

        return attr;
    }

    public EffectAttr GetAttr(int _pid)
    {
        if (_pid < 0 || _pid >= this.effectAttrs.Length)
        {
            return null;
        }

        effectAttrs[_pid].effectPreLoad();
        return effectAttrs[_pid];
    }
}