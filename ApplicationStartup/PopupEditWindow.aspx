<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PopupEditWindow.aspx.cs"  StylesheetTheme="CMStandard" Inherits="VA.NAC.CM.ApplicationStartup.PopupEditWindow" %>

<%@ Register Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html>
<html>

<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title>Pending Changes</title>
    <link href="ApplicationStartupDetails.css" rel="stylesheet" type="text/css" />
    
    <script type="text/javascript">
       <!--
           function CloseWindow(response, nameOfHiddenField, clientIdOfHiddenField) {
               window.opener.document.getElementById(clientIdOfHiddenField).value = response;
            window.opener.document.forms[0].submit();

            top.window.opener = top;
               top.window.open('', '_parent', '');
            top.window.close();
        }

        //-->
      </script>
</head>
<body >
    <form id="PopupEditWindowForm" runat="server" style="z-index: 5; position:fixed; top:0px; left:0px; width:412px; height:202px;"   >
    
       <asp:ScriptManager ID="PopupEditWindowScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="PopupEditWindowScriptManager_OnAsyncPostBackError" >
          
        </asp:ScriptManager>
            
        <asp:panel runat="server" ID="ThreeButtonPanel" OnPreRender="ThreeButtonPanel_OnPreRender" >
            <table class="OutsetBox" style="height: 202px; table-layout:fixed; width: 404px;" >        
                <colgroup>
                    <col style="width:8px;" />
                    <col style="width:126px; text-align:center;" />
                    <col style="width:126px; text-align:center;" />
                    <col style="width:126px; text-align:center;" />
                    <col style="width:8px;" />
                </colgroup>            
                <tr  >
                    <td style="vertical-align:middle;  margin:auto;" colspan="5">
                        <asp:Label ID="MessageLabel3" runat="server"  OnPreRender="MessageLabel_OnPreRender" />
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <asp:Button ID="YesButton3" runat="server" Width="98%" OnPreRender="YesButton_OnPreRender" />
                    </td>    
                    <td>
                        <asp:Button ID="NoButton3" runat="server" Width="98%" OnPreRender="NoButton_OnPreRender" />
                    </td>
                    <td>
                        <asp:Button ID="CancelButton3" runat="server" Width="98%" OnPreRender="CancelButton_OnPreRender" />
                    </td>
                    <td></td>
                </tr>
            </table>
        </asp:panel>
       <asp:panel runat="server" ID="TwoButtonPanel" OnPreRender="TwoButtonPanel_OnPreRender">
            <table class="OutsetBox" style="height: 202px; table-layout:fixed; width: 404px;" >   
                <colgroup>
                    <col style="width:8px;" />
                    <col style="width:186px; text-align:center;" />
                    <col style="width:186px; text-align:center;" />
                    <col style="width:8px;" />
                </colgroup>            
                <tr>
                    <td style="vertical-align:middle;  margin:auto;"  colspan="4">
                        <asp:Label ID="MessageLabel2" runat="server"  OnPreRender="MessageLabel_OnPreRender" />
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <asp:Button ID="NoButton2" runat="server" Width="98%" OnPreRender="NoButton_OnPreRender" />
                    </td>                  
                    <td>
                        <asp:Button ID="CancelButton2" runat="server" Width="98%" OnPreRender="CancelButton_OnPreRender" />
                    </td>
                    <td></td>
                </tr>
            </table>
        </asp:panel>
    </form>
</body>
</html>
