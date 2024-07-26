<%@ Page Title="" Language="C#"  StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="EditContractSales.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.EditContractSales" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %> 


<!DOCTYPE html>
<html>

<head runat="server">
    <title></title>


     <script type="text/javascript" >

         function CloseWindow(withRefresh) {
             if (withRefresh == "true") {
                 window.opener.document.forms[0].RefreshSalesSummaryScreenOnSubmit.value = withRefresh;
                 window.opener.document.forms[0].submit();
             }
             top.window.opener = top;
             top.window.open('', '_parent', '');
             top.window.close();
         }
 
         /* called on grid div scroll */
         function setEditSalesScrollForRestore(divToScroll) {

             if (divToScroll != "0") {
                 $get("editSalesScrollPos").value = divToScroll.scrollTop;
             }
         }


         function presentConfirmationMessage(msg) {
             $get("confirmationMessageResults").value = confirm(msg);
         }

         function presentPromptMessage(msg) {
             $get("promptMessageResults").value = prompt(msg, "");
         }

         function pageLoad(sender, args) {
             RestoreEditSalesGridSelectionOnAsyncPostback();
         }

         function RestoreEditSalesGridSelectionOnAsyncPostback() {
             var editSalesScrollPos = $get("editSalesScrollPos").value;
             var highlightedEditSalesRow = $get("highlightedEditSalesRow").value;

             RestoreEditSalesGridSelection(editSalesScrollPos, highlightedEditSalesRow);
         }

         /* called from form load */
         function RestoreEditSalesGridSelection(editSalesScrollPos, highlightedEditSalesRow) {
             $get("editSalesScrollPos").value = editSalesScrollPos;
             if (editSalesScrollPos) {
                 if (editSalesScrollPos >= 0) {

                     var theEditSalesDiv = document.getElementById('<%#EditSalesGridViewDiv.ClientID %>');   /* = to # */
                     if (theEditSalesDiv) {
                         theEditSalesDiv.scrollTop = editSalesScrollPos;
                     }
                 }
             }

             if (highlightedEditSalesRow) {
                 if (highlightedEditSalesRow >= 0) {
                     $get("highlightedEditSalesRow").value = highlightedEditSalesRow;
                     highlightEditSalesRow();
                 }
             }
         }

         function setEditSalesHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
             $get("highlightedEditSalesRow").value = rowIndex;
             $get("highlightedEditSalesRowOriginalColor").value = originalColor;
             highlightEditSalesRow();

         }

         function highlightEditSalesRow() {

             var selectedRowIndex = $get("highlightedEditSalesRow").value;
             if (selectedRowIndex < 0) {
                 return;
             }

             var rebateGridView = document.getElementById("<%#EditSalesGridView.ClientID%>"); /* ok */ /* = to # */
             var currentSelectedRow = null;
             if (rebateGridView) {
                 currentSelectedRow = rebateGridView.rows[selectedRowIndex];   /* ok */
             }
             if (currentSelectedRow) {
                 currentSelectedRow.style.backgroundColor = '#E3FBDD';
                 currentSelectedRow.className = 'CMGridSelectedCellStyle';
             }

         }

         function unhighlightEditSalesRow() {

             var selectedRowIndex = $get("highlightedEditSalesRow").value;
             var highlightedEditSalesRowOriginalColor = $get("highlightedEditSalesRowOriginalColor").value;

             if (selectedRowIndex < 0) {
                 return;
             }

             $get("highlightedEditSalesRow").value = -1;
             var rebateGridView = document.getElementById("<%#EditSalesGridView.ClientID%>"); /* = to # */
             var currentSelectedRow = null;
             if (rebateGridView) {
                 currentSelectedRow = rebateGridView.rows[selectedRowIndex];
             }

             if (currentSelectedRow) {
                 if (highlightedEditSalesRowOriginalColor == 'alt') {
                     currentSelectedRow.style.backgroundColor = 'white';
                     currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
                 }
                 else if (highlightedEditSalesRowOriginalColor == 'norm') {
                     currentSelectedRow.style.backgroundColor = '#F7F6F3';
                     currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
                 }
             }
         }

         /* called from onclick */
         function resetEditSalesHighlighting(sourceGridName, rowIndex, rowColor) {
             if (sourceGridName == "EditSalesGridView") {

                 unhighlightEditSalesRow();

                 $get("highlightedEditSalesRow").value = rowIndex;
                 $get("highlightedEditSalesRowOriginalColor").value = rowColor;

                 highlightEditSalesRow();
             }
         }

</script>


</head>
<body>
    <form id="EditSalesForm" name="EditSalesForm" runat="server" style="position:absolute; top:0px; left:0px; width:100%; height:100%;" >

    <asp:ScriptManager ID="EditSalesScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="EditSalesScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>


   <table style="height: 100%; width:100%; table-layout:fixed; text-align:left; border:solid 1px black;" >
        <colgroup> 
            <col style="width:100%;" />
        </colgroup>
        <tr  style="height:20%; vertical-align:top;">
          <td style="vertical-align:top;" >
            <div id="EditSalesHeaderDiv" style="width:99%; height:108px; border:solid 1px black; background-color:White; margin:3px;" >
                <ep:UpdatePanelEventProxy ID="UpdateHeaderUpdatePanelEventProxy" runat="server" />

                <asp:UpdatePanel ID="EditSalesHeaderUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="UpdateHeaderUpdatePanelEventProxy" EventName="ProxiedEvent" />
                </Triggers>
                <ContentTemplate>                
                    <table style="border:none; height:106px; width:100%; table-layout:fixed;  text-align:center; "  >
                        <tr>
                            <td colspan="3">
                                <asp:FormView ID="EditSalesHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="EditSalesHeaderFormView_OnPreRender" >
                                    <EditItemTemplate>
                                        <table style="border:none; width:100%; padding: 0px 0px 0px 0px; border-spacing: 0px 0px;" >
                                            <tr class="OutsetBoxHeaderRow" >
                                                <td style="width:190px;" ></td>
                                                <td  style="text-align:center; width:100%;" >
                                                    <asp:Label ID="EditSalesHeaderFormViewHeaderLabel1" runat="server" Text='<%# FormatHeaderLabel1() %>' CssClass="HeaderText"  />
                                                </td>
                                                <td style="width:190px; text-align:right; padding-right:12px;" >
                                                    <asp:Button ID="CloseEditSalesButton" runat="server" Height="20px" Text="Close" OnClick="CloseEditSalesButton_OnClick" />
                                                </td>
                                            </tr>
                                            <tr>
                                                 <td style="width:190px;" ></td>
                                                 <td style="padding-top:10px; text-align:center; width:100%;" >
                                                    <asp:Label ID="EditSalesHeaderFormViewHeaderLabel2" Height="24px" runat="server" Text='<%# FormatContractNumber( DataBinder.Eval( Container.DataItem, "ContractNumber" ))%>'  CssClass="HeaderText"  />
                                                </td>
                                                <td style="width:190px;" >
                                                </td>
                                            </tr>
                                            <tr>
                                                 <td style="width:190px;" ></td>
                                                 <td  style="text-align:center; width:100%;" >
                                                    <asp:Label ID="EditSalesHeaderFormViewHeaderLabel3" Height="24px" runat="server" Text='<%# FormatYearQuarter( DataBinder.Eval( Container.DataItem, "Year" ), DataBinder.Eval( Container.DataItem, "Quarter" ))%>' CssClass="HeaderText"  />
                                                </td>
                                                <td style="width:190px;" >
                                                </td>
                                            </tr>
                                        </table>
                                    </EditItemTemplate>
                                </asp:FormView>
                            </td>
                        </tr>
                        <tr style="vertical-align:top;">
                            <td style="width:190px;" ></td>
                            <td style="padding-bottom:8px; text-align:center; vertical-align:top; width:100%; height:24px; ">  
                                <asp:Label ID="EditSalesYearQuarterLabel" Height="22px" runat="server" CssClass="HeaderText" Text="Select Year/Qtr to Add:" />                   
                                <asp:DropDownList ID="EditSalesYearQuarterDropDownList" Height="22px" runat="server" CssClass="HeaderText" AutoPostBack="true" OnSelectedIndexChanged="EditSalesYearQuarterDropDownList_OnSelectedIndexChanged"  />
                            </td>
                            <td style="width:190px;" >
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td></td>
                            <td></td>
                        </tr>
                    </table>       
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </td>
      </tr>
      <tr style="border: solid 1px black; height:76%; vertical-align:top;">
            <td style="vertical-align:top;" >
  
                <asp:Panel ID="EditSalesGridPanel" runat="server" Width="99%" Height="99%"  OnPreRender="EditSalesGridPanel_OnPreRender" >
                    <div id="EditSalesPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="editSalesScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedEditSalesRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedEditSalesRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    </div>
                                    
                    <div id="EditSalesGridViewDiv"  runat="server"  style="border:1px solid black; width:986px; height:364px; overflow: scroll" onscroll="javascript:setEditSalesScrollForRestore( this );"  onkeypress="javascript:setEditSalesScrollForRestore( this );"  >
                          <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="EditSalesGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                                <ep:GridView ID="EditSalesGridView" 
                                    runat="server" 
                                    DataKeyNames="ExternalSalesId,SalesId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="CMGrid" 
                                    Visible="True" 
                                    OnRowDataBound="EditSalesGridView_RowDataBound"
                                    AllowSorting="True" 
                                    AutoGenerateEditButton="false"
                                    EditRowStyle-CssClass="CMGridEditRowStyle"                
                                    OnRowCreated="EditSalesGridView_OnRowCreated"
                                    OnRowEditing="EditSalesGridView_RowEditing" 
                                    OnRowCancelingEdit="EditSalesGridView_RowCancelingEdit"
                                    AllowInserting="True"
                                    InsertCommandColumnIndex="1"
                                    EmptyDataRowStyle-CssClass="CMGrid" 
                                    EmptyDataText="There are no SINs defined for the selected contract."
                                    ContextMenuID="ItemContextMenu"
                                    Font-Names="Arial" 
                                    Font-Size="small" >
                                <HeaderStyle CssClass="EditSalesGridHeaders" />
                                <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="CMGridAltItems" />
                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                <Columns>   
                                                                            
                                        <asp:TemplateField >  
                                            <ItemTemplate>
                                                    <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="EditSalesGridView_ButtonCommand"   CommandName="EditSales"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ExternalSalesId")  %>'   Width="58px"   >
                                                    </asp:Button>   

                                                    <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="EditSalesGridView_ButtonCommand"   CommandName="SaveSales" CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ExternalSalesId") %>'   Width="58px"   >
                                                    </asp:Button>   

                                                    <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="EditSalesGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ExternalSalesId")  %>'  Width="58px"   >
                                                    </asp:Button>   
                                  
                                            </ItemTemplate>
                                        </asp:TemplateField>      
                                            
 
                                    <asp:TemplateField HeaderText="SIN"  >
                                    <ItemTemplate>
                                        <asp:Label ID="SINLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SIN" ) %>'  >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:Label ID="SINLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SIN" ) %>'  >
                                        </asp:Label>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="VA Sales"  >
                                    <ItemTemplate>
                                        <asp:Label ID="VASalesLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "VASales", "{0:c}"  ) %>' >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                         <ep:TextBox ID="VASalesTextBox" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "VASales", "{0:c}"  ) %>' >
                                        </ep:TextBox>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="OGA Sales"  >
                                    <ItemTemplate>
                                        <asp:Label ID="OGASalesLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "OGASales", "{0:c}" ) %>' >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                         <ep:TextBox ID="OGASalesTextBox" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "OGASales", "{0:c}" ) %>' >
                                        </ep:TextBox>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="S/C/L Govt. Sales"  >
                                    <ItemTemplate>
                                        <asp:Label ID="SLGSalesLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SLGSales", "{0:c}" ) %>' >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                         <ep:TextBox ID="SLGSalesTextBox" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SLGSales", "{0:c}" ) %>' >
                                        </ep:TextBox>
                                    </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                    <asp:TemplateField HeaderText="Comments"   >
                                        <ItemTemplate>
                                            <asp:Label ID="salesCommentsLabel" runat="server"  Width="400px"  Text='<%# DataBinder.Eval( Container.DataItem, "Comments" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="salesCommentsTextBox" runat="server"  Width="400px" MaxLength="255" TextMode="MultiLine" Text='<%# DataBinder.Eval( Container.DataItem, "Comments" )%>'  ></ep:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                          
                                    <asp:TemplateField HeaderText="Last Modified By"   >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModifiedByLabel" runat="server"  Width="140px"  Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModifiedByLabel" runat="server" Width="140px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                                                             
                                    <asp:TemplateField HeaderText="Last Modification Date"   >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModificationDateLabel" runat="server"  Width="70px" Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModificationDateLabel" runat="server" Width="70px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
    
                                                     
                                    <asp:TemplateField HeaderText="ExternalSalesId"  >
                                        <ItemTemplate>
                                            <asp:Label ID="EditExternalSalesIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ExternalSalesId" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="EditExternalSalesIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ExternalSalesId" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
    
                                                     
                                    <asp:TemplateField HeaderText="SalesId"  >
                                        <ItemTemplate>
                                            <asp:Label ID="EditSalesIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SalesId" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="EditSalesIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "SalesId" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="IsNew"   >
                                        <ItemTemplate>
                                            <asp:Label ID="EditSalesIsNewLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "IsNew" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="EditSalesIsNewLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IsNew" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                </Columns>
                            
                                </ep:GridView>
                            </ContentTemplate>                                              
                        </asp:UpdatePanel>
                    </div>
 
               </asp:Panel>
            </td>                           
        </tr>
        <tr>
  
        </tr>
    </table>    

        <ep:MsgBox ID="MsgBox" NameofMsgBox="EditContractSales" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />  
    </form>
</body>
</html>
