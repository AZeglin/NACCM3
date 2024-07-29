<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" EnableEventValidation="false"  CodeBehind="Items.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.Items" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register src="ItemHeader.ascx" tagname="ItemHeader" tagprefix="uc1" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html /> 
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Med/Surg Items</title>
    
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
    
    <form id="medSurgItemForm" name="medSurgItemForm" runat="server"  style="position:absolute; top:0px; left:0px; width:100%; height:100%;" >

    <asp:ScriptManager ID="ItemsScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ItemsScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>
    
    <script type="text/javascript" >
    
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest( beginRequest );
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded( pageLoaded );
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(ActivateItemGridSpinner);

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
            $get("ItemsGridViewDiv").scrollTop = newPositionValue;       
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
            $get( "ItemPricesGridViewDiv" ).scrollTop = newPositionValue;
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

            var itemScrollValue = $get("scrollPos").value;
            var priceScrollValue = $get( "priceScrollPos" ).value;

            if (postbackElement.id.toLowerCase().indexOf('itemsgridview') > -1)
            {
                $get( "ItemsGridViewDiv" ).scrollTop = itemScrollValue;
                
            }
            if( postbackElement.id.toLowerCase().indexOf('itempricesgridview' ) > -1 )
            {
                $get( "ItemPricesGridViewDiv" ).scrollTop = priceScrollValue;
                
                $get( "ItemsGridViewDiv" ).scrollTop = itemScrollValue;                
            }
            
            highlightItemRow();
    
        }
        
        function SaveItemGridViewRowHeight()
        {
            var itemGridView = $get("ItemsGridView");
            var i = 0;
            var x = 0;
            var avg = 0;
            var hf = $get("avgRowHeight");
            var oldAvg = 0;
            if (hf != "0") {
                oldAvg = $get("avgRowHeight").value;
            }
            if (itemGridView != "0") {
                $(itemGridView)
                        .find('tr')
                            .each(function (row) {
                                i++;
                                x += $(this).height();
                            });
            }

            if (i != 0) {
                avg = x / i;
                    $get("avgRowHeight").value = avg;
                }
           
        }

        function highlightItemRow()
        {
            var selectedRowIndex = $get("highlightedItemRow").value;
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var itemGridView = $get( "ItemsGridView" );
            var currentSelectedRow = null;
            if (itemGridView != null)
            {
                currentSelectedRow = itemGridView.rows[selectedRowIndex];
            }
            if( currentSelectedRow != null )
            {
                currentSelectedRow.className = 'ItemSelectedCellStyle';
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

        function setItemHighlightedRowIndexAndOriginalColor( rowIndex, originalColor )
        {
            $get("highlightedItemRow").value = rowIndex;
            $get("highlightedItemRowOriginalColor").value = originalColor;
            
            highlightItemRow();
        }
        
        function highlightItemPriceRow()
        {
            var selectedRowIndex = $get("highlightedItemPriceRow").value;
            
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var itemPricesGridView = $get( "ItemPricesGridView" );
            var currentSelectedRow = null;
            if (itemPricesGridView != null)
            {
                currentSelectedRow = itemPricesGridView.rows[selectedRowIndex];
            }
            if( currentSelectedRow != null )
            {
                currentSelectedRow.className = 'ItemSelectedCellStyle';
            }
        }
        
   
        
        function setItemPriceHighlightedRowIndexAndOriginalColor( rowIndex, originalColor )
        {
            $get("highlightedItemPriceRow").value = rowIndex;
            $get("highlightedItemPriceRowOriginalColor").value = originalColor;
            
            highlightItemPriceRow();
        }
        
        function presentConfirmationMessage( msg )
        {
            $get("confirmationMessageResults").value = confirm( msg ); 
        }
        
        function presentPromptMessage( msg )
        {
            $get("promptMessageResults").value = prompt( msg, "" );
        }

        function ActivateItemGridSpinner(sender, args) {

            EnableProgressIndicator(false);

            var spinnerOptions = {
                lines: 11, // The number of lines to draw
                length: 16, // The length of each line
                width: 4, // The line thickness
                radius: 10, // The radius of the inner circle
                scale: 0.8,  // Scales overall size of the spinner
                corners: 1.0, // Corner roundness (0..1)
                rotate: 0, // The rotation offset
                direction: 1, // 1: clockwise, -1: counterclockwise
                color: '#0A0A0A', // #rgb or #rrggbb
                opacity: 0,          // Opacity of the lines
                speed: 0.9, // Rounds per second
                trail: 70, // Afterglow percentage
                fps: 20,  // Frames per second when using setTimeout()
                shadow: false, // Whether to render a shadow
                hwaccel: false, // Whether to use hardware acceleration
                className: 'spinner', // The CSS class to assign to the spinner
                zIndex: 2e9, // The z-index (defaults to 2000000000)
                top: '50%', // Top position relative to parent in px
                left: '50%', // Left position relative to parent in px
                position: 'absolute'  // Element positioning
            };

            // run spinner and make containing div visible/hidden
            var progressIndicatorDiv = document.getElementById('<%=ItemGridProgressIndicator.ClientID %>');
                  //   alert(progressIndicatorDiv);
            var spinner = new Spinner(spinnerOptions).spin(progressIndicatorDiv);
                  //  alert(spinner);

      }


      function EnableProgressIndicator(bEnable) {
          //    alert('In enable=' + bEnable.toString());
          var progressIndicator = document.getElementById('<%=ItemGridProgressIndicator.ClientID %>');

            if (progressIndicator != null) {
                if (bEnable == true) {
                    progressIndicator.style.visibility = "visible";

                    if (spinner) {

                        spinner.spin(progressIndicator);

                    }
                    //else {
                    //    alert('spinner was null');
                    //}
                }
                else {
                    progressIndicator.style.visibility = "hidden";
                }
            }
            //else {
            //    alert('progressIndicator was null');
            //}
        }

        function HideProgressIndicator() {
            var progressIndicator = document.getElementById('<%=ItemGridProgressIndicator.ClientID %>');
              progressIndicator.style.visibility = "hidden";
        }

    </script>
    

        <table  style="width:100%; height:100%; table-layout:fixed; border:solid 1px black;" >
            <colgroup>
                <col style="width:100%;" />
            </colgroup>
            <tr  style="vertical-align:top; height:14%;">
                <td >
                    <div id="ItemHeaderDiv"  style="width:99%; height:112px; border:solid 1px black; background-color:White; margin:3px;" >
                        <asp:Panel ID="ItemHeaderPanel" runat="server" DefaultButton="SearchButton" >
                        <asp:UpdatePanel ID="ItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="InsertItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="PageGridViewUpdatePanelEventProxy" EventName="ProxiedEvent" />
                        </Triggers>   
                             <ContentTemplate>
                               <table  style="width:100%; border:none; height:90px; table-layout:fixed" align="center" >
                                <tr >
                                    <td style="width:190px; text-align:center;" rowspan="3" >
                                         <div id="ItemGridProgressIndicator" runat="server" class="spinner" clientidmode="Static" style="z-index: 124; height: 73px; width: 73px; position:relative;" >
                                         </div>
                                    </td>
                                    <td style="width:100%; text-align:center;">
                                        <asp:Label ID="MainHeaderTitleLabel1" runat="server" 
                                            Text="Items"  CssClass="ItemHeaderText"/>
                                    </td>
                                    <td style="width:190px; text-align:right; margin-right:10px;">
                                        <asp:Button ID="formCloseButton" runat="server" Height="20px" Text="Close" SkinID="SmallItemButton" />
                                    </td>
                                </tr>
                                <tr >
                                    <td style="width:100%; text-align:center;">
                                        <asp:Label ID="MainHeaderTitleLabel2" runat="server" Text="For Contract" 
                                             CssClass="ItemHeaderText"/>
                                    </td>
                                    <td style="width:200px; text-align:right;">
                                        <table style="height:20px; width:190px; margin:0px; table-layout:fixed;"  >
                                            <tr style="text-align:left; margin:0px;">
                                                <td >
                                                    <asp:Button runat="server"  ID="PrintItemsAndPricesButton" Width="90px" Height="20px" Text="Print Report" OnClick="PrintItemsAndPricesButton_OnClick"  SkinID="SmallItemButton" /> 
                                                </td>
                                                <td >
                                                    <asp:Button runat="server"  ID="PrintScreenButton"  Width="90px" Height="20px" Text="Print Screen" OnClientClick="window.print();"  SkinID="SmallItemButton" /> 
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:center; width:100%;" >
                                        <asp:Label ID="MainHeaderTitleLabel3" runat="server" Text="Vendor" Height="20px" CssClass="ItemRegularText" />
                                    </td>
                                    <td style="width:190px; text-align:right;" >
                                        <asp:Label ID="MainHeaderItemCount" runat="server" Text="Total Items" Height="20px" CssClass="ItemRegularText" />
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
                                                     <asp:Label ID="ItemSearchLabel" runat="server" Text="Search Text" CssClass="ItemSearchText"  />
                                                </td>
                                                <td style="width:144px; text-align:left;" >
                                                    <ep:TextBox ID="ItemSearchTextBox" runat="server" onkeydown="simulateDefaultButton(event);" Width="142px"></ep:TextBox>
                                                </td>
                                                <td style="width:70px; text-align:center;" >
                                                    <asp:Button ID="SearchButton"  Text="Search" runat="server" Width="70px" OnClick="SearchButton_OnClick" OnClientClick="EnableProgressIndicator(true);" />
                                                </td>
                                                <td style="width:102px; text-align:center;" >
                                                    <asp:Button ID="ClearSearchButton"  Text="Clear Search" runat="server" Width="100px" OnClick="ClearSearchButton_OnClick" OnClientClick="EnableProgressIndicator(true);" />
                                                </td>
                                                <td style="width:200px; min-width:200px; text-align:right;" >                             
                                                     <asp:RadioButtonList ID="ItemFilterRadioButtonList" runat="server" Width="69%" CssClass="ItemRadioButtonList" AutoPostBack="True" EnableViewState="true" ToolTip="Filter Items" RepeatDirection="Horizontal"  OnSelectedIndexChanged="ItemFilterRadioButtonList_OnSelectedIndexChanged"  >
                                                        <asp:ListItem Value="A" Selected="True" >Active</asp:ListItem>                                                      
                                                        <asp:ListItem Value="H">Historical</asp:ListItem>
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

            <tr style="border: solid 1px black; vertical-align:top; height:48%;">
                <td >
                    <input id="scrollPos" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="avgRowHeight" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedItemRow" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedItemRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedItemPriceRow" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="highlightedItemPriceRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                    <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                    <input id="ContextMenuRowIndex_ItemsGridView" runat="server" type="hidden" value="-1" enableviewstate="true" />
                    <div id="ItemsGridViewDiv"  runat="server" style="width:100%; height:100%; max-height:352px; min-height:122px; top:113px; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="InsertItemButtonClickUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="SelectParentItemForBPAUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="SelectServiceCategoryUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="RightClickUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="PageGridViewUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="ItemsGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true" OnLoad="ItemsGridViewUpdatePanel_OnLoad">
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="InsertItemButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="SelectParentItemForBPAUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="SelectServiceCategoryUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="RightClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="PageGridViewUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                                <ep:ContextMenu ID="ItemContextMenu" runat="server" ActivateOnRightClick="True" RolloverColor="#E5FDDF"  ForeColor="Black" BackColor="White" IsContextMenuInUpdatePanel="True" >   
                                       <ContextMenuItems>  
                                       </ContextMenuItems>   
                                  </ep:ContextMenu>     

                                 <ep:GridView ID="ItemsGridView" 
                                            runat="server" 
                                            DataKeyNames="ItemId"  
                                            AutoGenerateColumns="False" 
                                            Width="99%" 
                                            CssClass="ItemGrids" 
                                            Visible="True" 
                                            onrowcommand="ItemsGridView_RowCommand" 
                                            OnSelectedIndexChanged="ItemsGridView_OnSelectedIndexChanged" 
                                            OnRowDataBound="ItemsGridView_RowDataBound"
                                            AllowSorting="True" 
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="ItemEditRowStyle" 
                                            onprerender="ItemsGridView_PreRender" 
                                            OnInit="ItemsGridView_Init"
                                            OnRowCreated="ItemsGridView_OnRowCreated"
                                            onrowdeleting="ItemsGridView_RowDeleting" 
                                            onrowediting="ItemsGridView_RowEditing" 
                                            onrowupdating="ItemsGridView_RowUpdating" 
                                            onrowcancelingedit="ItemsGridView_RowCancelingEdit"
                                            AllowInserting="True"                     
                                            OnRowInserting="ItemsGridView_RowInserting" 
                                            
                                            EmptyDataRowStyle-CssClass="ItemGrids" 
                                            EmptyDataText="There are no items for the selected contract."
                                            ContextMenuID="ItemContextMenu"   
                                            PagerID="ItemPager"
                                           >
                                        <HeaderStyle CssClass="ItemGridHeaders" />
                                        <RowStyle  CssClass="ItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="ItemGridAltItems" />                                        
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                                                                               
                                            <asp:TemplateField >   
                                                <ItemTemplate>
                                                     <asp:Button runat="server" ID="RefreshPricesButton" Text="Refresh Prices" OnCommand="ItemsGridView_ButtonCommand" CommandName="RefreshPriceList"  CommandArgument='<%#   Container.DataItemIndex + "," + Eval("ItemId") %>'  Width="70px" CssClass="ButtonWrapText" />                                 
                                                </ItemTemplate>
                                            </asp:TemplateField>      

                                            <asp:TemplateField >  
                                                <ItemTemplate>
                                                       <asp:Button runat="server" ID="OpenItemDetailsForItemButton" Text="Details" OnCommand="ItemsGridView_ButtonCommand"   CommandName="OpenItemDetailsForItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemId") %>'  Width="64px"   >
                                                      </asp:Button>   
                                  
                                                </ItemTemplate>
                                            </asp:TemplateField>   
                                            
                                            <asp:TemplateField >  
                                                <ItemTemplate>
                                                       <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="ItemsGridView_ButtonCommand"   CommandName="EditItem"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemId") %>'   Width="60px"   >
                                                      </asp:Button>   

                                                       <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="ItemsGridView_ButtonCommand"   CommandName="SaveItem" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemId") %>'   Width="60px"   >
                                                      </asp:Button>   

                                                       <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="ItemsGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemId") %>'   Width="60px"   >
                                                      </asp:Button>   
                                  
                                                </ItemTemplate>
                                            </asp:TemplateField>      
                                           
                                         <asp:TemplateField HeaderText="Parent Item"  ItemStyle-Width="260px"  >
                                            <ItemTemplate>
                                                <asp:Label ID="parentItemLabel" Width="260px"  runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ParentCatalogNumber" )%>' >
                                                </asp:Label>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                 <ep:PaddedDropDownList ID="parentItemDropDownList" DataValueField="ItemId"  Width="260px"   DataTextField="OverallDescription" runat="server" 
                                                    OnDataBound="ParentItemDropDownList_OnDataBound" OnSelectedIndexChanged="ParentItemDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                 </ep:PaddedDropDownList>
                                            </EditItemTemplate>
                                         </asp:TemplateField>
                                                                   
                                            
                                        <asp:TemplateField HeaderText="Service Category"  ItemStyle-Width="260px"  >
                                            <ItemTemplate>
                                                <asp:Label ID="serviceCategoryLabel" Width="260px"  runat="server" OnDataBinding="serviceCategoryLabel_OnDataBinding" >
                                                </asp:Label>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                 <ep:PaddedDropDownList ID="serviceCategoryDropDownList" DataValueField="ServiceCategoryId"  Width="260px"   DataTextField="CategorySelected" runat="server" 
                                                    OnDataBound="ServiceCategoryDropDownList_OnDataBound" OnSelectedIndexChanged="ServiceCategoryDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                 </ep:PaddedDropDownList>
                                            </EditItemTemplate>
                                         </asp:TemplateField>
                                                                                                                                                                 
                                         <asp:TemplateField HeaderText="Part Number"  ItemStyle-Width="180px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="catalogNumberLabel1" runat="server"  Width="180px" OnDataBinding="CatalogNumberLabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <ep:TextBox ID="catalogNumberTextBox" runat="server" TextMode="MultiLine" MaxLength="70"  Width="180px" Height="56px" OnDataBinding="CatalogNumberTextBox_OnDataBinding" ></ep:TextBox>
                                                    <asp:Label ID="catalogNumberLabel2" runat="server"  Width="180px" OnDataBinding="CatalogNumberLabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>               
                                                                                        
                                              <asp:TemplateField  HeaderText="Description"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="itemDescriptionLabel1" runat="server" Width="99%" Height="99%" OnDataBinding="ItemDescriptionLabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <ep:TextBox ID="itemDescriptionTextBox" runat="server" TextMode="MultiLine" MaxLength="800"  CssClass="ItemMultilineInEditMode" Width="99%" Height="56px" OnDataBinding="ItemDescriptionTextBox_OnDataBinding" > ></ep:TextBox>
                                                    <asp:Label ID="itemDescriptionLabel2" runat="server" Width="99%" Height="99%" OnDataBinding="ItemDescriptionLabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                          
                                          

                                            <asp:TemplateField  HeaderText="SIN"  ItemStyle-Width="100px" >
                                                <ItemTemplate>
                                                    <asp:Label ID="SINLabel" runat="server" Width="100px" Text='<%# DataBinder.Eval( Container.DataItem, "SIN" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ItemSINDropDownList" DataValueField="SIN"  Width="100px"   DataTextField="SIN" runat="server" OnDataBound="ItemSINDropDownList_OnDataBound" OnSelectedIndexChanged="ItemSINDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >                                                                                               
                                                 </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>                                                
                                          
                                             <asp:TemplateField HeaderText="Package As Priced"  ItemStyle-Width="60px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="packageAsPricedLabel" runat="server" Width="60px" Text='<%# DataBinder.Eval( Container.DataItem, "PackageAsPriced" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                   <asp:DropDownList ID="packageAsPricedDropDownList" DataValueField="PackageAbbreviation" Width="60px"  DataTextField="PackageAbbreviation" runat="server" OnDataBound="packageAsPricedDropDownList_OnDataBound" OnSelectedIndexChanged="packageAsPricedDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" > 
                                                  </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                         
                                             <asp:TemplateField HeaderText="Current Price"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="itemPriceLabel" runat="server" Width="80px" OnDataBinding="ItemPriceLabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="itemPriceLabel" runat="server"  Width="80px" OnDataBinding="ItemPriceLabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="Price Start Date"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="itemPriceStartDateLabel" runat="server"  Width="80px" OnDataBinding="ItemPriceStartDateLabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                     <asp:Label ID="itemPriceStartDateLabel" runat="server"  Width="80px" OnDataBinding="ItemPriceStartDateLabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="Price End Date"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="itemPriceEndDateLabel" runat="server"  Width="80px" OnDataBinding="ItemPriceEndDateLabel_OnDataBinding" ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="itemPriceEndDateLabel" runat="server"  Width="80px" OnDataBinding="ItemPriceEndDateLabel_OnDataBinding" ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                         
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
                                                    <asp:Label ID="lastModificationTypeLabel" runat="server"  Width="90px"  Text='<%# CMGlobals.GetMedSurgLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="lastModificationTypeLabel" runat="server" Width="90px" Text='<%# CMGlobals.GetMedSurgLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                                                 
                                             <asp:TemplateField HeaderText="Reason Moved To History"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="reasonMovedToHistoryLabel" runat="server"  Width="90px"  Text='<%# DataBinder.Eval( Container.DataItem, "ReasonMovedToHistory" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="reasonMovedToHistoryLabel" runat="server" Width="90px" Text='<%# DataBinder.Eval( Container.DataItem, "ReasonMovedToHistory" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Moved To History By"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="movedToHistoryByLabel" runat="server"  Width="90px" Style="word-wrap: normal; word-break: break-all;"  Text='<%# DataBinder.Eval( Container.DataItem, "MovedToHistoryBy" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="movedToHistoryByLabel" runat="server" Width="90px" Style="word-wrap: normal; word-break: break-all;" Text='<%# DataBinder.Eval( Container.DataItem, "MovedToHistoryBy" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Date Moved To History"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="dateMovedToHistoryLabel" runat="server"  Width="90px"  Text='<%# DataBinder.Eval( Container.DataItem, "DateMovedToHistory", "{0:d}" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="dateMovedToHistoryLabel" runat="server" Width="90px" Text='<%# DataBinder.Eval( Container.DataItem, "DateMovedToHistory", "{0:d}" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                                                                               
                                            <asp:TemplateField >
                                                <ItemTemplate>
                                                    <asp:Button runat="server"  ID="RemoveItemAndPricesButton" Text="Remove Item And Prices"  OnCommand="ItemsGridView_ButtonCommand" CommandName="RemoveItemAndItemPrices" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemId") %>'  Width="82px" CssClass="ButtonWrapTextSmaller" >               
                                                        </asp:Button >                                    
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Button runat="server"  ID="RemoveItemAndPricesButton" Text="Remove Item And Prices"  OnCommand="ItemsGridView_ButtonCommand" CommandName="RemoveItemAndItemPrices" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemId") %>'  Width="82px" CssClass="ButtonWrapTextSmaller" >                
                                                        </asp:Button >                                    
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="Parent Item Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ParentItemIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ParentItemId" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ParentItemIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ParentItemId" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                              <asp:TemplateField HeaderText="Service Category Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ServiceCategoryIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ServiceCategoryId" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ServiceCategoryIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ServiceCategoryId" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField HeaderText="ServiceCategorySIN"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ServiceCategorySINLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "SIN" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ServiceCategorySINLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "SIN" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
    
                                            
                                             <asp:TemplateField HeaderText="ParentActive"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ParentActiveLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ParentActive" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ParentActiveLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ParentActive" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            
                                             <asp:TemplateField HeaderText="ParentHistorical"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ParentHistoricalLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ParentHistorical" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ParentHistoricalLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ParentHistorical" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="ItemHistoryId"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="ItemHistoryIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ItemHistoryId" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="ItemHistoryIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "ItemHistoryId" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField HeaderText="Restorable"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                <ItemTemplate>
                                                    <asp:Label ID="RestorableLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "Restorable" )%>' ></asp:Label>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="RestorableLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "Restorable" )%>' ></asp:Label>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                            
                                    </ep:GridView>
                                  
                                </ContentTemplate>
                          </asp:UpdatePanel> 
                 </div>
                </td>
            </tr>
            <tr style="vertical-align:top; height:38px;" >                
                <td style="text-align:right; width:99%; min-width:602px;" >  
                    <asp:UpdatePanel ID="PagerUpdatePanel" runat="server" UpdateMode="Always" ChildrenAsTriggers="true" >
                    <ContentTemplate>    
                        <div id="ItemPagerDiv"  style="width:99%; height:34px; position:absolute; padding-top:4px; background-color:white; border-top:solid 1px black; " >               
                            <ep:Pager ID="ItemPager" runat="server" OnInit="ItemPager_OnInit" AssociatedControlID="ItemsGridView" CssClass="ItemPagerStyle"   EnableViewState="true" OnClientClickScript="EnableProgressIndicator(true);" NextPageImageUrl="~/Images/RightArrowF.gif" PreviousPageImageUrl="~/Images/LeftArrowF.gif" Visible="true"  />                  
                        </div>
                    </ContentTemplate>
                    </asp:UpdatePanel>
                </td>            
            </tr>
            

            <tr style="vertical-align:top; height:10%;" >
                <td>
                    <div id="PriceHeader1Div" style="height: 70px; width: 100%;" > 
                        <asp:UpdatePanel ID="SelectedItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="RefreshPricesButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="PriceListChangeUpdatePanelEventProxy" EventName="ProxiedEvent" />   
                            </Triggers>
                            <ContentTemplate>
                                <uc1:ItemHeader ID="SelectedItemHeader" runat="server" />
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
                                         <asp:CheckBoxList ID="PriceFilterCheckBoxList" runat="server" CssClass="ItemRegularText" AutoPostBack="True" ToolTip="Filter Prices" RepeatDirection="Horizontal" OnSelectedIndexChanged="PriceFilterCheckBoxList_OnSelectedIndexChanged"  >
                                            <asp:ListItem Value="Active" Selected="True">Active</asp:ListItem>
                                            <asp:ListItem Value="Future" Selected="True">Future</asp:ListItem>
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
                    <div id="ItemPricesGridViewDiv" style="height: 100%; width: 100%; max-height:182px; min-height:120px; position:relative; bottom: 0; left: 0; overflow: scroll" runat="server" onscroll="javascript:setPriceScrollForRestore( this );"  onkeypress="javascript:setPriceScrollForRestore( this );"  > 
                      
                      <ep:UpdatePanelEventProxy ID="RefreshPricesButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="InsertPriceButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="EditCancelPriceButtonClickUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="ChangeItemPriceHighlightUpdatePanelEventProxy" runat="server" />
                      <ep:UpdatePanelEventProxy ID="PriceListChangeUpdatePanelEventProxy" runat="server" />

                      <asp:UpdatePanel ID="ItemPricesGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RefreshPricesButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="InsertPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="EditCancelPriceButtonClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="ChangeItemPriceHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            <asp:AsyncPostBackTrigger ControlID="PriceFilterChangeUpdatePanelEventProxy" EventName="ProxiedEvent" />   
                       </Triggers>
                        <ContentTemplate>
                           <ep:GridView ID="ItemPricesGridView" 
                                    runat="server" 
                                    DataKeyNames="ItemPriceId,ItemPriceHistoryId"  
                                    AutoGenerateColumns="False" 
                                    Width="99%" 
                                    CssClass="ItemGrids" 
                                    onrowcommand="ItemPricesGridView_RowCommand" 
                                    OnRowDataBound="ItemPricesGridView_RowDataBound"
                                    onrowediting="ItemPricesGridView_RowEditing" 
                                    onrowcancelingedit="ItemPricesGridView_RowCancelingEdit"
                                    OnRowCreated="ItemPricesGridView_OnRowCreated"
                                    OnPreRender="ItemPricesGridView_OnPreRender"
                                    AllowSorting="True" 
                                    EditRowStyle-CssClass="ItemEditRowStyle" 
                                    ondatabound="ItemPricesGridView_OnDataBound" 
                                    ShowWhenEmpty="true" >
                                <HeaderStyle CssClass="ItemGridHeaders" />
                                <RowStyle  CssClass="ItemGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                <AlternatingRowStyle CssClass="ItemGridAltItems" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
             
                                <Columns>         
                                     <asp:TemplateField ItemStyle-Width="14%" ItemStyle-Height="99%">  
                                        <ItemTemplate>
                                               <asp:Button runat="server" ID="OpenItemPriceDetailsForItemPriceButton" Text="Details" OnCommand="ItemPricesGridView_ButtonCommand"   CommandName="OpenItemPriceDetailsForItemPrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemPriceId") + "," + Eval("ItemPriceHistoryId") %>' Width="99%"   >
                                              </asp:Button>   
                          
                                        </ItemTemplate>
                                    </asp:TemplateField>      
                                   
                                    <asp:TemplateField ItemStyle-Width="14%" ItemStyle-Height="99%">  
                                        <ItemTemplate>
                                               <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="ItemPricesGridView_ButtonCommand"   CommandName="EditPrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemPriceId") %>'   Width="99%"   >
                                              </asp:Button>   

                                               <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="ItemPricesGridView_ButtonCommand"   CommandName="SavePrice"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemPriceId") %>'   Width="48%"   >
                                              </asp:Button>   

                                               <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="ItemPricesGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex + "," + Eval("ItemPriceId") %>'   Width="48%"   >
                                              </asp:Button>   
                          
                                        </ItemTemplate>
                                    </asp:TemplateField>                                          
                                    
                                     <asp:TemplateField HeaderText="Price Start Date"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceStartDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "PriceStartDate", "{0:d}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="priceStartDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "PriceStartDate", "{0:d}" )%>' ></asp:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField HeaderText="Price End Date"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceEndDateLabel" runat="server"  Width="80px"  Text='<%# DataBinder.Eval( Container.DataItem, "PriceStopDate", "{0:d}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="priceEndDateTextBox" runat="server" Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "PriceStopDate", "{0:d}" )%>' ></asp:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                     <asp:TemplateField HeaderText="Price"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="priceLabel" runat="server"  Width="80px" OnDataBinding="PriceLabel_OnDataBinding" ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="priceTextBox" runat="server" Width="80px" OnDataBinding="PriceTextBox_OnDataBinding" ></asp:TextBox>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:CheckBoxField runat="server" DataField="IsTemporary" HeaderText="TPR"  HeaderStyle-CssClass="SmallVerticalText" />

                                     <asp:TemplateField HeaderText="Last Modified By"  ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="lastModificationTypeLabel1" runat="server"  Width="90px"  Text='<%# CMGlobals.GetMedSurgLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="lastModificationTypeLabel2" runat="server" Width="90px" Text='<%# CMGlobals.GetMedSurgLastModificationTypeDescription( DataBinder.Eval( Container.DataItem, "LastModificationType" ))%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                    
                                    <asp:BoundField DataField="LastModificationDate" HeaderText="Modification Date" DataFormatString="{0:d}" ReadOnly="true" />   

                                    <asp:TemplateField HeaderText="Reason Moved To History"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="reasonMovedToHistoryLabel" runat="server"  Width="90px"  Text='<%# DataBinder.Eval( Container.DataItem, "ReasonMovedToHistory" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="reasonMovedToHistoryLabel" runat="server" Width="90px" Text='<%# DataBinder.Eval( Container.DataItem, "ReasonMovedToHistory" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Moved To History By"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="movedToHistoryByLabel" runat="server"  Width="90px" Style="word-wrap: normal; word-break: break-all;"  Text='<%# DataBinder.Eval( Container.DataItem, "MovedToHistoryBy" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="movedToHistoryByLabel" runat="server" Width="90px" Style="word-wrap: normal; word-break: break-all;" Text='<%# DataBinder.Eval( Container.DataItem, "MovedToHistoryBy" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Date Moved To History"  ItemStyle-Width="90px" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Label ID="dateMovedToHistoryLabel" runat="server"  Width="90px"  Text='<%# DataBinder.Eval( Container.DataItem, "DateMovedToHistory", "{0:d}" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="dateMovedToHistoryLabel" runat="server" Width="90px" Text='<%# DataBinder.Eval( Container.DataItem, "DateMovedToHistory", "{0:d}" )%>' ></asp:Label>
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                      <asp:TemplateField ItemStyle-Width="14%" ItemStyle-Height="99%" >
                                        <ItemTemplate>
                                            <asp:Button runat="server"  ID="RemovePriceButton" Text="Remove Price"  OnCommand="ItemPricesGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemPriceId") %>'  Width="99%" CssClass="ButtonWrapTextSmaller" >               
                                                </asp:Button >                                    
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Button runat="server"  ID="RemovePriceButton" Text="Remove Price"  OnCommand="ItemPricesGridView_ButtonCommand" CommandName="RemovePrice" CommandArgument='<%# Container.DataItemIndex + "," + Eval("ItemPriceId") %>'  Width="99%" CssClass="ButtonWrapTextSmaller" >                
                                                </asp:Button >                                    
                                        </EditItemTemplate>
                                    </asp:TemplateField>

                                                                      
                                    <asp:TemplateField HeaderText="Is From History"  >
                                        <ItemTemplate>
                                            <asp:Label ID="IsFromHistoryLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "IsFromHistory" )%>' ></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:Label ID="IsFromHistoryLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "IsFromHistory" )%>' ></asp:Label>
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
                        <asp:HiddenField ID="RefreshItemScreenOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RefreshItemPriceScreenOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RebindItemScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                        <asp:HiddenField ID="RebindItemPriceScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
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
