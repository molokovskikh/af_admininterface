<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.searchc" CodePage="1251"
    CodeFile="searchc.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Информация о клиентах</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server" defaultbutton="GoFind">
        <table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
            <tr>
                <td colspan="2" height="20">
                    <p align="center">
                        <strong><font face="Verdana" size="2">Статистика работы клиента:</font></strong></p>
                </td>
            </tr>
            <tr>
                <td valign="baseline" align="center" colspan="2">
                    <table id="Table2" cellspacing="0" cellpadding="0" width="350" bgcolor="#e7f6e0"
                        border="0">
                        <tr>
                            <td colspan="3">
                                <p align="center">
                                    <font face="Verdana" size="2">Выполните поиск клиента:</font></p>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 181px; height: 117px;">
                                <p align="center">
                                    &nbsp;&nbsp;
                                    <asp:TextBox ID="FindTB" runat="server" BorderStyle="None" Font-Names="Verdana" Font-Size="8pt"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="FindTB"
                                        ErrorMessage="*"></asp:RequiredFieldValidator></p>
                            </td>
                            <td align="center" style="height: 117px">
                                <div align="left">
                                <asp:RadioButtonList ID="FindRB" runat="server" BorderStyle="None" Font-Names="Verdana"
                                    Font-Size="8pt" Width="81px">
                                    <asp:ListItem Value="0" Selected="True">Имя</asp:ListItem>
                                    <asp:ListItem Value="1">ID</asp:ListItem>
                                    <asp:ListItem Value="3">Billing ID</asp:ListItem>
                                    <asp:ListItem Value="2">Логин</asp:ListItem>
                                </asp:RadioButtonList>
                                </div>
                                </td>
                            <td style="height: 117px">
                                <p align="center">
                                    <asp:Button ID="GoFind" runat="server" BorderStyle="None" Font-Names="Verdana" Font-Size="8pt"
                                        Text="Найти" OnClick="GoFind_Click" CausesValidation="true"></asp:Button></p>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3">
                                <asp:CheckBox ID="ADCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Показывать статус Active Directory"
                                    Checked="True"></asp:CheckBox></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="baseline" align="center" colspan="2" height="10">
                    <p align="left">
                        &nbsp;</p>
                </td>
            </tr>
            <tr>
                <td valign="baseline" align="center" colspan="2" style="height: 211px">
                    <p>
                        <asp:Table ID="Table3" runat="server" BorderStyle="Solid" Font-Names="Verdana" Font-Size="8pt"
                            BorderColor="#DADADA" CellSpacing="0" CellPadding="0" GridLines="Both" Visible="False"
                            BackColor="#EBEBEB">
                            <asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue"
                                Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">
                                <asp:TableCell Text="Биллинг&lt;br&gt;код"></asp:TableCell>
                                <asp:TableCell Width="70px" Text="Код"></asp:TableCell>
                                <asp:TableCell Text="Наименование"></asp:TableCell>
                                <asp:TableCell Text="Регион"></asp:TableCell>
                                <asp:TableCell Text="Текущее&lt;br&gt;(подтвержденное)&lt;br&gt;обновление"></asp:TableCell>
                                <asp:TableCell Text="Предыдущее&lt;br&gt;(неподтвержденное)&lt;br&gt;обновление"></asp:TableCell>
                                <asp:TableCell Text="EXE"></asp:TableCell>
                                <asp:TableCell Text="MDB"></asp:TableCell>
                                <asp:TableCell Text="Login"></asp:TableCell>
                                <asp:TableCell Text="Сегмент"></asp:TableCell>
                                <asp:TableCell Text="Тип"></asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </p>
                    <p>
                        <asp:Label ID="Label1" runat="server" Font-Names="Verdana" Font-Size="9pt" Visible="False"
                            Font-Bold="True">Клиент не найден</asp:Label></p>
                    <p align="center">
                        <asp:Table ID="Table4" runat="server" Font-Names="Verdana" Font-Size="8pt" Width="253px"
                            Visible="False">
                            <asp:TableRow>
                                <asp:TableCell BackColor="#FF6600" Width="30px"></asp:TableCell>
                                <asp:TableCell Text=" - Клиент отключен"></asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Aqua"></asp:TableCell>
                                <asp:TableCell Text=" - Учетная запись отключена"></asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Violet"></asp:TableCell>
                                <asp:TableCell Text=" - Учетная запись заблокированна"></asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Gray"></asp:TableCell>
                                <asp:TableCell Text=" - Обновение более 2 суток назад"></asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Red"></asp:TableCell>
                                <asp:TableCell Text=" - Ошибка Active Directory"></asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </p>
                    <asp:Label ID="TimeFLB" runat="server" Font-Names="Verdana" Font-Size="8pt">Время поиска:</asp:Label><asp:Label
                        ID="TimeSLB" runat="server" Font-Names="Verdana" Font-Size="8pt"></asp:Label></td>
            </tr>
            <tr>
                <td style="height: 16px" height="16">
                    <p align="center">
                        <font face="Arial, Helvetica, sans-serif"><font color="#000000" size="1">© АК "</font>
                            <a href="http://www.analit.net/"><font size="1">Инфорум</font></a><font color="#000000"
                                size="1">" 2004</font></font></p>
                </td>
            </tr>
        </table>
        &nbsp;</form>
</body>
</html>
