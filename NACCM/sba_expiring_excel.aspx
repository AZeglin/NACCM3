<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sba_expiring_excel.aspx.vb" Inherits="NACCM.sba_expiring_excel" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
     <asp:gridview ID="gvExpiringSBA" runat="server" AutoGenerateColumns="False" 
        CellPadding="4" DataSourceID="ExpiringDataSource" ForeColor="#333333" 
        GridLines="Both" Font-Names="Arial" Visible="false" AllowSorting="true" >
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <Columns>
            <asp:BoundField DataField="LastName" HeaderText="CO" 
                SortExpression="LastName" />
            <asp:BoundField DataField="Contractor_Name" HeaderText="Vendor" 
                SortExpression="Contractor_Name" />
            <asp:BoundField DataField="Expr1" HeaderText="Expiration" SortExpression="Expr1" DataFormatString="{0:d}" />
            <asp:BoundField DataField="TeamLead" HeaderText="Asst. Director" 
                SortExpression="TeamLead" />
        </Columns>
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#999999" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    </asp:gridview>
    <asp:SqlDataSource ID="ExpiringDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT [LastName], [Contractor_Name], [Expr1], [TeamLead] FROM [Report_SBA_Plans_Expiring]">
    </asp:SqlDataSource>
    </form>
</body>
</html>
