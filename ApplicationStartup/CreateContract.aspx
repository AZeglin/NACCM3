<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateContract.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.CreateContract" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html />
<html xmlns="http://www.w3.org/1999/xhtml" >

<head runat="server">
   <meta http-equiv="X-UA-Compatible" content="IE=7"> 
    <title></title>
    <link href="ApplicationStartupDetails.css" rel="stylesheet" type="text/css" />

</head>
<body style="background-color:#ece9d8;">
    <form id="CreateContractForm" runat="server"   >
      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="CreateContractScriptManager_OnAsyncPostBackError" >
    </asp:ScriptManager>

    <script type="text/javascript" >
    
        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest( beginRequest );
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded( pageLoaded );

            
        var postbackElement;
        
        function beginRequest( sender, args )
        {
            postbackElement = args.get_postBackElement();
        }
  
        function pageLoaded( sender, args )
        {
            if( typeof( postbackElement ) == "undefined" )
            {
                return;
            }

            if (postbackElement.id.toLowerCase().indexOf('fred') > -1) 
            {
                return;
            }

        }
 
        function openContractDetailsWindow(rowIndex, contractNumber, scheduleNumber) 
        {
            unHighlightContractRow();
            $get("highlightedContractRow").value = rowIndex;
            highlightContractRow();
            var w = window.open("NAC_CM_Contracts.aspx?CntrctNum=" + contractNumber + "&SchNum=" + scheduleNumber, "ContractDetails", "toolbar=0,status=0,menubar=0,scrollbars=1,resizable=1,top=80,left=170,width=910,height=730", "true");
            w.focus();
        }     
        
        function presentConfirmationMessage( msg )
        {
            $get("confirmationMessageResults").value = confirm( msg ); 
        }
        
        function presentPromptMessage( msg )
        {
            $get("promptMessageResults").value = prompt( msg, "" );
        }

    </script>
    <asp:Panel ID="CreateContractPanel" runat="server" BackColor="Brown" Width="100%" >
       <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8; border-style:none; " >
            <col style="width:2%;" />
            <col style="width:16%;" />
            <col style="width:16%;" />
            <col style="width:16%;" />
            <col style="width:16%;" />
            <col style="width:16%;" />
            <col style="width:16%;" />
            <col style="width:2%;" />
            <tr  style="vertical-align:top;"  >
                <td colspan="8" style="text-align:center;" >
                    <asp:Label ID="CreateContractTitleLabel" Text="Contract Addition" runat="server" CssClass="PageHeaderText" ></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td align="right" >
                    <asp:Label ID="DivisionLabel" Text="Division" runat="server" CssClass="FieldLabelText" ></asp:Label>
                </td>
                <td >
                    <asp:DropDownList ID="DivisionDropDownList" runat="server" Width="98%" OnSelectedIndexChanged="DivisionDropDownList_OnSelectedIndexChanged"  EnableViewState="true" AutoPostBack="true"  ></asp:DropDownList>
                </td>
            </tr>
           <tr>
                <td>
                </td>
                <td align="right" >
                    <asp:Label ID="ScheduleLabel" Text="Schedule or Program" runat="server" CssClass="FieldLabelText" ></asp:Label>
                </td>
                <td colspan="3">
                    <asp:DropDownList ID="ScheduleDropDownList" runat="server" Width="98%" OnSelectedIndexChanged="ScheduleDropDownList_OnSelectedIndexChanged" EnableViewState="true" AutoPostBack="true" ></asp:DropDownList>
                </td>
            </tr>

            <tr>
                <td>
                </td>
                <td align="right" >
                    <asp:Label ID="ContractingOfficerLabel" Text="Contracting Officer" runat="server" CssClass="FieldLabelText" ></asp:Label>
                </td>
                <td colspan="3">
                    <asp:DropDownList ID="ContractingOfficerDropDownList" runat="server" Width="98%" ></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td align="right" >
                    <asp:Label ID="ContractNumberLabel" Text="Contract Number" runat="server" CssClass="FieldLabelText" ></asp:Label>
                </td>
                <td>
                    <ep:TextBox ID="ContractNumberTextBox" runat="server" Width="94%" MaxLength="50" ></ep:TextBox>
                </td>
            </tr>
            <tr>
	            <td>
                </td>
                <td align="right" >
                     <asp:Label ID="AwardDateLabel" Text="Award Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                </td>
                <td style="HEIGHT: 26px">
                 <ep:TextBox ID="AwardDateTextBox" runat="server" Width="75%"></ep:TextBox>
                    <img src="Images/calendar.GIF" alt="calendar to select award date" onclick="OnAwardDateButtonClick()" />    
                </td>
                <td align="right" >
                     <asp:Label ID="EffectiveDateLabel" Text="Effective Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                </td>
				<td style="HEIGHT: 26px">
                    <ep:TextBox ID="EffectiveDateTextBox" runat="server" Width="75%"></ep:TextBox>
                    <img src="Images/calendar.GIF" alt="calendar to select effective date" onclick="OnEffectiveDateButtonClick()" />
                </td>
                <td align="right" >
                     <asp:Label ID="ExpirationDateLabel" Text="Expiration Date" runat="server" CssClass="FieldLabelText" ></asp:Label>                
                </td>
				<td style="HEIGHT: 26px">
                    <ep:TextBox ID="ExpirationDateTextBox" runat="server" Width="75%"></ep:TextBox>
                    <img src="Images/calendar.GIF" alt="calendar to select expiration date" onclick="OnExpirationDateButtonClick()" />
                </td>

            </tr>
            <tr>
                <td>
                </td>
                <td align="right" >
                    <asp:Label ID="VendorNameLabel" Text="Vendor Name" runat="server" CssClass="FieldLabelText" ></asp:Label>
                </td>
                <td colspan="4" >
                    <ep:TextBox ID="VendorNameTextBox" runat="server"  Width="98%" MaxLength="75"></ep:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                </td>
                 <td>
                </td>
                <td colspan="2" >
                    <asp:RadioButtonList ID="RebateRequiredRadioButtonList" CellSpacing="1" CellPadding="1" runat="server" Width="99%" CssClass="FieldLabelText" EnableViewState="true" RepeatDirection="Horizontal" >
                        <asp:ListItem Value="1" >Rebate Required</asp:ListItem>
                        <asp:ListItem Value="0" >Rebate Not Required</asp:ListItem>
                    </asp:RadioButtonList>               
                 </td>      
                <td>
                </td>
                 <td>
                </td>                                        
            </tr>
            <tr>
                <td colspan="8"   style="border-top: solid 1px black; text-align:center; " >
                    <asp:Label ID="VendorDetailsTitleLabel" Text="Vendor Details" runat="server" CssClass="HeaderText" ></asp:Label>
                </td>               
            </tr>          
            <tr>
                <td rowspan="6" colspan="4"  style="vertical-align:top;"  >
                   <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8;  border-style:none;" >
                        <col style="width:2%;" />       
                        <col style="width:10%;" />       
                        <col style="width:50%;" />       
                        <col style="width:6%;" />       
                        <col style="width:20%;" />       
                        <col style="width:2%;" />       
                        <tr>
                            <td colspan="6" style="text-align:center;">
                                <asp:Label ID="PointOfContactTitleLabel" Text="Point Of Contact" runat="server" CssClass="UnderHeaderText" ></asp:Label> 
                            </td>
                        </tr>                                     
                        <tr>
                            <td>
                            </td>
                            <td align="right" >
                               <asp:Label ID="NameLabel" Text="Name" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                            </td>
                            <td colspan="3">
                                <ep:TextBox ID="PointOfContactNameTextBox" runat="server"  Width="98%" MaxLength="30"></ep:TextBox>
                            </td>
                             <td>
                            </td>
                       </tr>                                  
                        <tr>
                            <td>
                            </td>
                            <td align="right" >
                               <asp:Label ID="PhoneLabel" Text="Phone" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                            </td>
                            <td>
                                <ep:TextBox ID="PointOfContactPhoneTextBox" runat="server"  Width="98%" MaxLength="15"></ep:TextBox>
                            </td>
                            <td>
                               <asp:Label ID="PhoneExtensionLabel" Text="Ext" runat="server" CssClass="FieldLabelText" ></asp:Label> 
                            </td>
                            <td>
                                <ep:TextBox ID="PointOfContactPhoneExtensionTextBox" runat="server"  Width="90%" MaxLength="5"></ep:TextBox>
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
                                <ep:TextBox ID="PointOfContactFaxTextBox" runat="server"  Width="98%" MaxLength="15"></ep:TextBox>
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
                                <ep:TextBox ID="PointOfContactEmailTextBox" runat="server"  Width="98%" MaxLength="50"></ep:TextBox>
                            </td>
                              <td>
                            </td>
                      </tr>  
                        <tr>
                        </tr>
                    </table>
                </td>         
                <td rowspan="6" colspan="4" >
                    <table  width="100%" style="height: 100%; table-layout:fixed; background-color:#ece9d8; border-style:none; " >
                        <col style="width:2%;" />       
                        <col style="width:22%;" />       
                        <col style="width:50%;" />       
                        <col style="width:6%;" />       
                        <col style="width:18%;" />       
                        <col style="width:2%;" />       
                        <tr>
                            <td colspan="6" style="text-align:center;">
                                <asp:Label ID="VendorAddressTitleLabel" Text="Vendor Address" runat="server" CssClass="UnderHeaderText" ></asp:Label> 
                            </td>
                        </tr>         
                        <tr>
                            <td>
                            </td>
                            <td align="right" >
                               <asp:Label ID="Address1Label" Text="Address 1" runat="server" CssClass="FieldLabelText"></asp:Label> 
                            </td>
                            <td colspan="3">
                                <ep:TextBox ID="Address1TextBox" runat="server"  Width="98%" MaxLength="100"></ep:TextBox>
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
                                <ep:TextBox ID="Address2TextBox" runat="server"  Width="98%" MaxLength="100"></ep:TextBox>
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
                                <ep:TextBox ID="CityTextBox" runat="server"  Width="98%" MaxLength="20"></ep:TextBox>
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
                               <asp:DropDownList ID="StateDropDownList" runat="server" Width="24%" EnableViewState="true" AutoPostBack="true"  ></asp:DropDownList>
                            </td>                       
                            <td>
                               <asp:Label ID="ZipLabel" Text="Zip" runat="server" CssClass="FieldLabelText"></asp:Label> 
                            </td>
                            <td>
                                <ep:TextBox ID="ZipTextBox" runat="server"  Width="88%" MaxLength="10"></ep:TextBox>
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
                                <ep:TextBox ID="CompanyUrlTextBox" runat="server"  Width="98%"></ep:TextBox>
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
                </td>            
            </tr>  
            <tr>
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
                <td>
                </td>
                <td align="right">
                    <asp:Button ID="ContractAdditionCancelButton" Text="Cancel" runat="server" OnClick="ContractAdditionCancelButton_OnClick" CausesValidation="false" /> 
                </td>   
                <td align="right">
                    <asp:Button ID="ContractAdditionSaveButton" Text="Create Contract" runat="server" OnClick="ContractAdditionSaveButton_OnClick" CausesValidation="true" ValidationGroup="CreateContractRequiredFields" /> 
                </td>          
                <td>
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td colspan="4">
                    <asp:ValidationSummary ID="CreateContractRequiredFieldsValidationSummary" runat="server" ValidationGroup="CreateContractRequiredFields"  ShowMessageBox="true"  DisplayMode="BulletList"  ShowSummary="false"  HeaderText="Please provide the following required fields:&#xA; " />    
                </td>
            </tr>
         </table>
         <ep:MsgBox ID="MsgBox" NameofMsgBox="CreateContract" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />
       </asp:Panel>
       <asp:HiddenField ID="RefreshDateValueOnSubmit" runat="server" Value="Undefined" />
       <asp:HiddenField ID="RefreshOrNotOnSubmit" runat="server" Value="False" />
        <asp:RequiredFieldValidator ID="DivisionFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate ="DivisionDropDownList" SetFocusOnError="true"  ErrorMessage="Division"  ForeColor="#FFFFFF" Display="None" InitialValue="-1" />
        <asp:RequiredFieldValidator ID="ScheduleFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate ="ScheduleDropDownList" SetFocusOnError="true" ErrorMessage="Schedule" ForeColor="#FFFFFF" Display="None" InitialValue="-1" />
        <asp:RequiredFieldValidator ID="ContractingOfficerFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="ContractingOfficerDropDownList" SetFocusOnError="true" ErrorMessage="Contracting Officer" ForeColor="#FFFFFF" Display="None" InitialValue="-1" />
        <asp:RequiredFieldValidator ID="ContractNumberFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="ContractNumberTextBox" SetFocusOnError="true" ErrorMessage="Contract Number" ForeColor="#FFFFFF" Display="None" />
        <asp:RequiredFieldValidator ID="AwardDateFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="AwardDateTextBox" SetFocusOnError="true" ErrorMessage="Award Date" ForeColor="#FFFFFF" Display="None" />
        <asp:RequiredFieldValidator ID="EffectiveDateFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="EffectiveDateTextBox" SetFocusOnError="true" ErrorMessage="Effective Date" ForeColor="#FFFFFF" Display="None" />
        <asp:RequiredFieldValidator ID="ExpirationDateFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="ExpirationDateTextBox" SetFocusOnError="true" ErrorMessage="Expiration Date"  ForeColor="#FFFFFF" Display="None" />
        <asp:RequiredFieldValidator ID="VendorNameFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="VendorNameTextBox" SetFocusOnError="true" ErrorMessage="Vendor name" ForeColor="#FFFFFF" Display="None"  />
        <asp:RequiredFieldValidator ID="RebateRequiredFieldValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="RebateRequiredRadioButtonList" SetFocusOnError="true" ErrorMessage="Rebate Required/Not Required" ForeColor="#FFFFFF" Display="None"  />
        <asp:CustomValidator ID="ContractNumberCustomValidator" runat="server" ValidationGroup="CreateContractRequiredFields" ControlToValidate="ContractNumberTextBox" SetFocusOnError="true" OnServerValidate="ContractNumberCustomValidator_OnServerValidate"  ForeColor="#FFFFFF"  />
    </form>            
</body>            
