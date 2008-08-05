<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SenderProperties.aspx.cs" 
	Inherits="AdminInterface.SenderProperties" Theme="Main" MasterPageFile="~/Main.Master" %>


<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
    <form id="form1" runat="server">
    <div class="MainBlock">
		<h3 class="MainHeader">
			<asp:Label ID="Header" runat="server" />
		</h3>
		<div class="ContentBlock">
			<asp:GridView runat="server" ID="Properties" 
			AutoGenerateColumns="false" onrowcommand="Properties_RowCommand" onrowdeleting="Properties_RowDeleting">
			<EmptyDataTemplate>
				<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="Добавить свойство" />
			</EmptyDataTemplate>
			<Columns>
				<asp:TemplateField>
					<HeaderTemplate>
						<asp:Button ID="AddButton" CommandName="Add" runat="server" Text="Добавить" />
					</HeaderTemplate>
					<ItemTemplate>
						<asp:Button ID="DeleteButton" CommandName="Delete" runat="server" Text="Удалить" />
					</ItemTemplate>
					<ItemStyle Width="10%" />
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Свойство">
					<ItemTemplate>
						<asp:TextBox ID="Name" runat="server" Text='<%# Eval("Name") %>' />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Значение">
					<ItemTemplate>
						<asp:TextBox ID="Value" runat="server" Text='<%# Eval("Value") %>' />
					</ItemTemplate>
				</asp:TemplateField>
			</Columns>
			</asp:GridView>
		</div>
		<div class="Submit">
			<asp:Button ID="Save" runat="server" Text="Сохранить" onclick="Save_Click" />
		</div>
    </div>
    </form>
</asp:Content>
