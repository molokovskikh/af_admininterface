<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.viewcl" CodeBehind="viewcl.aspx.cs" Theme="Main" %>
<%@ Import Namespace="AdminInterface.Helpers"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Интерфейс управления клиентами</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
    <link rel="stylesheet" type="text/css" href="~/Css/Table.css" />
	<link rel="stylesheet" type="text/css" href="~/Css/Contacts.css" />
	<link rel="stylesheet" type="text/css" href="~/Css/Billing.css" />
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
                    &nbsp;</td>
            </tr>
        </table>
                    <asp:GridView ID="CLList" runat="server" 
			CellPadding="0" AutoGenerateColumns="False" 
			AllowSorting="True" DataSource='<%# StatisticsDataView %>' onrowcreated="CLList_RowCreated" 
			onsorting="CLList_Sorting" >
                        <Columns>
							<asp:BoundField DataField="RequestTime" HeaderText="Время" SortExpression="RequestTime" />
							<asp:HyperLinkField DataNavigateUrlFields="FirmCode" DataNavigateUrlFormatString="Client/info.rails?cc={0}" DataTextField="ShortName" HeaderText="Клиент" SortExpression="ShortName" />
							<asp:BoundField DataField="Region" HeaderText="Регион" SortExpression="Region" />
							<asp:BoundField DataField="AppVersion" HeaderText="Версия EXE" SortExpression="AppVersion" />
							<asp:TemplateField HeaderText="Размер приготовленных данных" SortExpression="ResultSize">
								<ItemTemplate>
								<%# ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(Eval("ResultSize"))) %>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:BoundField DataField="Addition" HeaderText="Описание" />
                        </Columns>
                    </asp:GridView>
    </form>
</body>
</html>
