<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="VA.NAC.ReportManager.ReportViewer" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge">
    <title></title>    
</head>
<body style="width:100%; height:100%;" >
 
    <form id="ReportViewerForm" runat="server" style="width:100%; height:100%;">
    
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
      
    <rsweb:ReportViewer ID="ReportViewer1" Width="100%" Height="100%" runat="server" Enabled="true" Visible="true" >
  
    </rsweb:ReportViewer>
   
    </form>

</body>
</html>
