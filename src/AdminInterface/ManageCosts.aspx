<%@ Page Language="c#" AutoEventWireup="true" CodeFile="ManageCosts.aspx.cs" Inherits="AddUser.ManageCosts" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>������� ���������� "�������" - ������������� �������� "�������"</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
        <table id="Table1" cellspacing="0" cellpadding="0" width="98%" border="0">
            <tr>
                <td style="height: 19px" align="center">
                    <font face="Verdana" size="2"><strong>��������� ��� ��� ����� - �����</strong> </font>
                    <asp:Label ID="PriceNameLB" runat="server" Font-Bold="True" Font-Names="Verdana"
                        Font-Size="9pt"></asp:Label></td>
            </tr>
            <tr>
                <td align="center">
                    <asp:DataGrid ID="CostsDG" runat="server" Font-Names="Verdana" Font-Size="8pt" BorderColor="#DADADA"
                        DataSource="<%# Costs %>" HorizontalAlign="Center" AutoGenerateColumns="False">
                        <FooterStyle HorizontalAlign="Center"></FooterStyle>
                        <AlternatingItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#EEF8FF">
                        </AlternatingItemStyle>
                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#F6F6F6"></ItemStyle>
                        <HeaderStyle Font-Bold="True" HorizontalAlign="Center" BackColor="#EBEBEB"></HeaderStyle>
                        <Columns>
                            <asp:TemplateColumn SortExpression="CostName" HeaderText="������������">
                                <HeaderStyle Width="160px"></HeaderStyle>
                                <ItemTemplate>
                                    <asp:TextBox ID="CostName" Font-Size="8pt" Font-Names="Verdana" runat="server" Width="150px"
                                        Text='<%#DataBinder.Eval(Container, "DataItem.CostName") %>'>
                                    </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfv1" ControlToValidate="CostName" runat="server"
                                        Text="*" ErrorMessage="���������� ������� ������������ ���� � �����, ��������� ������ *."
                                        Visible="true"></asp:RequiredFieldValidator>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="��������">
                                <HeaderStyle Width="100px"></HeaderStyle>
                                <ItemTemplate>
                                    <asp:CheckBox ID="Ena" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled"))%>'>
                                    </asp:CheckBox>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="�����������">
                                <HeaderStyle Width="100px"></HeaderStyle>
                                <ItemTemplate>
                                    <asp:CheckBox ID="Pub" runat="server" Checked='<%# Convert.ToBoolean(Eval("AgencyEnabled"))%>'>
                                    </asp:CheckBox>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn SortExpression="BaseCost" HeaderText="�������&lt;br&gt;����">
                                <HeaderStyle Width="80px"></HeaderStyle>
                                <ItemTemplate>
                                    <input value='<%# DataBinder.Eval(Container, "DataItem.CostCode")%>' type="radio"
                                       name="uid" <%# IsChecked(Convert.ToBoolean(DataBinder.Eval(Container, "DataItem.BaseCost"))) %> />
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:BoundColumn DataField="CostID" SortExpression="CostID" HeaderText="�������������">
                                <HeaderStyle Width="150px"></HeaderStyle>
                            </asp:BoundColumn>
                            <asp:BoundColumn Visible="False" DataField="CostCode" HeaderText="CostCode"></asp:BoundColumn>
                        </Columns>
                    </asp:DataGrid>
                    <asp:LinkButton ID="CreateCost" runat="server" OnClick="CreateCost_Click" Font-Names="Verdana" Font-Size="8pt">����� ������� �������</asp:LinkButton></td>
            </tr>
            <tr>
                <td height="10">
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Label ID="UpdateLB" runat="server" Font-Bold="True" Font-Names="Verdana" Font-Size="8pt"
                        Font-Italic="True" ForeColor="Green"></asp:Label></td>
            </tr>
            <tr>
                <td height="10">
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button ID="PostB" runat="server" OnClick="PostB_Click" Font-Names="Verdana" Font-Size="8pt" Text="���������">
                    </asp:Button></td>
            </tr>
            <tr>
                <td align="center" style="height: 14px">
                    <asp:Label ID="ErrLB" runat="server" Font-Names="Verdana" Font-Size="9pt" ForeColor="#C00000"></asp:Label></td>
            </tr>
            <tr>
                <td align="center">
                    <font face="Arial, Helvetica, sans-serif"><font face="veranda" color="#000000" size="1">
                        � �� "</font> <a href="http://www.analit.net/"><font face="veranda" size="1">�������</font></a><font
                            face="veranda" color="#000000" size="1">" 2005</font></font></td>
            </tr>
        </table>
    </form>
</body>
</html>
