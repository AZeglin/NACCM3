<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="OfferSelect.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferSelect" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
  <meta http-equiv="X-UA-Compatible" content="IE=7">
    <title></title>
    <link href="./App_Themes/CMStandard/CMStandard.css" rel="stylesheet" type="text/css" />

</head>
<body style="background-color:#ece9d8;">
    <form id="OfferSelectForm" runat="server"  onprerender="OfferSelectForm_OnPreRender" oninit="OfferSelectForm_OnInit" >
      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="OfferSelectScriptManager_OnAsyncPostBackError" >
          
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
            $get("DocumentSelectGridViewDiv").scrollTop = newPositionValue;
        }
            
        var postbackElement;

        function beginRequest(sender, args) 
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

            if (postbackElement.id.toLowerCase().indexOf('offerselectgridview') > -1) 
            {
                $get("DocumentSelectGridViewDiv").scrollTop = itemScrollValue;
            }

            highlightOfferRow();
        }
       
        function simulateDefaultButton(evt) 
        {
            if (evt.which || evt.keyCode) 
            {
                if ((evt.which == 13) || (evt.keyCode == 13)) 
                {
                    __doPostBack( '<%= OfferSelectFilterButton.UniqueID  %>', '' );
                    return false;   
                }
                return true;
            }
        }


        function highlightOfferRow()
        {
            var selectedRowIndex = $get("highlightedOfferRow").value;
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var offerSelectGridView = $get( "OfferSelectGridView" );
            var currentSelectedRow = null;
            if (offerSelectGridView != null)
            {
                currentSelectedRow = offerSelectGridView.rows[selectedRowIndex];
            }
            if (currentSelectedRow != null) 
            {
                currentSelectedRow.style.backgroundColor = '#E3FBDD';
                currentSelectedRow.className = 'CMGridSelectedCellStyle';
            }
        }

        function unHighlightOfferRow() 
        {
            var selectedRowIndex = $get("highlightedOfferRow").value;
            var highlightedRowOriginalColor = $get("highlightedOfferRowOriginalColor").value;
            
            if (selectedRowIndex == -1) {
                return;
            }

            var offerSelectGridView = $get("OfferSelectGridView");
            var currentSelectedRow = null;
            if (offerSelectGridView != null) 
            {
                currentSelectedRow = offerSelectGridView.rows[selectedRowIndex];
            }
            if (currentSelectedRow != null) 
            {
                if (highlightedRowOriginalColor == 'alt') 
                {
                    currentSelectedRow.style.backgroundColor = 'white';
                    currentSelectedRow.className = 'DocumentSelectUnSelectedCellStyleAlt';
                }
                else if (highlightedRowOriginalColor == 'norm') 
                {
                    currentSelectedRow.style.backgroundColor = '#F7F6F3';
                    currentSelectedRow.className = 'DocumentSelectUnSelectedCellStyleNorm';
                }
            }
        }

        function refreshSearchWindowFromContextMenu() 
        {
            $get("OfferSelectGridViewUpdatePanel").Update;
        }
        
        function openOfferDetailsWindow(offerButton, rowIndex, offerId, scheduleNumber, COID, vendor, contractNumber, receivedDate, assignedDate, completeString, rowColor) 
        {
            unHighlightOfferRow();

            $get("highlightedOfferRow").value = rowIndex;
            $get("highlightedOfferRowOriginalColor").value = rowColor;

            highlightOfferRow();

            $get("OfferSelectGridViewUpdatePanel").Update;

            var w = window.open("NAC_Offers_Edit.aspx?OfferID=" + offerId + "&scheduleNumber=" + scheduleNumber + "&COID=" + COID + "&vendor=" + vendor + "&contractNumber=" + contractNumber + "&receivedDate=" + receivedDate + "&assignedDate=" + assignedDate + "&completeString=" + completeString, "OfferDetails", "toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=90,left=170,width=912,height=730", "true");
            w.focus();
        }

        function openContractWindowFromOffer(contractButton, rowIndex, contractNumber, scheduleNumber, rowColor) {
            unHighlightOfferRow();

            $get("highlightedOfferRow").value = rowIndex;
            $get("highlightedOfferRowOriginalColor").value = rowColor;

            highlightOfferRow();

            $get("OfferSelectGridViewUpdatePanel").Update;

            var w = window.open("NAC_CM_Contracts.aspx?CntrctNum=" + contractNumber + "&SchNum=" + scheduleNumber, "ContractDetails", "toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=80,left=170,width=910,height=730", "true");
            w.focus();
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

        function setOfferHighlightedRowIndexAndOriginalColor(rowIndex, rowColor) 
        {
            unHighlightOfferRow();

            $get("highlightedOfferRow").value = rowIndex;
            $get("highlightedOfferRowOriginalColor").value = rowColor;
        }

        function vendorMouseChange(offerButton, overOut, rowIndex, rowColor) 
        {
            if( overOut == 'over' )
            {
                offerButton.style.color = 'purple';
                offerButton.style.cursor='hand';
            }
            else 
            {
                offerButton.style.color = 'black';
                offerButton.style.cursor = 'pointer';            
            }
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
    <asp:Panel ID="OfferSelectPanel" runat="server" BackColor="Brown" Width="100%" >
       <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8;" 
            border="solid 1px black" >
            <col style="width:98%;" />
            <tr style="border: solid 1px black; vertical-align:top;">
                <td >
                    <div id="DocumentSelectHeaderDiv"  runat="server"   >
                        <asp:UpdatePanel ID="OfferSelectHeaderUpdatePanel" runat="server"  
                            UpdateMode="Conditional" ChildrenAsTriggers="true" >
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>   
                             <ContentTemplate>
                                  <table width="100%" style="table-layout:auto" >
                                  <col style="width:98%;" />
                                    <tr  style="text-align:center;" >
                                        <td >
                                            <asp:Label ID="OfferSelectFormTitle" Text="Offer Select" runat="server"  Font-Names="Arial" ForeColor="#333333" Font-Size="X-Large" />    
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="left">
                                            <table id="DocumentSelectHeaderTable" width="100%"  style="height:50px; table-layout:auto" >
                                              <col style="width:22%;" />
                                              <col style="width:20%;" />
                                              <col style="width:8%;" />
                                              <col style="width:6%;" />
                                              <col style="width:22%;" />
                                              <col style="width:20%;" />
                                                <tr>
                                                    <td align="right">
                                                        <asp:Label ID="FilterByLabel" Text="Filter By:" runat="server" />
                                                    </td>
                                                    <td>
                                                        <asp:DropDownList ID="SearchDropDownList" runat="server" Width="138px" TabIndex="1">
                                                             <asp:ListItem Text="Contracting Officer" Value="ContractingOfficer" />
                                                             <asp:ListItem Text="Contractor Name" Value="Vendor" />
                                                             <asp:ListItem Text="Status" Value="Status" />
                                                             <asp:ListItem Text="Schedule" Value="Schedule" />
                                                         </asp:DropDownList>
                                                     </td>    
                                                     <td>
                                                     </td>
                                                     <td>
                                                     </td>
                                                     <td>
                                                     </td>
                                                     <td  style="text-align:center;" rowspan="2" >
                                                        <asp:UpdateProgress ID="OfferFilterUpdateProgress" runat="server" >
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
                                                </tr>
                                                <tr>
                                                    <td align="right">
                                                        <asp:Label ID="SearchForLabel" Text="Search For:" runat="server" />
                                                    </td>
                                                    <td >
                                                        <ep:TextBox ID="OfferSelectFilterValueTextBox" runat="server" Width="138px" TabIndex="2" EnableViewState="true" onkeydown="simulateDefaultButton(event);" />
                                                    </td>
                                                    <td align="left">
                                                        <asp:Button ID="OfferSelectFilterButton" runat="server" Width="78px" Text="Filter" TabIndex="3" AccessKey="F" OnClick="OfferSelectFilterButton_OnClick"  CausesValidation="false"  />
                                                    </td>
                                                    <td align="left" >
                                                        <asp:Button ID="OfferSelectViewAllButton" runat="server" Width="88px" Text="View All" AccessKey="V" TabIndex="4" ToolTip="View All Open Offers" OnClick="OfferSelectViewAllButton_OnClick" />
                                                    </td>
                                                    <td>
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
                    </div>
                </td>
            </tr>

            <tr style="border: solid 1px black; vertical-align:top;">
                <td>
                   <input id="scrollPos" runat="server" type="hidden" value="0" enableviewstate="true"  />
                   <input id="highlightedOfferRow" runat="server" type="hidden" value="1" enableviewstate="true"  />
                   <input id="highlightedOfferRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                   <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                   <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                   <input id="ContextMenuRowIndex_OfferSelectGridView" runat="server" type="hidden" value="-1" enableviewstate="true" />

                   <div id="DocumentSelectGridViewDiv"  runat="server" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="RightClickUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="OfferSelectGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="RightClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                                <ep:ContextMenu ID="OfferSelectContextMenu" runat="server" ActivateOnRightClick="True" RolloverColor="#E5FDDF"  ForeColor="Black" BackColor="White" IsContextMenuInUpdatePanel="True" >
                                    <ContextMenuItems>
                                    </ContextMenuItems>
                                </ep:ContextMenu>
                                 <ep:GridView ID="OfferSelectGridView" 

                                            runat="server" 
                                            EnableViewState="true"
                                            DataKeyNames="Offer_ID, Schedule_Number"  
                                            AutoGenerateColumns="False" 
                                            Width="100%" 
                                            RowStyle-Height="36px"
                                            CssClass="CMGrid" 
                                            Visible="True" 
                                            AllowSorting="True" 
                                            OnSorting="OfferSelectGridView_OnSorting"
                                            OnRowDataBound="OfferSelectGridView_RowDataBound"
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="CMGridEditRowStyle" 
                                            onprerender="OfferSelectGridView_PreRender" 
                                            OnInit="OfferSelectGridView_Init"
                                            EmptyDataRowStyle-CssClass="CMGrid" 
                                            EmptyDataText="There are no offers matching the selected filter criteria."
                                           >
                                        <HeaderStyle CssClass="DocumentSelectGridHeaders" />
                                        <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="CMGridAltItems" />
                                        <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                            
                                            <asp:TemplateField  ControlStyle-Width="200px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractorNameColumnHeaderButton" Text="Vendor Name" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Contractor_Name" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="SelectOfferButton" Text='<%# Eval( "Contractor_Name" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>      


                                            <asp:TemplateField  ControlStyle-Width="85px" >                                             
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ReceivedDateColumnHeaderButton" Text="Date Received" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_Received" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ReceivedDateLabel" Text='<%# Eval( "Dates_Received", "{0:d}" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>    

                                            <asp:TemplateField  ControlStyle-Width="85px" >                                             
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="AssignmentDateColumnHeaderButton" Text="Date Assigned" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_Assigned" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="AssignmentDateLabel" Text='<%# Eval( "Dates_Assigned", "{0:d}" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     
                                                      
                                             <asp:TemplateField  ControlStyle-Width="160px" >  
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ScheduleNameColumnHeaderButton" Text="Schedule Name" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Schedule_Name" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ScheduleNameLabel" Text='<%# Eval( "Schedule_Name" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                             <asp:TemplateField  ControlStyle-Width="100px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractingOfficerNameColumnHeaderButton" Text="Contracting Officer" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="LastName" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="ContractingOfficerNameLabel" Text='<%# Eval( "FullName" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:TemplateField  ControlStyle-Width="70px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="OfferStatusColumnHeaderButton" Text="Status" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Action_Description" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="OfferStatusLabel" Text='<%# Eval( "Action_Description" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:TemplateField  ControlStyle-Width="60px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractNumberHeaderButton" Text="Contract" OnCommand="OfferSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="ContractNumber" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="SelectContractButton" Text='<%# Eval( "ContractNumber" ) %>' /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>     

                                            <asp:BoundField Visible="false" ShowHeader="false" DataField="LastName"  />

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
                    <asp:HiddenField ID="RefreshOfferSelectScreenOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="RebindOfferSelectScreenOnRefreshOnSubmit" runat="server" Value="false" /> 
                    <asp:HiddenField ID="ReselectPreviouslySelectedItem" runat="server" Value="false" /> 
                </div>
            </td>
        </tr>
    </table>
    <ep:MsgBox ID="MsgBox" NameofMsgBox="OfferSelect" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />
    </asp:Panel>
 </form>
</body>
</html>
