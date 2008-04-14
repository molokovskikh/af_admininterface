<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OfficeUserRegistrationReport.aspx.cs" Inherits="AdminInterface.OfficeUserRegistrationReport" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body>
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="650" border="0">
            <tr>
                <td>
                    <span>
                        <table id="Table6" height="100" cellspacing="0" cellpadding="0" width="100%" border="0">
                            <tr>
                                <td valign="top" align="left" width="230">
                                    <p>
                                        <img src="./Images/logo.gif">
                                    </p>
                                </td>
                                <td valign="top" align="center">
                                    <font face="Verdana" size="2"><strong>Регистрационная карта</strong>&nbsp; </font>
                                    <br>
                                    <font face="Verdana" size="2"><font size="1">Дата операции:</font>
                                    <asp:Label ID="RegDate" runat="server" Font-Size="7pt" Font-Names="Verdana">2005-01-01 00:00:00</asp:Label></font>
                                <br>
                            </tr>
                        </table>
                    </span>
                </td>
            </tr>
            <tr>
                <td align="center" height="15">
                    <p>
                        <font face="Arial" size="2">
                            <table id="Table2" bordercolor="#000000" cellspacing="0" cellpadding="0" width="90%"
                                border="0">
                                <tr>
                                    <td align="right" colspan="2" height="40">
                                        <p align="center">
                                            <strong><font face="Verdana" size="2">Учетные данные:</font></strong></p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <font face="Verdana" size="2"><strong>ФИО:</strong>&nbsp;</font>
                                    </td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBShortName" runat="server" Font-Names="Times New Roman" Font-Size="9pt" Font-Bold="True" />
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman';
                                            mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><font
                                                face="Verdana" size="2"><strong>Имя для доступа к серверу(login):</strong>&nbsp;</font></span></td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
										<asp:Label ID="LBLogin" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman" />
									</td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA">
                                        <font face="Verdana" size="2"><strong>Пароль:</strong>&nbsp;</font></span>
                                    </td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBPassword" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman" />
                                    </td>
                                </tr>
                            </table>
                    </p>
                </td>
            </tr>
            <tr>
                <td align="center" height="15">
                </td>
            </tr>
             <tr>
                <td style="height: 320px">
                    <p>
                        <font size="1"><font face="Verdana"><strong><em>Настоятельно рекомендуем изменить пароль
                            перед началом работы!</em></strong></font><br>
                            <font face="Verdana">Желательно, чтобы новый пароль: </font></font>
                    </p>
                    <ul>
                        <li><font face="Verdana" size="1">Не совпадал с логином&nbsp;или не был логином, записанным
                            в обратном порядке. </font>
                            <li><font face="Verdana" size="1">Не был одиночным английским словом или русским словом,
                                набранным английскими буквами. </font>
                                <li><font face="Verdana" size="1">Не был тем же, что в предыдущем пункте, но с цифрой
                                    в конце. </font>
                                    <li><font face="Verdana" size="1">Не был Вашим именем, кличкой Вашей собаки, номером
                                        Вашей машины. </font>
                                        <li><font face="Verdana" size="1">Не был просто числом. </font>
                                            <li><font face="Verdana" size="1">Хорошо бы изменять пароли не реже раза в месяц. </font>
                                            </li>
                    </ul>
                    <p align="justify">
                        <font face="Verdana" size="1">Обязательные требования к новому паролю: </font>
                    </p>
                    <ul>
                        <li><font face="Verdana" size="1">Не должен содержать символов пробела и точки с запятой.
                        </font>
                            <li><font face="Verdana" size="1">Не должен быть менее&nbsp;8 (восьми) символов. </font>
                                <li><font face="Verdana" size="1">Не должен содержать только цифры. </font>
                                    <li><font face="Verdana" size="1">Использование русских букв недопустимо.</font></li></ul>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
