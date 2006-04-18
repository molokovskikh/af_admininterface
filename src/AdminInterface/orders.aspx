<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.orders" CodePage="1251"
    CodeFile="orders.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Статистика заказов</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
            <tr>
                <td>
                    <div align="center">
                        <table id="Table2" cellspacing="0" cellpadding="0" width="320" align="center" border="0">
                            <tr>
                                <td colspan="2">
                                    <p align="center">
                                        <font face="Arial" size="2"><strong>Укажите период</strong></font></p>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <font face="Arial" size="2"><strong>C:</strong></font></td>
                                <td colspan="1">
                                    <font face="Arial" size="2"><strong>По:</strong></font></td>
                            </tr>
                            <tr>
                                <td>
                                    <p align="right">
                                        <asp:Calendar ID="CalendarFrom" runat="server" TitleFormat="Month" ShowGridLines="True"
                                            Font-Size="10pt" Font-Names="Arial" BackColor="#F6F6F6">
                                            <SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
                                        </asp:Calendar>
                                    </p>
                                </td>
                                <td>
                                    <p align="right">
                                        <asp:Calendar ID="CalendarTo" runat="server" TitleFormat="Month" FirstDayOfWeek="Monday"
                                            ShowGridLines="True" Font-Size="10pt" Font-Names="Arial" BackColor="#F6F6F6">
                                            <SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
                                        </asp:Calendar>
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <p align="right">
                                        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Показать" Font-Names="Arial" Font-Size="10pt"
                                            BorderStyle="None"></asp:Button></p>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div align="center">
                        <asp:Table ID="Table3" runat="server" BorderStyle="Solid" Font-Size="8pt" Font-Names="Arial"
                            CellPadding="0" CellSpacing="0" GridLines="Both" BackColor="#F6F6F6">
                            <asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue"
                                Font-Size="10pt" Font-Bold="True">
                                <asp:TableCell Width="70px" Text="№"></asp:TableCell>
                                <asp:TableCell Width="110px" Text="Дата&lt;br&gt;заказа"></asp:TableCell>
                                <asp:TableCell Width="110px" Text="Дата&lt;br&gt;прайса"></asp:TableCell>
                                <asp:TableCell Width="120px" Text="Потребитель"></asp:TableCell>
                                <asp:TableCell Width="120px" Text="Поставщик"></asp:TableCell>
                                <asp:TableCell Width="100px" Text="Прайс"></asp:TableCell>
                                <asp:TableCell Width="70px" Text="Позиций"></asp:TableCell>
                                <asp:TableCell Width="90px" Text="SMTP ID"></asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <p align="center">
                        <font face="Arial, Helvetica, sans-serif"><font color="#000000" size="1">© АК "</font>
                            <a href="http://www.analit.net/"><font size="1">Инфорум</font></a><font color="#000000"
                                size="1">" 2004</font></font></p>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
