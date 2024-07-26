<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="ContractComments.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractComments" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ContractCommentsContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="ContractCommentsUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="ContractCommentsUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="ContractCommentsUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

<table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="ContractCommentsFormView" runat="server" DefaultMode="Edit" Width="100%" OnChange="ContractCommentsFormView_OnChange" OnPreRender="ContractCommentsFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="width: 99%;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" >
                            <asp:Label ID="ContractCommentsHeaderLabel" runat="server" Text="Contract Comments" />
                        </td>
                    </tr>       
                    <tr>
                        <td >
                            <ep:TextBox ID="ContractCommentsTextBox" runat="server" Text='<%#Bind("GeneralContractNotes") %>' TextMode="MultiLine" MaxLength="800" Rows="11"  Width="99%" OnTextChanged="ContractCommentsFormView_OnChange" />
                        </td>            
                    </tr>
                </table>
                </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td style="height:274px; vertical-align:top;">
            </td>
        </tr>
    </table>

</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
