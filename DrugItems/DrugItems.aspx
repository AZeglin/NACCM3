<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="DIStandard"  EnableEventValidation="false"  CodeBehind="DrugItems.aspx.cs" Inherits="VA.NAC.CM.DrugItems.DrugItems" %>


<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register src="DrugItemHeader.ascx" tagname="DrugItemHeader" tagprefix="uc1" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Pharmaceutical Items</title>
    
    <script src="Scripts/JQuery/jquery-3.7.1.min.js" type="text/javascript"></script>
    <script src="Scripts/Ajax/jquery.unobtrusive-ajax.min.js" type="text/javascript"></script>     
    <script src="JQuery/spin.js" type="text/javascript" ></script>   
      
       <script type="text/javascript">
       <!--
           function RefreshParent()
           {
               window.opener.document.forms[0].fvContractInfo$RefreshPricelistScreenOnSubmit.value = true;
               window.opener.document.forms[0].submit();
               setTimeout('', 240);
           }
           
           function CloseWindow()
           {               
               setTimeout('', 240);
               top.window.opener = top;
               top.window.open('', '_parent', '');
               setTimeout('top.window.close()', 620);
           }
            
           function RefreshParent2()
           {
               window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ItemPriceCountsFormView$RefreshPricelistCountsOnSubmit.value = true;
               window.opener.document.forms[0].submit();
               top.window.opener = top;
               top.window.open('', '_parent', '');
               top.window.close();
           }

        //-->
        </script>
    
    
</head>
<body>
    
    <form id="drugItemForm" name="drugItemForm" runat="server"  style="position:absolute; top:0px; left:0px; width:100%; height:100%;" >

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="DrugItemsScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>
    
    <script type="text/javascript" >
    
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest( beginRequest );
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded( pageLoaded );
        
        function setItemScrollForRestore( divToScroll )
        {        
            if( divToScroll != "0" )
            {
                $get( "scrollPos" ).value = divToScroll.scrollTop;
            }      
        }
        
        function setItemScrollOnChange( newPositionValue )
        {           
            $get( "scrollPos" ).value = newPositionValue;
            $get("DrugItemsGridViewDiv").scrollTop = newPositionValue;       
        }

        
        function setPriceScrollForRestore( divToScroll )
        {
            if( divToScroll != "0" )
            {
                $get( "priceScrollPos" ).value = divToScroll.scrollTop;
            }
        }
        
        function setPriceScrollOnChange( newPositionValue )
        {
            $get( "priceScrollPos" ).value = newPositionValue;
            $get( "DrugItemPricesGridViewDiv" ).scrollTop = newPositionValue;
        }
            
        var postbackElement;
        
        function beginRequest( sender, args )
        {
            postbackElement = args.get_postBackElement();       
        }
    
        function pageLoaded( sender, args )
        {
            if( typeof( postbackElement ) == "undefined" )
            {
                return;
            } 

            var itemScrollValue = $get( "scrollPos" ).value;
            var priceScrollValue = $get( "priceScrollPos" ).value;

            if( postbackElement.id.toLowerCase().indexOf('drugitemsgridview' ) > -1 )
            {
                $get( "DrugItemsGridViewDiv" ).scrollTop = itemScrollValue;
                
            }
            if( postbackElement.id.toLowerCase().indexOf('drugitempricesgridview' ) > -1 )
            {
                $get( "DrugItemPricesGridViewDiv" ).scrollTop = priceScrollValue;
                
                $get( "DrugItemsGridViewDiv" ).scrollTop = itemScrollValue;                
            }
            
            highlightDrugItemRow();
        }
        
        function highlightDrugItemRow()
        {
            var selectedRowIndex = $get("highlightedDrugItemRow").value;
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var drugItemGridView = $get( "DrugItemsGridView" );
            var currentSelectedRow = null;
            if( drugItemGridView != null )
            {
                currentSelectedRow = drugItemGridView.rows[ selectedRowIndex ];
            }
            if( currentSelectedRow != null )
            {
                currentSelectedRow.className = 'DrugItemSelectedCellStyle';
            }
        }


        function simulateDefaultButton(evt) {
            if (evt.which || evt.keyCode) {
                if ((evt.which == 13) || (evt.keyCode == 13)) {

                    evt.stopPropogation();
                    evt.preventDefault();                   
                    return false;
                }
                return true;
            }            
            return true;
        }
       
        function simulateDefaultButtonOld(evt) {
            if (evt.which || evt.keyCode) {
                if ((evt.which == 13) || (evt.keyCode == 13)) {
                    __doPostBack('<%= SearchButton.UniqueID  %>', '');
                    return false;
                }
                return true;
            }
        }
      
        function setDrugItemHighlightedRowIndexAndOriginalColor( rowIndex, originalColor )
        {
            $get("highlightedDrugItemRow").value = rowIndex;
            $get("highlightedDrugItemRowOriginalColor").value = originalColor;
            
            highlightDrugItemRow();
        }
        
        function highlightDrugItemPriceRow()
        {
            var selectedRowIndex = $get("highlightedDrugItemPriceRow").value;
            
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var drugItemPricesGridView = $get( "DrugItemPricesGridView" );
            var currentSelectedRow = null;
            if( drugItemPricesGridView != null )
            {
                currentSelectedRow = drugItemPricesGridView.rows[ selectedRowIndex ];
            }
            if( currentSelectedRow != null )
            {
                currentSelectedRow.className = 'DrugItemSelectedCellStyle';
            }
        }
        
   
        
        function setDrugItemPriceHighlightedRowIndexAndOriginalColor( rowIndex, originalColor )
        {
            $get("highlightedDrugItemPriceRow").value = rowIndex;
            $get("highlightedDrugItemPriceRowOriginalColor").value = originalColor;
            
            highlightDrugItemPriceRow();
        }
        
        function presentConfirmationMessage( msg )
        {
            $get("confirmationMessageResults").value = confirm( msg ); 
        }
        
        function presentPromptMessage( msg )
        {
            $get("promptMessageResults").value = prompt( msg, "" );
        }
    </script>
    

        <table  style="width:100%; height:100%; table-layout:fixed; border:solid 1px black;" >
            <colgroup>
                <col style="width:100%;" />
            </colgroup>
            <tr  style="vertical-align:top; height:14%;">
                <td >
                    <div id="DrugItemHeaderDiv"  style="width:99%; height:112px; border:solid 1px black; background-color:White; margin:3px;" >
                        <asp:Panel ID="DrugItemHeaderPanel" runat="server" DefaultButton="SearchButton" >
                        <asp:UpdatePanel ID="DrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="InsertItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                        </Triggers>   
                             <ContentTemplate>
                               <table  style="width:100%; border:none; height:90px; table-layout:fixed" align="center" >
                                <tr >
                                    <td style="width:190px; text-align:center;"  rowspan="3" >
                                        <asp:UpdateProgress ID="DrugItemUpdateProgress" runat="server" >
                                            <ProgressTemplate>
                                                <div id="OuterTableCellOverlay">
                                                    <div id="InnerTableCellOverlay">
                                                        <b><i> loading ... </i></b>
                                                        <br />
                                                        <asp:Image ID="LoadImage" runat="server" ImageUrl="~/Images/ajax-loader.gif" />
                                                  </div>
                                                </div>
                                            </ProgressTemplate>
                                        </asp:UpdateProgress>
                                    </td>
                                    <td style="width:100%; text-align:center;">
                                        <asp:Label ID="MainHeaderTitleLabel1" runat="server" 
                                            Text="Pharmaceutical Items"  CssClass="DrugItemHeaderText"/>
                                    </td>
                                    <td style="width:190px; text-align:right; margin-right:10px;">
                                        <asp:Button ID="formCloseButton" runat="server" Height="20px" Text="Close" SkinID="SmallDrugItemButton" />
                                    </td>
                                </tr>
                                <tr >
                                    <td style="width:100%; text-align:center;">
                                        <asp:Label ID="MainHeaderTitleLabel2" runat="server" Text="For Contract" 
                                             CssClass="DrugItemHeaderText"/>
                                    </td>
                                    <td style="width:200px; text-align:right;">
                                        <table style="height:20px; width:190px; margin:0px; table-layout:fixed;"  >
                                            <tr style="text-align:left; margin:0px;">
                                                <td >
                                                    <asp:Button runat="server"  ID="PrintItemsAndPricesButton" Width="90px" Height="20px" Text="Print Report" OnClick="PrintItemsAndPricesButton_OnClick"  SkinID="SmallDrugItemButton" /> 
                                                </td>
                                                <td >
                                                    <asp:Button runat="server"  ID="PrintScreenButton"  Width="90px" Height="20px" Text="Print Screen" OnClientClick="window.print();"  SkinID="SmallDrugItemButton" /> 
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:center; width:100%;" >
                                        <asp:Label ID="MainHeaderTitleLabel3" runat="server" Text="Vendor" Height="20px" CssClass="DrugItemRegularText" />
                                    </td>
                                    <td style="width:190px; text-align:right;" >
                                        <asp:Label ID="MainHeaderItemCount" runat="server" Text="Total Items" Height="20px" CssClass="DrugItemRegularText" />
                                    </td>
                                </tr>
                                <tr >
                                    <td  colspan="3" style="text-align:left; width:99%;" >
                                         <table  style="border-top:solid 1px Black; height:34px; table-layout:fixed; text-align:left; width:100%;" >
                                            <tr>
                                                <td style="width:118px;" >
                                                    <asp:Button runat="server"  ID="AddNewItemButton" Text="Add New Item" OnClick="AddNewItemButton_OnClick" >            
                                                    </asp:Button >      
                                                </td>
                                                <td style="width:65px; text-align:right; padding-right:2px;" >
                                                     <asp:Label ID="ItemSearchLabel" runat="server" Text="Search Text" CssClass="DrugItemSearchText"  />
                                                </td>
                                                <td style="width:144px; text-align:left;" >
                                                    <ep:TextBox ID="ItemSearchTextBox" runat="server" onkeydown="simulateDefaultButton(event);" Width="142px"></ep:TextBox>
                                                </td>
                                                <td style="width:70px; text-align:center;" >
                                                    <asp:Button ID="SearchButton"  Text="Search" runat="server" Width="70px" OnClick="SearchButton_OnClick" />
                                                </td>
                                                <td style="width:102px; text-align:center;" >
                                                    <asp:Button ID="ClearSearchButton"  Text="Clear Search" runat="server" Width="100px" OnClick="ClearSearchButton_OnClick" />
                                                </td>
                                                <td style="width:360px; text-align:right; min-width:340px;" >                             
                                                     <asp:RadioButtonList ID="ItemFilterRadioButtonList" CellSpacing="1" CellPadding="1" runat="server" Width="99%" CssClass="DrugItemRadioButtonList" AutoPostBack="True" EnableViewState="true" ToolTip="Filter Items" RepeatDirection="Horizontal"  OnSelectedIndexChanged="ItemFilterRadioButtonList_OnSelectedIndexChanged"  >
                                                        <asp:ListItem Value="B" Selected="True">Active</asp:ListItem>
                                                        <asp:ListItem Value="C">Covered Only</asp:ListItem>
                                                        <asp:ListItem Value="N">Non-Covered Only</asp:ListItem>
                                                        <asp:ListItem Value="D">Discontinued</asp:ListItem>
                                                    </asp:RadioButtonList>
                                               </td>
                                            </tr>                    
                                         </table>
                                     </td>
                                    
                                </tr>
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    </asp:Panel>
                 </div>
                </td>
            </tr>

            <tr style="border: solid 1px black; vertical-align:top; height:45%;">
                <td >
                    <input id="scrollPos" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedDrugItemRow" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedDrugItemRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedDrugItemPriceRow" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedDrugItemPriceRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    <input id="ContextMenuRowIndex_DrugItemsGridView" runat="server" type="hidden" value="-1" enableviewstate="true" />
                    <div id="DrugItemsGridViewDiv"  runat="server" style="width:100%; height:100%; max-height:340px; min-height:120px; top:113px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="InsertItemButtonClickUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="SelectParentItemForBPAUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="RightClickUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="DrugItemsGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="InsertItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="SelectParentItemForBPAUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="RightClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                                <ep:ContextMenu ID="ItemContextMenu" runat="server" ActivateOnRightClick="True" RolloverColor="#E5FDDF"  ForeColor="Black" BackColor="White" IsContextMenuInUpdatePanel="True" >   
                                       <ContextMenuItems>  
                                       </ContextMenuItems>   
                                  </ep:ContextMenu>     

                                 <ep:GridView ID="DrugItemsGridView" 
                                            runat="server" 
                                            DataKeyNames="DrugItemId"  
                                            AutoGenerateColumns="False" 
                                            Width="99%" 
                                            CssClass="DrugItemGrids" 
                                            Visible="True" 
                                            onrowcommand="DrugItemsGridView_RowCommand" 
                                            OnSelectedIndexChanged="DrugItemsGridView_OnSelectedIndexChanged" 
                                            OnRowDataBound="DrugItemsGridView_RowDataBound"
                                            AllowSorting="True" 
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                            onprerender="DrugItemsGridView_PreRender" 
                                            OnInit="DrugItemsGridView_Init"
                                            OnRowCreated="DrugItemsGridView_OnRowCreated"
                                            onrowdeleting="DrugItemsGridView_RowDeleting" 
                                            onrowediting="DrugItemsGridView_RowEditing" 
                                            onrowupdating="DrugItemsGridView_RowUpdating" 
                                            onrowcancelingedit="DrugItemsGridView_RowCancelingEdit"
                                            AllowInserting="True"
                                            OnRowInserting="DrugItemsGridView_RowInserting" 
                                          
                                            EmptyDataRowStyle-CssClass="DrugItemGrids" 
                                            EmptyDataText="There are no items for the selected contract."
                                            ContextMenuID="ItemContextMenu"   
                                           >
                                        <HeaderStyle CssClass="DrugItemGridHeaders" />
                                        <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="DrugItemGridAltItems" />                                        
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                                                                               
                                            <asp:TemplateField >   
                                                <ItemTemplate>
                                                     <asp:Button runat="server" ID="RefreshPricesButton" Text="Refresh Prices" OnCommand="DrugItemsGridView_ButtonCommand" CommandName="RefreshPriceList"  CommandArgument='<%#   Container.DataItemIndex + "," + Eval("DrugItemId") %>'  Width="70px" CssClass="ButtonWrapText" />                                 
                                                </ItemTemplate>
                                            </asp:TemplateField>      
                                                        
                                            <asp:TemplateField >  
                                                <ItemTemplate>
                                                       <asp:Button runat="server" ID="OpenItemDetailsForItemButton" Text="Details" OnCommand="DrugItemsGridView_ButtonCommand"   CommandName="OpenItemDetailsForItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemId") %>'  Width="64px"   >
                                                      </asp:Button>   
                                  
                                                </ItemTemplate>
                                            </asp:TemplateField>      
                                           
                                            <asp:TemplateField >  
                                                <ItemTemplate>
                                                       <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="DrugItemsGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemId") %>'   Width="60px"   >
                                                      </asp:Button>   

                                                       <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="DrugItemsGridView_ButtonCommand"   CommandName="SaveItem" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemId") %>'   Width="60px"   >
                                                      </asp:Button>   

                                                       <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="DrugItemsGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemId") %>'   Width="60px"   >
                                                      </asp:Button>   
                                  
                                                </ItemTemplate>
                                            </asp:TemplateField>      
                                            
                                   <%--        <ep:ExtendedCommandField ShowEditButton="true" EditText="Edit" ButtonType="Button" CancelText="Cancel" UpdateText="Save" InsertVisible="true" /> --%> 
                                           
                                    <%--           <asp:BoundField DataField="FdaAssignedLabelerCode" HeaderText="Fda Assigned Labeler Code"   ControlStyle-Width="50px"/>  --%> 

                                         <asp:TemplateField HeaderText="Parent Item"  ItemStyle-Width="260px"  >
                                            <ItemTemplate>
                                                <asp:Label ID="parentItemLabel" Width="260px"  runat="server" OnDataBinding="parentItemLabel_OnDataBinding" >
                                                </asp:Label>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:DropDownList ID="parentItemDropDownList" DataValueField="ParentDrugItemId"  Width="260px"   DataTextField="OverallDescription" runat="server" OnDataBound="ParentItemDropDownList_DataBound" OnSelectedIndexChanged="ParentItemDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                </asp:DropDownList>
                                            </EditItemTemplate>
                                         </asp:TemplateField>

                                         <asp:TemplateField HeaderText="Fda Assigned Labeler Code"  ItemStyle-Width="50px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="fdaAssignedLabelerCodeLabel" runat="server"  Width="50px"  Text='<%# DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="fdaAssignedLabelerCodeTextBox" runat="server" MaxLength="5"  Width="50px" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "FdaAssignedLabelerCode" ))%>' ></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            
                                    <%--        <asp:BoundField DataField="ProductCode" HeaderText="Product Code" ControlStyle-Width="50px" />  --%>
                                    <%--        <asp:BoundField DataField="PackageCode" HeaderText="Package Code" ControlStyle-Width="50px" />  --%>
                                            
                                             <asp:TemplateField HeaderText="Product Code"  ItemStyle-Width="50px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="productCodeLabel" runat="server"  Width="50px"  Text='<%# DataBinder.Eval( Container.DataItem, "ProductCode" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="productCodeTextBox" runat="server"  MaxLength="4"  Width="50px" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "ProductCode" ))%>' ></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="Package Code"  ItemStyle-Width="50px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="packageCodeLabel" runat="server"  Width="50px"  Text='<%# DataBinder.Eval( Container.DataItem, "PackageCode" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="packageCodeTextBox" runat="server"  MaxLength="2"  Width="50px" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "PackageCode" ))%>' ></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

 

                                            <asp:TemplateField  HeaderText="Covered ?"  ItemStyle-Width="50px" >
                                                <ItemTemplate>
                                                    <asp:Label ID="coveredLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "Covered" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="coveredDropDownList" runat="server" OnDataBound="CoveredDropDownList_DataBound" >
                                                        <asp:ListItem Text="Covered" Value="T" />
                                                        <asp:ListItem Text="Non-covered" Value="F" />                                              
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField  HeaderText="Generic"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="genericLabel" runat="server" Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Generic" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <ep:TextBox ID="genericTextBox" runat="server" TextMode="MultiLine" MaxLength="64"  CssClass="DrugItemMultilineInEditMode" Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "Generic" )%>' ></ep:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="Trade Name"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="tradeNameLabel" runat="server"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <ep:TextBox ID="tradeNameTextBox" runat="server" ReadOnly="false" TextMode="MultiLine" MaxLength="45" CssClass="DrugItemMultilineInEditMode"  Width="220px" Text='<%# DataBinder.Eval( Container.DataItem, "TradeName" )%>' ></ep:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
      
                                             <asp:TemplateField HeaderText="Dispensing Unit"  ItemStyle-Width="60px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="dispensingUnitLabel" runat="server" Width="60px" Text='<%# DataBinder.Eval( Container.DataItem, "DispensingUnit" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="dispensingUnitTextBox" runat="server"  Width="60px" MaxLength="10" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "DispensingUnit" ))%>' ></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                          
                                             <asp:TemplateField HeaderText="Package Size"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="packageSizeLabel" runat="server" Width="60px" Style="word-wrap: normal; word-break: break-all;" Text='<%# DataBinder.Eval( Container.DataItem, "PackageDescription" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="packageSizeTextBox" runat="server" Width="60px" MaxLength="14" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "PackageDescription" ))%>' ></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:BoundField DataField="CurrentFSSPrice" HeaderText="Current FSS Price" ReadOnly="true" DataFormatString="{0:c}" ControlStyle-Width="50px" /> 
                                            <asp:BoundField DataField="PriceStartDate" HeaderText="Price Start Date" ReadOnly="true" DataFormatString="{0:d}" ControlStyle-Width="50px"/> 
                                            <asp:BoundField DataField="PriceEndDate" HeaderText="Price End Date" ReadOnly="true" DataFormatString="{0:d}" ControlStyle-Width="50px" /> 
                                         
                                            <asp:TemplateField HeaderText="Has BPA ?"  ItemStyle-Width="40px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="hasBPALabel" runat="server" Width="40px" OnDataBinding="HasBPALabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="hasBPALabel" runat="server" Width="40px" OnDataBinding="HasBPALabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                         
                                            <asp:TemplateField HeaderText="Last Modified By"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="lastModificationTypeLabel" runat="server"  Width="90px"  Text='<%# CMGlobals.GetLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="lastModificationTypeLabel" runat="server" Width="90px" Text='<%# CMGlobals.GetLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                    
                                             <asp:TemplateField HeaderText="Discontinuation Date"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="discontinuationDateLabel" runat="server"  Width="90px"  Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationDate", "{0:d}" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="discontinuationDateLabel" runat="server" Width="90px" Text='<%# DataBinder.Eval( Container.DataItem, "DiscontinuationDate", "{0:d}" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                         
                                            <asp:TemplateField HeaderText="Discontinuation Reason"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="discontinuationReasonLabel" runat="server"  Width="90px"  Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "DiscontinuationReason" ))%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="discontinuationReasonLabel" runat="server" Width="90px" Text='<%# CMGlobals.Trim( DataBinder.Eval( Container.DataItem, "DiscontinuationReason" ))%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                           
                                            <asp:TemplateField >
                                                <ItemTemplate>
                                                    <asp:Button runat="server"  ID="RemoveItemAndPricesButton" Text="Remove Item And Prices"  OnCommand="DrugItemsGridView_ButtonCommand" CommandName="RemoveItemAndItemPrices" CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemId") %>'  Width="82px" CssClass="ButtonWrapTextSmaller" >               
                                                        </asp:Button >                                    
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Button runat="server"  ID="RemoveItemAndPricesButton" Text="Remove Item And Prices"  OnCommand="DrugItemsGridView_ButtonCommand" CommandName="RemoveItemAndItemPrices" CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemId") %>'  Width="82px" CssClass="ButtonWrapTextSmaller" >                
                                                        </asp:Button >                                    
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Drug Item NDC Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="NDCIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DrugItemNDCId" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="NDCIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "DrugItemNDCId" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Parent Drug Item Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ParentDrugItemIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ParentDrugItemId" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ParentDrugItemIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ParentDrugItemId" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Dual Price Designation"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="DualPriceDesignationLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DualPriceDesignation" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="DualPriceDesignationLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "DualPriceDesignation" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                            
                                    </ep:GridView>

                                </ContentTemplate>
                          </asp:UpdatePanel> 
                 </div>
                </td>
            </tr>
            <tr style="vertical-align:top; height:11%;" >
                <td>
                    <div id="drugPriceHeader1Div" style="height: 84px; width: 100%;" > 
                        <asp:UpdatePanel ID="SelectedDrugItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="RefreshPricesButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="PriceListChangeUpdatePanelEventProxy" EventName="ProxiedEvent" />   
                            </Triggers>
                            <ContentTemplate>
                                <uc1:DrugItemHeader ID="SelectedDrugItemHeader" runat="server" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                 </td>
             </tr>
             <tr style="vertical-align:top; height:3%;" >
                 <td>
                 <div id="PriceHeader2Div"  style="width:99%; height:26px; margin:2px; border-top:solid 1px black; " >
                    <ep:UpdatePanelEventProxy ID="PriceFilterChangeUpdatePanelEventProxy" runat="server" />
                    <asp:UpdatePanel ID="PriceHeader2UpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="PriceFilterChangeUpdatePanelEventProxy" EventName="ProxiedEvent" />   
                        </Triggers>
                        <ContentTemplate>
                          <table style="width:100%; height:25px; table-layout:fixed; text-align:left;" >
                              <colgroup>
                                  <col style="width:160px;" />
                                  <col style="width:100%;" />
                                  <col style="width:230px;" />
                                  <col style="width:16px;" />
                              </colgroup>
                                <tr >
                                    <td >
                                        <asp:Button runat="server"  ID="AddNewPriceButton" Text="Add New Price" OnClick="AddNewPriceButton_OnClick"  >            
                                        </asp:Button >      
                                    </td>
                                    <td ></td>
                                    <td style="text-align:right;"  >                              
                                         <asp:CheckBoxList ID="PriceFilterCheckBoxList" runat="server" CssClass="DrugItemRegularText" AutoPostBack="True" ToolTip="Filter Prices" RepeatDirection="Horizontal" OnSelectedIndexChanged="PriceFilterCheckBoxList_OnSelectedIndexChanged"  >
                                            <asp:ListItem Value="Active" Selected="True">Active</asp:ListItem>
                                            <asp:ListItem Value="Future">Future</asp:ListItem>
                                            <asp:ListItem Value="Historical">Historical</asp:ListItem>
                                        </asp:CheckBoxList>
                                   </td>
                                   <td >
                                   </td>
                                </tr>
                             </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
            <tr style="border: solid 1px black; vertical-align:top; height:25%;">
                 <td >
                    <input id="priceScrollPos" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <div id="DrugItemPricesGridViewDiv" style="height: 100%; width: 100%; max-height:182px; min-height:120px; position:relative; bottom: 0; left: 0; overflow: scroll" runat="server" onscroll="javascript:setPriceScrollForRestore( this );"  onkeypress="javascript:setPriceScrollForRestore( this );"  > 
                      
                      <ep:UpdatePanelEventProxy ID="RefreshPricesButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="InsertPriceButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="EditCancelPriceButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="ChangeItemPriceHighlightUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="PriceListChangeUpdatePanelEventProxy" runat="server" />

                      <asp:UpdatePanel ID="DrugItemPricesGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RefreshPricesButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="InsertPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="EditCancelPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="ChangeItemPriceHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="PriceFilterChangeUpdatePanelEventProxy" EventName="ProxiedEvent" />   
                       </Triggers>
                        <ContentTemplate>
                           <ep:GridView ID="DrugItemPricesGridView" 
                                    runat="server" 
                                    DataKeyNames="DrugItemPriceId,DrugItemPriceHistoryId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="DrugItemGrids" 
                                    onrowcommand="DrugItemPricesGridView_RowCommand" 
                                    OnRowDataBound="DrugItemPricesGridView_RowDataBound"
                                    onrowediting="DrugItemPricesGridView_RowEditing" 
                                    onrowcancelingedit="DrugItemPricesGridView_RowCancelingEdit"
                                    OnRowCreated="DrugItemPricesGridView_OnRowCreated"
                                    OnPreRender="DrugItemPricesGridView_OnPreRender"
                                    AllowSorting="True" 
                                    EditRowStyle-CssClass="DrugItemEditRowStyle" 
                                    ondatabound="DrugItemPricesGridView_DataBound" 
                                    ShowWhenEmpty="true" >
                                <HeaderStyle CssClass="DrugItemGridHeaders" />
                                <RowStyle  CssClass="DrugItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="DrugItemGridAltItems" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
             
                                <Columns>         
                                     <asp:TemplateField >  
                                        <ItemTemplate>
                                               <asp:Button runat="server" ID="OpenItemPriceDetailsForItemPriceButton" Text="Details" OnCommand="DrugItemPricesGridView_ButtonCommand"   CommandName="OpenItemPriceDetailsForItemPrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemPriceId") + "," + Eval("DrugItemPriceHistoryId") %>' Width="64px"   >
                                              </asp:Button>   
                          
                                        </ItemTemplate>
                                    </asp:TemplateField>      
                                   
                                    <asp:TemplateField >  
                                        <ItemTemplate>
                                               <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="DrugItemPricesGridView_ButtonCommand"   CommandName="EditPrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemPriceId") %>'   Width="60px"   >
                                              </asp:Button>   

                                               <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="DrugItemPricesGridView_ButtonCommand"   CommandName="SavePrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemPriceId") %>'   Width="60px"   >
                                              </asp:Button>   

                                               <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="DrugItemPricesGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("DrugItemPriceId") %>'   Width="60px"   >
                                              </asp:Button>   
                          
                                        </ItemTemplate>
                                    </asp:TemplateField>                                          
                                    
                                     <asp:TemplateField HeaderText="Price Start Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceStartDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "PriceStartDate", "{0:d}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Panel ID="DrugItemPriceStartDatePanel" runat="server" DefaultButton="SaveButton" >
                                                <asp:TextBox ID="priceStartDateTextBox" runat="server" onkeydown="simulateDefaultButton(event);" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "PriceStartDate", "{0:d}" )%>' ></asp:TextBox>
                                            </asp:Panel>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField HeaderText="Price End Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceEndDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "PriceEndDate", "{0:d}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Panel ID="DrugItemPriceEndDatePanel" runat="server" DefaultButton="SaveButton" >
                                                <asp:TextBox ID="priceEndDateTextBox" runat="server" onkeydown="simulateDefaultButton(event);" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "PriceEndDate", "{0:d}" )%>' ></asp:TextBox>
                                            </asp:Panel>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField HeaderText="Price"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceLabel" runat="server"  Width="80px" OnDataBinding="PriceLabel_OnDataBinding" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Panel ID="DrugItemPricePanel" runat="server" DefaultButton="SaveButton" >
                                                <asp:TextBox ID="priceTextBox" runat="server" onkeydown="simulateDefaultButton(event);" Width="80px" OnDataBinding="PriceTextBox_OnDataBinding" ></asp:TextBox>
                                            </asp:Panel>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField HeaderText="FCP"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="fcpLabel1" runat="server"  Width="80px"  OnDataBinding="FCP_OnDataBinding"  ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="fcpLabel2" runat="server"  Width="80px" OnDataBinding="FCP_OnDataBinding"   ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    
                                    <asp:CheckBoxField runat="server" DataField="IsTemporary" HeaderText="TPR"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsFSS" HeaderText="FSS"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsBIG4" HeaderText="BIG4" HeaderStyle-CssClass="SmallVerticalText"  />
                                    <asp:CheckBoxField runat="server" DataField="IsVA" HeaderText="VA" HeaderStyle-CssClass="SmallVerticalText"  />
                                    <asp:CheckBoxField runat="server" DataField="IsBOP" HeaderText="BOP"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsCMOP" HeaderText="CMOP" HeaderStyle-CssClass="SmallVerticalText"  />
                                    <asp:CheckBoxField runat="server" DataField="IsDOD" HeaderText="DOD" HeaderStyle-CssClass="SmallVerticalText"   />
                                    <asp:CheckBoxField runat="server" DataField="IsHHS" HeaderText="HHS"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsIHS" HeaderText="IHS"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsIHS2" HeaderText="IHS2" HeaderStyle-CssClass="SmallVerticalText"  />
                                    <asp:CheckBoxField runat="server" DataField="IsDIHS" HeaderText="DIHS" HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsNIH" HeaderText="NIH"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsPHS" HeaderText="PHS"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsSVH" HeaderText="SVH" HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsSVH1" HeaderText="SVH1" HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsSVH2" HeaderText="SVH2"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsTMOP" HeaderText="TMOP"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsUSCG" HeaderText="USCG"  HeaderStyle-CssClass="SmallVerticalText" />
                                    <asp:CheckBoxField runat="server" DataField="IsFHCC" HeaderText="FHCC"  HeaderStyle-CssClass="SmallVerticalText" />

                                     <asp:TemplateField HeaderText="Sub-Item Identifier"  ItemStyle-Width="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="subItemIdentifierLabel" Width="40px" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SubItemIdentifier" ) %>' >
                                            </asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="subItemIdentifierDropDownList" DataValueField="DrugItemSubItemId"  Width="40px"   DataTextField="SubItemIdentifier" runat="server" OnDataBound="SubItemIdentifierDropDownList_DataBound" AutoPostBack="true" >
                                            </asp:DropDownList>
                                        </EditItemTemplate>
                                     </asp:TemplateField>

                                     <asp:TemplateField HeaderText="Last Modified By"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModificationTypeLabel1" runat="server"  Width="90px"  Text='<%# CMGlobals.GetLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModificationTypeLabel2" runat="server" Width="90px" Text='<%# CMGlobals.GetLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:BoundField DataField="LastModificationDate" HeaderText="Modification Date" DataFormatString="{0:d}" ReadOnly="true" />        
                                                                
                                    <asp:TemplateField >
                                        <ItemTemplate>
                                            <asp:Button runat="server"  ID="RemovePriceButton" Text="Remove Price" OnCommand="DrugItemPricesGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("DrugItemPriceId") %>'   Width="82px" CssClass="ButtonWrapText" >            
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                    </asp:TemplateField>                                
                                
                                    <asp:TemplateField HeaderText="VAIFF"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                       <ItemTemplate>
                                           <asp:Label ID="VAIFFLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "VAIFF" )%>' ></asp:Label>
                                       </ItemTemplate>
                                       <EditItemTemplate>
                                           <asp:Label ID="VAIFFLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "VAIFF" )%>' ></asp:Label>
                                       </EditItemTemplate>
                                   </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Dual Price Designation"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="DualPriceDesignationLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "DualPriceDesignation" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="DualPriceDesignationLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "DualPriceDesignation" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField HeaderText="Is From History"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="IsFromHistoryLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "IsFromHistory" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="IsFromHistoryLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IsFromHistory" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Is History From Archive"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="IsHistoryFromArchiveLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "IsHistoryFromArchive" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="IsHistoryFromArchiveLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IsHistoryFromArchive" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="TPR Always Has Base Price"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="TPRAlwaysHasBasePriceLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "TPRAlwaysHasBasePrice" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="TPRAlwaysHasBasePriceLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "TPRAlwaysHasBasePrice" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                    
                                <EmptyDataTemplate>
                                    <asp:Label ID="PriceGridEmptyDataLabel" Width="98%" runat="server" Text="There are no prices for the selected item." ></asp:Label>
                                </EmptyDataTemplate>
                            </ep:GridView>
                          </ContentTemplate>
                      </asp:UpdatePanel>
                   </div>
                </td>
            </tr>
            <tr >
                <td>
                    <div id="HiddenDiv"  style="width:880px; height:0px; border:none; background-color:White; margin:0px;" >
                        <asp:HiddenField ID="RefreshDrugItemScreenOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RefreshDrugItemPriceScreenOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RebindDrugItemScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RebindDrugItemPriceScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RefreshItemHeaderCountOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="ServerConfirmationDialogResults" runat="server" Value="false" /> 
                   </div>
                </td>
            </tr>
        </table>
        
         <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />
    </form>
</body>
</html>
