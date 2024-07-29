<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="offer_addtion.aspx.vb" Inherits="NACCM.offer_addtion" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Offer Addtion</title>
</head>
<body style="background-color:#ECE9D8">
    <form id="form1" runat="server">
    <asp:RequiredFieldValidator ID="requiredField1" runat="server" ControlToValidate="tbVendor" ValidationGroup="AllRequiredFields" ErrorMessage="Vendor is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="requiredField2" runat="server" ControlToValidate="dlSolicitation" ValidationGroup="AllRequiredFields" ErrorMessage="Solicitation is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="requiredField3" runat="server" ControlToValidate="dlCOName" ValidationGroup="AllRequiredFields" ErrorMessage="Contracting Officer is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="requiredField4" runat="server" ControlToValidate="dlSchedule" ValidationGroup="AllRequiredFields" ErrorMessage="Schedule is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="requiredField5" runat="server" ControlToValidate="dlReceivedDate" ValidationGroup="AllRequiredFields" ErrorMessage="Received date is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="requireField2" runat="server" ControlToValidate="dlProposalType" ValidationGroup="AllRequiredFields" ErrorMessage="Proposal Type is required." ForeColor="#ECE9D8" />
    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="dlCurrent" ValidationGroup="AllRequiredFields" ErrorMessage="Current Action is required." ForeColor="#ECE9D8" />
     <asp:CustomValidator ID="DateComparisonValidator" runat="server" ControlToValidate="dlAssignmentDate" ValidationGroup="AllRequiredFields" ErrorMessage="Assignment date must not be before received date." ForeColor="#ECE9D8" />
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
                            <asp:Button ID="btnOfferSearc" runat="server" Text="Offer Search" Visible="false" />
                            <asp:Button ID="btnMainMenu" runat="server" Text="Main Menu" Visible="false" />
                            <asp:Button ID="btnExit" runat="server" Text="Exit NAC CM" Visible="false"/>
                        </td>
                    </tr>
                    <tr valign="top">
                        <td valign="top" align="left" colspan="3">
                           
                                    <table style="height: 500; width: 700; background-color: #9baeb9">
                                        <tr style="height: 30">
                                            <td style="width: 25; height: 25">
                                            </td>
                                            <td align="left" colspan="3" style="color: #000099">
                                                <strong>New Offer Record</strong>
                                            </td>
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
                                                            <asp:TextBox  ID="tbVendor" runat="server" 
                                                                Width="250" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Solicitation:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlSolicitation" runat="server" Width="250px" 
                                                                DataSourceID="SolicitationDataSource" DataTextField="Solicitation_Number" 
                                                                DataValueField="Solicitation_ID" AppendDataBoundItems="true"><asp:ListItem Text="" /></asp:DropDownList>
                                                           
                                                            <asp:SqlDataSource ID="SolicitationDataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT tlkup_Solicitation_Numbers.Solicitation_ID, tlkup_Solicitation_Numbers.Solicitation_Number FROM tlkup_Solicitation_Numbers ORDER BY Solicitation_Number">
                                                            </asp:SqlDataSource>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            CO Name:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlCOName" runat="server" Width="250px" 
                                                                DataSourceID="CODataSource" DataTextField="FullName" DataValueField="CO_ID" AppendDataBoundItems="true"><asp:ListItem Text="" Selected="True" /></asp:DropDownList>
                                                           
                                                            <asp:SqlDataSource ID="CODataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="exec [NACSEC].[dbo].SelectContractingOfficers3 1, 0, 'F' ">
                                                            </asp:SqlDataSource>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Schedule:
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlSchedule" runat="server" AppendDataBoundItems="True" 
                                                                Width="250px" DataSourceID="ScheduleDataSource" DataTextField="Schedule_Name" 
                                                                DataValueField="Schedule_Number"><asp:ListItem Text="" /></asp:DropDownList>
                                                           
                                                            <asp:SqlDataSource ID="ScheduleDataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT Schedule_Number, Schedule_Name FROM [tlkup_Sched/Cat] WHERE (Inactive = 0) AND (Division = 1) ORDER BY Schedule_Name ">
                                                            </asp:SqlDataSource>
                                                           
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Offer Number:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="OfferNumberTextBox" runat="server" Text='<%#Eval("OfferNumber") %>' Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small" align="left">
                                                            Proposal Type:
                                                        </td>
                                                        <td>
                                                       <asp:DropDownList ID="dlProposalType" runat="server" Width="250px" 
                                                                DataSourceID="ProposalDataSource" DataTextField="Proposal_Type_Description" 
                                                                DataValueField="Proposal_Type_ID" AppendDataBoundItems="true"><asp:ListItem Text="" /></asp:DropDownList>
                                                            
                                                            <asp:SqlDataSource ID="ProposalDataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT tlkup_Offers_Proposal_Type.Proposal_Type_ID, tlkup_Offers_Proposal_Type.Proposal_Type_Description FROM tlkup_Offers_Proposal_Type ORDER BY Proposal_Type_ID">
                                                            </asp:SqlDataSource>
                                                            
                                                        </td>
                                                    </tr>
                                                    <tr style="font-size: x-small" align="left">
                                                        <td>
                                                            Received Date:
                                                        </td>
                                                     <td><asp:DropDownList ID="dlReceivedDate" runat="server" 
                                                            DataTextFormatString="{0:d}" AppendDataBoundItems="True" 
                                                            DataSourceID="ReceivedDateDataSource" DataTextField="Date" DataValueField="Date"><asp:ListItem Text="" /></asp:DropDownList>
                                                        <asp:SqlDataSource ID="ReceivedDateDataSource" runat="server" 
                                                            ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                            SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date &gt;= { fn NOW() } - 90) AND (Date &lt;= { fn NOW() }) ORDER BY Date DESC">
                                                        </asp:SqlDataSource>
                                                        </td></tr>
                                                    <tr style="font-size: x-small" align="left">
                                                        <td>
                                                            Assignment Date:
                                                        </td>
                                                     <td><asp:DropDownList ID="dlAssignmentDate" runat="server" 
                                                            DataTextFormatString="{0:d}" AppendDataBoundItems="True" 
                                                            DataSourceID="AssignDataSource" DataTextField="Date" DataValueField="Date"><asp:ListItem Text="" /></asp:DropDownList>
                                                        <asp:SqlDataSource ID="AssignDataSource" runat="server" 
                                                            ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                            SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date &gt;= { fn NOW() } - 90) AND (Date &lt;= { fn NOW() }) ORDER BY Date DESC">
                                                        </asp:SqlDataSource>
                                                        </td></tr>
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
                                                            <asp:DropDownList ID="dlCurrent" runat="server" AppendDataBoundItems="True" 
                                                                DataSourceID="StatusdataSource" DataTextField="Action_Description" 
                                                                DataValueField="Action_ID">
                                                                <asp:ListItem Text=""></asp:ListItem>
                                                            </asp:DropDownList>
                                                            <asp:SqlDataSource ID="StatusdataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT Action_ID, Action_Description FROM tlkup_Offers_Action_Type WHERE (Action_ID = 1 OR Action_ID = 2 OR Action_ID = 6 OR Action_ID = 7 OR Action_ID = 8 OR Action_ID = 11 OR Action_ID = 16 ) ORDER BY Action_Description">
                                                            </asp:SqlDataSource>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="width: 112px; height: 23px; font-size: x-small" align="right">
                                                            Estimated Completion
                                                        </td>
                                                       <td>
                                                        <asp:DropDownList ID="dlEstCompDate" runat="server" Width="100px" 
                                                                DataTextFormatString="{0:d}"  AppendDataBoundItems="True" 
                                                                DataSourceID="CompleteDataSource" DataTextField="Date" DataValueField="Date"><asp:ListItem Text="" /></asp:DropDownList>
                                                           
                                                            <asp:SqlDataSource ID="CompleteDataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date &gt;= { fn NOW() }) AND (Date &lt;= { fn NOW() } + 90) ORDER BY Date">
                                                            </asp:SqlDataSource>
                                                           
                                                        </td>
                                                       
                                                    </tr>
                                                </table>
                                               
                                            </td>
                                            <td style="width: 50; vertical-align: top">
                                                
                                                <asp:Button ID="btnAddAward" runat="server" ForeColor="Green" Width="115px" Height="44px"
                                                    ValidationGroup="AllRequiredFields"  /><br />
                                                <asp:Button ID="btnOpenContract" runat="server" ForeColor="Green" Height="54px" Width="115px"/>
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
                                                            <asp:TextBox  ID="tbName" runat="server" 
                                                                Width="300" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            Phone:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbPhone" runat="server" Width="150" />
                                                            
                                                            <span  style="font-size: x-small; text-align:  inherit;  vertical-align:text-top " >Ext:</span>
                                                        
                                                            <asp:TextBox  ID="tbPhoneExtension" runat="server" Width="80" />
                                                        </td>
                                                   </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            Fax:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbFax" runat="server" 
                                                                Width="300" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" style="font-size: x-small">
                                                            email:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbemail" runat="server" 
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
                                                            <asp:TextBox  ID="tbAddress1" runat="server" 
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Address 2:
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbAddress2" runat="server" 
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            City
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbCity" runat="server" 
                                                                Width="200" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            State
                                                        </td>
                                                        <td>
                                                        <asp:DropDownList ID="dlState" runat="server" AppendDataBoundItems="True" 
                                                                DataSourceID="statedataSource" DataTextField="Abbr" DataValueField="Abbr"><asp:ListItem Text="" /></asp:DropDownList>
                                                            
                                                            <asp:SqlDataSource ID="statedataSource" runat="server" 
                                                                ConnectionString="<%$ ConnectionStrings:CM %>" 
                                                                SelectCommand="SELECT [Abbr], [State/Province], [Country] FROM tlkup_StateAbbr GROUP BY [Abbr], [State/Province], [Country] ORDER BY [State/Province]; ">
                                                            </asp:SqlDataSource>
                                                            
                                                            <asp:TextBox  ID="tbZip" runat="server" Width="75px"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="font-size: x-small">
                                                            Company Web Page
                                                        </td>
                                                        <td>
                                                            <asp:TextBox  ID="tbWebPage" runat="server" 
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
                                                            <asp:TextBox  ID="tbComments" runat="server" Width="700px" Text='<%#Eval("Comments") %>'
                                                                TextMode="MultiLine" Rows="5" />
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
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                            <td>
                                            </td>
                                        </tr>
                                    </table>
                            <asp:ValidationSummary ID="VSPage" runat="server" ShowMessageBox="true" ValidationGroup="AllRequiredFields" ForeColor="#ECE9D8" />
                        </td>
                    </tr>
                    <tr>
                        <td>
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
