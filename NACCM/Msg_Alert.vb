Public Class Msg_Alert
    Public Shared Sub Client_Alert_OnLoad(ByVal sMessage As String, Optional ByVal sURL As String = "")
        On Error Resume Next
        Dim str As String
        Dim P As System.Web.UI.Page = CType(System.Web.HttpContext.Current.Handler, System.Web.UI.Page)
        Dim sb As New StringBuilder(Len(sMessage) * 5)
        sMessage = sMessage.Replace(Chr(0), "")
        For Each c As String In sMessage : sb.Append("\x" & Right("0" & Hex(Asc(c)), 2)) : Next
        str = vbCrLf & "<script language=javascript>" & vbCrLf
        str = str & "    alert('" & sb.ToString & "');" & vbCrLf
        If Len(sURL) > 0 Then str = str & "    window.location='" & sURL & "';" & vbCrLf
        str = str & "</script>" & vbCrLf
        P.ClientScript.RegisterStartupScript(P.GetType, "", str)
    End Sub
End Class
