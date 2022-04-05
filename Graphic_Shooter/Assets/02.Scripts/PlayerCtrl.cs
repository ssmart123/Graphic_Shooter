using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MovementState { normal=0, run, crouch, jump, dead} // 캐릭터 상태 관련 

public class PlayerCtrl : MonoBehaviour
{

    public Transform AimSpot;

    Vector3 test;



    // 무기 위치 관련
    [Header("총 위치")]
    [SerializeField] private Transform Gun;
    [SerializeField] private Transform WeaponHolder;
    [SerializeField] private Transform RunHolder;

    // 스크립트 선언
    private Camera MainCam;
    private Animator RefAnimator;


    // 총알관련
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePos;
    //총알 발사 사운드
    public AudioClip fireSfx;
    //AudioSource 컴포넌트를 저장할 변수
    private AudioSource source = null;
    //MuzzleFlash의 MeshRenderer 컴포넌트 연결 변수
    public MeshRenderer muzzleFlash;
    // 총기 딜레이 
    public float fireDur = 0.5f;
    float curfireDur;

    // 캐릭터 진행방향 벡터
    private Vector3 m_CamVecFor = Vector3.zero;  // 카메라 전방방향 벡터
    [HideInInspector] public Vector3 m_TargetDir = Vector3.zero;  // 캐릭터 전방방향 벡터

    // 키보드 입력값
    private float h = 0.0f;
    private float v = 0.0f;

    // 이동 속도 변수
    [SerializeField] private int m_CrouchingSpeed = 3; // 앉기 이동 속도
    [SerializeField] private int m_NormalSpeed = 5;   // 기본 이동 속도
    [SerializeField] private int m_RunSpeed = 7;    // 달리기
    private float m_CurSpeed = 0;   // 현재 속도

    // 캐릭터 회전속도 변수
    [SerializeField] private int m_RotCharSpeed = 15;

    // 조준속도 변수
    [SerializeField] private int m_AimSpeed = 20;

    // 상태 변수
    bool isCrouch = false;
    bool isSprint = false;
    bool isFineSight = false;

    // 플레이어 상태
    [HideInInspector] public MovementState PlayerState;




    private void Awake()
    {
        // 선언한 스크립트 할당
        MainCam = Camera.main;
        RefAnimator = this.gameObject.GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // 변수 초기화
        m_CurSpeed = m_NormalSpeed;
        PlayerState = MovementState.normal;

        curfireDur = fireDur;

        //AudioSource 컴포넌트를 추출한 후 변수에 할당
        source = GetComponent<AudioSource>();
        //최초에 MuzzleFlash MeshRenderer를 비활성화
        muzzleFlash.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 사망시 움직이지 못하게 처리
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        // 입력
        KeyInput();
        // 총기 포지션
       FinePositionUpdate();
        // 애니메이션
        PlayerAnimation();


        // 총기 발사
        curfireDur = curfireDur - Time.deltaTime;
    }
    private void FixedUpdate()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;
        // 움직임
        Movement();
    }

    // 키보드 입력 메소드
    void KeyInput()
    {
        // 이동방향 입력
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");


        // 뛰기 입력 - 홀드
        if (Input.GetKey(KeyCode.LeftShift) && isCrouch == false && m_TargetDir.magnitude >= 0.2f)
        {
            PlayerState = MovementState.run;  // 뛰는 상태로 변경
            isSprint = true;
            RefAnimator.SetBool("IsSprint", isSprint); // 애니메이션
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            PlayerState = MovementState.normal;
            isSprint = false;
        }

        // 앉기 입력 - 토글
        if (Input.GetKeyDown(KeyCode.C) && isSprint == false)
        {
            PlayerState = MovementState.crouch;  // 앉는 상태로 변경
            isCrouch = !isCrouch;  // 스위치
        }

        if (curfireDur <= 0.0f && isSprint == false)
        {
            curfireDur = 0.0f;
            // 마우스 왼쪽 버튼을 클릭했을 때 Fire 함수 호출
            if (Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject())
            {
                Fire();
                curfireDur = fireDur;
            }
        }


        if (Input.GetButtonDown("Fire2"))
        {
            isFineSight = !isFineSight;

        }

    }


    // 캐릭터 움직임 메소드
    void Movement()
    {
        // 카메라 전방방향 벡터 가져오기
        m_CamVecFor = MainCam.transform.forward;
        m_CamVecFor.y = 0;

        // 카메라 전방벡터 내적의 오른쪽 방향 구하기
        Vector3 a_RightDir = Vector3.Cross(Vector3.up, m_CamVecFor);

        m_TargetDir = (m_CamVecFor * v) + (a_RightDir * h);
        m_TargetDir.Normalize();

        // 각종 상태에 따른 이동속도 계산
        if (isCrouch == true)
            m_CurSpeed = m_CrouchingSpeed;
        else if (isSprint == true)
            m_CurSpeed = m_RunSpeed;
        else
            m_CurSpeed = m_NormalSpeed;

        // 캐릭터 이동
        transform.position += m_TargetDir * m_CurSpeed * Time.deltaTime;

        // 캐릭터가 이동방향을 보게 회전
        //if (0.2f <= m_TargetDir.magnitude)
        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            float yawCamera = MainCam.transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), m_RotCharSpeed * Time.deltaTime);
        }

    }

    // 캐릭터 애니메이션 메소드
    void PlayerAnimation()
    {
        RefAnimator.SetFloat("hInput", h);
        RefAnimator.SetFloat("vInput", v);

        RefAnimator.SetBool("isSprint", isSprint);
        RefAnimator.SetBool("isCrouch", isCrouch);


    }

    // 사격 자세 업데이트
    private void FinePositionUpdate()
    {
        if (isSprint == true)  // 달리기 시 총 위치
        {
            RefAnimator.SetLayerWeight(1, 0);
            RefAnimator.SetLayerWeight(2, 0);
            Gun.position = Vector3.Lerp(Gun.position, RunHolder.position, m_AimSpeed * Time.deltaTime);
            Gun.rotation = RunHolder.rotation;
        }
        else
        {
            RefAnimator.SetLayerWeight(1, 1);
            RefAnimator.SetLayerWeight(2, 1);
            Gun.position = Vector3.Lerp(Gun.position, WeaponHolder.position, m_AimSpeed * Time.deltaTime);
            Gun.rotation = WeaponHolder.rotation;
        }
    }


    void Fire()
    {
        //Bullet 프리팹을 동적으로 생성
        Instantiate(bullet, firePos.position, firePos.rotation);

        //사운드 발생 함수
        source.PlayOneShot(fireSfx, 0.2f);
        //잠시 기다리는 루틴을 위해 코루틴 함수로 호출
        StartCoroutine(this.ShowMuzzleFlash());
    }

    //MuzzleFlash 활성/비활성화를 짧은 시간 동안 반복
    IEnumerator ShowMuzzleFlash()
    {
        float scale = Random.Range(1.0f, 2.0f); //(1.0f, 1.5f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localRotation = rot;

        //활성화 해서 보이게 함
        muzzleFlash.enabled = true;
        //불규칙적인 시간 동안 Delay한 다음 MeshRenderer를 비활성화
        yield return new WaitForSeconds(Random.Range(0.01f, 0.03f));  //Random.Range(0.05f, 0.3f));        
        //비활성화해서 보이지 않게 함
        muzzleFlash.enabled = false;
    }
}
