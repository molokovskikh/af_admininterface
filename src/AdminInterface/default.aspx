<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser._default" CodePage="1251"
	CodeBehind="default.aspx.cs" Theme="Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>��������� ���������� ���������</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<style type="text/css">
		.StatisticData
		{
			background-color:#d8f1ff; 
			text-align:right; 
			font-size: 9pt; 
			font-family:Verdana;
		}
	</style>
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="left" width="200" style="height: 427px">
					<p>
						<asp:HyperLink ID="RegisterHL" runat="server" NavigateUrl="register.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">����������� ��������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="CloneHL" runat="server" NavigateUrl="CopySynonym.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">������������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ClInfHL" runat="server" NavigateUrl="searchc.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">���������� � ��������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ShowStatHL" runat="server" NavigateUrl="statcont.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">���������� ���������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="BillingHL" runat="server" NavigateUrl="~/Billing/Search.rails">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">�������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="FTPHL" runat="server" NavigateUrl="docs.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">����� ���������</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ViewAdministrators" Font-Names="Verdana, Arial, Helvetica, sans-serif"
							Font-Size="small" runat="server" NavigateUrl="ViewAdministrators.aspx">������������ ��������������</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="MonitorUpdates" 
							Font-Names="Verdana,Arial,Helvetica,sans-serif" Font-Size="Small" 
							runat="server" NavigateUrl="./Logs/ClientRegistrationLog.rails">���������� ���������� ��������</asp:HyperLink>
					</p>
				</td>
				<td align="center" style="height: 427px">
					<fieldset style="width:20%;float:right;background-color:#d8f1ff;border:0;padding:3px 3px 3px 3px;">
						<legend style="font-weight:bold;">������ ���������� </legend>
						<div style="text-align:left;background-color:#eef8ff;padding: 3px;">
							<label for="RegionList">������:</label>
							<asp:DropDownList Width="100%" ID="RegionList" runat="server" />
						</div>
						<div style="text-align:left;background-color:#eef8ff; padding: 3px;">
							<label for="FromCalendar">������� � ����:</label>
							<asp:Calendar ID="FromCalendar" runat="server" BackColor="White" BorderColor="#999999"
								CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt"
								ForeColor="Black" Width="100%">
								<SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
								<TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
								<SelectorStyle BackColor="#CCCCCC" />
								<WeekendDayStyle BackColor="#ebebeb" />
								<OtherMonthDayStyle ForeColor="#808080" />
								<NextPrevStyle VerticalAlign="Bottom" />
								<DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
								<TitleStyle BackColor="#dadada" BorderColor="Black" Font-Bold="True" />
							</asp:Calendar>
						</div>
						<div style="text-align:left;background-color:#eef8ff;padding: 3px;">
							<label for="ToCalendar">�� ����:</label>
							<asp:Calendar ID="ToCalendar" runat="server" BackColor="White" BorderColor="#999999"
								CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt"
								ForeColor="Black" Width="100%">
								<SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
								<TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
								<SelectorStyle BackColor="#CCCCCC" />
								<WeekendDayStyle BackColor="#ebebeb" />
								<OtherMonthDayStyle ForeColor="#808080" />
								<NextPrevStyle VerticalAlign="Bottom" />
								<DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
								<TitleStyle BackColor="#dadada" BorderColor="Black" Font-Bold="True" />
							</asp:Calendar>
						</div>
						<div style="text-align:right;">
							<asp:Button ID="ShowButton" runat="server" Text="��������" OnClick="ShowButton_Click" />
						</div>
					</fieldset>
					<table id="Table2" bordercolor="#dadada" cellspacing="1" cellpadding="0" width="69%"
						border="0">
						<tr>
							<td bgcolor="#eef8ff" colspan="4" height="30">
								<p align="center">
									<strong><font face="Verdana" size="2">����������:</font></strong></p>
							</td>
						</tr>
						<tr>
							<td align="center" bgcolor="#dadada" colspan="4" height="20">
								<font face="Verdana" size="2"><strong>����������:</strong></font></td>
						</tr>
						<tr>
							<td style="height: 18px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">������������ �������:&nbsp;</font>
							</td>
							<td style="width: 76px; height: 18px" bgcolor="#d8f1ff" height="18">
								<asp:HyperLink ID="ConfHL" runat="server" NavigateUrl="viewcl.aspx?id=2" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
							<td style="height: 18px" align="right" bgcolor="#d8f1ff" height="18">
								<font face="Verdana" size="2">� ��������:</font></td>
							<td style="height: 18px; width: 58px;" bgcolor="#d8f1ff" height="18">
								<asp:HyperLink ID="ReqHL" runat="server" NavigateUrl="viewcl.aspx?id=4" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">������������ ��:</font></td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:HyperLink ID="CUHL" runat="server" NavigateUrl="viewcl.aspx?id=1" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
							<td style="height: 19px" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">��������:</font></td>
							<td style="height: 19px; width: 58px;" bgcolor="#d8f1ff" height="19">
								<asp:HyperLink ID="ADHL" runat="server" NavigateUrl="viewcl.aspx?id=0" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>							<td>
						</tr>
						<tr>
							<td style="height: 19px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">������:</font>
							</td>
							<td style="width: 76px; text-align: center;" bgcolor="#d8f1ff" height="19">
								<asp:HyperLink ID="ErrUpHL" runat="server" NavigateUrl="viewcl.aspx?id=3" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink>
							</td>
							<td class="StatisticData">
								���������� ������:
							</td>
							<td class="StatisticData">
								<asp:Label runat="server" ID="DownloadDataSize" />
							</td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">��������� ����������:</font></td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="MaxUpdateTime" runat="server" Enabled="False" Font-Size="8pt" Font-Names="Verdana"
									Font-Bold="True">-</asp:Label></td>
							<td class="StatisticData">
								��������� ����������:
							</td>
							<td class="StatisticData">
								<asp:Label runat="server" ID="DownloadDocumentSize" />
							</td>
						</tr>
						<tr>
							<td align="center" bgcolor="#dadada" colspan="4" height="20">
								<font face="Verdana" size="2"><strong>������:</strong></font></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px">
								<font face="Verdana" size="2">�������:</font>
							</td>
							<td style="width: 76px" bgcolor="#ebebeb" height="20">
								<asp:Label ID="OPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" height="20">
								<font face="Verdana" size="2">��������:</font></td>
							<td bgcolor="#ebebeb" height="20" style="width: 58px">
								<asp:HyperLink ID="OADHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px; height: 19px;">
								<font face="Verdana" size="2">�������:</font></td>
							<td style="width: 76px; height: 19px;" bgcolor="#ebebeb">
								<asp:Label ID="OprLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" style="height: 19px">
								<font face="Verdana" size="2">������:</font></td>
							<td bgcolor="#ebebeb" style="height: 19px; width: 58px;">
								<asp:HyperLink ID="OErrHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px">
								<font face="Verdana" size="2">��������� �����:</font></td>
							<td style="width: 76px" bgcolor="#ebebeb" height="20">
								<asp:Label ID="LOT" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" height="20">
								<font face="Verdana" size="2">�����:</font></td>
							<td bgcolor="#ebebeb" height="20" style="width: 58px">
								<asp:Label ID="SumLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td bgcolor="#dadada" colspan="4" height="20">
								<p align="center">
									<font face="Verdana" size="2"><strong>������:</strong></font></p>
							</td>
						</tr>
						<tr>
							<td style="height: 20px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">��������:</font></td>
							<td style="width: 76px; height: 20px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceDOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td style="height: 20px" align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">�� ��������:</font></td>
							<td style="height: 20px; width: 58px;" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceDERRLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">��������� ���������:</font></td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="DownPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">�� ��������:</font></td>
							<td bgcolor="#d8f1ff" height="20" style="width: 58px">
								<asp:Label ID="DownErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">�������������:</font></td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceFOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">�� �������������:</font></td>
							<td bgcolor="#d8f1ff" height="20" style="width: 58px">
								<asp:Label ID="FormErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td style="height: 19px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">��������� ������������:</font>
							</td>
							<td style="width: 76px; height: 19px" bgcolor="#d8f1ff" height="19">
								<asp:Label ID="FormPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td style="height: 19px" align="right" bgcolor="#d8f1ff" height="19">
								<font face="Verdana" size="2"><font face="Verdana" size="2">������� ������������:</font></font></td>
							<td style="height: 19px; width: 58px;" bgcolor="#d8f1ff" height="19">
								<asp:Label ID="WaitPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td style="height: 19px" align="center" bgcolor="#eef8ff" colspan="4">
								<font face="Verdana" size="2">� ������� �������� ���������� ���������� ��������.</font></td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
		<p>
			<asp:Label ID="PassLB" runat="server" Font-Size="9pt" Font-Names="Verdana"></asp:Label></p>
	</form>
</body>
</html>
