<%@ Page Language="vb" AutoEventWireup="false" EnableEventValidation="false" CodeBehind="NAC_CM_Contracts.aspx.vb"
    Inherits="NACCM.NAC_CM_Contracts" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>NAC CM Contracts</title>
   
</head>
<body style="background-color: #ece9d8;" >
    <form id="form1" runat="server" style="z-index: 4; position:fixed; top:0px; left:0px; width:900px; height:700px;" >

    <table style="width: 875">
        <tr>
            <td>
                <div style="color: #003399; font-size: x-large">
                    <strong>NAC CM</strong></div>
                <div style="font-size: large">
                    <em>Contract Management</em></div>
            </td>
            <td style="width: 256px">
            </td>
            <td align="left">
                <asp:Button ID="btnContractSearch" runat="server" Text="Contract Search" Width="109px" Visible="false">
                </asp:Button><asp:Button ID="btnMainMenu" runat="server" Text="Main Menu" Visible="false"></asp:Button><asp:Button
                    ID="btnExit" runat="server" Text="Close Window" Width="94px"></asp:Button>
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
            </td>
            <td>
            </td>
        </tr>
    </table>
    <table style="width: 875">
        <tr valign="top">
            <td valign="top" align="left" colspan="3">
                <asp:FormView ID="fvContractInfo" runat="server" DataKeyNames="CntrctNum" DataSourceID="ContractDataSource"
                    OnPreRender="fvContractInfo_PreRender" CellPadding="4" ForeColor="#333333">
                    <ItemTemplate>
                        <table style="height: 500; width: 700; background-color: #9baeb9">
                            <tr valign="top" align="center">
                                <td style="width: 290px; height: 93px" colspan="2">
                                    <table style="height: 50; width: 300; background-color: White">
                                        <tr align="left" valign="top">
                                            <td colspan="2" rowspan="2">
                                                <asp:Label ID="lbContractNumber" runat="server" Text='<%#FormatContractNumber(Eval("CntrctNum") )%>'
                                                    Font-Names="Arial"></asp:Label>
                                            </td>
                                            <td>
                                                <div style="font-family: Arial; color: #000099; font-size: small">
                                                    Contract Status:</div>
                                            </td>
                                        </tr>
                                        <tr align="left">
                                            <td>
                                                <asp:Label ID="lbDateCompleted" runat="server" Text='<%#ContractStatus(Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>'
                                                    Font-Names="Arial" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="3" align="left">
                                                <asp:Label ID="lbContractName" runat="server" Text='<%# Eval("Contractor_Name") %>'
                                                    Font-Names="Arial"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="height: 93px" align="left">
                                    <table style="width: 150; background-color: #ece9d8">
                                        <tr>
                                            <td style="background-color: Silver" align="center">
                                                <div style="font-family: Arial; color: #990066; font-size: small">
                                                    <strong>CO Information</strong></div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:DropDownList ID="dlContractOfficers" runat="server" Width="250px" Font-Names="Arial" AutoPostBack="True"
                                                 OnSelectedIndexChanged="dlContractOfficers_OnSelectedIndexChanged"    />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="center">
                                                <div style="font-family: Arial; color: #000099; font-size: small">
                                                    Phone</div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lbCOPhone" runat="server" Width="136px" Text='<%# Eval("CO_Phone") %>'
                                                    Font-Names="Arial"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="height: 93px">
                                    <table style="width: 250; background-color: #ece9d8">
                                        <tr>
                                            <td align="center" style="background-color: silver" colspan="2">
                                                <div style="font-family: Arial; color: #990066; font-size: small">
                                                    <strong>Schedule Information</strong>
                                                </font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2" style="font-family: Arial">
                                                <asp:Label ID="lbScheduleName" runat="server" Text='<%# Eval("Schedule_Name") %>'
                                                    Font-Names="Arial"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="width: 124px" align="center">
                                                <div  style="font-family: Arial; color: #000099; font-size: small">                                                    
                                                    <asp:Label visible="false" ID="AssistantDirectorLabel" runat="server" Text="Assistant Director" Font-Names="Arial"></asp:Label>
                                                 </div>
                                            </td>
                                            <td align="center">
                                                <div style="font-family: Arial; color: #000099; font-size: small">
                                                    <asp:Label visible="false" ID="SeniorContractSpecialistLabel" runat="server" Text="Senior Contract Specialist" Font-Names="Arial"></asp:Label>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="width: 124px">
                                                <asp:Label  visible="false" ID="lbADName" runat="server" Text='<%# Eval("AD_Name") %>' Font-Names="Arial"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:Label  visible="false" ID="lbSMName" runat="server" Text='<%# Eval("SM_Name") %>' Font-Names="Arial"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr style="background-color: #ece9d8">
                                <td style="height: 38px" valign="top" colspan="4">
                                    <asp:Button ID="btnGeneral" runat="server" Text="General" OnClick="btnView_click"
                                        BackColor="AntiqueWhite" ForeColor="Red" />
                                    <asp:Button ID="btnBPAInfo" runat="server" Text="BPA Information" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" Visible="false" />
                                    <asp:Button ID="btnBPAPrice" runat="server" Text="BPA Pricelist" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" Visible="false" />
                                    <asp:Button ID="btnVendor" runat="server" Text="Vendor" OnClick="btnView_click" BackColor="White"
                                        ForeColor="Black" />
                                    <asp:Button ID="btnContract" runat="server" Text="Contract" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" />
                                    <asp:Button ID="btnPOC" runat="server" Text="Point of Contact" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" />
                                    <asp:Button ID="btnPrice" runat="server" Text="Price List" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" />
                                    <asp:Button ID="btnSales" runat="server" Text="Sales" OnClick="btnView_click" BackColor="White"
                                        ForeColor="Black" />
                                    <asp:Button ID="btnChecks" runat="server" Text="Checks" OnClick="btnView_click" BackColor="White"
                                        ForeColor="Black" />
                                    <asp:Button ID="btnSBA" runat="server" Text="SBA Plans" OnClick="btnView_click" BackColor="White"
                                        ForeColor="Black" />
                                    <asp:Button ID="btnFSSDetails" runat="server" Text="FSS Contract Details" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" Visible="false" />
                                    <asp:Button ID="btnBOC" runat="server" Text="Bill of Collections" OnClick="btnView_click"
                                        BackColor="White" ForeColor="Black" Visible="false" />
                                    <asp:MultiView ID="itemView" runat="server">
                                        <asp:View ID="general" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700">
                                                <tr valign="top">
                                                    <td>
                                                        <asp:Table ID="tblVendorMail" runat="server" Width="400" BorderStyle="Outset" Font-Names="Arial">
                                                            <asp:TableRow BackColor="Silver">
                                                                <asp:TableCell ColumnSpan="4" HorizontalAlign="Center" Font-Names="Arial" Font-Size="Small"
                                                                    ForeColor="#000099">Vendor Mailing Address/Web Address</asp:TableCell></asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell Font-Size="X-Small">Address 1:</asp:TableCell><asp:TableCell ColumnSpan="3"
                                                                    Font-Size="Small">
                                                                    <asp:TextBox ID="tbAddress1" runat="server" ReadOnly="True" Text='<%# Eval("Primary_Address_1") %>'
                                                                        Font-Size="Small" Width="150" /></asp:TableCell></asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell Font-Size="X-Small"> Address 2:</asp:TableCell><asp:TableCell ColumnSpan="3"
                                                                    Font-Size="Small">
                                                                    <asp:TextBox ID="tbAddress2" runat="server" ReadOnly="True" Text='<%# Eval("Primary_Address_2") %>'
                                                                        Font-Size="Small" Width="150" /></asp:TableCell></asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell Font-Size="X-Small"> City:</asp:TableCell><asp:TableCell Font-Size="Small">
                                                                    <asp:TextBox ID="tbCity" runat="server" ReadOnly="True" Text='<%# Eval("Primary_City") %>'
                                                                        Font-Size="Small" Width="150" /></asp:TableCell><asp:TableCell>
                                                                            <asp:DropDownList ID="dlState" runat="server" Enabled="false" Font-Size="Smaller"  />
                                                                        </asp:TableCell><asp:TableCell Font-Size="X-Small" Wrap="false">
                                                                            Zip:
                                                                            <asp:TextBox ID="tbZip" runat="server" Columns="9" ReadOnly="true" Text='<%# Eval("Primary_Zip") %>'
                                                                                Font-Size="Small" Width="75" /></asp:TableCell></asp:TableRow>
                                                                                <asp:TableRow><asp:TableCell Font-Size="Small" ForeColor="#000099">Company Web Page:</asp:TableCell><asp:TableCell ColumnSpan="3"> <asp:TextBox ID="tbCompanyWebPage" runat="server" ReadOnly="True" Text='<%# Eval("POC_VendorWeb") %>'
                                                                        Font-Size="Small" Width="250" /></asp:TableCell></asp:TableRow>
                                                        </asp:Table>
                                                        <asp:FormView ID="fvFSSContractInfo" runat="server" Visible="false" Font-Names="Arial">
                                                            <ItemTemplate>
                                                                <asp:Table ID="tblBPAContractInfo" runat="server" Width="400" BorderStyle="Outset">
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="3">FSS Counterpart Contract #</asp:TableHeaderCell><asp:TableHeaderCell
                                                                            ColumnSpan="1">FSS Contract Status:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="3">
                                                                        <asp:Label ID="lbFSSContractNum" runat="server" Text='<%#Eval("CntrctNum") %>' /><br />
                                                                            <asp:Label ID="lbFSSContractName" runat="server" Text='<%#Eval("Contractor_Name") %>' /></asp:TableCell><asp:TableCell>
                                                                                <asp:Label ID="lbFSSContractStatus" runat="server" Text='<%#ContractStatus(Eval("Dates_CntrctExp"),Eval("Dates_Completion"))%>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="4">Schedule:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="4">
                                                                            <asp:Label ID="lbFSSSchedule" runat="server" Text='<%#Eval("Schedule_Name") %>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="4">CO:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="4">
                                                                            <asp:Label ID="lbFSSCO" runat="server" Text='<%#Eval("CO_Name") %>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell>Awarded:</asp:TableHeaderCell><asp:TableHeaderCell>Effective:</asp:TableHeaderCell><asp:TableHeaderCell>Expiration:</asp:TableHeaderCell><asp:TableHeaderCell>End Date:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell>
                                                                            <asp:Label ID="lbFSSDateAward" runat="server" Text='<%#Eval("Dates_CntrctAward","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                <asp:Label ID="lbFSSDateEff" runat="server" Text='<%#Eval("Dates_Effective","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                    <asp:Label ID="lbFSSDateCntrctExp" runat="server" Text='<%#Eval("Dates_CntrctExp","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                        <asp:Label ID="lbFSSDatecomp" runat="server" Text='<%#Eval("Dates_Completion","{0:d}") %>' /></asp:TableCell></asp:TableRow>
                                                                                        
                                                                </asp:Table>
                                                            </ItemTemplate>
                                                        </asp:FormView>
                                                    </td>
                                                    <td align="left">
                                                        <table>
                                                            <tr>
                                                            <td>
                                                            <table width="250">
                                                                <tr>
                                                                    <td colspan="3" align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                        Short Contract Description
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td colspan="3" style="background-color: White" align="center">
                                                                        <div style="font-family: Arial; font-size: small">
                                                                            <asp:TextBox ID="tbShortdesc" runat="server" Width="250" ReadOnly="True" Text='<%# Eval("Drug_Covered") %>' /></div>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td colspan="3" align="center">
                                                                        <table style="border-right: #ece9d8 5px solid; border-top: #ece9d8 5px solid; border-left: #ece9d8 5px solid;
                                                                            border-bottom: #ece9d8 5px solid; width: 200px; background-color: #ECE9D8">
                                                                            <tr>
                                                                                <td align="center">
                                                                                    <asp:Table ID="tblPrimeVendor" runat="server" BorderStyle="Outset" BackColor="Silver"
                                                                                        Font-Names="Arial">
                                                                                        <asp:TableRow>
                                                                                            <asp:TableCell HorizontalAlign="Center" Font-Size="Small" ForeColor="#000099">
                                                                                                Prime Vendor<br>
                                                                                                Participation<asp:CheckBox ID="cbPrimeVendor" runat="server" Enabled="false" Font-Size="1"
                                                                                                    ForeColor="#000099" Width="40" Checked='<%# Eval("PV_Participation") %>' /></asp:TableCell></asp:TableRow>
                                                                                        <asp:TableRow>
                                                                                            <asp:TableCell HorizontalAlign="Center" Font-Size="Small" ForeColor="#000099">
                                                                                                VA/DOD<br>
                                                                                                Contract<asp:CheckBox ID="cbDODVACOntract" runat="server" Enabled="false" Font-Names="Arial"
                                                                                                    Font-Size="1" ForeColor="#000099" Width="40" Checked='<%#Eval("VA_DOD") %>' /></asp:TableCell></asp:TableRow>
                                                                                    </asp:Table>
                                                                                    <asp:Table ID="tblInsurance" runat="server" Font-Names="Arial" Visible="false">
                                                                                        <asp:TableHeaderRow>
                                                                                            <asp:TableHeaderCell ColumnSpan="2" BackColor="Silver" ForeColor="#000099" Font-Size="Small">Insurance Policy Info:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                                        <asp:TableRow>
                                                                                            <asp:TableCell>
                                                                                                <asp:TextBox ID="tbInsDateEff" ReadOnly="true" runat="server" Text='<%#Eval("Insurance_Policy_Effective_Date", "{0:d}") %>'
                                                                                                    ToolTip="Insurance Policy Effective Date" Width="75" />
                                                                                            </asp:TableCell><asp:TableCell>
                                                                                                <asp:TextBox ID="TextBox1" ReadOnly="true" runat="server" Text='<%#Eval("Insurance_Policy_Expiration_Date", "{0:d}" )%>'
                                                                                                    ToolTip="Insurance Policy Expiration Date" Width="75" /></asp:TableCell></asp:TableRow>
                                                                                    </asp:Table>
                                                                                </td>
                                                                                <td align="center">
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                     </tr>
                                                     <tr>
                                                                <td valign="top"  align="center" >
                                                                     <table  id="TradeAgreementTable"  visible="false" runat="server"  style="table-layout:fixed; border-style: outset; background-color: #ECE9D8; width: 150px">
                                                                        <tr>
                                                                            <td colspan="2" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                                font-size:small" align="center">
                                                                                <asp:Label ID="TradeAgreementHeaderLabel"  visible="false"  runat="server" Text="Trade Agreement Act Compliant" Font-Size="Small" />
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                              <td style="font-family: Arial; color: #000099; font-size: small; " align="left">
                                                                                    
                                                                                    <asp:CheckBox ID="TAAYesCheckBox"  Visible="false" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                        Checked='<%#GetTAACheckBoxValue( Eval("TradeAgreementActCompliance"), "Yes" ) %>' 
                                                                                         Text="Yes" Enabled="false" autopostback="true"/>
                                                                              </td>
                                                                         </tr>
                                                                        <tr>
                                                                             <td style="font-family: Arial; font-size: small;" align="left">
                                                                                    
                                                                                    <asp:CheckBox ID="TAAOtherCheckBox"  Visible="false"  runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                         Checked='<%#GetTAACheckBoxValue( Eval("TradeAgreementActCompliance"), "Other" ) %>' 
                                                                                       Text="Other"  Enabled="false" autopostback="true"/>
                                                                              </td>
                                                                         </tr>
                                                                    </table>                                                               
                                                                </td>                                                       
                                                          </tr>
                                                             <tr>
                                                                <td>
                                                                    <table id="StimulusActTable"  visible="true" runat="server"  style="table-layout:fixed; border-style: outset; background-color: #ECE9D8; width: 150px">
                                                                       <tr>
                                                                            <td colspan="2" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                                font-size:small" align="center">
                                                                                <asp:Label ID="StimulusActHeaderLabel"  visible="true"  runat="server" Text="Stimulus Act" Font-Size="Small" />
                                                                            </td>
                                                                        </tr>                                                           
                                                                        <tr>
                                                                              <td style="font-family: Arial; color: #000099; font-size: small; " align="center">
                                                                                    
                                                                                    <asp:CheckBox ID="StimulusActCheckBox"  Visible="true"  Enabled="false" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                        Checked='<%# Eval("StimulusAct") %>' 
                                                                                         Text="" />
                                                                              </td>
                                                                        </tr>
                                                                     </table>  
                                                                </td>
                                                            </tr>
                                                         
                                                          
                                                     </table>
                                                    </td>
                                                    <td>
                                                        <table>
                                                            <tr>
                                                                <td>
                                                                    <asp:Panel ID="pnGVSINS" runat="server" Width="125" Height="175" ScrollBars="Vertical">
                                                                        <asp:GridView ID="gvSINS" runat="server" AutoGenerateColumns="False" DataSourceID="SINdataSource"
                                                                            Width="100">
                                                                            <Columns>
                                                                                <asp:BoundField DataField="SINs" HeaderText="SINs" HeaderStyle-BackColor="Silver"
                                                                                    HeaderStyle-ForeColor="#000099" />
                                                                                <asp:CheckBoxField DataField="Recoverable" HeaderText="RC" HeaderStyle-BackColor="Silver"
                                                                                    HeaderStyle-ForeColor="#000099" />
                                                                            </Columns>
                                                                        </asp:GridView>
                                                                    </asp:Panel>
                                                                    <asp:Button ID="btnRemove" runat="server" Text="Remove" Width="60" Font-Size="10" Visible="false">
                                                                    </asp:Button>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                     <table id="StandardizedTable"  visible="true" runat="server"  style="table-layout:fixed; border-style: outset; background-color: #ECE9D8; width: 150px">
                                                                        <tr>
                                                                            <td colspan="2" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                                font-size:small" align="center">
                                                                                <asp:Label ID="StandardizedLabel"  visible="true"  runat="server" Text="Standardized" Font-Size="Small" />
                                                                            </td>
                                                                        </tr>                                                           
                                                                        <tr>
                                                                            <td style="font-family: Arial; color: #000099; font-size: small; " align="center">
                                                                                <asp:CheckBox ID="StandardizedCheckBox"  Visible="True"  Enabled="False" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                    Checked='<%#  Eval("Standardized") %>' Text=""  />
                                                                            </td>
                                                                        </tr>
                                                                     </table>  
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td valign="top">
                                                        <table style="border-style: outset; background-color: #ECE9D8; width: 400">
                                                            <tr>
                                                                <td colspan="4" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                    font-size: x-small" align="center">
                                                                    Contract Dates
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                    Awarded:
                                                                </td>
                                                                <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                    Effective:
                                                                </td>
                                                                <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                    Expiration:
                                                                </td>
                                                                <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                    End Date:
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Label ID="lbAwardDate" runat="server" Text='<%# Eval("Dates_CntrctAward","{0:d}") %>'
                                                                        Width="90" />
                                                                </td>
                                                                <td>
                                                                    <asp:Label ID="lbEffectiveDate" runat="server" Text='<%#Eval("Dates_Effective","{0:d}") %>'
                                                                        Width="90"></asp:Label>
                                                                </td>
                                                                <td>
                                                                <asp:TextBox ID="tbExpirationDate" runat="server" ReadOnly="true" Text='<%#Eval("Dates_CntrctExp","{0:d}") %>' Width="90" />
                                                                   
                                                                </td>
                                                                <td>
                                                                <asp:TextBox ID="tbEndDate" runat="server" Width="90"  ForeColor="Red" readonly="true" Text='<%#Eval("Dates_Completion","{0:d}") %>' />
                                                                    
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Total Option Yrs:
                                                                </td>
                                                                <td>
                                                                <asp:TextBox ID="tbOptionYrs" runat="server" Text='<%#Eval("Dates_TotOptYrs") %>' Width="75" ReadOnly="true" />
                                                                  
                                                                </td>
                                                                <td colspan="2" rowspan="2">
                                                                    <table style="border-style: outset" width="175">
                                                                        <tr>
                                                                            <td colspan="2" align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                Terminated By:
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                Default:
                                                                                <asp:CheckBox ID="cbTermDefault" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                    Font-Size="1" Checked='<%#Eval("Terminated_Default") %>' Visible="true"  Enabled="false" />
                                                                            </td>
                                                                            <td style="font-family: Arial; font-size: x-small">
                                                                                Convenience:
                                                                                <asp:CheckBox ID="cbTermConvenience" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                    Font-Size="1" Checked='<%#Eval("Terminated_Convenience") %>' Visible="true"  Enabled="false" />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Current Option Year:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbCurrentOptionYear" runat="server" Width="75" ReadOnly="True" Text='<%#GetCurrentOptionYear(Eval("Dates_TotOptYrs"),Eval("Dates_Effective"),Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table style="border-style: outset; background-color: #ECE9D8">
                                                            <tr>
                                                                <td colspan="4" style="background-color: silver; font-family: Arial; color: #000099;
                                                                    font-size: smaller">
                                                                    Vendor Contract Administration
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbNameVC" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Name") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbPhoneVC" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Phone","{0:(###)###-####}") %>'></asp:TextBox>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbExtVC" runat="server" Width="40" ReadOnly="True" Text='<%# Eval("POC_Primary_Ext") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbFaxVC" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Fax") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbEmailVC" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Email") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td colspan="3" style="font-family: Arial; font-size: smaller">
                                                        Contract Notes:<br>
                                                        <asp:TextBox ID="tbContractNotes" runat="server" Rows="10" Width="700" ReadOnly="True"
                                                            Text='<%# Eval("POC_Notes") %>' Wrap="true" TextMode="MultiLine" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="vendorView" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                <tr>
                                                    <td valign="top">
                                                        <table style="border-style: outset" width="350">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="5" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Vendor Socio-Economic Information
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Size:<br>
                                                                    <asp:DropDownList ID="dlBusinessSize" runat="server" Width="75" Enabled="false">
                                                                    </asp:DropDownList>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Small Disadvantaged Business<br>
                                                                    <asp:CheckBox ID="cbSDB" runat="server" Enabled="false" Checked='<%#Eval("Socio_SDB") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    8A<br>
                                                                    <asp:CheckBox ID="cbSA" runat="server" Enabled="false" Checked='<%#Eval("Socio_8a") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Woman<br>
                                                                    <asp:CheckBox ID="cbWoman" runat="server" Enabled="false" Checked='<%#Eval("Socio_Woman") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Hub Zone<br>
                                                                    <asp:CheckBox ID="cbHub" runat="server" Enabled="false" Checked='<%#Eval("Socio_HubZone") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                </td>
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Veteran Status<br>
                                                                    <asp:DropDownList ID="dlVeteranStatus" Enabled="false" runat="server" Width="125">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <br>
                                                        <table width="350">
                                                            <tr align="center">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    DUNS<br>
                                                                    <asp:TextBox ID="tbDUNS" runat="server" Width="125" ReadOnly="True" Text='<%# Eval("DUNS") %>'
                                                                        Font-Size="small" />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Vendor Type<br>
                                                                    <asp:DropDownList ID="dlVendorType" runat="server" Width="125" Enabled="false">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Tax Identification Number (TIN)<br>
                                                                    <asp:TextBox ID="tbTIN" runat="server" Width="125" ReadOnly="True" Font-Size="small"
                                                                        Text='<%# Eval("TIN") %>'></asp:TextBox>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Geographic Coverage<br>
                                                                    <asp:DropDownList ID="dlGeographicCoverage" runat="server" Width="125" Enabled="false"
                                                                        Font-Size="small">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #cc0066">
                                                                    Note: Please enter 9 digit numbers only, without any dashes or spaces.
                                                                </td>
                                                                <td rowspan="3">
                                                                    <asp:TextBox ID="tbStateCoverage" runat="server" Width="125" Height="200" ReadOnly="True"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Credit Card Accepted<br>
                                                                    <asp:CheckBox ID="cbCreditCards" runat="server" Enabled="false" Checked='<%#Eval("Credit_Card_Accepted") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Hazardous<br>
                                                                    <asp:CheckBox ID="cbHazardous" runat="server" Enabled="false" Checked='<%#Eval("Hazard") %>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td valign="top">
                                                        <table style="border-style: outset" width="350">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="3" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Warranty Information
                                                                </td>
                                                            </tr>
                                                            <tr align="left">
                                                                <td align="left" style="font-family: Arial; font-size: x-small; color: #000099; width: 100">
                                                                    Warranty Duration:
                                                                </td>
                                                                <td align="left">
                                                                    <asp:TextBox ID="tbWarrantyDuration" runat="server" Width="100" ReadOnly="True" Font-Size="small"
                                                                        Text='<%# Eval("Warranty_Duration") %>'></asp:TextBox>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; font-style: italic">
                                                                    (example 1 year, 60 days, ect)
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="left" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Warranty Description:
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbWarrantyDesc" runat="server" Rows="15" Width="350" Height="100"
                                                                        ReadOnly="True" Text='<%#Eval("Warranty_Notes") %>' Wrap="true" TextMode="MultiLine" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <br>
                                                        <table style="border-style: outset" width="350">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="2" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Returned Goods Policy Information
                                                                </td>
                                                            </tr>
                                                            <tr align="left">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099; width: 150">
                                                                    Returned Goods Policy Type:
                                                                </td>
                                                                <td align="left">
                                                                    <asp:DropDownList ID="dlReturnGoodsPolicyType" runat="server" Width="125" Enabled="false"
                                                                        Font-Size="small" />
                                                                </td>
                                                            </tr>
                                                            <tr align="left">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Returned Goods Policy Notes:
                                                                </td>
                                                                <td>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2">
                                                                    <asp:TextBox ID="tbReturnPolicyNotes" runat="server" Rows="15" Width="350" Height="100"
                                                                        ReadOnly="True" Font-Size="small" Text='<%#Eval("Returned_Goods_Policy_Notes") %>'
                                                                        Wrap="true" TextMode="MultiLine" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="contractView" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                <tr valign="top">
                                                    <td style="width: 350">
                                                        <table>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Solicitation Number<br>
                                                                    <asp:TextBox ID="tbSolicitationNumber" runat="server" Width="125" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Solicitation_Number") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Tracking Customer Number<br>
                                                                    <asp:TextBox ID="tbTrackCustomer" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                        Text='<%#Eval("Tracking_Customer") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    IFF Absorbed/Embedded<br>
                                                                     <asp:DropDownList ID="dlIFFEmbedded" runat="server" Width="125" Font-Size="Small" SelectedValue='<%#Eval("IFF_Type_ID") %>' DataSourceID="IFFDataSource" DataTextField="IFF_Type_Description" DataValueField="IFF_Type_ID" Enabled="false" AppendDataBoundItems="true"><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                                    
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    End of Year Discount<br>
                                                                    <asp:TextBox ID="tbYearEndDiscount" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                        Text='<%#Eval("Annual_Rebate") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Estimated Value<br>
                                                                    <asp:TextBox ID="tbEstimatedValue" runat="server" Width="125" ReadOnly="True" Font-Size="small"
                                                                        Text='<%#Eval("Estimated_Contract_Value","{0:c}") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Minimum Order<br>
                                                                    <asp:TextBox ID="tbMinOrder" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                        Text='<%#Eval("Mininum_Order") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    FPR Date<br>
                                                                    <asp:TextBox ID="tbFPRDate" runat="server" Width="125" ReadOnly="True" Font-Size="small"
                                                                        Text='<%# Eval("BF_Offer") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Ratio<br>
                                                                    <asp:TextBox ID="tbRatio" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                        Text='<%#Eval("Ratio") %>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <br>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="2" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Delivery Information
                                                                </td>
                                                            </tr>
                                                            <tr valign="top">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Standard
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbStandard" runat="server" Width="300" Height="100" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Delivery_Terms") %>' TextMode="MultiLine" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr valign="top">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Expedited
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbExpedited" runat="server" Width="300" Height="100" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Expedited_Delivery_Terms") %>' TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td style="width: 350">
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Discount Information
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Basic Discount:<br>
                                                                    <asp:TextBox ID="tbBasicDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Discount_Basic") %>' Wrap="true" TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Quantity Discount:<br>
                                                                    <asp:TextBox ID="tbQuanDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Discount_Quantity") %>' Wrap="true" TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Prompt Pay Discount:<br>
                                                                    <asp:TextBox ID="tbPromptPay" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Discount_Prompt_Pay") %>' Wrap="true" TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Credit Card Discount:<br>
                                                                    <asp:TextBox ID="tbCreidtCardDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Discount_Credit_Card") %>' TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Addiontal Incentive Discount Information:<br>
                                                                    <asp:TextBox ID="tbAddtionalDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                        Font-Size="small" Text='<%#Eval("Incentive_Description") %>' TextMode="MultiLine"
                                                                        Rows="5" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="POCView" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                <tr valign="top">
                                                    <td>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Vendor Contract Administrator
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbVCAdminName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Primary_Name") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbVCAdminPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Primary_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbVCAdminExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Primary_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbVCAdminFax" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Primary_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbVCAdminEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Primary_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Alternate Vendor Contact
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbAVCName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Alternate_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbAVCPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Alternate_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbAVCExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Alternate_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbAVCFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Alternate_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbAVCEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Alternate_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Technical Contact
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbTCName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Tech_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbTCPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Tech_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbTCExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Tech_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbTCFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Tech_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbTCEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Tech_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    24 Hour Emergency Contact
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tb24HRName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Emergency_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb24HRPhone2" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Emergency_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb24HRExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Emergency_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tb24HRFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Emergency_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tb24HREmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Emergency_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="3">
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099;
                                                                    background-color: Silver" colspan="6">
                                                                    Ordering Contact
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Address1:
                                                                </td>
                                                                <td colspan="5">
                                                                    <asp:TextBox ID="tbOrderAddress1" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_Address_1")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Address2:
                                                                </td>
                                                                <td colspan="5">
                                                                    <asp:TextBox ID="tbOrderAddress2" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_Address_2")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    City:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderCity" runat="server" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_City")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    State:
                                                                </td>
                                                                <td>
                                                                    <asp:DropDownList ID="dlOrderState" runat="server" Enabled="false" />
                                                                </td>
                                                                <td style="font-family: Arial" colspan="2">
                                                                    <div style="font-size: x-small">
                                                                        Zip:</div>
                                                                    <asp:TextBox ID="tbOrderZip" runat="server" Font-Size="Smaller" Text='<%#Eval("Ord_Zip")%>'
                                                                        ReadOnly="true" Width="75" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderPhone" runat="server" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_Telephone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderExt" runat="server" ReadOnly="True" Width="50" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_Ext")%>' />
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                 <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrdFax" runat="server" Font-Size="Smaller" Text='<%#Eval("Ord_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td>
                                                                </td>
                                                           
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="5">
                                                                    <asp:TextBox ID="tbOrderEmail" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("Ord_EMail")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table style="border-style: outset">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Sales Contact
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Name:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbSalesName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Sales_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbSalesPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Sales_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbSalesExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Sales_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbSalesFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Sales_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbSalesEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                        Text='<%#Eval("POC_Sales_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                     <td>
                                                    </td>
                                               </tr>                                               
                                            </table>
                                        </asp:View>
                                        <asp:View ID="PricelistView" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top" align="left">
                                                    <td align="right">
                                                        <asp:Label ID="ibPricelistVerified" Text="Pricelist Verified:" runat="server" Width="100"
                                                            Font-Names="Arial" Font-Size="12px"></asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:CheckBox ID="cbPricelistVerify" runat="server" TextAlign="Left" Checked='<%#Eval("Pricelist_Verified") %>'
                                                            Enabled="false" />
                                                    </td>
                                                    <td align="right">
                                                        <asp:Label ID="lbPriceListVerifiedBy" runat="server" Font-Names="arial" Font-Size="12px">Verified By:</asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistVerifiedBy" runat="server" Text='<%#Eval("Verified_By") %>' />
                                                    </td>
                                                    <td valign="top" style="font-family: Arial; font-size: smaller">
                                                        Notes:
                                                    </td>
                                                    <td rowspan="4" colspan="2">
                                                        <asp:TextBox ID="tbPricelistNotes" runat="server" Width="290" TextMode="MultiLine"
                                                            Rows="20" Text='<%#Eval("Pricelist_Notes") %>' />
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Current Mod#:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistCurrMod" runat="server" Width="75" Text='<%#Eval("Current_Mod_Number") %>' />
                                                    </td>
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Mod Effective Date:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistModDate" runat="server" Width="75" Text='<%#Eval("Verification_Date","{0:d}") %>' />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr> <!-- $$$ -->
                                                    <tr align="left" valign="top">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbMedSurgItemCountLabel"  runat="server" Text="Active / Future Prices:"  /><br />
                                                        <asp:Label ID="lbMedSurgItemCount"  runat="server"  />
                                                    </td>
                                                    <td colspan="2" style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbViewMedSurgPricelistLabel"  Text="View Company Pricelist:" runat="server" /><br />
                                                        <asp:Button ID="btnViewPricelist" runat="server" Text="View Pricelist" Font-Names="Arial"
                                                            ForeColor="#009900" Font-Size="12px" />
                                                    </td>
                                                     <td colspan="2" style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbExportPriceList" runat="server" Text="Export/Upload Med/Surg Pricelist:" /> <br />
                                                        <asp:ImageButton ID="btnExportPricelistToExcel" runat="server" ImageUrl="Images/Excel-48.gif" />
                                                     
                                                    </td>
                                                   <td>
                                                    </td>
                                                </tr>
                                                <tr align="left" valign="top">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbDrugItemCountLabel"  Text="Drug Item Count:" runat="server" /><br />
                                                        <asp:Label ID="lbDrugItemCount"  runat="server" />
                                                    </td>
                                                   <td colspan="2" style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbViewDrugItemPricelistLabel"  Text="View Drug Item Pricelist:" runat="server" /><br />
                                                        <asp:Button ID="btnViewDrugItemPricelist" runat="server" Text="View Pricelist" Font-Names="Arial"
                                                            ForeColor="#8b8dbb" Font-Size="12px" />
                                                    </td>
                                                     <td colspan="2" style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbExportDrugItemPriceList" runat="server" Text="Export/Upload Drug Item Pricelist:" /> <br />
                                                        <asp:ImageButton ID="btnExportDrugItemPricelistToExcel" runat="server" ImageUrl="Images/Excel-48-blue.gif" />
                                                
                                                    </td>                           
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr align="left">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbCoveredFCPCount" runat="server" /><br />
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr align="left">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbPPVTotalCount" runat="server" /><br />
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>                                                
                                            </table>
                                        <asp:HiddenField ID="RefreshPricelistScreenOnSubmit" runat="server" Value="false" />

                                        </asp:View>
                                        <asp:View ID="SalesView" runat="server">
                                            <br />
                                            <asp:Button ID="btnAddSales" runat="server" Visible="false" OnClick="openSalesEntry" />
                                            <asp:Panel ID="pnSales" runat="server" ScrollBars="Vertical" Height="400">
                                                <asp:GridView ID="gvSales" runat="server" Width="700px" AutoGenerateColumns="false"
                                                    Font-Names="Arial" Font-Size="small" DataSourceID="SalesDataSource" CellPadding="4"
                                                    AllowSorting="true" OnRowCommand="grid_Command"  DataKeyNames="Quarter_ID" >
                                                    <Columns>
                                                   
                                                        <asp:BoundField DataField="Title" ItemStyle-Wrap="false" />
                                                        <asp:BoundField DataField="VA_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                            DataFormatString="{0:c}" HeaderText="VA Sales" SortExpression="VA_Sales_Sum" />
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbVAQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbVAQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbVAYear" runat="server" Text="Year" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbVAYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAYearNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="OGA_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                            DataFormatString="{0:c}" HeaderText="OGA Sales" />
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbOGAQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbOGAQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_OGA_Sales_Sum"),Eval("OGA_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbOGAYear" runat="server" Text="Year" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbOGAYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_OGA_Sales_Sum"),Eval("OGA_Sales_Sum"),"lbVAYearNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="SLG_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                            DataFormatString="{0:c}" HeaderText="S/C/L Govt. Sales" />
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbSLGQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbSLGQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_SLG_Sales_Sum"),Eval("SLG_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbSLGYear" runat="server" Text="Year" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbSLGYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_SLG_Sales_Sum"),Eval("SLG_Sales_Sum"),"lbVAYearNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="Total_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                            DataFormatString="{0:c}" HeaderText="Totals" />
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbTLQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbTLQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_Total_Sales"),Eval("Total_Sum"),"lbVAQtrNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbTLYear" runat="server" Text="Year" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbTLYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_Total_Sales"),Eval("Total_Sum"),"lbVAYearNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:HyperLinkField Text="Details" DataNavigateUrlFormatString="sales_detail.aspx?CntrctNum={1}&QtrID={0}" DataNavigateUrlFields="Quarter_ID,CntrctNum" Target="_blank"/>
                                                        <asp:ButtonField  ButtonType="Button" Text="Edit" CommandName="Update_Data" Visible="false"/>
                                                    </Columns>
                                                </asp:GridView>
                                            <asp:HiddenField ID="RefreshSalesDataGridOnSubmit" runat="server" Value="false" />

                                            </asp:Panel>
                                            <asp:Button ID="SalesIFFCheckCompareButton" runat="server" Text="View IFF/Check Comparison" Height="26px" Width="181px" OnClick="SalesIFFCheckCompareButton_OnClick" />
                                            <asp:Label ID="lbSalesHistory" runat="server" Text="Sales history in datasheet view"
                                                Font-Names="Arial" Font-Size="X-Small" ForeColor="#993375" />
                                            <asp:Button ID="DetailedSalesHistoryButton" runat="server" Text="Full Sales History" OnClick="DetailedSalesHistoryButton_OnClick" />
                                            <asp:Button ID="QuarterlySalesHistoryButton" runat="server" Text="Sales by Qtr"  OnClick="QuarterlySalesHistoryButton_OnClick" />
                                            <asp:Button ID="AnnualSalesHistoryButton" runat="server" Text="Sales by Year" OnClick="AnnualSalesHistoryButton_OnClick"  />
                                            <asp:Button ID="exportUploadSalesButton" runat="server" Height="26px" Text="Export/Upload Sales" Width="190px" />
                                        </asp:View>
                                        <asp:View ID="ChecksView" runat="server">
                                            <br />
                                            <asp:Button ID="btnViewIFFCheck" runat="server" Text="View IFF/Check Comparison" OnClick="SalesIFFCheckCompareButton_OnClick" />
                                             <asp:Panel ID="pnChecks" runat="server" ScrollBars="Both" Height="400">
                                                <asp:GridView ID="gvChecks" runat="server" DataSourceID="ChecksDataSource" Font-Names="Arial"
                                                    AutoGenerateColumns="false" CellPadding="4" HeaderStyle-Wrap="true" AllowSorting="true"
                                                    ShowFooter="false" ShowHeader="true" DataKeyNames="ID" OnRowCommand="UpdateChecksGrid" OnDataBound="gvChecks_OnDataBound" OnRowUpdating="gvChecks_OnRowUpdating" >
                                                    <Columns>
                                                        <asp:TemplateField HeaderText="Quarter" SortExpression="Quarter_ID">
                                                            <ItemTemplate>
                                                                <asp:DropDownList ID="dlQuarter" runat="server" DataSourceID="QuarterDataSource"
                                                                    DataTextField="Title" DataValueField="Quarter_ID" SelectedValue='<%#Eval("Quarter_ID")%>' Enabled="false">
                                                                    <asp:ListItem Text="" Value="" />
                                                                </asp:DropDownList>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                 <asp:DropDownList ID="dlQuarterEdit" runat="server" DataSourceID="QuarterDataSource"
                                                                    DataTextField="Title" DataValueField="Quarter_ID" SelectedValue='<%#Bind("Quarter_ID")%>'  >
                                                                    <asp:ListItem Text="" Value="" />
                                                                </asp:DropDownList>
                                   
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:DropDownList ID="dlQuarterNew" runat="server" DataSourceID="QuarterDataSource"
                                                                    DataTextField="Title" DataValueField="Quarter_ID">
                                                                    <asp:ListItem Text="" Value="" />
                                                                </asp:DropDownList>
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Check Amount" SortExpression="CheckAmt">
                                                            <ItemTemplate>
                                                                <%#Eval("CheckAmt","{0:c}") %>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="tbCheckAmountEdit" runat="server" Text='<%#Bind("CheckAmt", "{0:c}") %>' />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:TextBox ID="tbCheckAmountNew" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Check #" SortExpression="CheckNum">
                                                            <ItemTemplate>
                                                                <%#Eval("CheckNum")%>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="tbChecknumEdit" runat="server" Text='<%#Bind("CheckNum") %>' />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:TextBox ID="tbChecknumNew" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Deposit #" SortExpression="depositNum">
                                                            <ItemTemplate>
                                                                <%#Eval("DepositNum")%>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="tbCheckDepositEdit" runat="server" Text='<%#Bind("DepositNum")%>' />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:TextBox ID="tbCheckDepositNew" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Date Received" SortExpression="DateRcvd">
                                                            <ItemTemplate>
                                                                <%#Eval("DateRcvd", "{0:d}")%>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="tbdateRecvdEdit" runat="server" Text='<%#Bind("DateRcvd","{0:d}")%>' />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:TextBox ID="tbdateRecvdNew" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Comments" ItemStyle-Wrap="true">
                                                            <ItemTemplate>
                                                                <%#Eval("Comments")%>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="tbCommentsEdit" runat="server" Text='<%#Bind("Comments")%>' />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:TextBox ID="tbCommentsNew" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField Visible="false">
                                                            <ItemTemplate>
                                                                <asp:Button ID="btnItemEdit" runat="server" Text="Edit" CommandName="Edit" />
                                                                <asp:Button ID="btnItemDelete" runat="server" Text="Remove" CommandName="Delete" />
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:Button ID="btnUpdate" runat="server" Text="Update" CommandName="Update" />
                                                                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CommandName="Cancel" />
                                                            </EditItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:Button ID="btnAddNew" runat="server" Text="Save" CommandName="InsertNew" />
                                                                <asp:Button ID="btnCancelNew" runat="server" Text="Cancel" CommandName="Cancel" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <EmptyDataTemplate>
                                                        <asp:Panel ID="checksEmptyDataTemplatePanel" runat="server" EnableViewState="true" >
                                                            Quarter:<asp:DropDownList ID="dlQuarterInsertNew" runat="server" DataSourceID="QuarterDataSource"
                                                                DataTextField="Title" DataValueField="Quarter_ID">
                                                                <asp:ListItem Text="" Value="" />
                                                            </asp:DropDownList>
                                                            Check Amount:
                                                            <asp:TextBox ID="tbCheckAmountInsertNew" runat="server" />
                                                            Check #:<asp:TextBox ID="tbChecknumInsertNew" runat="server" />
                                                            <br />
                                                            Deposit #:<asp:TextBox ID="tbCheckDepositInsertNew" runat="server" />
                                                            Date Received:<asp:TextBox ID="tbdateRecvdInsertNew" runat="server" />
                                                            Comments<asp:TextBox ID="tbCommentsInsertNew" runat="server" />
                                                            <asp:Button ID="btnAddNewEmpty" runat="server" Text="Add" CommandName="InsertEmpty" />
                                                        </asp:Panel>
                                                        <asp:Panel ID="checksEmptyDataPanelNoPermissionsPanel" runat="server" EnableViewState="true"  >
                                                            <asp:Label ID="checksEmptyDataPanelNoPermissionsLabel" runat="server" EnableViewState="true"  Text="There have been no checks entered for the selected contract."/>
                                                        </asp:Panel>
                                                   </EmptyDataTemplate> 
                                                </asp:GridView>
                                                <asp:Button ID="btnAddCheck" runat="server" Visible="false" Text="Add Check" OnClick="btnAddCheck_OnClick" />
                                                <asp:Button ID="btnCancelAddCheck" runat="server" Text="Cancel" OnClick="btnCancelAddCheck_OnClick" EnableViewState="true" Visible="false" />
                                            </asp:Panel>
                                        </asp:View>
                                       
                                        <asp:View ID="SBAView" runat="server">
                                            <table>
                                                <tr valign="top">
                                                    <td >
                                                        <table style="font-family: Arial; border-style: outset; width: 260">
                                                            <tr style="background-color: Silver; font-size: small">
                                                                <td colspan="2">
                                                                    SBA Plan Name
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2">
                                                                    <asp:DropDownList ID="dlSBAPlanName" runat="server" Enabled="True" 
                                                                         OnDataBinding="dlSBAPlanName_OnDataBinding"
                                                                         OnSelectedIndexChanged="dlSBAPlanName_OnSelectedIndexChanged" 
                                                                        AutoPostBack="True" EnableViewState="true" OnInit="dlSBAPlanName_OnInit" >
                                                                        
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Label ID="lbSBANote" runat="server" BorderStyle="Solid" Font-Size="X-Small"
                                                                        Text="Note: Selecting a plan and clicking save sba associates this contract with the selected plan." />
                                                                </td>
                                                                <td>
                                                                    <asp:Button ID="btnAddSBA" runat="server" ForeColor="Green"  OnInit="setFormatAddNewSBAPlanButton" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    
                                                    <td rowspan="2" colspan="2">
                                                        <table style="border-style: outset; width: 150">
                                                            <tr>
                                                                <td style="background-color: Silver" align="center">
                                                                    <asp:Label ID="lbSBAProj" runat="server" Text="SBA Plan Projections" Font-Size="Smaller"
                                                                        ForeColor="#000099" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:GridView ID="gvSBAProjections" runat="server" AutoGenerateColumns="false" Width="150"
                                                                        BorderStyle="Outset" OnRowCommand="SBAProjection_Command" DataKeyNames="ProjectionID,SBAPlanID"
                                                                        ShowFooter="true">
                                                                        <Columns>
                                                                            <asp:TemplateField>
                                                                                <HeaderTemplate>
                                                                                    Start Date</HeaderTemplate>
                                                                                <ItemTemplate>
                                                                                    <asp:Label ID="lbStart" runat="server" Text='<%#Eval("StartDate","{0:d}") %>' />
                                                                                </ItemTemplate>
                                                                                <FooterTemplate>
                                                                                    <asp:Button ID="btnAddSBAProjNew" runat="server" ForeColor="Green" OnInit="setFormatSBAProj" />
                                                                                </FooterTemplate>
                                                                            </asp:TemplateField>
                                                                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" DataFormatString="{0:d}" />
                                                                            <asp:BoundField DataField="EndDate" HeaderText="End Date" DataFormatString="{0:d}" />
                                                                            <asp:HyperLinkField Text="Details" Target="_blank" DataNavigateUrlFormatString="sba_projections.aspx?SBAPlanID={1}&ProjID={0}&state=1"
                                                                                DataNavigateUrlFields="ProjectionID,SBAPlanID" />
                                                                        </Columns>
                                                                        <EmptyDataTemplate>
                                                                            <asp:Button ID="btnAddSBAProjEmpty" runat="server" ForeColor="Green" OnInit="setFormatSBAProj" />
                                                                        </EmptyDataTemplate>
                                                                    </asp:GridView>
                                                                    <asp:HiddenField ID="hfSBAProjectAdd" runat="server" Value="false" />
                                                                    <asp:HiddenField ID="hfRefreshSBAPlanList" runat="server" Value="false" />
                                                                    <asp:HiddenField ID="hfNewSBAPlanId" runat="server" Value="false" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr valign="top">  
                                                    
                                                        <td colspan="1">
                                                            <table style="width: 450px">
                                                            <tr valign="top">
                                                            <td rowspan="2" style="width:200px">
                                                            <table style="border-style: outset; width: 180px">
                                                            <tr>
                                                                <td style="background-color: Silver" align="center">
                                                                    <asp:Label ID="Label1" runat="server" Text="SBA 294/295 Info" Font-Size="Smaller"
                                                                        ForeColor="#000099" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:GridView ID="gv294Reports" runat="server" 
                                                                     AutoGenerateColumns="false" DataKeyNames="ACC_Record_ID" ShowFooter="true">
                                                                    <Columns>
                                                                        <asp:HyperLinkField Text="Details" DataNavigateUrlFormatString="sba_accomp.aspx?ACCID={0}&state=1"
                                                                            DataNavigateUrlFields="Acc_Record_ID" Target="_blank" FooterText="" HeaderText="" />
                                                                        <asp:BoundField DataField="Fiscal_Year" HeaderText="Year" />
                                                                         <asp:TemplateField>
                                                                        <HeaderTemplate>
                                                                        Report Type</HeaderTemplate>
                                                                        <ItemTemplate>
                                                                        <asp:Label ID="lblAccompPeriod" runat="server" Text='<%# Eval("Accomplishment_Period") %>' />
                                                                        </ItemTemplate>
                                                                          <FooterTemplate>
                                                                                    <asp:Button ID="btnAddSBA294jNew" runat="server" ForeColor="Green" OnInit="setFormatSBA294" />
                                                                                </FooterTemplate>
                                                                             </asp:TemplateField>
                                                                    </Columns>
                                                                    <EmptyDataTemplate>
                                                                            <asp:Button ID="btnAddSBA294Empty" runat="server" ForeColor="Green" OnInit="setFormatSBA294" />
                                                                        </EmptyDataTemplate>
                                                                    </asp:GridView>
                                                                    <asp:HiddenField ID="hfSBA294" runat="server" Value="false" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        </td>
                                                        <td>
                                                           
                                                           <asp:Panel ID="ContractResponsibleForSBAPanel" runat="server" Visible="true" >
                                                                    <table style="border-style: outset; width: 180px">
                                                                        <tr>
                                                                            <td colspan="2" style="font-family: Arial; font-size: smaller; color: #000099">
                                                                                Contract Responsible For Plan:
                                                                            </td>
                                                                        </tr>
                                                                        <tr style="font-family: Arial; font-size: x-small">
                                                                            <td>
                                                                                Contract:
                                                                            </td>
                                                                            <td>
                                                                                <asp:Label ID="lbSBACntrctResp" runat="server" />
                                                                            </td>
                                                                        </tr>
                                                                        <tr style="font-family: Arial; font-size: x-small">
                                                                            <td>
                                                                                CO:
                                                                            </td>
                                                                            <td>
                                                                                <asp:Label ID="lbSBACOResp" runat="server"  />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                            </asp:Panel>
                                                            </td>
                                                            </tr>
                                                            <tr valign="top" align="left">
                                                            
                                                            <td style="width: 180px">
                                                            <table style="border-style: outset; width: 180px">
                                                                <tr style="font-family: Arial; font-size: x-small">
                                                                    <td style="text-align:center">
                                                                        <asp:CheckBox ID="SBAPlanExemptCheckBox"  Text="SBA Plan Exempt" runat="server"  Checked='<%#Bind("SBA_Plan_Exempt") %>'/>
                                                                    </td> 
                                                                </tr>
                                                                <tr style="font-family: Arial; font-size: x-small">
                                                                    <td>
                                                                    <span style="text-align:center; ">( Used to identify vendors who would typically require an SBA plan but are exempt due to various circumstances. )</span>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                             </td>
                                                             
                                                                                                                         
                                                            <td style="width: 90px">
                                                            <table style="border-style: outset; width: 90px">
                                                                <tr style="font-family: Arial; font-size: x-small">
                                                                    <td style="text-align:center">
                                                                        <asp:Button ID="SBAOverallSaveButton" OnClick="SBAOverallSaveButton_OnClick"   Text="Save SBA" runat="server" />
                                                                    </td> 
                                                                </tr>
                                            
                                                            </table>
                                                             </td>
                                                             </tr>
                                                         </table>
                                                    </td>
                                                <td></td>                                                    
                                                   <td></td>   
                                                </tr>
                                                
                                                <tr valign="top">
                                                    <td>
                                                        <asp:FormView ID="fvSBAplanType" runat="server" Width="200">
                                                            <ItemTemplate>
                                                                <table style="font-family: Arial; width: 200; border-style: outset">
                                                                    <tr>
                                                                        <td style="background-color: Silver; color: #000099; font-size: smaller" colspan="2">
                                                                            Plan Type:
                                                                        </td>
                                                                        <td colspan="4">
                                                                            <asp:DropDownList ID="dlPlanType" runat="server"  AutoPostBack="true"  DataSourceID="PlanTypeDataSource"
                                                                                DataTextField="PlanTypeDescription" DataValueField="PlanTypeID" SelectedValue='<%#Eval("PlanTypeID") %>' />
                                                                            <asp:SqlDataSource ID="PlanTypeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                                                                                SelectCommand="SELECT [PlanTypeID], [PlanTypeDescription] FROM [tbl_sba_PlanType]">
                                                                            </asp:SqlDataSource>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6" style="background-color: Silver; color: #000099; font-size: smaller">
                                                                            Company SBA Plan Administrator
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Name:
                                                                        </td>
                                                                        <td colspan="5">
                                                                            <asp:TextBox ID="tbPlanAdminName" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_Name") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Email:
                                                                        </td>
                                                                        <td colspan="5">
                                                                            <asp:TextBox ID="tbPlanAdminEmail" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_email") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Address:
                                                                        </td>
                                                                        <td colspan="5">
                                                                            <asp:TextBox ID="tbPlanAdminAddress" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_Address1") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            City
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminCity" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_City") %>' />
                                                                        </td>
                                                                        <td>
                                                                            St:
                                                                        </td>
                                                                        <td>
                                                                            <asp:DropDownList ID="dlPlanAdminstate" runat="server"  AutoPostBack="true"  DataSourceID="StateDataSource"
                                                                                DataTextField="abbr" DataValueField="Abbr" SelectedValue='<%#Eval("Plan_Admin_State") %>'
                                                                                AppendDataBoundItems="true">
                                                                                <asp:ListItem Value="" />
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                        <td>
                                                                            Zip
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminZip" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_Zip") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Phone:
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminPhone" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_Phone") %>' />
                                                                        </td>
                                                                        <td colspan="2">
                                                                            Fax:
                                                                        </td>
                                                                        <td colspan="2">
                                                                            <asp:TextBox ID="tbPlanAdminFax" runat="server"  AutoPostBack="true"  Text='<%#Eval("Plan_Admin_Fax") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6">
                                                                            <asp:Button ID="btnEditSBAAdmin" runat="server"  AutoPostBack="true"  Text="Update" OnClick="UpdateSBAAdmin" />
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </ItemTemplate>
                                                        </asp:FormView>
                                                    </td>
                                                    <td colspan="2">
                                                        <asp:Panel ID="pnSBAComplayList" runat="server" ScrollBars="Both" Height="250" Width="325">
                                                            <asp:DataList ID="dlSBACompanyList" runat="server" BorderStyle="None" Width="300"
                                                                Font-Names="Arial">
                                                                <ItemTemplate>
                                                                    <table style="border-style: none; vertical-align: top">
                                                                        <tr>
                                                                            <td>
                                                                                <asp:Label ID="lbSBACOCntrctNum" runat="server" Text='<%#Eval("CntrctNum") %>' />
                                                                            </td>
                                                                            <td>
                                                                                <asp:Label ID="lbSBACOSchNum" runat="server" Text='<%#Eval("Schedule_Name") %>' />
                                                                            </td>
                                                                            <td>
                                                                                <asp:Label ID="lbSBACOStatus" Font-Bold="true" runat="server" Text='<%#ContractStatus(Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>' />
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="border-bottom-style: solid; border-bottom-color: Black; border-bottom-width: medium">
                                                                                <asp:Label ID="lbSBACOContractor" runat="server" Text='<%#Eval("Contractor_Name") %>' />
                                                                            </td>
                                                                            <td style="border-bottom-style: solid; border-bottom-color: Black; border-bottom-width: medium">
                                                                                <asp:Label ID="lbSBACOEst" runat="server" Text='<%#Eval("Estimated_Contract_Value","{0:c}") %>' />
                                                                            </td>
                                                                            <td style="border-bottom-style: solid; border-bottom-color: Black; border-bottom-width: medium">
                                                                                <asp:Label ID="lbSBACOName" runat="server" Text='<%#Eval("FullName") %>' />
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </ItemTemplate>
                                                            </asp:DataList>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        
                                        <asp:View ID="BOCView" runat="server">
                                        </asp:View>
                                        <asp:View ID="BPAInfoView" runat="server">
                                            <table style="font-family: Arial">
                                                <tr style="font-size: smaller; color: #000099">
                                                    <td>
                                                        Solicitation Number
                                                    </td>
                                                    <td style="width: 25">
                                                        &nbsp;
                                                    </td>
                                                    <td>
                                                        BPA Minimum Order
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td>
                                                        <asp:TextBox ID="tbBPAInfoSolicitationNum" runat="server" Text='<%#Eval("Solicitation_Number") %>'
                                                            ReadOnly="true" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbBPAInfoMin" runat="server" Text='<%#Eval("Mininum_Order") %>'
                                                            ReadOnly="true" Width="400" />
                                                    </td>
                                                </tr>
                                                <tr style="font-size: smaller; color: #000099">
                                                    <td>
                                                        Estimated Value
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                        BPA Standard Delivery Terms
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td>
                                                        <asp:TextBox ID="tbBPAInfoEst" runat="server" Text='<%#Eval("Estimated_Contract_Value","{0:c}") %>'
                                                            ReadOnly="true" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td rowspan="3">
                                                        <asp:TextBox ID="tbBPAInfoDel" runat="server" Text='<%#Eval("Delivery_Terms") %>'
                                                            ReadOnly="true" Width="400" TextMode="MultiLine" Rows="5" />
                                                    </td>
                                                </tr>
                                                <tr style="font-size: smaller; color: #000099">
                                                    <td>
                                                        Geographic Coverage
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td>
                                                        <asp:TextBox ID="tbBPAInfoGeo" runat="server" Text='<%#Eval("Geographic_Coverage_ID") %>'
                                                            ReadOnly="true" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="BPAPriceView" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top" align="left">
                                                    <td align="right">
                                                        <asp:Label ID="lbBPAPriceVerified" Text="Pricelist Verified:" runat="server" Width="100"
                                                            Font-Names="Arial" Font-Size="12px"></asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:CheckBox ID="cbBPAPriceVerifed" runat="server" TextAlign="Left" Checked='<%#Eval("Pricelist_Verified") %>'
                                                            Enabled="false" />
                                                    </td>
                                                    <td align="right">
                                                        <asp:Label ID="lbBPAPriceVerifiedBy" runat="server" Font-Names="arial" Font-Size="12px">Verified By:</asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbBPAPriceVerifiedBy" runat="server" Text='<%#Eval("Verified_By") %>' />
                                                    </td>
                                                    <td valign="top" style="font-family: Arial; font-size: smaller">
                                                        Notes:
                                                    </td>
                                                    <td rowspan="5" colspan="2">
                                                        <asp:TextBox ID="tbBPAPriceNotes" runat="server" Width="300" Rows="20" Text='<%#Eval("Pricelist_Notes") %>'
                                                            TextMode="MultiLine" />
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Current Mod#:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbBPAPriceCurMod" runat="server" Width="75" Text='<%#Eval("Current_Mod_Number") %>' />
                                                    </td>
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Verification Date:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbBPAPriceVerifyDate" runat="server" Width="75" Text='<%#Eval("Verification_Date","{0:d}") %>' />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr align="left" valign="top">
                                                    <td colspan="3" style="font-family: Arial; font-size: small" rowspan="3">
                                                        <table>
                                                            <tr>
                                                                <td>
                                                                    View Company Pricelist:
                                                                </td>
                                                                <td>
                                                                    <asp:Button ID="btnBPAViewPricelist" runat="server" Text="View Pricelist" Font-Names="Arial"
                                                                        ForeColor="#009900" Font-Size="12px" />
                                                                </td>
                                                            </tr>
                                                            <tr>
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
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr align="left" valign="top">
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr align="left">
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="FSSDetailsView" runat="server">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSGeneral" runat="server" Text="General" OnClick="btnBPAView_click"
                                                            BackColor="AntiqueWhite" ForeColor="Red" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSVendor" runat="server" Text="Vendor" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSContract" runat="server" Text="Contract" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSPrice" runat="server" Text="Price List" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" Visible="false" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSPOC" runat="server" Text="Point of Contact" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSSales" runat="server" Text="Sales" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" />
                                                    </td>
                                                    <td>
                                                        <asp:Button ID="btnBPAFSSChecks" runat="server" Text="Checks" OnClick="btnBPAView_click"
                                                            BackColor="White" ForeColor="Black" />
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:FormView ID="fvBPAFSSGeneral" runat="server">
                                                <ItemTemplate>
                                                    <asp:MultiView ID="mvBPAFSSDetail" runat="server">
                                                        <asp:View ID="BPAFSSGeneralView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSDetail" runat="server" ScrollBars="Vertical">
                                                                <table style="background-color: #ECE9D8; width: 700">
                                                                    <tr valign="top">
                                                                        <td>
                                                                            <table width="400" style="border-style: outset">
                                                                                <tr style="background-color: silver">
                                                                                    <td colspan="4" align="center">
                                                                                        <div style="font-family: Arial; color: #000099; font-size: small">
                                                                                            Vendor Mailing Address/Web Address</div>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address 1:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSgeneralAddr1" runat="server" ReadOnly="True" Text='<%# Eval("Primary_Address_1") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address 2:
                                                                                    </td>
                                                                                    <td colspan="3" style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ID="tbBPAFSSgeneralAddr2" runat="server" ReadOnly="True" Text='<%# Eval("Primary_Address_2") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        City:
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ID="tbBPAFSSgeneralCity" runat="server" ReadOnly="True" Text='<%# Eval("Primary_City") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralState" ReadOnly="true" runat="server" Text='<%#Eval("Primary_State") %>'
                                                                                            Columns="2" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Zip:
                                                                                        <asp:TextBox ID="tbBPSFSSGeneralZip" runat="server" Columns="9" ReadOnly="true" Text='<%# Eval("Primary_Zip") %>'
                                                                                            Font-Size="Small" Width="75" />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td align="left">
                                                                            <table width="250">
                                                                                <tr>
                                                                                    <td colspan="3" align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                        Short Contract Description
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td colspan="3" style="background-color: White" align="center">
                                                                                        <div style="font-family: Arial; font-size: small">
                                                                                            <asp:TextBox ID="tbBPAFSSGeneralDrug" runat="server" Width="250" ReadOnly="True"
                                                                                                Text='<%# Eval("Drug_Covered") %>' /></div>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td colspan="3" align="center">
                                                                                        <table style="border-right: #ece9d8 5px solid; border-top: #ece9d8 5px solid; border-left: #ece9d8 5px solid;
                                                                                            border-bottom: #ece9d8 5px solid; width: 200px; background-color: #ECE9D8">
                                                                                            <tr>
                                                                                                <td align="center">
                                                                                                    <table style="border-style: outset; background-color: silver">
                                                                                                        <tr>
                                                                                                            <td align="center">
                                                                                                                <div style="font-family: Arial; font-size: small; color: #000099">
                                                                                                                    Prime Vendor<br>
                                                                                                                    Participation</div>
                                                                                                                <asp:CheckBox ID="cbBPAFSSGeneralPV" runat="server" Enabled="false" Font-Size="1"
                                                                                                                    ForeColor="#000099" Width="40" Checked='<%# Eval("PV_Participation") %>' />
                                                                                                            </td>
                                                                                                        </tr>
                                                                                                    </table>
                                                                                                </td>
                                                                                                <td align="center">
                                                                                                    <table style="border-style: outset; background-color: Silver">
                                                                                                        <tr>
                                                                                                            <td align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                                                VA/DOD<br>
                                                                                                                Contract<asp:CheckBox ID="cbBPAFSSGeneralVADOD" runat="server" Enabled="false" Font-Names="Arial"
                                                                                                                    Font-Size="1" ForeColor="#000099" Width="40" Checked='<%#Eval("VA_DOD") %>' />
                                                                                                            </td>
                                                                                                        </tr>
                                                                                                    </table>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                      <asp:Panel ID="pnBPAFSSSINS" runat="server" Width="125" Height="175" ScrollBars="Vertical">
                                                                                <asp:GridView ID="gvBPAFSSSINS" runat="server" AutoGenerateColumns="False" Width="100px"
                                                                                DataKeyNames="CntrctNum,SINs" >
                                                                                    <Columns>
                                                                                        <asp:BoundField DataField="SINs" HeaderText="SINs" HeaderStyle-BackColor="Silver"
                                                                                            HeaderStyle-ForeColor="#000099" />
                                                                                        <asp:CheckBoxField DataField="Recoverable" HeaderText="RC" HeaderStyle-BackColor="Silver"
                                                                                            HeaderStyle-ForeColor="#000099" />
                                                                                    </Columns>
                                                                                </asp:GridView>
                                                                            </asp:Panel>
                                                                        </td>
                                                                    </tr>
                                                                    <tr valign="top">
                                                                        <td valign="top">
                                                                            <table style="border-style: outset; background-color: #ECE9D8; width: 400">
                                                                                <tr>
                                                                                    <td colspan="4" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                                        font-size: x-small" align="center">
                                                                                        Contract Dates
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                        Awarded:
                                                                                    </td>
                                                                                    <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                        Effective:
                                                                                    </td>
                                                                                    <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                        Expiration:
                                                                                    </td>
                                                                                    <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                        End Date:
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td>
                                                                                        <asp:Label ID="lbBPAFSSGeneralAwardDate" runat="server" Text='<%# Eval("Dates_CntrctAward","{0:d}") %>'
                                                                                            Width="90" />
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="lbBPAFSSGeneralEffDate" runat="server" Text='<%#Eval("Dates_Effective","{0:d}") %>'
                                                                                            Width="90"></asp:Label>
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="lbBPAFSSGeneralExpDate" runat="server" Text='<%#Eval("Dates_CntrctExp","{0:d}") %>'
                                                                                            Width="90"></asp:Label>
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:Label ID="lbBPAFSSGeneralEndDate" runat="server" Text='<%#Eval("Dates_Completion","{0:d}") %>'
                                                                                            Width="90" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Total Option Yrs:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:DropDownList ID="DropDownList4" runat="server" Width="75">
                                                                                            <asp:ListItem Text="0" Value="0" />
                                                                                            <asp:ListItem Text="1" Value="1" />
                                                                                            <asp:ListItem Text="2" Value="2" />
                                                                                            <asp:ListItem Text="3" Value="3" />
                                                                                            <asp:ListItem Text="4" Value="4" />
                                                                                            <asp:ListItem Text="5" Value="5" />
                                                                                            <asp:ListItem Text="6" Value="6" />
                                                                                            <asp:ListItem Text="7" Value="7" />
                                                                                            <asp:ListItem Text="8" Value="8" />
                                                                                            <asp:ListItem Text="9" Value="9" />
                                                                                            <asp:ListItem Text="10" Value="10" />
                                                                                        </asp:DropDownList>
                                                                                    </td>
                                                                                    <td colspan="2" rowspan="2">
                                                                                        <table style="border-style: outset" width="175">
                                                                                            <tr>
                                                                                                <td colspan="2" align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                                    Terminated By:
                                                                                                </td>
                                                                                            </tr>
                                                                                            <tr>
                                                                                                <td style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                                    Default:
                                                                                                    <asp:CheckBox ID="cbBPAFSSGeneralTermDefa" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                                        Font-Size="1" Checked='<%#Eval("Terminated_Default") %>'  Visible="true"  Enabled="false"  />
                                                                                                </td>
                                                                                                <td style="font-family: Arial; font-size: x-small">
                                                                                                    Convenience:
                                                                                                    <asp:CheckBox ID="cbBPAFSSGeneralTermConv" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                                        Font-Size="1" Checked='<%#Eval("Terminated_Convenience") %>'  Visible="true"  Enabled="false"  />
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Current Option Year:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralCurOpYear" runat="server" Width="75" ReadOnly="True"
                                                                                            Text='<%#GetCurrentOptionYear(Eval("Dates_TotOptYrs"),Eval("Dates_Effective"),Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                            <table style="border-style: outset; background-color: #ECE9D8">
                                                                                <tr>
                                                                                    <td colspan="4" style="background-color: silver; font-family: Arial; color: #000099;
                                                                                        font-size: smaller">
                                                                                        Vendor Contract Administration
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralPriName" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Name") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralPriPhone" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Phone","{0:(###)###-####}") %>'></asp:TextBox>
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tlbBPAFSSGeneralPriExt" runat="server" Width="40" ReadOnly="True"
                                                                                            Text='<%# Eval("POC_Primary_Ext") %>'></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralPriFax" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Fax") %>'></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSGeneralPriEmail" runat="server" ReadOnly="True" Text='<%# Eval("POC_Primary_Email") %>'></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                        </td>
                                                                    </tr>
                                                                    <tr valign="top">
                                                                        <td colspan="3" style="font-family: Arial; font-size: smaller">
                                                                            Contract Notes:<br>
                                                                            <asp:TextBox ID="tbBPAFSSGeneralNotes" runat="server" Rows="10" Width="700" ReadOnly="True"
                                                                                Text='<%# Eval("POC_Notes") %>' Wrap="true" TextMode="MultiLine" />
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </asp:Panel>
                                                        </asp:View>
                                                        <asp:View ID="BPAFSSVendorView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSVendor" runat="server" ScrollBars="Vertical" Height="500">
                                                                <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                                    <tr>
                                                                        <td valign="top">
                                                                            <table style="border-style: outset" width="350">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="5" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Vendor Socio-Economic Information
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Size:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSVendorSize" runat="server" Width="75" ReadOnly="true" Text='<%#Eval("Socio_Business_Size_ID") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Small Disadvantaged Business<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSSDB" runat="server" Enabled="false" Checked='<%#Eval("Socio_SDB") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        8A<br>
                                                                                        <asp:CheckBox ID="cbBPAFSS8A" runat="server" Enabled="false" Checked='<%#Eval("Socio_8a") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Woman<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSWoman" runat="server" Enabled="false" Checked='<%#Eval("Socio_Woman") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Hub Zone<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSHub" runat="server" Enabled="false" Checked='<%#Eval("Socio_HubZone") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td>
                                                                                    </td>
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Veteran Status<br>
                                                                                        <asp:TextBox ID="tbBPAFSSVetStatus" ReadOnly="true" runat="server" Text='<%#Eval("Socio_VetStatus_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                            <br>
                                                                            <table width="350">
                                                                                <tr align="center">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        DUNS<br>
                                                                                        <asp:TextBox ID="tbBPAFSSDun" runat="server" Width="125" ReadOnly="True" Text='<%# Eval("DUNS") %>'
                                                                                            Font-Size="small" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Vendor Type<br>
                                                                                        <asp:TextBox ID="tbBPAFSSVendorTpe" ReadOnly="true" runat="server" Text='<%#Eval("Dist_Manf_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Tax Identification Number (TIN)<br>
                                                                                        <asp:TextBox ID="tbBPAFSSTIN" runat="server" Width="125" ReadOnly="True" Font-Size="small"
                                                                                            Text='<%# Eval("TIN") %>'></asp:TextBox>
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Geographic Coverage<br>
                                                                                        <asp:TextBox ID="tbBPAFSSGeo" ReadOnly="true" runat="server" Text='<%#Eval("Geographic_Coverage_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #cc0066">
                                                                                        Note: Please enter 9 digit numbers only, without any dashes or spaces.
                                                                                    </td>
                                                                                    <td rowspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSNotes" runat="server" Width="125" Height="200" ReadOnly="True" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Credit Card Accepted<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSCreditCard" runat="server" Enabled="false" Checked='<%#Eval("Credit_Card_Accepted") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Hazardous<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSHaz" runat="server" Enabled="false" Checked='<%#Eval("Hazard") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td valign="top">
                                                                            <table style="border-style: outset" width="350">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="3" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Warranty Information
                                                                                    </td>
                                                                                </tr>
                                                                                <tr align="left">
                                                                                    <td align="left" style="font-family: Arial; font-size: x-small; color: #000099; width: 100">
                                                                                        Warranty Duration:
                                                                                    </td>
                                                                                    <td align="left">
                                                                                        <asp:TextBox ID="tbBPAFSSWarrantDuration" runat="server" Width="100" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%# Eval("Warranty_Duration") %>'></asp:TextBox>
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; font-style: italic">
                                                                                        (example 1 year, 60 days, ect)
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td align="left" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Warranty Description:
                                                                                    </td>
                                                                                    <td>
                                                                                    </td>
                                                                                    <td>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSWarrantNotes" runat="server" Rows="15" Width="350" Height="100"
                                                                                            ReadOnly="True" Text='<%#Eval("Warranty_Notes") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                            <br>
                                                                            <table style="border-style: outset" width="350">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="2" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Returned Goods Policy Information
                                                                                    </td>
                                                                                </tr>
                                                                                <tr align="left">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099; width: 150">
                                                                                        Returned Goods Policy Type:
                                                                                    </td>
                                                                                    <td align="left">
                                                                                        <asp:TextBox ID="tbReturnPolicy" ReadOnly="true" runat="server" Text='<%#Eval("Returned_Goods_Policy_Type")  %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr align="left">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Returned Goods Policy Notes:
                                                                                    </td>
                                                                                    <td>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td colspan="2">
                                                                                        <asp:TextBox ID="tbBPAFSSReturnNotes" runat="server" Rows="15" Width="350" Height="100"
                                                                                            ReadOnly="True" Font-Size="small" Text='<%#Eval("Returned_Goods_Policy_Notes") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </asp:Panel>
                                                        </asp:View>
                                                        <asp:View ID="BPAFSSContractView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSContract" runat="server" ScrollBars="Vertical" Height="500">
                                                                <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                                    <tr valign="top">
                                                                        <td style="width: 350">
                                                                            <table>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Solicitation Number<br>
                                                                                        <asp:TextBox ID="tbBPAFSSSolicitationNumber" runat="server" Width="125" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Solicitation_Number") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Tracking Customer Number<br>
                                                                                        <asp:TextBox ID="tbBPAFSSTrackCustomer" runat="server" Width="200" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Tracking_Customer") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        IFF Absorbed/Embedded<br>
                                                                                             <asp:DropDownList ID="dlFSSIFFEmbedded" runat="server" Width="125" Font-Size="Small" SelectedValue='<%#Eval("IFF_Type_ID") %>' DataSourceID="IFFDataSource" DataTextField="IFF_Type_Description" DataValueField="IFF_Type_ID" Enabled="false" AppendDataBoundItems="true">
                                                                                             <asp:ListItem Text="" Value="" />
                                                                                             </asp:DropDownList>
                                                               
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        End of Year Discount<br>
                                                                                        <asp:TextBox ID="tbBPAFSSYearEndDiscount" runat="server" Width="200" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Annual_Rebate") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Estimated Value<br>
                                                                                        <asp:TextBox ID="tbBPAFSSEstimatedValue" runat="server" Width="125" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Estimated_Contract_Value") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Minimum Order<br>
                                                                                        <asp:TextBox ID="tbBPAFSSMinOrder" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                                            Text='<%#Eval("Mininum_Order") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        FPR Date<br>
                                                                                        <asp:TextBox ID="tbBPAFSSFPRDate" runat="server" Width="125" ReadOnly="True" Font-Size="small"
                                                                                            Text='<%# Eval("BF_Offer") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Ratio<br>
                                                                                        <asp:TextBox ID="tbBPAFSSRatio" runat="server" Width="200" ReadOnly="True" Font-Size="small"
                                                                                            Text='<%#Eval("Ratio") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                            <br>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="2" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Delivery Information
                                                                                    </td>
                                                                                </tr>
                                                                                <tr valign="top">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Standard
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSStandard" runat="server" Width="300" Height="100" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Delivery_Terms") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr valign="top">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Expedited
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSExpedited" runat="server" Width="300" Height="100" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Expedited_Delivery_Terms") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td style="width: 350">
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Discount Information
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Basic Discount:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSBasicDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Discount_Basic") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Quantity Discount:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSQuanDiscount" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Discount_Quantity") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Prompt Pay Discount:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSPromptPay" runat="server" Width="300" Height="60" ReadOnly="True"
                                                                                            Font-Size="small" Text='<%#Eval("Discount_Prompt_Pay") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Credit Card Discount:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSCreidtCardDiscount" runat="server" Width="300" Height="60"
                                                                                            ReadOnly="True" Font-Size="small" Text='<%#Eval("Discount_Credit_Card") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Addiontal Incentive Discount Information:<br>
                                                                                        <asp:TextBox ID="tbBPAFSSAddtionalDiscount" runat="server" Width="300" Height="60"
                                                                                            ReadOnly="True" Font-Size="small" Text='<%#Eval("Incentive_Description") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </asp:Panel>
                                                        </asp:View>
                                                        <asp:View ID="BPAFSSPOCView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSPOCView" runat="server" ScrollBars="Vertical" Height="500">
                                                                <table style="background-color: #ECE9D8; width: 700; height: 400">
                                                                    <tr valign="top">
                                                                        <td>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Vendor Contract Administrator
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSVCAdminName" runat="server" Width="225" ReadOnly="True"
                                                                                            Font-Size="Smaller" Text='<%#Eval("POC_Primary_Name") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSVCAdminPhone" runat="server" Width="175" ReadOnly="True"
                                                                                            Font-Size="Smaller" Text='<%#Eval("POC_Primary_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSVCAdminExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Primary_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSVCAdminFax" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Primary_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSVCAdminEmail" runat="server" Width="225" ReadOnly="True"
                                                                                            Font-Size="Smaller" Text='<%#Eval("POC_Primary_Email")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Alternate Vendor Contact
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSAVCName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSAVCPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSAVCExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSAVCFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSAVCEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Email")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                        </td>
                                                                    </tr>
                                                                    <tr valign="top">
                                                                        <td>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Technical Contact
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSTCName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSTCPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSTCExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSTCFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSTCEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Email")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        24 Hour Emergency Contact
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSS24HRName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSS24HRPhone2" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSS24HRExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSS24HRFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSS24HREmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Email")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="3">
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099;
                                                                                        background-color: Silver" colspan="6">
                                                                                        Ordering Contact
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address1:
                                                                                    </td>
                                                                                    <td colspan="5">
                                                                                        <asp:TextBox ID="tbOrderAddress1" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Address_1")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address2:
                                                                                    </td>
                                                                                    <td colspan="5">
                                                                                        <asp:TextBox ID="tbOrderAddress2" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Address_2")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        City:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSOrderCity" runat="server" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_City")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        State:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSOrderState" runat="server" ReadOnly="true" Text='<%#Eval("Ord_State") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial" colspan="2">
                                                                                        <div style="font-size: x-small">
                                                                                            Zip:</div>
                                                                                        <asp:TextBox ID="tbBPAFSSOrderZip" runat="server" Font-Size="Smaller" Text='<%#Eval("Ord_Zip")%>'
                                                                                            ReadOnly="true" Width="75" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSOrderPhone" runat="server" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Telephone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSOrderExt" runat="server" ReadOnly="True" Width="50" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Ext")%>' />
                                                                                    </td>
                                                                                    <td>
                                                                                    </td>
                                                                                    <td>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="5">
                                                                                        <asp:TextBox ID="tbBPAFSSOrderEmail" runat="server" Width="200" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_EMail")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                                                <td>
                                                                            <table style="border-style: outset">
                                                                                <tr style="background-color: Silver">
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Sales Contact
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Name:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSSalesName" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Sales_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSSalesPhone" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Sales_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbBPAFSSSalesExt" runat="server" Width="50" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Sales_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSSalesFax" runat="server" Width="175" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Sales_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbBPAFSSSalesEmail" runat="server" Width="225" ReadOnly="True" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Sales_Email")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </asp:Panel>
                                                        </asp:View>
                                                        <asp:View ID="BPAFSSPriceView" runat="server">
                                                        </asp:View>
                                                        <asp:View ID="BPASalesView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSSales" runat="server" ScrollBars="Vertical" Height="400">
                                                                <asp:GridView ID="gvBPAFSSSales" runat="server" Width="700px" AutoGenerateColumns="false"
                                                                    Font-Names="Arial" Font-Size="small" CellPadding="4" AllowSorting="true">
                                                                    <Columns>
                                                                        <asp:BoundField DataField="Title" ItemStyle-Wrap="false" />
                                                                        <asp:BoundField DataField="VA_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                                            HeaderText="VA Sales" SortExpression="VA_Sales_Sum" />
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSVAQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSVAQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSVAYear" runat="server" Text="Year" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSVAYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAYearNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        <asp:BoundField DataField="OGA_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                                            HeaderText="OGA Sales" SortExpression="OGA_Sales_Sum" />
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSOGAQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSOGAQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_OGA_Sales_Sum"),Eval("OGA_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSOGAYear" runat="server" Text="Year" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSOGAYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_OGA_Sales_Sum"),Eval("OGA_Sales_Sum"),"lbVAYearNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                         <asp:BoundField DataField="SLG_Sales_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                                            HeaderText="S/C/L Govt. Sales" SortExpression="SLG_Sales_Sum" />
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSSLGQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSSLGQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_SLG_Sales_Sum"),Eval("SLG_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSSLGYear" runat="server" Text="Year" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSSLGYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_SLG_Sales_Sum"),Eval("SLG_Sales_Sum"),"lbVAYearNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>             
                                                                        <asp:BoundField DataField="Total_Sum" HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua"
                                                                            HeaderText="Totals" SortExpression="Total_Sum" />
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSTLQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSTLQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_Total_Sales"),Eval("Total_Sum"),"lbVAQtrNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                                            <HeaderTemplate>
                                                                                <asp:Label ID="lbBPAFSSTLYear" runat="server" Text="Year" Font-Bold="true" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lbBPAFSSTLYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_Total_Sales"),Eval("Total_Sum"),"lbVAYearNums" )%>'
                                                                                    OnDataBinding="setRowFormat" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                        
                                                                       
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </asp:Panel>
                                                            <asp:Button ID="btnBPAFSSIFF" runat="server" Text="View IFF/Check Comparison" Visible="false" />
                                                            <asp:Label ID="lbBPAFSSSalesHistory" runat="server" Text="Sales history in datasheet view"
                                                                Font-Names="Arial" Font-Size="X-Small" ForeColor="#993375" Visible="false" />
                                                            <asp:Button ID="btnBPAFSSFullSales" runat="server" Text="Full Sales History" Visible="false"/>
                                                            <asp:Button ID="btnBPAFSSSalesQtr" runat="server" Text="Sales by Qtr" Visible="false" />
                                                            <asp:Button ID="btnBPAFSSSalesYear" runat="server" Text="Sales by Year" Visible="false" />
                                                        </asp:View>
                                                        <asp:View ID="BPAChecksView" runat="server">
                                                            <asp:Panel ID="pnBPAFSSChecks" runat="server" ScrollBars="Vertical" Height="400">
                                                                <asp:GridView ID="gvBPAFSSChecks" runat="server" Font-Names="Arial" AutoGenerateColumns="false"
                                                                    CellPadding="4" HeaderStyle-Wrap="true" AllowSorting="true">
                                                                    <Columns>
                                                                        <asp:BoundField DataField="Title" HeaderText="Quarter" SortExpression="Quarter_ID" />
                                                                        <asp:BoundField DataField="CheckAmt" HeaderText="Check Amount" DataFormatString="{0:c}"
                                                                            SortExpression="CheckAmt" />
                                                                        <asp:BoundField DataField="CheckNum" HeaderText="Check #" SortExpression="CheckNum" />
                                                                        <asp:BoundField DataField="DepositNum" HeaderText="Deposit #" SortExpression="DepositNum" />
                                                                        <asp:BoundField DataField="DateRcvd" HeaderText="Date Received" DataFormatString="{0:d}"
                                                                            SortExpression="DateRcvd" />
                                                                        <asp:BoundField DataField="Comments" HeaderText="Comments" ItemStyle-Wrap="true" />
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </asp:Panel>
                                                        </asp:View>
                                                    </asp:MultiView>
                                                </ItemTemplate>
                                            </asp:FormView>
                                        </asp:View>
                                    </asp:MultiView>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td style="width: 249px">
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                            </tr>
                        </table>
                        <asp:HiddenField ID="hfADUser" runat="server" Visible="false" Value='<%#Eval("AD_User") %>' />
                        <asp:HiddenField ID="hfSMUser" runat="server" Visible="false" Value='<%#Eval("SM_User") %>' />
                        <asp:HiddenField ID="hfCOUser" runat="server" Visible="false" Value='<%#Eval("CO_User") %>' />
                        <asp:HiddenField ID="hfData1" runat="server" Visible="false" Value='<%#Eval("Data_Entry_Full_1_UserName") %>' />
                        <asp:HiddenField ID="hfData2" runat="server" Visible="false" Value='<%#Eval("Data_Entry_Full_2_UserName") %>' />
                        <asp:HiddenField ID="hfBPAFSSContract" runat="server" Visible="false" Value='<%#Eval("BPA_FSS_Counterpart") %>' />
                       
                    </ItemTemplate>
                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                    <EditItemTemplate>
                    </EditItemTemplate>
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <EditRowStyle BackColor="#999999" />
                </asp:FormView>
                <asp:SqlDataSource ID="ContractDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT * FROM [View_Contracts_Full] WHERE ([CntrctNum] = @CntrctNum)">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ChecksDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" OnDeleting="ChecksDataSource_OnDeleting"
                    SelectCommand="SELECT dbo.tbl_Cntrcts_Checks.ID, dbo.tbl_Cntrcts_Checks.CntrctNum, dbo.tbl_Cntrcts_Checks.Quarter_ID, dbo.tbl_Cntrcts_Checks.CheckAmt, dbo.tbl_Cntrcts_Checks.CheckNum, dbo.tbl_Cntrcts_Checks.DepositNum, dbo.tbl_Cntrcts_Checks.DateRcvd, dbo.tbl_Cntrcts_Checks.Comments, dbo.tlkup_year_qtr.Title FROM dbo.tbl_Cntrcts_Checks INNER JOIN dbo.tlkup_year_qtr ON dbo.tbl_Cntrcts_Checks.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID WHERE (dbo.tbl_Cntrcts_Checks.CntrctNum = @CntrctNum) ORDER BY dbo.tbl_Cntrcts_Checks.Quarter_ID DESC"
                    UpdateCommand="UPDATE dbo.tbl_Cntrcts_Checks SET Quarter_ID = @Quarter_ID, CheckAmt = CONVERT(money,@CheckAmt), CheckNum = @CheckNum, DepositNum = @DepositNum, DateRcvd = @DateRcvd, Comments = @Comments, LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate WHERE (ID = @ID)"
                    DeleteCommand="DELETE FROM dbo.tbl_Cntrcts_Checks Output 'tbl_Cntrcts_Checks', Deleted.ID, (@LastModifiedBy), (@LastModificationDate) into Audit_Deleted_Data_By_User WHERE (ID = @ID)">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                    <DeleteParameters>
                        <asp:Parameter Name="ID" />
                    </DeleteParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Quarter_ID" />
                        <asp:Parameter Name="CheckAmt" />
                        <asp:Parameter Name="CheckNum" />
                        <asp:Parameter Name="DepositNum" />
                        <asp:Parameter Name="DateRcvd" />
                        <asp:Parameter Name="Comments" />
                        <asp:Parameter Name="ID" />
                        <asp:Parameter Name="LastModifiedBy" />
                        <asp:Parameter Name="LastModificationDate" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SalesDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    
                    SelectCommand="SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.Year, dbo.tlkup_year_qtr.Qtr, dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, dbo.View_Contract_Preview.Contractor_Name, dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF AS VA_IFF_Amount, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF AS OGA_IFF_Amount, (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF) + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.tbl_IFF.SLG_IFF AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_Total_Sales, View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales, dbo.tlkup_year_qtr.start_quarter_id, dbo.tbl_IFF.VA_IFF, dbo.tbl_IFF.OGA_IFF, dbo.tbl_IFF.SLG_IFF, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.tbl_IFF.SLG_IFF AS SLG_IFF_Amount FROM dbo.[tlkup_Sched/Cat] INNER JOIN dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number INNER JOIN dbo.tbl_IFF ON dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.Schedule_Number LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A INNER JOIN dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON dbo.tbl_IFF.Start_Quarter_Id = dbo.tlkup_year_qtr.start_quarter_id AND dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_1 ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_2 ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID WHERE (dbo.View_Contract_Preview.CntrctNum = @CntrctNum) ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SINDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT * FROM [tbl_Cntrcts_SINs] WHERE ([CntrctNum] = @CntrctNum) order by SINs ">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAAccompDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>">
                </asp:SqlDataSource>
                 <asp:SqlDataSource ID="StatedataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Abbr], [State/Province], [Country] FROM tlkup_StateAbbr GROUP BY [Abbr], [State/Province], [Country] ORDER BY [State/Province]">
                </asp:SqlDataSource>
               <asp:SqlDataSource ID="SBACntrctrespDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAPlanTypeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAPlanDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [SBAPlanID], [PlanName] FROM view_sba_plans_sorted"></asp:SqlDataSource>

                  <asp:SqlDataSource ID="QuarterDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT Quarter_ID, Title FROM tlkup_year_qtr WHERE (End_Date < { fn NOW() }) ORDER BY Quarter_ID DESC">
                </asp:SqlDataSource>
                 <asp:SqlDataSource ID="IFFDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                SelectCommand="SELECT [IFF_Type_ID], [IFF_Type_Description] FROM tlkup_IFF_Type">
                </asp:SqlDataSource>
           
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td style="width: 256px">
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td align="center" colspan="3">
            </td>
        </tr>
    </table>
     <asp:HiddenField ID="hfRefreshSales" runat="server" OnValueChanged="refreshSalesDate" />
     <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />    
    </form>
</body>
</html>
