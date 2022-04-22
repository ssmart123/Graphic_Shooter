using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCtrl : MonoBehaviour
{
    public SSM.PlayerCtrl m_PlayerCtrl;
    public Transform m_Player = null;  // 따라다닐 플레이어

    [Header("Field of View")]
    public int FOVMin = 70;
    public int FOVMax = 120;
    public int FOV = 90;

    RaycastHit hit;
    //------ 감도
    [Space(10)]
    public float m_SensitiveMin = 0.01f, m_SensitiveMax = 50f;
    private float m_SensitiveCurH = 10.0f;
    private float m_SensitiveCurV = 10.0f;
    public float HSensP { get { return m_SensitiveCurH; } set { m_SensitiveCurH = value; } }
    public float VSensP { get { return m_SensitiveCurV; } set { m_SensitiveCurV = value; } }
    // 카메라 회전 보정 속도
    float m_RotSpeed = 10.0f;

    // 카메라를 조정하기 위한 방향벡터
    private Vector3 m_AimPivot = Vector3.zero;  // 회전축

    // 마우스 입력값
    private float m_InputH = 0;
    private float m_InputV = 0;

    // 마우스 회전값
    private float m_RotH = 0.0f;
    private float m_RotV = 0.0f;


    // 카메라 위치 계산용 변수
    [Header("카메라 위치 변수")]
    public float m_hight = 1.8f;  // 에임 높이

    public float m_Dist_Aim = 0.23f;  // 에임과 캐릭터의 거리
    public float m_Dist_Cam = 1.8f;   // 캠과 에임과의 거리
    float m_CurDist = 1.8f;   // 캠과 에임과의 거리


    //위 아래 각도 제한
    [SerializeField] private float vMinLimit = -80.0f; 
    [SerializeField] private float vMaxLimit = 80.0f;

    void Start()
    {
        m_CurDist = m_Dist_Cam;
        HorizontalRot(m_Player.position, m_hight);

        VerticalRot(m_AimPivot);
    }
    private void FixedUpdate()
    {
        CheckCameraDistance();
    }
    private void CheckCameraDistance()
    {
        Vector3 a_RayDir = (transform.position - m_AimPivot).normalized;

        if (Physics.Raycast(m_AimPivot, a_RayDir, out hit, m_Dist_Cam + 0.2f))
        {
            m_CurDist = hit.distance;

            if (m_CurDist <= 0.5f)
                m_CurDist = 0.5f;
        }
        else
            m_CurDist = m_Dist_Cam;

       
    }
    private void Update()
    {
        MouseInput();
        ClampRotation();
        ChangeStateValue();


    }

    private void MouseInput()
    {
        m_InputH = Input.GetAxis("Mouse X");
        m_InputV = Input.GetAxis("Mouse Y");
    }
    private void ClampRotation()
    { 

        // 카메라 각도 계산
        m_RotH += m_InputH * m_SensitiveCurH * m_RotSpeed * Time.deltaTime;
        m_RotV -= m_InputV * m_SensitiveCurV * m_RotSpeed * Time.deltaTime;

        // 수평 회전 범위 제한
        if (m_RotH < -360)
            m_RotH += 360;
        if (m_RotH > 360)
            m_RotH -= 360;

        // 수직 회전 범위 제한
        m_RotV = Mathf.Clamp(m_RotV, vMinLimit, vMaxLimit);
    }

    private void ChangeStateValue()
    {
        if (m_PlayerCtrl.IsAimP == true)
        {
            m_hight = 1.8f;
            m_Dist_Aim = 0.5f;
            m_Dist_Cam = 0.9f;

        }
        else
        {
            m_hight = 1.8f;
            m_Dist_Aim = 0.23f;
            m_Dist_Cam = 1.8f;
        }
    }

    void LateUpdate()
    {
        HorizontalRot(m_Player.position, m_hight);
        VerticalRot(m_AimPivot);
        transform.LookAt(m_AimPivot);
    }

    // 에임피봇 위치 계산
    Vector3 HorizontalRot(Vector3 a_PlayerPos, float a_hight)
    {

        m_AimPivot = a_PlayerPos;
        m_AimPivot.y = m_AimPivot.y + a_hight;

        // 캐릭터와 AimPivot의 방향과 거리 계산
        Vector3 a_CalcAimPivot = Vector3.right * m_Dist_Aim;

        // a_CalcAimPos가 회전할 각도
        Quaternion a_CalcPivotRotH = Quaternion.Euler(0, m_RotH, 0);

        // AimPivot의 위치 반환 
        return m_AimPivot = (a_CalcPivotRotH * a_CalcAimPivot) + m_AimPivot;
    }

    // 카메라 위치 계산
    Vector3 VerticalRot(Vector3 a_AimPos)
    {
        // AimPivot과 카메라의 방향과 거리 계산
        Vector3 a_CalcCamPos = Vector3.back * m_CurDist;
        // 카메라가 회전할 각도
        Quaternion a_CalcCamRot = Quaternion.Euler(m_RotV, m_RotH, 0);

        // Camera의 위치 반환
        return transform.position = (a_CalcCamRot * a_CalcCamPos) + a_AimPos;
    }

}
