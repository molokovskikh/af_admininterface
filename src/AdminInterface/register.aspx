<%@ Page MaintainScrollPositionOnPostback="true" Theme="Main" Language="c#" AutoEventWireup="true"
	Inherits="AdminInterface.RegisterPage" CodeBehind="register.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Регистрация пользователей</title>
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
						<strong>Регистрация клиента</strong>
					</p>
				</td>
			</tr>
			<tr>
				<td colspan="4">
					<div id="Info" class="Fields">
						<div class="TwoColumn">
							<div class="SimpleField">
								<label class="Required" for="FullNameTB">
									Юридическое наименование:
								</label>
								<asp:TextBox ID="FullNameTB" runat="server"></asp:TextBox>
								<asp:RegularExpressionValidator ID="FullNameValidator" runat="server" ErrorMessage="Длинна строки не может быть больше чем 40 символов" ValidationExpression="^.{1,40}$" ValidationGroup="0" ControlToValidate="FullNameTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredFullName" runat="server" ControlToValidate="FullNameTB"
									ErrorMessage="Поле «Полное наименование» должно быть заполнено" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
							<div class="SimpleField">
								<label class="Required" for="ShortNameTB">
									Краткое наименование:
								</label>
								<asp:TextBox ID="ShortNameTB" runat="server" />
								<asp:RegularExpressionValidator ID="ShortNameValidator" runat="server"
									ErrorMessage="Длинна строки не может быть больше чем 50 символов" ValidationExpression="^.{1,50}$" ValidationGroup="0" ControlToValidate="ShortNameTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredShortName" runat="server" ControlToValidate="ShortNameTB"
									ErrorMessage="Поле «Краткое наименование» должно быть заполнено" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
							<div class="SimpleField">
								<label class="Required" for="AddressTB">
									Адрес доставки медикаментов:
								</label>
								<asp:TextBox ID="AddressTB" runat="server" />
								<asp:RegularExpressionValidator ID="AddressValidator" runat="server"
									ErrorMessage="Длинна строки не может быть больше чем 100 символов" ValidationExpression="^.{1,100}$" ValidationGroup="0" ControlToValidate="AddressTB" Display="Dynamic">*</asp:RegularExpressionValidator>
								<asp:RequiredFieldValidator ID="RequiredAddress" runat="server" ControlToValidate="AddressTB"
									ErrorMessage="Поле «Адрес доставки медикаментов» должно быть заполнено" ValidationGroup="0" Display="Dynamic">*</asp:RequiredFieldValidator></div>
						</div>
						<div class="TwoColumn">
							<div class="SimpleField">
								<label class="Required" for="PhoneTB">
									Телефон:
								</label>
								<asp:TextBox ID="PhoneTB" runat="server" />
								<asp:RequiredFieldValidator ID="RequiredPhone" 
															runat="server" 
															ControlToValidate="PhoneTB"
															ErrorMessage="Поле «Телефон» должно быть заполнено" ValidationGroup="0">*</asp:RequiredFieldValidator>
								<asp:RegularExpressionValidator ID="RegularExpressionValidator2" 
																runat="server" ControlToValidate="PhoneTB"
																ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
																ValidationExpression="\s*(\d{3,4})-(\d{6,7})\s*" 
																Display="None" 
																ValidationGroup="0" />
							</div>
							<div class="SimpleField">
								<label for="EmailTB">
									E-mail:
								</label>
								<asp:TextBox ID="EmailTB" runat="server" />
								<asp:RegularExpressionValidator ID="RegularExpressionValidator1" 
																runat="server" 
																ControlToValidate="EmailTB"
																ErrorMessage="Ошибка в e-mail" 
																ValidationExpression="\s*\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*\s*"
																Display="None" 
																ValidationGroup="0" />
							</div>
							<div class="SimpleField">
								<label for="LoginTB">
									Имя пользователя:
								</label>
								<asp:TextBox ID="LoginTB" runat="server" />
								<asp:CustomValidator ID="LoginValidator" 
													runat="server" 
													ErrorMessage="Поле «Имя пользователя» должно быть заполнено"
													ControlToValidate="LoginTB" 
													OnServerValidate="LoginValidator_ServerValidate"
													ValidationGroup="0" 
													EnableClientScript="False" 
													ValidateEmptyText="True">*</asp:CustomValidator>
													
								<asp:RegularExpressionValidator ID="LoginValidator1" 
																runat="server" 
																ControlToValidate="LoginTB"
																ErrorMessage="Имя пользователя должно начинаться с латинской буквы, может содержать буквы латинского алфавита, цифры и символ подчеркивания, другие символы не допускаются." 
																ValidationExpression="^\s*[a-z][a-z|0-9|_]+\s*$"
																Display="None" 
																ValidationGroup="0" />
							</div>
						</div>
					</div>
					<div id="ExtInfo" class="Fields">
						<div class="TwoColumn">
							<div class="ComplexField">
								<asp:CheckBox ID="PayerPresentCB" runat="server" OnCheckedChanged="PayerPresentCB_CheckedChanged"
									Text="Плательщик существует" AutoPostBack="True"></asp:CheckBox>
								<asp:TextBox ID="PayerFTB" runat="server" Visible="False" Width="90px" />
								<asp:Button ID="FindPayerB" runat="server" OnClick="FindPayerB_Click" Text="Найти"
									Visible="False" ValidationGroup="1" />
								<asp:DropDownList ID="PayerDDL" runat="server" Visible="False"
									DataTextField="PayerName" DataValueField="PayerID" />
								<asp:Label ID="PayerCountLB" runat="server" ForeColor="Green" Visible="False" />
							</div>
							<div class="ComplexField">
							</div>
							<div>
								<asp:CheckBox runat="server" ID="ServiceClient" Text="Сотрудник АК Инфорум" />
							</div>
							<div>
								<asp:CheckBox ID="EnterBillingInfo" runat="server" Text="Заполнять информацию для биллинга" Checked="true" />
							</div>
							<div>
								<asp:CheckBox ID="ShowRegistrationCard" runat="server" Checked="true" Text="Показывать регистрационную карту" />
							</div>
							<div>
								<fieldset>
								<legend>
									<asp:CheckBox ID="SendRegistrationCard" runat="server" Checked="true" Text="Отправлять регистрационную карту клиенту" />
								</legend>
								<table>
									<tr>
										<td style="width: 40%">
											Дополнительные адреса для отправки регистрационной карты:
										</td>
										<td style="width: 40%">
											<asp:TextBox ID="AdditionEmailToSendRegistrationCard" runat="server" TextMode="MultiLine" Width="100%"></asp:TextBox>
										</td>
									</tr>
								</table>
								</fieldset>
							</div>
						</div>
						<div class="TwoColumn">
							<div class="DropDownField">
								<label for="">
									Тип клиента в интерфейсе поставщика:
								</label>
								<div style="float:left">
									<asp:DropDownList ID="CustomerType" runat="server" Visible="true" OnSelectedIndexChanged="CustomerTypeChanged" AutoPostBack="True">
										<asp:ListItem Value="0" Text="Стандартный" />
										<asp:ListItem Value="2" Text="Скрытый" />
									</asp:DropDownList>
									<asp:TextBox ID="SupplierSearchText" runat="server" Visible="False" Width="90px" />
									<asp:Button ID="SearchSupplier" runat="server" OnClick="SearchSupplierClick" Text="Найти" Visible="False" ValidationGroup="1" />
									<asp:DropDownList ID="Suppliers" runat="server" Visible="False" DataTextField="SupplierName" DataValueField="SupplierId" />
									<br />
									<asp:CustomValidator runat=server ValidateEmptyText=true ErrorMessage="Необходимо указать поставщика для которого регистрируется данная копия" ValidationGroup="0" OnServerValidate="ValidateSupplier"></asp:CustomValidator>
								</div>
							</div>
							<div class="DropDownField">
								<label for="TypeDD">Тип:</label>
								<asp:DropDownList ID="TypeDD" runat="server" onselectedindexchanged="ClientTypeChanged" AutoPostBack="True" />
							</div>
							<div class="DropDownField">
								<label for="RegionDD">Домашний регион:</label>
								<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
									AutoPostBack="True" DataTextField="Region" DataValueField="RegionCode" />
							</div>
							<div class="DropDownField">
								<label for="SegmentDD">Сегмент:</label>
								<asp:DropDownList ID="SegmentDD" runat="server" />
							</div>
						</div>
					</div>
					<div runat="server" id="PermissionsDiv">
						<fieldset>
							<legend>Права доступа</legend>
							<asp:CheckBoxList ID="Permissions" runat="server" DataValueField="Id" DataTextField="Name">
							</asp:CheckBoxList>
						</fieldset>
					</div>
				</td>
			</tr>
			<tr>
				<td align="right" height="25">
					<font face="Verdana" size="2"></font>
				</td>
				<td height="25">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="При регистрации возникли ошибки:"
						ShowSummary="False" ShowMessageBox="True" ValidationGroup="0" />
				</td>
			</tr>
			<tr>
				<td valign="middle" align="left" colspan="4" style="height: 25px">
					<p>
					</p>
					<asp:CheckBox ID="CheckBox1" runat="server" Text="Показывать все регионы" OnCheckedChanged="CheckBox1_CheckedChanged"
						AutoPostBack="True"></asp:CheckBox>
					<table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
						align="center" border="1">
						<tr>
							<td style="height: 18px">
								<strong>Доступные регионы:</strong>
							</td>
							<td id="OrderRegionsLabel" runat=server style="height: 18px">
								<strong>Регионы заказа:</strong>
							</td>
						</tr>
						<tr>
							<td>
								<asp:CheckBoxList ID="WRList" runat="server" BorderStyle="None"
									DataTextField="Region" DataValueField="RegionCode" />
							</td>
							<td>
								<asp:CheckBoxList ID="OrderList" runat="server" BorderStyle="None"
									DataTextField="Region" DataValueField="RegionCode" />
							</td>
						</tr>
					</table>
					<p>
				</td>
			</tr>
			<tr>
				<td style="background-color: #dadada" colspan="4">
					<strong>Дополнительные сведения о клиенте</strong>
					<div id="Phones" class="Fields">
						<div class="TwoColumn">
							<span id="OrderManagerGroupLabel" runat="server">
								Ответственный за работу с программой:
							</span>
							<div class="SimpleField">
								<label for="TBOrderManagerName">
									Имя:
								</label>
								<asp:TextBox ID="TBOrderManagerName" runat="server" />
								<asp:RegularExpressionValidator ID="OrderManagerNameValidator" runat="server" ErrorMessage="Длинна строки не может быть больше чем 100 символов" ValidationGroup="0" ValidationExpression="^.{1,100}$" ControlToValidate="TBOrderManagerName" Display="Dynamic" >*</asp:RegularExpressionValidator>
							</div>
							<div class="SimpleField">
								<label for="TBOrderManagerPhone">
									Тел.:
								</label>
								<asp:TextBox ID="TBOrderManagerPhone" runat="server" />
								<asp:RegularExpressionValidator ID="RegularExpressionValidator9" 
																runat="server" 
																ControlToValidate="TBOrderManagerPhone"
																ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
																ValidationExpression="\s*(\d{3,4})-(\d{6,7})\s*" 
																Display="None" 
																ValidationGroup="0" />
							</div>
							<div class="SimpleField">
								<label for="TBOrderManagerMail">
									E-mail:
								</label>
								<asp:TextBox ID="TBOrderManagerMail" runat="server" />
								<asp:RegularExpressionValidator ID="Regularexpressionvalidator4" 
																runat="server" 
																ControlToValidate="TBOrderManagerMail"
																ErrorMessage="Ошибка в e-mail Order Manager" 
																ValidationExpression="\s*\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*\s*"
																Display="None" 
																ValidationGroup="0" />
							</div>
						</div>
						<div class="TwoColumn" id="ClientManagerGropBlock" runat="server" visible="false">
							<span>
								Управляющий в интерфейсе:
							</span>
							<div class="SimpleField">
								<label for="TBClientManagerName">
									Имя:
								</label>
								<asp:TextBox ID="TBClientManagerName" runat="server" />
								<asp:RegularExpressionValidator ID="ClientManagerNameValidator" runat="server"	ErrorMessage="Длинна строки не может быть больше чем 100 символов" ValidationGroup="0" ValidationExpression="^.{1,100}$" ControlToValidate="TBClientManagerName" Display="Dynamic" >*</asp:RegularExpressionValidator>
							</div>
							<div class="SimpleField">
								<label for="TBClientManagerPhone">
									Тел.:
								</label>
								<asp:TextBox ID="TBClientManagerPhone" runat="server" />
								<asp:RegularExpressionValidator ID="RegularExpressionValidator8" 
																runat="server" 
																ControlToValidate="TBClientManagerPhone"
																ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
																ValidationExpression="\s*(\d{3,4})-(\d{6,7})\s*"
																Display="None" 
																ValidationGroup="0"></asp:RegularExpressionValidator>
							</div>
							<div class="SimpleField">
								<label for="TBClientManagerMail">
									E-mail:
								</label>
								<asp:TextBox ID="TBClientManagerMail" runat="server" />
								<asp:RegularExpressionValidator ID="Regularexpressionvalidator5" 
																runat="server" 
																ControlToValidate="TBClientManagerMail"
																ErrorMessage="Ошибка в e-mail Client Manager" 
																ValidationExpression="\s*\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*\s*"
																Display="None" 
																ValidationGroup="0" />

							</div>
						</div>
					</div>
				</td>
			</tr>
			<tr valign="bottom" height="50">
				<td align="center" colspan="4" style="height: 50px">
					<asp:Button ID="Register" runat="server" OnClick="Register_Click" Text="Зарегистрировать"
						ValidationGroup="0" />
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
