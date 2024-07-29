<%@ Page Title="" Language="C#"  MasterPageFile="~/OfferView.Master"  StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="OfferAward.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferAward" %>
<%@ MasterType  VirtualPath="~/OfferView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="OfferAwardContent" ContentPlaceHolderID="CommonContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="OfferAwardUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="OfferAwardUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="OfferAwardUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

 <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="OfferAwardFormView" runat="server"  Width="100%" OnDataBound="OfferAwardFormView_OnDataBound" OnPreRender="OfferAwardFormView_OnPreRender" >
                <ItemTemplate>
                <table class="OutsetBox" style="width: 400px;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" colspan="3">
                            <asp:Label ID="OfferAwardHeaderLabel" runat="server" Text="Award Summary" />
                        </td>
                    </tr>       
                    <tr>
                        <td align="right">
                            <asp:Label ID="AwardedContractLabel" runat="server" Text="Awarded Contract:" />
                        </td>
                        <td>   </td>
                        <td align="left" >
                            <asp:Button runat="server" ID="SelectAwardedContractButton" class="documentSelectButton" UseSubmitBehavior="false" CommandName="JumpToAwardedContract" CommandArgument='<%# Eval( "ContractId" ) + "," + Eval( "ContractNumber" ) + "," + Eval( "ScheduleNumber" ) %>' 
                                    Text='<%# Eval( "ContractNumber" ) %>' OnCommand="SelectAwardedContractButton_OnCommand" /> 
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3" style="border-bottom:1px solid black;">
                        </td>
                    </tr>
                    <tr>
                        <td  align="right">
                            <asp:Label ID="ContractingOfficerLabel" runat="server" Text="Contracting Officer:" />
                        </td>
                        <td>   </td>
                        <td align="left" >
                            <asp:Label ID="CONameDataLabel" runat="server" Text='<%#Eval("AwardedContractDetails.AwardedContractingOfficerFullName") %>' />
                        </td>
                    </tr>
                    <tr>
                        <td  align="right">
                            <asp:Label ID="AwardDateLabel" runat="server" Text="Award Date:" />
                        </td>
                        <td>   </td>
                        <td align="left" >
                            <asp:Label ID="AwardDateDataLabel" runat="server"  Text='<%#Eval("AwardedContractDetails.ContractAwardDate", "{0:d}") %>' />
                        </td>
                    </tr>
                    <tr>
                        <td  align="right">
                            <asp:Label ID="ExpirationDateLabel" runat="server" Text="Expiration Date:" />
                        </td>
                        <td>   </td>
                        <td align="left" >
                            <asp:Label ID="ExpirationDateDataLabel" runat="server"  Text='<%#Eval("AwardedContractDetails.ContractExpirationDate", "{0:d}") %>' />
                        </td>
                    </tr>
                    <tr>
                        <td  align="right">
                            <asp:Label ID="ContractStatusLabel" runat="server" Text="Contract Status:" />
                        </td>
                        <td>   </td>
                        <td align="left" >
                            <asp:Label ID="ContractStatusDataLabel" runat="server"  Text='<%# ContractStatus( Eval("AwardedContractDetails.ContractExpirationDate"), Eval("AwardedContractDetails.ContractCompletionDate"), "ContractStatusLabel" ) %>' />
                        </td>
                    </tr>
 
                </table>        
                </ItemTemplate>
                <EditItemTemplate>
                  <table class="OutsetBox" style="width: 400px;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" >
                            <asp:Label ID="OfferAwardHeaderLabel" runat="server" Text="Award" />
                        </td>
                    </tr>       
                    <tr>
                        <td style="text-align:center;">
                            <asp:Button ID="AwardContractButton" runat="server" Text="Enter Award Information" OnClick="AwardContractButton_OnClick" />
                        </td>
                    </tr>             
                  </table>                    
                </EditItemTemplate>
                <InsertItemTemplate>
        
                   <table class="OutsetBox" style="width: 400px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;">
                                <asp:Label ID="OfferAwardHeaderLabel" runat="server" Text="Award Summary" />
                            </td>
                        </tr>    
                        <tr>
                            <td>
                            </td>
                        </tr>   
                        <tr>
                            <td style="text-align:center;">
                                <asp:Label ID="NoDataLabel" runat="server" Text="Award information is not available for this offer." />
                            </td>
                           
                        </tr>          
                      </table>  
                </InsertItemTemplate>
                </asp:FormView>            
            </td>
        </tr>
        <tr>
            <td style="height:350px; vertical-align:top;">
                        
            </td>
        </tr>
    </table>

</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
