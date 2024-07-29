<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" CodeBehind="ItemPriceDetails.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ItemPriceDetails" %>

<%@ Register src="ItemHeader.ascx" tagname="ItemHeader" tagprefix="uc1" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Price Details</title>
        
       <script type="text/javascript">
       <!--
            function CloseWindow( withRefresh )
            {
                window.opener.document.forms[0].RefreshItemPriceScreenOnSubmit.value = withRefresh;
                window.opener.document.forms[0].RebindItemPriceScreenOnRefreshOnSubmit.value = 'false';               

                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
  
          
</head>
<body >

    <form id="ItemPriceDetailsForm" runat="server" 
    style="z-index: 5; position:fixed; top:0px; left:0px; width:1096px; height:430px;" >
   
       <asp:ScriptManager ID="ItemPriceDetailsScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ItemPriceDetailsScriptManager_OnAsyncPostBackError" >
           
        </asp:ScriptManager>
  
    
        <table  style="width:1094px; height: 364px; table-layout:fixed; border:solid 1px black;" class="ItemPriceDetails"  >
            <colgroup >
                <col style="width: 1090px;" />
            </colgroup>
 
            <tr style="height:80px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 79px; width: 1090px;" > 
                        <asp:UpdatePanel ID="SelectedItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemPriceDetailsUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc1:ItemHeader ID="SelectedItemHeader" runat="server"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
 
            <tr style="height:32px; text-align:right;" >   
                <td  style="width:98%; text-align:right;" >
                     <table style="border-top:solid 1px black;  width:1088px; height:28px; table-layout:fixed; text-align:right; padding-top:4px;" >
                        <tr style:"text-align:right;">    
                            <td  >
                            </td>
                            <td style="width: 80px" >               
                                <asp:Button runat="server"  ID="PrintItemPriceDetailsButton" Text="Print" OnClientClick="window.print();"  >            
                                </asp:Button >      
                            </td>
                            <td style="width: 80px" >     
                                <asp:Button runat="server"  ID="UpdateItemPriceDetailsButton" Text="Save" OnClick="UpdateItemPriceDetailsButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="width: 80px" >     
                                <asp:Button runat="server"  ID="CancelItemPriceDetailsButton" Text="Close" OnClick="CancelItemPriceDetailsButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:200px; vertical-align:top; border: solid 1px black">
              <td>
                <asp:Panel ID="ItemPriceDetailsBodyPanel" runat="server" Width="1084px" Height="267px" >
                        <div id="ItemPriceDetailsBodyDiv" style="width:1084px; height:264px; top:0px;  left:0px; border:solid 1px black; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemPriceDetailsUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="ItemPriceDetailsFormView" 
                                runat="server" OnDataBound="ItemPriceDetailsFormView_OnDataBound" 
                                DefaultMode="Edit"  OnPreRender="ItemPriceDetailsFormView_OnPreRender"
                                 RenderOuterTable="false" >
                            <ItemTemplate>
                                <asp:Table ID="ItemPriceDetailsItemTable" runat="server" style="height: 262px; table-layout:fixed; width: 1080px;" >
                                 
                                    <asp:TableHeaderRow style="height:60px;" >
                                        <asp:TableHeaderCell style="width: 40px;" >
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="TrackingCustomerRatioCell" runat="server"   style="vertical-align:middle; text-align:center; width: 70px;">
                                            <asp:Label ID="TrackingCustomerRatioLabel" runat="server"   Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Ratio"} ) %>' Width="94%"  />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="TrackingCustomerPriceCell" runat="server"   style="vertical-align:middle; text-align:center; width: 90px;">
                                            <asp:Label ID="TrackingCustomerPriceLabel" runat="server"    Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Price"} ) %>' Width="94%" />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="TrackingCustomerNameCell" runat="server"   style="vertical-align:middle; text-align:center; width: 140px;">
                                            <asp:Label ID="TrackingCustomerNameLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Name"} ) %>'  Width="94%" />       
                                        </asp:TableHeaderCell>       
                                         <asp:TableHeaderCell ID="TrackingCustomerFOBTermsCell" runat="server"   style="vertical-align:middle; text-align:center; width: 140px;">
                                            <asp:Label ID="TrackingCustomerFOBTermsLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "FOB Terms"} ) %>'  Width="94%" />       
                                        </asp:TableHeaderCell>     
                                        <asp:TableHeaderCell ID="BlankHeaderCell1" runat="server" style="width: 70px;" >
                                        </asp:TableHeaderCell>                                       
                                         <asp:TableHeaderCell ID="TieredPriceGridHeaderCell" runat="server"  style="width: 480px;" >
                                        </asp:TableHeaderCell>                                    
                                    </asp:TableHeaderRow>
                                    <asp:TableRow style="height:25px;" >
                                        <asp:TableCell style="width: 40px;" >
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerRatioDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 70px;"  AssociatedHeaderCellID="TrackingCustomerRatioCell">
                                            <asp:Label ID="TrackingCustomerRatioDataLabel" runat="server" Text='<%# Eval("TrackingCustomerRatio") %>' Width="88%" />
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerPriceDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 90px;"  AssociatedHeaderCellID="TrackingCustomerPriceCell" >
                                            <asp:Label ID="TrackingCustomerPriceDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerPrice" )%>' runat="server" Width="94%" />
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerNameDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 140px;"  AssociatedHeaderCellID="TrackingCustomerNameCell" >
                                            <asp:Label ID="TrackingCustomerNameDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerName" )%>' Width="94%" />       
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerFOBTermsDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 140px;"  AssociatedHeaderCellID="TrackingCustomerFOBTermsCell" >
                                            <asp:Label ID="TrackingCustomerFOBTermsDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerFOBTerms" )%>' Width="94%" />       
                                        </asp:TableCell>
                                        <asp:TableCell ID="BlankDataCell1" runat="server"   AssociatedHeaderCellID="BlankHeaderCell1" style="width: 70px;">
                                        </asp:TableCell>
                                          
                                        <asp:TableCell rowspan="4" ID="TieredPriceGridCell" runat="server"  style="vertical-align:middle; text-align:center; width: 480px; "  AssociatedHeaderCellID="TieredPriceGridHeaderCell" >
                                             <table style="height: 140px; table-layout:fixed; width: 480px; text-align:center;"  >
                                                <colgroup >                                                    
                                                    <col style="width: 136px" />
                                                    <col style="width: 136px" />
                                                    <col style="width: 136px" />
                                                </colgroup >

                                                  <tr style="vertical-align:top;">
                                                        <td colspan="3" >
                                                            <asp:Label ID="TieredPricingGridLabel" runat="server" Text="Tiered Pricing" CssClass="ItemHeaderText"/>
                                                        </td>
                                                  </tr>
                                                  <tr style="vertical-align:top;">
                                                        <td>
                                                        </td>
                                                        <td>
                                                        </td>
                                                        <td>
                                                        </td>
                                                   </tr>
                                                   <tr style="vertical-align:top;">
                                                
                                                        <td colspan="3">
                                                        
                                                          <div id="TieredPriceGridViewDiv"  runat="server" style="width:474px; height:138px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                                   <ep:UpdatePanelEventProxy ID="InsertTieredPriceButtonClickUpdatePanelEventProxy" runat="server" />
                                                                   <ep:UpdatePanelEventProxy ID="ChangeTieredPriceHighlightUpdatePanelEventProxy" runat="server" />
                                                                   <ep:UpdatePanelEventProxy ID="OuterFormViewWasSavedUpdatePanelEventProxy" runat="server" />

                                                                  <asp:UpdatePanel ID="TieredPriceGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                                   <Triggers>
                                                                        <asp:AsyncPostBackTrigger ControlID="InsertTieredPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                        <asp:AsyncPostBackTrigger ControlID="ChangeTieredPriceHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                        <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                   </Triggers>
                                                                    <ContentTemplate> 
                                         
                                                                         <ep:GridView ID="TieredPriceGridViewReadOnly" 
                                                                                    runat="server" 
                                                                                    DataKeyNames="ItemTieredPriceId"  
                                                                                    AutoGenerateColumns="False" 
                                                                                    Width="472px" 
                                                                                    CssClass="ItemGrids" 
                                                                                    Visible="True" 
                                                                                    OnRowDataBound="TieredPriceGridView_RowDataBound"
                                                                                    AutoGenerateEditButton="false"
                                                                                    EditRowStyle-CssClass="ItemEditRowStyle" 
                                                                                    onprerender="TieredPriceGridView_PreRender" 
                                                                                    onrowediting="TieredPriceGridView_RowEditing" 
                                                                                    onrowcancelingedit="TieredPriceGridView_RowCancelingEdit"
                                                                                    AllowInserting="True"
                                                                                    InsertCommandColumnIndex="2"
                                                                                    EmptyDataRowStyle-CssClass="ItemDetailsEditArea" 
                                                                                    EmptyDataText="There are no tiered prices for the selected price."
                                                                                   >
                                                                                <HeaderStyle CssClass="ItemGridHeaders" />
                                                                                <RowStyle  CssClass="ItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                                <AlternatingRowStyle CssClass="ItemGridAltItems" />
                                                                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                             
                                                                                <Columns>   
                                                                                                                                       
                                                                                    
                                                                                    <asp:TemplateField >  
                                                                                        <ItemTemplate>
                                                                                               <asp:Button  CausesValidation="false" Visible="true" Enabled="false" runat="server" ID="EditButton" Text="Edit" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button  CausesValidation="false" Visible="false" Enabled="false"  runat="server" ID="SaveButton" Text="Save" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="SaveItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button CausesValidation="false"  Visible="false" Enabled="false"  runat="server" ID="CancelButton" Text="Cancel" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   
                                                                          
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>      
                                                                                    
                                                                                    <asp:TemplateField HeaderText="Tier Sequence"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="tierSequenceLabel" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TierSequence" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="tierSequenceTextBox" runat="server" Width="80px" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "TierSequence" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price Start Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceStartDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStartDate", "{0:d}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceStartDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStartDate", "{0:d}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price End Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceEndDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStopDate", "{0:d}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceEndDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStopDate", "{0:d}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "Price", "{0:c}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "Price", "{0:c}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                    <asp:TemplateField HeaderText="Tier Criteria"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="tierCriteriaLabel" runat="server"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TierCriteria" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <ep:TextBox ID="tierCriteriaTextBox" runat="server" TextMode="MultiLine" MaxLength="200" CssClass="ItemMultilineInEditMode"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TierCriteria" )%>' ></ep:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                    
                                                                                    <asp:TemplateField HeaderText="Minimum Value"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="minimumValueLabel" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "MinimumValue" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="minimumValueTextBox" runat="server" Width="80px" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "MinimumValue" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                    
                                                                                    <asp:TemplateField >
                                                                                        <ItemTemplate>
                                                                                            <asp:Button runat="server"  ID="RemovePriceButton" Enabled="false"  Text="Remove Price"  OnCommand="TieredPriceGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'  CssClass="ButtonWrapText" ControlStyle-Width="82px" >            
                                                                                                </asp:Button >                                    
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>                                

                                                                                </Columns>
                                                                    
                                                                            </ep:GridView>
                                                                        </ContentTemplate>
                                                                  </asp:UpdatePanel> 
                                                         </div>                                                      
                                                    </td>
                                                </tr>
                                             </table>                                    
                                        </asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow style="height:30px;" >
                                        <asp:TableCell >
                                        </asp:TableCell>
                                        <asp:TableCell ColumnSpan="7">
                                            <table style="border-top:solid 1px black; height: 28px; table-layout:fixed; width: 934px;" align="center"  >                                             
                                                <colgroup>
                                                     <col style="width:280px;" />
                                                     <col style="width:272px;" />
                                                     <col style="width:332px;" />
                                                </colgroup>                                             
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

                                        </asp:TableCell>
                                        <asp:TableCell>
                                        </asp:TableCell>
                                    </asp:TableRow>             
                                
                                </asp:Table>
                  
                            </ItemTemplate>
                            <EditItemTemplate>
                            
                                <asp:Table ID="ItemPriceDetailsEditItemTable" runat="server"  style="height: 262px; table-layout:fixed; width: 1080px;" class="ItemPriceDetailsEditArea" >                                    
                                  
                                    <asp:TableHeaderRow style="height:60px;" >
                                        <asp:TableHeaderCell ID="BlankHeaderCell0" runat="server" >
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell  ID="TrackingCustomerRatioCell" runat="server"   style="vertical-align:middle; text-align:center; width: 70px;">
                                            <asp:Label ID="TrackingCustomerRatioLabel" runat="server" Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Ratio"} ) %>' Width="94%"  />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="TrackingCustomerPriceCell" runat="server"   style="vertical-align:middle; text-align:center; width: 90px;">
                                            <asp:Label ID="TrackingCustomerPriceLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Price"} ) %>' Width="94%" />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell  ID="TrackingCustomerNameCell" runat="server"   style="vertical-align:middle; text-align:center; width: 140px;">
                                            <asp:Label ID="TrackingCustomerNameLabel" runat="server" Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Name"} ) %>'  Width="94%" />       
                                        </asp:TableHeaderCell >  
                                         <asp:TableHeaderCell ID="TrackingCustomerFOBTermsCell" runat="server"   style="vertical-align:middle; text-align:center; width: 140px;">
                                            <asp:Label ID="TrackingCustomerFOBTermsLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "FOB Terms"} ) %>'  Width="94%" />       
                                        </asp:TableHeaderCell>   
                                        <asp:TableHeaderCell ID="BlankHeaderCell1" runat="server" style="width: 50px;">
                                        </asp:TableHeaderCell>                                           
                                        <asp:TableHeaderCell  rowspan="4" ID="TieredPriceGridHeaderCell" runat="server" >
                                             <table style="height: 140px; table-layout:fixed; width: 480px; text-align:center;"  >
                                                <colgroup>
                                                    <col style="width: 136px;" />
                                                    <col style="width: 136px;" />
                                                    <col style="width: 136px;" />
                                                </colgroup>
                                                  <tr style="vertical-align:top;">
                                                        <td colspan="3" >
                                                            <asp:Label ID="TieredPriceGridLabel" runat="server" Text="Tiered Pricing" CssClass="ItemHeaderText"/>
                                                        </td>
                                                  </tr>
                                                  <tr style="vertical-align:top;">
                                                        <td>
                                                            <asp:Button ID="AddTieredPriceButton" runat="server" Text="Add Tiered Price" Width="99%" OnClick="AddNewTieredPriceButton_OnClick" />
                                                        </td>
                                                        <td>
                                                        </td>
                                                        <td>
                                                        </td>
                                                   </tr>
                                                   <tr style="vertical-align:top;">
                                                
                                                        <td colspan="3">
                                                        
                                                          <div id="TieredPriceGridViewDiv"  runat="server" style="width:474px; height:138px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                                   <ep:UpdatePanelEventProxy ID="InsertTieredPriceButtonClickUpdatePanelEventProxy" runat="server" />
                                                                   <ep:UpdatePanelEventProxy ID="ChangeTieredPriceHighlightUpdatePanelEventProxy" runat="server" />
                                                                   <ep:UpdatePanelEventProxy ID="OuterFormViewWasSavedUpdatePanelEventProxy" runat="server" />

                                                                  <asp:UpdatePanel ID="TieredPriceGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                                   <Triggers>
                                                                        <asp:AsyncPostBackTrigger ControlID="InsertTieredPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                        <asp:AsyncPostBackTrigger ControlID="ChangeTieredPriceHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                        <asp:AsyncPostBackTrigger ControlID="OuterFormViewWasSavedUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                                                   </Triggers>
                                                                    <ContentTemplate> 
                                         
                                                                         <ep:GridView ID="TieredPriceGridView" 
                                                                                    runat="server" 
                                                                                    DataKeyNames="ItemTieredPriceId"  
                                                                                    AutoGenerateColumns="False" 
                                                                                    Width="472px" 
                                                                                    CssClass="ItemGrids" 
                                                                                    Visible="True" 
                                                                                    OnRowDataBound="TieredPriceGridView_RowDataBound"
                                                                                    AutoGenerateEditButton="false"
                                                                                    EditRowStyle-CssClass="ItemEditRowStyle" 
                                                                                    onprerender="TieredPriceGridView_PreRender" 
                                                                                    onrowediting="TieredPriceGridView_RowEditing" 
                                                                                    onrowcancelingedit="TieredPriceGridView_RowCancelingEdit"
                                                                                    AllowInserting="True"
                                                                                    
                                                                                    EmptyDataRowStyle-CssClass="ItemDetailsEditArea" 
                                                                                    EmptyDataText="There are no tiered prices for the selected price."
                                                                                   >
                                                                                <HeaderStyle CssClass="ItemGridHeaders" />
                                                                                <RowStyle  CssClass="ItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                                <AlternatingRowStyle CssClass="ItemGridAltItems" />
                                                                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                             
                                                                                <Columns>   
                                                                                                                                       
                                                                                    
                                                                                    <asp:TemplateField >  
                                                                                        <ItemTemplate>
                                                                                               <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="SaveItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   
                                                                          
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>      
                                                                                    
                                                                                     <asp:TemplateField HeaderText="Tier Sequence"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="tierSequenceLabel" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TierSequence" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="tierSequenceTextBox" runat="server" Width="80px" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "TierSequence" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price Start Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceStartDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStartDate", "{0:d}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceStartDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStartDate", "{0:d}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price End Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceEndDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStopDate", "{0:d}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceEndDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "TieredPriceStopDate", "{0:d}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Price"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="priceLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "Price", "{0:c}" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="priceTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "Price", "{0:c}" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                     <asp:TemplateField HeaderText="Tier Criteria"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="tierCriteriaLabel" runat="server"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TierCriteria" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <ep:TextBox ID="tierCriteriaTextBox" runat="server" TextMode="MultiLine" MaxLength="200" CssClass="ItemMultilineInEditMode"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TierCriteria" )%>' ></ep:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>
                                                                               
                                                                                    <asp:TemplateField HeaderText="Minimum Value"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="minimumValueLabel" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "MinimumValue" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <asp:TextBox ID="minimumValueTextBox" runat="server" Width="80px" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "MinimumValue" )%>' ></asp:TextBox>
                                                                                        </EditItemTemplate>
                                                                                    </asp:TemplateField>
 
                                                                                    <asp:TemplateField >
                                                                                        <ItemTemplate>
                                                                                            <asp:Button runat="server"  ID="RemovePriceButton" Text="Remove Price" OnCommand="TieredPriceGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemTieredPriceId") %>'  CssClass="ButtonWrapText"  ControlStyle-Width="82px" >            
                                                                                                </asp:Button >                                    
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>                                

                                                                                </Columns>
                                                                    
                                                                            </ep:GridView>
                                                                        </ContentTemplate>
                                                                  </asp:UpdatePanel> 
                                                         </div>                                                      
                                                    </td>
                                                </tr>
                                             </table>                                    
                                        </asp:TableHeaderCell>
                                    </asp:TableHeaderRow>
                                    <asp:TableRow style="height:25px;" >
                                        <asp:TableCell ID="BlankDataCell0" runat="server"   AssociatedHeaderCellID="BlankHeaderCell0" >
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerRatioDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 70px;"  AssociatedHeaderCellID="TrackingCustomerRatioCell">
                                            <asp:TextBox ID="TrackingCustomerRatioTextBox" runat="server"  MaxLength="100" Text='<%# Eval("TrackingCustomerRatio") %>' Width="88%" />
                                        </asp:TableCell>
                                        <asp:TableCell ID="TrackingCustomerPriceDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 90px;"  AssociatedHeaderCellID="TrackingCustomerPriceCell" >
                                            <asp:TextBox ID="TrackingCustomerPriceTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerPrice" )%>' Width="94%" />
                                        </asp:TableCell>
                                        <asp:TableCell  ID="TrackingCustomerNameDataCell" runat="server"  style="vertical-align:middle; text-align:center; width: 140px;"  AssociatedHeaderCellID="TrackingCustomerNameCell" >
                                            <asp:TextBox ID="TrackingCustomerNameTextBox" runat="server" MaxLength="100" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerName" )%>' Width="94%" />
                                        </asp:TableCell>
                                        <asp:TableCell  ID="TrackingCustomerNameFOBTermsCell" runat="server"  style="vertical-align:middle; text-align:center; width: 140px;"  AssociatedHeaderCellID="TrackingCustomerFOBTermsCell" >
                                            <asp:TextBox ID="TrackingCustomerFOBTermsTextBox" runat="server" MaxLength="11" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerFOBTerms" )%>' Width="94%" />
                                        </asp:TableCell>
                                        <asp:TableCell ID="BlankDataCell1" runat="server"   AssociatedHeaderCellID="BlankHeaderCell1" style="width: 50px;" >
                                        
                                        </asp:TableCell>
                                                                            
                                    </asp:TableRow>
                                    <asp:TableRow style="height:20px;" >
                                        <asp:TableCell></asp:TableCell>
                                        <asp:TableCell>
                                        </asp:TableCell>
                                        <asp:TableCell ColumnSpan="4">                                   
                                        </asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow style="height:20px;" >
                                        <asp:TableCell></asp:TableCell>
                                        <asp:TableCell>
                                        </asp:TableCell>
                                        <asp:TableCell ColumnSpan="4"> 
                                         </asp:TableCell>
                                   </asp:TableRow>
                                    <asp:TableRow style="height:30px;" >
                                        <asp:TableCell ColumnSpan="7">
                                            <table class="ItemPriceDetailsFooterArea" style="border-top:solid 1px black;  height: 28px; table-layout:fixed; width: 1076px;" >
                                                <colgroup>
                                                     <col style="width:280px;" />
                                                     <col style="width:272px;" />
                                                     <col style="width:332px;" />
                                                </colgroup>
                                             <tr>
                                                    <td style="text-align:left; width:98%;">
                                                        <asp:Label ID="LastModificationDateLabel" runat="server" Text="test" OnDataBinding="LastModificationDateLabel_OnDataBinding" />
                                                    </td>
                                                    <td style="width:99%;">
                                                        <asp:Label ID="LastModifedByLabel" runat="server"  OnDataBinding="LastModifedByLabel_OnDataBinding"  />
                                                    </td>
                                                    <td style="text-align:right; width:98%;">
                                                        <asp:Label ID="LastModificationTypeLabel" runat="server"  OnDataBinding="LastModificationTypeLabel_OnDataBinding"  />
                                                    </td>
                                                </tr>
                                            </table>

                                        </asp:TableCell>
                                    </asp:TableRow>                      
                                
                                </asp:Table>
          
                        
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

