<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.managep" CodeBehind="managep.aspx.cs"
	Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
<form id="form1" runat="server">
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
							<asp:TemplateField  HeaderText="������������">
								<ItemTemplate>
									<asp:HyperLink runat="server" Text='<%# Eval("PriceName") %>' NavigateUrl='<%# Eval("CostType").Equals(DBNull.Value) ? "" : String.Format("managecosts.aspx?pc={0}", Eval("PriceCode")) %>'  />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField  HeaderText="���� ����� �����">
								<ItemTemplate>
									<asp:Label runat="server"><%# Eval("CostType").Equals(1) || Eval("CostType").Equals(DBNull.Value) ? "-" : Eval("PriceDate") %></asp:Label>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="�������">
								<ItemTemplate>
									<asp:TextBox ID="UpCostText" runat="server" Text='<%# Eval("UpCost") %>' />
									<asp:RegularExpressionValidator ID="UpCostValidator" runat="server" ErrorMessage="*"
										ValidationExpression="^([-+])?\d+(\,\d+)?$" ControlToValidate="UpCostText"></asp:RegularExpressionValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="��� ������� �������">
								<ItemTemplate>
									<asp:DropDownList ID="CostType" runat="server" SelectedValue='<%# Eval("CostType") %>' DataSource='<%# GetCostTypeSource(Eval("CostType")) %>' DataTextField="Value" DataValueField="Key" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="��� ������">
								<ItemTemplate>
									<asp:DropDownList ID="PriceTypeList" runat="server" SelectedValue='<%# Eval("PriceType") %>'>
										<asp:ListItem Value="0">�������</asp:ListItem>
										<asp:ListItem Value="1">��������������</asp:ListItem>
										<asp:ListItem Value="2">VIP</asp:ListItem>
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
						<EmptyDataTemplate>
							<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="�������� ����� ����" />
						</EmptyDataTemplate>
					</asp:GridView>
				</div>
			</div>
			<div class="BorderedBlock">
				<h3 class="Header">
					������������ �������
				</h3>
				<div class="ContentBlock">
					<asp:GridView ID="ShowClientsGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="ShowClientsGrid_RowCommand" DataMember="ShowClients"
						OnRowDataBound="ShowClientsGrid_RowDataBound" OnRowDeleting="ShowClientsGrid_RowDeleting">
						<EmptyDataTemplate>
							<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="�������� �������" />
						</EmptyDataTemplate>
						<Columns>
							<asp:TemplateField>
								<HeaderTemplate>
									<asp:Button ID="AddButton" CommandName="Add" runat="server" Text="��������" />
								</HeaderTemplate>
								<ItemTemplate>
									<asp:Button ID="DeleteButton" CommandName="Delete" runat="server" Text="�������" />
								</ItemTemplate>
								<ItemStyle Width="10%" />
							</asp:TemplateField>
							<asp:TemplateField HeaderText="������">
								<ItemTemplate>
									<asp:TextBox ID="SearchText" runat="server" />
									<asp:Button ID="SearchButton" CommandName="Search" runat="server" Text="�����" ValidationGroup="3" />
									<asp:DropDownList ID="ShowClientsList" runat="server" DataTextField="ShortName" DataValueField="FirmCode">
									</asp:DropDownList>
									<asp:CustomValidator ID="ShowCleintsValidator" runat="server" ControlToValidate="ShowClientsList"
										ErrorMessage="���������� ������� �������." OnServerValidate="ParentValidator_ServerValidate"
										ValidateEmptyText="True" ValidationGroup="1">*</asp:CustomValidator>
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="��� ������������� �������">
								<ItemTemplate>
									<asp:DropDownList ID="ShowType" runat="server" SelectedValue='<%# Eval("ShowType") %>'>
										<asp:ListItem Value="0">�������</asp:ListItem>
										<asp:ListItem Value="1">������</asp:ListItem>
										<asp:ListItem Value="2">���</asp:ListItem>
									</asp:DropDownList>
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
			<div class="BorderedBlock">
				<h3 class="Header">
					��������� �������� �������
				</h3>
				<div class="ContentBlock">
					<asp:GridView id="OrderSendRules" runat="server" AutoGenerateColumns="false" 
						DataMember="OrderSendConfig"
						onrowcommand="OrderSettings_RowCommand" 
						onrowdeleting="OrderSettings_RowDeleting" onrowdatabound="OrderSettings_RowDataBound">
						<EmptyDataTemplate>
							<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="�������� ��������� �������� ������" />
						</EmptyDataTemplate>
						<Columns>
							<asp:TemplateField>
								<HeaderTemplate>
									<asp:Button ID="AddButton" CommandName="Add" runat="server" Text="��������" />
								</HeaderTemplate>
								<ItemTemplate>
									<asp:Button ID="DeleteButton" CommandName="Delete" runat="server" Text="�������" />
								</ItemTemplate>
								<ItemStyle Width="10%" />
							</asp:TemplateField>
							<asp:TemplateField HeaderText="���������">
								<ItemTemplate>
									<asp:DropDownList ID="Sender" runat="server" DataTextField="ClassName" DataValueField="Id" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="��������">
								<ItemTemplate>
									<asp:DropDownList ID="Formater" runat="server" DataTextField="ClassName" DataValueField="Id" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="������">
								<ItemTemplate>
									<asp:DropDownList ID="Region" runat="server" DataTextField="Region" DataValueField="RegionCode" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="���������� ����� ������ � tech@analit.net">
								<ItemTemplate>
									<asp:CheckBox ID="SendDebugMessage" runat="server" Checked='<%#  Convert.ToBoolean(Eval("SendDebugMessage")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="����������� �������� sms ���������� �� ������ �� �������� ����� ������">
								<ItemTemplate>
									<asp:TextBox ID="SmsSendDelay" runat="server" Text='<%# Eval("ErrorNotificationDelay")  %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:HyperLinkField Text="��������" DataNavigateUrlFields="id" DataNavigateUrlFormatString="~/SenderProperties.aspx?ruleid={0}" />
						</Columns>
					</asp:GridView>
				</div>
			</div>
			<div class="Submit">
				<asp:Button ID="SaveButton" runat="server" Text="���������" OnClick="SaveButton_Click" />
			</div>
		</div>
	</form>
</asp:Content>