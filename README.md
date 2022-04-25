![타이틀](https://user-images.githubusercontent.com/63942174/164639670-e76cb9b8-5c65-4526-88ea-b29708d36536.png)![공격](https://user-images.githubusercontent.com/63942174/164636382-cd35fef6-5289-4a51-aca5-6f9034341042.png)

# 🎮Graphic Shooter🎮  

외계 공장에서 몰려드는 적을 처치하며 처철한 사투속에서 살아남으세요!

## <제작 의도>    

    배그와 같이 부드러운 애니메이션을 가진 TPS를 구현하려고 했습니다. 무료 에셋을 사용하여 만들었고
    AnimationIK 기능을 이용하여 보다 자연스러운 움직임이 가능하도록 만들었습니다.

## <게임 방법>
- WASD를 이용하여 캐릭터를 움직입니다.
- 마우스를 사용하여 조준을 하고 좌클릭을 눌러 총을 발사합니다.
- 좌측 상단 체력바가 다 떨어지면 게임오버가 됩니다.
- 체력바 밑 하얀색 게이지가 가득찬 뒤 G를 누르면 스킬을 사용할 수가 있고 시전중에는 적들이 멈춤니다.
- 적을 처치하면 점수가 50점씩 오르게 됩니다.

## 1. 타이틀 화면
![타이틀-로그인](https://user-images.githubusercontent.com/63942174/164639473-b1f876f1-4723-43ff-b366-ee5bc124237a.png)![타이틀 계정생성](https://user-images.githubusercontent.com/63942174/164639574-31527fe3-3dd3-4fea-bfcd-bd790ad022b2.png)


    타이틀 화면입니다. 아이디를 입력하여 게임에 접속할 수 있습니다. 
    계정이 없다면 계정생성 버튼을 누르고 아이디와 닉네임, 비밀번호를 입력하면 데이터베이스에서 
    중복되는 아이디나 닉네임이 없는지 확인하고 새로운 아이디를 생성합니다.
    

<details>
    <summary>타이틀화면-웹데이터베이스 정보 받기(C#)</summary>
  
``` C#
using SimpleJSON;
using UnityEngine.Networking;
    
public class TitleMgr : MonoBehaviour
{
    `````````````````
    // MSQL 도메인에 있는 PHP파일 주소
    private string Url_LogIn = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_LogIn.php";
    private string Url_CreateAccount = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_CreateAccount.php";
    ``````````````
    
     // 로그인 버튼 클릭시
    void LoginBtnClick()
    {
        // 로그인 인풋필드의 text받아오기
        string a_IdStr = Input_LoginID.text;
        string a_PwStr = Input_LoginPW.text;

        // 로그인 제약조건
        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "") // 아이디, 비밀번호에 빈칸이 있는지 체크
        {   
            GUI_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }
        if (!(m_IDCountMin <= a_IdStr.Length && a_IdStr.Length < m_IDCountMax))  // ID 글자수 체크
        {
            GUI_Message = string.Format("ID는 {0}글자 이상 {1}글자 이하로 작성해 주세요.", m_IDCountMin, m_IDCountMax);
            return;
        }
        if (!(m_PWCountMin <= a_PwStr.Length && a_PwStr.Length < m_PWCountMax))  // PW 글자수 체크
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
        // POST방식으로 데이터를 보낸다.
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
    
```
 </details>  
 
<details>
    <summary>로그인 PHP(MySQL)</summary>
    
``` MySQL
<?php
	$u_id = $_POST[ "Input_user" ]; 
	$u_pw = $_POST[ "Input_pass" ];

	$con = mysqli_connect("localhost", "ssmart123", "Helkas2073!", "ssmart123");

	if(!$con)
		die( "Could not Connect" . mysqli_connect_error() ); 

	$check = mysqli_query($con, "SELECT * FROM GraphicShooter WHERE user_id = '".$u_id."'" );	
	$numrows = mysqli_num_rows($check);

	if($numrows == 0)
	{ 
		die("ID does not exist. \n");
	}

	$row = mysqli_fetch_assoc($check); 
	if($row)
	{
		if($u_pw == $row["user_pw"])
		{
			//JSON 생성을 위한 변수
			$RowDatas = array();
			$RowDatas["user_nick"] = $row["user_nick"];		
			$RowDatas["best_score"] = $row["best_score"];
			$output = json_encode($RowDatas, JSON_UNESCAPED_UNICODE);	//PHP 5.4 이상 버전에서 한글 않깨지게..
			//JSON 파일 생성	
	
			//출력
			echo $output;
			echo "\n";
			echo "Login-Success!!";
		}
		else
		{
			die("Pass does not Match. \n");
		}
	}

	mysqli_close($con);
?>

```
</details>


    

<details>
    <summary>계정생성 PHP(MySQL)</summary>
  
``` MySQL
<?php
	$u_id  = $_POST[ "Input_user" ]; 
	$nick   = $_POST[ "Input_nick" ];
	$u_pw = $_POST[ "Input_pass" ];

	$con = mysqli_connect("localhost", "ssmart123", "Helkas2073!", "ssmart123");

	if(!$con)
		die( "Could not Connect" . mysqli_connect_error() ); 
	
	$check = mysqli_query($con, "SELECT user_id FROM GraphicShooter WHERE user_id = '".$u_id."'" );
	$numrows = mysqli_num_rows($check);

	if($numrows != 0) //즉 0이 아니라는 뜻은 내가 찾는 ID값이 존재한 다는 뜻이다.
	{
		die("ID does exist. \n");
	}

	$check = mysqli_query($con, "SELECT user_nick FROM GraphicShooter WHERE user_nick = '".$nick."'" );
	$numrows = mysqli_num_rows($check);

	if($numrows != 0)  //즉 0이 아니라는 뜻은 내가 찾는 Nickname값이 존재한 다는 뜻이다.
	{
		die("Nickname does exist. \n");
	}

	$Result = mysqli_query($con, 
	"INSERT INTO GraphicShooter (user_id, user_nick, user_pw) VALUES ('".$u_id."',  '".$nick."', '".$u_pw."');" );

	if($Result)
		die("Create Success. \n");
	else
		die("Create error. \n");		

	mysqli_close($con);

?>
```
</details>      
    
    
## 2. 로비 화면
![로비](https://user-images.githubusercontent.com/63942174/164644378-517e2c35-bc81-4f2e-ad3e-fb3b0833c897.png)![로비-플레이방법](https://user-images.githubusercontent.com/63942174/164644393-d572bd9d-1141-413b-9176-b30634232f29.png)


     데이터베이스에서 가져온 정보를 점수가 높은 순서대로 나열하였습니다. 자신의 등수는 초록색글씨로 표시하였습니다.
     5초마다 자동으로 업데이트 되지만 내 점수 옆의 버튼을 누르면 바로 업데이트가 되도록 구현하였습니다. 
     HowToPlay버튼을 누르면 게임방법에 대한 설명이 나옵니다.

    
<details>
    <summary>유저 데이터를 가져오고 정렬 (C#)</summary>
  
``` C#
using SimpleJSON; 
using UnityEngine.Networking;
	
// 유저 정보 클래스	
public class UserInfo
{
    public string m_ID = "";
    public string m_Nick = "";
    public int m_BestScore = 0;
}
	
public class LobbyMgr : MonoBehaviour
{	
    // 데이터를 교환하기 위한 URL
    private string Url_GetRanking = "http://ssmart123.dothome.co.kr/_GraphicShooter/GS_GetRanking.php";

    [SerializeField] private float m_LeaderboardResetDelay = 5.0f;	// 리더보드 리셋 딜레이
    private float m_RefreshTime = 5.0f;
	
    // 가져온 유저정보를 리스트로 임시 할당
    List<UserInfo> m_UserList = new List<UserInfo>();	
    // 랭킹 저장
    private int m_MyRank = 0;
    // 순위를 표시할 UIPrefab
    GameObject[] a_RankNode = null;

    // 코루틴에서 오브젝트 반복 생성을 막기 위한 상태변수
    bool islock = false;
	
    private void GetLeaderbord()
    {
        StartCoroutine(RefreshRankingCo());
    }
    // 데이터베이스 유저정보를 가져옴
    IEnumerator RefreshRankingCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

	// 포스트방식으로 데이터 전송
        UnityWebRequest a_www = UnityWebRequest.Post(Url_GetRanking, form);
        yield return a_www.SendWebRequest();

	
        if (a_www.error == null) //에러가 나지 않았을 때 동작
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //<--이렇게 해야 안드로이드에서 한글이 않깨진다.
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get Ranking List Success~") == true)
            {
                // 점수를 표시하는 함수를 호출
                GetRanking(a_ReStr);
            }
        }
        else
        {
            Debug.Log(a_www.error);
        }
    }
    // 가져온 유저데이터를 정렬하기		     
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

        }

        if (RankNodePrefab == null)
            return;

        string a_RankingNodeText = "";

        a_RankNode = new GameObject[m_UserList.Count];

	// UserList에 들어있는 정보를 RankNode에 넣기 위한 반복문
        for (int i = 0; i < m_UserList.Count; i++)
        {
            if(islock == false)
            a_RankNode[i] = (GameObject)Instantiate(RankNodePrefab, RankScrollContent.transform, false);


            a_RankingNodeText = "";

            // 내 ID와 유저 정보가 일치할 때 녹색으로 강조					     
            if (m_UserList[i].m_ID == GlobalValue.g_Unique_ID)
            {
                GlobalValue.g_BestScore = m_UserList[i].m_BestScore;
                a_RankingNodeText += "<color=#008000>";
            }
            else
                a_RankingNodeText += "<color=#000000>";

            a_RankingNodeText += m_UserList[i].m_Nick + "\t\t" + (i + 1).ToString() + "등 \n" + "BestScore : " + m_UserList[i].m_BestScore + "</color>";

            RankScrollContent.transform.GetChild(i).GetComponentInChildren<Text>().text = a_RankingNodeText;

	// RankNode가 하나씩만 생성되도록 제한하는 조건
            if (i == m_UserList.Count - 1)
                islock = true;
        }

	// 내 랭크의 Json데이터를 m_MyRank에 대입
        if (N["my_rank"] != null)
            m_MyRank = N["my_rank"].AsInt;
	
        RefreshMyInfo();
    }
    // 로비 상단에 내 정보 표시
    private void RefreshMyInfo()
    {
        Txt_MyRankInfo.text = GlobalValue.g_UserNick + "\t\t" + m_MyRank + "등\n"
                              + "BestScore : " + GlobalValue.g_BestScore;
    }
}
	
```
</details>
    
	
<details>
	<summary>유저 데이터 정렬 (MySQL)</summary>
	
``` MySQL
<?php
	$u_id = $_POST["input_user"];

	$con = mysqli_connect("localhost", "ssmart123", "Helkas2073!", "ssmart123");

	if(!$con)
		die( "Could not Connect" . mysqli_connect_error() ); 

	$check = mysqli_query($con, "SELECT user_id FROM GraphicShooter WHERE user_id = '".$u_id."'" );
	$numrows = mysqli_num_rows($check);

	if ( !$check || $numrows == 0)
	{
 		die("ID does not exist. \n");
	}

	$JSONBUFF = array(); 

	$BSList = mysqli_query($con, "SELECT * FROM GraphicShooter ORDER BY best_score DESC LIMIT 0, 10");
		
	$rowsCount = mysqli_num_rows($BSList);
	if (!BSList || $rowsCount == 0)
	{
		die("List does not exist. \n");
	}

	$RowDatas = array();
	$Return   = array();

	for($i = 0; $i < $rowsCount; $i++)
	{
		$a_row = mysqli_fetch_array($BSList);       //행 정보 가져오기
		if($a_row != false)
		{
			$RowDatas["user_id"]   = $a_row["user_id"];     
			$RowDatas["user_nick"] = $a_row["user_nick"];  
			$RowDatas["best_score"] = $a_row["best_score"];
			array_push($Return, $RowDatas); 
		}
	}

	$JSONBUFF['RkList'] = $Return;   

	mysqli_query($con, "SET @curRank := 0");

	$check = mysqli_query($con, "SELECT user_id, myrankidx FROM (SELECT user_id, 
   		 rank() over(ORDER BY best_score DESC) as myrankidx
   			 FROM GraphicShooter) as CNT 
  			 	WHERE user_id='".$u_id."'");

	$numrows = mysqli_num_rows($check);

	if (!$check || $numrows == 0)
	{
		die("Ranking search failed for ID. \n");
	}

	if($row = mysqli_fetch_assoc($check))
	{	
		$JSONBUFF["my_rank"]   = $row["myrankidx"];   
		$output = json_encode($JSONBUFF, JSON_UNESCAPED_UNICODE); 
		echo $output;
		echo "Get Ranking List Success~";
	}

	mysqli_close($con);
?>	
	
```
</details>
	
    

    
## 3-1. 인게임 카메라 이동 및 마우스 감도 설정   
![벽에 붙었을때](https://user-images.githubusercontent.com/63942174/164950842-07a34cae-20f5-40f8-8a82-7064b72e8fe9.PNG)![카메라 벽인식](https://user-images.githubusercontent.com/63942174/164950850-bfc1ab8d-aa42-454d-af3b-7e78336157b4.PNG)


     TPS의 카메라 무브를 구현하였습니다.
     에임에서 카메라 방향으로 Ray를 쏩니다. 오브젝트에 Ray가 부딪치면 
     에임과 오브젝트간의 거리를 측정하고 카메라의 거리를 조절합니다.

<details>
    <summary>카메라 컨트롤 (C#)</summary>
  
``` C#
    // 카메라가 회전하기 위한 감도
    private float m_SensitiveCurH = 10.0f;
    private float m_SensitiveCurV = 10.0f;
	
    // 보안수준을 유지하면서 다른곳에서 읽기 및 수정을 하기 위한 프로퍼티
    public float HSensP { get { return m_SensitiveCurH; } set { m_SensitiveCurH = value; } }
    public float VSensP { get { return m_SensitiveCurV; } set { m_SensitiveCurV = value; } }
	
    // 카메라 위치 계산용 변수
    [Header("카메라 위치 변수")]
    public float m_hight = 1.8f;  // 에임 높이

    public float m_Dist_Aim = 0.23f;  // 에임과 캐릭터의 거리
    public float m_Dist_Cam = 1.8f;   // 캠과 에임과의 거리
    float m_CurDist = 1.8f;  	      // 캠과 에임과의 거리
	
	
    private void FixedUpdate()
    {
        CheckCameraDistance();
    }
	
    // 에임에서 카메라 방향으로 레이를 쏴서 부딪칠 때의 거리를 구하고 
    // 카메라와 에임과의 거리를 조절한다.
    private void CheckCameraDistance()
    {
	// 카메라에서 에임을 향하는 방향벡터
        Vector3 a_RayDir = (transform.position - m_AimPivot).normalized;

	// RayCast를 사용하여 부딪친 장소에 hit정보를 가져옴
        if (Physics.Raycast(m_AimPivot, a_RayDir, out hit, m_Dist_Cam + 0.2f))
        {
	    // 카메라의 거리 조절
            m_CurDist = hit.distance - 0.01f;
	    // 카메라 거리 제약
            if (m_CurDist <= 0.1f)
                m_CurDist = 0.1f;
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
    // 마우스 입력값 메소드
    private void MouseInput()
    {
        m_InputH = Input.GetAxis("Mouse X");
        m_InputV = Input.GetAxis("Mouse Y");
    }
    // 카메라의 회전을 제한하는 메소드			 
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
    // 견착시 카메라의 거리 변경을 위한 메소드
    private void ChangeStateValue()
    {
	// 앉기 상태이거나 점프상태일때 높이 조절 
        if (m_PlayerCtrl.e_PlayerState == PlayerState.Crouch || m_PlayerCtrl.e_PlayerState == PlayerState.Jump)
            m_hight = 1.2f;
        else 
            m_hight = 1.8f;

	// 견착상태일 때 에임의 오프셋 조절
        if (m_PlayerCtrl.IsAimP == true)
        {
            m_Dist_Aim = 0.5f;
            m_Dist_Cam = 0.9f;

        }
        else
        {
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

    // 에임 위치 계산
    Vector3 HorizontalRot(Vector3 a_PlayerPos, float a_hight)
    {
        m_AimPivot = a_PlayerPos;
	// 에임의 높이
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

```
    
 </details>
    
    
    
## 3-2. 캐릭터 이동 및 조작
![에임](https://user-images.githubusercontent.com/63942174/164950324-cb303f88-a6af-46ae-9f12-380cbe1d2600.PNG)![앉기](https://user-images.githubusercontent.com/63942174/164950331-0ce82195-3a14-43fd-a32b-a740687efe93.PNG)![점프](https://user-images.githubusercontent.com/63942174/164950339-018619e8-bf64-4e72-a341-ef0415ac8dd8.PNG)![공격](https://user-images.githubusercontent.com/63942174/164950431-dd9b4ee6-eb36-407f-80bd-b042c60869a5.png)


    캐릭터 이동 및 조작입니다. 
    AddForce의 무게를 무시하고 일정한 속도로 움직이는 ForceMode.VelocityChange를 사용했습니다.
    캐릭터는 카메라의 전방방향을 바라보게 구현하였습니다. 

    

<details>
    <summary>플레이어 정보 및 컨트롤(C#)</summary>
  
``` C#
public class PlayerInit : MonoBehaviour
{
    [Header("플레이어 이동속도 변수")]
    [SerializeField] protected float m_PlayerCrouchSpeed;
    [SerializeField] protected float m_PlayerWalkSpeed;
    [SerializeField] protected float m_PlayerRunSpeed;    
    [SerializeField] protected float m_PlayerSprintSpeed;
    [SerializeField] protected float m_PlayerJumpForce;
    [Header("플레이어 HP")] 
    [SerializeField] protected int m_PlayerHP = 200;

}
--------------------------------------------	
	
// 플레이어의 행동상태를 결정하는 enum함수
public enum PlayerState { Normal = 0, Sprint, Crouch, Jump }
	
namespace SSM
{
	// PlayerInit을 상속받음
    public class PlayerCtrl : PlayerInit
    {
        // 플레이어 상태
        public PlayerState e_PlayerState = PlayerState.Normal;

        // 입력값 저장 변수
        private float m_HInput = 0;
        private float m_VInput = 0;

        // 이동 방향 벡터
        private Vector3 m_TargetMoveDirection = Vector3.zero;   // 카메라 전방방향을 바라보는 방향벡터
        private Vector3 m_CalcTargetDirection = Vector3.zero;   // 이동속도, 중력을 적용한 벡터

        // 캐릭터 회전 속도
        private float m_RotSpeed = 15f;
        private float m_PlayerApplySpeed = 0;
	
	// Animatino Rigging 전환 시간
        private float m_AimDuration = 0.15f; 	  // 에임 상태로 바뀔때까지의 시간
        private float m_SprintDuration = 0.3f;	  // 달리기 상태로 바뀔때까지의 
	
	// 현재 총 발사 딜레이
        private float m_CurFireRate = 0;

	
        // 상태변수
        private bool isMoveSprint = false;
        private bool isMoveCrouch = false;
        private bool isGround = true;
        private bool isAim = false;
	// 상태변수 프로퍼티
        public bool IsMoveCrouchP { get { return isMoveCrouch; } }
        public bool IsAimP { get { return isAim; } }
	
	  private void FixedUpdate()
        {
            ChackIsGround();
            RigidMove();	
        }
	
	// Ray를 캐릭터에서 월드축 Y방향으로 쏴서 바닥을 확인하기
        private void ChackIsGround()
        {
            isGround = Physics.Raycast(transform.position+Vector3.up*0.1f, Vector3.down, capsuleCollider.center.y+ 0.3f);
        }
	
	// 리지드바디를 이용한 물리 이동
        private void RigidMove()
        {
	    // 계산된 목표방향의 속도로 움직입니다.
            rigidBody.AddForce(m_CalcTargetDirection, ForceMode.VelocityChange); 
        }
	
        void Update()
        {
            KeyInput();
            CalcMoveDirection();
            PlayerRotation();
            StateMoveChange();
            StatePlayAnimation();

        }
	
	// 인풋에 관련된 조건을 한곳에 몰아넣음
        private void KeyInput()
        {
            m_HInput = Input.GetAxis("Horizontal");
            m_VInput = Input.GetAxis("Vertical");

	    // LeftShift를 홀드해서 달리기 실행
            if (Input.GetKey(KeyCode.LeftShift))
                TrySprint();
	    // LeftShift에서 손을 때면 달리기 취소
            if (Input.GetKeyUp(KeyCode.LeftShift))
                CancleSprint();
	    // C버튼을 눌렀을 경우 앉기상태 변경
            if (Input.GetKeyDown(KeyCode.C))
                TryCrouch();
	    // Space를 누르고 땅에 있을 경우 점프 실행
            if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
                TryJump();
	    // 마우스 좌클릭을 누르고 있을 시 공격 실행
            if (Input.GetMouseButton(0))
                TryFire();
	    // 마우스 우클릭을 토글하면 조준상태 변경
            if (Input.GetMouseButtonDown(1))
                TryAim();
        }
	// 카메라 전방방향으로 이동하기 위한 계산을 하는 메소드
        private void CalcMoveDirection()
        {
	    // 카메라의 전방(Z) 벡터
            Vector3 a_CameraForward = playerCamera.transform.forward;
            a_CameraForward.y = 0;  
	    
	    // 전방방향에 내적인 오른쪽 방향 구하기
            Vector3 a_CameraRight = Vector3.Cross(Vector3.up, a_CameraForward);
	    // 캐릭터가 움직일 방향벡터 계산
            m_TargetMoveDirection = (a_CameraForward * m_VInput) + (a_CameraRight * m_HInput);
            m_TargetMoveDirection.Normalize();
	    // 캐릭터 상태에 따른 이동속도가 계산된 목표방향
            m_CalcTargetDirection = new Vector3(m_TargetMoveDirection.x * m_PlayerApplySpeed, 
						-0.3f,
						m_TargetMoveDirection.z * m_PlayerApplySpeed);
        }
	// 캐릭터를 회전시키는 메소드
	private void PlayerRotation()
        {
  	    // 캐릭터가 달리는 상황일 때 이동방향을 바라보고 회전한다.
            if (e_PlayerState == PlayerState.Sprint)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(m_TargetMoveDirection),
                                                      Time.deltaTime * m_RotSpeed);
            }
            else
            {   // 캐릭터가 카메라의 전방 방향을 바라보고 회전한다.
		// 카메라의 y축 각도를 가져온다.
                float a_yawCamera = playerCamera.transform.eulerAngles.y;
		// 캐릭터의 y축 각도를 카메라의 y축 각도로 RotSpeed의 빠르기로 회전한다. 
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.Euler(new Vector3(0, a_yawCamera, 0)),
                                                      Time.deltaTime * m_RotSpeed);
            }
        }
	// 캐릭터가 달리는 상태일때 호출되는 
	  private void TrySprint()
        {
            isMoveSprint = true;
	    // 캐릭터가 움직이지 않을 경우 제약조건
            if (m_TargetMoveDirection.magnitude <= 0.1)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }
	    // 달리는 상태에서 w를 땠을때 제약조건	      
            if (isMoveSprint == true && m_VInput <= 0.8f)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }
	    // 캐릭터 상태를 스프린트 상태로 설정
            e_PlayerState = PlayerState.Sprint;
	    // 애니메이터의 1번 상체 레이어 끄기
            playerAnimator.SetLayerWeight(1, 0);
        }
	// 캐릭터의 달리기를 캔슬시키는 메소드
        private void CancleSprint()
        {
            isMoveSprint = false;
	    // 캐릭터의 상태를 일반 상태로 변경
            e_PlayerState = PlayerState.Normal;
	    // 애니메이터의 1번 상체 레이어 켜기
            playerAnimator.SetLayerWeight(1, 1);
        }
	
	// 캐릭터를 앉게 해주는 메소드
        private void TryCrouch()
        {
            isMoveCrouch = !isMoveCrouch;
	    // 캐릭터의 상태를 앉기 상태로 변경
            if (isMoveCrouch == true)
                e_PlayerState = PlayerState.Crouch;
            else
                e_PlayerState = PlayerState.Normal;
        }
	
	// 캐릭터를 점프시키는 메소드
        private void TryJump()
        {
            e_PlayerState = PlayerState.Jump;
	    // 캐릭터가 Y축의 방향으로 PlayerForce의 힘을 한순간에 받음
            rigidBody.AddForce(new Vector3(0, 
					   m_PlayerJumpForce, 
				           0), ForceMode.Impulse);
	    // 애니메이터의 "doJump"트리거를 호출
            playerAnimator.SetTrigger("doJump");
        }
	// 캐릭터가 공격을 할때 호출되는 메소드
	 private void TryFire()
        {
	    // 연사간격 계산
            m_CurFireRate -= Time.deltaTime;
	    // UI를 클릭할 시 발사가 안되도록 막기
            if (EventSystem.current.IsPointerOverGameObject())
                return;
	    // 연사간격이 0이하가 될 경우
            if (m_CurFireRate <= 0)
            {
	        // gunCtrl의 Fire()메소드를 호출
                gunCtrl.Fire();
		// 연사간격 초기화
                m_CurFireRate = gunCtrl.FireRateP;
            }
        }
	// 조준을 할 경우
        private void TryAim()
        {
            isAim = !isAim;
				  
	    // AnimationRigging AimLayer의 weight변경			  
            if (isAim == true)  // 에임모드일시
                rigAimLayer.weight += Time.deltaTime / m_AimDuration;
            else		// 에임모드 해제시
                rigAimLayer.weight -= Time.deltaTime / m_AimDuration;
        }
		
	// 캐릭터 상태 머신
        private void StateMoveChange()
        {
	    // switch-case문을 사용하여 상태 전환에 따른 변수 변경
            switch (e_PlayerState)
            {
		// 일반 상태일 경우
                case PlayerState.Normal:
                    m_PlayerApplySpeed = m_PlayerRunSpeed;
	   	    // Animation Rigging의 SpringLayer의 weight를 감소
        	    rigSprintLayer.weight -= Time.deltaTime / m_SprintDuration;
                    break;

	   	// 달리는 상태일 경우
                case PlayerState.Sprint:
                    m_PlayerApplySpeed = m_PlayerSprintSpeed;
		    // Animation Rigging의 SpringLayer의 weight를 감소	  
                    rigSprintLayer.weight += Time.deltaTime / m_SprintDuration;
                    isMoveCrouch = false;
                    isAim = false;
                    break;

		// 앉기 상태일 경우
                case PlayerState.Crouch:
                    m_PlayerApplySpeed = m_PlayerCrouchSpeed;
                    isMoveSprint = false;
                    break;

		// 점프 상태일 
                case PlayerState.Jump:
                    isMoveCrouch = false;
                    isMoveSprint = false;
                    isAim = false;
                    break;
            }

        }
				  
	// enum 캐릭터 상태 머신 애니메이션
	private void StatePlayAnimation()
        {
            playerAnimator.SetFloat("Horizontal", m_HInput);
            playerAnimator.SetFloat("Vertical", m_VInput);


            switch (e_PlayerState)
            {
                case PlayerState.Normal:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Sprint:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Crouch:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;

                case PlayerState.Jump:
                    playerAnimator.SetBool("isSprint", isMoveSprint);
                    playerAnimator.SetBool("isCrouch", isMoveCrouch);
                    playerAnimator.SetBool("isGround", isGround);
                    playerAnimator.SetBool("isAim", isAim);
                    break;
            }
        }
	// 콜라이더가 Floor태그를 가진 오브젝트에 부딪치면 한번 발생
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.tag == "FLOOR" )
            {
                e_PlayerState = PlayerState.Normal;
            }
        }
	// 몬스터의 공격범위가 캐릭터와 충돌할 때 한번 실행
        private void OnTriggerEnter(Collider other)
        {
            //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
            if (other.gameObject.tag == "PUNCH")
            {
                if (m_PlayerCurHP <= 0.0f)
                    return;

                m_PlayerCurHP -= 10;

                //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
                //Debug.Log("Player HP = " + hp.ToString());

                a_HPRatio = (float)m_PlayerCurHP / (float)m_PlayerHP;
		// gameMgr의 HPFillP 프로퍼티에 값을 대입
                gameMgr.HPFillP = a_HPRatio;

                //Player의 생명이 0이하이면 사망 처리
                if (m_PlayerCurHP <= 0)
                {
                    PlayerDie();
                }
            }//if (other.gameObject.tag == "PUNCH")

	    // 플레이어가 가서는 안되는 위치에 간 경우
            if (other.gameObject.tag == "DeadZone")
            {
                m_PlayerCurHP = 0;
                a_HPRatio = (float)m_PlayerCurHP / (float)m_PlayerHP;

                gameMgr.HPFillP = a_HPRatio;
                PlayerDie();
            }
        }
        //Player의 사망 처리 루틴
        void PlayerDie()
        {
            // Debug.Log("Player Die !!");

            playerAnimator.SetTrigger("doDie");

            //MONSTER라는 Tag를 자진 모든 게임오브젝트를 찾아옴
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

            gameMgr.EndTextUpdate("패배");

            //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
            foreach (GameObject monster in monsters)
            {
                monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            }
	    // 게임 상태 변경
            GameMgr.s_GameState = GameState.GameEnd;
        }
    }
	  
	
```
    
 </details>
    
    
## 3-3. Animation Rigging을 사용한 애니메이션
![Rigging이미지](https://user-images.githubusercontent.com/63942174/165030289-1070cbb3-4b0a-4f99-a59f-5882b1763428.PNG)
![플레이어 캐릭터 인스펙터 창](https://user-images.githubusercontent.com/63942174/165032425-31ee3d07-ba7b-44ad-9566-31a85fda7f9a.PNG)![건 포즈](https://user-images.githubusercontent.com/63942174/165032443-b75b1888-8b0a-45a8-8ff6-143aae67bbb9.PNG)

![왼손 IK](https://user-images.githubusercontent.com/63942174/165032999-1d21f179-7152-4d60-bdab-23de2a2a3a73.PNG)![오른손IK](https://user-images.githubusercontent.com/63942174/165033004-2973a9d2-d48c-47b1-bd1b-6fe49863ad5a.PNG)

	애니메이션을 수정하지 않고 자연스럽게 자세를 만들기 위해서 AnimationRigging기능을 사용했습니다. 
	무기에 손잡이 오브젝트를 추가해서 총을 잡는 팔을 자연스럽게 위치하도록 만들었고 
	총구가 에임포인트를 향하도록 구현하였습니다. 조준상태는 RigLayer의 Weight를 조절함으로써
	총을 들고있는 자세를 바꿨습니다.

    
    
## 4. 몬스터 컨트롤 및 생성, 스킬 이펙트 구현  
![몬스터 프리팹](https://user-images.githubusercontent.com/63942174/165088180-1c9c2a66-e309-4fca-bee4-404b84fd6328.PNG)

	
    몬스터를 일정시간마다 생성하고 랜덤한 스폰 포인트에 생성하는 매니저입니다. 
    생성된 몬스터는 오브젝트 풀에 저장하고 사용하는 방식으로 구현하였습니다.
    몬스터의 쉐이더와 스킬사용 및 사망시의 커스텀 쉐이더을 만들고 스킬 사용시나 몬스터 사망시에
    쉐이더를 교체하여 이펙트를 구현하였습니다.

<details>
    <summary>몬스터 메니저(C#)</summary>
  
``` C#
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


    //----- 스킬 사용시 변수
    public Image SkillCTImg = null;				// 스킬 이미지
    private bool m_isSkUse = false;				// 스킬 사용 여부
    public bool IsSkillUseP { get { return m_isSkUse; } }	// 스킬 사용 여부 프로퍼티

    float m_SkCoolTime = 5.0f;		// 스킬 쿨타임
    float m_SkCount = 0;		// 스킬 쿨타임 카운트

    public float SkCountP { get; }	// 스킬 쿨타임 카운트 프로퍼티
    public float SkcCoolTimeP { get; }  // 스킬 쿨타임 프로퍼티


    //몬스터를 발생시킬 주기
    public float createTime = 1.5f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //게임 종료 여부 변수
    public bool isGameOver = false;

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    float m_AI_Delay = 0.0f;
	
    private void Awake()
    {
        //Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        // 셰이더 찾아오기
        g_DefShader = Shader.Find("Legacy Shaders/Bumped Specular");    // 기본 셰이더
        g_GrayscaleShader = Shader.Find("Custom/Grayscale");    // 스톤 셰이더 - 회색
        g_DeadShader = Shader.Find("Custom/AddTexColor");   // 사망시 세이더
	
	// 몬스터풀을 묶어줄 부모 오브젝트
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) == true && SSM.GameMgr.s_GameState == GameState.GameIng)
            m_isSkUse = true;

        if (m_isSkUse == true )
            UseSkill();
    }

    // 스킬 사용시 호출되는 메서드
    void UseSkill()
    {
	// 쿨타임 카운트
        m_SkCount += Time.deltaTime;
	
	// 스킬 쿨타임과 카운트 비율을 이미지의 FillAmount에 적용
        float a = m_SkCount / m_SkCoolTime;
        SkillCTImg.fillAmount = a;

	// 스킬 지속시간이 끝나면
        if (m_SkCount >= m_SkCoolTime)
        {
            m_SkCount = 0;
            SkillCTImg.fillAmount = 1;
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
                }
            }
        }
    }
}
    
```
    
 </details>

	
<details>
    <summary>몬스터 컨트롤(C#)</summary>
  
``` C#
// Navigation을 사용하기 위한 네임스페이스
using UnityEngine.AI;
	
public class MonsterCtrl : MonoBehaviour
{
    //몬스터의 상태 정보가 있는 Enumerable 변수 선언
    public enum MonsterState { idle, trace, attack, die };

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public MonsterState monsterState = MonsterState.idle;

    //속도 향상을 위해 각종 컴포넌트를 변수에 할당
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent nvAgent;
    private Animator animator;
    private MonsterMgr monsterMgr;

    //추적 사정거리
    public float traceDist = 10.0f;

    //공격 사정거리
    public float attackDist = 2.0f;

    //몬스터의 사망 여부
    private bool isDie = false;

    //혈흔 효과 프리팹
    public GameObject bloodEffect;
    //얇은 데칼 효과 프리팹
    public GameObject bloodDecal;

    //몬스터 생명 변수
    private int hp = 100;

    //GameMgr에 접근하기 위한 변수
    private GameMgr gameMgr;

    // 스킬 이펙트를 적용하기 위한 Material과 SkinnedMeshRenderer 선언
    private SkinnedMeshRenderer[] m_SMRList = null;
    private Material[] m_Materials;

    void Awake()
    {
	// 추적 거리 설정
        traceDist = 100.0f;
	// 공격 사거리
        attackDist = 1.8f;

        //몬스터의 Transform 할당
        monsterTr = this.gameObject.GetComponent<Transform>();

        //추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        //NavMeshAgent 컴포넌트 할당
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();

        //Animator 컴포넌트 할당
        animator = this.gameObject.GetComponent<Animator>();

	// 컴포넌트를 찾아서 연결하기
        gameMgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();

        m_SMRList = this.GetComponentsInChildren<SkinnedMeshRenderer>();

        monsterMgr = GameObject.Find("MonsterMgr").GetComponent<MonsterMgr>();
    }

    void Update()
    {
	// 게임 종료시 정지
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        ChangeShader();

	// 스킬 사용시 몬스터 행동 정지
        if (monsterMgr.IsSkillUseP == true)
            return;
	
        CheckMonStateUpdate();
        MonActionUpdate();
        
    }

    // 몬스터 상태에 따른 쉐이더 변화
    void ChangeShader()
    {
        if (monsterMgr.IsSkillUseP == false && monsterState != MonsterState.die)
        { 
	    // 몬스터가 일반 상태일 때
            animator.speed = 1;
            for (int i = 0; i < m_SMRList.Length; i++)
            {
                m_Materials = m_SMRList[i].materials;
                for (int a = 0; a < m_Materials.Length; a++)
                {
                    if (MonsterMgr.g_DefShader != null &&
                        m_Materials[a].shader != MonsterMgr.g_DefShader)
                    {
                        m_Materials[a].shader = MonsterMgr.g_DefShader;
                    }
                }
            }
        } 
        else if (monsterMgr.IsSkillUseP == true && monsterState != MonsterState.die)
        {
	    // 몬스터가 돌이 되었을 때
            animator.speed = 0;
            nvAgent.isStopped = true;
            for (int i = 0; i < m_SMRList.Length; i++)
            {
                m_Materials = m_SMRList[i].materials;
                for (int a = 0; a < m_Materials.Length; a++)
                {
                    if (MonsterMgr.g_GrayscaleShader != null &&
                        m_Materials[a].shader != MonsterMgr.g_GrayscaleShader)
                    {
                        m_Materials[a].shader = MonsterMgr.g_GrayscaleShader;
                    }
                }
            }
        }
        else if (monsterState == MonsterState.die)  
        {
            // 몬스터가 죽었을 때
            animator.speed = 1;
            // 몬스터의 쉐이더에 변경시킬 적용 색깔
            Color a_CalcColor = new Color(0, 1.0f, 0, 1.0f);
            for (int i = 0; i < m_SMRList.Length; i++)
            {
                // 몬스터의 Material을 찾는다
                m_Materials = m_SMRList[i].materials;

                for (int a = 0; a < m_Materials.Length; a++)
                {
                    if (MonsterMgr.g_DeadShader != null)
                    {
                        m_Materials[a].shader = MonsterMgr.g_DeadShader;
                        m_Materials[a].SetColor("_AddColor", a_CalcColor);
                    }
                }
            }
        }
    }

    // 몬스터의 상태를 업데이트
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;
	// 몬스터의 상태를 변경하기 위한 
        m_AI_Delay -= Time.deltaTime;
	
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //몬스터와 플레이어 사이의 거리 측정
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);

        if (dist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //추적거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.trace; //몬스터의 상태를 추적으로 설정
        }
        else
        {
            monsterState = MonsterState.idle; //몬스터의 상태를 idle 모드로 설정
        }
    }

    //몬스터의 상태값에 따라 적절한 동작을 수행하는 함수
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            // idle 상태
            case MonsterState.idle:
                // 추적 중지
                nvAgent.isStopped = true;  //nvAgent.Stop(); 
                // Animator의 IsTrace 변수를 false로 설정
                animator.SetBool("IsTrace", false);
                break;

            //추적 상태
            case MonsterState.trace:
                // 추적 대상의 위치를 넘겨줌
                nvAgent.destination = playerTr.position;
                nvAgent.isStopped = false;  //nvAgent.Resume();
                // Animator의 IsTrace 변수를 false로 설정
                animator.SetBool("IsAttack", false);
                // Animator의 IsTrace 변수를 true로 설정
                animator.SetBool("IsTrace", true);
                // 애니메이션 속도는 Animator의 Walk노드에서 개별적으로 Speed 설정해 주었다.
                break;

            //공격 상태
            case MonsterState.attack:
                {
                    nvAgent.isStopped = true;  //nvAgent.Stop();    
                    // Animator의 IsAttack 변수를 true로 설정 Attack State로 전이
                    animator.SetBool("IsAttack", true);
                    // 애니메이션 속도는 Animator의 Attack노드에서 개별적으로 Speed 설정해 주었다.

                    // 몬스터가 주인공을 공격하면서 바라 보도록 해야 한다.   
                    float m_RotSpeed = 6.0f;              //초당 회전 속도
                    Vector3 a_CacVLen = playerTr.position - monsterTr.position;
                    a_CacVLen.y = 0.0f;
                    Quaternion a_TargetRot =
                                Quaternion.LookRotation(a_CacVLen.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                              a_TargetRot, Time.deltaTime * m_RotSpeed);
                }
                break;
        }

    }
    //Bullet과 충돌 체크
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "BULLET")
        {
            //혈흔 효과 함수 호출
            CreateBloodEffect(coll.transform.position);

            //맞은 총알의 Damage를 추출해 몬스터 hp ckrka
            hp -= coll.gameObject.GetComponent<BulletCtrl>().m_BulletDmg;
            if (hp <= 0)
            {
                MonsterDie();
            }
          //  Debug.Log(hp);
            //Bullet 삭제
            Destroy(coll.gameObject);
            //IsHit Trigger를 발생시키면 Any State에 gothit로 전이됨
            animator.SetTrigger("IsHit");
        }
    }

    void CreateBloodEffect(Vector3 pos)
    {
        //혈흔 효과 생성
        GameObject blood1 =
                (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        // 데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);
        // 데칼의 회전값을 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        // 데칼 프리팹 생성
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        // 데칼의 크기가 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        // 5초 후에 혈흔효과 프리팹을 삭제
        Destroy(blood2, 5.0f);
    }

    //플레이어가 사망했을 때 실행되는 함수
    void OnPlayerDie()
    {
        if (isDie == true)
            return;

        //몬스터의 상태를 체크하는 코루틴 함수를 모두 정지시킴
        StopAllCoroutines();

        //추적을 정지하고 애니메이션을 수행
        nvAgent.isStopped = true;  //nvAgent.Stop();

        animator.SetTrigger("IsPlayerDie");
    }

    //몬스터 사망 시 처리 루틴
    void MonsterDie()
    {
        gameObject.tag = "Untagged";

        //모든 코루틴을 정지
        StopAllCoroutines();

        isDie = true;

        monsterState = MonsterState.die;
        nvAgent.isStopped = true;  //nvAgent.Stop();
        animator.SetTrigger("IsDie");

        //몬스터에 추가된 Collider를 비활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        // GameUI의 스코어 누적과 스코어 표시 함수 호출
        gameMgr.DispScore(50);

        // 몬스터 오브젝트 풀로 환원시키는 코루틴 함수 호출
        StartCoroutine(this.PushObjectPool());
    }

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        // 각종 변수 초기화
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        // 몬스터에 추가된 Collider을 다시 활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        // 몬스터를 비활성화
        gameObject.SetActive(false);
    }
}


```
    
 </details>
	
## 5. GameMgr과 UI, 포스트 프로세싱	
	
	
<details>
    <summary>게임 메니저(C#)</summary>
  
``` C#
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
	
public enum GameState { GameIng, GameEnd }
	
public class GameMgr : MonoBehaviour
{	
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
	// 네트워크 업데이트가 중복 실행되는것을 막기 위한 변수
        bool isNetUpdateLock = false;
	
        private void Awake()
        {
            s_GameState = GameState.GameIng;
        }
	
        void Start()
        {
	    // 시작시 스코어를 0으로 초기화
	    DispScore(0);

	    // 마우스 민감도 InputField
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
	
	    // MySQL 연결
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
	
	// 게임중에는 커서가 안보이도록 기능을 하는 메서드
	 private void CursorOption()
        {
            if (isPause == true)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = isPause;
        }
	// ESC를 눌러 일시정지를 시키는 					       
        private void PauseUpdate()
        {
	    // 일시정지 판넬 활성화					       
            PausePanel.SetActive(isPause);
	    // 일시정지 여부에 따른 타임스케일 설정
            if (isPause == true)		
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }
	// 마우스 수평감도 설정
        private void IF_SenceH(string a_Value)
        {
            IF_HSensitive.text = a_Value;
            float a_sensH = float.Parse(a_Value);
            camCtrl.HSensP = a_sensH;
        }
        // 마우스 수직감도 설정
        private void IF_SenceV(string a_Value)
        {
            IF_VSensitive.text = a_Value;
            float a_sensV = float.Parse(a_Value);
            camCtrl.VSensP = a_sensV;
        }
	// 게임 오버가 되었을 때 실행되는 메서드
	public void EndTextUpdate(string a_menutext)
        {
            isPause = true;
            PausePanel.SetActive(isPause);
            ConfigPanel.SetActive(false);
            EndPanel.SetActive(true);
            Text_Menu.text = a_menutext;
        }

	// 스코어 업데이트 메서드
        private void ScoreUpdate()
        {
	    // 최대 점수 제한
            if (m_TotalScore >= 9999)
            {
                m_TotalScore = 9999; 
                GlobalValue.g_BestScore = m_TotalScore;
                EndTextUpdate("승리");
            }
	    // 데이터베이스의 최고 스코어보다 인게임 스코어가 높을 경우
            if (GlobalValue.g_BestScore < m_TotalScore)
            {
                GlobalValue.g_BestScore = m_TotalScore;
                StartCoroutine(UpdateBestScoreCo());
            }
		
            Text_BestScore.text = "BestScore : <color=red>" + GlobalValue.g_BestScore.ToString("D4") + "</color>";
            Text_CurrentScore.text = "Score : <color=green>" + m_TotalScore.ToString("D4") + "</color>";
        }
	// 데이터베이스에 최고스코어를 업데이트 하기 위한 코루틴
        IEnumerator UpdateBestScoreCo()
        {

            WWWForm form = new WWWForm();

            form.AddField("input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
            form.AddField("input_score", GlobalValue.g_BestScore);

	    // 데이터 업데이트가 반복적으로 실행되지 않고 한번만 업로드 되도록 제약					       
            isNetUpdateLock = true;

            if (isNetUpdateLock == true)
            {
		// 최고 점수를 데이터베이스에 업로드
                UnityWebRequest a_www = UnityWebRequest.Post(BestScoreURL, form);
                yield return a_www.SendWebRequest();
            }
            isNetUpdateLock = false;
        }
	// 포스트프로세싱의 처리를 하는 메서드
	private void PostPorseccOption()
        {
            PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();

            if (monsterMgr.IsSkillUseP == true)			// 스킬 사용중
                volume.enabled = monsterMgr.IsSkillUseP;
            else						// 스킬 종료
                volume.enabled = monsterMgr.IsSkillUseP;
            
	    // Bloom의 Value를 조정한다
            volume.profile.GetSetting<Bloom>().intensity.value = PostProcessSlider.value;
        }

        // 점수 누적 및 화면 표시
        public void DispScore(int score)
        {
            m_TotalScore += score;
            textScore.text = "SCORE <color=#ff0000>" + m_TotalScore.ToString("D4") + "</color>";
        }
	// 타이틀로 가는 버튼을 눌렀을 때 발생하는 메서드
        private void GoToTitle()
        {
            Time.timeScale = 1;
	    // 저장되어있는 유저 정보를 초기화 시킴
            GlobalValue.g_Unique_ID = "";
            GlobalValue.g_UserNick = "";
            GlobalValue.g_BestScore = 0;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }
	// 로비로 돌아가기를 눌렀을 때 발생하는 메서드
        private void GoToLobby()
        {
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
```
    
 </details>
	
	
<details>
    <summary>최고점수 업데이트(MySQL)</summary>
``` MySQL
<?php
	// 클라이언트에서 보낸 정보를 변수로 저장
	$u_id = $_POST["input_user"];
	$Bscore = $_POST["input_score"];
	// 서버의 데이터베이스 아이디와 비밀번호를 확인
	$con = mysqli_connect("localhost", "ssmart123", "Helkas2073!", "ssmart123");
	
	// 접속을 하지 못했을 경우 에러 메세지
	if(!$con)
		die( "Could not Connect" . mysqli_connect_error() ); 

	// GraphicShooter의 user_id가 클라이언트에서 받아온 u_id와 같은지 확인
	$check = mysqli_query($con, "SELECT user_id FROM GraphicShooter WHERE user_id = '".$u_id."'");

	// 확인된 check의 갯수를 카운트함
	$numrows = mysqli_num_rows($check);
	// check가 1개도 없을 경우
	if ($numrows == 0)
	{	
 		die("ID does not exist. \n");
	}

	// 데이터베이스에 BestScore를 set
	if( $row = mysqli_fetch_assoc($check) ) 
	{
		mysqli_query($con, 
		"UPDATE GraphicShooter SET `best_score` = '".$Bscore."' WHERE `user_id`= '".$u_id."'");  


		echo ("UpDate Success.");			
	}

	mysqli_close($con);
?>
	
```
    
 </details>
