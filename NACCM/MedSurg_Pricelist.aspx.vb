Imports System.Data
Imports System.Data.SqlClient

Imports VA.NAC.NACCMBrowser.BrowserObj
Imports VA.NAC.NACCMBrowser.DBInterface
Imports VA.NAC.Application.SharedObj

Imports TextBox = System.Web.UI.WebControls.TextBox

Partial Public Class MedSurg_Pricelist
    Inherits System.Web.UI.Page
    Protected myContractNumber As String
    Protected insertPriceParameter(6) As SqlParameter

    Private _currentContractNumber As String = ""
    Private _parentContractNumber As String = ""  ' the parent fss contract for the current bpa
    Private _pricelistType As String = ""
    Private _canEditYN As String = "N"
    Private _isContractActive As String = "F"



    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Dim cs As ClientScriptManager = Page.ClientScript
        'cs.RegisterHiddenField("RefreshPricelistGridOnSubmit", "False")
        CMGlobals.CheckIfStartedProperly(Me.Page)

        Dim currentDocument As CurrentDocument = CType(Session("CurrentDocument"), CurrentDocument)

        If Not IsPostBack Then
            Dim myrefer_page As String = Session("Requested")

            ' support for version 2 in parallel
            If myrefer_page <> "Contract" And myrefer_page <> "Contract2" Then
                Response.Write("<script>window.close();</script>")
            End If
            If Not Request.QueryString("CntrctNum") = Nothing Then
                _currentContractNumber = Request.QueryString("CntrctNum")
                Session("CurrentContractNumberForPricelist") = _currentContractNumber
            End If
            If Not Request.QueryString("PricelistType") = Nothing Then
                'B for BPA, F for FSS, 6 for 621
                _pricelistType = Request.QueryString("PricelistType")
                Session("PricelistType") = _pricelistType
            End If
            If Not Request.QueryString("IsContractActive") = Nothing Then
                _isContractActive = Request.QueryString("IsContractActive")
                Session("IsContractActive") = _isContractActive
            End If

            'If Not Request.QueryString("Edit") Is Nothing Then
            '    _canEditYN = Request.QueryString("Edit")
            '    Session("CanEditPricelist") = _canEditYN
            'End If
            If _isContractActive = "F" Then
                _canEditYN = "N"
            Else
                If Not currentDocument Is Nothing Then
                    If currentDocument.IsAccessAllowed(BrowserSecurity2.AccessPoints.MedSurgItems) = True Then
                        _canEditYN = "Y"
                    Else
                        _canEditYN = "N"
                    End If
                End If
            End If
            Session("CanEditPricelist") = _canEditYN

            If _pricelistType = "F" Then
                fvFSSPriceList.Visible = True
                gvFSSPriceList.Visible = True

                ' set visibility of edit functions
                btnAddFSSPrice.Visible = CanEdit()   ' add button
                gvFSSPriceList.Columns(7).Visible = CanEdit() ' remove button

            ElseIf _pricelistType = "6" Then
                fvServicesPriceList.Visible = True
                gvServicesPricelist.Visible = True

                ' set visibility of edit functions
                btnServicesOpenAdd.Visible = CanEdit()   ' add button
                gvServicesPricelist.Columns(4).Visible = CanEdit() ' remove button


            ElseIf _pricelistType = "B" Then
                'expect the bpa to specify a parent in the session
                'from the general tab of the main screen
                'If Not Session("ParentFSSContractNumber") = Nothing Then
                '    _parentContractNumber = CType(Session("ParentFSSContractNumber"), String)
                '    'Else
                '    '    Throw New Exception("Parent FSS contract number not selected for BPA. Select parent FSS before editing pricelist.")
                'End If

                If Not currentDocument.ParentDocument Is Nothing Then
                    _parentContractNumber = currentDocument.ParentDocument.ContractNumber
                End If

                If currentDocument.HasParent = True Then
                    fvBPAPriceList.Visible = True
                    gvBPAPricelist.Visible = True

                    ' set visibility of edit functions
                    btnBPAOpenAdd.Visible = CanEdit()   ' add button
                    gvBPAPricelist.Columns(7).Visible = CanEdit() ' remove button
                Else
                    'Non-standard contract
                    _pricelistType = "NB"
                    Session("PricelistType") = _pricelistType
                    fvNonStandardBPAPriceList.Visible = True
                    gvNonStandardBPAPricelist.Visible = True

                    ' set visibility of edit functions
                    btnNonStandardBPAOpenAdd.Visible = CanEdit()   ' add button
                    gvNonStandardBPAPricelist.Columns(3).Visible = CanEdit() ' remove button                End If

                End If
            End If

        Else
            _canEditYN = CType(Session("CanEditPricelist"), String)
            _pricelistType = CType(Session("PricelistType"), String)
            _currentContractNumber = CType(Session("CurrentContractNumberForPricelist"), String)
            _isContractActive = CType(Session("IsContractActive"), String)

            If _pricelistType = "B" Then
                'If Not Session("ParentFSSContractNumber") = Nothing Then
                '    _parentContractNumber = CType(Session("ParentFSSContractNumber"), String)
                '    '    Else
                '    '        Throw New Exception("Parent FSS contract number not selected for BPA. Select parent FSS before editing pricelist.")
                'End If

                If Not currentDocument.ParentDocument Is Nothing Then
                    _parentContractNumber = currentDocument.ParentDocument.ContractNumber
                End If

            End If


            Dim refreshGrid As String

            Dim RefreshPricelistGridOnSubmit As HiddenField
            RefreshPricelistGridOnSubmit = CType(MedSurgPricelistForm.FindControl("RefreshPricelistGridOnSubmit"), HiddenField)
            If Not RefreshPricelistGridOnSubmit Is Nothing Then
                refreshGrid = RefreshPricelistGridOnSubmit.Value
                If refreshGrid.Contains("true") Then
                    RefreshPricelistGridOnSubmit.Value = "false"
                    RefreshPricelistGrid()
                End If

            End If

        End If

        AddClientCloseEvent()

    End Sub

    Private Sub AddClientCloseEvent()
        Dim formCloseButton As Button

        Dim referrerPage As String
        referrerPage = Session("Requested")

        Dim closeFunctionText As String = ""

        If referrerPage = "Contract2" Then
            closeFunctionText = "CloseWindow2();"
        ElseIf referrerPage = "Contract" Then
            closeFunctionText = "CloseWindow();"
        End If

        If _pricelistType = "F" Then
            formCloseButton = CType(fvFSSPriceList.FindControl("formCloseButton"), Button)
            formCloseButton.Attributes.Add("onclick", closeFunctionText)
        ElseIf _pricelistType = "6" Then
            formCloseButton = CType(fvServicesPriceList.FindControl("formCloseButton"), Button)
            formCloseButton.Attributes.Add("onclick", closeFunctionText)
        ElseIf _pricelistType = "B" Then
            formCloseButton = CType(fvBPAPriceList.FindControl("formCloseButton"), Button)
            formCloseButton.Attributes.Add("onclick", closeFunctionText)
        ElseIf _pricelistType = "NB" Then
            formCloseButton = CType(fvNonStandardBPAPriceList.FindControl("formCloseButton"), Button)
            formCloseButton.Attributes.Add("onclick", closeFunctionText)
        End If
    End Sub

    Private Function CanEdit() As Boolean
        If _canEditYN = "Y" Then
            Return True
        Else
            Return False
        End If
    End Function
    Protected Sub RefreshPricelistGrid()

        Dim isReasonToRebind As Boolean = False
        If Not Session("PricelistDetailHasChanged") Is Nothing Then
            isReasonToRebind = CType(Session("PricelistDetailHasChanged"), Boolean)
        End If

        If isReasonToRebind = True Then
            If _pricelistType = "F" Then
                fvFSSPriceList.DataBind()
                gvFSSPriceList.DataBind()
            ElseIf _pricelistType = "6" Then
                fvServicesPriceList.DataBind()
                gvServicesPricelist.DataBind()
            ElseIf _pricelistType = "B" Then
                fvBPAPriceList.DataBind()
                gvBPAPricelist.DataBind()
            ElseIf _pricelistType = "NB" Then
                fvNonStandardBPAPriceList.DataBind()
                gvNonStandardBPAPricelist.DataBind()
            End If
        End If
    End Sub

    Protected Sub close_window(ByVal s As Object, ByVal e As EventArgs)
        Response.Write("<script>window.close();</script>")
    End Sub
    Private Sub OpenPricelistDetailsWindow(ByVal logNumber As Integer, ByVal canEditYN As String, ByVal pricelistType As String, ByVal currentContractNumber As String, Optional ByVal parentContractNumber As String = "", Optional ByVal parentLogNumber As Integer = -1)
        'init session var used by details screen
        Session("PricelistDetailDataSource") = Nothing
        Session("ParentLogNumberString") = Nothing
        Session("PricelistDetailNewFSSForBPADataSource") = Nothing
        Session("PricelistDetailHasChanged") = Nothing


        If pricelistType = "B" Then
            Response.Write("<script>window.open('pricelist_details.aspx?LogNumber=" _
                         & logNumber & "&Edit=" & canEditYN & "&PricelistType=" & pricelistType & "&CntrctNum=" & currentContractNumber & "&ParentContractNumber=" & parentContractNumber & "&ParentLogNumber=" & parentLogNumber & "', 'Pricelist_Detail','toolbar=0,status=0,menubar=0," _
                          & "scrollbars=0,resizable=0,top=200,left=240,width=860,height=650, resizable=0')</Script>")
        ElseIf pricelistType = "NB" Then
            Response.Write("<script>window.open('pricelist_details.aspx?LogNumber=" _
                         & logNumber & "&Edit=" & canEditYN & "&PricelistType=" & pricelistType & "&CntrctNum=" & currentContractNumber & "&ParentContractNumber=" & parentContractNumber & "&ParentLogNumber=" & parentLogNumber & "', 'Pricelist_Detail','toolbar=0,status=0,menubar=0," _
                          & "scrollbars=0,resizable=0,top=200,left=240,width=860,height=650, resizable=0')</Script>")
        ElseIf pricelistType = "F" Or pricelistType = "6" Then
            Response.Write("<script>window.open('pricelist_details.aspx?LogNumber=" _
                         & logNumber & "&Edit=" & canEditYN & "&PricelistType=" & pricelistType & "&CntrctNum=" & currentContractNumber & "', 'Pricelist_Detail','toolbar=0,status=0,menubar=0," _
                          & "scrollbars=0,resizable=0,top=200,left=240,width=860,height=650, resizable=0')</Script>")
        End If
    End Sub


    Protected Sub gvFSSPriceList_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName.ToString = "seeDetails" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)

            OpenPricelistDetailsWindow(mydataKey.Value, _canEditYN, _pricelistType, _currentContractNumber, _parentContractNumber)

            'ElseIf e.CommandName.ToString = "InsertNew" Then
            '    Dim myindex As Integer = e.CommandArgument
            '    Dim myGrid As GridView = CType(sender, GridView)
            '    Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            '    Dim mydataKey As DataKey = myGrid.DataKeys(myindex)

            '    OpenPricelistDetailsWindow(mydataKey.Value, _canEditYN, _pricelistType, _currentContractNumber, _parentContractNumber)

            'ElseIf e.CommandName.ToString = "InsertEmpty" Then
            '    Dim myDropList As DropDownList = CType(gvFSSPriceList.Controls(0).Controls(0).FindControl("dlEmptySIN"), DropDownList)
            '    Dim myTextBox As TextBox = CType(gvFSSPriceList.Controls(0).Controls(0).FindControl("tbEmptyCatalog"), TextBox)
            '    Dim myCatlogNumber As SqlParameter = New SqlParameter("@Contractor_Catalog_Number", SqlDbType.NVarChar, 50)
            '    Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.NVarChar, 50)
            '    myCatlogNumber.Direction = ParameterDirection.Input
            '    mySIN.Direction = ParameterDirection.Input
            '    myCatlogNumber.Value = myTextBox.Text
            '    mySIN.Value = myDropList.SelectedValue
            '    myTextBox = CType(gvFSSPriceList.Controls(0).Controls(0).FindControl("tbEmptyLongDesc"), TextBox)
            '    Dim myLongDesc As SqlParameter = New SqlParameter("@Product_Long_Description", SqlDbType.NVarChar, 800)
            '    myLongDesc.Direction = ParameterDirection.Input
            '    myLongDesc.Value = myTextBox.Text
            '    myTextBox = CType(gvFSSPriceList.Controls(0).Controls(0).FindControl("FSSEmptyPrice"), TextBox)
            '    Dim myFSSPrice As SqlParameter = New SqlParameter("@FSS_Price", SqlDbType.Decimal)
            '    myFSSPrice.Direction = ParameterDirection.Input
            '    myFSSPrice.Value = CType(myTextBox.Text, Decimal)
            '    myDropList = CType(gvFSSPriceList.Controls(0).Controls(0).FindControl("dlEmptyPackSize"), DropDownList)
            '    Dim myPackage As SqlParameter = New SqlParameter("@Package_Size_Priced_on_Contract", SqlDbType.NVarChar, 2)
            '    myPackage.Direction = ParameterDirection.Input
            '    myPackage.Value = myDropList.SelectedValue
            '    Dim myCntrctNum As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            '    myCntrctNum.Direction = ParameterDirection.Input
            '    myCntrctNum.Value = _currentContractNumber
            '    insertPriceParameter(0) = myCatlogNumber
            '    insertPriceParameter(1) = mySIN
            '    insertPriceParameter(2) = myLongDesc
            '    insertPriceParameter(3) = myFSSPrice
            '    insertPriceParameter(4) = myPackage
            '    insertPriceParameter(5) = myCntrctNum
            '    FSSPriceListDataSource.Insert()
        ElseIf e.CommandName = "Remove" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)
            Dim myLogNumber As String = CType(mydataKey.Value, String)
            Dim strSQL As String = ""
            strSQL = String.Format("UPDATE tbl_pricelist SET Removed = 1, LastModifiedBy = '{0}', Date_Modified = getdate() WHERE LogNumber = {1}", browserSecurity.UserInfo.LoginName, myLogNumber)
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim updateCommand As SqlCommand = New SqlCommand()
                updateCommand.Connection = conn
                updateCommand.CommandText = strSQL
                conn.Open()
                updateCommand.ExecuteNonQuery()
                gvFSSPriceList.DataBind()
                fvFSSPriceList.DataBind()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the Pricelist insert. " & ex.ToString)
            Finally
                conn.Close()
            End Try

        End If
    End Sub

    Protected Sub windowPrint(ByVal sender As Object, ByVal e As EventArgs)
        ' Dim strPrint As String = "<script language='javascript'>window.print()</script>"
        'Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "anything", strPrint)
        Response.Write("<script language='javascript'>window.print()</script>")
    End Sub

    Protected Sub btnAddFSSPrice_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlAddFSSPrice.Visible = True

        btnAddFSSPrice.Visible = False
    End Sub

    Protected Sub btnAddFSSPriceCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlAddFSSPrice.Visible = False
        tbAddCatalogNumber.Text = ""
        tbAddFSSPrice.Text = ""
        tbAddLongDescription.Text = ""

        btnAddFSSPrice.Visible = True
    End Sub

    Protected Sub btnSaveFSSPrice_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Try
            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Dim strSQL2 As String = "exec InsertMedSurgFSSPriceFromGUI @UserLogin, @ContractNumber, @ContractorCatalogNumber, @ProductLongDescription, @FSSPrice, @PackageSizePricedOnContract, @SIN, @DateEffective, @ExpirationDate, @CreatedBy, @DateEntered, @LastModifiedBy, @DateModified, @LogNumber OUTPUT "
            '   Dim strSQL As String = "INSERT INTO [tbl_pricelist] ([CntrctNum], [Contractor Catalog Number], [Product Long Description], [FSS Price], [Package Size Priced on Contract], [SIN], [DateEffective], [ExpirationDate], [CreatedBy], [Date_Entered], [LastModifiedBy], [Date_Modified] ) VALUES (@CntrctNum, @Contractor_Catalog_Number, @Product_Long_Description, @FSS_Price, @Package_Size_Priced_on_Contract, @SIN, @DateEffective, @ExpirationDate, @CreatedBy, @Date_Entered, @LastModifiedBy, @Date_Modified )"
            Dim insertCommand As SqlCommand = New SqlCommand()
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL2
            insertCommand.Parameters.Add("@UserLogin", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@ContractNumber", SqlDbType.NVarChar, 50).Value = _currentContractNumber
            insertCommand.Parameters.Add("@ContractorCatalogNumber", SqlDbType.NVarChar, 50).Value = tbAddCatalogNumber.Text
            insertCommand.Parameters.Add("@ProductLongDescription", SqlDbType.NVarChar, 800).Value = tbAddLongDescription.Text
            insertCommand.Parameters.Add("@FSSPrice", SqlDbType.Decimal).Value = CType(tbAddFSSPrice.Text, Decimal)
            insertCommand.Parameters.Add("@PackageSizePricedOnContract", SqlDbType.NVarChar, 2).Value = dlAddPackage.SelectedValue.ToString
            insertCommand.Parameters.Add("@SIN", SqlDbType.NVarChar, 50).Value = dlAddSin.SelectedValue.ToString
            insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = FSSEffectiveDateTextBox.Text
            insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = FSSExpirationDateTextBox.Text
            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@DateEntered", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@DateModified", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LogNumber", SqlDbType.Int).Direction = ParameterDirection.Output
            conn.Open()
            insertCommand.ExecuteNonQuery()
            gvFSSPriceList.DataBind()
            fvFSSPriceList.DataBind()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The following error occurred when attempting to insert (F) the price: " & ex.Message)
        Finally
            conn.Close()
        End Try
        pnlAddFSSPrice.Visible = False
        tbAddCatalogNumber.Text = ""
        tbAddFSSPrice.Text = ""

        tbAddLongDescription.Text = ""
        btnAddFSSPrice.Visible = True

        AddClientCloseEvent()

    End Sub

    Protected Sub btnBPACancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlBPAAdd.Visible = False
        btnBPAOpenAdd.Visible = CanEdit()
        tbBPADescription.Text = ""
        tbBPAFSSPrice.Text = ""
        tbBPAPrice.Text = ""
        dlFSSCatalogNum.SelectedIndex = 0
    End Sub

    Protected Sub btnSaveBPA_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Try
            Dim strBPAPrice As Decimal = 0
            Dim strBPADescription As String = tbBPADescription.Text
            If tbBPAPrice.Text <> "" Then
                strBPAPrice = CType(tbBPAPrice.Text, Decimal)
            End If
            If strBPADescription.Length > 255 Then
                strBPADescription = strBPADescription.Substring(0, 255)
            End If

            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Dim strSQL2 As String = "exec InsertMedSurgBPAPriceFromGUI @UserLogin, @ContractNumber, @FSSLogNumber, @Description, @BPAPrice, @DateEffective, @ExpirationDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate, @BPALogNumber OUTPUT "
            '  Dim strSQL As String = "INSERT INTO [tbl_BPA_pricelist] ([CntrctNum], [FSSLogNumber], [Description], [BPA/BOA Price], [DateEffective], [ExpirationDate], [CreatedBy], [CreationDate], [LastModifiedBy], [LastModificationDate]) VALUES (@CntrctNum, @FSSLogNumber, @Description, @BPA_Price, @DateEffective, @ExpirationDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate )"
            Dim insertCommand As SqlCommand = New SqlCommand()
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL2
            insertCommand.Parameters.Add("@UserLogin", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@ContractNumber", SqlDbType.NVarChar, 50).Value = _currentContractNumber
            insertCommand.Parameters.Add("@FSSLogNumber", SqlDbType.Int).Value = CType(dlFSSCatalogNum.SelectedValue, Integer)
            insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = strBPADescription
            insertCommand.Parameters.Add("@BPAPrice", SqlDbType.Money).Value = strBPAPrice
            insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = BPAEffectiveDateTextBox.Text
            insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = BPAExpirationDateTextBox.Text
            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@BPALogNumber", SqlDbType.Int).Direction = ParameterDirection.Output
            conn.Open()
            insertCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The following error occurred when attempting to insert (B) the price: " & ex.Message)
        Finally
            conn.Close()
        End Try
        pnlBPAAdd.Visible = False
        tbBPADescription.Text = ""
        tbBPAFSSPrice.Text = ""
        tbBPAPrice.Text = ""
        dlFSSCatalogNum.SelectedIndex = 0
        btnBPAOpenAdd.Visible = CanEdit()
        fvBPAPriceList.DataBind()
        gvBPAPricelist.DataBind()

        AddClientCloseEvent()
    End Sub

    Protected Sub btnBPAOpenAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlBPAAdd.Visible = True
        btnBPAOpenAdd.Visible = False
    End Sub
    Protected Sub btnNonStandardBPACancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlNonStandardBPAAdd.Visible = False
        btnNonStandardBPAOpenAdd.Visible = CanEdit()
        tbNonStandardBPADescription.Text = ""
        tbNonStandardBPAPrice.Text = ""
    End Sub

    Protected Sub btnSaveNonStandardBPA_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Try
            Dim strNonStandardBPAPrice As Decimal = 0
            Dim strNonStandardBPADescription As String = tbNonStandardBPADescription.Text
            If tbNonStandardBPAPrice.Text <> "" Then
                strNonStandardBPAPrice = CType(tbNonStandardBPAPrice.Text, Decimal)
            End If
            If strNonStandardBPADescription.Length > 255 Then
                strNonStandardBPADescription = strNonStandardBPADescription.Substring(0, 255)
            End If
            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Dim strSQL2 As String = "exec InsertMedSurgNonStandardBPAPriceFromGUI @UserLogin, @ContractNumber, @Description, @BPAPrice, @DateEffective, @ExpirationDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate, @BPALogNumber OUTPUT "
            'Dim strSQL As String = "INSERT INTO [tbl_BPA_pricelist] ([CntrctNum], [Description], [BPA/BOA Price], [DateEffective], [ExpirationDate], [CreatedBy], [CreationDate], [LastModifiedBy], [LastModificationDate]) VALUES (@CntrctNum, @Description, @BPA_Price, @DateEffective, @ExpirationDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate )"
            Dim insertCommand As SqlCommand = New SqlCommand()
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL2
            'insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = _currentContractNumber
            'insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = strNonStandardBPADescription
            'insertCommand.Parameters.Add("@BPA_Price", SqlDbType.Decimal).Value = strNonStandardBPAPrice
            'insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = NonStandardBPAEffectiveDateTextBox.Text
            'insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = NonStandardBPAExpirationDateTextBox.Text
            'insertCommand.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = DateTime.Now
            'insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            'insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            'insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName

            insertCommand.Parameters.Add("@UserLogin", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@ContractNumber", SqlDbType.NVarChar, 50).Value = _currentContractNumber
            insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = strNonStandardBPADescription
            insertCommand.Parameters.Add("@BPAPrice", SqlDbType.Money).Value = strNonStandardBPAPrice
            insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = NonStandardBPAEffectiveDateTextBox.Text
            insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = NonStandardBPAExpirationDateTextBox.Text
            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@LastModificationDate", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@BPALogNumber", SqlDbType.Int).Direction = ParameterDirection.Output

            conn.Open()
            insertCommand.ExecuteNonQuery()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The following error occurred when attempting to insert (N) the price: " & ex.Message)
        Finally
            conn.Close()
        End Try
        pnlNonStandardBPAAdd.Visible = False
        tbNonStandardBPADescription.Text = ""
        tbNonStandardBPAPrice.Text = ""
        btnNonStandardBPAOpenAdd.Visible = CanEdit()
        fvNonStandardBPAPriceList.DataBind()
        gvNonStandardBPAPricelist.DataBind()

        AddClientCloseEvent()
       
    End Sub

    Protected Sub btnNonStandardBPAOpenAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlNonStandardBPAAdd.Visible = True
        btnNonStandardBPAOpenAdd.Visible = False
    End Sub

    Protected Sub dlFSSCatalogNum_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim myLogNumber As String = dlFSSCatalogNum.SelectedValue
        Dim strSQL As String = "SELECT [Product Long Description],[FSS Price] FROM tbl_Pricelist WHERE LogNumber =" & myLogNumber
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Dim cmd As SqlDataAdapter = New SqlDataAdapter(strSQL, conn)
        Dim ds As DataSet = New DataSet
        cmd.Fill(ds)
        tbBPADescription.Text = ds.Tables(0).Rows(0).Item("Product Long Description").ToString
        Dim myFSSPrice As Double = CType(ds.Tables(0).Rows(0).Item("FSS Price"), Double)
        tbBPAFSSPrice.Text = myFSSPrice.ToString("C")
    End Sub
    'Protected Sub Format_Curr(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim DollarNumber As Double = CType(tbBPAPrice.Text, Double)
    '    tbBPAPrice.Text = DollarNumber.ToString("C")
    'End Sub
 
    'Protected Sub Format_CurrForNSBPA(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim DollarNumber As Double = CType(tbNonStandardBPAPrice.Text, Double)
    '    tbNonStandardBPAPrice.Text = DollarNumber.ToString("C")
    'End Sub

    Protected Sub gvBPAPricelist_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName.ToString = "seeDetails" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)

            ' from gridview definition:  DataKeyNames="BPALogNumber,FSS_LogNumber" 
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)
            Dim parentFSSLogNumberString As String = mydataKey.Values.Item(1).ToString
            Dim parentFSSLogNumber As Integer = Integer.Parse(parentFSSLogNumberString)

            OpenPricelistDetailsWindow(mydataKey.Values.Item(0).ToString, _canEditYN, _pricelistType, _currentContractNumber, _parentContractNumber, parentFSSLogNumber)
            'Response.Write("<script>window.open('pricelist_details.aspx?LogNum=" _
            '             & mydataKey.Values.Item(1).ToString & "&Edit=" & _canEditYN & "&PricelistType=" & _pricelistType & "&CntrctNum=" & myFSS & "', 'Pricelist_Detail','toolbar=0,status=0,menubar=0," _
            '              & "scrollbars=1,resizable=0,top=300,width=750,height=500, resizable=0')</Script>")
        ElseIf e.CommandName = "Remove" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)
            Dim myLogNumber As String = CType(mydataKey.Value, String)
            Dim strSQL As String = ""
            strSQL = String.Format("UPDATE tbl_BPA_pricelist SET Removed = 1, LastModifiedBy = '{0}', LastModificationDate = getdate() WHERE BPALogNumber = {1}", browserSecurity.UserInfo.LoginName, myLogNumber)
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim updateCommand As SqlCommand = New SqlCommand()
                updateCommand.Connection = conn
                updateCommand.CommandText = strSQL
                conn.Open()
                updateCommand.ExecuteNonQuery()
                gvBPAPricelist.DataBind()
                fvBPAPriceList.DataBind()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the Pricelist insert. " & ex.ToString())
            Finally
                conn.Close()
            End Try

        End If
    End Sub
    Protected Sub gvNonStandardBPAPricelist_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName.ToString = "seeDetails" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)

            ' from gridview definition:  DataKeyNames="BPALogNumber" 
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)

            OpenPricelistDetailsWindow(mydataKey.Values.Item(0).ToString, _canEditYN, _pricelistType, _currentContractNumber, "", -1)
            'Response.Write("<script>window.open('pricelist_details.aspx?LogNum=" _
            '             & mydataKey.Values.Item(1).ToString & "&Edit=" & _canEditYN & "&PricelistType=" & _pricelistType & "&CntrctNum=" & myFSS & "', 'Pricelist_Detail','toolbar=0,status=0,menubar=0," _
            '              & "scrollbars=1,resizable=0,top=300,width=750,height=500, resizable=0')</Script>")
        ElseIf e.CommandName = "Remove" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)
            Dim myLogNumber As String = CType(mydataKey.Value, String)
            Dim strSQL As String = ""
            strSQL = String.Format("UPDATE tbl_BPA_pricelist SET Removed = 1, LastModifiedBy = '{0}', LastModificationDate = getdate() WHERE BPALogNumber = {1}", browserSecurity.UserInfo.LoginName, myLogNumber)
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim updateCommand As SqlCommand = New SqlCommand()
                updateCommand.Connection = conn
                updateCommand.CommandText = strSQL
                conn.Open()
                updateCommand.ExecuteNonQuery()
                gvNonStandardBPAPricelist.DataBind()
                fvNonStandardBPAPriceList.DataBind()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the Pricelist remove. " & ex.ToString())
            Finally
                conn.Close()
            End Try

        End If
    End Sub

    ' the service description becomes the item description
    Protected Sub servicesDropDownList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        tbServiceCategory.Text = GetSINAndDescriptionFromServiceList()(1)
    End Sub

    Protected Sub btnServicesCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlServicesAdd.Visible = False
        tbServiceCategory.Text = ""
        tbServiceFSSPrice.Text = ""
        btnServicesOpenAdd.Visible = True
    End Sub
    Protected Sub btnServicesOpenAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        pnlServicesAdd.Visible = True
        btnServicesOpenAdd.Visible = False
    End Sub
    Protected Sub btnSaveServices_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
        Try
            Dim browserSecurity As BrowserSecurity2
            browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

            Dim strSQL2 As String = "exec InsertMedSurgServicePriceFromGUI @UserLogin, @ContractNumber, @ContractorCatalogNumber, @ProductLongDescription, @FSSPrice, @PackageSizePricedOnContract, @SIN, @ServiceDescriptionId, @DateEffective, @ExpirationDate, @CreatedBy, @DateEntered, @LastModifiedBy, @DateModified, @LogNumber OUTPUT "
            ' Dim strSQL As String = "INSERT INTO [tbl_pricelist] ([CntrctNum], [Contractor Catalog Number], [Product Long Description], [FSS Price], [Package Size Priced on Contract], [SIN], [621I_Category_ID], [DateEffective], [ExpirationDate], [CreatedBy], [Date_Entered], [LastModifiedBy], [Date_Modified] ) VALUES (@CntrctNum, @Contractor_Catalog_Number, @Product_Long_Description, @FSS_Price, @Package_Size_Priced_on_Contract, @SIN, @ServiceDescriptionId, @DateEffective, @ExpirationDate, @CreatedBy, @Date_Entered, @LastModifiedBy, @Date_Modified )"
            Dim insertCommand As SqlCommand = New SqlCommand()
            insertCommand.Connection = conn
            insertCommand.CommandText = strSQL2

            'insertCommand.Parameters.Add("@Contractor_Catalog_Number", SqlDbType.NVarChar).Value = "Services"
            'insertCommand.Parameters.Add("@CntrctNum", SqlDbType.NVarChar).Value = _currentContractNumber
            'insertCommand.Parameters.Add("@SIN", SqlDbType.NVarChar).Value = GetIDSINFromServiceList()(1)
            'insertCommand.Parameters.Add("@Product_Long_Description", SqlDbType.NVarChar).Value = servicesDropDownList.SelectedItem.Text
            'insertCommand.Parameters.Add("@FSS_Price", SqlDbType.Decimal).Value = CType(tbServiceFSSPrice.Text, Decimal)
            'insertCommand.Parameters.Add("@Package_Size_Priced_on_Contract", SqlDbType.NVarChar).Value = "HR"
            'insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = ServiceItemEffectiveDateTextBox.Text
            'insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = ServiceItemExpirationDateTextBox.Text

            'insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            'insertCommand.Parameters.Add("@Date_Entered", SqlDbType.DateTime).Value = DateTime.Now
            'insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar).Value = browserSecurity.UserInfo.LoginName
            'insertCommand.Parameters.Add("@Date_Modified", SqlDbType.DateTime).Value = DateTime.Now

            insertCommand.Parameters.Add("@UserLogin", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@ContractNumber", SqlDbType.NVarChar, 50).Value = _currentContractNumber
            insertCommand.Parameters.Add("@ContractorCatalogNumber", SqlDbType.NVarChar, 50).Value = "Services"
            insertCommand.Parameters.Add("@ProductLongDescription", SqlDbType.NVarChar, 800).Value = tbServiceCategory.Text ' servicesDropDownList.SelectedItem.Text
            insertCommand.Parameters.Add("@FSSPrice", SqlDbType.Decimal).Value = CType(tbServiceFSSPrice.Text, Decimal)
            insertCommand.Parameters.Add("@PackageSizePricedOnContract", SqlDbType.NVarChar, 2).Value = "HR"
            insertCommand.Parameters.Add("@SIN", SqlDbType.NVarChar, 50).Value = GetIDSINFromServiceList()(1)
            insertCommand.Parameters.Add("@ServiceDescriptionId", SqlDbType.Int).Value = GetIDSINFromServiceList()(0)
            insertCommand.Parameters.Add("@DateEffective", SqlDbType.DateTime).Value = ServiceItemEffectiveDateTextBox.Text
            insertCommand.Parameters.Add("@ExpirationDate", SqlDbType.DateTime).Value = ServiceItemExpirationDateTextBox.Text

            insertCommand.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@DateEntered", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LastModifiedBy", SqlDbType.NVarChar, 120).Value = browserSecurity.UserInfo.LoginName
            insertCommand.Parameters.Add("@DateModified", SqlDbType.DateTime).Value = DateTime.Now
            insertCommand.Parameters.Add("@LogNumber", SqlDbType.Int).Direction = ParameterDirection.Output

            conn.Open()
            insertCommand.ExecuteNonQuery()
            gvServicesPricelist.DataBind()
            fvServicesPriceList.DataBind()
        Catch ex As Exception
            Msg_Alert.Client_Alert_OnLoad("The following error occurred when attempting to insert (6) the price: " & ex.Message)
        Finally
            conn.Close()
        End Try
        pnlServicesAdd.Visible = False
        tbServiceCategory.Text = ""
        tbServiceFSSPrice.Text = ""
        btnServicesOpenAdd.Visible = True

        AddClientCloseEvent()
    End Sub

    Private Function GetIDSINFromServiceList() As String()
        Dim idSIN As String
        Dim idSINArray() As String

        idSIN = CType(servicesDropDownList.SelectedValue, String)
        idSINArray = idSIN.Split("|")

        idSINArray(0) = idSINArray(0).Trim
        idSINArray(1) = idSINArray(1).Trim

        Return idSINArray
    End Function

    Private Function GetSINAndDescriptionFromServiceList() As String()
        Dim sinAndDescription As String
        Dim sinAndDescriptionArray() As String

        sinAndDescription = servicesDropDownList.SelectedItem.Text
        sinAndDescriptionArray = sinAndDescription.Split(":")

        sinAndDescriptionArray(0) = sinAndDescriptionArray(0).Trim
        sinAndDescriptionArray(1) = sinAndDescriptionArray(1).Trim

        Return sinAndDescriptionArray
    End Function

    Protected Sub gvServicesPriceList_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs)

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If e.CommandName.ToString = "seeDetails" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)


            OpenPricelistDetailsWindow(mydataKey.Value, _canEditYN, _pricelistType, _currentContractNumber, _parentContractNumber)

            'ElseIf e.CommandName.ToString = "InsertNew" Then
            '    Dim myindex As Integer = e.CommandArgument
            '    Dim myGrid As GridView = CType(sender, GridView)
            '    Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            '    Dim mydataKey As DataKey = myGrid.DataKeys(myindex)

            '    OpenPricelistDetailsWindow(mydataKey.Value, _canEditYN, _pricelistType, _currentContractNumber, _parentContractNumber)

            'ElseIf e.CommandName.ToString = "InsertEmpty" Then
            '    Dim myDropList As DropDownList = CType(gvPriceList.Controls(0).Controls(0).FindControl("dlEmptySIN"), DropDownList)
            '    Dim myTextBox As TextBox = CType(gvPriceList.Controls(0).Controls(0).FindControl("tbEmptyCatalog"), TextBox)
            '    Dim myCatlogNumber As SqlParameter = New SqlParameter("@Contractor_Catalog_Number", SqlDbType.NVarChar, 50)
            '    Dim mySIN As SqlParameter = New SqlParameter("@SIN", SqlDbType.NVarChar, 50)
            '    myCatlogNumber.Direction = ParameterDirection.Input
            '    mySIN.Direction = ParameterDirection.Input
            '    myCatlogNumber.Value = myTextBox.Text
            '    mySIN.Value = myDropList.SelectedValue
            '    myTextBox = CType(gvPriceList.Controls(0).Controls(0).FindControl("tbEmptyLongDesc"), TextBox)
            '    Dim myLongDesc As SqlParameter = New SqlParameter("@Product_Long_Description", SqlDbType.NVarChar, 800)
            '    myLongDesc.Direction = ParameterDirection.Input
            '    myLongDesc.Value = myTextBox.Text
            '    myTextBox = CType(gvPriceList.Controls(0).Controls(0).FindControl("FSSEmptyPrice"), TextBox)
            '    Dim myFSSPrice As SqlParameter = New SqlParameter("@FSS_Price", SqlDbType.Decimal)
            '    myFSSPrice.Direction = ParameterDirection.Input
            '    myFSSPrice.Value = CType(myTextBox.Text, Decimal)
            '    myDropList = CType(gvPriceList.Controls(0).Controls(0).FindControl("dlEmptyPackSize"), DropDownList)
            '    Dim myPackage As SqlParameter = New SqlParameter("@Package_Size_Priced_on_Contract", SqlDbType.NVarChar, 2)
            '    myPackage.Direction = ParameterDirection.Input
            '    myPackage.Value = myDropList.SelectedValue
            '    Dim myCntrctNum As SqlParameter = New SqlParameter("@CntrctNum", SqlDbType.NVarChar, 50)
            '    myCntrctNum.Direction = ParameterDirection.Input
            '    myCntrctNum.Value = _currentContractNumber
            '    insertPriceParameter(0) = myCatlogNumber
            '    insertPriceParameter(1) = mySIN
            '    insertPriceParameter(2) = myLongDesc
            '    insertPriceParameter(3) = myFSSPrice
            '    insertPriceParameter(4) = myPackage
            '    insertPriceParameter(5) = myCntrctNum
            '    pricelistDataSource.Insert()
        ElseIf e.CommandName = "Remove" Then
            Dim myindex As Integer = e.CommandArgument
            Dim myGrid As Global.System.Web.UI.WebControls.GridView = CType(sender, Global.System.Web.UI.WebControls.GridView)
            Dim mydataRow As GridViewRow = myGrid.Rows(myindex)
            Dim mydataKey As DataKey = myGrid.DataKeys(myindex)
            Dim myLogNumber As String = CType(mydataKey.Value, String)
            Dim strSQL As String = ""
            strSQL = String.Format("UPDATE tbl_pricelist SET Removed = 1, LastModifiedBy = '{0}', Date_Modified = getdate() WHERE LogNumber = {1}", browserSecurity.UserInfo.LoginName, myLogNumber)
            Dim conn As New SqlConnection(ConfigurationManager.ConnectionStrings("CM").ConnectionString)
            Try
                Dim updateCommand As SqlCommand = New SqlCommand()
                updateCommand.Connection = conn
                updateCommand.CommandText = strSQL
                conn.Open()
                updateCommand.ExecuteNonQuery()
                gvServicesPricelist.DataBind()
                fvServicesPriceList.DataBind()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad("This is a result of the 621i Pricelist insert. " & ex.ToString)
            Finally
                conn.Close()
            End Try

        End If
    End Sub
    'Protected Sub FSSPriceListDataSource_OnInserting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles FSSPriceListDataSource.Inserting
    '    e.Command.Parameters.Clear()
    '    Dim i As Integer
    '    For i = 0 To 5
    '        e.Command.Parameters.Add(insertPriceParameter(i))
    '    Next i
    'End Sub


    'Protected Sub FSSPriceListDataSource_OnUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceCommandEventArgs) Handles FSSPriceListDataSource.Updating

    'End Sub
End Class
