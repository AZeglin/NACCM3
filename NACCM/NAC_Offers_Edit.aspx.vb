Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Imports System.Runtime.Serialization

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj

Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList


Partial Public Class NAC_Offers_Edit
    Inherits System.Web.UI.Page
    Dim myScheduleNumber As String

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        If Not IsPostBack Then

            If Session("NACCM") Is Nothing Then
                Response.Redirect("Old1NCM.aspx")
            End If

            'NAC_Offers_Edit.aspx?OfferID=" & offerId & "&scheduleNumber=" & scheduleNumber & "&COID=" & COID & "&vendor=" & vendor & "&contractNumber=" & contractNumber & "&receivedDate=" & receivedDate & "&assignedDate=" & assignedDate & "&completeString=" & completeString, "OfferDetails"
            Dim selectedOfferId As Integer
            Dim scheduleNumber As Integer
            Dim contractorName As String
            Dim contractNumber As String = ""
            Dim receivedDate As DateTime
            Dim assignmentDate As DateTime
            Dim assignedContractingOfficerId As Integer
            Dim bComplete As Boolean

            If Not Request.QueryString("OfferID") Is Nothing Then
                selectedOfferId = Request.QueryString("OfferID")
            End If
            If Not Request.QueryString("scheduleNumber") Is Nothing Then
                scheduleNumber = Request.QueryString("scheduleNumber")
            End If
            If Not Request.QueryString("COID") Is Nothing Then
                assignedContractingOfficerId = Request.QueryString("COID")
            End If
            If Not Request.QueryString("vendor") Is Nothing Then
                contractorName = Request.QueryString("vendor")
                contractorName = CMGlobals.RestoreQuote(contractorName, "^")
            End If
            If Not Request.QueryString("contractNumber") Is Nothing Then
                contractNumber = Request.QueryString("contractNumber")
            End If
            If Not Request.QueryString("receivedDate") Is Nothing Then
                receivedDate = Request.QueryString("receivedDate")
            End If
            If Not Request.QueryString("assignedDate") Is Nothing Then
                assignmentDate = Request.QueryString("assignedDate")
                Session("OfferAssignmentDateWasInitiallyBlank") = False
            Else
                Session("OfferAssignmentDateWasInitiallyBlank") = True
            End If
            If Not Request.QueryString("completeString") Is Nothing Then
                bComplete = Boolean.Parse(Request.QueryString("completeString"))
            End If

            Dim currentDocument As CurrentDocument
            currentDocument = New CurrentDocument(selectedOfferId, scheduleNumber, contractorName, receivedDate, assignmentDate, assignedContractingOfficerId, contractNumber, bComplete, CType(Session("OfferDB"), OfferDB), CType(Session("ContractDB"), ContractDB), CType(Session("DrugItemDB"), DrugItemDB), CType(Session("ItemDB"), ItemDB))
            Session("CurrentDocument") = currentDocument

            Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
            bs.SetDocumentEditStatus(currentDocument)
        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Page.Focus()

        AssignOfferButtonEvent()
        AssignOfferButtonText()

        'Dim openOrCreateContractButton As Button
        'openOrCreateContractButton = CType(fvOfferRecord.Row.FindControl("OpenOrCreateContractButton"), Button)

        'Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        '' open existing contract
        'If currentDocument.ContractNumber.Length > 0 Then
        '    openOrCreateContractButton.Text = "Open Contract" & vbCrLf & "Record"
        'Else
        '    openOrCreateContractButton.Text = " Enter Award" & vbCrLf & "Information"
        'End If

        'If Not IsPostBack Then

        '    Dim myContractNumber As HiddenField = CType(fvOfferRecord.Row.FindControl("hfContractNum"), HiddenField)
        '    If Not myContractNumber Is Nothing Then
        '        If myContractNumber.Value <> "" Then
        '            myButton = CType(fvOfferRecord.Row.FindControl("btnAddAward"), Button)
        '            If Not myButton Is Nothing Then
        '                myButton.Visible = "False"
        '            End If
        '            Dim myTable As Table = CType(fvOfferRecord.Row.FindControl("tblContractInfo"), Table)
        '            If Not myTable Is Nothing Then
        '                myTable.Visible = "True"
        '                myButton = CType(fvOfferRecord.Row.FindControl("btnOpenContract"), Button)
        '                If Not myButton Is Nothing Then
        '                    myButton.Visible = "True"
        '                End If
        '            End If
        '        End If
        '    End If
        'End If

        If IsPostBack Then
            Dim refreshDateType As String = ""
            Dim refreshOrNot As Boolean = False

            Dim refreshDateValueOnSubmitHiddenField As HiddenField = CType(fvOfferRecord.FindControl("RefreshDateValueOnSubmit"), HiddenField)
            Dim refreshOrNotOnSubmitHiddenField As HiddenField = CType(fvOfferRecord.FindControl("RefreshOrNotOnSubmit"), HiddenField)

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

    End Sub

    Private Sub AssignOfferButtonEvent()

        Dim openOrCreateContractButton As Button
        openOrCreateContractButton = CType(fvOfferRecord.Row.FindControl("OpenOrCreateContractButton"), Button)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        ' open existing contract
        If currentDocument.ContractNumber.Length > 0 Then
            ' openOrCreateContractButton.Attributes.Add("OnClick", "OpenContract")
            ' AddHandler openOrCreateContractButton.Click, AddressOf OpenContract
            openOrCreateContractButton.CommandArgument = "fred"
            openOrCreateContractButton.CommandName = "Open"
            AddHandler openOrCreateContractButton.Command, AddressOf OpenContract
        Else
            'openOrCreateContractButton.Attributes.Add("OnClick", "CreateContract")
            'AddHandler openOrCreateContractButton.Click, AddressOf CreateContract
            openOrCreateContractButton.CommandArgument = "fred"
            openOrCreateContractButton.CommandName = "Open"
            AddHandler openOrCreateContractButton.Command, AddressOf CreateContract

        End If

    End Sub
    Private Sub AssignOfferButtonText()

        Dim openOrCreateContractButton As Button
        openOrCreateContractButton = CType(fvOfferRecord.Row.FindControl("OpenOrCreateContractButton"), Button)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        ' open existing contract
        If currentDocument.ContractNumber.Length > 0 Then
            openOrCreateContractButton.Text = "Open Contract" & vbCrLf & "Record"
        Else
            openOrCreateContractButton.Text = " Enter Award" & vbCrLf & "Information"
        End If

    End Sub
    Private Sub btnExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExit.Click
        Response.Write("<script>window.close();</script>")
    End Sub
    Protected Sub SaveOfferButton_OnClick(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim AssignmentDateDropDownList As DropDownList = CType(fvOfferRecord.FindControl("dlAssignmentDate"), DropDownList)
        Dim DateReceivedTextBox As System.Web.UI.WebControls.TextBox = CType(fvOfferRecord.FindControl("tbReceived"), System.Web.UI.WebControls.TextBox)

        Try
            If DateReceivedTextBox.Text.Length <= 0 Then
                Throw New Exception("Received date cannot be null.")
            End If

            If AssignmentDateDropDownList.SelectedValue.Length > 0 Then
                Dim receivedDate As DateTime = CType(DateReceivedTextBox.Text, DateTime)
                Dim assignmentDate As DateTime = CType(AssignmentDateDropDownList.SelectedValue, DateTime)

                If DateTime.Compare(receivedDate, assignmentDate) > 0 Then
                    Throw New Exception("Assignment date must be the same as or after received date.")
                End If
            End If

        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("Offer save failed. " & ex.Message)
        End Try
    End Sub

    Protected Sub OpenContract(ByVal s As Object, ByVal e As System.Web.UI.WebControls.CommandEventArgs)
        'Dim myContractNumber As HiddenField = CType(fvOfferRecord.Row.FindControl("hfContractNum"), HiddenField)
        'Dim myScheduleNumber As HiddenField = CType(fvOfferRecord.Row.FindControl("hfScheduleNumber"), HiddenField)
        'Dim mySchNum As String = ""
        'If Not myScheduleNumber Is Nothing Then
        '    mySchNum = myScheduleNumber.Value.ToString
        'End If
        'If Not myContractNumber Is Nothing Then
        '    If myContractNumber.Value.ToString <> "" Then
        Dim currentOfferDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
        currentOfferDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If Not currentOfferDocument.ContractNumber Is Nothing Then
            If currentOfferDocument.ContractNumber.Length > 0 Then

                'add new CurrentDocument
                Dim currentDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
                ' depricated in release 2.2
                currentDocument = New CurrentDocument(-1, currentOfferDocument.ContractNumber, currentOfferDocument.ScheduleNumber, CType(Session("ContractDB"), ContractDB), CType(Session("DrugItemDB"), DrugItemDB), CType(Session("ItemDB"), ItemDB))

                'since contract details are not available, get them
                currentDocument.LookupCurrentDocument()
                Session("CurrentDocument") = currentDocument

                Dim browserSecurity As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
                browserSecurity.SetDocumentEditStatus(currentDocument)

                Response.Redirect("NAC_CM_Contracts.aspx?CntrctNum=" & currentOfferDocument.ContractNumber & "&SchNum=" & currentOfferDocument.ScheduleNumber)
            Else
                Msg_Alert.Client_Alert_OnLoad("The Offer is not linked to its Contract.")
            End If
        Else
            Msg_Alert.Client_Alert_OnLoad("The Offer is not linked to its Contract (2).")
        End If
    End Sub
    Protected Sub CreateContract(ByVal s As Object, ByVal e As System.Web.UI.WebControls.CommandEventArgs)
        'Dim myContractNumber As HiddenField = CType(fvOfferRecord.Row.FindControl("hfContractNum"), HiddenField)
        'Dim myScheduleNumber As HiddenField = CType(fvOfferRecord.Row.FindControl("hfScheduleNumber"), HiddenField)
        'Dim mySchNum As String = ""
        'If Not myScheduleNumber Is Nothing Then
        '    mySchNum = myScheduleNumber.Value.ToString
        'End If
        'Dim myOfferID As String = fvOfferRecord.DataKey.Value.ToString
        'If Not myContractNumber Is Nothing Then
        '    If myContractNumber.Value.ToString = "" Then
        '  Response.Redirect("contract_addition.aspx?ScheduleNum=" & mySchNum & "&OfferID=" & myOfferID)
        Dim currentOfferDocument As VA.NAC.NACCMBrowser.BrowserObj.CurrentDocument
        currentOfferDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If Not currentOfferDocument.ContractNumber Is Nothing Then
            If currentOfferDocument.ContractNumber.Length > 0 Then
                Msg_Alert.Client_Alert_OnLoad("The Offer already has a Contract. ")
            Else
                Response.Redirect("CreateContract.aspx?ScheduleNum=" & currentOfferDocument.ScheduleNumber & "&OfferID=" & currentOfferDocument.OfferId)
            End If
        Else
            Response.Redirect("CreateContract.aspx?ScheduleNum=" & currentOfferDocument.ScheduleNumber & "&OfferID=" & currentOfferDocument.OfferId)
        End If
    End Sub
    Protected Sub setStatus()

    End Sub
    Protected Function ContractStatus(ByVal Expire As Object, ByVal Comp As Object) As String
        Dim myStatus As String = ""
        If Not Expire.Equals(DBNull.Value) Then
            If Not Comp.Equals(DBNull.Value) Then
                Dim myLabel As Label = CType(fvOfferRecord.Row.FindControl("lbContractStatus"), Label)

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
            End If
        End If
        Return myStatus
    End Function
    Protected Sub dataSolicitation()

    End Sub
    Protected Sub CheckDate(ByVal s As Object, ByVal e As EventArgs)
        Dim myDropDownlist As DropDownList = CType(s, DropDownList)
        Dim strSQL As String = ""
        If myDropDownlist.ID = "dlAssignmentDate" Then
            strSQL = "SELECT Dates_Assigned FROM dbo.view_Offers_Full WHERE Offer_ID ='" & Request.QueryString("OfferID").ToString & "'"
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
                    If rdr("Dates_Assigned").Equals(DBNull.Value) Then
                        myExpiration = "01/01/1900"
                    Else
                        myExpiration = CType(rdr("Dates_Assigned"), DateTime)
                    End If

                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            If Not myExpiration = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = myExpiration.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlReassignmentDate" Then
            strSQL = "SELECT Dates_Reassigned FROM dbo.view_Offers_Full WHERE Offer_ID ='" & Request.QueryString("OfferID").ToString & "'"
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
                    If rdr("Dates_Reassigned").Equals(DBNull.Value) Then
                        myExpiration = "01/01/1900"
                    Else
                        myExpiration = CType(rdr("Dates_Reassigned"), DateTime)
                    End If

                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            If Not myExpiration = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = myExpiration.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlActionCompDate" Then
            strSQL = "SELECT Dates_Action FROM dbo.view_Offers_Full WHERE Offer_ID='" & Request.QueryString("OfferID").ToString & "'"
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
                    If rdr("Dates_Action").Equals(DBNull.Value) Then
                        myExpiration = "01/01/1900"
                    Else
                        myExpiration = CType(rdr("Dates_Action"), DateTime)
                    End If
                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            If Not myExpiration = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = myExpiration.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlEstCompDate" Then
            strSQL = "SELECT Dates_Expected_Completion FROM dbo.view_Offers_Full WHERE Offer_ID='" & Request.QueryString("OfferID").ToString & "'"
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
                    If rdr("Dates_Expected_Completion").Equals(DBNull.Value) Then
                        myExpiration = "01/01/1900"
                    Else
                        myExpiration = CType(rdr("Dates_Expected_Completion"), DateTime)
                    End If
                End While
                conn.Close()
            Catch ex As Exception

            Finally
                conn.Close()
            End Try
            If Not myExpiration = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(myExpiration.ToShortDateString, myExpiration.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = myExpiration.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlAuditDate" Then
            strSQL = "SELECT Dates_Sent_for_Preaward FROM dbo.view_Offers_Full WHERE Offer_ID='" & Request.QueryString("OfferID").ToString & "'"
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand
            Dim rdr As SqlDataReader
            Dim preAwardDate As DateTime
            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                While rdr.Read
                    If rdr("Dates_Sent_for_Preaward").Equals(DBNull.Value) Then
                        preAwardDate = "01/01/1900"
                    Else
                        preAwardDate = CType(rdr("Dates_Sent_for_Preaward"), DateTime)
                    End If
                End While
                conn.Close()
            Catch ex As Exception

            Finally
                conn.Close()
            End Try
            If Not preAwardDate = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(preAwardDate.ToShortDateString, preAwardDate.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = preAwardDate.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlReturnDate" Then
            strSQL = "SELECT Dates_Returned_to_Office FROM dbo.view_Offers_Full WHERE Offer_ID='" & Request.QueryString("OfferID").ToString & "'"
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand
            Dim rdr As SqlDataReader
            Dim returnToOfficeDate As DateTime
            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                While rdr.Read
                    If rdr("Dates_Returned_to_Office").Equals(DBNull.Value) Then
                        returnToOfficeDate = "01/01/1900"
                    Else
                        returnToOfficeDate = CType(rdr("Dates_Returned_to_Office"), DateTime)
                    End If
                End While
                conn.Close()
            Catch ex As Exception

            Finally
                conn.Close()
            End Try
            If Not returnToOfficeDate = "01/01/1900" Then
                Dim mylistItem As ListItem = New ListItem(returnToOfficeDate.ToShortDateString, returnToOfficeDate.ToShortDateString)
                myDropDownlist.Items.Add(mylistItem)
                myDropDownlist.SelectedValue = returnToOfficeDate.ToShortDateString
            Else
                myDropDownlist.SelectedValue = ""
            End If
        End If
        If myDropDownlist.ID = "dlState" Then
            strSQL = "SELECT Primary_State FROM dbo.view_Offers_Full WHERE Offer_ID='" & Request.QueryString("OfferID").ToString & "'"
            Dim ds As New DataSet
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Dim cmd As SqlCommand
            Dim rdr As SqlDataReader
            Dim myState As String = ""
            Try
                conn.Open()
                cmd = New SqlCommand(strSQL, conn)
                rdr = cmd.ExecuteReader
                While rdr.Read
                    myState = CType(rdr("Primary_State"), String)
                End While
                conn.Close()
            Catch ex As Exception
            Finally
                conn.Close()
            End Try
            Dim mylistItem As ListItem = New ListItem(myState.ToString, myState.ToString)
            myDropDownlist.Items.Add(mylistItem)
            myDropDownlist.SelectedValue = myState.ToString
        End If

    End Sub
    Protected Sub fvOfferRecord_OnPreRender(ByVal sender As Object, ByVal args As EventArgs)
        Dim fvOfferRecord As FormView = CType(sender, FormView)

        Dim saveOfferButton As Button = CType(fvOfferRecord.FindControl("SaveOfferButton"), Button)
        Dim openOrCreateContractButton As Button = CType(fvOfferRecord.Row.FindControl("OpenOrCreateContractButton"), Button)
        Dim currentActionDropDownList As DropDownList = CType(fvOfferRecord.Row.FindControl("dlCurrent"), DropDownList)

        Dim bs As BrowserSecurity2 = CType(Session("BrowserSecurity"), BrowserSecurity2)
        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If Not bs Is Nothing Then
            If Not saveOfferButton Is Nothing Then
                If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.Offers) = False) Then
                    saveOfferButton.Visible = False
                End If
                If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True) Then
                    saveOfferButton.Visible = True
                End If
            End If

            AssignOfferButtonText()

            If Not openOrCreateContractButton Is Nothing Then
                If Not currentDocument Is Nothing Then
                    'if no contract has been assigned, then consider creation permissions
                    If currentDocument.ContractNumber.Trim.Length = 0 Then
                        If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.CreateContract) = False) Then
                            openOrCreateContractButton.Enabled = "False"
                        End If
                        If Not currentActionDropDownList Is Nothing Then
                            'withdrawn or no award will not be getting a contract assignment
                            If currentActionDropDownList.SelectedValue = 7 Or currentActionDropDownList.SelectedValue = 8 Then
                                openOrCreateContractButton.Enabled = "False"
                            End If
                        End If
                    End If
                End If
            End If
        End If

        'if there is an assignment date, then do not allow it to be edited
        ' Dim assignmentDateString As String
        'Dim assignmentDateTextBox As TextBox = CType(fvOfferRecord.FindControl("tbAssignment"), TextBox)
        'If Not assignmentDateTextBox Is Nothing Then
        '    assignmentDateString = assignmentDateTextBox.Text
        '    If assignmentDateString.Length > 0 Then
        '        assignmentDateTextBox.ReadOnly = True
        '    End If
        'End If

        'if there is an assignment date, then do not allow it to be edited
        'Dim assignmentDateString As String
        Dim dlAssignmentDate As DropDownList = CType(fvOfferRecord.FindControl("dlAssignmentDate"), DropDownList)

        If Not dlAssignmentDate Is Nothing Then
            Dim bOfferAssignmentDateWasInitiallyBlank As Boolean

            If Not Session("OfferAssignmentDateWasInitiallyBlank") Is Nothing Then

                bOfferAssignmentDateWasInitiallyBlank = CType(Session("OfferAssignmentDateWasInitiallyBlank"), Boolean)
                If bOfferAssignmentDateWasInitiallyBlank = True Then
                    dlAssignmentDate.Enabled = True
                Else
                    dlAssignmentDate.Enabled = False
                    'unless the user has admin rights
                    If Not bs Is Nothing Then
                        If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True) Then
                            dlAssignmentDate.Enabled = True
                        End If
                    End If
                End If
            End If
        End If

        'Dim selectedItem As ListItem
        'selectedItem = dlAssignmentDate.SelectedItem

        'If Not selectedItem Is Nothing Then
        '    If selectedItem.Text.Length > 0 Then
        '        dlAssignmentDate.Enabled = False
        '        'unless the user has admin rights
        '        If Not bs Is Nothing Then
        '            If (bs.CheckPermissions(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True) Then
        '                dlAssignmentDate.Enabled = True
        '            End If
        '        End If
        '    End If

        'End If
        'End If

        EnableOfferDateEditing(sender)

    End Sub


    Protected Sub fvOfferRecord_OnDataBound(ByVal sender As Object, ByVal args As EventArgs)


    End Sub

    Protected Sub OfferDataSource_OnUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles OfferDataSource.Updating
        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        Dim dlCOName As DropDownList = CType(fvOfferRecord.Row.FindControl("dlCOName"), DropDownList)
        Dim assignedCOID As Integer = dlCOName.SelectedValue

        e.Command.Parameters("@LastModifiedBy").Value = browserSecurity.UserInfo.LoginName
        e.Command.Parameters("@Date_Modified").Value = DateTime.Now.ToString()
        e.Command.Parameters("@CO_ID").Value = assignedCOID

    End Sub


    'this applies to the date buttons which are intended to be reserved for admins
    Protected Sub EnableOfferDateEditing(ByVal sender As FormView)

        Dim currentPage As Page
        Dim fvOfferRecord As FormView = CType(sender, FormView)

        currentPage = fvOfferRecord.Page

        'create image button scripts
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OReceivedDateButtonOnClickScript", GetDateButtonScript("OReceived"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OAssignmentDateButtonOnClickScript", GetDateButtonScript("OAssignment"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OReassignmentDateButtonOnClickScript", GetDateButtonScript("OReassignment"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OEstimatedCompletionDateButtonOnClickScript", GetDateButtonScript("OEstimatedCompletion"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OActionDateButtonOnClickScript", GetDateButtonScript("OAction"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OAuditDateButtonOnClickScript", GetDateButtonScript("OAudit"), True)
        ScriptManager.RegisterStartupScript(CType(currentPage, Page), currentPage.GetType(), "OReturnDateButtonOnClickScript", GetDateButtonScript("OReturn"), True)

        Dim currentDocument As CurrentDocument
        currentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        'OfferReceivedDateImageButton
        Dim OfferReceivedDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferReceivedDateImageButton"), ImageButton)
        Dim DateReceivedTextBox As System.Web.UI.WebControls.TextBox = CType(fvOfferRecord.FindControl("tbReceived"), System.Web.UI.WebControls.TextBox)
        If Not OfferReceivedDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferReceivedDateImageButton.Visible = True
                DateReceivedTextBox.Enabled = True
            Else
                OfferReceivedDateImageButton.Visible = False
            End If
        End If

        'OfferAssignmentDateImageButton
        Dim OfferAssignmentDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferAssignmentDateImageButton"), ImageButton)
        If Not OfferAssignmentDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferAssignmentDateImageButton.Visible = True
            Else
                OfferAssignmentDateImageButton.Visible = False
            End If
        End If

        'OfferReassignmentDateImageButton
        Dim OfferReassignmentDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferReassignmentDateImageButton"), ImageButton)
        If Not OfferReassignmentDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferReassignmentDateImageButton.Visible = True
            Else
                OfferReassignmentDateImageButton.Visible = False
            End If
        End If

        'OfferEstimatedCompletionDateImageButton
        Dim OfferEstimatedCompletionDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferEstimatedCompletionDateImageButton"), ImageButton)
        If Not OfferEstimatedCompletionDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferEstimatedCompletionDateImageButton.Visible = True
            Else
                OfferEstimatedCompletionDateImageButton.Visible = False
            End If
        End If

        'OfferActionDateImageButton
        Dim OfferActionDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferActionDateImageButton"), ImageButton)
        If Not OfferActionDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferActionDateImageButton.Visible = True
            Else
                OfferActionDateImageButton.Visible = False
            End If
        End If

        'OfferAuditDateImageButton
        Dim OfferAuditDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferAuditDateImageButton"), ImageButton)
        If Not OfferAuditDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferAuditDateImageButton.Visible = True
            Else
                OfferAuditDateImageButton.Visible = False
            End If
        End If

        'OfferReturnDateImageButton
        Dim OfferReturnDateImageButton As ImageButton = CType(fvOfferRecord.FindControl("OfferReturnDateImageButton"), ImageButton)
        If Not OfferReturnDateImageButton Is Nothing Then
            If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.AdministerOfferDates) = True Then
                OfferReturnDateImageButton.Visible = True
            Else
                OfferReturnDateImageButton.Visible = False
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
    Protected Sub tbReceived_OnDataBinding(ByVal sender As Object, ByVal args As EventArgs)
        Dim ReceivedDateTextBox As System.Web.UI.WebControls.TextBox = CType(sender, System.Web.UI.WebControls.TextBox)

        Dim fvOfferRecord As FormView = CType(ReceivedDateTextBox.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim receivedDateString As String
        If Not currentRow.Item("Dates_Received").Equals(DBNull.Value) Then
            receivedDateString = FormatDateTime(currentRow.Item("Dates_Received"), DateFormat.ShortDate).ToString
            Session("OReceived") = receivedDateString
        End If

    End Sub
    Protected Sub dlAssignmentDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim AssignmentDateDropDownList As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(AssignmentDateDropDownList.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim assignmentDateString As String
        If Not currentRow.Item("Dates_Assigned").Equals(DBNull.Value) Then
            assignmentDateString = FormatDateTime(currentRow.Item("Dates_Assigned"), DateFormat.ShortDate).ToString
            Session("OAssignment") = assignmentDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlReassignmentDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlReassignmentDate As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlReassignmentDate.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim reassignmentDateString As String
        If Not currentRow.Item("Dates_Reassigned").Equals(DBNull.Value) Then
            reassignmentDateString = FormatDateTime(currentRow.Item("Dates_Reassigned"), DateFormat.ShortDate).ToString
            Session("OReassignment") = reassignmentDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlEstCompDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlEstCompDate As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlEstCompDate.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim estimatedCompletionDateString As String
        If Not currentRow.Item("Dates_Expected_Completion").Equals(DBNull.Value) Then
            estimatedCompletionDateString = FormatDateTime(currentRow.Item("Dates_Expected_Completion"), DateFormat.ShortDate).ToString
            Session("OEstimatedCompletion") = estimatedCompletionDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlActionCompDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlActionCompDate As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlActionCompDate.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim actionDateString As String
        If Not currentRow.Item("Dates_Action").Equals(DBNull.Value) Then
            actionDateString = FormatDateTime(currentRow.Item("Dates_Action"), DateFormat.ShortDate).ToString
            Session("OAction") = actionDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlAuditDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlAuditDate As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlAuditDate.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim auditDateString As String
        If Not currentRow.Item("Dates_Sent_for_Preaward").Equals(DBNull.Value) Then
            auditDateString = FormatDateTime(currentRow.Item("Dates_Sent_for_Preaward"), DateFormat.ShortDate).ToString
            Session("OAudit") = auditDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlReturnDate_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlReturnDate As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlReturnDate.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim returnDateString As String
        If Not currentRow.Item("Dates_Returned_to_Office").Equals(DBNull.Value) Then
            returnDateString = FormatDateTime(currentRow.Item("Dates_Returned_to_Office"), DateFormat.ShortDate).ToString
            Session("OReturn") = returnDateString
        End If

        CheckDate(sender, e)
    End Sub

    Protected Sub dlCOName_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlCOName As DropDownList = CType(sender, DropDownList)

        Dim fvOfferRecord As FormView = CType(dlCOName.NamingContainer, FormView)
        Dim currentRow As DataRowView = CType(fvOfferRecord.DataItem, DataRowView)

        Dim assignedCOID As Integer
        Dim assignedCOFullName As String
        Dim selectedCOListItem As ListItem

        If Not currentRow.Item("CO_ID").Equals(DBNull.Value) Then
            assignedCOID = Integer.Parse(currentRow.Item("CO_ID").ToString())
            assignedCOFullName = currentRow.Item("FullName").ToString()
            selectedCOListItem = New ListItem(assignedCOFullName, assignedCOID)

            If dlCOName.Items.Contains(selectedCOListItem) = True Then
                dlCOName.SelectedValue = assignedCOID
            Else
                dlCOName.Items.Add(selectedCOListItem)
                dlCOName.SelectedValue = assignedCOID
            End If
        End If
    End Sub

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

        Dim bIsExpired As Boolean = False
        If currentDocument.IsOfferCompleted = True Then
            bIsExpired = True
        End If

        e.Command.Parameters("@DivisionId").Value = divisionId
        e.Command.Parameters("@SelectFlag").Value = 0
        e.Command.Parameters("@OrderByLastNameFullName").Value = "F"
        e.Command.Parameters("@IsExpired").Value = bIsExpired
    End Sub
    Public Sub RefreshDate(ByVal dateTypeString As String)

        Dim displayDate As DateTime

        If dateTypeString.Contains("OReceived") = True Then
            Dim ReceivedDateTextBox As System.Web.UI.WebControls.TextBox = CType(fvOfferRecord.FindControl("tbReceived"), System.Web.UI.WebControls.TextBox)
            If Not Session("OReceived") Is Nothing Then
                displayDate = DateTime.Parse(Session("OReceived").ToString())
                ReceivedDateTextBox.Text = displayDate.ToShortDateString()
            Else
                ReceivedDateTextBox.Text = "x"
            End If
        End If

        'If dateTypeString.Contains("OAssignment") = True Then
        '    Dim AssignmentDateTextBox As TextBox = CType(fvOfferRecord.FindControl("tbAssignment"), TextBox)
        '    If Not Session("OAssignment") Is Nothing Then
        '        displayDate = DateTime.Parse(Session("OAssignment").ToString())
        '        AssignmentDateTextBox.Text = displayDate.ToShortDateString()
        '    Else
        '        AssignmentDateTextBox.Text = "x"
        '    End If
        'End If

        If dateTypeString.Contains("OAssignment") = True Then
            Dim dlAssignmentDate As DropDownList = CType(fvOfferRecord.FindControl("dlAssignmentDate"), DropDownList)

            If Not Session("OAssignment") Is Nothing Then
                displayDate = DateTime.Parse(Session("OAssignment").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlAssignmentDate.Items.Contains(newItem) = True) Then
                    dlAssignmentDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlAssignmentDate.ClearSelection()
                    newItem.Selected = True
                    dlAssignmentDate.Items.Insert(0, newItem)
                End If
            End If
        End If

        If dateTypeString.Contains("OReassignment") = True Then
            Dim dlReassignmentDate As DropDownList = CType(fvOfferRecord.FindControl("dlReassignmentDate"), DropDownList)

            If Not Session("OReassignment") Is Nothing Then
                displayDate = DateTime.Parse(Session("OReassignment").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlReassignmentDate.Items.Contains(newItem) = True) Then
                    dlReassignmentDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlReassignmentDate.ClearSelection()
                    newItem.Selected = True
                    dlReassignmentDate.Items.Insert(0, newItem)
                End If
            End If
        End If



        If dateTypeString.Contains("OEstimatedCompletion") = True Then
            Dim dlEstCompDate As DropDownList = CType(fvOfferRecord.FindControl("dlEstCompDate"), DropDownList)

            If Not Session("OEstimatedCompletion") Is Nothing Then
                displayDate = DateTime.Parse(Session("OEstimatedCompletion").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlEstCompDate.Items.Contains(newItem) = True) Then
                    dlEstCompDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlEstCompDate.ClearSelection()
                    newItem.Selected = True
                    dlEstCompDate.Items.Insert(0, newItem)
                End If
            End If
        End If

        If dateTypeString.Contains("OAction") = True Then
            Dim dlActionCompDate As DropDownList = CType(fvOfferRecord.FindControl("dlActionCompDate"), DropDownList)
            If Not Session("OAction") Is Nothing Then
                displayDate = DateTime.Parse(Session("OAction").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlActionCompDate.Items.Contains(newItem) = True) Then
                    dlActionCompDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlActionCompDate.ClearSelection()
                    newItem.Selected = True
                    dlActionCompDate.Items.Insert(0, newItem)
                End If
            End If
        End If

        If dateTypeString.Contains("OAudit") = True Then
            Dim dlAuditDate As DropDownList = CType(fvOfferRecord.FindControl("dlAuditDate"), DropDownList)
            If Not Session("OAudit") Is Nothing Then
                displayDate = DateTime.Parse(Session("OAudit").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlAuditDate.Items.Contains(newItem) = True) Then
                    dlAuditDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlAuditDate.ClearSelection()
                    newItem.Selected = True
                    dlAuditDate.Items.Insert(0, newItem)
                End If
            End If
        End If

        If dateTypeString.Contains("OReturn") = True Then
            Dim dlReturnDate As DropDownList = CType(fvOfferRecord.FindControl("dlReturnDate"), DropDownList)
            If Not Session("OReturn") Is Nothing Then
                displayDate = DateTime.Parse(Session("OReturn").ToString())

                Dim newItem As ListItem = New ListItem(displayDate)
                If (dlReturnDate.Items.Contains(newItem) = True) Then
                    dlReturnDate.SelectedValue = displayDate.ToShortDateString()
                Else
                    dlReturnDate.ClearSelection()
                    newItem.Selected = True
                    dlReturnDate.Items.Insert(0, newItem)
                End If
            End If
        End If

    End Sub
    ' if the action is a completion action, then clear the action dates to enforce accurate date editing
    Protected Sub dlCurrent_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlCurrentOfferAction As DropDownList = CType(sender, DropDownList)
        Dim currentOfferActionDescription As String = CMGlobals.GetSelectedTextFromDropDownList(dlCurrentOfferAction)
        Dim bCompletion As Boolean = False

        If Not currentOfferActionDescription Is Nothing Then
            ' Award includes No Award
            If currentOfferActionDescription.Contains("Award") = True Or currentOfferActionDescription.Contains("Withdrawn") = True Then
                bCompletion = True
            End If
        End If

        If bCompletion = True Then
            Dim EstimatedCompletionDateDropDownList As DropDownList = CType(fvOfferRecord.FindControl("dlEstCompDate"), DropDownList)
            Dim LastActionCompletionDateDropDownList As DropDownList = CType(fvOfferRecord.FindControl("dlActionCompDate"), DropDownList)

            EstimatedCompletionDateDropDownList.SelectedIndex = 0
            LastActionCompletionDateDropDownList.SelectedIndex = 0
        End If
    End Sub

End Class
