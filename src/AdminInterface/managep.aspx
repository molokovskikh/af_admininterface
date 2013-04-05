	<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.managep" CodeBehind="managep.aspx.cs"
	Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server">
		<asp:ScriptManager ID="ScriptManager1" runat="server" AjaxFrameworkMode="Disabled" EnableScriptLocalization="False" EnableScriptGlobalization="False" EnablePageMethods="False" EnablePartialRendering="False" EnableSecureHistoryState="False">
			<Scripts>
				<asp:ScriptReference Path="~/Assets/Javascripts/jquery-1.6.2.min.js" />
				<asp:ScriptReference Path="~/Assets/Javascripts/jquery-ui-1.10.2.min.js" />
			</Scripts>
		</asp:ScriptManager>

		<link rel="stylesheet" href="Assets/Stylesheets/themes/base/jquery-ui.css" type="text/css" />

		<script type="text/javascript">
			$(function () {
				$('.PriceTypeList').change(function () {
					$('#messageDiv').html('');
					$('#messageDiv').append("��� ������� ����� ��������� �� VIP �������");
					if ($(this).val() == 2) {
						$('#messageDiv').css("display", "block");
					} else {
						$('#messageDiv').css("display", "none");
					}
				});

				$(document).tooltip({
					hide: { effect: "explode", duration: 400 },
				});
			});
		</script>

		<div class="MainBlock">
			<h4 class="MainHeader">
				<asp:Label ID="HeaderLabel" runat="server" />
			</h4>
			<div runat="server" id="messageDiv" class="Warning" style="display: none; padding: 10px 10px 10px 50px;" ClientIDMode="Static"></div>
			<div class="block">
				<ul class="navigation-menu">
					<li><asp:HyperLink runat="server" ID="HandlersLink">��������� ���������� � ����������� ��������� ����������</asp:HyperLink></li>
					<li><asp:HyperLink runat="server" ID="WaybillExcludeFiles">�����, ����������� �� ������� � �������� ���������</asp:HyperLink></li>
					<li><asp:HyperLink runat="server" ID="WaybillSourceSettings">��������� �������� ����������</asp:HyperLink></li>
				</ul>
			</div>
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
									<asp:Button ID="DeleteButton" runat="server" Visible='<%# CanDelete(Eval("PriceCode")) %>' CommandName="Delete" Text="�������" />
									<asp:HyperLink ID="ToolTip" CssClass="ToolTip" title='<%# GetNoDeleteReason(Eval("PriceCode")) %>' runat="server" Visible='<%# !CanDelete(Eval("PriceCode")) %>' Text='���������' NavigateUrl='#' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField>
								<ItemTemplate>
									<asp:HyperLink runat="server" Text='�������������' NavigateUrl='<%# String.Format("~/Prices/{0}/Edit", Eval("PriceCode")) %>' />
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
									<asp:DropDownList ID="PriceTypeList" runat="server" SelectedValue='<%# Eval("PriceType") %>' ClientIDMode="Inherit" CssClass="PriceTypeList">
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
							<asp:TemplateField HeaderText="�������">
								<ItemTemplate>
									<asp:CheckBox ID="BuyingMatrix" runat="server" Checked='<%# Convert.ToBoolean(Eval("BuyingMatrix")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="���������">
								<ItemTemplate>
									<asp:CheckBox ID="IsLocal" ToolTip="������, ��������� �� �����-������, ����������, ��� &quot;���������&quot; �� ��������� � ���������� ������� �� ������ �������" runat="server" Checked='<%# Convert.ToBoolean(Eval("IsLocal")) %>' />
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
					<div>
						<asp:Label runat="server" ForeColor="Red" ID="RegionValidationError" Visible="False">�� �� ������� ������� ������</asp:Label>
					</div>
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
							<asp:TemplateField HeaderText="���������� ����� ������ � zakaz_copy@analit.net">
								<ItemTemplate>
									<asp:CheckBox ID="SendDebugMessage" runat="server" Checked='<%# Convert.ToBoolean(Eval("SendDebugMessage")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="����������� �������� sms ���������� �� ������ �� �������� ����� ������">
								<ItemTemplate>
									<asp:TextBox ID="SmsSendDelay" runat="server" Text='<%# Eval("ErrorNotificationDelay") %>' />
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