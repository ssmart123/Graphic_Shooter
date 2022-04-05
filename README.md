# <Graphic_Shooter>  
IK애니메이션을 이용한 TPS게임입니다.

<게임 방법>
- WASD를 이용하여 캐릭터를 움직입니다.
- 마우스를 사용하여 조준을 하고 좌클릭을 눌러 총을 발사합니다.
- 좌측 상단 체력바가 다 떨어지면 게임오버가 됩니다.
- 체력바 밑 하얀색 게이지가 가득찬 뒤 G를 누르면 스킬을 사용할 수가 있고 시전중에는 적들이 멈춤니다.
- 적을 처치하면 점수가 50점씩 오르게 됩니다.

## 1. 타이틀 화면
https://user-images.githubusercontent.com/63942174/161748121-b4f0eb94-e665-4e2f-a31b-f1f2c67a9a43.mp4

    타이틀 화면입니다. 아무 키나 누른 후 START버튼을 누르면 게임이 시작되게 됩니다.
    맵씬과 플레이어씬을 분리해 놔서 UI를 적용하기 편하도록 만들었습니다.

<details>
    <summary>타이틀화면(LobbyMgr)</summary>
  
``` C#
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class LobbyMgr : MonoBehaviour
{
    [Header("오브젝트")]
    public GameObject m_Title_Root;
    public GameObject m_StartMenu;
    
    [Header("텍스트")]
    public Text m_Title_PressKey;
    
    [Header("버튼")]
    public Button m_Start_Btn;
    public Button m_Exit_Btn;
    
    private bool isKeyClick = false;
    
    
    void Start()
    {
    StartCoroutine(BlinkTextAlpha());
    
    if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);
        if (m_Exit_Btn != null)
            m_Exit_Btn.onClick.AddListener(() =>
            {
#if  UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
     }
    
    
    private void Update()
    {
        if (Input.anyKeyDown && !isKeyClick)
        {
            StopAllCoroutines();
            isKeyClick = true;
            m_Title_Root.SetActive(false);
            m_StartMenu.SetActive(true);
        }
    }
    
    
#region 글자 점멸 코루틴

public IEnumerator BlinkTextAlpha()
{ 
   m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, 0);


    while (m_Title_PressKey.color.a < 1.0f)
    {
        m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, m_Title_PressKey.color.a + (Time.deltaTime/2 ));
        yield return null;
    }

    StartCoroutine(BlinkTextAlpha2());
}
public IEnumerator BlinkTextAlpha2()
{
    m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, 1);
    while (m_Title_PressKey.color.a > 0.0f)
    {
        m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, m_Title_PressKey.color.a - (Time.deltaTime/2 ));
        yield return null;
    }
    StartCoroutine(BlinkTextAlpha());
}

#endregion
}
    
```
    
 </details>



    
## 2. 카메라 이동 및 마우스 감도 설정 
https://user-images.githubusercontent.com/63942174/161751139-322bc8fc-4928-4eb1-8f45-94a5d8649734.mp4
    
     인게임에서 ESC를 누르면 설정창이 열립니다. 설정창이 열린 동안엔 TimeScale을 0으로해서 
    게임이 일시정지 되도록 구현하였습니다.
     설정 화면에서는 마우스의 좌우, 수직 감도와 포스트프로세싱 강도를 조절할 수 있도록 하였습니다.
     나가기 버튼을 누르면 타이틀 화면으로 돌아갑니다.

<details>
    <summary>CameraCtrl</summary>
  
``` C#
    
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCtrl : MonoBehaviour
{
    public Transform m_Player = null;  // 따라다닐 플레이어

    public Slider SliderH;  // 마우스 좌우감도 슬라이더
    public Slider SliderV;  // 마우스 상하감도 슬라이더
    public Text ValueH;     // 마우스 좌우감도 값
    public Text ValueV;     // 마우스 상하감도 값
    
    [Header("Field of View")]
    public int FOVMin = 70;
    public int FOVMax = 120;
    public int FOV = 90;
    
    
    //------ 감도
    [Space(10)]
    public float m_SensitiveMin = 0.01f, m_SensitiveMax = 50f;
    float m_SensitiveCurH = 10.0f;
    float m_SensitiveCurV = 10.0f;

    float m_RotSpeed = 10.0f;

    // 카메라를 조정하기 위한 방향벡터
    private Vector3 m_AimPos = Vector3.zero;  // 에임 상대 좌표
    private Vector3 m_CalcAimPos = Vector3.zero;
    private Vector3 m_CalcCamPos = Vector3.zero;

    // 현재 회전을 적용하기위한 쿼터니온
    private Quaternion m_CalcAimRotH;
    private Quaternion m_CalcCamRotV;

    // 마우스 회전값
    [HideInInspector] public float m_RotH = 0.0f;
    [HideInInspector] public float m_RotV = 0.0f;


    // 카메라 위치 계산용 변수
    [Header("카메라 위치 변수")]
    public float m_hight = 2.0f;  // 에임 높이

    public float m_Dist_Aim = 0.25f;  // 에임과 캐릭터의 거리
    public float m_Dist_Cam = 3.0f;   // 캠과 에임과의 거리

    private float zoomSpeed = 60.0f;  // 마우스 휠을 했을 때 거리가 줄어드는 속도

    private float minDist = 1.0f;  // 에임과 카메라와의 최소거리
    private float maxDist = 50; // 에임과 카메라와의 최대
    private float m_CurDist = 21.0f;  // 에임과 카메라와의 현제 거리
    // 카메라 위치 계산용 변수

    //위 아래 각도 제한
    [SerializeField] private float vMinLimit = -80.0f; //-7.0f;  
    [SerializeField] private float vMaxLimit = 80.0f; //80.0f;   



    // Start is called before the first frame update
    void Start()
    {
        // 카메라 위치 초기화
        m_AimPos = m_Player.position;
        m_AimPos.y = m_AimPos.y + m_hight;

        m_RotH = m_Player.rotation.eulerAngles.y;
        HorizontalRot(m_Player.position, m_hight);

        m_RotV = m_CurDist;
        VerticalRot(m_AimPos);

        SliderH.minValue = m_SensitiveMin;
        SliderH.maxValue = m_SensitiveMax;
        SliderH.value = m_SensitiveCurH;

        SliderV.minValue = m_SensitiveMin;
        SliderV.maxValue = m_SensitiveMax;
        SliderV.value = m_SensitiveCurV;
    }

    private void Update()
    {
        MouseInputH();
        MouseInputV();
        ValueH.text = SliderH.value.ToString("F2");
        ValueV.text = SliderV.value.ToString("F2");

    }

    void MouseInputH()
    {
        // 마우스 입력
        m_RotH += Input.GetAxis("Mouse X") * SliderH.value * m_RotSpeed * Time.deltaTime;
        m_RotV -= Input.GetAxis("Mouse Y") * SliderV.value * m_RotSpeed * Time.deltaTime;

        // 수평 회전 범위 제한
        if (m_RotH < -360)
            m_RotH += 360;
        if (m_RotH > 360)
            m_RotH -= 360;
    }
    void MouseInputV()
    {
        // 수직 회전 범위 제한
        m_RotV = Mathf.Clamp(m_RotV, vMinLimit, vMaxLimit);

        // 마우스 휠로 에임점과 카메라와의 거리 조절
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && m_Dist_Cam < maxDist)
            m_Dist_Cam += zoomSpeed * Time.deltaTime;
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && m_Dist_Cam > minDist)
            m_Dist_Cam -= zoomSpeed * Time.deltaTime;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        HorizontalRot(m_Player.position, m_hight);
        VerticalRot(m_AimPos);
        transform.LookAt(m_AimPos);
    }

    // 수평방향 회전 메소드
    Vector3 HorizontalRot(Vector3 a_PlayerPos, float a_hight)
    {
        m_AimPos = a_PlayerPos;
        m_AimPos.y = m_AimPos.y + a_hight;

        m_CalcAimRotH = Quaternion.Euler(0, m_RotH, 0);

        m_CalcAimPos.x = m_Dist_Aim;
        m_CalcAimPos.y = 0.0f;
        m_CalcAimPos.z = 0.0f;

        return m_AimPos = m_CalcAimRotH * m_CalcAimPos + m_AimPos;
    }

    // 수직방향 회전 메소드
    Vector3 VerticalRot(Vector3 a_AimPos)
    {
        m_CalcCamRotV = Quaternion.Euler(m_RotV, m_RotH, 0);

        m_CalcCamPos.x = 0;
        m_CalcCamPos.y = 0;
        m_CalcCamPos.z = -m_Dist_Cam;

        return transform.position = m_CalcCamRotV * m_CalcCamPos + a_AimPos;
    }

}
```



    
 </details>
    
    
    
## 3. 캐릭터 조작 및 총 발사
https://user-images.githubusercontent.com/63942174/161752860-2dc177d2-564c-492e-beea-37f48dda2b0d.mp4
    
https://user-images.githubusercontent.com/63942174/161752923-8e894d14-6a82-47d8-a16c-132ab0e4d717.mp4

    
    캐릭터 조작과 총 발사입니다. IK를 이용하여 애니메이션을 크게 수정하지 않고 총을 잡고
    화면 중앙의 에임에 총구를 향하도록 구현하였습니다.

    

<details>
    <summary>PlayerCtrl</summary>
  
``` C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MovementState { normal=0, run, crouch, jump, dead} // 캐릭터 상태 관련 

public class PlayerCtrl : MonoBehaviour
{
    // 에임 위치
    public Transform AimSpot;

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

    
```
    
 </details>
    
    
    
    

<details>
    <summary>IKTest</summary>
  
``` C#
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 본을 인스펙터창에 표시하기 위한 클래스
[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class IKTest : MonoBehaviour
{
    // 총기의 손 위치
    [Header("WeaponHandle")]
    public Transform LeftHandle;
    public Transform RightHandle;
    
    private Transform spine;
    private Transform head;

    public Transform targetTransform;   // 타겟 위치
    public Transform aimTransform;      // 총 견착 위치
    public Transform Headbone;          // 머리 본 위치
    
    private Animator playerAnimator;

    public int iterations = 10;     // 반복횟수
    public float weight = 1.0f;     // 보정률

    public HumanBone[] humanBones;  // 본의 갯수 배열
    Transform[] boneTransforms;  // 설정한 본에 해당되는 트렌스폼

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        spine = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);


        boneTransforms = new Transform[humanBones.Length]; // boneTransforms 배열의 크기 선언

        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i] = playerAnimator.GetBoneTransform(humanBones[i].bone);  // 본의 transform 정의
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // 사망시 움직이지 못하도록
        if (GameMgr.s_GameState == GameState.GameEnd) 
            return;
                                                  
        // 견착
        Vector3 targetPosition = targetTransform.position;  // 타겟의 위치를 가져온다.

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length-1; j++)
            {
                Transform bone = boneTransforms[j];
                float boneWeight = humanBones[j].weight * weight;
                // 총이 에임을 바라보도록 만든다.
                AimAtTarget(bone, aimTransform, targetPosition, boneWeight);
            }

            Transform hashs = boneTransforms[4];
            // 머리가 에임을 바라보도록 한다.
            AimAtTarget(hashs, Headbone, targetPosition,weight);
        }

    }

    // 타겟을 바라보도록 하는 메소드
    private void AimAtTarget(Transform bone,Transform aimPosition, Vector3 targetPosition, float weight)
    {
       // Vector3 aimDirection = aimTransform.forward;    // 에임의 전방벡터
        Vector3 aimDirection = aimPosition.forward;    // 에임의 전방벡터
        Vector3 targetDirection = targetPosition - aimPosition.position; // 타겟 위치과 에임 위치를 빼서 타겟 방향을 구함
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);  // 에임이 바라볼 방향
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    private void OnDrawGizmos()
    {
        // 어깨와 에임을 잇는 선
        Gizmos.DrawLine(aimTransform.position, targetTransform.position);
    }

    // 팔의 IK를 적용
    private void OnAnimatorIK(int layerIndex)
    {

        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        // 팔 IK애니메이션 설정
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);


        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,LeftHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,LeftHandle.rotation);

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand,RightHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand,RightHandle.rotation);

    }
}

```
    
 </details>
