using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. ī�޶�� ���� ��ġ ������ �Ǻ� ������ ����
/// 2. �浹üũ : ����üũ
/// ĳ���ͷκ��� ī�޶� ����
/// ī�޶�κ��� ĳ���� ����
/// 3. Recoil
/// 4. FOV
/// </summary>
[RequireComponent(typeof(Camera))] //ī�޶� ���� ���� ���� ����
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

    //�׳� transform���� �ҷ����� �ӵ��� ���� ����� ������ �ֱ� ������ ĳ���� �Ѵ�
    private Transform transformCamera;
    private Camera fovCamera;
    //�÷��̾�κ��� ī�޶� ������ ����
    private Vector3 posRealCamera;
    //ī�޶�κ��� �÷��̾� ������ �Ÿ�
    private float posDistanceRealCamera;

    private Vector3 lerpPivotOffset;
    private Vector3 lerpCamOffset;
    private Vector3 targetPivotOffset; //Ÿ������ ����
    private Vector3 targetCamOffset; //

    private float lerpDefaultFOV;
    private float lerpTargetFOV;

    //�ݵ� �ִ� ����
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

    //�ٸ� �ڵ忡 ������ ���� �ʴ� �ڵ�
    private void Awake()
    {
        transformCamera = transform;
        fovCamera = transformCamera.GetComponent<Camera>();

        transformCamera.position = charactorPlayer.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        transformCamera.rotation = Quaternion.identity;

        posRealCamera = transformCamera.position - charactorPlayer.position;
        posDistanceRealCamera = posRealCamera.magnitude - 0.5f;

        //���� �ʱ갪
        lerpPivotOffset = pivotOffset;
        lerpCamOffset = camOffset;

        lerpDefaultFOV = fovCamera.fieldOfView;
        angleHorizontal = charactorPlayer.eulerAngles.y;

        //���� ����
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
    //���� üũ
    bool ckDoubleViewingPos(Vector3 ckPos, float offset)
    {
        float playerFocusHeight = charactorPlayer.GetComponent<CapsuleCollider>().height * 0.75f;
        return ckViewingPos(ckPos, playerFocusHeight) && ckViewingPosR(ckPos, playerFocusHeight, offset);
    }
    private void Update()
    {
        //���� ������ �ݴ�� x����
        angleHorizontal += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1f) * aimingMouseSpeedH;
        angleVertical += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1f) * aimingMouseSpeedV;

        angleVertical = Mathf.Clamp(angleVertical,angleMinV,maxVerticaleAngleTartet);

        angleVertical = Mathf.LerpAngle(angleVertical, angleVertical + angleRecoil, 10 * Time.deltaTime);
        //ī�޶� ȸ��
        Quaternion camRotationY = Quaternion.Euler(0f, angleHorizontal, 0f);
        Quaternion aimRotation = Quaternion.Euler(-angleVertical, angleHorizontal, 0f);
        transformCamera.rotation = aimRotation;

        fovCamera.fieldOfView = Mathf.Lerp(fovCamera.fieldOfView, lerpTargetFOV, Time.deltaTime);
        //����
        Vector3 posBaseTemp = charactorPlayer.position + camRotationY * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset;

        //���ؽ�
        //Ÿ���� �÷��̾����׷� �ٰ���
        for(float offsetZ= targetCamOffset.z; offsetZ<=0f;offsetZ+=0.5f)
        {
            noCollisionOffset.z = offsetZ;
            if(ckDoubleViewingPos(posBaseTemp+aimRotation*noCollisionOffset,Mathf.Abs(offsetZ))/*Ÿ���� ������ ���� ��*/||offsetZ==0f)//�ʹ� ����� ��
            {
                break; //��������
            }
        }

        lerpCamOffset = Vector3.Lerp(lerpCamOffset, noCollisionOffset, smooth * Time.deltaTime);
        lerpPivotOffset = Vector3.Lerp(lerpPivotOffset, targetPivotOffset, smooth * Time.deltaTime);

        transformCamera.position = charactorPlayer.position + camRotationY * lerpPivotOffset + aimRotation * lerpCamOffset;
        //�� �� �ݵ�
        if(angleRecoil>0.0f)
        {
            angleRecoil -= angleBounceRecoil * Time.deltaTime;
        }else if (angleRecoil<0.0f)
        {
            angleRecoil += angleBounceRecoil * Time.deltaTime;
        }
    }
    //���ݽ� �Ÿ��� ������� ���� �ε巴�� �����̵���
    public float getCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset-lerpPivotOffset).magnitude);
    }
}