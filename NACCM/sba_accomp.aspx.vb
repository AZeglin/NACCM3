Imports System.Data
Imports System.Data.SqlClient

Imports VA.NAC.NACCMBrowser.BrowserObj

Imports TextBox = System.Web.UI.WebControls.TextBox

Partial Public Class sba_accomp
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim accomplishmentId As Integer = -1
        Dim sbaPlanID As Integer = -1
        Dim isInsert As Boolean = False

        If Not IsPostBack Then
            If Not Request.QueryString("State") Is Nothing Then
                If CType(Request.QueryString("state"), Integer) = 1 Then
                    fvSBAAccomplish.ChangeMode(FormViewMode.Edit)
                End If
            End If
            If Not Request.QueryString("Insert") Is Nothing Then
                If Request.QueryString("Insert").ToString = "Y" Then
                    fvSBAAccomplish.DefaultMode = FormViewMode.Insert
                    isInsert = True
                End If
            End If

            If isInsert = True Then
                If Not Request.QueryString("SBAPlanID") Is Nothing Then
                    sbaPlanID = Request.QueryString("SBAPlanID")
                    Session("AccomplishmentSBAPlanID") = sbaPlanID
                End If
            Else
                If Not Request.QueryString("ACCID") Is Nothing Then
                    accomplishmentId = Request.QueryString("ACCID")
                    Session("AccomplishmentAccomplishmentID") = accomplishmentId
                    DataLoad(accomplishmentId)
                End If
            End If
        Else
            accomplishmentId = Session("AccomplishmentAccomplishmentID")
            ' DataLoad(accomplishmentId)
        End If

        DisableSBAAccomplishmentsEditControls()

    End Sub

    Protected Sub DisableSBAAccomplishmentsEditControls()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim btnSaveEdit As Button
        btnSaveEdit = CType(fvSBAAccomplish.FindControl("btnSaveEdit"), Button)
        If Not btnSaveEdit Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True  Then
                btnSaveEdit.Enabled = True
                btnSaveEdit.Visible = True
            Else
                btnSaveEdit.Enabled = False
                btnSaveEdit.Visible = False
            End If

        End If  'insertProjectionButton

        Dim btnSaveInsert As Button
        btnSaveInsert = CType(fvSBAAccomplish.FindControl("updateProjectionButton"), Button)
        If Not btnSaveInsert Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True  Then
                btnSaveInsert.Enabled = True
                btnSaveInsert.Visible = True
            Else
                btnSaveInsert.Enabled = False
                btnSaveInsert.Visible = False
            End If

        End If

    End Sub

    Protected Function CalTotalDollars(ByVal LargeDollars As String, ByVal SmallDollars As String) As String
        Dim strTotal As String = ""
        Dim DbLargeDollars As Double
        Dim DbSmallDollars As Double
        If LargeDollars <> "" Then
            DbLargeDollars = CType(LargeDollars, Double)
        Else
            DbLargeDollars = 0
        End If
        If SmallDollars <> "" Then
            DbSmallDollars = CType(SmallDollars, Double)
        Else
            DbSmallDollars = 0
        End If
        Dim DbTotal As Double = DbLargeDollars + DbSmallDollars
        strTotal = FormatCurrency(DbTotal)
        Return strTotal
    End Function

    Protected Function CalPercentDollars(ByVal GroupDollars As String, ByVal LargeDollars As String, ByVal SmallDollars As String) As String
        Dim strPercent As String = ""
        Dim dbGroupDollars As Double
        Dim DbSmallDollars As Double
        Dim DbLargeDollars As Double
        If GroupDollars <> "" Then
            dbGroupDollars = CType(GroupDollars, Double)
        Else
            dbGroupDollars = 0
        End If
        If LargeDollars <> "" Then
            DbLargeDollars = CType(LargeDollars, Double)
        Else
            DbLargeDollars = 0
        End If
        If SmallDollars <> "" Then
            DbSmallDollars = CType(SmallDollars, Double)
        Else
            DbSmallDollars = 0
        End If
        Dim DbTotal As Double = dbGroupDollars / (DbLargeDollars + DbSmallDollars)
        strPercent = FormatPercent(DbTotal, 2)
        Return strPercent
    End Function
    Protected Function CalPercentToVA(ByVal GroupDollars As String, ByVal VARatio As String) As String
        Dim strPercent As String = ""
        Dim dbGroupDollars As Double
        Dim DbVaRatio As Double
        If GroupDollars <> "" Then
            dbGroupDollars = CType(GroupDollars, Double)
        Else
            dbGroupDollars = 0
        End If
        If VARatio <> "" Then
            DbVaRatio = CType(VARatio, Double)
        Else
            DbVaRatio = 0
        End If
        Dim DbTotal As Double = dbGroupDollars * DbVaRatio
        strPercent = FormatCurrency(DbTotal, 2)
        Return strPercent
    End Function
    Protected Function CalTotalToVA(ByVal LargeDollars As String, ByVal SmallDollars As String, ByVal VARatio As String) As String
        Dim strPercent As String = ""
        Dim dbGroupDollars As Double
        Dim DbVaRatio As Double
        dbGroupDollars = CType(CalTotalDollars(LargeDollars, SmallDollars), Double)
        If VARatio <> "" Then
            DbVaRatio = CType(VARatio, Double)
        Else
            DbVaRatio = 0
        End If
        Dim DbTotal As Double = dbGroupDollars * DbVaRatio
        strPercent = FormatCurrency(DbTotal, 2)
        Return strPercent
    End Function
    Protected Sub CloseWindow(ByVal sender As Object, ByVal e As System.EventArgs)
        Response.Write("<script>window.close()</script>")
    End Sub



    Protected Sub btnSaveEdit_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim vaRatio As Decimal
        Dim myDropDown As DropDownList
        Dim myTextBox As TextBox
        Dim strSQL As String = "UPDATE [tbl_sba_Accomplishments]" _
        & "SET Fiscal_Year=@Fiscal_Year,Accomplishment_Period=@Accomplishment_Period,Comments=@Comments," _
        & "VA_Ratio=@VA_Ratio,SmallBusDollars=@SmallBusDollars,LargeBusinessDollars=@LargeBusinessDollars," _
        & "SDBDollars=@SDBDollars,WomanOwnedDollars=@WomanOwnedDollars,VetOwnedDollars=@VetOwnedDollars," _
        & "DisabledVetDollars=@DisabledVetDollars,HubZoneDollars=@HubZoneDollars,HBDCDollars=@HBDCDollars, LastModifiedBy=@LastModifiedBy, LastModificationDate=@LastModificationDate " _
        & "WHERE ([Acc_Record_ID] = @Acc_Record_ID)"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim UpdateCommand As SqlCommand = New SqlCommand()
        Try
            UpdateCommand.Connection = conn
            UpdateCommand.CommandText = strSQL
            UpdateCommand.Parameters.Add("@Acc_Record_ID", SqlDbType.Int).Value = CType(Request.QueryString("AccID"), Integer)
            myDropDown = CType(fvSBAAccomplish.FindControl("dlFiscalYear"), DropDownList)
            If Not myDropDown Is Nothing Then
                UpdateCommand.Parameters.Add("@Fiscal_Year", SqlDbType.Int).Value = CType(myDropDown.SelectedValue, Integer)
            End If
            myDropDown = CType(fvSBAAccomplish.FindControl("dlAccompPeriod"), DropDownList)
            If Not myDropDown Is Nothing Then
                UpdateCommand.Parameters.Add("@Accomplishment_Period", SqlDbType.NVarChar).Value = myDropDown.SelectedValue.ToString
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbCommentsEdit"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = myTextBox.Text
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbVARatio"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If

                vaRatio = Decimal.Parse(myTextBox.Text)
                If vaRatio < 1 Then
                    UpdateCommand.Parameters.Add("@VA_Ratio", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
                Else
                    Msg_Alert.Client_Alert_OnLoad("Please express the VA Percentage as a number less than 1.")
                    Return
                End If
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbSBConDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@SmallBusDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbLBConDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@LargeBusinessDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbSDBDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@SDBDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbWODollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@WomanOwnedDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbVetDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@VetOwnedDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbDVetDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@DisabledVetDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbHubDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@HubZoneDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbHBDCDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                UpdateCommand.Parameters.Add("@HBDCDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If

            Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
            Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
            LastModificationDateParm.Value = DateTime.Now

            UpdateCommand.Parameters.Add(LastModifiedByParm)
            UpdateCommand.Parameters.Add(LastModificationDateParm)

            conn.Open()
            UpdateCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of updating SBA Accomplishments: " & ex.ToString)
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub btnSaveInsert_Click(ByVal sender As Object, ByVal e As EventArgs)

        Dim vaRatio As Decimal
        Dim myID As Integer
        Dim myDropDown As DropDownList
        Dim myTextBox As TextBox
        Dim strSQL As String = "INSERT INTO [tbl_sba_Accomplishments]" _
        & "(Fiscal_Year,Accomplishment_Period,Comments,VA_Ratio,SmallBusDollars,LargeBusinessDollars," _
        & "SDBDollars,WomanOwnedDollars,VetOwnedDollars,DisabledVetDollars,HubZoneDollars,HBDCDollars,SBAPlanID, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate) " _
        & "VALUES(@Fiscal_Year,@Accomplishment_Period,@Comments," _
        & "@VA_Ratio,@SmallBusDollars,@LargeBusinessDollars," _
        & "@SDBDollars,@WomanOwnedDollars,@VetOwnedDollars," _
        & "@DisabledVetDollars,@HubZoneDollars,@HBDCDollars,@SBAPlanID, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate); " _
        & " SELECT @AccID = Scope_IDENTITY()"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim InsertCommand As SqlCommand = New SqlCommand()
        Try
            InsertCommand.Connection = conn
            InsertCommand.CommandText = strSQL
            InsertCommand.Parameters.Add("@SBAPlanID", SqlDbType.Int).Value = CType(Request.QueryString("SBAPlanID"), Integer)
            myDropDown = CType(fvSBAAccomplish.FindControl("dlFiscalYear"), DropDownList)
            If Not myDropDown Is Nothing Then
                InsertCommand.Parameters.Add("@Fiscal_Year", SqlDbType.Int).Value = CType(myDropDown.SelectedValue, Integer)
            End If
            myDropDown = CType(fvSBAAccomplish.FindControl("dlAccompPeriod"), DropDownList)
            If Not myDropDown Is Nothing Then
                InsertCommand.Parameters.Add("@Accomplishment_Period", SqlDbType.NVarChar).Value = myDropDown.SelectedValue.ToString
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbCommentsEdit"), TextBox)
            If Not myTextBox Is Nothing Then
                InsertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = myTextBox.Text
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbVARatio"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If

                vaRatio = Decimal.Parse(myTextBox.Text)
                If vaRatio < 1 Then
                    InsertCommand.Parameters.Add("@VA_Ratio", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
                Else
                    Msg_Alert.Client_Alert_OnLoad("Please express the VA Percentage as a number less than 1.")
                    Return
                End If

            End If

            myTextBox = CType(fvSBAAccomplish.FindControl("tbSBConDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@SmallBusDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbLBConDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@LargeBusinessDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbSDBDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@SDBDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbWODollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@WomanOwnedDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbVetDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@VetOwnedDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbDVetDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@DisabledVetDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbHubDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@HubZoneDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If
            myTextBox = CType(fvSBAAccomplish.FindControl("tbHBDCDollars"), TextBox)
            If Not myTextBox Is Nothing Then
                If myTextBox.Text = "" Then
                    myTextBox.Text = "0"
                End If
                InsertCommand.Parameters.Add("@HBDCDollars", SqlDbType.Decimal).Value = CType(myTextBox.Text, Decimal)
            End If

            Dim myAccID As SqlParameter = New SqlParameter("@AccID", SqlDbType.Int)
                myAccID.Direction = ParameterDirection.Output

            InsertCommand.Parameters.Add(myAccID)

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

            InsertCommand.Parameters.Add(CreatedByParm)
            InsertCommand.Parameters.Add(CreationDateParm)
            InsertCommand.Parameters.Add(LastModifiedByParm)
            InsertCommand.Parameters.Add(LastModificationDateParm)

            InsertCommand.Connection.Open()
            InsertCommand.ExecuteNonQuery()

            myID = CType(myAccID.Value, Integer)

            Session("AccomplishmentAccomplishmentID") = myID

        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of updating SBA Accomplishments: " & ex.ToString)
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub DataLoad(ByVal accomplishmentId As Integer)
        Dim strSQL As String = "SELECT * FROM [tbl_sba_Accomplishments] WHERE ([Acc_Record_ID] = @Acc_Record_ID)"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader

        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            cmd.Parameters.Add("@Acc_Record_ID", SqlDbType.Int).Value = accomplishmentId
            rdr = cmd.ExecuteReader
            If rdr.HasRows Then
                fvSBAAccomplish.DataSource = rdr
                fvSBAAccomplish.DataBind()
            End If

        Catch ex As Exception
            'MsgBox("This is error loading the data. " & ex.ToString, MsgBoxStyle.OkOnly)
        Finally
            conn.Close()
        End Try
    End Sub

    'Private Sub form1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles form1.PreRender
    '    DataLoad()
    'End Sub

    Private Sub fvSBAAccomplish_ItemInserting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.FormViewInsertEventArgs) Handles fvSBAAccomplish.ItemInserting
        'fvSBAAccomplish.ChangeMode(FormViewMode.Edit)
        'fvSBAAccomplish.DataBind()
    End Sub

    Private Sub fvSBAAccomplish_ItemUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.FormViewUpdateEventArgs) Handles fvSBAAccomplish.ItemUpdating
        'fvSBAAccomplish.ChangeMode(FormViewMode.Edit)
        'fvSBAAccomplish.DataBind()
    End Sub




End Class