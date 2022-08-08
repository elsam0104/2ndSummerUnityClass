using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
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

    }
}