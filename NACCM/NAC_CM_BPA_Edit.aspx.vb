Imports System.Data
Imports System.Data.SqlClient
'Imports Excel = Microsoft.Office.Interop.Excel
Imports System.Runtime.InteropServices

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj
Imports VA.NAC.ReportManager

Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList


Partial Public Class NAC_CM_BPA_Edit
    Inherits System.Web.UI.Page
    Protected myEditable As String
    Protected myScheduleNumber As Integer
    Protected myContractNumber As String
    Protected insertParameters(3) As SqlParameter
    Protected insertStateParameters(2) As SqlParameter
    Protected insertCheckParameters(7) As SqlParameter

    Private Sub NAC_CM_Edit_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Session("Requested") = Nothing
        Session("CurrentViewIndex") = Nothing
        Session("CntrctNum") = Nothing
        Session("Editable") = Nothing
    End Sub

    
    Protected Sub form1_oninit(ByVal sender As Object, ByVal e As System.EventArgs)
 

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim view As MultiView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)
        If Not IsPostBack Then
            Session("CurrentSelectedSBAPlanId") = Nothing
         
            Session("Requested") = "Contract"
            Dim myrefer_page As String = ""
            If Request.ServerVariables("HTTP_REFERER") Is Nothing Then
                If Session("NACCM") Is Nothing Then
                    Response.Redirect("Old1NCM.aspx")
                Else
                    myrefer_page = "/contract_search.aspx"
                End If
            Else
                myrefer_page = Request.ServerVariables("HTTP_REFERER").ToString
            End If
            Dim myIndex As Integer = myrefer_page.LastIndexOf("/") + 1
            myrefer_page = myrefer_page.Substring(myIndex)
            If Not myrefer_page.Contains("contract_search.aspx") Then
                If Not myrefer_page.Contains("CreateContract") Then '("contract_addition") Then
                    If Not myrefer_page.Contains("offer_search") Then
                        If Not myrefer_page.Contains("NAC_Offers") Then
                            If Not myrefer_page.Contains("sales_entry") Then
                                Response.Redirect("Old1NCM.aspx")
                            End If
                        End If
                    End If
                End If
            End If
            btnContractSearch.Text = "Contract Search" & vbCrLf & "Without Saving"
            btnMainMenu.Text = "Main Menu" & vbCrLf & "Without Saving"
            btnExit.Text = "Exit NAC CM" & vbCrLf & "Without Saving"
            If Not view Is Nothing Then
                If myrefer_page.Contains("sales_entry") Then
                    view.ActiveViewIndex = 7
                    setButtonColors("btnSales")
                Else
                    view.ActiveViewIndex = 0
                End If

            End If
            Dim myScheduleNumber As Integer = 0
            Dim myContractNumber As String = ""
            If Not Request.QueryString("SchNum") Is Nothing Then
                myScheduleNumber = CInt(Request.QueryString("SchNum"))
            End If
            If Not Request.QueryString("CntrctNum") Is Nothing Then
                myContractNumber = Request.QueryString("CntrctNum")
            End If
            'setControls()
            setScheduleAttributes(myScheduleNumber)

            setBtnAttributes()
            dataSBAPlanTypes()

        End If

        CMGlobals.AddKeepAlive(Me.Page, 30000)

        If IsPostBack Then
            Dim refreshDateType As String = ""
            Dim refreshOrNot As Boolean = False

            Dim refreshDateValueOnSubmitHiddenField As HiddenField = CType(fvContractInfo.FindControl("RefreshDateValueOnSubmit"), HiddenField)
            Dim refreshOrNotOnSubmitHiddenField As HiddenField = CType(fvContractInfo.FindControl("RefreshOrNotOnSubmit"), HiddenField)

            If Not refreshDateValueOnSubmitHiddenField Is Nothing Then

                refreshDateType = refreshDateValueOnSubmitHiddenField.Value
                If Not refreshOrNotOnSubmitHiddenField Is Nothing Then

                    refreshOrNot = Boolean.Parse(refreshOrNotOnSubmitHiddenField.Value)

                    If refreshDateType.Contains("Undefined") = False Then
                        If refreshOrNot = True Then
                            RefreshDate(refreshDateType)
                        Else
                            ' reset date
                            Session(refreshDateType) = Session("CalendarInitialDate")
                        End If
                        refreshDateValueOnSubmitHiddenField.Value = "Undefined"
                        refreshOrNotOnSubmitHiddenField.Value = "False"
                    End If
                End If
            End If
        End If

        setUpSalesFun()
        SetupSalesExport()
        If (IsPostBack) Then
            Dim refreshGrid As String
            Dim refreshSalesDataGridOnSubmitHiddenField As HiddenField = CType(fvContractInfo.Row.FindControl("RefreshSalesDataGridOnSubmit"), HiddenField)
            If Not refreshSalesDataGridOnSubmitHiddenField Is Nothing Then
                refreshGrid = refreshSalesDataGridOnSubmitHiddenField.Value
                If refreshGrid.Contains("true") Then
                    refreshSalesDataGridOnSubmitHiddenField.Value = "false"
                    RefreshSalesDataGrid()
                    RefreshPOCView()
                End If

            End If

            Dim refreshPricelist As String
            Dim refreshPricelistScreenOnSubmit As HiddenField = CType(fvContractInfo.Row.FindControl("RefreshPricelistScreenOnSubmit"), HiddenField)

            If Not refreshPricelistScreenOnSubmit Is Nothing Then
                refreshPricelist = refreshPricelistScreenOnSubmit.Value
                If refreshPricelist.Contains("true") Then
                    refreshPricelistScreenOnSubmit.Value = "false"
                    UpdateItemCounts()
                End If
            End If
        End If

        If Not Session("CurrentViewIndex") Is Nothing Then
            If Not view Is Nothing Then
                view.ActiveViewIndex = CType(Session("CurrentViewIndex"), Integer)
            End If
        End If

    End Sub

    Public Function GetDateButtonScript(ByVal dateTypeString As String) As String

        Dim defaultDateString As String = ""

        If Not Session(dateTypeString) Is Nothing Then
            defaultDateString = CType(Session(dateTypeString), String)
        Else
            defaultDateString = CType(DateTime.Today.ToShortDateString(), String)
        End If

        Dim script As String = String.Format("function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=160,height=310'); return false;}}", dateTypeString, defaultDateString, dateTypeString)
        Return (script)
    End Function

    Public Sub RefreshDate(ByVal dateTypeString As String)

        Dim displayDate As DateTime

        If dateTypeString.Contains("CAward") = True Then
            Dim lbAwardDate As Label = CType(fvContractInfo.FindControl("lbAwardDate"), Label)
            If Not Session("CAward") Is Nothing Then
                displayDate = DateTime.Parse(Session("CAward").ToString())
                lbAwardDate.Text = displayDate.ToShortDateString()
            Else
                lbAwardDate.Text = "x"
            End If
        End If

        If dateTypeString.Contains("CEffective") = True Then
            Dim lbEffectiveDate As Label = CType(fvContractInfo.FindControl("lbEffectiveDate"), Label)
            If Not Session("CEffective") Is Nothing Then
                displayDate = DateTime.Parse(Session("CEffective").ToString())
                lbEffectiveDate.Text = displayDate.ToShortDateString()
            Else
                lbEffectiveDate.Text = "x"
            End If
        End If

        If dateTypeString.Contains("CExpiration") = True Then
            Dim ExpirationDateTextBox As TextBox = CType(fvContractInfo.FindControl("ExpirationDateTextBox"), TextBox)
            If Not Session("CExpiration") Is Nothing Then
                displayDate = DateTime.Parse(Session("CExpiration").ToString())
                ExpirationDateTextBox.Text = displayDate.ToShortDateString()
            Else
                ExpirationDateTextBox.Text = "x"
            End If
        End If

        If dateTypeString.Contains("CCompletion") = True Then
            Dim CompletionDateTextBox As TextBox = CType(fvContractInfo.FindControl("CompletionDateTextBox"), TextBox)
            If Not Session("CCompletion") Is Nothing Then
                displayDate = DateTime.Parse(Session("CCompletion").ToString())
                CompletionDateTextBox.Text = displayDate.ToShortDateString()
            Else
                CompletionDateTextBox.Text = "x"
            End If
        End If

    End Sub

    Protected Sub EffectiveDate_OnDataBinding(ByVal s As Object, ByVal e As EventArgs)
        Dim EffectiveDateLabel As Label = CType(s, Label)

        Dim fvContractInfo As FormView = CType(EffectiveDateLabel.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        Dim effectiveDateString As String
        If Not currentRow.Item("Dates_Effective").Equals(DBNull.Value) Then
            effectiveDateString = FormatDateTime(currentRow.Item("Dates_Effective"), DateFormat.ShortDate).ToString
            Session("CEffective") = effectiveDateString
        End If

    End Sub


    Protected Sub AwardDate_OnDataBinding(ByVal s As Object, ByVal e As EventArgs)
        Dim AwardDateLabel As Label = CType(s, Label)

        Dim fvContractInfo As FormView = CType(AwardDateLabel.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        Dim awardDateString As String
        If Not currentRow.Item("Dates_CntrctAward").Equals(DBNull.Value) Then
            awardDateString = FormatDateTime(currentRow.Item("Dates_CntrctAward"), DateFormat.ShortDate).ToString
            Session("CAward") = awardDateString

            'save the value to the CurrentDocument
            Dim currentDocument As CurrentDocument
            If Not Session("CurrentDocument") Is Nothing Then
                currentDocument = CType(Session("CurrentDocument"), CurrentDocument)
                currentDocument.AwardDate = CType(awardDateString, DateTime)
            End If
        End If

    End Sub

    Protected Sub CompletionDate_OnDataBinding(ByVal s As Object, ByVal e As EventArgs)
        Dim CompletionDateTextBox As TextBox = CType(s, TextBox)

        Dim fvContractInfo As FormView = CType(CompletionDateTextBox.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        Dim completionDateString As String
        If Not currentRow.Item("Dates_Completion").Equals(DBNull.Value) Then
            completionDateString = FormatDateTime(currentRow.Item("Dates_Completion"), DateFormat.ShortDate).ToString
            Session("CCompletion") = completionDateString

            'save the value to the CurrentDocument
            Dim currentDocument As CurrentDocument
            If Not Session("CurrentDocument") Is Nothing Then
                currentDocument = CType(Session("CurrentDocument"), CurrentDocument)
                currentDocument.CompletionDate = CType(completionDateString, DateTime)
            End If
        End If

        ' copied this code from old CheckExpired function
        'Dim strSQL As String = "SELECT Dates_Completion FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
        'Dim ds As New DataSet
        'Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        'Dim cmd As SqlCommand
        'Dim rdr As SqlDataReader
        'Dim myExpiration As DateTime

        'Try
        '    conn.Open()
        '    cmd = New SqlCommand(strSQL, conn)
        '    rdr = cmd.ExecuteReader
        '    While rdr.Read
        '        If Not rdr("Dates_Completion").Equals(DBNull.Value) Then
        '            myExpiration = CType(rdr("Dates_Completion"), DateTime)
        '        End If
        '    End While
        '    conn.Close()
        'Catch ex As Exception
        'Finally
        '    conn.Close()
        'End Try

        'Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)

        'If myExpiration = "01/01/0001" Then
        '    CompletionDateTextBox.Text = ""
        'Else
        '    CompletionDateTextBox.Text = myExpiration.ToShortDateString
        'End If


    End Sub

    'this was "CheckExpired"
    Protected Sub ExpirationDate_OnDataBinding(ByVal s As Object, ByVal e As EventArgs)
        Dim ExpirationDateTextBox As TextBox = CType(s, TextBox)

        Dim fvContractInfo As FormView = CType(ExpirationDateTextBox.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        Dim expirationDateString As String
        If Not currentRow.Item("Dates_CntrctExp").Equals(DBNull.Value) Then
            expirationDateString = FormatDateTime(currentRow.Item("Dates_CntrctExp"), DateFormat.ShortDate).ToString
            Session("CExpiration") = expirationDateString

            'save the value to the CurrentDocument
            Dim currentDocument As CurrentDocument
            If Not Session("CurrentDocument") Is Nothing Then
                currentDocument = CType(Session("CurrentDocument"), CurrentDocument)
                currentDocument.ExpirationDate = CType(expirationDateString, DateTime)
            End If
        End If

        ' copied this code from old CheckExpired function
        'Dim strSQL As String = "SELECT Dates_CntrctExp FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
        'Dim ds As New DataSet
        'Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        'Dim cmd As SqlCommand

        'Dim rdr As SqlDataReader
        'Dim myExpiration As DateTime
        'Try
        '    conn.Open()
        '    cmd = New SqlCommand(strSQL, conn)
        '    rdr = cmd.ExecuteReader
        '    While rdr.Read
        '        myExpiration = CType(rdr("Dates_CntrctExp"), DateTime)
        '    End While
        '    conn.Close()
        'Catch ex As Exception
        'Finally
        '    conn.Close()
        'End Try

        'Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
        ''   ExpirationDateDropDownList.Items.Add(mylistItem)
        ''   ExpirationDateDropDownList.SelectedValue = myExpiration.ToShortDateString
        'ExpirationDateTextBox.Text = myExpiration.ToShortDateString


    End Sub

    Protected Sub fvContractInfo_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles fvContractInfo.PreRender
        setBtnAttributes()
        MakeTradeAgreementTableVisible()
        MakeExportViewPricelistButtonsVisible()
        checkContractAssignment()
        EnableContractDateEditing(sender)
        MakeParentFSSContractInfoVisible()
        MakeStandardizedTableVisible()
    End Sub
    Private Sub MakeTradeAgreementTableVisible()
        Dim TradeAgreementTable As HtmlTable
        Dim TAAYesCheckBox As CheckBox
        Dim TAAOtherCheckBox As CheckBox
        Dim TradeAgreementHeaderLabel As Label
        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If currentDocument.ScheduleNumber <> 36 And currentDocument.ScheduleNumber <> 42 Then
            TradeAgreementTable = CType(fvContractInfo.Row.FindControl("TradeAgreementTable"), HtmlTable)
            TradeAgreementTable.Visible = True

            TAAYesCheckBox = CType(fvContractInfo.Row.FindControl("TAAYesCheckBox"), CheckBox)
            TAAOtherCheckBox = CType(fvContractInfo.Row.FindControl("TAAOtherCheckBox"), CheckBox)
            TradeAgreementHeaderLabel = CType(fvContractInfo.Row.FindControl("TradeAgreementHeaderLabel"), Label)
            TAAYesCheckBox.Visible = True
            TAAOtherCheckBox.Visible = True
            TradeAgreementHeaderLabel.Visible = True

            If currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
                TAAYesCheckBox.Enabled = True
                TAAOtherCheckBox.Enabled = True
            End If
        End If
    End Sub
    Private Sub MakeParentFSSContractInfoVisible()
        Dim fvParentFSSContractInfo As FormView
        fvParentFSSContractInfo = CType(fvContractInfo.Row.FindControl("fvParentFSSContractInfo"), FormView)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If currentDocument.HasParent = True Then
            fvParentFSSContractInfo.Visible = True
        Else
            fvParentFSSContractInfo.Visible = False
        End If
    End Sub

    Private Sub MakeStandardizedTableVisible()
        Dim standardizedTable As HtmlTable
        standardizedTable = CType(fvContractInfo.Row.FindControl("StandardizedTable"), HtmlTable)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If currentDocument.CanHaveStandardizedItems(currentDocument.ScheduleNumber) = True Then
            standardizedTable.Visible = True
        Else
            standardizedTable.Visible = False
        End If
    End Sub
    Protected Function checkIfEditable() As String
        Dim currentUser As String = CType(Session("UserName"), String)
        If currentUser.IndexOf("\") > 0 Then
            Dim myStart As Integer = currentUser.IndexOf("\") + 1
            currentUser = currentUser.Substring(myStart)
        End If
        Dim myAD As String = ""
        Dim mySM As String = ""
        Dim myCO As String = ""
        Dim AD As HiddenField = CType(fvContractInfo.Row.FindControl("hfADUser"), HiddenField)
        Dim SM As HiddenField = CType(fvContractInfo.Row.FindControl("hfSMUser"), HiddenField)
        Dim CO As HiddenField = CType(fvContractInfo.Row.FindControl("hfCOUser"), HiddenField)
        If Not AD Is Nothing Then
            myAD = AD.Value.ToString
        End If
        If Not SM Is Nothing Then
            mySM = SM.Value.ToString
        End If
        If Not CO Is Nothing Then
            myCO = CO.Value.ToString
        End If
        If myAD.Contains(currentUser) Or mySM.Contains(currentUser) Or myCO.Contains(currentUser) Then
            myEditable = "Y"
        Else
            myEditable = "N"
        End If
        Return myEditable
    End Function
    Private Sub btnExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExit.Click
        Session("CurrentViewIndex") = 0
        Response.Write("<script>window.close();</script>")
    End Sub

    Private Sub btnMainMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnMainMenu.Click
        Session("CurrentViewIndex") = 0
        Response.Redirect("Old1NCM.aspx")
    End Sub

    Private Sub btnContractSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnContractSearch.Click
        Session("CurrentViewIndex") = 0
        Response.Redirect("contract_search.aspx")
    End Sub
    Protected Sub btnView_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim myButton As Button = CType(sender, Button)
        Dim myView As MultiView
        myView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)
        If myButton.ID.ToString = "btnGeneral" Then
            myView.ActiveViewIndex = 0
            setButtonColors("btnGeneral")
        ElseIf myButton.ID.ToString = "btnVendor" Then
            myView.ActiveViewIndex = 1
            setButtonColors("btnVendor")
        ElseIf myButton.ID.ToString = "btnContract" Then
            myView.ActiveViewIndex = 2
            setButtonColors("btnContract")
        ElseIf myButton.ID.ToString = "btnPOC" Then
            myView.ActiveViewIndex = 3
            setButtonColors("btnPOC")
        ElseIf myButton.ID.ToString = "btnPrice" Then
            myView.ActiveViewIndex = 4
            setButtonColors("btnPrice")
        ElseIf myButton.ID.ToString = "btnSales" Then
            myView.ActiveViewIndex = 5
            setButtonColors("btnSales")
        ElseIf myButton.ID.ToString = "btnChecks" Then
            myView.ActiveViewIndex = 6
            setButtonColors("btnChecks")
        ElseIf myButton.ID.ToString = "btnSBA" Then
            myView.ActiveViewIndex = 7
            setButtonColors("btnSBA")
        ElseIf myButton.ID.ToString = "btnBOC" Then
            myView.ActiveViewIndex = 8
            setButtonColors("btnBOC")
        ElseIf myButton.ID.ToString = "btnBPAInfo" Then
            myView.ActiveViewIndex = 9
            setButtonColors("btnBPAInfo")
        ElseIf myButton.ID.ToString = "btnBPAPrice" Then
            myView.ActiveViewIndex = 10
            setButtonColors("btnBPAPrice")
        ElseIf myButton.ID.ToString = "btnFSSDetails" Then
            myView.ActiveViewIndex = 11
            setButtonColors("btnFSSDetails")
            populateFSSDetail()
            Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
            If Not myFormView Is Nothing Then
                Dim myViewFSS As MultiView
                myViewFSS = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
                myViewFSS.ActiveViewIndex = 0
            End If
        End If
        Session("CurrentViewIndex") = myView.ActiveViewIndex
    End Sub
    Protected Sub setButtonColors(ByVal button As String)
        Dim myButton As Button = CType(fvContractInfo.Row.FindControl(button), Button)
        If button = "btnGeneral" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnVendor" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnContract" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnPOC" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnPrice" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnSales" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnChecks" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnSBA" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBOC" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAInfo" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAPrice" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnFSSDetails" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnSBA"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        End If
    End Sub
    Protected Function FormatContractNumber(ByVal s As String) As String
        Dim myFormatContractNumber As String
        myFormatContractNumber = s
        'If myFormatContractNumber.Substring(0, 1) <> "V" Or myFormatContractNumber.Substring(0, 1) <> "S" Then
        '    myFormatContractNumber = "V797P-" & s
        'End If
        Return myFormatContractNumber
    End Function

    Protected Function ContractStatus(ByVal Expire As Object, ByVal Comp As Object) As String
        Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbDateCompleted"), Label)
        Dim myStatus As String
        Dim myExpiration, myCompletion As Date
        If Expire.Equals(DBNull.Value) Then
            myExpiration = "1/1/1900'"
        Else
            myExpiration = CDate(Expire)
        End If
        If Comp.Equals(DBNull.Value) Then
            myCompletion = "1/1/1900"
        Else
            myCompletion = (CDate(Comp))
        End If
        If myExpiration = "1/1/1900" And myCompletion = "1/1/1900" Then
            myStatus = "Unknown"
        ElseIf myCompletion < Date.Now.Date And myCompletion <> "1/1/1900" Then
            myStatus = "Canceled"
            myLabel.ForeColor = Drawing.Color.Red
        ElseIf myExpiration < Date.Now.Date And myExpiration <> "1/1/1900" Then
            myStatus = "Expired"
            myLabel.ForeColor = Drawing.Color.Red
        Else
            myStatus = "Active"
            myLabel.ForeColor = Drawing.Color.Green
        End If
        Return myStatus
    End Function
    'Protected Sub dataContractOfficers()
    '    Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlContractOfficers"), DropDownList)
    '    If Not myDropDownList Is Nothing Then
    '        Dim strSQL As String = "exec [NACSEC].[dbo].SelectContractingOfficers"
    '        Dim ds As New DataSet
    '        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '        Dim cmd As SqlCommand
    '        Dim rdr As SqlDataReader
    '        Try
    '            conn.Open()
    '            cmd = New SqlCommand(strSQL, conn)
    '            rdr = cmd.ExecuteReader
    '            myDropDownList.DataTextField = "FullName"
    '            myDropDownList.DataValueField = "CO_ID"
    '            myDropDownList.DataSource = rdr
    '            myDropDownList.DataBind()
    '            conn.Close()
    '        Catch ex As Exception
    '        Finally
    '            conn.Close()
    '        End Try
    '        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '        Dim myCOID As String = myData.Item("CO_ID").ToString  ' was 12
    '        myDropDownList.SelectedValue = myCOID
    '    End If
    'End Sub

    Protected Sub CODataSource_OnSelecting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceSelectingEventArgs)
        Dim divisionId As Int32
        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.FSS) Then
            divisionId = 1
        ElseIf (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.National) Then
            divisionId = 2
        ElseIf (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.SAC) Then
            divisionId = 6
        Else
            divisionId = -1
        End If

        ' users don't always update the exp date to match the completion date, so completion should also be looked at
        ' this is a bug fix which must be promoted as of 12/16/2014 $$$
        Dim bIsExpired As Boolean = False
        If (currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Expired) Then
            bIsExpired = True
        End If

        'If (currentDocument.ExpirationDate.CompareTo(DateTime.Today) < 0) Then
        ' bIsExpired = True
        'End If

        e.Command.Parameters("@DivisionId").Value = divisionId
        e.Command.Parameters("@SelectFlag").Value = 0
        e.Command.Parameters("@OrderByLastNameFullName").Value = "F"
        e.Command.Parameters("@IsExpired").Value = bIsExpired

    End Sub

    Protected Sub checkContractAssignment()

        Dim dlContractOfficers As Global.System.Web.UI.WebControls.DropDownList = CType(fvContractInfo.Row.FindControl("dlContractOfficers"), Global.System.Web.UI.WebControls.DropDownList)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractAssignment) = True Then
            dlContractOfficers.Enabled = True
        Else
            dlContractOfficers.Enabled = False
        End If
    End Sub

    'each contract date is authorized separately
    Protected Sub EnableContractDateEditing(ByVal sender As FormView)

        Dim currentPage As Page
        Dim fvContractInfo As FormView = CType(sender, FormView)

        currentPage = fvContractInfo.Page

        'create image button scripts
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "CAwardDateButtonOnClickScript", GetDateButtonScript("CAward"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "CEffectiveDateButtonOnClickScript", GetDateButtonScript("CEffective"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "CExpirationDateButtonOnClickScript", GetDateButtonScript("CExpiration"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "CCompletionDateButtonOnClickScript", GetDateButtonScript("CCompletion"), True)

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim AwardDateImageButton As ImageButton = CType(fvContractInfo.FindControl("AwardDateImageButton"), ImageButton)
        If Not AwardDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractAwardDate) = True Then
                AwardDateImageButton.Visible = True
            Else
                AwardDateImageButton.Visible = False
            End If
        End If

        Dim EffectiveDateImageButton As ImageButton = CType(fvContractInfo.FindControl("EffectiveDateImageButton"), ImageButton)
        If Not EffectiveDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractEffectiveDate) = True Then
                EffectiveDateImageButton.Visible = True
            Else
                EffectiveDateImageButton.Visible = False
            End If
        End If

        Dim ExpirationDateTextBox As TextBox = CType(fvContractInfo.FindControl("ExpirationDateTextBox"), TextBox)
        Dim ExpirationDateImageButton As Image = CType(fvContractInfo.FindControl("ExpirationDateImageButton"), ImageButton)

        If Not ExpirationDateTextBox Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractExpirationDate) = True Or _
                currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
                ExpirationDateTextBox.Enabled = True
                ExpirationDateImageButton.Visible = True
            Else
                ExpirationDateTextBox.Enabled = False
                ExpirationDateImageButton.Visible = False
            End If
        End If

        Dim CompletionDateTextBox As TextBox = CType(fvContractInfo.FindControl("CompletionDateTextBox"), TextBox)
        Dim CompletionDateImageButton As Image = CType(fvContractInfo.FindControl("CompletionDateImageButton"), ImageButton)

        If Not CompletionDateTextBox Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractCompletionDate) = True Or _
                currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
                CompletionDateTextBox.Enabled = True
                CompletionDateImageButton.Visible = True
            Else
                CompletionDateTextBox.Enabled = False
                CompletionDateImageButton.Visible = False
            End If
        End If

    End Sub
    'Protected Sub dataStateGeneral(ByVal stateBtn As String)
    '    Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl(stateBtn), DropDownList)
    '    '       Dim strSQL As String = "SELECT [Abbr], [State/Province], [Country] FROM tlkup_StateAbbr GROUP BY [Abbr], [State/Province], [Country] ORDER BY [State/Province]"
    '    Dim ds As New DataSet
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim cmd As SqlCommand
    '    Dim rdr As SqlDataReader
    '    Try
    '        conn.Open()
    '        cmd = New SqlCommand(strSQL, conn)
    '        rdr = cmd.ExecuteReader
    '        myDropDownList.DataTextField = "Abbr"
    '        myDropDownList.DataValueField = "Abbr"
    '        myDropDownList.DataSource = rdr
    '        myDropDownList.DataBind()
    '        conn.Close()
    '    Catch ex As Exception
    '    Finally
    '        conn.Close()
    '    End Try
    '    If myDropDownList.ID = "dlState" Then
    '        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '        Dim myCOID As String = myData.Item("Primary_State").ToString
    '        myDropDownList.SelectedValue = myCOID
    '    ElseIf myDropDownList.ID = "dlOrderState" Then
    '        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '        Dim myCOID As String = myData.Item("Ord_State").ToString
    '        myDropDownList.SelectedValue = myCOID
    '    End If
    'End Sub

    'Protected Sub LoadAndSelectState(ByVal stateDropDownListName As String)
    '    Dim stateDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl(stateDropDownListName), DropDownList)

    '    Dim currentDataRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '    Dim selectedAbbreviation As String = "--" 'default to none selected

    '    If stateDropDownList.ID = "dlState" Then
    '        selectedAbbreviation = currentDataRow.Item("Primary_State").ToString
    '    ElseIf stateDropDownList.ID = "dlOrderState" Then
    '        selectedAbbreviation = currentDataRow.Item("Ord_State").ToString
    '    End If

    '    LoadStates(selectedAbbreviation, stateDropDownList)
    'End Sub
    'Protected Sub LoadStates(ByVal selectedAbbreviation As String, ByRef stateDropDownList As DropDownList)

    '    Dim bSuccess As Boolean = True
    '    Dim dsStateCodes As DataSet = Nothing
    '    Dim contractDB As ContractDB

    '    If Not Session("StateCodeDataSet") Is Nothing Then
    '        dsStateCodes = CType(Session("StateCodeDataSet"), DataSet)
    '    Else
    '        contractDB = CType(Session("ContractDB"), ContractDB)

    '        bSuccess = contractDB.SelectStateCodes(dsStateCodes)
    '    End If

    '    If bSuccess = True Then
    '        stateDropDownList.ClearSelection()
    '        stateDropDownList.Items.Clear()
    '        stateDropDownList.DataSource = dsStateCodes
    '        stateDropDownList.DataMember = "StateCodesTable"
    '        stateDropDownList.DataTextField = "StateAbbreviation"
    '        stateDropDownList.DataValueField = "StateName"
    '        stateDropDownList.DataBind()

    '        stateDropDownList.SelectedItem.Text = selectedAbbreviation

    '        Session("StateCodeDataSet") = dsStateCodes

    '    Else
    '        '$$$  MsgBox.Alert(contractDB.ErrorMessage)
    '    End If

    'End Sub

    Protected Sub fvContractInfo_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)

        Dim currentDataRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        Dim selectedOrderingStateAbbreviation As String

        If Not IsPostBack Then

            'save selection to session
            If Not currentDataRow Is Nothing Then
                selectedOrderingStateAbbreviation = currentDataRow.Item("Ord_State").ToString
                Session("CurrentSelectedOrderingState") = selectedOrderingStateAbbreviation
            End If

            LoadAndBindStateDropDownList("dlOrderState")

        Else
            LoadAndBindStateDropDownList("dlOrderState")

        End If

        Dim parentFSSContractNumber As String

        If Not IsPostBack Then
            Session("ActiveFSSContractsForBPAParentDataSet") = Nothing
            Session("CurrentSelectedParentFSSContract") = Nothing

            ' save parent to session
            If Not currentDataRow Is Nothing Then
                parentFSSContractNumber = currentDataRow.Item("BPA_FSS_Counterpart").ToString
                Session("CurrentSelectedParentFSSContract") = parentFSSContractNumber
            End If

            LoadAndBindParentFSSContractDownList()

        Else
            LoadAndBindParentFSSContractDownList()
        End If

    End Sub


    Protected Sub dlState_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlState As DropDownList = CType(sender, DropDownList)
        Session("CurrentSelectedState") = CMGlobals.GetSelectedTextFromDropDownList(dlState)
    End Sub
    Protected Sub dlOrderState_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlOrderState As DropDownList = CType(sender, DropDownList)
        Session("CurrentSelectedOrderingState") = CMGlobals.GetSelectedTextFromDropDownList(dlOrderState)
    End Sub

    Protected Sub LoadAndBindStateDropDownList(ByVal stateDropDownListName As String)

        Dim stateDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl(stateDropDownListName), DropDownList)

        Dim bSuccess As Boolean = True
        Dim dsStateCodes As DataSet = Nothing
        Dim contractDB As ContractDB

        If Not Session("StateCodeDataSet") Is Nothing Then
            dsStateCodes = CType(Session("StateCodeDataSet"), DataSet)
        Else
            contractDB = CType(Session("ContractDB"), ContractDB)

            '       bSuccess = contractDB.SelectStateCodes(dsStateCodes)  $$$ took out to allow compilation
        End If

        If bSuccess = True Then
            stateDropDownList.ClearSelection()
            stateDropDownList.Items.Clear()
            stateDropDownList.DataSource = dsStateCodes
            stateDropDownList.DataMember = "StateCodesTable"
            stateDropDownList.DataTextField = "StateAbbreviation"
            stateDropDownList.DataValueField = "StateName"
            stateDropDownList.DataBind()

            Session("StateCodeDataSet") = dsStateCodes

        Else
            '$$$  MsgBox.Alert(contractDB.ErrorMessage)
        End If

    End Sub

    Protected Sub dlState_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlState As DropDownList = CType(sender, DropDownList)
        Dim selectedState As String = ""

        Dim selectedItem As ListItem

        'set selection into control
        If Not Session("CurrentSelectedState") Is Nothing Then
            selectedState = CType(Session("CurrentSelectedState"), String)
            If selectedState.Length > 0 Then
                selectedItem = dlState.SelectedItem
                If Not selectedItem Is Nothing Then
                    CMGlobals.SelectTextInDropDownList(dlState, selectedState)
                End If
            End If
        End If

    End Sub

    Protected Sub dlOrderState_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlOrderState As DropDownList = CType(sender, DropDownList)
        Dim selectedState As String = ""

        Dim selectedItem As ListItem

        'set selection into control
        If Not Session("CurrentSelectedOrderingState") Is Nothing Then
            selectedState = CType(Session("CurrentSelectedOrderingState"), String)
            If selectedState.Length > 0 Then
                selectedItem = dlOrderState.SelectedItem
                If Not selectedItem Is Nothing Then
                    CMGlobals.SelectTextInDropDownList(dlOrderState, selectedState)
                End If
            End If
        End If
    End Sub

    Protected Sub dataSIN(ByVal cntrclNum As String)
        Dim myDropDown As DropDownList = CType(fvContractInfo.Row.FindControl("dlSIN"), DropDownList)
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim adpt As New SqlDataAdapter
        Try
            adpt.SelectCommand = New SqlCommand("sp_Contract_SIN_Selection", conn)
            adpt.SelectCommand.CommandType = CommandType.StoredProcedure
            adpt.SelectCommand.Parameters.AddWithValue("@CntrctNum", cntrclNum)
            adpt.Fill(ds, "Active")
            myDropDown.DataSource = ds
            myDropDown.DataBind()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the SIN into the dropdownlist.")
        End Try
    End Sub
    Protected Sub dataexpriationDate()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlExpirationDate"), DropDownList)
        Dim strSQL As String = "SELECT DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() }) AND (Date <= { fn NOW() } + 4000) ORDER BY Date"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Date"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myExpiration As String = ""
            If Not myData.Item("Dates_CntrctExp").Equals(DBNull.Value) Then
                myExpiration = FormatDateTime(myData.Item("Dates_CntrctExp"), DateFormat.ShortDate).ToString
            End If
            myDropDownList.SelectedValue = myExpiration
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Expiration Dates into the dropdownlist.")
        Finally
            conn.Close()
        End Try

    End Sub
    Protected Sub dataEndDate()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlEndDate"), DropDownList)
        Dim strSQL As String = "SELECT  DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 7) AND (Date <= { fn NOW() } + 120) ORDER BY Date"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Date"
            myDropDownList.DataValueField = "Date"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myEnd As String = ""
            If Not myData.Item("Dates_Completion").Equals(DBNull.Value) Then
                myEnd = FormatDateTime(myData.Item("Dates_Completion"), DateFormat.ShortDate).ToString
            End If
            myDropDownList.SelectedValue = myEnd
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the End Dates into the dropdownlist.")
        Finally
            conn.Close()
        End Try


    End Sub
    'Protected Function CurrentOptionYear(ByVal total As Object, ByVal Award As Object) As String
    '    Dim myCurrentOptionYear As String = ""
    '    Dim myAwardDate As Date
    '    Dim options As Integer
    '    If total.Equals(DBNull.Value) Then
    '        myCurrentOptionYear = "N/A"
    '    End If
    '    If Award.Equals(DBNull.Value) Then
    '        myCurrentOptionYear = "N/A"
    '    Else
    '        myAwardDate = CType(Award, Date)
    '    End If
    '    If Not myCurrentOptionYear = "N/A" Then
    '        options = DatePart("yyyy", Now) - DatePart("yyyy", myAwardDate)
    '        myCurrentOptionYear = options.ToString
    '    End If
    '    Return myCurrentOptionYear
    'End Function

    Protected Function GetCurrentOptionYear(ByVal totalOptionYears As Object, ByVal effectiveDate As Object, ByVal expirationDate As Object, ByVal endDate As Object) As String
        Dim currentOptionYearString As String = ""
        Dim effectiveDateAsDate As Date
        Dim currentOptionYear As Integer
        Dim currentDateTime As Date = Date.Now
        Dim bIsExpired As Boolean = False
        Dim testDate As Date

        If Not totalOptionYears.Equals(DBNull.Value) Then

            If Not effectiveDate.Equals(DBNull.Value) Then

                If Not expirationDate.Equals(DBNull.Value) Then
                    testDate = CType(expirationDate, Date)
                    If testDate.Date.CompareTo(currentDateTime.Date) < 0 Then
                        bIsExpired = True
                    End If
                End If

                If Not endDate.Equals(DBNull.Value) Then
                    testDate = CType(endDate, Date)
                    If testDate.Date.CompareTo(currentDateTime.Date) < 0 Then
                        bIsExpired = True
                    End If
                End If

                If Not bIsExpired Then
                    effectiveDateAsDate = CType(effectiveDate, Date)

                    Dim effectiveDateYear As Integer = effectiveDateAsDate.Year
                    Dim currentDateYear As Integer = currentDateTime.Year

                    currentOptionYear = currentDateYear - effectiveDateYear

                    'adjust option year for current month/day
                    'handle leap day
                    If (currentDateTime.Day = 29 And currentDateTime.Month = 2) Then
                        testDate = New Date(effectiveDateYear, 2, 28)
                    Else
                        testDate = New Date(effectiveDateYear, currentDateTime.Month, currentDateTime.Day)
                    End If

                    If (testDate.CompareTo(effectiveDateAsDate) < 0) Then
                        currentOptionYear = currentOptionYear - 1
                    End If

                    'currentOptionYear = DatePart("yyyy", Now) - DatePart("yyyy", awardDateAsDate)
                    currentOptionYearString = currentOptionYear.ToString
                Else
                    currentOptionYearString = "N/A"
                End If
            Else
                currentOptionYearString = "N/A"
            End If
        Else
            currentOptionYearString = "N/A"
        End If

        Return currentOptionYearString

    End Function


    Protected Sub dataBusinessSize()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlBusinessSize"), DropDownList)
        Dim strSQL As String = "SELECT [Business_Size_ID], [Business_Size] FROM tlkup_Business_Size GROUP BY [Business_Size_ID], [Business_Size]"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Business_Size"
            myDropDownList.DataValueField = "Business_Size_ID"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myBusinessSize As String = myData.Item("Socio_Business_Size_ID").ToString
            myDropDownList.SelectedValue = myBusinessSize
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Business Size into the dropdownlist.")
        Finally
            conn.Close()
        End Try

    End Sub
    Protected Sub dataVendorStatus()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlVeteranStatus"), DropDownList)
        Dim strSQL As String = "SELECT [VetStatus_ID], [VetStatus_Description] FROM tlkup_VetStatus GROUP BY [VetStatus_ID], [VetStatus_Description]"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "VetStatus_Description"
            myDropDownList.DataValueField = "VetStatus_ID"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myVetStatus As String = myData.Item("Socio_VetStatus_ID").ToString
            myDropDownList.SelectedValue = myVetStatus
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Vet Status into the dropdownlist.")
        Finally
            conn.Close()
        End Try
    End Sub
    Protected Sub dataVendorType()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlVendorType"), DropDownList)
        Dim strSQL As String = "SELECT [Dist_Manf_ID], [Dist_Manf_Description] FROM tlkup_Dist_Manf"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Dist_Manf_Description"
            myDropDownList.DataValueField = "Dist_Manf_ID"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myVendorType As String = myData.Item("Dist_Manf_ID").ToString
            myDropDownList.SelectedValue = myVendorType
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Vendor Type into the dropdownlist.")
        Finally
            conn.Close()
        End Try
    End Sub
    Protected Sub dataGeoCoverage()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlGeographicCoverage"), DropDownList)
        Dim strSQL As String = "SELECT [Geographic_ID], [Geographic_Description] FROM tlkup_Geographic GROUP BY [Geographic_ID], [Geographic_Description]"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Geographic_Description"
            myDropDownList.DataValueField = "Geographic_ID"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myGeo As String = myData.Item("Geographic_Coverage_ID").ToString
            myDropDownList.SelectedValue = myGeo
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Geographic Coverage into the dropdownlist.")
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub dataReturnGoods()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlReturnGoodsPolicyType"), DropDownList)
        Dim strSQL As String = "SELECT [Returned_Goods_Policy_Type_ID], [Returned_Goods_Policy_Description] FROM tlkup_Ret_Goods_Policy"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myDropDownList.DataTextField = "Returned_Goods_Policy_Description"
            myDropDownList.DataValueField = "Returned_Goods_Policy_Type_ID"
            myDropDownList.DataSource = rdr
            myDropDownList.DataBind()
            conn.Close()
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myReturn As String = myData.Item("Returned_Goods_Policy_Type").ToString
            myDropDownList.SelectedValue = myReturn
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the returns good policy into the dropdownlist.")
        Finally
            conn.Close()
        End Try
    End Sub
    Protected Sub dataSales()
        Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        Dim strSQL As String = "SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum," _
& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
& "dbo.View_Contract_Preview.Contractor_Name, " _
& "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
& "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
& "a.VA_IFF, a.OGA_IFF, a.SLG_IFF," _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF AS SLG_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF" _
& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum," _
& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales" _
& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
& "join dbo.tbl_IFF a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
& " LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
& "WHERE View_Contract_Preview.CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "' " _
& " AND View_Sales_Variance_by_Year_A.quarter_id is not null " _
& " AND View_Sales_Variance_by_Year_A.quarter_id between a.start_quarter_id and a.End_quarter_id " _
& " union " _
& " SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum," _
& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
& "dbo.View_Contract_Preview.Contractor_Name, " _
& "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
& "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
& "a.VA_IFF, a.OGA_IFF, a.SLG_IFF," _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF AS SLG_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF" _
& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum," _
& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales" _
& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
& "join dbo.tbl_IFF a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
& " LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
& "WHERE View_Contract_Preview.CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "' " _
& " AND View_Sales_Variance_by_Year_A.quarter_id is null " _
& " AND a.start_quarter_id = dbo.getMaxStartQuarterIdFromTblIff( dbo.[tlkup_Sched/Cat].schedule_number ) " _
& " ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            myGridView.DataSource = rdr
            myGridView.DataBind()
            conn.Close()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the sales into a grid view.")
        Finally
            conn.Close()
        End Try

    End Sub

    'Protected Sub OpenReport(ByVal s As Object, ByVal e As EventArgs)
    '    Dim myContract As String = Request.QueryString("CntrctNum").ToString
    '    Dim strWindow As String = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Qtr','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
    '    Dim myButton As Button = CType(fvContractInfo.Row.FindControl("QuarterlySalesHistoryButton"), Button)
    '    myButton.Attributes.Add("onclick", strWindow)

    'End Sub
    Protected Sub setBtnAttributes()
        myEditable = "Y"
        Dim strWindowFeatures As String = "window.open('pending_price_change.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "','Pending_Change','resizable=0,scrollbars=1,width=700,height=500,left=100,top=300')"
        Dim myContract As String = Request.QueryString("CntrctNum").ToString
        Dim myButton As Button
        Dim strWindow As String
        '  Dim myButton As Button = CType(fvContractInfo.Row.FindControl("SalesIFFCheckCompareButton"), Button)
        '  Dim strWindow As String = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=100,top=300')"
        '  myButton.Attributes.Add("onclick", strWindow)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Full','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        '  myButton = CType(fvContractInfo.Row.FindControl("DetailedSalesHistoryButton"), Button)
        '  myButton.Attributes.Add("onclick", strWindow)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Qtr','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        '  myButton = CType(fvContractInfo.Row.FindControl("QuarterlySalesHistoryButton"), Button)
        ' myButton.Attributes.Add("onclick", strWindow)
        ' strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Year','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        ' myButton = CType(fvContractInfo.Row.FindControl("AnnualSalesHistoryButton"), Button)
        ' myButton.Attributes.Add("onclick", strWindow)
        myButton = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)
        myButton.Text = "ADD SALES" & vbNewLine & "FIGURES"
        myButton.ForeColor = Drawing.Color.Green
        '  myButton = CType(fvContractInfo.Row.FindControl("btnViewIFFCheck"), Button)
        ' strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=100,top=300')"
        '  myButton.Attributes.Add("onclick", strWindow)
        Dim myBtnViewPending As Button = CType(fvContractInfo.Row.FindControl("btnViewPending"), Button)
        ' Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl(""), FormView)
        Dim myBtnBPAViewPending As Button = CType(fvContractInfo.Row.FindControl("btnBPAViewPending"), Button)
        If Not myBtnViewPending Is Nothing Then
            myBtnViewPending.Text = "Pending" & vbNewLine & "Changes"
            myBtnViewPending.Attributes.Add("onclick", strWindowFeatures)
        End If
        If Not myBtnBPAViewPending Is Nothing Then
            myBtnBPAViewPending.Attributes.Add("onclick", strWindowFeatures)
        End If
        If Not Request.QueryString("CntrctNum") Is Nothing Then
            myContractNumber = Request.QueryString("CntrctNum")
        End If

        ' set up correct pricelist for the contract type
        Dim pricelistType As String

        Dim mybtnViewPricelist As Button = CType(fvContractInfo.Row.FindControl("btnViewPricelist"), Button)
        Dim mybtnBPAViewPricelist As Button = CType(fvContractInfo.Row.FindControl("btnBPAViewPricelist"), Button)

        Dim currentDocument As CurrentDocument
        currentDocument = Session("CurrentDocument")
        Dim priceListWindowWidth As Integer

        Dim isContractActive As String
        If currentDocument.ActiveStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.ActiveStatuses.Expired Then
            isContractActive = "F"
        Else
            isContractActive = "T"
        End If

        If currentDocument.DocumentType = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.DocumentTypes.BPA Then
            pricelistType = "B"
            strWindowFeatures = "window.open('MedSurg_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&IsContractActive=" & isContractActive & "&PricelistType=" & pricelistType & "','Pricelist','resizable=0,scrollbars=1,width=800,height=500,left=280,top=140')"
            If Not mybtnBPAViewPricelist Is Nothing Then
                mybtnBPAViewPricelist.Attributes.Add("onclick", strWindowFeatures)
            End If

        Else
            If currentDocument.IsService(currentDocument.ScheduleNumber) = True Then
                pricelistType = "6"
                priceListWindowWidth = 680
            Else
                pricelistType = "F"
                priceListWindowWidth = 770
            End If
            strWindowFeatures = "window.open('MedSurg_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&IsContractActive=" & isContractActive & "&PricelistType=" & pricelistType & "','Pricelist','resizable=0,scrollbars=1,width=" & priceListWindowWidth.ToString() & ",height=500,left=280,top=140')"
            If Not mybtnViewPricelist Is Nothing Then
                mybtnViewPricelist.Attributes.Add("onclick", strWindowFeatures)
            End If

        End If


        'If myScheduleNumber = 36 Then
        '    strWindow = "window.open('MedSurg_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&BPA=N','Pricelist','resizable=0,scrollbars=1,width=730,height=500,left=280,top=140')"
        '    mybtnViewPricelist.Attributes.Add("onclick", strWindow)
        'Else
        '    strWindow = "window.open('MedSurg_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&BPA=N','Pricelist','resizable=0,scrollbars=1,width=730,height=500,left=280,top=140')"
        '    mybtnViewPricelist.Attributes.Add("onclick", strWindow)
        'End If  'this line had a tag of 5:

        ' Dim myExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnViewExcel"), ImageButton)
        'If Not myExcel Is Nothing Then
        'Dim strExcelWindow As String = "window.open('pricelist_excel.aspx?CntrctNum=" & myContractNumber & "','Pricelist','')"
        'myExcel.Attributes.Add("onclick", strExcelWindow)
        'End If
        'Dim myExcel1 As ImageButton = CType(fvContractInfo.Row.FindControl("ibtnBPAPriceExcel"), ImageButton)
        'If Not myExcel1 Is Nothing Then
        'Dim strExcelWindow As String = "window.open('BPApricelist_excel.aspx?CntrctNum=" & myContractNumber & "','Pricelist','')"
        ' myExcel1.Attributes.Add("onclick", strExcelWindow)
        'End If

        'Dim myExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnViewExcel"), ImageButton)
        'If Not myExcel Is Nothing Then
        '    Dim strExcelWindow As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & myContractNumber & "','Pricelist','')"
        '    myExcel.Attributes.Add("onclick", strExcelWindow)
        'End If

    End Sub
    'Protected Sub SetDrugItemExportParms(ByVal ContractNumber As String, ByVal ScheduleNumber As Integer, ByVal ExportType As String)
    '    Dim btnExportDrugItemPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)
    '    If Not btnExportDrugItemPricelistToExcel Is Nothing Then
    '        '   Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','')"
    '        'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,width=300,height=200,left=170,top=110,status=yes');"

    '        'old way $$$
    '        'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=280,left=320,width=340,height=160, resizable=0');"

    '        Dim exportDrugItemsToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemUpload.aspx?ContractNumber=" & ContractNumber & "&ScheduleNumber=" & ScheduleNumber.ToString() & "&ExportUploadType=P" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"

    '        btnExportDrugItemPricelistToExcel.Attributes.Add("onclick", exportDrugItemsToExcelWindowScript)
    '    End If
    'End Sub

    Protected Sub SetDrugItemExportParms(ByVal ContractNumber As String, ByVal ScheduleNumber As Integer)
        Dim btnExportDrugItemPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)
        If Not btnExportDrugItemPricelistToExcel Is Nothing Then
            '   Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','')"
            'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,width=300,height=200,left=170,top=110,status=yes');"

            'old way $$$
            'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=280,left=320,width=340,height=160, resizable=0');"

            Dim exportDrugItemsToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemUpload.aspx?ContractNumber=" & ContractNumber & "&ScheduleNumber=" & ScheduleNumber.ToString() & "&ExportUploadType=P" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"

            btnExportDrugItemPricelistToExcel.Attributes.Add("onclick", exportDrugItemsToExcelWindowScript)
        End If
    End Sub
    'Protected Sub DrugItemExportTypeRadioButtonList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    '    Dim DrugItemExportTypeRadioButtonList As RadioButtonList
    '    Dim selectedExportTypeValue As String
    '    Dim ExportType As String = "U"

    '    DrugItemExportTypeRadioButtonList = CType(sender, RadioButtonList)
    '    If Not DrugItemExportTypeRadioButtonList Is Nothing Then
    '        Dim selectedItem As ListItem
    '        selectedItem = DrugItemExportTypeRadioButtonList.SelectedItem
    '        If Not selectedItem Is Nothing Then

    '            selectedExportTypeValue = DrugItemExportTypeRadioButtonList.SelectedItem.Value
    '            If (selectedExportTypeValue.CompareTo("C") = 0) Then
    '                ExportType = "C"
    '            Else
    '                ExportType = "B"
    '            End If
    '        End If
    '    End If

    '    Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
    '    currentDocument = Session("CurrentDocument")

    '    If Not currentDocument Is Nothing Then
    '        SetDrugItemExportParms(currentDocument.ContractNumber, currentDocument.ScheduleNumber, ExportType)
    '    End If

    'End Sub

    Protected Function getVAVariance(ByVal PVAQtrSales As Object, ByVal VASales As Object, ByVal passLabel As String) As String
        Dim myVariance As String = ""
        Dim myVAQtr As Double = 0
        Dim myVASales As Double = 0
        Dim myLabelName As String = passLabel
        If Not PVAQtrSales.Equals(DBNull.Value) Then
            myVAQtr = CType(PVAQtrSales, Double)
        End If
        If Not VASales.Equals(DBNull.Value) Then
            myVASales = CType(VASales, Double)
        End If
        If myVAQtr >= 1 Then
            Dim mytemp As Double = (myVASales - myVAQtr) / (myVAQtr)
            If mytemp > 9.99 Then
                myVariance = FormatPercent(9.99, 0)
            Else
                myVariance = FormatPercent((myVASales - myVAQtr) / (myVAQtr), 0).ToString
            End If
            If mytemp < -9.99 Then
                myVariance = FormatPercent(-9.99, 0)
            Else
                myVariance = FormatPercent((myVASales - myVAQtr) / (myVAQtr), 0).ToString
            End If
            ' myVariance = FormatPercent((myVASales - myVAQtr) / (myVASales), 0).ToString
        Else
            myVariance = "0%"
        End If

        Dim myLabel As Label = CType(fvContractInfo.Row.FindControl(myLabelName), Label)
        Return myVariance
    End Function

    Protected Sub setRowFormat(ByVal s As Object, ByVal e As EventArgs)
        Dim myLabel As Label = Nothing
        Dim myPercent As Double = 0
        myLabel = CType(s, Label)
        If Not myLabel.Text = "" Then
            If Not myLabel.Text = "N/A" Then
                Dim mynumber As Long = myLabel.Text.Substring(0, myLabel.Text.Length - 1)
                myPercent = CType(mynumber, Double)
                If myPercent < 0 Then
                    myLabel.ForeColor = Drawing.Color.Red
                Else
                    myLabel.ForeColor = Drawing.Color.Green
                End If
            End If
        End If
    End Sub
    Protected Sub setOption(ByVal s As Object, ByVal e As EventArgs)
        Dim mydata As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
        Dim myOptin As String = mydata.Item("Dates_TotOptYrs").ToString
    End Sub
    Protected Sub btnUpdate_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        'validate the dates
        Dim lbAwardDate As Label = CType(fvContractInfo.FindControl("lbAwardDate"), Label)
        Dim awardDate As DateTime

        If (DateTime.TryParse(lbAwardDate.Text, awardDate) = False) Then
            MsgBox.Alert("Award date is not a valid date.")
            Return
        End If

        Dim lbEffectiveDate As Label = CType(fvContractInfo.FindControl("lbEffectiveDate"), Label)
        Dim effectiveDate As DateTime

        If (DateTime.TryParse(lbEffectiveDate.Text, effectiveDate) = False) Then
            MsgBox.Alert("Effective date is not a valid date.")
            Return
        End If

        Dim ExpirationDateTextBox As TextBox = CType(fvContractInfo.FindControl("ExpirationDateTextBox"), TextBox)
        Dim expirationDate As DateTime

        If ExpirationDateTextBox.Text.Length > 0 Then
            If (DateTime.TryParse(ExpirationDateTextBox.Text, expirationDate) = False) Then
                MsgBox.Alert("Expiration date is not a valid date.")
                Return
            End If
        Else
            MsgBox.Alert("Expiration date is required.")
            Return
        End If

        Dim CompletionDateTextBox As TextBox = CType(fvContractInfo.FindControl("CompletionDateTextBox"), TextBox)
        Dim completionDate As DateTime

        If CompletionDateTextBox.Text.Length > 0 Then
            If (DateTime.TryParse(CompletionDateTextBox.Text, completionDate) = False) Then
                MsgBox.Alert("Completion date is not a valid date.")
                Return
            End If
        End If

        'compare award, effective, expiration and completion dates
        If DateTime.Compare(awardDate, effectiveDate) > 0 Then
            MsgBox.Alert("Award date must be before effective date.")
            Return
        End If

        If DateTime.Compare(effectiveDate, expirationDate) > 0 Then
            MsgBox.Alert("Expiration date must be after effective date.")
            Return
        End If

        If CompletionDateTextBox.Text.Length > 0 Then
            If DateTime.Compare(awardDate, completionDate) > 0 Then
                MsgBox.Alert("Completion date must be after award date.")
                Return
            End If
        End If

        fvContractInfo.UpdateItem(True)
    End Sub
    Protected Sub setSBAbtnName(ByVal s As Object, ByVal e As EventArgs)
        Dim myButton As Button = CType(s, Button)
        myButton.Text = "Add Vendor" & vbCrLf & "Record"
    End Sub
    'Protected Sub setFormatSBAcom(ByVal s As Object, ByVal e As EventArgs)
    '    Dim myButton As Button = CType(s, Button)
    '    myButton.Text = "Add" & vbCrLf & "294/295"
    'End Sub
    Protected Sub setFormatSBAProj(ByVal s As Object, ByVal e As EventArgs)
        Dim myButton As Button = CType(s, Button)
        myButton.Text = "Add" & vbCrLf & "Projection"
    End Sub


    Protected Sub dataSBAPlanTypes()
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlSBAPlanName"), DropDownList)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Dim da As SqlDataAdapter
        Dim ds As DataSet
        Dim testRow As System.Data.DataRow

        Dim mySBAID As String = ""
        Dim planTypesFormView As FormView

        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
        If Not myData Is Nothing Then
            mySBAID = myData.Item("SBAPlanID").ToString
            'no id, so nothing will bind anyway
            If mySBAID.Length = 0 Then
                Return
            End If
        End If
        Dim projectionGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSBAProjections"), Global.System.Web.UI.WebControls.GridView)

        '        Dim strSQL As String = "SELECT TOP 100 PERCENT COALESCE (dbo.tbl_sba_Projection.SBAPlanID, dbo.view_SBA_contract_master.SBAPlanID) AS SBAPlanID, " _
        '& "dbo.tbl_sba_Projection.StartDate, dbo.tbl_sba_Projection.EndDate, dbo.view_SBA_contract_master.CntrctNum, " _
        '& "dbo.view_SBA_contract_master.CO_User, dbo.view_SBA_contract_master.AD_User, dbo.view_SBA_contract_master.SM_User, " _
        '& "dbo.view_SBA_contract_master.CO_Name, dbo.view_SBA_contract_master.Logon_Name, dbo.tbl_sba_Projection.ProjectionID, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_Full_1_UserName, dbo.view_SBA_contract_master.Data_Entry_Full_2, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_Full_2_FullName, dbo.view_SBA_contract_master.Data_Entry_Full_2_UserName, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_1, dbo.view_SBA_contract_master.Data_Entry_SBA_1_FullName, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_1_UserName, dbo.view_SBA_contract_master.Data_Entry_SBA_2, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_2_FullName, dbo.view_SBA_contract_master.Data_Entry_SBA_2_UserName " _
        '& "FROM dbo.tbl_sba_Projection JOIN " _
        '& "dbo.view_SBA_contract_master ON dbo.tbl_sba_Projection.SBAPlanID = dbo.view_SBA_contract_master.SBAPlanID " _
        '& "WHERE dbo.view_SBA_contract_master.SBAPlanID = " & mySBAID & " ORDER BY dbo.tbl_sba_Projection.StartDate DESC"

        Dim strSQL As String = "select SBAPlanID, StartDate, EndDate, ProjectionID from View_SBA_Projection_Sorted where SBAPlanID = " & mySBAID & " "

        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            da = New SqlDataAdapter(cmd)
            ds = New DataSet()

            da.Fill(ds)

            If Not ds.Tables Is Nothing Then
                If Not ds.Tables(0) Is Nothing Then
                    If Not ds.Tables(0).Rows Is Nothing Then
                        If ds.Tables(0).Rows.Count > 0 Then
                            testRow = ds.Tables(0).Rows(0)
                            If Not testRow Is Nothing Then
                                If Not testRow("ProjectionID") Is DBNull.Value Then
                                    If Not projectionGridView Is Nothing Then
                                        projectionGridView.DataSource = ds
                                        projectionGridView.DataBind()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            conn.Close()

            'conn.Open()
            'cmd = New SqlCommand(strSQL, conn)
            'rdr = cmd.ExecuteReader
            'If rdr.HasRows Then
            '    If Not projectionGridView Is Nothing Then
            '        'adopt to poorly formed view
            '        If rdr.Read() Then
            '            If rdr.IsDBNull(1) = False Then
            '                projectionGridView.DataSource = rdr
            '                projectionGridView.DataBind()
            '            End If
            '        End If
            '    End If
            'Else
            '    If Not projectionGridView Is Nothing Then
            '        If projectionGridView.Rows.Count > 0 Then
            '            projectionGridView.DeleteRow(0)
            '        End If
            '    End If
            'End If
        Catch ex As Exception
        Finally
            conn.Close()
        End Try
        Dim accomplishmentsGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gv294Reports"), Global.System.Web.UI.WebControls.GridView)
        '        strSQL = "SELECT     TOP 100 PERCENT COALESCE (dbo.tbl_sba_Accomplishments.SBAPlanID, dbo.view_SBA_contract_master.SBAPlanID) AS SBAPlanID, " _
        '& "dbo.view_SBA_contract_master.CntrctNum, dbo.view_SBA_contract_master.CO_User, dbo.view_SBA_contract_master.AD_User, " _
        '& "dbo.view_SBA_contract_master.SM_User, dbo.view_SBA_contract_master.CO_Name, dbo.view_SBA_contract_master.Logon_Name, " _
        '& "dbo.tbl_sba_Accomplishments.Acc_Record_ID, dbo.tbl_sba_Accomplishments.Fiscal_Year, dbo.tbl_sba_Accomplishments.Accomplishment_Period, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_Full_1_UserName, dbo.view_SBA_contract_master.Data_Entry_Full_2, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_Full_2_FullName, dbo.view_SBA_contract_master.Data_Entry_Full_2_UserName, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_1, dbo.view_SBA_contract_master.Data_Entry_SBA_1_FullName, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_1_UserName, dbo.view_SBA_contract_master.Data_Entry_SBA_2, " _
        '& "dbo.view_SBA_contract_master.Data_Entry_SBA_2_FullName, dbo.view_SBA_contract_master.Data_Entry_SBA_2_UserName " _
        '& "FROM dbo.tbl_sba_Accomplishments JOIN dbo.view_SBA_contract_master ON dbo.tbl_sba_Accomplishments.SBAPlanID = dbo.view_SBA_contract_master.SBAPlanID" _
        '& " WHERE(dbo.tbl_sba_Accomplishments.SBAPlanID = " & mySBAID & ") ORDER BY dbo.tbl_sba_Accomplishments.Fiscal_Year DESC, dbo.tbl_sba_Accomplishments.Accomplishment_Period"

        strSQL = "select SBAPlanID, Acc_Record_ID, Fiscal_Year, Accomplishment_Period from view_SBA_Accomplishment_Sorted where SBAPlanID = " & mySBAID & " "

        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            da = New SqlDataAdapter(cmd)
            ds = New DataSet()

            da.Fill(ds)

            If Not ds.Tables Is Nothing Then
                If Not ds.Tables(0) Is Nothing Then
                    If Not ds.Tables(0).Rows Is Nothing Then
                        If ds.Tables(0).Rows.Count > 0 Then
                            testRow = ds.Tables(0).Rows(0)
                            If Not testRow Is Nothing Then
                                If Not testRow("Acc_Record_ID") Is DBNull.Value Then
                                    If Not accomplishmentsGridView Is Nothing Then
                                        accomplishmentsGridView.DataSource = ds
                                        accomplishmentsGridView.DataBind()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            'conn.Open()
            'cmd = New SqlCommand(strSQL, conn)
            'rdr = cmd.ExecuteReader
            'If rdr.HasRows Then
            '    If Not accomplishmentsGridView Is Nothing Then
            '        'adopt to poorly formed view
            '        If rdr.Read() Then
            '            If rdr.IsDBNull(1) = False Then
            '                accomplishmentsGridView.DataSource = rdr
            '                accomplishmentsGridView.DataBind()
            '            End If
            '        End If
            '    End If
            '    'Else
            '    '    If Not accomplishmentsGridView Is Nothing Then
            '    '        accomplishmentsGridView.Visible = "False"
            '    '    End If
            'End If
            conn.Close()
        Catch ex As Exception
        Finally
            conn.Close()
        End Try
        '  Dim contractResponsibleFormView As FormView = CType(fvContractInfo.Row.FindControl("fvSBACntrctresp"), FormView)
        '        strSQL = "SELECT     TOP 100 PERCENT dbo.View_Contracts_Full.SBAPlanID, dbo.View_Contracts_Full.CntrctNum, dbo.View_Contracts_Full.CO_User, " _
        '& "dbo.View_Contracts_Full.AD_User, dbo.View_Contracts_Full.SM_User, dbo.View_Contracts_Full.CO_Name, CURRENT_USER AS Logon_Name, " _
        '& "dbo.View_Contracts_Full.Data_Entry_Full_1_UserName, dbo.View_Contracts_Full.Data_Entry_Full_2, " _
        '& "dbo.View_Contracts_Full.Data_Entry_Full_2_FullName, dbo.View_Contracts_Full.Data_Entry_Full_2_UserName, " _
        '& "dbo.View_Contracts_Full.Data_Entry_SBA_1, dbo.View_Contracts_Full.Data_Entry_SBA_1_FullName, " _
        '& "dbo.View_Contracts_Full.Data_Entry_SBA_1_UserName, dbo.View_Contracts_Full.Data_Entry_SBA_2, " _
        '& "dbo.View_Contracts_Full.Data_Entry_SBA_2_FullName, dbo.View_Contracts_Full.Data_Entry_SBA_2_UserName " _
        '& "FROM dbo.View_Contracts_Full INNER JOIN dbo.sys_qry_sba_contract_Value ON dbo.View_Contracts_Full.SBAPlanID = dbo.sys_qry_sba_contract_Value.SBAPlanID AND " _
        '& "dbo.View_Contracts_Full.Estimated_Contract_Value = dbo.sys_qry_sba_contract_Value.Expr1 " _
        '& " WHERE dbo.View_Contracts_Full.SBAPlanID = " & mySBAID & "ORDER BY dbo.View_Contracts_Full.SBAPlanID"

        '     strSQL = "select CntrctNum, CO_Name from view_sba_contract_master where SBAPlanID = " & mySBAID & " "



        'Try
        '    conn.Open()
        '    cmd = New SqlCommand(strSQL, conn)
        '    rdr = cmd.ExecuteReader
        '    If rdr.HasRows Then
        '        contractResponsibleFormView.DataSource = rdr
        '        contractResponsibleFormView.DataBind()
        '    Else
        '        If Not contractResponsibleFormView Is Nothing Then
        '            contractResponsibleFormView.Visible = "False"
        '        End If
        '    End If
        '    conn.Close()
        'Catch ex As Exception
        '    '  Msg_Alert.Client_Alert_OnLoad("SBA Contract Rep info " & ex.ToString)
        'Finally
        '    conn.Close()
        'End Try
        If Not mySBAID = Nothing Then
            If mySBAID.Length > 0 Then
                planTypesFormView = CType(fvContractInfo.Row.FindControl("fvSBAplanType"), FormView)
                strSQL = "SELECT * FROM tbl_sba_SBAPlan A Join tbl_sba_Plantype B on A.PlanTypeID = B.PlanTypeID  WHERE SBAPlanID= " & mySBAID
                Try
                    conn.Open()
                    cmd = New SqlCommand(strSQL, conn)
                    rdr = cmd.ExecuteReader
                    If rdr.HasRows Then
                        planTypesFormView.DataSource = rdr
                        planTypesFormView.DataBind()
                    Else
                        planTypesFormView.Visible = "False"
                    End If
                    conn.Close()
                Catch ex As Exception
                    Msg_Alert.Client_Alert_OnLoad("This is error at SBA. ")
                Finally
                    conn.Close()
                End Try
            End If
        End If
        If Not mySBAID = Nothing Then
            If mySBAID.Length > 0 Then
                Dim sbaCompanyDatalist As DataList = CType(fvContractInfo.Row.FindControl("dlSBACompanyList"), DataList)
                'strSQL = "SELECT * FROM tbl_sba_SBAPlan A Join tbl_Cntrcts B on A.SBAPlanID = B.SBAPlanID Join [tlkup_Sched/Cat] C on B.Schedule_Number = C.Schedule_Number Join [NACSEC].[dbo].SEC_UserProfile D on B.CO_ID = D.CO_ID WHERE A.SBAPlanID= " & mySBAID

                ' new version
                strSQL = "SELECT c.CntrctNum, s.Schedule_Name, c.Dates_CntrctExp, c.Dates_Completion, dbo.IsContractActiveFunction( c.CntrctNum, getdate() ) as IsActive, c.Contractor_Name, c.Estimated_Contract_Value, p.FullName " & _
                "FROM tbl_sba_SBAPlan b join tbl_Cntrcts c on b.SBAPlanID = c.SBAPlanID " & _
                "join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number " & _
                "join  [NACSEC].[dbo].SEC_UserProfile p on c.CO_ID = p.CO_ID " & _
                "WHERE b.SBAPlanID = " & mySBAID & _
                "order by IsActive desc, c.CntrctNum asc "

                Try
                    conn.Open()
                    cmd = New SqlCommand(strSQL, conn)
                    rdr = cmd.ExecuteReader
                    If rdr.HasRows Then
                        sbaCompanyDatalist.DataSource = rdr
                        sbaCompanyDatalist.DataBind()
                    Else
                        Dim myPanel As Panel = CType(fvContractInfo.Row.FindControl("pnSBAComplayList"), Panel)
                        myPanel.Visible = "False"
                    End If
                    conn.Close()
                Catch ex As Exception
                    Msg_Alert.Client_Alert_OnLoad("This is error at SBA. ")
                Finally
                    conn.Close()
                End Try
            Else
                Dim myPanel As Panel = CType(fvContractInfo.Row.FindControl("pnSBAComplayList"), Panel)
                myPanel.Visible = "False"
            End If
        Else
            Dim myPanel As Panel = CType(fvContractInfo.Row.FindControl("pnSBAComplayList"), Panel)
            myPanel.Visible = "False"
        End If
    End Sub

    Protected Sub LoadSBAPlanContractResponsible(ByVal sbaPlanId As Integer)

        Dim bSuccess As Boolean = True
        Dim contractDB As ContractDB
        Dim contractResponsibleForSBAPlan As String = ""
        Dim contractResponsibleForSBAPlanCOName As String = ""
        Dim contractResponsibleForSBAPlanCOID As Integer = -1

        contractDB = CType(Session("ContractDB"), ContractDB)

        bSuccess = contractDB.GetContractResponsibleForSBAPlan(sbaPlanId, contractResponsibleForSBAPlan, contractResponsibleForSBAPlanCOID, contractResponsibleForSBAPlanCOName)

        Dim contractResponsibleLabel As System.Web.UI.WebControls.Label = CType(fvContractInfo.Row.FindControl("lbSBACntrctResp"), System.Web.UI.WebControls.Label)
        Dim contractResponsibleCOLabel As System.Web.UI.WebControls.Label = CType(fvContractInfo.Row.FindControl("lbSBACOResp"), System.Web.UI.WebControls.Label)
        Dim contractResponsiblePanel As Panel = CType(fvContractInfo.Row.FindControl("ContractResponsibleForSBAPanel"), Panel)

        If bSuccess = True Then
            If Not contractResponsibleLabel Is Nothing And Not contractResponsibleCOLabel Is Nothing Then
                contractResponsibleLabel.Text = contractResponsibleForSBAPlan
                contractResponsibleCOLabel.Text = contractResponsibleForSBAPlanCOName
            End If
        Else
            contractResponsiblePanel.Visible = "False"
        End If

    End Sub
    Protected Sub populateFSSDetail()
        Dim myFSSContractNum As String = ""
        Dim myFSSCOntract As HiddenField = CType(fvContractInfo.Row.FindControl("hfBPAFSSContract"), HiddenField)
        If Not myFSSCOntract Is Nothing Then
            myFSSContractNum = myFSSCOntract.Value.ToString
            If Not myFSSContractNum.Contains("GS") Then
                Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
                Dim mySQLStr As String = "SELECT * FROM [View_Contracts_Full] WHERE ([CntrctNum] = '" & myFSSContractNum.ToString & "')"
                Dim ds As New DataSet
                Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
                Dim cmd As SqlCommand
                Dim rdr As SqlDataReader
                Try
                    conn.Open()
                    cmd = New SqlCommand(mySQLStr, conn)
                    rdr = cmd.ExecuteReader
                    If rdr.HasRows Then
                        myFormView.DataSource = rdr
                        myFormView.DataBind()
                    End If

                Catch ex As Exception
                    Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Detail. ")
                Finally
                    conn.Close()
                End Try
                Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(myFormView.Row.FindControl("gvBPAFSSSales"), Global.System.Web.UI.WebControls.GridView)
                If Not myGridView Is Nothing Then
                    '  mySQLStr = "SELECT     dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
                    ' & "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum," _
                    ' & "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                    ' & "dbo.View_Contract_Preview.Contractor_Name, " _
                    ' & "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
                    ' & "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
                    '& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
                    '& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
                    '& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                    '& "a.VA_IFF, a.OGA_IFF, a.SLG_IFF," _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF AS SLG_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF" _
                    '& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                    '& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
                    '    & "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
                    '   & "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum," _
                    '  & " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales" _
                    ' & "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
                    '& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                    '    & "join dbo.tbl_IFF a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
                    '   & " LEFT OUTER JOIN " _
                    '  & "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
                    '  & "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
                    ' & "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
                    '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
                    '& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
                    '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
                    '& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                    '& "WHERE View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "' " _
                    '& " AND View_Sales_Variance_by_Year_A.quarter_id is not null " _
                    '& " AND View_Sales_Variance_by_Year_A.quarter_id between a.start_quarter_id and a.End_quarter_id " _
                    '& " union " _
                    '& " SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum," _
                    '& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                    '& "dbo.View_Contract_Preview.Contractor_Name, " _
                    '& "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
                    '& "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
                    '& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
                    '& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
                    '& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                    '& "a.VA_IFF, a.OGA_IFF, a.SLG_IFF," _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF AS SLG_IFF_Amount, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * a.SLG_IFF" _
                    '& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                    '& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
                    '& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
                    '& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum," _
                    '& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales" _
                    '& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
                    '& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                    '& "join dbo.tbl_IFF a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
                    '& " LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
                    '& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
                    '& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
                    '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
                    '& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
                    '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
                    '& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                    '& "WHERE View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "' " _
                    '& " AND View_Sales_Variance_by_Year_A.quarter_id is null " _
                    '& " AND a.start_quarter_id = dbo.getMaxStartQuarterIdFromTblIff( dbo.[tlkup_Sched/Cat].schedule_number ) " _
                    '& " ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC"

                    'this was live until 4/19/2010
                    'mySQLStr = "(SELECT     dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum,  " _
                    '& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                    '& "(dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum - View_Sales_Variance_by_Year_A_1.VA_Sales_Sum) " _
                    '& "/ (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum + .00001) AS Previous_Qtr_VA_Variance, " _
                    '& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, " _
                    '& "(dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum - View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum) " _
                    '& "/ (dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum + .00001) AS Previous_Qtr_OGA_Variance, " _
                    '& "View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_Total_Sales, " _
                    '& "(dbo.View_Sales_Variance_by_Year_A.Total_Sum - View_Sales_Variance_by_Year_A_1.Total_Sum) " _
                    '& "/ (dbo.View_Sales_Variance_by_Year_A.Total_Sum + .00001) AS Previous_Qtr_Total_Variance, " _
                    '& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, " _
                    '& "(dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum - View_Sales_Variance_by_Year_A_2.VA_Sales_Sum) " _
                    '& "/ (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum + .00001) AS Previous_Year_VA_Variance, " _
                    '& "View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum - View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum + .00001) AS Previous_Year_OGA_Variance, " _
                    '  & "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.Total_Sum - View_Sales_Variance_by_Year_A_2.Total_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.Total_Sum + .00001) AS Previous_Year_Total_Variance, dbo.View_Contract_Preview.Contractor_Name, " _
                    '  & "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
                    '  & "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
                    '  & "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
                    '  & "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
                    '  & "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                    '  & "a.VA_IFF, a.OGA_IFF, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS Total_IFF_Amount, " _
                    '& "dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                    '& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName " _
                    '& "FROM         dbo.[tlkup_Sched/Cat] INNER JOIN " _
                    '& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                    '& "join dbo.tbl_iff a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
                    '& "LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
                    '& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
                    '& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
                    '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
                    '  & "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                    '& "where View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "' AND " _
                    '& "View_Sales_Variance_by_Year_A.quarter_id is not null " _
                    '& "and View_Sales_Variance_by_Year_A.quarter_id between a.start_quarter_id and a.End_quarter_id " _
                    '& "union " _
                    '& "SELECT      dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, " _
                    '& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                    '& "(dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum - View_Sales_Variance_by_Year_A_1.VA_Sales_Sum) " _
                    '& "/ (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum + .00001) AS Previous_Qtr_VA_Variance, " _
                    '& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, " _
                    ' & "(dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum - View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum + .00001) AS Previous_Qtr_OGA_Variance, " _
                    '  & "View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_Total_Sales, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.Total_Sum - View_Sales_Variance_by_Year_A_1.Total_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.Total_Sum + .00001) AS Previous_Qtr_Total_Variance, " _
                    '  & "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum - View_Sales_Variance_by_Year_A_2.VA_Sales_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum + .00001) AS Previous_Year_VA_Variance, " _
                    '  & "View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum - View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum + .00001) AS Previous_Year_OGA_Variance, " _
                    '  & "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_Total_Sales, " _
                    '  & "(dbo.View_Sales_Variance_by_Year_A.Total_Sum - View_Sales_Variance_by_Year_A_2.Total_Sum) " _
                    '  & "/ (dbo.View_Sales_Variance_by_Year_A.Total_Sum + .00001) AS Previous_Year_Total_Variance, dbo.View_Contract_Preview.Contractor_Name, " _
                    '  & "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
                    '  & "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
                    '  & "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
                    '  & "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
                    '  & "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                    '  & "a.VA_IFF, a.OGA_IFF, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF AS VA_IFF_Amount, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF AS OGA_IFF_Amount, " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * a.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * a.OGA_IFF " _
                    '   & "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                    '& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName " _
                    '& "FROM         dbo.[tlkup_Sched/Cat] INNER JOIN " _
                    '& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                    '& "join dbo.tbl_iff a on dbo.View_Contract_Preview.Schedule_Number = a.schedule_number " _
                    '& "LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
                    '& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
                    '& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
                    '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
                    ' & "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
                    '  & "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
                    '  & "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                    '& "where View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "' AND " _
                    '& "View_Sales_Variance_by_Year_A.quarter_id is  null " _
                    '& "and a.start_quarter_id =  dbo.getMaxStartQuarterIdFromTblIFF( dbo.[tlkup_Sched/Cat].schedule_number)" _
                    '& ")"

                    ' this version copied from FSS section
                    mySQLStr = "SELECT dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID," _
                    & " dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.Year, dbo.tlkup_year_qtr.Qtr, " _
                    & " dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, " _
                    & " dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                    & " dbo.View_Contract_Preview.Contractor_Name, dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, " _
                    & " dbo.View_Contract_Preview.CO_Phone, dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, " _
                    & " dbo.View_Contract_Preview.SM_Name, dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, " _
                    & " dbo.View_Contract_Preview.SM_User, dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, " _
                    & " dbo.View_Contract_Preview.Dates_CntrctAward, dbo.View_Contract_Preview.Dates_Effective, " _
                    & " dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                    & " dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF AS VA_IFF_Amount, " _
                    & " dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF AS OGA_IFF_Amount, " _
                    & " (dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.tbl_IFF.OGA_IFF) + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.tbl_IFF.SLG_IFF AS Total_IFF_Amount, " _
                    & " dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                    & " dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
                    & " View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, " _
                    & " View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales, " _
                    & " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, " _
                    & " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales, " _
                    & " dbo.tbl_IFF.VA_IFF, dbo.tbl_IFF.OGA_IFF, dbo.tbl_IFF.SLG_IFF, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum, " _
                    & " View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, " _
                    & " dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.tbl_IFF.VA_IFF AS SLG_IFF_Amount " _
                    & " FROM dbo.[tlkup_Sched/Cat] INNER JOIN dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                    & "  INNER JOIN dbo.tbl_IFF ON dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.Schedule_Number " _
                    & " LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A " _
                    & " INNER JOIN dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID " _
                    & " ON dbo.tbl_IFF.Start_Quarter_Id = dbo.tlkup_year_qtr.start_quarter_id " _
                    & " AND dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum " _
                    & " LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_1 " _
                    & " ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum " _
                    & " AND dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID " _
                    & " LEFT OUTER JOIN dbo.View_Sales_Variance_by_Year_A AS View_Sales_Variance_by_Year_A_2 " _
                    & " ON dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum " _
                    & " AND dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                    & " WHERE (dbo.View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "' ) " _
                    & " AND dbo.View_Sales_Variance_by_Year_A.Quarter_ID is not Null " _
                    & " ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC"


                    Try
                        conn.Open()
                        cmd = New SqlCommand(mySQLStr, conn)
                        rdr = cmd.ExecuteReader
                        If rdr.HasRows Then
                            myGridView.DataSource = rdr
                            myGridView.DataBind()
                        End If

                    Catch ex As Exception
                        Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Sales Detail. ")
                    Finally
                        conn.Close()
                    End Try
                End If
                myGridView = CType(myFormView.Row.FindControl("gvBPAFSSChecks"), Global.System.Web.UI.WebControls.GridView)
                If Not myGridView Is Nothing Then
                    mySQLStr = "SELECT dbo.tbl_Cntrcts_Checks.ID, dbo.tbl_Cntrcts_Checks.CntrctNum, dbo.tbl_Cntrcts_Checks.Quarter_ID, dbo.tbl_Cntrcts_Checks.CheckAmt, dbo.tbl_Cntrcts_Checks.CheckNum, dbo.tbl_Cntrcts_Checks.DepositNum, dbo.tbl_Cntrcts_Checks.DateRcvd, dbo.tbl_Cntrcts_Checks.Comments, dbo.tlkup_year_qtr.Title FROM dbo.tbl_Cntrcts_Checks INNER JOIN dbo.tlkup_year_qtr ON dbo.tbl_Cntrcts_Checks.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID WHERE (dbo.tbl_Cntrcts_Checks.CntrctNum =  '" & myFSSContractNum.ToString & "') ORDER BY dbo.tbl_Cntrcts_Checks.Quarter_ID DESC"
                    Try
                        conn.Open()
                        cmd = New SqlCommand(mySQLStr, conn)
                        rdr = cmd.ExecuteReader
                        If rdr.HasRows Then
                            myGridView.DataSource = rdr
                            myGridView.DataBind()
                        End If

                    Catch ex As Exception
                        Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Checks Detail. ")
                    Finally
                        conn.Close()
                    End Try
                End If
                Dim gvBPAFSSSINS As Global.System.Web.UI.WebControls.GridView
                gvBPAFSSSINS = CType(myFormView.Row.FindControl("gvBPAFSSSINS"), Global.System.Web.UI.WebControls.GridView)
                If Not gvBPAFSSSINS Is Nothing Then
                    mySQLStr = "SELECT * FROM [tbl_Cntrcts_SINs] WHERE [CntrctNum] = '" & myFSSContractNum.ToString & "'"
                    Try
                        conn.Open()
                        cmd = New SqlCommand(mySQLStr, conn)
                        rdr = cmd.ExecuteReader
                        If rdr.HasRows Then
                            gvBPAFSSSINS.DataSource = rdr
                            gvBPAFSSSINS.DataBind()
                        End If

                    Catch ex As Exception
                        Msg_Alert.Client_Alert_OnLoad("Error retrieving list of SINs for FSS Contract.")
                    Finally
                        conn.Close()
                    End Try
                End If
            End If
        End If
    End Sub
    Protected Sub setFSSDetailButtons(ByVal button As String)
        Dim myButton As Button = CType(fvContractInfo.Row.FindControl(button), Button)
        If button = "btnBPAFSSGeneral" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSVendor" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSContract" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSPOC" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSPrice" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSSales" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSChecks"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        ElseIf button = "btnBPAFSSChecks" Then
            myButton.ForeColor = Drawing.Color.Red
            myButton.BackColor = Drawing.Color.AntiqueWhite
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSSales"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSVendor"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSContract"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPOC"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSPrice"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
            myButton = CType(fvContractInfo.Row.FindControl("btnBPAFSSGeneral"), Button)
            myButton.ForeColor = Drawing.Color.Black
            myButton.BackColor = Drawing.Color.White
        End If
    End Sub

    Protected Sub btnBPAView_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim myButton As Button = CType(sender, Button)
        Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
        Dim myView As MultiView
        myView = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
        If myButton.ID.ToString = "btnBPAFSSGeneral" Then
            myView.ActiveViewIndex = 0
            setFSSDetailButtons("btnBPAFSSGeneral")
        ElseIf myButton.ID.ToString = "btnBPAFSSVendor" Then
            myView.ActiveViewIndex = 1
            setFSSDetailButtons("btnBPAFSSVendor")
        ElseIf myButton.ID.ToString = "btnBPAFSSContract" Then
            myView.ActiveViewIndex = 2
            setFSSDetailButtons("btnBPAFSSContract")
        ElseIf myButton.ID.ToString = "btnBPAFSSPOC" Then
            myView.ActiveViewIndex = 3
            setFSSDetailButtons("btnBPAFSSPOC")
        ElseIf myButton.ID.ToString = "btnBPAFSSPrice" Then
            myView.ActiveViewIndex = 4
            setFSSDetailButtons("btnBPAFSSPrice")
        ElseIf myButton.ID.ToString = "btnBPAFSSSales" Then
            myView.ActiveViewIndex = 5
            setFSSDetailButtons("btnBPAFSSSales")
        ElseIf myButton.ID.ToString = "btnBPAFSSChecks" Then
            myView.ActiveViewIndex = 6
            setFSSDetailButtons("btnBPAFSSChecks")
        End If

    End Sub
    ' called when parent form view is bound
    Protected Sub LoadAndBindParentFSSContractDownList()

        Dim parentFSSContractDownList As DropDownList = CType(fvContractInfo.Row.FindControl("ParentFSSContractDropDownList"), DropDownList)

        Dim bSuccess As Boolean = True
        Dim dsActiveFSSContractsForBPAParent As DataSet = Nothing
        Dim contractDB As ContractDB

        If Not Session("ActiveFSSContractsForBPAParentDataSet") Is Nothing Then
            dsActiveFSSContractsForBPAParent = CType(Session("ActiveFSSContractsForBPAParentDataSet"), DataSet)
        Else
            contractDB = CType(Session("ContractDB"), ContractDB)

            bSuccess = contractDB.SelectActiveFSSContractsForBPAParent(dsActiveFSSContractsForBPAParent)

            Session("ActiveFSSContractsForBPAParentDataSet") = dsActiveFSSContractsForBPAParent

        End If

        If bSuccess = True Then
            parentFSSContractDownList.ClearSelection()
            parentFSSContractDownList.Items.Clear()
            parentFSSContractDownList.DataSource = dsActiveFSSContractsForBPAParent
            parentFSSContractDownList.DataMember = "ActiveFSSContractsForBPAParentTable"
            parentFSSContractDownList.DataTextField = "CntrctNum"
            parentFSSContractDownList.DataValueField = "CntrctNum"
            parentFSSContractDownList.DataBind()

        Else
            '$$$  MsgBox.Alert(contractDB.ErrorMessage)
        End If

    End Sub

    Protected Sub ParentFSSContractDropDownList_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim ParentFSSContractDropDownList As DropDownList = CType(sender, DropDownList)
        Dim selectedParentFSSContract As String = ""

        Dim selectedItem As ListItem

        'set selection into control
        If Not Session("CurrentSelectedParentFSSContract") Is Nothing Then
            selectedParentFSSContract = CType(Session("CurrentSelectedParentFSSContract"), String)
            If selectedParentFSSContract.Length > 0 Then
                '  selectedItem = ParentFSSContractDropDownList.SelectedItem
                '  If Not selectedItem Is Nothing Then
                CMGlobals.SelectTextInDropDownListWithAdd(ParentFSSContractDropDownList, selectedParentFSSContract)
                LoadParentFSSContractFormView()
                'End If
            End If
        End If

    End Sub

    Protected Sub ParentFSSContractDropDownList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim ParentFSSContractDropDownList As DropDownList = CType(sender, DropDownList)
        Session("CurrentSelectedParentFSSContract") = CMGlobals.GetSelectedTextFromDropDownList(ParentFSSContractDropDownList)
        LoadParentFSSContractFormView()
    End Sub

    Protected Sub LoadParentFSSContractFormView()

        Dim fvParentFSSContractInfo As FormView = CType(fvContractInfo.Row.FindControl("fvParentFSSContractInfo"), FormView)

        If Not fvParentFSSContractInfo Is Nothing Then

            Dim parentFSSContractNumber As String = ""

            If Not Session("CurrentSelectedParentFSSContract") Is Nothing Then

                parentFSSContractNumber = Session("CurrentSelectedParentFSSContract")

                Dim bSuccess As Boolean = True

                Dim contractDB As ContractDB

                contractDB = CType(Session("ContractDB"), ContractDB)

                Dim dsParentContractInfo As DataSet = Nothing

                Try
                    bSuccess = contractDB.GetParentContractInfoForBPA(parentFSSContractNumber, dsParentContractInfo)

                    If bSuccess = True Then

                        fvParentFSSContractInfo.DataSource = dsParentContractInfo
                        fvParentFSSContractInfo.DataMember = "ParentContractInfoTable"
                        fvParentFSSContractInfo.DataBind()

                    End If
                Catch ex As Exception
                    Msg_Alert.Client_Alert_OnLoad(contractDB.ErrorMessage)
                End Try

            End If
        End If

    End Sub


    ''called on databound event for fss counterpart contract number drop down list on general page for bpa
    ''taking this one down on 9/6/2012
    'Protected Sub ParentFSSContractDropDownList_OnDataBinding(ByVal s As Object, ByVal e As EventArgs)
    '    Dim strSQL As String = "SELECT BPA_FSS_Counterpart FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
    '    Dim ds As New DataSet
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim cmd As SqlCommand
    '    Dim rdr As SqlDataReader
    '    Dim myContract As String = ""
    '    Try
    '        conn.Open()
    '        cmd = New SqlCommand(strSQL, conn)
    '        rdr = cmd.ExecuteReader
    '        While rdr.Read
    '            myContract = CType(rdr("BPA_FSS_Counterpart"), String)
    '        End While
    '        conn.Close()
    '    Catch ex As Exception
    '    Finally
    '        conn.Close()
    '    End Try
    '    'shouldnt always just add it, and what if this is a new bpa, is it already populated
    '    'on a "create" screen?  $$$
    '    Dim myDropDownlist As DropDownList = CType(s, DropDownList)
    '    Dim mylistItem As ListItem = New ListItem(myContract, myContract)
    '    myDropDownlist.Items.Add(mylistItem)
    '    myDropDownlist.SelectedValue = myContract
    '    Session("ParentFSSContractNumber") = myContract
    '    'If IsPostBack Then
    '    '    LoadAndBindParentFSSContractFormView()
    '    'End If
    'End Sub


    ' this is old version of onselectedindexchanged for ddl
    'Dim myDropDown As DropDownList = CType(sender, DropDownList)
    'Dim myContract As String = Request.QueryString("CntrctNum").ToString
    'Dim strSQL As String = "UPDATE tbl_Cntrcts Set BPA_FSS_Counterpart = '" & myDropDown.SelectedValue & "' WHERE CntrctNum ='" & myContract & "'"
    'Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    'Dim cmd As New SqlCommand(strSQL, conn)
    'Try
    '    conn.Open()
    '    cmd.ExecuteNonQuery()
    'Catch ex As Exception
    '    Msg_Alert.Client_Alert_OnLoad("Error Updating: " & ex.ToString)
    'Finally
    '    conn.Close()
    'End Try
    'Dim myFSSCOntract As HiddenField = CType(fvContractInfo.Row.FindControl("hfBPAFSSContract"), HiddenField)
    'myFSSCOntract.Value = myDropDown.SelectedValue

    ''save updated parent contract info into current document
    'Try
    '    Dim currentDocument As CurrentDocument
    '    Dim bSuccess As Boolean = False
    '    currentDocument = CType(Session("CurrentDocument"), CurrentDocument)
    '    If Not currentDocument Is Nothing Then
    '        currentDocument.UpdateParentDocument() $$$$
    '    End If
    'Catch ex As Exception
    '    Msg_Alert.Client_Alert_OnLoad("The following error was encountered when attempting to update the parent pharmaceutical contract: " & ex.ToString)

    'End Try

    'LoadAndBindParentFSSContractFormView() ' this accesses parent document in current document

    'End Sub
    Protected Sub setScheduleAttributes(ByVal scheduleNumber As Integer)
        Dim myScheduleNumber As Integer = scheduleNumber
        If myScheduleNumber = 15 Or myScheduleNumber = 39 Or myScheduleNumber = 41 Or myScheduleNumber = 44 Then
            Dim myButton As Button = CType(fvContractInfo.Row.FindControl("btnBPAInfo"), Button)
            If Not myButton Is Nothing Then
                myButton.Visible = "True"
                myButton = CType(fvContractInfo.Row.FindControl("btnBPAPrice"), Button)
                myButton.Visible = "True"
                myButton = CType(fvContractInfo.Row.FindControl("btnFSSDetails"), Button)
                myButton.Visible = "True"
                myButton = CType(fvContractInfo.Row.FindControl("btnContract"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnVendor"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnPOC"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
                myButton.Visible = "False"
                'Dim myImageButton As ImageButton = CType(fvContractInfo.Row.FindControl("btnViewExcel"), ImageButton)
                'If Not myImageButton Is Nothing Then
                '    myImageButton.Visible = False
                'End If
                'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbExportPriceList"), Label)
                'If Not myLabel Is Nothing Then
                '    myLabel.Visible = False
                'End If
            End If
        End If
        If myScheduleNumber = 10 Then
            Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbPolicyInfo"), Label)
            If Not myLabel Is Nothing Then
                myLabel.Visible = "True"
            End If
            Dim mytextBox As TextBox = CType(fvContractInfo.Row.FindControl("tbInsDateEff"), TextBox)
            If Not mytextBox Is Nothing Then
                mytextBox.Visible = "True"
            End If
            mytextBox = CType(fvContractInfo.Row.FindControl("tbInsurePolicyExpDate"), TextBox)
            If Not mytextBox Is Nothing Then
                mytextBox.Visible = "True"
            End If
        End If
        If myScheduleNumber = 36 Then
            Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbPolicyInfo"), Label)
            If Not myLabel Is Nothing Then
                myLabel.Visible = "True"
            End If
            Dim mytextBox As TextBox = CType(fvContractInfo.Row.FindControl("tbInsDateEff"), TextBox)
            If Not mytextBox Is Nothing Then
                mytextBox.Visible = "True"
            End If
            mytextBox = CType(fvContractInfo.Row.FindControl("tbInsurePolicyExpDate"), TextBox)
            If Not mytextBox Is Nothing Then
                mytextBox.Visible = "True"
            End If
            myLabel = CType(fvContractInfo.Row.FindControl("lbPrimeVendor"), Label)
            If Not myLabel Is Nothing Then
                myLabel.Visible = "False"
            End If
            myLabel = CType(fvContractInfo.Row.FindControl("lbVADOD"), Label)
            If Not myLabel Is Nothing Then
                myLabel.Visible = "False"
            End If
            Dim myCheckBox As CheckBox = CType(fvContractInfo.Row.FindControl("cbPrimeVendor"), CheckBox)
            If Not myCheckBox Is Nothing Then
                myCheckBox.Visible = "False"
            End If
            myCheckBox = CType(fvContractInfo.Row.FindControl("cbDODVACOntract"), CheckBox)
            If Not myCheckBox Is Nothing Then
                myCheckBox.Visible = "False"
            End If
            'Dim myButton As ImageButton = CType(fvContractInfo.Row.FindControl("btnViewExcel"), ImageButton)
            'If Not myButton Is Nothing Then
            '    myButton.Visible = "False"
            'End If
            'myLabel = CType(fvContractInfo.Row.FindControl("lbExportPriceList"), Label)
            'If Not myLabel Is Nothing Then
            '    myLabel.Visible = "False"
            'End If

        End If
    End Sub
    Protected Sub setControls()
        Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSINS"), Global.System.Web.UI.WebControls.GridView)
        Dim myPanel As Panel = CType(fvContractInfo.Row.FindControl("pnGVSINS"), Panel)

        If myGridView Is Nothing Then
            If Not myPanel Is Nothing Then
                myPanel.Visible = "False"
            End If

        Else
            If myGridView.Rows.Count > 0 Then
                If Not myPanel Is Nothing Then
                    myPanel.Visible = "True"
                End If

            Else
                If Not myPanel Is Nothing Then
                    myPanel.Visible = "True"

                End If


            End If
        End If
    End Sub
    Protected Sub grid_Command(ByVal s As Object, ByVal e As GridViewCommandEventArgs)
        Dim mySender As String = ""
        If e.CommandName = "Detail" Then
            Dim myIndex As Integer = CType(e.CommandArgument, Integer)
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myIndex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myIndex)
            Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
            Dim myScript As String = "<script>window.open('sales_detail.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Details','toolbar=no,menubar=no,resizable=no,width=865,height=300')</script>"
            Response.Write(myScript)
        End If
        If e.CommandName = "Update_Data" Then
            Dim myIndex As Integer = CType(e.CommandArgument, Integer)
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myIndex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myIndex)
            Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
            Dim myScript As String = "<script>window.open('sales_detail_edit.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Edit','toolbar=no,menubar=no,resizable=no,width=300,height=300,scrollbars=yes')</script>"
            Response.Write(myScript)
        End If
    End Sub
    Protected Sub refreshSalesDate(ByVal s As Object, ByVal e As EventArgs)
        Dim myValue As String = hfRefreshSales.Value.ToString
    End Sub
    ' took out Protected Sub checkSalesEditable() on 12/29/2009 - it wasnt used

    Protected Sub openSalesEntry(ByVal s As Object, ByVal e As EventArgs)
        Dim myCntrctNum As String = Request.QueryString("cntrctNum").ToString
        Dim myScript As String = "<script>window.open('sales_entry.aspx?CntrctNum=" & myCntrctNum & "&Page=3&SchNum=" & Request.QueryString("SchNum").ToString & "','Details','toolbar=no,menubar=no,resizable=1,width=865,height=400')</script>"
        Response.Write(myScript)
    End Sub

    Protected Sub CheckExpired(ByVal s As Object, ByVal e As EventArgs)
        Dim myDropdownlist As DropDownList = CType(s, DropDownList)
        If myDropdownlist.ID = "dlExpirationDate" Then
            Dim strSQL As String = "SELECT Dates_CntrctExp FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand

            Dim rdr As SqlDataReader
            Dim myExpiration As DateTime
            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                While rdr.Read
                    myExpiration = CType(rdr("Dates_CntrctExp"), DateTime)
                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
            myDropdownlist.Items.Add(mylistItem)
            myDropdownlist.SelectedValue = myExpiration.ToShortDateString
        ElseIf myDropdownlist.ID = "dlEndDate" Then
            Dim strSQL As String = "SELECT Dates_Completion FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand
            Dim rdr As SqlDataReader
            Dim myExpiration As DateTime

            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                While rdr.Read
                    If Not rdr("Dates_Completion").Equals(DBNull.Value) Then
                        myExpiration = CType(rdr("Dates_Completion"), DateTime)
                    End If
                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
            If myExpiration = "01/01/0001" Then
                myDropdownlist.Items.Add(mylistItem)
                myDropdownlist.SelectedValue = ""
            Else
                myDropdownlist.Items.Add(mylistItem)
                myDropdownlist.SelectedValue = myExpiration.ToShortDateString
            End If

        End If
    End Sub

    Protected Function checkExpiration() As Boolean
        Dim strSQL As String = "SELECT Dates_CntrctExp FROM tbl_Cntrcts WHERE CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "'"
        Dim ds As New DataSet
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Dim myExpiration As DateTime
        Dim Expired As Boolean
        Try
            conn.Open()
            cmd = New SqlCommand(strSQL, conn)
            rdr = cmd.ExecuteReader
            While rdr.Read
                myExpiration = CType(rdr("Dates_CntrctExp"), DateTime)
            End While
            conn.Close()
        Catch ex As Exception
        Finally
            conn.Close()
        End Try
        If myExpiration < Now Then
            Expired = "True"
        Else
            Expired = "False"
        End If
        Return Expired
    End Function
    Protected Sub CheckContrct(ByVal sender As Object, ByVal e As EventArgs)
        If checkExpiration() Then
            Dim myDropDown As DropDownList = CType(sender, DropDownList)
            Dim myListItem As ListItem = New ListItem(Request.QueryString("CntrctNum").ToString)
            myDropDown.Items.Add(myListItem)
            myDropDown.SelectedValue = Request.QueryString("CntrctNum").ToString
        End If
    End Sub

    Private Sub ContractDataSource_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles ContractDataSource.Init

    End Sub
    Private Sub ContractDataSource_Updated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs) Handles ContractDataSource.Updated
        Dim myScript As String = ""
        Dim view As MultiView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)
        If Not view Is Nothing Then
            Session("CurrentViewIndex") = view.ActiveViewIndex
        End If

        '      Dim dlState As DropDownList = CType(fvContractInfo.FindControl("dlState"), DropDownList)
        '      If Not dlState Is Nothing Then
        'dlState.DataBind()
        '     End If

        Dim dlOrderState As DropDownList = CType(fvContractInfo.FindControl("dlOrderState"), DropDownList)
        If Not dlOrderState Is Nothing Then
            dlOrderState.DataBind()
        End If

    End Sub

    Protected Sub itemView_prerender(ByVal s As Object, ByVal e As EventArgs)
        If IsPostBack Then
            Dim myMultiview As MultiView = CType(s, MultiView)
            myMultiview.ActiveViewIndex = CType(Session("CurrentViewIndex"), Integer)
            setButtonColors()
        End If
    End Sub

    Protected Sub setButtonColors()
        Dim myView As Integer = CType(Session("CurrentViewIndex"), Integer)
        If myView = 0 Then
            setButtonColors("btnGeneral")
        ElseIf myView = 1 Then
            setButtonColors("btnVendor")
        ElseIf myView = 2 Then
            setButtonColors("btnContract")
        ElseIf myView = 3 Then
            setButtonColors("btnPOC")
        ElseIf myView = 4 Then
            setButtonColors("btnPrice")
        ElseIf myView = 5 Then
            setButtonColors("btnSales")
        ElseIf myView = 6 Then
            setButtonColors("btnChecks")
        ElseIf myView = 7 Then
            setButtonColors("btnSBA")
        ElseIf myView = 8 Then
            setButtonColors("btnBOC")
        ElseIf myView = 9 Then
            setButtonColors("btnBPAInfo")
        ElseIf myView = 10 Then
            setButtonColors("btnBPAPrice")
        ElseIf myView = 11 Then
            setButtonColors("btnFSSDetails")

            'populateFSSDetail()

            'Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
            'Dim myViewFSS As MultiView
            'myViewFSS = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
            'myViewFSS.ActiveViewIndex = 0

        End If
    End Sub
    Protected Sub UpdateSINGrid(ByVal s As Object, ByVal e As GridViewCommandEventArgs)


        If e.CommandName = "NoDataInsert" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvBPAFSSSINS"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.Controls(0).Controls(0).FindControl("dlNoSin"), DropDownList)
            Dim myCheckBox As CheckBox = CType(myGridView.Controls(0).Controls(0).FindControl("cbInsertRC"), CheckBox)
            Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.VarChar, 10)
            Dim myRC As SqlParameter = New SqlParameter("@Recoverable", SqlDbType.Bit)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            mySIN.Direction = ParameterDirection.Input
            mySIN.Value = myDropList.SelectedValue
            myRC.Direction = ParameterDirection.Input
            myContractNumber.Direction = ParameterDirection.Input
            myContractNumber.Value = CntrctNumber
            If myCheckBox.Checked Then
                myRC.Value = 1
            Else
                myRC.Value = 0
            End If

            insertParameters(0) = mySIN
            insertParameters(1) = myRC
            insertParameters(2) = myContractNumber
            SINDataSource.Insert()
        ElseIf e.CommandName = "InsertNew" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvBPAFSSSINS"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.FooterRow.FindControl("dlAddSin"), DropDownList)
            Dim myCheckBox As CheckBox = CType(myGridView.FooterRow.FindControl("cbInsertRC"), CheckBox)
            Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.VarChar, 10)
            Dim myRC As SqlParameter = New SqlParameter("@Recoverable", SqlDbType.Bit)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            mySIN.Direction = ParameterDirection.Input
            mySIN.Value = myDropList.SelectedValue
            myRC.Direction = ParameterDirection.Input
            myContractNumber.Direction = ParameterDirection.Input
            myContractNumber.Value = CntrctNumber
            If myCheckBox.Checked Then
                myRC.Value = 1
            Else
                myRC.Value = 0
            End If
            insertParameters(0) = mySIN
            insertParameters(1) = myRC
            insertParameters(2) = myContractNumber
            SINDataSource.Insert()
        End If
    End Sub
    Private Sub SINDataSource_Inserting1(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles SINDataSource.Inserting
        e.Command.Parameters.Clear()
        Dim i As Integer
        For i = 0 To 2
            e.Command.Parameters.Add(insertParameters(i))
        Next i
    End Sub


    Protected Sub UpdateStateCoverageGrid(ByVal s As Object, ByVal e As GridViewCommandEventArgs)
        If e.CommandName = "InsertNew" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvStateCoverage"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.Controls(0).Controls(0).FindControl("dlNewState"), DropDownList)
            Dim myState As SqlParameter = New SqlParameter("@abbr", SqlDbType.NVarChar, 2)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            myState.Direction = ParameterDirection.Input
            myState.Value = myDropList.SelectedValue
            myContractNumber.Direction = ParameterDirection.Input
            myContractNumber.Value = CntrctNumber
            insertStateParameters(0) = myState
            insertStateParameters(1) = myContractNumber
            StateCoverageDataSource.Insert()
        ElseIf e.CommandName = "InsertNow" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvStateCoverage"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.FooterRow.FindControl("dlItemState"), DropDownList)
            Dim myState As SqlParameter = New SqlParameter("@abbr", SqlDbType.NVarChar, 2)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            myState.Direction = ParameterDirection.Input
            myState.Value = myDropList.SelectedValue
            myContractNumber.Direction = ParameterDirection.Input
            myContractNumber.Value = CntrctNumber
            insertStateParameters(0) = myState
            insertStateParameters(1) = myContractNumber
            StateCoverageDataSource.Insert()
        End If
    End Sub

    Private Sub StateCoverageDataSource_Inserting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles StateCoverageDataSource.Inserting
        e.Command.Parameters.Clear()
        Dim i As Integer
        For i = 0 To 1
            e.Command.Parameters.Add(insertStateParameters(i))
        Next i
    End Sub
    Protected Sub UpdateChecksGrid(ByVal s As Object, ByVal e As GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName = "InsertEmpty" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.Controls(0).Controls(0).FindControl("dlQuarterInsertNew"), DropDownList)
            Dim myTextBox As TextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCheckAmountInsertNew"), TextBox)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            Dim myInt As Integer = CType(myDropList.SelectedValue, Integer)
            Dim myQuarter As Integer = myInt
            Dim myContractNumber As String = CntrctNumber

            Dim myCheckAmount As Double = CType(myTextBox.Text, Double)
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbChecknumInsertNew"), TextBox)
            Dim myCheckNumber As String = myTextBox.Text
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCheckDepositInsertNew"), TextBox)
            Dim myDepositNum As String = myTextBox.Text
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbdateRecvdInsertNew"), TextBox)
            Dim myDateRecvd As DateTime = CType(myTextBox.Text, DateTime)
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCommentsInsertNew"), TextBox)
            Dim myComments As String = myTextBox.Text
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim strSQL As String = "INSERT INTO dbo.tbl_Cntrcts_Checks (CntrctNum, Quarter_ID, CheckAmt, CheckNum, DepositNum, DateRcvd, Comments, LastModifiedBy, LastModificationDate) VALUES (@CntrctNum, @Quarter_ID, @CheckAmt, @CheckNum, @DepositNum, @DateRcvd, @Comments, @LastModifiedBy, @LastModificationDate)"
                Dim insertCommand As SqlCommand = New SqlCommand()
                insertCommand.Connection = conn
                insertCommand.CommandText = strSQL
                insertCommand.Parameters.Add("@Quarter_ID", SqlDbType.Int).Value = myQuarter
                insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = myContractNumber
                insertCommand.Parameters.Add("@CheckAmt", SqlDbType.Money).Value = myCheckAmount
                insertCommand.Parameters.Add("@CheckNum", SqlDbType.VarChar, 50).Value = myCheckNumber
                insertCommand.Parameters.Add("@DepositNum", SqlDbType.VarChar, 50).Value = myDepositNum
                insertCommand.Parameters.Add("@DateRcvd", SqlDbType.DateTime).Value = myDateRecvd
                insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar, 255).Value = myComments
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
                insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()
                conn.Open()
                insertCommand.ExecuteNonQuery()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the insert. ")
            Finally
                conn.Close()
            End Try
        ElseIf e.CommandName = "InsertNew" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.Controls(0).Controls(0).FindControl("dlQuarterInsertNew"), DropDownList)
            Dim myTextBox As TextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCheckAmountInsertNew"), TextBox)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            Dim myInt As Integer = CType(myDropList.SelectedValue, Integer)
            Dim myQuarter As Integer = myInt
            Dim myContractNumber As String = CntrctNumber
            Dim myCheckAmount As Double = CType(myTextBox.Text, Double)

            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbChecknumNew"), TextBox)
            Dim myCheckNumber As String = myTextBox.Text
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCheckDepositNew"), TextBox)
            Dim myDepositNum As String = myTextBox.Text
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbdateRecvdNew"), TextBox)
            Dim myDateRecvd As DateTime = CType(myTextBox.Text, DateTime)
            myTextBox = CType(myGridView.Controls(0).Controls(0).FindControl("tbCommentsNew"), TextBox)
            Dim myComments As String = myTextBox.Text
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim strSQL As String = "INSERT INTO dbo.tbl_Cntrcts_Checks (CntrctNum, Quarter_ID, CheckAmt, CheckNum, DepositNum, DateRcvd, Comments, LastModifiedBy, LastModificationDate) VALUES (@CntrctNum, @Quarter_ID, @CheckAmt, @CheckNum, @DepositNum, @DateRcvd, @Comments, @LastModifiedBy, @LastModificationDate)"
                Dim insertCommand As SqlCommand = New SqlCommand()
                insertCommand.Connection = conn
                insertCommand.CommandText = strSQL
                insertCommand.Parameters.Add("@Quarter_ID", SqlDbType.Int).Value = myQuarter
                insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = myContractNumber
                insertCommand.Parameters.Add("@CheckAmt", SqlDbType.Money).Value = myCheckAmount
                insertCommand.Parameters.Add("@CheckNum", SqlDbType.VarChar, 50).Value = myCheckNumber
                insertCommand.Parameters.Add("@DepositNum", SqlDbType.VarChar, 50).Value = myDepositNum
                insertCommand.Parameters.Add("@DateRcvd", SqlDbType.DateTime).Value = myDateRecvd
                insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar, 255).Value = myComments
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
                insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()
                conn.Open()
                insertCommand.ExecuteNonQuery()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the insert. ")
            Finally
                conn.Close()
            End Try
        End If
    End Sub
    Protected Sub ShowCheckFooter(ByVal s As Object, ByVal e As System.EventArgs)
        If isFinance2() Then
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
            Dim myButton As Button = CType(s, Button)
            If myButton.ID.Equals("btnShowFooter") Then
                myGrid.ShowFooter = "True"
                myGrid.Columns(6).Visible = "True"
            ElseIf myButton.ID.Equals("btnHideFooter") Then
                myGrid.ShowFooter = "False"
            End If
        Else
            Msg_Alert.Client_Alert_OnLoad("You must be idenified as a member of finance to enter check info.  See the system admin.")
        End If
    End Sub
    Private Sub ChecksDataSource_Updating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles ChecksDataSource.Updating
        'e.Command.Parameters.Clear()
        'Dim i As Integer
        'For i = 0 To 1
        '    e.Command.Parameters.Add(insertCheckParameters(i))
        'Next i
    End Sub
    Protected Sub AddProjection_Click(ByVal s As Object, ByVal e As EventArgs)
        Dim strURL As String = ""
        Dim strFeature As String = ""
        Response.Write("<script>window.open('" & strURL & "','Proj','" & strFeature & "')</script>)")
    End Sub
    Protected Sub SBAProjection_Command(ByVal s As Object, ByVal e As GridViewCommandEventArgs)

        If e.CommandName = "Details" Then
            Dim myIndex As Integer = CType(e.CommandArgument, Integer)
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myIndex)
            Dim mydataKey As String = myGrid.DataKeys(myIndex).Values(0).ToString
            Dim myProjectID As String = ""
            Dim mySBAPlanID As String = ""
            mySBAPlanID = myGrid.DataKeys(myIndex).Values(1).ToString
            Dim strURL As String = "sba_projections.aspx?SBAPlanID=" & mySBAPlanID
            Dim myScript As String = "<script>window.open('sba_projections.aspx?SBAPlanID=" & mySBAPlanID & "&ProjID=" & mydataKey & "&state=1','Details','toolbar=no,menubar=no,resizable=no,width=805,height=450')</script>"
            Response.Write(myScript)
        ElseIf e.CommandName = "AddProjections" Then

        End If
    End Sub
    Protected Sub SBAPlanUpdate(ByVal s As Object, ByVal e As EventArgs)
        dataSBAPlanTypes()
    End Sub

    Protected Function isFinance2() As Boolean
        Dim bs As BrowserSecurity2
        Dim isAllowed As Boolean
        bs = CType(Session("BrowserSecurity"), BrowserSecurity2)
        If Not bs Is Nothing Then
            If Not bs.SecurityMatrix Is Nothing Then
                isAllowed = bs.CheckPermissions(BrowserSecurity2.AccessPoints.Checks)
            End If
        End If

        Return isAllowed
    End Function

    'Protected Function isFinance() As Boolean
    '    Dim myAnswer As Boolean = "False"
    '    Dim CurrentUser As String = ""
    '    If Not Session("UserName") Is Nothing Then
    '        If Session("UserName").ToString.IndexOf("\") > 0 Then
    '            Dim myTemp As String = CType(Session("UserName"), String)
    '            Dim start As Integer = myTemp.IndexOf("\") + 1
    '            Dim finish As Integer = myTemp.Length - 1
    '            CurrentUser = myTemp.Substring(start)
    '        End If
    '    End If
    '    Dim strSQL = "SELECT * FROM tlkup_UserProfile WHERE (UserName ='" & Session("UserName") & "' AND USERTitle = 1) OR (UserName ='" & CurrentUser & "' AND USERTitle = 1)"
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim cmd As SqlCommand
    '    Dim rdr As SqlDataReader
    '    Try
    '        conn.Open()
    '        cmd = New SqlCommand(strSQL, conn)
    '        rdr = cmd.ExecuteReader
    '        If rdr.HasRows Then
    '            myAnswer = "True"
    '        End If
    '        conn.Close()
    '    Catch ex As Exception
    '    Finally
    '        conn.Close()
    '    End Try
    '    Return myAnswer
    'End Function

    Protected Sub checkCheckContent()
        Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
        If myGridView.Rows.Count < 1 Then
            Dim myButton As Button = CType(fvContractInfo.Row.FindControl("btnShowFooter"), Button)
            myButton.Visible = "False"
            myButton = CType(fvContractInfo.Row.FindControl("btnHideFooter"), Button)
            myButton.Visible = "False"
        End If

    End Sub
    Protected Sub ShowColumn(ByVal sender As Object, ByVal e As EventArgs)
        If isFinance2() Then
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
            myGrid.Columns(6).Visible = "True"
        Else
            Msg_Alert.Client_Alert_OnLoad("You must be idenified as a member of finance to enter check info.  See the system admin.")
        End If
    End Sub

    ' add client script to
    ' set up the click function for the export button
    ' and to refresh the parent window
    Protected Sub SetupSalesExport()
        'Dim contractNumber As String = Request.QueryString("CntrctNum").ToString
        'Dim exportUploadSalesButton As Button = CType(fvContractInfo.Row.FindControl("exportUploadSalesButton"), Button)
        'exportUploadSalesButton.Attributes.Add("onclick", "ExportUpload('" & contractNumber & "')")

        'Dim clientScriptManager As ClientScriptManager = Page.ClientScript

        'Dim openFunctionCallString As String = "function ExportUpload( contractNumber ) { exportWindow = window.open(""../SalesExportUpload/QuarterlySalesReport.aspx?ContractNumber="" + contractNumber ,"""",""resizable=0,scrollbars=1,menubar=no,width=420,height=530,left=440,top=130"") ; }"
        'clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "ExportUpload", openFunctionCallString, True)

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        Dim contractNumber As String = currentDocument.ContractNumber
        Dim scheduleNumberStr As String = currentDocument.ScheduleNumber.ToString()

        Dim exportUploadSalesButton As Button = CType(fvContractInfo.Row.FindControl("exportUploadSalesButton"), Button)
        exportUploadSalesButton.Attributes.Add("onclick", "ExportUpload('" & contractNumber & "','" & scheduleNumberStr & "')")

        Dim clientScriptManager As ClientScriptManager = Page.ClientScript

        Dim openFunctionCallString As String = "function ExportUpload( contractNumber, scheduleNumber ) { exportWindow = window.open(""../SalesExportUpload/QuarterlySalesReport.aspx?ContractNumber="" + contractNumber + ""&ScheduleNumber="" + scheduleNumber ,"""",""resizable=0,scrollbars=1,menubar=no,width=420,height=630,left=440,top=130"") ; }"
        clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "ExportUpload", openFunctionCallString, True)

        Dim refreshSalesDataGridFunctionCallString As String = "function RefreshSalesDataGrid() { document.getElementById('fvContractInfo_RefreshSalesDataGridOnSubmit').value = ""true""; __doPostBack("",""); }"
        clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "RefreshSalesDataGrid", refreshSalesDataGridFunctionCallString, True)
    End Sub
    Public Sub RefreshSalesDataGrid()
        Dim gvSales As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        gvSales.DataBind()
    End Sub
    Public Sub RefreshPOCView()
        Dim formViewContractInfo As FormView = CType(form1.FindControl("fvContractInfo"), FormView)
        formViewContractInfo.DataBind()
    End Sub
    'Protected Sub excel_Click(ByVal s As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
    '    Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
    '    Dim myTitle As String = "PriceList_" & myContractNumber & "_" & Date.Now.Month.ToString & Date.Now.Day.ToString & Date.Now.Year.ToString & Date.Now.Hour.ToString & Date.Now.Minute.ToString & ".xls"
    '    open_Excel(myCntrctNum, myTitle)
    'End Sub
    'Protected Sub open_Excel(ByVal contractNumber As String, ByVal title As String)
    '    Dim xlApp As Excel.Application
    '    Dim XlBook As Excel.Workbook
    '    Dim xlWorkSheet As Excel.Worksheet
    '    Dim myFile As String = "C:\" & title
    '    xlApp = New Excel.Application
    '    XlBook = xlApp.Workbooks.Open("\\ammhinsql1\Templates$\Pricelist_template.xls")
    '    xlWorkSheet = XlBook.Sheets("Mods")
    '    xlApp.Visible = True
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim strSQL As String = ""
    '    Dim ds As New DataSet
    '    Dim cmdCall As New SqlCommand
    '    cmdCall.Connection = conn
    '    cmdCall.CommandType = CommandType.Text
    '    cmdCall.CommandText = "SELECT TOP 100 PERCENT * FROM(tbl_pricelist) WHERE (Removed = 0) AND CntrctNum = @CntrctNum ORDER BY CntrctNum, SIN, [Contractor Catalog Number]"
    '    cmdCall.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = contractNumber
    '    Dim myDataAdapter As New SqlDataAdapter
    '    myDataAdapter.SelectCommand = cmdCall
    '    myDataAdapter.Fill(ds)
    '    For i = 0 To ds.Tables(0).Rows.Count - 1
    '        For j = 0 To ds.Tables(0).Columns.Count - 1
    '            xlWorkSheet.Cells(i + 2, j + 1) = _
    '            ds.Tables(0).Rows(i).Item(j)
    '        Next
    '    Next
    '    xlWorkSheet.Columns("A:A").NumberFormat = "0"
    '    xlWorkSheet.SaveAs(myFile)
    '    ' XlBook.Close()
    '    ' xlApp.Quit()
    '    ' releaseObject(xlApp)
    '    ' releaseObject(XlBook)
    '    ' releaseObject(xlWorkSheet)
    '    'Msg_Alert.Client_Alert_OnLoad("The exported file can be found at " & myFile)
    'End Sub

    Private Sub releaseObject(ByVal obj As Object)
        Try
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
        Finally
            GC.Collect()
        End Try
    End Sub
    Protected Sub setUpSalesFun()
        Dim clientScriptManager As ClientScriptManager = Page.ClientScript
        Dim openFunctionCallString As String = "function SalesUpdate( contractNumber, QtrID ) { exportWindow = window.open(""sales_detail_edit.aspx?CntrctNum="" + contractNumber + ""&QTRID="" + QtrID ,"""",""resizable=0,scrollbars=1,menubar=no,width=800,height=530,left=0,top=0"") ; }"
        clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "SalesUpload", openFunctionCallString, True)
        Dim refreshSalesDataGridFunctionCallString As String = "function RefreshSalesDataGrid() { document.getElementById('fvContractInfo_RefreshSalesDataGridOnSubmit').value = ""true""; __doPostBack("",""); }"
        clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "RefreshSalesDataGrid", refreshSalesDataGridFunctionCallString, True)
    End Sub

    Protected Function GetTAACheckBoxValue(ByVal TradeAgreementActCompliance As String, ByVal CheckBoxType As String) As Boolean
        Dim checked As Boolean = False
        If CheckBoxType.CompareTo("Yes") = 0 Then
            If TradeAgreementActCompliance.CompareTo("C") = 0 Then
                checked = True
            End If
        ElseIf CheckBoxType.CompareTo("Other") = 0 Then
            If TradeAgreementActCompliance.CompareTo("O") = 0 Then
                checked = True
            End If
        End If
        'else, its a null string, so false
        Return checked
    End Function

    Protected Sub TAAYesCheckBox_OnCheckedChanged(ByVal sender As Object, ByVal args As EventArgs)
        Dim TAAYesCheckBox As CheckBox = CType(sender, CheckBox)
        Dim TAAOtherCheckBox As CheckBox
        Dim ContractFormView As FormView

        If Not TAAYesCheckBox Is Nothing Then
            ContractFormView = CType(TAAYesCheckBox.NamingContainer, FormView)
            If Not ContractFormView Is Nothing Then
                TAAOtherCheckBox = CType(ContractFormView.FindControl("TAAOtherCheckBox"), CheckBox)
                If Not TAAOtherCheckBox Is Nothing Then
                    If TAAYesCheckBox.Checked = True Then
                        TAAOtherCheckBox.Checked = False
                    End If
                End If
            End If
        End If
    End Sub

    Protected Sub TAAOtherCheckBox_OnCheckedChanged(ByVal sender As Object, ByVal args As EventArgs)
        Dim TAAOtherCheckBox As CheckBox = CType(sender, CheckBox)
        Dim TAAYesCheckBox As CheckBox
        Dim ContractFormView As FormView

        If Not TAAOtherCheckBox Is Nothing Then
            ContractFormView = CType(TAAOtherCheckBox.NamingContainer, FormView)
            If Not ContractFormView Is Nothing Then
                TAAYesCheckBox = CType(ContractFormView.FindControl("TAAYesCheckBox"), CheckBox)
                If Not TAAYesCheckBox Is Nothing Then
                    If TAAOtherCheckBox.Checked = True Then
                        TAAYesCheckBox.Checked = False
                    End If
                End If
            End If
        End If
    End Sub

    Protected Sub ContractDataSource_Updating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles ContractDataSource.Updating
        'transform the TAA checkbox into a string 
        Dim TAAOtherCheckBox As CheckBox
        Dim TAAYesCheckBox As CheckBox
        Dim savedValue As String = ""

        TAAYesCheckBox = CType(fvContractInfo.FindControl("TAAYesCheckBox"), CheckBox)
        TAAOtherCheckBox = CType(fvContractInfo.FindControl("TAAOtherCheckBox"), CheckBox)

        If Not TAAOtherCheckBox Is Nothing Then
            If Not TAAYesCheckBox Is Nothing Then
                If TAAYesCheckBox.Checked = True Then
                    savedValue = "C"
                ElseIf TAAOtherCheckBox.Checked = True Then
                    savedValue = "O"
                End If
            End If
        End If
        'else could be null string
        e.Command.Parameters("@TradeAgreementActCompliance").Value = savedValue

        Dim stateDropDownList As DropDownList
        Dim selectedState As String
        Dim orderingStateDropDownList As DropDownList
        Dim selectedOrderingState As String
        Dim noneSelectedAbbreviation As String = "--" 'none selected

        'stateDropDownList = CType(fvContractInfo.FindControl("dlState"), DropDownList)

        'If Not stateDropDownList Is Nothing Then
        '    selectedState = CMGlobals.GetSelectedTextFromDropDownList(stateDropDownList)
        '    If selectedState.CompareTo(noneSelectedAbbreviation) <> 0 Then
        '        e.Command.Parameters("@Primary_State").Value = selectedState
        '    End If
        'End If

        orderingStateDropDownList = CType(fvContractInfo.FindControl("dlOrderState"), DropDownList)

        If Not orderingStateDropDownList Is Nothing Then
            selectedOrderingState = CMGlobals.GetSelectedTextFromDropDownList(orderingStateDropDownList)
            If selectedOrderingState.CompareTo(noneSelectedAbbreviation) <> 0 Then
                e.Command.Parameters("@Ord_State").Value = selectedOrderingState
            End If
        End If
    End Sub
    Protected Sub MakeExportViewPricelistButtonsVisible()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim lbViewMedSurgPricelistLabel As Label
        Dim btnViewPricelist As Button
        lbViewMedSurgPricelistLabel = CType(fvContractInfo.FindControl("lbViewMedSurgPricelistLabel"), Label)
        btnViewPricelist = CType(fvContractInfo.FindControl("btnViewPricelist"), Button)

        Dim lbViewDrugItemPricelistLabel As Label
        Dim btnViewDrugItemPricelist As Button
        lbViewDrugItemPricelistLabel = CType(fvContractInfo.FindControl("lbViewDrugItemPricelistLabel"), Label)
        btnViewDrugItemPricelist = CType(fvContractInfo.FindControl("btnViewDrugItemPricelist"), Button)

        Dim lbExportPriceList As Label
        Dim btnExportPricelistToExcel As ImageButton
        lbExportPriceList = CType(fvContractInfo.FindControl("lbExportPriceList"), Label)
        btnExportPricelistToExcel = CType(fvContractInfo.FindControl("btnExportPricelistToExcel"), ImageButton)

        Dim lbExportDrugItemPriceList As Label
        Dim btnExportDrugItemPricelistToExcel As ImageButton
        lbExportDrugItemPriceList = CType(fvContractInfo.FindControl("lbExportDrugItemPriceList"), Label)
        btnExportDrugItemPricelistToExcel = CType(fvContractInfo.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)

        'Dim DrugItemExportTypeRadioButtonList As RadioButtonList
        'DrugItemExportTypeRadioButtonList = CType(fvContractInfo.FindControl("DrugItemExportTypeRadioButtonList"), RadioButtonList)

        Dim lbMedSurgItemCount As Label
        Dim lbDrugItemCountLabel As Label
        Dim lbDrugItemCount As Label
        Dim lbCoveredFCPCount As Label
        Dim lbPPVTotalCount As Label

        lbMedSurgItemCount = CType(fvContractInfo.FindControl("lbMedSurgItemCount"), Label)
        lbDrugItemCountLabel = CType(fvContractInfo.FindControl("lbDrugItemCountLabel"), Label)
        lbDrugItemCount = CType(fvContractInfo.FindControl("lbDrugItemCount"), Label)
        lbCoveredFCPCount = CType(fvContractInfo.FindControl("lbCoveredFCPCount"), Label)
        lbPPVTotalCount = CType(fvContractInfo.FindControl("lbPPVTotalCount"), Label)

        lbMedSurgItemCount.Text = String.Format("{0}/{1}", currentDocument.ActiveMedSurgItemCount, currentDocument.FutureMedSurgItemCount)

        ' vars used by drug item form
        Dim openPricelistWindowScript As String
        '$$$ replace some of these parms by retrieving current document on the next form
        Dim contractNumber As String = currentDocument.ContractNumber
        Dim isEditable As String = "N"
        If currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
            isEditable = "Y"
        End If

        Dim isNational As String = "N"
        If (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.National) Then
            isNational = "Y"
        End If

        Dim vendorName As String = currentDocument.VendorName

        If (currentDocument.IsPharmaceutical(currentDocument.ScheduleNumber) = True) Then
            'lbViewDrugItemPricelistLabel.Visible = False
            'btnViewDrugItemPricelist.Visible = False
            'lbExportDrugItemPriceList.Visible = False
            'btnExportDrugItemPricelistToExcel.Visible = False
            'lbDrugItemCountLabel.Visible = False
            'lbDrugItemCount.Visible = False
            'lbCoveredFCPCount.Visible = False
            'DrugItemExportTypeRadioButtonList.Visible = False

            lbViewDrugItemPricelistLabel.Visible = True
            btnViewDrugItemPricelist.Visible = True
            lbExportDrugItemPriceList.Visible = True
            btnExportDrugItemPricelistToExcel.Visible = True
            SetDrugItemExportParms(currentDocument.ContractNumber, currentDocument.ScheduleNumber)
            lbDrugItemCountLabel.Visible = True
            lbDrugItemCount.Visible = True
            lbCoveredFCPCount.Visible = True
            lbPPVTotalCount.Visible = True
            'DrugItemExportTypeRadioButtonList.Visible = True

            lbDrugItemCount.Text = currentDocument.DrugItemCount.ToString()
            lbCoveredFCPCount.Text = String.Format("FCP / Covered = {0}/{1}", currentDocument.WithFCPDrugItemCount, currentDocument.CoveredDrugItemCount)
            lbPPVTotalCount.Text = String.Format("PPV / Total = {0}/{1}", currentDocument.PPVDrugItemCount, currentDocument.DrugItemCount)

            ' set up button click for drug item screen
            Session("LoadComplete") = False 'initializes loader flag used by DrugItems.aspx
            openPricelistWindowScript = "window.open('DrugItems.aspx?ContractNumber=" & contractNumber & "&VendorName=" & vendorName & "&Edit=" & isEditable & "&National=" & isNational & "','Pricelist','resizable=1,scrollbars=0,top=30,left=170,width=1020,height=800,modal=1')"
            btnViewDrugItemPricelist.Attributes.Add("onclick", openPricelistWindowScript)
        Else
            lbViewDrugItemPricelistLabel.Visible = False
            btnViewDrugItemPricelist.Visible = False
            lbExportDrugItemPriceList.Visible = False
            btnExportDrugItemPricelistToExcel.Visible = False
            lbDrugItemCountLabel.Visible = False
            lbDrugItemCount.Visible = False
            lbCoveredFCPCount.Visible = False
            lbPPVTotalCount.Visible = False
            'DrugItemExportTypeRadioButtonList.Visible = False
        End If

        If (currentDocument.IsService(currentDocument.ScheduleNumber) = True) Then
            lbExportPriceList.Enabled = False
            btnExportPricelistToExcel.Enabled = False
        End If

    End Sub

    Protected Sub UpdateItemCounts()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim lbMedSurgItemCount As Label
        Dim lbDrugItemCount As Label

        lbMedSurgItemCount = CType(fvContractInfo.FindControl("lbMedSurgItemCount"), Label)
        lbDrugItemCount = CType(fvContractInfo.FindControl("lbDrugItemCount"), Label)

        'hit the db for new counts
        currentDocument.UpdateItemCounts()

        lbMedSurgItemCount.Text = String.Format("{0}/{1}", currentDocument.ActiveMedSurgItemCount, currentDocument.FutureMedSurgItemCount)

        If (currentDocument.IsPharmaceutical(currentDocument.ScheduleNumber) = True) Then
            lbDrugItemCount.Text = currentDocument.DrugItemCount.ToString()
        End If
    End Sub
    Protected Sub ContractDataSource_OnUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles ContractDataSource.Updating
        Dim StimulusActCheckBox As CheckBox
        Dim StimulusActValue As Boolean

        StimulusActCheckBox = CType(fvContractInfo.FindControl("StimulusActCheckBox"), CheckBox)

        If Not StimulusActCheckBox Is Nothing Then
            StimulusActValue = StimulusActCheckBox.Checked
        End If

        e.Command.Parameters("@StimulusAct").Value = StimulusActValue

        Dim StandardizedCheckBox As CheckBox
        Dim StandardizedValue As Boolean = False

        StandardizedCheckBox = CType(fvContractInfo.FindControl("StandardizedCheckBox"), CheckBox)

        If Not StandardizedCheckBox Is Nothing Then
            StandardizedValue = StandardizedCheckBox.Checked
        End If

        e.Command.Parameters("@Standardized").Value = StandardizedValue

        'parent fss contract number
        Dim parentFSSContractNumber As String
        Dim ParentFSSContractDropDownList As DropDownList = CType(fvContractInfo.FindControl("ParentFSSContractDropDownList"), DropDownList)


        If Not ParentFSSContractDropDownList Is Nothing Then
            parentFSSContractNumber = CMGlobals.GetSelectedTextFromDropDownList(ParentFSSContractDropDownList)
        End If

        e.Command.Parameters("@BPA_FSS_Counterpart").Value = parentFSSContractNumber

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        e.Command.Parameters("@LastModifiedBy").Value = browserSecurity.UserInfo.LoginName
        e.Command.Parameters("@LastModificationDate").Value = DateTime.Now.ToString()

    End Sub
    Protected Sub ContractDataSource_OnUpdated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs) Handles ContractDataSource.Updated
        'reflect parent document updates in DrugItem database
        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        currentDocument.UpdateParentDocument()
    End Sub


    Private Sub NAC_CM_BPA_Edit_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        checkSalesEditable2()

        If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
            LoadSBAPlanContractResponsible(CType(Session("CurrentSelectedSBAPlanId"), Integer))
        End If

    End Sub

    ' check sales editable using current document settings
    Protected Sub checkSalesEditable2()
        Dim salesGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        Dim addSalesButton As Button = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.Sales) = True Then
            salesGrid.Columns(13).Visible = True
            addSalesButton.Visible = True
        Else
            salesGrid.Columns(13).Visible = False
            addSalesButton.Visible = False
        End If

    End Sub

    Protected Sub gvSales_RowDataBound(ByVal sender As Object, ByVal args As GridViewRowEventArgs)
        Dim salesGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
        Dim editButton As LinkButton
        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim contractNumber As String = Request.QueryString("CntrctNum").ToString
        Dim currentIndex As Integer
        Dim currentRowQuarterIdString As String

        If (args.Row.RowType = DataControlRowType.DataRow) Then
            editButton = CType(args.Row.FindControl("btnItemEdit"), LinkButton)
            If Not editButton Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.Sales) = True Then
                    salesGrid.Columns(13).Visible = True
                    editButton.Visible = True
                    currentIndex = args.Row.RowIndex
                    currentRowQuarterIdString = CType(salesGrid.DataKeys(currentIndex).Value, String)
                    editButton.Attributes.Add("onclick", "SalesUpdate('" & contractNumber & "','" & currentRowQuarterIdString & "')")
                Else
                    salesGrid.Columns(13).Visible = False
                    editButton.Visible = False
                End If
            End If
        End If
    End Sub

    Protected Sub gvSales_OnDataBound(ByVal sender As Object, ByVal args As EventArgs)
        Dim salesGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
        Dim addSalesButton As Button = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.Sales) = True Then
            addSalesButton.Visible = True
        Else
            addSalesButton.Visible = False
        End If


    End Sub

    Protected Sub gvChecks_OnRowUpdating(ByVal sender As Object, ByVal e As GridViewUpdateEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        e.NewValues("LastModifiedBy") = browserSecurity.UserInfo.LoginName
        e.NewValues("LastModificationDate") = DateTime.Now.ToString()

    End Sub

    Protected Sub ChecksDataSource_OnDeleting(ByVal sender As Object, ByVal e As SqlDataSourceCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
        Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

        LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
        LastModificationDateParm.Value = DateTime.Now

        e.Command.Parameters.Add(LastModifiedByParm)
        e.Command.Parameters.Add(LastModificationDateParm)

    End Sub

    'Protected Sub SetupSalesUpdate(ByVal s As Object, ByVal e As GridViewRowEventArgs)
    '    Dim contractNumber As String = Request.QueryString("CntrctNum").ToString
    '    Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
    '    Dim myRow As Integer = myGrid.DataKeys.Count - 1
    '    Dim myQTRID As String = ""
    '    Dim UpdateSalesButton As LinkButton = CType(e.Row.Cells(13).FindControl("btnItemEdit"), LinkButton)
    '    If Not UpdateSalesButton Is Nothing Then
    '        myQTRID = CType(myGrid.DataKeys(myRow).Value, String)
    '        UpdateSalesButton.Attributes.Add("onclick", "SalesUpdate('" & contractNumber & "','" & myQTRID & "')")
    '    End If
    'End Sub


    Protected Sub NACCMBPAEditScriptManager_OnAsyncPostBackError(ByVal sender As Object, ByVal e As AsyncPostBackErrorEventArgs)
        Dim errorMsg As String = ""

        If Not e.Exception.Data("NACCMBPAEditErrorMessage") Is Nothing Then
            errorMsg = String.Format("The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data("NACCMBPAEditErrorMessage"))
        Else
            errorMsg = String.Format("The following error was encountered during async postback: {0}", e.Exception.Message)
        End If

        ScriptManager1.AsyncPostBackErrorMessage = errorMsg
    End Sub

    Protected Sub SalesIFFCheckCompareButton_OnClick(ByVal sender As Object, ByVal args As EventArgs)
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim salesReport As Report = New Report("/Sales/Reports/IFFCheckComparisonByContract")
        salesReport.AddParameter("ContractNumber", currentDocument.ContractNumber)
        salesReport.AddReportUserLoginIdParameter()

        Session("ReportToShow") = salesReport

        Dim windowOpenScript As String
        windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=640,height=800');"
        ScriptManager.RegisterStartupScript(Me.Page, GetType(System.Web.UI.Page), "ReportViewerWindowOpenScript", windowOpenScript, True)
    End Sub

    Protected Sub DetailedSalesHistoryButton_OnClick(ByVal sender As Object, ByVal args As EventArgs)
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim salesReport As Report = New Report("/Sales/Reports/DetailedSalesByContractReport")
        salesReport.AddParameter("ContractNumber", currentDocument.ContractNumber)
        salesReport.AddReportUserLoginIdParameter()

        Session("ReportToShow") = salesReport

        Dim windowOpenScript As String
        windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1300,height=800');"
        ScriptManager.RegisterStartupScript(Me.Page, GetType(System.Web.UI.Page), "ReportViewerWindowOpenScript", windowOpenScript, True)

    End Sub

    Protected Sub QuarterlySalesHistoryButton_OnClick(ByVal sender As Object, ByVal args As EventArgs)
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim salesReport As Report = New Report("/Sales/Reports/QuarterlySalesByContractReport")
        salesReport.AddParameter("ContractNumber", currentDocument.ContractNumber)
        salesReport.AddReportUserLoginIdParameter()

        Session("ReportToShow") = salesReport

        Dim windowOpenScript As String
        windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1290,height=800');"
        ScriptManager.RegisterStartupScript(Me.Page, GetType(System.Web.UI.Page), "ReportViewerWindowOpenScript", windowOpenScript, True)


    End Sub

    Protected Sub AnnualSalesHistoryButton_OnClick(ByVal sender As Object, ByVal args As EventArgs)
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim salesReport As Report = New Report("/Sales/Reports/YearlySalesByContractReport")
        salesReport.AddParameter("ContractNumber", currentDocument.ContractNumber)
        salesReport.AddReportUserLoginIdParameter()

        Session("ReportToShow") = salesReport

        Dim windowOpenScript As String
        windowOpenScript = "window.open('ReportViewer.aspx','ReportViewer','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=1,top=20,left=24,width=1340,height=800');"
        ScriptManager.RegisterStartupScript(Me.Page, GetType(System.Web.UI.Page), "ReportViewerWindowOpenScript", windowOpenScript, True)


    End Sub
End Class

