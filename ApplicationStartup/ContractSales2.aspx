<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractSales2.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractSales2" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>


<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<asp:Content ID="ContractSales2Content" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">


 <script type="text/javascript" >
     /* called on grid div scroll */
     function setSalesSummaryScrollForRestore(divToScroll) { 

         if (divToScroll != "0") {
             $get("SalesSummaryScrollPos").value = divToScroll.scrollTop;
         }
     }


     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestoreSalesSummaryGridSelectionOnAsyncPostback();
     }

     function RestoreSalesSummaryGridSelectionOnAsyncPostback() {
         var SalesSummaryScrollPos = $get("SalesSummaryScrollPos").value;
         var highlightedSalesSummaryRow = $get("highlightedSalesSummaryRow").value;

         RestoreSalesSummaryGridSelection(SalesSummaryScrollPos, highlightedSalesSummaryRow);
     }

     /* called from form load */
     function RestoreSalesSummaryGridSelection(SalesSummaryScrollPos, highlightedSalesSummaryRow) {
         $get("SalesSummaryScrollPos").value = SalesSummaryScrollPos;
         if (SalesSummaryScrollPos) {
             if (SalesSummaryScrollPos >= 0) {

                 var theSalesSummaryDiv = document.getElementById('<%=SalesSummaryGridViewDiv.ClientID %>');
                 if (theSalesSummaryDiv) {
                     theSalesSummaryDiv.scrollTop = SalesSummaryScrollPos;
                 }
             }
         }

         if (highlightedSalesSummaryRow) {
             if (highlightedSalesSummaryRow >= 0) {
                 $get("highlightedSalesSummaryRow").value = highlightedSalesSummaryRow;
                 highlightSalesSummaryRow();
             }
         }
     }

     function setSalesSummaryHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedSalesSummaryRow").value = rowIndex;
         $get("highlightedSalesSummaryRowOriginalColor").value = originalColor;
         highlightSalesSummaryRow();

     }

     function highlightSalesSummaryRow() {

         var selectedRowIndex = $get("highlightedSalesSummaryRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var SalesSummaryGridView = document.getElementById("<%=SalesSummaryGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (SalesSummaryGridView) {
             currentSelectedRow = SalesSummaryGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightSalesSummaryRow() {

         var selectedRowIndex = $get("highlightedSalesSummaryRow").value;
         var highlightedSalesSummaryRowOriginalColor = $get("highlightedSalesSummaryRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedSalesSummaryRow").value = -1;
         var SalesSummaryGridView = document.getElementById("<%=SalesSummaryGridView.ClientID%>");
         var currentSelectedRow = null;
         if (SalesSummaryGridView) {
             currentSelectedRow = SalesSummaryGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedSalesSummaryRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedSalesSummaryRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetSalesSummaryHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "SalesSummaryGridView") {

             unhighlightSalesSummaryRow();

             $get("highlightedSalesSummaryRow").value = rowIndex;
             $get("highlightedSalesSummaryRowOriginalColor").value = rowColor;

             highlightSalesSummaryRow();
         }
     }

</script>

<ep:UpdatePanelEventProxy ID="SalesSummaryUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="SalesSummaryUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="SalesSummaryUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

   <table class="OuterTable" >
        <tr>
          <td style="vertical-align:top;" >
                <asp:FormView ID="SalesHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="SalesHeaderFormView_OnPreRender"  >
                    <EditItemTemplate>
                    <table class="OutsetBox" style="width:100%;">
                            <tr class="OutsetBoxHeaderRow" >
                                <td  style="text-align:center;" colspan="4" >
                                    <asp:Label ID="SalesHeaderFormViewHeaderLabel" runat="server" Text="Sales"  />
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align:center;" >
                                   <asp:Button runat="server" ID="ViewIFFSalesComparisonButton"  Width="200px" Text="View IFF/Payment Comparison" OnClick="ViewIFFSalesComparisonButton_OnClick"  />
                                </td>
  
                                <td style="text-align:center;" >                                             
                                   <asp:Button runat="server" ID="ViewFullSalesHistoryButton"  Width="200px" Text="View Full Sales History" OnClick="ViewFullSalesHistoryButton_OnClick"  />
                                </td>
                                    
                                <td style="text-align:center;" >
                                  <asp:Button runat="server" ID="ViewSalesHistoryByYearButton"  Width="200px" Text="View Sales History By Year" OnClick="ViewSalesHistoryByYearButton_OnClick"  />                                    
                                </td>
                                
                                <td style="text-align:center;" >                                                     
                                    <asp:Button runat="server" ID="ViewSalesHistoryByQuarterButton"  Width="200px" Text="View Sales History By Quarter" OnClick="ViewSalesHistoryByQuarterButton_OnClick"  />
                                </td>
                                         
                        </table>
                        </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td style="vertical-align:top;" >
  
                <asp:Panel ID="SalesGridPanel" runat="server" Width="100%" Height="100%"  OnPreRender="SalesGridPanel_OnPreRender" >
                    <div id="SalesPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="SalesSummaryScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSalesSummaryRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                        <asp:HiddenField ID="highlightedSalesSummaryRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                        <input id="RefreshSalesSummaryScreenOnSubmit" runat="server" type="hidden"  value="false" enableviewstate="true" /> 
                        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    </div>
                    
                    <div id="SalesSummaryGridViewDiv"  runat="server"  style="border:3px solid black; height:368px; overflow: scroll" onscroll="javascript:setSalesScrollForRestore( this );"  onkeypress="javascript:setSalesScrollForRestore( this );"  >

                        <ep:GridView ID="SalesSummaryGridView" 
                                    runat="server" 
                                    DataKeyNames="Quarter_ID"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="CMGrid" 
                                    Visible="True" 
                                    onrowcommand="SalesSummaryGridView_RowCommand" 
                                    OnSelectedIndexChanged="SalesSummaryGridView_OnSelectedIndexChanged" 
                                    OnRowDataBound="SalesSummaryGridView_RowDataBound"
                                    AllowSorting="True" 
                                    AutoGenerateEditButton="false"
                                    EditRowStyle-CssClass="CMGridEditRowStyle" 
                                    onprerender="SalesSummaryGridView_PreRender" 
                                    OnInit="SalesSummaryGridView_Init"
                                    OnRowCreated="SalesSummaryGridView_OnRowCreated"
                                    OnRowDeleting="SalesSummaryGridView_RowDeleting" 
                                    OnRowEditing="SalesSummaryGridView_RowEditing" 
                                    OnRowUpdating="SalesSummaryGridView_RowUpdating" 
                                    OnRowCancelingEdit="SalesSummaryGridView_RowCancelingEdit"
                                    AllowInserting="True"
                                    OnRowInserting="SalesSummaryGridView_RowInserting" 
                                    InsertCommandColumnIndex="1"
                                    EmptyDataRowStyle-CssClass="CMGrid" 
                                    EmptyDataText="There are no Sales for the selected contract."
                                    Font-Names="Arial" 
                                    Font-Size="small" >
                                <HeaderStyle CssClass="SalesSummaryGridHeaders" />
                                <RowStyle  CssClass="SalesSummaryGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="CMGridAltItems" />
                                <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                <Columns>   
                                                                            
                                        <asp:TemplateField ItemStyle-Width="8%" ItemStyle-HorizontalAlign="Center" >
                                        <ItemTemplate>
                                            <asp:Button runat="server"  ID="ViewSalesVarianceBySINButton" Text="View Sales Variance By SIN" OnDataBinding="ViewSalesVarianceBySINButton_DataBinding" OnCommand="SalesSummaryGridView_ButtonCommand" CommandName="ViewSalesVarianceBySIN" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("Quarter_ID") + "," +  Eval("Year") + "," +  Eval("Qtr") %>' ButtonType="Button" CssClass="multilineButtonText" Width="120px" >            
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Button runat="server"  ID="ViewSalesVarianceBySINButton" Text="View Sales Variance By SIN" OnDataBinding="ViewSalesVarianceBySINButton_DataBinding" OnCommand="SalesSummaryGridView_ButtonCommand" CommandName="ViewSalesVarianceBySIN" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("Quarter_ID") + "," +  Eval("Year") + "," +  Eval("Qtr") %>' ButtonType="Button" CssClass="multilineButtonText" Width="120px" >            
                                                </asp:Button >                                    
                                        </EditItemTemplate>
                                        </asp:TemplateField>
                                                                        
                                                                                                          
                                        <asp:TemplateField ItemStyle-Width="8%" ItemStyle-HorizontalAlign="Center" >  
                                          <ItemTemplate>
                                            <asp:Button runat="server"  ID="ViewSalesBySINButton" Text="View Sales By SIN" OnDataBinding="ViewSalesBySINButton_DataBinding" OnCommand="SalesSummaryGridView_ButtonCommand" CommandName="ViewSalesBySINButton" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("Quarter_ID") + "," +  Eval("Year") + "," +  Eval("Qtr") %>' ButtonType="Button" CssClass="multilineButtonText" Width="100px" >            
                                                </asp:Button >                                    
                                          </ItemTemplate>
                                          <EditItemTemplate>
                                            <asp:Button runat="server"  ID="ViewSalesBySINButton" Text="View Sales By SIN" OnDataBinding="ViewSalesBySINButton_DataBinding" OnCommand="SalesSummaryGridView_ButtonCommand" CommandName="ViewSalesBySINButton" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("Quarter_ID") + "," +  Eval("Year") + "," +  Eval("Qtr") %>' ButtonType="Button" CssClass="multilineButtonText" Width="100px" >            
                                                </asp:Button >                                    
                                          </EditItemTemplate>
                                        </asp:TemplateField>      
                                            
 
                                    <asp:TemplateField ItemStyle-Width="9%" ItemStyle-HorizontalAlign="Center" >
                                        <ItemTemplate>
                                            <asp:Label ID="YearQuarterLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "Title" )%>'  CssClass="rightAlign" >
                                            </asp:Label>
                                        </ItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="YearQuarterLabel" Width="100px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "Title" )%>'  CssClass="rightAlign" >
                                            </asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="VA Sales"  ItemStyle-Width="9%"  >
                                        <ItemTemplate>
                                            <asp:Label ID="VASalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "VA_Sales", "{0:c}"  )%>' CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="VASalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "VA_Sales", "{0:c}"  )%>' CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                   <asp:TemplateField HeaderText="Qtr"  ItemStyle-Width="5%" ItemStyle-BackColor="#D3D3D3" >
                                        <ItemTemplate>
                                            <asp:Label ID="VAQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterVA" ), 0 )%>'   OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="VAQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterVA" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                   <asp:TemplateField HeaderText="Year"  ItemStyle-Width="5%"  ItemStyle-BackColor="#D3D3D3" >
                                        <ItemTemplate>
                                            <asp:Label ID="VAYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearVA" ), 0 )%>'   OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="VAYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearVA" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="OGA Sales"  ItemStyle-Width="9%" >
                                        <ItemTemplate>
                                            <asp:Label ID="OGASalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "OGA_Sales", "{0:c}"  )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="OGASalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "OGA_Sales", "{0:c}"  )%>'  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                   <asp:TemplateField HeaderText="Qtr"   ItemStyle-Width="5%"  ItemStyle-BackColor="#D3D3D3" >
                                        <ItemTemplate>
                                            <asp:Label ID="OGAQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterOGA" ), 0 )%>'   OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="OGAQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterOGA" ), 0 )%>'   OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                   <asp:TemplateField HeaderText="Year"   ItemStyle-Width="5%"   ItemStyle-BackColor="#D3D3D3" >
                                        <ItemTemplate>
                                            <asp:Label ID="OGAYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearOGA" ), 0 )%>'   OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="OGAYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearOGA" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="S/C/L Govt. Sales"  ItemStyle-Width="9%" >
                                        <ItemTemplate>
                                            <asp:Label ID="SLGSalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "SLG_Sales", "{0:c}"  )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="SLGSalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "SLG_Sales", "{0:c}"  )%>'  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                   <asp:TemplateField HeaderText="Qtr"   ItemStyle-Width="5%"  ItemStyle-BackColor="#D3D3D3">
                                        <ItemTemplate>
                                            <asp:Label ID="SLGQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%#FormatAsPercent(  DataBinder.Eval( Container.DataItem, "VarianceQuarterSLG" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="SLGQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterSLG" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                   <asp:TemplateField HeaderText="Year"   ItemStyle-Width="5%"   ItemStyle-BackColor="#D3D3D3">
                                        <ItemTemplate>
                                            <asp:Label ID="SLGYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearSLG" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="SLGYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearSLG" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                   <asp:TemplateField HeaderText="Totals"  ItemStyle-Width="9%" >
                                        <ItemTemplate>
                                            <asp:Label ID="TotalSalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "Total_Sales", "{0:c}"  )%>'  CssClass="rightAlign" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="TotalSalesLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "Total_Sales", "{0:c}" )%>'  CssClass="rightAlign"  ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                            
                                   <asp:TemplateField HeaderText="Qtr"  ItemStyle-Width="5%" ItemStyle-BackColor="#D3D3D3">
                                        <ItemTemplate>
                                            <asp:Label ID="TotalQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterTotal" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                             <asp:Label ID="TotalQuarterlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceQuarterTotal" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                   <asp:TemplateField HeaderText="Year"  ItemStyle-Width="5%" ItemStyle-BackColor="#D3D3D3">
                                        <ItemTemplate>
                                            <asp:Label ID="TotalYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearTotal" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="TotalYearlyVarianceLabel" runat="server"  Width="54px"  Text='<%# FormatAsPercent( DataBinder.Eval( Container.DataItem, "VarianceYearTotal" ), 0 )%>'  OnDataBinding="SalesVariance_OnDataBinding"  CssClass="rightAlign" ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                  <asp:TemplateField HeaderText="QuarterId"   >
                                        <ItemTemplate>
                                            <asp:Label ID="QuarterIdLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Quarter_ID" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="QuarterIdLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Quarter_ID" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                 <asp:TemplateField HeaderText="Year" >
                                        <ItemTemplate>
                                            <asp:Label ID="YearLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Year" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="YearLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Year" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                 <asp:TemplateField HeaderText="Quarter"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="QuarterLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Qtr" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="QuarterLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "Qtr" )%>' ></asp:Label>
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
