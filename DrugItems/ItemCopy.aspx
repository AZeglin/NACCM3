<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard" CodeBehind="ItemCopy.aspx.cs" Inherits="VA.NAC.CM.DrugItems.ItemCopy" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register src="CompactDrugItemHeader.ascx" tagname="CompactDrugItemHeader" tagprefix="uc2" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Item Copy</title>
     
       <script type="text/javascript">
       <!--
           function CloseWindow(withRefresh, withRefreshPrices, withRefreshItemHeaderCount)
            {
                window.opener.document.forms[0].RefreshDrugItemScreenOnSubmit.value = withRefresh;               
                window.opener.document.forms[0].RefreshDrugItemPriceScreenOnSubmit.value = withRefreshPrices;
                
                window.opener.document.forms[0].RebindDrugItemScreenOnRefreshOnSubmit.value = 'true';
                window.opener.document.forms[0].RebindDrugItemPriceScreenOnRefreshOnSubmit.value = 'true';
                window.opener.document.forms[0].RefreshItemHeaderCountOnSubmit.value = withRefreshItemHeaderCount;

                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
  
        
</head>
<body >
    <form id="ItemCopyForm" runat="server"  oninit="ItemCopyForm_OnInit" style="z-index: 5; position:fixed; top:0px; left:0px; width:548px;" >
   
       <asp:ScriptManager ID="ItemCopyScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ItemCopyScriptManager_OnAsyncPostBackError" >
          
        </asp:ScriptManager>


        <table style="table-layout:fixed; width: 542px; text-align:left;" >
            <col style="width:546px;" />
 
            <tr style=" height:90px; width:534px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 90px; width: 534px;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemCopyUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc2:CompactDrugItemHeader ID="SelectedDrugItemHeader" runat="server"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
 
            <tr >
                <td style="width:98%; text-align:left; padding-top:4px;" >
                     <table style="border-top:solid 1px black; height:28px; table-layout:fixed; text-align:center; width:544px;" >
                        <col style="width:20px;" />
                        <col style="width:162px;" />
                        <col style="width:162px;" />
                        <col style="width:80px;" />
                        
                        <tr style="text-align:right;" >
                            <td style="padding-top:4px; padding-bottom:0px;" >
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >
                                <asp:Button Width="94%" EnableViewState="true"  runat="server"  ID="UpdateAndContinueItemCopyButton" Text="Save And Continue" OnClick="UpdateAndContinueItemCopyButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >
                                <asp:Button Width="94%" runat="server"  ID="UpdateItemCopyButton" Text="Save And Close" OnClick="UpdateItemCopyButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >
                                <asp:Button Width="94%" runat="server"  ID="CancelItemCopyButton" Text="Close" OnClick="CancelItemCopyButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:342px; border: solid 1px black; vertical-align:top;">
                <td style="vertical-align:top; padding-top:0px;">
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server" CssClass="DrugItemDetails" Width="540px" Height="390px" >
                        <div id="ItemDetailsBodyDiv" style="width:536px; height:388px; top:0px;  left:0px; border:solid 1px black; background-color:white; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemCopyUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:MultiView ID="ItemCopyMultiView" runat="server" >
                                <asp:View ID="ItemCopyToSameContractView" runat="server" >
                                
                                    <asp:FormView ID="ItemCopyToSameContractFormView" CssClass="DrugItemDetailsEditArea" 
                                        runat="server" OnDataBound="ItemCopyToSameOrDifferentContractFormView_OnDataBound" 
                                        DefaultMode="Edit" OnPreRender="ItemCopyToSameContractFormView_OnPreRender" 
                                        Height="387px" Width="536px" >
                                    <ItemTemplate>
                                        <table style="height: 385px; table-layout:fixed; width: 536px; vertical-align:top; text-align:left;"  >
                                        <col style="width:28px;" />
                                        <col style="width:90px;" />
                                        <col style="width:105px;" />
                                        <col style="width:65px;" />
                                        <col style="width:100px;" />
                                        <col style="width:90px;" />
                                        <col style="width:18px;" />
                                        <tr style="height:20px;"> 
                                                <td>
                                                </td>
                                                <td colspan="3" style="border-bottom:solid 1px black; text-align:center;">
                                                    <asp:Label ID="ItemCopyLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
                                                </td>
                                                <td colspan="3">
                                                </td>
                                        </tr>
                                        <tr style="height:30px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                                </td>
                                                <td>
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
                                                   <asp:Label ID="FdaAssignedLabelerCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' runat="server" Width="92%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode" )%>' runat="server"  Width="92%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  runat="server" Width="88%" />                                               
                                                </td>
                                                <td>
                                                </td>
                                                 <td>
                                                </td>
                                                 <td>
                                                </td>
                                            </tr>
                                            <tr>
                                                 <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="GenericNameLabel" runat="server"  CssClass="DrugItemRegularText" Text="Generic Name" Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                    <asp:Label ID="GenericNameDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "GenericName" )%>' Width="94%" />       
                                                </td>
                                                <td>
                                                </td>

                                            </tr>
                                            <tr>
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="TradeNameLabel" runat="server"  CssClass="DrugItemRegularText" Text="Trade Name" Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                    <asp:Label ID="TradeNameDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' Width="94%" />       
                                                </td>
                                                <td>
                                                </td>
                                            </tr>           
                                            <tr style="height:50px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfSaleLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit of", "Sale"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitOfSaleLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Quantity in", "Unit of Sale"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitPackageLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit", "Package"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitPackageLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Quantity in", "Unit Package"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfMeasureLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit of", "Measure"} ) %>'  Width="80%" />       
                                                </td>
                                                 <td>
                                                </td>
                                          </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfSaleDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitOfSale" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitOfSaleDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitOfSale" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitPackageDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitPackage" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitPackageDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitPackage" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfMeasureDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "UnitOfMeasure" )%>' Width="80%" />       
                                                </td>
                                                 <td>
                                                </td>
                                          </tr>
                                             <tr style="height:40px;" >
                                                <td>
                                                </td>
                                               <td>
                                                    <asp:Label ID="CopyPricingToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Pricing", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="CopySubItemsToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Sub-Items", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>                                        
                                                <td colspan="4">
                                                </td>
                                            </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopyPricingCheckBox" runat="server" OnDataBinding="CopyPricingCheckBox_OnDataBinding" Enabled="false"  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopySubItemsCheckBox" runat="server" OnDataBinding="CopySubItemsCheckBox_OnDataBinding"  Enabled="false"  Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                </td>
                                            </tr>
                                

                                        
                                        </table>
                          
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                    
                                        <table style="height: 384px; table-layout:fixed; width: 526px; vertical-align:top; text-align:left" >
                                        <col style="width:28px;" />
                                        <col style="width:90px;" />
                                        <col style="width:105px;" />
                                        <col style="width:65px;" />
                                        <col style="width:100px;" />
                                        <col style="width:90px;" />
                                        <col style="width:18px;" />
                                        <tr style="height:20px;"> 
                                                <td>
                                                </td>
                                                <td colspan="3" style="border-bottom:solid 1px black; text-align:center;">
                                                    <asp:Label ID="ItemCopyLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
                                                </td>
                                                <td colspan="3">
                                                </td>
                                        </tr>
                                        <tr style="height:30px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server"   CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                                </td>
                                                <td>
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
                                                    <asp:TextBox ID="FdaAssignedLabelerCodeTextBox" runat="server" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' Width="80%" />
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="ProductCodeTextBox" runat="server" MaxLength="4" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode", "{0:d}" )%>' Width="60%" />       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="PackageCodeTextBox" runat="server" MaxLength="2" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  Width="70%" />       
                                                </td>
                                                <td>
                                                </td>
                                                <td>
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="GenericNameLabel" runat="server"  CssClass="DrugItemRegularText" Text="Generic Name" Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                    <asp:TextBox ID="GenericNameTextBox" runat="server" MaxLength="64"  Text='<%# DataBinder.Eval( Container.DataItem, "GenericName" )%>' Width="94%" />       
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="TradeNameLabel" runat="server"  CssClass="DrugItemRegularText" Text="Trade Name" Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                    <asp:TextBox ID="TradeNameTextBox" runat="server" MaxLength="45"  Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' Width="94%" />       
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="DispensingUnitLabel" runat="server"  CssClass="DrugItemRegularText" Text="Dispensing Unit" Width="94%" />                                                       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="DispensingUnitTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageDescriptionLabel" runat="server"  CssClass="DrugItemRegularText" Text="Package Size" Width="94%" />                                                       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="PackageDescriptionTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                             <tr style="height:50px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfSaleLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit of", "Sale"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitOfSaleLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Quantity in", "Unit of Sale"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitPackageLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit", "Package"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="QuantityInUnitPackageLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Quantity in", "Unit Package"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="UnitOfMeasureLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Unit of", "Measure"} ) %>'  Width="80%" />       
                                                </td>
                                                <td>
                                                </td>
                                          </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="UnitOfSaleDropDownList" runat="server" OnDataBound="UnitOfSaleDropDownList_OnDataBound" Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="QuantityInUnitOfSaleTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitOfSale" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="UnitPackageDropDownList" runat="server" OnDataBound="UnitPackageDropDownList_OnDataBound" Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="QuantityInUnitPackageTextBox" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "QuantityInUnitPackage" )%>' Width="80%" />       
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="UnitOfMeasureDropDownList" runat="server" OnDataBound="UnitOfMeasureDropDownList_OnDataBound" Width="80%" />       
                                                </td>
                                                <td>
                                                </td>
                                          </tr>
                                            <tr style="height:40px;" >
                                                <td>
                                                </td>
                                               <td>
                                                    <asp:Label ID="CopyPricingToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Pricing", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="CopySubItemsToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Sub-Items", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>                                        
                                                <td colspan="4">
                                                </td>
                                            </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopyPricingCheckBox" runat="server" OnDataBinding="CopyPricingCheckBox_OnDataBinding"   Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopySubItemsCheckBox" runat="server" OnDataBinding="CopySubItemsCheckBox_OnDataBinding"   Width="94%" />       
                                                </td>
                                                <td colspan="4">
                                                </td>
                                            </tr>
                                        
                                        </table>
                                          
                                    </EditItemTemplate>
                                    
                                    </asp:FormView>
                                    
                               </asp:View>
                                <asp:View ID="ItemCopyToDifferentContractView" runat="server" >
                                   <asp:FormView ID="ItemCopyToDifferentContractFormView" CssClass="DrugItemDetailsEditArea" 
                                        runat="server" OnDataBound="ItemCopyToSameOrDifferentContractFormView_OnDataBound" 
                                        DefaultMode="Edit" OnPreRender="ItemCopyToDifferentContractFormView_OnPreRender" 
                                        Height="386px" Width="536px" >
                                    <ItemTemplate>
                                        <table style="height: 384px; table-layout:fixed; width: 536px; vertical-align:top; text-align:left;" >
                                        <col style="width:28px" />
                                        <col style="width:90px" />
                                        <col style="width:105px" />
                                        <col style="width:65px" />
                                        <col style="width:100px" />
                                        <col style="width:18px" />
                                        <tr style="height:30px;"> 
                                                <td>
                                                </td>
                                                <td colspan="3" style="border-bottom:solid 1px black;  text-align:center;">
                                                    <asp:Label ID="ItemCopyLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
                                                </td>
                                                <td colspan="2">
                                                </td>
                                        </tr>
                                     <tr style="height:50px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server"   CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="DestinationContractNumberLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Destination", "Contract", "Number"} ) %>' Width="94%" />       
                                                </td>
                                                 <td>
                                                </td>
                                            </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                   <asp:Label ID="FdaAssignedLabelerCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' runat="server" Width="92%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode" )%>' runat="server"  Width="92%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  runat="server" Width="88%" />                                               
                                                </td>
                                                <td>
                                                    <asp:Label ID="DestinationContractNumberDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DestinationContractNumber" )%>' Width="94%" />       
                                                </td>
                                                 <td>
                                                </td>
                                            </tr>
                                             <tr style="height:50px;" >
                                                <td>
                                                </td>
                                               <td>
                                                    <asp:Label ID="CopyPricingToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Pricing", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="CopySubItemsToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Sub-Items", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>                                        
                                                <td colspan="3">
                                                </td>
                                            </tr>
                                            <tr style="height:35px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopyPricingCheckBox" runat="server" OnDataBinding="CopyPricingCheckBox_OnDataBinding" Enabled="false"  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopySubItemsCheckBox" runat="server" OnDataBinding="CopySubItemsCheckBox_OnDataBinding"  Enabled="false"  Width="94%" />       
                                                </td>
                                                <td colspan="3">
                                                </td>
                                            </tr>
                                

                                        
                                        </table>
                          
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                    
                                        <table style="height: 384px; table-layout:fixed; width: 526px; vertical-align:top; text-align:left;" >
                                        <col style="width:28px" />
                                        <col style="width:90px" />
                                        <col style="width:105px" />
                                        <col style="width:65px" />
                                        <col style="width:100px" />
                                        <col style="width:18px" />
                                        <tr style="height:30px;"> 
                                                <td>
                                                </td>
                                                <td colspan="3" style="border-bottom:solid 1px black; text-align:center;" >
                                                    <asp:Label ID="ItemCopyLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
                                                </td>
                                                <td colspan="2">
                                                </td>
                                        </tr>
                                        <tr style="height:50px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server"  CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                                </td>
                                                <td>
                                                    <asp:Label ID="ProductCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="PackageCodeLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="DestinationContractNumberLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Destination", "Contract", "Number"} ) %>' Width="94%" />       
                                                </td>
                                                 <td>
                                                </td>
                                            </tr>
                                            <tr style="height:25px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="FdaAssignedLabelerCodeTextBox" runat="server" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' Width="80%" />
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="ProductCodeTextBox" runat="server" MaxLength="4" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode", "{0:d}" )%>' Width="60%" />       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="PackageCodeTextBox" runat="server" MaxLength="2" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  Width="70%" />       
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="DestinationContractNumberTextBox" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "DestinationContractNumber" )%>' Width="94%" />       
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                            <tr style="height:50px;" >
                                                <td>
                                                </td>
                                               <td>
                                                    <asp:Label ID="CopyPricingToNewNDCLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Copy Pricing", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:Label ID="CopySubItemsToNewNDCLabel" runat="server"  CssClass="DrugItemRegularText" Text='<%# MultilineText( new string[] {"Copy Sub-Items", "To", "New NDC"} ) %>' Width="94%" />       
                                                </td>                                        
                                                <td colspan="3">
                                                </td>
                                            </tr>
                                            <tr style="height:35px;" >
                                                <td>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopyPricingCheckBox" runat="server" OnDataBinding="CopyPricingCheckBox_OnDataBinding"   Width="94%" />       
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="CopySubItemsCheckBox" runat="server" OnDataBinding="CopySubItemsCheckBox_OnDataBinding"   Width="94%" />       
                                                </td>
                                                <td colspan="3">
                                                </td>
                                            </tr>
                                        
                                        </table>
                                          
                                    </EditItemTemplate>
                                    
                                    </asp:FormView>                                
                                </asp:View>
                            
                            </asp:MultiView>

                        </div>
                  </asp:Panel>
                </td>
            </tr>
        </table>
            <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />  
    </form>
</body>
</html>
