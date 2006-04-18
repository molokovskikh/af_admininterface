<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.statcont" CodePage="1251"
    CodeFile="statcont.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Статистика обращений</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
        <div align="center">
            <table id="Table2" cellspacing="0" cellpadding="0" width="320" align="center" border="0">
                <tr>
                    <td colspan="2">
                        <p align="center">
                            <font face="Arial" size="2"><strong>Выберите&nbsp;период</strong></font></p>
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
                            <asp:Calendar ID="CalendarFrom" runat="server" OnSelectionChanged="CalendarFrom_SelectionChanged" Font-Names="Arial" Font-Size="10pt"
                                TitleFormat="Month" ShowGridLines="True" BackColor="#F6F6F6">
                                <SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
                            </asp:Calendar>
                        </p>
                    </td>
                    <td>
                        <p align="right">
                            <asp:Calendar ID="CalendarTo" runat="server" OnSelectionChanged="CalendarTo_SelectionChanged" Font-Names="Arial" Font-Size="10pt"
                                TitleFormat="Month" ShowGridLines="True" BackColor="#F6F6F6" FirstDayOfWeek="Monday">
                                <SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
                            </asp:Calendar>
                        </p>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <p align="right">
                            <asp:Button ID="Button1" runat="server" Font-Names="Arial" Font-Size="10pt" Text="Показать"
                                BorderStyle="None"></asp:Button></p>
                    </td>
                </tr>
            </table>
        </div>
        <p align="center">
            <asp:Table ID="Table5" runat="server" Font-Names="Arial" Font-Size="8pt" BackColor="#EBEBEB"
                GridLines="Both" CellPadding="0" CellSpacing="0" BorderColor="#DADADA" BorderStyle="Solid">
                <asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue"
                    Font-Size="10pt" Font-Names="Arial" Font-Bold="True">
                    <asp:TableCell Width="90px" Text="Дата"></asp:TableCell>
                    <asp:TableCell Width="90px" Text="Оператор"></asp:TableCell>
                    <asp:TableCell Width="120px" Text="Клиент"></asp:TableCell>
                    <asp:TableCell Width="100px" Text="Регион"></asp:TableCell>
                    <asp:TableCell Text="Сообщение"></asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </p>
    </form>
</body>
</html>
