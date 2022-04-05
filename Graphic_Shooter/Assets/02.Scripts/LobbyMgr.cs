using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SimpleJSON;
using UnityEngine.Networking;

public class UserInfo
{
    public string m_ID = "";
    public string m_Nick = "";
}

public class LobbyMgr : MonoBehaviour
{
    [Header("오브젝트")]
    public GameObject m_Title_Root;
    public GameObject m_LogIn_Root;
    public GameObject m_LogIn_Panel;
    public GameObject m_CreateAccount_Panel;
    public GameObject m_StartMenu;

    [Header("텍스트")]
    public Text m_Title_PressKey;
    string GUI_Message = "";

    [Header("입력")]
    public InputField m_LoginID_Input;
    public InputField m_LoginPW_Input;
    public InputField m_CreateID_Input;
    public InputField m_CreateNick_Input;
    public InputField m_CreatePW_Input;


    [Header("버튼")]
    public Button m_Login_Btn;
    public Button m_AccountPanel_Btn;
    public Button m_CreateAccount_Btn;
    public Button m_Back_Btn;
    public Button m_Start_Btn;

    public Button m_Exit_Btn;

    private bool isKeyClick = false;
    private bool isChange = false;


    //string LogInUrl = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_LogIn.php";
    //string CreateAccountUrl = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_CreateAccount.php";



    void Start()
    {
        m_Title_Root.SetActive(true);
        isKeyClick = false;

        m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, 1);

        StartCoroutine(BlinkTextAlpha());

        //if (m_Login_Btn != null)
        //    m_Login_Btn.onClick.AddListener(LoginFunc);
        //if (m_AccountPanel_Btn != null)
        //    m_AccountPanel_Btn.onClick.AddListener(OpenCreateAccount);

        //if (m_CreateAccount_Btn != null)
        //    m_CreateAccount_Btn.onClick.AddListener(CreateNewAccount);
        //if (m_Back_Btn != null)
        //    m_Back_Btn.onClick.AddListener(BacktoLogin);

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);
        if (m_Exit_Btn != null)
            m_Exit_Btn.onClick.AddListener(()=> {
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

    void OpenCreateAccount()
    {
        if (m_LogIn_Panel != null)
            m_LogIn_Panel.SetActive(false);

        if (m_CreateAccount_Panel != null)
            m_CreateAccount_Panel.SetActive(true);
    }

    void BacktoLogin()
    {
        if (m_LogIn_Panel != null)
            m_LogIn_Panel.SetActive(true);

        if (m_CreateAccount_Panel != null)
            m_CreateAccount_Panel.SetActive(false);
    }

    void CreateNewAccount()
    {
        string a_IDString = m_CreateID_Input.text;
        string a_NickString = m_CreateNick_Input.text;
        string a_PWString = m_CreatePW_Input.text;

        if (a_IDString.Trim() == "" ||
            a_NickString.Trim() == "" ||
            a_PWString.Trim() == "")
        {
            GUI_Message = "ID, PW, 별명 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IDString.Length && a_IDString.Length < 15))
        {
            GUI_Message = "ID는 3글자 이상 15글자 이하로 작성해 주세요.";
            return;
        }

        if (!(3 <= a_PWString.Length && a_PWString.Length < 15))
        {
            GUI_Message = "비밀번호는 3글자 이상 15글자 이하로 작성해 주세요.";
            return;
        }

        if (!(2 <= a_NickString.Length && a_NickString.Length < 15))
        {
            GUI_Message = "별명은 2글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }


       // StartCoroutine(CreateAccountCo(a_IDString, a_NickString, a_PWString));
    }

     void LoginFunc()
    {
        string a_IdStr = m_LoginID_Input.text;
        string a_PwStr = m_LoginPW_Input.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
        {
            GUI_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 15))  //3~15
        {
            GUI_Message = "ID는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }
        if (!(3 <= a_PwStr.Length && a_PwStr.Length < 15))  //6~15
        {
            GUI_Message = "비밀번호는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }

       // StartCoroutine(LoginCo(a_IdStr, a_PwStr));

    }//public void LoginBtn()
    void StartBtnClick()
    {
        GameMgr.s_GameState = GameState.GameIng;
        UnityEngine.SceneManagement.SceneManager.LoadScene("scPlay_Map 1");
        UnityEngine.SceneManagement.SceneManager.LoadScene("scPlay_Unit", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }


    private void OnGUI()
    {
        if (GUI_Message != "")
        {
            GUI.Label(new Rect(20, 15, 1500, 1000),
                "<color=Black><size=28>" + GUI_Message + "</size></color>");
        }
    }

#region 로그인 코루틴
/*
IEnumerator LoginCo(string a_IdStr, string a_PwStr)
{
    WWWForm form = new WWWForm();
    form.AddField("Input_user", a_IdStr, System.Text.Encoding.UTF8);
    form.AddField("Input_pass", a_PwStr);

    UnityWebRequest a_www = UnityWebRequest.Post(LogInUrl, form);
    yield return a_www.SendWebRequest(); //응답이 올때까지 대기하기...

    if (a_www.error == null) //에러가 나지 않았을 때 동작
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        string sz = enc.GetString(a_www.downloadHandler.data);
        GUI_Message = sz;

        GlobalValue.g_Unique_ID = a_IdStr; 

        if (sz.Contains("Login-Success!!") == false)
            yield break;
        if (sz.Contains("user_nick") == false)
            yield break;

        var N = JSON.Parse(sz);

        if (N == null)
            yield break;

        if (N["user_nick"] != null)
            GlobalValue.g_UserNick = N["user_nick"];
        if (N["best_score"] != null)
            GlobalValue.g_BestScore = N["best_score"].AsInt;

        m_LogIn_Root.SetActive(false);
        m_StartMenu.SetActive(true);
    }
    else
    {
        Debug.Log(a_www.error);
    }
}
#endregion

#region 계정생성 코루틴
IEnumerator CreateAccountCo(string a_IDString, string a_NickString, string a_PWString)
{
    WWWForm form = new WWWForm();
    form.AddField("Input_user", a_IDString, System.Text.Encoding.UTF8);
    form.AddField("Input_nick", a_NickString, System.Text.Encoding.UTF8);
    form.AddField("Input_pass", a_PWString);

    UnityWebRequest a_www = UnityWebRequest.Post(CreateAccountUrl, form);

    // 응답 대기
    yield return a_www.SendWebRequest(); 

    if (a_www.error == null) //성공시
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        string sz = enc.GetString(a_www.downloadHandler.data);
        GUI_Message = sz;

        if (sz.Contains("Create Success.") == true)
        {
            GUI_Message = "계정생성 성공.";
        }
        else
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
    }
    else  // 실패시
    {
        Debug.Log(a_www.error);
    }
}
*/
#endregion

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
