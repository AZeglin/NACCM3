<%@ Page Title=""  Language="C#" MasterPageFile="~/NACCM.Master" StylesheetTheme="CMStandard"  AutoEventWireup="true" CodeBehind="Start.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.Start" %>
<%@ MasterType VirtualPath="~/NACCM.Master" %>

<asp:Content ID="StartContent" ContentPlaceHolderID="ContentPlaceHolderMain" runat="server">

        <script type="text/javascript" >
          
            window.onload = function (event) {
                var obj = {};
                obj.Height = screen.height;
                obj.Width = screen.width;
                var jsonData = JSON.stringify(obj);

                $.ajax({
                    url: 'WindowSizeHandler.ashx',
                    type: 'POST',
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    data: jsonData,
                                     
                    success: function (response) {                                             
                        var status = response.Status;
                        var message = response.Message;                       
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        alert( 'error was' + XMLHttpRequest.status + "\n" + XMLHttpRequest.responseText);
                    }

                })

                event.preventDefault();
            }
            
        </script>

 <table style="table-layout:fixed; width:100%; height:100%;" >
        <tr>
            <td style="vertical-align:top;">
                <asp:Panel ID="StartupPicturePanel" runat="server" Width="100%" Height="100%" >
                    <asp:Image ID="AmericanFlagTransparentImage" runat="server" ImageAlign="Middle" Width="100%" Height="100%" ImageUrl="~/Images/NACCMAmericanFlagTransparent.gif" />
                </asp:Panel>            
            </td>
        </tr>
</table>

</asp:Content>
