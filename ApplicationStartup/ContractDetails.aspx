<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractDetails.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractDetails" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ContractDetailsContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

    <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <table class="OutsetBox" style="width: 380px;">
                    <tr class="OutsetBoxHeaderRow" >
                        <td colspan="2" style="text-align:center;">
                            <asp:Label ID="AttributesHeaderLabel" runat="server" Text="Contract Attributes" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:FormView ID="ContractDetailsAttributesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractDetailsAttributesFormView_OnPreRender" OnDataBound="ContractDetailsAttributesFormView_OnDataBound" >
                                <EditItemTemplate> 
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="EstimatedValueLabel" Text="Estimated Value" runat="server" Font-Size="X-Small"/>
                                            </td>
                                            <td>
                                                <asp:Label ID="MinimumOrderLabel" Text="Minimum Order" runat="server" Font-Size="X-Small"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <ep:TextBox ID="EstimatedValueTextBox" runat="server" Width="166px"  Text='<%# Eval("EstimatedContractValue","{0:c}") %>' />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="MinimumOrderTextBox" runat="server" Width="166px"  Text='<%# Eval("MinimumOrder") %>' MaxLength="255" />
                                            </td>
                                        </tr>                            
 
                                        <tr>
                                            <td>
                                                <asp:Label ID="TrackingCustomerLabel" runat="server" Text="Tracking Customer" Font-Size="X-Small"/>
                                            </td>
                                            <td>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <ep:TextBox ID="TrackingCustomerTextBox" runat="server"  Width="336px"  Text='<%# Eval("TrackingCustomerName") %>' MaxLength="255" />
                                            </td>
                                       
                                        </tr>      
                                        <tr>
                                            <td>
                                                <asp:Label ID="FPRDateLabel" runat="server" Text="FPR Date" Font-Size="X-Small"/>
                                            </td>
                                            <td>
                                                <asp:Label ID="RatioLabel" runat="server" Text="Ratio" Font-Size="X-Small"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <ep:TextBox ID="FPRDateTextBox" runat="server" Width="166px"  Text='<%# Eval("FPRFreeFormatDateString") %>' MaxLength="255" />
                                            </td>
                                            <td>
                                                <ep:TextBox ID="RatioTextBox" runat="server" Width="166px"  Text='<%# Eval("Ratio") %>' MaxLength="255"/>
                                            </td>
                                        </tr>
                                        <tr>                                                                                           
                                            <td>
                                                <asp:Label ID="SolicitationNumberLabel" runat="server" Text="Solicitation Number" Font-Size="X-Small"/>
                                            </td>
                                            <td>
                                                <asp:Label ID="IffHeaderLabel" runat="server" Text="IFF"  Font-Size="X-Small"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:DropDownList ID="SolicitationDropDownList" runat="server"  AutoPostBack="true" EnableViewState="true"   />      
                                                <ep:TextBox ID="SolicitationNumberTextBox" runat="server" Width="175px"  MaxLength="40" />
                                            </td>
                                            <td>
                                                <table style="border: 1px black; width:175px;">                         
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="IffAbsorbedCheckBox" Text="Absorbed" Font-Size="Small" Checked='<%#Eval("IffAbsorbed") %>' runat="server" OnCheckedChanged="IffAbsorbedCheckBox_OnCheckedChanged" autopostback="true"/>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="IffEmbeddedCheckBox" Text="Embedded" Font-Size="Small"  Checked='<%#Eval("IffEmbedded") %>' runat="server" OnCheckedChanged="IffEmbeddedCheckBox_OnCheckedChanged" autopostback="true"/>
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
           <td rowspan="3" style="vertical-align:top;">
                <asp:FormView ID="DiscountFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="DiscountFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 600px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td colspan="2" style="text-align:center;">
                                <asp:Label ID="DiscountHeaderLabel" runat="server" Text="Discount Information" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:Label ID="BasicDiscountLabel" runat="server" Text="Basic Discount" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="BasicDiscountTextBox" runat="server"  Text='<%# Eval("BasicDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true"  MaxLength="255" /> 
                                        </td>
                                    </tr>         
                                    <tr>
                                        <td>
                                            <asp:Label ID="QuantityDiscountLabel" runat="server" Text="Quantity Discount" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                         <td>
                                            <ep:TextBox ID="QuantityDiscountTextBox" runat="server" Text='<%# Eval("QuantityDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true"  MaxLength="255" /> 
                                        </td>                                  
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="CreditCardDiscountLabel" runat="server" Text="Credit Card Discount" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                         <td>
                                            <ep:TextBox ID="CreditCardDiscountTextBox" runat="server" Text='<%# Eval("CreditCardDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true" MaxLength="255" />                                           
                                        </td>                                  
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <table>
                                   <tr>
                                        <td>
                                            <asp:Label ID="EndOfYearDiscountLabel" runat="server" Text="End of Year Discount"  Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                             <ep:TextBox ID="EndOfYearDiscountTextBox" runat="server" Text='<%# Eval("EndOfYearDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true" MaxLength="255" />                                       
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="PromptPayLabel" runat="server" Text="Prompt Pay Discount" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="PromptPayTextBox" runat="server" Text='<%# Eval("PromptPayDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true"  MaxLength="255" /> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="AdditionalDiscountLabel" runat="server" Text="Additional Discount" Font-Size="X-Small"/>
                                        </td>
                                     </tr>
                                     <tr>
                                         <td>
                                            <ep:TextBox ID="AdditionalDiscountTextBox" runat="server" Text='<%# Eval("AdditionalDiscount") %>' Rows="5" TextMode="MultiLine" Width="296px" Wrap="true"  MaxLength="255" /> 
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
       <tr style="vertical-align:top;" >
            <td style="vertical-align:top;" >
                <asp:FormView ID="DeliveryFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="DeliveryFormView_OnPreRender" >
                <EditItemTemplate>
                    <table class="OutsetBox" style="width: 380px;">
                        <tr class="OutsetBoxHeaderRow" >
                            <td  style="text-align:center;">
                                <asp:Label ID="DeliveryHeaderLabel" runat="server" Text="Delivery Information" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table>
                                     <tr>
                                        <td>
                                            <asp:Label ID="StandardDeliveryLabel" runat="server" Text="Standard Delivery" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="StandardDeliveryTextBox" runat="server" Text='<%# Eval("DeliveryTerms") %>' Rows="3" TextMode="MultiLine" Width="370px" Wrap="true" MaxLength="255" />    
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="ExpeditedDeliveryLabel" runat="server" Text="Expedited Delivery" Font-Size="X-Small"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <ep:TextBox ID="ExpeditedDeliveryTextBox" runat="server" Text='<%# Eval("ExpeditedDeliveryTerms") %>' Rows="3" TextMode="MultiLine" Width="370px" Wrap="true" MaxLength="255" />
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
            <td style="height:84px; vertical-align:top;">
            </td>
        </tr>
    </table>
</asp:Content>
