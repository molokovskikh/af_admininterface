<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.managep" CodeFile="managep.aspx.cs"
	Theme="Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>������������ ������������</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
</head>
<body>
	<form id="Form1" method="post" runat="server">
		<div class="MainBlock">
			<h4 class="MainHeader">
				<asp:Label ID="HeaderLabel" runat="server" />
			</h4>
			<div class="BorderedBlock">
				<h3 class="Header">
					����� �����
				</h3>
				<div class="ContentBlock">
					<asp:GridView ID="PricesGrid" runat="server" AutoGenerateColumns="False" DataMember="Prices"
						OnRowCommand="PricesGrid_RowCommand" OnRowDeleting="PricesGrid_RowDeleting">
						<Columns>
							<asp:TemplateField>
								<HeaderTemplate>
									<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="��������" />
								</HeaderTemplate>
								<ItemTemplate>
									<asp:Button ID="DeleteButton" runat="server" CommandName="Delete" Text="�������" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:HyperLinkField HeaderText="������������" DataTextField="PriceName" DataNavigateUrlFormatString="managecosts.aspx?pc={0}"
								DataNavigateUrlFields="PriceCode" />
							<asp:BoundField HeaderText="�������" DataField="DateCurPrice" />
							<asp:BoundField HeaderText="�������������" DataField="DateLastForm" />
							<asp:TemplateField HeaderText="�������">
								<ItemTemplate>
									<asp:TextBox ID="UpCostText" runat="server" Text='<%# Eval("UpCost") %>' />
									<asp:RegularExpressionValidator ID="UpCostValidator" runat="server" ErrorMessage="*"
										ValidationExpression="^([-+])?\d+(\,\d+)?$" ControlToValidate="UpCostText"></asp:RegularExpressionValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="��� ������">
								<ItemTemplate>
									<asp:DropDownList ID="PriceTypeList" runat="server" DataValueField='<%# Eval("PriceType") %>'>
										<asp:ListItem Value="0">�������</asp:ListItem>
										<asp:ListItem Value="1">��������������</asp:ListItem>
									</asp:DropDownList>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="���.">
								<ItemTemplate>
									<asp:CheckBox ID="EnableCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("AgencyEnabled")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="� ������">
								<ItemTemplate>
									<asp:CheckBox ID="InWorkCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="������.">
								<ItemTemplate>
									<asp:CheckBox ID="IntegratedCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("AlowInt")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
						</Columns>
					</asp:GridView>
				</div>
			</div>
			<div class="BorderedBlock">
				<h3 class="Header">
					������������ ���������
				</h3>
				<div class="ContentBlock">
					<label for="RegionDD">
						�������� ������:
					</label>
					<asp:DropDownList ID="HomeRegion" runat="server" DataTextField="Region" DataMember="Regions"
						DataValueField="RegionCode">
					</asp:DropDownList>
					<asp:CheckBox ID="ShowAllRegionsCheck" runat="server" Text="���������� ��� �������." AutoPostBack="True" OnCheckedChanged="ShowAllRegionsCheck_CheckedChanged" />
				</div>
				<div class="ContentBlock">
					<label for="WRList">
						��������� �������:
					</label>
					<asp:CheckBoxList ID="WorkRegionList" runat="server" BorderStyle="None" DataMember="EnableRegions"
						DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
					</asp:CheckBoxList>
				</div>
				<div class="ContentBlock">
					<asp:GridView ID="RegionalSettingsGrid" runat="server" AutoGenerateColumns="False"
						DataMember="RegionSettings" OnRowCreated="RegionalSettingsGrid_RowCreated">
						<Columns>
							<asp:BoundField DataField="Region" HeaderText="������" />
							<asp:TemplateField HeaderText="�������">
								<ItemTemplate>
									<asp:CheckBox ID="EnabledCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="�����">
								<ItemTemplate>
									<asp:CheckBox ID="StorageCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("Storage")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Email ��������������">
								<ItemTemplate>
									<asp:TextBox ID="AdministratorEmailText" runat="server" Text='<%# Eval("AdminMail") %>' />
									<asp:RegularExpressionValidator ID="AdministratorEmailValidator" runat="server" ControlToValidate="AdministratorEmailText"
										ErrorMessage="*" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Email � �������">
								<ItemTemplate>
									<asp:TextBox ID="RegionalEmailText" runat="server" Text='<%# Eval("TmpMail") %>' />
									<asp:RegularExpressionValidator ID="RegionalEmailValidator" runat="server" ControlToValidate="RegionalEmailText"
										ErrorMessage="*" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="������� � �������">
								<ItemTemplate>
									<asp:TextBox ID="SupportPhoneText" runat="server" Text='<%# Eval("SupportPhone") %>' />
									<asp:RegularExpressionValidator ID="PhoneValidator" runat="server" ControlToValidate="SupportPhoneText"
										ErrorMessage="*" ValidationExpression="(\d{3,4})-(\d{6,7})"></asp:RegularExpressionValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:HyperLinkField HeaderText="����������" Text="����������" DataNavigateUrlFormatString="EditRegionalInfo.aspx?id={0}"
								DataNavigateUrlFields="RowID" />
						</Columns>
					</asp:GridView>
				</div>
			</div>
			<div class="Submit">
				<asp:Button ID="SaveButton" runat="server" Text="���������" OnClick="SaveButton_Click" />
			</div>
			<div class="CopyRight">
				� �� <a href="http://www.analit.net/">"�������"</a> 2004
			</div>
		</div>
	</form>
</body>
</html>
