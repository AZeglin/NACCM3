<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="pricelist_details.aspx.vb" Inherits="NACCM.pricelist_details"  Culture="en-US" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<!DOCTYPE html />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Pricelist Details</title>
       <script type="text/javascript">
       <!--
            function CloseWindow(pricelistType)
            {
                window.opener.document.forms[0].RefreshPricelistGridOnSubmit.value = "true";               
                
                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
</head>
<body style="background-color: #ece9d8">
    <form id="PricelistDetailForm" runat="server"   style="font-family: Arial;" > 
 
         <table width="830px" style="height: 608px; table-layout:fixed;"
           border="solid 1px black" align="left"  >
            <col width="828px" />
            <tr  valign="top" >
                <td >
                    <div id="PricelistDiv" style="width:814px; height:606px; " >
                        <asp:FormView ID="fvPricelistDetail" runat="server" 
                            Width="812px" Height="604px"  Enabled="true" Visible="true"  OnDataBound="fvPricelistDetail_OnDataBound" >
                            <EditItemTemplate>
                                <table  width="810px" style="height: 602px; table-layout:fixed;">   
                                <col width="112px" />
                                <col width="164px" />
                                <col width="112px" />
                                <col width="164px" />
                                <col width="112px" />
                                <col width="100px" />
                                    <tr>
                                        <td >
                                            <asp:Table ID="FSSPriceTable" Enabled="false" Visible="false" runat="server"  style="border-style: outset; font-family: Arial; width: 112px; ">
                                                <asp:TableRow style="background-color: Silver; color:#000099; " HorizontalAlign="center">
                                                    <asp:TableCell>                                                       
                                                         <asp:Label  ID="FSSPriceLabel"  Enabled="false" Visible="false" runat="server" Text="FSS Price" Font-Size="Smaller" />
                                                   </asp:TableCell>
                                                </asp:TableRow>                        
                                 
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="FSSPriceTextBox"  Enabled="false" Visible="false" runat="server"   OnDataBinding="FSSPriceTextBox_OnDataBinding" Width="95%" MaxLength="18" />
                                                           <asp:RegularExpressionValidator ID="FSSPriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="FSSPriceTextBox"
                                                             ErrorMessage="*" Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" EnableClientScript="false"/>   
                                                          <asp:RequiredFieldValidator ID="FSSPriceTextBoxRequiredFieldValidator" ControlToValidate="FSSPriceTextBox" ErrorMessage="*"   InitialValue="" runat="server"  EnableClientScript="false"/>
                                                  </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell >
                                                        <asp:Label ID="packageSizePricedOnContractLabel"  Enabled="false" Visible="false" runat="server" Text="Package Size Priced On Contract"  Font-Size="X-Small"/>
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:DropDownList ID="packageSizePricedOnContractDropDownList"  Enabled="false" Visible="false" runat="server"  DataSourceID="UOMDataSource" DataTextField="Short" DataValueField="Short" AppendDataBoundItems="true"  OnDataBinding="packageSizePricedOnContractDropDownList_OnDataBinding" Width="96%"  ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                        <asp:TextBox ID="packageSizePricedOnContractTextBox" Visible="false" runat="server"  OnDataBinding="packageSizePricedOnContractTextBox_OnDataBinding"  Width="96%"  />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </td>
                                        <td align="left">
                                            <asp:Table ID="FSSPriceEffectiveDateTable"  Enabled="false" Visible="false" runat="server"  style="border-style: outset; font-family: Arial; width: 162px; height: 110px; table-layout:fixed;">
                                                <asp:TableRow style="background-color: Silver; color:#000099; " HorizontalAlign="center">
                                                    <asp:TableCell >                                                       
                                                         <asp:Label  ID="priceEffectiveDateRangeLabel"  Enabled="false" Visible="false" runat="server" Text="Price Effective Date Range" Font-Size="Smaller" />
                                                   </asp:TableCell>
                                                </asp:TableRow>                        
                                                <asp:TableRow style="height:16px;">
                                                    <asp:TableCell>
                                                        <asp:Label ID="FSSEffectiveDateLabel"  Enabled="false" Visible="false" runat="server" Text="Effective" Font-Size="X-Small"  Height="100%"/>
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="FSSEffectiveDateTextBox"  Enabled="false" Visible="false" runat="server" OnDataBinding="FSSEffectiveDateTextBox_OnDataBinding"   Width="96%" />
                                                         <asp:CompareValidator ID="FSSEffectiveDateTextBoxCompareValidator" runat="server" ControlToValidate="FSSEffectiveDateTextBox"
                                                             ErrorMessage="*" EnableClientScript="false"  Type="Date" Display="Static" Operator="DataTypeCheck" />
                                                         <asp:RequiredFieldValidator ID="FSSEffectiveDateTextBoxRequiredFieldValidator" ControlToValidate="FSSEffectiveDateTextBox" ErrorMessage="*" EnableClientScript="false"  InitialValue="" runat="server" />

                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow style="height:16px;">
                                                    <asp:TableCell>
                                                        <asp:Label ID="FSSExpirationDateLabel"  Enabled="false" Visible="false" runat="server" Text="Expires" Font-Size="X-Small"  Height="100%"/>
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="FSSExpirationDateTextBox"  Enabled="false" Visible="false" runat="server" OnDataBinding="FSSExpirationDateTextBox_OnDataBinding"   Width="96%" />
                                                         <asp:CompareValidator ID="FSSExpirationDateTextBoxCompareValidator" runat="server" ControlToValidate="FSSExpirationDateTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Type="Date" Display="Static" Operator="DataTypeCheck" />                                                        
                                                          <asp:RequiredFieldValidator ID="FSSExpirationDateTextBoxRequiredFieldValidator" ControlToValidate="FSSExpirationDateTextBox" ErrorMessage="*" EnableClientScript="false"  InitialValue="" runat="server" />
                                                   </asp:TableCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </td>     
                                       <td >
                                            <asp:Table  ID="BPAPriceTable" visible="false"  runat="server"  style="border-style: outset; font-family: Arial; width: 112px; ">
                                                <asp:TableRow style="background-color: Silver; color:#000099; " >
                                                    <asp:TableCell>                                                       
                                                         <asp:Label  ID="BPAPriceLabel"  Enabled="false" Visible="false" runat="server" Text="BPA/BOA Price" Font-Size="Smaller" />
                                                   </asp:TableCell>
                                                </asp:TableRow>                        
                                 
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="BPAPriceTextBox" Enabled="false" Visible="false" runat="server"  OnDataBinding="BPAPriceTextBox_DataBinding" Width="96%" MaxLength="18" />
                                                          <asp:RegularExpressionValidator ID="BPAPriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="BPAPriceTextBox"
                                                             ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                           <asp:RequiredFieldValidator ID="BPAPriceTextBoxRequiredFieldValidator" ControlToValidate="BPAPriceTextBox" ErrorMessage="*" EnableClientScript="false"  InitialValue="" runat="server" />
                                                 </asp:TableCell>
                                                </asp:TableRow>
         
                                            </asp:Table>      
                                        </td>
                                        <td >
                                            <table id="BPAPriceEffectiveDateTable"  visible="false" runat="server" style="border-style: outset; font-family: Arial; width: 162px; height: 110px; table-layout:fixed;">
                                                <tr style="background-color: Silver; color:#000099; " align="center">
                                                    <td>                                                       
                                                         <asp:Label  ID="BPAPriceEffectiveDateRangeLabel" Enabled="false" Visible="false" runat="server" Text="BPA/BOA Price Effective Date Range" Font-Size="Smaller" />
                                                   </td>
                                                </tr>                        
                                                <tr  style="height:16px;">
                                                    <td >
                                                        <asp:Label ID="BPAEffectiveDateLabel" Enabled="false" Visible="false" runat="server" Text="Effective" Font-Size="X-Small" Height="100%"/>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:TextBox ID="BPAEffectiveDateTextBox" Enabled="false" Visible="false" runat="server" OnDataBinding="BPAEffectiveDateTextBox_DataBinding" Width="96%" />
                                                        <asp:CompareValidator ID="BPAEffectiveDateTextBoxCompareValidator" runat="server" ControlToValidate="BPAEffectiveDateTextBox"
                                                                ErrorMessage="*" EnableClientScript="false"  Type="Date" Display="Static" Operator="DataTypeCheck" />                                                        
                                                        <asp:RequiredFieldValidator ID="BPAEffectiveDateTextBoxRequiredFieldValidator" ControlToValidate="BPAEffectiveDateTextBox" ErrorMessage="*" EnableClientScript="false"  InitialValue="" runat="server" />
                                                 </td>
                                                </tr>
                                                <tr  style="height:16px;">
                                                    <td >
                                                        <asp:Label ID="BPAExpirationDateLabel" Enabled="false" Visible="false" runat="server" Text="Expires" Font-Size="X-Small" Height="100%"/>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:TextBox ID="BPAExpirationDateTextBox" Enabled="false" Visible="false" runat="server" OnDataBinding="BPAExpirationDateTextBox_DataBinding" Width="96%" />
                                                        <asp:CompareValidator ID="BPAExpirationDateTextBoxCompareValidator" runat="server" ControlToValidate="BPAExpirationDateTextBox"
                                                             ErrorMessage="*" EnableClientScript="false"  Type="Date" Display="Static" Operator="DataTypeCheck" />   
                                                         <asp:RequiredFieldValidator ID="BPAExpirationDateTextBoxRequiredFieldValidator" ControlToValidate="BPAExpirationDateTextBox" ErrorMessage="*" EnableClientScript="false"  InitialValue="" runat="server" />
                                                   </td>
                                                </tr>
                                            </table>
                                        </td> 
                                        <td>
                                        </td>    
                                        <td>
                                            <table width="96%">
                                                <tr >
                                                    <td align="right">
                                                        <asp:Button ID="closeFormButton" runat="server" Text="Close" Width="80px"/>                                        
                                                    </td>
                                                </tr>    
                                                 <tr>
                                                    <td align="right">
                                                        <asp:Button ID="saveButton" runat="server" Text="Save" OnClick="Save_Click" Width="80px" />                                        
                                                    </td>
                                                </tr>    
                                                <tr>
                                                    <td align="right">
                                                        <asp:Button ID="printButton" runat="server" Text="Print" OnClick="Print_Click" Width="80px"/>                                        
                                                    </td>
                                                </tr>    
                                             </table>
                                        </td>                             
                                    </tr>
                                    <tr>
                                        <td colspan="2" align="right">
                                            <asp:Label ID="BPADescriptionLabel"  Enabled="false" Visible="false" runat="server" Text="BPA/BOA Description"  Font-Size="Smaller" />
                                        </td>
                                        <td colspan="4">
                                            <asp:TextBox ID="BPADescriptionTextBox" Enabled="false" Visible="false" runat="server"  OnDataBinding="BPADescriptionTextBox_DataBinding"  Width="96%" />
                                        </td>
                                    </tr>
                                    <tr>
                                         <td colspan="2" align="right">
                                             <asp:Label ID="contractorCatalogNumberLabel"  Enabled="false" Visible="false" runat="server" Text="Contractor Catalog Number"  Font-Size="Smaller" />
                                         </td>
                                         <td colspan="2">
                                            <ep:PaddedDropDownList ID="contractorCatalogNumberDropDownList" runat="server"  Width="96%" style="font-family:Courier New;"
                                                DataSourceID="ParentFSSItemsDataSource"  DataTextField="DisplayText"
                                                visible="false" Enabled="false" DataValueField="FSSLogNumber" AutoPostBack="true" OnDataBinding="contractorCatalogNumberDropDownList_DataBinding" AppendDataBoundItems="true" OnSelectedIndexChanged="contractorCatalogNumberDropDownList_SelectedIndexChanged">
                                                <asp:ListItem Text="" />
                                             </ep:PaddedDropDownList>
     
                                           <asp:TextBox ID="contractorCatalogNumberTextBox"  Enabled="false" Visible="false" runat="server" OnDataBinding="contractorCatalogNumberTextBox_OnDataBinding"  Width="95%"  MaxLength="50" />
                                         </td>
                          
                                         <td colspan="2" valign="middle">
                                            <table id="SINTable" runat="server" >
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="SINLabel"  Enabled="false" Visible="false" runat="server" Text="SIN"   Font-Size="Smaller"  Width="40px"/>
                                                    </td>
                                                    <td>
                                                       <asp:DropDownList ID="SINDropDownList"  Enabled="false" Visible="false" runat="server"  Width="120px"
                                                            DataSourceID="SINDataSource" DataTextField="Sins" 
                                                            DataValueField="Sins"  AutoPostBack="true" AppendDataBoundItems="true"  OnDataBound="SINDropDownList_DataBound">
                                                            
                                                         </asp:DropDownList>
                                             
                                                       <asp:TextBox ID="SINTextBox" visible="false" Enabled="false" runat="server"  OnDataBinding="SINTextBox_OnDataBinding"  Width="120px" MaxLength="50" />
                                                    </td>
                                                </tr>
                                             </table>
                                         </td>
                                    </tr>
                                    <tr>
                                      <td colspan="2" align="right">
                                        <asp:Label ID="serviceCategoryLabel" Enabled="false" Visible="false" runat="server" Text="Service Category"  Font-Size="Smaller" />
                                      </td>                                    
                                      <td colspan="3">
                                           <asp:DropDownList ID="serviceCategoriesDropDownList" runat="server"  Width="96%"
                                                DataSourceID="ServiceListDataSource" DataTextField="Category_Selected" 
                                                Enabled="false" Visible="false" DataValueField="ID_SIN" AutoPostBack="true" AppendDataBoundItems="true" OnSelectedIndexChanged="serviceCategoriesDropDownList_SelectedIndexChanged"
                                                 OnDataBound="serviceCategoriesDropDownList_DataBound">
                                                <asp:ListItem Text="" />
                                             </asp:DropDownList>
                                           <asp:TextBox ID="serviceCategoriesTextBox"  OnDataBinding="serviceCategoriesTextBox_DataBinding" runat="server" Width="96%" Visible="false" Enabled="false"  />
                                     </td>
                                     <td>
                                     </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" align="right">
                                            <asp:Label ID="productLongDescriptionLabel"  Enabled="false" Visible="false" runat="server" Text="Product Long Description"  Font-Size="Smaller" />
                                        </td>
                                        <td colspan="4">
                                            <asp:TextBox ID="productLongDescriptionTextBox"  Enabled="false" Visible="false" runat="server" OnDataBinding="productLongDescriptionTextBox_OnDataBinding"   Width="96%" MaxLength="800" />
                                        </td>
                                    </tr>

                      
                                    <tr >    

                                        <td colspan="6">
                                            <table  id="tieredPricingTable"   Visible="false"  runat="server" style="border-style: outset; font-family: Arial; height:80px; width:96%; table-layout:fixed;">
                                              <col width="10%" />
                                              <col width="18%" />
                                              <col width="18%" />
                                              <col width="18%" />
                                              <col width="18%" />
                                              <col width="18%" />
                                                  <tr style="background-color: Silver; color:#000099; " align="center">
                                                    <td colspan="6">
                                                        <asp:Label  ID="tieredPricingLabel" Enabled="false" Visible="false" runat="server" Text="Tiered Pricing" Font-Size="Smaller" >
                                                        </asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        
                                                    </td>
                                                    <td style=" font-size:smaller;">
                                                        Tier 1
                                                    </td>
                                                    <td style=" font-size:smaller;">
                                                        Tier 2
                                                    </td>
                                                    <td style=" font-size:smaller;">
                                                        Tier 3
                                                    </td>
                                                    <td style=" font-size:smaller;">
                                                        Tier 4
                                                    </td>
                                                    <td style=" font-size:smaller;">
                                                        Tier 5
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td  style="width: 30px; font-size:smaller;">
                                                        Prices:    
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier1PriceTextBox" Enabled="false" Visible="false" runat="server" OnDataBinding="tier1PriceTextBox_OnDataBinding"    Width="75%" MaxLength="18" />
                                                         <asp:RegularExpressionValidator ID="tier1PriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="tier1PriceTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                  </td>
                                                    <td>
                                                        <asp:TextBox ID="tier2PriceTextBox" Enabled="false" Visible="false" runat="server"  OnDataBinding="tier2PriceTextBox_OnDataBinding" Width="75%" MaxLength="18" />
                                                          <asp:RegularExpressionValidator ID="tier2PriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="tier2PriceTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                   </td>
                                                    <td>
                                                        <asp:TextBox ID="tier3PriceTextBox" Enabled="false" Visible="false" runat="server"   OnDataBinding="tier3PriceTextBox_OnDataBinding"  Width="75%" MaxLength="18" />
                                                         <asp:RegularExpressionValidator ID="tier3PriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="tier3PriceTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier4PriceTextBox" Enabled="false" Visible="false" runat="server"   OnDataBinding="tier4PriceTextBox_OnDataBinding"    Width="75%" MaxLength="18" />
                                                         <asp:RegularExpressionValidator ID="tier4PriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="tier4PriceTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier5PriceTextBox" Enabled="false" Visible="false" runat="server"   OnDataBinding="tier5PriceTextBox_OnDataBinding"    Width="75%" MaxLength="18" />
                                                         <asp:RegularExpressionValidator ID="tier5PriceTextBoxRegularExpressionValidator" runat="server" ControlToValidate="tier5PriceTextBox"
                                                         ErrorMessage="*" EnableClientScript="false"  Display="Static"  ValidationExpression="^\$?(?:\d+|\d{1,3}(?:,\d{3})*)(?:\.\d{1,2}){0,1}$" />   
                                                    </td>                   
                                                </tr>
                                                <tr>
                                                    <td  style="width: 30px; font-size:smaller;">
                                                        Tier<br />
                                                        Notes:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier1NotesTextBox" TextMode="MultiLine"  Enabled="false" Visible="false" runat="server"   OnDataBinding="tier1NotesTextBox_OnDataBinding" 
                                                            Height="50px" Width="100%"  Font-Size="Small" MaxLength="255" />
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier2NotesTextBox" TextMode="MultiLine"   Enabled="false" Visible="false"  runat="server"   OnDataBinding="tier2NotesTextBox_OnDataBinding" 
                                                            Height="50px" Width="100%" Font-Size="Small" MaxLength="255" />
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier3NotesTextBox" TextMode="MultiLine"   Enabled="false" Visible="false"  runat="server"   OnDataBinding="tier3NotesTextBox_OnDataBinding" 
                                                            Height="50px" Width="100%" Font-Size="Small" MaxLength="255" />
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier4NotesTextBox" TextMode="MultiLine"   Enabled="false" Visible="false"  runat="server"   OnDataBinding="tier4NotesTextBox_OnDataBinding" 
                                                            Height="50px" Width="100%" Font-Size="Small" MaxLength="255" />
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tier5NotesTextBox" TextMode="MultiLine"   Enabled="false" Visible="false"  runat="server"   OnDataBinding="tier5NotesTextBox_OnDataBinding" 
                                                            Height="50px" Width="100%" Font-Size="Small" MaxLength="255" />
                                                    </td>
                                                 </tr>
                                                
                                            </table>
                                        </td>
                                    </tr>
                               
                                </table>
                            </EditItemTemplate>
                            <EmptyDataTemplate>
                                Could not find the details for this item.
                            </EmptyDataTemplate>


                        </asp:FormView>
                      </div>
                </td>
             </tr>
         </table>

    <asp:SqlDataSource ID="ServiceListDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT [621I_Category_ID], [SIN] + ' : ' + [621I_Category_Description] AS Category_Selected, [SIN], 
              [621I_Category_Description], convert( varchar(24), [621I_Category_ID] ) + '|' + convert( varchar(45), [SIN] ) as ID_SIN  FROM tlkup_621I_Category_List where [SIN] in ( select [SINs] from tbl_Cntrcts_SINs  where CntrctNum = @CntrctNum ) ">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" 
                Type="String" />
        </SelectParameters>
    </asp:SqlDataSource>             
                                            
    <asp:SqlDataSource ID="UOMDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SELECT [tlkup_Unit_of_Measure].[Short] FROM tlkup_Unit_of_Measure ORDER BY [tlkup_Unit_of_Measure].[Short]">
    </asp:SqlDataSource>
    
   <asp:SqlDataSource ID="SINDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        SelectCommand="SelectSINSForContract" SelectCommandType="StoredProcedure">
       <SelectParameters>
           <asp:QueryStringParameter Name="ContractNumber" QueryStringField="CntrctNum" 
               Type="String" />
           <asp:Parameter Name="IncludeInvalid" Type="Int32" DefaultValue="1" Direction="Input" />
       </SelectParameters>
    </asp:SqlDataSource>
    
    <asp:SqlDataSource ID="ParentFSSItemsDataSource" runat="server"
          ConnectionString="<%$ ConnectionStrings:CM %>" 
          SelectCommand = "Select [Contractor Catalog Number] as Contractor_Catalog_Number, ExpirationDate, LogNumber as FSSLogNumber,  LTRIM(RTRIM([Contractor Catalog Number])) + convert( nvarchar(50), REPLICATE (' ', 50 - LEN(LTRIM(RTRIM([Contractor Catalog Number]))))) + 'Exp: ' + convert( nvarchar(10), convert(date, ExpirationDate )) as DisplayText from tbl_pricelist where CntrctNum = @CntrctNum order by Contractor_Catalog_Number" >
     <SelectParameters>
           <asp:QueryStringParameter Name="CntrctNum" QueryStringField="ParentContractNumber" 
               Type="String" />
       </SelectParameters>
       
     </asp:SqlDataSource>

    </form>
</body>
</html>
