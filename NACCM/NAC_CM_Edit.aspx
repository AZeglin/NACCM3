<%@ Page Language="vb" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="NAC_CM_Edit.aspx.vb" Inherits="NACCM.NAC_CM_Edit" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>NAC CM Editable Contract</title>
    <link href="NACCMStyle.css" rel="stylesheet" type="text/css" />

</head>
<body style="background-color: #ece9d8">
    <form id="form1" runat="server">
         
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="NACCMEditScriptManager_OnAsyncPostBackError" >
        <Scripts>
            <asp:ScriptReference Path="~/NACCMEdit.js" />
        </Scripts>
    </asp:ScriptManager>

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
                <asp:Button ID="btnContractSearch" runat="server" Text="Contract Search" Width="109px"
                    Visible="false"></asp:Button><asp:Button ID="btnMainMenu" runat="server" Text="Main Menu"
                        Visible="False"></asp:Button><asp:Button ID="btnExit" runat="server" Text="Exit NAC CM"
                            Width="94px"></asp:Button>
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
                    OnPreRender="fvContractInfo_PreRender" CellPadding="4" ForeColor="#333333" DefaultMode="Edit" OnDataBound="fvContractInfo_OnDataBound" >
                    <ItemTemplate>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <table style="width: 800; background-color: #9baeb9">
                            <tr>
                                <td style="width: 300px">
                                    &nbsp;
                                </td>
                                <td style="width: 256px">
                                    &nbsp;
                                </td>
                                <td align="right" style="width: 250px">
                                    <asp:Button ID="btnUpdate" runat="server" Text="Save" ToolTip="Save and Update data"
                                        Height="50" Font-Size="Large" UseSubmitBehavior="false" OnClick="btnUpdate_OnClick" />
                                </td>
                            </tr>
                        </table>
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
                                                <asp:TextBox ID="tbContractName" runat="server" Text='<%# Bind("Contractor_Name") %>'
                                                    Font-Names="Arial" Width="300"></asp:TextBox>
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
                                                <asp:DropDownList ID="dlContractOfficers" runat="server" Width="250px" Font-Names="Arial"
                                                     SelectedValue='<%#Bind("CO_ID") %>' DataSourceID="CODataSource"
                                                    DataTextField="FullName" DataValueField="CO_ID"  AppendDataBoundItems="true"><asp:ListItem Text="" Value="" /></asp:DropDownList>  
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
                                                <asp:Label ID="lbCOPhone" runat="server" Width="136px" Text='<%#Eval("CO_Phone") %>'
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
                                                <asp:Label ID="lbScheduleName" runat="server" Text='<%#Eval("Schedule_Name") %>'
                                                    Font-Names="Arial"></asp:Label>
                                                <asp:Label ID="lbScheduleNumber" runat="server" Text='<%#Bind("Schedule_Number") %>'
                                                    Visible="false" />
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
                                                <asp:Label visible="false" ID="lbADName" runat="server" Text='<%#Eval("AD_Name") %>' Font-Names="Arial"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:Label visible="false" ID="lbSMName" runat="server" Text='<%#Eval("SM_Name") %>' Font-Names="Arial"></asp:Label>
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
                                    <asp:Button ID="btnRebate" runat="server" Text="Rebates" OnClick="btnView_click"
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
                                    <asp:MultiView ID="itemView" runat="server" OnPreRender="itemView_prerender">
                                        <asp:View ID="general" runat="server">
                                            <table style="background-color: #ECE9D8; width: 700">
                                                <tr valign="top">
                                                    <td>
                                                        <table style="width: 200; border-style: Outset; font-family: Arial">
                                                            <tr style="background-color: Silver">
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: Small; color: #000099">
                                                                    Vendor Mailing Address/Web Address
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-size: X-Small; width: 50px">
                                                                    Address 1:
                                                                </td>
                                                                <td colspan="3" style="font-size: Small">
                                                                    <asp:TextBox ID="tbPrimary_Address_1" runat="server" Text='<%# Bind("Primary_Address_1") %>'
                                                                        Font-Size="Small" Width="150" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-size: X-Small">
                                                                    Address 2:
                                                                </td>
                                                                <td colspan="3" style="font-size: Small">
                                                                    <asp:TextBox ID="tbAddress2" runat="server" Text='<%# Bind("Primary_Address_2") %>'
                                                                        Font-Size="Small" Width="150" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-size: X-Small">
                                                                    City:
                                                                </td>
                                                                <td style="font-size: Small">
                                                                    <asp:TextBox ID="tbCity" runat="server" Text='<%# Bind("Primary_City") %>' Font-Size="Small"
                                                                        Width="150" />
                                                                </td>
                                                                <td>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <div style="font-size: X-Small">
                                                                        State:</div>
                                                                    <asp:DropDownList ID="dlState" runat="server" Enabled="True"  EnableViewState="true" AutoPostBack="true" Font-Size="Small" OnDataBound="dlState_OnDataBound" OnSelectedIndexChanged="dlState_OnSelectedIndexChanged"  > </asp:DropDownList>
                                                                </td>
                                                                <td style="font-size: X-Small" colspan="3" align="left">
                                                                    Zip:
                                                                    <asp:TextBox ID="tbZip" runat="server" Columns="9" Text='<%# Bind("Primary_Zip") %>'
                                                                        Font-Size="Small" Width="75" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-size: Small; color: #000099">
                                                                    Company Web<br />
                                                                    Page:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbCompanyWebPage" runat="server" Text='<%# Bind("POC_VendorWeb") %>'
                                                                        Font-Size="Small" Width="200" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <asp:FormView ID="fvFSSContractInfo" runat="server" Visible="false" Font-Names="Arial">
                                                            <ItemTemplate>
                                                                <asp:Table ID="tblBPAContractInfo" runat="server" Width="400" BorderStyle="Outset">
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="3">FSS Counterpart Contract #</asp:TableHeaderCell><asp:TableHeaderCell
                                                                            ColumnSpan="1">FSS Contract Status:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="3">
                                                                            <asp:Label ID="lbFSSContractNum" runat="server" Text='<%#Bind("Contractor_Name") %>' /></asp:TableCell><asp:TableCell>
                                                                                <asp:Label ID="lbFSSContractStatus" runat="server" Text='<%#ContractStatus(Eval("Dates_CntrctExp"),Eval("Dates_Completion"))%>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="4">Schedule:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="4">
                                                                            <asp:Label ID="lbFSSSchedule" runat="server" Text='<%#Bind("Schedule_Name") %>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell ColumnSpan="4">CO:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="4">
                                                                            <asp:Label ID="lbFSSCO" runat="server" Text='<%#Bind("CO_Name") %>' /></asp:TableCell></asp:TableRow>
                                                                    <asp:TableHeaderRow ForeColor="#cc0066">
                                                                        <asp:TableHeaderCell>Awarded:</asp:TableHeaderCell><asp:TableHeaderCell>Effective:</asp:TableHeaderCell><asp:TableHeaderCell>Expiration:</asp:TableHeaderCell><asp:TableHeaderCell>End Date:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell>
                                                                            <asp:Label ID="lbFSSDateAward" runat="server" Text='<%#Bind("Dates_CntrctAward","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                <asp:Label ID="lbFSSDateEff" runat="server" Text='<%#Bind("Dates_Effective","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                    <asp:Label ID="lbFSSDateCntrctExp" runat="server" Text='<%#Bind("Dates_CntrctExp","{0:d}") %>' /></asp:TableCell><asp:TableCell>
                                                                                        <asp:Label ID="lbFSSDatecomp" runat="server" Text='<%#Bind("Dates_Completion","{0:d}") %>' /></asp:TableCell></asp:TableRow>
                                                                </asp:Table>
                                                            </ItemTemplate>
                                                        </asp:FormView>
                                                    </td>
                                                    <td align="left">
                                                        <table width="200">
                                                            <tr>
                                                                <td colspan="3" align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                    Short Contract Description
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="3" style="background-color: White" align="center">
                                                                    <div style="font-family: Arial; font-size: small">
                                                                        <asp:TextBox ID="tbShortdesc" runat="server" Width="200" Text='<%# Bind("Drug_Covered") %>' /></div>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="3" align="center">
                                                                    <asp:Panel ID="pnGVSINS" runat="server" Width="200px" Height="175" ScrollBars="Vertical">
                                                                        <asp:GridView ID="gvSINS" runat="server" AutoGenerateColumns="False" DataSourceID="SINdataSource"
                                                                            Width="100" DataKeyNames="CntrctNum,SINs" OnRowCommand="UpdateSINGrid" ShowFooter="true">
                                                                            <Columns>
                                                                                <asp:TemplateField HeaderText="SINs" HeaderStyle-BackColor="Silver" HeaderStyle-ForeColor="#000099">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="SelectedSINLabel" runat="server" Text='<%#Eval("SINs")%>' />
                                                                                    </ItemTemplate>
                                                                                    <FooterTemplate>
                                                                                        <asp:DropDownList ID="dlAddSin" runat="server" DataSourceID="addSINDataSource" DataTextField="SIN"
                                                                                            DataValueField="SIN" AppendDataBoundItems="true">
                                                                                            <asp:ListItem Text="" Value="" />
                                                                                        </asp:DropDownList>
                                                                                    </FooterTemplate>
                                                                                </asp:TemplateField>
                                                                                     <asp:TemplateField HeaderText="RC" HeaderStyle-BackColor="Silver" HeaderStyle-ForeColor="#000099">
                                                                                   <ItemTemplate>
                                                                                      
                                                                                       <asp:CheckBox Checked='<%#Bind("Recoverable") %>' OnCheckedChanged="RecoverableCheckBox_OnCheckedChanged"  ID="RecoverableCheckBox" runat="server"  AutoPostBack = "true" Enabled="true" />
                                                                                   </ItemTemplate>
                                                                                   <FooterTemplate>
                                                                                        <asp:CheckBox Checked='<%#Bind("Recoverable") %>' ID="cbInsertRC" runat="server" />
                                                                                   </FooterTemplate>
                                                                                </asp:TemplateField>                                                                             
                                                                                <asp:TemplateField HeaderStyle-BackColor="Silver" HeaderStyle-ForeColor="#000099">
                                                                                    <ItemTemplate>
                                                                                        <asp:Button ID="Delete" Text="Remove" runat="server" CommandName="Delete" />
                                                                                    </ItemTemplate>
                                                                                    <FooterTemplate>
                                                                                        <asp:Button ID="btnAddSin" runat="server" CommandName="InsertNew" Text="Add" />
                                                                                        <asp:Button ID="btnCancelSin" runat="server" CommandName="CancelNew" Text="Cancel" />
                                                                                    </FooterTemplate>
                                                                                </asp:TemplateField>
                                                                            </Columns>
                                                                            <EmptyDataTemplate>
                                                                                SIN:
                                                                                <asp:DropDownList ID="dlNoSin" runat="server" DataSourceID="addSINDataSource" DataTextField="SIN"
                                                                                    DataValueField="SIN" AppendDataBoundItems="true">
                                                                                    <asp:ListItem Text="" Value="" />
                                                                                </asp:DropDownList>
                                                                                RC:
                                                                                <asp:CheckBox Checked='<%#Bind("Recoverable") %>' ID="cbInsertRC" runat="server" />
                                                                                <asp:Button ID="nodataInsert" runat="server" Text="Add" CommandName="NoDataInsert" />
                                                                            </EmptyDataTemplate>
                                                                        </asp:GridView>
                                                                    </asp:Panel>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                        <table>
                                                         
                                                            <tr>
                                                            <td>
                                                            <table style="border-right: #ece9d8 5px solid; border-top: #ece9d8 5px solid; border-left: #ece9d8 5px solid;
                                                                border-bottom: #ece9d8 5px solid; width: 125px; background-color: #ECE9D8">
                                                                <tr>
                                                                    <td>
                                                                        <table id="PrimeVendorTable"  visible="false" runat="server" style="border-style: Outset; background-color: Silver; font-family: Arial">
                                                                            <tr>
                                                                                <td align="left" style="font-size: Small; color: #000099">
                                                                                    <asp:Label ID="lbPrimeVendor" visible="false" runat="server" Text="Prime Vendor Participation" AssociatedControlID="cbPrimeVendor" />
                                                                                    <asp:CheckBox ID="cbPrimeVendor" visible="false"  Enabled="false" runat="server" Font-Size="1" ForeColor="#000099"
                                                                                        Width="40" Checked='<%# Bind("PV_Participation") %>' AutoPostBack="true"  />
                                                                                </td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td align="left" style="font-size: Small; color: #000099">
                                                                                    <asp:Label ID="lbVADOD" visible="false" runat="server" Text="VA/DOD Contract" AssociatedControlID="cbDODVACOntract" />
                                                                                    <asp:CheckBox ID="cbDODVACOntract" visible="false"  Enabled="false" runat="server" Font-Names="Arial" Font-Size="1"
                                                                                        ForeColor="#000099" Width="40" Checked='<%#Bind("VA_DOD") %>' AutoPostBack="true"  />
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table id="InsurancePolicyTable" visible="false" runat="server" style="border-style: Outset; background-color: #ECE9D8; font-family: Arial">
                                                                            <tr>
                                                                                <td colspan="2" style="background-color: Silver; font-family: Arial; color: #000099; font-size:small" align="center">
                                                                                    <asp:Label ID="InsurancePolicyHeaderLabel"  Visible="false"  runat="server" Text="Insurance Policy" Font-Size="Small" />
                                                                                </td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td>
                                                                                    <asp:Label ID="InsurancePolicyEffectiveDateLabel" Visible="false"  Width="75" Text="Effective Date" runat="server"  Font-Size="X-Small"></asp:Label>
                                                                                </td>
                                                                                <td>
                                                                                    <asp:Label ID="InsurancePolicyExpirationDateLabel" Visible="false"  Width="75" Text="Expiration Date" runat="server"  Font-Size="X-Small" ></asp:Label>
                                                                                </td>
                                                                                
                                                                            </tr>
                                                                            <tr>
                                                                           <td>
                                                                                <asp:TextBox ID="InsurancePolicyEffectiveDateTextBox" Visible="false"  Enabled="false" runat="server" Text='<%#Bind("Insurance_Policy_Effective_Date", "{0:d}") %>' Width="75"  />
                                                                                </td>
                                                                                <td>
                                                                                    <asp:TextBox ID="InsurancePolicyExpirationDateTextBox" Visible="false"  Enabled="false" runat="server" Text='<%#Bind("Insurance_Policy_Expiration_Date", "{0:d}" )%>' Width="75"  />
                                                                                </td>
                                                   
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                    <td align="center">
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                          </td>
                                                        </tr>
                                                           <tr>
                                                                <td valign="top">
                                                                     <table id="TradeAgreementTable"  visible="false" runat="server"  style="table-layout:fixed; border-style: outset; background-color: #ECE9D8; width: 150px">
                                                                        <tr>
                                                                            <td colspan="2" style="background-color: Silver; font-family: Arial; color: #000099;
                                                                                font-size:small" align="center">
                                                                                <asp:Label ID="TradeAgreementHeaderLabel"  visible="false"  runat="server" Text="Trade Agreement Act Compliant" Font-Size="Small" />
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                              <td style="font-family: Arial; color: #000099; font-size: small; " align="left">
                                                                                    
                                                                                    <asp:CheckBox ID="TAAYesCheckBox"  Visible="false"  Enabled="false" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                        Checked='<%#GetTAACheckBoxValue( Eval("TradeAgreementActCompliance"), "Yes" ) %>' 
                                                                                         Text="Yes" OnCheckedChanged="TAAYesCheckBox_OnCheckedChanged" autopostback="true"/>
                                                                              </td>
                                                                        </tr>
                                                                        <tr>
                                                                             <td style="font-family: Arial; font-size: small;" align="left">
                                                                                    
                                                                                    <asp:CheckBox ID="TAAOtherCheckBox"  Visible="false"  Enabled="false" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                         Checked='<%#GetTAACheckBoxValue( Eval("TradeAgreementActCompliance"), "Other" ) %>' 
                                                                                       Text="Other" OnCheckedChanged="TAAOtherCheckBox_OnCheckedChanged" autopostback="true"/>
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
                                                                                <asp:Label ID="StimulusActHeaderLabel"  visible="true"  runat="server" Text="Stimulus Act" AssociatedControlID="StimulusActCheckBox" Font-Size="Small" />
                                                                            </td>
                                                                        </tr>                                                           
                                                                        <tr>
                                                                              <td style="font-family: Arial; color: #000099; font-size: small; " align="center">
                                                                                    
                                                                                    <asp:CheckBox ID="StimulusActCheckBox"  Visible="true"  Enabled="true" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                        Checked='<%# Bind("StimulusAct") %>' 
                                                                                        AutoPostBack="true"  Text="" />
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
                                                                    <asp:Label ID="lbAwardDate" runat="server" Text='<%# Bind("Dates_CntrctAward","{0:d}") %>'
                                                                        Width="90" OnDataBinding="AwardDate_OnDataBinding" />
                                                                </td>
                                                                <td>
                                                                    <asp:Label ID="lbEffectiveDate" runat="server" Text='<%#Bind("Dates_Effective","{0:d}") %>'
                                                                        Width="90" OnDataBinding="EffectiveDate_OnDataBinding" ></asp:Label>

                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="ExpirationDateTextBox" runat="server" Width="100" Enabled="True"                     
                                                                        Text='<%#Bind("Dates_CntrctExp","{0:d}") %>' OnDataBinding="ExpirationDate_OnDataBinding">
                                                                    </asp:TextBox>                                                                        
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="CompletionDateTextBox" runat="server" Width="100" ForeColor="Red" 
                                                                        Text='<%#Bind("Dates_Completion","{0:d}") %>' OnDataBinding="CompletionDate_OnDataBinding">                                                
                                                                    </asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:ImageButton ID="AwardDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select award date" OnClientClick="OnCAwardDateButtonClick()" />                                                                
                                                                </td>
                                                                <td>
                                                                    <asp:ImageButton ID="EffectiveDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select effective date" OnClientClick="OnCEffectiveDateButtonClick()" />
                                                                </td>
                                                                <td>
                                                                    <asp:ImageButton ID="ExpirationDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select expiration date" OnClientClick="OnCExpirationDateButtonClick()" />                                                                
                                                                </td>
                                                                <td>
                                                                    <asp:ImageButton ID="CompletionDateImageButton"  runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select completion date" OnClientClick="OnCCompletionDateButtonClick()" />
                                                                </td>
                                                            </tr>                                                            
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Total Option Yrs:
                                                                </td>
                                                                <td>
                                                                    <asp:DropDownList ID="dlOptionYears" runat="server" Width="75" SelectedValue='<%#Bind("Dates_TotOptYrs") %>'>
                                                                        <asp:ListItem Text="" Value="" />
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
                                                                                <asp:CheckBox ID="cbTermDefault" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                    Font-Size="1" Checked='<%#Bind("Terminated_Default") %>' />
                                                                            </td>
                                                                            <td style="font-family: Arial; font-size: x-small">
                                                                                Convenience:
                                                                                <asp:CheckBox ID="cbTermConvenience" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                    Font-Size="1" Checked='<%#Bind("Terminated_Convenience") %>' />
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
                                                                    <asp:TextBox ID="tbCurrentOptionYear" runat="server" Width="75" Text='<%#GetCurrentOptionYear(Eval("Dates_TotOptYrs"),Eval("Dates_Effective"),Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>'
                                                                        ReadOnly="true" />
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
                                                                    <asp:TextBox ID="tbNameVC" runat="server" Text='<%# Bind("POC_Primary_Name") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbPhoneVC" runat="server" Text='<%# Bind("POC_Primary_Phone","{0:(###)###-####}") %>'></asp:TextBox>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbExtVC" runat="server" Width="40" Text='<%# Bind("POC_Primary_Ext") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbFaxVC" runat="server" Text='<%# Bind("POC_Primary_Fax") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbEmailVC" runat="server" Text='<%# Bind("POC_Primary_Email") %>'></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                               <table>
                                                    <tr>
                                                        <td>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                           
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
                                                                        <asp:CheckBox ID="StandardizedCheckBox"  Visible="True"  Enabled="True" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                            Checked='<%# Bind("Standardized") %>' Text=""  />
                                                                    </td>
                                                                </tr>
                                                             </table>  
                                                        </td>
                                                    </tr>
                                                </table>

                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td colspan="4" style="font-family: Arial; font-size: smaller">
                                                        Contract Notes:<br>
                                                        <asp:TextBox ID="tbContractNotes" runat="server" Rows="10" Width="700" Text='<%# Bind("POC_Notes") %>'
                                                            Wrap="true" TextMode="MultiLine" />
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined" />
                                            <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False" />

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
                                                                    <asp:DropDownList ID="dlBusinessSize" runat="server" Width="75" DataSourceID="businessSizeDataSource"
                                                                        DataTextField="Business_Size" DataValueField="Business_Size_ID" SelectedValue='<%#Bind("Socio_Business_Size_ID") %>'>
                                                                    </asp:DropDownList>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Small Disadvantaged Business<br>
                                                                    <asp:CheckBox ID="cbSDB" runat="server" Enabled="True" Checked='<%#Bind("Socio_SDB") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    8A<br>
                                                                    <asp:CheckBox ID="cbSA" runat="server" Enabled="True" Checked='<%#Bind("Socio_8a") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Woman<br>
                                                                    <asp:CheckBox ID="cbWoman" runat="server" Enabled="True" Checked='<%#Bind("Socio_Woman") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Hub Zone<br>
                                                                    <asp:CheckBox ID="cbHub" runat="server" Enabled="True" Checked='<%#Bind("Socio_HubZone") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                </td>
                                                                <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Veteran Status<br>
                                                                    <asp:DropDownList ID="dlVeteranStatus" Enabled="True" runat="server" Width="125"
                                                                        DataSourceID="vetStatusDataSource" DataTextField="VetStatus_Description" DataValueField="VetStatus_ID"
                                                                        SelectedValue='<%#Bind("Socio_VetStatus_ID") %>'>
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <br>
                                                        <table width="350">
                                                            <tr align="center">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    DUNS<br>
                                                                    <asp:TextBox ID="tbDUNS" runat="server" Width="125" Text='<%# Bind("DUNS") %>' Font-Size="small" />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Vendor Type<br>
                                                                    <asp:DropDownList ID="dlVendorType" runat="server" Width="125" DataSourceID="VendorTypeDateSource"
                                                                        DataTextField="Dist_Manf_Description" DataValueField="Dist_Manf_ID" SelectedValue='<%#Bind("Dist_Manf_ID") %>'
                                                                        AppendDataBoundItems="true">
                                                                        <asp:ListItem Text="" Value="" />
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Tax Identification Number (TIN)<br>
                                                                    <asp:TextBox ID="tbTIN" runat="server" Width="125" Font-Size="small" Text='<%# Bind("TIN") %>'></asp:TextBox>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Geographic Coverage<br>
                                                                    <asp:DropDownList ID="dlGeographicCoverage" runat="server" Width="125" Font-Size="small"
                                                                        DataSourceID="GeoCoverageDataSource" DataTextField="Geographic_Description" DataValueField="Geographic_ID"
                                                                        SelectedValue='<%#Bind("Geographic_Coverage_ID") %>' AppendDataBoundItems="true">
                                                                        <asp:ListItem Text="" Value="" />
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #cc0066">
                                                                    Note: Please enter 9 digit numbers only, without any dashes or spaces.
                                                                </td>
                                                                <td rowspan="3">
                                                                    <asp:Panel ID="Panel1" runat="server" Width="150px" Height="200" ScrollBars="Vertical">
                                                                        <asp:GridView ID="gvStateCoverage" runat="server" DataKeyNames="id" DataSourceID="StateCoverageDataSource"
                                                                            Width="125" Height="200" ShowFooter="true" OnRowCommand="UpdateStateCoverageGrid"
                                                                            AutoGenerateColumns="false">
                                                                            <Columns>
                                                                                <asp:TemplateField Visible="false">
                                                                                    <ItemTemplate>
                                                                                        <%#Eval("ID")%>
                                                                                    </ItemTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="States">
                                                                                    <ItemTemplate>
                                                                                        <%#Eval("Abbr")%>
                                                                                    </ItemTemplate>
                                                                                    <FooterTemplate>
                                                                                        <asp:DropDownList ID="dlItemState" runat="server" DataSourceID="StateDataSource"
                                                                                            DataTextField="Abbr" DataValueField="Abbr" AppendDataBoundItems="true">
                                                                                            <asp:ListItem Text="" Value="" />
                                                                                        </asp:DropDownList>
                                                                                    </FooterTemplate>
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField>
                                                                                    <ItemTemplate>
                                                                                        <asp:Button ID="btnRemoveItem" runat="server" Text="Remove" CommandName="Delete" />
                                                                                    </ItemTemplate>
                                                                                    <FooterTemplate>
                                                                                        <asp:Button ID="btnAddItem" runat="server" Text="Add" CommandName="InsertNow" />
                                                                                        <asp:Button ID="btnCancelItem" runat="server" Text="Cancel" CommandName="Cancel" />
                                                                                    </FooterTemplate>
                                                                                </asp:TemplateField>
                                                                            </Columns>
                                                                            <EmptyDataTemplate>
                                                                                State:<asp:DropDownList ID="dlNewState" runat="server" AppendDataBoundItems="true"
                                                                                    DataSourceID="StateDataSource" DataTextField="Abbr" DataValueField="Abbr">
                                                                                    <asp:ListItem Text="" Value="" />
                                                                                </asp:DropDownList>
                                                                                <asp:Button ID="btnAddNew" runat="server" Text="Add" CommandName="InsertNew" />
                                                                            </EmptyDataTemplate>
                                                                        </asp:GridView>
                                                                    </asp:Panel>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Credit Card Accepted<br>
                                                                    <asp:CheckBox ID="cbCreditCards" runat="server" Enabled="True" Checked='<%#Bind("Credit_Card_Accepted") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Hazardous<br>
                                                                    <asp:CheckBox ID="cbHazardous" runat="server" Enabled="True" Checked='<%#Bind("Hazard") %>' />
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
                                                                    <asp:TextBox ID="tbWarrantyDuration" runat="server" Width="100" Font-Size="small"
                                                                        Text='<%# Bind("Warranty_Duration") %>'></asp:TextBox>
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
                                                                        Text='<%#Bind("Warranty_Notes") %>' Wrap="true" TextMode="MultiLine" />
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
                                                                    <asp:DropDownList ID="dlReturnGoodsPolicyType" runat="server" Width="125" Font-Size="small"
                                                                        DataSourceID="ReturnDataSource" DataTextField="Returned_Goods_Policy_Description"
                                                                        DataValueField="Returned_Goods_Policy_Type_ID" SelectedValue='<%#Bind("Returned_Goods_Policy_Type") %>'
                                                                        AppendDataBoundItems="true">
                                                                        <asp:ListItem Text="" Value="" />
                                                                    </asp:DropDownList>
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
                                                                        Font-Size="small" Text='<%#Bind("Returned_Goods_Policy_Notes") %>' Wrap="true"
                                                                        TextMode="MultiLine" />
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
                                                                    <asp:TextBox ID="tbSolicitationNumber" runat="server" Width="125" Font-Size="small"
                                                                        Text='<%#Bind("Solicitation_Number") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Tracking Customer Number<br>
                                                                    <asp:TextBox ID="tbTrackCustomer" runat="server" Width="200" Font-Size="small" Text='<%#Bind("Tracking_Customer") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    IFF Absorbed/Embedded<br>
                                                                    <asp:DropDownList ID="dlIFFEmbedded" runat="server" Width="125" Font-Size="Small"
                                                                        SelectedValue='<%#Bind("IFF_Type_ID") %>' DataSourceID="IFFDataSource" DataTextField="IFF_Type_Description"
                                                                        DataValueField="IFF_Type_ID" AppendDataBoundItems="true">
                                                                        <asp:ListItem Text="" Value="" />
                                                                    </asp:DropDownList>
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    End of Year Discount<br>
                                                                    <asp:TextBox ID="tbYearEndDiscount" runat="server" Width="200" Font-Size="small"
                                                                        Text='<%#Bind("Annual_Rebate") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Estimated Value<br>
                                                                    <asp:TextBox ID="tbEstimatedValue" runat="server" Width="125" Font-Size="small" Text='<%#Bind("Estimated_Contract_Value","{0:c}") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Minimum Order<br>
                                                                    <asp:TextBox ID="tbMinOrder" runat="server" Width="200" Font-Size="small" Text='<%#Bind("Mininum_Order") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    FPR Date<br>
                                                                    <asp:TextBox ID="tbFPRDate" runat="server" Width="125" Font-Size="small" Text='<%# Bind("BF_Offer") %>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Ratio<br>
                                                                    <asp:TextBox ID="tbRatio" runat="server" Width="200" Font-Size="small" Text='<%#Bind("Ratio") %>' />
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
                                                                    <asp:TextBox ID="tbStandard" runat="server" Width="300" Height="100" Font-Size="small"
                                                                        Text='<%#Bind("Delivery_Terms") %>' TextMode="MultiLine" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr valign="top">
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Expedited
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbExpedited" runat="server" Width="300" Height="100" Font-Size="small"
                                                                        Text='<%#Bind("Expedited_Delivery_Terms") %>' TextMode="MultiLine" Rows="5" />
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
                                                                    <asp:TextBox ID="tbBasicDiscount" runat="server" Width="300" Height="60" Font-Size="small"
                                                                        Text='<%#Bind("Discount_Basic") %>' Wrap="true" TextMode="MultiLine" onkeyDown="checkTextAreaMaxLength(this,event,'255');" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Quantity Discount:<br>
                                                                    <asp:TextBox ID="tbQuanDiscount" runat="server" Width="300" Height="60" Font-Size="small"
                                                                        Text='<%#Bind("Discount_Quantity") %>' Wrap="true" TextMode="MultiLine" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Prompt Pay Discount:<br>
                                                                    <asp:TextBox ID="tbPromptPay" runat="server" Width="300" Height="60" Font-Size="small"
                                                                        Text='<%#Bind("Discount_Prompt_Pay") %>' Wrap="true" TextMode="MultiLine" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Credit Card Discount:<br>
                                                                    <asp:TextBox ID="tbCreidtCardDiscount" runat="server" Width="300" Height="60" Font-Size="small"
                                                                        Text='<%#Bind("Discount_Credit_Card") %>' TextMode="MultiLine" Rows="5" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                    Addiontal Incentive Discount Information:<br>
                                                                    <asp:TextBox ID="tbAddtionalDiscount" runat="server" Width="300" Height="60" Font-Size="small"
                                                                        Text='<%#Bind("Incentive_Description") %>' TextMode="MultiLine" Rows="5" />
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
                                                                    <asp:TextBox ID="tbVCAdminName" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Primary_Name") %>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbVCAdminPhone" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Primary_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbVCAdminExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("POC_Primary_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbVCAdminFax" runat="server" Width="200" Font-Size="Smaller" Text='<%#Bind("POC_Primary_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbVCAdminEmail" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Primary_Email")%>' />
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
                                                                    <asp:TextBox ID="tbAVCName" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Alternate_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbAVCPhone" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Alternate_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbAVCExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("POC_Alternate_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbAVCFax" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Alternate_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbAVCEmail" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Alternate_Email")%>' />
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
                                                                    <asp:TextBox ID="tbTCName" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Tech_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbTCPhone" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Tech_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbTCExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("POC_Tech_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbTCFax" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Tech_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbTCEmail" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Tech_Email")%>' />
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
                                                                    <asp:TextBox ID="tb24HRName" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Emergency_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb24HRPhone2" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Emergency_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tb24HRExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("POC_Emergency_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tb24HRFax" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Emergency_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tb24HREmail" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Emergency_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td colspan="1">
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
                                                                    <asp:TextBox ID="tbOrderAddress1" runat="server" Width="200" Font-Size="Smaller"
                                                                        Text='<%#Bind("Ord_Address_1")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Address2:
                                                                </td>
                                                                <td colspan="5">
                                                                    <asp:TextBox ID="tbOrderAddress2" runat="server" Width="200" Font-Size="Smaller"
                                                                        Text='<%#Bind("Ord_Address_2")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    City:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderCity" runat="server" Font-Size="Smaller" Text='<%#Bind("Ord_City")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    State:
                                                                </td>
                                                                <td>
                                                                    <asp:DropDownList ID="dlOrderState" runat="server" Enabled="True" EnableViewState="true" AutoPostBack="true" OnDataBound="dlOrderState_OnDataBound" OnSelectedIndexChanged="dlOrderState_OnSelectedIndexChanged" ></asp:DropDownList>
                                                                </td>
                                                                <td style="font-family: Arial" colspan="2">
                                                                    <div style="font-size: x-small">
                                                                        Zip:</div>
                                                                    <asp:TextBox ID="tbOrderZip" runat="server" Font-Size="Smaller" Text='<%#Bind("Ord_Zip")%>'
                                                                        Width="75" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderPhone" runat="server" Font-Size="Smaller" Text='<%#Bind("Ord_Telephone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbOrderExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("Ord_Ext")%>' />
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
                                                                    <asp:TextBox ID="tbOrdFax" runat="server" Font-Size="Smaller" Text='<%#Bind("Ord_Fax","{0:(###)###-####}")%>' />
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
                                                                    <asp:TextBox ID="tbOrderEmail" runat="server" Width="200" Font-Size="Smaller" Text='<%#Bind("Ord_EMail")%>' />
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
                                                                    <asp:TextBox ID="tbSalesName" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Sales_Name")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Phone:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbSalesPhone" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Sales_Phone","{0:(###)###-####}")%>' />
                                                                </td>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Ext:
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="tbSalesExt" runat="server" Width="50" Font-Size="Smaller" Text='<%#Bind("POC_Sales_Ext")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Fax:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbSalesFax" runat="server" Width="175" Font-Size="Smaller" Text='<%#Bind("POC_Sales_Fax","{0:(###)###-####}")%>' />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="font-family: Arial; font-size: x-small">
                                                                    Email:
                                                                </td>
                                                                <td colspan="3">
                                                                    <asp:TextBox ID="tbSalesEmail" runat="server" Width="225" Font-Size="Smaller" Text='<%#Bind("POC_Sales_Email")%>' />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:View>
                                        <asp:View ID="RebateView" runat="server">
                                                
                                             <table style="background-color: #ECE9D8; width: 800; height: 800; font-family:Arial; font-size:small;">

                                                <col width="10px" />
                                                <col width="800px" />
                                                <col width="10px" />
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td align="center">
                                                        <asp:Label runat="server" ID="RebateTabLabel" Text="Rebates" Font-Bold="true" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td align="center">
                                                        <asp:CheckBox runat="server" ID="RebateRequiredCheckBox" Text="Rebate Information Is Required" Checked='<%#Bind("RebateRequired")%>' OnCheckedChanged="RebateRequiredCheckBox_OnCheckedChanged" AutoPostBack="true" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        
                                                    </td>
                                                    <td>
                                                        <asp:Button runat="server" ID="AddRebateButton" Text="Add Rebate" OnClick="AddNewRebateButton_OnClick" />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td>
                                                        <div id="RebateGridViewDiv"  runat="server" style="width:100%; height:100%; overflow: scroll" onscroll="javascript:setItemScrollForRestore( this );"  onkeypress="javascript:setItemScrollForRestore( this );"  >
                                                           <ep:GridView ID="RebateGridView" 
                                                                        runat="server" 
                                                                        DataKeyNames="RebateId"  
                                                                        AutoGenerateColumns="False" 
                                                                        Width="886px" 
                                                                        CssClass="RebateGrid" 
                                                                        Visible="True" 
                                                                        onrowcommand="RebateGridView_RowCommand" 
                                                                        OnSelectedIndexChanged="RebateGridView_OnSelectedIndexChanged" 
                                                                        OnRowDataBound="RebateGridView_RowDataBound"
                                                                        AllowSorting="True" 
                                                                        AutoGenerateEditButton="false"
                                                                        EditRowStyle-CssClass="RebateEditRowStyle" 
                                                                        onprerender="RebateGridView_PreRender" 
                                                                        OnInit="RebateGridView_Init"
                                                                        OnRowCreated="RebateGridView_OnRowCreated"
                                                                        onrowdeleting="RebateGridView_RowDeleting" 
                                                                        onrowediting="RebateGridView_RowEditing" 
                                                                        onrowupdating="RebateGridView_RowUpdating" 
                                                                        onrowcancelingedit="RebateGridView_RowCancelingEdit"
                                                                        AllowInserting="True"
                                                                        OnRowInserting="RebateGridView_RowInserting" 
                                                                      
                                                                        EmptyDataRowStyle-CssClass="RebateGrid" 
                                                                        EmptyDataText="There are no rebates for the selected contract."
                                                                        ContextMenuID="ItemContextMenu"
                                                                        Font-Names="Arial" 
                                                                        Font-Size="small" >
                                                                    <HeaderStyle CssClass="RebateGridHeaders" />
                                                                    <RowStyle  CssClass="RebateGridItems"  HorizontalAlign="Left" VerticalAlign="Top" />
                                                                    <AlternatingRowStyle CssClass="RebateGridAltItems" />
                                                                    <SelectedRowStyle  BorderStyle="Double" BorderColor="LightGreen"  />
                                                                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                     
                                                                    <Columns>   
                                                                            
                                                                            <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="ViewRebateTextButton" Text="View Rebate Text" OnDataBinding="ViewRebateTextButton_DataBinding" OnCommand="RebateGridView_ButtonCommand" CommandName="ViewRebateText" CommandArgument='<%# Container.DataItemIndex & "," &  Eval("RebateId") & "," &  Eval("RebatePercentOfSales") & "," &  Eval("RebateThreshold") %>' ButtonType="Button" ControlStyle-Width="112px" >            
                                                                                    </asp:Button >                                    
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Button runat="server"  ID="ViewRebateTextButton" Text="Remove View Rebate Text" OnDataBinding="ViewRebateTextButton_DataBinding" OnCommand="RebateGridView_ButtonCommand" CommandName="ViewRebateText" CommandArgument='<%# Container.DataItemIndex & "," &  Eval("RebateId") & "," &  Eval("RebatePercentOfSales") & "," &  Eval("RebateThreshold")  %>' ButtonType="Button" ControlStyle-Width="112px" >            
                                                                                    </asp:Button >                                    
                                                                            </EditItemTemplate>
                                                                            </asp:TemplateField>
                                                                        
                                                                                                          
                                                                             <asp:TemplateField >  
                                                                            <ItemTemplate>
                                                                                   <asp:Button  CausesValidation="false" Visible="true" runat="server" ID="EditButton" Text="Edit" OnCommand="RebateGridView_ButtonCommand"   CommandName="EditRebate"  CommandArgument='<%#  Container.DataItemIndex  & "," &  Eval("RebateId")  & "," &  Eval("RebatePercentOfSales") & "," &  Eval("RebateThreshold")  %>' ButtonType="Button"  ControlStyle-Width="50px"   >
                                                                                  </asp:Button>   

                                                                                   <asp:Button  CausesValidation="false" Visible="false" runat="server" ID="SaveButton" Text="Save" OnCommand="RebateGridView_ButtonCommand"   CommandName="SaveRebate" OnClick="Save_ButtonClick"  CommandArgument='<%#  Container.DataItemIndex  & "," &  Eval("RebateId") %>' ButtonType="Button"  ControlStyle-Width="50px"   >
                                                                                  </asp:Button>   

                                                                                   <asp:Button CausesValidation="false"  Visible="false" runat="server" ID="CancelButton" Text="Cancel" OnCommand="RebateGridView_ButtonCommand"   CommandName="Cancel"  CommandArgument='<%#  Container.DataItemIndex  & "," &  Eval("RebateId") & "," &  Eval("RebatePercentOfSales") & "," &  Eval("RebateThreshold") %>' ButtonType="Button"  ControlStyle-Width="50px"   >
                                                                                  </asp:Button>   
                                  
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>      
                                            
 
                                                                     <asp:TemplateField HeaderText="Start Year Quarter"  >
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="startYearQuarterLabel" Width="100px" runat="server" AutoPostBack="true" OnDataBinding="startYearQuarterLabel_OnDataBinding" >
                                                                            </asp:Label>
                                                                        </ItemTemplate>
                                                                        <EditItemTemplate>
                                                                            <asp:DropDownList ID="startYearQuarterDropDownList" DataValueField="Quarter_ID"  Width="120px"    DataTextField="YearQuarterDescription" runat="server" OnDataBound="startYearQuarterDropDownList_DataBound" OnSelectedIndexChanged="startYearQuarterDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                                            </asp:DropDownList>
                                                                        </EditItemTemplate>
                                                                     </asp:TemplateField>

                                                                     <asp:TemplateField HeaderText="End Year Quarter"  >
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="endYearQuarterLabel" Width="100px" runat="server" AutoPostBack="true" OnDataBinding="endYearQuarterLabel_OnDataBinding" >
                                                                            </asp:Label>
                                                                        </ItemTemplate>
                                                                        <EditItemTemplate>
                                                                            <asp:DropDownList ID="endYearQuarterDropDownList" DataValueField="Quarter_ID"  Width="120px"   DataTextField="YearQuarterDescription" runat="server" OnDataBound="endYearQuarterDropDownList_DataBound" OnSelectedIndexChanged="endYearQuarterDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                                            </asp:DropDownList>
                                                                        </EditItemTemplate>
                                                                     </asp:TemplateField>

                                                                     <asp:TemplateField HeaderText="Percent of Sales"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="percentOfSalesLabel" runat="server"  Width="54px"  Text='<%# DataBinder.Eval( Container.DataItem, "RebatePercentOfSales" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="percentOfSalesTextBox" runat="server"  Width="54px" Text='<%# DataBinder.Eval( Container.DataItem, "RebatePercentOfSales" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                            
                                            
                                                                         <asp:TemplateField HeaderText="Threshold"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="rebateThresholdLabel" runat="server"  Width="100px"  Text='<%# DataBinder.Eval( Container.DataItem, "RebateThreshold", "{0:0.00}" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="rebateThresholdTextBox" runat="server"  Width="100px" Text='<%# DataBinder.Eval( Container.DataItem, "RebateThreshold", "{0:0.00}" )%>' ></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
 

                                                                        <asp:TemplateField  HeaderText="Rebate Clause Type"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="rebateClauseNameLabel" runat="server" Width="220px"  Text='<%# DataBinder.Eval( Container.DataItem, "StandardClauseName" )%>'  ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:DropDownList ID="rebateClauseNameDropDownList" DataValueField="StandardRebateTermId"  Width="220px"     DataTextField="StandardClauseName" runat="server" OnDataBound="rebateClauseNameDropDownList_DataBound" OnSelectedIndexChanged="rebateClauseNameDropDownList_OnSelectedIndexChanged"  AutoPostBack="true" >
                                                                            </asp:DropDownList>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                         
                                                                        <asp:TemplateField HeaderText="Last Modified By"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lastModifiedByLabel" runat="server"  Width="70px" Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Label ID="lastModifiedByLabel" runat="server" Width="70px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModifiedBy" ) %>' ></asp:Label>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                                                             
                                                                        <asp:TemplateField HeaderText="Last Modification Date"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lastModificationDateLabel" runat="server"  Width="70px" Font-Size="Smaller" Text='<%#  DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Label ID="lastModificationDateLabel" runat="server" Width="70px" Font-Size="Smaller" Text='<%# DataBinder.Eval( Container.DataItem, "LastModificationDate", "{0:d}" ) %>' ></asp:Label>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField >
                                                                            <ItemTemplate>
                                                                                <asp:Button runat="server"  ID="RemoveRebateButton" Text="Remove Rebate" OnDataBinding="RemoveRebateButton_DataBinding"  OnCommand="RebateGridView_ButtonCommand" CommandName="RemoveRebate" CommandArgument='<%# Container.DataItemIndex & "," &  Eval("RebateId") %>' ButtonType="Button" ControlStyle-Width="82px" >            
                                                                                    </asp:Button >                                    
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Button runat="server"  ID="RemoveRebateButton" Text="Remove Rebate" OnDataBinding="RemoveRebateButton_DataBinding"  OnCommand="RebateGridView_ButtonCommand" CommandName="RemoveRebate" CommandArgument='<%# Container.DataItemIndex & "," &  Eval("RebateId") %>' ButtonType="Button" ControlStyle-Width="82px" >            
                                                                                    </asp:Button >                                    
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                      
                                                                         <asp:TemplateField HeaderText="Rebate Term Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="RebateTermIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "RebateTermId" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Label ID="RebateTermIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "RebateTermId" )%>' ></asp:Label>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>

                                                                           <asp:TemplateField HeaderText="Rebates Standard Rebate Term Id"  ItemStyle-Width="99%" ItemStyle-Height="99%" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="RebatesStandardRebateTermIdLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "RebatesStandardRebateTermId" )%>' ></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:Label ID="RebatesStandardRebateTermIdLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "RebatesStandardRebateTermId" )%>' ></asp:Label>
                                                                            </EditItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                            
                                                                </ep:GridView>                                              
                                                        </div>
                                                    </td>
                                                    <td>
                                                    
                                                    </td>
                                                    
                                                </tr>
                 
                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td>
                                                        <table>
                                                            <tr>
                                                                <td align="left">
                                                                    <asp:Label runat="server" ID="CustomStartDateLabel" Text="Custom Start Date" Font-Bold="true"  />
                                                                </td>
                                                                <td>
                                                                     <asp:TextBox ID="CustomStartDateTextBox" runat="server" ReadOnly="true" MaxLength="10" Width="100px" Text=""  ></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>                                             
                                                    </td>
                                                    <td>
                                                       
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td>
                                                    </td>
                                                    <td align="center">
                                                        <asp:Label runat="server" ID="RebateClauseHeaderLabel" Text="Rebate Clause" Font-Bold="true"  />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td>
                                                    </td>

                                                    <td>
                                                        <div id="RebateClauseDiv" style="height:130px;"> 
                                                            <asp:TextBox ID="RebateClauseTextBox" runat="server" ReadOnly="true" MaxLength="4000" TextMode="MultiLine" CssClass="RebateMultilineInEditMode" Wrap="true"  ControlStyle-Width="99%"  Text="test" Height="120px" ></asp:TextBox>
                                                        </div> 
                                                    </td>
                                                    <td>
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
                                                        <asp:CheckBox ID="cbPricelistVerify" runat="server" TextAlign="Left" Checked='<%#Bind("Pricelist_Verified") %>'
                                                            Enabled="True" />
                                                    </td>
                                                    <td align="right">
                                                        <asp:Label ID="lbPriceListVerifiedBy" runat="server" Font-Names="arial" Font-Size="12px">Verified By:</asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistVerifiedBy" runat="server" Text='<%#Bind("Verified_By") %>' />
                                                    </td>
                                                    <td valign="top" style="font-family: Arial; font-size: smaller">
                                                        Notes:
                                                    </td>
                                                    <td rowspan="4" colspan="2">
                                                        <asp:TextBox ID="tbPricelistNotes" runat="server" Width="255" TextMode="MultiLine"
                                                            Rows="20" Text='<%#Bind("Pricelist_Notes") %>' />
                                                    </td>
                                                </tr>
                                                <tr valign="top">
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Current Mod#:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistCurrMod" runat="server" Width="75" Text='<%#Bind("Current_Mod_Number") %>' />
                                                    </td>
                                                    <td align="right" style="font-family: Arial; font-size: smaller">
                                                        Mod Effective Date:
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbPricelistModDate" runat="server" Width="75" Text='<%#Bind("Verification_Date","{0:d}") %>' />
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
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
                                                            ForeColor="#797BB0" Font-Size="12px" />
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
                                                <tr align="left" valign="top">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbCoveredFCPCount" runat="server" /><br />
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
                                                <tr align="left" valign="top">
                                                    <td  style="font-family: Arial; font-size: small">
                                                        <asp:Label ID="lbPPVTotalCount" runat="server" /><br />
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
                                            </table>
                                        <asp:HiddenField ID="RefreshPricelistScreenOnSubmit" runat="server" Value="false" />

                                        </asp:View>
                                        <asp:View ID="SalesView" runat="server">
                                            <br />
                                            <asp:Button ID="btnAddSales" runat="server" Visible="True" OnClick="openSalesEntry" />
                                            <asp:Panel ID="pnSales" runat="server" ScrollBars="Vertical" Height="400">
                                                <asp:GridView ID="gvSales" runat="server" Width="700px" AutoGenerateColumns="false"
                                                    Font-Names="Arial" Font-Size="small" DataSourceID="SalesDataSource" CellPadding="4"
                                                    AllowSorting="true" OnRowCommand="grid_Command" DataKeyNames="Quarter_ID" ShowFooter="False"
                                                    OnRowDataBound="gvSales_RowDataBound" OnDataBound="gvSales_OnDataBound">
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbTitle" runat="server" Text='<%#Eval("Title") %>' />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                Qtr:
                                                                <asp:DropDownList ID="dlQuarter" runat="server" AppendDataBoundItems="True" DataSourceID="QuarterDataSource"
                                                                    DataTextField="Title" DataValueField="Quarter_ID">
                                                                    <asp:ListItem Text="" Value="" />
                                                                </asp:DropDownList>
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua" SortExpression="VA_Sales_Sum">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbVASAles" runat="server" Text="VA Sales" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbItemVASales" runat="server" Text='<%#Eval("VA_Sales_Sum","{0:c}") %>' />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                VA Sales:<asp:TextBox ID="tbVASales" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbVAQtr" runat="server" Text="Qtr" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbVAQtrNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Qtr_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAQtrNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                SIN:
                                                                <asp:DropDownList ID="dlNewSIN" runat="server" AppendDataBoundItems="true" DataSourceID="SINDataSource"
                                                                    DataTextField="SINs" DataValueField="SINs">
                                                                    <asp:ListItem Text="" Value="" />
                                                                </asp:DropDownList>
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Wrap="false">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbVAYear" runat="server" Text="Year" Font-Bold="true" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbVAYearNums" runat="server" Text='<%#getVAVariance(Eval("Previous_Year_VA_Sales_Sum"),Eval("VA_Sales_Sum"),"lbVAYearNums" )%>'
                                                                    OnDataBinding="setRowFormat" />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbHeadOGASales" runat="server" Text="OGA Sales" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbItemOGASales" runat="server" Text='<%#Eval("OGA_Sales_Sum", "{0:c}") %>' />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                OGA Sales:
                                                                <asp:TextBox ID="tbInsertOGASales" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
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
                                                        <asp:TemplateField HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbHeadSLGSales" runat="server" Text="S/C/L Govt. Sales" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbItemSLGSales" runat="server" Text='<%#Eval("SLG_Sales_Sum", "{0:c}") %>' />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                S/C/L Govt. Sales:
                                                                <asp:TextBox ID="tbInsertSLGSales" runat="server" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
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
                                                        <asp:TemplateField HeaderStyle-BackColor="Aqua" ItemStyle-BackColor="Aqua">
                                                            <HeaderTemplate>
                                                                <asp:Label ID="lbHeadTotalSales" runat="server" Text="Totals" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:Label ID="lbItemTotalASales" runat="server" Text='<%#Eval("Total_Sum", "{0:c}") %>' />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
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
                                                        <asp:TemplateField Visible="True">
                                                            <ItemTemplate>
                                                                <asp:Button ID="btnItemDetails" runat="server" Text="Details" CommandName="Detail"
                                                                    Visible="false" />
                                                                <asp:LinkButton ID="btnItemEdit" runat="server" Text="Edit" Visible="True" />
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                <asp:Button ID="btnInsertAdd" runat="server" Text="Save" CommandName="InsertAdd" />
                                                                <asp:Button ID="btnInserCancel" runat="server" Text="Cancel" CommandName="Cancel" />
                                                            </FooterTemplate>
                                                        </asp:TemplateField>
                                                        <asp:HyperLinkField Text="Details" DataNavigateUrlFormatString="sales_detail.aspx?CntrctNum={1}&QtrID={0}"
                                                            DataNavigateUrlFields="Quarter_ID,CntrctNum" Target="_blank" FooterText="" HeaderText="" />
                                                    </Columns>
                                                </asp:GridView>
                                                <asp:Button ID="btnSalesFooter" runat="server" Text="Add Sales" OnClick="ShowSalesFooter"
                                                    Visible="false" />
                                                <asp:Button ID="btnSalesCancelFooter" runat="server" Text="Cancel" OnClick="ShowSalesFooter"
                                                    Visible="false" />
                                                <asp:HiddenField ID="RefreshSalesDataGridOnSubmit" runat="server" Value="false" />
                                            </asp:Panel>
                                            <asp:Button ID="SalesIFFCheckCompareButton" runat="server" Text="View IFF/Check Comparison" OnClick="SalesIFFCheckCompareButton_OnClick" />
                                            <asp:Label ID="lbSalesHistory" runat="server" Text="Sales history in datasheet view"
                                                Font-Names="Arial" Font-Size="X-Small" ForeColor="#993375" />
                                            <asp:Button ID="DetailedSalesHistoryButton" runat="server" Text="Full Sales History" OnClick="DetailedSalesHistoryButton_OnClick" />
                                            <asp:Button ID="QuarterlySalesHistoryButton" runat="server" Text="Sales by Qtr" OnClick="QuarterlySalesHistoryButton_OnClick" />
                                            <asp:Button ID="AnnualSalesHistoryButton" runat="server" Text="Sales by Year"  OnClick="AnnualSalesHistoryButton_OnClick"  />
                                            <asp:Button ID="exportUploadSalesButton" runat="server" Text="Export/Upload Sales" Width="155px" CausesValidation="False" />
                                        </asp:View>
                                        <asp:View ID="ChecksView" runat="server">
                                            <br />
                                            <asp:Button ID="btnViewIFFCheck" runat="server" Text="View IFF/Check Comparison" OnClick="SalesIFFCheckCompareButton_OnClick" />
                                            <asp:Panel ID="pnChecks" runat="server" ScrollBars="Both" Height="400">
                                                <asp:GridView ID="gvChecks" runat="server" DataSourceID="ChecksDataSource" Font-Names="Arial"
                                                    AutoGenerateColumns="false" CellPadding="4" HeaderStyle-Wrap="true" AllowSorting="true"
                                                    ShowFooter="false" ShowHeader="true" DataKeyNames="ID" OnRowCommand="UpdateChecksGrid"  OnDataBound="gvChecks_OnDataBound" OnRowUpdating="gvChecks_OnRowUpdating" >
                                                    <Columns>
                                                        <asp:TemplateField HeaderText="Quarter" SortExpression="Quarter_ID">
                                                            <ItemTemplate>
                                                                <asp:DropDownList ID="dlQuarter" runat="server" DataSourceID="QuarterDataSource"
                                                                    DataTextField="Title" DataValueField="Quarter_ID" SelectedValue='<%#Eval("Quarter_ID")%>'
                                                                    Enabled="false">
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
                                                                <asp:TextBox ID="tbCheckAmountNew" runat="server" Width="84px" />
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
                                                                <asp:TextBox ID="tbChecknumNew" runat="server" Width="64px" />
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
                                                                <asp:TextBox ID="tbCheckDepositNew" runat="server" Width="70px"/>
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
                                                                <asp:TextBox ID="tbdateRecvdNew" runat="server" Width="78px" />
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
                                                                <asp:TextBox ID="tbCommentsNew" runat="server" Width="120px"/>
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
                                                                <asp:Button ID="btnAddNew" runat="server" Text="Add" CommandName="InsertNew" />
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
                                                                        Text="Note: Selecting a plan and clicking save associates this contract with the selected plan." />
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
                                                                                <asp:Label ID="lbSBACntrctResp" runat="server"  />
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
                                                                            <asp:DropDownList ID="dlPlanType" runat="server" AutoPostBack="true" DataSourceID="PlanTypeDataSource"
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
                                                                            <asp:TextBox ID="tbPlanAdminName" MaxLength="50" runat="server"   Text='<%#Eval("Plan_Admin_Name") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Email:
                                                                        </td>
                                                                        <td colspan="5">
                                                                            <asp:TextBox ID="tbPlanAdminEmail" MaxLength="50"  runat="server"  Text='<%#Eval("Plan_Admin_email") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Address:
                                                                        </td>
                                                                        <td colspan="5">
                                                                            <asp:TextBox ID="tbPlanAdminAddress" MaxLength="50" runat="server" Text='<%#Eval("Plan_Admin_Address1") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            City
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminCity" MaxLength="50" runat="server"  Text='<%#Eval("Plan_Admin_City") %>' />
                                                                        </td>
                                                                        <td>
                                                                            St:
                                                                        </td>
                                                                        <td>
                                                                            <asp:DropDownList ID="dlPlanAdminstate" runat="server"  DataSourceID="StateDataSource"
                                                                                DataTextField="abbr" DataValueField="Abbr" SelectedValue='<%#Eval("Plan_Admin_State") %>'
                                                                                AppendDataBoundItems="true">
                                                                                <asp:ListItem Value="" />
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                        <td>
                                                                            Zip
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminZip" MaxLength="15" runat="server"  Text='<%#Eval("Plan_Admin_Zip") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            Phone:
                                                                        </td>
                                                                        <td>
                                                                            <asp:TextBox ID="tbPlanAdminPhone" MaxLength="30" runat="server"   Text='<%#Eval("Plan_Admin_Phone") %>' />
                                                                        </td>
                                                                        <td colspan="2">
                                                                            Fax:
                                                                        </td>
                                                                        <td colspan="2">
                                                                            <asp:TextBox ID="tbPlanAdminFax" MaxLength="15" runat="server"   Text='<%#Eval("Plan_Admin_Fax") %>' />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="6">
                                                                            <asp:Button ID="btnEditSBAAdmin" runat="server" Text="Update" OnClick="UpdateSBAAdmin" />
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
                                                        <asp:TextBox ID="tbBPAInfoSolicitationNum" runat="server" Text='<%#Eval("Solicitation_Number") %>' />
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="tbBPAInfoMin" runat="server" Text='<%#Eval("Mininum_Order") %>'
                                                            Width="400" />
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
                                                        <asp:TextBox ID="tbBPAInfoEst" runat="server" Text='<%#Eval("Estimated_Contract_Value","{0:c}") %>' />
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td rowspan="3">
                                                        <asp:TextBox ID="tbBPAInfoDel" runat="server" Text='<%#Eval("Delivery_Terms") %>'
                                                            Width="400" TextMode="MultiLine" Rows="5" />
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
                                                        <asp:TextBox ID="tbBPAInfoGeo" runat="server" Text='<%#Eval("Geographic_Coverage_ID") %>' />
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
                                                            Enabled="True" />
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
                                                        <asp:TextBox ID="tbBPAPriceNotes" runat="server" Width="100" Rows="20" Text='<%#Eval("Pricelist_Notes") %>'
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSgeneralAddr1" runat="server" Text='<%#Eval("Primary_Address_1") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address 2:
                                                                                    </td>
                                                                                    <td colspan="3" style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSgeneralAddr2" runat="server" Text='<%#Eval("Primary_Address_2") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        City:
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSgeneralCity" runat="server" Text='<%#Eval("Primary_City") %>'
                                                                                            Font-Size="Small" Width="150" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: small">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralState" runat="server" Text='<%#Eval("Primary_State") %>'
                                                                                            Columns="2" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Zip:
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPSFSSGeneralZip" runat="server" Columns="9" Text='<%#Eval("Primary_Zip") %>'
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
                                                                                            <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralDrug" runat="server" Width="250"
                                                                                                Text='<%#Eval("Drug_Covered") %>' /></div>
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
                                                                                                                <asp:CheckBox ID="cbBPAFSSGeneralPV" runat="server" Enabled="False" Font-Size="1"
                                                                                                                    ForeColor="#000099" Width="40" Checked='<%#Eval("PV_Participation") %>' />
                                                                                                            </td>
                                                                                                        </tr>
                                                                                                    </table>
                                                                                                </td>
                                                                                                <td align="center">
                                                                                                    <table style="border-style: outset; background-color: Silver">
                                                                                                        <tr>
                                                                                                            <td align="center" style="font-family: Arial; color: #000099; font-size: x-small">
                                                                                                                VA/DOD<br>
                                                                                                                Contract<asp:CheckBox ID="cbBPAFSSGeneralVADOD" runat="server" Enabled="False" Font-Names="Arial"
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
                                                                            <asp:Panel ID="pnBPAFSSSin" runat="server" Width="125" Height="175" ScrollBars="Vertical">
                                                                                <asp:GridView ID="gvBPAFSSSIN" runat="server" AutoGenerateColumns="False" DataSourceID="SINdataSource"
                                                                                    Width="100">
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
                                                                                        <asp:Label ID="lbBPAFSSGeneralAwardDate" runat="server" Text='<%#Eval("Dates_CntrctAward","{0:d}") %>'
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
                                                                                        <asp:DropDownList Enabled="false" ID="DropDownList4" runat="server" Width="75">
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
                                                                                                        Font-Size="1" Checked='<%#Eval("Terminated_Default") %>' />
                                                                                                </td>
                                                                                                <td style="font-family: Arial; font-size: x-small">
                                                                                                    Convenience:
                                                                                                    <asp:CheckBox ID="cbBPAFSSGeneralTermConv" runat="server" ForeColor="Black" Font-Names="Arial"
                                                                                                        Font-Size="1" Checked='<%#Eval("Terminated_Convenience") %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralCurOpYear" runat="server" Width="75"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralPriName" runat="server" Text='<%#Eval("POC_Primary_Name") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralPriPhone" runat="server" Text='<%#Eval("POC_Primary_Phone","{0:(###)###-####}") %>'></asp:TextBox>
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tlbBPAFSSGeneralPriExt" runat="server" Width="40"
                                                                                            Text='<%#Eval("POC_Primary_Ext") %>'></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralPriFax" runat="server" Text='<%#Eval("POC_Primary_Fax") %>'></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralPriEmail" runat="server" Text='<%#Eval("POC_Primary_Email") %>'></asp:TextBox>
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
                                                                            <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeneralNotes" runat="server" Rows="10" Width="700"
                                                                                Text='<%#Eval("POC_Notes") %>' Wrap="true" TextMode="MultiLine" />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVendorSize" runat="server" Width="75" Text='<%#Eval("Socio_Business_Size_ID") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Small Disadvantaged Business<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSSDB" runat="server" Enabled="True" Checked='<%#Eval("Socio_SDB") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        8A<br>
                                                                                        <asp:CheckBox ID="cbBPAFSS8A" runat="server" Enabled="True" Checked='<%#Eval("Socio_8a") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Woman<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSWoman" runat="server" Enabled="True" Checked='<%#Eval("Socio_Woman") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Hub Zone<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSHub" runat="server" Enabled="True" Checked='<%#Eval("Socio_HubZone") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td>
                                                                                    </td>
                                                                                    <td colspan="4" align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Veteran Status<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVetStatus" runat="server" Text='<%#Eval("Socio_VetStatus_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                            <br>
                                                                            <table width="350">
                                                                                <tr align="center">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        DUNS<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSDun" runat="server" Width="125" Text='<%#Eval("DUNS") %>'
                                                                                            Font-Size="small" />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Vendor Type<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVendorTpe" runat="server" Text='<%#Eval("Dist_Manf_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Tax Identification Number (TIN)<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTIN" runat="server" Width="125" Font-Size="small"
                                                                                            Text='<%#Eval("TIN") %>'></asp:TextBox>
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Geographic Coverage<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSGeo" runat="server" Text='<%#Eval("Geographic_Coverage_ID") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #cc0066">
                                                                                        Note: Please enter 9 digit numbers only, without any dashes or spaces.
                                                                                    </td>
                                                                                    <td rowspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSNotes" runat="server" Width="125" Height="200" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Credit Card Accepted<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSCreditCard" runat="server" Enabled="True" Checked='<%#Eval("Credit_Card_Accepted") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td align="center" style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Hazardous<br>
                                                                                        <asp:CheckBox ID="cbBPAFSSHaz" runat="server" Enabled="True" Checked='<%#Eval("Hazard") %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSWarrantDuration" runat="server" Width="100"
                                                                                            Font-Size="small" Text='<%#Eval("Warranty_Duration") %>'></asp:TextBox>
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSWarrantNotes" runat="server" Rows="15" Width="350"
                                                                                            Height="100" Text='<%#Eval("Warranty_Notes") %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbReturnPolicy" runat="server" Text='<%#Eval("Returned_Goods_Policy_Type")  %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSReturnNotes" runat="server" Rows="15" Width="350"
                                                                                            Height="100" Font-Size="small" Text='<%#Eval("Returned_Goods_Policy_Notes") %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSSolicitationNumber" runat="server" Width="125"
                                                                                            Font-Size="small" Text='<%#Eval("Solicitation_Number") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Tracking Customer Number<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTrackCustomer" runat="server" Width="200"
                                                                                            Font-Size="small" Text='<%#Eval("Tracking_Customer") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        IFF Absorbed/Embedded<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSIFFEmbedded" runat="server" Width="125"
                                                                                            Font-Size="small" Text='<%#Eval("IFF_Type_ID") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        End of Year Discount<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSYearEndDiscount" runat="server" Width="200"
                                                                                            Font-Size="small" Text='<%#Eval("Annual_Rebate") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Estimated Value<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSEstimatedValue" runat="server" Width="125"
                                                                                            Font-Size="small" Text='<%#Eval("Estimated_Contract_Value") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Minimum Order<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSMinOrder" runat="server" Width="200" Font-Size="small"
                                                                                            Text='<%#Eval("Mininum_Order") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        FPR Date<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSFPRDate" runat="server" Width="125" Font-Size="small"
                                                                                            Text='<%#Eval("BF_Offer") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Ratio<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSRatio" runat="server" Width="200" Font-Size="small"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSStandard" runat="server" Width="300" Height="100"
                                                                                            Font-Size="small" Text='<%#Eval("Delivery_Terms") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr valign="top">
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Expedited
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSExpedited" runat="server" Width="300" Height="100"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSBasicDiscount" runat="server" Width="300"
                                                                                            Height="60" Font-Size="small" Text='<%#Eval("Discount_Basic") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Quantity Discount:<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSQuanDiscount" runat="server" Width="300"
                                                                                            Height="60" Font-Size="small" Text='<%#Eval("Discount_Quantity") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Prompt Pay Discount:<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSPromptPay" runat="server" Width="300" Height="60"
                                                                                            Font-Size="small" Text='<%#Eval("Discount_Prompt_Pay") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Credit Card Discount:<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSCreidtCardDiscount" runat="server" Width="300"
                                                                                            Height="60" Font-Size="small" Text='<%#Eval("Discount_Credit_Card") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small; color: #000099">
                                                                                        Addiontal Incentive Discount Information:<br>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAddtionalDiscount" runat="server" Width="300"
                                                                                            Height="60" Font-Size="small" Text='<%#Eval("Incentive_Description") %>' />
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVCAdminName" runat="server" Width="225"
                                                                                            Font-Size="Smaller" Text='<%#Eval("POC_Primary_Name") %>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVCAdminPhone" runat="server" Width="175"
                                                                                            Font-Size="Smaller" Text='<%#Eval("POC_Primary_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVCAdminExt" runat="server" Width="50" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Primary_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVCAdminFax" runat="server" Width="200" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Primary_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSVCAdminEmail" runat="server" Width="225"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAVCName" runat="server" Width="225" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAVCPhone" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAVCExt" runat="server" Width="50" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAVCFax" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Alternate_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSAVCEmail" runat="server" Width="225" Font-Size="Smaller"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTCName" runat="server" Width="225" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTCPhone" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTCExt" runat="server" Width="50" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTCFax" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Tech_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSTCEmail" runat="server" Width="225" Font-Size="Smaller"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSS24HRName" runat="server" Width="225" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSS24HRPhone2" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSS24HRExt" runat="server" Width="50" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSS24HRFax" runat="server" Width="175" Font-Size="Smaller"
                                                                                            Text='<%#Eval("POC_Emergency_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSS24HREmail" runat="server" Width="225" Font-Size="Smaller"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbOrderAddress1" runat="server" Width="200" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Address_1")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Address2:
                                                                                    </td>
                                                                                    <td colspan="5">
                                                                                        <asp:TextBox ReadOnly="True" ID="tbOrderAddress2" runat="server" Width="200" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Address_2")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        City:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderCity" runat="server" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_City")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        State:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderState" runat="server" Text='<%#Eval("Ord_State") %>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial" colspan="2">
                                                                                        <div style="font-size: x-small">
                                                                                            Zip:</div>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderZip" runat="server" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Zip")%>' Width="75" />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderPhone" runat="server" Font-Size="Smaller"
                                                                                            Text='<%#Eval("Ord_Telephone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderExt" runat="server" Width="50" Font-Size="Smaller"
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
                                                                                        <asp:TextBox ReadOnly="True" ID="tbBPAFSSOrderEmail" runat="server" Width="200" Font-Size="Smaller"
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
                                                                                        <asp:TextBox ID="tbSalesName2" runat="server" Width="225" Font-Size="Smaller" Text='<%#Eval("POC_Sales_Name")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Phone:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbSalesPhone2" runat="server" Width="175" Font-Size="Smaller" Text='<%#Eval("POC_Sales_Phone","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Ext:
                                                                                    </td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="tbSalesExt2" runat="server" Width="50" Font-Size="Smaller" Text='<%#Eval("POC_Sales_Ext")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Fax:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbSalesFax2" runat="server" Width="175" Font-Size="Smaller" Text='<%#Eval("POC_Sales_Fax","{0:(###)###-####}")%>' />
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td style="font-family: Arial; font-size: x-small">
                                                                                        Email:
                                                                                    </td>
                                                                                    <td colspan="3">
                                                                                        <asp:TextBox ID="tbSalesEmail2" runat="server" Width="225" Font-Size="Smaller" Text='<%#Eval("POC_Sales_Email")%>' />
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
                                                                        <asp:ButtonField ButtonType="Button" Text="Details" />
                                                                        <asp:ButtonField ButtonType="Button" Text="Edit" />
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </asp:Panel>
                                                            <asp:Button ID="btnBPAFSSIFF" runat="server" Text="View IFF/Check Comparison" />
                                                            <asp:Label ID="lbBPAFSSSalesHistory" runat="server" Text="Sales history in datasheet view"
                                                                Font-Names="Arial" Font-Size="X-Small" ForeColor="#993375" Visible="false"/>
                                                            <asp:Button ID="btnBPAFSSFullSales" runat="server" Text="Full Sales History" Visible="false" />
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
                        <asp:HiddenField ID="hfADUser" runat="server" Visible="false" Value='<%#Bind("AD_User") %>' />
                        <asp:HiddenField ID="hfSMUser" runat="server" Visible="false" Value='<%#Bind("SM_User") %>' />
                        <asp:HiddenField ID="hfCOUser" runat="server" Visible="false" Value='<%#Bind("CO_User") %>' />
                        <asp:HiddenField ID="hfBPAFSSContract" runat="server" Visible="false" Value='<%#Bind("BPA_FSS_Counterpart") %>' />
                    </EditItemTemplate>
                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <EditRowStyle BackColor="#999999" />
                </asp:FormView>
                <asp:SqlDataSource ID="ContractDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                     OnUpdating="ContractDataSource_OnUpdating"
                    SelectCommand="SELECT * FROM [View_Contracts_Full] WHERE ([CntrctNum] = @CntrctNum)"
                    UpdateCommand="UPDATE dbo.tbl_Cntrcts SET Schedule_Number = @Schedule_Number, CO_ID = @CO_ID, Contractor_Name = @Contractor_Name, DUNS = @DUNS, TIN = @TIN, PV_Participation = @PV_Participation, Solicitation_Number = @Solicitation_Number, Primary_Address_1 = @Primary_Address_1, Primary_Address_2 = @Primary_Address_2, Primary_City = @Primary_City, Primary_State = @Primary_State, Primary_Zip = @Primary_Zip, POC_Primary_Name = @POC_Primary_Name, POC_Primary_Phone = @POC_Primary_Phone, POC_Primary_Ext = @POC_Primary_Ext, POC_Primary_Fax = @POC_Primary_Fax, POC_Primary_Email = @POC_Primary_Email, POC_VendorWeb = @POC_VendorWeb, POC_Notes = @POC_Notes, POC_Alternate_Name = @POC_Alternate_Name, POC_Alternate_Phone = @POC_Alternate_Phone, POC_Alternate_Ext = @POC_Alternate_Ext, POC_Alternate_Fax = @POC_Alternate_Fax, POC_Alternate_Email = @POC_Alternate_Email, POC_Emergency_Name = @POC_Emergency_Name, POC_Emergency_Phone = @POC_Emergency_Phone, POC_Emergency_Ext = @POC_Emergency_Ext, POC_Emergency_Fax = @POC_Emergency_Fax, POC_Emergency_Email = @POC_Emergency_Email, 
                    POC_Tech_Name = @POC_Tech_Name, POC_Tech_Phone = @POC_Tech_Phone, POC_Tech_Ext = @POC_Tech_Ext, POC_Tech_Fax = @POC_Tech_Fax, POC_Tech_Email = @POC_Tech_Email, 
                    POC_Sales_Name = @POC_Sales_Name, POC_Sales_Phone = @POC_Sales_Phone, POC_Sales_Ext = @POC_Sales_Ext, POC_Sales_Fax = @POC_Sales_Fax, POC_Sales_Email = @POC_Sales_Email, 
                    Socio_VetStatus_ID = @Socio_VetStatus_ID, Socio_Business_Size_ID = @Socio_Business_Size_ID, Socio_SDB = @Socio_SDB, Socio_8a = @Socio_8a, Socio_Woman = @Socio_Woman, Socio_HubZone = @Socio_HubZone, Discount_Basic = @Discount_Basic, Discount_Credit_Card = @Discount_Credit_Card, Discount_Prompt_Pay = @Discount_Prompt_Pay, Discount_Quantity = @Discount_Quantity, Geographic_Coverage_ID = @Geographic_Coverage_ID, Tracking_Customer = @Tracking_Customer, Mininum_Order = @Mininum_Order, Delivery_Terms = @Delivery_Terms, Expedited_Delivery_Terms = @Expedited_Delivery_Terms, Annual_Rebate = @Annual_Rebate, BF_Offer = @BF_Offer, Credit_Card_Accepted = @Credit_Card_Accepted, Hazard = @Hazard, Warranty_Duration = @Warranty_Duration, Warranty_Notes = @Warranty_Notes, IFF_Type_ID = @IFF_Type_ID, Ratio = @Ratio, Returned_Goods_Policy_Type = @Returned_Goods_Policy_Type, Returned_Goods_Policy_Notes = @Returned_Goods_Policy_Notes, Incentive_Description = @Incentive_Description, Dist_Manf_ID = @Dist_Manf_ID, Ord_Address_1 = @Ord_Address_1, Ord_Address_2 = @Ord_Address_2, Ord_City = @Ord_City, Ord_State = @Ord_State, Ord_Zip = @Ord_Zip, Ord_Telephone = @Ord_Telephone, Ord_Ext = @Ord_Ext, Ord_Fax = @Ord_Fax, Ord_EMail = @Ord_EMail, Estimated_Contract_Value = CONVERT (money, @Estimated_Contract_Value), Dates_CntrctAward = @Dates_CntrctAward, Dates_Effective = @Dates_Effective, Dates_CntrctExp = @Dates_CntrctExp, Dates_Completion = @Dates_Completion, Dates_TotOptYrs = @Dates_TotOptYrs, Pricelist_Verified = @Pricelist_Verified, Verification_Date = @Verification_Date, Verified_By = @Verified_By, Current_Mod_Number = @Current_Mod_Number, Pricelist_Notes = @Pricelist_Notes, SBAPlanID = @SBAPlanID, VA_DOD = @VA_DOD, Terminated_Convenience = @Terminated_Convenience, Terminated_Default = @Terminated_Default, Drug_Covered = @Drug_Covered, BPA_FSS_Counterpart = @BPA_FSS_Counterpart, VA_IFF = @VA_IFF, OGA_IFF = @OGA_IFF, Cost_Avoidance = @Cost_Avoidance, SBA_Plan_Exempt = @SBA_Plan_Exempt, Insurance_Policy_Effective_Date = @Insurance_Policy_Effective_Date, Insurance_Policy_Expiration_Date = @Insurance_Policy_Expiration_Date, Solicitation_ID = @Solicitation_ID, Offer_ID = @Offer_ID, [65IB_Contract_Type] = @65IB_Contract_Type, TradeAgreementActCompliance = @TradeAgreementActCompliance, StimulusAct = @StimulusAct, Standardized = @Standardized, RebateRequired = @RebateRequired, LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate WHERE (CntrctNum = @CntrctNum)">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Schedule_Number" Type="Int32" />
                        <asp:Parameter Name="CO_ID" Type="Int32" />
                        <asp:Parameter Name="Contractor_Name" Type="String" />
                        <asp:Parameter Name="DUNS" Type="String" />
                        <asp:Parameter Name="TIN" Type="String" />
                        <asp:Parameter Name="PV_Participation" DefaultValue="0" />
                        <asp:Parameter Name="Solicitation_Number" Type="String" />
                        <asp:Parameter Name="Primary_Address_1" Type="String" />
                        <asp:Parameter Name="Primary_Address_2" Type="String" />
                        <asp:Parameter Name="Primary_City" Type="String" />
                        <asp:Parameter Name="Primary_State" Type="String" />
                        <asp:Parameter Name="Primary_Zip" Type="String" />
                        <asp:Parameter Name="POC_Primary_Name" Type="String" />
                        <asp:Parameter Name="POC_Primary_Phone" Type="String" />
                        <asp:Parameter Name="POC_Primary_Ext" Type="String" />
                        <asp:Parameter Name="POC_Primary_Fax" Type="String" />
                        <asp:Parameter Name="POC_Primary_Email" Type="String" />
                        <asp:Parameter Name="POC_VendorWeb" Type="String" />
                        <asp:Parameter Name="POC_Notes" Type="String" />
                        <asp:Parameter Name="POC_Alternate_Name" Type="String" />
                        <asp:Parameter Name="POC_Alternate_Phone" Type="String" />
                        <asp:Parameter Name="POC_Alternate_Ext" Type="String" />
                        <asp:Parameter Name="POC_Alternate_Fax" Type="String" />
                        <asp:Parameter Name="POC_Alternate_Email" Type="String" />
                        <asp:Parameter Name="POC_Emergency_Name" Type="String" />
                        <asp:Parameter Name="POC_Emergency_Phone" Type="String" />
                        <asp:Parameter Name="POC_Emergency_Ext" Type="String" />
                        <asp:Parameter Name="POC_Emergency_Fax" Type="String" />
                        <asp:Parameter Name="POC_Emergency_Email" Type="String" />
                        <asp:Parameter Name="POC_Tech_Name" Type="String" />
                        <asp:Parameter Name="POC_Tech_Phone" Type="String" />
                        <asp:Parameter Name="POC_Tech_Ext" Type="String" />
                        <asp:Parameter Name="POC_Tech_Fax" Type="String" />
                        <asp:Parameter Name="POC_Tech_Email" Type="String" />
                        <asp:Parameter Name="POC_Sales_Name" Type="String" />
                        <asp:Parameter Name="POC_Sales_Phone" Type="String" />
                        <asp:Parameter Name="POC_Sales_Ext" Type="String" />
                        <asp:Parameter Name="POC_Sales_Fax" Type="String" />
                        <asp:Parameter Name="POC_Sales_Email" Type="String" />
                        <asp:Parameter Name="Socio_VetStatus_ID" Type="Int32" />
                        <asp:Parameter Name="Socio_Business_Size_ID" Type="Int32" />
                        <asp:Parameter Name="Socio_SDB" />
                        <asp:Parameter Name="Socio_8a" />
                        <asp:Parameter Name="Socio_Woman" />
                        <asp:Parameter Name="Socio_HubZone" />
                        <asp:Parameter Name="Discount_Basic" Type="String" />
                        <asp:Parameter Name="Discount_Credit_Card" Type="String" />
                        <asp:Parameter Name="Discount_Prompt_Pay" Type="String" />
                        <asp:Parameter Name="Discount_Quantity" Type="String" />
                        <asp:Parameter Name="Geographic_Coverage_ID" Type="Int32" />
                        <asp:Parameter Name="Tracking_Customer" Type="String" />
                        <asp:Parameter Name="Mininum_Order" Type="String" />
                        <asp:Parameter Name="Delivery_Terms" Type="String" />
                        <asp:Parameter Name="Expedited_Delivery_Terms" Type="String" />
                        <asp:Parameter Name="Annual_Rebate" Type="String" />
                        <asp:Parameter Name="BF_Offer" Type="String" />
                        <asp:Parameter Name="Credit_Card_Accepted" />
                        <asp:Parameter Name="Hazard" />
                        <asp:Parameter Name="Warranty_Duration" Type="String" />
                        <asp:Parameter Name="Warranty_Notes" Type="String" />
                        <asp:Parameter Name="IFF_Type_ID" Type="Int32" />
                        <asp:Parameter Name="Ratio" Type="String" />
                        <asp:Parameter Name="Returned_Goods_Policy_Type" Type="Int32" />
                        <asp:Parameter Name="Returned_Goods_Policy_Notes" Type="String" />
                        <asp:Parameter Name="Incentive_Description" Type="String" />
                        <asp:Parameter Name="Dist_Manf_ID" Type="Int32" />
                        <asp:Parameter Name="Ord_Address_1" Type="String" />
                        <asp:Parameter Name="Ord_Address_2" Type="String" />
                        <asp:Parameter Name="Ord_City" Type="String" />
                        <asp:Parameter Name="Ord_State" Type="String" />
                        <asp:Parameter Name="Ord_Zip" Type="String" />
                        <asp:Parameter Name="Ord_Telephone" Type="String" />
                        <asp:Parameter Name="Ord_Ext" Type="String" />
                        <asp:Parameter Name="Ord_Fax" Type="String" />
                        <asp:Parameter Name="Ord_EMail" Type="String" />
                        <asp:Parameter Name="Estimated_Contract_Value" />
                        <asp:Parameter Name="Dates_CntrctAward" Type="DateTime" />
                        <asp:Parameter Name="Dates_Effective" Type="DateTime" />
                        <asp:Parameter Name="Dates_CntrctExp" Type="DateTime" />
                        <asp:Parameter Name="Dates_Completion" Type="DateTime" />
                        <asp:Parameter Name="Dates_TotOptYrs" Type="Int32" />
                        <asp:Parameter Name="Pricelist_Verified" />
                        <asp:Parameter Name="Verification_Date" Type="DateTime" />
                        <asp:Parameter Name="Verified_By" Type="String" />
                        <asp:Parameter Name="Current_Mod_Number" Type="String" />
                        <asp:Parameter Name="Pricelist_Notes" Type="String" />
                        <asp:Parameter Name="SBAPlanID" Type="Int32" />
                        <asp:Parameter Name="VA_DOD" DefaultValue="0" />
                        <asp:Parameter Name="Terminated_Convenience" />
                        <asp:Parameter Name="Terminated_Default" />
                        <asp:Parameter Name="Drug_Covered" Type="String" />
                        <asp:Parameter Name="BPA_FSS_Counterpart" Type="String" />
                        <asp:Parameter Name="VA_IFF" Type="Decimal" />
                        <asp:Parameter Name="OGA_IFF" Type="Decimal" />
                        <asp:Parameter Name="Cost_Avoidance" Type="Decimal" />
                        <asp:Parameter Name="SBA_Plan_Exempt" DefaultValue="0" />
                        <asp:Parameter Name="Insurance_Policy_Effective_Date" Type="DateTime" />
                        <asp:Parameter Name="Insurance_Policy_Expiration_Date" Type="DateTime" />
                        <asp:Parameter Name="Solicitation_ID" Type="Int32" />
                        <asp:Parameter Name="Offer_ID" Type="Int32" />
                        <asp:Parameter Name="65IB_Contract_Type" Type="Int32" />
                        <asp:Parameter Name="TradeAgreementActCompliance" Type="String"  />
                        <asp:Parameter Name="StimulusAct" DefaultValue="0"  />
                        <asp:Parameter Name="Standardized" DefaultValue="0" />
                        <asp:Parameter Name="RebateRequired" DefaultValue="0"  />
                        <asp:Parameter Name="CntrctNum" Type="String" />
                        <asp:Parameter Name="LastModifiedBy" Type="String" />
                        <asp:Parameter Name="LastModificationDate" Type="DateTime" />
                   </UpdateParameters>
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
                    SelectCommand="SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.Year, dbo.tlkup_year_qtr.Qtr, dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, dbo.View_Contract_Preview.Contractor_Name, dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF AS VA_IFF_Amount, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF AS OGA_IFF_Amount, (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF) + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.tbl_IFF.SLG_IFF AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales, View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales, dbo.tbl_IFF.VA_IFF, dbo.tbl_IFF.OGA_IFF, dbo.tbl_IFF.SLG_IFF, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF AS SLG_IFF_Amount FROM dbo.[tlkup_Sched/Cat] INNER JOIN dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number INNER JOIN dbo.tbl_IFF ON dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.Schedule_Number LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A INNER JOIN dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON dbo.tbl_IFF.Start_Quarter_Id = dbo.tlkup_year_qtr.start_quarter_id AND dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_1 ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_2 ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID WHERE (dbo.View_Contract_Preview.CntrctNum = @CntrctNum) AND dbo.View_Sales_Variance_by_Year_A.Quarter_ID is not Null ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SINdataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT * FROM [tbl_Cntrcts_SINs] WHERE ([CntrctNum] = @CntrctNum) order by SINs "
                    InsertCommand="INSERT INTO [tbl_Cntrcts_SINs] (CntrctNum, SINs, Recoverable,LastModifiedBy,LastModificationDate) VALUES (@CntrctNum, @SIN, @Recoverable, @LastModifiedBy, @LastModificationDate)"
                    UpdateCommand="UPDATE [tbl_Cntrcts_SINs] set Recoverable = @Recoverable, LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate where CntrctNum = @CntrctNum AND SINs = @SIN"
                    DeleteCommand="exec DeleteSINFromContract @UserLogin, @UserId, @ContractNumber, @SIN " >
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
  
                    <InsertParameters>
                        <asp:Parameter Name="CntrctNum" />
                        <asp:Parameter Name="SIN" />
                        <asp:Parameter Name="Recoverable" />
                    </InsertParameters>
                     
                </asp:SqlDataSource>

  
                <asp:SqlDataSource ID="SBACntrctrespDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAPlanTypeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="dateExpirationdataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() }) AND (Date <= { fn NOW() } + 4000) ORDER BY Date">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="dateCompDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT  DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 7) AND (Date <= { fn NOW() } + 120) ORDER BY Date">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="businessSizeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Business_Size_ID], [Business_Size] FROM tlkup_Business_Size GROUP BY [Business_Size_ID], [Business_Size]">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="vetStatusDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [VetStatus_ID], [VetStatus_Description] FROM tlkup_VetStatus GROUP BY [VetStatus_ID], [VetStatus_Description]">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="VendorTypeDateSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Dist_Manf_ID], [Dist_Manf_Description] FROM tlkup_Dist_Manf">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="GeoCoverageDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Geographic_ID], [Geographic_Description] FROM tlkup_Geographic GROUP BY [Geographic_ID], [Geographic_Description]">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ReturnDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Returned_Goods_Policy_Type_ID], [Returned_Goods_Policy_Description] FROM tlkup_Ret_Goods_Policy">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAPlanDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [SBAPlanID], [PlanName] FROM view_sba_plans_sorted"></asp:SqlDataSource>
                <asp:SqlDataSource ID="CODataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" OnSelecting="CODataSource_OnSelecting"
                    SelectCommand="exec [NACSEC].[dbo].SelectContractingOfficers3 @DivisionId, @SelectFlag, @OrderByLastNameFullName, @IsExpired ">
                <SelectParameters>
                    <asp:Parameter Name="DivisionId" Type="Int32" />
                    <asp:Parameter Name="SelectFlag" Type="Int32" />
                    <asp:Parameter Name="OrderByLastNameFullName" Type="String" />
                    <asp:Parameter Name="IsExpired" Type="Boolean" />
                </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="StatedataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [Abbr], [State/Province], [Country] FROM tlkup_StateAbbr GROUP BY [Abbr], [State/Province], [Country] ORDER BY [State/Province]">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="AddSindataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="sp_Contract_SIN_Selection" SelectCommandType="StoredProcedure">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="StateCoverageDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT * FROM [tbl_Cntrcts_State_Coverage] WHERE ([CntrctNum] = @CntrctNum)"
                    DeleteCommand="DELETE FROM [tbl_Cntrcts_State_Coverage] WHERE [ID] = @ID" InsertCommand="INSERT INTO [tbl_Cntrcts_State_Coverage] ([CntrctNum], [Abbr]) VALUES (@CntrctNum, @Abbr)">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
                    </SelectParameters>
                    <DeleteParameters>
                        <asp:Parameter Name="ID" Type="Int32" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="CntrctNum" Type="String" />
                        <asp:Parameter Name="Abbr" Type="String" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="QuarterDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT Quarter_ID, Title FROM tlkup_year_qtr WHERE (End_Date < { fn NOW() }) ORDER BY Quarter_ID DESC">
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SBAPlanProjections" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
                    SelectCommand="SELECT [EndDate], [StartDate], [ProjectionID] FROM [tbl_sba_Projection] WHERE ([SBAPlanID] = @SBAPlanID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SBAPlanID" SessionField="SBAPlanID" Type="Int32" />
                    </SelectParameters>
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
  
    <input id="scrollPos" runat="server" type="hidden" value="0" enableviewstate="true"  />
    <input id="highlightedRebateRow" runat="server" type="hidden" value="0" enableviewstate="true"  />
    <input id="highlightedRebateRowOriginalColor" runat="server" type="hidden" value="0" enableviewstate="true"  />
    <input id="confirmationMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />
    <input id="promptMessageResults" runat="server" type="hidden" value="false" enableviewstate="true"  />                                         
  
    <ep:MsgBox ID="MsgBox" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />

    </form>
</body>
</html>
