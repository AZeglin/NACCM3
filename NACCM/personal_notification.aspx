<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="personal_notification.aspx.vb" Inherits="NACCM.personal_notification" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Personal Notification</title>
</head>
<body style="background-color: #ece9d8">
    <form id="form1" runat="server">
    <asp:Label ID="lbTitle" runat="server" Text="Personalized Notification" Font-Bold="true" Font-Size="XX-Large" />
    <asp:Panel ID="pnNotice" runat="server" ScrollBars="Vertical" Height="400" Width="550">
    <asp:Repeater ID="NoticeReperter" runat="server" DataSourceID="PersonalDataSource">
     <ItemTemplate>
     <asp:LinkButton ID="lBtnCntrctNum" runat="server" Text='<%#Eval("CntrctNum") %>' CommandName="OpenContract" Width="75" />
     <asp:Label ID="lBtnPersonal" runat="server" Text='<%#Eval("Name_Number_Join") %>' Width="450" />
    <br /><asp:Label ID="lbMessage" Text='<%#Eval("Notice") %>' runat="server" NavigateUrl="~/CM_Splash.htm"/><br />
    </ItemTemplate>
   
    </asp:Repeater>
    
    </asp:Panel>
    <asp:SqlDataSource ID="PersonalDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        
        SelectCommand="SELECT * FROM [View_Notification_Web] WHERE (([CO_COID] = @CO_COID) OR ([AD_COID] = @AD_COID) OR ([SM_COID] = @SM_COID) OR ([DIR_COID] = @DIR_COID)) ORDER BY CntrctNum">
        <SelectParameters>
            <asp:QueryStringParameter Name="CO_COID" QueryStringField="COID" Type="Int32" />
            <asp:QueryStringParameter Name="AD_COID" QueryStringField="COID" Type="Int32" />
            <asp:QueryStringParameter Name="SM_COID" QueryStringField="COID" Type="Int32" />
            <asp:QueryStringParameter Name="DIR_COID" QueryStringField="COID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    
    </form>
</body>
</html>
