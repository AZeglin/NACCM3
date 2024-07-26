<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard" CodeBehind="NDCChange.aspx.cs" Inherits="VA.NAC.CM.DrugItems.NDCChange" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register src="CompactDrugItemHeader.ascx" tagname="CompactDrugItemHeader" tagprefix="uc2" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>NDC Change</title>
    
       <script type="text/javascript">
       <!--
            function CloseWindow( withRefresh, withRefreshPrices )
            {
                window.opener.document.forms[0].RefreshDrugItemScreenOnSubmit.value = withRefresh;               
                window.opener.document.forms[0].RefreshDrugItemPriceScreenOnSubmit.value = withRefreshPrices;

                window.opener.document.forms[0].RebindDrugItemScreenOnRefreshOnSubmit.value = 'true';
                window.opener.document.forms[0].RebindDrugItemPriceScreenOnRefreshOnSubmit.value = 'true';               

                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
  
        
</head>
<body >
    <form id="NDCChangeForm" runat="server" style="z-index: 5; position:fixed; top:0px; left:0px; width:544px;" >
   
       <asp:ScriptManager ID="NDCChangeScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="NDCChangeScriptManager_OnAsyncPostBackError" >
           
        </asp:ScriptManager>


        <table style="table-layout:fixed; width: 542px; text-align:left;" >
            <col style="width:540px;" />
 
            <tr style=" height:90px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 90px; width: 538px;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="NDCChangeUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
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
                        <tr  style="text-align:right;" >
                            <td style="padding-top:4px; padding-bottom:0px;" >
                            </td>
                            <td style="padding-top:4px; padding-bottom:0px;" >           
                            </td>
                            <td style="width:80px; padding-top:4px; padding-bottom:0px;" >
                                <asp:Button runat="server"  ID="UpdateNDCChangeButton" Text="Save" OnClick="UpdateNDCChangeButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="width:80px; padding-top:4px; padding-bottom:0px;" >
                                <asp:Button runat="server"  ID="CancelNDCChangeButton" Text="Close" OnClick="CancelNDCChangeButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:292px; border: solid 1px black; vertical-align:top;">
                <td>
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server" CssClass="DrugItemDetails" Width="540px" Height="290px" >
                        <div id="ItemDetailsBodyDiv" style="width:536px; height:288px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="NDCChangeUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="NDCChangeFormView" CssClass="DrugItemDetailsEditArea" 
                                runat="server" OnDataBound="NDCChangeFormView_OnDataBound" 
                                DefaultMode="Edit" OnPreRender="NDCChangeFormView_OnPreRender" 
                                Height="286px" Width="536px" >
                            <ItemTemplate>
                                <table style="height: 284px; table-layout:fixed; width: 480px; vertical-align:top; text-align:left;"  >
                                <col style="width:28px;" />
                                <col style="width:90px;" />
                                <col style="width:105px;" />
                                <col style="width:65px;" />
                                <col style="width:100px;" />
                                <col style="width:100px;" />
                                <col style="width:18px;" />
                                <tr style="height:30px;"> 
                                        <td>
                                        </td>
                                        <td colspan="3" style="border-bottom:solid 1px black; text-align:center;">
                                            <asp:Label ID="NDCChangeLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
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
                                            <asp:Label ID="ProductCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="OldNDCDiscontinuationDateLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Old NDC", "Discontinuation", "Date"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="NewNDCEffectiveDateLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"New NDC", "Effective", "Date"} ) %>' Width="94%" />       
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
                                            <asp:Label ID="DiscontinuationDateForOldNDCDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationDateForOldNDC", "{0:d}" )%>' Width="80%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="EffectiveDateForNewNDCDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "EffectiveDateForNewNDC", "{0:d}" )%>' Width="80%" />       
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
                            
                                <table style="height: 284px; table-layout:fixed; width: 480px; vertical-align:top; text-align:left;" >
                                <col style="width:28px;" />
                                <col style="width:90px;" />
                                <col style="width:105px;" />
                                <col style="width:65px;" />
                                <col style="width:100px;" />
                                <col style="width:100px;" />
                                <col style="width:18px;" />
                                <tr style="height:30px;"> 
                                        <td>
                                        </td>
                                        <td colspan="3" align="center" style="border-bottom:solid 1px black;">
                                            <asp:Label ID="NDCChangeLabel" runat="server"  Text="New NDC" CssClass="DrugItemHeaderText"  />
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
                                            <asp:Label ID="ProductCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Product", "Code"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="PackageCodeLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Package", "Code"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="OldNDCDiscontinuationDateLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Old NDC", "Discontinuation", "Date"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="NewNDCEffectiveDateLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"New NDC", "Effective", "Date"} ) %>' Width="94%" />       
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
                                            <asp:TextBox ID="DiscontinuationDateForOldNDCTextBox" runat="server" OnDataBinding="DiscontinuationDateForOldNDCTextBox_OnDataBinding" Width="80%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="EffectiveDateForNewNDCTextBox" runat="server" OnDataBinding="EffectiveDateForNewNDCTextBox_OnDataBinding" Width="80%" />       
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
                                            <asp:Label ID="CopySubItemsToNewNDCLabel" runat="server" CssClass="DrugItemRegularText"  Text='<%# MultilineText( new string[] {"Copy Sub-Items", "To", "New NDC"} ) %>' Width="94%" />       
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
                        </div>
                  </asp:Panel>
                </td>
            </tr>
        </table>
            <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />  
    </form>
</body>
</html>
