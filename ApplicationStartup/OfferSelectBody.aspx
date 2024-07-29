<%@ Page Title="" Language="C#" MasterPageFile="~/OfferSearch.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="OfferSelectBody.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferSelectBody" %>
<%@ MasterType VirtualPath="~/OfferSearch.Master" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<asp:Content ID="OfferSearchContent" ContentPlaceHolderID="OfferSearchContentPlaceHolder" runat="server">

 <script type="text/javascript" >
     /* called on grid div scroll */
     function setSearchScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("SearchScrollPos").value = divToScroll.scrollTop;
         }
     }

     function setItemScrollOnChange(newPositionValue) {
         $get("SearchScrollPos").value = newPositionValue;
         $get("SearchGridViewDiv").scrollTop = newPositionValue;
     }

     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestoreSearchGridSelectionOnAsyncPostback();
     }

     function RestoreSearchGridSelectionOnAsyncPostback() {
         var SearchScrollPos = $get("SearchScrollPos").value;
         var highlightedSearchRow = $get("highlightedSearchRow").value;

         RestoreSearchGridSelection(SearchScrollPos, highlightedSearchRow);
     }

     /* called from form load */
     function RestoreSearchGridSelection(SearchScrollPos, highlightedSearchRow) {
         $get("SearchScrollPos").value = SearchScrollPos;
         if (SearchScrollPos) {
             if (SearchScrollPos >= 0) {

                 var theSearchDiv = document.getElementById('<%=SearchGridViewDiv.ClientID %>');
                 if (theSearchDiv) {
                     theSearchDiv.scrollTop = SearchScrollPos;
                 }
             }
         }

         if (highlightedSearchRow) {
             if (highlightedSearchRow >= 0) {
                 $get("highlightedSearchRow").value = highlightedSearchRow;
                 highlightSearchRow();
             }
         }
     }

     function setSearchHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedSearchRow").value = rowIndex;
         $get("highlightedSearchRowOriginalColor").value = originalColor;
         highlightSearchRow();

     }

     function highlightSearchRow() {

         var selectedRowIndex = $get("highlightedSearchRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var SearchGridView = document.getElementById("<%=SearchGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (SearchGridView) {
             currentSelectedRow = SearchGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMSearchGridSelectedCellStyle';
         }

     }

     function unhighlightSearchRow() {

         var selectedRowIndex = $get("highlightedSearchRow").value;
         var highlightedSearchRowOriginalColor = $get("highlightedSearchRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedSearchRow").value = -1;
         var SearchGridView = document.getElementById("<%=SearchGridView.ClientID%>");
         var currentSelectedRow = null;
         if (SearchGridView) {
             currentSelectedRow = SearchGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedSearchRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMSearchGridUnSelectedCellStyleAlt';
             }
             else if (highlightedSearchRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMSearchGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetSearchHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "SearchGridView") {

             unhighlightSearchRow();

             $get("highlightedSearchRow").value = rowIndex;
             $get("highlightedSearchRowOriginalColor").value = rowColor;

             highlightSearchRow();
         }
     }


     function refreshSearchWindowFromContextMenu() {
         $get("SearchGridViewUpdatePanel").Update;
     }

     function offerNumberMouseChange(offerButton, overOut, rowIndex, rowColor) {
         if (overOut == 'over') {
             offerButton.style.color = 'purple';
             offerButton.style.cursor = 'hand';
         }
         else {
             offerButton.style.color = 'black';
             offerButton.style.cursor = 'pointer';
         }
     }

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

</script>

<ep:UpdatePanelEventProxy ID="SearchUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="SearchUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="SearchUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

<asp:Panel ID="SearchGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="SearchGridPanel_OnPreRender" >
 
<table class="OuterTable" >
        <tr style="border: solid 1px black; vertical-align:top;">
            <td>
  
                    <div id="SearchPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="SearchScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSearchRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSearchRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    </div>
        
                   <div id="SearchGridViewDiv"  runat="server" style="border:3px solid black; width:99%; height:460px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="RightClickUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="SearchGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="RightClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
 
                                 <ep:GridView ID="SearchGridView" 

                                            runat="server" 
                                            EnableViewState="true"
                                            DataKeyNames="Offer_ID, Schedule_Number"  
                                            AutoGenerateColumns="False" 
                                            Width="100%" 
                                            RowStyle-Height="36px"
                                            CssClass="CMGrid" 
                                            Visible="True" 
                                            AllowSorting="True" 
                                            OnSorting="SearchGridView_OnSorting"
                                            OnRowDataBound="SearchGridView_RowDataBound"
                                            OnRowCommand="SearchGridView_OnRowCommand" 
                                            CommandSeparator="^"
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="CMGridEditRowStyle" 
                                            onprerender="SearchGridView_PreRender" 
                                            OnDataBound="SearchGridView_OnDataBound"
                                            OnInit="SearchGridView_Init"
                                            EmptyDataRowStyle-CssClass="CMGrid" 
                                            EmptyDataText="There are no offers matching the selected filter criteria."
                                           >
                                        <HeaderStyle CssClass="SearchGridHeaders" />
                                        <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="CMGridAltItems" />
                                        <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                            
                                            <asp:TemplateField  ControlStyle-Width="340px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractorNameColumnHeaderButton" Text="Vendor Name" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Contractor_Name" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Button  ID="SelectOfferButton" runat="server" class="documentSelectButton" UseSubmitBehavior="false" CommandName="EditSearchOffer" CommandArgument='<%# Container.DataItemIndex + "^" + Eval( "Offer_ID" ) + "^" + Eval( "Schedule_Number" ) + "^" + Eval( "Schedule_Name" ) + "^" + Eval( "Contractor_Name" ) + "^" + Eval( "Dates_Received" ) + "^" + Eval( "Dates_Assigned" ) + "^" + Eval( "CO_ID" ) + "^" + Eval( "FullName" ) + "^" + Eval( "ContractNumber" ) + "^" + Eval( "ContractId" ) + "^" + Eval( "Complete" )  + "^" + Eval( "OfferNumber" ) + "^" + Eval( "ProposalTypeId" )  + "^" + Eval( "ExtendsContractNumber" ) + "^" + Eval( "ExtendsContractId" ) %>' 
                                                                                Text='<%# Eval( "Contractor_Name" ) %>'  />                                          
                                                </ItemTemplate>
                                            </asp:TemplateField>      

                                            <asp:TemplateField  ControlStyle-Width="100px" >                                             
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="OfferNumberColumnHeaderButton" Text="Offer Number" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="OfferNumber" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="OfferNumberLabel" Text='<%# Eval( "OfferNumber" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>    
                                            
                                            <asp:TemplateField  ControlStyle-Width="75px" >                                             
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ReceivedDateColumnHeaderButton" Text="Date Received" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_Received" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ReceivedDateLabel" Text='<%# Eval( "Dates_Received", "{0:d}" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>    

                                            <asp:TemplateField  ControlStyle-Width="75px" >                                             
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="AssignmentDateColumnHeaderButton" Text="Date Assigned" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_Assigned" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="AssignmentDateLabel" Text='<%# Eval( "Dates_Assigned", "{0:d}" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     
                                                      
                                             <asp:TemplateField  ControlStyle-Width="150px" >  
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ScheduleNameColumnHeaderButton" Text="Schedule Name" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Schedule_Name" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ScheduleNameLabel" Text='<%# Eval( "Schedule_Name" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                             <asp:TemplateField  ControlStyle-Width="90px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractingOfficerNameColumnHeaderButton" Text="Contracting Officer" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="LastName" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ContractingOfficerNameLabel" Text='<%# Eval( "FullName" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:TemplateField  ControlStyle-Width="60px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="OfferStatusColumnHeaderButton" Text="Status" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Action_Description" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="OfferStatusLabel" Text='<%# Eval( "Action_Description" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:TemplateField  ControlStyle-Width="100px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ExtendsContractNumberHeaderButton" Text="Extension of Contract" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="ExtendsContractNumber" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Button runat="server" ID="SelectExtendsContractNumberButton" class="documentSelectButton" UseSubmitBehavior="false" CommandName="EditExtendsContract" CommandArgument='<%# Container.DataItemIndex + "^" + Eval( "ExtendsContractId" ) + "^" + Eval( "ExtendsContractNumber" ) + "^" + Eval( "Schedule_Number" ) %>' 
                                                            Text='<%# Eval( "ExtendsContractNumber" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>    

                                            <asp:TemplateField  ControlStyle-Width="100px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractNumberHeaderButton" Text="Contract" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="ContractNumber" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Button runat="server" ID="SelectContractButton" class="documentSelectButton" UseSubmitBehavior="false" CommandName="EditSearchContract" CommandArgument='<%# Container.DataItemIndex + "^" + Eval( "ContractId" ) + "^" + Eval( "ContractNumber" ) + "^" + Eval( "Schedule_Number" ) %>' 
                                                            Text='<%# Eval( "ContractNumber" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:BoundField Visible="false" ShowHeader="false" DataField="LastName"  />
                                           
                                        </Columns>
                            
                            </ep:GridView>

                        </ContentTemplate>
                    </asp:UpdatePanel> 
                </div>
            </td>
    
        </tr>
        <tr>
            <td>
                <div id="HiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                    <asp:HiddenField ID="RefreshOfferSelectScreenOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="RebindOfferSelectScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="ReselectPreviouslySelectedItem" runat="server" Value="false" /> 
                </div>
            </td>
        </tr>
    </table>
    <ep:MsgBox ID="MsgBox" NameofMsgBox="OfferSelectBody" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />

</asp:Panel>
</ContentTemplate> 
</asp:UpdatePanel> 
</asp:Content>

