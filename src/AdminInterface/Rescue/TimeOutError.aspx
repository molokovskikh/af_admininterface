<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TimeOutError.aspx.cs" Inherits="AdminInterface.Rescue.TimeOutError" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Ошибка в Административном интерфейсе</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
<style>
body{
font-family: Verdana;
color:#303030;
margin:10px;
}
h1{
font-size:1.5em;
}
p{
font-size:0.8em;
}
</style>
</head>
<body>
	<h1 style="color: red">
	Ошибка в Административном интерфейсе
	</h1>
	<p>Операция завершилось неудачей. Попробуйте повторить через несколько минут.</p>
	<p><a href="javascript:history.back()">Возврат на предыдущую страницу</a></p>
</body>
</html>
