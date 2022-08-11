using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �̵� ���� ����
/// </summary>
public class MovePlug : BasePlugAble
{
    //�ȱ� �ٱ� �ν��� 
    public float spdWalk = 0.15f;
    public float spdRun = 1.0f;
    public float spdBooster = 2.0f;
    //����
    public float spdDampTime = 0.1f;
    //���� �������ΰ�
    private bool flagJumpping;

    //�ִϸ��̼� üũ ���� /�����ִ���
    private int ckGrounded;
    private int ckJump;

    //������ ����
    public float jumpHeight = 1.5f;
    //���� �� ����
    public float JumpFrontForce = 10f;
    //���콺 �ٷ� �ӵ�
    public float spdMouse, spdSeeker;

    //�浹 üũ
    private bool flagColliding;
    private CapsuleCollider capsuleCollider;

    //ĳ���� ĳ��
    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        ckJump = Animator.StringToHash("Jump");
        ckGrounded = Animator.StringToHash("Grounded");
        controllerPlug.GetAnimator.SetBool(ckGrounded, true);

        //�ܼ�Ʈ�� �÷��׸� ���
        controllerPlug.AddPlugs(this);
        //�ܼ�Ʈ�� �⺻ �÷��� ���
        controllerPlug.regDefaultPlugs(this.plugsCode);
        //���� ���ǵ�� �޸���
        spdSeeker = spdRun;
    }

    Vector3 rotationMove(float horizontal, float vertical)
    {
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);//ī�޶� �ٶ󺸴� ��
        forward.y = 0;
        forward = forward.normalized;//��������

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);//����
        Vector3 targetDirection = Vector3.zero;//�ʱ�ȭ

        targetDirection = forward * vertical + right * horizontal; //�� *�յ�(���) + ������ * ��(���)

        if (controllerPlug.getFlagMoving() && targetDirection != Vector3.zero)//�̵����̰ų� Ÿ���� ���ΰ� �ƴϸ�
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(controllerPlug.GetRigidbody.rotation, targetRotation, controllerPlug.lerpTurn);
            controllerPlug.GetRigidbody.MoveRotation(newRotation);
            controllerPlug.setDirLast(targetDirection);
        }
        if (Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f)//���� ������ ����� ����
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
        else if (!controllerPlug.GetAnimator.GetBool(ckJump) && controllerPlug.GetRigidbody.velocity.y > 0)//�����ߵ� �ƴѵ� ���߿� ���ִ� = ����
        {
            deleteVerticalVelocity();
        }
        rotationMove(horizontal, vertical);//�� ȸ��

        Vector3 dir = new Vector2(horizontal, vertical);
        spdMouse = Vector2.ClampMagnitude(dir, 1f).magnitude;//���� ��������
        spdSeeker += Input.GetAxis("Mouse ScrollWheel");
        spdSeeker = Mathf.Clamp(spdSeeker, spdWalk, spdRun);
        spdMouse *= spdSeeker;
        if (controllerPlug.getFlagReadyRunning())//�ν��� �����ϸ�
        {
            spdMouse = spdBooster;//�ν��Ϳ� �������
        }
        controllerPlug.GetAnimator.SetFloat((int)spdFloat, spdMouse, spdDampTime, Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        flagColliding = true;
        if (controllerPlug.getFlagCurrentPlugs(plugsCode) 
            && collision.GetContact(0).normal.y <= 0.1f)
        //GetContact(0) = ù��°�� ���� �浹ü. ���� y�� 0.1���ϸ� �������� ����
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
            && controllerPlug.getFlagGrounded())//�ٷ� �غ��ϴ� ����
        {
            controllerPlug.LockPlugs(plugsCode);
            controllerPlug.GetAnimator.SetBool(ckJump, true);
            if (controllerPlug.GetAnimator.GetFloat("Speed") > 0.1f)//�߷��� �ϴ� ��
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

                //������ �ٽ� ��
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
