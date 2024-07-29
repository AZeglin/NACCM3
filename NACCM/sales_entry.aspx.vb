Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.Common

Imports VA.NAC.NACCMBrowser.BrowserObj

Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList

Partial Public Class sales_entry
    Inherits System.Web.UI.Page
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        btnSalesCancelFooter.Visible = False
    End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Protected Sub HideModificationCapability()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.Sales) = False  Then
            btnAddFooter.Visible = False
            btnSalesCancelFooter.Visible = False
            gvSales.ShowFooter = False
            gvSales.Columns(7).Visible = False
        End If
    
    End Sub

    Protected Sub insertAddSales(ByVal sender As Object, ByVal e As EventArgs)
        Dim myQtrDDL As DropDownList = CType(gvSales.Controls(0).Controls(0).FindControl("dlQuarterEmpty"), DropDownList)
        Dim mySINDDL As DropDownList = CType(gvSales.Controls(0).Controls(0).FindControl("dlSIN"), DropDownList)
        Dim myVASalesTB As TextBox = CType(gvSales.Controls(0).Controls(0).FindControl("tbVASales"), TextBox)
        Dim myOGASalesTB As TextBox = CType(gvSales.Controls(0).Controls(0).FindControl("tbOGASales"), TextBox)
        Dim mySLGSalesTB As TextBox = CType(gvSales.Controls(0).Controls(0).FindControl("tbSLGSales"), TextBox)
        Dim myCommentsTB As TextBox = CType(gvSales.Controls(0).Controls(0).FindControl("tbComments"), TextBox)
        Dim strSQL As String = "INSERT INTO tbl_Cntrcts_Sales (CntrctNum, SIN, Quarter_ID,VA_Sales, OGA_Sales, SLG_Sales, Comments, LastModifiedBy, LastModificationDate )" _
        & " VALUES(@CntrctNum, @SIN, @QuarterID, @VASales, @OGASales, @SGLSales, @Comments, @LastModifiedBy, @LastModificationDate)"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim insertCommand As SqlCommand = New SqlCommand()

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Try
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL
            insertCommand.Parameters.Add("@QuarterID", SqlDbType.Int).Value = myQtrDDL.SelectedValue
            insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = Request.QueryString("CntrctNum").ToString
            insertCommand.Parameters.Add("@SIN", SqlDbType.NVarChar).Value = mySINDDL.SelectedValue.ToString
            insertCommand.Parameters.Add("@VASales", SqlDbType.Money).Value = CType(myVASalesTB.Text, Double)
            insertCommand.Parameters.Add("@OGASales", SqlDbType.Money).Value = CType(myOGASalesTB.Text, Double)
            insertCommand.Parameters.Add("@SGLSales", SqlDbType.Money).Value = CType(mySLGSalesTB.Text, Double)
            insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = myCommentsTB.Text

            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()
            conn.Open()

            insertCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of the insert. " & ex.ToString)
        Finally
            conn.Close()
        End Try
        gvSales.DataBind()
    End Sub
    Protected Sub ShowSalesFooter(ByVal S As Object, ByVal e As EventArgs)
        Dim myButton As Button = CType(S, Button)
        If myButton.ID.Equals("btnAddFooter") Then
            gvSales.ShowFooter = "True"
            btnSalesCancelFooter.Visible = "True"
            btnAddFooter.Visible = "False"
        ElseIf myButton.ID.Equals("btnSalesCancelFooter") Then
            gvSales.ShowFooter = "False"
            btnSalesCancelFooter.Visible = "False"
            btnAddFooter.Visible = "True"
        End If

    End Sub

    'Private Sub btnClose_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClose.Click
    '    If Request.QueryString("Page") = 1 Then
    '        Response.Redirect("NAC_CM_Edit.aspx?CntrctNum=" & Request.QueryString("CntrctNum") & "&SchNum=" & Request.QueryString("SchNum").ToString)
    '    ElseIf Request.QueryString("Page") = 2 Then
    '        Response.Redirect("NAC_CM_Contracts.aspx?CntrctNum=" & Request.QueryString("CntrctNum") & "&SchNum=" & Request.QueryString("SchNum").ToString)
    '    ElseIf Request.QueryString("Page") = 3 Then
    '        Response.Redirect("NAC_CM_BPA_Edit.aspx?CntrctNum=" & Request.QueryString("CntrctNum").ToString & "&SchNum=" & Request.QueryString("SchNum").ToString)
    '    End If
    'End Sub

    Private Sub gvSales_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles gvSales.RowCommand

        If e.CommandName = "Insert" Then
            Dim myRow As GridViewRow = gvSales.FooterRow
            Dim myQtrDDL As DropDownList = CType(myRow.FindControl("dlQuarterAdd"), DropDownList)
            Dim mySINDDL As DropDownList = CType(myRow.FindControl("dlSINAdd"), DropDownList)
            Dim myVASalesTB As TextBox = CType(myRow.FindControl("tbVASalesAdd"), TextBox)
            Dim myOGASalesTB As TextBox = CType(myRow.FindControl("tbOGASalesAdd"), TextBox)
            Dim mySLGSalesTB As TextBox = CType(myRow.FindControl("tbSLGSalesAdd"), TextBox)
            Dim myCommentsTB As TextBox = CType(myRow.FindControl("tbComments"), TextBox)
            Dim checkSQL = "SELECT * FROM tbl_Cntrcts_Sales WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "' AND Quarter_ID =" & myQtrDDL.SelectedValue & " AND SIN='" & mySINDDL.SelectedValue & "'"
            Dim strSQL As String = "INSERT INTO tbl_Cntrcts_Sales (CntrctNum, SIN, Quarter_ID,VA_Sales, OGA_Sales, SLG_Sales, Comments, LastModifiedBy, LastModificationDate )" _
            & " VALUES(@CntrctNum, @SIN, @QuarterID, @VASales, @OGASales, @SGLSales, @Comments, @LastModifiedBy, @LastModificationDate )"
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim insertCommand As SqlCommand = New SqlCommand()
            Dim rdr As SqlDataReader
            Dim mySQLCommand As SqlCommand = New SqlCommand(checkSQL, conn)
            If myVASalesTB.Text = "" Then
                myVASalesTB.Text = "0"
            End If
            If myOGASalesTB.Text = "" Then
                myOGASalesTB.Text = "0"
            End If
            If mySLGSalesTB.Text = "" Then
                mySLGSalesTB.Text = "0"
            End If

            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Try
                Dim recordExist As Boolean = True
                insertCommand.Connection = conn
                insertCommand.CommandText = strSQL
                insertCommand.Parameters.Add("@QuarterID", SqlDbType.Int).Value = myQtrDDL.SelectedValue
                insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = Request.QueryString("CntrctNum").ToString
                insertCommand.Parameters.Add("@SIN", SqlDbType.NVarChar).Value = mySINDDL.SelectedValue.ToString
                insertCommand.Parameters.Add("@VASales", SqlDbType.Money).Value = CType(myVASalesTB.Text, Double)
                insertCommand.Parameters.Add("@OGASales", SqlDbType.Money).Value = CType(myOGASalesTB.Text, Double)
                insertCommand.Parameters.Add("@SGLSales", SqlDbType.Money).Value = CType(mySLGSalesTB.Text, Double)
                insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = CType(myCommentsTB.Text, String)
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
                insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()

                conn.Open()
                rdr = mySQLCommand.ExecuteReader
                If Not rdr.HasRows Then
                    recordExist = False
                Else
                    Msg_Alert.Client_Alert_OnLoad("The period and SIN are already entered.  Please delete and reeneter.")
                End If
                conn.Close()
                conn.Open()
                If Not recordExist Then
                    insertCommand.ExecuteNonQuery()
                End If
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the insert. " & ex.ToString)
            Finally
                conn.Close()
            End Try
            gvSales.DataBind()
        ElseIf e.CommandName = "Delete" Then
            Dim rowIndex As Integer = Convert.ToInt32(e.CommandArgument)
            Dim selectedRow As GridViewRow = gvSales.Rows(rowIndex)
            Dim selectedDataKey As DataKey = gvSales.DataKeys(rowIndex)

            Dim contractNumber As String = selectedDataKey.Values("CntrctNum").ToString()
            Dim quarterId As String = selectedDataKey.Values("Quarter_ID").ToString()
            Dim salesSIN As String = selectedDataKey.Values("SIN").ToString()

            Dim deleteSQL As String
            deleteSQL = "delete tbl_Cntrcts_Sales where CntrctNum = '" & contractNumber & "' and Quarter_ID =" & quarterId & " and SIN='" & salesSIN & "'"
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim deleteCommand As SqlCommand = New SqlCommand(deleteSQL, conn)
            Try
                conn.Open()
                deleteCommand.ExecuteNonQuery()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the delete. " & ex.ToString)
            Finally
                conn.Close()
            End Try
            gvSales.DataBind()
        End If
    End Sub

    Private Sub sales_entry_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        HideModificationCapability()
    End Sub
End Class