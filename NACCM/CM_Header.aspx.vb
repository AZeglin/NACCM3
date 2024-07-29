Imports System
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Configuration
Imports System.Data
Imports System.Text
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Xml.Linq

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.Application.SharedObj
Imports VA.NAC.NACCMBrowser.DBInterface

Partial Public Class CM_Header
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Page.IsPostBack = False Then
            PostBackForValues()
        Else
            CheckSize()
        End If
    End Sub
    ' Public Sub TreeViewForm_OnClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles TreeViewForm.OnClick
    Public Sub CheckSize()

        Dim ClientScreenWidthHiddenField As HiddenField = CType(CMHeaderForm.FindControl("ClientScreenWidth"), HiddenField)
        Dim ClientScreenHeightHiddenField As HiddenField = CType(CMHeaderForm.FindControl("ClientScreenHeight"), HiddenField)

        Dim clientScreenWidth As Integer = 0
        Dim clientScreenHeight As Integer = 0

        If Not ClientScreenWidthHiddenField Is Nothing Then
            clientScreenWidth = CType(ClientScreenWidthHiddenField.Value, Integer)
        End If

        If Not ClientScreenHeightHiddenField Is Nothing Then
            clientScreenHeight = CType(ClientScreenHeightHiddenField.Value, Integer)
        End If

        Dim cmGlobals As CMGlobals

        If Not Session("CMGlobals") Is Nothing Then
            cmGlobals = CType(Session("CMGlobals"), CMGlobals)

            cmGlobals.ClientScreenWidth = clientScreenWidth
            cmGlobals.ClientScreenHeight = clientScreenHeight
        End If

    End Sub

    Public Sub PostBackForValues()
        Dim callPostBackFunctionScript As String
        callPostBackFunctionScript = "saveClientScreenResolutionInfo();"
        ScriptManager.RegisterStartupScript(Me.Page, Me.Page.GetType(), "CallPostBackFunctionScript", callPostBackFunctionScript, True)
    End Sub


    Protected Sub CMHeaderScriptManager_OnAsyncPostBackError(ByVal sender As Object, ByVal e As AsyncPostBackErrorEventArgs)

        Dim errorMsg As String = ""

        If Not e.Exception.Data("CMHeaderErrorMessage") Is Nothing Then
            errorMsg = String.Format("The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data("CMHeaderErrorMessage"))
        Else
            errorMsg = String.Format("The following error was encountered during async postback: {0}", e.Exception.Message)
        End If
     
        CMHeaderScriptManager.AsyncPostBackErrorMessage = errorMsg

    End Sub
End Class