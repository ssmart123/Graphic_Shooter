using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public enum GameState
{
    GameIng,
    GameEnd
}
namespace SSM
{
    public class GameMgr : MonoBehaviour
    {
        public static GameState s_GameState = GameState.GameIng;

        // UI관련
        public Image Img_HPGage = null;
        public Text textScore = null;

        public Text Text_Menu = null;
        public GameObject PausePanel = null;
        public Button Btn_Back = null;

        public GameObject ConfigPanel = null;
        public InputField IF_HSensitive = null;
        public InputField IF_VSensitive = null;

        public GameObject EndPanel = null;
        public Text Text_BestScore = null;
        public Text Text_CurrentScore = null;

        public Button Btn_BackToTitle = null;
        public Button Btn_BackToLobby = null;

        // 포스트프로세싱 관련
        public PostProcessProfile profile = null;
        public Slider PostProcessSlider = null;

        public MonsterMgr monsterMgr = null;
        public CameraCtrl camCtrl = null;

        //누적 점수를 기록하기 위한 변수
        private int m_TotalScore = 0;

        // HP바 FillAmount 변수
        private float m_HPFillAmount = 1;
        public float HPFillP { set { m_HPFillAmount = value; } get { return m_HPFillAmount; } }

        // 상태변수
        private bool isPause = false;

        private string BestScoreURL;

        bool isNetUpdateLock = false;

        void Start()
        {
            DispScore(0);

            IF_HSensitive.onValueChanged.AddListener(IF_SenceH);
            IF_VSensitive.onValueChanged.AddListener(IF_SenceV);


            Btn_Back.onClick.AddListener(() =>
            {
                isPause = false;
                PausePanel.SetActive(false);
            });
            Btn_BackToTitle.onClick.AddListener(GoToTitle);
            Btn_BackToLobby.onClick.AddListener(GoToLobby);


            //--- 슬라이더 설정
            PostProcessSlider.maxValue = 50;
            PostProcessSlider.minValue = 0;
            PostProcessSlider.wholeNumbers = true;

            BestScoreURL = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_UpdateBestScore.php";


        }

        void Update()
        {
            ScoreUpdate();
            CursorOption();
            if (Input.GetKeyDown(KeyCode.Escape))
                isPause = !isPause;

            PauseUpdate();

            Img_HPGage.fillAmount = HPFillP;

            PostPorseccOption();

        }

        private void PauseUpdate()
        {
            PausePanel.SetActive(isPause);

            if (isPause == true)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
        private void IF_SenceH(string a_Value)
        {
            IF_HSensitive.text = a_Value;
            float a_sensH = float.Parse(a_Value);
            camCtrl.HSensP = a_sensH;
        }

        private void IF_SenceV(string a_Value)
        {
            IF_VSensitive.text = a_Value;
            float a_sensV = float.Parse(a_Value);
            camCtrl.VSensP = a_sensV;
        }

        private void ScoreUpdate()
        {

            if (m_TotalScore >= 9999)
            {
                m_TotalScore = 9999; 
                GlobalValue.g_BestScore = m_TotalScore;
                EndTextUpdate("승리");
            }

            if (GlobalValue.g_BestScore < m_TotalScore)
            {
                GlobalValue.g_BestScore = m_TotalScore;
                StartCoroutine(UpdateBestScoreCo());
            }

            Text_BestScore.text = "BestScore : <color=red>" + GlobalValue.g_BestScore.ToString("D4") + "</color>";
            Text_CurrentScore.text = "Score : <color=green>" + m_TotalScore.ToString("D4") + "</color>";
        }

        public void EndTextUpdate(string a_menutext)
        {
            isPause = true;
            PausePanel.SetActive(isPause);
            ConfigPanel.SetActive(false);
            EndPanel.SetActive(true);
            Text_Menu.text = a_menutext;
        }

        IEnumerator UpdateBestScoreCo()
        {

            WWWForm form = new WWWForm();

            form.AddField("input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
            form.AddField("input_score", GlobalValue.g_BestScore);

            isNetUpdateLock = true;

            if (isNetUpdateLock == true)
            {
                UnityWebRequest a_www = UnityWebRequest.Post(BestScoreURL, form);
                yield return a_www.SendWebRequest();
            }
            isNetUpdateLock = false;
        }

        private void CursorOption()
        {
            if (isPause == true)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = isPause;
        }


        private void PostPorseccOption()
        {
            PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();


            if (monsterMgr.IsSkillUseP == true)
            {
                volume.enabled = monsterMgr.IsSkillUseP;
            }
            else
            {
                volume.enabled = monsterMgr.IsSkillUseP;

                volume.profile.GetSetting<Bloom>().intensity.value = PostProcessSlider.value;
            }
            

        }

        // 점수 누적 및 화면 표시
        public void DispScore(int score)
        {
            m_TotalScore += score;
            textScore.text = "SCORE <color=#ff0000>" + m_TotalScore.ToString("D4") + "</color>";
        }

        private void GoToTitle()
        {
            Time.timeScale = 1;
            GlobalValue.g_Unique_ID = "";
            GlobalValue.g_UserNick = "";
            GlobalValue.g_BestScore = 0;
            s_GameState = GameState.GameIng;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }

        private void GoToLobby()
        {
            Time.timeScale = 1;
            s_GameState = GameState.GameIng;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
       
    }
}