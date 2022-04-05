using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCtrl : MonoBehaviour
{
    public Transform m_Player = null;  // 따라다닐 플레이어

    [Space(10)]
    public Slider SliderH;
    public Slider SliderV;
    public Text ValueH;
    public Text ValueV;

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
