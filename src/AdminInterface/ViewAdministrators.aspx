<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeFile="ViewAdministrators.aspx.cs"
	Inherits="ViewAdministrators" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>���������� ������������� ����������������</title>
	<script type="text/javascript" language="javascript" src="./JavaScript/Main.js" ></script>
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<h3>������������ ��������������</h3>
			<asp:ObjectDataSource ID="ObjectDataSource1" runat="server"
				SelectMethod="GetAdministratorList" TypeName="DAL.CommandFactory" DataObjectTypeName="DAL.Administrator" DeleteMethod="DeleteAdministrator" ConflictDetection="CompareAllValues"></asp:ObjectDataSource>
			<asp:GridView ID="Administrators" runat="server" AutoGenerateColumns="False" DataSourceID="ObjectDataSource1" OnRowCommand="Administrators_RowCommand" OnRowCreated="Administrators_RowCreated" DataKeyNames="ID">
				<Columns>
					<asp:TemplateField ShowHeader="False">
						<HeaderTemplate>
							<asp:Button ID="Button2" runat="server" Text="�������" CausesValidation="False" CommandName="Create" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Button ID="Button1" CommandArgument='<%# Bind("ID")%>' runat="server" CausesValidation="False" CommandName="Delete"
								Text="�������" />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Left" />
						<HeaderStyle HorizontalAlign="Left" />
					</asp:TemplateField>
					<asp:BoundField DataField="ID" HeaderText="AdministratorID" SortExpression="ID" Visible="False" />
					<asp:BoundField DataField="FIO" HeaderText="���" SortExpression="FIO" />
					<asp:TemplateField HeaderText="Login" SortExpression="Login">
						<ItemTemplate>
							<asp:LinkButton CommandArgument='<%# Bind("ID")%>' ID="LinkButton1" Text='<%# Bind("Login")%>' runat="server" CommandName="Edit">LinkButton</asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="Email" HeaderText="E-mail" SortExpression="Email" />
					<asp:BoundField DataField="Phone" HeaderText="�������" SortExpression="Phone" />
					<asp:CheckBoxField DataField="AllowManageAdminAccounts" HeaderText="MAC" SortExpression="AllowManageAdminAccounts" />
					<asp:CheckBoxField DataField="AllowCreateRetail" HeaderText="CR" SortExpression="AllowCreateRetail" />
					<asp:CheckBoxField DataField="AllowRetailInterface" HeaderText="RI" SortExpression="AllowRetailInterface" />
					<asp:CheckBoxField DataField="AllowCreateVendor" HeaderText="CV" SortExpression="AllowCreateVendor" />
					<asp:CheckBoxField DataField="AllowVendorInterface" HeaderText="VI" SortExpression="AllowVendorInterface" />
					<asp:CheckBoxField DataField="AllowCreateInvisible" HeaderText="CI" SortExpression="AllowCreateInvisible" />
					<asp:CheckBoxField DataField="UseRegistrant" HeaderText="UR" SortExpression="UseRegistrant" />
					<asp:CheckBoxField DataField="AllowChangeSegment" HeaderText="CS" SortExpression="AllowChangeSegment" />
					<asp:CheckBoxField DataField="SendAlert" HeaderText="SA" SortExpression="SendAlert" />
					<asp:CheckBoxField DataField="AllowChangePassword" HeaderText="CP" SortExpression="AllowChangePassword" />
					<asp:CheckBoxField DataField="AllowManage" HeaderText="M" SortExpression="AllowManage" />
					<asp:CheckBoxField DataField="AllowClone" HeaderText="C" SortExpression="AllowClone" />
					<asp:CheckBoxField DataField="ShowRetail" HeaderText="SR" SortExpression="ShowRetail" />
					<asp:CheckBoxField DataField="ShowVendor" HeaderText="SV" SortExpression="ShowVendor" />
				</Columns>
			</asp:GridView>
			<ul class="Legend">
				<li>MAC - ��������� ��������� ���������� ������������ ���������������</li>
				<li>CR - ��������� �������������� ������</li>
				<li>RI - ��������� ���������� ����������� ������</li>
				<li>CV - ��������� ��������� �����������</li>
				<li>VI - ��������� ���������� ����������� ����������</li>
				<li>CI - ��������� ��������� ��������� ��������</li>
				<li>UR - ������������ ���������� � ������������ ��� ���������� ��������</li>
				<li>CS - ��������� ��������� ��������</li>
				<li>SA - �������� ���������� �� E-mail</li>
				<li>CP - ��������� ���������� ������</li>
				<li>M - ��������� ���������� ��������</li>
				<li>C - ��������� ������������</li>
				<li>SR - ���������� ������</li>
				<li>SV - ���������� �����������</li>
			</ul>
		</div>
	</form>
</body>
</html>
