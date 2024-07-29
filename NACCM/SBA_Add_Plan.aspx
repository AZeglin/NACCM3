<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="SBA_Add_Plan.aspx.vb" Inherits="NACCM.SBA_Add_Plan" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Add SBA Plan Record</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Label ID="lbTitle" runat="server" Text="Add Vendor SBA Record" />
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" />
    <asp:Button ID="btnSave" runat="server" Text="Save and Exit" />
    <table>
    <tr><td>Enter a Plan Name</td><td><asp:TextBox ID="tbPlan" runat="server" Width="200" /></td></tr>
    <tr><td>Select a Plan Type</td><td><asp:DropDownList ID="dlPlanType" runat="server" 
                    DataSourceID="PlanTypeDataSource" DataTextField="PlanTypeDescription" 
                    DataValueField="PlanTypeID" />
                <asp:SqlDataSource ID="PlanTypeDataSource" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:CM %>" 
                    SelectCommand="SELECT [PlanTypeID], [PlanTypeDescription] FROM [tbl_sba_PlanType]">
                </asp:SqlDataSource>
                </td></tr>
    </table>
    </form>
</body>
</html>
