<?php
	$u_id = $_POST["input_user"];
	$Bscore = $_POST["input_score"];

	$con = mysqli_connect("localhost", "ssmart123", "Helkas2073!", "ssmart123");
	
	if(!$con)
		die( "Could not Connect" . mysqli_connect_error() ); 

	$check = mysqli_query($con, "SELECT user_id FROM GraphicShooter WHERE user_id = '".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if ($numrows == 0)
	{
 		die("ID does not exist. \n");
	}


	if( $row = mysqli_fetch_assoc($check) ) 
	{
		mysqli_query($con, 
		"UPDATE GraphicShooter SET `best_score` = '".$Bscore."' WHERE `user_id`= '".$u_id."'");  


		echo ("UpDate Success.");			
	}

	mysqli_close($con);
?>