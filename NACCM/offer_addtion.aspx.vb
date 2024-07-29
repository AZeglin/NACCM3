Imports System.Data
Imports System.Data.SqlClient

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface

Imports TextBox = System.Web.UI.WebControls.TextBox

Partial Public Class offer_addtion
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            btnAddAward.Text = "Save Entry" & vbCrLf & "and Exit"
            btnAddAward.ForeColor = Drawing.Color.Green
            btnOpenContract.Text = "Cancel Entry" & vbCrLf & "and Exit"
            btnOpenContract.ForeColor = Drawing.Color.Red
        End If
    End Sub
    Private Sub btnExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExit.Click
        Response.Write("<script>window.close();</script>")
    End Sub

    Private Sub btnMainMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnMainMenu.Click
        Response.Redirect("Old1NCM.aspx")
    End Sub

    Private Sub btnOfferSearc_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOfferSearc.Click
        Response.Redirect("offer_search.aspx")
    End Sub

    Private Sub btnAddAward_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddAward.Click
        Dim Exist As String = ""
        Dim strSQL As String = ""
        Dim errorText As String = ""
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Try
            strSQL = "INSERT INTO tbl_Offers (Contractor_Name, Solicitation_ID, CO_ID, " _
           & "Schedule_Number, OfferNumber, Proposal_Type_ID, Dates_Received, Dates_Assigned,Action_ID,Dates_Expected_Completion," _
           & " POC_Primary_Name,POC_Primary_Phone,POC_Primary_Ext,POC_Primary_Fax,POC_Primary_Email,Primary_Address_1, Primary_Address_2, Primary_City, Primary_State, Primary_Zip, POC_VendorWeb, Comments, CreatedBy, Date_Entered, LastModifiedBy, Date_Modified ) " _
           & "VALUES(@ContractName,@SolicitationID, @CO, @Schedule, @OfferNumber, @ProposalID, @DateReceived, @DateAssigned, @ActionID, @ExpectedDate," _
           & "@POCPrimaryName, @POCPrimaryPhone, @POCPrimaryExt, @POCPrimaryFax, @POCPrimaryEmail, @POCAddress1, @POCAddress2, @POCCity, @POCState, @POCZip, @POCWeb, @Comments, @CreatedBy, @Date_Entered, @LastModifiedBy, @Date_Modified )"
            Dim insertCommand As SqlCommand = New SqlCommand()
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL
            insertCommand.Parameters.Add("@CO", SqlDbType.Int).Value = dlCOName.SelectedValue
            insertCommand.Parameters.Add("@ContractName", SqlDbType.NVarChar).Value = tbVendor.Text
            insertCommand.Parameters.Add("@SolicitationID", SqlDbType.Int).Value = dlSolicitation.SelectedValue
            insertCommand.Parameters.Add("@OfferNumber", SqlDbType.NVarChar).Value = OfferNumberTextBox.Text
            insertCommand.Parameters.Add("@Schedule", SqlDbType.Int).Value = dlSchedule.SelectedValue
            insertCommand.Parameters.Add("@ProposalID", SqlDbType.Int).Value = dlProposalType.SelectedValue
            insertCommand.Parameters.Add("@ActionID", SqlDbType.Int).Value = dlCurrent.SelectedValue

            If dlReceivedDate.SelectedValue.Length <= 0 Then
                Throw New Exception("Received date cannot be null.")
            End If

            insertCommand.Parameters.Add("@DateReceived", SqlDbType.DateTime).Value = dlReceivedDate.SelectedValue

            Dim assignmentDateParm As SqlParameter = insertCommand.Parameters.Add("@DateAssigned", SqlDbType.DateTime)
            If dlAssignmentDate.SelectedValue.Length > 0 Then
                Dim receivedDate As DateTime = CType(dlReceivedDate.SelectedValue, DateTime)
                Dim assignmentDate As DateTime = CType(dlAssignmentDate.SelectedValue, DateTime)

                If DateTime.Compare(receivedDate, assignmentDate) > 0 Then
                    Throw New Exception("Assignment date must be the same as or after received date.")
                End If

                assignmentDateParm.Value = dlAssignmentDate.SelectedValue
            Else
                assignmentDateParm.Value = DBNull.Value
            End If

            Dim estimatedCompletionDateParm As SqlParameter = insertCommand.Parameters.Add("@ExpectedDate", SqlDbType.DateTime)
            If dlEstCompDate.SelectedValue.Length > 0 Then
                estimatedCompletionDateParm.Value = dlEstCompDate.SelectedValue
            Else
                estimatedCompletionDateParm.Value = DBNull.Value
            End If
            insertCommand.Parameters.Add("@POCPrimaryName", SqlDbType.NVarChar).Value = tbName.Text
            insertCommand.Parameters.Add("@POCPrimaryPhone", SqlDbType.NVarChar).Value = tbPhone.Text
            insertCommand.Parameters.Add("@POCPrimaryExt", SqlDbType.NVarChar).Value = tbPhoneExtension.Text
            insertCommand.Parameters.Add("@POCPrimaryFax", SqlDbType.NVarChar).Value = tbFax.Text
            insertCommand.Parameters.Add("@POCPrimaryEmail", SqlDbType.NVarChar).Value = tbemail.Text
            insertCommand.Parameters.Add("@POCAddress1", SqlDbType.NVarChar).Value = tbAddress1.Text
            insertCommand.Parameters.Add("@POCAddress2", SqlDbType.NVarChar).Value = tbAddress2.Text
            insertCommand.Parameters.Add("@POCCity", SqlDbType.NVarChar).Value = tbCity.Text
            insertCommand.Parameters.Add("@POCState", SqlDbType.NVarChar).Value = dlState.SelectedValue
            insertCommand.Parameters.Add("@POCZip", SqlDbType.NVarChar).Value = tbZip.Text
            insertCommand.Parameters.Add("@POCWeb", SqlDbType.NVarChar).Value = tbWebPage.Text
            insertCommand.Parameters.Add("@Comments", SqlDbType.NText).Value = tbComments.Text

            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@Date_Entered", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@Date_Modified", SqlDbType.DateTime).Value = DateTime.Now

            conn.Open()
            insertCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("Offer creation failed. " & ex.Message)
            errorText = ex.Message
        Finally
            conn.Close()
        End Try
        If errorText = "" Then
            Response.Redirect("CM_Splash.htm")
        End If
       
Finish:
    End Sub

    Private Sub btnOpenContract_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOpenContract.Click
        Response.Redirect("CM_Splash.htm")
    End Sub
End Class