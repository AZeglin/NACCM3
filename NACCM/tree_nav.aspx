<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="tree_nav.aspx.vb" Inherits="NACCM.tree_nav" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>NAC CM Tree View</title>
  
</head>
<body style="background-color: #ece9d8"  >
    <form id="TreeViewForm" runat="server"  >

        <asp:TreeView ID="tvNACCM" runat="server" ImageSet="XPFileExplorer" 
        NodeIndent="15" PathSeparator="/"  >
        <ParentNodeStyle Font-Bold="False" />
        <HoverNodeStyle Font-Underline="True" ForeColor="#6666AA" />
        <SelectedNodeStyle BackColor="#B5B5B5" Font-Underline="False" 
            HorizontalPadding="0px" VerticalPadding="0px" />
        <Nodes>
            <asp:TreeNode Text="NAC CM" Value="NAC CM" NavigateUrl="~/CM_Splash.htm" Target="mainFrame" ToolTip="NAC CM Home" SelectAction="Expand">
                <asp:TreeNode Text="Contracts" Value="Contracts" Expanded="False" NavigateUrl="~/CM_Splash.htm" Target="mainFrame" ToolTip="Contracts Folder" SelectAction="SelectExpand" >
                    <asp:TreeNode Text="My Contracts" Value="My Contracts" Target="mainFrame" Expanded="False" ToolTip="My Contracts Folder" SelectAction="SelectExpand">
                        <asp:TreeNode Text="Active Contracts" Value="Active Contracts" ToolTip="My Active Contracts" Target="mainFrame"></asp:TreeNode>
                        <asp:TreeNode Text="Expired Contracts" Value="Expired Contracts" ToolTip="My expired Contracts"></asp:TreeNode>
                    </asp:TreeNode>
                    <asp:TreeNode Text="Search All Contracts" Value="Search All Contracts" ToolTip="Search All Contracts" NavigateUrl="~/ContractSelect.aspx?Status=All&Owner=All" Target="mainFrame" ></asp:TreeNode>
                    <asp:TreeNode Text="Active Contracts" Value="Active Contracts" NavigateUrl="~/ContractSelect.aspx?Status=Active&Owner=All" Target="mainFrame" ToolTip="All Active Contracts"></asp:TreeNode>
                    <asp:TreeNode Text="Expired Contracts" Value="Expired Contracts" NavigateUrl="~/ContractSelect.aspx?Status=Closed&Owner=All" Target="mainFrame" ToolTip="All Expired Contracts"></asp:TreeNode>
                    <asp:TreeNode Text="Contract Addition" 
                        Value="Contract Addition" NavigateUrl="~/CreateContract.aspx" Target="mainFrame" ToolTip="Add Contracts"></asp:TreeNode>
                        
                </asp:TreeNode>
                <asp:TreeNode Text="Offers" Value="Offers" Expanded="False" NavigateUrl="~/CM_Splash.htm" Target="mainFrame" ToolTip="Offers Folder" SelectAction="Expand">
                    <asp:TreeNode Text="My Offers" Value="My Offers" Expanded="False" ToolTip="My Offers Folder" NavigateUrl="~/CM_Splash.htm" Target="mainFrame" SelectAction="Expand">
                        <asp:TreeNode Text="Open Offers" Value="Open Offers" ToolTip="My Open Offers"></asp:TreeNode>
                        <asp:TreeNode Text="Closed Offers" Value="Closed Offers" ToolTip="My Closed Offers"></asp:TreeNode>
                    </asp:TreeNode>
                    <asp:TreeNode Text="Search All Offers" Value="Search All Offers" ToolTip="Search All Offers" NavigateUrl="~/OfferSelect.aspx?Status=All&Owner=All" Target="mainFrame"></asp:TreeNode>
                    <asp:TreeNode Text="Open Offers" Value="Open Offers" ToolTip="All Open Offers" NavigateUrl="~/OfferSelect.aspx?Status=Open&Owner=All" Target="mainFrame"></asp:TreeNode>
                    <asp:TreeNode Text="Closed Offers" Value="Closed Offers" ToolTip="All Closed Offers" NavigateUrl="~/OfferSelect.aspx?Status=Completed&Owner=All" Target="mainFrame"></asp:TreeNode>
                    <asp:TreeNode Text="Offer Addition (FSS)" Value="Offer Addition (FSS)" NavigateUrl="~/offer_addtion.aspx" Target="mainFrame" ToolTip="Add Offers">
                    </asp:TreeNode>
                </asp:TreeNode>
                <asp:TreeNode Text="Catalog Search Tool" Value="Catalog Search Tool" NavigateUrl="http://www.va.gov/nac/" Target="mainFrame" ToolTip="NAC Web Catalog Search Tool"></asp:TreeNode>
                  
                <asp:TreeNode Text="Reports" Value="Reports" Target="_blank" ToolTip="Reports" SelectAction="Expand" Expanded="False" >
                    <asp:TreeNode Text="Contract Reports" Value="Contract Reports" Target="_blank" ToolTip="Contract Reports"> </asp:TreeNode>
                    <asp:TreeNode Text="Sales Reports" Value="Sales Reports" Target="_blank" ToolTip="Sales Reports"> </asp:TreeNode>
                    <asp:TreeNode Text="Fiscal Reports" Value="Fiscal Reports" Target="_blank" ToolTip="Fiscal Reports"> </asp:TreeNode>
                    <asp:TreeNode Text="Pharmaceutical Reports" Value="Pharmaceutical Reports" Target="_blank" ToolTip="Pharmaceutical Reports"> </asp:TreeNode>
                    <asp:TreeNode Text="SBA Reports" Value="SBA Reports" Target="_blank" ToolTip="SBA Reports"> </asp:TreeNode>                   
                </asp:TreeNode>
            </asp:TreeNode>
  
        </Nodes>
        <NodeStyle Font-Names="Tahoma" Font-Size="8pt" ForeColor="Black" 
            HorizontalPadding="2px" NodeSpacing="0px" VerticalPadding="2px" />
    </asp:TreeView>

  
    </form>
</body>
</html>
