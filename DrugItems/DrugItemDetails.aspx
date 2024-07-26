<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard" CodeBehind="DrugItemDetails.aspx.cs" Inherits="VA.NAC.CM.DrugItems.DrugItemDetails" %>

<%@ Register src="DrugItemHeader.ascx" tagname="DrugItemHeader" tagprefix="uc1" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Item Details</title>
  

       <script type="text/javascript">
       <!--
            function CloseWindow( withRefresh, withRefreshPrices )
            {
                window.opener.document.forms[0].RefreshDrugItemScreenOnSubmit.value = withRefresh;
                window.opener.document.forms[0].RebindDrugItemScreenOnRefreshOnSubmit.value = 'false';
                window.opener.document.forms[0].RefreshDrugItemPriceScreenOnSubmit.value = withRefreshPrices;
                window.opener.document.forms[0].RebindDrugItemPriceScreenOnRefreshOnSubmit.value = 'false';               

                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
  
        
</head>
<body >
    <form id="DrugItemDetailsForm" runat="server" style="z-index: 5; position:fixed; top:0px; left:0px; width:898px; height:613px;" >
           
    <asp:ScriptManager ID="DrugItemDetailsScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="DrugItemDetailsScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>
    

        <table style="width:894px; height: 608px; table-layout:fixed; border:solid 1px black;" class="DrugItemDetails"  >
            <colgroup >
                <col style="width: 892px;" />
            </colgroup>

            <tr  style="height:84px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 83px; width: 890px;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemDetailsUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc1:DrugItemHeader ID="SelectedDrugItemHeader" runat="server"/>
                            </ContentTemplate>
                        </asp:UpdatePanel> 
                    </div>
                </td>
            </tr>
 
            <tr style="height:32px; text-align:right;" >     
                <td style="width:98%; text-align:right;" >
                     <table  style="border-top:solid 1px black;  width:890px; height:28px; table-layout:fixed; text-align:right;" >
                        <tr style:"text-align:right;">    
                            <td></td>                    
                            <td style="width: 80px" >            
                                <asp:Button runat="server"  ID="PrintItemDetailsButton" Text="Print" OnClientClick="window.print();"  >            
                                </asp:Button >      
                            </td>
                            <td style="width: 80px" >
                                <asp:Button runat="server"  ID="UpdateItemDetailsButton" Text="Save" OnClick="UpdateItemDetailsButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="width: 80px" >
                                <asp:Button runat="server"  ID="CancelItemDetailsButton" Text="Close" OnClick="CancelItemDetailsButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:592px; vertical-align:top; border: solid 1px black">
                <td >
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server" Width="890px" Height="587px" >
                        <div id="ItemDetailsBodyDiv" style="width:888px; height:587px; top:0px;  left:0px; border:solid 1px black; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemDetailsUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="DrugItemDetailsFormView" 
                                runat="server" OnDataBound="DrugItemDetailsFormView_OnDataBound" 
                                DefaultMode="Edit" OnPreRender="DrugItemDetailsFormView_OnPreRender" 
                                 RenderOuterTable="false" >
                            <ItemTemplate>
                                <table style="height: 464px; table-layout:fixed; width: 886px;" class="DrugItemDetailsEditArea"  >     
                                    <colgroup >
                                        <col style="width: 20px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 100px;" />
                                        <col style="width: 80px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 100px;" />
                                        <col style="width: 100px;" />
                                        <col style="width: 20px;" />
                                    </colgroup >
                                    <tr >
                                        <td>
                                        </td>
                                        <td >
                                            <asp:Label ID="DateEnteredMarketLabel" runat="server" Text='<%# MultilineText( new string[] {"Date", "Entered", "Market"} ) %>' Width="94%" />
                                        </td>
                                        <td >
                                            <asp:Label ID="PrimeVendorFlagLabel" runat="server" Text='<%# MultilineText( new string[] {"Prime", "Vendor"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PrimeVendorChangedDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Prime Vendor", "Changed Date"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PassThroughFlagLabel" runat="server" Text='<%# MultilineText( new string[] {"Pass", "Through", "Flag"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="VAClassLabel" runat="server" Text='<%# MultilineText( new string[] {"VA", "Class"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExcludeFromExportLabel" runat="server" Text='<%# MultilineText( new string[] {"Exclude", "From", "Export"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="NonTAALabel" runat="server" Text='<%# MultilineText( new string[] { "Non", "TAA" } ) %>' Width="94%" />   
                                        </td>
                                        <td>
                                            <asp:Label ID="IncludedFETAmountLabel" runat="server" Text='<%# MultilineText( new string[] { "FET", "Amount" } ) %>' Width="94%" /> 
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="DateEnteredMarketDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "DateEnteredMarket", "{0:d}" )%>' runat="server" Width="94%" />
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="PrimeVendorFlagCheckBox" runat="server" OnDataBinding="PrimeVendorFlagCheckBox_OnDataBinding"   Enabled="false" Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PrimeVendorChangedDateDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "PrimeVendorChangedDate", "{0:d}" )%>' runat="server"  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PassThroughFlagDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "PassThrough" )%>'  runat="server" Width="88%" />                                               
                                        </td>
                                        <td>
                                            <asp:Label ID="VAClassDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "VAClass" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="ExcludeFromExportCheckBox" runat="server" OnDataBinding="ExcludeFromExportCheckBox_OnDataBinding"   Enabled="false"  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="NonTAACheckBox" runat="server" OnDataBinding="NonTAACheckBox_OnDataBinding" Width="94%" /> 
                                        </td>
                                        <td>
                                            <asp:Label ID="IncludedFETAmountDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IncludedFETAmount" )%>' Width="94%" />  
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitOfSaleLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit of", "Sale"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitOfSaleLabel" runat="server" Text='<%# MultilineText( new string[] {"Quantity in", "Unit of", "Sale"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitPackageLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit", "Package"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitPackageLabel" runat="server" Text='<%# MultilineText( new string[] {"Quantity in", "Unit Package"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitOfMeasureLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit of", "Measure"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PriceMultiplierLabel" runat="server" Text='<%# MultilineText( new string[] {"Price", "Multiplier"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PriceDividerLabel" runat="server" Text='<%# MultilineText( new string[] {"Price", "Divider"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                  </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitOfSaleDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitOfSale" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitOfSaleDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitOfSale" )%>' Width="92%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitPackageDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitPackage" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitPackageDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitPackage" )%>' Width="92%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitOfMeasureDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitOfMeasure" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PriceMultiplierDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PriceMultiplier" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PriceDividerDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PriceDivider" )%>' Width="50%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>                                
                                  </tr>
                                  <tr style="height:27px; border-top: 1px black;">
                                        <td colspan="8" style="vertical-align:bottom; text-align:center" >
                                            <asp:Label ID="DistributorGridLabel" runat="server" Text="Specialty Distributors" CssClass="DrugItemHeaderText" />
                                        </td>
                                    </tr>
                                   <tr style="height:22px; border-top: 1px black; vertical-align:bottom; text-align:center;" >
                                        <td colspan="8" style="vertical-align:bottom; text-align:center" >
                                            <asp:Label ID="SubItemGridLabel" runat="server" Text="Sub-Items With Same NDC" CssClass="DrugItemHeaderText" />
                                        </td>
                                    </tr>

                                  <tr style="height:22px; border-top: 1px black; vertical-align:bottom; text-align:center;" >
                                        <td colspan="2" style="text-align:left">
                                            <asp:Button ID="AddNewDistributorButton" runat="server" Width="94%" Text="Add Distributor" OnClick="AddNewDistributorButton_OnClick" />
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>

                                   <tr style="height:118px;" >
                                        <td colspan="9" style="text-align:left;">
                                              <div id="DistributorGridViewDiv"  runat="server" style="width:860px; height:126px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                       <ep:UpdatePanelEventProxy ID="InsertDistributorButtonClickUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="ChangeDistributorHighlightUpdatePanelEventProxy" runat="server" />
 
                                                      <asp:UpdatePanel ID="DistributorGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                       <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="InsertDistributorButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="ChangeDistributorHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                       </Triggers>
                                                        <ContentTemplate> 
                             
                                                             <ep:GridView ID="DistributorGridViewReadOnly" 
                                                                        runat="server" 
                                                                        DataKeyNames="DrugItemDistributorId"  
                                                                        AutoGenerateColumns="False" 
                                                                        Width="840px" 
                                                                        CssClass="DrugItemGrids" 
                                                                        Visible="True" 
                                                                        OnRowDataBound="DistributorGridView_RowDataBound"
                                                                        AutoGenerateEditButton="false"
                                                                        EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                        onprerender="DistributorGridView_PreRender" 
                                                                        onrowediting="DistributorGridView_RowEditing" 
                                                                        onrowcancelingedit="DistributorGridView_RowCancelingEdit"
                                                                        AllowInserting="True"
                                                                        
                                                                        EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                        EmptyDataText="There are no distributors defined for the selected item."
                                                                       >
                                                                    <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                    <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                    <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                    <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                 
                                                                    <Columns>  
                                                                     
                                                                        <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="EditButton" Text="Edit" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" CommandName="EditItem" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    ButtonType="Button" Width="60px" CausesValidation="false" 
                                                                                    Visible="true" >            
                                                                                    </asp:Button >                                    
                                                                                <asp:Button ID="SaveButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    CommandName="SaveItem" Width="60px" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" Text="Save" Visible="false" />
                                                                                <asp:Button ID="CancelButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    CommandName="Cancel" Width="60px" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" Text="Cancel" Visible="false" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                                       
                                                                        
                                                                        <asp:TemplateField HeaderText="Distributor Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorNameLabel" runat="server"  Width="165px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "DistributorName"  )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                 <ajax:ComboBox ID="distributorNameComboBox"   DropDownStyle="DropDown" CssClass="CustomComboBoxStyle" MaxLength="100" style="z-index: 15;"  RenderMode="Block" AutoCompleteMode="None" ItemInsertLocation="Prepend"  DataValueField="DistributorName"  Width="165px"   DataTextField="DistributorName" runat="server" OnDataBound="distributorNameComboBox_DataBound"  AutoPostBack="true" >
                                                                                            </ajax:ComboBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Contact Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                              <ItemTemplate>
                                                                                <asp:Label ID="contactNameLabel" runat="server"  Width="160px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "ContactPerson" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="contactNameTextBox" runat="server" MaxLength="30" Width="160px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "ContactPerson" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Phone"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorPhoneLabel" runat="server"  Width="100px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Phone" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="distributorPhoneTextBox" runat="server" Width="100px" MaxLength="15" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Phone" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>
                                                                                        
                                                                        <asp:TemplateField HeaderText="Notes"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorNotesLabel" runat="server"  Width="215px"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Notes" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="distributorNotesTextBox" runat="server" Width="215px" MaxLength="800" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Notes" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>                                                                                        
                                                                                                           
                                                                        <asp:TemplateField ItemStyle-Width="99%" ItemStyle-Height="99%">  
                                                                            <ItemTemplate>
                                                                                   <asp:Button runat="server" ID="RemoveDistributorButton" Text="Remove Distributor" 
                                                                                       OnCommand="DistributorGridView_ButtonCommand"   CommandName="RemoveDistributor"  
                                                                                       CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                       Width="82px" 
                                                                                       CssClass="ButtonWrapText"  >
                                                                                  </asp:Button>   

                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                       
                                                                        

                                                                    </Columns>
                                                        
                                                                </ep:GridView>
                                                            </ContentTemplate>
                                                      </asp:UpdatePanel> 
                                             </div>                                                                          
                                        </td>
                                    </tr>
                                    <tr style="height:27px; border-top: 1px black;">
                                        <td colspan="8" style="text-align:center; vertical-align:bottom;">
                                            <asp:Label ID="SubItemsLabel" runat="server" Text="Sub-Items" CssClass="DrugItemHeaderText" />
                                        </td>
                                    </tr>
                                   <tr style="height:22px; border-top: 1px black; text-align:right; vertical-align:bottom;" >
                                        <td colspan="2" style="text-align:left;">
                                            <asp:Button ID="AddNewSubItemButton" runat="server" Width="94%" Text="Add Sub-Item" OnClick="AddNewSubItemButton_OnClick" />
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:118px;" >
                                        <td colspan="9" style="text-align:left">
                                              <div id="SubItemGridViewDiv"  runat="server" style="width:860px; height:126px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                       <ep:UpdatePanelEventProxy ID="InsertSubItemButtonClickUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="ChangeSubItemHighlightUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="OuterFormViewWasSavedUpdatePanelEventProxy" runat="server" />

                                                      <asp:UpdatePanel ID="SubItemGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                       <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="InsertSubItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="ChangeSubItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                       </Triggers>
                                                        <ContentTemplate> 
                             
                                                             <ep:GridView ID="SubItemGridViewReadOnly" 
                                                                        runat="server" 
                                                                        DataKeyNames="DrugItemSubItemId"  
                                                                        AutoGenerateColumns="False" 
                                                                        Width="810px" 
                                                                        CssClass="DrugItemGrids" 
                                                                        Visible="True" 
                                                                        OnRowDataBound="SubItemGridView_RowDataBound"
                                                                        AutoGenerateEditButton="false"
                                                                        EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                        onprerender="SubItemGridView_PreRender" 
                                                                        onrowediting="SubItemGridView_RowEditing" 
                                                                        onrowcancelingedit="SubItemGridView_RowCancelingEdit"
                                                                        AllowInserting="True"
                                                                        InsertCommandColumnIndex="2"
                                                                        EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                        EmptyDataText="There are no sub-items defined for the selected item."
                                                                       >
                                                                    <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                    <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                    <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                    <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                 
                                                                    <Columns>  
                                                                     
                                                                        <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="EditButton" Text="Edit" Enabled="false"
                                                                                    OnCommand="SubItemGridView_ButtonCommand" CommandName="EditItem" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    ButtonType="Button" Width="60px" CausesValidation="false" 
                                                                                    Visible="true" >            
                                                                                    </asp:Button >                                    
                                                                                <asp:Button ID="SaveButton" runat="server" ButtonType="Button" Enabled="false"
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    CommandName="SaveItem" Width="60px" 
                                                                                    OnCommand="SubItemGridView_ButtonCommand" Text="Save" Visible="false" />
                                                                                <asp:Button ID="CancelButton" runat="server" ButtonType="Button" Enabled="false"
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    CommandName="Cancel" Width="60px" 
                                                                                    OnCommand="SubItemGridView_ButtonCommand" Text="Cancel" Visible="false" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                                       
                                                                        
                                      
                                                                        
                                                                         <asp:TemplateField HeaderText="Sub-Item Identifier"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                                                                                     <ItemTemplate>
                                                                                <asp:Label ID="subItemIdentifierLabel" runat="server"  Width="60px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "SubItemIdentifier" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="subItemIdentifierTextBox" runat="server" Width="60px" 
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "SubItemIdentifier" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Generic Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="genericNameLabel" runat="server"  Width="200px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Generic"  )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="genericNameTextBox" runat="server" Width="200px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Generic"  )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Trade Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                                                                                     <ItemTemplate>
                                                                                <asp:Label ID="tradeNameLabel" runat="server"  Width="200px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="tradeNameTextBox" runat="server" Width="200px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Package Description"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="packageDescriptionLabel" runat="server"  Width="90px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="packageDescriptionTextBox" runat="server" Width="90px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Dispensing Unit"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="dispensingUnitLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="dispensingUnitTextBox" runat="server" Width="100px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                                                                                                             
                                                                        <asp:TemplateField ItemStyle-Width="99%" ItemStyle-Height="99%">  
                                                                            <ItemTemplate>
                                                                                   <asp:Button runat="server" ID="RemoveSubItemButton" Text="Remove Sub-Item" Enabled="false"
                                                                                       OnCommand="SubItemGridView_ButtonCommand"   CommandName="RemoveSubItem"  
                                                                                       CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                       ButtonType="Button"  Width="82px" 
                                                                                       OnDataBinding="RemoveSubItemButton_DataBinding"   >
                                                                                  </asp:Button>   

                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                       
                                                                        

                                                                    </Columns>
                                                        
                                                                </ep:GridView>
                                                            </ContentTemplate>
                                                      </asp:UpdatePanel> 
                                             </div>                                                                          
                                        </td>
                                    </tr>

                                    <tr style="height:30px;" >
                                        <td>
                                        </td>
                                        <td colspan="7">
                                            <table class="DrugItemDetailsFooterArea" style="border-top:solid 1px black; height: 28px; table-layout:fixed; width: 860px; text-align:center;"  >
                                             <colgroup >
                                                <col style="width: 277px" />
                                                <col style="width: 268px" />
                                                <col style="width: 323px" />
                                             </colgroup >
                                             <tr>
                                                    <td style="text-align:left; width:98%;">
                                                        <asp:Label ID="LastModificationDateLabel" runat="server" Text="test" OnDataBinding="LastModificationDateLabel_OnDataBinding" />
                                                    </td>
                                                    <td style="width:98%;">
                                                        <asp:Label ID="LastModifedByLabel" runat="server"  OnDataBinding="LastModifedByLabel_OnDataBinding"  />
                                                    </td>
                                                    <td style="text-align:right; width:98%;">
                                                        <asp:Label ID="LastModificationTypeLabel" runat="server"  OnDataBinding="LastModificationTypeLabel_OnDataBinding"  />
                                                    </td>
                                                </tr>
                                            </table>

                                        </td>
                                        <td>
                                        </td>
                                    </tr>             
                                
                                </table>
                  
                            </ItemTemplate>
                            <EditItemTemplate>
                            
                                <table style="height: 464px; table-layout:fixed; width: 886px;" class="DrugItemDetailsEditArea"  >          
                                <colgroup >
                                    <col style="width: 20px;" />
                                    <col style="width: 90px;" />
                                    <col style="width: 90px;" />
                                    <col style="width: 100px;" />
                                    <col style="width: 80px;" />
                                    <col style="width: 90px;" />
                                    <col style="width: 90px;" />
                                    <col style="width: 100px;" />
                                    <col style="width: 100px;" />
                                    <col style="width: 20px;" />
                                </colgroup >
                                <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="DateEnteredMarketLabel" runat="server"  Text='<%# MultilineText( new string[] {"Date", "Entered", "Market"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="PrimeVendorFlagLabel" runat="server" Text='<%# MultilineText( new string[] {"Prime", "Vendor"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PrimeVendorChangedDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Prime Vendor", "Changed Date"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PassThroughFlagLabel" runat="server" Text='<%# MultilineText( new string[] {"Pass", "Through", "Flag"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="VAClassLabel" runat="server" Text='<%# MultilineText( new string[] {"VA", "Class"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExcludeFromExportLabel" runat="server" Text='<%# MultilineText( new string[] {"Exclude", "From", "Export"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="NonTAALabel" runat="server" Text='<%# MultilineText( new string[] { "Non", "TAA" } ) %>' Width="94%" />   
                                        </td>
                                        <td>
                                            <asp:Label ID="IncludedFETAmountLabel" runat="server" Text='<%# MultilineText( new string[] { "FET", "Amount" } ) %>' Width="94%" /> 
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="DateEnteredMarketTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "DateEnteredMarket", "{0:d}" )%>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="PrimeVendorFlagCheckBox" runat="server" OnDataBinding="PrimeVendorFlagCheckBox_OnDataBinding"   Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PrimeVendorChangedDateTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "PrimeVendorChangedDate", "{0:d}" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PassThroughFlagTextBox" runat="server" MaxLength="1" Text='<%# DataBinder.Eval( Container.DataItem, "PassThrough" )%>'  Width="88%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="VAClassTextBox" runat="server" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "VAClass" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="ExcludeFromExportCheckBox" runat="server" OnDataBinding="ExcludeFromExportCheckBox_OnDataBinding"   Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="NonTAACheckBox" runat="server" OnDataBinding="NonTAACheckBox_OnDataBinding" Width="94%" /> 
                                        </td>
                                        <td>
                                            <asp:TextBox ID="IncludedFETAmountTextBox" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IncludedFETAmount" )%>' Width="94%" />  
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:60px;" >
                                         <td>
                                        </td>
                                       <td>
                                            <asp:Label ID="UnitOfSaleLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit of", "Sale"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitOfSaleLabel" runat="server" Text='<%# MultilineText( new string[] {"Quantity in", "Unit of", "Sale"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitPackageLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit", "Package"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="QuantityInUnitPackageLabel" runat="server" Text='<%# MultilineText( new string[] {"Quantity in", "Unit Package"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="UnitOfMeasureLabel" runat="server" Text='<%# MultilineText( new string[] {"Unit of", "Measure"} ) %>'  Width="94%" />       
                                        </td>
                                         <td>
                                            <asp:Label ID="PriceMultiplierLabel" runat="server" Text='<%# MultilineText( new string[] {"Price", "Multiplier"} ) %>'  Width="94%" />       
                                        </td>
                                         <td>
                                            <asp:Label ID="PriceDividerLabel" runat="server" Text='<%# MultilineText( new string[] {"Price", "Divider"} ) %>'  Width="94%" />       
                                        </td>
                                         <td>
                                        </td>
                                        <td>
                                        </td>
                         
                                  </tr>
                                    <tr style="height:25px;" >
                                         <td>
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="UnitOfSaleDropDownList" runat="server"  Width="94%" OnDataBound="UnitOfSaleDropDownList_OnDataBound" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="QuantityInUnitOfSaleTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitOfSale" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="UnitPackageDropDownList" runat="server"  Width="94%" OnDataBound="UnitPackageDropDownList_OnDataBound" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="QuantityInUnitPackageTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitPackage" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="UnitOfMeasureDropDownList" runat="server"  Width="94%" OnDataBound="UnitOfMeasureDropDownList_OnDataBound" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PriceMultiplierTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PriceMultiplier" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PriceDividerTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PriceDivider" )%>' Width="50%" />
                                        </td>
                                          <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                   <tr style="height:27px; border-top: 1px black;">
                                        <td colspan="8" style="vertical-align:bottom; text-align:center;" >
                                            <asp:Label ID="DistributorGridLabel" runat="server" Text="Specialty Distributors" CssClass="DrugItemHeaderText" />
                                        </td>
                                    </tr>
                                   <tr style="height:22px; border-top: 1px black; vertical-align:bottom; text-align:center;" >
                                        <td colspan="2" style="text-align:left;">
                                            <asp:Button ID="AddNewDistributorButton" runat="server" Width="94%" Text="Add Distributor" OnClick="AddNewDistributorButton_OnClick" />
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>

                                   <tr style="height:118px;" >
                                        <td colspan="9" style="text-align:left;">
                                              <div id="DistributorGridViewDiv"  runat="server" style="width:860px; height:126px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                       <ep:UpdatePanelEventProxy ID="InsertDistributorButtonClickUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="ChangeDistributorHighlightUpdatePanelEventProxy" runat="server" />
 
                                                      <asp:UpdatePanel ID="DistributorGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                       <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="InsertDistributorButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="ChangeDistributorHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                       </Triggers>
                                                        <ContentTemplate> 
                             
                                                             <ep:GridView ID="DistributorGridView" 
                                                                        runat="server" 
                                                                        DataKeyNames="DrugItemDistributorId"  
                                                                        AutoGenerateColumns="False" 
                                                                        Width="840px" 
                                                                        CssClass="DrugItemGrids" 
                                                                        Visible="True" 
                                                                        OnRowDataBound="DistributorGridView_RowDataBound"
                                                                        AutoGenerateEditButton="false"
                                                                        EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                        onprerender="DistributorGridView_PreRender" 
                                                                        onrowediting="DistributorGridView_RowEditing" 
                                                                        onrowcancelingedit="DistributorGridView_RowCancelingEdit"
                                                                        AllowInserting="True"
                                                                        InsertCommandColumnIndex="2"
                                                                        EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                        EmptyDataText="There are no distributors defined for the selected item."
                                                                       >
                                                                    <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                    <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                    <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                    <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                 
                                                                    <Columns>  
                                                                     
                                                                        <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="EditButton" Text="Edit" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" CommandName="EditItem" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    ButtonType="Button" Width="60px" CausesValidation="false" 
                                                                                    Visible="true" >            
                                                                                    </asp:Button >                                    
                                                                                <asp:Button ID="SaveButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    CommandName="SaveItem" Width="60px" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" Text="Save" Visible="false" />
                                                                                <asp:Button ID="CancelButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                    CommandName="Cancel" Width="60px" 
                                                                                    OnCommand="DistributorGridView_ButtonCommand" Text="Cancel" Visible="false" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                                       
                                                                        
                                                                        <asp:TemplateField HeaderText="Distributor Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorNameLabel" runat="server"  Width="165px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "DistributorName"  )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                 <ajax:ComboBox ID="distributorNameComboBox"   DropDownStyle="DropDown"  CssClass="CustomComboBoxStyle" MaxLength="100" style="z-index: 15;"  RenderMode="Block" AutoCompleteMode="None" ItemInsertLocation="Prepend"  DataValueField="DistributorName"  Width="165px"   DataTextField="DistributorName" 
                                                                                     runat="server" OnDataBound="distributorNameComboBox_DataBound" AutoPostBack="true" >
                                                                                            </ajax:ComboBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Contact Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                              <ItemTemplate>
                                                                                <asp:Label ID="contactNameLabel" runat="server"  Width="160px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "ContactPerson" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="contactNameTextBox" runat="server" MaxLength="30" Width="160px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "ContactPerson" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Phone"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorPhoneLabel" runat="server"  Width="100px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Phone" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="distributorPhoneTextBox" runat="server" Width="100px" MaxLength="15" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Phone" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>
                                                                                        
                                                                        <asp:TemplateField HeaderText="Notes"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="distributorNotesLabel" runat="server"  Width="215px"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Notes" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="distributorNotesTextBox" runat="server" Width="215px" MaxLength="800" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Notes" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>                                                                                        
                                                                                                           
                                                                        <asp:TemplateField ItemStyle-Width="99%" ItemStyle-Height="99%">  
                                                                            <ItemTemplate>
                                                                                   <asp:Button runat="server" ID="RemoveDistributorButton" Text="Remove Distributor" 
                                                                                       OnCommand="DistributorGridView_ButtonCommand"   CommandName="RemoveDistributor"  
                                                                                       CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemDistributorId") %>' 
                                                                                       ButtonType="Button"  Width="82px" 
                                                                                       CssClass="ButtonWrapText"   >
                                                                                  </asp:Button>   

                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                       
                                                                        

                                                                    </Columns>
                                                        
                                                                </ep:GridView>
                                                            </ContentTemplate>
                                                      </asp:UpdatePanel> 
                                             </div>                                                                          
                                        </td>
                                    </tr>
                                    <tr style="height:27px; border-top: 1px black;">
                                        <td colspan="8" style="text-align:center; vertical-align:bottom;">
                                            <asp:Label ID="SubItemsLabel" runat="server" Text="Sub-Items" CssClass="DrugItemHeaderText" />
                                        </td>
                                    </tr>
                                    <tr style="height:22px; border-top: 1px black; text-align:right; vertical-align:bottom;" >
                                        <td colspan="2" style="text-align:left;">
                                            <asp:Button ID="AddNewSubItemButton" runat="server" Width="94%" Text="Add Sub-Item" OnClick="AddNewSubItemButton_OnClick" />
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                     </tr>
                                   
                                    <tr style="height:118px;" >
                                        <td colspan="9" style="text-align:left;">
                                              <div id="SubItemGridViewDiv"  runat="server" style="width:860px; height:126px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                       <ep:UpdatePanelEventProxy ID="InsertSubItemButtonClickUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="ChangeSubItemHighlightUpdatePanelEventProxy" runat="server" />
                                                       <ep:UpdatePanelEventProxy ID="OuterFormViewWasSavedUpdatePanelEventProxy" runat="server" />

                                                      <asp:UpdatePanel ID="SubItemGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                       <Triggers>
                                                            <asp:AsyncPostBackTrigger ControlID="InsertSubItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="ChangeSubItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                            <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                       </Triggers>
                                                        <ContentTemplate> 
                             
                                                             <ep:GridView ID="SubItemGridView" 
                                                                        runat="server" 
                                                                        DataKeyNames="DrugItemSubItemId"  
                                                                        AutoGenerateColumns="False" 
                                                                        Width="810px" 
                                                                        CssClass="DrugItemGrids" 
                                                                        Visible="True" 
                                                                        OnRowDataBound="SubItemGridView_RowDataBound"
                                                                        AutoGenerateEditButton="false"
                                                                        EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                        onprerender="SubItemGridView_PreRender" 
                                                                        onrowediting="SubItemGridView_RowEditing" 
                                                                        onrowcancelingedit="SubItemGridView_RowCancelingEdit"
                                                                        AllowInserting="True"
                                                                        InsertCommandColumnIndex="2"
                                                                        EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                        EmptyDataText="There are no sub-items defined for the selected item."
                                                                       >
                                                                    <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                    <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                    <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                    <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                 
                                                                    <Columns>  
                                                                     
                                                                        <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="EditButton" Text="Edit" 
                                                                                    OnCommand="SubItemGridView_ButtonCommand" CommandName="EditItem" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    ButtonType="Button" Width="60px" CausesValidation="false" 
                                                                                    Visible="true" >            
                                                                                    </asp:Button >                                    
                                                                                <asp:Button ID="SaveButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    CommandName="SaveItem" Width="60px" 
                                                                                    OnCommand="SubItemGridView_ButtonCommand" Text="Save" Visible="false" />
                                                                                <asp:Button ID="CancelButton" runat="server" ButtonType="Button" 
                                                                                    CausesValidation="false" 
                                                                                    CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                    CommandName="Cancel" Width="60px" 
                                                                                    OnCommand="SubItemGridView_ButtonCommand" Text="Cancel" Visible="false" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                                       
                                                                        
                                      
                                                                        
                                                                         <asp:TemplateField HeaderText="Sub-Item Identifier"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                                                                                     <ItemTemplate>
                                                                                <asp:Label ID="subItemIdentifierLabel" runat="server"  Width="60px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "SubItemIdentifier" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="subItemIdentifierTextBox" runat="server" Width="60px" 
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "SubItemIdentifier" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Generic Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="genericNameLabel" runat="server"  Width="200px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Generic"  )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="genericNameTextBox" runat="server" Width="200px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "Generic"  )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Trade Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                                                                                     <ItemTemplate>
                                                                                <asp:Label ID="tradeNameLabel" runat="server"  Width="200px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="tradeNameTextBox" runat="server" Width="200px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Package Description"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                         
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="packageDescriptionLabel" runat="server"  Width="90px"  
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="packageDescriptionTextBox" runat="server" Width="90px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>

                                                                        </asp:TemplateField>

                                                                         <asp:TemplateField HeaderText="Dispensing Unit"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="dispensingUnitLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="dispensingUnitTextBox" runat="server" Width="100px" TextMode="MultiLine"  CssClass="DrugItemMultilineInEditMode"
                                                                                    Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                                                                                                             
                                                                        <asp:TemplateField ItemStyle-Width="99%" ItemStyle-Height="99%">  
                                                                            <ItemTemplate>
                                                                                   <asp:Button runat="server" ID="RemoveSubItemButton" Text="Remove Sub-Item" 
                                                                                       OnCommand="SubItemGridView_ButtonCommand"   CommandName="RemoveSubItem"  
                                                                                       CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemSubItemId") %>' 
                                                                                       ButtonType="Button"  Width="82px" 
                                                                                       CssClass="ButtonWrapText"   >
                                                                                  </asp:Button>   

                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                       
                                                                        

                                                                    </Columns>
                                                        
                                                                </ep:GridView>
                                                            </ContentTemplate>
                                                      </asp:UpdatePanel> 
                                             </div>                                                                          
                                        </td>
                                    </tr>
  
                                    <tr style="height:30px;" >
                                        <td colspan="9">
                                            <table class="DrugItemDetailsFooterArea" style="border-top:solid 1px black;  height: 28px; table-layout:fixed; width: 868px; text-align:center;"  >
                                             <colgroup >
                                                <col style="width: 277px" />
                                                <col style="width: 268px" />
                                                <col style="width: 323px" />
                                             </colgroup >
                                             <tr>
                                                    <td style="text-align:left; width:98%;">
                                                        <asp:Label ID="LastModificationDateLabel" runat="server" Text="test" OnDataBinding="LastModificationDateLabel_OnDataBinding" />
                                                    </td>
                                                    <td style="width:98%;">
                                                        <asp:Label ID="LastModifedByLabel" runat="server"  OnDataBinding="LastModifedByLabel_OnDataBinding"  />
                                                    </td>
                                                    <td style="text-align:right; width:98%;">
                                                        <asp:Label ID="LastModificationTypeLabel" runat="server"  OnDataBinding="LastModificationTypeLabel_OnDataBinding"  />
                                                    </td>
                                                </tr>
                                            </table>

                                        </td>
                                    </tr>                      
                                
                                </table>
          
                        
                            </EditItemTemplate>
                            
                            </asp:FormView>
                        </div>
                  </asp:Panel>
                </td>
            </tr>
        </table>
            <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />  
    </form>
</body>
</html>
