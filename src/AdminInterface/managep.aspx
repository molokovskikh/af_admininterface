<%@ Import namespace="System.ComponentModel"%>
<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.managep" CodeFile="managep.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Конфигурация пользователя</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
            <tr>
                <td valign="top" align="center">
                    <div align="center">
                        <font face="Verdana"><font size="2"><strong>Конфигурация клиента</strong> </font></font>
                        <asp:Label ID="NameLB" runat="server" Font-Size="9pt" Font-Names="Verdana" Font-Bold="True"></asp:Label></div>
                </td>
            </tr>
            <tr>
                <td valign="top" align="center">
                    <table id="Table4" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
                        align="center" border="1">
                        <tbody>
                            <tr>
                                <td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
                                    <p align="center">
                                        <font face="Verdana" size="2"><strong>Прайс-листы:</strong></font></p>
                                </td>
                            </tr>
                            <tr>
                                <td style="height: 8px" valign="middle" align="center" colspan="3">
                                    <br>
                                    <asp:Repeater ID="R" runat="server" OnItemCommand="R_ItemCommand" DataSource="<%# PD %>">
                                        <ItemTemplate>
                                            <tr bgcolor="#EEF8FF" style="font-size: 8pt">
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.PriceCode") %>
                                                </td>
                                                <td>
                                                    <asp:HyperLink ID="HL" runat="server" NavigateUrl='<%#"managecosts.aspx?pc=" + DataBinder.Eval(Container, "DataItem.PriceCode")%>'><%#DataBinder.Eval(Container, "DataItem.PriceName") %></asp:HyperLink>
                                                </td>
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.DateCurPrice")%>
                                                </td>
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.DateLastForm") %>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AgencyEnabled") %>'>
                                                    </asp:CheckBox>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.Enabled") %>'>
                                                    </asp:CheckBox>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AlowInt") %>'>
                                                    </asp:CheckBox>
                                                </td>
                                                <td align="center" bgcolor="#ebebeb">
                                                    <asp:CheckBox runat="server" Checked="False"></asp:CheckBox>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <AlternatingItemTemplate>
                                            <tr bgcolor="#F6F6F6" style="font-size: 8pt">
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.PriceCode") %>
                                                </td>
                                                <td>
                                                    <asp:HyperLink ID="HL" runat="server" NavigateUrl='<%#"managecosts.aspx?pc=" + DataBinder.Eval(Container, "DataItem.PriceCode")%>'><%# DataBinder.Eval(Container, "DataItem.PriceName") %>'</asp:HyperLink>
                                                </td>
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.DateCurPrice")%>
                                                </td>
                                                <td>
                                                    <%#DataBinder.Eval(Container, "DataItem.DateLastForm") %>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AgencyEnabled") %>'
                                                        ID="Checkbox1"></asp:CheckBox>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.Enabled") %>'
                                                        ID="Checkbox2"></asp:CheckBox>
                                                </td>
                                                <td align="center">
                                                    <asp:CheckBox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AlowInt") %>'
                                                        ID="Checkbox4"></asp:CheckBox>
                                                </td>
                                                <td align="center" bgcolor="#ebebeb">
                                                    <asp:CheckBox runat="server" Checked="False" ID="Checkbox5"></asp:CheckBox>
                                                </td>
                                            </tr>
                                        </AlternatingItemTemplate>
                                        <HeaderTemplate>
                                            <table cellspacing="0" cellpadding="0" rules="all" bordercolor="#DADADA" border="1"
                                                style="font-size: 8pt; border-left-color: #dadada; border-bottom-color: #dadada;
                                                border-top-style: solid; border-top-color: #dadada; font-family: Verdana; border-right-style: solid;
                                                border-left-style: solid; border-collapse: collapse; border-right-color: #dadada;
                                                border-bottom-style: solid">
                                                <tr bgcolor="#ebebeb">
                                                    <th width="70">
                                                        Код
                                                    </th>
                                                    <th width="140">
                                                        Наименование
                                                    </th>
                                                    <th width="140">
                                                        Получен
                                                    </th>
                                                    <th width="140">
                                                        Формализован
                                                    </th>
                                                    <th width="70">
                                                        Вкл.
                                                    </th>
                                                    <th width="70">
                                                        В работе
                                                    </th>
                                                    <th width="70">
                                                        Интегр.
                                                    </th>
                                                    <th width="70">
                                                        Удал.
                                                    </th>
                                                </tr>
                                        </HeaderTemplate>
                                        <FooterTemplate>
                                            <tr bgcolor="#ebebeb" style="font-weight: bold">
                                                <td colspan="5" align="right">
                                                    &nbsp;
                                                </td>
                                                <td colspan="3" align="center">
                                                    <asp:Button ID="Button2" runat="server" Font-Names="Verdana" BorderStyle="None" Text="Новый"
                                                        Font-Size="8pt"></asp:Button>
                                                </td>
                                            </tr>
                                            </TABLE>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                    <br>
                                </td>
                            </tr>
                            <tr>
                                <td valign="middle" align="right" colspan="3">
                                    <asp:Button ID="Button1" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Применить"
                                        BorderStyle="None"></asp:Button></td>
                            </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="top" align="center">
                    <div align="center">
                        &nbsp;</div>
                    <div align="center">
                        <table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
                            align="center" border="1">
                            <tr>
                                <td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
                                    <p align="center">
                                        <font face="Verdana" size="2"><strong>Региональная настройка</strong></font></p>
                                </td>
                            </tr>
                            <tr>
                                <td style="height: 8px" valign="middle" align="left" colspan="3">
                                    <font face="Verdana" size="2">Домашний регион:</font>
                                    <asp:DropDownList ID="RegionDD" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                        DataSource="<%# Regions %>" AutoPostBack="True" DataTextField="Region" DataValueField="RegionCode">
                                    </asp:DropDownList></td>
                            </tr>
                            <tr>
                                <td valign="middle" align="center" colspan="3">
                                    <table id="Table3" cellspacing="0" cellpadding="0" border="0">
                                        <tr>
                                            <td width="210">
                                                <font face="Arial" size="2"><strong><font face="Verdana">Показываемые регионы:</font></strong><font
                                                    face="Times New Roman" size="3">&nbsp;</font></font></td>
                                            <td width="210">
                                                <font face="Verdana" size="2"><strong>Доступные регионы:</strong></font></td>
                                        </tr>
                                        <tr>
                                            <td valign="top" align="left" width="210">
                                                <asp:CheckBoxList ID="ShowList" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                                    DataSource="<%# WorkReg %>" BorderStyle="None" DataTextField="Region" DataValueField="RegionCode"
                                                    CellSpacing="0" CellPadding="0">
                                                </asp:CheckBoxList></td>
                                            <td valign="top" align="left" width="210">
                                                <asp:CheckBoxList ID="WRList" runat="server" Font-Size="8pt" Font-Names="Verdana"
                                                    DataSource="<%# WorkReg %>" BorderStyle="None" DataTextField="Region" DataValueField="RegionCode"
                                                    CellSpacing="0" CellPadding="0">
                                                </asp:CheckBoxList></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td valign="middle" align="right" colspan="3">
                                    <asp:Button ID="ParametersSave" runat="server" Font-Size="10pt" Font-Names="Arial"
                                        Text="Применить" BorderStyle="None"></asp:Button></td>
                            </tr>
                            <tr>
                                <td valign="middle" align="center" colspan="3">
                                    <asp:Label ID="ResultL" runat="server" Font-Size="10pt" Font-Names="Arial" Font-Bold="True"
                                        ForeColor="Green" Font-Italic="True"></asp:Label></td>
                            </tr>
                        </table>
                    </div>
                    <div align="center">
                        <font face="Arial"><strong></strong></font>
                    </div>
                    <p align="center">
                        <font face="Arial, Helvetica, sans-serif"><font color="#000000" size="1">© АК "</font>
                            <a href="http://www.analit.net/"><font color="#800080" size="1">Инфорум</font></a><font
                                color="#000000" size="1">" 2004</font></font></p>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
