<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.report" codePage="1251" CodeFile="report.aspx.vb" %>
<HTML>
	<HEAD>
		<title></title>
		<meta content="False" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="650" border="0">
				<TR>
					<TD><SPAN>
							<TABLE id="Table6" height="100" cellSpacing="0" cellPadding="0" width="100%" border="0">
								<TR>
									<TD vAlign="top" align="left" width="230">
										<P><IMG src="logo_sm.gif"></P>
									</TD>
									<TD vAlign="top" align="center"><FONT face="Verdana" size="2"><STRONG>��������������� ����� 
												�</STRONG>&nbsp; </FONT>
										<asp:label id="LBCode" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Verdana">Test</asp:label><br>
										<FONT face="Verdana" size="2"><STRONG>������� � </STRONG></FONT>
										<asp:label id="DogNLB" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Verdana">Test</asp:label><BR>
										<FONT face="Verdana" size="2"><FONT size="1">���� ��������:</FONT>
											<asp:label id="RegDate" runat="server" Font-Size="7pt" Font-Names="Verdana">2005-01-01 00:00:00</asp:label></FONT><br>
										<asp:label id="ChPassMessLB" runat="server" Font-Size="7pt" Font-Names="Verdana" Visible="False">[��������� ������ �� ���������� �������]</asp:label></TD>
								</TR>
							</TABLE>
						</SPAN>
					</TD>
				</TR>
				<TR>
					<TD align="center" height="15">
						<P><FONT face="Arial" size="2">
								<TABLE id="Table2" borderColor="#000000" cellSpacing="0" cellPadding="0" width="90%" border="0">
									<TR>
										<TD align="right" colSpan="2" height="40">
											<P align="center"><STRONG><FONT face="Verdana" size="2">������� ������:</FONT></STRONG></P>
										</TD>
									</TR>
									<TR>
										<TD align="right" width="275" height="25">
											<P align="right"><SPAN style="FONT-SIZE: 12pt; FONT-FAMILY: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><FONT face="Verdana" size="2"><STRONG>������ 
															������������:</STRONG>&nbsp;</FONT></SPAN></P>
										</TD>
										<TD height="25"><FONT face="Verdana" size="2">&nbsp; </FONT>
											<asp:label id="LBClient" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">����-����</asp:label></TD>
									</TR>
									<TR>
										<TD align="right" height="25"><FONT face="Verdana" size="2"><STRONG>������� ������������:</STRONG>&nbsp;</FONT>
										</TD>
										<TD height="25">
											<FONT face="Verdana" size="2">&nbsp; </FONT>
											<asp:Label id="LBShortName" runat="server" Font-Names="Times New Roman" Font-Size="9pt" Font-Bold="True">����</asp:Label></TD>
									</TR>
									<TR>
										<TD align="right" height="25"><SPAN style="FONT-SIZE: 12pt; FONT-FAMILY: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><FONT face="Verdana" size="2"><STRONG>��� 
														�������:</STRONG>&nbsp;</FONT></SPAN></TD>
										<TD height="25"><FONT face="Verdana" size="2">&nbsp; </FONT>
											<asp:label id="TariffLB" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">�������� �������������� �������</asp:label></TD>
									</TR>
									<TR>
										<TD align="right" height="25"><SPAN style="FONT-SIZE: 12pt; FONT-FAMILY: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><FONT face="Verdana" size="2"><STRONG>��� 
														��� ������� � �������(login):</STRONG>&nbsp;</FONT></SPAN></TD>
										<TD height="25"><FONT face="Verdana" size="2">&nbsp; </FONT>
											<asp:label id="LBLogin" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">demonstr</asp:label></TD>
									</TR>
									<TR>
										<TD align="right" height="25"><SPAN style="FONT-SIZE: 12pt; FONT-FAMILY: Arial; mso-fareast-font-family: 'Times New Roman'; mso-ansi-language: RU; mso-fareast-language: RU; mso-bidi-language: AR-SA"><FONT face="Verdana" size="2"><STRONG>������:</STRONG>&nbsp;</FONT></SPAN></TD>
										<TD height="25"><FONT face="Verdana" size="2">&nbsp; </FONT>
											<asp:label id="LBPassword" runat="server" Font-Bold="True" Font-Size="9pt" Font-Names="Times New Roman">analog</asp:label></TD>
									</TR>
								</TABLE>
						</P>
					</TD>
				</TR>
				<TR>
					<TD align="center" height="15"></TD>
				</TR>
				<TR>
					<TD align="center" height="120">
						<P><FONT face="Arial" size="2">
								<TABLE id="Table7" borderColor="#000000" cellSpacing="0" cellPadding="0" width="95%" border="1">
									<TR>
										<TD align="center" height="30"><FONT face="Verdana" size="1"><STRONG>��������� 
													�������������:</STRONG></FONT></TD>
									</TR>
									<TR>
										<TD vAlign="top" height="60">
											<P><FONT size="1"><FONT face="Verdana">���������������� ������ ���.: (4732) 206-000(��. - 
														���. 9.00 - 18.00 MSK), e-mail: <A>tech@analit.net</A></FONT><BR>
												</FONT>
												<asp:label id="SupportPhoneLB" runat="server" Font-Size="7pt" Font-Names="Verdana" Visible="False"> ������������� � ����� �������. ���.: </asp:label><BR>
												<asp:label id="SupportNameLB" runat="server" Font-Size="7pt" Font-Names="Verdana" Visible="False">��� ������������ ��������: </asp:label></P>
										</TD>
									</TR>
								</TABLE>
						</P>
					
						<P>&nbsp;
				</TR>
				<TR>
					<TD vAlign="top" align="left" height="50">
						<P><FONT face="Verdana" size="1"><STRONG>���������� ������������� ����������� � ��������� 
									������ �������������� �� ����� www.analit.net<BR>
									� ������� "��� ������������������ �������������"</STRONG></FONT></P>
					</TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 320px">
						<P><FONT size="1"><FONT face="Verdana"><STRONG><EM>������������ ����������� �������� ������ ����� 
											������� ������!</EM></STRONG></FONT><br>
								<FONT face="Verdana">����������, ����� ����� ������: </FONT></FONT>
						</P>
						<UL>
							<LI>
								<FONT face="Verdana" size="1">�� �������� � �������&nbsp;��� �� ��� �������, 
									���������� � �������� �������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ��� ��������� ���������� ������ ��� ������� 
									������, ��������� ����������� �������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ��� ��� ��, ��� � ���������� ������, �� � ������ � 
									�����. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ��� ����� ������, ������� ����� ������, ������� 
									����� ������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ��� ������ ������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">������ �� �������� ������ �� ���� ���� � �����. </FONT>
							</LI>
						</UL>
						<P align="justify"><FONT face="Verdana" size="1">������������ ���������� � ������ 
								������: </FONT>
						</P>
						<UL>
							<LI>
								<FONT face="Verdana" size="1">�� ������ ��������� �������� ������� � ����� � 
									�������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ������ ���� �����&nbsp;8 (������) ��������. </FONT>
							<LI>
								<FONT face="Verdana" size="1">�� ������ ��������� ������ �����. </FONT>
							<LI>
								<FONT face="Verdana" size="1">������������� ������� ���� �����������.</FONT></LI></UL>
					</TD>
				</TR>
				<TR>
					<TD>---------------------------------------------------------------------------------------------------------</TD>
				</TR>
				<TR>
					<TD align="center">
						<P><FONT face="Verdana" size="1"><STRONG>�������� ����� � ����� � </STRONG></FONT>
							<asp:label id="LBCard" runat="server" Font-Bold="True" Font-Size="7pt" Font-Names="Verdana">Test</asp:label><STRONG><FONT face="Verdana" size="1">, 
									������� � </FONT></STRONG>
							<asp:label id="DogNNLB" runat="server" Font-Bold="True" Font-Size="7pt" Font-Names="Verdana">Test</asp:label><BR>
							<asp:label id="RepLb" runat="server" Font-Size="7pt" Font-Names="Verdana" Visible="False"> [��������� ������]</asp:label><FONT face="Verdana" size="1"></FONT></P>
					</TD>
				</TR>
				<TR>
					<TD>
						<TABLE id="Table3" cellSpacing="0" cellPadding="0" width="100%" border="0">
							<TR>
								<TD align="right" width="100" height="20"><FONT face="Verdana" size="1">������:&nbsp; </FONT>
								</TD>
								<TD height="20"><asp:label id="LBCCard" runat="server" Font-Size="7pt" Font-Names="Verdana">����-����</asp:label><FONT face="Verdana" size="1"></FONT></TD>
							</TR>
							<TR>
								<TD align="right" width="100" height="20"><FONT face="Verdana" size="1">Login:&nbsp; </FONT>
								</TD>
								<TD height="20"><asp:label id="LBLcard" runat="server" Font-Size="7pt" Font-Names="Verdana">demonstr</asp:label><FONT face="Verdana" size="1"></FONT></TD>
							</TR>
							<TR>
								<TD align="right" width="100" height="20">
									<P dir="ltr" style="MARGIN-RIGHT: 0px"><FONT face="Verdana" size="1">������:&nbsp; </FONT>
									</P>
								</TD>
								<TD height="20"><asp:label id="TariffD" runat="server" Font-Size="7pt" Font-Names="Verdana">�������� �������������� �������</asp:label><FONT face="Verdana" size="1"></FONT></TD>
							</TR>
						</TABLE>
					</TD>
				</TR>
				<TR>
					<TD>
						<P><FONT face="Verdana" size="1"><STRONG>��������������� ����� �������.��������������� � 
									������������������ ������� ������.</STRONG>
								<br>
								<br>
								�������������(�.�.�.): _____________________________________________________ </FONT>
						</P>
					</TD>
				</TR>
				<TR>
					<TD>
						<P><FONT size="2"><FONT face="Arial">
									<TABLE id="Table4" cellSpacing="0" cellPadding="0" width="100%" border="0">
										<TR>
											<TD><FONT face="Verdana" size="1">����:</FONT></TD>
											<TD><asp:label id="LBDate" runat="server" Font-Size="7pt" Font-Names="Verdana"></asp:label><FONT face="Verdana" size="1"></FONT></TD>
											<br>
											<TD align="right"><FONT face="Verdana" size="1">�������:</FONT></TD>
											<TD><FONT face="Verdana" size="1">_______________________</FONT></TD>
										</TR>
									</TABLE>
								</FONT></FONT>
						</P>
					</TD>
				</TR>
			</TABLE>
	</form>
	</body>
</HTML>
