<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" CodeBehind="EditSBAPlanDetails.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.EditSBAPlanDetails" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %> 

<!DOCTYPE html>
<html>


<head runat="server">
    <title></title>

     <script type="text/javascript" >

         function CloseWindow(withRefresh,sbaIdHasChanged) {
             if (withRefresh == "true") {
                 window.opener.document.forms[0].RefreshSBADetailsOnSubmit.value = withRefresh;
                 window.opener.document.forms[0].ChangeSelectedSBAIdOnSubmit.value = sbaIdHasChanged;
                 window.opener.document.forms[0].submit();
             }
             top.window.opener = top;
             top.window.open('', '_parent', '');
             top.window.close();
         }
 
         function presentConfirmationMessage(msg) {
             $get("confirmationMessageResults").value = confirm(msg);
         }

         function presentPromptMessage(msg) {
             $get("promptMessageResults").value = prompt(msg, "");
         }
         
</script>


</head>
<body>
    <form id="EditSBAPlanDetailsForm" runat="server">

    <asp:ScriptManager ID="EditSBAPlanDetailsScriptManager" runat="server" EnablePartialRendering="true" LoadScriptsBeforeUI="true"   OnAsyncPostBackError="EditSBAPlanDetailsScriptManager_OnAsyncPostBackError" >
        
    </asp:ScriptManager>

        <asp:Panel ID="EditSBAPlanDetailsPanel" runat="server"  Width="100%" Height="100%" CssClass="EditSBAPanel" OnPreRender="EditSBAPlanDetailsPanel_OnPreRender" >
    
            <table class="OuterTable"  style="width: 460px; height:100%; table-layout:fixed; ">
                 <tr>
                    <td colspan="3" style="width:100%;" >
                        <table style="table-layout:fixed; border-collapse:collapse; border-spacing:0px;" >                      
                            <tr class="OutsetBoxHeaderRow"  >
                                <td style="width:160px;" ></td>

                                <td  style="text-align:center; width:26%;" >
                                    <asp:Label ID="SBAPlanDetailsFormViewHeaderLabel" runat="server" Text="Plan Details" CssClass="HeaderText"   />
                                </td>
              
                                <td style="padding-right:12px; width:160px; text-align:right;" >
                                    <asp:Button ID="EditSBAPlanDetailsCancelButton" Text="Cancel" runat="server" OnClick="EditSBAPlanDetailsCancelButton_OnClick" CausesValidation="false" /> 
                        
                                    <asp:Button ID="EditSBAPlanDetailsSaveButton" Text="Save" runat="server" OnClick="EditSBAPlanDetailsSaveButton_OnClick" CausesValidation="false" /> 
                                </td>      
                           </tr>  
                        </table>
                    </td>
                   </tr>
                   <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="PlanNameLabel" runat="server" />
                        </td>
                        <td colspan="2">
                            <asp:DropDownList ID="ActivePlansDropDownList" Width="220px" runat="server" EnableViewState="true" AutoPostBack="true"  OnSelectedIndexChanged="ActivePlansDropDownList_OnSelectedIndexChanged" ></asp:DropDownList>
                      
                            <ep:TextBox ID="NewPlanNameTextBox" Width="220px" MaxLength="50" runat="server" />
                        </td>
                    </tr>      
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="PlanTypeLabel" runat="server" Text="Plan Type" />
                        </td>
                        <td colspan="2">
                            <asp:DropDownList ID="PlanTypeDropDownList" runat="server"  EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="PlanTypeDropDownList_OnSelectedIndexChanged"   ></asp:DropDownList>
                        </td>
                    </tr>                                                  
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorNameLabel" runat="server" Text="Plan Administrator Name" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorNameTextBox"  Width="220px"  MaxLength="50" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorEmailLabel" runat="server" Text="Email" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorEmailTextBox"  Width="220px"  MaxLength="50" runat="server"    />
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorAddressLabel" runat="server" Text="Address" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorAddressTextBox"  Width="220px"  MaxLength="50" runat="server"    />
                        </td>
                    </tr>
                   
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorCityLabel" runat="server" Text="City" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorCityTextBox"  Width="220px" MaxLength="50" runat="server"  />
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;" >
                            <asp:Label ID="AdministratorCountryLabel" Text="Country" runat="server" CssClass="FieldLabelText"></asp:Label> 
                        </td>
                        <td colspan="2">
                            <div id="CountryDiv" >
                                <asp:DropDownList ID="AdministratorCountryDropDownList" runat="server" Width="100%" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="AdministratorCountryDropDownList_OnSelectedIndexChanged" ></asp:DropDownList>
                            </div>                                    
                        </td>          
                    </tr>

                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorStateLabel" runat="server" Text="State" />
                        </td>
                        <td colspan="2" style="text-align:left;">
                            
                                <table>
                                    <tr>                               
                                        <td style="text-align:left;">
                                            <div id="StateDiv" >
                                                <asp:DropDownList ID="StateDropDownList" runat="server"   EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="StateDropDownList_OnSelectedIndexChanged"  ></asp:DropDownList>
                                            </div>
                                        </td>
                                        <td style="text-align:left;">
                                            <asp:Label ID="AdministratorZipLabel" runat="server" Text="Zip" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="AdministratorZipTextBox"  Width="100px"   MaxLength="15" runat="server"   />
                                        </td>
                                    </tr>
                                </table>
                            
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorPhoneLabel" runat="server" Text="Phone" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorPhoneTextBox"  MaxLength="30" runat="server"   />
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">
                            <asp:Label ID="AdministratorFaxLabel" runat="server" Text="Fax" />
                        </td>
                        <td colspan="2">
                            <ep:TextBox ID="AdministratorFaxTextBox"  MaxLength="15" runat="server"  />
                        </td>
                    </tr>
                    <tr>                
                        <td colspan="3">
                           
                        </td>
                    </tr>
                </table>
                <ep:MsgBox ID="MsgBox" NameofMsgBox="EditSBAPlanDetails" style="z-index:103; position:absolute; left:200px; top:200px;" runat="server" />
        </asp:Panel>
 
    </form>
</body>
</html>
