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