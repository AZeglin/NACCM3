<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard" CodeBehind="DrugItemPriceDetails.aspx.cs" Inherits="VA.NAC.CM.DrugItems.DrugItemPriceDetails" %>

<%@ Register src="DrugItemHeader.ascx" tagname="DrugItemHeader" tagprefix="uc1" %>

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
                window.opener.document.forms[0].RefreshDrugItemPriceScreenOnSubmit.value = withRefresh;
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

    <form id="DrugItemPriceDetailsForm" runat="server" 
    style="z-index: 5; position:fixed; top:0px; left:0px; width:898px; height:439px;" >
   
       <asp:ScriptManager ID="DrugItemPriceDetailsScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="DrugItemPriceDetailsScriptManager_OnAsyncPostBackError" >
          
        </asp:ScriptManager>
  
    
        <table  style="width:894px; height: 368px; table-layout:fixed; border:solid 1px black;" class="DrugItemPriceDetails"  >
            <colgroup >
                <col style="width: 892px;" />
            </colgroup>
 
            <tr style="height:84px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 83px; width: 890px;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemPriceDetailsUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc1:DrugItemHeader ID="SelectedDrugItemHeader" runat="server"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
 
            <tr style="height:32px; text-align:right;" >   
                <td  style="width:98%; text-align:right;" >
                     <table style="border-top:solid 1px black;  width:890px; height:28px; table-layout:fixed; text-align:right;" >
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
                <asp:Panel ID="ItemPriceDetailsBodyPanel" runat="server" Width="880px" Height="267px" >
                        <div id="ItemPriceDetailsBodyDiv" style="width:880px; height:264px; top:0px;  left:0px; border:solid 1px black; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemPriceDetailsUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="DrugItemPriceDetailsFormView" 
                                runat="server" OnDataBound="DrugItemPriceDetailsFormView_OnDataBound" 
                                DefaultMode="Edit"  OnPreRender="DrugItemPriceDetailsFormView_OnPreRender"
                                 RenderOuterTable="false" >
                            <ItemTemplate>
                                <table style="height: 255px; table-layout:fixed; width: 826px;" >
                                    <colgroup >
                                        <col style="width: 40px;" />
                                        <col style="width: 70px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 140px;" />
                                        <col style="width: 70px;" />
                                        <col style="width: 20px;" />
                                        <col style="width: 412px;" />
                                    </colgroup >
                                    <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerRatioLabel" runat="server"   Text='<%# MultilineText( new string[] {"FSS", "Tracking Customer", "Ratio"} ) %>' Width="94%"  />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerPriceLabel" runat="server"    Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Price"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerNameLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Name"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExcludeFromExportLabel" runat="server"  Text='<%# MultilineText( new string[] {"Exclude", "From", "Export"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td rowspan="4"  >
                                             <table style="height: 140px; table-layout:fixed; width: 410px; text-align:center;"  >
                                                <colgroup >
                                                    <col style="width: 148px;" />
                                                    <col style="width: 148px;" />
                                                    <col style="width: 148px;" />
                                                </colgroup >
                                                  <tr style="vertical-align:top;">
                                                    <td colspan="3" >
                                                        <asp:Label ID="TieredPriceTitleLabel" runat="server" Text="Tiered Pricing" CssClass="DrugItemHeaderText" />
                                                    </td>
                                                  </tr>
                                                  <tr style="vertical-align:top;">
                                                    <td>
                                                        <asp:Button ID="AddTieredPriceButton" runat="server" Text="Add Tiered Price" Width="99%" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                
                                                </tr>
                                             </table>                                    
                                        </td>
                                    </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerRatioDataLabel" runat="server" Text='<%# Eval("TrackingCustomerRatio") %>' Width="88%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerPriceDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerPrice" )%>' runat="server" Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerNameDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerName" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:CheckBox ID="ExcludeFromExportCheckBox" runat="server" OnDataBinding="ExcludeFromExportCheckBox_OnDataBinding"  Enabled="false"  Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td rowspan="4" >
                                             <table style="height: 140px; table-layout:fixed; width: 410px; text-align:center;"  >
                                                <colgroup >                                                    
                                                    <col style="width: 136px" />
                                                    <col style="width: 136px" />
                                                    <col style="width: 136px" />
                                                </colgroup >

                                                  <tr style="vertical-align:top;">
                                                        <td colspan="3" >
                                                            <asp:Label ID="TieredPricingGridLabel" runat="server" Text="Tiered Pricing" CssClass="DrugItemHeaderText"/>
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
                                                        
                                                          <div id="TieredPriceGridViewDiv"  runat="server" style="width:408px; height:138px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
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
                                                                                    DataKeyNames="DrugItemTieredPriceId"  
                                                                                    AutoGenerateColumns="False" 
                                                                                    Width="406px" 
                                                                                    CssClass="DrugItemGrids" 
                                                                                    Visible="True" 
                                                                                    OnRowDataBound="TieredPriceGridView_RowDataBound"
                                                                                    AutoGenerateEditButton="false"
                                                                                    EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                                    onprerender="TieredPriceGridView_PreRender" 
                                                                                    onrowediting="TieredPriceGridView_RowEditing" 
                                                                                    onrowcancelingedit="TieredPriceGridView_RowCancelingEdit"
                                                                                    AllowInserting="True"
                                                                                    InsertCommandColumnIndex="2"
                                                                                    EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                                    EmptyDataText="There are no tiered prices for the selected price."
                                                                                   >
                                                                                <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                                <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                                <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                             
                                                                                <Columns>   
                                                                                                                                       
                                                                                    
                                                                                    <asp:TemplateField >  
                                                                                        <ItemTemplate>
                                                                                               <asp:Button  CausesValidation="false" Visible="true" Enabled="false" runat="server" ID="EditButton" Text="Edit" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button  CausesValidation="false" Visible="false" Enabled="false"  runat="server" ID="SaveButton" Text="Save" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="SaveItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button CausesValidation="false"  Visible="false" Enabled="false"  runat="server" ID="CancelButton" Text="Cancel" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   ControlStyle-Width="60px"   >
                                                                                              </asp:Button>   
                                                                          
                                                                                        </ItemTemplate>
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

                                                                                    <asp:TemplateField HeaderText="Minimum"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="minimumLabel" runat="server"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Minimum" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <ep:TextBox ID="minimumTextBox" runat="server" TextMode="MultiLine" MaxLength="200" CssClass="DrugItemMultilineInEditMode"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Minimum" )%>' ></ep:TextBox>
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
                                                                                            <asp:Button runat="server"  ID="RemovePriceButton" Enabled="false"  Text="Remove Price"  OnCommand="TieredPriceGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'  CssClass="ButtonWrapText" ControlStyle-Width="82px" >            
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
                                        </td>
                                    </tr>
                                    <tr style="height:30px;" >
                                        <td>
                                        </td>
                                        <td colspan="7">
                                            <table style="border-top:solid 1px black; height: 28px; table-layout:fixed; width: 826px;" align="center"  >
                                                <colgroup>
                                                     <col style="width:275px;" />
                                                     <col style="width:265px;" />
                                                     <col style="width:320px;" />
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

                                        </td>
                                        <td>
                                        </td>
                                    </tr>             
                                
                                </table>
                  
                            </ItemTemplate>
                            <EditItemTemplate>
                            
                                <table style="height: 262px; table-layout:fixed; width: 878px;" class="DrugItemPriceDetailsEditArea" >
                                    <colgroup>
                                        <col style="width: 40px;" />
                                        <col style="width: 70px;" />
                                        <col style="width: 90px;" />
                                        <col style="width: 140px;" />
                                        <col style="width: 70px;" />
                                        <col style="width: 20px;" />
                                        <col style="width: 412px;" />
                                    </colgroup>
                                  
                                    <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerRatioLabel" runat="server" Text='<%# MultilineText( new string[] {"FSS", "Tracking Customer", "Ratio"} ) %>' Width="94%"  />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerPriceLabel" runat="server"  Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Price"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingCustomerNameLabel" runat="server" Text='<%# MultilineText( new string[] {"Tracking", "Customer", "Name"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExcludeFromExportLabel" runat="server" Text='<%# MultilineText( new string[] {"Exclude", "From", "Export"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td rowspan="4" >
                                             <table style="height: 140px; table-layout:fixed; width: 410px; text-align:center;"  >
                                                <colgroup>
                                                    <col style="width: 136px;" />
                                                    <col style="width: 136px;" />
                                                    <col style="width: 136px;" />
                                                </colgroup>
                                                  <tr style="vertical-align:top;">
                                                        <td colspan="3" >
                                                            <asp:Label ID="TieredPriceGridLabel" runat="server" Text="Tiered Pricing" CssClass="DrugItemHeaderText"/>
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
                                                        
                                                          <div id="TieredPriceGridViewDiv"  runat="server" style="width:408px; height:138px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
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
                                                                                    DataKeyNames="DrugItemTieredPriceId"  
                                                                                    AutoGenerateColumns="False" 
                                                                                    Width="406px" 
                                                                                    CssClass="DrugItemGrids" 
                                                                                    Visible="True" 
                                                                                    OnRowDataBound="TieredPriceGridView_RowDataBound"
                                                                                    AutoGenerateEditButton="false"
                                                                                    EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                                                                    onprerender="TieredPriceGridView_PreRender" 
                                                                                    onrowediting="TieredPriceGridView_RowEditing" 
                                                                                    onrowcancelingedit="TieredPriceGridView_RowCancelingEdit"
                                                                                    AllowInserting="True"
                                                                                    
                                                                                    EmptyDataRowStyle-CssClass="DrugItemDetailsEditArea" 
                                                                                    EmptyDataText="There are no tiered prices for the selected price."
                                                                                   >
                                                                                <HeaderStyle CssClass="DrugItemGridHeaders" />
                                                                                <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                                <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                                                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                             
                                                                                <Columns>   
                                                                                                                                       
                                                                                    
                                                                                    <asp:TemplateField >  
                                                                                        <ItemTemplate>
                                                                                               <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="SaveItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   

                                                                                               <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="TieredPriceGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'   Width="60px"   >
                                                                                              </asp:Button>   
                                                                          
                                                                                        </ItemTemplate>
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

                                                                                    <asp:TemplateField HeaderText="Minimum"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="minimumLabel" runat="server"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Minimum" )%>' ></asp:Label>
                                                                                        </ItemTemplate>
                                                                                        <EditItemTemplate>
                                                                                            <ep:TextBox ID="minimumTextBox" runat="server" TextMode="MultiLine" CssClass="DrugItemMultilineInEditMode"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Minimum" )%>' ></ep:TextBox>
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
                                                                                            <asp:Button runat="server"  ID="RemovePriceButton" Text="Remove Price" OnCommand="TieredPriceGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemTieredPriceId") %>'  CssClass="ButtonWrapText"  ControlStyle-Width="82px" >            
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
                                        </td>
                                    </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="TrackingCustomerRatioTextBox" runat="server"  MaxLength="10" Text='<%# Eval("TrackingCustomerRatio") %>' Width="88%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="TrackingCustomerPriceTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerPrice" )%>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="TrackingCustomerNameTextBox" runat="server" MaxLength="120" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingCustomerName" )%>' Width="94%" />
                                         </td>
                                        <td>
                                            <asp:CheckBox ID="ExcludeFromExportCheckBox" runat="server" OnDataBinding="ExcludeFromExportCheckBox_OnDataBinding"   Width="94%" />       
                                        </td>
                                        <td>
                                        
                                        </td>
                                        <td>
                                        
                                        </td>

                                    </tr>
                                    <tr style="height:20px;" >
                                        <td></td>
                                        <td>
                                        </td>
                                        <td colspan="4">                                   
                                        </td>
                                    </tr>
                                    <tr style="height:20px;" >
                                        <td></td>
                                        <td>
                                        </td>
                                        <td colspan="4"> 
                                         </td>
                                   </tr>
                                    <tr style="height:30px;" >
                                        <td colspan="7">
                                            <table class="DrugItemPriceDetailsFooterArea" style="border-top:solid 1px black;  height: 28px; table-layout:fixed; width: 860px;" >
                                                <colgroup>
                                                    <col style="width:265px;" />
                                                    <col style="width:255px;" />
                                                    <col style="width:330px;" />
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

