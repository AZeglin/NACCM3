Imports System.Data
Imports System.Data.SqlClient
Imports System.Data.Common
'Imports Excel = Microsoft.Office.Interop.Excel

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj
Imports VA.NAC.ReportManager

Imports GridView = System.Web.UI.WebControls.GridView

Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList

Partial Public Class NAC_CM_Edit
    Inherits System.Web.UI.Page
    Protected myEditable As String
    Protected myScheduleNumber As Integer
    Protected myContractNumber As String
    Protected insertParameters(5) As SqlParameter
    Protected SinDataSourceUpdateParameters(3) As Parameter
    Protected insertStateParameters(2) As SqlParameter
    Protected insertCheckParameters(7) As SqlParameter

   

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Page.Focus()
        Dim view As MultiView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)

        Dim myScheduleNumber As Integer = 0
        Dim myContractNumber As String = ""
        If Not Request.QueryString("SchNum") Is Nothing Then
            myScheduleNumber = CInt(Request.QueryString("SchNum"))
        End If
        If Not Request.QueryString("CntrctNum") Is Nothing Then
            myContractNumber = Request.QueryString("CntrctNum")
        End If


        If Not IsPostBack Then
            Session("CurrentSelectedSBAPlanId") = Nothing

            Session("Requested") = "Contract"

            If Session("NACCM") Is Nothing Then
                Response.Redirect("Old1NCM.aspx")
            End If

            Dim myrefer_page As String = ""
            If Not Request.ServerVariables("HTTP_REFERER") Is Nothing Then
                myrefer_page = Request.ServerVariables("HTTP_REFERER").ToString
            End If

            'If Request.ServerVariables("HTTP_REFERER") Is Nothing Then
            '    If Session("NACCM") Is Nothing Then
            '        Response.Redirect("NCM.aspx")
            '    Else
            '        myrefer_page = "/contract_search.aspx"
            '    End If
            'Else
            'myrefer_page = Request.ServerVariables("HTTP_REFERER").ToString
            'End If
            'Dim myIndex As Integer = myrefer_page.LastIndexOf("/") + 1
            'myrefer_page = myrefer_page.Substring(myIndex)
            'If Not myrefer_page.Contains("contract_search.aspx") Then
            '    If Not myrefer_page.Contains("CreateContract") Then '("contract_addition") Then
            '        If Not myrefer_page.Contains("offer_search") Then
            '            If Not myrefer_page.Contains("NAC_Offers") Then
            '                If Not myrefer_page.Contains("sales_entry") Then
            '                    Response.Redirect("NCM.aspx")
            '                End If
            '            End If
            '        End If
            '    End If
            'End If
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


            If myScheduleNumber = 15 Or myScheduleNumber = 39 Or myScheduleNumber = 41 Or myScheduleNumber = 44 Then
                Response.Redirect("NAC_CM_BPA_Edit.aspx")
            End If

            setControls()
            '  setScheduleAttributes(myScheduleNumber) try moving this out of not postback
            dataBPA()
            ' BindSBAControls(True, IsPostBack)
            setBtnAttributes()
            ' checkCheckContent()
            'Else
            '    BindSBAControls(False, IsPostBack)



        End If


        setScheduleAttributes(myScheduleNumber)

        CMGlobals.AddKeepAlive(Me.Page, 30000)

        InitRebateControls(IsPostBack, myContractNumber)
        Session("PostbackSourceName") = "Other"

        BindSBAControls(True, IsPostBack)

        setUpSalesFun()
        setSBA294()
        SetSBAAddProjects()

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



        If IsPostBack Then
            Dim mySBARefresh As HiddenField = CType(fvContractInfo.FindControl("hfSBAProjectAdd"), HiddenField)
            If Not mySBARefresh Is Nothing Then
                If mySBARefresh.Value = "true" Then
                    mySBARefresh.Value = "false"
                    RefreshSBAProjects()

                End If
            End If

            Dim newSBAPlanId As Integer
            Dim hfNewSBAPlanId As HiddenField = CType(fvContractInfo.FindControl("hfNewSBAPlanId"), HiddenField)
            Dim hfRefreshSBAPlanList As HiddenField = CType(fvContractInfo.FindControl("hfRefreshSBAPlanList"), HiddenField)
            If Not hfRefreshSBAPlanList Is Nothing Then
                If hfRefreshSBAPlanList.Value = "true" Then
                    hfRefreshSBAPlanList.Value = "false"

                    If Not hfNewSBAPlanId Is Nothing Then
                        newSBAPlanId = hfNewSBAPlanId.Value
                    End If
                    RefreshSBAPlanList(newSBAPlanId)
                End If
            End If

        End If
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

    Private Sub NAC_CM_Edit_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Session("Requested") = Nothing
        Session("CurrentViewIndex") = Nothing
        Session("CntrctNum") = Nothing
        Session("Editable") = Nothing
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

    Protected Sub fvContractInfo_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles fvContractInfo.PreRender
        setBtnAttributes()
        MakePrimeVendorTableVisible()
        MakeInsurancePolicyVisible()
        MakeTradeAgreementTableVisible()
        MakeExportViewPricelistButtonsVisible()
        DisableSBAEditControls()
        checkContractAssignment()
        MakeAssistantDirectorVisible()
        EnableContractDateEditing(sender)
        EnableDisableRebateEditControls()
        MakeStandardizedTableVisible()
    End Sub
    Private Sub MakeTradeAgreementTableVisible()
        Dim TradeAgreementTable As HtmlTable
        Dim TAAYesCheckBox As CheckBox
        Dim TAAOtherCheckBox As CheckBox
        Dim TradeAgreementHeaderLabel As Label
        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
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
    Private Sub MakePrimeVendorTableVisible()
        Dim PrimeVendorTable As HtmlTable
        Dim PrimeVendorCheckBox As CheckBox
        Dim DODVAContractCheckBox As CheckBox
        Dim PrimeVendorLabel As Label
        Dim VADODLabel As Label
        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If currentDocument.ScheduleNumber <> 36 Then
            PrimeVendorTable = CType(fvContractInfo.Row.FindControl("PrimeVendorTable"), HtmlTable)
            PrimeVendorTable.Visible = True

            PrimeVendorCheckBox = CType(fvContractInfo.Row.FindControl("cbPrimeVendor"), CheckBox)
            DODVAContractCheckBox = CType(fvContractInfo.Row.FindControl("cbDODVACOntract"), CheckBox)
            PrimeVendorLabel = CType(fvContractInfo.Row.FindControl("lbPrimeVendor"), Label)
            VADODLabel = CType(fvContractInfo.Row.FindControl("lbVADOD"), Label)
            PrimeVendorCheckBox.Visible = True
            DODVAContractCheckBox.Visible = True
            PrimeVendorLabel.Visible = True
            VADODLabel.Visible = True

            If currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
                PrimeVendorCheckBox.Enabled = True
                DODVAContractCheckBox.Enabled = True
            End If
        End If
    End Sub

    Private Sub MakeInsurancePolicyVisible()
        Dim InsurancePolicyTable As HtmlTable
        Dim InsurancePolicyHeaderLabel As Label
        Dim InsurancePolicyEffectiveDateLabel As Label
        Dim InsurancePolicyExpirationDateLabel As Label
        Dim InsurancePolicyEffectiveDateTextBox As TextBox
        Dim InsurancePolicyExpirationDateTextBox As TextBox

        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.ScheduleNumber = 36 Or currentDocument.ScheduleNumber = 10 Then
            InsurancePolicyTable = CType(fvContractInfo.Row.FindControl("InsurancePolicyTable"), HtmlTable)
            InsurancePolicyTable.Visible = True

            InsurancePolicyHeaderLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyHeaderLabel"), Label)
            InsurancePolicyEffectiveDateLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyEffectiveDateLabel"), Label)
            InsurancePolicyExpirationDateLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyExpirationDateLabel"), Label)

            InsurancePolicyEffectiveDateTextBox = CType(fvContractInfo.Row.FindControl("InsurancePolicyEffectiveDateTextBox"), TextBox)
            InsurancePolicyExpirationDateTextBox = CType(fvContractInfo.Row.FindControl("InsurancePolicyExpirationDateTextBox"), TextBox)

            InsurancePolicyHeaderLabel.Visible = True
            InsurancePolicyEffectiveDateLabel.Visible = True
            InsurancePolicyExpirationDateLabel.Visible = True
            InsurancePolicyEffectiveDateTextBox.Visible = True
            InsurancePolicyExpirationDateTextBox.Visible = True


            If currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then
                InsurancePolicyEffectiveDateTextBox.Enabled = True
                InsurancePolicyExpirationDateTextBox.Enabled = True
            End If
        End If
    End Sub

    'Protected Sub InsurancePolicyEffectiveDateTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    'Text='<%#Bind("Insurance_Policy_Effective_Date", "{0:d}") %>'
    '    Dim insurancePolicyEffectiveDateTextBox As TextBox = CType(sender, TextBox)
    '    Dim fvContractInfo As FormView = CType(insurancePolicyEffectiveDateTextBox.NamingContainer, FormView)
    '    Dim currentRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

    '    insurancePolicyEffectiveDateTextBox.Text = currentRow("Insurance_Policy_Effective_Date").ToString()

    'End Sub
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

    Private Sub MakeAssistantDirectorVisible()
        Dim AssistantDirectorLabel As Label
        Dim SeniorContractSpecialistLabel As Label
        Dim lbADName As Label
        Dim lbSMName As Label

        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        If currentDocument.Division <> currentDocument.Divisions.FSS Then
            AssistantDirectorLabel = CType(fvContractInfo.Row.FindControl("AssistantDirectorLabel"), Label)
            SeniorContractSpecialistLabel = CType(fvContractInfo.Row.FindControl("SeniorContractSpecialistLabel"), Label)
            lbADName = CType(fvContractInfo.Row.FindControl("lbADName"), Label)
            lbSMName = CType(fvContractInfo.Row.FindControl("lbSMName"), Label)

            AssistantDirectorLabel.Visible = True
            SeniorContractSpecialistLabel.Visible = True
            lbADName.Visible = True
            lbSMName.Visible = True
        End If
    End Sub
    Private Sub btnExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExit.Click
        Session("CurrentViewIndex") = Nothing
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
        ElseIf myButton.ID.ToString = "btnRebate" Then
            myView.ActiveViewIndex = 4
            setButtonColors("btnRebate")
        ElseIf myButton.ID.ToString = "btnPrice" Then
            myView.ActiveViewIndex = 5
            setButtonColors("btnPrice")
        ElseIf myButton.ID.ToString = "btnSales" Then
            myView.ActiveViewIndex = 6
            setButtonColors("btnSales")
        ElseIf myButton.ID.ToString = "btnChecks" Then
            myView.ActiveViewIndex = 7
            setButtonColors("btnChecks")
        ElseIf myButton.ID.ToString = "btnSBA" Then
            myView.ActiveViewIndex = 8
            setButtonColors("btnSBA")
        ElseIf myButton.ID.ToString = "btnBOC" Then
            myView.ActiveViewIndex = 9
            setButtonColors("btnBOC")
        ElseIf myButton.ID.ToString = "btnBPAInfo" Then
            myView.ActiveViewIndex = 10
            setButtonColors("btnBPAInfo")
        ElseIf myButton.ID.ToString = "btnBPAPrice" Then
            myView.ActiveViewIndex = 11
            setButtonColors("btnBPAPrice")
        ElseIf myButton.ID.ToString = "btnFSSDetails" Then
            myView.ActiveViewIndex = 12
            setButtonColors("btnFSSDetails")
            populateFSSDetail()
            Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
            Dim myViewFSS As MultiView
            myViewFSS = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
            myViewFSS.ActiveViewIndex = 0
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
        ElseIf button = "btnRebate" Then
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
            myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
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
    '        Dim myCOID As String = myData.Item("CO_ID").ToString
    '        Dim myIdInList As ListItem = myDropDownList.Items.FindByValue(myCOID) 'myDropDownList.SelectedValue = myCOID
    '        If Not myIdInList Is Nothing Then
    '            myIdInList.Selected = True
    '        End If
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

    Protected Sub fvContractInfo_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)

        Dim dlState As DropDownList = CType(fvContractInfo.Row.FindControl("dlState"), DropDownList)
        Dim dlOrderState As DropDownList = CType(fvContractInfo.Row.FindControl("dlOrderState"), DropDownList)
        Dim currentDataRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
        Dim selectedStateAbbreviation As String
        Dim selectedOrderingStateAbbreviation As String
        Dim currentContractNumber As String

        If Not currentDataRow Is Nothing Then
            currentContractNumber = currentDataRow.Item("CntrctNum").ToString
        End If

        If Not IsPostBack Then

            'save selection to session
            If Not currentDataRow Is Nothing Then
                selectedStateAbbreviation = currentDataRow.Item("Primary_State").ToString
                Session("CurrentSelectedState") = selectedStateAbbreviation

                selectedOrderingStateAbbreviation = currentDataRow.Item("Ord_State").ToString
                Session("CurrentSelectedOrderingState") = selectedOrderingStateAbbreviation
            End If

            LoadAndBindStateDropDownList("dlOrderState")
            LoadAndBindStateDropDownList("dlState")
        Else
            LoadAndBindStateDropDownList("dlOrderState")
            LoadAndBindStateDropDownList("dlState")

        End If

        InitRebateControls(IsPostBack, currentContractNumber)
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

            ' bSuccess = contractDB.SelectStateCodes(dsStateCodes) $$$ took out to allow compilation
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
            ' MsgBox("The follow exception was thrown loading the SIN into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "SIN Failure")
        End Try
    End Sub
    'Protected Sub dataexpriationDate()
    '    Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlExpirationDate"), DropDownList)
    '    Dim strSQL As String = "SELECT DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() }) AND (Date <= { fn NOW() } + 4000) ORDER BY Date"
    '    Dim ds As New DataSet
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim cmd As SqlCommand
    '    Dim rdr As SqlDataReader
    '    Try
    '        conn.Open()
    '        cmd = New SqlCommand(strSQL, conn)
    '        rdr = cmd.ExecuteReader
    '        myDropDownList.DataTextField = "Date"
    '        myDropDownList.DataSource = rdr
    '        myDropDownList.DataBind()
    '        conn.Close()
    '        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '        Dim myExpiration As String = ""
    '        If Not myData.Item("Dates_CntrctExp").Equals(DBNull.Value) Then
    '            myExpiration = FormatDateTime(myData.Item("Dates_CntrctExp"), DateFormat.ShortDate).ToString
    '        End If
    '        myDropDownList.SelectedValue = myExpiration
    '    Catch ex As Exception
    '        ' MsgBox("The follow exception was thrown loading the Expiration Dates into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Expiration Date Failure")
    '    Finally
    '        conn.Close()
    '    End Try

    'End Sub
    'Protected Sub dataEndDate()
    '    Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlEndDate"), DropDownList)
    '    Dim strSQL As String = "SELECT  DISTINCT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 7) AND (Date <= { fn NOW() } + 120) ORDER BY Date"
    '    Dim ds As New DataSet
    '    Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
    '    Dim cmd As SqlCommand
    '    Dim rdr As SqlDataReader
    '    Try
    '        conn.Open()
    '        cmd = New SqlCommand(strSQL, conn)
    '        rdr = cmd.ExecuteReader
    '        myDropDownList.DataTextField = "Date"
    '        myDropDownList.DataValueField = "Date"
    '        myDropDownList.DataSource = rdr
    '        myDropDownList.DataBind()
    '        conn.Close()
    '        Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
    '        Dim myEnd As String = ""
    '        If Not myData.Item("Dates_Completion").Equals(DBNull.Value) Then
    '            myEnd = FormatDateTime(myData.Item("Dates_Completion"), DateFormat.ShortDate).ToString
    '        End If
    '        myDropDownList.SelectedValue = myEnd
    '    Catch ex As Exception
    '        ' MsgBox("The follow exception was thrown loading the End Dates into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "End Date Failure")
    '    Finally
    '        conn.Close()
    '    End Try


    'End Sub
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
            '  MsgBox("The follow exception was thrown loading the Business Size into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Business Size Failure")
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
            ' MsgBox("The follow exception was thrown loading the Vet Status into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Vet Status Failure")
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
            ' MsgBox("The follow exception was thrown loading the Vendor Type into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Vendor Type Failure")
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
            ' MsgBox("The follow exception was thrown loading the Geographic Coverage into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Goe Coverage Failure")
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
            ' MsgBox("The follow exception was thrown loading the returns good policy into the dropdownlist." & ex.ToString, MsgBoxStyle.OkOnly, "Returned Goods Failure")
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
& "dbo.[tbl_IFF].VA_IFF, dbo.[tbl_IFF].OGA_IFF, dbo.[tbl_IFF].SLG_IFF," _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF AS VA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF AS OGA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF AS SLG_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF " _
& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, " _
& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales " _
& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
& " join dbo.tbl_IFF on dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.ScheduleNumber LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
& "WHERE (View_Contract_Preview.CntrctNum = '" & Request.QueryString("CntrctNum").ToString & "')" _
& " AND View_Sales_Variance_by_Year_A.Quarter_ID is not null " _
& " AND View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tbl_IFF.start_quarter_id " _
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
            ' MsgBox("The follow exception was thrown loading the sales into a grid view." & ex.ToString, MsgBoxStyle.OkOnly, "Sales Failure")
        Finally
            conn.Close()
        End Try

    End Sub

    'Protected Sub OpenReport(ByVal s As Object, ByVal e As EventArgs)
    '    Dim myContract As String = Request.QueryString("CntrctNum").ToString
    '    Dim strWindow As String = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Qtr','','resizable=0,scrollbars=1,width=820,height=500,left=0,top=0')"
    '    Dim myButton As Button = CType(fvContractInfo.Row.FindControl("QuarterlySalesHistoryButton"), Button)
    '    myButton.Attributes.Add("onclick", strWindow)

    'End Sub
    Protected Sub setBtnAttributes()
        myEditable = "Y"
        Dim strWindowFeatures As String = "window.open('pending_price_change.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "','Pending_Change','resizable=0,scrollbars=1,width=700,height=500,left=0,top=0')"
        Dim myContract As String = Request.QueryString("CntrctNum").ToString
        Dim myButton As Button
        Dim strWindow As String
        ' Dim myButton As Button = CType(fvContractInfo.Row.FindControl("SalesIFFCheckCompareButton"), Button)
        ' Dim strWindow As String = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=0,top=0')"
        ' myButton.Attributes.Add("onclick", strWindow)
        ' strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Full','','resizable=0,scrollbars=1,width=820,height=500,left=0,top=0')"
        '   myButton = CType(fvContractInfo.Row.FindControl("DetailedSalesHistoryButton"), Button)
        '   myButton.Attributes.Add("onclick", strWindow)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Qtr','','resizable=0,scrollbars=1,width=820,height=500,left=0,top=0')"
        '  myButton = CType(fvContractInfo.Row.FindControl("QuarterlySalesHistoryButton"), Button)
        '  myButton.Attributes.Add("onclick", strWindow)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Year','','resizable=0,scrollbars=1,width=820,height=500,left=0,top=0')"
        '  myButton = CType(fvContractInfo.Row.FindControl("AnnualSalesHistoryButton"), Button)
        '  myButton.Attributes.Add("onclick", strWindow)
        myButton = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)
        myButton.Text = "ADD SALES" & vbNewLine & "FIGURES"
        myButton.ForeColor = Drawing.Color.Green
        strWindow = "window.open('sales_entry.aspx?CntrctNum=" & myContract & "&Page=1','Details','toolbar=no,menubar=no,resizable=1,width=865,height=400')"
        myButton.Attributes.Add("onclick", strWindow)
        ' myButton = CType(fvContractInfo.Row.FindControl("btnViewIFFCheck"), Button)
        ' strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=0,top=0')"
        ' myButton.Attributes.Add("onclick", strWindow)
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
        'Dim mybtnViewPricelist As Button = CType(fvContractInfo.Row.FindControl("btnViewPricelist"), Button)
        'If Not mybtnViewPricelist Is Nothing Then
        '    strWindowFeatures = "window.open('FSS_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&BPA=Y','Pricelist','resizable=0,scrollbars=1,width=730,height=500,left=280,top=140')"
        '    Dim mybtnBPAViewPricelist As Button = CType(fvContractInfo.Row.FindControl("btnBPAViewPricelist"), Button)
        '    mybtnBPAViewPricelist.Attributes.Add("onclick", strWindowFeatures)
        '    If myScheduleNumber = 36 Then
        '        strWindow = "window.open('FSS_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&BPA=N','Pricelist','resizable=0,scrollbars=1,width=730,height=500,left=280,top=140')"
        '        mybtnViewPricelist.Attributes.Add("onclick", strWindow)
        '    Else
        '        strWindow = "window.open('FSS_Pricelist.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "&BPA=N','Pricelist','resizable=0,scrollbars=1,width=730,height=500,left=280,top=140')"
        '        mybtnViewPricelist.Attributes.Add("onclick", strWindow)
        '    End If
        'End If

        Dim btnExportPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportPricelistToExcel"), ImageButton)
        If Not btnExportPricelistToExcel Is Nothing Then

            'old way $$$
            'Dim exportMedSurgToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & myContractNumber & "&ExportType=M" & "','Pricelist','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,width=300,height=200,left=170,top=110,status=yes');"

            Dim exportMedSurgToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemExportUpload.aspx?ContractNumber=" & currentDocument.ContractNumber & "&ScheduleNumber=" & currentDocument.ScheduleNumber & "&ExportUploadType=M" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"
            btnExportPricelistToExcel.Attributes.Add("onclick", exportMedSurgToExcelWindowScript)
        End If


    End Sub

    'Protected Sub SetDrugItemExportParms(ByVal ContractNumber As String, ByVal ScheduleNumber As Integer, ByVal ExportType As String)
    '    Dim btnExportDrugItemPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)
    '    If Not btnExportDrugItemPricelistToExcel Is Nothing Then
    '        '   Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','')"
    '        'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=no,menubar=no,resizable=yes,scrollbars=yes,width=300,height=200,left=170,top=110,status=yes');"
    '        'old way $$$$
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
            'old way $$$$
            'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=280,left=320,width=340,height=160, resizable=0');"
            Dim exportDrugItemsToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemExportUpload.aspx?ContractNumber=" & ContractNumber & "&ScheduleNumber=" & ScheduleNumber.ToString() & "&ExportUploadType=P" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"
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
            ' Else
            '     myVariance = "0%"
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


    Protected Sub DisableSBAEditControls()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim dlSBAPlanName As DropDownList
        dlSBAPlanName = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not dlSBAPlanName Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                dlSBAPlanName.Enabled = True
            Else
                dlSBAPlanName.Enabled = False
            End If

        End If

        Dim btnAddSBA As Button
        btnAddSBA = CType(fvContractInfo.FindControl("btnAddSBA"), Button)
        If Not btnAddSBA Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                btnAddSBA.Enabled = True
            Else
                btnAddSBA.Enabled = False
            End If

        End If

        Dim gvSBAProjections As GridView
        gvSBAProjections = CType(fvContractInfo.FindControl("gvSBAProjections"), GridView)
        Dim projectionsFooterRow As GridViewRow
        Dim projectionsEmptyDataTemplateRow As GridViewRow
        Dim projectionsEmptyDataTable As Table

        If Not gvSBAProjections Is Nothing Then


            'no rows, need to apply security to empty data template
            If gvSBAProjections.Rows.Count = 0 Then
                projectionsEmptyDataTable = CType(gvSBAProjections.Controls(0), Table)
                projectionsEmptyDataTemplateRow = CType(projectionsEmptyDataTable.Controls(0), GridViewRow)


                Dim btnAddSBAProjEmpty As Button
                btnAddSBAProjEmpty = CType(projectionsEmptyDataTemplateRow.FindControl("btnAddSBAProjEmpty"), Button)
                If Not btnAddSBAProjEmpty Is Nothing Then
                    If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                        btnAddSBAProjEmpty.Enabled = True
                    Else
                        btnAddSBAProjEmpty.Enabled = False
                    End If

                End If

            Else ' look at footer row

                projectionsFooterRow = gvSBAProjections.FooterRow

                Dim btnAddSBAProjNew As Button
                btnAddSBAProjNew = CType(projectionsFooterRow.FindControl("btnAddSBAProjNew"), Button)
                If Not btnAddSBAProjNew Is Nothing Then
                    If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                        btnAddSBAProjNew.Enabled = True
                    Else
                        btnAddSBAProjNew.Enabled = False
                    End If

                End If
            End If

        End If

        Dim gv294Reports As GridView
        gv294Reports = CType(fvContractInfo.FindControl("gv294Reports"), GridView)
        Dim accomplishmentsFooterRow As GridViewRow
        Dim accomplishmentsEmptyDataTemplateRow As GridViewRow
        Dim accomplishmentsEmptyDataTable As Table

        If Not gv294Reports Is Nothing Then

            'no rows, need to apply security to empty data template
            If gv294Reports.Rows.Count = 0 Then
                accomplishmentsEmptyDataTable = CType(gv294Reports.Controls(0), Table)
                accomplishmentsEmptyDataTemplateRow = CType(accomplishmentsEmptyDataTable.Controls(0), GridViewRow)

                Dim btnAddSBA294Empty As Button
                btnAddSBA294Empty = CType(accomplishmentsEmptyDataTemplateRow.FindControl("btnAddSBA294Empty"), Button)
                If Not btnAddSBA294Empty Is Nothing Then
                    If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                        btnAddSBA294Empty.Enabled = True
                    Else
                        btnAddSBA294Empty.Enabled = False
                    End If

                End If

            Else ' look at footer row

                accomplishmentsFooterRow = gv294Reports.FooterRow

                Dim btnAddSBA294jNew As Button
                btnAddSBA294jNew = CType(accomplishmentsFooterRow.FindControl("btnAddSBA294jNew"), Button)
                If Not btnAddSBA294jNew Is Nothing Then
                    If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                        btnAddSBA294jNew.Enabled = True
                    Else
                        btnAddSBA294jNew.Enabled = False
                    End If

                End If


            End If

        End If

        Dim SBAPlanExemptCheckBox As CheckBox
        SBAPlanExemptCheckBox = CType(fvContractInfo.FindControl("SBAPlanExemptCheckBox"), CheckBox)
        If Not SBAPlanExemptCheckBox Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                SBAPlanExemptCheckBox.Enabled = True
            Else
                SBAPlanExemptCheckBox.Enabled = False
            End If

        End If

        Dim fvSBAplanType As FormView
        fvSBAplanType = CType(fvContractInfo.FindControl("fvSBAplanType"), FormView)

        If Not fvSBAplanType Is Nothing Then

            Dim dlPlanType As DropDownList
            dlPlanType = CType(fvSBAplanType.FindControl("dlPlanType"), DropDownList)
            If Not dlPlanType Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    dlPlanType.Enabled = True
                Else
                    dlPlanType.Enabled = False
                End If

            End If

            Dim tbPlanAdminName As TextBox
            tbPlanAdminName = CType(fvSBAplanType.FindControl("tbPlanAdminName"), TextBox)
            If Not tbPlanAdminName Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminName.Enabled = True
                Else
                    tbPlanAdminName.Enabled = False
                End If

            End If

            Dim tbPlanAdminEmail As TextBox
            tbPlanAdminEmail = CType(fvSBAplanType.FindControl("tbPlanAdminEmail"), TextBox)
            If Not tbPlanAdminEmail Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminEmail.Enabled = True
                Else
                    tbPlanAdminEmail.Enabled = False
                End If

            End If

            Dim tbPlanAdminAddress As TextBox
            tbPlanAdminAddress = CType(fvSBAplanType.FindControl("tbPlanAdminAddress"), TextBox)
            If Not tbPlanAdminAddress Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminAddress.Enabled = True
                Else
                    tbPlanAdminAddress.Enabled = False
                End If

            End If

            Dim tbPlanAdminCity As TextBox
            tbPlanAdminCity = CType(fvSBAplanType.FindControl("tbPlanAdminCity"), TextBox)
            If Not tbPlanAdminCity Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminCity.Enabled = True
                Else
                    tbPlanAdminCity.Enabled = False
                End If

            End If


            Dim dlPlanAdminstate As DropDownList
            dlPlanAdminstate = CType(fvSBAplanType.FindControl("dlPlanAdminstate"), DropDownList)
            If Not dlPlanAdminstate Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    dlPlanAdminstate.Enabled = True
                Else
                    dlPlanAdminstate.Enabled = False
                End If

            End If

            Dim tbPlanAdminZip As TextBox
            tbPlanAdminZip = CType(fvSBAplanType.FindControl("tbPlanAdminZip"), TextBox)
            If Not tbPlanAdminZip Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminZip.Enabled = True
                Else
                    tbPlanAdminZip.Enabled = False
                End If

            End If

            Dim tbPlanAdminPhone As TextBox
            tbPlanAdminPhone = CType(fvSBAplanType.FindControl("tbPlanAdminPhone"), TextBox)
            If Not tbPlanAdminPhone Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminPhone.Enabled = True
                Else
                    tbPlanAdminPhone.Enabled = False
                End If

            End If

            Dim tbPlanAdminFax As TextBox
            tbPlanAdminFax = CType(fvSBAplanType.FindControl("tbPlanAdminFax"), TextBox)
            If Not tbPlanAdminFax Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    tbPlanAdminFax.Enabled = True
                Else
                    tbPlanAdminFax.Enabled = False
                End If

            End If

            Dim btnEditSBAAdmin As Button
            btnEditSBAAdmin = CType(fvSBAplanType.FindControl("btnEditSBAAdmin"), Button)
            If Not btnEditSBAAdmin Is Nothing Then
                If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                    btnEditSBAAdmin.Enabled = True
                Else
                    btnEditSBAAdmin.Enabled = False
                End If

            End If
        End If

    End Sub


    Protected Sub setFormatAddNewSBAPlanButton(ByVal s As Object, ByVal e As EventArgs)
        Dim addPlanWindowOpenCommand As String = ""
        addPlanWindowOpenCommand = "window.open('SBA_Add_Plan.aspx','SBA_Add_List','resizable=0,scrollbars=1,width=350,height=200,left=400,top=400')"
        Dim myButton As Button = CType(s, Button)
        myButton.Text = "Add New SBA" & vbCrLf & "Vendor/Plan Name"
        myButton.Attributes.Add("onclick", addPlanWindowOpenCommand)
    End Sub

    Protected Sub dlSBAPlanName_OnInit(ByVal s As Object, ByVal e As EventArgs)

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
    Protected Sub setFormatSBA294(ByVal s As Object, ByVal e As EventArgs)
        Dim myButton As Button = CType(s, Button)
        myButton.Text = "Add" & vbCrLf & "294/295"
    End Sub
    Protected Sub dlSBAPlanName_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlSBAPlanName As DropDownList = CType(sender, DropDownList)
        Dim currentSelectedSBAId As String = ""
        '  Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)

        ' a new item was created
        If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
            currentSelectedSBAId = Session("CurrentSelectedSBAPlanId")
        End If

        If currentSelectedSBAId.Length = 0 Then
            Return
        Else
            If dlSBAPlanName.Items.Count > 1 Then
                dlSBAPlanName.SelectedValue = currentSelectedSBAId
            Else
                ReloadSBAPlanList()
            End If
        End If

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


        ' compare award, effective, expiration and completion dates  

        ' exception for award, effective comparison for these four contracts which were active before validation 
        ' was put in place: VA797-BO-0310 VA797-BO-0361 V797P-4108b V797P-3153m
        If Not Session("CurrentDocument") Is Nothing Then
            Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
            If Not (currentDocument.ContractNumber = "VA797-BO-0310" Or currentDocument.ContractNumber = "VA797-BO-0361" Or currentDocument.ContractNumber = "V797P-4108b" Or currentDocument.ContractNumber = "V797P-3153m") Then
                If DateTime.Compare(awardDate, effectiveDate) > 0 Then
                    MsgBox.Alert("Award date must be before effective date.")
                    Return
                End If
            End If
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

        Session("PostbackSourceName") = "BigUpdate"

        fvContractInfo.UpdateItem(True)

    End Sub

    Protected Sub ReloadSBAPlanList()
        Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.Row.FindControl("dlSBAPlanName"), DropDownList)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Dim da As SqlDataAdapter
        Dim ds As DataSet
        Dim testRow As System.Data.DataRow

        Dim strSQL As String = ""
        Dim mySBAID As String = ""

        ' set up the plan list, regardless of whether there is a selected plan yet
        strSQL = "SELECT [SBAPlanID], [PlanName] FROM view_sba_plans_sorted"

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
                                If Not testRow("SBAPlanID") Is DBNull.Value Then
                                    If Not dlSBAPlanName Is Nothing Then
                                        'dlSBAPlanName.ClearSelection()
                                        'dlSBAPlanName.Items.Clear()
                                        dlSBAPlanName.DataSource = ds.Tables(0)
                                        dlSBAPlanName.DataTextField = ds.Tables(0).Columns("PlanName").ColumnName.ToString()
                                        dlSBAPlanName.DataValueField = ds.Tables(0).Columns("SBAPlanID").ColumnName.ToString()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("Error refreshing sba plan list " & ex.ToString)
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub BindSBAControls(ByVal bLoadSBAPlanNameList As Boolean, ByVal bIsPostback As Boolean)
        Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.Row.FindControl("dlSBAPlanName"), DropDownList)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim rdr As SqlDataReader
        Dim da As SqlDataAdapter
        Dim ds As DataSet
        Dim testRow As System.Data.DataRow

        Dim strSQL As String = ""
        Dim planTypesFormView As FormView
        Dim mySBAID As String = ""


        If bLoadSBAPlanNameList = True Then
            ReloadSBAPlanList()
        End If

        If bIsPostback = False Then

            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            If Not myData Is Nothing Then
                mySBAID = myData.Item("SBAPlanID").ToString
                'no id, so nothing will bind anyway
                If mySBAID.Length = 0 Then
                    dlSBAPlanName.DataBind()
                    Return
                Else
                    Session("CurrentSelectedSBAPlanId") = mySBAID
                End If
            End If

            If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
                dlSBAPlanName.SelectedValue = CType(Session("CurrentSelectedSBAPlanId"), Integer)
            End If

            dlSBAPlanName.DataBind()
        Else 'IsPostback
            If Me.Request.Form("__EVENTTARGET") = dlSBAPlanName.UniqueID Then
                'get new value
                Session("CurrentSelectedSBAPlanId") = dlSBAPlanName.SelectedValue
                mySBAID = CType(Session("CurrentSelectedSBAPlanId"), String)
            Else 'restore
                If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
                    dlSBAPlanName.SelectedValue = CType(Session("CurrentSelectedSBAPlanId"), Integer)
                    mySBAID = CType(Session("CurrentSelectedSBAPlanId"), String)
                End If
            End If
        End If


        If mySBAID <> "" Then

            Dim projectionGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSBAProjections"), Global.System.Web.UI.WebControls.GridView)
            '        strSQL = "SELECT TOP 100 PERCENT COALESCE (dbo.tbl_sba_Projection.SBAPlanID, dbo.view_SBA_contract_master.SBAPlanID) AS SBAPlanID, " _
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

            strSQL = "select SBAPlanID, StartDate, EndDate, ProjectionID from View_SBA_Projection_Sorted where SBAPlanID = " & mySBAID & " "

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

                '   rdr = cmd.ExecuteReader
                '   rdr2 = cmd.ExecuteReader
                'If rdr.HasRows Then
                '    If Not projectionGridView Is Nothing Then
                '        'adopt to poorly formed view
                '        If rdr.Read() Then
                '            If rdr.IsDBNull(1) = False Then
                '                projectionGridView.DataSource = rdr2
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
                Msg_Alert.Client_Alert_OnLoad("SBAPlan info " & ex.ToString)
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
                'conn.Close()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("SBA Accomp info " & ex.ToString)
            Finally
                conn.Close()
            End Try
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

            ' strSQL = "select CntrctNum, CO_Name from view_sba_contract_master where SBAPlanID = " & mySBAID & " "


            'Try
            '    conn.Open()
            '    cmd = New SqlCommand(strSQL, conn)
            '    rdr = cmd.ExecuteReader
            '    If rdr.HasRows Then
            '        If Not contractResponsibleFormView Is Nothing Then
            '            contractResponsibleFormView.DataSource = rdr
            '            contractResponsibleFormView.DataBind()
            '        End If
            '    Else
            '        If Not contractResponsibleFormView Is Nothing Then
            '            contractResponsibleFormView.Visible = "False"
            '        End If
            '    End If
            '    conn.Close()
            'Catch ex As Exception
            '    Msg_Alert.Client_Alert_OnLoad("SBA Contract Rep info " & ex.ToString)
            'Finally
            '    conn.Close()
            'End Try
        End If
        If Not mySBAID = Nothing Then
            If mySBAID.Length > 0 Then
                If bIsPostback = False Then

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
                        Msg_Alert.Client_Alert_OnLoad("This is error at SBA. " & ex.ToString)
                    Finally
                        conn.Close()
                    End Try
                End If
            End If
        End If

        If Not mySBAID = Nothing Then
            If mySBAID.Length > 0 Then
                Dim sbaCompanyDatalist As DataList = CType(fvContractInfo.Row.FindControl("dlSBACompanyList"), DataList)
                'strSQL = "SELECT * FROM tbl_sba_SBAPlan A Join tbl_Cntrcts B on A.SBAPlanID = B.SBAPlanID Join [tlkup_Sched/Cat] C on B.Schedule_Number = C.Schedule_Number Join  [NACSEC].[dbo].SEC_UserProfile D on B.CO_ID = D.CO_ID WHERE A.SBAPlanID= " & mySBAID

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
                    Msg_Alert.Client_Alert_OnLoad("This is error at SBA. " & ex.ToString)
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
    Protected Sub dlSBAPlanName_OnSelectedIndexChanged(ByVal s As Object, ByVal e As EventArgs)
        'BindSBAControls(False)
        'Dim dlSBAPlanName As DropDownList = CType(s, DropDownList)
        'Session("CurrentSelectedSBAPlanId") = dlSBAPlanName.SelectedItem.Value
        'dlSBAPlanName.DataBind()
    End Sub

    Protected Sub populateFSSDetail()
        Dim myFSSContractNum As String = ""
        Dim myFSSCOntract As HiddenField = CType(fvContractInfo.Row.FindControl("hfBPAFSSContract"), HiddenField)
        If Not myFSSCOntract Is Nothing Then
            myFSSContractNum = myFSSCOntract.Value.ToString
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
                ' MsgBox("This is error at BPA FSS Detail. " & ex.ToString, MsgBoxStyle.OkOnly)
            Finally
                conn.Close()
            End Try
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(myFormView.Row.FindControl("gvBPAFSSSales"), Global.System.Web.UI.WebControls.GridView)
            If Not myGridView Is Nothing Then
                mySQLStr = "SELECT     dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum, " _
& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
& "dbo.View_Contract_Preview.Contractor_Name, " _
& "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
& "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
& "dbo.[tbl_IFF].VA_IFF, dbo.[tbl_IFF].OGA_IFF, dbo.[tbl_IFF].SLG_IFF," _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF AS VA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF AS OGA_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF AS SLG_IFF_Amount, " _
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF " _
& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, " _
& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales " _
& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
& " join dbo.tbl_IFF on dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.ScheduleNumber LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
& "WHERE (View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "')" _
& " AND View_Sales_Variance_by_Year_A.Quarter_ID is not null " _
& " AND View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tbl_IFF.start_quarter_id " _
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
                    ' MsgBox("This is error at BPA FSS Sales Detail. " & ex.ToString, MsgBoxStyle.OkOnly)
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
                    ' MsgBox("This is error at BPA FSS Checks Detail. " & ex.ToString, MsgBoxStyle.OkOnly)
                Finally
                    conn.Close()
                End Try
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

    Protected Sub dataBPA()

        Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvFSSContractInfo"), FormView)
        If Not myFormView Is Nothing Then
            Dim myFSSContractNum As String = ""
            Dim myFSSCOntract As HiddenField = CType(fvContractInfo.Row.FindControl("hfBPAFSSContract"), HiddenField)
            If Not myFSSCOntract Is Nothing Then
                myFSSContractNum = myFSSCOntract.Value.ToString
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
                    ' MsgBox("This is error at FSS Detail. " & ex.ToString, MsgBoxStyle.OkOnly)
                Finally
                    conn.Close()
                End Try
            End If
            myFormView.Visible = "True"
        End If
    End Sub
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
                myButton = CType(fvContractInfo.Row.FindControl("btnRebate"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
                myButton.Visible = "False"
                Dim myImageButton As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportPricelistToExcel"), ImageButton)
                If Not myImageButton Is Nothing Then
                    myImageButton.Visible = False
                End If
                Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbExportPriceList"), Label)
                If Not myLabel Is Nothing Then
                    myLabel.Visible = False
                End If
            End If
        End If
        'If myScheduleNumber = 10 Or myScheduleNumber = 36 Then
        '    Dim InsurancePolicyTable As HtmlTable
        '    InsurancePolicyTable = CType(fvContractInfo.Row.FindControl("InsurancePolicyTable"), HtmlTable)
        '    InsurancePolicyTable.Visible = True

        '    Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("InsurancePolicyHeaderLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "True"
        '    End If
        '    myLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyEffectiveDateLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "True"
        '    End If
        '    myLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyExpirationDateLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "True"
        '    End If
        '    Dim mytextBox As TextBox = CType(fvContractInfo.Row.FindControl("tbInsDateEff"), TextBox)
        '    If Not mytextBox Is Nothing Then
        '        mytextBox.Visible = "True"
        '    End If
        '    mytextBox = CType(fvContractInfo.Row.FindControl("tbInsurePolicyExpDate"), TextBox)
        '    If Not mytextBox Is Nothing Then
        '        mytextBox.Visible = "True"
        '    End If
        'Else
        '    Dim InsurancePolicyTable As HtmlTable
        '    InsurancePolicyTable = CType(fvContractInfo.Row.FindControl("InsurancePolicyTable"), HtmlTable)
        '    InsurancePolicyTable.Visible = False

        '    Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("InsurancePolicyHeaderLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "False"
        '    End If
        '    myLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyEffectiveDateLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "False"
        '    End If
        '    myLabel = CType(fvContractInfo.Row.FindControl("InsurancePolicyExpirationDateLabel"), Label)
        '    If Not myLabel Is Nothing Then
        '        myLabel.Visible = "False"
        '    End If

        '    Dim mytextBox As TextBox = CType(fvContractInfo.Row.FindControl("tbInsDateEff"), TextBox)
        '    If Not mytextBox Is Nothing Then
        '        mytextBox.Visible = "False"
        '    End If
        '    mytextBox = CType(fvContractInfo.Row.FindControl("tbInsurePolicyExpDate"), TextBox)
        '    If Not mytextBox Is Nothing Then
        '        mytextBox.Visible = "False"
        '    End If
        'End If
        '        If myScheduleNumber = 36 Then
        'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbPolicyInfo"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "True"
        'End If
        'Dim mytextBox As TextBox = CType(fvContractInfo.Row.FindControl("tbInsDateEff"), TextBox)
        'If Not mytextBox Is Nothing Then
        '    mytextBox.Visible = "True"
        'End If
        'mytextBox = CType(fvContractInfo.Row.FindControl("tbInsurePolicyExpDate"), TextBox)
        'If Not mytextBox Is Nothing Then
        '    mytextBox.Visible = "True"
        'End If
        'Dim PrimeVendorTable As HtmlTable
        'PrimeVendorTable = CType(fvContractInfo.Row.FindControl("PrimeVendorTable"), HtmlTable)
        'PrimeVendorTable.Visible = False

        'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbPrimeVendor"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "False"
        'End If
        'myLabel = CType(fvContractInfo.Row.FindControl("lbVADOD"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "False"
        'End If
        'Dim myCheckBox As CheckBox = CType(fvContractInfo.Row.FindControl("cbPrimeVendor"), CheckBox)
        'If Not myCheckBox Is Nothing Then
        '    myCheckBox.Visible = "False"
        '    myCheckBox.Enabled = "False"
        'End If
        'myCheckBox = CType(fvContractInfo.Row.FindControl("cbDODVACOntract"), CheckBox)
        'If Not myCheckBox Is Nothing Then
        '    myCheckBox.Visible = "False"
        '    myCheckBox.Enabled = "False"
        'End If
        'Dim myButton As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportPricelistToExcel"), ImageButton)
        'If Not myButton Is Nothing Then
        '    myButton.Visible = "False"
        'End If
        'myLabel = CType(fvContractInfo.Row.FindControl("lbExportPriceList"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "False"
        'End If
        'Else
        'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbPrimeVendor"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "True"
        'End If
        'myLabel = CType(fvContractInfo.Row.FindControl("lbVADOD"), Label)
        'If Not myLabel Is Nothing Then
        '    myLabel.Visible = "True"
        'End If
        'Dim myCheckBox As CheckBox = CType(fvContractInfo.Row.FindControl("cbPrimeVendor"), CheckBox)
        'If Not myCheckBox Is Nothing Then
        '    myCheckBox.Visible = "True"
        '    myCheckBox.Enabled = "True"
        'End If
        'myCheckBox = CType(fvContractInfo.Row.FindControl("cbDODVACOntract"), CheckBox)
        'If Not myCheckBox Is Nothing Then
        '    myCheckBox.Visible = "True"
        '    myCheckBox.Enabled = "True"
        'End If

        'End If
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
        Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        If e.CommandName = "InsertAdd" Then
            Dim myDropList As DropDownList = CType(myGridView.FooterRow.FindControl("dlQuarter"), DropDownList)
            Dim myTextBox As TextBox = CType(myGridView.FooterRow.FindControl("tbVASales"), TextBox)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString
            Dim myInt As Integer = CType(myDropList.SelectedValue, Integer)
            Dim myQuarter As Integer = myInt
            Dim myContractNumber As String = CntrctNumber
            Dim myVASales As Double = CType(myTextBox.Text, Double)
            myTextBox = CType(myGridView.FooterRow.FindControl("tbInsertOGASales"), TextBox)
            Dim myOGASales As Double = CType(myTextBox.Text, Double)
            Dim slgTextBox As TextBox = CType(myGridView.FooterRow.FindControl("tbInsertSLGSales"), TextBox)
            Dim slgSales As Double = CType(slgTextBox.Text, Double)
            '  Dim myLabel As Label = CType(myGridView.FooterRow.FindControl("lbInsertTotalSales"), Label)
            '  Dim myTotalSales As Double = CType(myLabel.Text, Double)
            myDropList = CType(myGridView.FooterRow.FindControl("dlNewSIN"), DropDownList)
            Dim mySINS As String = CType(myDropList.SelectedValue, String)
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)

            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Try
                Dim strSQL As String = "INSERT INTO dbo.tbl_Cntrcts_Sales (CntrctNum, Quarter_ID, SIN, VA_Sales, OGA_Sales, SLG_Sales, LastModifiedBy, LastModificationDate ) VALUES (@CntrctNum, @Quarter_ID, @SIN, @VA_Sales, @OGA_Sales, @SLG_Sales, @LastModifiedBy, @LastModificationDate )"
                Dim insertCommand As SqlCommand = New SqlCommand()
                insertCommand.Connection = conn
                insertCommand.CommandText = strSQL
                insertCommand.Parameters.Add("@Quarter_ID", SqlDbType.Int).Value = myQuarter
                insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = myContractNumber
                insertCommand.Parameters.Add("@SIN", SqlDbType.VarChar, 10).Value = mySINS
                insertCommand.Parameters.Add("@VA_Sales", SqlDbType.Money).Value = myVASales
                insertCommand.Parameters.Add("@OGA_Sales", SqlDbType.Money).Value = myOGASales
                insertCommand.Parameters.Add("@SLG_Sales", SqlDbType.Money).Value = slgSales
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
                insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()

                conn.Open()
                insertCommand.ExecuteNonQuery()
                myGridView.DataBind()
            Catch ex As Exception
                ' MsgBox("This is a result of the Sales insert. " & ex.ToString, MsgBoxStyle.OkOnly)
            Finally
                conn.Close()
            End Try
        ElseIf e.CommandName = "Cancel" Then
        Else
            ' Dim mySender As String = ""
            ' Dim myIndex As Integer = CType(e.CommandArgument, Integer)
            ' Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
            ' Dim mydataRow As GridViewRow = myGrid.Rows(myIndex)
            ' Dim mydataKey As DataKey = myGrid.DataKeys(myIndex)
            ' Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
            ' If e.CommandName = "Detail" Then
            'Dim myScript As String = "<script>window.open('sales_detail.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Details','toolbar=no,menubar=no,resizable=no,width=865,height=300')</script>"
            'Response.Write(myScript)
            'End If
            If e.CommandName = "Update_Data" Then
                'Dim myScript As String = "<script>window.open('sales_detail_edit.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Edit','toolbar=no,menubar=no,resizable=no,width=300,height=300,scrollbars=yes')</script>"
                'Response.Write(myScript)
                ' myGridView.EditIndex = -1
            End If
            If e.CommandName = "Delete" Then

            End If
        End If
    End Sub
    Protected Sub refreshSalesDate(ByVal s As Object, ByVal e As EventArgs)
        Dim myValue As String = hfRefreshSales.Value.ToString
    End Sub
    ' removed function Protected Sub checkSalesEditable() on 12/29/2009 - not used
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
    Protected Sub checkContractAssignment()

        Dim dlContractOfficers As Global.System.Web.UI.WebControls.DropDownList = CType(fvContractInfo.Row.FindControl("dlContractOfficers"), Global.System.Web.UI.WebControls.DropDownList)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.ContractAssignment) = True Then
            dlContractOfficers.Enabled = True
        Else
            dlContractOfficers.Enabled = False
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
    Protected Sub openSalesEntry(ByVal s As Object, ByVal e As EventArgs)
        Dim myCntrctNum As String = Request.QueryString("cntrctNum").ToString
        'Dim myScript As String = "<script language=javascript>window.open('sales_entry.aspx?CntrctNum=" & myCntrctNum & "&Page=1','Details','toolbar=no,menubar=no,resizable=no,width=865,height=300')</script>"
        'Dim P As System.Web.UI.Page = CType(System.Web.HttpContext.Current.Handler, System.Web.UI.Page)
        'P.ClientScript.RegisterStartupScript(P.GetType, "", myScript)
        Response.Write("<script language=javascript>window.open('sales_entry.aspx?CntrctNum=" & myCntrctNum & "&Page=1&SchNum=" & Request.QueryString("SchNum").ToString & "','Details','toolbar=no,menubar=no,resizable=1,width=865,height=400')</script>")
    End Sub

    Private Sub ContractDataSource_Updated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs) Handles ContractDataSource.Updated
        Dim myScript As String = ""
        Dim view As MultiView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)
        If Not view Is Nothing Then
            Session("CurrentViewIndex") = view.ActiveViewIndex
        End If

        Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not dlSBAPlanName Is Nothing Then
            dlSBAPlanName.DataBind()
        End If

        Dim dlState As DropDownList = CType(fvContractInfo.FindControl("dlState"), DropDownList)
        If Not dlState Is Nothing Then
            dlState.DataBind()
        End If

        Dim dlOrderState As DropDownList = CType(fvContractInfo.FindControl("dlOrderState"), DropDownList)
        If Not dlOrderState Is Nothing Then
            dlOrderState.DataBind()
        End If

    End Sub
    Protected Sub ContractDataSource_OnUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles ContractDataSource.Updating
        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        'adding timeout to accommodate new trigger
        e.Command.CommandTimeout = 120

        e.Command.Parameters("@LastModifiedBy").Value = browserSecurity.UserInfo.LoginName
        e.Command.Parameters("@LastModificationDate").Value = DateTime.Now.ToString()

        Dim StandardizedCheckBox As CheckBox
        Dim StandardizedValue As Boolean = False

        StandardizedCheckBox = CType(fvContractInfo.FindControl("StandardizedCheckBox"), CheckBox)

        If Not StandardizedCheckBox Is Nothing Then
            StandardizedValue = StandardizedCheckBox.Checked
        End If

        e.Command.Parameters("@Standardized").Value = StandardizedValue
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
            setButtonColors("btnRebate")
        ElseIf myView = 5 Then
            setButtonColors("btnPrice")
        ElseIf myView = 6 Then
            setButtonColors("btnSales")
        ElseIf myView = 7 Then
            setButtonColors("btnChecks")
        ElseIf myView = 8 Then
            setButtonColors("btnSBA")
        ElseIf myView = 9 Then
            setButtonColors("btnBOC")
        ElseIf myView = 10 Then
            setButtonColors("btnBPAInfo")
        ElseIf myView = 11 Then
            setButtonColors("btnBPAPrice")
        ElseIf myView = 12 Then
            setButtonColors("btnFSSDetails")
            populateFSSDetail()
            Dim myFormView As FormView = CType(fvContractInfo.Row.FindControl("fvBPAFSSGeneral"), FormView)
            Dim myViewFSS As MultiView
            myViewFSS = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
            myViewFSS.ActiveViewIndex = 0
        End If
    End Sub
    Protected Sub UpdateSINGrid(ByVal s As Object, ByVal e As GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName = "NoDataInsert" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSINS"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.Controls(0).Controls(0).FindControl("dlNoSin"), DropDownList)
            Dim myCheckBox As CheckBox = CType(myGridView.Controls(0).Controls(0).FindControl("cbInsertRC"), CheckBox)
            Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.VarChar, 10)
            Dim myRC As SqlParameter = New SqlParameter("@Recoverable", SqlDbType.Bit)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString

            Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
            Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

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

            LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
            LastModificationDateParm.Value = DateTime.Now

            insertParameters(0) = mySIN
            insertParameters(1) = myRC
            insertParameters(2) = myContractNumber
            insertParameters(3) = LastModifiedByParm
            insertParameters(4) = LastModificationDateParm
            SINdataSource.Insert()
        ElseIf e.CommandName = "InsertNew" Then
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSINS"), Global.System.Web.UI.WebControls.GridView)
            Dim myDropList As DropDownList = CType(myGridView.FooterRow.FindControl("dlAddSin"), DropDownList)
            Dim myCheckBox As CheckBox = CType(myGridView.FooterRow.FindControl("cbInsertRC"), CheckBox)
            Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.VarChar, 10)
            Dim myRC As SqlParameter = New SqlParameter("@Recoverable", SqlDbType.Bit)
            Dim myContractNumber As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString

            Dim LastModifiedByParm As SqlParameter = New SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, 120)
            Dim LastModificationDateParm As SqlParameter = New SqlParameter("@LastModificationDate", SqlDbType.DateTime)

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

            LastModifiedByParm.Value = browserSecurity.UserInfo.LoginName
            LastModificationDateParm.Value = DateTime.Now

            insertParameters(0) = mySIN
            insertParameters(1) = myRC
            insertParameters(2) = myContractNumber
            insertParameters(3) = LastModifiedByParm
            insertParameters(4) = LastModificationDateParm
            SINdataSource.Insert()
        ElseIf e.CommandName = "Delete" Then

            Dim removeButton As Button = CType(e.CommandSource, Button)
            Dim currentRow As GridViewRow = CType(removeButton.NamingContainer, GridViewRow)
            Dim selectedSINLabel As Label = CType(currentRow.FindControl("SelectedSINLabel"), Label)

            '<asp:Parameter Name="UserLogin" />
            '                <asp:Parameter Name="UserId" />
            '                <asp:Parameter Name="ContractNumber" />
            '                <asp:Parameter Name="SIN" />

            Dim userLoginParm As Parameter = New Parameter("UserLogin", TypeCode.String)
            Dim userIdParm As Parameter = New Parameter("UserId", TypeCode.String)
            Dim contractNumberParm As Parameter = New Parameter("ContractNumber", TypeCode.String)
            Dim selectedSINParm As Parameter = New Parameter("SIN", TypeCode.String)

            Dim currentDocument As CurrentDocument
            currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

            userLoginParm.DefaultValue = browserSecurity.UserInfo.LoginName
            userIdParm.DefaultValue = browserSecurity.UserInfo.UserId.ToString()
            contractNumberParm.DefaultValue = currentDocument.ContractNumber
            selectedSINParm.DefaultValue = selectedSINLabel.Text

            SINdataSource.DeleteParameters.Add(userLoginParm)
            SINdataSource.DeleteParameters.Add(userIdParm)
            SINdataSource.DeleteParameters.Add(contractNumberParm)
            SINdataSource.DeleteParameters.Add(selectedSINParm)

            SINdataSource.Delete()
        End If
    End Sub
  
    Protected Sub RecoverableCheckBox_OnCheckedChanged(ByVal obj As Object, ByVal e As EventArgs)
        '       Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSINS"), Global.System.Web.UI.WebControls.GridView)

        Dim recoverableCheckBox As CheckBox = CType(obj, CheckBox)
        Dim currentRow As GridViewRow = CType(recoverableCheckBox.NamingContainer, GridViewRow)
        Dim selectedSINLabel As Label = CType(currentRow.FindControl("SelectedSINLabel"), Label)
        Dim CntrctNumber As String = Request.QueryString("CntrctNum").ToString

        Dim mySIN As Parameter = New Parameter("SIN", TypeCode.String)
        Dim recoverableCheckBoxParm As Parameter = New Parameter("Recoverable", TypeCode.Boolean)
        Dim myContractNumber As Parameter = New Parameter("CntrctNum", TypeCode.String)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Dim LastModifiedByParm As Parameter = New Parameter("LastModifiedBy", TypeCode.String)
        Dim LastModificationDateParm As Parameter = New Parameter("LastModificationDate", TypeCode.DateTime)

        LastModifiedByParm.DefaultValue = browserSecurity.UserInfo.LoginName
        LastModificationDateParm.DefaultValue = DateTime.Now.ToString()

        mySIN.Direction = ParameterDirection.Input
        mySIN.DefaultValue = selectedSINLabel.Text

        myContractNumber.Direction = ParameterDirection.Input
        myContractNumber.DefaultValue = CntrctNumber


        recoverableCheckBoxParm.Direction = ParameterDirection.Input
        If recoverableCheckBox.Checked Then
            recoverableCheckBoxParm.DefaultValue = "True"
        Else
            recoverableCheckBoxParm.DefaultValue = "False"
        End If

        SINdataSource.UpdateParameters.Add(myContractNumber)
        SINdataSource.UpdateParameters.Add(mySIN)
        SINdataSource.UpdateParameters.Add(recoverableCheckBoxParm)
        SINdataSource.UpdateParameters.Add(LastModifiedByParm)
        SINdataSource.UpdateParameters.Add(LastModificationDateParm)

        SINdataSource.Update()

    End Sub
    Private Sub SINDataSource_Inserting1(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles SINdataSource.Inserting
        e.Command.Parameters.Clear()
        Dim i As Integer
        For i = 0 To 4
            e.Command.Parameters.Add(insertParameters(i))
        Next i
    End Sub

    Private Sub SINDataSource_Deleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles SINdataSource.Deleting
        Dim i As Integer = 1
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
        If isFinance2() = True Then

            Dim checksGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
            Dim contractNumber As String = Request.QueryString("CntrctNum").ToString
            Dim quarterDropDownList As DropDownList
            Dim checkAmountTextBox As TextBox
            Dim checkNumberTextBox As TextBox
            Dim depositNumberTextBox As TextBox
            Dim dateReceivedTextBox As TextBox
            Dim commentsTextBox As TextBox

            Dim selectedQuarter As Integer
            Dim checkAmount As Double
            Dim checkNumber As String
            Dim depositNumber As String
            Dim dateReceived As DateTime
            Dim comments As String

            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            If e.CommandName = "InsertEmpty" Then
                quarterDropDownList = CType(checksGridView.Controls(0).Controls(0).FindControl("dlQuarterInsertNew"), DropDownList)
                checkAmountTextBox = CType(checksGridView.Controls(0).Controls(0).FindControl("tbCheckAmountInsertNew"), TextBox)
                checkNumberTextBox = CType(checksGridView.Controls(0).Controls(0).FindControl("tbChecknumInsertNew"), TextBox)
                depositNumberTextBox = CType(checksGridView.Controls(0).Controls(0).FindControl("tbCheckDepositInsertNew"), TextBox)
                dateReceivedTextBox = CType(checksGridView.Controls(0).Controls(0).FindControl("tbdateRecvdInsertNew"), TextBox)
                commentsTextBox = CType(checksGridView.Controls(0).Controls(0).FindControl("tbCommentsInsertNew"), TextBox)

            ElseIf e.CommandName = "InsertNew" Then
                quarterDropDownList = CType(checksGridView.FooterRow.FindControl("dlQuarterNew"), DropDownList)
                checkAmountTextBox = CType(checksGridView.FooterRow.FindControl("tbCheckAmountNew"), TextBox)
                checkNumberTextBox = CType(checksGridView.FooterRow.FindControl("tbChecknumNew"), TextBox)
                depositNumberTextBox = CType(checksGridView.FooterRow.FindControl("tbCheckDepositNew"), TextBox)
                dateReceivedTextBox = CType(checksGridView.FooterRow.FindControl("tbdateRecvdNew"), TextBox)
                commentsTextBox = CType(checksGridView.FooterRow.FindControl("tbCommentsNew"), TextBox)
            Else
                'not a valid command so return
                Return
            End If

            selectedQuarter = CType(quarterDropDownList.SelectedValue, Integer)
            If selectedQuarter <= 0 Then
                Msg_Alert.Client_Alert_OnLoad("Quarter must be selected before saving check entry")
                Return
            End If
            'check amount is not required
            If checkAmountTextBox.Text.Length > 0 Then
                checkAmount = CType(checkAmountTextBox.Text, Double)
            Else
                checkAmount = 0
            End If
            'check number is not required
            checkNumber = checkNumberTextBox.Text

            depositNumber = depositNumberTextBox.Text
            If depositNumber.Length = 0 Then
                Msg_Alert.Client_Alert_OnLoad("Deposit number must be entered before saving check entry")
                Return
            End If

            If dateReceivedTextBox.Text.Length > 0 Then
                Try
                    dateReceived = CType(dateReceivedTextBox.Text, DateTime)
                Catch dateEx As Exception
                    Msg_Alert.Client_Alert_OnLoad("Invalid date format for date received")
                    Return
                End Try
            Else
                Msg_Alert.Client_Alert_OnLoad("Date received must be entered before saving check entry")
                Return
            End If

            'comments are required if no check amount
            comments = commentsTextBox.Text
            If checkAmount = 0 And comments.Length = 0 Then
                Msg_Alert.Client_Alert_OnLoad("Comments are required if the check amount is blank or zero. Enter comments before saving check entry.")
                Return
            End If

            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim strSQL As String = "INSERT INTO dbo.tbl_Cntrcts_Checks (CntrctNum, Quarter_ID, CheckAmt, CheckNum, DepositNum, DateRcvd, Comments, LastModifiedBy, LastModificationDate ) VALUES (@CntrctNum, @Quarter_ID, @CheckAmt, @CheckNum, @DepositNum, @DateRcvd, @Comments, @LastModifiedBy, @LastModificationDate)"
                Dim insertCommand As SqlCommand = New SqlCommand()
                insertCommand.Connection = conn
                insertCommand.CommandText = strSQL
                insertCommand.Parameters.Add("@Quarter_ID", SqlDbType.Int).Value = selectedQuarter
                insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = contractNumber
                insertCommand.Parameters.Add("@CheckAmt", SqlDbType.Money).Value = checkAmount
                insertCommand.Parameters.Add("@CheckNum", SqlDbType.VarChar, 50).Value = checkNumber
                insertCommand.Parameters.Add("@DepositNum", SqlDbType.VarChar, 50).Value = depositNumber
                insertCommand.Parameters.Add("@DateRcvd", SqlDbType.DateTime).Value = dateReceived
                insertCommand.Parameters.Add("@Comments", SqlDbType.NVarChar, 255).Value = comments
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
                insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now.ToString()
                conn.Open()
                insertCommand.ExecuteNonQuery()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("Error inserting check information for contract: " & contractNumber & " was as follows " & ex.ToString)
            Finally
                conn.Close()
            End Try

            checksGridView.DataBind()
        Else
            Msg_Alert.Client_Alert_OnLoad("You are not authorized to enter check information. If you think this is an error contact the data base administrator.")
        End If
    End Sub
    'Protected Sub checkCheckContent()
    '    Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '    If myGridView.Rows.Count < 1 Then
    '        Dim myButton As Button = CType(fvContractInfo.Row.FindControl("btnShowFooter"), Button)
    '        myButton.Visible = "False"
    '        myButton = CType(fvContractInfo.Row.FindControl("btnHideFooter"), Button)
    '        myButton.Visible = "False"
    '    End If

    'End Sub
    'Protected Sub ShowColumn(ByVal sender As Object, ByVal e As EventArgs)
    '    If isFinance() Then
    '        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '        myGrid.Columns(6).Visible = "True"
    '    Else
    '        Msg_Alert.Client_Alert_OnLoad("You must be idenified as a member of finance to enter check info.  See the system admin.")
    '    End If
    'End Sub
    Protected Sub btnAddCheck_OnClick(ByVal s As Object, ByVal e As System.EventArgs)
        Dim gvChecks As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
        gvChecks.ShowFooter = True
        Dim cancelCheckButton As Button = CType(fvContractInfo.Row.FindControl("btnCancelAddCheck"), Button)
        cancelCheckButton.Visible = True
    End Sub

    Protected Sub btnCancelAddCheck_OnClick(ByVal s As Object, ByVal e As System.EventArgs)
        Dim gvChecks As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
        gvChecks.ShowFooter = False
        Dim cancelCheckButton As Button = CType(fvContractInfo.Row.FindControl("btnCancelAddCheck"), Button)
        cancelCheckButton.Visible = False
    End Sub

    Protected Sub gvChecks_OnDataBound(ByVal sender As Object, ByVal args As System.EventArgs)
        checkChecksEditable()
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
    Protected Sub checkChecksEditable()
        Dim gvChecks As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
        Dim addCheckButton As Button = CType(fvContractInfo.Row.FindControl("btnAddCheck"), Button)
        Dim cancelCheckButton As Button = CType(fvContractInfo.Row.FindControl("btnCancelAddCheck"), Button)

        'If Not IsPostBack Then
        '    cancelCheckButton.Visible = False ' initial state is hidden
        'End If

        If isFinance2() = True Then
            gvChecks.Columns(6).Visible = True
            If gvChecks.Rows.Count > 0 Then
                addCheckButton.Visible = True
            End If
        Else
            gvChecks.Columns(6).Visible = False
            addCheckButton.Visible = False
            If gvChecks.Rows.Count = 0 Then
                ReplaceChecksEmptyDataTemplate()
            End If
        End If
    End Sub
    'replace the checks empty data template with a simple message for use when the user
    'does not have permissions to add a record
    Protected Sub ReplaceChecksEmptyDataTemplate()
        Dim gvChecks As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
        Dim checksEmptyDataTemplatePanel As Panel = FindControlRecursive(gvChecks, "checksEmptyDataTemplatePanel")
        Dim checksEmptyDataPanelNoPermissionsPanel As Panel = FindControlRecursive(gvChecks, "checksEmptyDataPanelNoPermissionsPanel")

        If Not checksEmptyDataTemplatePanel Is Nothing Then
            checksEmptyDataTemplatePanel.Visible = False
        End If

        If Not checksEmptyDataPanelNoPermissionsPanel Is Nothing Then
            checksEmptyDataPanelNoPermissionsPanel.Visible = True
        End If
    End Sub

    'Protected Sub dlQuarterEdit_OnDataBound(ByVal sender As Object, ByVal e As System.EventArgs)

    '    Dim quarterDropDownList As DropDownList = CType(sender, DropDownList)
    '    Dim gridViewRow As GridViewRow = CType(quarterDropDownList.NamingContainer, GridViewRow)

    '    Dim selectedQuarterId As Integer = -1
    '    Dim selectedQuarterIdString As String
    '    Dim currentDataRowView As DataRowView
    '    Dim listItem As ListItem
    '    If Not gridViewRow.DataItem Is Nothing Then
    '        currentDataRowView = gridViewRow.DataItem
    '        selectedQuarterIdString = currentDataRowView("Quarter_ID").ToString()

    '        If Not selectedQuarterIdString Is Nothing Then

    '            If Integer.TryParse(selectedQuarterIdString, selectedQuarterId) = True Then
    '                listItem = quarterDropDownList.Items.FindByValue(selectedQuarterId.ToString())
    '                If Not listItem Is Nothing Then
    '                    listItem.Selected = True
    '                End If
    '            End If
    '        End If
    '    End If

    'End Sub





    Private Function FindControlRecursive(ByVal ctrl As Control, ByVal id As String) As Control
        Dim nextCtrl As Control

        If (ctrl.ID = id) Then
            Return ctrl
        End If

        For Each testControl As Control In ctrl.Controls
            nextCtrl = FindControlRecursive(testControl, id)
            If Not nextCtrl Is Nothing Then
                Return nextCtrl
            End If
        Next

        Return Nothing
    End Function
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
            Dim myPlanID As String = ""
            Dim myDropDown As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
            If Not myDropDown Is Nothing Then
                myPlanID = CType(myDropDown.SelectedValue, String)
            End If

            '  Dim strURL As String = "sba_projections_add.aspx?Insert=Y&SBAPlanID=" & myPlanID
            ' Dim strFeature As String = "toolbar=no,menubar=no,resizable=no,width=805,height=450"
            ' Response.Write("<script>window.open('" & strURL & "','Proj','" & strFeature & "')</script>)")



        End If
    End Sub

    Protected Function CalculateTotal() As String
        Dim myTotal As String = ""
        Dim myVATotal As Double = 0
        Dim myOGATotal As Double = 0
        Return myTotal
    End Function
    Protected Sub ShowSalesFooter(ByVal S As Object, ByVal e As EventArgs)
        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        Dim myButton As Button = CType(S, Button)
        Dim myCancel As Button = CType(fvContractInfo.Row.FindControl("btnSalesCancelFooter"), Button)
        If myButton.ID.Equals("btnSalesFooter") Then
            myGrid.ShowFooter = "True"
            myCancel.Visible = "True"
        ElseIf myButton.ID.Equals("btnSalesCancelFooter") Then
            myGrid.ShowFooter = "False"
            myCancel.Visible = "False"
        End If

    End Sub

    Private Sub NAC_CM_Edit_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        If myGrid.Rows.Count >= 1 Then
            Dim myLabel As Label = CType(myGrid.Controls(0).Controls(0).FindControl("lbTitle"), Label)
            If Not myLabel Is Nothing Then
                Dim myButton As Button = CType(myGrid.Controls(0).Controls(0).FindControl("btnItemDetails"), Button)
                myButton.Visible = "True"
                myButton = CType(myGrid.Controls(0).Controls(0).FindControl("btnItemEdit"), Button)
                myButton.Visible = "True"
            End If
        End If
    End Sub


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
    '    ' Return "True"
    'End Function

    'Protected Sub checkCheckContent()
    '    Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '    If myGridView.Rows.Count < 1 Then
    '        Dim myButton As Button = CType(fvContractInfo.Row.FindControl("btnShowFooter"), Button)
    '        myButton.Visible = "False"
    '        myButton = CType(fvContractInfo.Row.FindControl("btnHideFooter"), Button)
    '        myButton.Visible = "False"
    '    End If

    'End Sub
    'Protected Sub ShowColumn(ByVal sender As Object, ByVal e As EventArgs)
    '    If isFinance2() Then
    '        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '        myGrid.Columns(6).Visible = "True"
    '    Else
    '        ' MsgBox("You must be idenified as a member of finance to enter check info.  See the system admin.", MsgBoxStyle.OkOnly)
    '    End If
    'End Sub

    ' add client script to
    ' set up the click function for the export button
    ' and to refresh the parent window
    Protected Sub SetupSalesExport()
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

    Protected Sub setUpSalesFun()
        Dim clientScriptManager As ClientScriptManager = Page.ClientScript
        Dim openFunctionCallString As String = "function SalesUpdate( contractNumber, QtrID ) { exportWindow = window.open(""sales_detail_edit.aspx?CntrctNum="" + contractNumber + ""&QTRID="" + QtrID ,"""",""resizable=0,scrollbars=1,menubar=no,width=800,height=530,left=0,top=0"") ; }"
        clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "SalesUpload", openFunctionCallString, True)
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
    Protected Sub excel_Click(ByVal s As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
        'Dim myTitle As String = "PriceList_" & myCntrctNum & "_" & Date.Now.Month.ToString & Date.Now.Day.ToString & Date.Now.Year.ToString & Date.Now.Hour.ToString & Date.Now.Minute.ToString & ".xls"
        'open_Excel(myCntrctNum, myTitle)
        'Response.Write("<script>window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & myCntrctNum & "','Details','toolbar=no,menubar=no,resizable=no,width=805,height=450')</script>")
    End Sub


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


    Protected Sub SetupSBA()
        'Dim addPlanWindowOpenCommand As String = ""
        'addPlanWindowOpenCommand = "window.open('SBA_Add_Plan.aspx','SBA_Add_List','resizable=0,scrollbars=1,width=350,height=200,left=400,top=400')"
        'Dim btnAddSBA As Button = CType(fvContractInfo.FindControl("btnAddSBA"), Button)
        'btnAddSBA.Text = "Add New SBA" & vbCrLf & "Vendor/Plan Name"
        'btnAddSBA.Attributes.Add("onclick", addPlanWindowOpenCommand)


    End Sub
    Protected Sub SetSBAAddProjects()
        Dim myPlanID As String = ""
        Dim myDropDown As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not myDropDown Is Nothing Then
            myPlanID = CType(myDropDown.SelectedValue, String)
        End If
        Dim mySBAGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.FindControl("gvSBAProjections"), Global.System.Web.UI.WebControls.GridView)
        If Not mySBAGrid Is Nothing Then
            If mySBAGrid.Rows.Count > 0 Then
                Dim myButton As Button = CType(mySBAGrid.FooterRow.Cells(0).FindControl("btnAddSBAProjNew"), Button)
                If Not myButton Is Nothing Then
                    myButton.Attributes.Add("onclick", "AddProjection('" & myPlanID & "')")
                End If
            End If

            Dim myButton2 As Button = CType(mySBAGrid.Controls(0).Controls(0).FindControl("btnAddSBAProjEmpty"), Button)
            If Not myButton2 Is Nothing Then
                myButton2.Attributes.Add("onclick", "AddProjection('" & myPlanID & "')")
            End If

            Dim clientScriptManager As ClientScriptManager = Page.ClientScript
            Dim openFunctionCallString As String = "function AddProjection( myPlanID ) { var checkId = myPlanID.length; if( checkId == 0 || myPlanID == 511 ){ alert(""Please select a plan before entering projections."");  } else { ProjectWindow = window.open(""sba_projections_add.aspx?Insert=Y&SBAPlanID="" + myPlanID ,"""",""resizable=0,scrollbars=1,menubar=no,width=805,height=450,left=0,top=0"") ; }}"
            clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "AddProjection", openFunctionCallString, True)
            Dim refreshSBAPlanProjectFunctionCallString As String = "function RefreshSBAPlan() { document.getElementById('fvContractInfo_hfSBAProjectAdd').value = ""true""; __doPostBack("",""); }"
            clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "RefreshSBAPlan", refreshSBAPlanProjectFunctionCallString, True)
        End If
    End Sub
    Protected Sub setSBA294()
        Dim myPlanID As String = ""
        Dim myDropDown As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not myDropDown Is Nothing Then
            myPlanID = CType(myDropDown.SelectedValue, String)
        End If
        Dim mySBAGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.FindControl("gv294Reports"), Global.System.Web.UI.WebControls.GridView)
        If Not mySBAGrid Is Nothing Then
            If mySBAGrid.Rows.Count > 0 Then
                Dim myButton As Button = CType(mySBAGrid.FooterRow.Cells(0).FindControl("btnAddSBA294jNew"), Button)
                If Not myButton Is Nothing Then
                    myButton.Attributes.Add("onclick", "Add294('" & myPlanID & "')")
                End If
            End If
            Dim myButton2 As Button = CType(mySBAGrid.Controls(0).Controls(0).FindControl("btnAddSBA294Empty"), Button)
            If Not myButton2 Is Nothing Then
                myButton2.Attributes.Add("onclick", "Add294('" & myPlanID & "')")
            End If
            Dim clientScriptManager As ClientScriptManager = Page.ClientScript
            Dim openFunctionCallString As String = "function Add294( myPlanID ) { ProjectWindow = window.open(""sba_accomp.aspx?Insert=Y&SBAPlanID="" + myPlanID ,"""",""resizable=0,scrollbars=1,menubar=no,width=805,height=550,left=0,top=0"") ; }"
            clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "Add294", openFunctionCallString, True)
            Dim refreshSBAPlanProjectFunctionCallString As String = "function RefreshSBAPlanProjections() { document.getElementById('fvContractInfo_hfSBAProjectAdd').value = ""true""; __doPostBack("",""); }"
            clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "RefreshSBAPlanProjections", refreshSBAPlanProjectFunctionCallString, True)
            Dim refreshSBAPlanListFunctionCallString As String = "function RefreshSBAPlanList(newSBAPlanId) { document.getElementById('fvContractInfo_hfRefreshSBAPlanList').value = ""true""; document.getElementById('fvContractInfo_hfNewSBAPlanId').value = newSBAPlanId;__doPostBack("",""); }"
            clientScriptManager.RegisterClientScriptBlock(Me.GetType(), "RefreshSBAPlanList", refreshSBAPlanListFunctionCallString, True)
        End If
    End Sub
    Protected Sub RefreshSBAPlanList(ByVal newSBAPlanId As Integer)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim da As SqlDataAdapter
        Dim ds As DataSet
        Dim testRow As System.Data.DataRow

        Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not dlSBAPlanName Is Nothing Then

            'save for select during binding
            Session("CurrentSelectedSBAPlanId") = newSBAPlanId

            'dlSBAPlanName.ClearSelection()
            'dlSBAPlanName.Items.Clear()
            'dlSBAPlanName.DataBind()

            Dim strSQL As String = "SELECT [SBAPlanID], [PlanName] FROM view_sba_plans_sorted"

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
                                    If Not testRow("SBAPlanID") Is DBNull.Value Then
                                        If Not dlSBAPlanName Is Nothing Then
                                            dlSBAPlanName.ClearSelection()
                                            dlSBAPlanName.Items.Clear()
                                            dlSBAPlanName.DataSource = ds.Tables(0)
                                            dlSBAPlanName.DataTextField = ds.Tables(0).Columns("PlanName").ColumnName.ToString()
                                            dlSBAPlanName.DataValueField = ds.Tables(0).Columns("SBAPlanID").ColumnName.ToString()
                                            dlSBAPlanName.DataBind()
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

                conn.Close()


            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("Error refreshing sba plan list " & ex.ToString)
            Finally
                conn.Close()
            End Try

        End If
    End Sub
    Protected Sub RestoreSelectedSBAPlanId()
        Dim currentSelectedSBAPlanId As String = ""
        Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)

        If Not dlSBAPlanName Is Nothing Then
            currentSelectedSBAPlanId = CType(Session("CurrentSelectedSBAPlanId"), String)
            If Not currentSelectedSBAPlanId Is Nothing Then
                If currentSelectedSBAPlanId.Length > 0 And currentSelectedSBAPlanId.CompareTo("") > 0 Then
                    dlSBAPlanName.SelectedValue = currentSelectedSBAPlanId
                End If
            End If
        End If
    End Sub


    Protected Sub RefreshSBAProjects()
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlCommand
        Dim mySBAID As String = ""
        Dim da As SqlDataAdapter
        Dim ds As DataSet
        Dim testRow As System.Data.DataRow

        Dim myDropDown As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not myDropDown Is Nothing Then
            mySBAID = CType(myDropDown.SelectedValue, String)
        End If
        Dim projectionGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSBAProjections"), Global.System.Web.UI.WebControls.GridView)

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


        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("SBAPlan info " & ex.ToString)
        Finally
            conn.Close()
        End Try
    End Sub
    Protected Sub UpdateSBAAdmin(ByVal s As Object, ByVal e As EventArgs)
        Dim mySBAID As String = ""
        Dim myDropDown As DropDownList = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not myDropDown Is Nothing Then
            mySBAID = CType(myDropDown.SelectedValue, String)
        End If
        Dim myFormView As FormView = CType(fvContractInfo.FindControl("fvSBAplanType"), FormView)
        Dim myPlanAdminName As TextBox = CType(myFormView.FindControl("tbPlanAdminName"), TextBox)
        Dim myPlanAdminEmail As TextBox = CType(myFormView.FindControl("tbPlanAdminEmail"), TextBox)
        Dim myPlanAdminAddress As TextBox = CType(myFormView.FindControl("tbPlanAdminAddress"), TextBox)
        Dim myPlanAdminCity As TextBox = CType(myFormView.FindControl("tbPlanAdminCity"), TextBox)
        Dim myPlanAdminstate As DropDownList = CType(myFormView.FindControl("dlPlanAdminstate"), DropDownList)
        Dim myPlanAdminZip As TextBox = CType(myFormView.FindControl("tbPlanAdminZip"), TextBox)
        Dim myPlanAdminPhone As TextBox = CType(myFormView.FindControl("tbPlanAdminPhone"), TextBox)
        Dim myPlanAdminFax As TextBox = CType(myFormView.FindControl("tbPlanAdminFax"), TextBox)
        Dim myPlanTypeID As DropDownList = CType(myFormView.FindControl("dlPlanType"), DropDownList)
        Dim strSQL As String = "UPDATE tbl_sba_SBAPlan SET Plan_Admin_Name = @planAdminName, Plan_Admin_Address1=@planAdminAddress1," _
        & " Plan_Admin_City=@planAdminCity, Plan_Admin_State=@planAdminState,Plan_Admin_Zip=@planAdminZip, Plan_Admin_Phone=@planAdminPhone, Plan_Admin_Fax=@planAdminFax, Plan_Admin_Email=@planAdminEmail, PlanTypeID = @planTypeID, LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate " _
        & " WHERE SBAPlanID =@SBAPlanID"
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim UpdateCommand As SqlCommand = New SqlCommand()

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Try
            UpdateCommand.Connection = conn
            UpdateCommand.CommandText = strSQL
            UpdateCommand.Parameters.Add("@planAdminName", SqlDbType.NVarChar).Value = CType(myPlanAdminName.Text, String)
            UpdateCommand.Parameters.Add("@planAdminAddress1", SqlDbType.NVarChar).Value = CType(myPlanAdminAddress.Text, String)
            UpdateCommand.Parameters.Add("@planAdminCity", SqlDbType.NVarChar).Value = CType(myPlanAdminCity.Text, String)
            UpdateCommand.Parameters.Add("@planAdminState", SqlDbType.NVarChar).Value = CType(myPlanAdminstate.Text, String)
            UpdateCommand.Parameters.Add("@planAdminZip", SqlDbType.NVarChar).Value = CType(myPlanAdminZip.Text, String)
            UpdateCommand.Parameters.Add("@planAdminPhone", SqlDbType.NVarChar).Value = CType(myPlanAdminPhone.Text, String)
            UpdateCommand.Parameters.Add("@planAdminFax", SqlDbType.NVarChar).Value = CType(myPlanAdminFax.Text, String)
            UpdateCommand.Parameters.Add("@planAdminEmail", SqlDbType.NVarChar).Value = CType(myPlanAdminEmail.Text, String)
            UpdateCommand.Parameters.Add("@planTypeID", SqlDbType.Int).Value = CType(myPlanTypeID.SelectedValue, Integer)
            UpdateCommand.Parameters.Add("@SBAPlanID", SqlDbType.Int).Value = CType(mySBAID, Integer)
            UpdateCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            UpdateCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            conn.Open()
            UpdateCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("This is a result of updating the Admin info: " & ex.ToString)
        Finally
            conn.Close()
        End Try

    End Sub

    Private Sub NAC_CM_Edit_PreLoad(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreLoad

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

        Dim PrimeVendorCheckBox As CheckBox
        Dim DODVAContractCheckBox As CheckBox

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.ScheduleNumber <> 36 Then
            PrimeVendorCheckBox = CType(fvContractInfo.Row.FindControl("cbPrimeVendor"), CheckBox)
            If Not PrimeVendorCheckBox Is Nothing Then
                e.Command.Parameters("@PV_Participation").Value = PrimeVendorCheckBox.Checked
            End If
            DODVAContractCheckBox = CType(fvContractInfo.Row.FindControl("cbDODVACOntract"), CheckBox)
            If Not DODVAContractCheckBox Is Nothing Then
                e.Command.Parameters("@VA_DOD").Value = DODVAContractCheckBox.Checked
            End If
        End If

        Dim effectiveDate As DateTime
        Dim expirationDate As DateTime

        If currentDocument.ScheduleNumber = 36 Or currentDocument.ScheduleNumber = 10 Then

            Dim InsurancePolicyEffectiveDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("InsurancePolicyEffectiveDateTextBox"), TextBox)
            If Not InsurancePolicyEffectiveDateTextBox Is Nothing Then
                If (DateTime.TryParse(InsurancePolicyEffectiveDateTextBox.Text, effectiveDate) = True) Then
                    e.Command.Parameters("@Insurance_Policy_Effective_Date").Value = effectiveDate
                End If
            End If

            Dim InsurancePolicyExpirationDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("InsurancePolicyExpirationDateTextBox"), TextBox)
            If Not InsurancePolicyExpirationDateTextBox Is Nothing Then
                If (DateTime.TryParse(InsurancePolicyExpirationDateTextBox.Text, expirationDate) = True) Then
                    e.Command.Parameters("@Insurance_Policy_Expiration_Date").Value = expirationDate
                End If
            End If

        End If

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

        Dim StimulusActCheckBox As CheckBox
        Dim StimulusActValue As Boolean

        StimulusActCheckBox = CType(fvContractInfo.FindControl("StimulusActCheckBox"), CheckBox)

        If Not StimulusActCheckBox Is Nothing Then
            StimulusActValue = StimulusActCheckBox.Checked
        End If

        e.Command.Parameters("@StimulusAct").Value = StimulusActValue

        Dim RebateRequiredCheckBox As CheckBox
        Dim RebateRequiredValue As Boolean

        RebateRequiredCheckBox = CType(fvContractInfo.FindControl("RebateRequiredCheckBox"), CheckBox)

        If Not RebateRequiredCheckBox Is Nothing Then
            RebateRequiredValue = RebateRequiredCheckBox.Checked
        End If

        e.Command.Parameters("@RebateRequired").Value = RebateRequiredValue

        If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
            e.Command.Parameters("@SBAPlanId").Value = CType(Session("CurrentSelectedSBAPlanId"), Integer)
        End If

        Dim stateDropDownList As DropDownList
        Dim selectedState As String
        Dim orderingStateDropDownList As DropDownList
        Dim selectedOrderingState As String
        Dim noneSelectedAbbreviation As String = "--" 'none selected

        stateDropDownList = CType(fvContractInfo.FindControl("dlState"), DropDownList)

        If Not stateDropDownList Is Nothing Then
            selectedState = CMGlobals.GetSelectedTextFromDropDownList(stateDropDownList)
            If selectedState.CompareTo(noneSelectedAbbreviation) <> 0 Then
                e.Command.Parameters("@Primary_State").Value = selectedState
            End If
        End If

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

        Dim lbExportPriceList As Label
        Dim btnExportPricelistToExcel As ImageButton
        lbExportPriceList = CType(fvContractInfo.FindControl("lbExportPriceList"), Label)
        btnExportPricelistToExcel = CType(fvContractInfo.FindControl("btnExportPricelistToExcel"), ImageButton)

        Dim lbViewDrugItemPricelistLabel As Label
        Dim btnViewDrugItemPricelist As Button
        lbViewDrugItemPricelistLabel = CType(fvContractInfo.FindControl("lbViewDrugItemPricelistLabel"), Label)
        btnViewDrugItemPricelist = CType(fvContractInfo.FindControl("btnViewDrugItemPricelist"), Button)

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
            lbExportPriceList.ForeColor = Drawing.Color.LightGray
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


    Private Sub NAC_CM_Edit_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        ' SetupSBA() changed to OnInit call of setFormatAddNewSBAPlanButton

        checkSalesEditable2()
        checkChecksEditable()
        RestoreSelectedSBAPlanId()

        If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
            LoadSBAPlanContractResponsible(CType(Session("CurrentSelectedSBAPlanId"), Integer))
        End If
    End Sub


    Protected Sub NACCMEditScriptManager_OnAsyncPostBackError(ByVal sender As Object, ByVal e As AsyncPostBackErrorEventArgs)
        Dim errorMsg As String = ""

        If Not e.Exception.Data("NACCMEditErrorMessage") Is Nothing Then
            errorMsg = String.Format("The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data("NACCMEditErrorMessage"))
        Else
            errorMsg = String.Format("The following error was encountered during async postback: {0}", e.Exception.Message)
        End If

        ScriptManager1.AsyncPostBackErrorMessage = errorMsg
    End Sub
#Region "rebateparmsconstantsandinit"

    Private _rebateDataSource As DocumentDataSource
    Private _rebateDateDataSource As DocumentDataSource
    Private _rebateStandardClauseNameDataSource As DocumentDataSource
    Private _dsRebateStandardClauses As DataSet

    Private _withAddRebateParameter As Parameter
    Private _contractNumberParameter As Parameter
    Private _userLoginParameter As Parameter
    Private _currentUserParameter As Parameter
    Private _clauseTypeParameter As Parameter
    Private _rebateDateSelectStartDate As Parameter
    Private _rebateDateSelectEndDate As Parameter

    Private _standardRebateTermIdParameter As Parameter
    Private _rebateIdParameter As Parameter
    Private _startQuarterIdParameter As Parameter
    Private _endQuarterIdParameter As Parameter
    Private _rebatePercentOfSalesParameter As Parameter
    Private _rebateThresholdParameter As Parameter
    Private _amountReceivedParameter As Parameter
    Private _isCustomParameter As Parameter
    Private _rebateClauseParameter As Parameter
    Private _customRebateIdParameter As Parameter
    Private _rebateTermIdParameter As Parameter
    Private _rebateIdForInsertParameter As Parameter
    Private _rebateCustomStartDateParameter As Parameter

    Private Const RebateViewTextButtonFieldNumber As Integer = 0 ' $$$+
    Private Const RebateEditButtonFieldNumber As Integer = 1  ' $$$+
    Private Const RebateStartYearQuarterFieldNumber As Integer = 2 '$$$+
    Private Const RebateEndYearQuarterFieldNumber As Integer = 3 '$$$+
    Private Const RebatePercentOfSalesFieldNumber As Integer = 4 '$$$+
    Private Const RebateThresholdFieldNumber As Integer = 5 '$$$+
    Private Const RebateClauseNameFieldNumber As Integer = 6 '$$$+
    Private Const RebateModifiedByFieldNumber As Integer = 7 '$$$+
    Private Const RebateLastModificationDateFieldNumber As Integer = 8 '$$$+

    Private Const RebateRemoveButtonFieldNumber As Integer = 9  ' $$$+
    Private Const RebateTermIdFieldNumber As Integer = 10  ' $$$+
    Private Const RebatesStandardRebateTermIdFieldNumber As Integer = 11  ' $$$+

    Private Const RebatePercentTagString As String = "{percent}"
    Private Const RebateThresholdTagString As String = "{threshold}"

    Private Sub InitRebateControls(ByVal IsPostBack As Boolean, ByVal contractNumber As String)

        Dim bSuccess As Boolean
        Dim contractDB As ContractDB
        Dim dsRebates As System.Data.DataSet
        Dim postbackSourceName As String = ""

        Dim bs As BrowserSecurity2
        Dim currentDocument As CurrentDocument

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        bs = CType(Session("BrowserSecurity"), BrowserSecurity2)
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If IsPostBack = False Then
            ClearSessionVariables()
        End If

        If Session("RebateDataSource") Is Nothing Then

            _rebateDataSource = New DocumentDataSource(bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, False)

            _rebateDataSource.ID = "RebateDataSource"
            _rebateDataSource.DataSourceMode = SqlDataSourceMode.DataSet

            _rebateDataSource.SelectCommand = "SelectRebatesForContract"

            _rebateDataSource.UpdateCommand = "UpdateContractRebate"

            _rebateDataSource.InsertCommand = "InsertContractRebate"

            _rebateDataSource.DeleteCommand = "DeleteContractRebate"

            CreateRebateDataSourceParameters()

            _rebateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure

            _rebateDataSource.SelectParameters.Add(_contractNumberParameter)
            _contractNumberParameter.DefaultValue = contractNumber
            _rebateDataSource.SelectParameters.Add(_userLoginParameter)
            _userLoginParameter.DefaultValue = bs.UserInfo.LoginName
            _withAddRebateParameter.DefaultValue = "false" ' not adding
            _rebateDataSource.SelectParameters.Add(_withAddRebateParameter)

            _rebateDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure

            _rebateDataSource.UpdateParameters.Add(_contractNumberParameter)
            _rebateDataSource.UpdateParameters.Add(_userLoginParameter)

            _rebateDataSource.UpdateParameters.Add(_standardRebateTermIdParameter)
            _rebateDataSource.UpdateParameters.Add(_rebateIdParameter)
            _rebateDataSource.UpdateParameters.Add(_startQuarterIdParameter)
            _rebateDataSource.UpdateParameters.Add(_endQuarterIdParameter)
            _rebateDataSource.UpdateParameters.Add(_rebatePercentOfSalesParameter)
            _rebateDataSource.UpdateParameters.Add(_rebateThresholdParameter)
            _rebateDataSource.UpdateParameters.Add(_amountReceivedParameter)
            _rebateDataSource.UpdateParameters.Add(_isCustomParameter)
            _rebateDataSource.UpdateParameters.Add(_rebateClauseParameter)
            _rebateDataSource.UpdateParameters.Add(_customRebateIdParameter)
            _rebateDataSource.UpdateParameters.Add(_rebateTermIdParameter)
            _rebateDataSource.UpdateParameters.Add(_rebateCustomStartDateParameter)

            _rebateDataSource.InsertCommandType = SqlDataSourceCommandType.StoredProcedure

            _rebateDataSource.InsertParameters.Add(_userLoginParameter)
            _rebateDataSource.InsertParameters.Add(_contractNumberParameter)
            _rebateDataSource.InsertParameters.Add(_startQuarterIdParameter)
            _rebateDataSource.InsertParameters.Add(_endQuarterIdParameter)
            _rebateDataSource.InsertParameters.Add(_rebatePercentOfSalesParameter)
            _rebateDataSource.InsertParameters.Add(_rebateThresholdParameter)
            _rebateDataSource.InsertParameters.Add(_amountReceivedParameter)
            _rebateDataSource.InsertParameters.Add(_isCustomParameter)
            _rebateDataSource.InsertParameters.Add(_rebateClauseParameter)
            _rebateDataSource.InsertParameters.Add(_standardRebateTermIdParameter)
            _rebateDataSource.InsertParameters.Add(_rebateCustomStartDateParameter)

            _rebateDataSource.InsertParameters.Add(_rebateIdForInsertParameter)

            _rebateDataSource.InsertParameters.Add(_customRebateIdParameter)
            _rebateDataSource.InsertParameters.Add(_rebateTermIdParameter)

            _rebateDataSource.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure

            _rebateDataSource.DeleteParameters.Add(_userLoginParameter)
            _rebateDataSource.DeleteParameters.Add(_contractNumberParameter)
            _rebateDataSource.DeleteParameters.Add(_rebateIdParameter)

            ' save to session
            Session("RebateDataSource") = _rebateDataSource

        Else
            _rebateDataSource = CType(Session("RebateDataSource"), DocumentDataSource)

            ' rebateDataSource.RestoreDelegatesAfterDeserialization(this)

            RestoreRebateDataSourceParameters(_rebateDataSource)

        End If

        AddHandler _rebateDataSource.Updated, AddressOf rebateDataSource_Updated
        AddHandler _rebateDataSource.Inserted, AddressOf rebateDataSource_Inserted

        rebateGridView.DataSource = _rebateDataSource


        If Session("RebateDateDataSource") Is Nothing Then
            _rebateDateDataSource = New DocumentDataSource(bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, False)

            _rebateDateDataSource.ID = "RebateDateDataSource"
            _rebateDateDataSource.DataSourceMode = SqlDataSourceMode.DataSet

            _rebateDateDataSource.SelectCommand = "SelectYearQuartersForDateRange"
            _rebateDateDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure


            _rebateDateSelectStartDate = New Parameter("StartDate", TypeCode.DateTime)
            _rebateDateSelectEndDate = New Parameter("EndDate", TypeCode.DateTime)
            _rebateDateDataSource.SelectParameters.Add(_userLoginParameter)
            _rebateDateDataSource.SelectParameters.Add(_rebateDateSelectStartDate)
            _rebateDateDataSource.SelectParameters.Add(_rebateDateSelectEndDate)

            _userLoginParameter.DefaultValue = bs.UserInfo.LoginName
            _rebateDateSelectStartDate.DefaultValue = currentDocument.AwardDate.ToString()
            _rebateDateSelectEndDate.DefaultValue = currentDocument.ExpirationDate.ToString()


            ' save to session
            Session("RebateDateDataSource") = _rebateDateDataSource

        Else
            _rebateDateDataSource = CType(Session("RebateDateDataSource"), DocumentDataSource)

            _userLoginParameter = _rebateDateDataSource.SelectParameters("UserLogin")
            _rebateDateSelectStartDate = _rebateDateDataSource.SelectParameters("StartDate")
            _rebateDateSelectEndDate = _rebateDateDataSource.SelectParameters("EndDate")

        End If

        ' dates are bound during grid row binding

        If Session("StandardClauseNameDataSource") Is Nothing Then
            _rebateStandardClauseNameDataSource = New DocumentDataSource(bs, DocumentDataSource.TargetDatabases.NACCMCommonUser, False)

            _rebateStandardClauseNameDataSource.ID = "StandardClauseNameDataSource"
            _rebateStandardClauseNameDataSource.DataSourceMode = SqlDataSourceMode.DataSet

            _rebateStandardClauseNameDataSource.SelectCommand = "SelectStandardRebateTerms"
            _rebateStandardClauseNameDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure

            'values set above in rebate selection
            _rebateStandardClauseNameDataSource.SelectParameters.Add(_userLoginParameter)

            _clauseTypeParameter = New Parameter("ClauseType", TypeCode.String)
            _rebateStandardClauseNameDataSource.SelectParameters.Add(_clauseTypeParameter)
            _clauseTypeParameter.DefaultValue = "A"  ' All

            ' save to session
            Session("StandardClauseNameDataSource") = _rebateStandardClauseNameDataSource

        Else
            _rebateStandardClauseNameDataSource = CType(Session("StandardClauseNameDataSource"), DocumentDataSource)

            'restore parameters ( not already restored )
            _clauseTypeParameter = _rebateStandardClauseNameDataSource.SelectParameters("ClauseType")

        End If
        ' clause names are bound during grid row binding

        ' retrieve the standard clauses in a dataset
        If Session("StandardClauseNameDataSet") Is Nothing Then

            contractDB = CType(Session("ContractDB"), ContractDB)

            bSuccess = contractDB.SelectStandardRebateTerms(_dsRebateStandardClauses, "A")

            If (bSuccess = True) Then
                Session("StandardClauseNameDataSet") = _dsRebateStandardClauses
            End If
        Else
            _dsRebateStandardClauses = CType(Session("StandardClauseNameDataSet"), DataSet)
        End If



        If IsPostBack = False Then
            SetRebateGridViewSelectedItem(0, True)
            rebateGridView.DataBind()
            UpdateRebateTextFromSelectedItem()
        Else
            postbackSourceName = CType(Session("PostbackSourceName"), String)
            If (postbackSourceName.CompareTo("BigUpdate") = 0) Then
                SetRebateGridViewSelectedItem(0, True)
                rebateGridView.DataBind()
                UpdateRebateTextFromSelectedItem()
            End If
            RestoreRebateGridViewSelectedItem()
        End If

        If (rebateGridView.HasData = False) Then
            ClearRebateText(True)
        End If

    End Sub

    Private Sub ClearSessionVariables()
        Session("RebateDataSource") = Nothing
        Session("RebateDateDataSource") = Nothing
        Session("StandardClauseNameDataSource") = Nothing
        Session("RebateGridViewSelectedIndex") = Nothing
        Session("LastUpdatedRebateId") = Nothing
        Session("LastInsertedRebateId") = Nothing
        Session("CustomRebateTextForCurrentRow") = Nothing
        Session("StandardClauseNameDataSet") = Nothing
        Session("CustomStartDateForCurrentRow") = Nothing
    End Sub
    ' disable add button and text boxes
    Protected Sub EnableDisableRebateEditControls()
        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim rebateRequiredCheckBox As CheckBox = CType(fvContractInfo.Row.FindControl("RebateRequiredCheckBox"), CheckBox)
        Dim bRebateRequired As Boolean = rebateRequiredCheckBox.Checked

        If Not currentDocument Is Nothing Then
            If (currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.RebateTerms) <> True Or bRebateRequired = False) Then

                Dim addRebateButton As Button = CType(fvContractInfo.Row.FindControl("AddRebateButton"), Button)

                If Not addRebateButton Is Nothing Then
                    addRebateButton.Enabled = False
                End If

            End If

            If (currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.RebateRequired) <> True) Then

                If Not rebateRequiredCheckBox Is Nothing Then
                    rebateRequiredCheckBox.Enabled = False
                End If
            End If
        End If
    End Sub

    Protected Sub RebateRequiredCheckBox_OnCheckedChanged(ByVal obj As Object, ByVal e As EventArgs)
        Dim rebateRequiredCheckBox As CheckBox = CType(obj, CheckBox)
        Dim bRebateRequired As Boolean = rebateRequiredCheckBox.Checked

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If Not currentDocument Is Nothing Then

            Dim addRebateButton As Button = CType(fvContractInfo.Row.FindControl("AddRebateButton"), Button)

            If (currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.RebateTerms) <> True Or bRebateRequired = False) Then

                If Not addRebateButton Is Nothing Then
                    addRebateButton.Enabled = False
                End If
            Else
                If Not addRebateButton Is Nothing Then
                    addRebateButton.Enabled = True
                End If

            End If
        End If

        'rebind grid to enable/disable edit controls
        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)
        rebateGridView.DataBind()
    End Sub

#End Region

    Protected Sub RebateGridView_RowDataBound(ByVal sender As Object, ByVal e As GridViewRowEventArgs)

        Try

            Dim gv As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(sender, VA.NAC.NACCMBrowser.BrowserObj.GridView)
            Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

            Dim rebateRequiredCheckBox As CheckBox = CType(fvContractInfo.FindControl("RebateRequiredCheckBox"), CheckBox)
            Dim bRebateRequired As Boolean = rebateRequiredCheckBox.Checked

            If (e.Row.RowType = DataControlRowType.DataRow) Then

                If (((e.Row.RowState And DataControlRowState.Edit) <> DataControlRowState.Edit) And
                    ((e.Row.RowState And DataControlRowState.Insert) <> DataControlRowState.Insert)) Then

                    Dim removeRebateButton As Button = Nothing
                    removeRebateButton = CType(e.Row.FindControl("RemoveRebateButton"), Button)

                    If Not removeRebateButton Is Nothing Then
                        removeRebateButton.OnClientClick = "presentConfirmationMessage('Permanently delete the selected rebate from this contract?');"
                    End If

                    If Not currentDocument Is Nothing Then
                        If (currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.RebateTerms) <> True Or bRebateRequired = False) Then
                            If Not removeRebateButton Is Nothing Then
                                removeRebateButton.Enabled = False
                            End If

                            Dim editButton As Button = Nothing
                            editButton = CType(e.Row.FindControl("EditButton"), Button)
                            If Not editButton Is Nothing Then
                                editButton.Enabled = False
                            End If

                            Dim saveButton As Button = Nothing
                            saveButton = CType(e.Row.FindControl("SaveButton"), Button)
                            If Not saveButton Is Nothing Then
                                saveButton.Enabled = False
                            End If
                        End If
                    End If
                Else

                    ' bind ddls during edit mode
                    Dim startYearQuarterDropDownList As DropDownList = CType(e.Row.FindControl("startYearQuarterDropDownList"), DropDownList)
                    startYearQuarterDropDownList.DataSource = _rebateDateDataSource
                    startYearQuarterDropDownList.DataBind()

                    Dim endYearQuarterDropDownList As DropDownList = CType(e.Row.FindControl("endYearQuarterDropDownList"), DropDownList)
                    endYearQuarterDropDownList.DataSource = _rebateDateDataSource
                    endYearQuarterDropDownList.DataBind()

                    Dim rebateClauseNameDropDownList As DropDownList = CType(e.Row.FindControl("rebateClauseNameDropDownList"), DropDownList)
                    rebateClauseNameDropDownList.DataSource = _rebateStandardClauseNameDataSource
                    rebateClauseNameDropDownList.DataBind()

                End If
            End If

        Catch ex As Exception

            Dim msg As String = ex.Message

        End Try


    End Sub


#Region "minimallyusedevents"



    Protected Sub Save_ButtonClick(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
    Protected Sub RebateGridView_RowCommand(ByVal sender As Object, ByVal e As GridViewCommandEventArgs)
    End Sub
    Protected Sub RebateGridView_RowEditing(ByVal sender As Object, ByVal e As GridViewEditEventArgs)
    End Sub
    Protected Sub RebateGridView_RowInserting(ByVal sender As Object, ByVal e As GridViewInsertEventArgs)
    End Sub
    Protected Sub RebateGridView_RowCancelingEdit(ByVal sender As Object, ByVal e As GridViewCancelEditEventArgs)
        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(sender, VA.NAC.NACCMBrowser.BrowserObj.GridView)
        Dim cancelIndex As Integer = e.RowIndex
        Dim bInserting As Boolean = rebateGridView.InsertRowActive
        If (bInserting = True) Then
            rebateGridView.InsertRowActive = False ' cancels insert ( if inserting )

            _withAddRebateParameter.DefaultValue = "False"
            rebateGridView.EditIndex = -1
            rebateGridView.DataBind()

            'enable appropriate buttons for the selected row
            SetEnabledRebateControlsDuringEdit(CType(sender, VA.NAC.NACCMBrowser.BrowserObj.GridView), e.RowIndex, True)

            EnableControlsForRebateEditMode(True)

            HighlightRebateRow(0)
        Else ' editing existing row

            rebateGridView.EditIndex = -1 ' cancels the edit
            rebateGridView.DataBind()

            'enable appropriate buttons for the selected row
            SetEnabledRebateControlsDuringEdit(CType(sender, VA.NAC.NACCMBrowser.BrowserObj.GridView), e.RowIndex, True)

            EnableControlsForRebateEditMode(True)

            HighlightRebateRow(cancelIndex)

        End If

    End Sub
    Protected Sub RebateGridView_RowDeleting(ByVal sender As Object, ByVal e As GridViewDeleteEventArgs)
    End Sub
    Protected Sub RebateGridView_RowUpdating(ByVal sender As Object, ByVal e As GridViewUpdateEventArgs)
    End Sub
    Protected Sub RebateGridView_OnRowCreated(ByVal sender As Object, ByVal e As GridViewRowEventArgs)
        ' hide id fields
        If (e.Row.Cells.Count > RebateTermIdFieldNumber) And (e.Row.Cells.Count > RebatesStandardRebateTermIdFieldNumber) Then
            e.Row.Cells(RebateTermIdFieldNumber).Visible = False
            e.Row.Cells(RebatesStandardRebateTermIdFieldNumber).Visible = False
        End If
    End Sub
    Protected Sub RebateGridView_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
    End Sub
    Protected Sub RebateGridView_PreRender(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
    Protected Sub RebateGridView_Init(ByVal sender As Object, ByVal e As EventArgs)
        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(sender, VA.NAC.NACCMBrowser.BrowserObj.GridView)

    End Sub
#End Region

    Protected Sub RebateGridView_ButtonCommand(ByVal sender As Object, ByVal e As CommandEventArgs)
        Dim selectedRebateId As Integer = -1
        Dim itemIndex As Integer = -1

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        If (e.CommandName.CompareTo("ViewRebateText") = 0) Then
            Dim commandArgument As String = e.CommandArgument
            Dim argumentList As String() = commandArgument.Split(New Char() {","c})

            itemIndex = CType(argumentList(0), Integer)
            selectedRebateId = CType(argumentList(1), Integer)

            Dim rebatePercentOfSalesString As String
            Dim rebateThresholdString As String
            Dim rebatePercentOfSales As Decimal
            Dim rebateThreshold As Decimal

            rebatePercentOfSalesString = CType(argumentList(2), String)
            rebateThresholdString = CType(argumentList(3), String)

            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            ViewRebateClauseForSelectedRebate(selectedRebateId, rebatePercentOfSales, rebateThreshold, False)
            ViewCustomDateForSelectedRebate(selectedRebateId, False)

            HighlightRebateRow(itemIndex)

        ElseIf (e.CommandName.CompareTo("EditRebate") = 0) Then

            Dim commandArgument As String = e.CommandArgument
            Dim argumentList As String() = commandArgument.Split(New Char() {","c})

            itemIndex = CType(argumentList(0), Integer)
            selectedRebateId = CType(argumentList(1), Integer)

            ' update clause text box at start of edit operation
            Dim rebatePercentOfSalesString As String
            Dim rebateThresholdString As String
            Dim rebatePercentOfSales As Decimal
            Dim rebateThreshold As Decimal

            rebatePercentOfSalesString = CType(argumentList(2), String)
            rebateThresholdString = CType(argumentList(3), String)

            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            ViewRebateClauseForSelectedRebate(selectedRebateId, rebatePercentOfSales, rebateThreshold, True)
            ViewCustomDateForSelectedRebate(selectedRebateId, True)

            HighlightRebateRow(itemIndex)

            InitiateEditModeForRebate(itemIndex)

        ElseIf (e.CommandName.CompareTo("SaveRebate") = 0) Then

            Dim commandArgument As String = e.CommandArgument
            Dim argumentList As String() = commandArgument.Split(New Char() {","c})

            itemIndex = CType(argumentList(0), Integer)
            selectedRebateId = CType(argumentList(1), Integer)

            Dim validationMessage As String = ""

            ' validate the item before saving
            Dim bIsItemOk As Boolean = ValidateRebateBeforeUpdate(rebateGridView, itemIndex, selectedRebateId, validationMessage)

            If (bIsItemOk = True) Then

                ' is this an insert or an update
                Dim newOrUpdatedRowIndex As Integer = -1

                If (rebateGridView.InsertRowActive = True) Then
                    newOrUpdatedRowIndex = InsertRebate(rebateGridView, itemIndex)
                Else

                    newOrUpdatedRowIndex = UpdateRebate(rebateGridView, itemIndex)
                End If

                UpdateRebateTextFromSelectedItem()  ' also updates custom date

                HighlightRebateRow(newOrUpdatedRowIndex)

            Else
                MsgBox.Alert(validationMessage)
            End If

        ElseIf (e.CommandName.CompareTo("Cancel") = 0) Then
            Dim commandArgument As String = e.CommandArgument
            Dim argumentList As String() = commandArgument.Split(New Char() {","c})

            itemIndex = CType(argumentList(0), Integer)
            selectedRebateId = CType(argumentList(1), Integer)

            Dim rebatePercentOfSalesString As String
            Dim rebateThresholdString As String
            Dim rebatePercentOfSales As Decimal
            Dim rebateThreshold As Decimal

            rebatePercentOfSalesString = CType(argumentList(2), String)
            rebateThresholdString = CType(argumentList(3), String)

            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            'restore the text of the clause from the database which the user may have edited before the cancel
            ViewRebateClauseForSelectedRebate(selectedRebateId, rebatePercentOfSales, rebateThreshold, False)
            ViewCustomDateForSelectedRebate(selectedRebateId, False)

            HighlightRebateRow(itemIndex)
        ElseIf (e.CommandName.CompareTo("RemoveRebate") = 0) Then
            Dim commandArgument As String = e.CommandArgument
            Dim argumentList As String() = commandArgument.Split(New Char() {","c})

            itemIndex = CType(argumentList(0), Integer)
            selectedRebateId = CType(argumentList(1), Integer)

            Dim bContinueWithDelete As Boolean = False

            bContinueWithDelete = GetConfirmationMessageResults()

            If (bContinueWithDelete = True) Then
                Dim newRowIndex As Integer = DeleteRebate(rebateGridView, itemIndex)

                Session("RebateGridViewSelectedIndex") = newRowIndex ' might be zero with one or fewer rows

                UpdateRebateTextFromSelectedItem() ' also updates custom date

                HighlightRebateRow(newRowIndex)

            End If
        End If
    End Sub

    Private Function GetConfirmationMessageResults() As Boolean
        Dim bConfirmationResults As Boolean = False
        Dim confirmationResultsString As String = ""

        '        Dim confirmationMessageResultsHiddenField As HtmlInputHidden = CType(fvContractInfo.Row.FindControl("confirmationMessageResults"), HtmlInputHidden)
        Dim confirmationMessageResultsHiddenField As HtmlInputHidden = CType(form1.FindControl("confirmationMessageResults"), HtmlInputHidden)
        If Not confirmationMessageResultsHiddenField Is Nothing Then
            confirmationResultsString = confirmationMessageResultsHiddenField.Value
            If confirmationResultsString.Contains("true") = True Then
                bConfirmationResults = True
                confirmationMessageResultsHiddenField.Value = "false"
            End If
        End If
        Return (bConfirmationResults)
    End Function

    Protected Sub AddNewRebateButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        rebateGridView.Insert()

        _withAddRebateParameter.DefaultValue = "true"

        rebateGridView.DataBind()

        InitiateEditModeForRebate(0)

        'init the rebate text
        ClearRebateText(False)
        ClearCustomDate(False)

        HighlightRebateRow(0)

    End Sub

    Private Sub InitiateEditModeForRebate(ByVal editIndex As Integer)

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        rebateGridView.EditIndex = editIndex

        ' clear the most recent custom rebate text
        Session("CustomRebateTextForCurrentRow") = Nothing
        Session("CustomStartDateForCurrentRow") = Nothing

        ' select the edited item also
        If (rebateGridView.InsertRowActive = True) Then

            SetRebateGridViewSelectedItem(editIndex, True) 'scroll to new row

        Else

            SetRebateGridViewSelectedItem(editIndex, False)

        End If

        rebateGridView.DataBind()

        ' disable appropriate buttons for the selected row
        SetEnabledRebateControlsDuringEdit(rebateGridView, editIndex, False)

        ' disable the non-edit controls before going into edit mode
        EnableControlsForRebateEditMode(False)

    End Sub

#Region "currentselectedrebaterow"


    Private Sub SetRebateGridViewSelectedItem(ByVal selectedItemIndex As Integer, ByVal bIncludeScroll As Boolean)

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        If (selectedItemIndex < rebateGridView.Rows.Count) Then

            ' save for postback
            Session("RebateGridViewSelectedIndex") = selectedItemIndex

            ' set the row as selected
            rebateGridView.SelectedIndex = selectedItemIndex

            ' tell the client
            If (bIncludeScroll = True) Then
                ScrollToSelectedItem()
            End If

            ' allow the update postback to occur
            'InsertItemButtonClickUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        End If

    End Sub

    Private Sub ScrollToSelectedItem()

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        Dim rowIndex As Integer = rebateGridView.SelectedIndex

        '   //  TableItemStyle rowStyle = DrugItemsGridView.RowStyle;
        '   //  int rowHeight = ( int )rowStyle.Height.Value;  // this value is always zero
        '     int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
        '     int rowPosition = ( ITEMGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( ITEMGRIDVIEWROWHEIGHTESTIMATE * fudge );

        '     string scrollToRowScript = String.Format( "setItemScrollOnChange( {0} );", rowPosition );
        '     ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        '//     ScriptManager.RegisterClientScriptBlock( this.Page, this.GetType(), "SelectedIndexChangedScript", scrollToRowScript, true );  // runs prior to control rendering
        ' }
    End Sub

    Protected Sub HighlightRebateRow(ByVal itemIndex As Integer)

        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        Dim highlightedRowOriginalColor As String = ""
        Dim highlightedRowIndex As Integer = itemIndex + 1

        If (rebateGridView.HasData = True) Then
            Dim row As GridViewRow = rebateGridView.Rows(itemIndex)

            If (row.RowState = DataControlRowState.Alternate) Then
                highlightedRowOriginalColor = rebateGridView.AlternatingRowStyle.BackColor.ToString()
            Else
                highlightedRowOriginalColor = rebateGridView.RowStyle.BackColor.ToString()
            End If

            '         string preserveHighlightingScript = String.Format( "setDrugItemHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
            '         ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", preserveHighlightingScript, true );

            '         // allow the highlight postback to occur 
            '         ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        End If
    End Sub

    Private Sub RestoreRebateGridViewSelectedItem()
        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)
        rebateGridView.SelectedIndex = CType(Session("RebateGridViewSelectedIndex"), Integer)
    End Sub

    Private Sub SetEnabledRebateControlsDuringEdit(ByRef gv As VA.NAC.NACCMBrowser.BrowserObj.GridView, ByVal rowIndex As Integer, ByVal bEnabled As Boolean)

        gv.SetEnabledControlsForCell(rowIndex, RebateViewTextButtonFieldNumber, bEnabled) ' view button
        gv.SetEnabledControlsForCell(rowIndex, RebateRemoveButtonFieldNumber, bEnabled) ' remove button

        gv.SetVisibleControlsForCell(rowIndex, RebateEditButtonFieldNumber, "EditButton", bEnabled)
        gv.SetVisibleControlsForCell(rowIndex, RebateEditButtonFieldNumber, "SaveButton", Not bEnabled)
        gv.SetVisibleControlsForCell(rowIndex, RebateEditButtonFieldNumber, "CancelButton", Not bEnabled)

        '  SetRebateClauseReadOnly(bEnabled)
    End Sub

    Private Sub SetRebateClauseReadOnly(ByVal bReadOnly As Boolean)
        Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), System.Web.UI.WebControls.TextBox)
        rebateClauseTextBox.ReadOnly = bReadOnly
    End Sub

    Private Sub SetCustomStartDateReadOnly(ByVal bReadOnly As Boolean)
        Dim customStartDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), TextBox)
        customStartDateTextBox.ReadOnly = bReadOnly
    End Sub
    ' disable non-edit controls before going into edit mode
    Private Sub EnableControlsForRebateEditMode(ByVal bEnabled As Boolean)
        Dim addRebateButton As Button = CType(fvContractInfo.Row.FindControl("AddRebateButton"), Button)

        If Not addRebateButton Is Nothing Then
            addRebateButton.Enabled = bEnabled
        End If

    End Sub

#End Region

    'called from InitRebateControls 
    Private Sub UpdateRebateTextFromSelectedItem()
        Dim rebateId As Integer
        Dim rebatePercentOfSales As Decimal = -1
        Dim rebateThreshold As Decimal = -1

        Dim rowIndex As Integer = Session("RebateGridViewSelectedIndex")
        Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

        If (rowIndex < rebateGridView.Rows.Count) Then

            rebateId = rebateGridView.GetRowIdFromSelectedIndex(rowIndex, 0).ToString()
            Dim rebatePercentOfSalesString As String = rebateGridView.GetStringValueFromSelectedIndexForTemplateField(rowIndex, "percentOfSalesLabel")
            Dim rebateThresholdString As String = rebateGridView.GetStringValueFromSelectedIndexForTemplateField(rowIndex, "rebateThresholdLabel")

            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            ' hit the database for clause and substitute values if required
            ViewRebateClauseForSelectedRebate(rebateId, rebatePercentOfSales, rebateThreshold, False)
            ViewCustomDateForSelectedRebate(rebateId, False)
        Else
            ClearRebateText(True)
            ClearCustomDate(True)
        End If
    End Sub
    'format and present rebate text
    Private Sub ViewRebateClauseForSelectedRebate(ByVal rebateId As Integer, ByVal rebatePercentOfSales As Decimal, ByVal rebateThreshold As Decimal, ByVal bIsEditMode As Boolean)

        Dim contractDB As ContractDB
        Dim bSuccess As Boolean
        Dim bIsCustom As Boolean
        Dim rebateClause As String

        contractDB = CType(Session("ContractDB"), ContractDB)

        bSuccess = contractDB.GetRebateClauseForRebate(rebateId, bIsCustom, rebateClause)

        Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), System.Web.UI.WebControls.TextBox)

        If bSuccess = True Then
            If Not rebateClauseTextBox Is Nothing Then
                If bIsCustom = True Then
                    rebateClauseTextBox.Text = rebateClause
                    'preserve the most recently selected custom clause
                    Session("CustomRebateTextForCurrentRow") = rebateClause
                    If (bIsEditMode = True) Then
                        SetRebateClauseReadOnly(False)
                    Else
                        SetRebateClauseReadOnly(True)
                    End If
                Else
                    rebateClauseTextBox.Text = FormatRebateClauseWithData(rebateClause, rebatePercentOfSales, rebateThreshold)
                    SetRebateClauseReadOnly(True)
                End If

            End If
        End If
    End Sub

    ' present custom date or clear it if not custom
    Private Sub ViewCustomDateForSelectedRebate(ByVal rebateId As Integer, ByVal bIsEditMode As Boolean)

        Dim contractDB As ContractDB
        Dim bSuccess As Boolean

        Dim customStartDate As DateTime

        contractDB = CType(Session("ContractDB"), ContractDB)

        ' returns min date if null date
        bSuccess = contractDB.GetCustomDateForRebate(rebateId, customStartDate)

        Dim customDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), System.Web.UI.WebControls.TextBox)

        If bSuccess = True Then
            If Not customDateTextBox Is Nothing Then
                ' not really a custom date
                If (customStartDate = DateTime.MinValue) Then
                    customDateTextBox.Text = ""
                    Session("CustomStartDateForCurrentRow") = Nothing
                Else
                    customDateTextBox.Text = customStartDate.ToString("d")
                    Session("CustomStartDateForCurrentRow") = customStartDate.ToString()
                End If

            End If
        End If

        If (bIsEditMode = True) Then
            SetCustomStartDateReadOnly(False)
        Else
            SetCustomStartDateReadOnly(True)
        End If

    End Sub

    Private Sub ClearRebateText(ByVal bReadOnly As Boolean)
        Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), System.Web.UI.WebControls.TextBox)
        rebateClauseTextBox.Text = ""
        SetRebateClauseReadOnly(bReadOnly)
    End Sub
    Private Sub ClearCustomDate(ByVal bReadOnly As Boolean)
        Dim customStartDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), TextBox)
        customStartDateTextBox.Text = ""
        SetCustomStartDateReadOnly(bReadOnly)
    End Sub

    'format the text to substitute the numeric values for any located placeholders
    '-1 indicates a value has not been provided by the user
    Private Function FormatRebateClauseWithData(ByVal rebateClause As String, ByVal rebatePercentOfSales As Decimal, ByVal rebateThreshold As Decimal) As String
        Dim formattedRebateClause As String = ""

        Dim percentPosition As Integer = -1
        Dim thresholdPosition As Integer = -1
        Dim endOfPercent As Integer = -1
        Dim endOfThreshold As Integer = -1

        Dim s1 As String = ""
        Dim s2 As String = ""
        Dim s3 As String = ""

        percentPosition = GetRebateTagPosition(rebateClause, RebatePercentTagString)

        'percent is required
        If (percentPosition >= 0) Then
            endOfPercent = percentPosition + (RebatePercentTagString.Length - 1)
            s1 = rebateClause.Substring(0, percentPosition)
            s2 = CType(rebatePercentOfSales, String)
            s3 = rebateClause.Substring(endOfPercent + 1, (rebateClause.Length - endOfPercent) - 1)
            formattedRebateClause = s1 + s2 + s3
        Else
            formattedRebateClause = rebateClause
        End If

        thresholdPosition = GetRebateTagPosition(formattedRebateClause, RebateThresholdTagString)

        'threshold is required
        If (thresholdPosition >= 0) Then
            endOfThreshold = thresholdPosition + (RebateThresholdTagString.Length - 1)
            s1 = formattedRebateClause.Substring(0, thresholdPosition)
            s2 = CType(rebateThreshold, String)
            s3 = formattedRebateClause.Substring(endOfThreshold + 1, (formattedRebateClause.Length - endOfThreshold) - 1)
            formattedRebateClause = s1 + s2 + s3
        End If

        Return (formattedRebateClause)
    End Function

    Private Function GetRebateTagPosition(ByVal rebateClause As String, ByVal tag As String) As Integer
        Return (rebateClause.IndexOf(tag))
    End Function

    Private Function ValidateRebateBeforeUpdate(ByRef rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView, ByVal itemIndex As Integer, ByVal selectedRebateId As Integer, ByRef validationMessage As String) As Boolean
        Dim bIsValid As Boolean = True
        validationMessage = ""

        Dim bSuccess As Boolean

        Dim startQuarterId As Integer
        Dim endQuarterId As Integer
        Dim rebatePercentOfSales As Decimal = -1
        Dim rebateThreshold As Decimal = -1
        Dim rebateClause As String = ""
        Dim bIsCustom As Boolean = False

        Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), System.Web.UI.WebControls.TextBox)

        Dim startQuarterIdString As String = rebateGridView.GetStringValueFromSelectedControl(itemIndex, RebateStartYearQuarterFieldNumber, 0, False, "startYearQuarterDropDownList")
        Dim endQuarterIdString As String = rebateGridView.GetStringValueFromSelectedControl(itemIndex, RebateEndYearQuarterFieldNumber, 0, False, "endYearQuarterDropDownList")
        Dim rebatePercentOfSalesString As String = rebateGridView.GetStringValueFromSelectedControl(itemIndex, RebatePercentOfSalesFieldNumber, 0, False, "percentOfSalesTextBox")
        Dim rebateThresholdString As String = rebateGridView.GetStringValueFromSelectedControl(itemIndex, RebateThresholdFieldNumber, 0, False, "rebateThresholdTextBox")

        ' rebate clause
        Dim selectedClauseId As String = rebateGridView.GetStringValueFromSelectedControl(itemIndex, RebatesStandardRebateTermIdFieldNumber, 0, False, "rebateClauseNameDropDownList")

        ' custom date
        Dim customDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), System.Web.UI.WebControls.TextBox)
        Dim customDateString As String = customDateTextBox.Text
        Dim customDate As DateTime

        ' standard
        If selectedClauseId <> -1 Then
            bIsCustom = False
            rebateClause = GetStandardRebateTermFromId(selectedClauseId)
        Else ' custom
            bIsCustom = True
            If Not rebateClauseTextBox Is Nothing Then
                rebateClause = rebateClauseTextBox.Text
            End If
        End If

        ' standard
        If bIsCustom = False Then
            ' determine which, if any numeric values are required by the standard clause
            Dim bIsPercentRequired As Boolean = False
            Dim bIsThresholdRequired As Boolean = False

            If (GetRebateTagPosition(rebateClause, RebatePercentTagString) >= 0) Then
                bIsPercentRequired = True
            End If

            If (GetRebateTagPosition(rebateClause, RebateThresholdTagString) >= 0) Then
                bIsThresholdRequired = True
            End If

            ' validate that the required numeric value(s) are present
            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            If (bIsPercentRequired = True And rebatePercentOfSales = -1) Then
                bIsValid = False
                validationMessage = "A value for percent is required for the selected standard rebate clause."
            End If

            If (bIsThresholdRequired = True And rebateThreshold = -1) Then
                bIsValid = False
                validationMessage = "A value for threshold is required for the selected standard rebate clause."
            End If

        Else 'custom
            ' validate that some custom text is present
            If (rebateClause.Length <= 0) Then
                bIsValid = False
                validationMessage = "A custom rebate clause must be entered when 'custom' has been selected as the rebate type."
            End If
        End If

        ' date validation
        If (bIsValid = True) Then

            Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
            Dim contractDB As ContractDB = CType(Session("ContractDB"), ContractDB)

            Dim startQuarterStartDate As DateTime
            Dim startQuarterEndDate As DateTime
            Dim endQuarterStartDate As DateTime
            Dim endQuarterEndDate As DateTime
            Dim yearQuarterDescription As String
            Dim fiscalYear As Integer
            Dim quarter As Integer
            Dim calendarYear As Integer
            Dim startDateOfContractAwardQuarter As DateTime
            Dim endDateOfContractExpirationQuarter As DateTime
            Dim testQuarterId As Integer
            Dim testStartQuarterStartDate As DateTime
            Dim testStartQuarterEndDate As DateTime
            Dim testEndQuarterStartDate As DateTime
            Dim testEndQuarterEndDate As DateTime

            startQuarterId = CType(startQuarterIdString, Integer)
            endQuarterId = CType(endQuarterIdString, Integer)

            ' custom date
            If (startQuarterId = -1) Then
                If (DateTime.TryParse(customDateString, customDate) <> True) Then
                    bIsValid = False
                    validationMessage = "A valid custom date must be entered when 'custom' has been selected as the start date.'"
                Else
                    startQuarterStartDate = customDate
                    endQuarterEndDate = DateAdd(DateInterval.Year, 1, customDate) ' one year later
                    endQuarterEndDate = DateAdd(DateInterval.Day, -1, endQuarterEndDate) ' minus one day
                End If
            Else ' standard date
                bSuccess = contractDB.GetYearQuarterInfo(startQuarterId, yearQuarterDescription, fiscalYear, quarter, startQuarterStartDate, startQuarterEndDate, calendarYear)
                bSuccess = contractDB.GetYearQuarterInfo(endQuarterId, yearQuarterDescription, fiscalYear, quarter, endQuarterStartDate, endQuarterEndDate, calendarYear)
            End If

            ' compare standard dates to each other
            If (startQuarterId <> -1) Then
                If (startQuarterId > endQuarterId) Then
                    bIsValid = False
                    validationMessage = "Start Year Quarter must precede End Year Quarter."
                End If
            End If

            ' validate against contract dates
            If (bIsValid = True) Then

                'note not caring about most of the return parms in this case
                bSuccess = contractDB.GetYearQuarterInfo(currentDocument.AwardDate, yearQuarterDescription, fiscalYear, quarter, testStartQuarterStartDate, testStartQuarterEndDate, calendarYear, testQuarterId)
                bSuccess = contractDB.GetYearQuarterInfo(currentDocument.ExpirationDate, yearQuarterDescription, fiscalYear, quarter, testEndQuarterStartDate, testEndQuarterEndDate, calendarYear, testQuarterId)

                If (Date.Compare(startQuarterStartDate, testStartQuarterStartDate) < 0 Or
                   Date.Compare(endQuarterEndDate, testStartQuarterStartDate) < 0 Or
                    Date.Compare(startQuarterStartDate, testEndQuarterEndDate) > 0 Or
                   Date.Compare(endQuarterEndDate, testEndQuarterEndDate) > 0) Then

                    bIsValid = False

                    If (startQuarterId = -1) Then
                        validationMessage = "Custom dates must fall within the contract dates."
                    Else
                        validationMessage = "Selected Year Quarters must fall within the contract dates."
                    End If

                End If

            End If

        End If

        Return (bIsValid)
    End Function


    Protected Sub rebateClauseNameDropDownList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim rebateClauseNameDropDownList As DropDownList = CType(sender, DropDownList)

        Dim selectedItem As ListItem

        selectedItem = rebateClauseNameDropDownList.SelectedItem

        Dim standardRebateTermId As Integer

        standardRebateTermId = selectedItem.Value

        Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), TextBox)

        Dim rebateClause As String

        ' if custom
        If standardRebateTermId = -1 Then
            If Not Session("CustomRebateTextForCurrentRow") Is Nothing Then
                rebateClauseTextBox.Text = CType(Session("CustomRebateTextForCurrentRow"), String)
                SetRebateClauseReadOnly(False)
            Else
                rebateClauseTextBox.Text = "Enter custom rebate text."
                SetRebateClauseReadOnly(False)
            End If

        Else ' standard
            rebateClause = GetStandardRebateTermFromId(standardRebateTermId)

            Dim rebatePercentOfSales As Decimal = -1
            Dim rebateThreshold As Decimal = -1

            Dim rowIndex As Integer = Session("RebateGridViewSelectedIndex")
            Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

            Dim rebatePercentOfSalesString As String = rebateGridView.GetStringValueFromSelectedIndexForTemplateField(rowIndex, "percentOfSalesTextBox")
            Dim rebateThresholdString As String = rebateGridView.GetStringValueFromSelectedIndexForTemplateField(rowIndex, "rebateThresholdTextBox")

            If (Decimal.TryParse(rebatePercentOfSalesString, rebatePercentOfSales) = False) Then
                rebatePercentOfSales = -1
            End If

            If (Decimal.TryParse(rebateThresholdString, rebateThreshold) = False) Then
                rebateThreshold = -1
            End If

            rebateClauseTextBox.Text = FormatRebateClauseWithData(rebateClause, rebatePercentOfSales, rebateThreshold)

            SetRebateClauseReadOnly(True)

        End If

    End Sub

    Private Function GetStandardRebateTermFromId(ByVal standardRebateTermId As Integer) As String
        Dim rebateClause As String = ""

        Dim t As DataTable = _dsRebateStandardClauses.Tables(ContractDB.StandardRebateTermsTableName)
        Dim keyObject As Object = CType(standardRebateTermId, Object)
        Dim row As DataRow = t.Rows.Find(keyObject)

        If Not row Is Nothing Then
            rebateClause = CType(row("RebateClause"), String)
        End If

        Return (rebateClause)
    End Function

    Protected Sub rebateClauseNameDropDownList_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim rebateClauseNameDropDownList As DropDownList = CType(sender, DropDownList)
        Dim gridViewRow As GridViewRow = CType(rebateClauseNameDropDownList.NamingContainer, GridViewRow)
        Dim rebateTermId As Integer = -1
        Dim bFoundStandardClause As Boolean = False
        Dim customListItem As ListItem = New ListItem

        If Not gridViewRow Is Nothing Then
            Dim dataRowView As DataRowView = CType(gridViewRow.DataItem, DataRowView)
            Dim rebateTermIdString As String = dataRowView("RebateTermId").ToString()

            If Not rebateTermIdString Is Nothing Then
                If (Integer.TryParse(rebateTermIdString, rebateTermId)) Then
                    Dim listItem As ListItem = rebateClauseNameDropDownList.Items.FindByValue(rebateTermId.ToString())
                    If Not listItem Is Nothing Then
                        listItem.Selected = True
                        bFoundStandardClause = True
                        SetRebateClauseReadOnly(True)
                    End If

                    'add a row for custom
                    customListItem.Text = "Custom"
                    customListItem.Value = -1
                    rebateClauseNameDropDownList.Items.Add(customListItem)

                    If bFoundStandardClause = False Then
                        customListItem.Selected = True
                        SetRebateClauseReadOnly(False)
                    End If

                End If
            End If
        End If
    End Sub

    ''called when the rebate type is selected for a row being edited or inserted
    'Private Sub ViewRebateClauseForSelectedRebateType(ByVal rebateClauseId As Integer, ByVal rebatePercentOfSales As Decimal, ByVal rebateThreshold As Decimal)

    '    Dim contractDB As ContractDB
    '    Dim bSuccess As Boolean
    '    Dim bIsCustom As Boolean
    '    Dim rebateClause As String

    '    contractDB = CType(Session("ContractDB"), ContractDB)

    '    'bSuccess = contractDB.GetRebateClauseById(rebateClauseId, bIsCustom, rebateClause)

    '    'Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), System.Web.UI.WebControls.TextBox)

    '    'If bSuccess = True Then
    '    '    If Not rebateClauseTextBox Is Nothing Then
    '    '        If bIsCustom = True Then
    '    '            rebateClauseTextBox.Text = rebateClause
    '    '        Else
    '    '            rebateClauseTextBox.Text = FormatRebateClauseWithData(rebateClause, rebatePercentOfSales, rebateThreshold)
    '    '        End If

    '    '    End If
    '    'End If
    'End Sub

    'Private Sub UpdateRebateClauseTextFromCustom()
    '    Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), TextBox)

    '    If Not Session("SelectedCustomRebateText") Is Nothing Then
    '        Dim customRebateText As String = CType(Session("SelectedCustomRebateText"), String)
    '        rebateClauseTextBox.Text = customRebateText
    '    End If

    'End Sub

    'Private Sub PreserveCustomRebateTextForCurrentRow(ByVal editIndex As Integer, ByVal bIsNew As Boolean)
    '    Dim rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView = CType(fvContractInfo.Row.FindControl("RebateGridView"), VA.NAC.NACCMBrowser.BrowserObj.GridView)

    '    Dim gridViewRow As GridViewRow = rebateGridView.Rows(editIndex)
    '    Dim dataRowView As DataRowView = CType(gridViewRow.DataItem, DataRowView)
    '    Dim standardClauseName As String = dataRowView("StandardClauseName").ToString()

    '    Session("CustomRebateTextForCurrentRow") = standardClauseName
    'End Sub

    Protected Sub RemoveRebateButton_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim removeRebateButton As Button = CType(sender, Button)
        If Not removeRebateButton Is Nothing Then
            MultiLineButtonText(removeRebateButton, New String() {"Remove", "Rebate"})
        End If
    End Sub
    Protected Sub ViewRebateTextButton_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim viewRebateButton As Button = CType(sender, Button)
        If Not viewRebateButton Is Nothing Then
            MultiLineButtonText(viewRebateButton, New String() {"View", "Rebate Clause"})
        End If
    End Sub
#Region "updateinsertparameters"

    Private Function InsertRebate(ByRef rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView, ByVal rowIndex As Integer) As Integer
        Dim insertedRowIndex As Integer = 0

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)

        _contractNumberParameter.DefaultValue = currentDocument.ContractNumber
        _userLoginParameter.DefaultValue = bs.UserInfo.LoginName

        ' may be -1 for custom
        Dim startQuarterId As Integer
        startQuarterId = CType(rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateStartYearQuarterFieldNumber, 0, False, "startYearQuarterDropDownList"), Integer)

        _startQuarterIdParameter.DefaultValue = startQuarterId
        _endQuarterIdParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateEndYearQuarterFieldNumber, 0, False, "endYearQuarterDropDownList")
        _rebatePercentOfSalesParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebatePercentOfSalesFieldNumber, 0, False, "percentOfSalesTextBox")
        _rebateThresholdParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateThresholdFieldNumber, 0, False, "rebateThresholdTextBox")

        ' rebate clause
        Dim selectedClauseId As String = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebatesStandardRebateTermIdFieldNumber, 0, False, "rebateClauseNameDropDownList")

        ' standard
        If selectedClauseId <> -1 Then
            _isCustomParameter.DefaultValue = "False"
            _standardRebateTermIdParameter.DefaultValue = selectedClauseId
            _rebateClauseParameter.DefaultValue = "custom" ' placeholder value not saved

        Else ' custom
            _isCustomParameter.DefaultValue = "True"
            _standardRebateTermIdParameter.DefaultValue = "-1" ' placeholder value not saved

            ' must save custom text from textbox
            Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), TextBox)
            _rebateClauseParameter.DefaultValue = rebateClauseTextBox.Text
        End If

        ' custom date
        If (startQuarterId = -1) Then
            Dim customStartDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), TextBox)
            _rebateCustomStartDateParameter.DefaultValue = customStartDateTextBox.Text
        End If


        ' not used
        _amountReceivedParameter.DefaultValue = "0"

        Try
            _rebateDataSource.Insert()
        Catch ex As Exception
            MsgBox.ShowError(ex)
        End Try

        rebateGridView.InsertRowActive = False ' done with insert
        rebateGridView.EditIndex = -1 ' done with edit
        _withAddRebateParameter.DefaultValue = "false"   ' no extra row
        rebateGridView.DataBind() '  bind with new row

        If Not Session("LastInsertedRebateId") Is Nothing Then
            Dim newRebateId As Integer = CType(Session("LastInsertedRebateId"), Integer)
            insertedRowIndex = rebateGridView.GetRowIndexFromId(newRebateId, 0)

            SetRebateGridViewSelectedItem(insertedRowIndex, False)

            'bind to select
            rebateGridView.DataBind()
        End If

        ' enable appropriate buttons for the selected row
        SetEnabledRebateControlsDuringEdit(rebateGridView, insertedRowIndex, True)

        EnableControlsForRebateEditMode(True)

        Return (insertedRowIndex)
    End Function


    Private Function UpdateRebate(ByRef rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView, ByVal rowIndex As Integer) As Integer
        Dim updatedRowIndex As Integer = -1

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)

        _contractNumberParameter.DefaultValue = currentDocument.ContractNumber
        _userLoginParameter.DefaultValue = bs.UserInfo.LoginName

        _rebateIdParameter.DefaultValue = rebateGridView.GetRowIdFromSelectedIndex(rowIndex, 0).ToString()

        ' may be -1 for custom
        Dim startQuarterId As Integer
        startQuarterId = CType(rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateStartYearQuarterFieldNumber, 0, False, "startYearQuarterDropDownList"), Integer)

        _startQuarterIdParameter.DefaultValue = startQuarterId
        _endQuarterIdParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateEndYearQuarterFieldNumber, 0, False, "endYearQuarterDropDownList")
        _rebatePercentOfSalesParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebatePercentOfSalesFieldNumber, 0, False, "percentOfSalesTextBox")
        _rebateThresholdParameter.DefaultValue = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebateThresholdFieldNumber, 0, False, "rebateThresholdTextBox")

        ' rebate clause
        Dim selectedClauseId As String = rebateGridView.GetStringValueFromSelectedControl(rowIndex, RebatesStandardRebateTermIdFieldNumber, 0, False, "rebateClauseNameDropDownList")

        ' standard
        If selectedClauseId <> -1 Then
            _isCustomParameter.DefaultValue = "False"
            _standardRebateTermIdParameter.DefaultValue = selectedClauseId
            _rebateClauseParameter.DefaultValue = "custom" ' placeholder value not saved

        Else ' custom
            _isCustomParameter.DefaultValue = "True"
            _standardRebateTermIdParameter.DefaultValue = "-1" ' placeholder value not saved

            ' must save custom text from textbox
            Dim rebateClauseTextBox As TextBox = CType(fvContractInfo.Row.FindControl("RebateClauseTextBox"), TextBox)
            _rebateClauseParameter.DefaultValue = rebateClauseTextBox.Text
        End If

        ' custom date
        If (startQuarterId = -1) Then
            Dim customStartDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), TextBox)
            _rebateCustomStartDateParameter.DefaultValue = customStartDateTextBox.Text
        End If

        ' not used
        _amountReceivedParameter.DefaultValue = "0"

        Try
            _rebateDataSource.Update()
        Catch ex As Exception
            MsgBox.ShowError(ex)
        End Try

        rebateGridView.EditIndex = -1 ' done with edit
        rebateGridView.DataBind()

        If Not Session("LastUpdatedRebateId") Is Nothing Then
            Dim lastUpdatedRebateId As Integer = CType(Session("LastUpdatedRebateId"), Integer)
            updatedRowIndex = rebateGridView.GetRowIndexFromId(lastUpdatedRebateId, 0)

            SetRebateGridViewSelectedItem(updatedRowIndex, False)

            'bind to select
            rebateGridView.DataBind()
        End If

        ' enable appropriate buttons for the selected row
        SetEnabledRebateControlsDuringEdit(rebateGridView, updatedRowIndex, True)

        EnableControlsForRebateEditMode(True)


        Return (updatedRowIndex)

    End Function
    Private Function DeleteRebate(ByRef rebateGridView As VA.NAC.NACCMBrowser.BrowserObj.GridView, ByVal rowIndex As Integer) As Integer
        Dim updatedRowIndex As Integer = -1

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
        Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)

        _contractNumberParameter.DefaultValue = currentDocument.ContractNumber
        _userLoginParameter.DefaultValue = bs.UserInfo.LoginName

        _rebateIdParameter.DefaultValue = rebateGridView.GetRowIdFromSelectedIndex(rowIndex, 0).ToString()


        Try
            _rebateDataSource.Delete()
        Catch ex As Exception
            MsgBox.ShowError(ex)
        End Try

        ' previous row gets focus
        If (rowIndex >= 1) Then
            updatedRowIndex = rowIndex - 1
        Else
            updatedRowIndex = rowIndex
        End If

        SetRebateGridViewSelectedItem(updatedRowIndex, False)

        'bind to select
        rebateGridView.DataBind()

        Return (updatedRowIndex)

    End Function
    Public Sub rebateDataSource_Updated(ByVal sender As Object, ByVal e As SqlDataSourceStatusEventArgs)
        Dim rebateIdString As String = e.Command.Parameters("@RebateId").Value.ToString()

        If rebateIdString.Length > 0 Then
            Dim rebateId As Integer
            rebateId = Integer.Parse(rebateIdString)
            Session("LastUpdatedRebateId") = rebateId
        End If
    End Sub

    Public Sub rebateDataSource_Inserted(ByVal sender As Object, ByVal e As SqlDataSourceStatusEventArgs)

        'If (e.Command.Parameters.Count > 0) Then
        '    For Each p As SqlParameter In e.Command.Parameters
        '        If Not p Is Nothing Then

        '            Debug.WriteLine("Parm 0= " & p.ToString() & " value= " & p.Value.ToString())

        '        End If


        '    Next


        'End If
        If Not e.Command.Parameters("@RebateId").Value Is Nothing Then

            Dim rebateIdString As String = e.Command.Parameters("@RebateId").Value.ToString()

            If rebateIdString.Length > 0 Then
                Dim rebateId As Integer
                rebateId = Integer.Parse(rebateIdString)
                Session("LastInsertedRebateId") = rebateId
            End If
        Else
            Dim insertException As Exception
            insertException = e.Exception

            If Not insertException Is Nothing Then
                Throw New Exception(String.Format("RebateId returned from insert was null. Insert failed. {0}", insertException.Message))
            Else
                Throw New Exception("RebateId returned from insert was null. Insert failed.")
            End If
        End If

    End Sub

    Private Sub CreateRebateDataSourceParameters()

        'select parms
        _contractNumberParameter = New Parameter("ContractNumber", TypeCode.String)
        _userLoginParameter = New Parameter("UserLogin", TypeCode.String)
        _withAddRebateParameter = New Parameter("WithAdd", TypeCode.Boolean)

        'update parms
        _standardRebateTermIdParameter = New Parameter("StandardRebateTermId", TypeCode.Int32)
        _rebateIdParameter = New Parameter("RebateId", TypeCode.Int32)
        _startQuarterIdParameter = New Parameter("StartQuarterId", TypeCode.Int32)
        _endQuarterIdParameter = New Parameter("EndQuarterId", TypeCode.Int32)
        _rebatePercentOfSalesParameter = New Parameter("RebatePercentOfSales", TypeCode.Decimal)
        _rebateThresholdParameter = New Parameter("RebateThreshold", TypeCode.Decimal)
        _amountReceivedParameter = New Parameter("AmountReceived", TypeCode.Decimal)
        _isCustomParameter = New Parameter("IsCustom", TypeCode.Boolean)
        _rebateClauseParameter = New Parameter("RebateClause", TypeCode.String)
        _rebateCustomStartDateParameter = New Parameter("CustomStartDate", TypeCode.String)

        _customRebateIdParameter = New Parameter("CustomRebateId", TypeCode.Int32)
        _customRebateIdParameter.Direction = ParameterDirection.Output
        _rebateTermIdParameter = New Parameter("RebateTermId", TypeCode.Int32)
        _rebateTermIdParameter.Direction = ParameterDirection.Output

        'insert parameters
        _rebateIdForInsertParameter = New Parameter("RebateId", TypeCode.Int32)
        _rebateIdForInsertParameter.Direction = ParameterDirection.Output


    End Sub

    Private Sub RestoreRebateDataSourceParameters(ByRef rebateDataSource As DocumentDataSource)

        'select parms
        _contractNumberParameter = rebateDataSource.SelectParameters("ContractNumber")
        _userLoginParameter = rebateDataSource.SelectParameters("UserLogin")
        _withAddRebateParameter = rebateDataSource.SelectParameters("WithAdd")

        'update parms
        _standardRebateTermIdParameter = rebateDataSource.UpdateParameters("StandardRebateTermId")
        _rebateIdParameter = rebateDataSource.UpdateParameters("RebateId")
        _startQuarterIdParameter = rebateDataSource.UpdateParameters("StartQuarterId")
        _endQuarterIdParameter = rebateDataSource.UpdateParameters("EndQuarterId")
        _rebatePercentOfSalesParameter = rebateDataSource.UpdateParameters("RebatePercentOfSales")
        _rebateThresholdParameter = rebateDataSource.UpdateParameters("RebateThreshold")
        _amountReceivedParameter = rebateDataSource.UpdateParameters("AmountReceived")
        _isCustomParameter = rebateDataSource.UpdateParameters("IsCustom")
        _rebateClauseParameter = rebateDataSource.UpdateParameters("RebateClause")
        _customRebateIdParameter = rebateDataSource.UpdateParameters("CustomRebateId")
        _rebateTermIdParameter = rebateDataSource.UpdateParameters("RebateTermId")
        _rebateCustomStartDateParameter = rebateDataSource.UpdateParameters("CustomStartDate")

        'insert parameters
        _rebateIdForInsertParameter = rebateDataSource.InsertParameters("RebateId")

    End Sub

#End Region

#Region "rebateDateSelection"

    Protected Sub startYearQuarterLabel_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim startYearQuarterLabel As Label = CType(sender, Label)

        Dim contractDB As ContractDB
        contractDB = CType(Session("ContractDB"), ContractDB)

        Dim yearQuarterDescription As String
        Dim fiscalYear As Integer
        Dim quarter As Integer
        Dim quarterStartDate As DateTime
        Dim quarterEndDate As DateTime
        Dim calendarYear As Integer

        If Not startYearQuarterLabel Is Nothing Then
            Dim gridViewRow As GridViewRow = CType(startYearQuarterLabel.NamingContainer, GridViewRow)
            If Not gridViewRow Is Nothing Then
                If Not gridViewRow.DataItem Is Nothing Then
                    Dim dataRowView As DataRowView = gridViewRow.DataItem
                    'use targetCell for formatting
                    'Dim targetCell As TableCell = gridViewRow.Cells(5)
                    Dim startQuarterIdString As String = dataRowView("StartQuarterId").ToString()
                    Dim startQuarterId As Integer = Integer.Parse(startQuarterIdString)

                    If (startQuarterId <> -1) Then
                        If (contractDB.GetYearQuarterInfo(Integer.Parse(startQuarterIdString), yearQuarterDescription, fiscalYear, quarter, quarterStartDate, quarterEndDate, calendarYear) = True) Then
                            startYearQuarterLabel.Text = yearQuarterDescription
                        End If
                    Else 'custom
                        startYearQuarterLabel.Text = "Custom"
                    End If
                End If

            End If
        End If

    End Sub

    Protected Sub startYearQuarterDropDownList_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim startYearQuarterDropDownList As DropDownList = CType(sender, DropDownList)
        Dim gridViewRow As GridViewRow = CType(startYearQuarterDropDownList.NamingContainer, GridViewRow)
        Dim startQuarterId As Integer = -1
        Dim bFoundStandardQuarter As Boolean = False
        Dim customListItem As ListItem = New ListItem

        If Not gridViewRow Is Nothing Then
            Dim dataRowView As DataRowView = CType(gridViewRow.DataItem, DataRowView)
            Dim startQuarterIdString As String = dataRowView("StartQuarterId").ToString()

            If Not startQuarterIdString Is Nothing Then
                If (Integer.TryParse(startQuarterIdString, startQuarterId)) Then
                    Dim listItem As ListItem = startYearQuarterDropDownList.Items.FindByValue(startQuarterId.ToString())
                    If Not listItem Is Nothing Then
                        listItem.Selected = True
                        bFoundStandardQuarter = True
                    End If

                    'add a row for custom
                    customListItem.Text = "Custom"
                    customListItem.Value = -1
                    startYearQuarterDropDownList.Items.Add(customListItem)

                    If bFoundStandardQuarter = False Then
                        customListItem.Selected = True
                        SetCustomStartDateReadOnly(False)
                    End If

                End If
            End If
        End If
    End Sub

    Protected Sub startYearQuarterDropDownList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim startYearQuarterDropDownList As DropDownList = CType(sender, DropDownList)
        Dim selectedItem As ListItem

        selectedItem = startYearQuarterDropDownList.SelectedItem

        Dim selectedQuarterId As Integer

        selectedQuarterId = selectedItem.Value

        Dim customStartDateTextBox As TextBox = CType(fvContractInfo.Row.FindControl("CustomStartDateTextBox"), TextBox)
        Dim customStartDate As String

        ' if custom
        If selectedQuarterId = -1 Then
            If Not Session("CustomStartDateForCurrentRow") Is Nothing Then
                customStartDateTextBox.Text = CType(Session("CustomStartDateForCurrentRow"), String)
                SetCustomStartDateReadOnly(False)
            Else
                customStartDateTextBox.Text = "Enter date."
                SetCustomStartDateReadOnly(False)
            End If

        Else ' standard
            ' push the custom date
            Session("CustomStartDateForCurrentRow") = customStartDateTextBox.Text
            ' clear 
            ClearCustomDate(True)
        End If

        Dim gridViewRow As GridViewRow = CType(startYearQuarterDropDownList.NamingContainer, GridViewRow)
        Dim endYearQuarterDropDownList As DropDownList = CType(gridViewRow.FindControl("endYearQuarterDropDownList"), DropDownList)
        selectedItem = endYearQuarterDropDownList.SelectedItem

        Dim selectedEndQuarterId As Integer

        selectedEndQuarterId = selectedItem.Value

        ' if NOT alredy custom 
        If (selectedEndQuarterId <> -1) Then
            ' unselect in other drop down
            Dim currentItem As ListItem = endYearQuarterDropDownList.SelectedItem
            If Not currentItem Is Nothing Then
                currentItem.Selected = False
            End If

            ' select custom in the other drop down also
            Dim listItem As ListItem = endYearQuarterDropDownList.Items.FindByValue("-1")
            If Not listItem Is Nothing Then
                listItem.Selected = True
            End If
        End If

    End Sub

    Protected Sub endYearQuarterLabel_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim endYearQuarterLabel As Label = CType(sender, Label)

        Dim contractDB As ContractDB
        contractDB = CType(Session("ContractDB"), ContractDB)

        Dim yearQuarterDescription As String
        Dim fiscalYear As Integer
        Dim quarter As Integer
        Dim quarterStartDate As DateTime
        Dim quarterEndDate As DateTime
        Dim calendarYear As Integer

        If Not endYearQuarterLabel Is Nothing Then
            Dim gridViewRow As GridViewRow = CType(endYearQuarterLabel.NamingContainer, GridViewRow)
            If Not gridViewRow Is Nothing Then
                If Not gridViewRow.DataItem Is Nothing Then
                    Dim dataRowView As DataRowView = gridViewRow.DataItem
                    'use targetCell for formatting
                    'Dim targetCell As TableCell = gridViewRow.Cells(5)
                    Dim endQuarterIdString As String = dataRowView("endQuarterId").ToString()
                    Dim endQuarterId As Integer = Integer.Parse(endQuarterIdString)

                    If (endQuarterId <> -1) Then
                        If (contractDB.GetYearQuarterInfo(endQuarterId, yearQuarterDescription, fiscalYear, quarter, quarterStartDate, quarterEndDate, calendarYear) = True) Then
                            endYearQuarterLabel.Text = yearQuarterDescription
                        End If
                    Else 'custom
                        endYearQuarterLabel.Text = "Custom"
                    End If

                End If
            End If
        End If
    End Sub

    Protected Sub endYearQuarterDropDownList_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim endYearQuarterDropDownList As DropDownList = CType(sender, DropDownList)
        Dim gridViewRow As GridViewRow = CType(endYearQuarterDropDownList.NamingContainer, GridViewRow)
        Dim endQuarterId As Integer = -1
        Dim bFoundStandardQuarter As Boolean = False
        Dim customListItem As ListItem = New ListItem

        If Not gridViewRow Is Nothing Then
            Dim dataRowView As DataRowView = CType(gridViewRow.DataItem, DataRowView)
            Dim endQuarterIdString As String = dataRowView("endQuarterId").ToString()

            If Not endQuarterIdString Is Nothing Then
                If (Integer.TryParse(endQuarterIdString, endQuarterId)) Then
                    Dim listItem As ListItem = endYearQuarterDropDownList.Items.FindByValue(endQuarterId.ToString())
                    If Not listItem Is Nothing Then
                        listItem.Selected = True
                        bFoundStandardQuarter = True
                    End If

                    'add a row for custom
                    customListItem.Text = "Custom"
                    customListItem.Value = -1
                    endYearQuarterDropDownList.Items.Add(customListItem)

                    If bFoundStandardQuarter = False Then
                        customListItem.Selected = True
                        SetCustomStartDateReadOnly(False)
                    End If

                End If
            End If
        End If
    End Sub

    Protected Sub endYearQuarterDropDownList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim endYearQuarterDropDownList As DropDownList = CType(sender, DropDownList)
        Dim selectedItem As ListItem

        Dim gridViewRow As GridViewRow = CType(endYearQuarterDropDownList.NamingContainer, GridViewRow)
        Dim startYearQuarterDropDownList As DropDownList = CType(gridViewRow.FindControl("startYearQuarterDropDownList"), DropDownList)

        selectedItem = endYearQuarterDropDownList.SelectedItem

        Dim selectedQuarterId As Integer

        selectedQuarterId = selectedItem.Value

        ' if custom 
        If (selectedQuarterId = -1) Then
            ' unselect in other drop down
            Dim currentItem As ListItem = startYearQuarterDropDownList.SelectedItem
            If Not currentItem Is Nothing Then
                currentItem.Selected = False
            End If
            ' select custom in the other drop down also
            Dim listItem As ListItem = startYearQuarterDropDownList.Items.FindByValue(selectedQuarterId.ToString())
            If Not listItem Is Nothing Then
                listItem.Selected = True
            End If
        End If

    End Sub

    Private Sub MultiLineButtonText(ByRef button As Button, ByRef buttonTextArray() As String)
        Dim sb As StringBuilder = New StringBuilder()

        For i As Integer = 0 To buttonTextArray.Count() - 1 ' was < count
            sb.AppendLine(buttonTextArray(i))
        Next

        button.Text = sb.ToString()
    End Sub
#End Region
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
