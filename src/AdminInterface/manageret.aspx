<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.manageret" 
	CodePage="1251" CodeBehind="manageret.aspx.cs" 
	MaintainScrollPositionOnPostback="true" Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
<form id="form1" runat="server">
	<style type="text/css">
		.tdCheckBox {vertical-align: middle; text-align: left;}
	</style>
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="center" style="height: 754px">
					<div align="center">
						<strong>������������ �������</strong>
						<asp:Label ID="Name" runat="server" Font-Bold="True" />
					</div>
					<div align="center">
						<table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
							align="center" border="1">
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<strong>����� ���������</strong>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:DropDownList ID="VisileStateList" runat="server">
										<asp:ListItem Text="�������" Value="0"></asp:ListItem>
										<asp:ListItem Text="�� �������" Value="1"></asp:ListItem>
										<asp:ListItem Text="�������" Value="2"></asp:ListItem>
									</asp:DropDownList>
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
									<asp:CheckBox ID="ChangeSegmentCB" runat="server" Text="��������� ��������" BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="EnableUpdateCB" runat="server" Text="�������������� ���������� ������"
										BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox ID="ResetCopyIDCB" runat="server" Text="�������� ���������� �������������, �������:"
										Enabled="False" />
									<asp:TextBox ID="CopyIDWTB" runat="server" BorderStyle="None" BackColor="LightGray"
										Enabled="False" />
									<asp:Label ID="IDSetL" runat="server" ForeColor="Green">������������� �� ��������</asp:Label>
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="CalculateLeaderCB" Text="P����������� ������� ��� ��������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="AllowSubmitOrdersCB" Text="��������� ���������� �������������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="SubmitOrdersCB" Text="��������� ������������� �������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="ServiceClientCB" Text="��������� ������" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="OrdersVisualizationModeCB" Text="���������� ������ ��������" />
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
														<asp:ListItem Value="1">����</asp:ListItem>
														<asp:ListItem Value="2">�������</asp:ListItem>
													</asp:DropDownList>
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
										<EmptyDataTemplate>
											<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="��������� �������." />
										</EmptyDataTemplate>
									</asp:GridView>
								</td>
							</tr>
							<tr>
								<td  style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
									����� ��������
								</td>
							</tr>
							<tr>
								<td style="text-align:left;">
									<asp:CheckBoxList ID="ExportRulesList" runat="server" DataTextField="DisplayName" DataValueField="Id" BorderStyle="None">
									</asp:CheckBoxList>
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
									<font face="Arial" size="2">�������� ������:</font>
									<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
										AutoPostBack="True" DataSource="<%# admin %>" DataTextField="Region" DataValueField="RegionCode">
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
												<asp:CheckBoxList ID="WRList" runat="server" BorderStyle="None" DataSource="<%# WorkReg %>"
													DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="OrderList" runat="server" BorderStyle="None" DataSource="<%# WorkReg %>"
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
								<td colspan="3" style="height: 22px">
									<asp:Button runat="Server" ID="GeneratePasswords" Text="������������� ������"
										OnClick="GeneratePasswords_Click" ValidationGroup="0" />
								</td>
							</tr>
							<tr>
								<td colspan="3">
									<asp:Button ID="DeletePrepareDataButton" runat="server" ValidationGroup="0" OnClick="DeletePrepareDataButton_Click" Text="������� �������������� ������" />
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