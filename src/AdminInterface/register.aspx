<%@ Page MaintainScrollPositionOnPostback="true" Theme="Main" Language="c#" AutoEventWireup="true"
	Inherits="AddUser.WebForm1" CodeBehind="register.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>����������� �������������</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />

	<script type="text/javascript" language="javascript" src="./JavaScript/Main.js"></script>

</head>
<body>
	<form id="From1" method="post" runat="server" defaultbutton="Register">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" align="center" bgcolor="#ebebeb"
			border="0">
			<tr>
				<td align="right" colspan="4">
					<p align="center">
						<strong>����������� �������</strong>
					</p>
				</td>
			</tr>
			<tr>
				<td colspan="4">
					<div id="Info" class="Fields">
						<div class="TwoColumn">
							<div class="SimpleField">
								<label class="Required" for="FullNameTB">
									������ ������������:
								</label>
								<asp:TextBox ID="FullNameTB" runat="server"></asp:TextBox>
								<asp:RegularExpressionValidator ID="FullNameValidator" runat="server" ErrorMessage="������ ������ �� ����� ���� ������ ��� 40 ��������" ValidationExpression="^.{1,40}$" ValidationGroup="0" ControlToValidate="FullNameTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredFullName" runat="server" ControlToValidate="FullNameTB"
									ErrorMessage="���� ������� ������������ ������ ���� ���������" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
							<div class="SimpleField">
								<label class="Required" for="ShortNameTB">
									������� ������������:
								</label>
								<asp:TextBox ID="ShortNameTB" runat="server" />
								<asp:RegularExpressionValidator ID="ShortNameValidator" runat="server"
									ErrorMessage="������ ������ �� ����� ���� ������ ��� 50 ��������" ValidationExpression="^.{1,50}$" ValidationGroup="0" ControlToValidate="ShortNameTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredShortName" runat="server" ControlToValidate="ShortNameTB"
									ErrorMessage="���� �������� ������������ ������ ���� ���������" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
							<div class="SimpleField">
								<label class="Required" for="AddressTB">
									����� �������� ������������:
								</label>
								<asp:TextBox ID="AddressTB" runat="server" />
								<asp:RegularExpressionValidator ID="AddressValidator" runat="server"
									ErrorMessage="������ ������ �� ����� ���� ������ ��� 100 ��������" ValidationExpression="^.{1,100}$" ValidationGroup="0" ControlToValidate="AddressTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredAddress" runat="server" ControlToValidate="AddressTB"
									ErrorMessage="���� ������ �������� ������������ ������ ���� ���������" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
<%--							<div class="SimpleField">
								<label class="Required" for="PhoneTB">
									�������:
								</label>
								<asp:TextBox ID="PhoneTB" runat="server" />
								<asp:RequiredFieldValidator ID="RequiredPhone" runat="server" ControlToValidate="PhoneTB"
									ErrorMessage="���� ��������� ������ ���� ���������" ValidationGroup="0">*</asp:RequiredFieldValidator>
							</div>
--%>						</div>
						<div class="TwoColumn">
							<div class="SimpleField">
								<label class="Required" for="PhoneTB">
									�������:
								</label>
								<asp:TextBox ID="PhoneTB" runat="server" />
								<asp:RequiredFieldValidator ID="RequiredPhone" runat="server" ControlToValidate="PhoneTB"
									ErrorMessage="���� ��������� ������ ���� ���������" ValidationGroup="0">*</asp:RequiredFieldValidator>
							</div>
							<div class="SimpleField">
								<label for="EmailTB">
									E-mail:
								</label>
								<asp:TextBox ID="EmailTB" runat="server" />
							</div>
							<div class="SimpleField">
								<label for="LoginTB">
									Login:
								</label>
								<asp:TextBox ID="LoginTB" runat="server" />
								<asp:CustomValidator ID="LoginValidator" runat="server" ErrorMessage="���� �Login� ������ ���� ���������"
									ControlToValidate="LoginTB" OnServerValidate="LoginValidator_ServerValidate"
									ValidationGroup="0" EnableClientScript="False" ValidateEmptyText="True">*</asp:CustomValidator>
							</div>
						</div>
					</div>
					<div id="ExtInfo" class="Fields">
						<div class="TwoColumn">
							<div class="ComplexField">
								<asp:CheckBox ID="PayerPresentCB" runat="server" OnCheckedChanged="PayerPresentCB_CheckedChanged"
									Text="���������� ����������" AutoPostBack="True"></asp:CheckBox>
								<asp:TextBox ID="PayerFTB" runat="server" Visible="False" Width="90px" />
								<asp:Button ID="FindPayerB" runat="server" OnClick="FindPayerB_Click" Text="�����"
									Visible="False" ValidationGroup="1" />
								<asp:DropDownList ID="PayerDDL" runat="server" Visible="False" DataSource="<%# DataTable1 %>"
									DataTextField="PayerName" DataValueField="PayerID" />
								<asp:Label ID="PayerCountLB" runat="server" ForeColor="Green" Visible="False" />
							</div>
							<div>
								<asp:CheckBox ID="EnterBillingInfo" runat="server" Text="��������� ���������� ��� ��������" Checked="true" />
							</div>
							<div class="ComplexField">
								<asp:CheckBox ID="IncludeCB" runat="server" OnCheckedChanged="IncludeCB_CheckedChanged"
									Text="����������� ������" AutoPostBack="True" />
								<asp:TextBox ID="IncludeSTB" runat="server" Visible="False" />
								<asp:Button ID="IncludeSB" runat="server" OnClick="IncludeSB_Click" Text="�����"
									Visible="False" ValidationGroup="2" />
								<asp:Label ID="IncludeCountLB" runat="server" ForeColor="Green" Visible="False" />
								<asp:DropDownList ID="IncludeSDD" runat="server" OnSelectedIndexChanged="IncludeSDD_SelectedIndexChanged"
									Visible="False" DataSource="<%# Incudes %>" DataTextField="ShortName" DataValueField="FirmCode"
									AutoPostBack="True" />
								<asp:DropDownList ID="IncludeType" runat="server" Visible="False">
									<asp:ListItem Value="0">�������</asp:ListItem>
									<asp:ListItem Value="1">����</asp:ListItem>
									<asp:ListItem Value="2">�������</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div>
								<asp:CheckBox runat="server" ID="ServiceClient" Text="��������� �� �������" />
							</div>
						</div>
						<div class="TwoColumn">
							<div class="DropDownField">
								<label for="">
									��� ������� � ���������� ����������:
								</label>
								<asp:DropDownList ID="CustomerType" runat="server" Visible="true">
									<asp:ListItem Value="0" Text="�����������" />
									<asp:ListItem Value="1" Text="����������� ��� ��������" />
									<asp:ListItem Value="2" Text="�������" />
								</asp:DropDownList>
							</div>
							<div class="DropDownField">
								<asp:TextBox ID="PassTB" runat="server" Visible="False" Width="10px" />
								<label for="TypeDD">
									���:</label>
								<asp:DropDownList ID="TypeDD" runat="server" 
									onselectedindexchanged="ClientTypeChanged" AutoPostBack="True" />
								</div>
							<div class="DropDownField">
								<label for="RegionDD">
									�������� ������:</label>
								<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
									AutoPostBack="True" DataSource="<%# admin %>" DataTextField="Region" DataValueField="RegionCode" />
							</div>
							<div class="DropDownField">
								<label for="SegmentDD">
									�������:</label>
								<asp:DropDownList ID="SegmentDD" runat="server" />
							</div>
						</div>
					</div>
				</td>
			</tr>
			<tr>
				<td align="right" height="25">
					<asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="EmailTB"
						ErrorMessage="������ � e-mail" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
						Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="PhoneTB"
						ErrorMessage="���� &quot;�������&quot; ������ ���� ���������� ��� &quot;XXX(X)-XXXXXX(X)&quot;"
						ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<asp:RegularExpressionValidator ID="RegularExpressionValidator8" runat="server" ControlToValidate="TBClientManagerPhone"
						ErrorMessage="���� &quot;�������&quot; ������ ���� ���������� ��� &quot;XXX(X)-XXXXXX(X)&quot;"
						ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<asp:RegularExpressionValidator ID="Regularexpressionvalidator4" runat="server" ControlToValidate="TBOrderManagerMail"
						ErrorMessage="������ � e-mail Order Manager" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
						Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<asp:RegularExpressionValidator ID="Regularexpressionvalidator5" runat="server" ControlToValidate="TBClientManagerMail"
						ErrorMessage="������ � e-mail Client Manager" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
						Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<asp:RegularExpressionValidator ID="RegularExpressionValidator9" runat="server" ControlToValidate="TBOrderManagerPhone"
						ErrorMessage="���� &quot;�������&quot; ������ ���� ���������� ��� &quot;XXX(X)-XXXXXX(X)&quot;"
						ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None" ValidationGroup="0"></asp:RegularExpressionValidator>
					<font face="Verdana" size="2"></font>
				</td>
				<td height="25">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="��� ����������� �������� ������:"
						ShowSummary="False" ShowMessageBox="True" ValidationGroup="0" />
				</td>
			</tr>
			<tr>
				<td valign="middle" align="left" colspan="4" style="height: 25px">
					<p>
					</p>
					<asp:CheckBox ID="CheckBox1" runat="server" Text="���������� ��� �������" OnCheckedChanged="CheckBox1_CheckedChanged"
						AutoPostBack="True"></asp:CheckBox>
					<table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
						align="center" border="1">
						<tr>
							<td style="height: 18px">
								<strong>��������� �������:</strong>
							</td>
							<td id="WorkRegionLable" runat="server" visible="false" style="height: 18px">
								<strong>������� ������:</strong>
							</td>
							<td style="height: 18px">
								<strong>������� ������:</strong>
							</td>
						</tr>
						<tr>
							<td>
								<asp:CheckBoxList ID="WRList" runat="server" BorderStyle="None" DataSource="<%# WorkReg %>"
									DataTextField="Region" DataValueField="RegionCode" />
							</td>
							<td id="WorkRegion" runat="server" visible="false">
								<asp:CheckBoxList ID="WRList2" runat="server" BorderStyle="None" DataSource="<%# WorkReg %>"
									DataTextField="Region" DataValueField="RegionCode" />
							</td>
							<td>
								<asp:CheckBoxList ID="OrderList" runat="server" BorderStyle="None" DataSource="<%# WorkReg %>"
									DataTextField="Region" DataValueField="RegionCode" />
							</td>
						</tr>
					</table>
					<p>
				</td>
			</tr>
			<tr>
				<td style="background-color: #dadada" colspan="4">
					<strong>�������������� �������� � �������</strong>
					<div id="Phones" class="Fields">
						<div class="TwoColumn">
							<span id="OrderManagerGroupLabel" runat="server">
								������������� �� ������ � ����������:
							</span>
							<div class="SimpleField">
								<label for="TBOrderManagerName">
									���:
								</label>
								<asp:TextBox ID="TBOrderManagerName" runat="server" />
								<asp:RegularExpressionValidator ID="OrderManagerNameValidator" runat="server" ErrorMessage="������ ������ �� ����� ���� ������ ��� 100 ��������" ValidationGroup="0" ValidationExpression="^.{1,100}$" ControlToValidate="TBOrderManagerName" Display="Dynamic" >*</asp:RegularExpressionValidator>
							</div>
							<div class="SimpleField">
								<label for="TBOrderManagerPhone">
									���.:
								</label>
								<asp:TextBox ID="TBOrderManagerPhone" runat="server" />
							</div>
							<div class="SimpleField">
								<label for="TBOrderManagerMail">
									E-mail:
								</label>
								<asp:TextBox ID="TBOrderManagerMail" runat="server" />
							</div>
						</div>
						<div class="TwoColumn" id="ClientManagerGropBlock" runat="server" visible="false">
							<span>
								����������� � ����������:
							</span>
							<div class="SimpleField">
								<label for="TBClientManagerName">
									���:
								</label>
								<asp:TextBox ID="TBClientManagerName" runat="server" />
								<asp:RegularExpressionValidator ID="ClientManagerNameValidator" runat="server"	ErrorMessage="������ ������ �� ����� ���� ������ ��� 100 ��������" ValidationGroup="0" ValidationExpression="^.{1,100}$" ControlToValidate="TBClientManagerName" Display="Dynamic" >*</asp:RegularExpressionValidator>
							</div>
							<div class="SimpleField">
								<label for="TBClientManagerPhone">
									���.:
								</label>
								<asp:TextBox ID="TBClientManagerPhone" runat="server" />
							</div>
							<div class="SimpleField">
								<label for="TBClientManagerMail">
									E-mail:
								</label>
								<asp:TextBox ID="TBClientManagerMail" runat="server" />
							</div>
						</div>
					</div>
				</td>
			</tr>
			<tr valign="bottom" height="50">
				<td align="center" colspan="4" style="height: 50px">
					<asp:Button ID="Register" runat="server" OnClick="Register_Click" Text="����������������"
						ValidationGroup="0" />
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
