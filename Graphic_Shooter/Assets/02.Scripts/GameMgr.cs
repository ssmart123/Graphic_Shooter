using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;


    //몬스터가 출현할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab;
    //몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    //----- 몬스터 셰이더 변수
    public static Shader g_DefShader = null;
    public static Shader g_GrayscaleShader = null;
    public static Shader g_DeadShader = null;
    //----- 몬스터 셰이더 변수

    //----- 스킬 사용시 상태 변수
    public static bool g_Stone = false;  // 돌로 변하는 상태
    public bool m_isSkUse = false;

    float m_SkCoolTime = 5.0f;
    float m_SkCount = 0;


    //몬스터를 발생시킬 주기
    public float createTime = 1.5f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //게임 종료 여부 변수
    public bool isGameOver = false;


    //----- UI관련


    //Text UI 항목 연결을 위한 변수
    public Text textScore;
    //누적 점수를 기록하기 위한 변수
    private int totScore = 0;

    public GameObject SetupPanel;
    public Button BackBtn;
    public Button BackToLobby;
    public Image SkillCTImg = null;

    bool isConfig = false;

    // ----- 포스트프로세싱 관련
    public Slider PostProcessSlider = null;
    public Text PostValue = null;
    public PostProcessProfile profile = null;

    // Start is called before the first frame update
    void Start()
    {
        ////처음 실행 후 저장된 스코어 정보 로드
        //totScore = PlayerPrefs.GetInt("TOT_SCORE", 0);
        DispScore(0);

        BackBtn.onClick.AddListener(() => {
            isConfig = false;
            SetupPanel.SetActive(false);
        });
        BackToLobby.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        });
        // 처음 몬스터를 생성할 때 시간
        createTime = 1.0f;  
        //Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //----- 셰이더 찾기
        // 기본 셰이더
        g_DefShader = Shader.Find("Legacy Shaders/Bumped Specular");
        // 스톤 셰이더 - 회색
        g_GrayscaleShader = Shader.Find("Custom/Grayscale");
        // 사망시 세이더
        g_DeadShader = Shader.Find("Custom/AddTexColor");

        //--- 슬라이더 설정
        PostProcessSlider.maxValue = 50;
        PostProcessSlider.minValue = 0;
        PostProcessSlider.wholeNumbers = true;

        //몬스터를 생성해 오브젝트 풀에 저장
        for (int i = 0; i < maxMonster; i++)
        {
            //몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
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
        if (Input.GetKeyDown(KeyCode.G) == true && GameMgr.s_GameState == GameState.GameIng )
            m_isSkUse = true;
        if (m_isSkUse == true && isConfig == false)
            UseSkill();

        if (totScore >= 9999)
            totScore = 9999;

        if (Input.GetKeyDown(KeyCode.Escape))
            isConfig = !isConfig;


        SetupPanel.SetActive(isConfig);
        Cursor.visible = isConfig;
        if (isConfig == true)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
        }

        PostValue.text = PostProcessSlider.value.ToString();
    }

    void UseSkill()
    {
            PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();
            volume.enabled = true;

            volume.profile.GetSetting<Bloom>().intensity.value = PostProcessSlider.value;

            m_SkCount += Time.deltaTime;
            GameMgr.g_Stone = true;

            float a = m_SkCount / m_SkCoolTime;
            SkillCTImg.fillAmount = a;

            if (m_SkCount >= m_SkCoolTime)
            {
                m_SkCount = 0;
                SkillCTImg.fillAmount = 1;
                GameMgr.g_Stone = false;
                m_isSkUse = false;
                volume.enabled = false;
            }
        
    }



    //점수 누적 및 화면 표시
    public void DispScore(int score)
    {
        totScore += score;
        textScore.text = "SCORE <color=#ff0000>" + totScore.ToString("D4") + "</color>";

        ////스코어 저장
        //PlayerPrefs.SetInt("TOT_SCORE", totScore);
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
            if (GameMgr.s_GameState == GameState.GameEnd)
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
