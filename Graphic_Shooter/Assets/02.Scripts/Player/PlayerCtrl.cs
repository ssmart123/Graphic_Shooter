using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Animations.Rigging;

public enum PlayerState { Normal = 0, Sprint, Crouch, Jump }

namespace SSM
{
    public class PlayerCtrl : PlayerInit
    {
        // 플레이어 상태
        public PlayerState e_PlayerState = PlayerState.Normal;

        // 입력값 저장 변수
        private float m_HInput = 0;
        private float m_VInput = 0;

        // 이동 방향 벡터
        private Vector3 m_TargetMoveDirection = Vector3.zero;   // 카메라 전방방향을 바라보는 방향벡터
        private Vector3 m_CalcTargetDirection = Vector3.zero;   // 이동속도, 중력을 적용한 벡터
        private Vector3 m_SaveJumpDirectino = Vector3.zero;     // 점프시 이동방향을 임시 저장하는 벡터

        // 캐릭터 회전 속도
        private float m_RotSpeed = 15f;
        private float m_PlayerApplySpeed = 0;
        private float m_AimDuration = 0.15f;
        private float m_SprintDuration = 0.3f;

        private float m_CurFireRate = 0;

        private int m_PlayerCurHP;

        // 상태변수
        private bool isMoveSprint = false;
        private bool isMoveCrouch = false;
        private bool isGround = true;
        private bool isAim = false;

        // 컴포넌트 연결
        public Camera playerCamera = null;
        public Rig rigSprintLayer = null;
        public Rig rigAimLayer = null;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rigidBody;
        private Animator playerAnimator;
        public GunCtrl gunCtrl = null;
        public GameMgr gameMgr = null;

        public bool isAimP
        {
            get { return isAim; }
            set { isAim = value; }
        }

        private void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            rigidBody = GetComponent<Rigidbody>();
            playerAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            m_CurFireRate = gunCtrl.FireRateP;
            m_PlayerCurHP = m_PlayerHP;
        }

        private void FixedUpdate()
        {
            ChackIsGround();
            RigidMove();
        }

        private void ChackIsGround()
        {
            isGround = Physics.Raycast(transform.position+Vector3.up*0.1f, Vector3.down, capsuleCollider.center.y+ 0.3f);

            //if (isGround == true)
            //    e_PlayerState = PlayerState.Normal;
        }
        void Update()
        {
            StateMoveChange();
            KeyInput();
            CalcMoveDirection();
            PlayerRotation();
            StatePlayAnimation();

            AimChange();
        }

        private void StateMoveChange()
        {
            switch (e_PlayerState)
            {
                case PlayerState.Normal:
                    m_PlayerApplySpeed = m_PlayerRunSpeed;
                    rigSprintLayer.weight -= Time.deltaTime / m_SprintDuration;
                    break;


                case PlayerState.Sprint:
                    m_PlayerApplySpeed = m_PlayerSprintSpeed;
                    rigSprintLayer.weight += Time.deltaTime / m_SprintDuration;
                    isMoveCrouch = false;
                    isAim = false;
                    break;

                case PlayerState.Crouch:
                    m_PlayerApplySpeed = m_PlayerCrouchSpeed;
                    isMoveSprint = false;
                    break;

                case PlayerState.Jump:
                    isMoveCrouch = false;
                    isMoveSprint = false;
                    isAim = false;
                    break;
            }

        }
        private void StatePlayAnimation()
        {
            playerAnimator.SetFloat("Horizontal", m_HInput);
            playerAnimator.SetFloat("Vertical", m_VInput);


            switch (e_PlayerState)
            {
                case PlayerState.Normal:
                    //if(isMoveBlock == true)
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Sprint:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Crouch:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Jump:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;
            }
        }
        private void KeyInput()
        {
            m_HInput = Input.GetAxis("Horizontal");
            m_VInput = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
                TrySprint();
            if (Input.GetKeyUp(KeyCode.LeftShift))
                CancleSprint();

            if (Input.GetKeyDown(KeyCode.C))
                TryCrouch();

            if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
                TryJump();

            if (Input.GetMouseButton(0))
                TryFire();

            if (Input.GetMouseButtonDown(1))
                TryAim();

            
        }


        private void TrySprint()
        {
            isMoveSprint = true;

            if (m_TargetMoveDirection.magnitude <= 0.1)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }
            if (isMoveSprint == true && m_VInput <= 0.8f)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }

            e_PlayerState = PlayerState.Sprint;
            playerAnimator.SetLayerWeight(1, 0);
        }
        private void CancleSprint()
        {
            isMoveSprint = false;
            e_PlayerState = PlayerState.Normal;

            rigSprintLayer.weight -= Time.deltaTime / m_SprintDuration;

            playerAnimator.SetLayerWeight(1, 1);
        }
        private void TryCrouch()
        {
            isMoveCrouch = !isMoveCrouch;
            if (isMoveCrouch == true)
                e_PlayerState = PlayerState.Crouch;
            else
                e_PlayerState = PlayerState.Normal;
        }
        private void TryJump()
        {
            e_PlayerState = PlayerState.Jump;
            // 점프시 이동방향 고정
            // m_SaveJumpDirectino = m_CalcTargetDirection;

            rigidBody.AddForce(new Vector3(m_SaveJumpDirectino.x, m_PlayerJumpForce, m_SaveJumpDirectino.z), ForceMode.Impulse);

            playerAnimator.SetTrigger("doJump");
        }

        private void TryFire()
        {

            m_CurFireRate -= Time.deltaTime;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (m_CurFireRate <= 0)
            {
                gunCtrl.Fire();
                m_CurFireRate = gunCtrl.FireRateP;
            }
        }
        private void TryAim()
        {
            isAimP = !isAimP;
        }
        private void AimChange()
        {
            if (isAimP == true)
                rigAimLayer.weight += Time.deltaTime / m_AimDuration;
            else
                rigAimLayer.weight -= Time.deltaTime / m_AimDuration;
        }

        private void CalcMoveDirection()
        {
            Vector3 a_CameraForward = playerCamera.transform.forward;
            a_CameraForward.y = 0;

            Vector3 a_CameraRight = Vector3.Cross(Vector3.up, a_CameraForward);

            m_TargetMoveDirection = (a_CameraForward * m_VInput) + (a_CameraRight * m_HInput);
            m_TargetMoveDirection.Normalize();

            m_CalcTargetDirection = new Vector3(m_TargetMoveDirection.x * m_PlayerApplySpeed, -0.3f, m_TargetMoveDirection.z * m_PlayerApplySpeed);
        }
        private void RigidMove()
        {
            //    if (isGround == false)
            //        rigidBody.AddForce(m_SaveJumpDirectino, ForceMode.VelocityChange);
            //    else
            rigidBody.AddForce(m_CalcTargetDirection, ForceMode.VelocityChange); // 질량을 무시하고 강체에 즉각적인 속도 변경을 추가합니다.
        }
        private void PlayerRotation()
        {
            if (e_PlayerState == PlayerState.Sprint)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(m_TargetMoveDirection),
                                                      Time.deltaTime * m_RotSpeed);
            }
            else
            {
                float a_yawCamera = playerCamera.transform.eulerAngles.y;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.Euler(new Vector3(0, a_yawCamera, 0)),
                                                      Time.deltaTime * m_RotSpeed);
            }
        }
        float a_HPRatio;
        private void OnTriggerEnter(Collider other)
        {
            //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
            if (other.gameObject.tag == "PUNCH")
            {
                if (m_PlayerCurHP <= 0.0f)
                    return;

                m_PlayerCurHP -= 10;

                //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
                //Debug.Log("Player HP = " + hp.ToString());

                a_HPRatio = (float)m_PlayerCurHP / (float)m_PlayerHP;

                gameMgr.HPFillP = a_HPRatio;

                //Player의 생명이 0이하이면 사망 처리
                if (m_PlayerCurHP <= 0)
                {
                    PlayerDie();
                }
            }//if (other.gameObject.tag == "PUNCH")

            if (other.gameObject.tag == "DeadZone")
            {

                m_PlayerCurHP = 0;
                a_HPRatio = (float)m_PlayerCurHP / (float)m_PlayerHP;

                gameMgr.HPFillP = a_HPRatio;
                PlayerDie();
            }
        }
        //Player의 사망 처리 루틴
        void PlayerDie()
        {
            // Debug.Log("Player Die !!");

            playerAnimator.SetTrigger("doDie");

            //MONSTER라는 Tag를 자진 모든 게임오브젝트를 찾아옴
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

            gameMgr.EndTextUpdate("패배");

            //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
            foreach (GameObject monster in monsters)
            {
                monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            }

            GameMgr.s_GameState = GameState.GameEnd;
        }
    }

}