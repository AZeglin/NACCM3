Public Partial Class CM_Home
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'If Not Page.PreviousPage Is Nothing Then
        '    Dim referringPageName As String = Page.PreviousPage.Request.Url.AbsoluteUri  'Request.ServerVariables("HTTP_REFERER").ToString()
        '    If referringPageName.Contains("NCM.aspx") <> True Then
        '        Response.StatusCode = System.Net.HttpStatusCode.Forbidden
        '        Response.BufferOutput = True
        '        Response.Redirect("403A.htm")
        '        Return
        '    End If
        'Else
        '    Response.StatusCode = System.Net.HttpStatusCode.Forbidden
        '    Response.BufferOutput = True
        '    Response.Redirect("403A.htm")
        'End If

        If Session("NACCMStartedProperly") Is Nothing Then
            Response.StatusCode = System.Net.HttpStatusCode.Forbidden
            Response.BufferOutput = True
            Response.Redirect("403A.htm") ' forbidden error
        End If


        If Not IsPostBack Then
            Session("UserName") = getUserName()
        End If
    End Sub

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

End Class
