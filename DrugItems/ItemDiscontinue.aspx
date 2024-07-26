<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard" CodeBehind="ItemDiscontinue.aspx.cs" Inherits="VA.NAC.CM.DrugItems.ItemDiscontinue" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register src="CompactDrugItemHeader.ascx" tagname="CompactDrugItemHeader" tagprefix="uc2" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Discontinue Item</title>
    
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
    <form id="ItemDiscontinueForm" runat="server"  oninit="ItemDiscontinueForm_OnInit" style="z-index: 5; position:fixed; top:0px; left:0px; width:544px;" >
   
       <asp:ScriptManager ID="ItemDiscontinueScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ItemDiscontinueScriptManager_OnAsyncPostBackError" >
           
        </asp:ScriptManager>


        <table style="table-layout:fixed; width: 542px; text-align:left;" >
            <col style="width:540px;" />
 
            <tr style=" height:90px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 90px; width: 518px;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemDiscontinueUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
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
                     <table style="border-top:solid 1px black; height:28px; table-layout:fixed; text-align:center; width:544px; " >
                        <col style="width:20px;" />
                        <col style="width:58px;" />
                        <col style="width:46px;" />
                        
                        <tr style="text-align:right;" >
                            <td style="padding-top:4px; padding-bottom:0px;" >
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >
                                <asp:Button Width="94%" runat="server"  ID="DiscontinueItemButton" Text="Discontinue" OnClick="DiscontinueItemButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >
                                <asp:Button Width="94%" runat="server"  ID="CancelItemDiscontinueButton" Text="Cancel" OnClick="CancelItemDiscontinueButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:342px; border: solid 1px black; vertical-align:top;">
                <td>
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server" CssClass="DrugItemDetails" Width="540px" Height="390px" >
                        <div id="ItemDetailsBodyDiv" style="width:536px; height:388px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemDiscontinueUpdateUpdatePanelEventProxy" runat="server" />
                                
                            <asp:FormView ID="ItemDiscontinueFormView" CssClass="DrugItemDetailsEditArea" 
                                runat="server" OnDataBound="ItemDiscontinueFormView_OnDataBound" 
                                DefaultMode="Edit" OnPreRender="ItemDiscontinueFormView_OnPreRender" 
                                Height="386px" Width="536px" >
                            <ItemTemplate>
                                <table style="height: 384px; table-layout:fixed; width: 526px; vertical-align:top; text-align:left;"  >
                                <col style="width:28px;" />
                                <col style="width:90px;" />
                                <col style="width:105px;" />
                                <col style="width:65px;" />
                                <col style="width:100px;" />
                                <col style="width:90px;" />
                                <col style="width:18px;" />
  
                                <tr style="height:30px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server"  CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="ProductCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
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
                                            <asp:Label ID="ProductCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode" )%>' runat="server"  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageCodeDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  runat="server" Width="94%" />                                               
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
                                            <asp:Label ID="GenericNameLabel" runat="server" CssClass="DrugItemRegularText"  Text="Generic Name" Width="94%" />       
                                        </td>
                                        <td colspan="4">
                                            <asp:Label ID="GenericNameDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "GenericName" )%>' Width="80%" />       
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
                                            <asp:Label ID="TradeNameDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' Width="80%" />       
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
                                            <asp:Label ID="DispensingUnitDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' Width="80%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageDescriptionLabel" runat="server" CssClass="DrugItemRegularText"  Text="Package Size" Width="94%" />                                                       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageDescriptionDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' Width="80%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:35px;" >
                                        <td>
                                        </td>
                                         <td colspan="1">
                                            <asp:Label ID="DiscontinuationDateLabel" runat="server" CssClass="DrugItemRegularText"  Text="Discontinuation Date"  Width="80%" />       
                                        </td>
                                        <td colspan="1">
                                            <asp:Label ID="DiscontinuationDateDataLabel"  runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationDate", "{0:d}" )%>' Width="80%" />       
                                        </td>  
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>                                        
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:45px;" >
                                        <td>
                                        </td>
                                        <td colspan="1">
                                            <asp:Label ID="DiscontinueReasonLabel" runat="server" CssClass="DrugItemRegularText"  Text="Reason For Discontinuation"  Width="96%" />       
                                        </td>
                                        <td colspan="5">
                                            <asp:Label ID="ItemDiscontinuationReasonDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationReason" )%>' Width="98%" />       
                                        </td>
                                    </tr>
                                                            
                                </table>
                          
                            </ItemTemplate>
                            <EditItemTemplate>
                                    
                                <table style="height: 384px; table-layout:fixed; width: 526px; vertical-align:top; text-align:left;" >
                                <col style="width:28px;" />
                                <col style="width:90px;" />
                                <col style="width:105px;" />
                                <col style="width:65px;" />
                                <col style="width:100px;" />
                                <col style="width:90px;" />
                                <col style="width:18px;" />
 
                                <tr style="height:30px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="FdaAssignedLabelerCodeLabel" runat="server"  CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Fda Assigned", "Labeler Code"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="ProductCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
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
                                            <asp:TextBox ID="FdaAssignedLabelerCodeTextBox" ReadOnly="true" runat="server" MaxLength="5" Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' Width="80%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="ProductCodeTextBox"  ReadOnly="true" runat="server" MaxLength="4" Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode", "{0:d}" )%>' Width="60%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PackageCodeTextBox"  ReadOnly="true" runat="server" MaxLength="2" Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>'  Width="70%" />       
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
                                            <asp:Label ID="GenericNameLabel" runat="server" CssClass="DrugItemRegularText"  Text="Generic Name" Width="94%" />       
                                        </td>
                                        <td colspan="4">
                                            <asp:TextBox ID="GenericNameTextBox"  ReadOnly="true" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "GenericName" )%>' Width="94%" />       
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
                                            <asp:TextBox ID="TradeNameTextBox"  ReadOnly="true" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' Width="94%" />       
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
                                            <asp:TextBox ID="DispensingUnitTextBox"  ReadOnly="true" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' Width="80%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageDescriptionLabel" runat="server" CssClass="DrugItemRegularText"  Text="Package Size" Width="94%" />                                                       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="PackageDescriptionTextBox" ReadOnly="true"  runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' Width="80%" />       
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    <tr style="height:35px;" >
                                        <td>
                                        </td>
                                        <td colspan="1"> 
                                            <asp:Label ID="DiscontinuationDateLabel" runat="server"  CssClass="DrugItemRegularText" Text="Discontinuation Date"  Width="80%" />       
                                        </td>
                                        <td colspan="1">
                                            <asp:TextBox ID="DiscontinuationDateTextBox"  runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationDate", "{0:d}" )%>' Width="80%" />       
                                        </td>
                                       <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:45px;" >
                                        <td>
                                        </td>
                                        <td colspan="1">
                                            <asp:Label ID="DiscontinueReasonLabel" runat="server" CssClass="DrugItemRegularText"  Text="Reason For Discontinuation"  Width="96%" />       
                                        </td>
                                        <td colspan="5">
                                            <asp:DropDownList ID="DiscontinuationReasonDropDownList" runat="server" OnDataBound="DiscontinuationReasonDropDownList_OnDataBound" Width="98%" />       
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
