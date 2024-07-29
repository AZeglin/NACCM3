Imports System.Data
Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj
Imports System.Data.SqlClient

Imports TextBox = System.Web.UI.WebControls.TextBox


Partial Public Class sba_projections_add
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' this should be caught prior to load by js add function
        Dim sbaPlanIDString As String = Request.QueryString("SBAPlanID")
        If sbaPlanIDString Is Nothing Then
            Msg_Alert.Client_Alert_OnLoad("Please select an SBA Plan prior to entering projections.")
        End If
        If sbaPlanIDString.Length = 0 Then
            Msg_Alert.Client_Alert_OnLoad("Please select an SBA Plan prior to entering projections.")
        End If
    End Sub
    Protected Sub CalInsertPercentage(ByVal send As Object, ByVal e As EventArgs)
        Dim myLabel As String = send.ID.ToString
        Dim myTotal As Double = CType(tbTotalDollars.Text, Double)
        Dim myPercent As Double = 0
        If myLabel = "tbDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbSBPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbSDBDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbSDBPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbWODollar" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbWOPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbDVDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbDVPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbHubDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbHubPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbHBDollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbHBPercent.Text = FormatPercent(myPercent, 2).ToString()
        ElseIf myLabel = "tbVODollars" Then
            Dim myTextbox As TextBox = CType(send, TextBox)
            Dim myShare As Double = CType(myTextbox.Text, Double)
            myPercent = myShare / myTotal
            tbVOPercent.Text = FormatPercent(myPercent, 2).ToString()
        End If

    End Sub
    Protected Sub btnClose_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Response.Write("<script>window.close()</script>")
    End Sub

    Private Sub btnInsert_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnInsert.Click

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Dim strSQL As String = "INSERT INTO tbl_sba_projection (SBDollars, SDBDollars, WomenOwnedDollars,DisabledVetDollars, HubZoneDollars, HBCUDollars, VeteranOwnedDollars, TotalSubConDollars, Comments, EndDate, StartDate, SBAPlanID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate)" _
    & " VALUES(@SBDollars, @SDBDollars, @WomanOwnedDollars,@DisabledVetDollars, @HubZoneDollars, @HBCUDollars, @VeteranOwnedDollars, @TotalSubDollars, @Comments, @EndDate, @StartDate,@SBAPlanID, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate)"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim insertCommand As SqlCommand = New SqlCommand()
        If tbDollars.Text = "" Then
            tbDollars.Text = "0"
        End If
        If tbSDBDollars.Text = "" Then
            tbSDBDollars.Text = "0"
        End If
        If tbWODollar.Text = "" Then
            tbWODollar.Text = "0"
        End If
        If tbDVDollars.Text = "" Then
            tbDVDollars.Text = "0"
        End If
        If tbHubDollars.Text = "" Then
            tbHubDollars.Text = "0"
        End If
        If tbHBDollars.Text = "" Then
            tbHBDollars.Text = "0"
        End If
        If tbVODollars.Text = "" Then
            tbVODollars.Text = "0"
        End If
        If tbTotalDollars.Text = "" Then
            tbTotalDollars.Text = "0"
        End If
        Try
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL
            insertCommand.Parameters.Add("@SBDollars", SqlDbType.Money).Value = CType(tbDollars.Text, Double)
            insertCommand.Parameters.Add("@SDBDollars", SqlDbType.Money).Value = CType(tbSDBDollars.Text, Double)
            insertCommand.Parameters.Add("@WomanOwnedDollars", SqlDbType.Money).Value = CType(tbWODollar.Text, Double)
            insertCommand.Parameters.Add("@DisabledVetDollars", SqlDbType.Money).Value = CType(tbDVDollars.Text, Double)
            insertCommand.Parameters.Add("@HubZoneDollars", SqlDbType.Money).Value = CType(tbHubDollars.Text, Double)
            insertCommand.Parameters.Add("@HBCUDollars", SqlDbType.Money).Value = CType(tbHBDollars.Text, Double)
            insertCommand.Parameters.Add("@VeteranOwnedDollars", SqlDbType.Money).Value = CType(tbVODollars.Text, Double)
            insertCommand.Parameters.Add("@TotalSubDollars", SqlDbType.Money).Value = CType(tbTotalDollars.Text, Double)
            insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = CType(tbComments.Text, String)
            insertCommand.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = CType(tbEnddate.Text, DateTime)
            insertCommand.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = CType(tbStartDate.Text, DateTime)
            insertCommand.Parameters.Add("@SBAPlanID", SqlDbType.Int).Value = CType(Request.QueryString("SBAPlanID"), Integer)

            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            conn.Open()
            insertCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of the insert. " & ex.ToString)
        Finally
            conn.Close()
        End Try
        'Response.Write("<script>window.close()</script>")
        Dim closeScript As String = "window.opener.RefreshSBAPlanProjections();window.close();"
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "CloseWindowScript", closeScript, True)
    End Sub
End Class