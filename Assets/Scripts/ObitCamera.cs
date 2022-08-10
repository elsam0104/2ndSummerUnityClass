using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 카메라로 부터 위치 오프셋 피봇 오프셋 설정
/// 2. 충돌체크 : 이중체크
/// 캐릭터로부터 카메라 사이
/// 카메라로부터 캐릭터 사이
/// 3. Recoil
/// 4. FOV
/// </summary>
[RequireComponent(typeof(Camera))] //카메라 없음 쓰지 마셈 ㅇㅇ
public class ObitCamera : MonoBehaviour
{
    public Transform charactorPlayer;

    public Vector3 pivotOffset = new Vector3(0f, 1f, 0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f;
    public float aimingMouseSpeedH = 6.0f;
    public float aimingMouseSpeedV = 6.0f;
    public float angleMaxV = 30.0f;
    public float angleMinV = -60.0f;

    public float angleBounceRecoil = 5.0f;

    private float angleHorizontal = 0.0f;
    private float angleVertical = 0.0f;

    //그냥 transform으로 불러오면 속도가 느려 끊기는 느낌을 주기 때문에 캐싱을 한다
    private Transform transformCamera;
    private Camera fovCamera;
    //플레이어로부터 카메라 까지의 백터
    private Vector3 posRealCamera;
    //카메라로부터 플레이어 까지의 거리
    private float posDistanceRealCamera;

    private Vector3 lerpPivotOffset;
    private Vector3 lerpCamOffset;
    private Vector3 targetPivotOffset; //타겟포착 범위
    private Vector3 targetCamOffset; //

    private float lerpDefaultFOV;
    private float lerpTargetFOV;

    //반동 최대 각도
    private float maxVerticaleAngleTartet;
    private float angleRecoil = 0f;

    public float getHorizontal
    {
        //get => angleHorizontal;
        get
        {
            return angleHorizontal;
        }
    }

    //다른 코드에 영향을 주지 않는 코드
    private void Awake()
    {
        transformCamera = transform;
        fovCamera = transformCamera.GetComponent<Camera>();

        transformCamera.position = charactorPlayer.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        transformCamera.rotation = Quaternion.identity;

        posRealCamera = transformCamera.position - charactorPlayer.position;
        posDistanceRealCamera = posRealCamera.magnitude - 0.5f;

        //보간 초깃값
        lerpPivotOffset = pivotOffset;
        lerpCamOffset = camOffset;

        lerpDefaultFOV = fovCamera.fieldOfView;
        angleHorizontal = charactorPlayer.eulerAngles.y;

        //리셋 삼종
        //aim
        //fov
        //angle


    }
    public void reatAimOffset()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }
    public void restFOV()
    {
        this.lerpTargetFOV = lerpDefaultFOV;
    }
    public void restMaxVAnagle()
    {
        maxVerticaleAngleTartet = angleMaxV;
    }
    public void recoilBounceAngleV(float val)
    {
        angleRecoil = val;
    }
    public void setPosTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }
    public void setFOV(float val)
    {
        this.lerpTargetFOV = val;
    }

    bool ckViewingPos(Vector3 ckPos, float playerHeight)
    {
        Vector3 target = charactorPlayer.position + (Vector3.up * playerHeight);

        if (Physics.SphereCast(ckPos, 0.2f, target - ckPos, out RaycastHit hit, posDistanceRealCamera))
        {
            if (hit.transform != charactorPlayer && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }
    bool ckViewingPosR(Vector3 ckPos, float playerHeight, float maxDistance)
    {
        Vector3 origin = charactorPlayer.position + (Vector3.up * playerHeight);

        if (Physics.SphereCast(origin, 0.2f, ckPos - origin, out RaycastHit hit, maxDistance))
        {
            if (hit.transform != charactorPlayer && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }
    //이중 체크
    bool ckDoubleViewingPos(Vector3 ckPos, float offset)
    {
        float playerFocusHeight = charactorPlayer.GetComponent<CapsuleCollider>().height * 0.75f;
        return ckViewingPos(ckPos, playerFocusHeight) && ckViewingPosR(ckPos, playerFocusHeight, offset);
    }
    private void Update()
    {
        //수직 수평이 반대라서 x곱함
        angleHorizontal += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1f) * aimingMouseSpeedH;
        angleVertical += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1f) * aimingMouseSpeedV;

        angleVertical = Mathf.Clamp(angleVertical,angleMinV,maxVerticaleAngleTartet);

        angleVertical = Mathf.LerpAngle(angleVertical, angleVertical + angleRecoil, 10 * Time.deltaTime);
        //카메라 회전
        Quaternion camRotationY = Quaternion.Euler(0f, angleHorizontal, 0f);
        Quaternion aimRotation = Quaternion.Euler(-angleVertical, angleHorizontal, 0f);
        transformCamera.rotation = aimRotation;

        fovCamera.fieldOfView = Mathf.Lerp(fovCamera.fieldOfView, lerpTargetFOV, Time.deltaTime);
        //평상시
        Vector3 posBaseTemp = charactorPlayer.position + camRotationY * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset;

        //조준시
        //타겟이 플레이어한테로 다가옴
        for(float offsetZ= targetCamOffset.z; offsetZ<=0f;offsetZ+=0.5f)
        {
            noCollisionOffset.z = offsetZ;
            if(ckDoubleViewingPos(posBaseTemp+aimRotation*noCollisionOffset,Mathf.Abs(offsetZ))/*타겟이 보이지 않을 때*/||offsetZ==0f)//너무 가까울 때
            {
                break; //조준해제
            }
        }

        lerpCamOffset = Vector3.Lerp(lerpCamOffset, noCollisionOffset, smooth * Time.deltaTime);
        lerpPivotOffset = Vector3.Lerp(lerpPivotOffset, targetPivotOffset, smooth * Time.deltaTime);

        transformCamera.position = charactorPlayer.position + camRotationY * lerpPivotOffset + aimRotation * lerpCamOffset;
        //총 쏠때 반동
        if(angleRecoil>0.0f)
        {
            angleRecoil -= angleBounceRecoil * Time.deltaTime;
        }else if (angleRecoil<0.0f)
        {
            angleRecoil += angleBounceRecoil * Time.deltaTime;
        }
    }
    //저격시 거리의 평균으로 나눠 부드럽게 움직이도록
    public float getCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset-lerpPivotOffset).magnitude);
    }
}