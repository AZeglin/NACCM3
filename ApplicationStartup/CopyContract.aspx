<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CopyContract.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.CopyContract" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register src="CompactHeader.ascx" tagname="CompactHeader" tagprefix="uc2" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html>
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <title>Copy Contract</title>
    <link href="ApplicationStartupDetails.css" rel="stylesheet" type="text/css" />
    
       <script type="text/javascript">
       <!--
            function CloseWindow(withRefresh, onlyReselect)
            {
                window.opener.document.forms[0].RefreshContractSelectScreenOnSubmit.value = withRefresh;
                window.opener.document.forms[0].RebindContractSelectScreenOnRefreshOnSubmit.value = withRefresh;
                window.opener.document.forms[0].ReselectPreviouslySelectedItem.value = onlyReselect;
                
                window.opener.document.forms[0].submit();

                top.window.opener = top;
                top.window.open('','_parent','');
                top.window.close();
            }
        //-->
        </script>
  
        
</head>
<body >
    <form id="CopyContractForm" runat="server" 
    
    style="z-index: 5; position:fixed; top:0px; left:0px; width:498px; height:322px;" >
   
       <asp:ScriptManager ID="CopyContractScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="CopyContractScriptManager_OnAsyncPostBackError" >
           
        </asp:ScriptManager>


        <table style="height: 320px; table-layout:fixed; width: 494px; border:solid 1px black;" align="left" >
            <col style="width:492px;" />
 
            <tr style=" height:84px; border: solid 1px black; vertical-align:top;">
                <td >
                    <div style="height: 82px; width: 490px;" > 
                        <asp:UpdatePanel ID="SelectedItemHeaderUpdatePanel" runat="server"  UpdateMode="Conditional" ChildrenAsTriggers="false">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="CopyContractUpdateUpdatePanelEventProxy" EventName="ProxiedEvent" />
                            </Triggers>

                            <ContentTemplate>
                                <uc2:CompactHeader ID="SelectedItemHeader" runat="server" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
 
            <tr >
                <td  width="98%" align="left" >
                     <table  style="border-top:solid 1px black; height:28px; width:488px; table-layout:fixed;  text-align:center;" >
                        <tr style="text-align:right;">
                            <td  >
                            </td>
                            <td  >            
                            </td>
                            <td style="width:80px;" >
                                <asp:Button runat="server"  ID="UpdateCopyContractButton" Text="Copy" OnClick="UpdateCopyContractButton_OnClick" >            
                                </asp:Button >      
                            </td>
                            <td style="width:80px;" >
                                <asp:Button runat="server"  ID="CancelCopyContractButton" Text="Close" OnClick="CancelCopyContractButton_OnClick" >            
                                </asp:Button >      
                            </td>
 
                        </tr>
                     </table>
                 </td>
                
            </tr>
 
            <tr style="height:292px; border: solid 1px black; vertical-align:top;">
                <td>
                 <asp:Panel ID="ItemDetailsBodyPanel" runat="server"   
                     CssClass="ItemDetails" Width="488px" Height="290px" >
                        <div id="ItemDetailsBodyDiv"  
                            
                            style="width:486px; height:288px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;" >
                            <ep:UpdatePanelEventProxy ID="CopyContractUpdateUpdatePanelEventProxy" runat="server" />

                            <asp:FormView ID="CopyContractFormView" CssClass="ItemDetailsEditArea" 
                                runat="server" OnDataBound="CopyContractFormView_OnDataBound" 
                                DefaultMode="Edit" OnPreRender="CopyContractFormView_OnPreRender" 
                                Height="286px" >
                            <ItemTemplate>
                                <table style="height: 284px; table-layout:fixed; width: 484px; vertical-align:top;" align="left"  >
                                <col style="width:6px;" />
                                <col style="width:120px;" />
                                <col style="width:80px;" />
                                <col style="width:80px;" />
                                <col style="width:80px;" />
                                <col style="width:70px;" />
                                <col style="width:6px;" />
                                <tr style="height:30px;"> 
                                        <td>
                                        </td>
                                        <td colspan="5"  style="border-bottom:solid 1px black; text-align:center; ">
                                            <asp:Label ID="CopyContractLabel" runat="server"  Text="New Contract" CssClass="HeaderText"  />
                                        </td>
                                        <td>
                                        </td>
                                </tr>
                                <tr style="height:50px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="NewContractNumberLabel" runat="server"  Text='<%# MultilineText( new string[] {"Contract", "Number"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="AwardDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Award", "Date"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="EffectiveDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Effective", "Date"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExpirationDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Expiration", "Date"} ) %>' Width="94%" />       
                                        </td>
                                         <td>
                                            <asp:Label ID="OptionYearsLabel" runat="server" Text='<%# MultilineText( new string[] {"Option", "Years"} ) %>' Width="94%" />       
                                        </td>
                                           <td>
                                        </td>
                                 </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                           <asp:Label ID="NewContractNumberDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "NewContractNumber" )%>' runat="server" Width="92%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="AwardDateDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "AwardDate", "{0:d}" )%>' runat="server"  Width="92%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="EffectiveDateDataLabel" Text='<%# DataBinder.Eval( Container.DataItem, "EffectiveDate", "{0:d}" )%>'  runat="server" Width="92%" />                                               
                                        </td>
                                        <td>
                                            <asp:Label ID="ExpirationDateDataLabel" runat="server" Text='<%# DataBinder.Eval( Container.DataItem, "ExpirationDate", "{0:d}" )%>' Width="92%" />       
                                        </td>
                                         <td>
                                            <asp:Label ID="OptionYearsDataLabel" runat="server"  Text='<%# DataBinder.Eval( Container.DataItem, "OptionYears" )%>' Width="30%" />       
                                        </td>
                                         <td>
                                        </td>
                                   </tr>
                        
                        

                                
                                </table>
                  
                            </ItemTemplate>
                            <EditItemTemplate>
                            
                                <table style="height: 284px; table-layout:fixed; width: 484px; vertical-align:top; text-align:left;" >
                                <col style="width:6px;" />
                                <col style="width:120px;" />
                                <col style="width:80px;" />
                                <col style="width:80px;" />
                                <col style="width:80px;" />
                                <col style="width:70px;" />
                                <col style="width:6px;" />
                                <tr style="height:30px;"> 
                                        <td>
                                        </td>
                                        <td colspan="5"  style="border-bottom:solid 1px black; text-align:center; ">
                                            <asp:Label ID="CopyContractLabel" runat="server"  Text="New Contract" CssClass="HeaderText"  />
                                        </td>
                                        <td>
                                        </td>

                                </tr>
                                <tr style="height:50px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <asp:Label ID="NewContractNumberLabel" runat="server"  Text='<%# MultilineText( new string[] {"Contract", "Number"} ) %>' Width="94%" />
                                        </td>
                                        <td>
                                            <asp:Label ID="AwardDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Award", "Date"} ) %>'  Width="94%" />       
                                        </td>
                                        <td>
                                            <asp:Label ID="EffectiveDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Effective", "Date"} ) %>' Width="94%"  />       
                                        </td>
                                        <td>
                                            <asp:Label ID="ExpirationDateLabel" runat="server" Text='<%# MultilineText( new string[] {"Expiration", "Date"} ) %>' Width="94%" />       
                                        </td>
                                         <td>
                                            <asp:Label ID="OptionYearsLabel" runat="server" Text='<%# MultilineText( new string[] {"Option", "Years"} ) %>' Width="94%" />       
                                        </td>
                                          <td>
                                        </td>
                                  </tr>
                                    <tr style="height:25px;" >
                                        <td>
                                        </td>
                                        <td>
                                            <ep:TextBox ID="NewContractNumberTextBox" runat="server" MaxLength="20" Text='<%# DataBinder.Eval( Container.DataItem, "NewContractNumber" )%>' Width="92%" />
                                        </td>
                                        <td>
                                            <ep:TextBox ID="AwardDateTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "AwardDate", "{0:d}" )%>' Width="92%" />       
                                        </td>
                                        <td>
                                            <ep:TextBox ID="EffectiveDateTextBox" runat="server" MaxLength="10" Text='<%# DataBinder.Eval( Container.DataItem, "EffectiveDate", "{0:d}" )%>'  Width="92%" />       
                                        </td>
                                        <td>
                                            <ep:TextBox ID="ExpirationDateTextBox" runat="server" MaxLength="10"  Text='<%# DataBinder.Eval( Container.DataItem, "ExpirationDate", "{0:d}" )%>' Width="92%" />       
                                        </td>
                                        <td>
                                            <ep:TextBox ID="OptionYearsTextBox" runat="server" MaxLength="2" Text='<%# DataBinder.Eval( Container.DataItem, "OptionYears" )%>' Width="30%" />       
                                        </td>
                                         <td>
                                        </td>
                                   </tr>
                       
                                
                                </table>
                                  
                            </EditItemTemplate>
                            
                            </asp:FormView>
                        </div>
                  </asp:Panel>
                </td>
            </tr>
        </table>
            <ep:MsgBox ID="MsgBox" NameofMsgBox="CopyContract" style="z-index:103; position:absolute; left:400px; top:200px;" runat="server" />  
    </form>
</body>
</html>
