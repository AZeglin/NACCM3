<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard"  AutoEventWireup="true" EnableEventValidation="false"  CodeBehind="ContractSBAPlan.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractSBAPlan" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>


<asp:Content ID="ContractSBAPlanContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">


 <script type="text/javascript" >
     
    $(document).ready(function() {
        $('#ProjectionGridViewDiv').width($('#ScreenWidthHiddenDiv').width());
    });

     /* called on grid div scroll */
     function setProjectionScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("ProjectionScrollPos").value = divToScroll.scrollTop;
         }
     }

     function setAssociatedContractsScrollForRestore(divToScroll) {

         if (divToScroll != "0") {
             $get("AssociatedContractsScrollPos").value = divToScroll.scrollTop;
         }
     }
     function presentConfirmationMessage(msg) {
         $get("confirmationMessageResults").value = confirm(msg);
     }

     function presentPromptMessage(msg) {
         $get("promptMessageResults").value = prompt(msg, "");
     }

     function pageLoad(sender, args) {
         RestoreProjectionGridSelectionOnAsyncPostback();
         RestoreAssociatedContractsGridSelectionOnAsyncPostback();
     }

     function RestoreProjectionGridSelectionOnAsyncPostback() {
         var ProjectionScrollPos = $get("ProjectionScrollPos").value;
         var highlightedProjectionRow = $get("highlightedProjectionRow").value;

         RestoreProjectionGridSelection(ProjectionScrollPos, highlightedProjectionRow);
     }

     /* called from form load */
     function RestoreProjectionGridSelection(ProjectionScrollPos, highlightedProjectionRow) {
         $get("ProjectionScrollPos").value = ProjectionScrollPos;
         if (ProjectionScrollPos) {
             if (ProjectionScrollPos >= 0) {

                 var theProjectionDiv = document.getElementById('<%=ProjectionGridViewDiv.ClientID %>');
                 if (theProjectionDiv) {
                     theProjectionDiv.scrollTop = ProjectionScrollPos;
                 }
             }
         }

         if (highlightedProjectionRow) {
             if (highlightedProjectionRow >= 0) {
                 $get("highlightedProjectionRow").value = highlightedProjectionRow;
                 highlightProjectionRow();
             }
         }
     }

     function setProjectionHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedProjectionRow").value = rowIndex;
         $get("highlightedProjectionRowOriginalColor").value = originalColor;
         highlightProjectionRow();

     }

     function highlightProjectionRow() {

         var selectedRowIndex = $get("highlightedProjectionRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var ProjectionGridView = document.getElementById("<%=ProjectionGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (ProjectionGridView) {
             currentSelectedRow = ProjectionGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightProjectionRow() {

         var selectedRowIndex = $get("highlightedProjectionRow").value;
         var highlightedProjectionRowOriginalColor = $get("highlightedProjectionRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedProjectionRow").value = -1;
         var ProjectionGridView = document.getElementById("<%=ProjectionGridView.ClientID%>");
         var currentSelectedRow = null;
         if (ProjectionGridView) {
             currentSelectedRow = ProjectionGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedProjectionRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedProjectionRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetProjectionHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "ProjectionGridView") {

             unhighlightProjectionRow();

             $get("highlightedProjectionRow").value = rowIndex;
             $get("highlightedProjectionRowOriginalColor").value = rowColor;

             highlightProjectionRow();
         }
     }

     /* associated contracts grid */
     function RestoreAssociatedContractsGridSelectionOnAsyncPostback() {
         var AssociatedContractsScrollPos = $get("AssociatedContractsScrollPos").value;
         var highlightedAssociatedContractsRow = $get("highlightedAssociatedContractsRow").value;

         RestoreAssociatedContractsGridSelection(AssociatedContractsScrollPos, highlightedAssociatedContractsRow);
     }

     /* called from form load */
     function RestoreAssociatedContractsGridSelection(AssociatedContractsScrollPos, highlightedAssociatedContractsRow) {
         $get("AssociatedContractsScrollPos").value = AssociatedContractsScrollPos;
         if (AssociatedContractsScrollPos) {
             if (AssociatedContractsScrollPos >= 0) {

                 var theAssociatedContractsDiv = document.getElementById('<%=AssociatedContractsGridViewDiv.ClientID %>');
                 if (theAssociatedContractsDiv) {
                     theAssociatedContractsDiv.scrollTop = AssociatedContractsScrollPos;
                 }
             }
         }

         if (highlightedAssociatedContractsRow) {
             if (highlightedAssociatedContractsRow >= 0) {
                 $get("highlightedAssociatedContractsRow").value = highlightedAssociatedContractsRow;
                 highlightAssociatedContractsRow();
             }
         }
     }

     function setAssociatedContractsHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
         $get("highlightedAssociatedContractsRow").value = rowIndex;
         $get("highlightedAssociatedContractsRowOriginalColor").value = originalColor;
         highlightAssociatedContractsRow();

     }

     function highlightAssociatedContractsRow() {

         var selectedRowIndex = $get("highlightedAssociatedContractsRow").value;
         if (selectedRowIndex < 0) {
             return;
         }

         var AssociatedContractsGridView = document.getElementById("<%=AssociatedContractsGridView.ClientID%>"); /* ok */
         var currentSelectedRow = null;
         if (AssociatedContractsGridView) {
             currentSelectedRow = AssociatedContractsGridView.rows[selectedRowIndex];   /* ok */
         }
         if (currentSelectedRow) {
             currentSelectedRow.style.backgroundColor = '#E3FBDD';
             currentSelectedRow.className = 'CMGridSelectedCellStyle';
         }

     }

     function unhighlightAssociatedContractsRow() {

         var selectedRowIndex = $get("highlightedAssociatedContractsRow").value;
         var highlightedAssociatedContractsRowOriginalColor = $get("highlightedAssociatedContractsRowOriginalColor").value;

         if (selectedRowIndex < 0) {
             return;
         }

         $get("highlightedAssociatedContractsRow").value = -1;
         var AssociatedContractsGridView = document.getElementById("<%=AssociatedContractsGridView.ClientID%>");
         var currentSelectedRow = null;
         if (AssociatedContractsGridView) {
             currentSelectedRow = AssociatedContractsGridView.rows[selectedRowIndex];
         }

         if (currentSelectedRow) {
             if (highlightedAssociatedContractsRowOriginalColor == 'alt') {
                 currentSelectedRow.style.backgroundColor = 'white';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleAlt';
             }
             else if (highlightedAssociatedContractsRowOriginalColor == 'norm') {
                 currentSelectedRow.style.backgroundColor = '#F7F6F3';
                 currentSelectedRow.className = 'CMGridUnSelectedCellStyleNorm';
             }
         }
     }

     /* called from onclick */
     function resetAssociatedContractsHighlighting(sourceGridName, rowIndex, rowColor) {
         if (sourceGridName == "AssociatedContractsGridView") {

             unhighlightAssociatedContractsRow();

             $get("highlightedAssociatedContractsRow").value = rowIndex;
             $get("highlightedAssociatedContractsRowOriginalColor").value = rowColor;

             highlightAssociatedContractsRow();
         }
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

<ep:UpdatePanelEventProxy ID="SBAUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="SBAUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="SBAUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>
  

   <table class="OuterTable"  >
        <tr >
              <td style="vertical-align:top;" >
                    <asp:FormView ID="SBAHeaderFormView" runat="server" Width="100%" Height="100%" DefaultMode="Edit" OnPreRender="SBAHeaderFormView_OnPreRender" >
                        <EditItemTemplate>
                        <table class="OutsetBox" style="width: 100%;">
                                <tr class="OutsetBoxHeaderRow" >
                                    <td  style="text-align:center;" colspan="5" >
                                        <asp:Label ID="SBAHeaderFormViewHeaderLabel" runat="server" Text="SBA Plan"  />
                                    </td>
                                </tr>
 
                                <tr>
                                    <td>
                                       <asp:Button runat="server" ID="AddSBAPlanButton" Text="Add New SBA Plan" OnClick="AddNewSBAPlanButton_OnClick"  />
                                    </td>
                                    <td>
                                       <asp:Button runat="server" ID="SelectDifferentSBAPlanButton" Text="Select Different SBA Plan" OnClick="SelectDifferentSBAPlanButton_OnClick"  />
                                    </td>
                                    <td>
                                       <asp:Button runat="server" ID="EditExistingSBAPlanButton" Text="Edit Current SBA Plan" OnClick="EditExistingSBAPlanButton_OnClick"  />
                                    </td>
                                    <td>
                                       <asp:CheckBox runat="server" ID="SBAPlanExemptCheckBox" Text="SBA Plan Exempt" Checked='<%#Bind("SBAPlanExempt")%>' OnCheckedChanged="SBAPlanExemptCheckBox_OnCheckedChanged" AutoPostBack="true" />
                                    </td>
                                    <td>
                                       <asp:Button runat="server" ID="AddSBAProjectionButton" Text="Add New SBA Projection" OnClick="AddNewSBAProjectionButton_OnClick"  />
                                    </td>

                                </tr>
                                <tr>
                                    <td></td>
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
            <tr >
                <td style="vertical-align:top; width:100%; height:100%">
                    <table id="sbaBodyTable" style="width:100%; height:100%">
                        <col style="width:31%;" />
                        <col style="width:69%;" />
                        <tr>                
                            <td style="height:50%; vertical-align:top;">
                                <asp:FormView ID="SBAPlanDetailsFormView" runat="server"  DefaultMode="ReadOnly" OnPreRender="SBAPlanDetailsFormView_OnPreRender" EmptyDataText="No Plan or Exempt" >
                                    <ItemTemplate>
                                    <table class="OutsetBox" >
                                        <col style="width:19%;" />
                                        <col style="width:33%;" />
                                        <col style="width:15%;" />
                                        <col style="width:33%;" />
                                            <tr class="OutsetBoxHeaderRow" >
                                                <td    style="vertical-align:top; text-align:center; "  colspan="4">
                                                    <asp:Label ID="SBAPlanDetailsFormViewHeaderLabel" runat="server" Text="Plan Details"  />
                                                </td>
                                            </tr>
                                            <tr style="border-bottom: 1px solid black;">
                                                <td style="text-align:right;">
                                                    <asp:Label ID="SelectedPlanNameLabel" runat="server" Text="Plan Name: " />
                                                </td>
                                                <td >
                                                    <asp:Label ID="SelectedPlanNameDataLabel" runat="server" Text='<%#Eval("PlanName")%>' style="font-size: 12px; font-family:Arial; font-weight:bold;" />
                                                </td>               
                                                <td style="text-align:right;">
                                                    <asp:Label ID="PlanTypeLabel" runat="server" Text="Plan Type: " />
                                                </td>
                                                <td >
                                                    <asp:Label ID="PlanTypeDataLabel" runat="server" Text='<%#Eval("PlanTypeDescription")%>' />
                                                </td>
                                            </tr>                                                  
                                            <tr>
                                                <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorNameLabel" runat="server" Text="Administrator: " />
                                                </td>
                                                <td >
                                                    <asp:Label ID="AdministratorNameDataLabel" runat="server" Text='<%#Eval("PlanAdministratorName")%>' />
                                                </td>
                                                 <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorEmailLabel" runat="server" Text="Email: " />
                                                </td>
                                                <td rowspan="2">
                                                    <asp:Label ID="AdministratorEmailDataLabel"  runat="server" Text='<%# FormatEmailAddress( Eval( "PlanAdministratorEmail" ))%>'  />
                                                </td>
                                            </tr>
                                            <tr>                                                
                                                <td></td>
                                            </tr>
                                         <tr>
                                                <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorPhoneLabel" runat="server" Text="Phone: " />
                                                </td>
                                                <td>
                                                    <asp:Label ID="AdministratorPhoneDataLabel" runat="server" Text='<%#Eval("PlanAdministratorPhone")%>'    />
                                                </td>
                                                 <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorFaxLabel" runat="server" Text="Fax: " />
                                                </td>
                                                <td > 
                                                    <asp:Label ID="AdministratorFaxDataLabel" runat="server" Text='<%#Eval("PlanAdministratorFax")%>'    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorAddressLabel" runat="server" Text="Address: " />
                                                </td>
                                                <td rowspan="2" colspan="3">
                                                    <asp:Label ID="AdministratorAddressDataLabel" runat="server" Text='<%#Eval("PlanAdministratorAddress")%>'    />
                                                </td>
                                             </tr>
                                            <tr>
                                                <td></td>                                               
                                            </tr>
                                           
                                            <tr>
                                                <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorCityLabel" runat="server" Text="City: " />
                                                </td>
                                                <td >
                                                    <asp:Label ID="AdministratorCityDataLabel" runat="server" Text='<%#Eval("PlanAdministratorCity")%>'    />
                                                </td>
                                                 <td style="text-align:right;">
                                                       <asp:Label ID="AdministratorStateLabel" runat="server" Text="State: " />
                                                 </td>   
                                                 <td>
                                                     <table>
                                                         <col style="width:25%;" />
                                                         <col style="width:10%;" />
                                                         <col style="width:65%;" />
                                                         <tr>                                                                                          
                                                             <td style="text-align:left;">                                 
                                                                <asp:Label ID="AdministratorStateDataLabel" runat="server" Text='<%#Eval("PlanAdministratorState")%>'    />
                                                             </td>
                                                            <td style="text-align:right;">
                                                                <asp:Label ID="AdministratorZipLabel" runat="server" Text="Zip: " />
                                                            </td>
                                                            <td>
                                                                <asp:Label ID="AdministratorZipDataLabel" runat="server" Text='<%#Eval("PlanAdministratorZip")%>'    />
                                                            </td>
                                                         </tr>
                                                     </table>
                                                 </td>
                                            </tr>
                                            <tr>
                                                <td style="text-align:right;">
                                                    <asp:Label ID="AdministratorCountryLabel" runat="server" Text="Country: " />
                                                </td>
                                                <td colspan="3">
                                                    <asp:Label ID="AdministratorCountryDataLabel" runat="server" Text='<%#Eval("PlanAdministratorCountryName")%>'    />
                                                </td>                                                 
                                            </tr>
                                          
                                        </table>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <table class="OutsetBox"  style="width:420px; height:152px;">
       
                                            <tr class="OutsetBoxHeaderRow"  style="height:16px;" >
                                                <td    style="vertical-align:top; text-align:center; " >
                                                    <asp:Label ID="SBAPlanDetailsFormViewHeaderLabel" runat="server" Text="Plan Details"  />
                                                </td>
                                            </tr>
                                            <tr style="border-bottom: 1px solid black;">
                                                <td  style="vertical-align:middle; text-align:center; " >
                                                    <asp:Label ID="EmptyDataTextLabel" runat="server" Text="No Plan or Exempt." />
                                                </td>                                  
                                            </tr>              
                                        </table>

                                    </EmptyDataTemplate>
                                </asp:FormView>
                             </td>
                            <td rowspan="2" style="vertical-align:top;"  >         
                                <table class="OutsetBox" >
                                    <tr class="OutsetBoxHeaderRow">
                                        <td style="text-align:center;" >
                                            <asp:Label ID="ProjectionGridPanelHeaderLabel" runat="server" Text="Projections" />
                                        </td>
                                    </tr>         
                                    <tr>
                                        <td>                  
                                            <asp:Panel ID="ProjectionGridPanel" runat="server" Width="100%" Height="100%" OnPreRender="ProjectionGridPanel_OnPreRender" >
                                               <div id="ProjectionPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                                                     <asp:HiddenField ID="ProjectionScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                                     <asp:HiddenField ID="highlightedProjectionRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                                                     <asp:HiddenField ID="highlightedProjectionRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                                     <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined"  />
                                                     <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False"  />
                                                </div>
                                                <div id="ScreenWidthHiddenDiv" visible="false" ></div>
                                                <div id="ProjectionGridViewDiv"  runat="server"  style="border:3px solid black; height:362px; overflow-x:auto; overflow-y:scroll; width:1050px; position:relative; top:1px; right:1px; left:1px;"  onscroll="javascript:setProjectionScrollForRestore( this );"  onkeypress="javascript:setProjectionScrollForRestore( this );"  >

                                                    <ep:GridView ID="ProjectionGridView" 
                                                                runat="server" 
                                                                DataKeyNames="ProjectionID"  
                                                                AutoGenerateColumns="False" 
                                                                Width="99%" 
                                                                CssClass="CMGridSmall" 
                                                                Visible="True" 
                                                                onrowcommand="ProjectionGridView_RowCommand" 
                                                                OnSelectedIndexChanged="ProjectionGridView_OnSelectedIndexChanged" 
                                                                OnRowDataBound="ProjectionGridView_RowDataBound"
                                                                AllowSorting="True" 
                                                                AutoGenerateEditButton="false"
                                                                EditRowStyle-CssClass="CMGridEditRowStyleSmall" 
                                                                onprerender="ProjectionGridView_PreRender" 
                                                                OnInit="ProjectionGridView_Init"
                                                                OnRowCreated="ProjectionGridView_OnRowCreated"
                                                                OnRowDeleting="ProjectionGridView_RowDeleting" 
                                                                OnRowEditing="ProjectionGridView_RowEditing" 
                                                                OnRowUpdating="ProjectionGridView_RowUpdating" 
                                                                OnRowCancelingEdit="ProjectionGridView_RowCancelingEdit"
                                                                AllowInserting="True"
                                                                OnRowInserting="ProjectionGridView_RowInserting" 
                                                                InsertCommandColumnIndex="1"
                                                                EmptyDataRowStyle-CssClass="CMGrid" 
                                                                EmptyDataText="There are no projections for the selected sba plan."
                                                                ContextMenuID="ProjectionContextMenu"
                                                                Font-Names="Arial" 
                                                                Font-Size="small" >
                                                            <HeaderStyle CssClass="ProjectionGridHeaders" />
                                                            <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                            <AlternatingRowStyle CssClass="CMGridAltItems" />
                                                            <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                                            <Columns>       
                                                                                                                     
                                                                <asp:TemplateField ItemStyle-Width="8%" ItemStyle-HorizontalAlign="Center" >  
                                                                    <ItemTemplate>
                                                                            <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="ProjectionGridView_ButtonCommand"   CommandName="EditProjection"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ProjectionId")  %>' ButtonType="Button"  Width="57px"   >
                                                                            </asp:Button>   

                                                                            <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="ProjectionGridView_ButtonCommand"   CommandName="SaveProjection" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ProjectionId") %>' ButtonType="Button" Width="57px"   >
                                                                            </asp:Button>   

                                                                            <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="ProjectionGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex  + "," +  Eval("ProjectionId") %>' ButtonType="Button"  Width="57px"   >
                                                                            </asp:Button>   
                                  
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>      
                                                                                                                                                
                                                                <asp:TemplateField HeaderText="Start Date"  ItemStyle-Width="8%" >  
                                                                    <ItemTemplate >
                                                                        <asp:Label ID="startDateLabel" runat="server"  Width="74px" Text='<%#  DataBinder.Eval( Container.DataItem, "StartDate", "{0:d}" ) %>' ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <table>
                                                                            <tr>
                                                                                <td style="border:none;">
                                                                                    <ep:TextBox ID="startDateTextBox" runat="server"  Width="74px" Text='<%#  DataBinder.Eval( Container.DataItem, "StartDate", "{0:d}" ) %>'  SkinId="DateTextBox"  ></ep:TextBox>
                                                                                </td>
                                                                                <td style="border:none;">
                                                                                    <asp:ImageButton ID="startDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select start date"  />      
                                                                                </td>         
                                                                            </tr>
                                                                        </table>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="End Date" ItemStyle-Width="8%" >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="endDateLabel" runat="server"  Width="74px" Text='<%#  DataBinder.Eval( Container.DataItem, "EndDate", "{0:d}" ) %>' ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <table >
                                                                            <tr>
                                                                                <td style="border:none;">
                                                                                    <ep:TextBox ID="endDateTextBox" runat="server" Width="74px" Text='<%#  DataBinder.Eval( Container.DataItem, "EndDate", "{0:d}" ) %>'  SkinId="DateTextBox"  ></ep:TextBox>
                                                                                </td>
                                                                                <td style="border:none;">
                                                                                    <asp:ImageButton ID="endDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select end date" />      
                                                                                </td>         
                                                                            </tr>
                                                                        </table >                                        
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="Total SubK Dollars" ItemStyle-Width="10%" HeaderStyle-Width="10%"  >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="totalSubContractingDollarsLabel" runat="server" Width="104px"  Text='<%# DataBinder.Eval( Container.DataItem, "TotalSubConDollars", "{0:N0}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="totalSubContractingDollarsTextBox" runat="server"  Width="104px" Text='<%# DataBinder.Eval( Container.DataItem, "TotalSubConDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="SB $" ItemStyle-Width="5%" >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="sbDollarsLabel" runat="server"  Width="44px"  Text='<%# DataBinder.Eval( Container.DataItem, "SBPercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="sbDollarsTextBox" runat="server"  Width="82px" Text='<%# DataBinder.Eval( Container.DataItem, "SBDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="VO $" ItemStyle-Width="5%" >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="veteranOwnedDollarsLabel" runat="server"  Width="44px"  Text='<%# DataBinder.Eval( Container.DataItem, "VeteranOwnedPercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="veteranOwnedDollarsTextBox" runat="server" Width="82px" Text='<%# DataBinder.Eval( Container.DataItem, "VeteranOwnedDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                               <asp:TemplateField HeaderText="SDVO %" ItemStyle-Width="6%"  >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="disabledVetDollarsLabel" runat="server"  Width="44px" Text='<%# DataBinder.Eval( Container.DataItem, "DisabledVetPercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="disabledVetDollarsTextBox" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "DisabledVetDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="SDB $"  ItemStyle-Width="5%" >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="sdbDollarsLabel" runat="server"  Width="44px"  Text='<%# DataBinder.Eval( Container.DataItem, "SDBPercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="sdbDollarsTextBox" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "SDBDollars", "{0:N0}" )%>'  CssClass="rightAlign" ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="WO $" ItemStyle-Width="5%" >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="womenOwnedDollarsLabel" runat="server"  Width="44px"  Text='<%# DataBinder.Eval( Container.DataItem, "WomenOwnedPercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="womenOwnedDollarsTextBox" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "WomenOwnedDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField HeaderText="Hub $" ItemStyle-Width="5%"   >  
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="hubZoneDollarsLabel" runat="server" Width="44px"  Text='<%# DataBinder.Eval( Container.DataItem, "HubZonePercentage", "{0:0.0%}" )%>' CssClass="rightAlign" ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <ep:TextBox ID="hubZoneDollarsTextBox" runat="server"  Width="80px" Text='<%# DataBinder.Eval( Container.DataItem, "HubZoneDollars", "{0:N0}" )%>' CssClass="rightAlign"  ></ep:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>
           
                                                                <asp:TemplateField HeaderText="Projection Comments"   ItemStyle-Width="18%"  ItemStyle-Wrap="true" >
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="projectionCommentsLabel" runat="server"  Width="99%" Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "Comments" ) %>' ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                            <ep:TextBox ID="projectionCommentsTextBox" runat="server"  TextMode="MultiLine"  MaxLength="255" Text='<%# DataBinder.Eval( Container.DataItem, "Comments" )%>'  ></ep:TextBox>                                     
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>
                  
                                                                <asp:TemplateField ItemStyle-Width="10%" >
                                                                    <ItemTemplate>                                                                                         
                                                                            <asp:Button runat="server"  ID="RemoveProjectionButton" Text="Remove Projection"   OnCommand="ProjectionGridView_ButtonCommand" CommandName="RemoveProjection" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("ProjectionId") %>' ButtonType="Button" CssClass="multilineButtonText" Width="88px" >            
                                                                            </asp:Button >                                    
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <asp:Button runat="server"  ID="RemoveProjectionButton" Text="Remove Projection"   OnCommand="ProjectionGridView_ButtonCommand" CommandName="RemoveProjection" CommandArgument='<%# Container.DataItemIndex + "," +  Eval("ProjectionId") %>' ButtonType="Button"  CssClass="multilineButtonText" Width="88px" >            
                                                                            </asp:Button >                                    
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>

                                                                <asp:TemplateField  HeaderText="Projection Id"  >
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="projectionIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ProjectionID" )%>'  ></asp:Label>
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <asp:Label ID="projectionIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ProjectionID" )%>'  ></asp:Label>                                          
                                                                    </EditItemTemplate>
                                                                 </asp:TemplateField>
                                                            </Columns>
                            
                                                    </ep:GridView>                                              
                                                </div>
                                               
                                             </asp:Panel>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr style="vertical-align:top;">
                            <td style="height:50%; vertical-align:top;">
                                <table class="OutsetBox" >
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td style="text-align:center;" >
                                            <asp:Label ID="AssociatedContractsHeaderLabel" runat="server" Text="Associated Contracts"  />
                                        </td>
                                    </tr>         
                                    <tr>
                                        <td>
                        
                                            <asp:Panel ID="AssociatedContractsGridPanel" runat="server" Width="100%" Height="100%" OnPreRender="AssociatedContractsGridPanel_OnPreRender" >     
                                                <div id="AssociatedContractsPanelHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
                                                     <asp:HiddenField ID="AssociatedContractsScrollPos" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                                    <asp:HiddenField ID="highlightedAssociatedContractsRow" runat="server" ClientIDMode="Static" Value="-1" EnableViewState="true" />
                                                    <asp:HiddenField ID="highlightedAssociatedContractsRowOriginalColor" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
                                                </div>
                                                <div id="AssociatedContractsGridViewDiv"  runat="server"  style="border:3px solid black; height:136px; overflow: scroll" onscroll="javascript:setAssociatedContractsScrollForRestore( this );"  onkeypress="javascript:setAssociatedContractsScrollForRestore( this );"  >
                                                   <ep:GridView ID="AssociatedContractsGridView" 
                                                                runat="server" 
                                                                DataKeyNames="ContractId"  
                                                                AutoGenerateColumns="False" 
                                                                Width="99%" 
                                                                CssClass="CMGridSmall" 
                                                                Visible="True"                              
                                                                OnRowDataBound="AssociatedContractsGridView_RowDataBound"
                                                                AllowSorting="True" 
                                                                AutoGenerateEditButton="false"
                                                                EditRowStyle-CssClass="CMGridEditRowStyleSmall"   
                                                                OnRowCreated="AssociatedContractsGridView_OnRowCreated"
                                                                AllowInserting="True"
                                                                InsertCommandColumnIndex="1"
                                                                EmptyDataRowStyle-CssClass="CMGrid" 
                                                                EmptyDataText="There are no associated contracts for the selected sba plan."
                                                                ContextMenuID="AssociatedContractsContextMenu"
                                                                Font-Names="Arial" 
                                                                Font-Size="small" >
                                                            <HeaderStyle CssClass="CMGridNoHeaders" />
                                                            <RowStyle  CssClass="CMGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                            <AlternatingRowStyle CssClass="CMGridAltItems" />
                                                            <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                                            <Columns>                        
                                                                <asp:TemplateField  ControlStyle-Font-Size="11px" ShowHeader="false" ItemStyle-VerticalAlign="Top" ItemStyle-Width="400px"  >
                                                                        <ItemTemplate>  
                                                                            <table style="table-layout:fixed;"  >
                                                                            <col style="width:142px;" />
                                                                            <col style="width:134px;" />
                                                                            <col style="width:126px;" />
                            
                                                                                <tr>
                                                                                    <td>
                                                                                        <asp:Button  ID="SelectAssociatedContractButton" runat="server" class="documentSelectButton" UseSubmitBehavior="false" OnCommand="AssociatedContractsGridView_ButtonCommand" CommandName="JumpToAssociatedContract" CommandArgument='<%# Container.DataItemIndex + "," + Eval( "ContractId" ) + "," + Eval( "ContractNumber" ) + "," + Eval( "ScheduleNumber" ) %>' 
                                                                                            Width="120px" Text='<%# Eval( "ContractNumber" ) %>'  /> 
                                                                                        <asp:Label ID="ResponsibleContractLabel" runat="server" />     
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="ScheduleNameLabel" runat="server" Text='<%# Eval( "ScheduleName" ) %>' />
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="ContractStatusLabel" runat="server"  />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td>
                                                                                        <asp:Label ID="ContractorNameLabel" runat="server" Text='<%# Eval( "ContractorName" ) %>' />
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="EstimatedContractValueLabel" runat="server" Text='<%#Eval("EstimatedContractValue","{0:c}") %>' />
                                                                                    </td>     
                                                                                    <td>
                                                                                        <asp:Label ID="ContractingOfficerNameLabel" runat="server" Text='<%# Eval( "ContractingOfficerName" ) %>' />
                                                                                    </td>                                                               
                                                                                </tr>
                                                                            </table>                                                                                      
                                                                                              
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>      
                                                                </Columns>
                                                    </ep:GridView>    
                                                </div>
                                            </asp:Panel>         
                                        </td>
                                    </tr>            
                                </table>
                            </td>       
                        </tr>
                    </table>
                </td>       
            </tr>
    </table>    
     
    <div id="SBAPlanDetailsHiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
        <input id="RefreshSBADetailsOnSubmit" runat="server" type="hidden"  value="false" enableviewstate="true" /> 
        <input id="ChangeSelectedSBAIdOnSubmit" runat="server" type="hidden"  value="false" enableviewstate="true" /> 
        <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
        <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
    </div>
                       
</ContentTemplate>
</asp:UpdatePanel>

</asp:Content>
