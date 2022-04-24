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
    // MYSQL 도메인에 있는 PHP파일 주소
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
    
     TPS의 카메라 무브를 구현하였습니다.
     카메라가 벽이나 오브젝트에 가까이가면 에임과의 거리가 줄어들면서 카메라가 확대됩니다.

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
    float m_CurDist = 1.8f;   // 캠과 에임과의 거리
	
	
    private void FixedUpdate()
    {
        CheckCameraDistance();
    }
    // 에임에서 카메라 방향으로 레이를 쏴서 부딛힐 때의 거리를 구하고 
    // 카메라와 에임과의 거리를 조절한다.
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
        if (m_PlayerCtrl.IsAimP == true)  // 견착상태일때
        {
            m_hight = 1.8f;
            m_Dist_Aim = 0.5f;
            m_Dist_Cam = 0.9f;

        }
        else		// 일반 상태일때
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

```
    
 </details>
    
    
    
## 3-2. 캐릭터 이동 및 조작
![에임](https://user-images.githubusercontent.com/63942174/164950324-cb303f88-a6af-46ae-9f12-380cbe1d2600.PNG)![앉기](https://user-images.githubusercontent.com/63942174/164950331-0ce82195-3a14-43fd-a32b-a740687efe93.PNG)![점프](https://user-images.githubusercontent.com/63942174/164950339-018619e8-bf64-4e72-a341-ef0415ac8dd8.PNG)![공격](https://user-images.githubusercontent.com/63942174/164950431-dd9b4ee6-eb36-407f-80bd-b042c60869a5.png)


    캐릭터 이동 및 조작입니다. 
    Transform으로 캐릭터가 이동하는 방식을 사용했었으나 벽을 통과하는 문제가 발생했습니다.
    해결방안을 찾다가 AddForce의 무게를 무시하고 일정한 속도로 움직이는 ForceMode.VelocityChange를 사용했습니다.

    

<details>
    <summary>PlayerCtrl(C#)</summary>
  
``` C#
	
public enum PlayerState { Normal = 0, Sprint, Crouch, Jump }
	
namespace SSM
{
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
        private Vector3 m_SaveJumpDirectino = Vector3.zero;     // 점프시 이동방향을 임시 저장하는 벡터

        // 캐릭터 회전 속도
        private float m_RotSpeed = 15f;
        private float m_PlayerApplySpeed = 0;
        private float m_AimDuration = 0.15f;
        private float m_SprintDuration = 0.3f;

        private float m_CurFireRate = 0;

        private int m_PlayerCurHP;
	
        float a_HPRatio;
	
        // 상태변수
        private bool isMoveSprint = false;
        private bool isMoveCrouch = false;
        private bool isGround = true;
        private bool isAim = false;
        public bool IsAimP { get { return isAim; } }
	
	
	
	  private void FixedUpdate()
        {
            ChackIsGround();
            RigidMove();
	    if(isJump == true && isGround == true)
		TryJump();
	
        }

        private void ChackIsGround()
        {
            isGround = Physics.Raycast(transform.position+Vector3.up*0.1f, Vector3.down, capsuleCollider.center.y+ 0.3f);
        }
	
        private void RigidMove()
        {
	    // 질량을 무시하고 강체에 즉각적인 속도 변경을 추가합니다.
            rigidBody.AddForce(m_CalcTargetDirection, ForceMode.VelocityChange); 
        }
	
        private void TryJump()
        {
            e_PlayerState = PlayerState.Jump;

            rigidBody.AddForce(new Vector3(m_SaveJumpDirectino.x, m_PlayerJumpForce, m_SaveJumpDirectino.z), ForceMode.Impulse);

            playerAnimator.SetTrigger("doJump");
        }
	
	
        void Update()
        {
            StateMoveChange();
            KeyInput();
            CalcMoveDirection();
            PlayerRotation();
            StatePlayAnimation();

            AimChange();
        }
	private void StatePlayAnimation()
        {
            playerAnimator.SetFloat("Horizontal", m_HInput);
            playerAnimator.SetFloat("Vertical", m_VInput);


            switch (e_PlayerState)
            {
                case PlayerState.Normal:
                    //if(isMoveBlock == true)
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
        private void KeyInput()
        {
            m_HInput = Input.GetAxis("Horizontal");
            m_VInput = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
                TrySprint();
            if (Input.GetKeyUp(KeyCode.LeftShift))
                CancleSprint();

            if (Input.GetKeyDown(KeyCode.C))
                TryCrouch();

            if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
                isJump = true;

            if (Input.GetMouseButton(0))
                TryFire();

            if (Input.GetMouseButtonDown(1))
                TryAim();

            
        }

        private void CalcMoveDirection()
        {
            Vector3 a_CameraForward = playerCamera.transform.forward;
            a_CameraForward.y = 0;

            Vector3 a_CameraRight = Vector3.Cross(Vector3.up, a_CameraForward);

            m_TargetMoveDirection = (a_CameraForward * m_VInput) + (a_CameraRight * m_HInput);
            m_TargetMoveDirection.Normalize();

            m_CalcTargetDirection = new Vector3(m_TargetMoveDirection.x * m_PlayerApplySpeed, -0.3f, m_TargetMoveDirection.z * m_PlayerApplySpeed);
        }
	private void PlayerRotation()
        {
            if (e_PlayerState == PlayerState.Sprint)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(m_TargetMoveDirection),
                                                      Time.deltaTime * m_RotSpeed);
            }
            else
            {
                float a_yawCamera = playerCamera.transform.eulerAngles.y;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.Euler(new Vector3(0, a_yawCamera, 0)),
                                                      Time.deltaTime * m_RotSpeed);
            }
        }
	
	  private void TrySprint()
        {
            isMoveSprint = true;

            if (m_TargetMoveDirection.magnitude <= 0.1)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }
            if (isMoveSprint == true && m_VInput <= 0.8f)
            {
                isMoveSprint = false;
                e_PlayerState = PlayerState.Normal;
                return;
            }

            e_PlayerState = PlayerState.Sprint;
            playerAnimator.SetLayerWeight(1, 0);
        }
        private void CancleSprint()
        {
            isMoveSprint = false;
            e_PlayerState = PlayerState.Normal;

            rigSprintLayer.weight -= Time.deltaTime / m_SprintDuration;

            playerAnimator.SetLayerWeight(1, 1);
        }
        private void TryCrouch()
        {
            isMoveCrouch = !isMoveCrouch;
            if (isMoveCrouch == true)
                e_PlayerState = PlayerState.Crouch;
            else
                e_PlayerState = PlayerState.Normal;
        }
	 private void TryFire()
        {

            m_CurFireRate -= Time.deltaTime;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (m_CurFireRate <= 0)
            {
                gunCtrl.Fire();
                m_CurFireRate = gunCtrl.FireRateP;
            }
        }
        private void TryAim()
        {
            isAim = !isAim;
        }
        private void AimChange()
        {
            if (isAim == true)
                rigAimLayer.weight += Time.deltaTime / m_AimDuration;
            else
                rigAimLayer.weight -= Time.deltaTime / m_AimDuration;
        }
	
```
    
 </details>
    
    
    
    

<details>
    <summary>IKTest</summary>
  
``` C#
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 본을 인스펙터창에 표시하기 위한 클래스
[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class IKTest : MonoBehaviour
{
    // 총기의 손 위치
    [Header("WeaponHandle")]
    public Transform LeftHandle;
    public Transform RightHandle;
    
    private Transform spine;
    private Transform head;

    public Transform targetTransform;   // 타겟 위치
    public Transform aimTransform;      // 총 견착 위치
    public Transform Headbone;          // 머리 본 위치
    
    private Animator playerAnimator;

    public int iterations = 10;     // 반복횟수
    public float weight = 1.0f;     // 보정률

    public HumanBone[] humanBones;  // 본의 갯수 배열
    Transform[] boneTransforms;  // 설정한 본에 해당되는 트렌스폼

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        spine = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);


        boneTransforms = new Transform[humanBones.Length]; // boneTransforms 배열의 크기 선언

        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i] = playerAnimator.GetBoneTransform(humanBones[i].bone);  // 본의 transform 정의
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // 사망시 움직이지 못하도록
        if (GameMgr.s_GameState == GameState.GameEnd) 
            return;
                                                  
        // 견착
        Vector3 targetPosition = targetTransform.position;  // 타겟의 위치를 가져온다.

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length-1; j++)
            {
                Transform bone = boneTransforms[j];
                float boneWeight = humanBones[j].weight * weight;
                // 총이 에임을 바라보도록 만든다.
                AimAtTarget(bone, aimTransform, targetPosition, boneWeight);
            }

            Transform hashs = boneTransforms[4];
            // 머리가 에임을 바라보도록 한다.
            AimAtTarget(hashs, Headbone, targetPosition,weight);
        }

    }

    // 타겟을 바라보도록 하는 메소드
    private void AimAtTarget(Transform bone,Transform aimPosition, Vector3 targetPosition, float weight)
    {
       // Vector3 aimDirection = aimTransform.forward;    // 에임의 전방벡터
        Vector3 aimDirection = aimPosition.forward;    // 에임의 전방벡터
        Vector3 targetDirection = targetPosition - aimPosition.position; // 타겟 위치과 에임 위치를 빼서 타겟 방향을 구함
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);  // 에임이 바라볼 방향
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    private void OnDrawGizmos()
    {
        // 어깨와 에임을 잇는 선
        Gizmos.DrawLine(aimTransform.position, targetTransform.position);
    }

    // 팔의 IK를 적용
    private void OnAnimatorIK(int layerIndex)
    {

        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        // 팔 IK애니메이션 설정
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);


        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,LeftHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,LeftHandle.rotation);

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand,RightHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand,RightHandle.rotation);

    }
}

```
    
 </details>

    
    
## 4. 스킬 사용 및 사망

https://user-images.githubusercontent.com/63942174/161756801-dbdc0365-601e-4c8a-9422-8e3b5a6fe763.mp4


https://user-images.githubusercontent.com/63942174/161756821-36b49dfa-657c-484f-ae91-aa86d49a9ed3.mp4



    게임을 전체적으로 관리하는 GameMgr에서 몬스터의 쉐이더를 통합관리하고 스킬을 사용시
    포스트 프로세싱을 이용하여 효과를 나타냈습니다. 또한 몬스터풀을 사용하여 몬스터의 수를 조절하였습니다.

<details>
    <summary>GameMgr</summary>
  
``` C#
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
                }
            }
        }
    }
}

    
```
    
 </details>

    
