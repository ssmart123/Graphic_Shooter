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
    public int m_BestScore = 0;

}
public class LobbyMgr : MonoBehaviour
{
    public Image Fade_Panel = null;

    public GameObject RankScrollContent = null;
    public GameObject RankNodePrefab = null;

    public Text Txt_MyRankInfo = null;
    public GameObject HowToPlayPanel = null;
    public Button Btn_ResetRank = null;
    public Button Btn_Start = null;
    public Button Btn_HowToPlay = null;
    public Button Btn_LogOut = null;
    public Button Btn_Exit = null;


    private string Url_GetRanking = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_GetRanking.php";

    // 버튼 클릭 확인 변수
    private bool isStartClick = false;
    private bool isHowToClick = false;
    private bool isLogOutClick = false;
    private bool isExitClick = false;

    // Fade 관련 변수
    private bool isFadeIn = true;
    private float m_CurrentFadeTime = 0;
    private float m_FadeTime = 0;
    private float m_DuringFadeTime = 0.3f;

    [SerializeField] private float m_LeaderboardResetDelay = 5.0f;
    private float m_RefreshTime = 5.0f;

    List<UserInfo> m_UserList = new List<UserInfo>();
    GameObject[] a_RankNode = null;

    bool islock = false;
    private int m_MyRank = 0;

    void Start()
    {
        // 타임스케일 조정
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        // Fade변수 초기화
        Fade_Panel.gameObject.SetActive(true);
        m_CurrentFadeTime = 0;
        m_FadeTime = 0;
        isFadeIn = true;

        if (Btn_ResetRank != null)
            Btn_ResetRank.onClick.AddListener(SelfResetRank);

        if (Btn_Start != null)
            Btn_Start.onClick.AddListener(() => { isStartClick = true; });
        if (Btn_HowToPlay != null)
            Btn_HowToPlay.onClick.AddListener(() => { isHowToClick = !isHowToClick; });
        if (Btn_LogOut != null)
            Btn_LogOut.onClick.AddListener(() => { isLogOutClick = true; });
        if (Btn_Exit != null)
            Btn_Exit.onClick.AddListener(() => { isExitClick = true; });

        GetLeaderbord();
        RefreshMyInfo();

    }

    // Update is called once per frame
    void Update()
    {
        if (isFadeIn == true)
            FadeIn();

        if (isStartClick == true || isLogOutClick == true || isExitClick == true)
            FadeOut();

        if (m_RefreshTime > 0)
        {
            m_RefreshTime = m_RefreshTime - Time.deltaTime;

            if (m_RefreshTime <= 0.0f)
            {
                m_RefreshTime = m_LeaderboardResetDelay;
                GetLeaderbord();
            }
        }

        

        if (Input.GetKeyDown(KeyCode.Escape))
            isHowToClick = false;

        HowToPlayPanel.SetActive(isHowToClick);
    }

    private void FadeIn()
    {
        if (m_CurrentFadeTime <= 1.0f)
        {
            m_FadeTime = m_FadeTime + Time.deltaTime;
            m_CurrentFadeTime = m_FadeTime / m_DuringFadeTime;
            Fade_Panel.color = new Color(0, 0, 0, 1 - m_CurrentFadeTime);

            // FadeIn 비활성화
            if (1.0f < m_CurrentFadeTime)
            {
                m_FadeTime = 0;
                m_CurrentFadeTime = 0;
                isFadeIn = false;
                Fade_Panel.gameObject.SetActive(false);
            }
        }

    }

    private void FadeOut()
    {
        Fade_Panel.gameObject.SetActive(true);

        if (m_CurrentFadeTime <= 1.0f)
        {
            m_FadeTime = m_FadeTime + Time.deltaTime;
            m_CurrentFadeTime = m_FadeTime / m_DuringFadeTime;
            Fade_Panel.color = new Color(0, 0, 0, m_CurrentFadeTime);

            if (1.0f < m_CurrentFadeTime)
                ChangeScene();
        }
    }

    private void ChangeScene()
    {
        StopAllCoroutines();

        if (isStartClick == true)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("scPlay_Map");
            UnityEngine.SceneManagement.SceneManager.LoadScene("scPlay_Unit", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        if (isLogOutClick == true)
        {
            ResetGlobalValue();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }
        if (isExitClick == true)
        {
            ResetGlobalValue();
#if UNITY_EDITOR  // 유니티 에디터일 경우
            UnityEditor.EditorApplication.isPlaying = false;
#else             // 유니티 에디터가 아닐경우
                Application.Quit(); 
#endif
        }

    }

    private void ResetGlobalValue()
    {
        GlobalValue.g_Unique_ID = "";
        GlobalValue.g_UserNick = "";
        GlobalValue.g_BestScore = 0;
    }

    private void SelfResetRank()
    {
        m_RefreshTime = 0;
        GetLeaderbord();
        m_RefreshTime = m_LeaderboardResetDelay;
    }

    private void GetLeaderbord()
    {
        StartCoroutine(RefreshRankingCo());
    }
    IEnumerator RefreshRankingCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(Url_GetRanking, form);
        yield return a_www.SendWebRequest();



        if (a_www.error == null) //에러가 나지 않았을 때 동작
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //<--이렇게 해야 안드로이드에서 한글이 않깨진다.
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get Ranking List Success~") == true)
            {
                //점수를 표시하는 함수를 호출
                GetRanking(a_ReStr);
            }
        }
        else
        {
            Debug.Log(a_www.error);
        }
    }

    private void GetRanking(string a_ReStr)
    {
        if (a_ReStr.Contains("RkList") == false)
            return;
        m_UserList.Clear();

        //JSON 파일 파싱
        var N = JSON.Parse(a_ReStr);

        int ranking = 0;
        UserInfo a_UserNd;

        // 유저 리스트 채움
        for (int i = 0; i < N["RkList"].Count; i++)
        {
            ranking = i + 1;
            string userID = N["RkList"][i]["user_id"];
            string user_nick = N["RkList"][i]["user_nick"];
            int best_score = N["RkList"][i]["best_score"].AsInt;


            a_UserNd = new UserInfo();
            a_UserNd.m_ID = userID;
            a_UserNd.m_Nick = user_nick;
            a_UserNd.m_BestScore = best_score;
            m_UserList.Add(a_UserNd);

        }//for (int i = 0; i < N["RkList"].Count; i++)


        if (RankNodePrefab == null)
            return;

        string a_RankingNodeText = "";

        a_RankNode = new GameObject[m_UserList.Count];

        for (int i = 0; i < m_UserList.Count; i++)
        {
            if(islock == false)
            a_RankNode[i] = (GameObject)Instantiate(RankNodePrefab, RankScrollContent.transform, false);


            a_RankingNodeText = "";

            if (m_UserList[i].m_ID == GlobalValue.g_Unique_ID)
            {
                GlobalValue.g_BestScore = m_UserList[i].m_BestScore;
                a_RankingNodeText += "<color=#008000>";
            }
            else
                a_RankingNodeText += "<color=#000000>";

            a_RankingNodeText += m_UserList[i].m_Nick + "\t\t" + (i + 1).ToString() + "등 \n" + "BestScore : " + m_UserList[i].m_BestScore + "</color>";

            RankScrollContent.transform.GetChild(i).GetComponentInChildren<Text>().text = a_RankingNodeText;

            if (i == m_UserList.Count - 1)
                islock = true;
        }

        if (N["my_rank"] != null)
            m_MyRank = N["my_rank"].AsInt;
        RefreshMyInfo();
    }

    private void RefreshMyInfo()
    {
        Txt_MyRankInfo.text = GlobalValue.g_UserNick + "\t\t" + m_MyRank + "등\n"
                              + "BestScore : " + GlobalValue.g_BestScore;
    }

}
