Imports System.Data
Imports System.Data.SqlClient
'Imports Excel = Microsoft.Office.Interop.Excel

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj
Imports VA.NAC.ReportManager

Imports GridView = System.Web.UI.WebControls.GridView
Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList


Partial Public Class NAC_CM_Contracts
    Inherits System.Web.UI.Page
    Protected myEditable As String
    Protected myScheduleNumber As Integer
    Protected myContractNumber As String

    Private Sub NAC_CM_Contracts_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Session("Requested") = Nothing
    End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Page.Focus()
        If Not IsPostBack Then
            Session("Requested") = "Contract"

            If Session("NACCM") Is Nothing Then
                Response.Redirect("Old1NCM.aspx")
            End If

            'Dim myrefer_page As String = ""
            'If Request.ServerVariables("HTTP_REFERER") Is Nothing Then
            '    If Session("NACCM") Is Nothing Then
            '        Response.Redirect("NCM.aspx")
            '    Else
            '        myrefer_page = "/contract_search.aspx"
            '    End If
            'Else
            '    myrefer_page = Request.ServerVariables("HTTP_REFERER").ToString
            'End If
            'Dim myIndex As Integer = myrefer_page.LastIndexOf("/") + 1
            'myrefer_page = myrefer_page.Substring(myIndex)
            'If Not myrefer_page.Contains("contract_search.aspx") Then
            '    If Not myrefer_page.Contains("offer_search.aspx") Then
            '        If Not myrefer_page.Contains("NAC_Offers") Then
            '            If Not myrefer_page.Contains("CreateContract") Then '("contract_addition") Then
            '                If Not myrefer_page.Contains("sales_entry") Then
            '                    Response.Redirect("NCM.aspx")
            '                End If
            '            End If
            '        End If
            '    End If
            'End If
            Dim myScheduleNumber As Integer = 0
            Dim myContractNumber As String = ""
            If Not Request.QueryString("SchNum") Is Nothing Then
                myScheduleNumber = CInt(Request.QueryString("SchNum"))
            End If
            If Not Request.QueryString("CntrctNum") Is Nothing Then
                myContractNumber = Request.QueryString("CntrctNum")
            End If

            Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
            ' deprecating this call in R2.2
            currentDocument = New CurrentDocument(-1, myContractNumber, myScheduleNumber, CType(Session("ContractDB"), ContractDB), CType(Session("DrugItemDB"), DrugItemDB), CType(Session("ItemDB"), ItemDB))
            Session("CurrentDocument") = currentDocument
            currentDocument.LookupCurrentDocument()

            Dim browserSecurity As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
            browserSecurity.SetDocumentEditStatus(currentDocument)

            Session("CurrentSelectedSBAPlanId") = Nothing
            'initialize dates used by date control
            Session("CAward") = Nothing
            Session("CEffective") = Nothing
            Session("CExpiration") = Nothing
            Session("CCompletion") = Nothing

            If currentDocument.EditStatus = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.EditStatuses.CanEdit Then

                If currentDocument.IsBPA(myScheduleNumber) Then
                    '  If myScheduleNumber = 15 Or myScheduleNumber = 39 Or myScheduleNumber = 41 Or myScheduleNumber = 44 Then
                    Response.Redirect("NAC_CM_BPA_Edit.aspx?CntrctNum=" & myContractNumber & "&SchNum=" & myScheduleNumber)
                Else
                    Response.Redirect("NAC_CM_Edit.aspx?CntrctNum=" & myContractNumber & "&SchNum=" & myScheduleNumber)
                End If
            End If

            setControls()
            setScheduleAttributes(myScheduleNumber)
            dataBPA()
            Dim view As MultiView = CType(fvContractInfo.Row.FindControl("itemView"), MultiView)
            If Not view Is Nothing Then
                view.ActiveViewIndex = 0
            End If
            dataContractOfficers()
            LoadAndSelectState("dlState")
            LoadAndSelectState("dlOrderState")
            'dataSIN(myContractNumber)
            ' dataexpriationDate()
            'dataEndDate()
            dataBusinessSize()
            dataVendorStatus()
            dataVendorType()
            dataGeoCoverage()
            dataReturnGoods()
            '  dataSales()
            setBtnAttributes()
            'setRowFormat()
            'dataSBAPlanTypes()
            'checkSalesEditable() took out function defn on 12/29/09

            'checkCheckContent()
        End If

        CMGlobals.AddKeepAlive(Me.Page, 30000)

        BindSBAControls(True, IsPostBack)

        setSBA294()
        SetSBAAddProjects()

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

        If IsPostBack Then

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
    End Sub
    Protected Sub fvContractInfo_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles fvContractInfo.PreRender
        setBtnAttributes()
        MakeTradeAgreementTableVisible()
        MakeExportViewPricelistButtonsVisible()
        DisableSBAEditControls()
        checkContractAssignment()
        MakeAssistantDirectorVisible()
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
    Protected Function checkIfEditable() As String
        Dim currentUser As String = CType(Session("UserName"), String)
        If currentUser.IndexOf("\") > 0 Then
            Dim myStart As Integer = currentUser.IndexOf("\") + 1
            currentUser = currentUser.Substring(myStart).ToUpper
        End If
        Dim myAD As String = ""
        Dim mySM As String = ""
        Dim myCO As String = ""
        Dim myD1 As String = ""
        Dim myD2 As String = ""
        Dim AD As HiddenField = CType(fvContractInfo.Row.FindControl("hfADUser"), HiddenField)
        Dim SM As HiddenField = CType(fvContractInfo.Row.FindControl("hfSMUser"), HiddenField)
        Dim CO As HiddenField = CType(fvContractInfo.Row.FindControl("hfCOUser"), HiddenField)
        Dim D1 As HiddenField = CType(fvContractInfo.Row.FindControl("hfData1"), HiddenField)
        Dim D2 As HiddenField = CType(fvContractInfo.Row.FindControl("hfData2"), HiddenField)
        If Not AD Is Nothing Then
            myAD = AD.Value.ToString.ToUpper
        End If
        If Not SM Is Nothing Then
            mySM = SM.Value.ToString.ToUpper
        End If
        If Not CO Is Nothing Then
            myCO = CO.Value.ToString.ToUpper
        End If
        If Not D1 Is Nothing Then
            myD1 = D1.Value.ToString.ToUpper
        End If
        If Not D2 Is Nothing Then
            myD2 = D2.Value.ToString.ToUpper
        End If
        If myAD.Contains(currentUser) Or mySM.Contains(currentUser) Or myCO.Contains(currentUser) Or myD1.Contains(currentUser) Or myD2.Contains(currentUser) Then
            myEditable = "Y"
        Else
            myEditable = "N"
        End If
        If Session("Admin").ToString = "YES" Then
            myEditable = "Y"
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
        Response.Write("<script>window.close();</script>")
    End Sub

    Private Sub btnMainMenu_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnMainMenu.Click
        Response.Redirect("Old1NCM.aspx")
    End Sub

    Private Sub btnContractSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnContractSearch.Click
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
            Dim myViewFSS As MultiView
            myViewFSS = CType(myFormView.Row.FindControl("mvBPAFSSDetail"), MultiView)
            myViewFSS.ActiveViewIndex = 0
        End If

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
            myExpiration = "1/1/1900"
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
    Protected Sub dataContractOfficers()

        Dim divisionId As String
        Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl("dlContractOfficers"), DropDownList)
        If Not myDropDownList Is Nothing Then
            Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)
            If (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.FSS) Then
                divisionId = "1"
            ElseIf (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.National) Then
                divisionId = "2"
            ElseIf (currentDocument.Division = VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument.Divisions.SAC) Then
                divisionId = "6"
            Else
                divisionId = "-1"
            End If

            Dim IsExpired As String = "0"
            If (currentDocument.ExpirationDate.CompareTo(DateTime.Today) < 0) Then
                IsExpired = "1"
            End If

            Dim strSQL As String = "exec [NACSEC].[dbo].SelectContractingOfficers3 " + divisionId + ", 0, 'F', " + IsExpired + " "
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand
            Dim rdr As SqlDataReader
            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                myDropDownList.DataTextField = "FullName"
                myDropDownList.DataValueField = "CO_ID"
                myDropDownList.DataSource = rdr
                myDropDownList.DataBind()
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            Dim myData As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
            Dim myCOID As String = myData.Item("CO_ID").ToString ' was 12

            Dim myIdInList As ListItem = myDropDownList.Items.FindByValue(myCOID) 'myDropDownList.SelectedValue = myCOID
            If Not myIdInList Is Nothing Then
                myIdInList.Selected = True
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
    'Protected Sub dataStateGeneral(ByVal stateBtn As String)
    '    Dim myDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl(stateBtn), DropDownList)
    '    Dim strSQL As String = "SELECT [Abbr], [State/Province], [Country] FROM tlkup_StateAbbr GROUP BY [Abbr], [State/Province], [Country] ORDER BY [State/Province]"
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

    Protected Sub LoadAndSelectState(ByVal stateDropDownListName As String)
        Dim stateDropDownList As DropDownList = CType(fvContractInfo.Row.FindControl(stateDropDownListName), DropDownList)

        Dim currentDataRow As DataRowView = CType(fvContractInfo.DataItem, DataRowView)
        Dim selectedAbbreviation As String = "--" 'default to none selected

        If stateDropDownList.ID = "dlState" Then
            selectedAbbreviation = currentDataRow.Item("Primary_State").ToString
        ElseIf stateDropDownList.ID = "dlOrderState" Then
            selectedAbbreviation = currentDataRow.Item("Ord_State").ToString
        End If

        LoadStates(selectedAbbreviation, stateDropDownList)
    End Sub
    Protected Sub LoadStates(ByVal selectedAbbreviation As String, ByRef stateDropDownList As DropDownList)

        Dim bSuccess As Boolean = True
        Dim dsStateCodes As DataSet = Nothing
        Dim contractDB As ContractDB

        If Not Session("StateCodeDataSet") Is Nothing Then
            dsStateCodes = CType(Session("StateCodeDataSet"), DataSet)
        Else
            contractDB = CType(Session("ContractDB"), ContractDB)

            '  bSuccess = contractDB.SelectStateCodes(dsStateCodes   $$$ took out to allow compilation
        End If

        If bSuccess = True Then
            stateDropDownList.ClearSelection()
            stateDropDownList.Items.Clear()
            stateDropDownList.DataSource = dsStateCodes
            stateDropDownList.DataMember = "StateCodesTable"
            stateDropDownList.DataTextField = "StateAbbreviation"
            stateDropDownList.DataValueField = "StateName"
            stateDropDownList.DataBind()

            stateDropDownList.SelectedItem.Text = selectedAbbreviation

            Session("StateCodeDataSet") = dsStateCodes

        Else
            MsgBox.Alert(contractDB.ErrorMessage)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the SIN into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Expiration Dates into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the End Dates into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Business Size into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Vet Status into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Vendor Type into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the Geographic Coverage into the dropdownlist." & ex.ToString)
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the returns good policy into the dropdownlist." & ex.ToString)
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
& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF" _
& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum," _
& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales " _
& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
& " join dbo.tbl_IFF on dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.Schedule_Number LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID jo" _
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
            Msg_Alert.Client_Alert_OnLoad("The follow exception was thrown loading the sales into a grid view." & ex.ToString)
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
        Dim strWindowFeatures As String = "window.open('pending_price_change.aspx?CntrctNum=" & myContractNumber & "&Edit=" & myEditable & "','Pending_Change','resizable=0,scrollbars=1,width=700,height=500,left=100,top=300')"
        Dim myContract As String = Request.QueryString("CntrctNum").ToString
        Dim myButton As Button
        Dim strWindow As String
        '  Dim myButton As Button = CType(fvContractInfo.Row.FindControl("SalesIFFCheckCompareButton"), Button)
        '   Dim strWindow As String = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=100,top=300')"
        '   myButton.Attributes.Add("onclick", strWindow)
        '   strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Full','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        '   myButton = CType(fvContractInfo.Row.FindControl("DetailedSalesHistoryButton"), Button)
        '   myButton.Attributes.Add("onclick", strWindow)
        '   strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Qtr','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        '   myButton = CType(fvContractInfo.Row.FindControl("QuarterlySalesHistoryButton"), Button)
        '   myButton.Attributes.Add("onclick", strWindow)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Year','','resizable=0,scrollbars=1,width=820,height=500,left=100,top=300')"
        '  myButton = CType(fvContractInfo.Row.FindControl("AnnualSalesHistoryButton"), Button)
        '  myButton.Attributes.Add("onclick", strWindow)
        myButton = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)
        myButton.Text = "ADD SALES" & vbNewLine & "FIGURES"
        myButton.ForeColor = Drawing.Color.Green
        '  myButton = CType(fvContractInfo.Row.FindControl("btnViewIFFCheck"), Button)
        '  strWindow = "window.open('check_compare.aspx?CntrctNum=" & myContract & "&Form=Check','','resizable=0,scrollbars=1,width=660,height=500,left=100,top=300')"
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

        Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
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
            'Dim exportMedSurgToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & myContractNumber & "&ExportType=M" & "','Pricelist','')"
            Dim exportMedSurgToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemExportUpload.aspx?ContractNumber=" & currentDocument.ContractNumber & "&ScheduleNumber=" & currentDocument.ScheduleNumber & "&ExportUploadType=M" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"

            btnExportPricelistToExcel.Attributes.Add("onclick", exportMedSurgToExcelWindowScript)
        End If




    End Sub
    'Protected Sub SetDrugItemExportParms(ByVal ContractNumber As String, ByVal ScheduleNumber As Integer, ByVal ExportType As String)
    '    Dim btnExportDrugItemPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)
    '    If Not btnExportDrugItemPricelistToExcel Is Nothing Then
    '        'old way $$$
    '        'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=280,left=320,width=340,height=160, resizable=0');"
    '        Dim exportDrugItemsToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemUpload.aspx?ContractNumber=" & ContractNumber & "&ScheduleNumber=" & ScheduleNumber.ToString() & "&ExportUploadType=P" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"
    '        btnExportDrugItemPricelistToExcel.Attributes.Add("onclick", exportDrugItemsToExcelWindowScript)
    '    End If
    'End Sub

    Protected Sub SetDrugItemExportParms(ByVal ContractNumber As String, ByVal ScheduleNumber As Integer)
        Dim btnExportDrugItemPricelistToExcel As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportDrugItemPricelistToExcel"), ImageButton)
        If Not btnExportDrugItemPricelistToExcel Is Nothing Then
            'old way $$$
            'Dim exportDrugItemsToExcelWindowScript As String = "window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & ContractNumber & "&ExportType=" & ExportType & "','Pricelist','toolbar=0,status=0,menubar=0,scrollbars=0,resizable=0,top=280,left=320,width=340,height=160, resizable=0');"
            Dim exportDrugItemsToExcelWindowScript As String = "window.open('/NACCMExportUpload/ItemExportUpload.aspx?ContractNumber=" & ContractNumber & "&ScheduleNumber=" & ScheduleNumber.ToString() & "&ExportUploadType=P" & "','ExportUpload','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=424,height=680,left=170,top=90,status=yes');"
            btnExportDrugItemPricelistToExcel.Attributes.Add("onclick", exportDrugItemsToExcelWindowScript)
        End If
    End Sub

    'Protected Sub DrugItemExportTypeRadioButtonList_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    '    Dim DrugItemExportTypeRadioButtonList As RadioButtonList
    '    Dim selectedExportTypeString As String
    '    Dim ExportType As String = "U"

    '    DrugItemExportTypeRadioButtonList = CType(sender, RadioButtonList)
    '    If Not DrugItemExportTypeRadioButtonList Is Nothing Then
    '        Dim selectedItem As ListItem
    '        selectedItem = DrugItemExportTypeRadioButtonList.SelectedItem
    '        If Not selectedItem Is Nothing Then

    '            selectedExportTypeString = DrugItemExportTypeRadioButtonList.SelectedItem.Value
    '            If (selectedExportTypeString.CompareTo("C") = 0) Then
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
        '  Dim dlSBAPlanName As DropDownList = CType(fvContractInfo.Row.FindControl("dlSBAPlanName"), DropDownList)
        '  If Not dlSBAPlanName Is Nothing Then
        '      ReloadSBAPlanList()
        '     dlSBAPlanName.DataBind()
        'fvContractInfo.DataBind()
        ' Response.Write("<script language='javascript'>window.reload();</script>")
        ' Response.Write("<script language='javascript'>__doPostBack("","");</script>")
        '  End If
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
    Protected Sub SBAOverallSaveButton_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim SBAPlanID As Integer = -1
        Dim SBAPlanExempt As Boolean = False
        Dim SelectedSBAPlanItem As ListItem

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim dlSBAPlanName As DropDownList
        dlSBAPlanName = CType(fvContractInfo.FindControl("dlSBAPlanName"), DropDownList)
        If Not dlSBAPlanName Is Nothing Then
            SelectedSBAPlanItem = dlSBAPlanName.SelectedItem
            If Not SelectedSBAPlanItem Is Nothing Then
                SBAPlanID = SelectedSBAPlanItem.Value
            End If
        End If

        Dim SBAPlanExemptCheckBox As CheckBox
        SBAPlanExemptCheckBox = CType(fvContractInfo.FindControl("SBAPlanExemptCheckBox"), CheckBox)
        If Not SBAPlanExemptCheckBox Is Nothing Then
            SBAPlanExempt = SBAPlanExemptCheckBox.Checked
        End If

        Dim bSuccess As Boolean = True
        Dim contractDB As ContractDB
        contractDB = CType(Session("ContractDB"), ContractDB)

        If Not contractDB Is Nothing Then
            Try
                bSuccess = contractDB.UpdateSBAPlanInfoInContract(currentDocument.ContractNumber, SBAPlanID, SBAPlanExempt)
                If bSuccess <> True Then
                    Msg_Alert.Client_Alert_OnLoad("The following error was encountered when attempting to save the selected plan and exempt status: " & contractDB.ErrorMessage)
                End If
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("The following error was encountered when attempting to save the selected plan and exempt status: " & ex.ToString())
            End Try
        Else
            Msg_Alert.Client_Alert_OnLoad("Could not find database object. Save not attempted. Session Timout. Please exit application and try again.")
        End If

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

            ' strSQL = "select CntrctNum, CO_Name from view_sba_contract_master where SBAPlanID = " & mySBAID & " "
            'Dim bSuccess As Boolean = True
            'Dim contractDB As ContractDB
            'Dim contractResponsibleForSBAPlan As String = ""
            'Dim contractResponsibleForSBAPlanCOName As String = ""
            'Dim contractResponsibleForSBAPlanCOID As Integer = -1

            'contractDB = CType(Session("ContractDB"), ContractDB)

            'bSuccess = contractDB.GetContractResponsibleForSBAPlan(mySBAID, contractResponsibleForSBAPlan, contractResponsibleForSBAPlanCOID, contractResponsibleForSBAPlanCOName)

            'If Not contractResponsibleFormView Is Nothing Then

            '    Dim lbSBACntrctResp As Label = CType(contractResponsibleFormView.FindControl("lbSBACntrctResp"), Label)
            '    Dim lbSBACOResp As Label = CType(contractResponsibleFormView.FindControl("lbSBACOResp"), Label)

            '    If bSuccess = True Then
            '        lbSBACntrctResp.Text = contractResponsibleForSBAPlan
            '        lbSBACOResp.Text = contractResponsibleForSBAPlanCOName
            '    Else
            '        contractResponsibleFormView.Visible = "False"
            '    End If

            'End If

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

        Dim SBAOverallSaveButton As Button
        SBAOverallSaveButton = CType(fvContractInfo.FindControl("SBAOverallSaveButton"), Button)
        If Not SBAOverallSaveButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.SBA) = True Then
                SBAOverallSaveButton.Enabled = True
            Else
                SBAOverallSaveButton.Enabled = False
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
    Protected Sub dlSBAPlanName_OnSelectedIndexChanged(ByVal s As Object, ByVal e As EventArgs)
        'BindSBAControls(False)
        'Dim dlSBAPlanName As DropDownList = CType(s, DropDownList)
        'Session("CurrentSelectedSBAPlanId") = dlSBAPlanName.SelectedItem.Value
        'dlSBAPlanName.DataBind()
    End Sub
    Protected Sub dlContractOfficers_OnSelectedIndexChanged(ByVal s As Object, ByVal e As EventArgs)
        Dim dlContractOfficers As Global.System.Web.UI.WebControls.DropDownList = CType(fvContractInfo.Row.FindControl("dlContractOfficers"), Global.System.Web.UI.WebControls.DropDownList)
        Dim newContractOwnerId As Integer
        newContractOwnerId = dlContractOfficers.SelectedItem.Value

        Dim bSuccess As Boolean = True
        Dim contractDB As ContractDB
        contractDB = CType(Session("ContractDB"), ContractDB)

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        Dim browserSecurity As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If Not contractDB Is Nothing And Not currentDocument Is Nothing And Not browserSecurity Is Nothing Then
            Try
                bSuccess = contractDB.UpdateOwnerInfoInContract(currentDocument.ContractNumber, newContractOwnerId)
                If bSuccess <> True Then
                    Msg_Alert.Client_Alert_OnLoad("The following error was encountered when attempting to change the selected contract owner: " & contractDB.ErrorMessage)
                Else
                    'update current document
                    currentDocument.OwnerId = newContractOwnerId
                    'readjust current document security
                    browserSecurity.SetDocumentEditStatus(currentDocument)
                End If
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("The following error was encountered when attempting to change the selected contract owner: " & ex.ToString())
            End Try
        Else
            Msg_Alert.Client_Alert_OnLoad("Could not find database (session) object(s). Save not attempted. Session Timout. Please exit application and try again.")
        End If
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
                Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Detail. " & ex.ToString)
            Finally
                conn.Close()
            End Try
            Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(myFormView.Row.FindControl("gvBPAFSSSales"), Global.System.Web.UI.WebControls.GridView)
            If Not myGridView Is Nothing Then
                'making this code the same as NAC_CM_BPA_Edit.aspx file for same section
                '                mySQLStr = "SELECT     dbo.View_Contract_Preview.CntrctNum, dbo.View_Sales_Variance_by_Year_A.Contract_Record_ID, " _
                '& "dbo.View_Sales_Variance_by_Year_A.Quarter_ID, dbo.tlkup_year_qtr.Title, dbo.tlkup_year_qtr.[Year], dbo.tlkup_year_qtr.Qtr, " _
                '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum, dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum," _
                '& "dbo.View_Sales_Variance_by_Year_A.Total_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum AS Previous_Qtr_VA_Sales_Sum, " _
                '& "dbo.View_Contract_Preview.Contractor_Name, " _
                '& "dbo.View_Contract_Preview.Schedule_Name, dbo.View_Contract_Preview.Schedule_Number, dbo.View_Contract_Preview.CO_Phone, " _
                '& "dbo.View_Contract_Preview.CO_Name, dbo.View_Contract_Preview.AD_Name, dbo.View_Contract_Preview.SM_Name, " _
                '& "dbo.View_Contract_Preview.CO_User, dbo.View_Contract_Preview.AD_User, dbo.View_Contract_Preview.SM_User, " _
                '& "dbo.View_Contract_Preview.Logon_Name, dbo.View_Contract_Preview.CO_ID, dbo.View_Contract_Preview.Dates_CntrctAward, " _
                '& "dbo.View_Contract_Preview.Dates_Effective, dbo.View_Contract_Preview.Dates_CntrctExp, dbo.View_Contract_Preview.Dates_Completion, " _
                '& "dbo.[tbl_IFF].VA_IFF, dbo.[tbl_IFF].OGA_IFF, dbo.[tbl_IFF].SLG_IFF," _
                '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF AS VA_IFF_Amount, " _
                '& "dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF AS OGA_IFF_Amount, " _
                '& "dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF AS SLG_IFF_Amount, " _
                '& "dbo.View_Sales_Variance_by_Year_A.VA_Sales_Sum * dbo.[tbl_IFF].VA_IFF + dbo.View_Sales_Variance_by_Year_A.OGA_Sales_Sum * dbo.[tbl_IFF].OGA_IFF + dbo.View_Sales_Variance_by_Year_A.SLG_Sales_Sum * dbo.[tbl_IFF].SLG_IFF " _
                '& "AS Total_IFF_Amount, dbo.View_Contract_Preview.Data_Entry_Full_1_UserName, dbo.View_Contract_Preview.Data_Entry_Full_2_UserName, " _
                '& "dbo.View_Contract_Preview.Data_Entry_Sales_1_UserName, dbo.View_Contract_Preview.Data_Entry_Sales_2_UserName, " _
                '& "View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum AS Previous_Qtr_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_SLG_Sales_Sum, View_Sales_Variance_by_Year_A_1.VA_Sales_Sum + View_Sales_Variance_by_Year_A_1.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_1.SLG_Sales_Sum AS Previous_Qtr_Total_Sales," _
                '& "View_Sales_Variance_by_Year_A_2.VA_Sales_Sum AS Previous_Year_VA_Sales_Sum, View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum AS Previous_Year_OGA_Sales_Sum, View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_SLG_Sales_Sum, " _
                '& " View_Sales_Variance_by_Year_A_2.VA_Sales_Sum + View_Sales_Variance_by_Year_A_2.OGA_Sales_Sum + View_Sales_Variance_by_Year_A_2.SLG_Sales_Sum AS Previous_Year_Total_Sales " _
                '& "FROM  dbo.[tlkup_Sched/Cat] INNER JOIN " _
                '& "dbo.View_Contract_Preview ON dbo.[tlkup_Sched/Cat].Schedule_Number = dbo.View_Contract_Preview.Schedule_Number " _
                '& " join dbo.tbl_IFF on dbo.View_Contract_Preview.Schedule_Number = dbo.tbl_IFF.Schedule_Number LEFT OUTER JOIN " _
                '& "dbo.View_Sales_Variance_by_Year_A INNER JOIN " _
                '& "dbo.tlkup_year_qtr ON dbo.View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tlkup_year_qtr.Quarter_ID ON " _
                '& "dbo.View_Contract_Preview.CntrctNum = dbo.View_Sales_Variance_by_Year_A.CntrctNum LEFT OUTER JOIN " _
                '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_1 ON " _
                '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_1.CntrctNum AND " _
                '& "dbo.View_Sales_Variance_by_Year_A.[Previous Qtr] = View_Sales_Variance_by_Year_A_1.Quarter_ID LEFT OUTER JOIN " _
                '& "dbo.View_Sales_Variance_by_Year_A View_Sales_Variance_by_Year_A_2 ON " _
                '& "dbo.View_Sales_Variance_by_Year_A.CntrctNum = View_Sales_Variance_by_Year_A_2.CntrctNum AND " _
                '& "dbo.View_Sales_Variance_by_Year_A.[Previous Year] = View_Sales_Variance_by_Year_A_2.Quarter_ID " _
                '& "WHERE (View_Contract_Preview.CntrctNum = '" & myFSSContractNum.ToString & "')" _
                '& " AND View_Sales_Variance_by_Year_A.Quarter_ID is not null " _
                '& " AND View_Sales_Variance_by_Year_A.Quarter_ID = dbo.tbl_IFF.start_quarter_id " _
                '& " ORDER BY dbo.View_Contract_Preview.CntrctNum DESC, dbo.View_Sales_Variance_by_Year_A.Quarter_ID DESC"

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
                    Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Sales Detail. " & ex.ToString)
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
                    Msg_Alert.Client_Alert_OnLoad("This is error at BPA FSS Checks Detail. " & ex.ToString)
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
            If Not myFSSCOntract Is Nothing Or myFSSCOntract.Value = "" Then
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
                    Msg_Alert.Client_Alert_OnLoad("This is error at FSS Detail. " & ex.ToString)

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
                myButton = CType(fvContractInfo.Row.FindControl("btnPrice"), Button)
                myButton.Visible = "False"
                myButton = CType(fvContractInfo.Row.FindControl("btnChecks"), Button)
                myButton.Visible = "False"
                Dim myTable As Table = CType(fvContractInfo.Row.FindControl("tblVendorMail"), Table)
                If Not myTable Is Nothing Then
                    myTable.Visible = "false"
                End If
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
        If myScheduleNumber = 10 Then
            Dim myTable As Table = CType(fvContractInfo.Row.FindControl("tblInsurance"), Table)
            If Not myTable Is Nothing Then
                myTable.Visible = "True"
            End If
        End If
        If myScheduleNumber = 36 Then
            Dim myTable As Table = CType(fvContractInfo.Row.FindControl("tblInsurance"), Table)
            If Not myTable Is Nothing Then
                myTable.Visible = "True"
            End If
            myTable = CType(fvContractInfo.Row.FindControl("tblPrimeVendor"), Table)
            If Not myTable Is Nothing Then
                myTable.Visible = "false"
            End If
            'Dim myButton As ImageButton = CType(fvContractInfo.Row.FindControl("btnExportPricelistToExcel"), ImageButton)
            'If Not myButton Is Nothing Then
            '    myButton.Visible = "False"
            'End If
            'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl("lbExportPriceList"), Label)
            'If Not myLabel Is Nothing Then
            '    myLabel.Visible = "False"
            'End If

        End If
    End Sub
    Protected Sub setControls()
        Dim myGridView As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSINS"), Global.System.Web.UI.WebControls.GridView)
        Dim myPanel As Panel = CType(fvContractInfo.Row.FindControl("pnGVSINS"), Panel)
        Dim myRemoveBtn As Button = CType(fvContractInfo.Row.FindControl("btnRemove"), Button)
        If myGridView Is Nothing Then
            If Not myPanel Is Nothing Then
                myPanel.Visible = "False"
            End If
            If Not myRemoveBtn Is Nothing Then
                myRemoveBtn.Visible = "False"
            End If
        Else
            If myGridView.Rows.Count > 0 Then
                If Not myPanel Is Nothing Then
                    myPanel.Visible = "True"
                End If
                If Not myRemoveBtn Is Nothing Then
                    myRemoveBtn.Visible = "False"
                End If
            Else
                If Not myPanel Is Nothing Then
                    myPanel.Visible = "False"
                End If
                If Not myRemoveBtn Is Nothing Then
                    myRemoveBtn.Visible = "False"
                End If

            End If
        End If
    End Sub
    Protected Sub grid_Command(ByVal s As Object, ByVal e As GridViewCommandEventArgs)
        Dim mySender As String = ""
        Dim myIndex As Integer = CType(e.CommandArgument, Integer)
        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(s, Global.System.Web.UI.WebControls.GridView)
        Dim mydataRow As GridViewRow = myGrid.Rows(myIndex)
        Dim mydataKey As DataKey = myGrid.DataKeys(myIndex)
        Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
        If e.CommandName = "Detail" Then
            Dim myScript As String = "<script>window.open('sales_detail.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Details','toolbar=no,menubar=no,resizable=no,width=865,height=300')</script>"
            Response.Write(myScript)
        End If
        If e.CommandName = "Update_Data" Then
            Dim myScript As String = "<script>window.open('sales_detail_edit.aspx?CntrctNum=" & myCntrctNum & "&QtrID=" & mydataKey.Value & "','Edit','toolbar=no,menubar=no,resizable=no,width=300,height=300,scrollbars=yes')</script>"
            Response.Write(myScript)
        End If
    End Sub
    Protected Sub refreshSalesDate(ByVal s As Object, ByVal e As EventArgs)
        Dim myValue As String = hfRefreshSales.Value.ToString
    End Sub

    ' check sales editable using current document settings
    Protected Sub checkSalesEditable2()
        Dim salesGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvSales"), Global.System.Web.UI.WebControls.GridView)
        Dim addSalesButton As Button = CType(fvContractInfo.Row.FindControl("btnAddSales"), Button)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.Sales) = True Then
            salesGrid.Columns(14).Visible = True ' was 11
            addSalesButton.Visible = True
        Else
            salesGrid.Columns(14).Visible = False
            addSalesButton.Visible = False
        End If

    End Sub
    Protected Sub openSalesEntry(ByVal s As Object, ByVal e As EventArgs)
        Dim myCntrctNum As String = Request.QueryString("cntrctNum").ToString
        Dim myScript As String = "<script>window.open('sales_entry.aspx?CntrctNum=" & myCntrctNum & "&Page=2&SchNum=" & Request.QueryString("SchNum").ToString & "','Details','toolbar=no,menubar=no,resizable=1,width=865,height=400')</script>"
        Response.Write(myScript)
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


    'Protected Sub ShowCheckFooter(ByVal s As Object, ByVal e As System.EventArgs)
    '    If isFinance2() Then
    '        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '        Dim myButton As Button = CType(s, Button)
    '        If myButton.ID.Equals("btnShowFooter") Then
    '            myGrid.ShowFooter = "True"
    '            myGrid.Columns(6).Visible = "True"
    '        ElseIf myButton.ID.Equals("btnHideFooter") Then
    '            myGrid.ShowFooter = "False"
    '        End If
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
                insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = BrowserSecurity.UserInfo.LoginName
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
    '    If isFinance2() Then
    '        Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(fvContractInfo.Row.FindControl("gvChecks"), Global.System.Web.UI.WebControls.GridView)
    '        myGrid.Columns(6).Visible = "True"
    '    Else
    '        Msg_Alert.Client_Alert_OnLoad("You must be idenified as a member of finance to enter check info.  See the system admin.")
    '    End If
    'End Sub
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
    'Protected Sub excel_Click(ByVal s As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
    '    Dim myCntrctNum As String = Request.QueryString("CntrctNum").ToString
    '    'Dim myTitle As String = "PriceList_" & myContractNumber & "_" & Date.Now.Month.ToString & Date.Now.Day.ToString & Date.Now.Year.ToString & Date.Now.Hour.ToString & Date.Now.Minute.ToString & ".xls"
    '    ' open_Excel(myCntrctNum, myTitle)
    '    Response.Write("<script>window.open('/PricelistExport/pricelist_excel.aspx?CntrctNum=" & myCntrctNum & "','Details','toolbar=no,menubar=no,resizable=no,width=805,height=450')</script>")
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
            '    DrugItemExportTypeRadioButtonList.Visible = False
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
    Private Sub NAC_CM_Contracts_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        checkSalesEditable2()
        checkChecksEditable()
        RestoreSelectedSBAPlanId()

        If Not Session("CurrentSelectedSBAPlanId") Is Nothing Then
            LoadSBAPlanContractResponsible(CType(Session("CurrentSelectedSBAPlanId"), Integer))
        End If

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
