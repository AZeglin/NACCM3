Imports System.Data
Imports System.Data.SqlClient

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj

Imports TextBox = System.Web.UI.WebControls.TextBox

Partial Public Class SBA_Add_Plan
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim myPlanName As String = tbPlan.Text
        Dim myPlanType As Integer = CType(dlPlanType.SelectedValue, Integer)
        Dim newSBAPlanId As Integer
        If myPlanName.Trim().Length <= 0 Then
            Msg_Alert.Client_Alert_OnLoad("Blank plan names are not allowed.")
            Return
        End If

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Dim strSQL As String = "INSERT INTO tbl_sba_SBAPLAN (PlanName, PlanTypeID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate) VALUES(@PlanName, @PlanType, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate) select @Identity = @@identity"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim InsertCommand As SqlCommand = New SqlCommand()
        Try
            InsertCommand.Connection = conn
            InsertCommand.CommandText = strSQL
            InsertCommand.Parameters.Add("@PlanName", SqlDbType.NVarChar).Value = myPlanName
            InsertCommand.Parameters.Add("@PlanType", SqlDbType.Int).Value = myPlanType
            InsertCommand.Parameters.Add("@Identity", SqlDbType.Int).Direction = ParameterDirection.Output
            InsertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            InsertCommand.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = DateTime.Now
            InsertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            InsertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            conn.Open()
            InsertCommand.ExecuteNonQuery()

            newSBAPlanId = Integer.Parse(InsertCommand.Parameters("@Identity").Value.ToString())
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of the inserting SBA Plan. " & ex.ToString)
        Finally
            conn.Close()
        End Try
        '  Response.Write("<script>window.close();</script>")
        Dim closeScript As String = String.Format("window.opener.RefreshSBAPlanList({0});window.close();", newSBAPlanId.ToString())
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "CloseWindowScript", closeScript, True)

    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Response.Write("<script>window.close();</script>")
    End Sub
End Class