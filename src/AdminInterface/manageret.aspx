<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.manageret" 
	CodePage="1251" CodeBehind="manageret.aspx.cs" 
	MaintainScrollPositionOnPostback="true" Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
	<style type="text/css">
		.tdCheckBox {vertical-align: middle; text-align: left;}
	</style>
	
	<script>
		jQuery(document).ready(function() {
			jQuery("#ctl00_MainContentPlaceHolder_IncludeGrid input[type=text], #ctl00_MainContentPlaceHolder_ShowClientsGrid input[type=text]").keypress(function(e) {
				if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) {
					jQuery(this).parents('td').children('input[type=submit]').click()
					return false;
				} else {
					return true;
				}
			});
		});
	</script>
<form id="form1" runat="server" defaultfocus="ParametersSave" defaultbutton="ParametersSave">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="center" style="height: 754px">
					<div align="center">
						<strong>������������ �������</strong>
						<asp:Label ID="Name" runat="server" Font-Bold="True" />
					</div>
					<div align="center">
						<table id="Table2" style="width: 95%;" cellspacing="0" cellpadding="0" align="center" border="1">
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<strong>��������</strong>
								</td>
							</tr>
							<tr>
								<td style="text-align:left" colspan=3>
									<asp:Button runat="Server" ID="Button2" Text="��������� ����������� � ����������� �����������" OnClick="NotifySuppliers_Click" ValidationGroup="0" />
								</td>
							</tr>
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<strong>����� ���������</strong>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:DropDownList ID="VisileStateList" runat="server">
										<asp:ListItem Text="�����������" Value="0"></asp:ListItem>
										<asp:ListItem Text="�������" Value="2"></asp:ListItem>
									</asp:DropDownList>
									��� ������� � ���������� ����������
								</td>
							</tr>
							<tr id="NoiseRow" runat="server">
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="NoisedCosts" Text="��������� ����"  AutoPostBack="true" oncheckedchanged="NoisedCosts_CheckedChanged" />
									<br />
									<label ID="NotNoisedSupplierLabel" runat="server">�� ��������� �����-����� ����������</label>
									<asp:DropDownList ID="NotNoisedSupplier" runat="server" DataValueField="FirmCode" DataTextField="ShortName" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="ServiceClientCB" Text="��������� �� �������" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="RegisterCB" runat="server" Text="������"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="RejectsCB" runat="server" Text="����������" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:TextBox ID="MultiUserLevelTB" runat="server" BorderStyle="None" Width="33px"
										BackColor="LightGray" />-������������� �������
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="AdvertisingLevelCB" runat="server" Text="�������" BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="WayBillCB" runat="server" Text="���������" BorderStyle="None" Enabled="False" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="EnableUpdateCB" runat="server" Text="�������������� ���������� ������"
										BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="AllowSubmitOrdersCB" Text="��������� ������������ �������������� ������������/��������������� �������� ������������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="SubmitOrdersCB" Text="������������ �������� ������������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="OrdersVisualizationModeCB" Text="���������� ������ ��������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="CalculateLeaderCB" Text="P����������� ������� ��� ��������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="AllowDelayOfPaymentCB" Text="������������ �������� �������� ��������" />
								</td>
							</tr>
							<tr>
								<td style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
									������� ����������:
								</td>							
							</tr>
							<tr>
								<td>
									<asp:GridView ID="IncludeGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="IncludeGrid_RowCommand"
										OnRowDeleting="IncludeGrid_RowDeleting" OnRowDataBound="IncludeGrid_RowDataBound">
										<Columns>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Button ID="AddButton" runat="server" Text="��������" CommandName="Add" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Button ID="DeleteButton" runat="server" Text="�������" CommandName="Delete" />
												</ItemTemplate>
												<ItemStyle Width="10%" />
											</asp:TemplateField>
											<asp:TemplateField HeaderText="��������">
												<ItemTemplate>
													<asp:TextBox ID="SearchText" runat="server"></asp:TextBox>
													<asp:Button ID="SearchButton" runat="server" CommandName="Search" Text="�����" ValidationGroup="0" />
													<asp:DropDownList ID="ParentList" runat="server" DataTextField="ShortName" DataValueField="FirmCode"
														Width="200px">
													</asp:DropDownList>
													<asp:CustomValidator ID="ParentValidator" runat="server" ControlToValidate="ParentList"
														ErrorMessage="���������� ������� ��������." OnServerValidate="ParentValidator_ServerValidate" ClientValidationFunction="ValidateParent" ValidateEmptyText="True" ValidationGroup="1">*</asp:CustomValidator>
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField HeaderText="��� ����������">
												<ItemTemplate>
													<asp:DropDownList ID="IncludeTypeList" runat="server">
														<asp:ListItem Value="0">�������</asp:ListItem>
														<asp:ListItem Value="3">�������+</asp:ListItem>
														<asp:ListItem Value="1">����</asp:ListItem>
														<asp:ListItem Value="2">�������</asp:ListItem>
													</asp:DropDownList>
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
										<EmptyDataTemplate>
											<div style="text-align:left">
												<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="��������� �������." />
											</div>
										</EmptyDataTemplate>
									</asp:GridView>
								</td>
							</tr>
							<tr>
								<td class="Title">
									������������ �������:
								</td>
							</tr>
							<tr>
								<td>
									<asp:GridView ID="ShowClientsGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="ShowClientsGrid_RowCommand" OnRowDataBound="ShowClientsGrid_RowDataBound" OnRowDeleting="ShowClientsGrid_RowDeleting">
										<EmptyDataTemplate>
											<div style="text-align:left">
												<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="�������� �������" />
											</div>
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
													<asp:CustomValidator ID="ShowCleintsValidator" 
																		 runat="server" 
																		 ControlToValidate="ShowClientsList" 
																		 ErrorMessage="���������� ������� �������." 
																		 OnServerValidate="ParentValidator_ServerValidate" 
																		 ValidateEmptyText="True" 
																		 ValidationGroup="1">*</asp:CustomValidator>
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
								</td>
							</tr>
							<tr>
								<td  style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
									����� ������������ ��������� ��������������:
								</td>
							</tr>
							<tr>
								<td style="text-align:left;">
									<table cellpadding=0 cellspacing=0 style="border-collapse:collapse; width: 100%">
									<tr>
										<tr style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
											<td style="border: solid 1px black">
												�������� � Excel
											</td>
											<td style="border: solid 1px black">
												������
											</td>
										</tr>
										<td>
											<asp:CheckBoxList ID="ExportRulesList" runat="server" DataTextField="DisplayName" DataValueField="Id" BorderStyle="None">
											</asp:CheckBoxList>
										</td>
										<td style="vertical-align:top;">
											<asp:CheckBoxList ID="PrintRulesList" runat="server" DataTextField="DisplayName" DataValueField="Id" BorderStyle="None">
											</asp:CheckBoxList>
										</td>
									</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
									<p style="text-align: center;">
										<strong>������������ ���������</strong></p>
								</td>
							</tr>
							<tr>
								<td style="height: 8px" valign="middle" align="left" colspan="3">
									<label>�������� ������:</label>
									<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
										AutoPostBack="True" DataTextField="Region" DataValueField="RegionCode">
									</asp:DropDownList>
									<asp:CheckBox ID="AllRegCB" runat="server" OnCheckedChanged="AllRegCB_CheckedChanged"
										Text="�������� ��� �������" AutoPostBack="True"></asp:CheckBox></td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 79px">
									<table id="Table3" cellspacing="0" cellpadding="0" border="0">
										<tr>
											<td width="200">
												<strong>��������� �������:</strong>
											</td>
											<td width="200">
												<strong>������� ������:</strong>
											</td>
										</tr>
										<tr>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="WRList" runat="server" BorderStyle="None"
													DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="OrderList" runat="server" BorderStyle="None" 
													DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="right" colspan="3" style="height: 17px">
									<asp:Button ID="ParametersSave" runat="server" OnClick="ParametersSave_Click" Text="���������" ValidationGroup="1"></asp:Button>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 15px">
									<asp:Label ID="ResultL" runat="server" Font-Bold="True" ForeColor="Green" Font-Italic="True"></asp:Label></td>
							</tr>
						</table>
					</div>
				</td>
			</tr>
		</table>
	</form>
</asp:Content>