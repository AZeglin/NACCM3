<%@ Page Title="" Language="C#" MasterPageFile="~/ReportSearch.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ReportSelectBody.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ReportSelectBody" %>
<%@ MasterType VirtualPath="~/ReportSearch.Master" %>


<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ReportsContent" ContentPlaceHolderID="ReportSearchContentPlaceHolder" runat="server">


 <script type="text/javascript" >
     /* called on grid div scroll */
     function setSearchScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("SearchScrollPos").value = divToScroll.scrollTop;
         }
     }

     function setItemScrollOnChange(newPositionValue) {
         $get("SearchScrollPos").value = newPositionValue;
         $get("ReportSearchListViewDiv").scrollTop = newPositionValue;
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

                 var theSearchDiv = document.getElementById('<%=ReportSearchListViewDiv.ClientID %>');
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

         var ReportListView = document.getElementById("<%=ReportListView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (ReportListView) {
             currentSelectedRow = ReportListView.rows[selectedRowIndex];   /* ok */
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
         var ReportListView = document.getElementById("<%=ReportListView.ClientID%>");
         var currentSelectedRow = null;
         if (ReportListView) {
             currentSelectedRow = ReportListView.rows[selectedRowIndex];
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
     function resetSearchHighlighting(sourceListViewName, rowIndex, rowColor) {
         if (sourceListViewName == "ReportListView") {

             unhighlightSearchRow();

             $get("highlightedSearchRow").value = rowIndex;
             $get("highlightedSearchRowOriginalColor").value = rowColor;

             highlightSearchRow();
         }
     }


     function refreshSearchWindowFromContextMenu() {
         $get("ReportSearchListViewUpdatePanel").Update;
     }

     function reportNameMouseChange(reportButton, overOut) {
         if (overOut == 'over') {
             reportButton.style.color = 'purple';
             reportButton.style.cursor = 'hand';
         }
         else {
             reportButton.style.color = 'black';
             reportButton.style.cursor = 'pointer';
         }
     }

</script>

<asp:Panel ID="ReportSearchPanel" runat="server" Width="100%" Height="100%"  >

    <table class="OuterTable" >
        <tr style="border: solid 1px black; vertical-align:top;">
            <td>
                <table class="OutsetBox" style="width: 100%;">
                        
                        <tr>
                            <td>
                                 <div id="ReportSearchPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                                        <asp:HiddenField ID="SearchScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                        <asp:HiddenField ID="highlightedSearchRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                                        <asp:HiddenField ID="highlightedSearchRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                                 </div>

                                 <div id="ReportSearchListViewDiv"  runat="server" style="border:1px solid black; width:99%; height:380px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                            <ep:ListView ID="ReportListView" runat="server" ItemPlaceholderID="ReportItemPlaceholder" GroupItemCount="4"
                                                 InsertItemPosition="None" OnItemDataBound="ReportListView_OnItemDataBound" >
                                                <LayoutTemplate>
                                                
                                                    <table runat="server" id="reportListLayoutTable" cellpadding="5" cellspacing="5" >
                                                        <tr runat="server" id="groupPlaceholder" class="OutsetBoxHeaderRow">
                                                           
                                                        </tr>
                                                    </table>
                                               
                                                </LayoutTemplate>
                                                <GroupTemplate>
                                                    <tr runat="server" id="reportListLayoutRow" class="OutsetBoxHeaderRow">
                                                        <td runat="server" id="ReportItemPlaceHolder" >
                                                             
                                                        </td>
                                                    </tr>
                                                </GroupTemplate>
                                                <ItemTemplate>
                                                    <td style="border-width:thick;">
                                                        <asp:Button  ID="ReportSelectButton" runat="server" class="documentSelectButton" CommandName="RunReport" OnCommand="ReportSelectButton_OnCommand" CommandArgument='<%# Container.DataItemIndex + "," + Container.DisplayIndex + "," + Eval( "ReportName" ) + "," + Eval( "ReportExecutionPath" ) %>' 
                                                                                  OnClientClick="aspnetForm.target ='_blank';"  Width="300px" Text='<%# Eval( "ReportName" ) %>'   />                                                     
                                                    </td>
                                                </ItemTemplate>

                                                <EditItemTemplate>
                                                    <td>
                      
                                                    </td>                      
                                                </EditItemTemplate>

                                                <InsertItemTemplate>
                                                   <td>
                                                
                                                    </td>         
                                                </InsertItemTemplate>
          
                                            </ep:ListView>
                                       
                                </div>
                            </td>
                        </tr>
                    </table>          
                </td> 
            </tr>
        </table>
    </asp:Panel>
</asp:Content>
