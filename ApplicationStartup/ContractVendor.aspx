<%@ Page Title="" Language="C#" MasterPageFile="~/ContractView.Master" StylesheetTheme="CMStandard" AutoEventWireup="true" CodeBehind="ContractVendor.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ContractVendor" %>
<%@ MasterType VirtualPath="~/ContractView.Master" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<asp:Content ID="ContractVendorContent" ContentPlaceHolderID="ContractViewContentPlaceHolder" runat="server">

    <script type="text/javascript">
        
        var xPos, yPos;
        var prm = Sys.WebForms.PageRequestManager.getInstance();

        function BeginRequestHandler(sender, args) {
            if (document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>') != null) {
              // Get X and Y positions of scrollbar before the partial postback
                xPos = document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>').scrollLeft;
                yPos = document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>').scrollTop;
        }
    }

    function EndRequestHandler(sender, args) {
        if (document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>') != null) {
            
            document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>').scrollLeft = xPos;
            document.getElementById('<%=StateFormView.FindControl("StatePanel").ClientID%>').scrollTop = yPos;
         }
     }

    function isDigit( evt, txt ) {
        var charCode = (evt.which) ? evt.which : event.keyCode;

        var c = String.fromCharCode( charCode );

        if( txt.indexOf(c) > 0 && charCode == 46) {
            evt.returnValue = false;
            return false;
        }
        else if (charCode != 46 && charCode != 8 && charCode > 31 && (charCode < 48 || charCode > 57)) {
            evt.returnValue = false;
            return false;
        }
        else {
            return true;
        }
    }

     prm.add_beginRequest(BeginRequestHandler);
     prm.add_endRequest(EndRequestHandler);

 </script>


    <table class="OuterTable" >
        <tr>
            <td style="vertical-align:top;">
                <table>
                    <tr>
                        <td style="vertical-align:top;">
                             <asp:FormView ID="ContractVendorSocioFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractVendorSocioFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 410px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td colspan="4" style="text-align:center;">
                                                <asp:Label ID="SocioHeaderLabel" runat="server" Text="Vendor Socio-Economic Information" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <table>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="LargeBusinessCheckBox" Text="Large Business" Checked='<%# GetBusinessSizeCheckBoxValue( ( int )Eval("SocioBusinessSizeId"), "Large" ) %>' runat="server" OnCheckedChanged="LargeBusinessCheckBox_OnCheckedChanged" OnDataBinding="LargeBusinessCheckBox_OnDataBinding"  autopostback="true"/>
                                                            <asp:CheckBox ID="SmallBusinessCheckBox" Text="Small Business" Checked='<%# GetBusinessSizeCheckBoxValue( ( int )Eval("SocioBusinessSizeId"), "Small" ) %>' runat="server" OnCheckedChanged="SmallBusinessCheckBox_OnCheckedChanged" OnDataBinding="SmallBusinessCheckBox_OnDataBinding"  autopostback="true"/>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="VeteranOwnedCheckBox" Text="Veteran Owned" Checked='<%# GetVeteranStatusCheckBoxValue( ( int )Eval("SocioVetStatusId"), "Veteran" ) %>' runat="server" OnCheckedChanged="VeteranOwnedCheckBox_OnCheckedChanged"  autopostback="true"/>
                                                            <asp:CheckBox ID="DisabledVeteranOwnedCheckBox" Text="Disabled Veteran Owned" Checked='<%# GetVeteranStatusCheckBoxValue( ( int )Eval("SocioVetStatusId"), "Disabled Veteran" ) %>' OnCheckedChanged="DisabledVeteranOwnedCheckBox_OnCheckedChanged" runat="server"  autopostback="true"/>
                                                            <asp:CheckBox ID="WomanOwnedCheckBox" Text="Woman Owned" Checked='<%#Eval("SocioWomanOwned") %>' runat="server"  autopostback="true"/>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBox ID="SmallDisadvantagedCheckBox" Text="Small Disadvantaged Business" Checked='<%#Eval("SocioSDB") %>'  runat="server"  autopostback="true"/>
                                                            <asp:CheckBox ID="EightACheckBox" Text="8A" Checked='<%#Eval("Socio8a") %>' runat="server"  autopostback="true"/>
                                                            <asp:CheckBox ID="HubZoneCheckBox" Text="Hub Zone" Checked='<%#Eval("HubZone") %>' runat="server"  autopostback="true"/>
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
                        <td style="vertical-align:top;">
                            <asp:FormView ID="StateFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="StateFormView_OnPreRender"  >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 410px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td colspan="4" style="text-align:center;">
                                            <asp:Label ID="StateHeaderLabel" runat="server" Text="Geographic Coverage" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:panel runat="server" ID="StatePanel" CssClass="statePanel" Height="154px" >
                                                <table class="stateTable">
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox ID="Group52" Text="50 States, DC, PR" Checked='<%# Eval("GeographicCoverage.Group52") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left" OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox ID="Group51" Text="50 States, DC" Checked='<%# Eval("GeographicCoverage.Group51") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox ID="Group50" Text="50 States" Checked='<%# Eval("GeographicCoverage.Group50") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox ID="Group49" Text="48 Contiguous, DC" Checked='<%# Eval("GeographicCoverage.Group49") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox ID="AL" Text="Alabama" Checked='<%# Eval("GeographicCoverage.AL") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox ID="AK" Text="Alaska" Checked='<%# Eval("GeographicCoverage.AK") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Arizona" ID="AZ" Checked='<%# Eval("GeographicCoverage.AZ") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Arkansas" ID="AR" Checked='<%# Eval("GeographicCoverage.AR") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="California" ID="CA" Checked='<%# Eval("GeographicCoverage.CA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Colorado" ID="CO" Checked='<%# Eval("GeographicCoverage.CO") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Connecticut" ID="CT" Checked='<%# Eval("GeographicCoverage.CT") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Delaware" ID="DE" Checked='<%# Eval("GeographicCoverage.DE") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="District Of Columbia" ID="DC" Checked='<%# Eval("GeographicCoverage.DC") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Florida" ID="FL" Checked='<%# Eval("GeographicCoverage.FL") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Georgia" ID="GA" Checked='<%# Eval("GeographicCoverage.GA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Hawaii" ID="HI" Checked='<%# Eval("GeographicCoverage.HI") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Idaho" ID="ID" Checked='<%# Eval("GeographicCoverage.ID") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Illinois" ID="IL" Checked='<%# Eval("GeographicCoverage.IL") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Indiana" ID="IN" Checked='<%# Eval("GeographicCoverage.IN") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Iowa" ID="IA" Checked='<%# Eval("GeographicCoverage.IA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Kansas" ID="KS" Checked='<%# Eval("GeographicCoverage.KS") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Kentucky" ID="KY" Checked='<%# Eval("GeographicCoverage.KY") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Louisiana" ID="LA" Checked='<%# Eval("GeographicCoverage.LA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Maine" ID="ME" Checked='<%# Eval("GeographicCoverage.ME") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Maryland" ID="MD" Checked='<%# Eval("GeographicCoverage.MD") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Massachusetts" ID="MA" Checked='<%# Eval("GeographicCoverage.MA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Michigan" ID="MI" Checked='<%# Eval("GeographicCoverage.MI") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Minnesota" ID="MN" Checked='<%# Eval("GeographicCoverage.MN") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Mississippi" ID="MS" Checked='<%# Eval("GeographicCoverage.MS") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Missouri" ID="MO" Checked='<%# Eval("GeographicCoverage.MO") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Montana" ID="MT" Checked='<%# Eval("GeographicCoverage.MT") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Nebraska" ID="NE" Checked='<%# Eval("GeographicCoverage.NE") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Nevada" ID="NV" Checked='<%# Eval("GeographicCoverage.NV") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="New Hampshire" ID="NH" Checked='<%# Eval("GeographicCoverage.NH") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="New Jersey" ID="NJ" Checked='<%# Eval("GeographicCoverage.NJ") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="New Mexico" ID="NM" Checked='<%# Eval("GeographicCoverage.NM") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="New York" ID="NY" Checked='<%# Eval("GeographicCoverage.NY") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="North Carolina" ID="NC" Checked='<%# Eval("GeographicCoverage.NC") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="North Dakota" ID="ND" Checked='<%# Eval("GeographicCoverage.ND") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Ohio" ID="OH" Checked='<%# Eval("GeographicCoverage.OH") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Oklahoma" ID="OK" Checked='<%# Eval("GeographicCoverage.OK") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Oregon" ID="OR" Checked='<%# Eval("GeographicCoverage.OR") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Pennsylvania" ID="PA" Checked='<%# Eval("GeographicCoverage.PA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Rhode Island" ID="RI" Checked='<%# Eval("GeographicCoverage.RI") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="South Carolina" ID="SC" Checked='<%# Eval("GeographicCoverage.SC") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="South Dakota" ID="SD" Checked='<%# Eval("GeographicCoverage.SD") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Tennessee" ID="TN" Checked='<%# Eval("GeographicCoverage.TN") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Texas" ID="TX" Checked='<%# Eval("GeographicCoverage.TX") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Utah" ID="UT" Checked='<%# Eval("GeographicCoverage.UT") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Vermont" ID="VT" Checked='<%# Eval("GeographicCoverage.VT") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Virginia" ID="VA" Checked='<%# Eval("GeographicCoverage.VA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Washington" ID="WA" Checked='<%# Eval("GeographicCoverage.WA") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="West Virginia" ID="WV" Checked='<%# Eval("GeographicCoverage.WV") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Wisconsin" ID="WI" Checked='<%# Eval("GeographicCoverage.WI") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Wyoming" ID="WY"  Checked='<%# Eval("GeographicCoverage.WY") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBox" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>
                                                        <td align="right">
                                                            <asp:CheckBox Text="Puerto Rico" ID="PR"  Checked='<%# Eval("GeographicCoverage.PR") %>' Font-Size="X-Small" runat="server" AutoPostBack="true" CssClass="stateCheckBoxPuertoRico" TextAlign="Left"  OnCheckedChanged="StateCheckBox_OnCheckChanged"  />
                                                        </td>                                                        
                                                    </tr>                                                                       
                                                    </table>  
                                            </asp:panel>
                                            </td>
                              
                              
                                        </tr>
                                    </table>
                                </EditItemTemplate>
                            </asp:FormView>   
                        </td>
                    </tr>
                </table>
            </td>
            <td style="vertical-align:top;">
                <table>
                    <tr style="vertical-align:top;">
                        <td style="vertical-align:top;">
                            <asp:FormView ID="VendorAttributesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="VendorAttributesFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 400px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td colspan="4" style="text-align:center;">
                                                <asp:Label ID="VendorAttributesHeaderLabel" runat="server" Text="Vendor Attributes" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <table style="width: 398px;">
                                                    <tr>
                                                        <td></td>
                                                        <td style="text-align:right;">
                                                            <asp:Label ID="SAMUEILabel" Text="UEI" runat="server" />
                                                        </td>
                                                        <td>
                                                            <ep:TextBox ID="SAMUEITextBox" Text='<%# Eval("SAMUEI") %>' MaxLength="12" runat="server" Width="120px" OnTextChanged="VendorAttributesFormView_OnChange" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td></td>
                                                        <td style="text-align:right;">
                                                            <asp:Label ID="DunsLabel" Text="DUNS" runat="server" />
                                                        </td>
                                                        <td>
                                                            <ep:TextBox ID="DunsTextBox" Text='<%# Eval("DUNS") %>' MaxLength="9" runat="server" Width="120px" onkeypress="return isDigit(event,this.value);" OnTextChanged="VendorAttributesFormView_OnChange" />
                                                        </td>
                                                    </tr>
                                                    <tr style="border-bottom: 1px solid black;">
                                                        <td></td>
                                                        <td style="text-align:right;">
                                                            <asp:Label ID="TinLabel" Text="TIN" runat="server" />
                                                        </td>
                                       
                                                        <td>
                                                            <ep:TextBox ID="TinTextBox" Text='<%# Eval("TIN") %>' MaxLength="9" runat="server" Width="120px" onkeypress="return isDigit(event,this.value);" OnTextChanged="VendorAttributesFormView_OnChange" />
                                                        </td>
                                                    </tr>
                                                    <tr style="border-bottom: 1px solid black; height:30px;">
                                      
                                                        <td style="text-align:right;">
                                                            <asp:Label ID="VendorTypeLabel" Text="Vendor Type:" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="DistributorCheckBox" Text="Distributor" runat="server" AutoPostBack="true" OnCheckedChanged="DistributorCheckBox_OnCheckedChanged"  OnDataBinding="DistributorCheckBox_OnDataBinding" ToolTip="Distributor. Or, also check Manufacturer to indicate both."/>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="ManufacturerCheckBox" Text="Manufacturer" runat="server" AutoPostBack="true" OnCheckedChanged="ManufacturerCheckBox_OnCheckedChanged" OnDataBinding="ManufacturerCheckBox_OnDataBinding" ToolTip="Manufacturer. Or, also check Distributor to indicate both."/>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="ServicesCheckBox" Text="Service" runat="server" AutoPostBack="true" OnCheckedChanged="ServicesCheckBox_OnCheckedChanged" OnDataBinding="ServicesCheckBox_OnDataBinding" ToolTip="Service."/>
                                                        </td>
                                                    </tr>
                                                    <tr style="height:30px;" >
                                                        <td></td>
                                                        <td>
                                                            <asp:CheckBox ID="CreditCardCheckBox" Text="Credit Card" Checked='<%#Eval("CreditCardAccepted") %>' runat="server" AutoPostBack="true" ToolTip="Credit Card accepted." />
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="HazardousCheckBox" Text="Hazardous" Checked='<%#Eval("HazardousMaterial") %>' runat="server" AutoPostBack="true"  ToolTip="Vendor provides hazardous material." />
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
                        <td style="vertical-align:top;" >
                            <asp:FormView ID="ContractVendorInsuranceDatesFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ContractVendorInsuranceDatesFormView_OnPreRender" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 400px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td colspan="4" style="text-align:center;">
                                                <asp:Label ID="InsurancePolicyHeaderLabel" runat="server" Text="Insurance Policy" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td style="text-align:center;">   
                                                <table>
                                                    <tr>
                                                        <td colspan="2">
                                                            <asp:Label ID="InsurancePolicyEffectiveDateLabel" Text="Effective Date" runat="server" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <ep:TextBox ID="InsurancePolicyEffectiveDateTextBox" runat="server" OnDataBinding="InsurancePolicyEffectiveDateTextBox_OnDataBinding" OnTextChanged="ContractVendorInsuranceDatesFormView_OnChange" SkinId="DateTextBox"/>    
                                                        </td>
                                                        <td>
                                                            <asp:ImageButton ID="InsurancePolicyEffectiveDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select insurance policy effective date" OnClientClick="OnInsurancePolicyEffectiveDateButtonClick()" />                           
                                                        </td>                              
                                                    </tr>
                                                </table>                                                          
                                            </td>                                   
                                            <td style="text-align:center;">   
                                                <table>
                                                    <tr>
                                                        <td colspan="2">
                                                            <asp:Label ID="InsurancePolicyExpirationDateLabel" Text="Expiration Date" runat="server" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <ep:TextBox ID="InsurancePolicyExpirationDateTextBox" runat="server" OnDataBinding="InsurancePolicyExpirationDateTextBox_OnDataBinding" OnTextChanged="ContractVendorInsuranceDatesFormView_OnChange" SkinId="DateTextBox"/>    
                                                        </td>
                                                        <td>
                                                            <asp:ImageButton ID="InsurancePolicyExpirationDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select insurance policy expiration date" OnClientClick="OnInsurancePolicyExpirationDateButtonClick()" />                           
                                                        </td>                              
                                                    </tr>
                                                </table>                                                          
                                            </td>    
                                            <td>
                                            </td>                        
                                        </tr>
                                    </table>
                                    <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined"  />
                                    <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False"  />
                                </EditItemTemplate>
                            </asp:FormView>
                        </td>
                    </tr>
                </table>
            </td>
            <td  style="vertical-align:top;" >
                <table>
                    <tr>
                        <td  style="vertical-align:top;" >
                           <asp:FormView ID="WarrantyInformationFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="WarrantyInformationFormView_OnPreRender" >
                            <EditItemTemplate>
                                <table class="OutsetBox" style="width: 400px;">
                                    <tr class="OutsetBoxHeaderRow" >
                                        <td colspan="4" style="text-align:center;">
                                            <asp:Label ID="WarrantyInformationHeaderLabel" runat="server" Text="Warranty Information" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <table>
                                                <tr>                                       
                                                    <td >
                                                        <asp:Label ID="WarrantyDurationLabel" Text="Warranty Duration:" runat="server" Width="110px" />
                                                    </td>
                                                    <td>
                                                        <ep:TextBox ID="WarrantyDurationTextBox" Text='<%# Eval("WarrantyDuration") %>' MaxLength="20" runat="server" Width="100px"  OnTextChanged="WarrantyInformationFormView_OnChange" />
                                                    </td>
                                                    <td style="text-align:left;">
                                                        <asp:Label ID="WarrantyDurationHelpLabel" Width="140px" Text="(example 1 year, 60 days, etc.)" runat="server" Font-Size="X-Small"  />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:Label ID="WarrantyDescriptionLabel" Text="Warranty Description:" runat="server" />
                                                    </td>
                                                    <td>
                                                    </td>
     
                                                </tr>
                                                <tr>
                                                    <td colspan="3">
                                                        <ep:TextBox ID="WarrantyDescriptionTextBox" runat="server" MaxLength="1000" Rows="15" Width="380" Height="50"
                                                                                   Text='<%# Eval("WarrantyNotes") %>' Wrap="true" TextMode="MultiLine" OnTextChanged="WarrantyInformationFormView_OnChange" />
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
                        <td  style="vertical-align:top;" >
                            <asp:FormView ID="ReturnedGoodsPolicyFormView" runat="server" DefaultMode="Edit" Width="100%" OnPreRender="ReturnedGoodsPolicyFormView_OnPreRender"  OnDataBound="ReturnedGoodsPolicyFormView_OnDataBound" >
                                <EditItemTemplate>
                                    <table class="OutsetBox" style="width: 400px;">
                                        <tr class="OutsetBoxHeaderRow" >
                                            <td style="text-align:center;">
                                                <asp:Label ID="ReturnedGoodsPolicyHeaderLabel" runat="server" Text="Returned Goods Policy Information" />
                                            </td>
                                        </tr>
                                         <tr>
                                            <td>
                                                <table>  
                                                    <tr>                                                     
                                                        <td>
                                                            <asp:Label ID="ReturnedGoodsPolicyTypeLabel" Text="Returned Goods Policy Type:" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="ReturnedGoodsPolicyTypeDropDownList" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="ReturnedGoodsPolicyFormView_OnChange" />                                            
                                                        </td>                                                    
                                                    </tr>                                        
                                                    <tr>
                                                        <td colspan="2">
                                                            <asp:Label ID="ReturnedGoodsPolicyNotesLabel" Text="Returned Goods Policy Notes:" runat="server" />
                                                        </td>                           
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2">
                                                            <ep:TextBox ID="ReturnedGoodsPolicyNotesTextBox" runat="server" MaxLength="1000" Rows="15" Width="380" Height="100"
                                                                                        Text='<%# Eval("ReturnedGoodsPolicyNotes") %>' Wrap="true" TextMode="MultiLine" OnTextChanged="ReturnedGoodsPolicyFormView_OnChange" />
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
            <td style="height:154px; vertical-align:top;">
            </td>
        </tr>
    </table>
</asp:Content>
