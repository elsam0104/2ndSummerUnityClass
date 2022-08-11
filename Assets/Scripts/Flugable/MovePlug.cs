using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 점프 에임
/// </summary>
public class MovePlug : BasePlugAble
{
    //걷기 뛰기 부스터 
    public float spdWalk = 0.15f;
    public float spdRun = 1.0f;
    public float spdBooster = 2.0f;
    //뎀프
    public float spdDampTime = 0.1f;
    //현재 점프중인가
    private bool flagJumpping;

    //애니메이션 체크 점프 /땅에있는지
    private int ckGrounded;
    private int ckJump;

    //점프할 높이
    public float jumpHeight = 1.5f;
    //점프 시 관성
    public float JumpFrontForce = 10f;
    //마우스 휠로 속도
    public float spdMouse, spdSeeker;

    //충돌 체크
    private bool flagColliding;
    private CapsuleCollider capsuleCollider;

    //캐릭터 캐싱
    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        ckJump = Animator.StringToHash("Jump");
        ckGrounded = Animator.StringToHash("Grounded");
        controllerPlug.GetAnimator.SetBool(ckGrounded, true);

        //콘센트에 플러그를 등록
        controllerPlug.AddPlugs(this);
        //콘센트에 기본 플러그 등록
        controllerPlug.regDefaultPlugs(this.plugsCode);
        //현재 스피드는 달리기
        spdSeeker = spdRun;
    }

    Vector3 rotationMove(float horizontal, float vertical)
    {
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);//카메라가 바라보는 앞
        forward.y = 0;
        forward = forward.normalized;//단위백터

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);//내적
        Vector3 targetDirection = Vector3.zero;//초기화

        targetDirection = forward * vertical + right * horizontal; //앞 *앞뒤(상수) + 오른쪽 * 옆(상수)

        if (controllerPlug.getFlagMoving() && targetDirection != Vector3.zero)//이동중이거나 타겟이 제로가 아니면
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(controllerPlug.GetRigidbody.rotation, targetRotation, controllerPlug.lerpTurn);
            controllerPlug.GetRigidbody.MoveRotation(newRotation);
            controllerPlug.setDirLast(targetDirection);
        }
        if (Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f)//각도 범위를 벗어나면 리셋
        {
            controllerPlug.restPosition();
        }
        return targetDirection;
    }

    private void deleteVerticalVelocity()
    {
        Vector3 horizontalVelocity = controllerPlug.GetRigidbody.velocity;
        horizontalVelocity.y = 0f;
        controllerPlug.GetRigidbody.velocity = horizontalVelocity;
    }

    void moveManager(float horizontal, float vertical)
    {
        if (controllerPlug.getFlagGrounded())
        {
            controllerPlug.GetRigidbody.useGravity = true;
        }
        else if (!controllerPlug.GetAnimator.GetBool(ckJump) && controllerPlug.GetRigidbody.velocity.y > 0)//점프중도 아닌데 공중에 떠있다 = 낑김
        {
            deleteVerticalVelocity();
        }
        rotationMove(horizontal, vertical);//몸 회전

        Vector3 dir = new Vector2(horizontal, vertical);
        spdMouse = Vector2.ClampMagnitude(dir, 1f).magnitude;//백터 길이제한
        spdSeeker += Input.GetAxis("Mouse ScrollWheel");
        spdSeeker = Mathf.Clamp(spdSeeker, spdWalk, spdRun);
        spdMouse *= spdSeeker;
        if (controllerPlug.getFlagReadyRunning())//부스터 가능하면
        {
            spdMouse = spdBooster;//부스터에 집어넣음
        }
        controllerPlug.GetAnimator.SetFloat((int)spdFloat, spdMouse, spdDampTime, Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        flagColliding = true;
        if (controllerPlug.getFlagCurrentPlugs(plugsCode) 
            && collision.GetContact(0).normal.y <= 0.1f)
        //GetContact(0) = 첫번째로 만난 충돌체. 만약 y가 0.1이하면 경사면으로 추측
        {
            float vel = controllerPlug.GetAnimator.velocity.magnitude;
            Vector3 targetMove = Vector3.ProjectOnPlane(
                playerTransform.forward,
                collision.GetContact(0).normal
                ).normalized * vel;
            controllerPlug.GetRigidbody.AddForce(targetMove, ForceMode.VelocityChange);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        flagColliding = false;
    }
    void jumpManager()
    {
        if (flagJumpping && !controllerPlug.GetAnimator.GetBool(ckJump)
            && controllerPlug.getFlagGrounded())//뛰려 준비하는 상태
        {
            controllerPlug.LockPlugs(plugsCode);
            controllerPlug.GetAnimator.SetBool(ckJump, true);
            if (controllerPlug.GetAnimator.GetFloat("Speed") > 0.1f)//뜨려고 하는 중
            {
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;

                deleteVerticalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                controllerPlug.GetRigidbody.AddForce(Vector3.up * velocity,
                    ForceMode.VelocityChange
                    );
            }
        }else if(controllerPlug.GetAnimator.GetBool(ckJump))
        {
            if(!controllerPlug.getFlagGrounded()&&!flagColliding
                &&controllerPlug.getLockStatus())
            {
                controllerPlug.GetRigidbody.AddForce(
                    playerTransform.forward * JumpFrontForce * 
                    Physics.gravity.magnitude * spdBooster, ForceMode.Acceleration);
            }

            if(controllerPlug.GetRigidbody.velocity.y<0f&&controllerPlug.getFlagGrounded())
            {
                controllerPlug.GetAnimator.SetBool(ckGrounded, true);

                //마찰력 다시 줌
                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.6f;

                flagJumpping = false;

                controllerPlug.GetAnimator.SetBool(ckJump, false);

                controllerPlug.UnLockPlugs(this.plugsCode);
            }
        }
    }
    void Update()
    {
        if(!flagJumpping&&Input.GetButtonDown("Jump")
            &&controllerPlug.getFlagCurrentPlugs(plugsCode)
            &&!controllerPlug.getOverriding())
        {
            flagJumpping = true;
        }
    }

    public override void childFixedUpdate()
    {
        moveManager(controllerPlug.GetHorizontal, controllerPlug.GetVertial);
        jumpManager();
    }
}
