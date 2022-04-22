using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SimpleJSON;
using UnityEngine.Networking;


public class TitleMgr : MonoBehaviour
{
    // 컴포넌트 연결

    [Header("오브젝트 UI")]
    public GameObject Title_Root;               // 타이틀 UI
    public GameObject LogIn_Root;               // 로그인 UI
    public GameObject LogIn_Panel;              // 로그인 패널
    public GameObject CreateAccount_Panel;      // 계정 생성 패널
    public Image Fade_Panel;               // 페이드 패널

    [Header("로그인 입력 UI")]
    public InputField Input_LoginID;            // 로그인 ID 입력창
    public InputField Input_LoginPW;            // 로그인 PW 입력창

    [Header("계정생성 입력 UI")]
    public InputField Input_CreateID;           // ID생성 입력창
    public InputField Input_CreatePW;           // PW생성 입력창
    public InputField Input_CreateNick;         // 닉네임 생성 입력창

    [Header("버튼 UI")]
    public Button Btn_Login;                    // 로그인 버튼
    public Button Btn_AccountPanel;             // 계정 패널 버튼
    public Button Btn_CreateAccount;            // 계정 생성 버튼
    public Button Btn_Back;                     // 로그인 패널로 돌아가기 버튼
    public Button Btn_Exit;                     // 게임 종료 버튼

    // GUI 메시지 
    private string GUI_Message;                         

    // MYSQL 도메인에 있는 PHP파일 주소
    private string Url_LogIn = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_LogIn.php";
    private string Url_CreateAccount = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_CreateAccount.php";

    [Header("로그인 텍스트 입력 제한")]
    public int m_IDCountMin = 3; 
    public int m_IDCountMax = 15;
    public int m_NickCountMin = 3;
    public int m_NickCountMax = 20;
    public int m_PWCountMin = 6;
    public int m_PWCountMax = 15;

    // 상태 변수
    private bool isAnyKey = false;
    private bool isStartFadeOut = false;

    // Fade 관련 변수
    private float m_CurrentFadeTime = 0;
    private float m_AddTime = 0;
    private float m_DuringFadeTime = 0.3f;


    void Start()
    {
        // 타임스케일 조정
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        // 버튼 연결
        if (Btn_Login != null)                  // 로그인 버튼
            Btn_Login.onClick.AddListener(LoginBtnClick);
        if (Btn_AccountPanel != null)           // 계정 버튼
            Btn_AccountPanel.onClick.AddListener(AccountBtnClick);

        if (Btn_CreateAccount != null)          // 계정생성버튼
            Btn_CreateAccount.onClick.AddListener(CreateNewAccount);

        if (Btn_Back != null)                   // 로그인 창으로 돌아가기 버튼
            Btn_Back.onClick.AddListener(BacktoLogin);

        if (Btn_Exit != null)                   // 게임 종료버튼
            Btn_Exit.onClick.AddListener(()=> {
#if  UNITY_EDITOR                               // 유니티 에디터일 경우
                UnityEditor.EditorApplication.isPlaying = false;
#else                                           // 유니티 에디터가 아닐경우
                Application.Quit(); 
#endif
            });

    }

    private void Update()
    {
        // 아무 버튼이나 눌러 로그인 화면으로 전환
        if (Input.anyKeyDown && isAnyKey == false)               
        {
            isAnyKey = true;
            Title_Root.SetActive(false);
            LogIn_Root.SetActive(true);
        }

        // 페이드 아웃
        if (isStartFadeOut == true)
            FadeOut();

    }
    // 로그인 버튼 클릭시
    void LoginBtnClick()
    {
        string a_IdStr = Input_LoginID.text;
        string a_PwStr = Input_LoginPW.text;

        // 로그인 제약조건
        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
        {
            GUI_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }
        if (!(m_IDCountMin <= a_IdStr.Length && a_IdStr.Length < m_IDCountMax))  
        {
            GUI_Message = string.Format("ID는 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_IDCountMin, m_IDCountMax);
            return;
        }
        if (!(m_PWCountMin <= a_PwStr.Length && a_PwStr.Length < m_PWCountMax))  
        {
            GUI_Message = string.Format("비밀번호는 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_PWCountMin, m_PWCountMax);
            return;
        }

        StartCoroutine(LoginCo(a_IdStr, a_PwStr));
    }
    #region 로그인 코루틴

    IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        // 입력한 ID와 PW를 데이터베이스와 비교하기
        // POST
        WWWForm form = new WWWForm();
        form.AddField("Input_user", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pass", a_PwStr);

        UnityWebRequest a_www = UnityWebRequest.Post(Url_LogIn, form);

        yield return a_www.SendWebRequest(); //응답이 올때까지 대기하기

        if (a_www.error == null) // 연결 성공시
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            GlobalValue.g_Unique_ID = a_IdStr;  // 글로벌 데이터에 아이디 저장

            // PHP파일 내의 문자열과 비교하기
            if (sz.Contains("Login-Success!!") == false)
                yield break;
            if (sz.Contains("user_nick") == false)
                yield break;

            var N = JSON.Parse(sz);     // 데이터베이스의 플레이어 데이터를 JSON형식으로 받아오기

            if (N == null)
                yield break;

            // 유저 정보를 글로벌 변수에 저장
            if (N["user_nick"] != null)
                GlobalValue.g_UserNick = N["user_nick"];
            if (N["best_score"] != null)
                GlobalValue.g_BestScore = N["best_score"].AsInt;

            // 페이드 아웃 활성화
            isStartFadeOut = true;
            Fade_Panel.gameObject.SetActive(true);

            GUI_Message = "로그인 성공";
            yield break;
        }
        else  // 연결 실패시
        {
            // Debug.Log(a_www.error);
            GUI_Message = "연결에 실패하였습니다.";
        }
    }
    #endregion

    private void FadeOut()
    {
        if (m_CurrentFadeTime <= 1.0f)
        {
            m_AddTime = m_AddTime + Time.deltaTime;
            m_CurrentFadeTime = m_AddTime / m_DuringFadeTime;

            Fade_Panel.color = new Color(0, 0, 0, m_CurrentFadeTime);

            if (1.0f < m_CurrentFadeTime)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
        }
    }

    // 계정생성 패널로 가기
    void AccountBtnClick()
    {
        if (LogIn_Panel != null)
            LogIn_Panel.SetActive(false);

        if (CreateAccount_Panel != null)
            CreateAccount_Panel.SetActive(true);
    }
    // 로그인 패널로 돌아가기
    void BacktoLogin()
    {
        if (LogIn_Panel != null)
            LogIn_Panel.SetActive(true);

        if (CreateAccount_Panel != null)
            CreateAccount_Panel.SetActive(false);
    }
    // 새로운 계정 생성
    void CreateNewAccount()
    {
        string a_IDString = Input_CreateID.text;
        string a_NickString = Input_CreateNick.text;
        string a_PWString = Input_CreatePW.text;

        // 계정생성 제약 조건
        if (a_IDString.Trim() == "" ||
            a_NickString.Trim() == "" ||
            a_PWString.Trim() == "")
        {
            GUI_Message = "ID, PW, 별명 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(m_IDCountMin <= a_IDString.Length && a_IDString.Length < m_IDCountMax))
        {
            GUI_Message = string.Format("ID는 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_IDCountMin, m_IDCountMax);
            return;
        }

        if (!(m_PWCountMin <= a_PWString.Length && a_PWString.Length < m_PWCountMax))
        {
            GUI_Message = string.Format("비밀번호는 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_PWCountMin, m_PWCountMax);
            return;
        }

        if (!(m_NickCountMin <= a_NickString.Length && a_NickString.Length < m_NickCountMax))
        {
            GUI_Message = string.Format("별명은 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_NickCountMin, m_NickCountMax);
            return;
        }


        StartCoroutine(CreateAccountCo(a_IDString, a_NickString, a_PWString));
    }


    #region 계정생성 코루틴
    IEnumerator CreateAccountCo(string a_IDString, string a_NickString, string a_PWString)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_user", a_IDString, System.Text.Encoding.UTF8);
        form.AddField("Input_nick", a_NickString, System.Text.Encoding.UTF8);
        form.AddField("Input_pass", a_PWString);

        UnityWebRequest a_www = UnityWebRequest.Post(Url_CreateAccount, form);

        // 응답 대기
        yield return a_www.SendWebRequest();

        if (a_www.error == null) // 연결 성공시
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);
            GUI_Message = sz;

            if (sz.Contains("Create Success.") == true)
            {
                GUI_Message = "계정생성 성공.";
            }
            else  // 중복 확인
            {
                if (sz.Contains("ID does exist.") == true)
                {
                    GUI_Message = "중복된 ID가 존재합니다.";
                }
                if (sz.Contains("Nickname does exist.") == true)
                {
                    GUI_Message = "중복된 별명이 존재합니다.";
                }
            }

            BacktoLogin();

            yield break;
        }
        else  // 연결 실패시
        {
            // Debug.Log(a_www.error);
            GUI_Message = "연결에 실패했습니다.";
        }
    }

    #endregion


    //GUI텍스트 표시
    private void OnGUI()
    {
        if (GUI_Message != "")
        {
            GUI.Label(new Rect(20, 15, 1500, 1000),
                "<color=White><size=28>" + GUI_Message + "</size></color>");
        }
    }

}
