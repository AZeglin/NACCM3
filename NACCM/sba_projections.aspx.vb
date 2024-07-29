Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj
Imports System.Data.SqlClient

Imports TextBox = System.Web.UI.WebControls.TextBox

Partial Public Class sba_projectons
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            If Not Request.QueryString("State") Is Nothing Then
                If CType(Request.QueryString("state"), Integer) = 1 Then
                    fvSBAProjections.ChangeMode(FormViewMode.Edit)
                End If
            End If
            If Not Request.QueryString("Insert") Is Nothing Then
                If Request.QueryString("Insert").ToString = "Y" Then
                    fvSBAProjections.DefaultMode = FormViewMode.Insert
                End If
            End If
        End If
        DisableSBAProjectionEditControls()
    End Sub

    Protected Sub DisableSBAProjectionEditControls()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim updateProjectionButton As Button
        updateProjectionButton = CType(fvSBAProjections.FindControl("updateProjectionButton"), Button)
        If Not updateProjectionButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                updateProjectionButton.Enabled = True
                updateProjectionButton.Visible = True
            Else
                updateProjectionButton.Enabled = False
                updateProjectionButton.Visible = False
            End If

        End If  'insertProjectionButton

        Dim insertProjectionButton As Button
        insertProjectionButton = CType(fvSBAProjections.FindControl("updateProjectionButton"), Button)
        If Not updateProjectionButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                insertProjectionButton.Enabled = True
                insertProjectionButton.Visible = True
            Else
                insertProjectionButton.Enabled = False
                insertProjectionButton.Visible = False
            End If

        End If

    End Sub

    Protected Function CalPercentage(ByVal first As Object, ByVal second As Object) As String
        Dim myPercentage As String = ""
        Dim myFirst As Double = 0
        Dim mySecond As Double = 0
        If Not first.Equals(DBNull.Value) Then
            myFirst = CType(first, Double)
        End If
        If Not second.Equals(DBNull.Value) Then
            mySecond = CType(second, Double)
        End If
        myPercentage = FormatPercent((myFirst / mySecond), 2).ToString()
        Return myPercentage
    End Function
    Protected Sub CalInsertPercentage(ByVal send As Object, ByVal e As EventArgs)
        Dim myLabel As String = send.ID.ToString
        Dim dollarText As TextBox = CType(fvSBAProjections.FindControl("tbInsertTotalDollar"), TextBox)
        Dim myTotal As Double = CType(dollarText.Text, Double)
        Dim myPercent As Double = 0
        If myLabel = "tbInsertDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            myTextbox = CType(fvSBAProjections.FindControl("tbInsertSBPercent"), TextBox)
        End If

    End Sub
    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Response.Write("<script>window.close()</script>")
    End Sub

    Private Sub fvSBAProjections_ItemCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.FormViewCommandEventArgs) Handles fvSBAProjections.ItemCommand
        If e.CommandName = "Update" Then


        End If
    End Sub

    'SBAProjectionsDataSource
    Private Sub SBAProjectionsDataSource_Inserting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles SBAProjectionsDataSource.Inserting

        Dim CreatedByParm As SqlParameter = New SqlParameter("@CreatedBy", SqlDbType.NVarChar, 120)
        Dim CreationDateParm As SqlParameter = New SqlParameter("@CreationDate", SqlDbType.DateTime)
        Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
        Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        CreatedByParm.Value = browserSecurity.UserInfo.LoginName
        CreationDateParm.Value = DateTime.Now
        LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
        LastModificationDateParm.Value = DateTime.Now

        e.Command.Parameters.Add(CreatedByParm)
        e.Command.Parameters.Add(CreationDateParm)
        e.Command.Parameters.Add(LastModifiedByParm)
        e.Command.Parameters.Add(LastModificationDateParm)

    End Sub

    'SBAProjectionsDataSource
    Private Sub SBAProjectionsDataSource_Updating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles SBAProjectionsDataSource.Updating

        Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
        Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
        LastModificationDateParm.Value = DateTime.Now

        e.Command.Parameters.Add(LastModifiedByParm)
        e.Command.Parameters.Add(LastModificationDateParm)

    End Sub
End Class