<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditRegionalInfo.aspx.cs"
	Inherits="EditRegionalInfo" Theme="Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<div>
				<h4 class="MainHeader">
					<asp:Label ID="ClientInfoLabel" runat="server" />
				</h4>
			</div>
			<div class="BorderedBlock">
				<h3 class="Header">
					<asp:Label ID="RegionInfoLabel" runat="server" />
				</h3>
				<div class="ContentBlock">
					<label class="BaseLabelForInput" style="width: 150px;">
						Краткая информация:
					</label>
					<asp:TextBox ID="ContactInfoText" Width="60%" Height="300px" runat="server" TextMode="MultiLine" />
				</div>
				<div class="ContentBlock">
					<label class="BaseLabelForInput" style="width: 150px;">
						Информация:
					</label>
					<asp:TextBox ID="OperativeInfoText" Width="60%" Height="300px" runat="server" TextMode="MultiLine">
					</asp:TextBox>
				</div>
			</div>
			<div class="Submit">
				<asp:Button ID="SaveButton" runat="server" Text="Сохранить" OnClick="SaveButton_Click" />
				<asp:Button ID="CancelButton" runat="server" Text="Отменить" OnClick="CancelButton_Click" />
			</div>
		</div>
	</form>
</body>
</html>
