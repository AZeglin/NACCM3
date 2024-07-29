<%@ Page Title="" Language="C#" MasterPageFile="~/OfferView.Master"  StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="OfferGeneral.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferGeneral" %>
<%@ MasterType  VirtualPath="~/OfferView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="OfferGeneralContent" ContentPlaceHolderID="CommonContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="OfferGeneralUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="OfferGeneralUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="OfferGeneralUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

 <table class="OuterTable" >
        <tr>
            <td  style="vertical-align:top;"  rowspan="2">
                <asp:FormView ID="OfferAttributesFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferAttributesFormView_OnDataBound" OnPreRender="OfferAttributesFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="table-layout:fixed; width: 480px;">
                    <col style="width:112px;" />
                    <col style="width:368px;" />
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" colspan="2">
                            <asp:Label ID="OfferAttributesHeaderLabel" runat="server" Text="Offer Attributes" />
                        </td>
                    </tr>

                    <tr>
                        <td>
                            <asp:Label ID="VendorNameLabel" runat="server" Text="Vendor Name:" />
                        </td>
                        <td>
                            <ep:TextBox ID="VendorNameTextBox" runat="server" Text='<%#Bind("VendorName") %>' MaxLength="75" Width="246px" OnTextChanged="VendorNameTextBox_OnTextChanged" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="SolicitationLabel" runat="server" Text="Solicitation" />
                        </td>
                        <td>
                            <asp:DropDownList ID="SolicitationDropDownList"  Width="250px"  runat="server"  AutoPostBack="true" EnableViewState="true"  OnSelectedIndexChanged="SolicitationDropDownList_OnSelectedIndexChanged" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="CONameLabel" runat="server" Text="CO Name:" />
                        </td>
                        <td>
                            <asp:DropDownList ID="CONameDropDownList" Width="250px"  runat="server" AutoPostBack="true" EnableViewState="true"  OnSelectedIndexChanged="CONameDropDownList_OnSelectedIndexChanged" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="ScheduleLabel" runat="server" Text="Schedule" />
            
                        </td>
                        <td>
                            <asp:DropDownList ID="ScheduleDropDownList" runat="server"  AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="ScheduleDropDownList_OnSelectedIndexChanged"  />
                        </td>
                    </tr>                  
                    <tr>
                        <td>
                            <asp:Label ID="ProposalTypeLabel" runat="server" Text="Proposal Type" />
            
                        </td>
                        <td>
                            <asp:DropDownList ID="ProposalTypeDropDownList" runat="server" Width="200px" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="ProposalTypeDropDownList_OnSelectedIndexChanged" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="OfferNumberLabel" runat="server" Text="Offer Number" />
                        </td>
                        <td>
                            <ep:TextBox ID="OfferNumberTextBox" runat="server" Text='<%#Bind("OfferNumber") %>' Width="196px"  OnTextChanged="OfferAttributesFormView_OnChange" />
                        </td>
        
                    </tr>
                     <tr>
                        <td>
                            <asp:Label ID="ContractNumberLabel" runat="server" Text="Linked Contract" />
            
                        </td>
                        <td>
                            <asp:DropDownList ID="ContractNumberDropDownList" runat="server" Width="200px" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="OfferAttributesFormView_OnChange" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <table style="table-layout:fixed; text-align:center; "   >
                                <col style="width:140px;" />
                                <col style="width:140px;" />
                                <col style="width:140px;" />

                                <tr style="font-size: x-small" align="left">
                                    <td>
                                        <asp:Label ID="ReceivedDateLabel" runat="server" Text="Date Received:" />
                                    </td>
                                    <td>
                                        <asp:Label ID="AssignmentDateLabel" runat="server" Text="Assignment Date:" />
                                    </td>
                                    <td>
                                        <asp:Label ID="ReassignmentDateLabel" runat="server" Text="Reassignment Date:" />
                                    </td>                                                                
                                </tr>
                                <tr>
                                    <td>
                                        <ep:TextBox ID="ReceivedDateTextBox" runat="server" OnDataBinding="ReceivedDateTextBox_OnDataBinding" OnTextChanged="OfferAttributesDatesFormView_OnChange" SkinId="DateTextBox"/>    
                                        <asp:ImageButton ID="OfferReceivedDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer received date" ToolTip="Select offer received date" OnClientClick="OnO2ReceivedDateButtonClick()" /> 
                                    </td>
                                    <td>                                                       
                                        <ep:TextBox ID="AssignmentDateTextBox" runat="server" OnDataBinding="AssignmentDateTextBox_OnDataBinding"  OnTextChanged="OfferAttributesDatesFormView_OnChange" SkinId="DateTextBox"/>    
                                        <asp:ImageButton ID="OfferAssignmentDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer assignment date" ToolTip="Select offer assignment date" OnClientClick="OnO2AssignmentDateButtonClick()" />  
                                    </td>
                                        <td>
                                        <ep:TextBox ID="ReassignmentDateTextBox" runat="server" OnDataBinding="ReassignmentDateTextBox_OnDataBinding"  OnTextChanged="OfferAttributesDatesFormView_OnChange" SkinId="DateTextBox"/>    
                                        <asp:ImageButton ID="OfferReassignmentDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer reassignment date" ToolTip="Select offer reassignment date" OnClientClick="OnO2ReassignmentDateButtonClick()" />  
                                    </td>
                                </tr>
            
                            </table>
                        </td>
                    </tr>
                </table>
                <asp:HiddenField ID="RefreshOfferAttributesDateValueOnSubmit" runat="server" Value="Undefined"  />
                <asp:HiddenField ID="RefreshOfferAttributesOrNotOnSubmit" runat="server" Value="False"  />

                </EditItemTemplate>
                </asp:FormView>
            </td>
 
            <td style="vertical-align:top;">
                <asp:FormView ID="OfferActionFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferActionFormView_OnDataBound" OnPreRender="OfferActionFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="table-layout:fixed; width: 440px;">
                    <col style="width:12%;" />
                    <col style="width:38%;" />
                    <col style="width:48%;" />
                    <col style="width:2%;" />
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" colspan="4">
                            <asp:Label ID="OfferActionHeaderLabel" runat="server" Text="Offer Action" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td align="right">
                            <asp:Label ID="CurrentActionLabel" runat="server" Text="Current Action:" />
                        </td>
                        <td>
                            <asp:DropDownList ID="CurrentActionDropDownList" runat="server" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="CurrentActionDropDownList_OnSelectedIndexChanged" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="EstimatedCompletionDateLabel" runat="server" Text="Estimated Completion" />
                        </td>
                        <td>
                            <asp:Label ID="LastActionCompletedDateLabel" runat="server" Text="Last Action Completed" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <ep:TextBox ID="EstimatedCompletionDateTextBox" runat="server" OnDataBinding="EstimatedCompletionDateTextBox_OnDataBinding" OnTextChanged="OfferActionFormView_OnChange" SkinId="DateTextBox"/>    
                            <asp:ImageButton ID="OfferEstimatedCompletionDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select estimated completion date" ToolTip="Select action estimated completion date" OnClientClick="OnO2EstimatedCompletionDateButtonClick()" />     
                        </td>
                        <td>
                            <ep:TextBox ID="ActionCompletionDateTextBox" runat="server" OnDataBinding="ActionCompletionDateTextBox_OnDataBinding" OnTextChanged="OfferActionFormView_OnChange" SkinId="DateTextBox"/>    
                            <asp:ImageButton ID="OfferActionDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select actual completion date" ToolTip="Select action actual completion date" OnClientClick="OnO2ActionDateButtonClick()" />  
                        </td>
                        <td></td>
                    </tr>
                </table>   
                <asp:HiddenField ID="RefreshOfferActionDateValueOnSubmit" runat="server" Value="Undefined"  />
                <asp:HiddenField ID="RefreshOfferActionOrNotOnSubmit" runat="server" Value="False"  />
                </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td>
                <asp:FormView ID="OfferAuditFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferAuditFormView_OnDataBound" OnPreRender="OfferAuditFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="table-layout:fixed; width: 440px;">
                    <col style="width:2%;" />
                    <col style="width:68%;" />
                    <col style="width:18%;" />
                    <col style="width:10%;" />
                    <col style="width:2%;" />
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" colspan="5">
                            <asp:Label ID="OfferAuditHeaderLabel" runat="server" Text="Offer Audit" />
                        </td>
                    </tr>     
                    <tr>
                        <td></td>
                        <td align="right">
                            <asp:Label ID="AuditRequiredLabel" runat="server" Text="Audit Required:" />                     
                            <asp:CheckBox ID="AuditRequiredCheckBox" runat="server" Checked='<%#Bind("AuditIndicator") %>' OnCheckedChanged="OfferAuditFormView_OnChange" />
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="AuditDateLabel" runat="server" Text="Date of submission of the pre-award audit" />
                        </td>
                        <td>
                            <ep:TextBox ID="AuditDateTextBox" runat="server" OnDataBinding="AuditDateTextBox_OnDataBinding"  OnTextChanged="OfferAuditFormView_OnChange" SkinId="DateTextBox"/>    
                        </td>
                        <td>
                            <asp:ImageButton ID="OfferAuditDateImageButton" runat="server"  ImageUrl="Images/calendar.GIF" AlternateText="calendar to select audit date" ToolTip="Select offer audit date" OnClientClick="OnO2AuditDateButtonClick()" />                                                                
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="ReturnDateLabel" runat="server" Text="Date of issuance of the completed pre-award audit:" />
                        </td>
                        <td>
                            <ep:TextBox ID="ReturnDateTextBox" runat="server" OnDataBinding="ReturnDateTextBox_OnDataBinding"  OnTextChanged="OfferAuditFormView_OnChange" SkinId="DateTextBox"/>    
                        </td>
                        <td>
                            <asp:ImageButton ID="OfferReturnDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select return date" ToolTip="Select offer return date" OnClientClick="OnO2ReturnDateButtonClick()" />                                                                
                        </td>
                        <td></td>
                    </tr>
                </table>
                <asp:HiddenField ID="RefreshOfferAuditDateValueOnSubmit" runat="server" Value="Undefined"  />
                <asp:HiddenField ID="RefreshOfferAuditOrNotOnSubmit" runat="server" Value="False"  />

                </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td style="height:230px; vertical-align:top;">
            </td>
        </tr>
    </table>

</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
