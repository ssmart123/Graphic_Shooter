using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterMgr : MonoBehaviour
{

    //몬스터가 출현할 위치를 담을 배열
    //public Transform[] points = null;
    private Transform[] points = null;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab = null;
    private Transform monsterPoolGroup = null;
    //몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();


    //----- 몬스터 셰이더 변수
    public static Shader g_DefShader = null;
    public static Shader g_GrayscaleShader = null;
    public static Shader g_DeadShader = null;


    //----- 스킬 사용시 상태 변수

    public Image SkillCTImg = null;
  //  public static bool g_Stone = false;  // 돌로 변하는 상태
    private bool m_isSkUse = false;
    public bool IsSkillUseP { get { return m_isSkUse; } }

    float m_SkCoolTime = 5.0f;
    float m_SkCount = 0;

    public float SkCountP { get; }
    public float SkcCoolTimeP { get; }


    //몬스터를 발생시킬 주기
    public float createTime = 1.5f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //게임 종료 여부 변수
    public bool isGameOver = false;

    private void Awake()
    {
        //Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();


        // 셰이더 찾아오기
        g_DefShader = Shader.Find("Legacy Shaders/Bumped Specular");    // 기본 셰이더
        g_GrayscaleShader = Shader.Find("Custom/Grayscale");    // 스톤 셰이더 - 회색
        g_DeadShader = Shader.Find("Custom/AddTexColor");   // 사망시 세이더

        monsterPoolGroup = GameObject.Find("MonsterGroup").transform;

    }

    void Start()
    {

        // 처음 몬스터를 생성할 때 시간
        createTime = 1.0f;



        //몬스터를 생성해 오브젝트 풀에 저장
        for (int i = 0; i < maxMonster; i++)
        {
            //몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab, monsterPoolGroup);
            //생성한 몬스터의 이름 설정
            monster.name = "Monster_" + i.ToString();
            monster.SetActive(false);
            //생성한 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(monster);
            //  Debug.Log("몬스터풀 생성");
        }

        if (points.Length > 0)
        {
            //몬스터 생성 코루틴 함수 호출
            StartCoroutine(this.CreateMonster());
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.G) == true && SSM.GameMgr.s_GameState == GameState.GameIng)
            m_isSkUse = true;

        if (m_isSkUse == true )
            UseSkill();
    }

    void UseSkill()
    {

        m_SkCount += Time.deltaTime;
       // g_Stone = true;

        float a = m_SkCount / m_SkCoolTime;
        SkillCTImg.fillAmount = a;

        if (m_SkCount >= m_SkCoolTime)
        {
            m_SkCount = 0;
            SkillCTImg.fillAmount = 1;
          //  g_Stone = false;
            m_isSkUse = false;
        }

    }

    //몬스터 생성 코루틴 함수
    IEnumerator CreateMonster()
    {
        //게임 종료 시까지 무한 루프
        while (!isGameOver)
        {
            // 스킬을 사용하면 스킬 쿨타임만큼 몬스터를 생성하지 않음

            //몬스터 생성 주기 시간만큼 메인 루프에 양보
            yield return new WaitForSeconds(createTime);

            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (isGameOver)
                yield break;

            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (SSM.GameMgr.s_GameState == GameState.GameEnd)
                yield break;     //코루틴 함수에서 함수를 빠져나가는 명령 

            //오브젝트 풀의 처음부터 끝까지 순회
            foreach (GameObject monster in monsterPool)
            {
                //비활성화 여부로 사용 가능한 몬스터를 판단
                if (!monster.activeSelf)
                {
                    //몬스터를 출현시킬 위치의 인덱스값을 추출
                    int idx = Random.Range(1, points.Length);
                    //몬스터의 출현위치를 설정
                    monster.transform.position = points[idx].position;
                    //몬스터를 활성화함
                    monster.SetActive(true);
                    //오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
                    break;
                }//if (!monster.activeSelf)
            }//foreach (GameObject monster in monsterPool)
        }//while (!isGameOver)

    }//IEnumerator CreateMonster()

}
