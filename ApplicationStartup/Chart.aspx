<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" CodeBehind="Chart.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.Chart" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<!DOCTYPE html>
<html>

<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <title>Chart</title>
    <link href="./App_Themes/CMStandard/CMStandard.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">

        /* keep window on top */
        function window::onload() {
            window.focus();
        }

        function window::onblur() {
            window.focus();
        }

        function CloseWindow()
        {         
            top.window.opener = top;
            top.window.open('','_parent','');
            top.window.close();
        }
       </script>
 </head>
 <body>
    <form id="form1" runat="server" >
        <div style="text-align:center;" >
        <table style="height: 99%; width:100%;" >
            <colgroup>
                <col style="width:2%;" />
                <col style="width:96%;" />
                <col style="width:2%;" />
            </colgroup>
            <tr>
                <td></td>
                <td style="text-align:center; height:22px;" >
                    <asp:Label ID="ChartCaption1" runat="server" ></asp:Label>
                </td>
                <td></td>
            </tr>
            <tr >
                <td></td>
                <td style="text-align:center; height:22px;" >
                    <asp:Label ID="ChartCaption2" runat="server" ></asp:Label>
                </td>
                <td></td>
            </tr>
            <tr >
                <td></td>
                <td style="text-align:center; vertical-align:top; height:392px;" >

                    <asp:Chart ID="Chart1" runat="server" Height="390px" Width="360px">    
                                   
                    </asp:Chart>
                    

                </td>
                <td></td>
            </tr>
             <tr >
                 <td></td>
                 <td style="text-align:center; vertical-align:middle; height:39px;" >                            
                    <asp:Button ID="FormOkCloseButton" runat="server" Text="Ok" Width="60px" ToolTip="Dismiss chart window." CausesValidation="false" OnClientClick="CloseWindow();" />
            
                 </td>
                 <td></td>
             </tr>
         </table>
        
    </div>
    
    </form>
</body>
</html>
