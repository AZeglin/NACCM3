<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractGeneral.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractGeneral" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>


<asp:Content ID="ContractGeneralContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

 <script type="text/javascript" >

     function contractNumberMouseChange(contractButton, overOut, rowIndex, rowColor) {
         if (overOut == 'over') {
             contractButton.style.color = 'purple';
             contractButton.style.cursor = 'hand';
         }
         else {
             contractButton.style.color = 'black';
             contractButton.style.cursor = 'pointer';
         }
     }


     function setAssociatedBPAContractsScrollForRestore(divToScroll) {

         if (divToScroll) {
             $get("AssociatedBPAContractsScrollPos").value = divToScroll.scrollTop;
         }
     }


     function pageLoad(sender, args) {        
         RestoreAssociatedBPAContractsGridSelectionOnAsyncPostback();         
     }

     function RestoreAssociatedBPAContractsGridSelectionOnAsyncPostback() {        
         if ($get("AssociatedBPAContractsScrollPos")) {
             var AssociatedBPAContractsScrollPos = $get("AssociatedBPAContractsScrollPos").value;
             var highlightedAssociatedBPAContractsRow = $get("highlightedAssociatedBPAContractsRow").value;
             
             RestoreAssociatedBPAContractsGridSelection(AssociatedBPAContractsScrollPos, highlightedAssociatedBPAContractsRow);             
         }         
     }

     /* called from form load */
     function RestoreAssociatedBPAContractsGridSelection(AssociatedBPAContractsScrollPos, highlightedAssociatedBPAContractsRow) {
         $get("AssociatedBPAContractsScrollPos").value = AssociatedBPAContractsScrollPos;
         if (AssociatedBPAContractsScrollPos) {
             if (AssociatedBPAContractsScrollPos >= 0) {

                 var theAssociatedBPAContractsDiv = document.getElementById('<%=AssociatedBPAContractsGridViewDiv.ClientID %>');
                 if (theAssociatedBPAContractsDiv) {
                     theAssociatedBPAContractsDiv.scrollTop = AssociatedBPAContractsScrollPos;
                 }
             }
         }   
         if (highlightedAssociatedBPAContractsRow) {
             if (highlightedAssociatedBPAContractsRow >= 0) {
                 $get("highlightedAssociatedBPAContractsRow").value = highlightedAssociatedBPAContractsRow;
                 highlightAssociatedBPAContractsRow();
             }
         }        
     }

     function setAssociatedBPAContractsHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) { 
         $get("highlightedAssociatedBPAContractsRow").value = rowIndex;
         $get("highlightedAssociatedBPAContractsRowOriginalColor").value = originalColor;
         highlightAssociatedBPAContractsRow();
     }

     function highlightAssociatedBPAContractsRow() {

         var selectedRowIndex = $get("highlightedAssociatedBPAContractsRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var AssociatedBPAContractsGridView = document.getElementById("<%=AssociatedBPAContractsGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (AssociatedBPAContractsGridView) {
             currentSelectedRow = AssociatedBPAContractsGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightAssociatedBPAContractsRow() {

         var selectedRowIndex = $get("highlightedAssociatedBPAContractsRow").value;
         var highlightedAssociatedBPAContractsRowOriginalColor = $get("highlightedAssociatedBPAContractsRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedAssociatedBPAContractsRow").value = -1;
         var AssociatedBPAContractsGridView = document.getElementById("<%=AssociatedBPAContractsGridView.ClientID%>");
         var currentSelectedRow = null;
         if (AssociatedBPAContractsGridView) {
             currentSelectedRow = AssociatedBPAContractsGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedAssociatedBPAContractsRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedAssociatedBPAContractsRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetAssociatedBPAContractsHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "AssociatedBPAContractsGridView") {

             unhighlightAssociatedBPAContractsRow();

             $get("highlightedAssociatedBPAContractsRow").value = rowIndex;
             $get("highlightedAssociatedBPAContractsRowOriginalColor").value = rowColor;

             highlightAssociatedBPAContractsRow();
         }
     }



</script>

<ep:UpdatePanelEventProxy ID="ContractGeneralUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="ContractGeneralUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="ContractGeneralUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

 <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="ContractGeneralContractDatesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractGeneralContractDatesFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 520px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td colspan="4" style="text-align:center;">
                                <asp:Label ID="ContractDatesHeaderLabel" runat="server" Text="Contract Dates" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td colspan="2" style="padding-left: 10px;" >
                                            <asp:Label ID="AwardDateHeadingLabel" runat="server" Text="Awarded" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding-left: 10px;" >
                                            <ep:TextBox ID="AwardDateTextBox" runat="server" OnDataBinding="AwardDateTextBox_OnDataBinding" OnTextChanged="AwardDateTextBox_OnTextChanged" SkinId="DateTextBox"/>    
                                        </td>
                                        <td>
                                            <asp:ImageButton ID="AwardDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select award date" OnClientClick="OnC2AwardDateButtonClick()" />                           
                                        </td>                              
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td colspan="2">
                                                <asp:Label ID="EffectiveDateHeadingLabel" runat="server" Text="Effective" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="EffectiveDateTextBox" runat="server" OnDataBinding="EffectiveDateTextBox_OnDataBinding" OnTextChanged="EffectiveDateTextBox_OnTextChanged" SkinId="DateTextBox"/> 
                                        </td>
                                        <td>
                                            <asp:ImageButton ID="EffectiveDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select effective date" OnClientClick="OnC2EffectiveDateButtonClick()" />
                                        </td>                              
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td colspan="2">
                                            <asp:Label ID="ExpirationDateHeadingLabel" runat="server" Text="Expiration" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                             <ep:TextBox ID="ExpirationDateTextBox" runat="server" OnDataBinding="ExpirationDateTextBox_OnDataBinding" OnTextChanged="ExpirationDateTextBox_OnTextChanged" SkinId="DateTextBox"/>  
                                        </td>
                                        <td>
                                             <asp:ImageButton ID="ExpirationDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select expiration date" OnClientClick="OnC2ExpirationDateButtonClick()"/>                                                                     
                                        </td>                                   
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td colspan="2">
                                             <asp:Label ID="EndDateHeadingLabel" runat="server" Text="Termination" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="CompletionDateTextBox" runat="server" ForeColor="Red" OnDataBinding="CompletionDateTextBox_OnDataBinding" OnTextChanged="CompletionDateTextBox_OnTextChanged" SkinId="DateTextBox" />   
                                        </td>
                                        <td>
                                            <asp:ImageButton ID="CompletionDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select completion date" OnClientClick="OnC2CompletionDateButtonClick()" />
                                        </td>                              
                                    </tr>
                                </table>
                            </td>
                        </tr>              
                        <tr>
                            <td style="padding-left: 10px;" >
                                <asp:Label ID="TotalOptionYearsLabel" runat="server" Text="Total Option Years" />
                            </td>
                            <td>
                                <asp:DropDownList ID="OptionYearsDropDownList" runat="server" Width="79px" SelectedValue='<%#Bind("TotalOptionYears") %>' OnSelectedIndexChanged="ContractGeneralContractDatesFormView_OnChange" AutoPostBack="true">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="0" Value="0" />
                                    <asp:ListItem Text="1" Value="1" />
                                    <asp:ListItem Text="2" Value="2" />
                                    <asp:ListItem Text="3" Value="3" />
                                    <asp:ListItem Text="4" Value="4" />
                                    <asp:ListItem Text="5" Value="5" />
                                    <asp:ListItem Text="6" Value="6" />
                                    <asp:ListItem Text="7" Value="7" />
                                    <asp:ListItem Text="8" Value="8" />
                                    <asp:ListItem Text="9" Value="9" />
                                    <asp:ListItem Text="10" Value="10" />
                                </asp:DropDownList>
                            </td>
                            <td colspan="2" rowspan="2">
                                <table style="border: 1px black; width:234px;">
                                    <colgroup>
                                        <col style="width:2%" />
                                        <col style="width:40%" />
                                        <col style="width:9%" />
                                        <col style="width:47%" />
                                        <col style="width:2%" />
                                    </colgroup>
                                    <tr class="OutsetBoxHeaderRow">
                                        <td colspan="5" style="text-align:center;" >
                                            <asp:Label ID="TerminatedByHeaderLabel" runat="server" Text="Terminated By" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>

                                        </td>
                                        <td style="text-align:right;">
                                            <asp:CheckBox ID="TerminatedByDefaultCheckBox" runat="server" TextAlign="Left" Text="Cause" Checked='<%#Eval("TerminatedByDefault") %>' OnCheckedChanged="TerminatedByDefaultCheckBox_OnCheckedChanged"  AutoPostBack="true"  />                                                 
                                        </td>
                                        <td>

                                        </td>
                                        <td style="text-align:left;">
                                           <asp:CheckBox ID="TerminatedByConvenienceCheckBox" runat="server" TextAlign="Left" Text="Convenience" Checked='<%#Eval("TerminatedByConvenience") %>' OnCheckedChanged="TerminatedByConvenienceCheckBox_OnCheckedChanged" AutoPostBack="true"  /> 
                                        </td>
                                        <td>

                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td style="padding-left: 10px;" >
                                <asp:Label ID="CurrentOptionYearHeaderLabel" runat="server" Text="Current Option Year" />
                            </td>
                            <td>
                                <ep:TextBox ID="CurrentOptionYearLabel" runat="server" Width="75px" ReadOnly="true" />
                            </td>
                        </tr>
                    </table>
                    <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined"  />
                    <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False"  />

                </EditItemTemplate>
                </asp:FormView>
            </td>
             <td style="vertical-align:top; text-align:left;">         
                <asp:FormView ID="ContractGeneralContractAssignmentFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractGeneralContractAssignmentFormView_OnPreRender" OnDataBound="ContractGeneralContractAssignmentFormView_OnDataBound" >
                <EditItemTemplate>
                    <table id="ContractAssignmentTable" runat="server" class="OutsetBox" style="width:334px;">    
                        <tr class="OutsetBoxHeaderRow">
                            <td style="text-align:center;" colspan="4" >
                                <asp:Label ID="ContractAssignmentHeaderLabel" runat="server" Text="Contract Assignment" />
                            </td>
                        </tr>  
                        <tr>
                            <td align="right" style="width:36%;">
                                <asp:Label ID="ContractingOfficerLabel" Text="Contracting Officer" Width="98%" runat="server" CssClass="FieldLabelText" ></asp:Label>
                            </td>
                            <td colspan="3" style="width:64%;">
                                <asp:DropDownList ID="ContractingOfficerDropDownList" runat="server" OnSelectedIndexChanged="ContractGeneralContractAssignmentFormView_OnChange" Width="98%" EnableViewState="true" AutoPostBack="true"  ></asp:DropDownList>
                            </td>
                        </tr>              
                    </table>
                </EditItemTemplate>
                </asp:FormView>
            </td>
            <td rowspan="3" style="height:400px; vertical-align:top;">
                <asp:Panel ID="SINPanel" runat="server" OnPreRender="SINPanel_OnPreRender" >
                    <table class="OutsetBox">
                        <tr class="OutsetBoxHeaderRow">
                            <td style="text-align:center;">
                                <asp:Label ID="ContractSINHeaderLabel" runat="server" Text="Contract SINs" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Button ID="AddContractSIN" runat="server" Text="Add SIN" OnClick="AddContractSIN_OnClick" />
                            </td>
                        </tr>
                        <tr>
                            <td >
                                <div>
                                    <ep:ListView ID="SINListView" runat="server" ItemPlaceholderID="SINItemPlaceholder" 
                                         InsertItemPosition="None"
                                          OnItemCommand="SINListView_OnItemCommand" 
                                           OnItemCreated="SINListView_OnItemCreated"   >
                                        <LayoutTemplate>
                                                
                                            <table >
                                                <tr class="OutsetBoxHeaderRow">
                                                    <td>
                                                        <asp:Label ID="SINListViewSINHeaderLabel" runat="server" Text="SIN" Width="84px" />
                                                    </td>
                                                    <td>
                                                        <asp:Label ID="SINListViewRecoverableHeaderLabel" runat="server" Text="RC"  Width="28px"  />
                                                    </td>
                                                    <td colspan="2" style="width:100%;">
                                                    </td>
 
                                                </tr>
                                                <asp:PlaceHolder runat="server" ID="SINItemPlaceholder" />
                                            </table>
                                               
                                        </LayoutTemplate>

                                        <ItemTemplate>
                                            <tr>
                                                <td>
                                                    <asp:Label ID="SINLabel" runat="server" Text='<%#Eval("SIN") %>' />
                                                </td>
                                                <td>
                                                    <asp:Checkbox ID="RecoverableCheckBox" runat="server"  OnDataBinding="RecoverableCheckBox_OnDataBinding"  OnCheckedChanged="RecoverableCheckBox_OnCheckChanged" AutoPostBack="true"/>
                                                </td>
                                                <td colspan="2">
                                                    <asp:Button ID="SINListViewDeleteButton" runat="server" Text="Delete" CommandName="SINListViewDelete" CommandArgument='<%# Container.DataItemIndex + "," + Container.DisplayIndex + "," + Eval("SIN") %>' />
                                                </td>
  
                                            </tr>
                                        </ItemTemplate>

                                        <EditItemTemplate>
                                            <tr>
                      
                                            </tr>                      
                                        </EditItemTemplate>

                                        <InsertItemTemplate>
                                           <tr>
                                                <td>
                                                    <asp:DropDownList ID="SINDropDownList" runat="server" DataTextField="SIN" DataValueField="SIN"  AutoPostBack="True" EnableViewState="True" OnSelectedIndexChanged="SINDropDownList_OnSelectedIndexChanged"  />
                                                </td>
                                                <td>
                                                    <asp:Checkbox ID="RecoverableCheckBox" runat="server" />
                                                </td>
                                                <td>
                                                    <asp:Button ID="SINListViewSaveInsertButton" runat="server" Text="Save" CommandName="SINListViewSaveInsert" CommandArgument='<%# Container.DataItemIndex + "," + Container.DisplayIndex %>' />
                                                </td>
                                                 <td>
                                                    <asp:Button ID="SINListViewCancelInsertButton" runat="server" Text="Cancel" CommandName="SINListViewCancelInsert" CommandArgument='<%# Container.DataItemIndex + "," + Container.DisplayIndex %>' />
                                                </td>
                                            </tr>         
                                        </InsertItemTemplate>
          
                                    </ep:ListView>
                                    <div>
                                        <table style="border-top:solid 1px Black; width:100%;">
                                            <tr>
                                                <td style="text-align:center;">
   
                                                    <asp:DataPager ID="SINListViewDataPager"
                                                                    runat="server"
                                                                    PagedControlID="SINListView"
                                                                    PageSize="10"  OnPreRender="SINListViewDataPager_OnPreRender">

                                                        <Fields>
                                                        
                                                            <asp:NextPreviousPagerField ShowFirstPageButton="false"
                                                                        ButtonType="Button"
                                                                        ShowPreviousPageButton="true"
                                                                        ShowLastPageButton="false"
                                                                        ShowNextPageButton="false"
                                                                        ButtonCssClass="ListViewDataPagerLast"
                                                                        RenderNonBreakingSpacesBetweenControls="true"
                                                                        PreviousPageText="<"  />

                                                            <asp:TemplatePagerField>
                                                                <PagerTemplate>                                                            
                                                                    <asp:Label ID="SINListViewDataPagerCurrentRowLabel" runat="server" Text='<%# String.Format( "{0} to ", ( Container.StartRowIndex + 1 ) )%>' />
                                                               
                                                                    <asp:Label ID="SINListViewDataPagerPageSizeLabel" runat="server"  Text='<%# String.Format( "{0} of ", ( Container.StartRowIndex + Container.PageSize ) > Container.TotalRowCount ? Container.TotalRowCount : ( Container.StartRowIndex + Container.PageSize )) %>' />
                                                                
                                                                    <asp:Label ID="SINListViewDataPagerTotalRowsLabel" runat="server"  Text='<%# Container.TotalRowCount %>' />
                                                                </PagerTemplate>
                                                            </asp:TemplatePagerField>


                                                            <asp:NextPreviousPagerField ShowFirstPageButton="false"
                                                                        ButtonType="Button"
                                                                        ShowPreviousPageButton="false"
                                                                        ShowLastPageButton="false"
                                                                        ShowNextPageButton="true"
                                                                        ButtonCssClass="ListViewDataPagerFirst"
                                                                        RenderNonBreakingSpacesBetweenControls="true"
                                                                        NextPageText=">" />

                                                       
                                                        </Fields>
                                                    </asp:DataPager>
            
                                                </td>
                                            </tr>
                                        </table>                               
                                
                                    </div>

                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td style="vertical-align:top;">         
                <asp:FormView ID="ContractGeneralContractAttributesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractGeneralContractAttributesFormView_OnPreRender" >
                <EditItemTemplate>
                    <asp:Table ID="ContractAttributesTable" runat="server" CssClass="OutsetBox" style="width: 520px;">  
                        <asp:TableRow runat="server" CssClass="OutsetBoxHeaderRow">
                            <asp:TableCell  runat="server" ColumnSpan="4"  style="text-align:center;"  >
                                <asp:Label ID="ContractAttributesHeaderLabel" runat="server" Text="Contract Attributes" />
                            </asp:TableCell>
                        </asp:TableRow>  
                        <asp:TableRow  runat="server" >
                            <asp:TableCell ID="DescriptionCell" runat="server" ColumnSpan="4" style="text-align:left;" >
                                <asp:Table ID="DescriptionTable" runat="server" >               
                                    <asp:TableRow runat="server" >
                                        <asp:TableCell ID="ContractDescriptionHeaderCell" runat="server" Width="38%" style="padding-left: 10px;" >
                                            <asp:Label ID="ContractDescriptionHeadingLabel" runat="server" Text="Short Contract Description" /> 
                                        </asp:TableCell>
                                        <asp:TableCell ID="ContractDescriptionDataCell"  runat="server" Width="58%">
                                            <ep:TextBox ID="ContractDescriptionTextBox" runat="server" MaxLength="50" Text='<%# Eval("ContractDescription") %>' Width="284px"  /> 
                                        </asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </asp:TableCell>
                        </asp:TableRow>   
                                                        
                        <asp:TableRow  runat="server" >                         
                            <asp:TableCell ID="VADODDataCell" runat="server" ColumnSpan="2" style="padding-left: 10px; padding-top: 8px;" >
                                <asp:CheckBox ID="VADODContractCheckBox" runat="server" TextAlign="Left" Text="VA/DOD Contract" Checked='<%#Eval("VADOD") %>'  AutoPostBack="true"/>
                            </asp:TableCell>

                            <asp:TableCell ID="StandardizedDataCell" runat="server"  ColumnSpan="2" style="padding-top: 8px;" >
                                <asp:CheckBox ID="StandardizedCheckBox" runat="server" TextAlign="Left" Text="Standardized" Checked='<%#Eval("Standardized") %>' AutoPostBack="true" />
                            </asp:TableCell>                       
                        </asp:TableRow>

                         <asp:TableRow ID="StimulusRow"  runat="server" >
                             <asp:TableCell ID="StimulusDataCell"  runat="server"  ColumnSpan="1" style="padding-left: 10px; padding-top: 8px;" >
                                <asp:CheckBox ID="StimulusActCheckBox" runat="server" TextAlign="Left" Text="Stimulus Act" Checked='<%#Eval("StimulusAct") %>' AutoPostBack="true" />
                             </asp:TableCell>
                             <asp:TableCell ID="BlankSpacerCell3" runat="server" ColumnSpan="3" >
                             </asp:TableCell>
                         </asp:TableRow>

                           <asp:TableRow ID="MedSurgRow" runat="server" >                            
                            <asp:TableCell ID="MedSurgCell" runat="server" ColumnSpan="4" style="text-align:left; padding: 10px 10px 10px 10px;" >
                                <asp:Table ID="MedSurgInnerTable" runat="server" CssClass="InnerOutsetBox" style="width: 360px;">  
                                    <asp:TableRow ID="MedSurgHeaderRow" runat="server" CssClass="OutsetBoxHeaderRow">
                                        <asp:TableCell ID="MedSurgHeaderCell"  runat="server" style="text-align:center;"  ColumnSpan="2">
                                            <asp:Label ID="MedSurgHeaderLabel" runat="server" Text="Non-Pharm Items Only" />
                                        </asp:TableCell>
                                    </asp:TableRow>  

                   
                                    <asp:TableRow ID="PrimeVendorRow" runat="server" >
                            
                                        <asp:TableCell ID="PrimeVendorDataCell" runat="server" ColumnSpan="2" style="padding-left: 6px;">
                                            <asp:CheckBox ID="PrimeVendorCheckBox" runat="server" TextAlign="Left" Text="Prime Vendor Participation" Checked='<%#Eval("PrimeVendorParticipation") %>' AutoPostBack="true" />
                                        </asp:TableCell>
                                                                    
                                    </asp:TableRow>


                                    <asp:TableRow ID="TradeAgreementRow"  runat="server" >
                                        <asp:TableCell ID="TradeAgreementHeaderCell"  runat="server" CssClass="leftAlign" ColumnSpan="1" style="padding-left: 6px;">
                                            <asp:Label ID="TradeAgreementHeaderLabel"  runat="server"  Text="Trade Agreement Act Compliant" />
                                        </asp:TableCell>
                                        <asp:TableCell ID="TradeAgreementYesOtherTableCell" runat="server"  CssClass="leftAlign"  ColumnSpan="1" >
                                            <asp:Table ID="YesOtherTable" runat="server" >
                                                <asp:TableRow ID="YesOtherTableRow"  runat="server" >
                                                    <asp:TableCell ID="TradeAgreementYesDataCell"  runat="server"   CssClass="rightAlign"  VerticalAlign="Middle" ColumnSpan="1" >
                                                        <asp:CheckBox ID="TradeAgreementYesCheckBox" runat="server"  Width="70px" TextAlign="Left"  Text="Yes" Checked='<%#GetTradeAgreementCheckBoxValue( ( string )Eval("TradeAgreementActCompliance"), "Yes" ) %>' OnCheckedChanged="TradeAgreementYesCheckBox_OnCheckedChanged" AutoPostBack="true" EnableViewState="true" />
                                                    </asp:TableCell>
                                                    <asp:TableCell ID="TradeAgreementOtherDataCell" runat="server"   CssClass="leftAlign"  VerticalAlign="Middle" ColumnSpan="1" >
                                                        <asp:CheckBox ID="TradeAgreementOtherCheckBox" runat="server"  Width="70px" TextAlign="Left" Text="Other" Checked='<%#GetTradeAgreementCheckBoxValue( ( string )Eval("TradeAgreementActCompliance"), "Other" ) %>' OnCheckedChanged="TradeAgreementOtherCheckBox_OnCheckedChanged" AutoPostBack="true" EnableViewState="true" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                            </asp:Table>        
                                        </asp:TableCell>
                                        
                                    </asp:TableRow>

                                </asp:Table>
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>  
               </EditItemTemplate>
               </asp:FormView>
                               
            </td>
 
            <td style="vertical-align:top;  text-align:left;">
                <asp:FormView ID="ContractGeneralParentContractFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractGeneralParentContractFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width:334px;">
                        <tr class="OutsetBoxHeaderRow">
                            <td style="text-align:center;" colspan="4">
                                <asp:Label ID="ParentFSSContractHeaderLabel" runat="server" Text="Parent FSS Contract" />
                            </td>
                        </tr>
                        <tr>
                            <td >    
                                <asp:Button ID="SelectParentFSSContractNumberButton" runat="server" OnClick="SelectParentFSSContractNumberButton_OnClick" CssClass="documentSelectButton"  UseSubmitBehavior="false" Width="100px" Text='<%#Eval("ParentFSSContractNumber") %>'   Font-Bold="true" />
                            </td>
                            <td colspan="3" style="text-align:right; white-space:nowrap;">
                                <asp:Label ID="ParentFSSContractStatusHeaderLabel" runat="server" Text="Contract Status: " />
                                <asp:Label ID="ParentFSSContractStatusLabel" runat="server" Text='<%# ParentContractStatus( Eval("ParentExpirationDate"), Eval("ParentCompletionDate"), "ParentFSSContractStatusLabel" ) %>'  /> 
                            </td>
                        </tr>
                        <tr>
                            <td  colspan="4">
                                <asp:Label ID="ParentFSSVendorNameLabel" runat="server" Text='<%# Eval("ParentVendorName") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td  colspan="4">
                                <asp:Label ID="ParentFSSScheduleNameLabel" runat="server" Text='<%# String.Format( "Schedule: {0}", Eval("ParentScheduleName") )%>' />
                            </td>
                        </tr>       
                        <tr>
                            <td  colspan="4">
                                <asp:Label ID="ParentFSSContractingOfficerName" runat="server" Text='<%# String.Format( "Contracting Officer: {0}", Eval("ParentContractingOfficerFullName") )%>' />
                            </td> 
                        </tr>
                        <tr>
                            <td  colspan="4">
                                <table style="width:100%;" >
                                    <colgroup>
                                        <col style="width:25%" />
                                        <col style="width:25%" />
                                        <col style="width:25%" />
                                        <col style="width:25%" />
                                    </colgroup>
                                    <tr>
                                        <td>
                                             <asp:Label ID="ParentFSSAwardDateHeaderLabel" runat="server" Text="Awarded" />
                                        </td>
                                        <td >
                                             <asp:Label ID="ParentFSSEffectiveDateHeaderLabel" runat="server" Text="Effective" />
                                        </td>
                                        <td >
                                            <asp:Label ID="ParentFSSExpirationDateHeaderLabel" runat="server" Text="Expiration" />
                                        </td>
                                        <td>
                                             <asp:Label ID="ParentFSSCompletionDateHeaderLabel" runat="server" Text="End Date" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                             <asp:Label ID="ParentFSSAwardDateLabel" runat="server" Text='<%#Eval("ParentAwardDate") %>' />               
                                        </td>
                                        <td>
                                            <asp:Label ID="ParentFSSEffectiveDateLabel" runat="server" Text='<%#Eval("ParentEffectiveDate") %>' />
                                        </td>
                                        <td>
                                             <asp:Label ID="ParentFSSExpirationDateLabel" runat="server" Text='<%#Eval("ParentExpirationDate") %>' />                                                                      
                                        </td>
                                        <td>
                                            <asp:Label ID="ParentFSSCompletionDateLabel" runat="server" ForeColor="Red" OnDataBinding="ParentFSSCompletionDateLabel_OnDataBinding"  />   
                                        </td>
                                    </tr>
                                </table>

                            </td>
                        
                        </tr>
                       
                    </table>
                </EditItemTemplate>
                </asp:FormView>        
                
                <asp:Panel ID="AssociatedBPAContractsGridPanel" runat="server" Width="100%" Height="100%" OnPreRender="AssociatedBPAContractsGridPanel_OnPreRender" >     
                    <div id="AssociatedBPAContractsPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                            <asp:HiddenField ID="AssociatedBPAContractsScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedAssociatedBPAContractsRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedAssociatedBPAContractsRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                    </div>
                    <table class="OutsetBox" runat="server" style="width:316px;">
                        <tr class="OutsetBoxHeaderRow">
                            <td style="text-align:center;" colspan="4">
                                <asp:Label ID="AssociatedBPAContractsHeaderLabel" runat="server" Text="Associated BPA Contracts" />
                            </td>
                        </tr>
                        <tr>
                            <td>                           
                                <div id="AssociatedBPAContractsGridViewDiv"  runat="server"  style="border:3px solid black; width:316px; height:118px; overflow: scroll" onscroll="javascript:setAssociatedBPAContractsScrollForRestore( this );"  onkeypress="javascript:setAssociatedBPAContractsScrollForRestore( this );"  >
                                    <ep:GridView ID="AssociatedBPAContractsGridView" 
                                                runat="server" 
                                                DataKeyNames="ContractId"  
                                                AutoGenerateColumns="False" 
                                                Width="100%" 
                                                CssClass="CMGridSmall" 
                                                Visible="True"                              
                                                OnRowDataBound="AssociatedBPAContractsGridView_RowDataBound"
                                                AllowSorting="True" 
                                                AutoGenerateEditButton="false"
                                                EditRowStyle-CssClass="CMGridEditRowStyleSmall"   
                                                OnRowCreated="AssociatedBPAContractsGridView_OnRowCreated"
                                                AllowInserting="True"
                                               
                                                EmptyDataRowStyle-CssClass="CMGrid" 
                                                EmptyDataText="There are no associated BPA contracts."
                                                ContextMenuID="AssociatedBPAContractsContextMenu"
                                                Font-Names="Arial" 
                                                Font-Size="small" >
                                            <HeaderStyle CssClass="CMGridNoHeaders" />
                                            <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                            <AlternatingRowStyle CssClass="CMGridAltItems" />
                                            <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                            <Columns>                        
                                                <asp:TemplateField  ControlStyle-Font-Size="11px" ShowHeader="false" ItemStyle-VerticalAlign="Top" ItemStyle-Width="302px"  >
                                                    <ItemTemplate>  
                                                        <table style="table-layout:fixed;"  >
                                                            <col style="width:152px;" />
                                                            <col style="width:152px;" /> 
                                                            <tr>
                                                                <td>
                                                                    <asp:Button  ID="SelectBPAAssociatedContractButton" runat="server" CssClass="documentSelectButton" UseSubmitBehavior="false" OnCommand="AssociatedBPAContractsGridView_ButtonCommand" CommandName="JumpToAssociatedBPAContract" CommandArgument='<%# Container.DataItemIndex + "," + Eval( "ContractId" ) + "," + Eval( "ContractNumber" ) + "," + Eval( "ScheduleNumber" ) %>' 
                                                                    Width="146px" Text='<%# Eval( "ContractNumber" ) %>' Font-Bold="true" />                                                                 
                                                                </td>                                                            
                                                                <td>
                                                                    <asp:Label ID="ContractStatusLabel" runat="server"  />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Label ID="ScheduleNameLabel" runat="server" Text='<%# Eval( "ScheduleName" ) %>' />
                                                                </td>
                                                                <td>
                                                                    <asp:Label ID="ContractingOfficerNameLabel" runat="server" Text='<%# Eval( "ContractingOfficerName" ) %>' />
                                                                </td>                                                               
                                                            </tr>
                                                        </table>                                                                                      
                                                                                              
                                                    </ItemTemplate>
                                                </asp:TemplateField>      
                                            </Columns>
                                    </ep:GridView>    
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>                        
            </td>
            <td>
            </td>
        </tr> 
        <tr>
            <td style="height:164px; vertical-align:top;">
            </td>
        </tr> 
    </table>

</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
