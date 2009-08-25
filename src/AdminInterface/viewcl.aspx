<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.viewcl" CodeBehind="viewcl.aspx.cs" Theme="Main" MasterPageFile="~/Main.Master" %>
<%@ Import Namespace="AdminInterface.Helpers"%>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
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
                    <asp:GridView ID="CLList" runat="server" OnRowDataBound="RowDataBound"
			CellPadding="0" AutoGenerateColumns="False" 
			AllowSorting="True" DataSource='<%# StatisticsDataView %>' onrowcreated="CLList_RowCreated" 
			onsorting="CLList_Sorting" >
                        <Columns>
							<asp:BoundField DataField="RequestTime" HeaderText="Время" SortExpression="RequestTime" />
							<asp:HyperLinkField DataNavigateUrlFields="FirmCode" DataNavigateUrlFormatString="Client/{0}" DataTextField="ShortName" HeaderText="Клиент" SortExpression="ShortName" />
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
</asp:Content>
