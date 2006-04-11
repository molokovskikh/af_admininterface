<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.WebForm1" codePage="1251" aspCompat="False" CodeFile="register.aspx.vb" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Регистрация пользователей</title>
		<META http-equiv="Content-Type" content="text/html; charset=windows-1251">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" align="center" bgColor="#ebebeb"
				border="0">
				<TR>
					<TD align="right" colSpan="4">
						<P align="center"><STRONG><FONT face="Verdana" size="2">Регистрация&nbsp;клиента</FONT></STRONG></P>
					</TD>
				</TR>
				<TR>
					<TD colSpan="4"><asp:label id="Label3" runat="server" ForeColor="Red" Font-Size="8pt" Font-Bold="True" Font-Names="Verdana"></asp:label><asp:label id="Label2" runat="server" ForeColor="Red" Font-Size="8pt" Font-Bold="True" Font-Names="Verdana"></asp:label><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right"><FONT style="FONT-WEIGHT: bold" face="Verdana" color="#000000" size="2">Полное 
							наименование:</FONT></TD>
					<TD align="left"><asp:textbox id="FullNameTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidator1" runat="server" Font-Size="7pt" Font-Names="Verdana"
							ControlToValidate="FullNameTB" ErrorMessage="Поле «Полное наименование» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
					<TD align="right"><FONT style="FONT-WEIGHT: bold" face="Verdana" size="2">Адрес:</FONT></TD>
					<TD align="left"><asp:textbox id="AddressTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None">-</asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidator6" runat="server" Font-Size="8pt" Font-Names="Verdana"
							ControlToValidate="AddressTB" ErrorMessage="Поле «Адрес» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right"><FONT style="FONT-WEIGHT: bold" color="#000000"><FONT face="Verdana"><FONT size="2">Краткое 
									наименование:</FONT></FONT></FONT></TD>
					<TD align="left"><asp:textbox id="ShortNameTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidator2" runat="server" Font-Size="7pt" Font-Names="Verdana"
							ControlToValidate="ShortNameTB" ErrorMessage="Поле «Краткое наименование» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
					<TD align="right">
						<DIV align="right"><FONT face="Verdana" size="2">Виды транспорта:</FONT></DIV>
					</TD>
					<TD align="left"><asp:textbox id="BusInfoTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None">-</asp:textbox><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right"><FONT style="FONT-WEIGHT: bold" face="Verdana" size="2">Телефон:</FONT></TD>
					<TD align="left"><asp:textbox id="PhoneTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidator3" runat="server" Font-Size="7pt" Font-Names="Verdana"
							ControlToValidate="PhoneTB" ErrorMessage="Поле «Телефон» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
					<TD align="right"><FONT face="Verdana" size="2">Остановка транспорта:</FONT></TD>
					<TD align="left"><asp:textbox id=BussStopTB runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].bussstop") %>'></asp:textbox><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right"><FONT face="Verdana" size="2">Факс: </FONT>
					</TD>
					<TD align="left"><asp:textbox id=FaxTB runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].fax") %>'></asp:textbox><FONT face="Verdana" size="2"></FONT></TD>
					<TD align="right"><FONT style="FONT-WEIGHT: bold" face="Verdana" size="2">E-mail:</FONT></TD>
					<TD align="left"><asp:textbox id="EmailTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><asp:requiredfieldvalidator id="RequiredFieldValidator8" runat="server" Font-Size="8pt" Font-Names="Verdana"
							ControlToValidate="EmailTB" ErrorMessage="Поле «E-mail» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right"><FONT face="Verdana" size="2">URL:</FONT></TD>
					<TD align="left"><asp:textbox id=URLTB runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].url") %>'></asp:textbox><FONT face="Verdana" size="2"></FONT></TD>
					<TD align="right"><FONT face="Verdana" size="2">&nbsp;</FONT></TD>
					<TD align="left"><FONT face="Verdana" size="2">&nbsp; </FONT>
					</TD>
				</TR>
				<TR>
					<TD align="right" bgColor="#dadada"><FONT style="FONT-WEIGHT: bold" face="Verdana" size="2">Login:</FONT></TD>
					<TD bgColor="#dadada"><asp:textbox id="LoginTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><asp:requiredfieldvalidator id="Requiredfieldvalidator4" runat="server" Font-Size="7pt" Font-Names="Verdana"
							ControlToValidate="LoginTB" ErrorMessage="Поле «Login» должно быть заполнено">*</asp:requiredfieldvalidator><FONT face="Verdana" size="2"></FONT></TD>
					<TD style="HEIGHT: 3px" align="right" bgColor="#dadada" height="3"><asp:textbox id="PassTB" runat="server" Visible="False" Width="10px"></asp:textbox><FONT face="Verdana" size="2">Тип:</FONT></TD>
					<TD style="HEIGHT: 24px" align="left" bgColor="#dadada" height="24"><asp:dropdownlist id="TypeDD" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:dropdownlist><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2">Домашний регион:</FONT></TD>
					<TD align="left" bgColor="#dadada"><asp:dropdownlist id=RegionDD runat="server" Font-Size="8pt" Font-Names="Verdana" AutoPostBack="True" DataSource="<%# admin %>" DataTextField="Region" DataValueField="RegionCode"></asp:dropdownlist><FONT face="Verdana" size="2"></FONT></TD>
					<TD style="HEIGHT: 31px" align="right" bgColor="#dadada" height="31"><FONT face="Verdana" size="2">Сегмент:</FONT></TD>
					<TD style="HEIGHT: 31px" align="left" bgColor="#dadada" height="23"><asp:dropdownlist id="SegmentDD" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:dropdownlist><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right" bgColor="#dadada" style="HEIGHT: 31px"><FONT face="Verdana" size="2"><asp:checkbox id="PayerPresentCB" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Плательщик существует"
								AutoPostBack="True"></asp:checkbox></FONT></TD>
					<TD align="left" bgColor="#dadada" style="HEIGHT: 31px"><asp:textbox id="PayerFTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
							Visible="False" Width="90px"></asp:textbox><asp:button id="FindPayerB" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Найти"
							Visible="False"></asp:button><asp:dropdownlist id=PayerDDL runat="server" Font-Size="8pt" Font-Names="Verdana" Visible="False" DataSource="<%# DataTable1 %>" DataTextField="PayerName" DataValueField="PayerID">
						</asp:dropdownlist><asp:label id="PayerCountLB" runat="server" ForeColor="Green" Font-Size="8pt" Font-Names="Verdana"
							Visible="False"></asp:label><FONT face="Verdana" size="2"></FONT></TD>
					<TD style="HEIGHT: 31px" align="right" bgColor="#dadada" height="31"><FONT face="Verdana" size="2"></FONT></TD>
					<TD style="HEIGHT: 31px" align="left" bgColor="#dadada" height="32"><asp:checkbox id="InvCB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
							Text='"Невидимый" клиент' Visible="False"></asp:checkbox><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
				<TR>
					<TD align="right" bgColor="#dadada"><asp:checkbox id="IncludeCB" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Подчиненный клиент"
							AutoPostBack="True"></asp:checkbox></TD>
					<TD style="HEIGHT: 20px" align="left" bgColor="#dadada"><asp:dropdownlist id=IncludeSDD runat="server" Font-Size="8pt" Font-Names="Verdana" Visible="False" DataSource="<%# Incudes %>" DataTextField="ShortName" DataValueField="FirmCode" AutoPostBack="True"></asp:dropdownlist><asp:textbox id="IncludeSTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
							Visible="False"></asp:textbox><asp:button id="IncludeSB" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Найти"
							Visible="False"></asp:button><asp:label id="IncludeCountLB" runat="server" ForeColor="Green" Font-Size="8pt" Font-Names="Verdana"
							Visible="False"></asp:label></TD>
					<TD style="HEIGHT: 20px" align="right" bgColor="#dadada" height="20"></TD>
					<TD style="HEIGHT: 20px" align="left" bgColor="#dadada" height="20"></TD>
				</TR>
				<TR>
					<TD align="right" height="25"><asp:regularexpressionvalidator id="RegularExpressionValidator1" runat="server" ControlToValidate="LoginTB" ErrorMessage="Ошибка в учетном имени"
							ValidationExpression="\w+([-+.]\w+)*" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="RegularExpressionValidator3" runat="server" ControlToValidate="EmailTB" ErrorMessage="Ошибка в e-mail"
							ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="RegularExpressionValidator2" runat="server" ControlToValidate="PhoneTB" ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"'
							ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="RegularExpressionValidator7" runat="server" ControlToValidate="TBAccountantPhone"
							ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="RegularExpressionValidator8" runat="server" ControlToValidate="TBClientManagerPhone"
							ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="Regularexpressionvalidator4" runat="server" ControlToValidate="TBOrderManagerMail"
							ErrorMessage="Ошибка в e-mail Order Manager" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="Regularexpressionvalidator5" runat="server" ControlToValidate="TBClientManagerMail"
							ErrorMessage="Ошибка в e-mail Client Manager" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="Regularexpressionvalidator6" runat="server" ControlToValidate="TBAccountantMail"
							ErrorMessage="Ошибка в e-mail Accountant" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:regularexpressionvalidator><asp:regularexpressionvalidator id="RegularExpressionValidator9" runat="server" ControlToValidate="TBOrderManagerPhone"
							ErrorMessage='Поле "Телефон" должно быть заполненно как "XXX(X)-XXXXXX(X)"' ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:regularexpressionvalidator><FONT face="Verdana" size="2"></FONT></TD>
					<TD height="25"><FONT face="Verdana" size="2">&nbsp; </FONT>
						<asp:validationsummary id="ValidationSummary1" runat="server" HeaderText="При регистрации возникли ошибки:"
							ShowSummary="False" ShowMessageBox="True"></asp:validationsummary></TD>
					<TD align="right" height="25"><FONT face="Verdana" size="2">&nbsp;</FONT></TD>
					<TD vAlign="top" align="left" height="25"><FONT face="Verdana" size="2">&nbsp;</FONT></TD>
				</TR>
				<TR>
					<TD vAlign="middle" align="left" colSpan="4" height="25">
						<P><FONT face="Verdana" size="2"></FONT>&nbsp;</P>
						<asp:CheckBox id="CheckBox1" runat="server" Text="Показывать все регионы" Font-Names="Verdana"
							Font-Size="10pt" Enabled="False"></asp:CheckBox>
						<TABLE id="Table2" borderColor="#dadada" cellSpacing="0" cellPadding="0" width="95%" align="center"
							border="1">
							<TR>
								<TD><FONT face="Verdana"><FONT size="2"><STRONG>Показываемые регионы:</STRONG>&nbsp;</FONT></FONT></TD>
								<TD><FONT face="Verdana" size="2"><STRONG>Доступные регионы:</STRONG></FONT></TD>
								<TD><FONT face="Verdana" size="2"><STRONG>Регионы работы:</STRONG></FONT></TD>
								<TD><FONT face="Verdana" size="2"><STRONG>Регионы заказа:</STRONG></FONT></TD>
							</TR>
							<TR>
								<TD><asp:checkboxlist id=ShowList runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
									</asp:checkboxlist><FONT face="Verdana" size="2"></FONT></TD>
								<TD><asp:checkboxlist id=WRList runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
									</asp:checkboxlist><FONT face="Verdana" size="2"></FONT></TD>
								<TD><asp:checkboxlist id=WRList2 runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
									</asp:checkboxlist><FONT face="Verdana" size="2"></FONT></TD>
								<TD><asp:checkboxlist id=OrderList runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
									</asp:checkboxlist><FONT face="Verdana" size="2"></FONT></TD>
							</TR>
						</TABLE>
						<P><FONT face="Verdana" size="2"></FONT>&nbsp;</P>
					</TD>
				</TR>
				<TR>
					<TD align="left" bgColor="#dadada" colSpan="4" height="25"><FONT face="Verdana" size="2"><STRONG>Дополнительные 
								сведения о клиенте</STRONG></FONT></TD>
				</TR>
				<TR>
					<td align="center" bgColor="#dadada" colSpan="4">
						<table cellSpacing="0" cellPadding="0" width="90%" align="left" bgColor="#ebebeb" border="0">
							<TR>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2">Менджер заказов:</FONT></TD>
								<TD align="right" width="30" bgColor="#dadada"><FONT face="Verdana" size="2"></FONT></TD>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2">Менджер клиентов:</FONT></TD>
								<TD align="right" width="30" bgColor="#dadada"><FONT face="Verdana" size="2"></FONT></TD>
								<TD align="right" bgColor="#dadada" style="width: 201px"><FONT face="Verdana" size="2">Бухгалтерия:</FONT></TD>
							</TR>
							<tr>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2">Имя:</FONT><asp:textbox id="TBOrderManagerName" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">Тел.:</FONT>
									<asp:textbox id="TBOrderManagerPhone" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">E-mail:</FONT>
									<asp:textbox id="TBOrderManagerMail" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
								</TD>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2"></FONT></TD>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2">Имя:</FONT><asp:textbox id="TBClientManagerName" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">Тел.:</FONT>
									<asp:textbox id="TBClientManagerPhone" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">E-mail:</FONT>
									<asp:textbox id="TBClientManagerMail" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
								</TD>
								<TD align="right" bgColor="#dadada"><FONT face="Verdana" size="2"></FONT></TD>
								<TD align="right" bgColor="#dadada" style="width: 201px"><FONT face="Verdana" size="2">Имя:</FONT><asp:textbox id="TBAccountantName" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">Тел.:</FONT>
									<asp:textbox id="TBAccountantPhone" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
									<FONT face="Verdana" size="2">E-mail:</FONT>
									<asp:textbox id="TBAccountantMail" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:textbox><br>
								</TD>
							</tr>
						</table>
					</td>
				</TR>
				<TR vAlign="bottom" height="50">
					<TD align="center" colSpan="4" style="height: 50px"><asp:button id="Register" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Зарегистрировать"></asp:button><FONT face="Verdana" size="2"></FONT></TD>
				</TR>
			</TABLE>
		</form>
	</body>
</HTML>
