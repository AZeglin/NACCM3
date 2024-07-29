<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NACCMStartupErrorPage.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.NACCMStartupErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="NACCMStartupErrorPageForm" runat="server">
    <div style="width:100%; background-color:#C3C3C3;">
         <table width="100%">
            <tr style="vertical-align:top;">
                <td style="vertical-align:top;">
                    <ep:TextBox ID="StartupErrorTextBox" runat="server" ReadOnly="true" TextMode="MultiLine" MaxLength="2000" Rows="30"  Width="99%"  />
                </td>
            </tr>
         </table>                       

    </div>
    </form>
</body>
</html>
