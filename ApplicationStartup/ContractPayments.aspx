<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" EnableEventValidation="false"  CodeBehind="ContractPayments.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractPayments" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>


<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>


<asp:Content ID="ContractPaymentContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

 <script type="text/javascript" >
     /* called on grid div scroll */
     function setPaymentScrollForRestore(divToScroll) {

         if (divToScroll != "0") { 
             $get("PaymentScrollPos").value = divToScroll.scrollTop;
         }
     }


     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestorePaymentGridSelectionOnAsyncPostback();
     }

     function RestorePaymentGridSelectionOnAsyncPostback() {
         var PaymentScrollPos = $get("PaymentScrollPos").value;
         var highlightedPaymentRow = $get("highlightedPaymentRow").value;

         RestorePaymentGridSelection(PaymentScrollPos, highlightedPaymentRow);
     }

     /* called from form load */
     function RestorePaymentGridSelection(PaymentScrollPos, highlightedPaymentRow) {
         $get("PaymentScrollPos").value = PaymentScrollPos;
         if (PaymentScrollPos) {
             if (PaymentScrollPos >= 0) {

                 var thePaymentDiv = document.getElementById('<%=PaymentGridViewDiv.ClientID %>');
                 if (thePaymentDiv) {
                     thePaymentDiv.scrollTop = PaymentScrollPos;
                 }
             }
         }

         if (highlightedPaymentRow) {
             if (highlightedPaymentRow >= 0) {
                 $get("highlightedPaymentRow").value = highlightedPaymentRow;
                 highlightPaymentRow();
             }
         }
     }

     function setPaymentHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedPaymentRow").value = rowIndex;
         $get("highlightedPaymentRowOriginalColor").value = originalColor;
         highlightPaymentRow();

     }

     function highlightPaymentRow() {

         var selectedRowIndex = $get("highlightedPaymentRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var PaymentGridView = document.getElementById("<%=PaymentGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (PaymentGridView) {
             currentSelectedRow = PaymentGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightPaymentRow() {

         var selectedRowIndex = $get("highlightedPaymentRow").value;
         var highlightedPaymentRowOriginalColor = $get("highlightedPaymentRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedPaymentRow").value = -1;
         var PaymentGridView = document.getElementById("<%=PaymentGridView.ClientID%>");
         var currentSelectedRow = null;
         if (PaymentGridView) {
             currentSelectedRow = PaymentGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedPaymentRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedPaymentRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetPaymentHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "PaymentGridView") {

             unhighlightPaymentRow();

             $get("highlightedPaymentRow").value = rowIndex;
             $get("highlightedPaymentRowOriginalColor").value = rowColor;

             highlightPaymentRow();
         }
     }

</script>

<ep:UpdatePanelEventProxy ID="PaymentUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="PaymentUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="PaymentUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

   <table class="OuterTable" >
        <tr>
          <td style="vertical-align:top;" >
                <asp:FormView ID="PaymentsHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="PaymentsHeaderFormView_OnPreRender" >
                    <EditItemTemplate>
                    <table class="OutsetBox" style="width: 100%;">
                            <tr class="OutsetBoxHeaderRow" >
                                <td  style="text-align:center;" colspan="4" >
                                    <asp:Label ID="PaymentsHeaderFormViewHeaderLabel" runat="server" Text="Payments"  />
                                </td>
                            </tr>
 
                            <tr>
                                <td>                                                      
                                </td>
                                <td>                                                      
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
  
                <asp:Panel ID="PaymentGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="PaymentGridPanel_OnPreRender" >
                    <div id="PaymentPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="PaymentScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedPaymentRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedPaymentRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                  </div>
                    

                    
                    <div id="PaymentGridViewDiv"  runat="server"  style="border:3px solid black; height:398px; overflow: scroll" onscroll="javascript:setPaymentScrollForRestore( this );"  onkeypress="javascript:setPaymentScrollForRestore( this );"  >

                        <ep:GridView ID="PaymentGridView" 
                                    runat="server" 
                                    DataKeyNames="SRPActivityId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="CMGrid" 
                                    Visible="True" 
                                    onrowcommand="PaymentGridView_RowCommand" 
                                    OnSelectedIndexChanged="PaymentGridView_OnSelectedIndexChanged" 
                                    OnRowDataBound="PaymentGridView_RowDataBound"
                                    AllowSorting="True" 
                                    AutoGenerateEditButton="false"
                                    EditRowStyle-CssClass="CMGridEditRowStyle" 
                                    onprerender="PaymentGridView_PreRender" 
                                    OnInit="PaymentGridView_Init"
                                    OnRowCreated="PaymentGridView_OnRowCreated"                                    
                                    AllowInserting="False"                                   
                                    InsertCommandColumnIndex="1"
                                    EmptyDataRowStyle-CssClass="CMGrid" 
                                    EmptyDataText="There are no payments for the selected contract."
                                    ContextMenuID="ItemContextMenu"
                                    Font-Names="Arial" 
                                    Font-Size="small" >
                                <HeaderStyle CssClass="CheckGridHeaders" />
                                <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="CMGridAltItems" />
                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                <Columns>   
                                                                                                                                                                                                                           
                                    <asp:TemplateField HeaderText="Year Quarter"  ItemStyle-Width="6%" >
                                    <ItemTemplate>
                                        <asp:Label ID="yearQuarterLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "YearQuarterDescription" )%>'   CssClass="rightAlign" >
                                        </asp:Label>
                                    </ItemTemplate>
                                   
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Payment Amount" ItemStyle-Width="7%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="paymentAmountLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "PaymentAmount", "{0:0.00}" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
                                            
                                     <asp:TemplateField HeaderText="Submission<br />Date" ItemStyle-Width="4%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="submissionDateLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "SubmissionDate", "{0:d}" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
                                            
          
                                     <asp:TemplateField HeaderText="Submitted By" ItemStyle-Width="7%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="submittedByLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "SubmittedByUserName" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                         
                                    </asp:TemplateField>                                            

                                     <asp:TemplateField HeaderText="Payment <br />Method" ItemStyle-Width="3%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="paymentMethodLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "PaymentMethod" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                         
                                    </asp:TemplateField>   

                                     <asp:TemplateField HeaderText="Payment <br />Source" ItemStyle-Width="3%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="paymentSourceLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "PaymentSource" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                         
                                    </asp:TemplateField>   


                                     <asp:TemplateField HeaderText="Transaction Id" ItemStyle-Width="7%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="transactionIdLabel" runat="server"  Width="99%"  Text='<%# DataBinder.Eval( Container.DataItem, "TransactionId" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                         
                                    </asp:TemplateField>   

                                        <asp:TemplateField HeaderText="Tracking Id" ItemStyle-Width="5%" >
                                        <ItemTemplate>
                                            <asp:Label ID="trackingIdLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "PayGovTrackingId" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
 

                                    <asp:TemplateField  HeaderText="Deposit<br />Number"  ItemStyle-Width="4%" >
                                        <ItemTemplate>
                                            <asp:Label ID="depositNumberLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "DepositTicketNumber" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
                                         
                                    <asp:TemplateField  HeaderText="Debit Voucher<br />Number"  ItemStyle-Width="7%" >
                                        <ItemTemplate>
                                            <asp:Label ID="debitVoucherNumberLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "DebitVoucherNumber" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
                                                           
                                    <asp:TemplateField  HeaderText="Check Number"  ItemStyle-Width="7%" >
                                        <ItemTemplate>
                                            <asp:Label ID="checkNumberLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "CheckNumber" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>

                                                          
                                    <asp:TemplateField  HeaderText="Settlement<br />Date"  ItemStyle-Width="4%" >
                                        <ItemTemplate>
                                            <asp:Label ID="settlementDateLabel" runat="server" Width="99%"   Text='<%# DataBinder.Eval( Container.DataItem, "SettlementDate", "{0:d}" )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Payment Comments"  ItemStyle-Width="20%" ItemStyle-Wrap="true" >
                                        <ItemTemplate>
                                            <asp:Label ID="paymentCommentsLabel" runat="server"  Width="99%"  Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "Comments" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                  

                                     <asp:TemplateField  HeaderText="SRPActivityId"   >
                                        <ItemTemplate>
                                            <asp:Label ID="SRPActivityIdLabel" runat="server"  Width="160px"  Text='<%# DataBinder.Eval( Container.DataItem, "SRPActivityId" )%>'  ></asp:Label>
                                        </ItemTemplate>
                                       
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
