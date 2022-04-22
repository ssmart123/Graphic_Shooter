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