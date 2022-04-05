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
    
    
    
## 3. 캐릭터 움직임 및 총 발사
https://user-images.githubusercontent.com/63942174/161752860-2dc177d2-564c-492e-beea-37f48dda2b0d.mp4
    
https://user-images.githubusercontent.com/63942174/161752923-8e894d14-6a82-47d8-a16c-132ab0e4d717.mp4



    타이틀 화면입니다. 아무 키나 누른 후 START버튼을 누르면 게임이 시작되게 됩니다.
    맵씬과 플레이어씬을 분리해 놔서 UI를 적용하기 편하도록 만들었습니다.

<details>
    <summary>타이틀화면</summary>
  
``` C#
    
    
```
    
 </details>
