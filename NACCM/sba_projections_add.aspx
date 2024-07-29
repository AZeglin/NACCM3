<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sba_projections_add.aspx.vb" Inherits="NACCM.sba_projections_add" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SBA Projections</title>
</head>
<body style="background-color:#ECE9D8">
    <form id="form1" runat="server">
    <asp:Button ID="btnClose" runat="server" Text="Close" OnClick="btnClose_Click" />
      <table>
    <tr><td>StartDate</td><td><asp:TextBox runat="server" ID="tbStartDate"  /></td><td>EndDate</td><td><asp:TextBox runat="server" ID="tbEnddate"  /></td></tr>
    <tr><td>Total Subcontracting Dollars</td><td><asp:TextBox runat="server" ID="tbTotalDollars"   /></td><td></td><td></td></tr>
    <tr><td>SB Dollars:</td><td><asp:TextBox runat="server" ID="tbDollars"  OnTextChanged="CalInsertPercentage" AutoPostBack="true" /></td><td>SB Percent:</td><td>
    <asp:TextBox runat="server" ID="tbSBPercent"  ReadOnly="true" /></td></tr>
    <tr><td>Veteran Owned:</td><td><asp:TextBox runat="server" ID="tbVODollars" OnTextChanged="CalInsertPercentage" AutoPostBack="true"  /></td><td>Veteran Owned Percent:</td><td><asp:TextBox runat="server" ID="tbVOPercent"  ReadOnly="true"/></td></tr>
    <tr><td>Disabled Vet:</td><td><asp:TextBox runat="server" ID="tbDVDollars" OnTextChanged="CalInsertPercentage" AutoPostBack="true"  /></td><td>Disabled Vet Percent:</td><td><asp:TextBox runat="server" ID="tbDVPercent"  ReadOnly="true"/></td></tr>
    <tr><td>SDB Dollars:</td><td><asp:TextBox runat="server" ID="tbSDBDollars" OnTextChanged="CalInsertPercentage" AutoPostBack="true"  /></td><td>SDB Percent:</td><td><asp:TextBox runat="server" ID="tbSDBPercent"  ReadOnly="true" /></td></tr>
    <tr><td>Women Owned:</td><td><asp:TextBox runat="server" ID="tbWODollar" OnTextChanged="CalInsertPercentage" AutoPostBack="true" /></td><td>Women Owned Percent:</td><td><asp:TextBox runat="server" ID="tbWOPercent"  ReadOnly="true"/></td></tr>
    <tr><td>Hub Zone:</td><td><asp:TextBox runat="server" ID="tbHubDollars"  OnTextChanged="CalInsertPercentage" AutoPostBack="true" /></td><td>Hub Zone Percent:</td><td><asp:TextBox runat="server" ID="tbHubPercent"  ReadOnly="true" /></td></tr>
    <tr><td>HBCU Dollars:</td><td><asp:TextBox runat="server" ID="tbHBDollars" OnTextChanged="CalInsertPercentage" AutoPostBack="true" /></td><td>HBCU Percent:</td><td><asp:TextBox runat="server" ID="tbHBPercent"  ReadOnly="true" /></td></tr>
    <tr valign="top"><td>Comments</td><td colspan="3"><asp:TextBox runat="server" ID="tbComments" TextMode="MultiLine" Rows="5" Columns="50" /></td></tr>
    </table>
    <asp:Button ID="btnInsert" runat="server" Text="Save" />
    </form>
</body>
</html>
