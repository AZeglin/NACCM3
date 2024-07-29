<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sales_entry.aspx.vb" Inherits="NACCM.sales_entry" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Sales Entry</title>
</head>
<body>
    <form id="SalesEntryForm" runat="server" style="position:absolute; top:0px; left:0px; width:100%; height:100%;">
    <asp:Button ID="btnClose" runat="server" Text="Close Window" OnClientClick="window.close();" /><br />
    <div id="SalesEntryDiv"  style="width:100%; height:300px; overflow: scroll; border:solid 1px black; background-color:White; margin:3px;" >
        <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="False" 
            DataKeyNames="ID,CntrctNum,Quarter_ID,SIN" DataSourceID="SalesDataSource" ToolTip="Sales Breakdown">
            <Columns>
            <asp:TemplateField >
            <HeaderTemplate>
            Contract #
            </HeaderTemplate>
            <ItemTemplate>
            <asp:Label ID="lbContractNum" runat="server" Text='<%#Eval("CntrctNum")%>' ToolTip="Contract Number"/>
            </ItemTemplate>
            <FooterTemplate>
              </FooterTemplate>
            </asp:TemplateField>
             <asp:TemplateField>
            
             <HeaderTemplate>
             Quarter
             </HeaderTemplate>
             <ItemTemplate>
             <asp:Label ID="lbQuarter" runat="server" Text='<%#Eval("Title") %>' ToolTip="Quarter" />
             
             </ItemTemplate>
             <FooterTemplate>
              <asp:DropDownList ID="dlQuarterAdd" runat="server" 
            AppendDataBoundItems="True" DataSourceID="QuarterDataSource" 
            DataTextField="Title" DataValueField="Quarter_ID" ToolTip="Quarter"><asp:ListItem Text="" Value="" /></asp:DropDownList>
         
             </FooterTemplate>
             </asp:TemplateField>
               <asp:TemplateField>
               <HeaderTemplate>
               SIN
               </HeaderTemplate>
               <ItemTemplate>
               <asp:Label ID="lbSIN" runat="server" Text='<%#Eval("SIN") %>' ToolTip="SIN" />
               

               </ItemTemplate>
               <FooterTemplate>
                   SIN:<asp:DropDownList ID="dlSINAdd" runat="server" AppendDataBoundItems="True" 
                DataSourceID="SINDataSource" DataTextField="Sins" DataValueField="Sins" ToolTip="SIN"><asp:ListItem Text="" Value="" /></asp:DropDownList>
         
               </FooterTemplate>
               </asp:TemplateField>
               <asp:TemplateField>
               <HeaderTemplate>
               VA Sales
               </HeaderTemplate>
               <ItemTemplate>
               <asp:Label ID="lbVASales" runat="server" Text='<%#Eval("VA_Sales", "{0:c}") %>' ToolTip="VA Sales" />
               </ItemTemplate>
               <FooterTemplate>
        VA Sales: <asp:Textbox ID="tbVASalesAdd" runat="server" ToolTip="VA Sales" />        
               </FooterTemplate>
               </asp:TemplateField>
                      <asp:TemplateField>
                      <HeaderTemplate>
                      OGA Sales
                      </HeaderTemplate>
                      <ItemTemplate>
                      <asp:Label ID="lbOGASales" runat="server" Text='<%#Eval("OGA_Sales", "{0:c}") %>' ToolTip="OGA Sales" />
                      
                
                      </ItemTemplate>
                      <FooterTemplate>
                      OGA Sales:<asp:Textbox ID="tbOGASalesAdd" runat="server" ToolTip="OGA Sales" />
                      </FooterTemplate>
                      </asp:TemplateField>
                      <asp:TemplateField>
                      <HeaderTemplate>
                      S/C/L Govt Sales
                      </HeaderTemplate>
                      <ItemTemplate>
                      <asp:Label ID="lbSLG_Sales" runat="server" Text='<%#Eval("SLG_Sales","{0:c}") %>' ToolTip="State and Local Government Sales" />
                      </ItemTemplate>
                      <FooterTemplate>
        State and Local Government Sales:<asp:Textbox ID="tbSLGSalesAdd" runat="server" ToolTip="State and Local Government Sales" />
                      
                      </FooterTemplate>
                      </asp:TemplateField>
               <asp:TemplateField>
               <HeaderTemplate>
               Comments
               </HeaderTemplate>
               <ItemTemplate>
               <asp:Label ID="lbComments" runat="server" Text='<%#Eval("Comments") %>' ToolTip="Comments" />
               </ItemTemplate>
               <FooterTemplate>
               <asp:TextBox ID="tbComments" runat="server" TextMode="MultiLine" Rows="3" MaxLength="255" ToolTip="Comments" />
               </FooterTemplate>
               </asp:TemplateField>
                
                <asp:TemplateField>
                <ItemTemplate>
                <asp:Button ID="btnDelete" runat="server" CommandName="Delete" CommandArgument='<%# Container.DataItemIndex %>' Text="Delete" ToolTip="Delete Line" />
                </ItemTemplate>
                <FooterTemplate>
               <asp:Button ID="btnAddSalesNot" runat="server" CommandName="Insert" Text="Save" ToolTip="Save Entry" />
                </FooterTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
             <table><tr><td><asp:DropDownList ID="dlQuarterEmpty" runat="server" 
            AppendDataBoundItems="True" DataSourceID="QuarterDataSource" 
            DataTextField="Title" DataValueField="Quarter_ID"><asp:ListItem Text="" Value="" /></asp:DropDownList></td><td>
        SIN:<asp:DropDownList ID="dlSIN" runat="server" AppendDataBoundItems="True" 
                DataSourceID="SINDataSource" DataTextField="Sins" DataValueField="Sins"><asp:ListItem Text="" Value="" /></asp:DropDownList></td><td>
        VA Sales: <asp:Textbox ID="tbVASales" runat="server" /></td><td>
        OGA Sales:<asp:Textbox ID="tbOGASales" runat="server" /></td><td>
        State and Local Government Sales:<asp:Textbox ID="tbSLGSales" runat="server" />
        </td><td> Comments:<asp:Textbox ID="tbComments" runat ="server" TextMode="MultiLine" Rows="3" MaxLength="255" ToolTip="Comments"/> </td></tr></table>
        <asp:Button ID="btnAddSales" runat="server" Text="Save" OnClick="insertAddSales"/>
        
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
    
    <asp:Button ID="btnAddFooter" runat="server" Text="Add Sales" OnClick="ShowSalesFooter" ToolTip="Add Sales Info" />
    <asp:Button ID="btnSalesCancelFooter" runat="server" Text="Cancel" OnClick="ShowSalesFooter" ToolTip="Cancel Insert" />
    <asp:SqlDataSource ID="SalesDataSource" runat="server"  
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        
        SelectCommand="SELECT [tbl_Cntrcts_Sales].*, tlkup_year_qtr.Title  FROM [tbl_Cntrcts_Sales]
Join tlkup_year_qtr on tlkup_year_qtr.Quarter_ID = [tbl_Cntrcts_Sales].Quarter_ID
 WHERE ([CntrctNum] = @CntrctNum) ORDER BY [tbl_Cntrcts_Sales].Quarter_ID DESC " 
        DeleteCommand="DELETE FROM [tbl_Cntrcts_Sales] WHERE [CntrctNum] = @CntrctNum AND [Quarter_ID] = @Quarter_ID AND SIN = @SIN" 
        InsertCommand="INSERT INTO [tbl_Cntrcts_Sales] ([CntrctNum], [Quarter_ID], [SIN], [VA_Sales], [OGA_Sales], [SLG_Sales], [Comments], [LastModifiedBy], [LastModificationDate]) VALUES (@CntrctNum, @Quarter_ID, @SIN, @VA_Sales, @OGA_Sales, @SLG_Sales, @Comments, @LastModifiedBy, @LastModificationDate )" 
        OldValuesParameterFormatString="original_{0}" 
        
        UpdateCommand="UPDATE [tbl_Cntrcts_Sales] SET [ID] = @ID, [VA_Sales] = @VA_Sales, [OGA_Sales] = @OGA_Sales, [SLG_Sales] = @SLG_Sales, [Comments] = @Comments, [LastModifiedBy] = @LastModifiedBy, [LastModificationDate] = @LastModificationDate WHERE [CntrctNum] = @original_CntrctNum AND [Quarter_ID] = @original_Quarter_ID AND [SIN] = @original_SIN AND [ID] = @original_ID AND [VA_Sales] = @original_VA_Sales AND [OGA_Sales] = @original_OGA_Sales AND [SLG_Sales] = @original_SLG_Sales AND [Comments] = @original_Comments">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="CntrctNum" 
                QueryStringField="CntrctNum" Type="String" />
        </SelectParameters>
        <DeleteParameters>
            <asp:Parameter Name="CntrctNum" Type="String" />
            <asp:Parameter Name="Quarter_ID" Type="Int32" />
            <asp:Parameter Name="SIN" Type="String" />
          
        </DeleteParameters>
        <UpdateParameters>
            <asp:Parameter Name="ID" Type="Int32" />
            <asp:Parameter Name="VA_Sales" Type="Decimal" />
            <asp:Parameter Name="OGA_Sales" Type="Decimal" />
            <asp:Parameter Name="SLG_Sales" Type="Decimal" />
            <asp:Parameter Name="Comments" Type="String" />
            <asp:Parameter Name="LastModifiedBy" Type="String" />
            <asp:Parameter Name="LastModificationDate" Type="DateTime" />
            
        </UpdateParameters>
        <InsertParameters>
            <asp:Parameter Name="CntrctNum" Type="String" />
            <asp:Parameter Name="Quarter_ID" Type="Int32" />
            <asp:Parameter Name="SIN" Type="String" />
            <asp:Parameter Name="VA_Sales" Type="Decimal" />
            <asp:Parameter Name="OGA_Sales" Type="Decimal" />
            <asp:Parameter Name="SLG_Sales" Type="Decimal" />
            <asp:Parameter Name="Comments" Type="String" />
            <asp:Parameter Name="LastModifiedBy" Type="String" />
            <asp:Parameter Name="LastModificationDate" Type="DateTime" />
        </InsertParameters>
    </asp:SqlDataSource>
      <asp:SqlDataSource ID="QuarterDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT TOP 4 Quarter_ID, Title FROM tlkup_year_qtr WHERE (End_Date &lt; { fn NOW() }) ORDER BY Quarter_ID DESC">
    </asp:SqlDataSource><br />
     
    <asp:SqlDataSource ID="SINDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="sp_Contracted_SINS" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" DefaultValue="0" />
        </SelectParameters>
    </asp:SqlDataSource>
    </form>
</body>
</html>
