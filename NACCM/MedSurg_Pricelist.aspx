<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="MedSurg_Pricelist.aspx.vb" Inherits="NACCM.MedSurg_Pricelist" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<!DOCTYPE html />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Pricelist</title>  
       <script type="text/javascript">
       <!--
            function CloseWindow()
            {
                window.opener.document.forms[0].fvContractInfo$RefreshPricelistScreenOnSubmit.value = "true";                
                
                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }

            function CloseWindow2() 
            {
                window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ItemPriceCountsFormView$RefreshPricelistScreenOnSubmit.value = "true";

                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('', '_parent', '');
                top.window.close();
            }
        //-->
        </script>
</head>
<body style="background-color: #ece9d8">
    <form id="MedSurgPricelistForm" runat="server"  >
                
        <asp:Panel ID="hiddenFieldPanel" runat="server"  Width="0"  >
            <asp:HiddenField ID="RefreshPricelistGridOnSubmit" runat="server"  Value="false" />
        </asp:Panel>       
        
        <asp:FormView ID="fvFSSPriceList" runat="server" 
            DataSourceID="FSSPriceListSummaryDataSource" InsertRowStyle-BackColor="#ece9d8" 
             Width="700px" Visible="false" >
          
            <ItemTemplate>
                
               <table style="width:700px; background-color:#ece9d8">
                    <tr style="font-family:Arial; color:#000099">
                        <td>
                        </td>
                        <td>
                        </td>
                        <td>
                        </td>
                        <td>
                        </td>
                        <td align="right" style="width:50px">
                            <asp:Button ID="formCloseButton" runat="server" Text="Close"  />
                        </td>                                           
                    </tr>                        
                    <tr style="font-family:Arial; color:#000099">
                        <td>
                        </td>                       
                        <td style="width:100px">
                            <asp:Label ID="CntrctNumLabel" runat="server" Text='<%# Bind("CntrctNum") %>' />
                        </td>
                        <td style="width:350px">
                            <asp:Label ID="Contractor_NameLabel" runat="server" Text='<%# Bind("Contractor_Name") %>' />
                        </td>
                        <td style="width:200px">
                            <div style=" font-size:12px">Total Number of prices:</div>
                            <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("Count") %>' />
                        </td>
                        <td align="right" style="width:50px">
                            <asp:Button ID="btnPrint" runat="server" Text="Print" OnClick="windowPrint" />
                        </td>
                    </tr>
                </table>
            </ItemTemplate>
            
            <InsertRowStyle BackColor="#ECE9D8" />
        </asp:FormView>
        <asp:SqlDataSource ID="FSSPriceListSummaryDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
            SelectCommand="SELECT * FROM [view_Pricelist_Count] WHERE ([CntrctNum] = @CntrctNum)">
            <SelectParameters>
                <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            </SelectParameters>
        </asp:SqlDataSource>
     
        <asp:Button ID="btnAddFSSPrice" runat="server" Text="Add Item" Visible="false" OnClick="btnAddFSSPrice_Click" />
        
        <asp:Panel ID="pnlAddFSSPrice" runat="server" Visible="false">
            <table width="700px" style="table-layout:fixed">
                <tr>
                    <td style="width: 140px">Contractor Catalog Number</td>
                    <td style="width: 100px">SIN</td>
                    <td style="width: 100px">FSS Price</td>
                    <td style="width: 100px">Package Size Priced On Contract</td>
                    <td style="width: 100px">Price Start Date</td>
                    <td style="width: 100px">Price End Date</td>
                </tr>
                <tr>
                    <td style="width: 140px"><asp:TextBox ID="tbAddCatalogNumber" runat="server"></asp:TextBox></td>
                    <td style="width: 100px"><asp:DropDownList ID="dlAddSin" runat="server" DataSourceID="SINDataSource" DataTextField="SINs" DataValueField="SINs" Width="99%"></asp:DropDownList></td>
                    <td style="width: 100px"><asp:TextBox id="tbAddFSSPrice" runat="server" /></td>
                    <td style="width: 100px"><asp:DropDownList ID="dlAddPackage" runat="server" 
                            DataSourceID="PackageDataSource" DataTextField="Short" DataValueField="Short" width="99%"/></td>
                    <td style="width: 100px"><asp:TextBox ID="FSSEffectiveDateTextBox" runat="server" Width="99%"  /></td>
                    <td style="width: 100px"><asp:TextBox ID="FSSExpirationDateTextBox" runat="server"  Width="99%" /></td>
                </tr>
                <tr>
                    <td colspan="2" align="right" style="width: 240px;">Product Long Description:</td>
                    <td colspan="4" style="width: 430px" ><asp:TextBox id="tbAddLongDescription" runat="server" width="99%"/></td>
                </tr>
            </table>
    
            <asp:Button ID="btnSaveFSSPrice" runat="server" Text="Save" OnClick="btnSaveFSSPrice_Click" />
            <asp:Button ID="btnAddFSSPriceCancel" runat="server" Text="Cancel" OnClick="btnAddFSSPriceCancel_Click" />
        </asp:Panel>
    
        <asp:GridView ID="gvFSSPriceList" runat="server" 
            AutoGenerateColumns="False" 
            CellPadding="4"
            DataKeyNames="LogNumber"  
            DataSourceID="FSSPriceListDataSource" 
            ForeColor="#333333" AllowSorting="True" 
            HeaderStyle-HorizontalAlign="Left" 
            HeaderStyle-Wrap="true" 
            HeaderStyle-VerticalAlign="Bottom"
            Font-Names="Arial" 
            Visible="False" 
            OnRowCommand="gvFSSPriceList_RowCommand" 
             >
         
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" HorizontalAlign="Left" VerticalAlign="Top" />                
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            
            <Columns>
                
                <asp:ButtonField Text="Details" CommandName="seeDetails" ButtonType="Button"  />
                 <asp:BoundField DataField="Contractor Catalog Number" HeaderText="Contractor Catalog Number"
                    SortExpression="Contractor Catalog Number" />
                    <asp:BoundField DataField="SIN" HeaderText="SIN" SortExpression="SIN" />
                <asp:BoundField DataField="Product Long Description" HeaderText="Product Long Description"
                    SortExpression="Product Long Description" />
                <asp:BoundField DataField="LogNumber" HeaderText="LogNumber" InsertVisible="False"
                    ReadOnly="True" SortExpression="LogNumber" />
                <asp:BoundField DataField="FSS Price" HeaderText="FSS Price" SortExpression="FSS Price" DataFormatString="{0:c}" /> 
                 <asp:BoundField DataField="Package Size Priced on Contract" HeaderText="Package Size Priced on Contract"
                    SortExpression="Package Size Priced on Contract" />
                 
                  <asp:ButtonField Text="Remove" CommandName="Remove" ButtonType="Button"  />            
            </Columns>
           


        </asp:GridView>
    
        <asp:SqlDataSource ID="FSSPriceListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"               
            SelectCommand="SELECT LogNumber, CntrctNum, [Contractor Catalog Number], [Product Long Description], [FSS Price], [Package Size Priced on Contract], SIN, Removed, [Outer Pack UOM], [Outer Pack Unit of Conversion Factor], [Outer Pack Unit Shippable], [Outer Pack UPN], [Intermediate Pack UOM], [Intermediate Pack Unit of Conversion Factor], [Intermediate Pack Shippable], [Intermediate Pack UPN], [Base Packaging UOM], [Base Packaging Unit of Conversion Factor], [Base Packaging Unit Shippable], [Base Packaging UPN], [Tier 1 Price], [Tier 2 Price], [Tier 3 Price], [Tier 4 Price], [Tier 5 Price], [Tier 1 Note], [Tier 2 Note], [Tier 3 Note], [Tier 4 Note], [Tier 5 Note], [621I_Category_ID], Date_Entered, Date_Modified FROM dbo.tbl_pricelist WHERE (CntrctNum = @CntrctNum) AND (Removed &lt;&gt; 1) and (( @IsContractActive = 'T' and datediff(dd,ExpirationDate,getdate())<=0 ) or ( @IsContractActive = 'F' ))  " 


        >
        
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            <asp:QueryStringParameter Name="IsContractActive" QueryStringField="IsContractActive" Type="String" />
        </SelectParameters>
        


    </asp:SqlDataSource>
    
    <asp:FormView ID="fvBPAPriceList" runat="server" 
        DataSourceID="BPAPriceListSummaryDataSource" InsertRowStyle-BackColor="#ece9d8" 
         Width="700px" Visible="False">

        <ItemTemplate>
           <table style="width:700px; background-color:#ece9d8">
                <tr style="font-family:Arial; color:#000099">
                    <td>
                    </td>
                    <td>
                    </td>
                    <td>
                    </td>
                    <td>
                    </td>
                    <td align="right" style="width:50px">
                        <asp:Button ID="formCloseButton" runat="server" Text="Close"  />
                    </td>                                           
                </tr>                        
                <tr style="font-family:Arial; color:#000099">
                    <td>
                    </td>                       
                    <td style="width:100px">
                        <asp:Label ID="CntrctNumLabel" runat="server" Text='<%# Bind("CntrctNum") %>' />
                    </td>
                    <td style="width:350px">
                        <asp:Label ID="Contractor_NameLabel" runat="server" Text='<%# Bind("Contractor_Name") %>' />
                    </td>
                    <td style="width:200px">
                        <div style=" font-size:12px">Total Number of prices:</div>
                        <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("Count") %>' />
                    </td>
                    <td align="right" style="width:50px">
                        <asp:Button ID="btnPrint" runat="server" Text="Print" OnClick="windowPrint" />
                    </td>
                </tr>
            </table>
        </ItemTemplate>
        
        <EmptyDataTemplate></EmptyDataTemplate>
       
        <InsertRowStyle BackColor="#ECE9D8" />
    </asp:FormView>
       
    <asp:SqlDataSource ID="BPAPriceListSummaryDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT * FROM [view_Pricelist_Count_BPA] WHERE ([CntrctNum] = @CntrctNum)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum"  Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:Button ID="btnBPAOpenAdd" runat="server"  Text="Add Item" Visible="false"  OnClick="btnBPAOpenAdd_Click"/>
    
    <asp:Panel ID="pnlBPAAdd" runat="server" Visible="false">
    
        <table width="700px" style="table-layout:fixed">
            <tr>
                <td style="width: 140px">Contractor Catalog Number</td>
                <td style="width: 100px">FSS Price</td>
                <td style="width: 100px">BPA/BOA Price</td>
                <td style="width: 100px">Price Start Date</td>
                <td style="width: 100px">Price End Date</td>
            </tr>
            <tr>
                <td style="width: 140px">
                    <ep:PaddedDropDownList ID="dlFSSCatalogNum" runat="server" style="font-family:Courier New;"
                        DataSourceID="BPA_FSS_Catalog_List" DataTextField="DisplayText"  Width="170px"
                        DataValueField="LogNumber" AutoPostBack="true" AppendDataBoundItems="true" OnSelectedIndexChanged="dlFSSCatalogNum_SelectedIndexChanged">
                       <asp:ListItem Text="" />
                    </ep:PaddedDropDownList>
            
                    <asp:SqlDataSource ID="BPA_FSS_Catalog_List" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:CM %>" 
                        SelectCommand="SELECT dbo.tbl_pricelist.[Contractor Catalog Number], dbo.tbl_pricelist.LogNumber, LTRIM(RTRIM([Contractor Catalog Number])) + convert( nvarchar(50), REPLICATE (' ', 50 - LEN(LTRIM(RTRIM([Contractor Catalog Number]))))) + 'Exp: ' + convert( nvarchar(10), convert(date, ExpirationDate )) as DisplayText FROM dbo.tbl_pricelist INNER JOIN dbo.tbl_Cntrcts ON dbo.tbl_Cntrcts.BPA_FSS_Counterpart = dbo.tbl_pricelist.CntrctNum WHERE (dbo.tbl_Cntrcts.CntrctNum = @CntrctNum) AND (dbo.tbl_pricelist.Removed = 0) AND  datediff(dd,getdate(),ExpirationDate)>=0 ORDER BY dbo.tbl_pricelist.[Contractor Catalog Number]">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </td>

                <td style="width: 100px"><asp:TextBox id="tbBPAFSSPrice" runat="server" ReadOnly="true" /></td>
                <td style="width: 100px"><asp:TextBox ID="tbBPAPrice" runat="server"  /></td>
                <td style="width: 100px"><asp:TextBox ID="BPAEffectiveDateTextBox" runat="server"  /></td>
                <td style="width: 100px"><asp:TextBox ID="BPAExpirationDateTextBox" runat="server" /></td>
            </tr>
            <tr>
                <td align="right" style="width: 140px;">Description:</td>
                <td colspan="4" style="width: 400px" ><asp:TextBox id="tbBPADescription" TextMode="MultiLine" Rows="2" runat="server" width="99%"/></td>
            </tr>
      
        </table>
        
        <asp:Button ID="btnSaveBPA" runat="server" Text="Save"  OnClick="btnSaveBPA_Click" />
        
        <asp:Button ID="btnBPACancel" runat="server" Text="Cancel" OnClick="btnBPACancel_Click"/>
    
    </asp:Panel>
 
    <asp:GridView ID="gvBPAPricelist" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="BPALogNumber,FSS_LogNumber" 
        DataSourceID="BPAPriceListDataSource" 
        CellPadding="4" 
        ForeColor="#333333" 
        GridLines="Both" 
        Font-Names="Arial" 
        HeaderStyle-HorizontalAlign="Left" 
        OnRowCommand="gvBPAPricelist_RowCommand" 
         Visible="False" 
         >
         
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"  />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" VerticalAlign="Top" />           
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#999999" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />

        <Columns>
             <asp:ButtonField Text="Details" ButtonType="Button" CommandName="seeDetails"  />
             <asp:BoundField DataField="Contractor Catalog Number" 
                HeaderText="Catalog Number" 
                SortExpression="Contractor Catalog Number" ItemStyle-Wrap="false" >
                <ItemStyle Wrap="False"></ItemStyle>
 
            </asp:BoundField>
               <asp:BoundField DataField="Description" 
                HeaderText="Standardized Description"  HeaderStyle-Width="300"
                SortExpression="Standardized_Description" ItemStyle-Wrap="true" 
                ItemStyle-Width="300" >
                <HeaderStyle Width="300px"></HeaderStyle>
                <ItemStyle Wrap="True" Width="300px"></ItemStyle>
            </asp:BoundField>
            
            <asp:BoundField DataField="Corresponding_FSS_CntrctNum" 
                HeaderText="FSS Contract Number" 
                SortExpression="Corresponding_FSS_CntrctNum" ItemStyle-Wrap="false" 
                HeaderStyle-Wrap="true" ItemStyle-Width="30" >
                <HeaderStyle Wrap="True"></HeaderStyle>
                <ItemStyle Wrap="False" Width="30px"></ItemStyle>
            </asp:BoundField>
           
            <asp:BoundField DataField="Product Long Description" 
                HeaderText="Product Long Description" 
                SortExpression="Product Long Description" ItemStyle-Wrap="true" 
                ItemStyle-Width="300" >
                <ItemStyle Wrap="True" Width="300px"></ItemStyle>
            </asp:BoundField>
          
            <asp:BoundField DataField="FSS Price" HeaderText="FSS Price" 
                SortExpression="FSS Price" 
                ItemStyle-Wrap="false">
                <ItemStyle Wrap="False"></ItemStyle>
            </asp:BoundField>
            
            <asp:BoundField DataField="BPA/BOA Price" HeaderText="BPA/BOA Price" 
                SortExpression="BPA/BOA Price" DataFormatString="{0:c}" 
                ItemStyle-Wrap="false" >
                <ItemStyle Wrap="False"></ItemStyle>
            </asp:BoundField>
          
            <asp:ButtonField ButtonType="Button" CommandName="Remove" Text="Remove" />
               
        </Columns>
    </asp:GridView>
    
    <!--    SelectCommand="SELECT [tbl_pricelist].[Product Long Description], [tbl_pricelist].CntrctNum As Corresponding_FSS_CntrctNum,[tbl_pricelist].[FSS Price],[tbl_pricelist].LogNumber As FSS_LogNumber, * FROM [tbl_BPA_Pricelist]
                    Join [tbl_pricelist] on [tbl_pricelist].LogNumber = [tbl_BPA_Pricelist].FSSLogNumber
                     WHERE ([tbl_BPA_Pricelist].[CntrctNum] = @CntrctNum) AND [tbl_BPA_Pricelist].Removed = 0">
    -->                 

    <asp:SqlDataSource ID="BPAPriceListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" 
            
        SelectCommand="SELECT isnull( p.[Product Long Description], '' ) as [Product Long Description],
	    	isnull( p.CntrctNum, '' )  As Corresponding_FSS_CntrctNum,
	    	isnull( p.[Contractor Catalog Number], '' ) as [Contractor Catalog Number],
    		case when  p.Removed = 1  then '' else convert( nvarchar(30), isnull( p.[FSS Price], 0 )) end as [FSS Price],
		    isnull( p.LogNumber, 0 ) As FSS_LogNumber, b.* 
	        FROM [tbl_BPA_Pricelist] b left outer join [tbl_pricelist] p
	        ON b.FSSLogNumber = p.LogNumber 
            WHERE b.[CntrctNum] = @CntrctNum AND b.Removed = 0 and (( @IsContractActive = 'T' and datediff(dd,b.ExpirationDate,getdate())<=0 ) or ( @IsContractActive = 'F' ))  " > 
				
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            <asp:QueryStringParameter Name="IsContractActive" QueryStringField="IsContractActive" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>
    
        <asp:FormView ID="fvNonStandardBPAPriceList" runat="server" 
        DataSourceID="NonStandardBPAPriceListSummaryDataSource" InsertRowStyle-BackColor="#ece9d8" 
         Width="700px" Visible="False">

        <ItemTemplate>
           <table style="width:700px; background-color:#ece9d8">
                <tr style="font-family:Arial; color:#000099">
                    <td>
                    </td>
                    <td>
                    </td>
                    <td>
                    </td>
                    <td>
                    </td>
                    <td align="right" style="width:50px">
                        <asp:Button ID="formCloseButton" runat="server" Text="Close"  />
                    </td>                                           
                </tr>                        
                <tr style="font-family:Arial; color:#000099">
                    <td>
                    </td>                       
                    <td style="width:100px">
                        <asp:Label ID="CntrctNumLabel" runat="server" Text='<%# Bind("CntrctNum") %>' />
                    </td>
                    <td style="width:350px">
                        <asp:Label ID="Contractor_NameLabel" runat="server" Text='<%# Bind("Contractor_Name") %>' />
                    </td>
                    <td style="width:200px">
                        <div style=" font-size:12px">Total Number of prices:</div>
                        <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("Count") %>' />
                    </td>
                    <td align="right" style="width:50px">
                        <asp:Button ID="btnPrint" runat="server" Text="Print" OnClick="windowPrint" />
                    </td>
                </tr>
            </table>
        </ItemTemplate>
        
        <EmptyDataTemplate></EmptyDataTemplate>
       
        <InsertRowStyle BackColor="#ECE9D8" />
    </asp:FormView>
       
    <asp:SqlDataSource ID="NonStandardBPAPriceListSummaryDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT * FROM [view_Pricelist_Count_BPA] WHERE ([CntrctNum] = @CntrctNum)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum"  Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:Button ID="btnNonStandardBPAOpenAdd" runat="server"  Text="Add Item" Visible="false"  OnClick="btnNonStandardBPAOpenAdd_Click"/>
    
    <asp:Panel ID="pnlNonStandardBPAAdd" runat="server" Visible="false">
    
        <table width="700px" style="table-layout:fixed">
            <tr>
                <td style="width: 100px">BPA/BOA Price</td>
                <td style="width: 100px">Price Start Date</td>
                <td style="width: 100px">Price End Date</td>
            </tr>
            <tr>
                <td style="width: 100px"><asp:TextBox ID="tbNonStandardBPAPrice" runat="server"  /></td>
                <td style="width: 100px"><asp:TextBox ID="NonStandardBPAEffectiveDateTextBox" runat="server"  /></td>
                <td style="width: 100px"><asp:TextBox ID="NonStandardBPAExpirationDateTextBox" runat="server" /></td>
            </tr>
            <tr>
                <td align="right" style="width: 140px;">Description:</td>
                <td colspan="4" style="width: 400px" ><asp:TextBox id="tbNonStandardBPADescription" TextMode="MultiLine" Rows="2" runat="server" width="99%"/></td>
            </tr>
      
        </table>
        
        <asp:Button ID="btnSaveNonStandardBPA" runat="server" Text="Save"  OnClick="btnSaveNonStandardBPA_Click" />
        
        <asp:Button ID="btnNonStandardBPACancel" runat="server" Text="Cancel" OnClick="btnNonStandardBPACancel_Click"/>
    
    </asp:Panel>

 
    <asp:GridView ID="gvNonStandardBPAPricelist" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="BPALogNumber" 
        DataSourceID="NonStandardBPAPriceListDataSource" 
        CellPadding="4" 
        ForeColor="#333333" 
        GridLines="Both" 
        Font-Names="Arial" 
        HeaderStyle-HorizontalAlign="Left" 
        OnRowCommand="gvNonStandardBPAPricelist_RowCommand" 
         Visible="False" 
         >
         
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"  />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" VerticalAlign="Top" />           
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#999999" />
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />

        <Columns>

            <asp:ButtonField Text="Details" ButtonType="Button" CommandName="seeDetails"  /> 

            <asp:BoundField DataField="Description" 
                HeaderText="Standardized Description"  HeaderStyle-Width="300"
                SortExpression="Standardized_Description" ItemStyle-Wrap="true" 
                ItemStyle-Width="300" >
                <HeaderStyle Width="300px"></HeaderStyle>
                <ItemStyle Wrap="True" Width="300px"></ItemStyle>
            </asp:BoundField>
                    
            <asp:BoundField DataField="BPA/BOA Price" HeaderText="BPA/BOA Price" 
                SortExpression="BPA/BOA Price" DataFormatString="{0:c}" 
                ItemStyle-Wrap="false" >
                <ItemStyle Wrap="False"></ItemStyle>
            </asp:BoundField>
          
            <asp:ButtonField ButtonType="Button" CommandName="Remove" Text="Remove" />
               
        </Columns>
    </asp:GridView>
    

    <asp:SqlDataSource ID="NonStandardBPAPriceListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" 
            
        SelectCommand="SELECT * FROM [tbl_BPA_Pricelist] WHERE  [CntrctNum] = @CntrctNum AND Removed = 0 and (( @IsContractActive = 'T' and datediff(dd,ExpirationDate,getdate())<=0 ) or ( @IsContractActive = 'F' ))  " >
		
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            <asp:QueryStringParameter Name="IsContractActive" QueryStringField="IsContractActive" Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>
    
    <asp:SqlDataSource ID="SINDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="sp_Contracted_SINS" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>
    
    <asp:SqlDataSource ID="PackageDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT [Short] FROM [tlkup_Unit_of_Measure]">
    </asp:SqlDataSource>
    
    <asp:FormView ID="fvServicesPriceList" runat="server" 
            DataSourceID="ServicesPriceListSummaryDataSource" InsertRowStyle-BackColor="#ece9d8" 
             Width="700px" Visible="False">
 
        <ItemTemplate>
               <table style="width:600px; background-color:#ece9d8">
                    <tr style="font-family:Arial; color:#000099">
                        <td>
                        </td>
                        <td>
                        </td>
                        <td>
                        </td>
                        <td>
                        </td>
                        <td align="right" style="width:50px">
                            <asp:Button ID="formCloseButton" runat="server" Text="Close"  />
                        </td>                                           
                    </tr>                        
                    <tr style="font-family:Arial; color:#000099">
                        <td>
                        </td>                       
                        <td style="width:80px">
                            <asp:Label ID="CntrctNumLabel" runat="server" Text='<%# Bind("CntrctNum") %>' />
                        </td>
                        <td style="width:300px">
                            <asp:Label ID="Contractor_NameLabel" runat="server" Text='<%# Bind("Contractor_Name") %>' />
                        </td>
                        <td style="width:160px">
                            <div style=" font-size:12px">Total Number of prices:</div>
                            <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("Count") %>' />
                        </td>
                        <td align="right" style="width:50px">
                            <asp:Button ID="btnPrint" runat="server" Text="Print" OnClick="windowPrint" />
                        </td>
                    </tr>
                </table>
        </ItemTemplate>
            
        <EmptyDataTemplate></EmptyDataTemplate>
       
        <InsertRowStyle BackColor="#ECE9D8" />
    </asp:FormView>
       
    <asp:SqlDataSource ID="ServicesPriceListSummaryDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT * FROM [view_Pricelist_Count] WHERE ([CntrctNum] = @CntrctNum)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>
            
    <asp:Button ID="btnServicesOpenAdd" runat="server"  Text="Add Item" Visible="false"  OnClick="btnServicesOpenAdd_Click"/>
    
    <asp:Panel ID="pnlServicesAdd" runat="server" Visible="false" Width="630px">
        <table width="630px" style="table-layout:fixed">
            <tr>
                <td style="width: 300px">Service Category</td>
                <td style="width: 100px"></td>
                <td style="width: 100px"></td>
                <td style="width: 100px"></td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:DropDownList ID="servicesDropDownList" runat="server" width="99%"
                        DataSourceID="ServicesListDataSource" DataTextField="Category_Selected" 
                        DataValueField="ID_SIN" AutoPostBack="true" AppendDataBoundItems="true" OnSelectedIndexChanged="servicesDropDownList_SelectedIndexChanged">
                        <asp:ListItem Text="" />
                     </asp:DropDownList>
            
	                <asp:SqlDataSource ID="ServicesListDataSource" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:CM %>" 
                        SelectCommand="SELECT [621I_Category_ID], [SIN] + ' : ' + [621I_Category_Description] AS Category_Selected, [SIN], 
	                          [621I_Category_Description], convert( varchar(24), [621I_Category_ID] ) + '|' + convert( varchar(45), [SIN] ) as ID_SIN  FROM tlkup_621I_Category_List where [SIN] in ( select [SINs] from tbl_Cntrcts_SINs  where CntrctNum = @CntrctNum ) ">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                                Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </td>           
                <td style="width: 100px"></td>
            </tr>
            <tr>
                <td style="width: 300px">Description</td>
                <td style="width: 100px">FSS Price</td>
                <td style="width: 100px">Price Start Date</td>
                <td style="width: 100px">Price End Date</td>
            </tr>
            <tr>
                <td style="width: 300px">
                    <asp:TextBox id="tbServiceCategory" runat="server" TextMode="MultiLine" Rows="3" Width="100%"  />
                </td>
                <td style="width: 100px">
                    <asp:TextBox ID="tbServiceFSSPrice" runat="server" Width="99%" />
                </td>
                <td style="width: 100px"><asp:TextBox ID="ServiceItemEffectiveDateTextBox" runat="server"  Width="99%" /></td>
                <td style="width: 100px"><asp:TextBox ID="ServiceItemExpirationDateTextBox" runat="server" Width="99%" /></td>

            </tr>
        </table>
    
        <asp:Button ID="btnSaveServices" runat="server" Text="Save"  OnClick="btnSaveServices_Click" />
        <asp:Button ID="btnServicesCancel" runat="server" Text="Cancel" OnClick="btnServicesCancel_Click"/>
    
    </asp:Panel>
        
    <asp:GridView ID="gvServicesPricelist" runat="server" AutoGenerateColumns="False" 
            DataKeyNames="LogNumber" 
            DataSourceID="ServicesPriceListDataSource" 
            CellPadding="4" 
            ForeColor="#333333" 
            GridLines="Both" 
            Width="635px"
            Font-Names="Arial" 
            HeaderStyle-HorizontalAlign="Left" 
            OnRowCommand="gvServicesPriceList_RowCommand"
            Visible="False" 
            >
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"  />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" VerticalAlign="Top" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <EditRowStyle BackColor="#999999" />
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
   
            <Columns>

	    	    <asp:ButtonField Text="Details" ButtonType="Button" CommandName="seeDetails"  />
                    
		        <asp:BoundField DataField="SIN" HeaderText="SIN" SortExpression="SIN" />               
                   
                <asp:BoundField DataField="Product Long Description" 
                    HeaderText="Description" 
                    SortExpression="Product Long Description" ItemStyle-Wrap="true" 
                    ItemStyle-Width="300" >
          		    <ItemStyle Wrap="True" Width="300px"></ItemStyle>
                </asp:BoundField>
                  
                <asp:BoundField DataField="FSS Price" HeaderText="FSS Price" 
                    SortExpression="FSS Price" DataFormatString="{0:c}"  
                    ItemStyle-Wrap="false">
		            <ItemStyle Wrap="False"></ItemStyle>
                </asp:BoundField>
                   
                <asp:ButtonField ButtonType="Button" CommandName="Remove" Text="Remove" />
                   
            </Columns>

    </asp:GridView>

    <asp:SqlDataSource ID="ServicesPriceListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"        
        SelectCommand="SELECT LogNumber, [SIN], [Product Long Description], [FSS Price] FROM tbl_pricelist where  [CntrctNum] = @CntrctNum and Removed = 0 and (( @IsContractActive = 'T' and datediff(dd,ExpirationDate,getdate())<=0 ) or ( @IsContractActive = 'F' ))  " >
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" />
            <asp:QueryStringParameter Name="IsContractActive" QueryStringField="IsContractActive" 
                Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>
    
        
    </form>
</body>
</html>
   
