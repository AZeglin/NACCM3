Imports System.Data
Imports System.Data.SqlClient
Imports VA.NAC.Application.SharedObj
Imports VA.NAC.NACCMBrowser.BrowserObj

' took out on 11/3/2011
' <asp:TreeNode Text="Legacy Reports" Value="Legacy Reports" NavigateUrl="~/NAC_Reports.aspx" Target="mainFrame" ToolTip="Reports"> </asp:TreeNode>


Partial Public Class tree_nav
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim userName As String

        If Not IsPostBack Then
            userName = getUserName()
            Session("UserName") = userName
            getCOID()
            Session("NACCM") = "True"
            'Session("Admin") = getAdmin() removed from use 4/29/2010
            'If Session("Admin") = "YES" Then
            '    Add_Admin_Node()
            'End If

            RecordUserLoginEvent()
        End If

        CMGlobals.AddKeepAlive(Me.Page, 30000)

    End Sub
  

    Protected Sub getCOID()

        Dim myCOID As Integer = -1

        Dim bs As BrowserSecurity2
        bs = CType(Session("BrowserSecurity"), BrowserSecurity2)
        If Not bs Is Nothing Then
            If Not bs.UserInfo Is Nothing Then
                myCOID = bs.UserInfo.OldUserId
            End If
        End If


        Dim mynode As TreeNode = CType(tvNACCM.Nodes(0).ChildNodes(0).ChildNodes(0).ChildNodes(0), TreeNode)
        mynode.NavigateUrl = "~/ContractSelect.aspx?Status=Active&Owner=Mine" ' "my_active_contracts.aspx?COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(0).ChildNodes(0).ChildNodes(1), TreeNode)
        mynode.NavigateUrl = "~/ContractSelect.aspx?Status=Closed&Owner=Mine" ' "my_expired_contracts.aspx?COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(1).ChildNodes(0).ChildNodes(0), TreeNode)
        mynode.NavigateUrl = "~/OfferSelect.aspx?Status=Open&Owner=Mine" ' "my_open_offers.aspx?COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(1).ChildNodes(0).ChildNodes(1), TreeNode)
        mynode.NavigateUrl = "~/OfferSelect.aspx?Status=Completed&Owner=Mine" ' "my_closed_offers.aspx?COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(0), TreeNode)
        mynode.NavigateUrl = "personal_notification.aspx?COID=" & myCOID & "&AD_COID=" & myCOID & "&SM_COID=" & myCOID & "&DIR_COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(1), TreeNode)
        mynode.NavigateUrl = "personal_notification.aspx?COID=" & myCOID & "&AD_COID=" & myCOID & "&SM_COID=" & myCOID & "&DIR_COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(0).ChildNodes(0), TreeNode)
        mynode.NavigateUrl = "personal_notification.aspx?COID=" & myCOID & "&AD_COID=" & myCOID & "&SM_COID=" & myCOID & "&DIR_COID=" & myCOID
        mynode.Target = "mainFrame"
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(3).ChildNodes(0), TreeNode)
        mynode.NavigateUrl = GetReportUrl("/Contracts/Reports")
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(3).ChildNodes(1), TreeNode)
        mynode.NavigateUrl = GetReportUrl("/Sales/Reports")
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(3).ChildNodes(2), TreeNode)
        mynode.NavigateUrl = GetReportUrl("/Fiscal/Reports")
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(3).ChildNodes(3), TreeNode)
        mynode.NavigateUrl = GetReportUrl("/Pharmaceutical/Reports")
        mynode = CType(tvNACCM.Nodes(0).ChildNodes(3).ChildNodes(4), TreeNode)
        mynode.NavigateUrl = GetReportUrl("/SBA/Reports")


    End Sub
    'apply security to tree nodes
    Protected Sub EnableTreeNodes(ByVal mainTreeView As TreeView)
        Dim newOfferNode As TreeNode = CType(mainTreeView.Nodes(0).ChildNodes(1).ChildNodes(4), TreeNode)
        Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
        If Not newOfferNode Is Nothing Then
            If Not bs Is Nothing Then
                If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.CreateOffer) = False) Then
                    mainTreeView.Nodes(0).ChildNodes(1).ChildNodes.Remove(newOfferNode)
                End If
            End If
        End If

        Dim newContractNode As TreeNode = CType(mainTreeView.Nodes(0).ChildNodes(0).ChildNodes(4), TreeNode)
        If Not newContractNode Is Nothing Then
            If Not bs Is Nothing Then
                If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.CreateContract) = False) Then
                    mainTreeView.Nodes(0).ChildNodes(0).ChildNodes.Remove(newContractNode)
                End If
            End If
        End If
    End Sub
    Protected Function getAdmin() As String
        Dim IsAdmin As String = "NO"
        Dim strSQL As String = "SELECT * FROM tlkup_Admin_Users WHERE UserName = '" & Session("UserName") & "'"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            If rdr.HasRows Then
                IsAdmin = "YES"
            End If
            conn.Close()
        Catch ex As Exception
        Finally
            conn.Close()
        End Try
        Return IsAdmin
    End Function

    Public Function GetReportUrl(ByVal reportSubDirectoryName As String) As String
        Dim reportUrl As String
        If reportSubDirectoryName.Length > 0 Then
            reportUrl = String.Format("http://{0}/Reports/Pages/Folder.aspx?ItemPath={1}&ViewMode=List", Config.ReportingServicesServerName, reportSubDirectoryName)
        Else
            reportUrl = String.Format("http://{0}/Reports/Pages/Folder.aspx?ViewMode=List", Config.ReportingServicesServerName)
        End If
        Return reportUrl
    End Function

    Private Function getUserName() As String
        Dim UserName As String = ""
        Dim p As System.Security.Principal.WindowsPrincipal
        p = System.Threading.Thread.CurrentPrincipal
        UserName = p.Identity.Name.ToString
        If UserName.Equals("") Then
            UserName = "None"
        End If
        Return UserName
    End Function
    'Protected Sub Add_Admin_Node()
    '    Dim myTree As TreeView = CType(tvNACCM, TreeView)
    '    myTree.Nodes.Add(New TreeNode("Admin"))
    '    Dim myNode As TreeNode = CType(tvNACCM.Nodes(3), TreeNode)
    '    myNode.ChildNodes.Add(New TreeNode("Contracts"))
    '    myNode.ChildNodes.Add(New TreeNode("Offers"))
    '    myNode.Expand()
    '    myNode = CType(tvNACCM.Nodes(3).ChildNodes(0), TreeNode)
    '    myNode.NavigateUrl = "all_contracts.aspx"
    '    myNode.Target = "mainFrame"
    '    myNode = CType(tvNACCM.Nodes(3).ChildNodes(1), TreeNode)
    '    myNode.NavigateUrl = "all_offers.aspx"
    '    myNode.Target = "mainFrame"

    'End Sub

    Protected Sub RecordUserLoginEvent()
        Dim log As New UserActivity(VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.Login, "", VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.Undefined)
        log.LogUserActivity()
    End Sub

    Protected Sub tvNACCM_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles tvNACCM.PreRender
        EnableTreeNodes(CType(sender, TreeView))
    End Sub
End Class
