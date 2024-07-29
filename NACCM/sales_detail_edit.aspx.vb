Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.Common

Imports VA.NAC.NACCMBrowser.BrowserObj

Partial Public Class sales_detail_edit
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Protected Sub btn_Close(ByVal s As Object, ByVal e As EventArgs)
        Dim closeScript As String = "window.opener.RefreshSalesDataGrid();window.close();"
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "CloseWindowScript", closeScript, True)
        'Response.Write("<script>window.opener.document.getElementbyId('hfRefreshSales'.Value=An)</script>")
        'Dim strScript As String = "<script type='text/javascript'>window.opener.document.form1.hfRefreshSales.Value=now;"
        ' strScript += "self.close()</script>"
        'Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "anything", strScript)
        ' Page.ClientScript.RegisterStartupScript(Me.GetType, "anything", strScript)


    End Sub


    Private Sub gvSalesData_RowUpdated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewUpdatedEventArgs) Handles gvSalesData.RowUpdated
        '  Dim closeScript As String = "window.opener.RefreshSalesDataGrid();window.close();"
        ' ClientScript.RegisterClientScriptBlock(Me.GetType(), "CloseWindowScript", closeScript, True)
    End Sub
    Protected Sub SalesDataSource_OnUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs)
        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        e.Command.Parameters.Add(New SqlParameter("LastModifiedBy", SqlDbType.NVarChar, 120, ParameterDirection.Input, False, 0, 0, "LastModifiedBy", DataRowVersion.Default, browserSecurity.UserInfo.LoginName))
        e.Command.Parameters.Add(New SqlParameter("LastModificationDate", SqlDbType.DateTime, 8, ParameterDirection.Input, False, 0, 0, "LastModificationDate", DataRowVersion.Default, DateTime.Now.ToString()))

    End Sub
End Class