<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractItems.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractItems" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>
<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>


<asp:Content ID="ContractItemsContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">


<ep:UpdatePanelEventProxy ID="ContractItemUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="ContractItemUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="ContractItemUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

    <table class="OuterTable" >
        <tr>
          <td style="vertical-align:top;" >
                <asp:FormView ID="ItemPriceCountsFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ItemPriceCountsFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 420px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;" >
                                <asp:Label ID="ItemPriceCountsHeaderLabel" runat="server" Text="Item/Price Counts"  />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Table ID="PriceCountTable" Width="100%" runat="server" border="black 1px">
                                    <asp:TableHeaderRow>
                                        <asp:TableHeaderCell ID="MedSurgActivePriceCountHeaderCell" runat="server"  style="vertical-align:middle; text-align:center; " >
                                            <asp:Label ID="MedSurgActivePriceCountHeaderLabel" runat="server" Text="Med/Surg Active Prices" />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="MedSurgFuturePriceCountHeaderCell" runat="server"   style="vertical-align:middle; text-align:center; " >
                                            <asp:Label ID="MedSurgFuturePriceCountHeaderLabel" runat="server" Text="Med/Surg Future Prices" />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="PharmaceuticalItemCountHeaderCell" runat="server"   style="vertical-align:middle; text-align:center; " >
                                            <asp:Label ID="PharmaceuticalItemCountHeaderLabel" runat="server" Text="Pharmaceutical Items" />
                                        </asp:TableHeaderCell>
                                        <asp:TableHeaderCell ID="PharmaceuticalCoveredFCPItemCountHeaderCell" runat="server"   style="vertical-align:middle; text-align:center; "  >
                                            <asp:Label ID="PharmaceuticalCoveredFCPItemCountHeaderLabel" runat="server" Text="FCP/Covered"  />
                                        </asp:TableHeaderCell>                                     
                                        <asp:TableHeaderCell ID="PharmaceuticalPPVTotalItemCountHeaderCell" runat="server"   style="vertical-align:middle; text-align:center; " >
                                            <asp:Label ID="PharmaceuticalPPVTotalItemCountHeaderLabel" runat="server" Text="PPV/Total" />
                                        </asp:TableHeaderCell>
                                        
                                    </asp:TableHeaderRow>
                                    <asp:TableRow>
                                         <asp:TableCell ID="MedSurgActivePriceCountDataCell" runat="server"  style="vertical-align:middle; text-align:center; "  AssociatedHeaderCellID="MedSurgActivePriceCountHeaderCell" >
                                            <asp:Label ID="MedSurgActivePriceCountLabel"  Text="0" runat="server"  />
                                        </asp:TableCell>
                                        <asp:TableCell ID="MedSurgFuturePriceCountDataCell" runat="server"  style="vertical-align:middle; text-align:center; "  AssociatedHeaderCellID="MedSurgFuturePriceCountHeaderCell" >
                                            <asp:Label ID="MedSurgFutureItemCountLabel"   Text="0" runat="server"  />
                                        </asp:TableCell>
                                        <asp:TableCell ID="PharmaceuticalItemCountDataCell" runat="server"  style="vertical-align:middle; text-align:center; "  AssociatedHeaderCellID="PharmaceuticalItemCountHeaderCell" >
                                            <asp:Label ID="PharmaceuticalItemCountLabel"   Text="0" runat="server"  />
                                        </asp:TableCell>
                                        <asp:TableCell ID="PharmaceuticalCoveredFCPItemCountDataCell"  runat="server"  style="vertical-align:middle; text-align:center; "  AssociatedHeaderCellID="PharmaceuticalCoveredFCPItemCountHeaderCell" >
                                            <asp:Label ID="PharmaceuticalCoveredFCPItemCountLabel"   Text="0/0" runat="server"  />
                                        </asp:TableCell>
                                        <asp:TableCell ID="PharmaceuticalPPVTotalItemCountDataCell" runat="server"  style="vertical-align:middle; text-align:center; "  AssociatedHeaderCellID="PharmaceuticalPPVTotalItemCountHeaderCell" >
                                            <asp:Label ID="PharmaceuticalPPVTotalItemCountLabel"   Text="0/0" runat="server"  />
                                       </asp:TableCell>
                                         
                                    </asp:TableRow>
                                </asp:Table>   
                            </td>
                        </tr>

                    </table>
                    <asp:HiddenField ID="RefreshPricelistScreenOnSubmit" runat="server" Value="False"  />
                    <asp:HiddenField ID="RefreshPricelistCountsOnSubmit" runat="server" Value="False"  />
                </EditItemTemplate>
                </asp:FormView>           
            </td>
          <td style="vertical-align:top;" >
                <asp:FormView ID="PricelistVerificationFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="PricelistVerificationFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 420px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;" colspan="2" >
                                <asp:Label ID="PricelistVerificationHeaderLabel" runat="server" Text="Pricelist Verification" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="PricelistVerifiedLabel" runat="server" Text="Pricelist Verified:" />
                            </td>
                            <td >
                                <asp:CheckBox ID="PricelistVerifiedCheckBox"  runat="server" Checked='<%# Eval( "PricelistVerified" ) %>' />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="ModificationNumberLabel" runat="server" Text="Current Mod#:" />
                            </td>
                            <td>
                                <ep:TextBox ID="ModificationNumberTextBox" MaxLength="20" runat="server" Text='<%# Eval( "CurrentModNumber" ) %>'  OnTextChanged="PricelistVerificationFormView_OnChange" />
                            </td>

                        
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="ModificationEffectiveDateLabel" runat="server" Text="Mod Effective Date:" /> 
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="ModificationEffectiveDateTextBox" runat="server" OnDataBinding="ModificationEffectiveDateTextBox_OnDataBinding" OnTextChanged="PricelistVerificationFormView_OnChange"  SkinId="DateTextBox" />
                                        </td>
                                        <td>
                                            <asp:ImageButton ID="ModificationEffectiveDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select pricelist modification effective date" OnClientClick="OnC2PricelistModificationEffectiveDateButtonClick()" />                           
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="VerifiedByLabel" runat="server" Text="Verified By" />
                            </td>
                            <td>
                                <ep:TextBox ID="VerifiedByTextBox" MaxLength="25" runat="server"  Text='<%# Eval( "PricelistVerifiedBy" ) %>' />
                            </td>
                        </tr>
                    </table>
                    <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined"  />
                    <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False"  />
 
                </EditItemTemplate>
                </asp:FormView>
            </td>
        </tr>
        <tr>
  
        </tr>
       <tr>
            <td style="vertical-align:top;" >
                <asp:FormView ID="PricelistFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="PricelistFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 420px; height:210px">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;" >
                                <asp:Label ID="PricelistHeaderLabel" runat="server" Text="Contract Pricelists"  />
                            </td>
                        </tr>
                        <tr>
                            <td  style="vertical-align:middle; text-align:center; " >
                                <table style="width: 100%; height:180px; ">
                                    <tr>
                                        <td  style="vertical-align:middle; text-align:center; " >
                                            <asp:Button ID="ViewEditMedSurgPricelistButton" CssClass="editMedSurgPricelistButton"  runat="server" Height="80px" Width="120px" Text="View/Edit Med/Surg Prices"  />
                                        </td>
                                        <td  style="vertical-align:middle; text-align:center; " >
                                            <asp:Button ID="ExportUploadMedSurgPricelistButton" CssClass="exportUploadMedSurgButton" runat="server"  Height="80px" Width="120px" Text="Export/Upload Med/Surg Prices"  />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td  style="vertical-align:middle; text-align:center; " >
                                            <asp:Button ID="ViewEditPharmaceuticalPricelistButton" CssClass="editPharmPricelistButton" runat="server"  Height="80px" Width="120px" Text="View/Edit Pharmaceutical Prices" />
                                        </td>
                                        <td  style="vertical-align:middle; text-align:center; " >
                                            <asp:Button ID="ExportUploadPharmaceuticalPricelistButton" CssClass="exportUploadPharmButton" runat="server"  Height="80px" Width="120px" Text="Export/Upload Pharmaceutical Prices" />
                                        </td>
                                    </tr>                               
                                </table>   
                            </td>
                        </tr>

                    </table>
                </EditItemTemplate>
                </asp:FormView>           
            </td>
            <td style="vertical-align:top;" >
                <asp:FormView ID="PricelistNotesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="PricelistNotesFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 420px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;" >
                                <asp:Label ID="PricelistNotesHeaderLabel" runat="server" Text="Pricelist Notes"  />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <ep:TextBox ID="PricelistNotesTextBox" runat="server" Width="98%"  MaxLength="255" TextMode="MultiLine" Rows="5" Text='<%# Eval("PricelistNotes" ) %>'  OnTextChanged="PricelistNotesFormView_OnChange" />
                            </td>
                        </tr>
                    </table>
                </EditItemTemplate>
                </asp:FormView>           
            </td>
        </tr>
        <tr>
            <td style="height:136px; vertical-align:top;">
            </td>
        </tr>
    </table>

</ContentTemplate>

</asp:UpdatePanel>           

</asp:Content>




