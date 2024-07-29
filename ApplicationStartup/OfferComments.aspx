<%@ Page Title="" Language="C#" MasterPageFile="~/OfferView.Master"  StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="OfferComments.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferComments" %>
<%@ MasterType  VirtualPath="~/OfferView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="OfferCommentsContent" ContentPlaceHolderID="CommonContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="OfferCommentsUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="OfferCommentsUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="OfferCommentsUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

 <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="OfferCommentsFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferCommentsFormView_OnDataBound" OnPreRender="OfferCommentsFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="width: 99%;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" >
                            <asp:Label ID="OfferCommentsHeaderLabel" runat="server" Text="Offer Comments" />
                        </td>
                    </tr>       
                    <tr>
                        <td >
                            <ep:TextBox ID="OfferCommentsTextBox" runat="server" Text='<%#Bind("OfferComment") %>' TextMode="MultiLine" MaxLength="4000" Rows="20"  Width="99%" OnTextChanged="OfferCommentsFormView_OnChange" />
                        </td>            
                    </tr>
                </table>
                </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
            <td style="height:80px; vertical-align:top;">
            </td>
        </tr>
    </table>

</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
