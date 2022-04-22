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