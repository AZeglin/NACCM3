<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ContractSelect.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractSelect" %>

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
    <form id="ContractSelectForm" runat="server"  onprerender="ContractSelectForm_OnPreRender" oninit="ContractSelectForm_OnInit" >
      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="ContractSelectScriptManager_OnAsyncPostBackError" >
          
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

            if (postbackElement.id.toLowerCase().indexOf('contractselectgridview') > -1) 
            {
                $get("DocumentSelectGridViewDiv").scrollTop = itemScrollValue;
            }

            highlightContractRow();
        }
       
        function simulateDefaultButton(evt) 
        {
            if (evt.which || evt.keyCode) 
            {
                if ((evt.which == 13) || (evt.keyCode == 13)) 
                {
                    __doPostBack( '<%= ContractSelectFilterButton.UniqueID  %>', '' );
                    return false;   
                }
                return true;
            }
        }


        function highlightContractRow()
        {
            var selectedRowIndex = $get("highlightedContractRow").value;
            if( selectedRowIndex == -1 )
            {
                return;
            }
            
            var contractSelectGridView = $get( "ContractSelectGridView" );
            var currentSelectedRow = null;
            if (contractSelectGridView != null)
            {
                currentSelectedRow = contractSelectGridView.rows[selectedRowIndex];
            }
            if( currentSelectedRow != null )
            {
                currentSelectedRow.style.backgroundColor = '#E3FBDD';
                currentSelectedRow.className = 'CMGridSelectedCellStyle';
            }
        }

        function unHighlightContractRow() 
        {
            var selectedRowIndex = $get("highlightedContractRow").value;
            var highlightedRowOriginalColor = $get("highlightedContractRowOriginalColor").value;
            
            if (selectedRowIndex == -1) {
                return;
            }

            var contractSelectGridView = $get("ContractSelectGridView");
            var currentSelectedRow = null;
            if (contractSelectGridView != null) 
            {
                currentSelectedRow = contractSelectGridView.rows[selectedRowIndex];
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
            $get("ContractSelectGridViewUpdatePanel").Update;
        }
        
        function openContractDetailsWindow(contractButton, rowIndex, contractNumber, scheduleNumber, rowColor) 
        {
            unHighlightContractRow();

            $get("highlightedContractRow").value = rowIndex;
            $get("highlightedContractRowOriginalColor").value = rowColor;

            highlightContractRow();

            $get("ContractSelectGridViewUpdatePanel").Update;

            var w = window.open("NAC_CM_Contracts.aspx?CntrctNum=" + contractNumber + "&SchNum=" + scheduleNumber, "ContractDetails", "toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=80,left=170,width=910,height=730", "true");
            w.focus();
        }

        function setContractHighlightedRowIndexAndOriginalColor(rowIndex, rowColor) 
        {
            unHighlightContractRow();

            $get("highlightedContractRow").value = rowIndex;
            $get("highlightedContractRowOriginalColor").value = rowColor;
        }

        function contractNumberMouseChange(contractButton, overOut, rowIndex, rowColor) 
        {
            if( overOut == 'over' )
            {
                contractButton.style.color = 'purple';
                contractButton.style.cursor='hand';
            }
            else 
            {
                contractButton.style.color = 'black';
                contractButton.style.cursor = 'pointer';            
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
    <asp:Panel ID="ContractSelectPanel" runat="server" BackColor="Brown" Width="100%" >
       <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8;" 
            border="solid 1px black" >
            <col style="width:98%;" />
            <tr style="border: solid 1px black; vertical-align:top;">
                <td >
                    <div id="DocumentSelectHeaderDiv"  runat="server"   >
                        <asp:UpdatePanel ID="ContractSelectHeaderUpdatePanel" runat="server"  
                            UpdateMode="Conditional" ChildrenAsTriggers="true" >
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>   
                             <ContentTemplate>
                                  <table width="100%" style="table-layout:auto" >
                                  <col style="width:98%;" />
                                    <tr  style="text-align:center;" >
                                        <td >
                                            <asp:Label ID="ContractSelectFormTitle" Text="Contract Select" runat="server"  Font-Names="Arial" ForeColor="#333333" Font-Size="X-Large" />    
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
                                                             <asp:ListItem Text="Contract Number" Value="ContractNumber" />
                                                             <asp:ListItem Text="Contracting Officer" Value="ContractingOfficer" />
                                                             <asp:ListItem Text="Contractor Name" Value="Vendor" />
                                                             <asp:ListItem Text="Commodity Covered" Value="Description" />
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
                                                        <asp:UpdateProgress ID="ContractFilterUpdateProgress" runat="server" >
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
                                                        <ep:TextBox ID="ContractSelectFilterValueTextBox" runat="server" Width="138px" TabIndex="2" EnableViewState="true" onkeydown="simulateDefaultButton(event);" />
                                                    </td>
                                                    <td align="left">
                                                        <asp:Button ID="ContractSelectFilterButton" runat="server" Width="78px" Text="Filter" TabIndex="3" AccessKey="F" OnClick="ContractSelectFilterButton_OnClick"  CausesValidation="false"  />
                                                    </td>
                                                    <td align="left" >
                                                        <asp:Button ID="ContractSelectViewAllButton" runat="server" Width="88px" Text="View All" AccessKey="V" TabIndex="4" ToolTip="View All Active Contracts" OnClick="ContractSelectViewAllButton_OnClick" />
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
                   <input id="highlightedContractRow" runat="server" type="hidden" value="1" enableviewstate="true"  />
                   <input id="highlightedContractRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
                   <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                   <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
                   <input id="ContextMenuRowIndex_ContractSelectGridView" runat="server" type="hidden" value="-1" enableviewstate="true" />

                   <div id="DocumentSelectGridViewDiv"  runat="server" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                           <ep:UpdatePanelEventProxy ID="ChangeItemHighlightUpdatePanelEventProxy" runat="server" />
                           <ep:UpdatePanelEventProxy ID="RightClickUpdatePanelEventProxy" runat="server" />

                          <asp:UpdatePanel ID="ContractSelectGridViewUpdatePanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
                           <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ChangeItemHighlightUpdatePanelEventProxy" EventName="ProxiedEvent" />
                                <asp:AsyncPostBackTrigger ControlID="RightClickUpdatePanelEventProxy" EventName="ProxiedEvent" />
                           </Triggers>
                            <ContentTemplate> 
                                <ep:ContextMenu ID="ContractSelectContextMenu" runat="server" ActivateOnRightClick="True" RolloverColor="#E5FDDF"  ForeColor="Black" BackColor="White" IsContextMenuInUpdatePanel="True" >
                                    <ContextMenuItems>
                                    </ContextMenuItems>
                                </ep:ContextMenu>
                                 <ep:GridView ID="ContractSelectGridView" 

                                            runat="server" 
                                            EnableViewState="true"
                                            DataKeyNames="Contract_Record_ID, Schedule_Number"  
                                            AutoGenerateColumns="False" 
                                            Width="100%" 
                                            RowStyle-Height="36px"
                                            CssClass="CMGrid" 
                                            Visible="True" 
                                            AllowSorting="True" 
                                            OnSorting="ContractSelectGridView_OnSorting"
                                            OnRowDataBound="ContractSelectGridView_RowDataBound"
                                            AutoGenerateEditButton="false"
                                            EditRowStyle-CssClass="CMGridEditRowStyle" 
                                            onprerender="ContractSelectGridView_PreRender" 
                                            OnInit="ContractSelectGridView_Init"
                                            EmptyDataRowStyle-CssClass="CMGrid" 
                                            EmptyDataText="There are no contracts matching the selected filter criteria."
                                            ContextMenuID="ContractSelectContextMenu"
                                           >
                                        <HeaderStyle CssClass="DocumentSelectGridHeaders" />
                                        <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                        <AlternatingRowStyle CssClass="CMGridAltItems" />
                                        <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                        <Columns>   
                                            
                                            <asp:TemplateField  ControlStyle-Width="80px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ContractNumberColumnHeaderButton" Text="Contract Number" OnCommand="ContractSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="CntrctNum" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                     <asp:Label runat="server" ID="SelectContractButton" Text='<%# Eval( "CntrctNum" ) %>' />                                 
                                                </ItemTemplate>
                                            </asp:TemplateField>      

                                            <asp:BoundField DataField="Schedule_Name" HeaderText="Schedule" 
                                                SortExpression="Schedule_Name" />
                                                                                       
                                            <asp:BoundField DataField="CO_Name" HeaderText="CO" 
                                                SortExpression="CO_Name" />
                                           
                                            <asp:BoundField DataField="Contractor_Name" HeaderText="Contractor Name" 
                                                SortExpression="Contractor_Name" />
                                           
                                            <asp:BoundField DataField="Drug_Covered" HeaderText="Commodity Covered" 
                                                SortExpression="Drug_Covered"   />
                                           
                                            <asp:BoundField DataField="Dates_CntrctAward" HeaderText="Dates Awarded" 
                                                SortExpression="Dates_CntrctAward" DataFormatString="{0:d}" />
                                              
                                            <asp:TemplateField  ControlStyle-Width="80px" >   
                                                <HeaderTemplate>
                                                    <asp:LinkButton runat="server" ID="ExpirationDateColumnHeaderButton" Text="Expiration Date" OnCommand="ContractSelectGridView_ButtonCommand"  CommandName="Sort" CommandArgument="Dates_CntrctExp" >
                                                    </asp:LinkButton>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <table  >
                                                        <tr>
                                                            <td  style="border:none;">
                                                                <asp:Label runat="server" ID="ContractExpirationDate" Text='<%# Eval( "Dates_CntrctExp", "{0:d}" ) %>' />
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
    <ep:MsgBox ID="MsgBox" NameofMsgBox="ContractSelect" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />
    </asp:Panel>
 </form>
</body>
</html>
