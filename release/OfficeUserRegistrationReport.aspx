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
                                    <font face="Verdana" size="2"><strong>��������������� �����</strong>&nbsp; </font>
                                    <br>
                                    <font face="Verdana" size="2"><font size="1">���� ��������:</font>
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
                                            <strong><font face="Verdana" size="2">������� ������:</font></strong></p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <font face="Verdana" size="2"><strong>���:</strong>&nbsp;</font>
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
                                                face="Verdana" size="2"><strong>��� ��� ������� � �������(login):</strong>&nbsp;</font></span></td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
										<asp:Label ID="LBLogin" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman" />
									</td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA">
                                        <font face="Verdana" size="2"><strong>������:</strong>&nbsp;</font></span>
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
                        <font size="1"><font face="Verdana"><strong><em>������������ ����������� �������� ������
                            ����� ������� ������!</em></strong></font><br>
                            <font face="Verdana">����������, ����� ����� ������: </font></font>
                    </p>
                    <ul>
                        <li><font face="Verdana" size="1">�� �������� � �������&nbsp;��� �� ��� �������, ����������
                            � �������� �������. </font>
                            <li><font face="Verdana" size="1">�� ��� ��������� ���������� ������ ��� ������� ������,
                                ��������� ����������� �������. </font>
                                <li><font face="Verdana" size="1">�� ��� ��� ��, ��� � ���������� ������, �� � ������
                                    � �����. </font>
                                    <li><font face="Verdana" size="1">�� ��� ����� ������, ������� ����� ������, �������
                                        ����� ������. </font>
                                        <li><font face="Verdana" size="1">�� ��� ������ ������. </font>
                                            <li><font face="Verdana" size="1">������ �� �������� ������ �� ���� ���� � �����. </font>
                                            </li>
                    </ul>
                    <p align="justify">
                        <font face="Verdana" size="1">������������ ���������� � ������ ������: </font>
                    </p>
                    <ul>
                        <li><font face="Verdana" size="1">�� ������ ��������� �������� ������� � ����� � �������.
                        </font>
                            <li><font face="Verdana" size="1">�� ������ ���� �����&nbsp;8 (������) ��������. </font>
                                <li><font face="Verdana" size="1">�� ������ ��������� ������ �����. </font>
                                    <li><font face="Verdana" size="1">������������� ������� ���� �����������.</font></li></ul>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
