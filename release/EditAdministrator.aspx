<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeBehind="EditAdministrator.aspx.cs"
	Inherits="EditAdministrator" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
<form id="form1" runat="server">
		<div class="DetailsForm">
			<h3>
				Администратор</h3>
			<div>
				<label class="LabelForInput" for="Login">
					Login:</label>
				<asp:TextBox ID="Login" Text='<%# _current.Login %>' runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="Login"
					ErrorMessage="*"></asp:RequiredFieldValidator>
				<asp:CustomValidator ID="LoginValidator" runat="server" ControlToValidate="Login"
					ErrorMessage="Клиент для данного имени пользователя не зарегестрирован" OnServerValidate="LoginValidator_ServerValidate"></asp:CustomValidator></div>
			<div>
				<label class="LabelForInput" for="FIO">
					ФИО:</label>
				<asp:TextBox ID="FIO" Text='<%# _current.FIO %>' runat="server"></asp:TextBox>
			</div>
			<div>
				<label class="LabelForInput" for="Phone">
					Телефон:</label>
				<asp:TextBox ID="Phone" Text='<%# _current.Phone %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="PhoneValidator" runat="server" ControlToValidate="Phone"
					ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="Email">
					E-mail:</label>
				<asp:TextBox ID="Email" Text='<%# _current.Email %>' runat="server"></asp:TextBox>
				<asp:RegularExpressionValidator ID="EmailValidator" runat="server" ControlToValidate="Email"
					ErrorMessage="Поле E-mail заполненно не коректно" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator></div>
			<div>
				<label class="LabelForInput" for="DefaultSegment">
					Сегмент:</label>
				<asp:DropDownList SelectedIndex='<%# _current.DefaultSegment %>' ID="DefaultSegment"
					runat="server">
					<asp:ListItem Value="0" Selected="True">Опт</asp:ListItem>
					<asp:ListItem Value="1">Розница</asp:ListItem>
				</asp:DropDownList>
			</div>
			<fieldset>
				<legend>Регионы:</legend>
				<asp:CheckBoxList ID="Regions" runat="server" DataSourceID="RegionsDataSource" DataTextField="RegionName"
					DataValueField="RegionCode">
				</asp:CheckBoxList>
			</fieldset>
			<fieldset>
				<legend>Права:</legend>
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowManageAdminAccounts %>' ID="AllowManageAdminAccounts"
					runat="server" Text="Разрешить управлять региональными администраторами" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateRetail %>'
					ID="AllowCreateRetail" runat="server" Text="Разрешить регистрировать аптеки" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateVendor %>'
					ID="AllowCreateVendor" runat="server" Text="Разрешить создавать поставщиков" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowRetailInterface %>'
					ID="AllowRetailInterface" runat="server" Text="Разрешить управление интерфейсом аптеки" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowVendorInterface %>'
					ID="AllowVendorInterface" runat="server" Text="Разрешить управление интерфейсом поставщика" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowChangeSegment %>'
					ID="AllowChangeSegment" runat="server" Text="Разрешить изменение сегмента" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowManage %>' ID="AllowManage"
					runat="server" Text="Разрешить управление клиентом" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowClone %>' ID="AllowClone"
					runat="server" Text="Разрешить клонирование" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowChangePassword %>'
					ID="AllowChangePassword" runat="server" Text="Разрешить измененять пароль" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.AllowCreateInvisible %>'
					ID="AllowCreateInvisible" runat="server" Text="Разрешить создавать невидимых клиентов" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.SendAlert %>' ID="SendAlert"
					runat="server" Text="Посылать оповещения на E-mail" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.UseRegistrant %>' ID="UseRegistrant"
					runat="server" Text="Использовать информацию о регистраторе при управлении клиентом" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.ShowRetail %>' ID="ShowRetail"
					runat="server" Text="Показывать аптеки" />
				<asp:CheckBox CssClass="InputCheckBox" Checked='<%# _current.ShowVendor %>' ID="ShowVendor"
					runat="server" Text="Показывать поставщиков" />

			</fieldset>
			<div class="Buttons">
				<asp:Button runat="server" ID="Save" Text="Сохранить" OnClick="Save_Click" />
				<asp:Button runat="server" ID="Cancel" Text="Отменить" OnClick="Cancel_Click" />
				<asp:ObjectDataSource ID="RegionsDataSource" runat="server" SelectMethod="GetRegionList"
					TypeName="DAL.CommandFactory"></asp:ObjectDataSource>
			</div>
		</div>
	</form>
</asp:Content>