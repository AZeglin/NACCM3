<%@ Page Title="" Language="C#" MasterPageFile="~/ContractSearch.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractSelectBody.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractSelectBody" %>
<%@ MasterType VirtualPath="~/ContractSearch.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ContractSearchContent" ContentPlaceHolderID="ContractSearchContentPlaceHolder" runat="server">

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
                        <input id="ContextMenuRowIndex_SearchGridView" runat="server" type="hidden" value="-1" enableviewstate="true" />

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
                                <ep:ContextMenu ID="SearchContextMenu" runat="server" ActivateOnRightClick="True" RolloverColor="#E5FDDF"  ForeColor="Black" BackColor="White" IsContextMenuInUpdatePanel="True" >
                                    <ContextMenuItems>
                                    </ContextMenuItems>
                                </ep:ContextMenu>
                                 <ep:GridView ID="SearchGridView" 

                                            runat="server" 
                                            EnableViewState="true"
                                            DataKeyNames="Contract_Record_ID, Schedule_Number"  
                                            AutoGenerateColumns="False" 
                                            Width="100%" 
                                            RowStyle-Height="36px"
                                            CssClass="CMGrid" 
                                            Visible="True" 
                                            AllowSorting="True" 
                                            OnSorting="SearchGridView_OnSorting"
                                            OnRowDataBound="SearchGridView_RowDataBound"
                                            OnRowCommand="SearchGridView_OnRowCommand"
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="CMGridEditRowStyle" 
                                            onprerender="SearchGridView_PreRender" 
                                            OnDataBound="SearchGridView_OnDataBound"
                                            OnInit="SearchGridView_Init"
                                            EmptyDataRowStyle-CssClass="CMGrid" 
                                            EmptyDataText="There are no contracts matching the selected filter criteria."
                                            ContextMenuID="SearchContextMenu"
                                           >
                                        <HeaderStyle CssClass="SearchGridHeaders" />
                                        <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="CMGridAltItems" />
                                        <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                            
                                            <asp:TemplateField  ControlStyle-Width="126px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractNumberColumnHeaderButton" Text="Contract Number" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="CntrctNum" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Button  ID="SelectContractButton" runat="server" class="documentSelectButton" UseSubmitBehavior="false" CommandName="EditSearchContract" CommandArgument='<%# Container.DataItemIndex + "," + Eval( "Contract_Record_ID" ) + "," + Eval( "CntrctNum" ) + "," + Eval( "Schedule_Number" ) %>' 
                                                                                Width="124px" Text='<%# Eval( "CntrctNum" ) %>'  />                                          
                                                </ItemTemplate>
                                            </asp:TemplateField>      

                                            <asp:BoundField DataField="Schedule_Name" HeaderText="Schedule" 
                                                SortExpression="Schedule_Name"  />
                                                                                       
                                            <asp:BoundField DataField="CO_Name" HeaderText="CO" 
                                                SortExpression="CO_Name" />
                                           
                                            <asp:BoundField DataField="Contractor_Name" HeaderText="Contractor Name" 
                                                SortExpression="Contractor_Name" />
                                           
                                            <asp:BoundField DataField="Drug_Covered" HeaderText="Commodity Covered" 
                                                SortExpression="Drug_Covered" ControlStyle-Width="190px"   />
                                           
                                            <asp:BoundField DataField="Dates_CntrctAward" HeaderText="Dates Awarded" 
                                                SortExpression="Dates_CntrctAward" DataFormatString="{0:d}" />
                                              
                                            <asp:TemplateField  ControlStyle-Width="80px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ExpirationDateColumnHeaderButton" Text="Expiration Date" OnCommand="SearchGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_CntrctExp" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <table  >
                                                        <tr>
                                                            <td  style="border:none;">
                                                                <asp:Label runat="server" ID="ContractExpirationDate"  OnDataBinding="ContractExpirationDate_OnDataBinding" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td  style="border:none;">
                                                                <asp:Label runat="server" ID="ContractCancellationDate" Font-Size="Smaller" ForeColor="Red" OnDataBinding="CompletionDate_OnDataBinding" /> 
                                                            </td>
                                                        </tr>
                                                    </table>
                                                  
                                                </ItemTemplate>
                                             </asp:TemplateField>      
    
                    
                                             <asp:BoundField DataField="HasBPA" HeaderText="Has BPA"  />


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
    <ep:MsgBox ID="MsgBox" NameofMsgBox="ContractSelectBody" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />

</asp:Panel>
</ContentTemplate> 
</asp:UpdatePanel> 
</asp:Content>

