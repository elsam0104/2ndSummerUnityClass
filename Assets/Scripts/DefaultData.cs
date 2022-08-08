using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �������� ������ Ŭ���� �Դϴ�
/// 1. ������ �� ����
/// 2. ������ ���
/// 3. �߰� ���� ����
/// </summary>
public class DefaultData : ScriptableObject //�뷮�� �����͸� �����ϴµ� ����� �� �ִ� ������ �����̳�
{
    public const string pathData = "/Resoursces/Data/";
    //������ �н�. ���ҽ��������� ã�ƿ� �� �־ �� �ĺ��� �ۼ�

    //���� ���̵� �Ӽ�
    //���Ѱ��� ���� list���� �� �� ���������� ���� ���� ���� ���� �迭 ���
    public string[] idx = null;
    //length�� ���� count�� ����
    //list�� �߰��� �� ������ ���� �� �����ϱ�

    public DefaultData() { }

    //�� ���� ��������
    public int getDataCnt()
    {
        int _retCnt = 0;

        if (this.idx != null) //�ε��� �Ǿ����� �� �Ǿ����� üũ�ϱ� ��. (�ε� �Ǳ� ���� null�ϰ��̱� ����)
        {
            _retCnt = this.idx.Length; //this ���� ���� : ��Ÿ ����^^
        }
        return _retCnt;
    }
    public string[] getDataIdxList(bool flagID, string strFilter = "")
    {
        string[] retList = new string[0];
        if (this.idx == null)
        {
            return retList;
        }
        retList = new string[this.idx.Length];
        for (int i = 0; i < this.idx.Length; i++)
        {
            if (strFilter != "")//���� Ž���� �˻���ɰ� ���� �뵵
            {
                if (idx[i].ToLower().Contains(strFilter.ToLower()) == false)
                {
                    continue;
                }
            }

            if (flagID)
            {
                retList[i] = "[" + i.ToString() + "]" + this.idx[i];
            }
            else
            {
                retList[i] = this.idx[i];
            }
        }
        return retList;
    }
    public virtual int constuctorData(string _dataidx)
    {
        return getDataCnt();
    }
    public virtual void deleteData(int _pid) { }
    public virtual void defullcateData(int _pid) { }

}
