<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.searchc" CodePage="1251"
	CodeBehind="searchc.aspx.cs" Theme="Main" MasterPageFile="~/Main.Master" %>
<%@ Import Namespace="AdminInterface.Models.Security"%>
<%@ Import Namespace="AdminInterface.Security"%>



<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server" defaultbutton="GoFind">
		<h3>
			���������� ������ �������:
		</h3>
		<table style="margin-left:auto; margin-right: auto">
			<tr><td>
			<table style="background-color: #e7f6e0; padding: 10px; text-align:center; width: 450px;" id="Table2 onload="return SetSearchTitle();">
				<tr>
					<td colspan="3">
						��������� ����� �������:
					</td>
				</tr>
				<tr>
				
					<td style="width: 181px;">
						<asp:TextBox ID="FindTB" runat="server" />
						<asp:CustomValidator ID="SearchTextValidator" runat="server" ControlToValidate="FindTB"
							ErrorMessage="*" ClientValidationFunction="ValidateSearch" OnServerValidate="SearchTextValidator_ServerValidate" ValidateEmptyText="True" />
					</td>
					<td style="text-align: left;">
						<asp:RadioButtonList ID="FindRB" runat="server" BorderStyle="None" Width="120px">
							<asp:ListItem Value="Automate" Selected="True">��������������</asp:ListItem>
							<asp:ListItem Value="ShortName">���</asp:ListItem>
							<asp:ListItem Value="Code">���</asp:ListItem>
							<asp:ListItem Value="PayerId">�������</asp:ListItem>
							<asp:ListItem Value="Login">��� ������������</asp:ListItem>
							<asp:ListItem Value="JuridicalName">����������� ������������</asp:ListItem>
						</asp:RadioButtonList>
					</td>
					<td rowspan="2">
						<asp:Button ID="GoFind" runat="server" Text="�����" OnClick="GoFind_Click" CausesValidation="true" />
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						��� �������:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientType" runat="server">
							<asp:ListItem>���</asp:ListItem>
							<asp:ListItem>������</asp:ListItem>
							<asp:ListItem>����������</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						���������:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientState" runat="server">
							<asp:ListItem>���</asp:ListItem>
							<asp:ListItem>�������</asp:ListItem>
							<asp:ListItem>��������</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						������:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientRegion" runat="server" DataTextField="Region" DataValueField="RegionCode">
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td colspan="3">
						<asp:CheckBox ID="ADCB" runat="server" Text="���������� ������ Active Directory"
							Checked="True" />
					</td>
				</tr>
			</table>
			</td></tr>
		</table>
		<div style="margin-top: 20px;">
			<asp:GridView ID="ClientsGridView" CssClass="HighLightCurrentRow" runat="server" AutoGenerateColumns="False" DataSource='<%# ClientsDataView %>'
				OnRowDataBound="ClientsGridView_RowDataBound" AllowSorting="True" OnRowCreated="ClientsGridView_RowCreated"
				OnSorting="ClientsGridView_Sorting">
				<Columns>
					<asp:TemplateField HeaderText="�������" SortExpression="PayerId">
						<ItemTemplate>
							<asp:HyperLink ID="HyperLink1" 
											runat="server" 
											Enabled='<%# SecurityContext.Administrator.HavePermisions(PermissionType.Billing) %>' 
											Text='<%# Bind("PayerId") %>' 
											NavigateUrl='<%# String.Format("billing/edit.rails?ClientCode={0}", Eval("Id")) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="Id" HeaderText="���" SortExpression="Id"></asp:BoundField>
					<asp:TemplateField HeaderText="������������" SortExpression="Name">
						<ItemTemplate>
							<asp:HyperLink ID="HyperLink1" runat="server" 
								Text='<%# Bind("Name") %>' 
								NavigateUrl='<%# String.Format("client/{0}", Eval("Id")) %>'></asp:HyperLink>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="region" HeaderText="������" SortExpression="region"></asp:BoundField>
					<asp:TemplateField HeaderText="�������������� ����������" SortExpression="FirstUpdate">
						<ItemTemplate>
							<asp:Label ID="Label1" runat="server" Text='<%# Eval("FirstUpdate").ToString() %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="���������������� ����������" SortExpression="SecondUpdate">
						<ItemTemplate>
							<asp:Label ID="Label2" runat="server" Text='<%# Eval("SecondUpdate").ToString() %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="EXE" HeaderText="������" SortExpression="EXE"></asp:BoundField>
					<asp:BoundField DataField="UserName" HeaderText="��� ������������" SortExpression="UserName" />
					<asp:TemplateField HeaderText="�������" SortExpression="Segment">
						<ItemTemplate>
							<asp:Label ID="Label3" runat="server" Text='<%# Convert.ToInt32(Eval("Segment")) == 0 ? "���" : "�������" %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="���" SortExpression="FirmType">
						<ItemTemplate>
							<asp:Label ID="Label4" runat="server" Text='<%# Convert.ToUInt32(Eval("FirmType")) == 1 ? "������" : "���������" %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataRowStyle Font-Bold="True" HorizontalAlign="Center" />
				<EmptyDataTemplate>
					������ �� ������
				</EmptyDataTemplate>
			</asp:GridView>
			<table style="margin-top: 20px; margin-left: auto; margin-right: auto;">
				<tr>
				<td>
				<table runat=server id="Table4" visible=false>
					<tr>
						<td style="background-color:#FF6600; width:30px"></td>
						<td> - ������ ��������</td>
					</tr>
					<tr>
						<td class="DisabledLogin"></td>
						<td> - ������������ ��������</td>
					</tr>
					<tr>
						<td class="BlockedLogin"></td>
						<td> - ������������ ������������</td>
					</tr>
					<tr>
						<td class="LoginNotExists"></td>
						<td> - ������������ ������</td>
					</tr>
					<tr>
						<td class="not-base-client"></td>
						<td> - ������� ������</td>
					</tr>
					<tr>
						<td style="background-color:Gray"></td>
						<td> - ���������� ����� 2 ����� �����</td>
					</tr>
				</table>
				<div style="margin-top: 20px; text-align:center;">
					<asp:Label ID="SearchTimeLabel" Visible="false" runat="server" />
				</div>
				</td>
				</tr>
			</table>
		</div>
	</form>
</asp:Content>