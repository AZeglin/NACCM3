<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" CodeBehind="ItemDetails.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ItemDetails" %>

<%@ Register src="ItemHeader.ascx" tagname="ItemHeader" tagprefix="uc1" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Item Details</title>
    <link href="ApplicationStartupDetails.css" rel="stylesheet" type="text/css" />

       <script type="text/javascript">
       <!--
            function CloseWindow( withRefresh, withRefreshPrices )
            {
                window.opener.document.forms[0].RefreshItemScreenOnSubmit.value = withRefresh;
                window.opener.document.forms[0].RebindItemScreenOnRefreshOnSubmit.value = 'false';
                window.opener.document.forms[0].RefreshItemPriceScreenOnSubmit.value = withRefreshPrices;
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
    <form id="ItemDetailsForm" runat="server" style="z-index: 5; position:fixed; top:0px; left:0px; width:878px; height:413px;" >
           
    <asp:ScriptManager ID="ItemDetailsScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ItemDetailsScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>
    

        <table style="width:874px; height: 408px; table-layout:fixed; border:solid 1px black;" class="ItemDetails"  >
            <colgroup >
                <col style="width: 872px;" />
            </colgroup>

            <tr  style="height:84px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 83px; width: 870px;" > 
                        <asp:UpdatePanel ID="SelectedItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ItemDetailsUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc1:ItemHeader ID="SelectedItemHeader" runat="server"/>
                            </ContentTemplate>
                        </asp:UpdatePanel> 
                    </div>
                </td>
            </tr>
 
            <tr style="height:32px; text-align:right;" >     
                <td style="width:98%; text-align:right;" >
                     <table  style="border-top:solid 1px black;  width:870px; height:28px; table-layout:fixed; text-align:right;" >
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
 
            <tr style="height:372px; vertical-align:top; border: solid 1px black">
                <td >
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server" Width="870px" Height="367px" >
                        <div id="ItemDetailsBodyDiv" style="width:868px; height:367px; top:0px;  left:0px; border:solid 1px black; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="ItemDetailsUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="ItemDetailsFormView" 
                                runat="server" OnDataBound="ItemDetailsFormView_OnDataBound" 
                                DefaultMode="Edit" OnPreRender="ItemDetailsFormView_OnPreRender" 
                                 RenderOuterTable="false" >
                            <ItemTemplate>
                                <table style="height: 364px; table-layout:fixed; width: 866px;" class="ItemDetailsEditArea"  >     
                                    <colgroup >
                                        <col style="width: 20px;" />
                                        <col style="width: 117px;" />
                                        <col style="width: 117px;" />
                                        <col style="width: 117px;" />
                                        <col style="width: 117px;" />   
                                        <col style="width: 117px;" />   
                                        <col style="width: 195px;" />   
                                        <col style="width: 20px;" />
                                    </colgroup >
                                    <tr >
                                        <td>
                                        </td>
                                        <td >
                                            <asp:Label ID="ManufacturersCatalogNumberLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Part Number"} ) %>' Width="94%" />
                                        </td>
                                        <td >
                                            <asp:Label ID="ManufacturersNameLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Name"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ManufacturersCommercialListPriceLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Commercial", "List Price"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="LetterOfCommitmentDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Letter", "Of Commitment", "Date"} ) %>' Width="94%" />       
                                        </td>
                                       <td>
                                            <asp:Label ID="TrackingMechanismLabel" runat="server" Text='<%# MultilineText( new string[] {"Tracking", "Mechanism"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                             <asp:Label ID="CountriesOfOriginLabel" runat="server" Text='<%# MultilineText( new string[] {"Countries", "Of Origin"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="ManufacturersCatalogNumberDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersCatalogNumber" )%>' runat="server" Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="ManufacturersNameDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersName" )%>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ManufacturersCommercialListPriceDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersCommercialListPrice" )%>' runat="server"  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="LetterOfCommitmentDateDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "LetterOfCommitmentDate", "{0:d}" )%>'  runat="server" Width="88%" />                                               
                                        </td>    
                                        <td>
                                            <asp:Label ID="TrackingMechanismDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingMechanism" )%>'  runat="server" Width="88%" />                                                  
                                        </td>
                                        <td style="vertical-align:top;" rowspan="3"  >                                                  
                                            <asp:panel runat="server" ID="CountryPanel" CssClass="itemCountryPanel" Height="100%" Width="100%" >
                                                 <asp:CheckBoxList ID="CountriesOfOriginCheckBoxList" runat="server" CssClass="ItemRegularText" AutoPostBack="True" Enabled="false" ToolTip="Countries of Origin" RepeatDirection="Vertical" OnDataBound="CountriesOfOriginCheckBoxList_OnDataBound"  OnPreRender="CountriesOfOriginCheckBoxList_OnPreRender"  >
                                                   
                                                 </asp:CheckBoxList>
                                             </asp:panel>
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="AcquisitionCostLabel" runat="server" Text='<%# MultilineText( new string[] {"Acquisition", "Cost"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialListPriceLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "List Price"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "Pricelist", "Date"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistFOBTermsLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "Pricelist", "FOB Terms"} ) %>'  Width="94%" />       
                                        </td>                                       
                                        <td>
                                            <asp:Label ID="TypeOfContractorLabel" runat="server" Text='<%# MultilineText( new string[] {"Type", "Of Contractor"} ) %>'  Width="94%" />       
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
                                            <asp:TextBox ID="AcquisitionCostDataLabel" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "AcquisitionCost" )%>' Width="94%" />       
                                        </td>  
                                        <td>
                                            <asp:Label ID="CommercialListPriceDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialListPrice" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistDateDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialPricelistDate", "{0:d}" )%>' Width="92%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistFOBTermsDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialPricelistFOBTerms" )%>' Width="94%" />       
                                        </td>                                        
                                        <td>
                                            <asp:Label ID="TypeOfContractorDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "TypeOfContractor" )%>' Width="92%" />       
                                        </td>                                        
                                        <td>
                                        </td>   
                                        <td>
                                        </td>                                
                                  </tr>

                                   

                                  <tr style="height:30px;" >
                                        <td>
                                        </td>
                                        <td colspan="7">
                                            <table class="ItemDetailsFooterArea" style="border-top:solid 1px black; height: 28px; table-layout:fixed; width: 840px; text-align:center;"  >
                                             <colgroup >
                                                <col style="width: 271px" />
                                                <col style="width: 262px" />
                                                <col style="width: 317px" />
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
                            
                                <table style="height: 364px; table-layout:fixed; width: 866px;" class="ItemDetailsEditArea"  >          
                                <colgroup >
                                    <col style="width: 20px;" />
                                    <col style="width: 117px;" />
                                    <col style="width: 117px;" />
                                    <col style="width: 117px;" />
                                    <col style="width: 117px;" />   
                                    <col style="width: 117px;" />   
                                    <col style="width: 195px;" />   
                                    <col style="width: 20px;" />
                                </colgroup >
                                <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td >
                                            <asp:Label ID="ManufacturersCatalogNumberLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Part Number"} ) %>' Width="94%" />
                                        </td>
                                        <td >
                                            <asp:Label ID="ManufacturersNameLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Name"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ManufacturersCommercialListPriceLabel" runat="server" Text='<%# MultilineText( new string[] {"Manufacturers", "Commercial", "List Price"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="LetterOfCommitmentDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Letter", "Of Commitment", "Date"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="TrackingMechanismLabel" runat="server" Text='<%# MultilineText( new string[] {"Tracking", "Mechanism"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                             <asp:Label ID="CountriesOfOriginLabel" runat="server" Text='<%# MultilineText( new string[] {"Countries", "Of Origin"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="ManufacturersCatalogNumberTextBox" runat="server" MaxLength="100" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersCatalogNumber" )%>' Width="94%" />
                                        </td>                                       
                                        <td>
                                            <asp:TextBox ID="ManufacturersNameTextBox" runat="server" MaxLength="100" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersName" )%>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="ManufacturersCommercialListPriceTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "ManufacturersCommercialListPrice" )%>'  Width="88%" />       
                                        </td>
                                        <td>
                                            <asp:TextBox ID="LetterOfCommitmentDateTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "LetterOfCommitmentDate", "{0:d}" )%>' Width="94%" />       
                                        </td>   
                                         <td>
                                            <asp:TextBox ID="TrackingMechanismTextBox" runat="server" MaxLength="100" Text='<%# DataBinder.Eval( Container.DataItem, "TrackingMechanism" )%>'  Width="88%" />       
                                        </td>
                                        <td style="vertical-align:top;"  rowspan="3"  >                                                  
                                            <asp:panel runat="server" ID="CountryPanel" CssClass="itemCountryPanel" Height="100%" Width="100%" >
                                                 <asp:CheckBoxList ID="CountriesOfOriginCheckBoxList" runat="server" CssClass="ItemRegularText" AutoPostBack="True" ToolTip="Countries of Origin" RepeatDirection="Vertical" OnDataBound="CountriesOfOriginCheckBoxList_OnDataBound"  OnPreRender="CountriesOfOriginCheckBoxList_OnPreRender" OnSelectedIndexChanged="CountriesOfOriginCheckBoxList_OnSelectedIndexChanged"  >
                                                   
                                                 </asp:CheckBoxList>
                                             </asp:panel>
                                        </td>
                                        <td>
                                        </td>
                                   </tr>
                                    <tr style="height:60px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="AcquisitionCostLabel" runat="server" Text='<%# MultilineText( new string[] {"Acquisition", "Cost"} ) %>' Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialListPriceLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "List Price"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "Pricelist", "Date"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="CommercialPricelistFOBTermsLabel" runat="server" Text='<%# MultilineText( new string[] {"Commercial", "Pricelist", "FOB Terms"} ) %>'  Width="94%" />       
                                        </td>                                          
                                        <td>
                                            <asp:Label ID="TypeOfContractorLabel" runat="server" Text='<%# MultilineText( new string[] {"Type", "Of Contractor"} ) %>'  Width="94%" />       
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
                                            <asp:TextBox ID="AcquisitionCostTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "AcquisitionCost" )%>' Width="94%" />       
                                        </td>  
                                        <td>
                                            <asp:TextBox ID="CommercialListPriceTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialListPrice" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="CommercialPricelistDateTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialPricelistDate", "{0:d}" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <asp:TextBox ID="CommercialPricelistFOBTermsTextBox" runat="server" MaxLength="11" Text='<%# DataBinder.Eval( Container.DataItem, "CommercialPricelistFOBTerms" )%>' Width="92%" />
                                        </td>                                        
                                        <td>
                                            <asp:TextBox ID="TypeOfContractorTextBox" runat="server" MaxLength="100" Text='<%# DataBinder.Eval( Container.DataItem, "TypeOfContractor" )%>' Width="92%" />
                                        </td>
                                        <td>
                                        </td>  
                                        <td>
                                        </td>
                                    </tr>
 

                                    <tr style="height:30px;" >
                                        <td colspan="9">
                                            <table class="ItemDetailsFooterArea" style="border-top:solid 1px black;  height: 28px; table-layout:fixed; width: 848px; text-align:center;"  >
                                             <colgroup >
                                                <col style="width: 271px" />
                                                <col style="width: 262px" />
                                                <col style="width: 317px" />
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
