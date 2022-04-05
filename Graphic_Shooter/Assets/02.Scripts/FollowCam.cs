using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideWall
{
    public bool m_IsColl = false;
    public GameObject m_SideWalls = null;
    public Material m_WallMaterial = null;

    public SideWall()
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    public Transform targetTr;  //추적할 타깃 게임오브젝트의 Transform 변수
    public float dist = 10.0f;  //카메라와의 일정 거리
    public float height = 3.0f; //카메라의 높이 설정
    public float dampTrace = 20.0f; //부드러운 추적을 위한 변수                                
    private Transform tr;  //접근해야 하는 컴포넌트는 반드시 변수에 할당한 후 사용

    Vector3 a_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //---------- Side Wall 리스트 관련 변수
    Vector3 a_CacVLen = Vector3.zero;
    Vector3 a_CacDirVec = Vector3.zero;

    GameObject[] a_SideWalls = null;
    [HideInInspector] public LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //---------- Side Wall 리스트 관련 변수

    private void Awake()
    {
        //Application.targetFrameRate = 60;

        dist = 4.0f;
        height = 2.8f;

//#if UNITY_EDITOR
//        Cursor.lockState = CursorLockMode.Confined; // 게임 창 밖으로 마우스가 안나감
//                                                    //Esc키를 누르면 커서가 창 밖으로 나가게 할 수 있다.

//#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        //스크립스 처음에 Transform 컴포넌트 할당
        tr = GetComponent<Transform>();


        //----------Side Wall 리스트에 만들기...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");    
        //"SideWall"번 레이어만 피킹하려고 할 경우 

        Material material;
        a_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        if (0 < a_SideWalls.Length)
        {
            SideWall a_SdWall = null;
            for (int a_ii = 0; a_ii < a_SideWalls.Length; a_ii++)
            {
                a_SdWall = new SideWall();
                a_SdWall.m_IsColl = false;
                a_SdWall.m_SideWalls = a_SideWalls[a_ii];
                a_SdWall.m_WallMaterial =
                    a_SideWalls[a_ii].GetComponent<Renderer>().material;
                material = a_SdWall.m_WallMaterial;

                material.SetFloat("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.color = new Color(1, 1, 1, 1);

                m_SW_List.Add(a_SdWall);
            }
        }// if (0 < a_SideWalls.Length)
        //----------Side Wall 리스트에 만들기...
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        {
            height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));
            if (height < 0.1f)
                height = 0.1f;

            if (5.7f < height)
                height = 5.7f;
        }
    }

    //void LateUpdate()
    void FixedUpdate()
    {
        a_PlayerVec = targetTr.position;
        a_PlayerVec.y = a_PlayerVec.y + 1.2f;

        //카메라의 위치를 추적대상의 dist 변수만큼 뒤쪽으로 배치하고
        //hright 변수만큼 위로 올림
        tr.position = Vector3.Lerp(tr.position,
                                 targetTr.position 
                                 - (targetTr.forward * dist)
                                 + (Vector3.up * height)
                                 , Time.deltaTime * dampTrace);

        //카메라가 타깃 게임오브젝트를 바라보게 설정
        tr.LookAt(a_PlayerVec);

        //-------------- Wall 카메라 충돌 처리 부분 
        a_CacVLen = this.transform.position - targetTr.position;
        a_CacDirVec = a_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_hitInfo;
        if (Physics.Raycast(targetTr.position + (-a_CacDirVec * 1.0f),
            a_CacDirVec, out a_hitInfo, a_CacVLen.magnitude + 4.0f, m_WallLyMask.value))
        {
            a_FindObj = a_hitInfo.collider.gameObject;
        }

        Material material;
        for (int a_ii = 0; a_ii < m_SW_List.Count; a_ii++)
        {
            if (m_SW_List[a_ii].m_SideWalls == null)
                continue;

            if (m_SW_List[a_ii].m_SideWalls == a_FindObj)
            {
                if (m_SW_List[a_ii].m_IsColl == false)
                {
                    material = m_SW_List[a_ii].m_WallMaterial;

                    material.SetFloat("_Mode", 3);
                    material.SetInt("_SrcBlend",
                        (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend",
                        (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    material.color = new Color(1, 1, 1, 0.5f);

                    m_SW_List[a_ii].m_IsColl = true;
                }
            }//if(m_SW_List[a_ii].m_SideWalls == a_FindObj)
            else
            {
                if (m_SW_List[a_ii].m_IsColl == true)
                {
                    material = m_SW_List[a_ii].m_WallMaterial;

                    material.SetFloat("_Mode", 0);
                    material.SetInt("_SrcBlend",
                        (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend",
                        (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    material.color = new Color(1, 1, 1, 1);

                    m_SW_List[a_ii].m_IsColl = false;
                }
            }
        }//for (int a_ii = 0; a_ii < m_SW_List.Count; a_ii++)
        //-------------- Wall 카메라 충돌 처리 부분 
    }
}
