<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard"  AutoEventWireup="true" EnableEventValidation="false"  CodeBehind="ContractRebate.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractRebate" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ContractRebateContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

 <script type="text/javascript" >
     /* called on grid div scroll */
     function setRebateScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("rebateScrollPos").value = divToScroll.scrollTop;
         }
     }


     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestoreRebateGridSelectionOnAsyncPostback();
     }

     function RestoreRebateGridSelectionOnAsyncPostback() {
         var rebateScrollPos = $get("rebateScrollPos").value;
         var highlightedRebateRow = $get("highlightedRebateRow").value;
 
         RestoreRebateGridSelection(rebateScrollPos, highlightedRebateRow);
     }

     /* called from form load */
     function RestoreRebateGridSelection(rebateScrollPos, highlightedRebateRow) {
         $get("rebateScrollPos").value = rebateScrollPos;
         if (rebateScrollPos) {
             if (rebateScrollPos >= 0) {

                 var theRebateDiv = document.getElementById('<%=RebateGridViewDiv.ClientID %>');
                 if (theRebateDiv) {
                     theRebateDiv.scrollTop = rebateScrollPos;
                 }
             }
         }

         if (highlightedRebateRow) {
             if (highlightedRebateRow >= 0) {
                 $get("highlightedRebateRow").value = highlightedRebateRow;
                 highlightRebateRow();
             }
         }
     }

     function setRebateHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedRebateRow").value = rowIndex;
         $get("highlightedRebateRowOriginalColor").value = originalColor;
         highlightRebateRow();
        
     }

     function highlightRebateRow() {

         var selectedRowIndex = $get("highlightedRebateRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var rebateGridView = document.getElementById("<%=RebateGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (rebateGridView) {
             currentSelectedRow = rebateGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightRebateRow() {

         var selectedRowIndex = $get("highlightedRebateRow").value;
         var highlightedRebateRowOriginalColor = $get("highlightedRebateRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedRebateRow").value = -1;
         var rebateGridView = document.getElementById("<%=RebateGridView.ClientID%>");
         var currentSelectedRow = null;
         if (rebateGridView) {
             currentSelectedRow = rebateGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedRebateRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedRebateRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetRebateHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "RebateGridView") {

             unhighlightRebateRow();

             $get("highlightedRebateRow").value = rowIndex;
             $get("highlightedRebateRowOriginalColor").value = rowColor;

             highlightRebateRow();
         }
     }

     function trapBackspace(evt) {
         if (evt.which || evt.keyCode) {
             if ((evt.which == 8) || (evt.keyCode == 8)) {
                 if (evt.preventDefault)
                     evt.preventDefault();
                    return false;
                }
                return true;
            }
        }

</script>

<ep:UpdatePanelEventProxy ID="RebateUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="RebateUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="RebateUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

   <table class="OuterTable" >
        <tr>
          <td style="vertical-align:top;" >
                <asp:FormView ID="RebatesHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="RebatesHeaderFormView_OnPreRender" OnChange="RebatesHeaderFormView_OnChange" >
                    <EditItemTemplate>
                    <table class="OutsetBox" style="width: 100%;">
                            <tr class="OutsetBoxHeaderRow" >
                                <td  style="text-align:center;" colspan="3" >
                                    <asp:Label ID="RebatesHeaderFormViewHeaderLabel" runat="server" Text="Rebates"  />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td style="text-align:center;">
                                    <asp:CheckBox runat="server" ID="RebateRequiredCheckBox" Text="Rebate Information Is Required" Checked='<%#Bind("RebateRequired")%>' OnCheckedChanged="RebateRequiredCheckBox_OnCheckedChanged" AutoPostBack="true" />
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr>
                                <td>                                                      
                                </td>
                                <td>
                                    <asp:Button runat="server" ID="AddRebateButton" Text="Add Rebate" OnClick="AddNewRebateButton_OnClick"  />
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr>
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
  
                <asp:Panel ID="RebateGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="RebateGridPanel_OnPreRender" >
                    <div id="RebatePanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="rebateScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedRebateRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedRebateRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                  </div>
                    

                    
                    <div id="RebateGridViewDiv"  runat="server"  style="border:3px solid black; height:212px; overflow: scroll" onscroll="javascript:setRebateScrollForRestore( this );"  onkeypress="javascript:setRebateScrollForRestore( this );"  >

                        <ep:GridView ID="RebateGridView" 
                                    runat="server" 
                                    DataKeyNames="RebateId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="CMGrid" 
                                    Visible="True" 
                                    onrowcommand="RebateGridView_RowCommand" 
                                    OnSelectedIndexChanged="RebateGridView_OnSelectedIndexChanged" 
                                    OnRowDataBound="RebateGridView_RowDataBound"
                                    AllowSorting="True" 
                                    AutoGenerateEditButton="false"
                                    EditRowStyle-CssClass="CMGridEditRowStyle" 
                                    onprerender="RebateGridView_PreRender" 
                                    OnInit="RebateGridView_Init"
                                    OnRowCreated="RebateGridView_OnRowCreated"
                                    OnRowDeleting="RebateGridView_RowDeleting" 
                                    OnRowEditing="RebateGridView_RowEditing" 
                                    OnRowUpdating="RebateGridView_RowUpdating" 
                                    OnRowCancelingEdit="RebateGridView_RowCancelingEdit"
                                    AllowInserting="True"
                                    OnRowInserting="RebateGridView_RowInserting" 
                                    InsertCommandColumnIndex="1"
                                    EmptyDataRowStyle-CssClass="CMGrid" 
                                    EmptyDataText="There are no rebates for the selected contract."
                                    ContextMenuID="ItemContextMenu"
                                    Font-Names="Arial" 
                                    Font-Size="small" >
                                <HeaderStyle CssClass="RebateGridHeaders" />
                                <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="CMGridAltItems" />
                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                <Columns>   
                                                                            
                                        <asp:TemplateField  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Button runat="server"  ID="ViewRebateTextButton" Text="View Rebate Clause" OnDataBinding="ViewRebateTextButton_DataBinding" OnCommand="RebateGridView_ButtonCommand" CommandName="ViewRebateText" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("RebateId") + "," +  Eval("RebatePercentOfSales") + "," +  Eval("RebateThreshold") %>' ButtonType="Button" CssClass="multilineButtonText" ControlStyle-Width="112px" >            
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Button runat="server"  ID="ViewRebateTextButton" Text="View Rebate Clause" OnDataBinding="ViewRebateTextButton_DataBinding" OnCommand="RebateGridView_ButtonCommand" CommandName="ViewRebateText" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("RebateId") + "," +  Eval("RebatePercentOfSales") + "," +  Eval("RebateThreshold")  %>' ButtonType="Button" CssClass="multilineButtonText" ControlStyle-Width="112px" >            
                                                </asp:Button >                                    
                                        </EditItemTemplate>
                                        </asp:TemplateField>
                                                                        
                                                                                                          
                                        <asp:TemplateField  ItemStyle-Width="5%" > 
                                        <ItemTemplate>
                                                <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="RebateGridView_ButtonCommand"   CommandName="EditRebate"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("RebateId")  + "," +  Eval("RebatePercentOfSales") + "," +  Eval("RebateThreshold")  %>' ButtonType="Button"  ControlStyle-Width="60px"   >
                                                </asp:Button>   

                                                <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="RebateGridView_ButtonCommand"   CommandName="SaveRebate" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("RebateId") %>' ButtonType="Button"  ControlStyle-Width="60px"   >
                                                </asp:Button>   

                                                <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="RebateGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("RebateId") + "," +  Eval("RebatePercentOfSales") + "," +  Eval("RebateThreshold") %>' ButtonType="Button"  ControlStyle-Width="60px"   >
                                                </asp:Button>   
                                  
                                        </ItemTemplate>
                                    </asp:TemplateField>      
                                            
 
                                    <asp:TemplateField HeaderText="Start Year Quarter"  ItemStyle-Width="10%" >
                                    <ItemTemplate>
                                        <asp:Label ID="startYearQuarterLabel" Width="100px" runat="server" AutoPostBack="true" OnDataBinding="startYearQuarterLabel_OnDataBinding" >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="startYearQuarterDropDownList" DataValueField="Quarter_ID"  Width="120px"    DataTextField="YearQuarterDescription" runat="server" OnDataBound="startYearQuarterDropDownList_DataBound" OnSelectedIndexChanged="startYearQuarterDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="End Year Quarter"  ItemStyle-Width="10%" >
                                    <ItemTemplate>
                                        <asp:Label ID="endYearQuarterLabel" Width="100px" runat="server" AutoPostBack="true" OnDataBinding="endYearQuarterLabel_OnDataBinding" >
                                        </asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="endYearQuarterDropDownList" DataValueField="Quarter_ID"  Width="120px"   DataTextField="YearQuarterDescription" runat="server" OnDataBound="endYearQuarterDropDownList_DataBound" OnSelectedIndexChanged="endYearQuarterDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Percent of Sales"   ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="percentOfSalesLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "RebatePercentOfSales" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="percentOfSalesTextBox" runat="server"  Width="54px" Text='<%# DataBinder.Eval( Container.DataItem, "RebatePercentOfSales" )%>' > </ep:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                            
                                        <asp:TemplateField HeaderText="Threshold"  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="rebateThresholdLabel" runat="server"  Width="86px"  Text='<%# DataBinder.Eval( Container.DataItem, "RebateThreshold", "{0:0.00}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <ep:TextBox ID="rebateThresholdTextBox" runat="server"  Width="86px" Text='<%# DataBinder.Eval( Container.DataItem, "RebateThreshold", "{0:0.00}" )%>'  ></ep:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
 

                                    <asp:TemplateField  HeaderText="Rebate Clause Type"    ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="rebateClauseNameLabel" runat="server" Width="160px"  Text='<%# DataBinder.Eval( Container.DataItem, "StandardClauseName" )%>'  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="rebateClauseNameDropDownList" DataValueField="StandardRebateTermId"  Width="160px"  DataTextField="StandardClauseName" runat="server" OnDataBound="rebateClauseNameDropDownList_DataBound" OnSelectedIndexChanged="rebateClauseNameDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                        </asp:DropDownList>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                         
                                    <asp:TemplateField HeaderText="Last Modified By"  ItemStyle-Width="15%"  ItemStyle-Wrap="true" >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModifiedByLabel" runat="server"  Width="140px"  Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModifiedByLabel" runat="server" Width="140px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                                                             
                                    <asp:TemplateField HeaderText="Last Modification Date"  ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModificationDateLabel" runat="server"  Width="70px" Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModificationDateLabel" runat="server" Width="70px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField ItemStyle-Width="10%" >
                                        <ItemTemplate>
                                            <asp:Button runat="server"  ID="RemoveRebateButton" Text="Remove Rebate" OnDataBinding="RemoveRebateButton_DataBinding"  OnCommand="RebateGridView_ButtonCommand" CommandName="RemoveRebate" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("RebateId") %>' ButtonType="Button" CssClass="multilineButtonText" ControlStyle-Width="112px" >            
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Button runat="server"  ID="RemoveRebateButton" Text="Remove Rebate" OnDataBinding="RemoveRebateButton_DataBinding"  OnCommand="RebateGridView_ButtonCommand" CommandName="RemoveRebate" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("RebateId") %>' ButtonType="Button" CssClass="multilineButtonText" ControlStyle-Width="112px" >            
                                                </asp:Button >                                    
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                                      
                                        <asp:TemplateField HeaderText="Rebate Term Id"  >
                                        <ItemTemplate>
                                            <asp:Label ID="RebateTermIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "RebateTermId" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="RebateTermIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "RebateTermId" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Rebates Standard Rebate Term Id" >
                                        <ItemTemplate>
                                            <asp:Label ID="RebatesStandardRebateTermIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "RebatesStandardRebateTermId" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="RebatesStandardRebateTermIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "RebatesStandardRebateTermId" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            
                            </ep:GridView>                                              
                    </div>
 
               </asp:Panel>
            </td>                           
        </tr>
        <tr>
          <td style="vertical-align:top;" >
    
                <table class="OutsetBox" style="width: 100%;">
 
                    <tr>
                        <td>
                        </td>
                        <td>
                            <asp:FormView ID="RebatesFooterDateFormView" runat="server" Width="100%" Height="100%" DefaultMode="ReadOnly"  OnPreRender="RebatesFooterDateFormView_OnPreRender" OnDataBinding="RebatesFooterDateFormView_OnDataBinding" >
                                <ItemTemplate>
                                    <table>
                                        <tr>
                                            <td align="left">
                                                <asp:Label runat="server" ID="CustomStartDateLabel" Text="Custom Start Date" Font-Bold="true"  />
                                            </td>
                                            <td>
                                                    <ep:TextBox ID="CustomStartDateTextBox" ClientIDMode="Static" ReadOnly="true"  runat="server" MaxLength="10" Width="100px" Text='<%# Bind("CustomRebateStartDate")%>' ></ep:TextBox>
                                            </td>
                                        </tr>
                                    </table>    
                                </ItemTemplate>   
                                <EditItemTemplate>
                                    <table>
                                        <tr>
                                            <td align="left">
                                                <asp:Label runat="server" ID="CustomStartDateLabel" Text="Custom Start Date" Font-Bold="true"  />
                                            </td>
                                            <td>
                                                    <ep:TextBox ID="CustomStartDateTextBox" ClientIDMode="Static"  runat="server" MaxLength="10" Width="100px" Text='<%# Bind("CustomRebateStartDate")%>' ></ep:TextBox>
                                            </td>
                                        </tr>
                                    </table>       
                                </EditItemTemplate>
                            </asp:FormView>        
                                                                          
                        </td>
                        <td>                           
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td style="text-align:center;">
                            <asp:Label runat="server" ID="RebateClauseHeaderLabel" Text="Rebate Clause" Font-Bold="true"  />
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>

                        <td>
                            <asp:FormView ID="RebatesFooterClauseFormView" runat="server" Width="100%" Height="100%" DefaultMode="ReadOnly"  OnPreRender="RebatesFooterClauseFormView_OnPreRender" OnDataBinding="RebatesFooterClauseFormView_OnDataBinding" >
                                <ItemTemplate>
                                    <div id="RebateClauseDiv" style="height:66px;"> 
                                        <ep:TextBox ID="RebateClauseTextBox" ClientIDMode="Static" runat="server"  ReadOnly="true" MaxLength="4000" TextMode="MultiLine" CssClass="CMGridMultilineInEditMode" Wrap="true"  ControlStyle-Width="99%"  Text='<%# Bind("RebateClause")%>' Height="46px" ></ep:TextBox>
                                    </div> 
                                </ItemTemplate>   
                                <EditItemTemplate>                                        
                                    <div id="RebateClauseDiv" style="height:66px;"> 
                                        <ep:TextBox ID="RebateClauseTextBox" ClientIDMode="Static" runat="server" MaxLength="4000" TextMode="MultiLine" CssClass="CMGridMultilineInEditMode" Wrap="true"  ControlStyle-Width="99%"  Text='<%# Bind("RebateClause")%>' Height="46px"  ></ep:TextBox>
                                    </div>                           
                                </EditItemTemplate>
                            </asp:FormView>        
                        </td>
                        <td>
                        </td>
                    </tr>                       
                </table>                 
            </td>
        </tr>
     
    </table>
    
</ContentTemplate>
</asp:UpdatePanel>

</asp:Content>
