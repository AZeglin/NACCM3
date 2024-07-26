<%@ Page Title="" Language="C#" MasterPageFile="~/DocumentCreation.Master" StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="CreateContract2.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.CreateContract2" %>
<%@ MasterType VirtualPath="~/DocumentCreation.Master" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<asp:Content ID="CreateContract2Content" ContentPlaceHolderID="CommonContentPlaceHolder" runat="server">

<script type="text/javascript" >

</script>

<ep:UpdatePanelEventProxy ID="CreateContract2UpdatePanelEventProxy" runat="server"  />

<asp:updatepanel ID="CreateContract2UpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
    <asp:AsyncPostBackTrigger ControlID="CreateContract2UpdatePanelEventProxy" EventName="ProxiedEvent" />
</Triggers>  

<ContentTemplate>

<table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <asp:FormView ID="CreateContractFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="CreateContractFormView_OnDataBound" OnPreRender="CreateContractFormView_OnPreRender" >
                <EditItemTemplate>
                <table class="OutsetBox" style="width: 800px;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td  style="text-align:center;" >
                            <asp:Label ID="GeneralHeaderLabel" runat="server" Text="General" />
                        </td>
                    </tr>       
                    <tr>
                        <td>                           
                           <table style="height: 100%; width: 100%; table-layout:fixed; background-color:#ece9d8; border-style:none; " >
                                <col style="width:2%;" />
                                <col style="width:16%;" />
                                <col style="width:16%;" />
                                <col style="width:16%;" />
                                <col style="width:16%;" />
                                <col style="width:16%;" />
                                <col style="width:16%;" />
                                <col style="width:2%;" />
                                <tr  style="vertical-align:top;" >               
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="DivisionLabel" Text="Division" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="3">
                                        <asp:DropDownList ID="DivisionDropDownList" runat="server" Width="98%" OnSelectedIndexChanged="DivisionDropDownList_OnSelectedIndexChanged"  EnableViewState="true" AutoPostBack="true"  ></asp:DropDownList>
                                    </td>
                                    <td>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="ScheduleLabel" Text="Schedule or Program" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="4">
                                        <asp:DropDownList ID="ScheduleDropDownList" runat="server" Width="98%" OnSelectedIndexChanged="ScheduleDropDownList_OnSelectedIndexChanged" EnableViewState="true" AutoPostBack="true" ></asp:DropDownList>
                                    </td>
                                    <td>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="ContractingOfficerLabel" Text="Contracting Officer" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="4">
                                        <asp:DropDownList ID="ContractingOfficerDropDownList" runat="server" Width="98%" OnSelectedIndexChanged="ContractingOfficerDropDownList_OnSelectedIndexChanged" EnableViewState="true" AutoPostBack="true"  ></asp:DropDownList>
                                    </td>
                                    <td>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="ContractNumberLabel" Text="Contract Number" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="2">
                                        <ep:TextBox ID="ContractNumberTextBox" runat="server" Text='<%#Bind( "ContractNumber" ) %>' Width="94%" MaxLength="50" ></ep:TextBox>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="ParentContractNumberLabel" Text="Parent FSS Contract" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="2">
                                        <asp:DropDownList ID="ParentContractsDropDownList" runat="server" Width="98%" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="ParentContractsDropDownList_OnSelectedIndexChanged"  ></asp:DropDownList>
                                    </td>                                    
                                </tr>
                                <tr>
	                                <td>
                                    </td>
                                    <td style="text-align:right;" >
                                         <asp:Label ID="AwardDateLabel" Text="Award Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                                    </td>
                                    <td style="HEIGHT: 26px">
                                     <ep:TextBox ID="AwardDateTextBox" runat="server" Width="75%" OnDataBinding="AwardDateTextBox_OnDataBinding" OnTextChanged="CreateContractFormView_OnChange" SkinId="DateTextBox"/>    
                                        <img src="Images/calendar.GIF" alt="calendar to select award date" onclick="OnX2AwardDateButtonClick()" />    
                                    </td>
                                    <td style="text-align:right;" >
                                         <asp:Label ID="EffectiveDateLabel" Text="Effective Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                                    </td>
				                    <td style="HEIGHT: 26px">
                                        <ep:TextBox ID="EffectiveDateTextBox" runat="server" Width="75%" OnDataBinding="EffectiveDateTextBox_OnDataBinding" OnTextChanged="CreateContractFormView_OnChange" SkinId="DateTextBox"/>    
                                        <img src="Images/calendar.GIF" alt="calendar to select effective date" onclick="OnX2EffectiveDateButtonClick()" />
                                    </td>
                                    <td style="text-align:right;" >
                                         <asp:Label ID="ExpirationDateLabel" Text="Expiration Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                                    </td>
				                    <td style="HEIGHT: 26px">
                                        <ep:TextBox ID="ExpirationDateTextBox" runat="server" Width="75%"  OnDataBinding="ExpirationDateTextBox_OnDataBinding" OnTextChanged="CreateContractFormView_OnChange" SkinId="DateTextBox"/>    
                                        <img src="Images/calendar.GIF" alt="calendar to select expiration date" onclick="OnX2ExpirationDateButtonClick()" />
                                    </td>
                                    <td>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                    <td style="text-align:right;" >
                                        <asp:Label ID="VendorNameLabel" Text="Vendor Name" runat="server" CssClass="FieldLabelText" ></asp:Label>
                                    </td>
                                    <td colspan="4" >
                                        <ep:TextBox ID="VendorNameTextBox" runat="server" Text='<%#Bind( "VendorName" ) %>'  Width="98%" MaxLength="75"></ep:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                     <td>
                                    </td>
                                    <td colspan="4" >
                                        <asp:RadioButtonList ID="BusinessSizeRadioButtonList" runat="server"  CellSpacing="1" CellPadding="1"  Width="99%" CssClass="FieldLabelText" EnableViewState="true" RepeatDirection="Horizontal"  
                                                OnDataBinding="BusinessSizeRadioButtonList_OnDataBinding"  OnSelectedIndexChanged="BusinessSizeRadioButtonList_OnSelectedIndexChanged">
                                            <asp:ListItem Value="1" >Small Business</asp:ListItem>
                                            <asp:ListItem Value="2" >Large Business</asp:ListItem>
                                        </asp:RadioButtonList>               
                                     </td>      
                                    <td>
                                    </td>
                                     <td>
                                    </td>                                        
                                </tr>
                                <tr>
                                    <td>
                                    </td>
                                     <td>
                                    </td>
                                    <td colspan="4" >
                                        <asp:RadioButtonList ID="RebateRequiredRadioButtonList" runat="server"  CellSpacing="1" CellPadding="1"  Width="99%" CssClass="FieldLabelText" EnableViewState="true" RepeatDirection="Horizontal"  
                                                OnDataBinding="RebateRequiredRadioButtonList_OnDataBinding"  OnSelectedIndexChanged="RebateRequiredRadioButtonList_OnSelectedIndexChanged">
                                            <asp:ListItem Value="1" >Annual Rebate Required</asp:ListItem>
                                            <asp:ListItem Value="0" >Annual Rebate Not Required</asp:ListItem>
                                        </asp:RadioButtonList>               
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
                <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined" />
                <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False" />

            </EditItemTemplate>
        </asp:FormView>
    </td>
</tr>
<tr>
    <td style="text-align:center; border-top: solid 1px black;" >
        <asp:Label ID="VendorDetailsTitleLabel" Text="Vendor Details" runat="server" CssClass="HeaderText" ></asp:Label>
    </td>               
</tr>         
<tr>
    <td style="text-align:center;" >
        <asp:Label ID="BlankLabel1" Text=" " runat="server" />
    </td>               
</tr>       
<tr>
    <td>
        <table>
            <tr>
                <td style="vertical-align:top; text-align:right;">
                    <asp:FormView ID="VendorPOCFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="VendorPOCFormView_OnDataBound" OnPreRender="VendorPOCFormView_OnPreRender" >
                    <EditItemTemplate>
                    <table class="OutsetBox" style="width: 400px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;" >
                                <asp:Label ID="VendorPOCHeaderLabel" runat="server" Text="Point Of Contact" />
                            </td>
                        </tr>       
                        <tr>
                            <td rowspan="6" colspan="4"  style="vertical-align:top;"  >
                               <table style="height: 100%;  width:100%; table-layout:fixed; background-color:#ece9d8;  border-style:none;" >
                                    <col style="width:2%;" />       
                                    <col style="width:10%;" />       
                                    <col style="width:50%;" />       
                                    <col style="width:6%;" />       
                                    <col style="width:20%;" />       
                                    <col style="width:2%;" />       
                                    
                                    <tr>
                                        <td>
                                        </td>
                                        <td style="text-align:right;" >
                                           <asp:Label ID="NameLabel" Text="Name" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                        </td>
                                        <td colspan="3">
                                            <ep:TextBox ID="PointOfContactNameTextBox" runat="server" Text='<%#Bind( "VendorPrimaryContactName" ) %>' Width="98%" MaxLength="30" OnTextChanged="VendorPOCFormView_OnChange" ></ep:TextBox>
                                        </td>
                                         <td>
                                        </td>
                                   </tr>                                  
                                    <tr>
                                        <td>
                                        </td>
                                        <td style="text-align:right;" >
                                           <asp:Label ID="PhoneLabel" Text="Phone" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                        </td>
                                        <td>
                                            <ep:TextBox ID="PointOfContactPhoneTextBox" runat="server" Text='<%#Bind( "VendorPrimaryContactPhone" ) %>' Width="98%" MaxLength="15" OnTextChanged="VendorPOCFormView_OnChange" ></ep:TextBox>
                                        </td>
                                        <td>
                                           <asp:Label ID="PhoneExtensionLabel" Text="Ext" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                        </td>
                                        <td>
                                            <ep:TextBox ID="PointOfContactPhoneExtensionTextBox" runat="server" Text='<%#Bind( "VendorPrimaryContactExtension" ) %>'  Width="90%" MaxLength="5" OnTextChanged="VendorPOCFormView_OnChange" ></ep:TextBox>
                                        </td>                       
                                        <td>
                                        </td>
                                    </tr>     
                                    <tr>
                                        <td>
                                        </td>
                                        <td style="text-align:right;" >
                                           <asp:Label ID="FaxLabel" Text="Fax" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                        </td>
                                        <td>
                                            <ep:TextBox ID="PointOfContactFaxTextBox" runat="server" Text='<%#Bind( "VendorPrimaryContactFax" ) %>'  Width="98%" MaxLength="15" OnTextChanged="VendorPOCFormView_OnChange" ></ep:TextBox>
                                        </td>
                                         <td>
                                        </td>
                                    </tr>     
                                    <tr>
                                        <td>
                                        </td>
                                        <td style="text-align:right;" >
                                           <asp:Label ID="EmailLabel" Text="Email" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                        </td>
                                        <td colspan="2">
                                            <ep:TextBox ID="PointOfContactEmailTextBox" runat="server" Text='<%#Bind( "VendorPrimaryContactEmail" ) %>' Width="98%" MaxLength="50" OnTextChanged="VendorPOCFormView_OnChange" ></ep:TextBox>
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
                <td  style="vertical-align:top; text-align:left;">
                    <asp:FormView ID="VendorAddressFormView" runat="server" DefaultMode="Edit" Width="100%" OnDataBound="VendorAddressFormView_OnDataBound" OnPreRender="VendorAddressFormView_OnPreRender" >
                        <EditItemTemplate>
                        <table class="OutsetBox" style="width: 400px;">
                            <tr class="OutsetBoxHeaderRow" >
                                <td  style="text-align:center;" >
                                    <asp:Label ID="VendorAddressHeaderLabel" runat="server" Text="Vendor Address" />
                                </td>
                            </tr>       
                            <tr>                 
                                <td style="vertical-align:top;">
                                    <table style="height: 100%;  width: 100%; table-layout:fixed; background-color:#ece9d8; border-style:none; " >
                                        <col style="width:2%;" />       
                                        <col style="width:22%;" />       
                                        <col style="width:50%;" />       
                                        <col style="width:7%;" />       
                                        <col style="width:22%;" />       
                                        <col style="width:2%;" />       
 
                                        <tr>
                                            <td>
                                            </td>
                                            <td style="text-align:right;" >
                                               <asp:Label ID="Address1Label" Text="Address 1" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="Address1TextBox" runat="server" Text='<%#Bind( "VendorAddress1" ) %>'  Width="98%" MaxLength="100"  OnTextChanged="VendorAddressFormView_OnChange" ></ep:TextBox>
                                            </td>                       
                                             <td>
                                            </td>
                                       </tr>
                                       <tr>
                                            <td>
                                            </td>
                                            <td style="text-align:right;" >
                                               <asp:Label ID="Address2Label" Text="Address 2" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="Address2TextBox" runat="server" Text='<%#Bind( "VendorAddress2" ) %>' Width="98%" MaxLength="100"  OnTextChanged="VendorAddressFormView_OnChange" ></ep:TextBox>
                                            </td>                       
                                              <td>
                                            </td>
                                        </tr>        
                                        <tr>
                                            <td>
                                            </td>
                                            <td style="text-align:right;" >
                                               <asp:Label ID="CityLabel" Text="City" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="CityTextBox" runat="server" Text='<%#Bind( "VendorCity" ) %>' Width="98%" MaxLength="20"  OnTextChanged="VendorAddressFormView_OnChange" ></ep:TextBox>
                                            </td>                       
                                             <td>
                                            </td>
                                        </tr>        
                                        <tr>
                                            <td>
                                            </td>
                                            <td>
                                                <asp:Label ID="CountryLabel" runat="server" Text="Country:" />             
                                            </td>
                                            <td colspan="3">
                                               <div id="CountryDiv2" >
                                                    <asp:DropDownList ID="CountryDropDownList" Width="98%"  MaxLength="2"  runat="server" AutoPostBack="true" EnableViewState="true" OnSelectedIndexChanged="CountryDropDownList_OnSelectedIndexChanged" />
                                               </div>
                                            </td>
                                            <td>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td style="text-align:right;" >
                                               <asp:Label ID="StateLabel" Text="State" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                            </td>
                                            <td style="text-align:left;">
                                               <div id="StateDiv" class="StateClass" >
                                                  <asp:DropDownList ID="StateDropDownList" runat="server" Width="24%" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="VendorAddressFormView_OnChange"  OnDataBound="StateDropDownList_OnDataBound"  ></asp:DropDownList>
                                               </div>
                                            </td>                       
                                            <td>
                                               <asp:Label ID="ZipLabel" Text="Zip" runat="server" CssClass="FieldLabelText"></asp:Label> 
                                            </td>
                                            <td>
                                                <ep:TextBox ID="ZipTextBox" runat="server" Text='<%#Bind( "VendorZip" ) %>'  Width="90%" MaxLength="10" OnTextChanged="VendorAddressFormView_OnChange" ></ep:TextBox>
                                            </td>   
                                            <td>
                                            </td>
                            
                                        </tr>                            
                                        <tr>
                                             <td>
                                            </td>
                                            <td style="text-align:right;" >
                                               <asp:Label ID="CompanyUrlLabel" Text="Company Web Page" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                                            </td>
                                            <td colspan="3">
                                                <ep:TextBox ID="CompanyUrlTextBox" runat="server" Text='<%#Bind( "VendorWebAddress" ) %>'  Width="98%" OnTextChanged="VendorAddressFormView_OnChange" ></ep:TextBox>
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
    </table>
    </td>
</tr>
<tr>
    <td style="text-align:center;" >
        <asp:Label ID="BlankLabel2" Text=" " runat="server" />
    </td>
</tr>

</table>
         
</ContentTemplate> 
</asp:updatepanel> 
</asp:Content>
