
<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeBehind="ViewAdministrators.aspx.cs" MasterPageFile="~/Main.Master"
	Inherits="ViewAdministrators" %>
<%@ Import Namespace="AdminInterface.Models.Security"%>


<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server">
		<div>
			<h3>������������ �����</h3>
			<asp:GridView ID="Administrators" runat="server"  AllowSorting="True" OnSorting="SortRecords" AutoGenerateColumns="False" OnRowCommand="Administrators_RowCommand" CssClass="HighLightCurrentRow" DataKeyNames="ID">
				<Columns>
					<asp:TemplateField ShowHeader="False">
						<HeaderTemplate>
							<asp:Button ID="Button2" runat="server" Text="�������" CausesValidation="False" CommandName="Create" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Button ID="Button1" CommandArgument='<%# Bind("Id") %>' runat="server" CausesValidation="False" CommandName="Del"
								Text="�������" Visible='<%# GetDeleteBlockButtonVisibiliti((Eval("UserName").ToString())) %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Left" />
						<HeaderStyle HorizontalAlign="Left" />
					</asp:TemplateField>
					<asp:TemplateField>
						<ItemTemplate>
							<asp:Button ID="Button3" runat="server" CommandName='<%# GetButtonCommand((Eval("UserName").ToString())) %>' CausesValidation="False" Text='<%# GetButtonLabel(Eval("UserName").ToString()) %>' CommandArgument='<%# Bind("UserName") %>' Visible='<%# GetDeleteBlockButtonVisibiliti((Eval("UserName").ToString())) %>' />
						</ItemTemplate>
					</asp:TemplateField>					
					<asp:BoundField DataField="Id" HeaderText="AdministratorID" Visible="False" />
					<asp:BoundField DataField="ManagerName" HeaderText="���" SortExpression="ManagerName"/>
					<asp:TemplateField HeaderText="��� ������������" SortExpression="UserName">
						<ItemTemplate>
							<asp:LinkButton CommandArgument='<%# Bind("Id") %>' ID="LinkButton1" Text='<%# Bind("UserName") %>' runat="server" CommandName="Edit">LinkButton</asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="Email" HeaderText="E-mail" SortExpression="Email" />
					<asp:BoundField DataField="PhoneSupport" HeaderText="�������" SortExpression="PhoneSupport" />
				</Columns>
			</asp:GridView>
		</div>
	</form>
</asp:Content>