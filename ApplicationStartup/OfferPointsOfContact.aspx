<%@ Page Title="" Language="C#" MasterPageFile="~/OfferView.Master"  StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="OfferPointsOfContact.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.OfferPointsOfContact" %>
<%@ MasterType  VirtualPath="~/OfferView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="OfferPointsOfContactContent" ContentPlaceHolderID="CommonContentPlaceHolder" runat="server">

<ep:UpdatePanelEventProxy ID="OfferPointsOfContactUpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="OfferPointsOfContactUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="OfferPointsOfContactUpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

 <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="OfferVendorPOCFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferVendorPOCFormView_OnDataBound" OnPreRender="OfferVendorPOCFormView_OnPreRender"  >
                <EditItemTemplate>
                <table class="OutsetBox" style="width: 400px;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" colspan="2">
                            <asp:Label ID="OfferPOCHeaderLabel" runat="server" Text="Vendor Point Of Contact" />
                        </td>
                    </tr>       
                    <tr>
                        <td>
                            <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8;  border-style:none;" >
                                <col style="width:2%;" />       
                                <col style="width:10%;" />       
                                <col style="width:50%;" />       
                                <col style="width:6%;" />       
                                <col style="width:20%;" />       
                                <col style="width:2%;" />       
                        
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="NameLabel" Text="Name" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <ep:TextBox ID="PointOfContactNameTextBox" runat="server" Text='<%#Bind("VendorPrimaryContactName") %>'  Width="98%" MaxLength="30" OnTextChanged="OfferVendorPOCFormView_OnChange" ></ep:TextBox>
                                    </td>
                                    <td>
                                    </td>
                                </tr>                                  
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="PhoneLabel" Text="Phone" runat="server"  CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td>
                                        <ep:TextBox ID="PointOfContactPhoneTextBox" runat="server" Text='<%#Bind("VendorPrimaryContactPhone") %>'  Width="98%" MaxLength="15" OnTextChanged="OfferVendorPOCFormView_OnChange" ></ep:TextBox>
                                    </td>
                                    <td>
                                        <asp:Label ID="PhoneExtensionLabel" Text="Ext" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td>
                                        <ep:TextBox ID="PointOfContactPhoneExtensionTextBox" runat="server" Text='<%#Bind("VendorPrimaryContactExtension") %>'  Width="90%" MaxLength="5" OnTextChanged="OfferVendorPOCFormView_OnChange" ></ep:TextBox>
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>     
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="FaxLabel" Text="Fax" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td>
                                        <ep:TextBox ID="PointOfContactFaxTextBox" runat="server" Text='<%#Bind("VendorPrimaryContactFax") %>'   Width="98%" MaxLength="15" OnTextChanged="OfferVendorPOCFormView_OnChange" ></ep:TextBox>
                                    </td>
                                    <td>
                                    </td>
                                </tr>     
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="EmailLabel" Text="Email" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td colspan="2">
                                        <ep:TextBox ID="PointOfContactEmailTextBox" runat="server" Text='<%#Bind("VendorPrimaryContactEmail") %>'  Width="98%" MaxLength="50" OnTextChanged="OfferVendorPOCFormView_OnChange" ></ep:TextBox>
                                    </td>
                                    <td>
                                    </td>
                                </tr>  
                                <tr>
                                </tr>
                            </table>   
                        </td>
                    </tr>
                </table>        
                </EditItemTemplate>
                </asp:FormView>
            </td>
            <td style="vertical-align:top;" >
                <asp:FormView ID="OfferVendorAddressFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="OfferVendorAddressFormView_OnDataBound" OnPreRender="OfferVendorAddressFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="width: 440px;">                    
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;">
                            <asp:Label ID="OfferVendorAddressHeaderLabel" runat="server" Text="Vendor Address" />
                        </td>
                    </tr>       
                    <tr>
                        <td >
                            <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8; border-style:none; " >
                                <col style="width:2%;" />       
                                <col style="width:22%;" />       
                                <col style="width:48%;" />       
                                <col style="width:6%;" />       
                                <col style="width:20%;" />       
                                <col style="width:2%;" />       
    
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="Address1Label" Text="Address 1" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <ep:TextBox ID="Address1TextBox" runat="server" Text='<%#Bind("VendorAddress1") %>' Width="98%" MaxLength="100" OnTextChanged="OfferVendorAddressFormView_OnChange" ></ep:TextBox>
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="Address2Label" Text="Address 2" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <ep:TextBox ID="Address2TextBox" runat="server" Text='<%#Bind("VendorAddress2") %>'  Width="98%" MaxLength="100" OnTextChanged="OfferVendorAddressFormView_OnChange" ></ep:TextBox>
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>        
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="CityLabel" Text="City" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <ep:TextBox ID="CityTextBox" runat="server" Text='<%#Bind("VendorCity") %>'  Width="98%" MaxLength="20" OnTextChanged="OfferVendorAddressFormView_OnChange" ></ep:TextBox>
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>          
                                 <tr>
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="CountryLabel" Text="Country" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <div id="CountryDiv" >
                                            <asp:DropDownList ID="CountryDropDownList" runat="server" Width="100%" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="CountryDropDownList_OnSelectedIndexChanged" ></asp:DropDownList>
                                        </div>                                    
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>      
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="StateLabel" Text="State" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td align="left">
                                        <div id="StateDiv" >
                                            <asp:DropDownList ID="StateDropDownList" runat="server" Width="24%" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="OfferVendorAddressFormView_OnChange" OnDataBound="StateDropDownList_OnDataBound"   ></asp:DropDownList>
                                        </div>
                                    </td>                       
                                    <td>
                                        <asp:Label ID="ZipLabel" Text="Zip" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                    </td>
                                    <td>
                                        <ep:TextBox ID="ZipTextBox" runat="server" Text='<%#Bind("VendorZip") %>' Width="92%" MaxLength="10" OnTextChanged="OfferVendorAddressFormView_OnChange" ></ep:TextBox>
                                    </td>   
                                    <td>
                                    </td>        
                                </tr>                            
                                   
                                <tr>
                                    <td>
                                    </td>
                                    <td align="right" >
                                        <asp:Label ID="CompanyUrlLabel" Text="Company Web Page" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                    </td>
                                    <td colspan="3">
                                        <ep:TextBox ID="CompanyUrlTextBox" runat="server" Text='<%#Bind("VendorWebAddress") %>'  Width="98%" OnTextChanged="OfferVendorAddressFormView_OnChange" ></ep:TextBox>
                                    </td>                       
                                    <td>
                                    </td>
                                </tr>
                            </table>
                        </td>           
                    </tr>
                </table>
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
