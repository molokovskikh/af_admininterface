<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeFile="EditAdministrator.aspx.cs"
	Inherits="EditAdministrator" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>�������������� ������������� ��������������</title>
</head>
<body>
	<form id="form1" runat="server">
		<div class="DetailsForm">
			<h3>
				�������������</h3>
			<div>
				<label class="LabelForInput" for="Login">
					Login:</label>
				<asp:TextBox ID="Login" Text='<%# _current.Login %>' runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="Login"
					ErrorMessage="*"></asp:RequiredFieldValidator></div>
			<div>
				<label class="LabelForInput" for="FIO">
					���:</label>
				<asp:TextBox ID="FIO" Text='<%# _current.FIO %>' runat="server"></asp:TextBox>
			</div>
			<div>
				<label class="LabelForInput" for="Phone">
					�������:</label>
				<asp:TextBox ID="Phone" Text='<%# _current.Phone %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="PhoneValidator" runat="server" ControlToValidate="Phone"
					ErrorMessage='���� "�������" ������ ���� ���������� ��� "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="Email">
					E-mail:</label>
				<asp:TextBox ID="Email" Text='<%# _current.Email %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="EmailValidator" runat="server" ControlToValidate="Email"
					ErrorMessage="���� E-mail ���������� �� ��������" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="DefaultSegment">
					�������:</label>
				<asp:DropDownList SelectedIndex='<%# _current.DefaultSegment %>' ID="DefaultSegment"
					runat="server">
					<asp:ListItem Value="0" Selected="True">���</asp:ListItem>
					<asp:ListItem Value="1">�������</asp:ListItem>
				</asp:DropDownList>
			</div>
			<fieldset>
				<legend>�������:</legend>
				<asp:CheckBoxList ID="Regions" runat="server" DataSourceID="RegionsDataSource" DataTextField="RegionName"
					DataValueField="RegionCode">
				</asp:CheckBoxList>
			</fieldset>
			<fieldset>
				<legend>�����:</legend>
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowManageAdminAccounts %>' ID="AllowManageAdminAccounts"
					runat="server" Text="��������� ��������� ������������� ����������������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateRetail %>'
					ID="AllowCreateRetail" runat="server" Text="��������� �������������� ������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateVendor %>'
					ID="AllowCreateVendor" runat="server" Text="��������� ��������� �����������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowRetailInterface %>'
					ID="AllowRetailInterface" runat="server" Text="��������� ���������� ����������� ������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowVendorInterface %>'
					ID="AllowVendorInterface" runat="server" Text="��������� ���������� ����������� ����������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowChangeSegment %>'
					ID="AllowChangeSegment" runat="server" Text="��������� ��������� ��������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowManage %>' ID="AllowManage"
					runat="server" Text="��������� ���������� ��������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowClone %>' ID="AllowClone"
					runat="server" Text="��������� ������������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowChangePassword %>'
					ID="AllowChangePassword" runat="server" Text="��������� ���������� ������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateInvisible %>'
					ID="AllowCreateInvisible" runat="server" Text="��������� ��������� ��������� ��������" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.SendAlert %>' ID="SendAlert"
					runat="server" Text="�������� ���������� �� E-mail" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.UseRegistrant %>' ID="UseRegistrant"
					runat="server" Text="������������ ���������� � ������������ ��� ���������� ��������" />
			</fieldset>
			<div class="Buttons">
				<asp:Button runat="server" ID="Save" Text="���������" OnClick="Save_Click" />
				<asp:Button runat="server" ID="Cancel" Text="��������" OnClick="Cancel_Click" />
				<asp:ObjectDataSource ID="RegionsDataSource" runat="server" SelectMethod="GetRegionList"
					TypeName="DAL.CommandFactory"></asp:ObjectDataSource>
			</div>
		</div>
	</form>
</body>
</html>
