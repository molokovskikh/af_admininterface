<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.viewcl" CodeFile="viewcl.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Интерфейс управления клиентами</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="95%" align="center" border="0">
            <tr>
                <td align="center">
                    <asp:Label ID="HeaderLB" runat="server" Font-Names="Verdana" Font-Size="10pt" Font-Bold="True"></asp:Label></td>
            </tr>
            <tr>
                <td align="center">
                    <font face="Verdana" size="2">Клиентов, отвечающих условиям выборки:</font>
                    <asp:Label ID="CountLB" runat="server" Font-Names="Verdana" Font-Size="8pt"></asp:Label></td>
            </tr>
            <tr>
                <td align="center">
                    <asp:DataGrid ID="CLList" runat="server" Font-Names="Verdana" Font-Size="8pt" CellPadding="0"
                        AutoGenerateColumns="False" BorderColor="#DADADA" PageSize="20" DataSource="<%# DataTable1 %>">
                        <FooterStyle Font-Names="Verdana"></FooterStyle>
                        <AlternatingItemStyle BackColor="#F6F6F6"></AlternatingItemStyle>
                        <ItemStyle HorizontalAlign="Center" BackColor="#EEF8FF"></ItemStyle>
                        <HeaderStyle Font-Size="8pt" Font-Names="Verdana" Font-Bold="True" HorizontalAlign="Center"
                            VerticalAlign="Middle" BackColor="#EBEBEB"></HeaderStyle>
                        <Columns>
                            <asp:TemplateColumn HeaderText="Время">
                                <ItemTemplate>
                                    &nbsp;
                                    <asp:Label runat="server" Text='<%# Convert.ToDateTime(DataBinder.Eval(Container, "DataItem.LogTime")).ToString() %>'>
                                    </asp:Label>&nbsp;
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.LogTime") %>'>
                                    </asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateColumn>
                            <asp:HyperLinkColumn DataNavigateUrlField="FirmCode" DataNavigateUrlFormatString="Client/info.rails?cc={0}"
                                DataTextField="ShortName" HeaderText="Клиент"></asp:HyperLinkColumn>
                            <asp:BoundColumn DataField="Region" SortExpression="Region" HeaderText="Регион"></asp:BoundColumn>
                            <asp:BoundColumn DataField="Addition" HeaderText="Описание"></asp:BoundColumn>
                        </Columns>
                        <PagerStyle VerticalAlign="Middle" Font-Size="9pt" Font-Names="Verdana" Font-Bold="True"
                            HorizontalAlign="Center" Position="TopAndBottom" Mode="NumericPages"></PagerStyle>
                    </asp:DataGrid></td>
            </tr>
        </table>
    </form>
</body>
</html>
