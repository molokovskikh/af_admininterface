<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeBehind="EditAdministrator.aspx.cs"
	Inherits="EditAdministrator" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
<form id="form1" runat="server">
		<div class="DetailsForm">
			<h3>
				�������������
			</h3>
			<div>
				<label class="LabelForInput" for="Login">
					��� ������������:
				</label>
				<asp:TextBox ID="Login" Text='<%# _current.UserName %>' runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="Login"
					ErrorMessage="*"></asp:RequiredFieldValidator>
			</div>
			<div>
				<label class="LabelForInput" for="FIO">
					���:</label>
				<asp:TextBox ID="FIO" Text='<%# _current.ManagerName %>' runat="server"></asp:TextBox>
			</div>
			<div>
				<label class="LabelForInput" for="Phone">
					�������:
				</label>
				<asp:TextBox ID="Phone" Text='<%# _current.PhoneSupport %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="PhoneValidator" runat="server" ControlToValidate="Phone"
					ErrorMessage='���� "�������" ������ ���� ���������� ��� "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="Email">
					E-mail:
				</label>
				<asp:TextBox ID="Email" Text='<%# _current.Email %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="EmailValidator" runat="server" ControlToValidate="Email"
					ErrorMessage="���� E-mail ���������� �� ��������" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
			</div>
			<fieldset>
				<legend>�������:</legend>
				<asp:CheckBoxList ID="RegionSelector" runat="server" DataTextField="Name"
					DataValueField="Id">
				</asp:CheckBoxList>
			</fieldset>
			<fieldset>
				<legend>�����:</legend>
				<asp:CheckBoxList ID="PermissionSelector" runat="server" DataTextField="Name"
					DataValueField="Type">
				</asp:CheckBoxList>
			</fieldset>
			<div class="Buttons">
				<asp:Button runat="server" ID="Save" Text="���������" OnClick="Save_Click" />
				<asp:Button runat="server" ID="Cancel" Text="��������" OnClick="Cancel_Click" />
			</div>
		</div>
	</form>
</asp:Content>