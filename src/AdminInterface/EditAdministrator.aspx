<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeBehind="EditAdministrator.aspx.cs"
	Inherits="EditAdministrator" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
<form id="form1" runat="server">
		<div class="DetailsForm">
			<h3>
				Администратор
			</h3>
			<div>
				<label class="LabelForInput" for="Login">
					Имя пользователя:
				</label>
				<asp:TextBox ID="Login" Text='<%# _current.UserName %>' runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="Login"
					ErrorMessage="*"></asp:RequiredFieldValidator>
			</div>
			<div>
				<label class="LabelForInput" for="FIO">
					ФИО:</label>
				<asp:TextBox ID="FIO" Text='<%# _current.ManagerName %>' runat="server"></asp:TextBox>
			</div>
			<div>
				<label class="LabelForInput" for="Phone">
					Телефон:
				</label>
				<asp:TextBox ID="Phone" Text='<%# _current.PhoneSupport %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="PhoneValidator" runat="server" ControlToValidate="Phone"
					ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="Email">
					E-mail:
				</label>
				<asp:TextBox ID="Email" Text='<%# _current.Email %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="EmailValidator" runat="server" ControlToValidate="Email"
					ErrorMessage="Поле E-mail заполненно не коректно" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
			</div>
			<fieldset>
				<legend>Регионы:</legend>
				<asp:CheckBoxList ID="RegionSelector" runat="server" DataTextField="Name"
					DataValueField="Id">
				</asp:CheckBoxList>
			</fieldset>
			<fieldset>
				<legend>Права:</legend>
				<asp:CheckBoxList ID="PermissionSelector" runat="server" DataTextField="Name"
					DataValueField="Type">
				</asp:CheckBoxList>
			</fieldset>
			<div class="Buttons">
				<asp:Button runat="server" ID="Save" Text="Сохранить" OnClick="Save_Click" />
				<asp:Button runat="server" ID="Cancel" Text="Отменить" OnClick="Cancel_Click" />
			</div>
		</div>
	</form>
</asp:Content>