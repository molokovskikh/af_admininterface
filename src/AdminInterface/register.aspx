<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.WebForm1" CodePage="1251"
    AspCompat="False" CodeFile="register.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Регистрация пользователей</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form method="post" runat="server" defaultbutton="Registe">
        <table id="Table1" cellspacing="0" cellpadding="0" width="100%" align="center" bgcolor="#ebebeb"
            border="0">
            <tr>
                <td align="right" colspan="4">
                    <p align="center">
                        <strong><font face="Verdana" size="2">Регистрация&nbsp;клиента</font></strong></p>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <asp:Label ID="Label3" runat="server" ForeColor="Red" Font-Size="8pt" Font-Bold="True"
                        Font-Names="Verdana"></asp:Label><asp:Label ID="Label2" runat="server" ForeColor="Red"
                            Font-Size="8pt" Font-Bold="True" Font-Names="Verdana"></asp:Label><font face="Verdana"
                                size="2"></font></td>
            </tr>
            <tr>
                <td align="right">
                    <font style="font-weight: bold" face="Verdana" color="#000000" size="2">Полное наименование:</font></td>
                <td align="left">
                    <asp:TextBox ID="FullNameTB" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        BorderStyle="None"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1"
                            runat="server" Font-Size="7pt" Font-Names="Verdana" ControlToValidate="FullNameTB"
                            ErrorMessage="Поле «Полное наименование» должно быть заполнено">*</asp:RequiredFieldValidator><font
                                face="Verdana" size="2"></font></td>
                <td align="right">
                    <font style="font-weight: bold" face="Verdana" size="2">Адрес:</font></td>
                <td align="left">
                    <asp:TextBox ID="AddressTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None">-</asp:TextBox><asp:RequiredFieldValidator
                        ID="RequiredFieldValidator6" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        ControlToValidate="AddressTB" ErrorMessage="Поле «Адрес» должно быть заполнено">*</asp:RequiredFieldValidator><font
                            face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right">
                    <font style="font-weight: bold" color="#000000"><font face="Verdana"><font size="2">
                        Краткое наименование:</font></font></font></td>
                <td align="left">
                    <asp:TextBox ID="ShortNameTB" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        BorderStyle="None"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator2"
                            runat="server" Font-Size="7pt" Font-Names="Verdana" ControlToValidate="ShortNameTB"
                            ErrorMessage="Поле «Краткое наименование» должно быть заполнено">*</asp:RequiredFieldValidator><font
                                face="Verdana" size="2"></font></td>
                <td align="right">
                    <div align="right">
                        <font face="Verdana" size="2">Виды транспорта:</font></div>
                </td>
                <td align="left">
                    <asp:TextBox ID="BusInfoTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None">-</asp:TextBox><font
                        face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right">
                    <font style="font-weight: bold" face="Verdana" size="2">Телефон:</font></td>
                <td align="left">
                    <asp:TextBox ID="PhoneTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><asp:RequiredFieldValidator
                        ID="RequiredFieldValidator3" runat="server" Font-Size="7pt" Font-Names="Verdana"
                        ControlToValidate="PhoneTB" ErrorMessage="Поле «Телефон» должно быть заполнено">*</asp:RequiredFieldValidator><font
                            face="Verdana" size="2"></font></td>
                <td align="right">
                    <font face="Verdana" size="2">Остановка транспорта:</font></td>
                <td align="left">
                    <asp:TextBox ID="BussStopTB" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        BorderStyle="None" Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].bussstop") %>'></asp:TextBox><font
                            face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right">
                    <font face="Verdana" size="2">Факс: </font>
                </td>
                <td align="left">
                    <asp:TextBox ID="FaxTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
                        Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].fax") %>'></asp:TextBox><font
                            face="Verdana" size="2"></font></td>
                <td align="right">
                    <font style="font-weight: bold" face="Verdana" size="2">E-mail:</font></td>
                <td align="left">
                    <asp:TextBox ID="EmailTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><asp:RequiredFieldValidator
                        ID="RequiredFieldValidator8" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        ControlToValidate="EmailTB" ErrorMessage="Поле «E-mail» должно быть заполнено">*</asp:RequiredFieldValidator><font
                            face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right">
                    <font face="Verdana" size="2">URL:</font></td>
                <td align="left">
                    <asp:TextBox ID="URLTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
                        Text='<%# DataBinder.Eval(DS1, "Tables[Clientsdata].DefaultView.[0].url") %>'></asp:TextBox><font
                            face="Verdana" size="2"></font></td>
                <td align="right">
                    <font face="Verdana" size="2">&nbsp;</font></td>
                <td align="left">
                    <font face="Verdana" size="2">&nbsp; </font>
                </td>
            </tr>
            <tr>
                <td align="right" bgcolor="#dadada">
                    <font style="font-weight: bold" face="Verdana" size="2">Login:</font></td>
                <td bgcolor="#dadada">
                    <asp:TextBox ID="LoginTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><asp:RequiredFieldValidator
                        ID="Requiredfieldvalidator4" runat="server" Font-Size="7pt" Font-Names="Verdana"
                        ControlToValidate="LoginTB" ErrorMessage="Поле «Login» должно быть заполнено">*</asp:RequiredFieldValidator><font
                            face="Verdana" size="2"></font></td>
                <td style="height: 3px" align="right" bgcolor="#dadada" height="3">
                    <asp:TextBox ID="PassTB" runat="server" Visible="False" Width="10px"></asp:TextBox><font
                        face="Verdana" size="2">Тип:</font></td>
                <td style="height: 24px" align="left" bgcolor="#dadada" height="24">
                    <asp:DropDownList ID="TypeDD" runat="server" Font-Size="8pt" Font-Names="Verdana">
                    </asp:DropDownList><font face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right" bgcolor="#dadada">
                    <font face="Verdana" size="2">Домашний регион:</font></td>
                <td align="left" bgcolor="#dadada">
                    <asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged" Font-Size="8pt" Font-Names="Verdana"
                        AutoPostBack="True" DataSource="<%# admin %>" DataTextField="Region" DataValueField="RegionCode">
                    </asp:DropDownList><font face="Verdana" size="2"></font></td>
                <td style="height: 31px" align="right" bgcolor="#dadada" height="31">
                    <font face="Verdana" size="2">Сегмент:</font></td>
                <td style="height: 31px" align="left" bgcolor="#dadada" height="23">
                    <asp:DropDownList ID="SegmentDD" runat="server" Font-Size="8pt" Font-Names="Verdana">
                    </asp:DropDownList><font face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right" bgcolor="#dadada" style="height: 31px">
                    <font face="Verdana" size="2">
                        <asp:CheckBox ID="PayerPresentCB" runat="server" OnCheckedChanged="PayerPresentCB_CheckedChanged" Font-Size="8pt" Font-Names="Verdana"
                            Text="Плательщик существует" AutoPostBack="True"></asp:CheckBox></font></td>
                <td align="left" bgcolor="#dadada" style="height: 31px">
                    <asp:TextBox ID="PayerFTB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
                        Visible="False" Width="90px"></asp:TextBox><asp:Button ID="FindPayerB" runat="server" OnClick="FindPayerB_Click"
                            Font-Size="8pt" Font-Names="Verdana" Text="Найти" Visible="False"></asp:Button><asp:DropDownList
                                ID="PayerDDL" runat="server" Font-Size="8pt" Font-Names="Verdana" Visible="False"
                                DataSource="<%# DataTable1 %>" DataTextField="PayerName" DataValueField="PayerID">
                            </asp:DropDownList><asp:Label ID="PayerCountLB" runat="server" ForeColor="Green"
                                Font-Size="8pt" Font-Names="Verdana" Visible="False"></asp:Label><font face="Verdana"
                                    size="2"></font></td>
                <td style="height: 31px" align="right" bgcolor="#dadada" height="31">
                    <font face="Verdana" size="2"></font>
                </td>
                <td style="height: 31px" align="left" bgcolor="#dadada" height="32">
                    <asp:CheckBox ID="InvCB" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
                        Text="&quot;Невидимый&quot; клиент" Visible="False"></asp:CheckBox><font face="Verdana" size="2"></font></td>
            </tr>
            <tr>
                <td align="right" bgcolor="#dadada">
                    <asp:CheckBox ID="IncludeCB" runat="server" OnCheckedChanged="IncludeCB_CheckedChanged" Font-Size="8pt" Font-Names="Verdana"
                        Text="Подчиненный клиент" AutoPostBack="True"></asp:CheckBox></td>
                <td style="height: 20px" align="left" bgcolor="#dadada">
                    <asp:DropDownList ID="IncludeSDD" runat="server" OnSelectedIndexChanged="IncludeSDD_SelectedIndexChanged" Font-Size="8pt" Font-Names="Verdana"
                        Visible="False" DataSource="<%# Incudes %>" DataTextField="ShortName" DataValueField="FirmCode"
                        AutoPostBack="True">
                    </asp:DropDownList><asp:TextBox ID="IncludeSTB" runat="server" Font-Size="8pt" Font-Names="Verdana"
                        BorderStyle="None" Visible="False"></asp:TextBox><asp:Button ID="IncludeSB" runat="server" OnClick="IncludeSB_Click"
                            Font-Size="8pt" Font-Names="Verdana" Text="Найти" Visible="False"></asp:Button><asp:Label
                                ID="IncludeCountLB" runat="server" ForeColor="Green" Font-Size="8pt" Font-Names="Verdana"
                                Visible="False"></asp:Label></td>
                <td style="height: 20px" align="right" bgcolor="#dadada" height="20">
                </td>
                <td style="height: 20px" align="left" bgcolor="#dadada" height="20">
                </td>
            </tr>
            <tr>
                <td align="right" height="25">
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="LoginTB"
                        ErrorMessage="Ошибка в учетном имени" ValidationExpression="\w+([-+.]\w+)*" Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator
                            ID="RegularExpressionValidator3" runat="server" ControlToValidate="EmailTB" ErrorMessage="Ошибка в e-mail"
                            ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator
                                ID="RegularExpressionValidator2" runat="server" ControlToValidate="PhoneTB" ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
                                ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator
                                    ID="RegularExpressionValidator7" runat="server" ControlToValidate="TBAccountantPhone"
                                    ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;" ValidationExpression="(\d{3,4})-(\d{6,7})"
                                    Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator ID="RegularExpressionValidator8"
                                        runat="server" ControlToValidate="TBClientManagerPhone" ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
                                        ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator
                                            ID="Regularexpressionvalidator4" runat="server" ControlToValidate="TBOrderManagerMail"
                                            ErrorMessage="Ошибка в e-mail Order Manager" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                            Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator ID="Regularexpressionvalidator5"
                                                runat="server" ControlToValidate="TBClientManagerMail" ErrorMessage="Ошибка в e-mail Client Manager"
                                                ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator
                                                    ID="Regularexpressionvalidator6" runat="server" ControlToValidate="TBAccountantMail"
                                                    ErrorMessage="Ошибка в e-mail Accountant" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                                    Display="None"></asp:RegularExpressionValidator><asp:RegularExpressionValidator ID="RegularExpressionValidator9"
                                                        runat="server" ControlToValidate="TBOrderManagerPhone" ErrorMessage="Поле &quot;Телефон&quot; должно быть заполненно как &quot;XXX(X)-XXXXXX(X)&quot;"
                                                        ValidationExpression="(\d{3,4})-(\d{6,7})" Display="None"></asp:RegularExpressionValidator><font
                                                            face="Verdana" size="2"></font></td>
                <td height="25">
                    <font face="Verdana" size="2">&nbsp; </font>
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="При регистрации возникли ошибки:"
                        ShowSummary="False" ShowMessageBox="True"></asp:ValidationSummary>
                </td>
                <td align="right" height="25">
                    <font face="Verdana" size="2">&nbsp;</font></td>
                <td valign="top" align="left" height="25">
                    <font face="Verdana" size="2">&nbsp;</font></td>
            </tr>
            <tr>
                <td valign="middle" align="left" colspan="4" height="25">
                    <p>
                        <font face="Verdana" size="2"></font>&nbsp;</p>
                    <asp:CheckBox ID="CheckBox1" runat="server" Text="Показывать все регионы" Font-Names="Verdana"
                        Font-Size="10pt" Enabled="False"></asp:CheckBox>
                    <table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
                        align="center" border="1">
                        <tr>
                            <td>
                                <font face="Verdana"><font size="2"><strong>Показываемые регионы:</strong>&nbsp;</font></font></td>
                            <td>
                                <font face="Verdana" size="2"><strong>Доступные регионы:</strong></font></td>
                            <td>
                                <font face="Verdana" size="2"><strong>Регионы работы:</strong></font></td>
                            <td>
                                <font face="Verdana" size="2"><strong>Регионы заказа:</strong></font></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:CheckBoxList ID="ShowList" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
                                </asp:CheckBoxList><font face="Verdana" size="2"></font></td>
                            <td>
                                <asp:CheckBoxList ID="WRList" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
                                </asp:CheckBoxList><font face="Verdana" size="2"></font></td>
                            <td>
                                <asp:CheckBoxList ID="WRList2" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
                                </asp:CheckBoxList><font face="Verdana" size="2"></font></td>
                            <td>
                                <asp:CheckBoxList ID="OrderList" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode">
                                </asp:CheckBoxList><font face="Verdana" size="2"></font></td>
                        </tr>
                    </table>
                    <p>
                        <font face="Verdana" size="2"></font>&nbsp;</p>
                </td>
            </tr>
            <tr>
                <td align="left" bgcolor="#dadada" colspan="4" height="25">
                    <font face="Verdana" size="2"><strong>Дополнительные сведения о клиенте</strong></font></td>
            </tr>
            <tr>
                <td align="center" bgcolor="#dadada" colspan="4">
                    <table cellspacing="0" cellpadding="0" width="90%" align="left" bgcolor="#ebebeb"
                        border="0">
                        <tr>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2">Менджер заказов:</font></td>
                            <td align="right" width="30" bgcolor="#dadada">
                                <font face="Verdana" size="2"></font>
                            </td>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2">Менджер клиентов:</font></td>
                            <td align="right" width="30" bgcolor="#dadada">
                                <font face="Verdana" size="2"></font>
                            </td>
                            <td align="right" bgcolor="#dadada" style="width: 201px">
                                <font face="Verdana" size="2">Бухгалтерия:</font></td>
                        </tr>
                        <tr>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2">Имя:</font><asp:TextBox ID="TBOrderManagerName" runat="server"
                                    Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">Тел.:</font>
                                <asp:TextBox ID="TBOrderManagerPhone" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">E-mail:</font>
                                <asp:TextBox ID="TBOrderManagerMail" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                            </td>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2"></font>
                            </td>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2">Имя:</font><asp:TextBox ID="TBClientManagerName" runat="server"
                                    Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">Тел.:</font>
                                <asp:TextBox ID="TBClientManagerPhone" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">E-mail:</font>
                                <asp:TextBox ID="TBClientManagerMail" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                            </td>
                            <td align="right" bgcolor="#dadada">
                                <font face="Verdana" size="2"></font>
                            </td>
                            <td align="right" bgcolor="#dadada" style="width: 201px">
                                <font face="Verdana" size="2">Имя:</font><asp:TextBox ID="TBAccountantName" runat="server"
                                    Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">Тел.:</font>
                                <asp:TextBox ID="TBAccountantPhone" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                                <font face="Verdana" size="2">E-mail:</font>
                                <asp:TextBox ID="TBAccountantMail" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                    BorderStyle="None"></asp:TextBox><br>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr valign="bottom" height="50">
                <td align="center" colspan="4" style="height: 50px">
                    <asp:Button ID="Register" runat="server" OnClick="Register_Click" Font-Size="8pt" Font-Names="Verdana" Text="Зарегистрировать">
                    </asp:Button><font face="Verdana" size="2"></font></td>
            </tr>
        </table>
    </form>
</body>
</html>
