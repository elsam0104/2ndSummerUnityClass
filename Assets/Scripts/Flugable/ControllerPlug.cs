using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPlug : MonoBehaviour
{
    //기능들
    private List<BasePlugAble> plugs;
    //우선권이 필요한 기능 (덮어씌운다)
    private List<BasePlugAble> overridePlugs;

    //현재 기능 코드들
    private int currentPlugs;
    //기본 기능 코드들
    private int defaultPlugs;
    //잠긴 기능 코드들
    private int plugsLocked;

    //캐싱 위한 변수
    public Transform playerCamera;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private ObitCamera cameraScript;

    //horizontal, vertical axis
    private float h;
    private float v;

    //카메라를 향하도록 움직일 때 회전되는 속도
    public float lerpTurn = 0.05f;
    //달리기 기능이 카메라FOV에 기록되었나?
    private bool flagChangerFOV;
    //달리기 시야각
    public float runFOV = 100;
    //마지막으로 향했던 방향
    private Vector3 dirLast;
    //달리기 중인가?
    private bool flagRun;
    //애니메잇션 h,v축 값
    private float hFloat;
    private float vFloat;
    //땅 위에 붙어 있는가?
    private bool flagOnGround;
    //땅과 충돌체크를 위한 충돌체 영역
    private Vector3 colliderGround;

    public float GetHorizontal { get => h; }
    public float GetVertial { get => v; }
    public ObitCamera getCameraScript { get => cameraScript; }
    public Rigidbody GetRigidbody { get => playerRigidbody; }
    public Animator GetAnimator { get => playerAnimator; }

    //지금 어떤 기본 플러그가 꽃혀 있는가
    public int getDefaultPlugs { get => defaultPlugs; }

    private void Awake()
    {
        plugs = new List<BasePlugAble>();
        overridePlugs = new List<BasePlugAble>();
        playerAnimator = GetComponent<Animator>();
        cameraScript = playerCamera.GetComponent<ObitCamera>();
        playerRigidbody = GetComponent<Rigidbody>();
        colliderGround = GetComponent<Collider>().bounds.extents; //범위 안에 들어왔는지

        //애니메이션에 달려있는거라 임시
        hFloat = 0.0f;
        vFloat = 0.0f;
        flagOnGround = true;
    }

    //플레이어가 이동중인가?
    //우리가 이동하는 중에 플러그인을 붙이기 위함
    public bool getFlagMoving()
    {
        //앱실로는 극한값. 0의 근사치로써 실수를 0보다 큰지 확인할 때 쓰인다. 
        //return (h!=0)||(v!=0);
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    public bool getFlagHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    public bool getFlagRun()
    {
        foreach (BasePlugAble basePlugAble in plugs)
        {
            if (basePlugAble.flagAllowRun)
                return false;
        }
        foreach (BasePlugAble overPlugAble in overridePlugs)
        {
            if (overPlugAble.flagAllowRun)
                return false;
        }
        return true;
    }

    //달리는게 가능한가?
    public bool getFlagReadyRunning()
    {
        return flagRun && getFlagMoving() && getFlagRun();
    }
    //혹시 땅 위에 있는가? (버그로 점프 안 했는데 못들어가는곳 들어가 낑기는거 판별에 사용)
    public bool getFlagGrounded()
    {
        Ray ray = new Ray(playerTransform.position + Vector3.up * 2 * colliderGround.x, Vector3.down);
        return Physics.SphereCast(ray, colliderGround.x, colliderGround.x + 0.2f);
    }
    //키 조작 키 전환 상태 체크
    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        playerAnimator.SetFloat((int)hFloat, h, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat((int)vFloat, v, 0.1f, Time.deltaTime);
        //나중에
        flagRun = Input.GetButtonDown("Jump");

        if (getFlagReadyRunning())
        {
            flagChangerFOV = true;
            cameraScript.setFOV(runFOV);
        }
        else if (flagChangerFOV)
        {
            cameraScript.restFOV();
            flagChangerFOV = false;
        }

    }
    public void restPosition()
    {
        if (dirLast != Vector3.zero)
        {
            //y값을 0으로 지정 후 y계산하여 넣어주기 위함
            dirLast.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(dirLast);
            Quaternion newRotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, lerpTurn);
            playerRigidbody.MoveRotation(newRotation);
        }
    }
    //플러거블 조정
    private void FixedUpdate()
    {
        bool flagAnyPlugActive = false;

        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePlugAble basePlugAble in plugs)
            {
                if (basePlugAble.isActiveAndEnabled && currentPlugs == basePlugAble.GetPlugCode)
                {
                    flagAnyPlugActive = true;
                    basePlugAble.childFixedUpdate();
                }
            }
        }
        else
        {
            foreach (BasePlugAble basePlugAble in overridePlugs)
            {
                basePlugAble.childFixedUpdate();
            }
        }
        if (!flagAnyPlugActive && overridePlugs.Count == 0)
        {
            playerRigidbody.useGravity = true;
            restPosition();
        }
    }

    private void LateUpdate()
    {
        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePlugAble basePlugAble in plugs)
            {
                if (basePlugAble.isActiveAndEnabled && currentPlugs == basePlugAble.GetPlugCode)
                {
                    basePlugAble.childLateUpdate();
                }
            }
        }
        else
        {
            foreach (BasePlugAble basePlugAble in overridePlugs)
            {
                basePlugAble.childLateUpdate();
            }
        }
    }
    //플러그 추가
    public void AddPlugs(BasePlugAble basePlugAble)
    {
        plugs.Add(basePlugAble);
    }
    //디폴트,현재 코드에 플러그 추가
    public void regDefaultPlugs(int plugCode)
    {
        defaultPlugs = plugCode;
        currentPlugs = plugCode;
    }
    
    public void regPlug(int plugCode)
    {
        if (currentPlugs == defaultPlugs)
        {
            currentPlugs = plugCode;
        }
    }

    public void UnRegPlug(int plugCode)
    {
        if (currentPlugs == plugCode)
        {
            currentPlugs = defaultPlugs;
        }
    }
    //plug 우선순위로 만들기
    public bool OverrideWithPlugs(BasePlugAble basePlugAble)
    {
        if (!overridePlugs.Contains(basePlugAble))
        {
            if (overridePlugs.Count == 0)
            {
                foreach (BasePlugAble plugAble in plugs)
                {
                    if (plugAble.isActiveAndEnabled && currentPlugs == plugAble.GetPlugCode)
                    {
                        plugAble.OnOverride();
                        break;
                    }
                }
            }
            overridePlugs.Add(basePlugAble);
            return true;
        }
        return false;
    }
    //우선순위 해제
    public bool UnOverridingPlugs(BasePlugAble plugAble)
    {
        if(overridePlugs.Contains(plugAble))
        {
            overridePlugs.Remove(plugAble);
            return true;
        }
        return false;
    }
    //우선순위 플러그 탐색. 매개변수 없으면 우선순위 플러그가 있는지 확인
    public bool getOverriding(BasePlugAble basePlugAble = null)
    {
        if(basePlugAble == null)
        {
            return overridePlugs.Count > 0;
        }
        return overridePlugs.Contains(basePlugAble);
    }
    //현재 플러그인가?
    public bool getFlagCurrentPlugs(int plugCode)
    {
        return currentPlugs == plugCode;
    }

    public bool getLockStatus(int plugCode = 0)
    {
        return (plugsLocked != 0 && plugsLocked != plugCode);
    }

    public void LockPlugs(int plugCode)
    {
        if(plugsLocked==0)
        {
            plugsLocked = plugCode;
        }
    }

    public void UnLockPlugs(int plugCode)
    {
        if(plugsLocked == plugCode)
        {
            plugsLocked = 0;
        }
    }
    public Vector3 getDirLast()
    {
        return dirLast;
    }
    public void setDirLast(Vector3 dir)
    {
        dirLast = dir;
    }
}

//추상화 클래스 만드는 이유
//자식들을 하나의 클래스로 형변환 하여 거기에 플러그 관리를 할 수 있다.
public abstract class BasePlugAble : MonoBehaviour
{
    //속도 구별 (걷고 달리는거 구별)
    protected float spdFloat;
    protected ControllerPlug controllerPlug;
    //해쉬코드로 고유 코드 만들기
    protected int plugsCode;
    //할 수 있는가 여부
    protected bool getFlagRun;

    private void Awake()
    {
        this.controllerPlug = GetComponent<ControllerPlug>();
        getFlagRun = true;
        //해쉬코드로 고유 코드 만들기
        plugsCode = this.GetType().GetHashCode();
    }

    //플러그 고유 코드 가져오기
    public int GetPlugCode { get => plugsCode; }
    //달릴 수 있는가 여부
    public bool flagAllowRun { get => getFlagRun; }

    //여기 있는 플러그도 업데이트 해줘
    public virtual void childLateUpdate() { }
    public virtual void childFixedUpdate() { }

    //이 기능은 아주 중요해서 먼저 해야함
    public virtual void OnOverride() { }
}
