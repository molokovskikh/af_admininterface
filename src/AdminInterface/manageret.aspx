<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.manageret" CodePage="1251"
	CodeFile="manageret.aspx.cs" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>������������ ������������</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<style type="text/css">
		.tdCheckBox {vertical-align: middle; text-align: left; font-size: 8pt; font-family: Verdana;}
	</style>
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="center" style="height: 754px">
					<div align="center">
						<font face="Verdana"><font size="2"><strong>������������ �������</strong> </font></font>
						<asp:Label ID="Name" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="10pt"></asp:Label></div>
					<div align="center">
						<table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
							align="center" border="1">
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<font face="Verdana" size="2"><strong>����� ���������</strong></font></td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox ID="InvisibleCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="&#34;���������&#34; ������" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="RegisterCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="������"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="RejectsCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="����������"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<font face="Arial" size="2">
										<div>
											<asp:TextBox ID="MultiUserLevelTB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												BorderStyle="None" Width="33px" BackColor="LightGray"></asp:TextBox><font face="Verdana">&nbsp;-
													������������� �������</font></div>
									</font>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="AdvertisingLevelCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="�������" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="WayBillCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="���������" BorderStyle="None" Enabled="False"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="ChangeSegmentCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="��������� ��������" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="EnableUpdateCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="�������������� ���������� ������" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox ID="ResetCopyIDCB" runat="server" Font-Names="Verdana" Text="�������� ���������� �������������, �������:"
										Font-Size="8pt" Enabled="False"></asp:CheckBox>
									<asp:TextBox ID="CopyIDWTB" runat="server" Font-Names="Verdana" BorderStyle="None"
										Font-Size="8pt" BackColor="LightGray" Enabled="False"></asp:TextBox>
									<asp:Label ID="IDSetL" runat="server" Font-Size="8pt" Font-Names="Verdana" ForeColor="Green">������������� �� ��������</asp:Label></td>
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
								<td class="tdCheckBox" colspan="3">
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
								<td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
									<p style="font-size: small; font-family: Verdana; text-align: center;">
										<strong>������������ ���������</strong></p>
								</td>
							</tr>
							<tr>
								<td style="height: 8px" valign="middle" align="left" colspan="3">
									<font face="Arial" size="2">�������� ������:</font>
									<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
										Font-Names="Verdana" Font-Size="8pt" AutoPostBack="True" DataSource="<%# admin %>"
										DataTextField="Region" DataValueField="RegionCode">
									</asp:DropDownList>
									<asp:CheckBox ID="AllRegCB" runat="server" OnCheckedChanged="AllRegCB_CheckedChanged"
										Font-Size="8pt" Font-Names="Verdana" Text="�������� ��� �������" AutoPostBack="True">
									</asp:CheckBox></td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 79px">
									<table id="Table3" cellspacing="0" cellpadding="0" border="0">
										<tr>
											<td width="200">
												<font face="Verdana" size="2"><strong>��������� �������:</strong></font></td>
											<td width="200">
												<font face="Verdana" size="2"><strong>������� ������:</strong></font></td>
										</tr>
										<tr>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="WRList" runat="server" Font-Names="Verdana" Font-Size="8pt"
													BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode"
													CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="OrderList" runat="server" Font-Names="Verdana" Font-Size="8pt"
													BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode"
													CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="right" colspan="3" style="height: 17px">
									<asp:Button ID="ParametersSave" runat="server" OnClick="ParametersSave_Click" Font-Names="Verdana"
										Font-Size="8pt" Text="���������" BorderStyle="None"></asp:Button></td>
							</tr>
							<tr>
								<td colspan="3" style="height: 22px">
									<asp:Button runat="Server" ID="GeneratePasswords" Text="������������� ������" BorderStyle="Groove" Font-Names="Verdana" Font-Size="8pt" OnClick="GeneratePasswords_Click" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 15px">
									<asp:Label ID="ResultL" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="8pt"
										ForeColor="Green" Font-Italic="True"></asp:Label></td>
							</tr>
						</table>
					</div>
					<div align="center">
						<font face="Arial"><strong>
							<p align="center">
								<table id="Table4" cellspacing="0" cellpadding="0" border="0">
									<tbody>
										<tr>
											<td colspan="2">
												<p align="center">
													<strong><font face="Verdana" size="2">��������� �������:</font></strong></p>
											</td>
										</tr>
										<tr>
											<td valign="middle" align="center" colspan="2" style="height: 228px">
												<table id="Table5" cellspacing="0" cellpadding="0" width="300" border="0">
													<tbody>
														<tr>
															<td>
																<asp:TextBox ID="MessageTB" runat="server" BorderStyle="None" Width="400px" Height="150px"
																	TextMode="MultiLine" Font-Names="Verdana" Font-Size="8pt" BackColor="LightGray"></asp:TextBox></td>
														</tr>
														<tr>
															<td>
																<p align="left">
																	<font face="Verdana" size="2">��������</font>
																	<asp:DropDownList ID="SendMessageCountDD" runat="server">
																		<asp:ListItem Value="1" Selected="True">1</asp:ListItem>
																		<asp:ListItem Value="2">2</asp:ListItem>
																		<asp:ListItem Value="5">5</asp:ListItem>
																		<asp:ListItem Value="10">10</asp:ListItem>
																	</asp:DropDownList>&nbsp;<font face="Verdana" size="2">���.</font></p>
															</td>
														</tr>
														<tr>
															<td>
																<p align="right">
																	<asp:Button ID="SendMessage" runat="server" OnClick="SendMessage_Click" Font-Names="Arial"
																		Font-Size="10pt" Text="���������" BorderStyle="None"></asp:Button></p>
															</td>
														</tr>
													</tbody>
												</table>
												<p align="center">
													<asp:Label ID="StatusL" runat="server" Font-Bold="True" ForeColor="Green" Font-Names="Verdana"
														Font-Size="8pt" Font-Italic="True"> C�������� �����������.</asp:Label><br /> 
														<asp:Label ID="MessageLeftL" runat="Server" ForeColor="Green" Font-Names="Verdana"
														Font-Size="8pt" Font-Italic="True" Text="�������� �� ���������� ���������" /></p>
											</td>
										</tr>
									</tbody>
								</table>
							</p>
						</strong></font>
					</div>
					<p align="center">
						<font face="Verdana"><font size="1"><font color="#000000">� �� "</font> </font></font>
						<a href="http://www.analit.net/"><font color="#800080" size="1" face="Verdana">�������</font></a><font
							color="#000000" size="1" face="Verdana">" 2005</font></p>
				</td>
			</tr>
		</table>
		&nbsp;
	</form>
</body>
</html>
