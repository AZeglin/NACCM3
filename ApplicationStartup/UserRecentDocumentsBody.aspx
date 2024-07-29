<%@ Page Title="" Language="C#" MasterPageFile="~/UserRecentDocuments.master" StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="UserRecentDocumentsBody.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.UserRecentDocumentsBody" %>
<%@ MasterType VirtualPath="~/UserRecentDocuments.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="UserRecentDocumentsContent" ContentPlaceHolderID="UserRecentDocumentsPlaceHolder" runat="server">

 <script type="text/javascript" >
     /* called on grid div scroll */
     function setSearchScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("SearchScrollPos").value = divToScroll.scrollTop;
         }
     }

     function setItemScrollOnChange(newPositionValue) {
         $get("SearchScrollPos").value = newPositionValue;
         $get("UserRecentDocumentsGridViewDiv").scrollTop = newPositionValue;
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

                 var theSearchDiv = document.getElementById('<%=UserRecentDocumentsGridViewDiv.ClientID %>');
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

         var UserRecentDocumentsGridView = document.getElementById("<%=UserRecentDocumentsGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (UserRecentDocumentsGridView) {
             currentSelectedRow = UserRecentDocumentsGridView.rows[selectedRowIndex];   /* ok */
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
         var UserRecentDocumentsGridView = document.getElementById("<%=UserRecentDocumentsGridView.ClientID%>");
         var currentSelectedRow = null;
         if (UserRecentDocumentsGridView) {
             currentSelectedRow = UserRecentDocumentsGridView.rows[selectedRowIndex];
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
         if (sourceGridName == "UserRecentDocumentsGridView") {

             unhighlightSearchRow();

             $get("highlightedSearchRow").value = rowIndex;
             $get("highlightedSearchRowOriginalColor").value = rowColor;

             highlightSearchRow();
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

<ep:UpdatePanelEventProxy ID="UserRecentDocumentsUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="UserRecentDocumentsUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="UserRecentDocumentsUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

<asp:Panel ID="UserRecentDocumentsGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="UserRecentDocumentsGridPanel_OnPreRender" >
 
<table class="OuterTable" >
        <tr style="border: solid 1px black; vertical-align:top;">
            <td>
  
                    <div id="UserRecentDocumentsPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="SearchScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSearchRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSearchRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />

                    </div>
        

                   <div id="UserRecentDocumentsGridViewDiv"  runat="server" style="border:3px solid black; width:99%; height:440px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="UserRecentDocumentsGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                         
                                 <ep:GridView ID="UserRecentDocumentsGridView" 

                                            runat="server" 
                                            EnableViewState="true"
                                            DataKeyNames="UserPreferenceId"  
                                            AutoGenerateColumns="False" 
                                            Width="100%" 
                                            RowStyle-Height="36px"
                                            CssClass="CMGrid" 
                                            Visible="True" 
                                            AllowSorting="False" 
                                            OnRowDataBound="UserRecentDocumentsGridView_RowDataBound"
                                            OnRowCommand="UserRecentDocumentsGridView_OnRowCommand"
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="CMGridEditRowStyle" 
                                            onprerender="UserRecentDocumentsGridView_PreRender" 
                                            OnDataBound="UserRecentDocumentsGridView_OnDataBound"
                                            EmptyDataRowStyle-CssClass="CMGrid" 
                                            EmptyDataText="There are no documents in your recent documents list."
                                           >
                                        <HeaderStyle CssClass="SearchGridHeaders" />
                                        <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="CMGridAltItems" />
                                        <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                            
                                            <asp:TemplateField  ControlStyle-Width="134px" >   
                                                <HeaderTemplate>
                                                    <asp:Label ID="SelectDocumentHeaderLabel" runat="server" Text="Document Number" />
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Button  ID="SelectDocumentButton" runat="server" class="documentSelectButton" UseSubmitBehavior="false" CommandName="EditSelectedDocument" CommandArgument='<%# Container.DataItemIndex + "," + Eval( "DocumentId" ) + "," + Eval( "DocumentNumber" ) + "," + Eval( "Schedule_Number" ) + "," + Eval( "DocumentType" ) %>' 
                                                                                Width="128px" Text='<%# Eval( "DocumentNumber" ) %>'  />                                          
                                                </ItemTemplate>
                                            </asp:TemplateField>     
                                            
                                                 <asp:BoundField DataField="Schedule_Name" HeaderText="Schedule" 
                                                SortExpression="Schedule_Name" /> 

                                            <asp:BoundField DataField="DocumentType" HeaderText="Document Type" 
                                                SortExpression="DocumentType" />
                                                                                       
                                            <asp:BoundField DataField="CO_Name" HeaderText="CO" 
                                                SortExpression="CO_Name" />
                                           
                                            <asp:BoundField DataField="Contractor_Name" HeaderText="Contractor Name" 
                                                SortExpression="Contractor_Name" />
                                           
                                            <asp:BoundField DataField="DocumentDate" HeaderText="Date" 
                                                SortExpression="DocumentDate" ControlStyle-Width="190px"   />                                           
                                          
                                            <asp:TemplateField  ControlStyle-Width="126px" >   
                                                <HeaderTemplate>
                                                    <asp:Label ID="ActiveStatusHeaderLabel" runat="server" Text="Status" />
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label ID="ActiveStatusDataLabel" runat="server" />                                              
                                                </ItemTemplate>
                                            </asp:TemplateField>     

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
                    <asp:HiddenField ID="RefreshContractSelectScreenOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="RebindContractSelectScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="ReselectPreviouslySelectedItem" runat="server" Value="false" /> 
                </div>
            </td>
        </tr>
    </table>
    <ep:MsgBox ID="MsgBox" NameofMsgBox="UserRecentDocumentsBody" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />

</asp:Panel>
</ContentTemplate> 
</asp:UpdatePanel> 
</asp:Content>


