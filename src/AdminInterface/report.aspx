<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.report" CodePage="1251"
    CodeBehind="report.aspx.cs" %>

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
                                        <img src="./Images/logo.gif"></p>
                                </td>
                                <td valign="top" align="center">
                                    <font face="Verdana" size="2"><strong>��������������� ����� �</strong>&nbsp; </font>
                                    <asp:Label ID="LBCode" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Verdana">Test</asp:Label><br>
                                    <font face="Verdana" size="2"><strong>������� � </strong></font>
                                    <asp:Label ID="DogNLB" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Verdana">Test</asp:Label><br>
                                    <font face="Verdana" size="2"><font size="1">���� ��������:</font>
                                        <asp:Label ID="RegDate" runat="server" Font-Size="7pt" Font-Names="Verdana">2005-01-01 00:00:00</asp:Label></font><br>
                                    <asp:Label ID="ChPassMessLB" runat="server" Font-Size="7pt" Font-Names="Verdana"
                                        Visible="False">[��������� ������ �� ���������� �������]</asp:Label></td>
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
                                    <td align="right" width="275" height="25">
                                        <p align="right">
                                            <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman';
                                                mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><font
                                                    face="Verdana" size="2"><strong>������ ������������:</strong>&nbsp;</font></span></p>
                                    </td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBClient" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">����-����</asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <font face="Verdana" size="2"><strong>������� ������������:</strong>&nbsp;</font>
                                    </td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBShortName" runat="server" Font-Names="Times New Roman" Font-Size="9pt"
                                            Font-Bold="True">����</asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman';
                                            mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><font
                                                face="Verdana" size="2"><strong>��� �������:</strong>&nbsp;</font></span></td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="TariffLB" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">�������� �������������� �������</asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman';
                                            mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><font
                                                face="Verdana" size="2"><strong>��� ��� ������� � �������(login):</strong>&nbsp;</font></span></td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBLogin" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">demonstr</asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="right" height="25">
                                        <span style="font-size: 12pt; font-family: Arial; mso-fareast-font-family: 'Times New Roman';
                                            mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><font
                                                face="Verdana" size="2"><strong>������:</strong>&nbsp;</font></span></td>
                                    <td height="25">
                                        <font face="Verdana" size="2">&nbsp; </font>
                                        <asp:Label ID="LBPassword" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">analog</asp:Label></td>
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
                <td align="center" height="120">
                    <p>
                        <font face="Arial" size="2">
                            <table id="Table7" bordercolor="#000000" cellspacing="0" cellpadding="0" width="95%"
                                border="1">
                                <tr>
                                    <td align="center" height="30">
                                        <font face="Verdana" size="1"><strong>��������� �������������:</strong></font></td>
                                </tr>
                                <tr>
                                    <td valign="top" height="60">
                                        <p>
                                            <font size="1"><font face="Verdana">���������������� ������ ���.: (4732) 206-000(��.
                                                - ���. 9.00 - 18.00 MSK), e-mail: <a>tech@analit.net</a></font><br>
                                            </font>
                                            <asp:Label ID="SupportPhoneLB" runat="server" Font-Size="7pt" Font-Names="Verdana"
                                                Visible="False"> ������������� � ����� �������. ���.: </asp:Label><br>
                                            <asp:Label ID="SupportNameLB" runat="server" Font-Size="7pt" Font-Names="Verdana"
                                                Visible="False">��� ������������ ��������: </asp:Label></p>
                                    </td>
                                </tr>
                            </table>
                    </p>
                    <p>
                &nbsp;
            </tr>
            <tr>
                <td valign="top" align="left" height="50">
                    <p>
                        <font face="Verdana" size="1"><strong>���������� ������������� ����������� � ���������
                            ������ �������������� �� ����� www.analit.net<br>
                            � ������� "��� ������������������ �������������"</strong></font></p>
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
            <tr>
                <td>
                    ---------------------------------------------------------------------------------------------------------</td>
            </tr>
            <tr>
                <td align="center">
                    <p>
                        <font face="Verdana" size="1"><strong>�������� ����� � ����� � </strong></font>
                        <asp:Label ID="LBCard" runat="server" Font-Bold="True" Font-Size="7pt" Font-Names="Verdana">Test</asp:Label><strong><font
                            face="Verdana" size="1">, ������� � </font></strong>
                        <asp:Label ID="DogNNLB" runat="server" Font-Bold="True" Font-Size="7pt" Font-Names="Verdana">Test</asp:Label><br>
                        <asp:Label ID="RepLb" runat="server" Font-Size="7pt" Font-Names="Verdana" Visible="False"> [��������� ������]</asp:Label><font
                            face="Verdana" size="1"></font></p>
                </td>
            </tr>
            <tr>
                <td>
                    <table id="Table3" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <tr>
                            <td align="right" width="100" height="20">
                                <font face="Verdana" size="1">������:&nbsp; </font>
                            </td>
                            <td height="20">
                                <asp:Label ID="LBCCard" runat="server" Font-Size="7pt" Font-Names="Verdana">����-����</asp:Label><font
                                    face="Verdana" size="1"></font></td>
                        </tr>
                        <tr>
                            <td align="right" width="100" height="20">
                                <font face="Verdana" size="1">Login:&nbsp; </font>
                            </td>
                            <td height="20">
                                <asp:Label ID="LBLcard" runat="server" Font-Size="7pt" Font-Names="Verdana">demonstr</asp:Label><font
                                    face="Verdana" size="1"></font></td>
                        </tr>
                        <tr>
                            <td align="right" width="100" height="20">
                                <p dir="ltr" style="margin-right: 0px">
                                    <font face="Verdana" size="1">������:&nbsp; </font>
                                </p>
                            </td>
                            <td height="20">
                                <asp:Label ID="TariffD" runat="server" Font-Size="7pt" Font-Names="Verdana">�������� �������������� �������</asp:Label><font
                                    face="Verdana" size="1"></font></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <p>
                        <font face="Verdana" size="1"><strong>��������������� ����� �������.���������������
                            � ������������������ ������� ������.</strong>
                            <br>
                            <br>
                            �������������(�.�.�.): _____________________________________________________ </font>
                    </p>
                </td>
            </tr>
            <tr>
                <td>
                    <p>
                        <font size="2"><font face="Arial">
                            <table id="Table4" cellspacing="0" cellpadding="0" width="100%" border="0">
                                <tr>
                                    <td>
                                        <font face="Verdana" size="1">����:</font></td>
                                    <td>
                                        <asp:Label ID="LBDate" runat="server" Font-Size="7pt" Font-Names="Verdana"></asp:Label><font
                                            face="Verdana" size="1"></font></td>
                                    <br>
                                    <td align="right">
                                        <font face="Verdana" size="1">�������:</font></td>
                                    <td>
                                        <font face="Verdana" size="1">_______________________</font></td>
                                </tr>
                            </table>
                        </font></font>
                    </p>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
