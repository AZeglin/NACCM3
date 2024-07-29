Public Partial Class sba_expiring_excel
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim myContractNumber As String = CType(Request.QueryString("CntrctNum"), String)
        Dim TitleName As String = "SBA_Expiring_" & Date.Now.ToShortDateString
        Dim myHeader As String = "attachment;filename=" & TitleName & ".xls"
        Try
            Response.ContentType = "application/vnd.ms-excel"
            Response.AddHeader("content-disposition", myHeader)
            Response.Charset = ""
            Me.EnableViewState = "False"
            Dim tw As System.IO.StringWriter = Nothing
            Dim hw As New System.Web.UI.HtmlTextWriter(tw)
            gvExpiringSBA.RenderControl(hw)
            Response.Write(tw.ToString)
        Catch ex As Exception
        End Try
    End Sub

End Class