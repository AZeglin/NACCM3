<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CM_Header.aspx.vb" Inherits="NACCM.CM_Header" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<%@ Register Assembly="NACCMBrowserObj" Namespace="VA.NAC.NACCMBrowser.BrowserObj" TagPrefix="ep" %>

<%@ Register Assembly="ApplicationSharedObj" Namespace="VA.NAC.Application.SharedObj" TagPrefix="Shared" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title></title>
</head>
<body style="background-color: #ece9d8" >
    <form id="CMHeaderForm" runat="server" style="height:32px;"  >
  
         <asp:ScriptManager ID="CMHeaderScriptManager" runat="server" EnablePartialRendering="true"  OnAsyncPostBackError="CMHeaderScriptManager_OnAsyncPostBackError" >
            
        </asp:ScriptManager>

        <script type="text/javascript" >

            function saveClientScreenResolutionInfo() 
            {
                document.forms[0].ClientScreenHeight.value = screen.height;
                document.forms[0].ClientScreenWidth.value = screen.width;

                document.forms[0].submit();
            }

        </script>
  
        <asp:UpdatePanel ID="CMHeaderUpdatePanel" runat="server" >
            <ContentTemplate>
                <table style="height:30px; table-layout:fixed;">
                    <tr>
                        <td>
                            NAC CM Version 1.9K
                            
                        </td>
                        <td>
                            
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
             
            </ContentTemplate>
        </asp:UpdatePanel>
       
        <div id="HiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >
            <asp:HiddenField ID="ClientScreenWidth" runat="server" Value="0" EnableViewState="true" />
            <asp:HiddenField ID="ClientScreenHeight" runat="server" Value="0" EnableViewState="true" />
        </div>
    </form>
</body>
</html>
