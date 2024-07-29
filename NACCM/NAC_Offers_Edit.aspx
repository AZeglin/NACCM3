<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="NAC_Offers_Edit.aspx.vb" Inherits="NACCM.NAC_Offers_Edit" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Offers Edit</title>
</head>
<body style="background-color: #ece9d8; font-family: Arial">
    <form id="Form1" method="post" runat="server">
    <table style="font-family: Arial">
        <tr>
            <td>
            </td>
            <td>
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td valign="top" align="left" colspan="2">
                <table style="height: 600; width: 875">
                    <tr>
                        <td style="font-size: x-large; color: #003399">
                            <strong>NAC CM</strong><div style="font-size: large; color: Black">
                                <em>Contract Management</em></div>
                        </td>
                        <td style="width: 204px">
                        </td>
                        <td align="left">
                            <asp:Button ID="btnExit" runat="server" Text="Close Window" />
                        </td>
                    </tr>
                    <tr valign="top">
                        <td valign="top" align="left" colspan="3">
                            <asp:SqlDataSource ID="StateDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT * FROM [tlkup_StateAbbr]"></asp:SqlDataSource>
                            <asp:FormView ID="fvOfferRecord" runat="server" CellPadding="4" ForeColor="#333333"
                                DataSourceID="OfferDataSource" BackColor="#ECE9D8" DefaultMode="Edit" DataKeyNames="Offer_ID"
                                 OnPreRender="fvOfferRecord_OnPreRender" OnDataBound="fvOfferRecord_OnDataBound" >
                                <EditItemTemplate>
                                 <table style="height: 500; width: 700; background-color: #9baeb9">
                                        <tr style="height: 30">
                                            <td style="width: 25; height: 25">
                                            </td>
                                            <td align="left" colspan="2" style="color: #000099">
                                                <strong>New Offer Record</strong>
                                            </td>
                                            <td><asp:Button ID="SaveOfferButton" runat="server" OnClick="SaveOfferButton_OnClick" CommandName="Update" Text="Save" Font-Size="Large" ForeColor="Green" /></td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td style="width: 200; vertical-align: top" rowspan="2">
                                                <table style="width: 300; border-style: outset; background-color: #ece9d8">
                                                    <tr style="background-color: silver">
                                                        <td align="center" colspan="2" style="color: #000099">
                                                            <strong>Offer Assignment</strong>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="width: 75px; font-size: x-small" align="left">
                                                            Vendor Name:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbVendor" runat="server" Text='<%#Bind("Contractor_Name") %>'
                                                                Width="250" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Solicitation:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlSolicitation" runat="server" Width="250" SelectedValue='<%#Bind("Solicitation_ID") %>' DataSourceID="SolicitationDataSource" DataTextField="Solicitation_Number" DataValueField="Solicitation_ID"><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            CO Name:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlCOName" runat="server" Width="250" DataSourceID="CODataSource" OnDataBound="dlCOName_OnDataBound" DataValueField="CO_ID" DataTextField="FullName" AppendDataBoundItems="true" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Schedule:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlSchedule" runat="server" AppendDataBoundItems="true" Width="250" SelectedValue='<%#Bind("Schedule_Number") %>' DataSourceID="ScheduleDataSource" DataTextField="Schedule_Name" DataValueField="Schedule_Number"><asp:ListItem Text="" /></asp:DropDownList>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Offer Number:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="OfferNumberTextBox" runat="server" Text='<%#Bind("OfferNumber") %>' Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Proposal Type:
                                                        </td>
                                                        <td>
                                                       <asp:DropDownList ID="dlProposalType" runat="server" Width="250" SelectedValue='<%#Bind("Proposal_Type_ID") %>' DataSourceID="ProposalDataSource" DataTextField="Proposal_Type_Description"  DataValueField="Proposal_Type_ID"><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                            
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2">
                                                            <table style="table-layout:fixed;" align="center"  >
                                                                <col width="100px" />
                                                                <col width="100px" />
                                                                <col width="100px" />

                                                                <tr style="font-size: x-small" align="left">
                                                                    <td>
                                                                        Date Received:
                                                                    </td>
                                                                    <td>
                                                                        Assignment Date:
                                                                    </td>
                                                                    <td>
                                                                        Reassignment Date:
                                                                    </td>                                                                
                                                                </tr>
                                                               <tr>
                                                                    <td>
                                                                        <asp:TextBox ReadOnly="true" Enabled="false"  ID="tbReceived" runat="server" OnDataBinding="tbReceived_OnDataBinding" Text='<%#Bind("Dates_Received", "{0:d}") %>'></asp:TextBox>
                                                                    </td>
                                                                    <td>                                                       
                                                                        <asp:DropDownList ID="dlAssignmentDate" runat="server" OnDataBinding="dlAssignmentDate_OnDataBinding"  DataTextFormatString="{0:d}" AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Assigned") %>' DataSourceID="AssignmentDateDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                                    </td>
                                                                     <td>
                                                                        <asp:DropDownList ID="dlReassignmentDate" runat="server" OnDataBinding="dlReassignmentDate_OnDataBinding"  DataTextFormatString="{0:d}" AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Reassigned") %>' DataSourceID="ReassignDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td>
                                                                        <asp:ImageButton ID="OfferReceivedDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer received date" ToolTip="Select offer received date" OnClientClick="OnOReceivedDateButtonClick()" />                                                                
                                                                    </td>   
                                                                    <td>
                                                                        <asp:ImageButton ID="OfferAssignmentDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer assignment date" ToolTip="Select offer assignment date" OnClientClick="OnOAssignmentDateButtonClick()" />                                                                
                                                                    </td>   
                                                                    <td>
                                                                        <asp:ImageButton ID="OfferReassignmentDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer reassignment date" ToolTip="Select offer reassignment date" OnClientClick="OnOReassignmentDateButtonClick()" />                                                                
                                                                    </td>                                        
                                                                </tr>

                                                            </table>
                                                        </td>
                                                    </tr>
                                                 </table>
                                            </td>
                                            <td style="vertical-align: top; width: 200">
                                                <table style="width: 300; border-style: outset; background-color: #ece9d8">
                                                    <tr style="background-color: silver">
                                                        <td align="center" colspan="2" style="color: #000099">
                                                            <strong>Current Status</strong>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="width: 112px; height: 23px; font-size: x-small" align="right">
                                                            Current Action:
                                                        </td>
                                                        <td style="height: 23px">
                                                            <asp:DropDownList ID="dlCurrent" runat="server" AppendDataBoundItems="true" SelectedValue='<%#Bind("Action_ID") %>' DataSourceID="ActionDataSource" DataTextField="Action_Description" DataValueField="Action_ID" OnSelectedIndexChanged="dlCurrent_OnSelectedIndexChanged" AutoPostBack="true" >
                                                                <asp:ListItem Text=""></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr style="font-size: x-small">
                                                        <td>
                                                            Estimated Completion
                                                        </td>
                                                        <td>
                                                            Last Action Completed:
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:DropDownList ID="dlEstCompDate" runat="server" OnDataBinding="dlEstCompDate_OnDataBinding" Width="100" DataTextFormatString="{0:d}"  AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Expected_Completion") %>' DataSourceID="ExpectDateDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>   
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="dlActionCompDate" runat="server" OnDataBinding="dlActionCompDate_OnDataBinding" Width="100" DataTextFormatString="{0:d}"  AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Action") %>' DataSourceID="ActionCompDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:ImageButton ID="OfferEstimatedCompletionDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer estimated completion date" ToolTip="Select offer estimated completion date" OnClientClick="OnOEstimatedCompletionDateButtonClick()" />                                                                
                                                        </td>
                                                        <td>
                                                            <asp:ImageButton ID="OfferActionDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer action date" ToolTip="Select offer action date" OnClientClick="OnOActionDateButtonClick()" />                                                                
                                                        </td>
                                                    
                                                    </tr>
                                                </table>
                                                <table style="font-family: Arial; border-style: outset; background-color: #ece9d8">
                                                    <tr style="background-color: Silver; color: #000099" align="center">
                                                        <td colspan="3">
                                                            Audit History
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Audit Required:
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="cbAuditRequired" runat="server" Checked='<%#Bind("Audit_Indicator") %>' />
                                                        </td>
                                                        <td>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Date of submission<br />
                                                            of the pre-award Audit:
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="dlAuditDate" runat="server" OnDataBinding="dlAuditDate_OnDataBinding" Width="125" DataTextFormatString="{0:d}"  AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Sent_for_Preaward") %>' DataSourceID="AuditDateDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                        </td>
                                                        <td>
                                                            <asp:ImageButton ID="OfferAuditDateImageButton" runat="server"  ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer action date" ToolTip="Select offer action date" OnClientClick="OnOAuditDateButtonClick()" />                                                                
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Date of issuance of
                                                            <br />
                                                            the completed pre-award audit:
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="dlReturnDate" runat="server" OnDataBinding="dlReturnDate_OnDataBinding" Width="125" DataTextFormatString="{0:d}"  AppendDataBoundItems="true" SelectedValue='<%#Bind("Dates_Returned_to_Office") %>' DataSourceID="AuditDateDataSource" DataTextField="Date" DataValueField="Date" ><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                        </td>
                                                        <td>
                                                              <asp:ImageButton ID="OfferReturnDateImageButton" runat="server" ImageUrl="Images/calendar.GIF" AlternateText="calendar to select offer return date" ToolTip="Select offer return date" OnClientClick="OnOReturnDateButtonClick()" />                                                                
                                                      
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td style="width: 50; vertical-align: top">
                                                <asp:Table ID="tblContractInfo" runat="server" BorderStyle="Outset" Visible="false">
                                                    <asp:TableHeaderRow>
                                                        <asp:TableHeaderCell>Awarded Contract:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                    <asp:TableRow>
                                                        <asp:TableCell>
                                                            <asp:Label ID="lbContractNumber" runat="server" Text='<%#Eval("CntrctNum") %>' /></asp:TableCell></asp:TableRow>
                                                    <asp:TableHeaderRow>
                                                        <asp:TableHeaderCell>Current CO:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                    <asp:TableRow>
                                                        <asp:TableCell>
                                                            <asp:Label ID="lbContractAwardDate" runat="server" Text='<%#Eval("Current_CO") %>' /></asp:TableCell></asp:TableRow>
                                                    <asp:TableHeaderRow>
                                                        <asp:TableHeaderCell>Award Date:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                    <asp:TableRow>
                                                        <asp:TableCell>
                                                            <asp:Label ID="Label1" runat="server" Text='<%#Eval("Dates_CntrctAward", "{0:d}") %>' /></asp:TableCell></asp:TableRow>
                                                    <asp:TableHeaderRow>
                                                        <asp:TableHeaderCell>Expiration Date:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                    <asp:TableRow>
                                                        <asp:TableCell>
                                                            <asp:Label ID="Label2" runat="server" Text='<%#Eval("Dates_CntrctExp", "{0:d}") %>' /></asp:TableCell></asp:TableRow>
                                                    <asp:TableHeaderRow>
                                                        <asp:TableHeaderCell>Contract Status:</asp:TableHeaderCell></asp:TableHeaderRow>
                                                    <asp:TableRow>
                                                        <asp:TableCell>
                                                            <asp:Label ID="lbContractStatus" runat="server" Text='<%#ContractStatus(Eval("Dates_CntrctExp"),Eval("Dates_Completion")) %>' /></asp:TableCell></asp:TableRow>
                                                </asp:Table>
                                                <asp:Button ID="OpenOrCreateContractButton" runat="server" ForeColor="Green" Width="115px" Height="44px" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                            <td style="vertical-align: top">
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td valign="top">
                                                <table style="width: 300; border-style: outset; background-color: #ECE9D8">
                                                    <tr style="background-color: silver">
                                                        <td align="center" colspan="2" style="color: #000099">
                                                            <strong>Vendor Point of Contact</strong>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            Name:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbName" runat="server" Text='<%#Bind("POC_Primary_Name") %>'
                                                                Width="300" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            Phone:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbPhone" runat="server" Text='<%#Bind("POC_Primary_Phone") %>'
                                                                Width="150" />
                                                        
                                                            <span  style="font-size: x-small; text-align:  inherit;  vertical-align:text-top " >Ext:</span>
                                                        
                                                            <asp:TextBox  ID="tbPhoneExtension" runat="server" Text='<%#Bind("POC_Primary_Ext") %>'
                                                                Width="80" />
                                                        </td>
                                                   </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            Fax:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbFax" runat="server" Text='<%#Bind("POC_Primary_Fax") %>'
                                                                Width="300" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            email:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbemail" runat="server" Text='<%#Bind("POC_Primary_Email") %>'
                                                                Width="300" />
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
                                            <td colspan="2">
                                                <table style="width: 300; border-style: outset; background-color: #ECE9D8">
                                                    <tr style="background-color: silver">
                                                        <td align="center" colspan="2" style="color: #000099">
                                                            <strong>Vendor Mailing Address/Web Address</strong>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small;">
                                                            Address 1:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbAddress1" runat="server" Text='<%#Bind("Primary_Address_1") %>'
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Address 2:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbAddress2" runat="server" Text='<%#Bind("Primary_Address_2") %>'
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            City
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbCity" runat="server" Text='<%#Bind("Primary_City") %>'
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            State
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlState" runat="server" Width="75px" SelectedValue='<%#Bind("Primary_State") %>' DataSourceID="StateDataSource" DataTextField="abbr" DataValueField="abbr" AppendDataBoundItems="true" OnDataBinding="CheckDate"><asp:ListItem Text="" Value="" /></asp:DropDownList>
                                                           
                                                           <asp:TextBox  ID="tbZip" runat="server" Width="75px" Text='<%#Bind("Primary_Zip") %>'></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Company Web Page
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbWebPage" runat="server" Text='<%#Bind("POC_VendorWeb") %>'
                                                                Width="200" />
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
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td colspan="2">
                                                <table style="width: 600; border-style: outset">
                                                    <tr style="background-color: silver">
                                                        <td align="left" colspan="2" style="color: #000099">
                                                            <strong>Comments</strong>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:TextBox  ID="tbComments" runat="server" Width="700px" Text='<%#Bind("Comments") %>'
                                                                TextMode="MultiLine" Rows="5" />
                                                        </td>
                                                    </tr>
                                                </table>
                                                <asp:HiddenField ID="hfContractNum" runat="server" Value='<%#Eval("CntrctNum") %>' />
                                                <asp:HiddenField ID="hfScheduleNumber" runat="server" Value='<%#Eval("Schedule_Number") %>' />
                                                <asp:HiddenField ID="hfActionID" runat="server" Value='<%#Eval("Action_ID") %>' />
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
                                            <td>
                                            </td>
                                        </tr>
                                    </table>
                                    <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined" />
                                    <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False" />
                                </EditItemTemplate>
                                <ItemTemplate>
                                   
                                </ItemTemplate>

                            </asp:FormView>
                            <asp:SqlDataSource ID="OfferDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" OnUpdating="OfferDataSource_OnUpdating"


                                SelectCommand="SELECT Offer_ID, Solicitation_ID, CO_ID, Schedule_Number, OfferNumber, Proposal_Type_ID, Action_ID, Contractor_Name, Primary_Address_1, Primary_Address_2, Primary_City, Primary_State, Primary_Zip, Country, POC_Primary_Name, POC_Primary_Phone, POC_Primary_Ext, POC_Primary_Fax, POC_Primary_Email, POC_VendorWeb, Dates_Received, Dates_Assigned, Dates_Reassigned, CONVERT (varchar(10), Dates_Action, 101) AS Dates_Action, Dates_Expected_Completion, Dates_Expiration, Dates_Sent_for_Preaward, Dates_Returned_to_Office, Comments, Audit_Indicator, FullName, CO_User, AD_User, SM_User, Logon_Name, Contract_Record_ID, CntrctNum, Current_CO, Dates_CntrctAward, Dates_CntrctExp, Dates_Completion FROM dbo.view_Offers_Full WHERE (Offer_ID = @Offer_ID)" 
                                UpdateCommand="UPDATE dbo.tbl_Offers SET Solicitation_ID =@Solicitation_ID, CO_ID =@CO_ID, Schedule_Number =@Schedule_Number, OfferNumber = @OfferNumber, Proposal_Type_ID =@Proposal_Type_ID, Action_ID =@Action_ID, Contractor_Name =@Contractor_Name, Primary_Address_1 =@Primary_Address_1, Primary_Address_2 =@Primary_Address_2, Primary_City =@Primary_City, Primary_State =@Primary_State, Primary_Zip =@Primary_Zip, Country =@Country, POC_Primary_Name =@POC_Primary_Name, POC_Primary_Phone =@POC_Primary_Phone, POC_Primary_Ext =@POC_Primary_Ext, POC_Primary_Fax =@POC_Primary_Fax, POC_Primary_Email =@POC_Primary_Email, POC_VendorWeb =@POC_VendorWeb, Dates_Received =@Dates_Received, Dates_Assigned =@Dates_Assigned, Dates_Reassigned =@Dates_Reassigned, Dates_Action =@Dates_Action, Dates_Expected_Completion =@Dates_Expected_Completion, Dates_Expiration =@Dates_Expiration, Dates_Sent_for_Preaward =@Dates_Sent_for_Preaward, Dates_Returned_to_Office =@Dates_Returned_to_Office, Comments =@Comments, Audit_Indicator =@Audit_Indicator, ContractNumber =@ContractNumber, Date_Modified =@Date_Modified, LastModifiedBy=@LastModifiedBy WHERE Offer_ID = @Offer_ID">
                                <SelectParameters>
                                    <asp:QueryStringParameter Name="Offer_ID" QueryStringField="OfferID" Type="Int32" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="Solicitation_ID" Type="Int32" />
                                    <asp:Parameter Name="CO_ID" Type="Int32" />
                                    <asp:Parameter Name="Schedule_Number" Type="Int32"/>
                                    <asp:Parameter Name="OfferNumber" />
                                    <asp:Parameter Name="Proposal_Type_ID" Type="Int32"/>
                                    <asp:Parameter Name="Action_ID" Type="Int32"/>
                                    <asp:Parameter Name="Contractor_Name" />
                                    <asp:Parameter Name="Primary_Address_1" />
                                    <asp:Parameter Name="Primary_Address_2" />
                                    <asp:Parameter Name="Primary_City" />
                                    <asp:Parameter Name="Primary_State" />
                                    <asp:Parameter Name="Primary_Zip" />
                                    <asp:Parameter Name="Country" />
                                    <asp:Parameter Name="POC_Primary_Name" />
                                    <asp:Parameter Name="POC_Primary_Phone" />
                                    <asp:Parameter Name="POC_Primary_Ext" />
                                    <asp:Parameter Name="POC_Primary_Fax" />
                                    <asp:Parameter Name="POC_Primary_Email" />
                                    <asp:Parameter Name="POC_VendorWeb" />
                                    <asp:Parameter Name="Dates_Received" Type="DateTime" />
                                    <asp:Parameter Name="Dates_Assigned" Type="DateTime" />
                                    <asp:Parameter Name="Dates_Reassigned" Type="DateTime"/>
                                    <asp:Parameter Name="Dates_Action" Type="DateTime"/>
                                    <asp:Parameter Name="Dates_Expected_Completion" Type="DateTime" />
                                    <asp:Parameter Name="Dates_Expiration" Type="DateTime"/>
                                    <asp:Parameter Name="Dates_Sent_for_Preaward" Type="DateTime"/>
                                    <asp:Parameter Name="Dates_Returned_to_Office" Type="DateTime"/>
                                    <asp:Parameter Name="Comments" />
                                    <asp:Parameter Name="Audit_Indicator" />
                                    <asp:Parameter Name="ContractNumber" />
                                    <asp:Parameter Name="Date_Modified" />
                                    <asp:Parameter Name="Offer_ID" />
                                    <asp:Parameter Name="LastModifiedBy" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ExpectDateDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() }) AND (Date <= { fn NOW() } + 90) ORDER BY Date">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="AuditDateDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 7) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ActionCompDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT CONVERT(varchar(10),Date,101) as Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 14) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="CODataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>" OnSelecting="CODataSource_OnSelecting"
                                SelectCommand="exec [NACSEC].[dbo].SelectContractingOfficers3 @DivisionId, @SelectFlag, @OrderByLastNameFullName, @IsExpired ">
                            <SelectParameters>
                                <asp:Parameter Name="DivisionId" Type="Int32" />
                                <asp:Parameter Name="SelectFlag" Type="Int32" />
                                <asp:Parameter Name="OrderByLastNameFullName" Type="String" />
                                <asp:Parameter Name="IsExpired" Type="Boolean" />
                            </SelectParameters>
                            </asp:SqlDataSource>
                             <asp:SqlDataSource ID="AssignmentDateDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 90) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ReassignDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 14) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ProposalDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT tlkup_Offers_Proposal_Type.Proposal_Type_ID, tlkup_Offers_Proposal_Type.Proposal_Type_Description FROM tlkup_Offers_Proposal_Type ORDER BY Proposal_Type_ID">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ScheduleDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Schedule_Number, Schedule_Name FROM [tlkup_Sched/Cat] WHERE (Inactive = 0) AND Division = 1 ORDER BY Schedule_Name">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ActionDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT tlkup_Offers_Action_Type.Action_ID, tlkup_Offers_Action_Type.Action_Description, tlkup_Offers_Action_Type.Complete FROM tlkup_Offers_Action_Type ORDER BY Action_Description">
                            </asp:SqlDataSource>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:SqlDataSource ID="SolicitationDataSource" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                SelectCommand="SELECT Solicitation_ID, Solicitation_Number FROM tlkup_Solicitation_Numbers ORDER BY Solicitation_Number">
                            </asp:SqlDataSource>
                        </td>
                        <td style="width: 204px">
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" colspan="3">
                        </td>
                    </tr>
                </table>
            </td>
            <td>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
