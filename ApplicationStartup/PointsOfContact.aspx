<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="PointsOfContact.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.PointsOfContact" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>


<asp:Content ID="PointsOfContactContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="PointsOfContactUpdatePanelEventProxy" runat="server"  />
                    
<asp:UpdatePanel ID="PointsOfContactUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="PointsOfContactUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  
                    
<ContentTemplate>

    <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;" >
                <table>
                    <tr>
                        <td>
                           <asp:FormView ID="VendorOrderingContactFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="VendorOrderingContactFormView_OnDataBound" OnPreRender="VendorOrderingContactFormView_OnPreRender" >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 320px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td  style="text-align:center;" colspan="4">
                                            <asp:Label ID="VendorOrderingContactHeaderLabel" runat="server" Text="Vendor Ordering Contact" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="OrderingAddress1Label" runat="server" Text="Address:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="OrderingAddress1TextBox"  Width="98%" MaxLength="100"  runat="server" Text='<%# Eval( "OrderingAddress1" ) %>'  />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="OrderingAddress2TextBox"  Width="98%" MaxLength="100"  runat="server" Text='<%# Eval( "OrderingAddress2" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="OrderingCityLabel" runat="server" Text="City:" />             
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="OrderingCityTextBox"  MaxLength="20"  runat="server" Text='<%# Eval( "OrderingCity" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                         <td>
                                            <asp:Label ID="OrderingCountryLabel" runat="server" Text="Country:" />             
                                        </td>
                                        <td colspan="3">
                                           <div id="CountryDiv" >
                                                <asp:DropDownList ID="OrderingCountryDropDownList" Width="98%"  MaxLength="2"  runat="server" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="OrderingCountryDropDownList_OnSelectedIndexChanged" />
                                           </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                             <asp:Label ID="OrderingStateLabel" runat="server" Text="State:" />
                                        </td>
                                        <td colspan="3">
                                            <table>
                                                <tr>                                                   
                                                    <td>
                                                        <div id="StateDiv" >   
                                                            <asp:DropDownList ID="OrderingStateDropDownList" Width="60px"  MaxLength="2"  runat="server" AutoPostBack="true" EnableViewState="true" OnDataBound="OrderingStateDropDownList_OnDataBound" />
                                                        </div>
                                                    </td>
                                                    <td>

                                                    </td>
                                                    <td style="text-align:right;">
                                                        <asp:Label ID="OrderingZipLabel" runat="server" Text="Zip:" />
                                                    </td>
                                                    <td>
                                                        <ep:TextBox ID="OrderingZipTextBox"   Width="76px" MaxLength="10"  runat="server" Text='<%# Eval( "OrderingZip" ) %>' />
                                                    </td>
                                               </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="OrderingContactPhoneLabel" runat="server" Text="Phone:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="OrderingContactPhoneTextBox" MaxLength="15"   runat="server" Text='<%# Eval( "OrderingTelephone","{0:(###)###-####}" ) %>' />
                                        </td>
                                        <td>
                                            <asp:Label ID="OrderingPhoneExtLabel" runat="server" Text="Ext:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="OrderingContactPhoneExtTextBox" MaxLength="5" Width="60px"   runat="server" Text='<%# Eval( "OrderingExtension" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="OrderingContactFaxLabel" runat="server" Text="Fax:" />
                                        </td>
                                        <td colspan="2">
                                            <ep:TextBox ID="OrderingContactFaxTextBox" MaxLength="15"   runat="server" Text='<%# Eval( "OrderingFax","{0:(###)###-####}" ) %>' />
                                        </td>          
                                        <td>
                                        </td>              
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="OrderingContactEmailLabel" runat="server" Text="Email:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="OrderingContactEmailTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "OrderingEmail" ) %>' />
                                        </td>      
                                        
                                    </tr>
                                </table>
                            </EditItemTemplate>
                            </asp:FormView>

                        </td>
                    </tr>
                    <tr>
                        <td>
                             <asp:FormView ID="VendorBusinessAddressFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="VendorBusinessAddressFormView_OnDataBound" OnPreRender="VendorBusinessAddressFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 342px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td  style="text-align:center;" colspan="4">
                                                <asp:Label ID="VendorBusinessAddressHeaderLabel" runat="server" Text="Vendor Business Address" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="VendorNameLabel" runat="server" Text="Name:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="VendorNameTextBox"  Width="98%" MaxLength="75"  runat="server" Text='<%# Eval( "VendorName" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="BusinessAddress1Label" runat="server" Text="Address:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="BusinessAddress1TextBox"  Width="98%" MaxLength="100"  runat="server" Text='<%# Eval( "VendorAddress1" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="BusinessAddress2TextBox"  Width="98%" MaxLength="100"  runat="server" Text='<%# Eval( "VendorAddress2" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="BusinessCityLabel" runat="server" Text="City:" />             
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="BusinessCityTextBox"  MaxLength="20"  runat="server" Text='<%# Eval( "VendorCity" ) %>' />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>
                                                <asp:Label ID="BusinessCountryLabel" runat="server" Text="Country:" />             
                                            </td>
                                            <td colspan="3">
                                               <div id="CountryDiv2" >
                                                    <asp:DropDownList ID="BusinessCountryDropDownList" Width="98%"  MaxLength="2"  runat="server" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="BusinessCountryDropDownList_OnSelectedIndexChanged" />
                                               </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="BusinessStateLabel" runat="server" Text="State:" />
                                            </td>
                                            <td colspan="3">
                                                <table>
                                                    <tr>                                                             
                                                        <td>
                                                            <div id="StateDiv2" >
                                                                <asp:DropDownList ID="BusinessStateDropDownList" Width="60px"  MaxLength="2"  runat="server" AutoPostBack="true" EnableViewState="true" OnDataBound="BusinessStateDropDownList_OnDataBound" />
                                                            </div>
                                                        </td>
                                                        <td>

                                                        </td>
                                                        <td style="text-align:right;">
                                                            <asp:Label ID="BusinessZipLabel"  runat="server" Text="Zip:" />
                                                        </td>
                                                        <td>
                                                            <ep:TextBox ID="BusinessZipTextBox" Width="76px"  MaxLength="10"  runat="server" Text='<%# Eval( "VendorZip" ) %>' />
                                                        </td>
                                                   </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="BusinessWebAddressLabel" runat="server" Text="Web Site:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="BusinessWebAddressTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "VendorWebAddress" ) %>' />
                                            </td>      
                                        
                                        </tr>
                                    </table>
                                </EditItemTemplate>
                                </asp:FormView>

                        </td>
                    </tr>
                </table>
            </td>
            <td style="vertical-align:top;" >
                <table>
                    <tr>
                        <td>
                            <asp:FormView ID="VendorContractAdministratorFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorContractAdministratorFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 300px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td  style="text-align:center;" colspan="4">
                                                <asp:Label ID="VendorContractAdministratorHeaderLabel" runat="server" Text="Vendor Contract Administrator" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="AdministratorNameLabel" runat="server" Text="Name:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="AdministratorNameTextBox" Width="98%" MaxLength="30" runat="server" Text='<%# Eval( "VendorPrimaryContactName" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="AdministratorPhoneLabel" runat="server" Text="Phone:" />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="AdministratorPhoneTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorPrimaryContactPhone","{0:(###)###-####}" ) %>' />
                                            </td>
                                            <td>
                                                <asp:Label ID="AdministratorPhoneExtLabel" runat="server" Text="Ext:" />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="AdministratorPhoneExtTextBox" MaxLength="5" Width="60px"  runat="server" Text='<%# Eval( "VendorPrimaryContactExtension" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="AdministratorFaxLabel" runat="server" Text="Fax:" />
                                            </td>
                                            <td colspan="2">
                                                <ep:TextBox ID="AdministratorFaxTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorPrimaryContactFax","{0:(###)###-####}" ) %>' />
                                            </td>           
                                            <td>
                                            </td>             
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="AdministratorEmailLabel" runat="server" Text="Email:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="AdministratorEmailTextBox"  Width="98%" MaxLength="50"  runat="server" Text='<%# Eval( "VendorPrimaryContactEmail" ) %>' />
                                            </td>          
                                      
                                        </tr>
                                    </table>
                                </EditItemTemplate>
                                </asp:FormView>

                        </td>
                    </tr>
                    <tr>
                        <td>
                           <asp:FormView ID="VendorAlternateContactFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorAlternateContactFormView_OnPreRender" >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 300px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td  style="text-align:center;" colspan="4">
                                            <asp:Label ID="VendorAlternateContactHeaderLabel" runat="server" Text="Vendor Alternate Contact" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="AlternateContactNameLabel" runat="server" Text="Name:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="AlternateContactNameTextBox"  Width="98%" MaxLength="30"  runat="server" Text='<%# Eval( "VendorAlternateContactName" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="AlternateContactPhoneLabel" runat="server" Text="Phone:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="AlternateContactPhoneTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorAlternateContactPhone","{0:(###)###-####}" ) %>' />
                                        </td>
                                        <td>
                                            <asp:Label ID="AlternatePhoneExtLabel" runat="server" Text="Ext:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="AlternateContactPhoneExtTextBox"  MaxLength="5"  Width="60px" runat="server" Text='<%# Eval( "VendorAlternateContactExtension" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="AlternateContactFaxLabel" runat="server" Text="Fax:" />
                                        </td>
                                        <td colspan="2">
                                            <ep:TextBox ID="AlternateContactFaxTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorAlternateContactFax","{0:(###)###-####}" ) %>' />
                                        </td>  
                                        <td>
                                        </td>                      
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="AlternateContactEmailLabel" runat="server" Text="Email:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="AlternateContactEmailTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "VendorAlternateContactEmail" ) %>' />
                                        </td>          
                                    
                                    </tr>
                                </table>
                            </EditItemTemplate>
                            </asp:FormView>

                        </td>
                    </tr>
                    <tr>
                        <td>
                           <asp:FormView ID="VendorTechnicalContactFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorTechnicalContactFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 300px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td  style="text-align:center;" colspan="4">
                                                <asp:Label ID="VendorTechnicalContactHeaderLabel" runat="server" Text="Vendor Technical Contact" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="TechnicalContactNameLabel" runat="server" Text="Name:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="TechnicalContactNameTextBox"  Width="98%" MaxLength="30"  runat="server" Text='<%# Eval( "VendorTechnicalContactName" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="TechnicalContactPhoneLabel" runat="server" Text="Phone:" />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="TechnicalContactPhoneTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorTechnicalContactPhone","{0:(###)###-####}" ) %>' />
                                            </td>
                                            <td>
                                                <asp:Label ID="TechnicalPhoneExtLabel" runat="server" Text="Ext:" />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="TechnicalContactPhoneExtTextBox"  MaxLength="5" Width="60px"  runat="server" Text='<%# Eval( "VendorTechnicalContactExtension" ) %>' />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="TechnicalContactFaxLabel" runat="server" Text="Fax:" />
                                            </td>
                                            <td colspan="2">
                                                <ep:TextBox ID="TechnicalContactFaxTextBox"  MaxLength="15"  runat="server" Text='<%# Eval( "VendorTechnicalContactFax","{0:(###)###-####}" ) %>' />
                                            </td>  
                                            <td>
                                            </td>                      
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="TechnicalContactEmailLabel" runat="server" Text="Email:" />
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="TechnicalContactEmailTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "VendorTechnicalContactEmail" ) %>' />
                                            </td>        
                                        
                                        </tr>
                                    </table>
                                </EditItemTemplate>
                            </asp:FormView>

                        </td>
                    </tr>
                </table>

            </td>
            <td style="vertical-align:top;" >
                <table>
                    <tr>
                        <td>
                           <asp:FormView ID="VendorSalesContactFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorSalesContactFormView_OnPreRender" >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 300px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td  style="text-align:center;" colspan="4">
                                            <asp:Label ID="VendorSalesContactHeaderLabel" runat="server" Text="Vendor Sales Contact" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="SalesContactNameLabel" runat="server" Text="Name:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="SalesContactNameTextBox"  Width="98%" MaxLength="30"  runat="server" Text='<%# Eval( "VendorSalesContactName" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="SalesContactPhoneLabel" runat="server" Text="Phone:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="SalesContactPhoneTextBox" MaxLength="15"  runat="server" Text='<%# Eval( "VendorSalesContactPhone","{0:(###)###-####}" ) %>' />
                                        </td>
                                         <td>
                                            <asp:Label ID="SalesPhoneExtLabel" runat="server" Text="Ext:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="SalesContactPhoneExtTextBox" MaxLength="5" Width="60px"  runat="server" Text='<%# Eval( "VendorSalesContactExtension" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="SalesContactFaxLabel" runat="server" Text="Fax:" />
                                        </td>
                                        <td colspan="2">
                                            <ep:TextBox ID="SalesContactFaxTextBox" MaxLength="15"  runat="server" Text='<%# Eval( "VendorSalesContactFax","{0:(###)###-####}" ) %>' />
                                        </td>     
                                        <td>
                                        </td>                   
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="SalesContactEmailLabel" runat="server" Text="Email:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="SalesContactEmailTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "VendorSalesContactEmail" ) %>' />
                                        </td>
                                               
                                    </tr>
                                </table>
                            </EditItemTemplate>
                            </asp:FormView>

                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:FormView ID="VendorEmergencyContactFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorEmergencyContactFormView_OnPreRender" >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 300px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td  style="text-align:center;" colspan="4">
                                            <asp:Label ID="VendorEmergencyContactHeaderLabel" runat="server" Text="Vendor Emergency Contact" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="EmergencyContactNameLabel" runat="server" Text="Name:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="EmergencyContactNameTextBox"  Width="98%" MaxLength="30"  runat="server" Text='<%# Eval( "VendorEmergencyContactName" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="EmergencyContactPhoneLabel" runat="server" Text="Phone:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="EmergencyContactPhoneTextBox" MaxLength="15"   runat="server" Text='<%# Eval( "VendorEmergencyContactPhone","{0:(###)###-####}" ) %>' />
                                        </td>
                                        <td>
                                            <asp:Label ID="EmergencyPhoneExtLabel" runat="server" Text="Ext:" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="EmergencyContactPhoneExtTextBox" MaxLength="5" Width="60px"   runat="server" Text='<%# Eval( "VendorEmergencyContactExtension" ) %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="EmergencyContactFaxLabel" runat="server" Text="Fax:" />
                                        </td>
                                        <td colspan="2">
                                            <ep:TextBox ID="EmergencyContactFaxTextBox" MaxLength="15"   runat="server" Text='<%# Eval( "VendorEmergencyContactFax","{0:(###)###-####}" ) %>' />
                                        </td>       
                                        <td>
                                        </td>                 
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="EmergencyContactEmailLabel" runat="server" Text="Email:" />
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="EmergencyContactEmailTextBox"  Width="98%"  MaxLength="50" runat="server" Text='<%# Eval( "VendorEmergencyContactEmail" ) %>' />
                                        </td>      
                                             
                                    </tr>
                                </table>
                            </EditItemTemplate>
                            </asp:FormView>

                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="height:102px; vertical-align:top;" >
            </td>
        </tr>
    </table>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
