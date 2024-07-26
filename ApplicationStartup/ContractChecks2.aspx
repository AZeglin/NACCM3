<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" EnableEventValidation="false"  CodeBehind="ContractChecks2.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractChecks2" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>


<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>


<asp:Content ID="ContractCheckContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

 <script type="text/javascript" >
     /* called on grid div scroll */
     function setCheckScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("CheckScrollPos").value = divToScroll.scrollTop;
         }
     }


     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestoreCheckGridSelectionOnAsyncPostback();
     }

     function RestoreCheckGridSelectionOnAsyncPostback() {
         var CheckScrollPos = $get("CheckScrollPos").value;
         var highlightedCheckRow = $get("highlightedCheckRow").value;

         RestoreCheckGridSelection(CheckScrollPos, highlightedCheckRow);
     }

     /* called from form load */
     function RestoreCheckGridSelection(CheckScrollPos, highlightedCheckRow) {
         $get("CheckScrollPos").value = CheckScrollPos;
         if (CheckScrollPos) {
             if (CheckScrollPos >= 0) {

                 var theCheckDiv = document.getElementById('<%=CheckGridViewDiv.ClientID %>');
                 if (theCheckDiv) {
                     theCheckDiv.scrollTop = CheckScrollPos;
                 }
             }
         }

         if (highlightedCheckRow) {
             if (highlightedCheckRow >= 0) {
                 $get("highlightedCheckRow").value = highlightedCheckRow;
                 highlightCheckRow();
             }
         }
     }

     function setCheckHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedCheckRow").value = rowIndex;
         $get("highlightedCheckRowOriginalColor").value = originalColor;
         highlightCheckRow();

     }

     function highlightCheckRow() {

         var selectedRowIndex = $get("highlightedCheckRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var CheckGridView = document.getElementById("<%=CheckGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (CheckGridView) {
             currentSelectedRow = CheckGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightCheckRow() {

         var selectedRowIndex = $get("highlightedCheckRow").value;
         var highlightedCheckRowOriginalColor = $get("highlightedCheckRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedCheckRow").value = -1;
         var CheckGridView = document.getElementById("<%=CheckGridView.ClientID%>");
         var currentSelectedRow = null;
         if (CheckGridView) {
             currentSelectedRow = CheckGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedCheckRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedCheckRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetCheckHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "CheckGridView") {

             unhighlightCheckRow();

             $get("highlightedCheckRow").value = rowIndex;
             $get("highlightedCheckRowOriginalColor").value = rowColor;

             highlightCheckRow();
         }
     }

</script>

<ep:UpdatePanelEventProxy ID="CheckUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="CheckUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="CheckUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

   <table class="OuterTable" >
        <tr>
          <td style="vertical-align:top;" >
                <asp:FormView ID="ChecksHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="ChecksHeaderFormView_OnPreRender" >
                    <EditItemTemplate>
                    <table class="OutsetBox" style="width: 100%;">
                            <tr class="OutsetBoxHeaderRow" >
                                <td  style="text-align:center;" colspan="4" >
                                    <asp:Label ID="ChecksHeaderFormViewHeaderLabel" runat="server" Text="Checks"  />
                                </td>
                            </tr>
 
                            <tr>
                                <td>                                                      
                                </td>
                                <td style="text-align:left;" >
                                    <asp:Button runat="server" ID="AddCheckButton" Text="Add Check" OnClick="AddNewCheckButton_OnClick"  />
                                </td>
                                <td style="text-align:right;">
                                    <asp:Button runat="server" ID="ViewIFFSalesComparisonButton"  Width="200px" Text="View IFF/Payment Comparison" OnClick="ViewIFFSalesComparisonButton_OnClick"  />
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td></td>
                                <td></td>
                                <td></td>
                            </tr>
                        </table>
                        </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td style="vertical-align:top;" >
  
                <asp:Panel ID="CheckGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="CheckGridPanel_OnPreRender" >
                    <div id="CheckPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="CheckScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedCheckRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedCheckRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                  </div>
                    

                    
                    <div id="CheckGridViewDiv"  runat="server"  style="border:3px solid black; height:398px; overflow: scroll" onscroll="javascript:setCheckScrollForRestore( this );"  onkeypress="javascript:setCheckScrollForRestore( this );"  >

                        <ep:GridView ID="CheckGridView" 
                                    runat="server" 
                                    DataKeyNames="CheckId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="CMGrid" 
                                    Visible="True" 
                                    onrowcommand="CheckGridView_RowCommand" 
                                    OnSelectedIndexChanged="CheckGridView_OnSelectedIndexChanged" 
                                    OnRowDataBound="CheckGridView_RowDataBound"
                                    AllowSorting="True" 
                                    AutoGenerateEditButton="false"
                                    EditRowStyle-CssClass="CMGridEditRowStyle" 
                                    onprerender="CheckGridView_PreRender" 
                                    OnInit="CheckGridView_Init"
                                    OnRowCreated="CheckGridView_OnRowCreated"
                                    OnRowDeleting="CheckGridView_RowDeleting" 
                                    OnRowEditing="CheckGridView_RowEditing" 
                                    OnRowUpdating="CheckGridView_RowUpdating" 
                                    OnRowCancelingEdit="CheckGridView_RowCancelingEdit"
                                    AllowInserting="True"
                                    OnRowInserting="CheckGridView_RowInserting" 
                                    InsertCommandColumnIndex="1"
                                    EmptyDataRowStyle-CssClass="CMGrid" 
                                    EmptyDataText="There are no checks for the selected contract."
                                    ContextMenuID="ItemContextMenu"
                                    Font-Names="Arial" 
                                    Font-Size="small" >
                                <HeaderStyle CssClass="CheckGridHeaders" />
                                <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="CMGridAltItems" />
                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                <Columns>   
                                                                            
                                                            
                                        <asp:TemplateField >  
                                        <ItemTemplate>
                                                <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="CheckGridView_ButtonCommand"   CommandName="EditCheck"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("CheckId")  %>' ButtonType="Button"  Width="62px"   >
                                                </asp:Button>   

                                                <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="CheckGridView_ButtonCommand"   CommandName="SaveCheck" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("CheckId") %>' ButtonType="Button"  Width="62px"   >
                                                </asp:Button>   

                                                <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="CheckGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("CheckId") %>' ButtonType="Button"  Width="62px"   >
                                                </asp:Button>   
                                  
                                        </ItemTemplate>
                                    </asp:TemplateField>      
                                            
                                    <asp:TemplateField HeaderText="Year Quarter"  ItemStyle-Width="10%" >
                                    <ItemTemplate>
                                        <asp:Label ID="yearQuarterLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "YearQuarterDescription" )%>'   CssClass="rightAlign" >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="yearQuarterDropDownList" DataValueField="Quarter_ID"  Width="120px"    DataTextField="YearQuarterDescription" runat="server" OnDataBound="yearQuarterDropDownList_DataBound" OnSelectedIndexChanged="yearQuarterDropDownList_OnSelectedIndexChanged"  AutoPostBack="true"  CssClass="rightAlign" >
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Check Amount" ItemStyle-Width="10%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="checkAmountLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "CheckAmount", "{0:0.00}" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="checkAmountTextBox" runat="server"  Width="94%"  Text='<%# DataBinder.Eval( Container.DataItem, "CheckAmount", "{0:0.00}" )%>'  CssClass="rightAlign" > </ep:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                            
                                        <asp:TemplateField HeaderText="Check Number" ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="checkNumberLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "CheckNumber" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="checkNumberTextBox" runat="server"  Width="94%"  Text='<%# DataBinder.Eval( Container.DataItem, "CheckNumber" )%>'   CssClass="rightAlign" ></ep:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
 

                                    <asp:TemplateField  HeaderText="Deposit Ticket Number"  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="depositTicketNumberLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "DepositTicketNumber" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="depositTicketNumberTextBox" runat="server"  Width="94%"  Text='<%# DataBinder.Eval( Container.DataItem, "DepositTicketNumber" )%>'  CssClass="rightAlign" ></ep:TextBox>                                     
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                         

                                                           
                                    <asp:TemplateField HeaderText="Date Received"  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="dateReceivedLabel" runat="server"  Width="99%"  Text='<%#  DataBinder.Eval( Container.DataItem, "DateReceived", "{0:d}" ) %>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="dateReceivedTextBox" runat="server"  Width="94%"  Text='<%# DataBinder.Eval( Container.DataItem, "DateReceived", "{0:d}" )%>'   CssClass="rightAlign" ></ep:TextBox>
                                          </EditItemTemplate>
                                     </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Settlement Date"  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="settlementDateLabel" runat="server"  Width="99%"  Text='<%#  DataBinder.Eval( Container.DataItem, "SettlementDate", "{0:d}" ) %>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="settlementDateTextBox" runat="server"  Width="94%"  Text='<%# DataBinder.Eval( Container.DataItem, "SettlementDate", "{0:d}" )%>'   CssClass="rightAlign" ></ep:TextBox>
                                          </EditItemTemplate>
                                     </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Check Comments"  ItemStyle-Width="35%" ItemStyle-Wrap="true" >
                                        <ItemTemplate>
                                            <asp:Label ID="checkCommentsLabel" runat="server"  Width="99%"  Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "CheckComments" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <ep:TextBox ID="checkCommentsTextBox" runat="server" TextMode="MultiLine"  MaxLength="255"  Width="99%" Text='<%# DataBinder.Eval( Container.DataItem, "CheckComments" )%>'  ></ep:TextBox>                                     
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                  

                                    <asp:TemplateField >
                                        <ItemTemplate>                                                                                         
                                                <asp:Button runat="server"  ID="RemoveCheckButton" Text="Remove Check" OnDataBinding="RemoveCheckButton_DataBinding"  OnCommand="CheckGridView_ButtonCommand" CommandName="RemoveCheck" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("CheckId") %>' ButtonType="Button" Width="104px" >            
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Button runat="server"  ID="RemoveCheckButton" Text="Remove Check" OnDataBinding="RemoveCheckButton_DataBinding"  OnCommand="CheckGridView_ButtonCommand" CommandName="RemoveCheck" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("CheckId") %>' ButtonType="Button" Width="104px" >            
                                                </asp:Button >                                    
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField  HeaderText="Check Id"   >
                                        <ItemTemplate>
                                            <asp:Label ID="checkIdLabel" runat="server"  Width="160px"  Text='<%# DataBinder.Eval( Container.DataItem, "CheckId" )%>'  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="checkIdLabel" runat="server"  Width="160px"  Text='<%# DataBinder.Eval( Container.DataItem, "CheckId" )%>'  ></asp:Label>                                          
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            
                            </ep:GridView>                                              
                    </div>
 
               </asp:Panel>
            </td>                           
        </tr>
    </table>

    
</ContentTemplate>
</asp:UpdatePanel>

</asp:Content>
