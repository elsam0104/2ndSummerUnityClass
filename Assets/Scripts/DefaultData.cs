using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 공통적인 데이터 클래스 입니다
/// 1. 데이터 총 갯수
/// 2. 데이터 목록
/// 3. 추가 삭제 복사
/// </summary>
public class DefaultData : ScriptableObject //대량의 데이터를 저장하는데 사용할 수 있는 데이터 컨테이너
{
    public const string pathData = "/Resoursces/Data/";
    //데이터 패스. 리소스폴더까진 찾아올 수 있어서 그 후부터 작성

    //고유 아이디 속성
    //상한값이 없는 list보다 좀 더 유동적이지 않은 것을 쓰기 위해 배열 사용
    public string[] idx = null;
    //length는 길이 count는 갯수
    //list는 중간에 빈 공간이 있을 수 있으니까

    public DefaultData() { }

    //총 갯수 가져오기
    public int getDataCnt()
    {
        int _retCnt = 0;

        if (this.idx != null) //로딩이 되었는지 안 되었는지 체크하기 편리. (로딩 되기 전엔 null일것이기 때문)
        {
            _retCnt = this.idx.Length; //this 쓰는 이유 : 오타 방지^^
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
            if (strFilter != "")//파일 탐색기 검색기능과 같은 용도
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
