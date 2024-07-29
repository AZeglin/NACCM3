Imports System.Data
Imports System.Data.SqlClient

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface

Partial Public Class personal_notification
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        '$$$ this will never be true due to a recent change - need to fix by creating a new IsInRole function
        'If Session("Admin") = "YES" Then
        '    PersonalDataSource.SelectCommand = "SELECT * FROM [View_Notification_Web] ORDER BY CntrctNum"
        'End If

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)
        If Not browserSecurity Is Nothing Then
            If browserSecurity.IsInRole("NACCM Administrator") = True Then
                PersonalDataSource.SelectCommand = "SELECT * FROM [View_Notification_Web] ORDER BY CntrctNum"
            End If
        End If

        Dim log As New UserActivity(UserActivity.ActionTypes.ViewPersonalNotificationScreen, "", UserActivity.ActionDetailsTypes.ViewPersonalNotificationScreen)
        log.LogUserActivity()

    End Sub

    Private Sub NoticeReperter_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.RepeaterCommandEventArgs) Handles NoticeReperter.ItemCommand
        If e.CommandName = "OpenContract" Then
            Dim myLink As LinkButton = CType(e.CommandSource, LinkButton)
            Dim myCntrctNum As String = myLink.Text
            Dim myScheduleNum As String = GetSchedule(myCntrctNum)

            Dim selectedItem As RepeaterItem = e.Item

            ' create a current document for the selected contract
            Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
            ' deprecating this call in R2.2
            currentDocument = New CurrentDocument(-1, myCntrctNum, myScheduleNum, CType(Session("ContractDB"), ContractDB), CType(Session("DrugItemDB"), DrugItemDB), CType(Session("ItemDB"), ItemDB))
            Session("CurrentDocument") = currentDocument
            'currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Active
            currentDocument.LookupCurrentDocument()

            'currentDocument.VendorName = mydataRow.Cells(3).Text
            'currentDocument.Description = mydataRow.Cells(4).Text
            'currentDocument.AwardDate = DateTime.Parse(mydataRow.Cells(5).Text)
            'currentDocument.ExpirationDate = DateTime.Parse(mydataRow.Cells(6).Text)

            Dim browserSecurity As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
            browserSecurity.SetDocumentEditStatus(currentDocument)

            Dim strWindow As String = "<script>window.open('NAC_CM_Contracts.aspx?CntrctNum=" & myCntrctNum & "&SchNum=" & myScheduleNum & "','Details','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,top=80,left=170,width=910,height=730')</script>"
            Response.Write(strWindow)
        End If
    End Sub

    Protected Function GetSchedule(ByVal CntrctNUm As String) As String
        Dim mySchedule As String = ""
        Dim strSQL As String = "SELECT Schedule_Number from tbl_Cntrcts WHERE CntrctNum = '" & CntrctNUm & "'"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As New SqlCommand(strSQL, conn)
        Try
            cmd.Connection.Open()
            mySchedule = cmd.ExecuteScalar
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("Error getting schedule number" & ex.ToString)
        Finally
            cmd.Connection.Close()
        End Try
        Return mySchedule
    End Function
End Class