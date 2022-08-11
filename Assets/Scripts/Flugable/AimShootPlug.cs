using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class AimShootPlug : BasePlugAble
{
    public Texture2D imgCrossHair;
    public float lerpAimTurn = 0.15f;
    public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0f);
    public Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f);

    //���� �Ѱ���
    private int flagAim;
    //���� ������ �����ΰ�
    private bool flagAimming;
    private int conerAimming;
    private bool aimCorner;

    //�ִϸ��̼� ik
    private Vector3 initRootRotation;
    private Vector3 initHipRotation;
    private Vector3 initSpineRotation;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = transform;
        flagAim = Animator.StringToHash("Aim");
        conerAimming = Animator.StringToHash("Corner");

        Transform hip = controllerPlug.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initRootRotation = (hip.parent == transform) ? Vector3.zero : hip.parent.localEulerAngles;
        initHipRotation = hip.localEulerAngles;
        initSpineRotation = controllerPlug
            .GetAnimator
            .GetBoneTransform(HumanBodyBones.Spine)
            .localEulerAngles;
    }

    //ȸ�� ����� ��
    void Rotation()
    {
        // ī�޶�(�þ�)�� ���� �� ������ üũ�ؾ���
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);
        //y�� �ʱ�ȭ
        forward.y = 0f;
        //������ͷ� �������
        forward = forward.normalized;
        //������ ��� �� ���ΰ� (���콺,ĳ���� ��� ��)
        Quaternion targetRotation = Quaternion.Euler(0f,
            controllerPlug.getCameraScript.getHorizontal, 0.0f); //y�� �߽����� ȸ��(�߰����� y)
        float minSpd = Quaternion.Angle(playerTransform.rotation, targetRotation) * lerpAimTurn;
        //�Ϲ� ȸ��
        //controllerPlug.setDirLast(forward);
        //playerTransform.rotation = Quaternion.Slerp(
        //    playerTransform.rotation, targetRotation, minSpd * Time.deltaTime
        //    );
        if (aimCorner) //IK
        {
            playerTransform.rotation = Quaternion.LookRotation(-controllerPlug.getDirLast());
            targetRotation *= Quaternion.Euler(initRootRotation);
            targetRotation *= Quaternion.Euler(initHipRotation);
            targetRotation *= Quaternion.Euler(initSpineRotation);

            Transform spine = controllerPlug.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation;
        }
        else
        {
            controllerPlug.setDirLast(forward);
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetRotation,
                minSpd * Time.deltaTime
                );
        }
    }

    void AimManager()
    {
        Rotation();
    }

    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);
        if (controllerPlug.getLockStatus(plugsCode) || controllerPlug.getOverriding(this))
        {
            yield return false;
        }
        else
        {
            flagAimming = true;
            int signal = 1;
            if (aimCorner)
            {
                signal = (int)Mathf.Sign(controllerPlug.GetHorizontal);
            }
            aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;

            yield return new WaitForSeconds(0.1f);
            controllerPlug.GetAnimator.SetFloat(spdFloat, 0.0f);
            controllerPlug.OverrideWithPlugs(this);
        }
    }
    private IEnumerator ToggleAinOff()
    {
        flagAimming = false;
        yield return new WaitForSeconds(0.3f);
        controllerPlug.getCameraScript.reatAimOffset();
        controllerPlug.getCameraScript.restMaxVAnagle();
        yield return new WaitForSeconds(0.1f);

        controllerPlug.UnOverridingPlugs(this);
    }

    public override void childFixedUpdate()
    {
        if (flagAimming)
        {
            controllerPlug.getCameraScript.setPosTargetOffset(aimPivotOffset, aimCamOffset);

        }
    }

    public override void childLateUpdate()
    {
        // ĳ���Ͱ� �� �� ī�޶� ���ƾ� �ڿ�������
        AimManager();
    }
    private void Update()
    {
        aimCorner = controllerPlug.GetAnimator.GetBool(conerAimming);
        if (Input.GetAxisRaw("Fire2") != 0 && !flagAimming)
        {
            StartCoroutine(ToggleAimOn());
        }
        else if (flagAimming && Input.GetAxisRaw("Fire2") == 0)
        {
            StartCoroutine(ToggleAinOff());
        }

        getFlagRun = !flagAimming;

        if (flagAimming && Input.GetButtonDown("Fire3") && !aimCorner)
        {
            aimCamOffset.x = aimCamOffset.x * -1;
            aimPivotOffset.x = aimPivotOffset.x * -1;
        }
        controllerPlug.GetAnimator.SetBool(flagAim, flagAimming);
    }

    private void OnGUI()
    {
        if (imgCrossHair != null)
        {
            float length = controllerPlug.getCameraScript.getCurrentPivotMagnitude(aimPivotOffset);
            if (length < 0.05f)
            {
                //���� �ƿ� �� ���� �׸� ����
                GUI.DrawTexture(
                    new Rect(
                        Screen.width * 0.5f - (imgCrossHair.width * 0.5f),
                        Screen.height * 0.5f - (imgCrossHair.height * 0.5f),
                        imgCrossHair.width,
                        imgCrossHair.height),
                    imgCrossHair
                        );
            }
        }
    }
}
