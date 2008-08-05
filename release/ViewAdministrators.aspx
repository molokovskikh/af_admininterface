
<%@ Page Theme="Main" Language="C#" AutoEventWireup="true" CodeBehind="ViewAdministrators.aspx.cs" MasterPageFile="~/Main.Master"
	Inherits="ViewAdministrators" %>
<%@ Import Namespace="AdminInterface.Models.Security"%>


<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server">
		<div>
			<h3>Пользователи офиса</h3>
			<asp:GridView ID="Administrators" runat="server" AutoGenerateColumns="False" OnRowCommand="Administrators_RowCommand" CssClass="HighLightCurrentRow" DataKeyNames="ID">
				<Columns>
					<asp:TemplateField ShowHeader="False">
						<HeaderTemplate>
							<asp:Button ID="Button2" runat="server" Text="Создать" CausesValidation="False" CommandName="Create" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Button ID="Button1" CommandArgument='<%# Bind("Id")%>' runat="server" CausesValidation="False" CommandName="Del"
								Text="Удалить" Visible='<%# GetDeleteBlockButtonVisibiliti((Eval("UserName").ToString())) %>' />
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
					<asp:BoundField DataField="ManagerName" HeaderText="ФИО" />
					<asp:TemplateField HeaderText="Login" SortExpression="Login">
						<ItemTemplate>
							<asp:LinkButton CommandArgument='<%# Bind("Id")%>' ID="LinkButton1" Text='<%# Bind("UserName")%>' runat="server" CommandName="Edit">LinkButton</asp:LinkButton>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="Email" HeaderText="E-mail" SortExpression="Email" />
					<asp:BoundField DataField="PhoneSupport" HeaderText="Телефон" SortExpression="Phone" />
				
					<asp:TemplateField HeaderText="VD">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox1" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ViewDrugstore) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="MD">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox2" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ManageDrugstore) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="DI">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox3" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.DrugstoreInterface) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="RD">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox4" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.RegisterDrugstore) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="VS">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox5" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ViewSuppliers) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="MS">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox6" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ManageSuppliers) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="SI">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox7" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.SupplierInterface) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="RS">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox8" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.RegisterSupplier) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="MA">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox9" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ManageAdministrators) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="B">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox10" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.Billing) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="MU">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox11" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.MonitorUpdates) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="CI">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox12" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.RegisterInvisible) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="CP">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox13" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.ChangePassword) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="CS">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox14" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.CopySynonyms) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="RF">
						<ItemTemplate>
							<asp:CheckBox ID="CheckBox15" Enabled="false" runat="server" 
										  Checked='<%# ((Administrator)Container.DataItem).HavePermisions(PermissionType.CanRegisterClientWhoWorkForFree) %>' />
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
			</asp:GridView>
			<ul class="Legend">			
				<li><%# GetPermissionShortcut(PermissionType.ViewDrugstore) %> - <%# GetPermissionName(PermissionType.ViewDrugstore)%></li>
				<li><%# GetPermissionShortcut(PermissionType.RegisterDrugstore) %> - <%# GetPermissionName(PermissionType.RegisterDrugstore)%></li>
				<li><%# GetPermissionShortcut(PermissionType.ManageDrugstore) %> - <%# GetPermissionName(PermissionType.ManageDrugstore)%></li>
				<li><%# GetPermissionShortcut(PermissionType.DrugstoreInterface) %> - <%# GetPermissionName(PermissionType.DrugstoreInterface)%></li>


				<li><%# GetPermissionShortcut(PermissionType.ViewSuppliers) %> - <%# GetPermissionName(PermissionType.ViewSuppliers)%></li>
				<li><%# GetPermissionShortcut(PermissionType.RegisterSupplier) %> - <%# GetPermissionName(PermissionType.RegisterSupplier)%></li>
				<li><%# GetPermissionShortcut(PermissionType.ManageSuppliers) %> - <%# GetPermissionName(PermissionType.ManageSuppliers)%></li>
				<li><%# GetPermissionShortcut(PermissionType.SupplierInterface) %> - <%# GetPermissionName(PermissionType.SupplierInterface)%></li>

				<li><%# GetPermissionShortcut(PermissionType.ManageAdministrators) %> - <%# GetPermissionName(PermissionType.ManageAdministrators)%></li>
				<li><%# GetPermissionShortcut(PermissionType.Billing) %> - <%# GetPermissionName(PermissionType.Billing)%></li>
				<li><%# GetPermissionShortcut(PermissionType.MonitorUpdates) %> - <%# GetPermissionName(PermissionType.MonitorUpdates)%></li>
				<li><%# GetPermissionShortcut(PermissionType.RegisterInvisible) %> - <%# GetPermissionName(PermissionType.RegisterInvisible)%></li>
				<li><%# GetPermissionShortcut(PermissionType.SendNotification) %> - <%# GetPermissionName(PermissionType.SendNotification)%></li>
				<li><%# GetPermissionShortcut(PermissionType.ChangePassword) %> - <%# GetPermissionName(PermissionType.ChangePassword)%></li>
				<li><%# GetPermissionShortcut(PermissionType.CopySynonyms) %> - <%# GetPermissionName(PermissionType.CopySynonyms)%></li>				
				<li><%# GetPermissionShortcut(PermissionType.CanRegisterClientWhoWorkForFree)%> - <%# GetPermissionName(PermissionType.CanRegisterClientWhoWorkForFree)%></li>
			</ul>
		</div>
	</form>
</asp:Content>