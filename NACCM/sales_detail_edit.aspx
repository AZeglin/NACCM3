<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sales_detail_edit.aspx.vb" Inherits="NACCM.sales_detail_edit" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server" >   
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Sales Edit</title>
 </head>
<body style=" background-color:#ECE9D8">
    <form id="form1" runat="server">
    <asp:FormView ID="fvHeader" runat="server" 
        DataKeyNames="SIN,Quarter_ID,CntrctNum" DataSourceID="SalesDataSource">
       <ItemTemplate>
   <asp:Table ID="tblHeader" runat="server" CellPadding="5">
   <asp:TableRow><asp:TableCell>NAC CM Sales<br /> Report Edit</asp:TableCell><asp:TableCell><asp:Label ID="lbCntrctNum" runat="server" Text='<%#Eval("CntrctNum") %>' /><br /><asp:Label ID="lbTitle" runat="server" Text='<%#Eval("Title") %>' /></asp:TableCell><asp:TableCell><asp:Button ID="btnClose" runat="server" Text="Close" OnClick="btn_Close" /></asp:TableCell></asp:TableRow>
     </asp:Table>
        
          
        </ItemTemplate>
    </asp:FormView>
     <asp:GridView 
     ID="gvSalesData" 
     runat="server" 
     DataSourceID="SalesDataSource" 
     AutoGenerateColumns="False" 
     DataKeyNames="Quarter_ID,SIN,CntrctNum" 
     AutoGenerateEditButton="True">
     <Columns>
     <asp:BoundField DataField="SIN" HeaderText="SIN" ReadOnly="true" />
     <asp:BoundField DataField="VA_Sales" HeaderText="VA Sales" DataFormatString="{0:c}"  />
     <asp:BoundField DataField="OGA_Sales" HeaderText="OGA_Sales" DataFormatString="{0:c}" />
     
         <asp:BoundField DataField="SLG_Sales" DataFormatString="{0:c}" 
             HeaderText="S/C/L Govt. Sales" />
         <asp:BoundField DataField="Comments" HeaderText="Comments" runat="server"  />
     
     </Columns>
     </asp:GridView>
    <asp:SqlDataSource ID="SalesDataSource" runat="server" OnUpdating="SalesDataSource_OnUpdating"
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT CntrctNum, Quarter_ID, SIN, VA_Sales, OGA_Sales, SLG_Sales, Title, Comments FROM dbo.view_Sales_Edit WHERE (CntrctNum = @CntrctNum) AND (Quarter_ID = @QtrID)"
        
        
        UpdateCommand="UPDATE dbo.tbl_Cntrcts_Sales SET VA_Sales = CONVERT (Money, @VA_Sales), OGA_Sales = CONVERT (Money, @OGA_Sales), SLG_Sales = CONVERT (Money, @SLG_Sales), Comments = @Comments, LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate  WHERE (CntrctNum = @CntrctNum) AND (Quarter_ID = @Quarter_ID) AND (SIN = @SIN)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" />
            <asp:QueryStringParameter Name="QtrID" QueryStringField="QtrID" />
        </SelectParameters>
       <UpdateParameters>
       </UpdateParameters>
    </asp:SqlDataSource>
    </form>
</body>
</html>
