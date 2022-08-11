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

    //조준 넘겨줌
    private int flagAim;
    //현재 조준한 상태인가
    private bool flagAimming;
    private int conerAimming;
    private bool aimCorner;

    //애니메이션 ik
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

    //회전 만드는 법
    void Rotation()
    {
        // 카메라(시야)가 보는 앞 방향을 체크해야함
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);
        //y값 초기화
        forward.y = 0f;
        //방향백터로 만들어줌
        forward = forward.normalized;
        //수평을 어디에 둘 것인가 (마우스,캐릭터 어깨 등)
        Quaternion targetRotation = Quaternion.Euler(0f,
            controllerPlug.getCameraScript.getHorizontal, 0.0f); //y축 중심으로 회전(중간값이 y)
        float minSpd = Quaternion.Angle(playerTransform.rotation, targetRotation) * lerpAimTurn;
        //일반 회전
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
        // 캐릭터가 돈 후 카메라가 돌아야 자연스러움
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
                //레이 아웃 안 쓰고 그림 띄우기
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
