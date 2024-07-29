Imports System.Data
Imports System.Data.Common
Imports System.Data.SqlClient
Imports VA.NAC.NACCMBrowser.BrowserObj
Imports System.Collections
Imports System.Collections.Generic

Imports TextBox = System.Web.UI.WebControls.TextBox
Imports DropDownList = System.Web.UI.WebControls.DropDownList

Partial Public Class pricelist_details
    Inherits System.Web.UI.Page
    Private _currentContractNumber As String = ""
    Private _parentContractNumber As String = ""
    Protected _pricelistType As String = ""
    Private _canEditYN As String = "N"
    Private _logNumberString As String = ""
    Private _parentLogNumberString As String = ""

    Private _pricelistDetailDataSource As DocumentDataSource
    Private _pricelistDetailNewFSSForBPADataSource As SqlDataSource

    Private _pricelistTypeParameter As Parameter
    Private _logNumberParameter As Parameter ' for select
    Private _fssLogNumberParameter As Parameter
    Private _bpaLogNumberParameter As Parameter
    Private _fssLogNumberForRefreshParameter As Parameter

    Private _Contractor_Catalog_NumberParameter As Parameter
    Private _Product_Long_DescriptionParameter As Parameter
    Private _BPADescriptionParameter As Parameter
    Private _FSS_PriceParameter As Parameter
    Private _BPA_PriceParameter As Parameter
    Private _Package_Size_Priced_on_ContractParameter As Parameter
    Private _SINParameter As Parameter
    Private _OriginalSINValue As String
    'Private _Outer_Pack_UOMParameter As Parameter
    'Private _Outer_Pack_Unit_of_Conversion_FactorParameter As Parameter
    'Private _Outer_Pack_Unit_ShippableParameter As Parameter
    'Private _Outer_Pack_UPNParameter As Parameter
    'Private _Intermediate_Pack_UOMParameter As Parameter
    'Private _Intermediate_Pack_Unit_of_Conversion_FactorParameter As Parameter
    'Private _Intermediate_Pack_ShippableParameter As Parameter
    'Private _Intermediate_Pack_UPNParameter As Parameter
    'Private _Base_Packaging_UOMParameter As Parameter
    'Private _Base_Packaging_Unit_of_Conversion_FactorParameter As Parameter
    'Private _Base_Packaging_Unit_ShippableParameter As Parameter
    'Private _Base_Packaging_UPNParameter As Parameter
    Private _Tier_1_PriceParameter As Parameter
    Private _Tier_2_PriceParameter As Parameter
    Private _Tier_3_PriceParameter As Parameter
    Private _Tier_4_PriceParameter As Parameter
    Private _Tier_5_PriceParameter As Parameter
    Private _Tier_1_NoteParameter As Parameter
    Private _Tier_2_NoteParameter As Parameter
    Private _Tier_3_NoteParameter As Parameter
    Private _Tier_4_NoteParameter As Parameter
    Private _Tier_5_NoteParameter As Parameter
    Private _ServiceCategoryIdParameter As Parameter
    Private _FSSDateEffectiveParameter As Parameter
    Private _FSSExpirationDateParameter As Parameter
    Private _BPADateEffectiveParameter As Parameter
    Private _BPAExpirationDateParameter As Parameter
    Private _LastModifiedByParameter As Parameter


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim pricelistType As String = ""

        If Not Request.QueryString("LogNumber") Is Nothing Then
            _logNumberString = CType(Request.QueryString("LogNumber"), String)
        End If

        If Not Request.QueryString("PricelistType") = Nothing Then
            'B for BPA, F for FSS, 6 for 621, NB for non-standard agreement
            _pricelistType = Request.QueryString("PricelistType")
        End If

        If Not Request.QueryString("Edit") Is Nothing Then
            _canEditYN = CType(Request.QueryString("Edit"), String)
        End If

        If Not Request.QueryString("CntrctNum") Is Nothing Then
            _currentContractNumber = CType(Request.QueryString("CntrctNum"), String)
        End If

        If _pricelistType.CompareTo("B") = 0 Then

            If Not Request.QueryString("ParentContractNumber") Is Nothing Then
                _parentContractNumber = CType(Request.QueryString("ParentContractNumber"), String)
            End If

            If Session("ParentLogNumberString") Is Nothing Then
                If Not Request.QueryString("ParentLogNumber") Is Nothing Then
                    _parentLogNumberString = CType(Request.QueryString("ParentLogNumber"), String)
                End If
            Else
                _parentLogNumberString = Session("ParentLogNumberString").ToString()
            End If
        End If

        If Session("PricelistDetailDataSource") Is Nothing Then
            'create the main data source
            _pricelistDetailDataSource = New DocumentDataSource(CType(Session("BrowserSecurity"), BrowserSecurity2), VA.NAC.NACCMBrowser.DBInterface.DBCommon.TargetDatabases.NACCMCommonUser, False)  ' New SqlDataSource()
            '    _pricelistDetailDataSource.ConnectionString = ConfigurationManager.ConnectionStrings("CM").ConnectionString
            _pricelistDetailDataSource.ID = "PricelistDetailDataSource"
            _pricelistDetailDataSource.DataSourceMode = SqlDataSourceMode.DataSet
            _pricelistDetailDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure
            _pricelistDetailDataSource.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure

            _pricelistDetailDataSource.SelectCommand = "SelectPricelistDetailForMedSurgItem"

            If _pricelistType.CompareTo("F") = 0 Then
                _pricelistDetailDataSource.UpdateCommand = "UpdatePricelistDetailForMedSurgFSSItem"
            ElseIf _pricelistType.CompareTo("6") = 0 Then
                _pricelistDetailDataSource.UpdateCommand = "UpdatePricelistDetailForMedSurgServiceItem"
            ElseIf _pricelistType.CompareTo("B") = 0 Then
                _pricelistDetailDataSource.UpdateCommand = "UpdatePricelistDetailForMedSurgBPAItem"
            ElseIf _pricelistType.CompareTo("NB") = 0 Then
                _pricelistDetailDataSource.UpdateCommand = "UpdatePricelistDetailForMedSurgNonStandardBPAItem"
            End If

            CreatePricelistDetailParameters()
            AddPricelistDetailParameters()

            Session("PricelistDetailDataSource") = _pricelistDetailDataSource

        Else
            _pricelistDetailDataSource = CType(Session("PricelistDetailDataSource"), DocumentDataSource)

            RestorePricelistDetailParameters()

            _OriginalSINValue = CType(Session("OriginalSINValueForPricelistDetail"), String)
        End If

        If Session("PricelistDetailNewFSSForBPADataSource") Is Nothing Then
            'create data source for refreshing FSS portion
            _pricelistDetailNewFSSForBPADataSource = New DocumentDataSource(CType(Session("BrowserSecurity"), BrowserSecurity2), VA.NAC.NACCMBrowser.DBInterface.DBCommon.TargetDatabases.NACCMCommonUser, False)  ' New SqlDataSource()
            '  _pricelistDetailNewFSSForBPADataSource.ConnectionString = ConfigurationManager.ConnectionStrings("CM").ConnectionString
            _pricelistDetailNewFSSForBPADataSource.ID = "PricelistDetailNewFSSForBPADataSource"
            _pricelistDetailNewFSSForBPADataSource.DataSourceMode = SqlDataSourceMode.DataReader
            _pricelistDetailNewFSSForBPADataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure

            _pricelistDetailNewFSSForBPADataSource.SelectCommand = "SelectPricelistDetailNewFSSForBPA"

            _fssLogNumberForRefreshParameter = New Parameter("FSSLogNumber", TypeCode.Int32)
            _pricelistDetailNewFSSForBPADataSource.SelectParameters.Add(_fssLogNumberForRefreshParameter)

            Session("PricelistDetailNewFSSForBPADataSource") = _pricelistDetailNewFSSForBPADataSource
        Else
            _pricelistDetailNewFSSForBPADataSource = CType(Session("PricelistDetailNewFSSForBPADataSource"), SqlDataSource)

            _fssLogNumberForRefreshParameter = _pricelistDetailNewFSSForBPADataSource.SelectParameters("FSSLogNumber")
        End If

        fvPricelistDetail.DefaultMode = FormViewMode.Edit
        fvPricelistDetail.ChangeMode(FormViewMode.Edit)
        fvPricelistDetail.DataSource = _pricelistDetailDataSource

        Dim keyArray(0) As String
        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then
            keyArray(0) = "LogNumber"
            fvPricelistDetail.DataKeyNames = keyArray
        ElseIf _pricelistType.CompareTo("B") = 0 Or _pricelistType.CompareTo("NB") = 0 Then
            keyArray(0) = "BPALogNumber"
            fvPricelistDetail.DataKeyNames = keyArray
        End If

        fvPricelistDetail.Visible = True


        If Not IsPostBack Then
            Try
                GetSelectParameterValues()
                fvPricelistDetail.DataBind()
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad(String.Format("The following exception was encountered while binding to the data source: {0}", ex.Message))
            End Try

        End If

    End Sub
    Protected Sub fvPricelistDetail_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles fvPricelistDetail.PreRender
        SetVisibilityOfControlsForPricelistType()
        SetEditingOfControlsForPricelistTypeAndEditingStatus()

        Dim fvPricelistDetail As FormView = CType(sender, FormView)

        'adjust the height
        'Dim MainPricelistDetailTable As HtmlTable = CType(fvPricelistDetail.FindControl("MainPricelistDetailTable"), HtmlTable)
        'If Not MainPricelistDetailTable Is Nothing Then
        '    If _pricelistType = "NB" Then
        '        'style="height: 508px; table-layout:fixed;"
        '        'MainPricelistDetailTable.Attributes.Add("style", "height: 508px;")
        '        'MainPricelistDetailTable.Attributes.Add("width", "730px")
        '    Else
        '        'style="height: 608px; table-layout:fixed;"
        '        'MainPricelistDetailTable.Attributes.Add("style", "height: 608px; table-layout:fixed;")
        '        'MainPricelistDetailTable.Attributes.Add("width", "830px")
        '    End If
        'End If

        Dim closeFormButton As Button = CType(fvPricelistDetail.FindControl("closeFormButton"), Button)
        If Not closeFormButton Is Nothing Then
            Dim closeFunctionText As String = String.Format("CloseWindow('{0}');", _pricelistType)
            closeFormButton.Attributes.Add("onclick", closeFunctionText)
        End If
    End Sub
    Private Sub CreatePricelistDetailParameters()
        _pricelistTypeParameter = New Parameter("PricelistType", TypeCode.String)
        _logNumberParameter = New Parameter("LogNumber", TypeCode.Int32)
        _fssLogNumberParameter = New Parameter("FSSLogNumber", TypeCode.Int32)
        _bpaLogNumberParameter = New Parameter("BPALogNumber", TypeCode.Int32)
        _Contractor_Catalog_NumberParameter = New Parameter("ContractorCatalogNumber", TypeCode.String)
        _Product_Long_DescriptionParameter = New Parameter("ProductLongDescription", TypeCode.String)
        _BPADescriptionParameter = New Parameter("BPADescription", TypeCode.String)
        _FSS_PriceParameter = New Parameter("FSSPrice", TypeCode.Decimal)
        _BPA_PriceParameter = New Parameter("BPAPrice", TypeCode.Decimal)
        _Package_Size_Priced_on_ContractParameter = New Parameter("PackageSizePricedOnContract", TypeCode.String)
        _SINParameter = New Parameter("SIN", TypeCode.String)
        '_Outer_Pack_UOMParameter = New Parameter("OuterPackUOM", TypeCode.String)
        '_Outer_Pack_Unit_of_Conversion_FactorParameter = New Parameter("OuterPackUnitOfConversionFactor", TypeCode.Int32)
        '_Outer_Pack_Unit_ShippableParameter = New Parameter("OuterPackUnitShippable", TypeCode.Boolean)
        '_Outer_Pack_UPNParameter = New Parameter("OuterPackUPN", TypeCode.String)
        '_Intermediate_Pack_UOMParameter = New Parameter("IntermediatePackUOM", TypeCode.String)
        '_Intermediate_Pack_Unit_of_Conversion_FactorParameter = New Parameter("IntermediatePackUnitOfConversionFactor", TypeCode.Int32)
        '_Intermediate_Pack_ShippableParameter = New Parameter("IntermediatePackShippable", TypeCode.Boolean)
        '_Intermediate_Pack_UPNParameter = New Parameter("IntermediatePackUPN", TypeCode.String)
        '_Base_Packaging_UOMParameter = New Parameter("BasePackUOM", TypeCode.String)
        '_Base_Packaging_Unit_of_Conversion_FactorParameter = New Parameter("BasePackUnitOfConversionFactor", TypeCode.Int32)
        '_Base_Packaging_Unit_ShippableParameter = New Parameter("BasePackUnitShippable", TypeCode.Boolean)
        '_Base_Packaging_UPNParameter = New Parameter("BasePackUPN", TypeCode.String)
        _Tier_1_PriceParameter = New Parameter("Tier1Price", TypeCode.Decimal)
        _Tier_2_PriceParameter = New Parameter("Tier2Price", TypeCode.Decimal)
        _Tier_3_PriceParameter = New Parameter("Tier3Price", TypeCode.Decimal)
        _Tier_4_PriceParameter = New Parameter("Tier4Price", TypeCode.Decimal)
        _Tier_5_PriceParameter = New Parameter("Tier5Price", TypeCode.Decimal)
        _Tier_1_NoteParameter = New Parameter("Tier1Note", TypeCode.String)
        _Tier_2_NoteParameter = New Parameter("Tier2Note", TypeCode.String)
        _Tier_3_NoteParameter = New Parameter("Tier3Note", TypeCode.String)
        _Tier_4_NoteParameter = New Parameter("Tier4Note", TypeCode.String)
        _Tier_5_NoteParameter = New Parameter("Tier5Note", TypeCode.String)
        _ServiceCategoryIdParameter = New Parameter("ServiceCategoryId", TypeCode.Int32)
        _FSSDateEffectiveParameter = New Parameter("FSSEffectiveDate", TypeCode.DateTime)
        _FSSExpirationDateParameter = New Parameter("FSSExpirationDate", TypeCode.DateTime)
        _BPADateEffectiveParameter = New Parameter("BPAEffectiveDate", TypeCode.DateTime)
        _BPAExpirationDateParameter = New Parameter("BPAExpirationDate", TypeCode.DateTime)
        _LastModifiedByParameter = New Parameter("LastModifiedBy", TypeCode.String)

    End Sub
    Private Sub RestorePricelistDetailParameters()
        'select parameters
        _pricelistTypeParameter = _pricelistDetailDataSource.SelectParameters("PricelistType")
        _logNumberParameter = _pricelistDetailDataSource.SelectParameters("LogNumber")

        'update parameters
        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then
            _fssLogNumberParameter = _pricelistDetailDataSource.UpdateParameters("FSSLogNumber")
            _Contractor_Catalog_NumberParameter = _pricelistDetailDataSource.UpdateParameters("ContractorCatalogNumber")
            _Product_Long_DescriptionParameter = _pricelistDetailDataSource.UpdateParameters("ProductLongDescription")
            _FSS_PriceParameter = _pricelistDetailDataSource.UpdateParameters("FSSPrice")
            _Package_Size_Priced_on_ContractParameter = _pricelistDetailDataSource.UpdateParameters("PackageSizePricedOnContract")
            _SINParameter = _pricelistDetailDataSource.UpdateParameters("SIN")
            '_Outer_Pack_UOMParameter = _pricelistDetailDataSource.UpdateParameters("OuterPackUOM")
            '_Outer_Pack_Unit_of_Conversion_FactorParameter = _pricelistDetailDataSource.UpdateParameters("OuterPackUnitOfConversionFactor")
            '_Outer_Pack_Unit_ShippableParameter = _pricelistDetailDataSource.UpdateParameters("OuterPackUnitShippable")
            '_Outer_Pack_UPNParameter = _pricelistDetailDataSource.UpdateParameters("OuterPackUPN")
            '_Intermediate_Pack_UOMParameter = _pricelistDetailDataSource.UpdateParameters("IntermediatePackUOM")
            '_Intermediate_Pack_Unit_of_Conversion_FactorParameter = _pricelistDetailDataSource.UpdateParameters("IntermediatePackUnitOfConversionFactor")
            '_Intermediate_Pack_ShippableParameter = _pricelistDetailDataSource.UpdateParameters("IntermediatePackShippable")
            '_Intermediate_Pack_UPNParameter = _pricelistDetailDataSource.UpdateParameters("IntermediatePackUPN")
            '_Base_Packaging_UOMParameter = _pricelistDetailDataSource.UpdateParameters("BasePackUOM")
            '_Base_Packaging_Unit_of_Conversion_FactorParameter = _pricelistDetailDataSource.UpdateParameters("BasePackUnitOfConversionFactor")
            '_Base_Packaging_Unit_ShippableParameter = _pricelistDetailDataSource.UpdateParameters("BasePackUnitShippable")
            '_Base_Packaging_UPNParameter = _pricelistDetailDataSource.UpdateParameters("BasePackUPN")
            _Tier_1_PriceParameter = _pricelistDetailDataSource.UpdateParameters("Tier1Price")
            _Tier_2_PriceParameter = _pricelistDetailDataSource.UpdateParameters("Tier2Price")
            _Tier_3_PriceParameter = _pricelistDetailDataSource.UpdateParameters("Tier3Price")
            _Tier_4_PriceParameter = _pricelistDetailDataSource.UpdateParameters("Tier4Price")
            _Tier_5_PriceParameter = _pricelistDetailDataSource.UpdateParameters("Tier5Price")
            _Tier_1_NoteParameter = _pricelistDetailDataSource.UpdateParameters("Tier1Note")
            _Tier_2_NoteParameter = _pricelistDetailDataSource.UpdateParameters("Tier2Note")
            _Tier_3_NoteParameter = _pricelistDetailDataSource.UpdateParameters("Tier3Note")
            _Tier_4_NoteParameter = _pricelistDetailDataSource.UpdateParameters("Tier4Note")
            _Tier_5_NoteParameter = _pricelistDetailDataSource.UpdateParameters("Tier5Note")
            _FSSDateEffectiveParameter = _pricelistDetailDataSource.UpdateParameters("FSSEffectiveDate")
            _FSSExpirationDateParameter = _pricelistDetailDataSource.UpdateParameters("FSSExpirationDate")
            _LastModifiedByParameter = _pricelistDetailDataSource.UpdateParameters("LastModifiedBy")

            If _pricelistType.CompareTo("6") = 0 Then
                _ServiceCategoryIdParameter = _pricelistDetailDataSource.UpdateParameters("ServiceCategoryId")
            End If

        ElseIf _pricelistType.CompareTo("B") = 0 Then
            _bpaLogNumberParameter = _pricelistDetailDataSource.UpdateParameters("BPALogNumber")
            _fssLogNumberParameter = _pricelistDetailDataSource.UpdateParameters("FSSLogNumber")
            _BPA_PriceParameter = _pricelistDetailDataSource.UpdateParameters("BPAPrice")
            _BPADescriptionParameter = _pricelistDetailDataSource.UpdateParameters("BPADescription")
            _BPADateEffectiveParameter = _pricelistDetailDataSource.UpdateParameters("BPAEffectiveDate")
            _BPAExpirationDateParameter = _pricelistDetailDataSource.UpdateParameters("BPAExpirationDate")
            _LastModifiedByParameter = _pricelistDetailDataSource.UpdateParameters("LastModifiedBy")
        ElseIf _pricelistType.CompareTo("NB") = 0 Then
            _bpaLogNumberParameter = _pricelistDetailDataSource.UpdateParameters("BPALogNumber")
            _BPA_PriceParameter = _pricelistDetailDataSource.UpdateParameters("BPAPrice")
            _BPADescriptionParameter = _pricelistDetailDataSource.UpdateParameters("BPADescription")
            _BPADateEffectiveParameter = _pricelistDetailDataSource.UpdateParameters("BPAEffectiveDate")
            _BPAExpirationDateParameter = _pricelistDetailDataSource.UpdateParameters("BPAExpirationDate")
            _LastModifiedByParameter = _pricelistDetailDataSource.UpdateParameters("LastModifiedBy")
        End If
    End Sub

    Private Sub AddPricelistDetailParameters()
        'select parameters
        _pricelistDetailDataSource.SelectParameters.Add(_pricelistTypeParameter)
        _pricelistDetailDataSource.SelectParameters.Add(_logNumberParameter)

        'update parameters
        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then
            _pricelistDetailDataSource.UpdateParameters.Add(_fssLogNumberParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Contractor_Catalog_NumberParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Product_Long_DescriptionParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_FSS_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Package_Size_Priced_on_ContractParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_SINParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Outer_Pack_UOMParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Outer_Pack_Unit_of_Conversion_FactorParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Outer_Pack_Unit_ShippableParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Outer_Pack_UPNParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Intermediate_Pack_UOMParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Intermediate_Pack_Unit_of_Conversion_FactorParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Intermediate_Pack_ShippableParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Intermediate_Pack_UPNParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Base_Packaging_UOMParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Base_Packaging_Unit_of_Conversion_FactorParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Base_Packaging_Unit_ShippableParameter)
            '_pricelistDetailDataSource.UpdateParameters.Add(_Base_Packaging_UPNParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_1_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_2_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_3_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_4_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_5_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_1_NoteParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_2_NoteParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_3_NoteParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_4_NoteParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_Tier_5_NoteParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_FSSDateEffectiveParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_FSSExpirationDateParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_LastModifiedByParameter)

            If _pricelistType.CompareTo("6") = 0 Then
                _pricelistDetailDataSource.UpdateParameters.Add(_ServiceCategoryIdParameter)
            End If

        ElseIf _pricelistType.CompareTo("B") = 0 Then
            _pricelistDetailDataSource.UpdateParameters.Add(_bpaLogNumberParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_fssLogNumberParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPA_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPADescriptionParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPADateEffectiveParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPAExpirationDateParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_LastModifiedByParameter)
        ElseIf _pricelistType.CompareTo("NB") = 0 Then
            _pricelistDetailDataSource.UpdateParameters.Add(_bpaLogNumberParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPA_PriceParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPADescriptionParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPADateEffectiveParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_BPAExpirationDateParameter)
            _pricelistDetailDataSource.UpdateParameters.Add(_LastModifiedByParameter)
        End If
    End Sub

    Private Sub Save()
        If _canEditYN = "Y" Then
            Try
                GetUpdateParameterValues()
                _pricelistDetailDataSource.Update()
                fvPricelistDetail.DataBind()
                Session("PricelistDetailHasChanged") = True
            Catch ex As Exception
                Msg_Alert.Client_Alert_OnLoad(String.Format("The following exception was encountered while saving the item details: {0}", ex.Message))
            End Try
        End If
    End Sub
    Private Sub DisplayErrorDialog()
        Msg_Alert.Client_Alert_OnLoad("Please correct fields marked ""*"" before saving.")
    End Sub
    Private Sub GetSelectParameterValues()
        _pricelistTypeParameter.DefaultValue = _pricelistType
        _logNumberParameter.DefaultValue = _logNumberString
    End Sub
    Private Sub GetUpdateParameterValues()

        Dim browserSecurity As BrowserSecurity2
        browserSecurity = CType(Session("BrowserSecurity"), BrowserSecurity2)

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then
            'get controls
            Dim FSSPriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSPriceTextBox"), TextBox)
            Dim packageSizePricedOnContractDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("packageSizePricedOnContractDropDownList"), DropDownList)
            Dim FSSEffectiveDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSEffectiveDateTextBox"), TextBox)
            Dim FSSExpirationDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSExpirationDateTextBox"), TextBox)
            Dim contractorCatalogNumberTextBox As TextBox = CType(fvPricelistDetail.FindControl("contractorCatalogNumberTextBox"), TextBox)
            Dim SINDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("SINDropDownList"), DropDownList)
            Dim SINTextBox As TextBox = CType(fvPricelistDetail.FindControl("SINTextBox"), TextBox)
            Dim productLongDescriptionTextBox As TextBox = CType(fvPricelistDetail.FindControl("productLongDescriptionTextBox"), TextBox)
            Dim tier1PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier1PriceTextBox"), TextBox)
            Dim tier2PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier2PriceTextBox"), TextBox)
            Dim tier3PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier3PriceTextBox"), TextBox)
            Dim tier4PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier4PriceTextBox"), TextBox)
            Dim tier5PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier5PriceTextBox"), TextBox)
            Dim tier1NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier1NotesTextBox"), TextBox)
            Dim tier2NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier2NotesTextBox"), TextBox)
            Dim tier3NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier3NotesTextBox"), TextBox)
            Dim tier4NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier4NotesTextBox"), TextBox)
            Dim tier5NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier5NotesTextBox"), TextBox)
            'Dim basePackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("basePackUOMDropDownList"), DropDownList)
            'Dim basePackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("basePackQuantityTextBox"), TextBox)
            'Dim basePackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("basePackUPNTextBox"), TextBox)
            'Dim basePackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("basePackShippableCheckBox"), CheckBox)
            'Dim innerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("innerPackUOMDropDownList"), DropDownList)
            'Dim innerPackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("innerPackQuantityTextBox"), TextBox)
            'Dim innerPackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("innerPackUPNTextBox"), TextBox)
            'Dim innerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("innerPackShippableCheckBox"), CheckBox)
            'Dim outerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("outerPackUOMDropDownList"), DropDownList)
            'Dim outerPackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("outerPackQuantityTextBox"), TextBox)
            'Dim outerPackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("outerPackUPNTextBox"), TextBox)
            'Dim outerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("outerPackShippableCheckBox"), CheckBox)


            'get values

            _fssLogNumberParameter.DefaultValue = _logNumberString

            If FSSPriceTextBox.Text.Length > 0 Then
                _FSS_PriceParameter.DefaultValue = GetMoneyFromString(FSSPriceTextBox.Text, "FSS Price")
            Else
                _FSS_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If

            _Package_Size_Priced_on_ContractParameter.DefaultValue = packageSizePricedOnContractDropDownList.SelectedItem.Text
            _FSSDateEffectiveParameter.DefaultValue = FSSEffectiveDateTextBox.Text
            _FSSExpirationDateParameter.DefaultValue = FSSExpirationDateTextBox.Text
            _Contractor_Catalog_NumberParameter.DefaultValue = contractorCatalogNumberTextBox.Text
            _Product_Long_DescriptionParameter.DefaultValue = productLongDescriptionTextBox.Text

            If tier1PriceTextBox.Text.Length > 0 Then
                _Tier_1_PriceParameter.DefaultValue = GetMoneyFromString(tier1PriceTextBox.Text, "Tier 1 Price")
            Else
                _Tier_1_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If
            If tier2PriceTextBox.Text.Length > 0 Then
                _Tier_2_PriceParameter.DefaultValue = GetMoneyFromString(tier2PriceTextBox.Text, "Tier 2 Price")
            Else
                _Tier_2_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If
            If tier3PriceTextBox.Text.Length > 0 Then
                _Tier_3_PriceParameter.DefaultValue = GetMoneyFromString(tier3PriceTextBox.Text, "Tier 3 Price")
            Else
                _Tier_3_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If
            If tier4PriceTextBox.Text.Length > 0 Then
                _Tier_4_PriceParameter.DefaultValue = GetMoneyFromString(tier4PriceTextBox.Text, "Tier 4 Price")
            Else
                _Tier_4_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If
            If tier5PriceTextBox.Text.Length > 0 Then
                _Tier_5_PriceParameter.DefaultValue = GetMoneyFromString(tier5PriceTextBox.Text, "Tier 5 Price")
            Else
                _Tier_5_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If

            _Tier_1_NoteParameter.DefaultValue = tier1NotesTextBox.Text
            _Tier_2_NoteParameter.DefaultValue = tier2NotesTextBox.Text
            _Tier_3_NoteParameter.DefaultValue = tier3NotesTextBox.Text
            _Tier_4_NoteParameter.DefaultValue = tier4NotesTextBox.Text
            _Tier_5_NoteParameter.DefaultValue = tier5NotesTextBox.Text

            '_Base_Packaging_UOMParameter.DefaultValue = basePackUOMDropDownList.SelectedItem.Text
            '_Base_Packaging_Unit_of_Conversion_FactorParameter.DefaultValue = Integer.Parse(basePackQuantityTextBox.Text)
            '_Base_Packaging_UPNParameter.DefaultValue = basePackUPNTextBox.Text
            '_Base_Packaging_Unit_ShippableParameter.DefaultValue = basePackShippableCheckBox.Checked

            '_Intermediate_Pack_UOMParameter.DefaultValue = innerPackUOMDropDownList.SelectedItem.Text
            '_Intermediate_Pack_Unit_of_Conversion_FactorParameter.DefaultValue = Integer.Parse(innerPackQuantityTextBox.Text)
            '_Intermediate_Pack_UPNParameter.DefaultValue = innerPackUPNTextBox.Text
            '_Intermediate_Pack_ShippableParameter.DefaultValue = innerPackShippableCheckBox.Checked

            '_Outer_Pack_UOMParameter.DefaultValue = outerPackUOMDropDownList.SelectedItem.Text
            '_Outer_Pack_Unit_of_Conversion_FactorParameter.DefaultValue = Integer.Parse(outerPackQuantityTextBox.Text)
            '_Outer_Pack_UPNParameter.DefaultValue = outerPackUPNTextBox.Text
            '_Outer_Pack_Unit_ShippableParameter.DefaultValue = outerPackShippableCheckBox.Checked


            If _pricelistType.CompareTo("6") = 0 Then
                Dim serviceCategoriesDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("serviceCategoriesDropDownList"), DropDownList)

                _ServiceCategoryIdParameter.DefaultValue = GetIDSINFromServiceList()(0)
                _SINParameter.DefaultValue = SINTextBox.Text 'populated from last serv.cat. selection

            Else 'FSS
                Dim selectedSIN As String
                selectedSIN = SINDropDownList.SelectedValue.ToString()

                'do not change value if it is invalid
                If selectedSIN.CompareTo("Invalid") = 0 Then
                    _SINParameter.DefaultValue = _OriginalSINValue
                Else
                    _SINParameter.DefaultValue = selectedSIN
                End If

            End If

            _LastModifiedByParameter.DefaultValue = browserSecurity.UserInfo.LoginName

        ElseIf _pricelistType.CompareTo("B") = 0 Then

            _bpaLogNumberParameter.DefaultValue = _logNumberString

            Dim BPAPriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAPriceTextBox"), TextBox)
            Dim BPAEffectiveDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAEffectiveDateTextBox"), TextBox)
            Dim BPAExpirationDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAExpirationDateTextBox"), TextBox)
            Dim BPADescriptionTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPADescriptionTextBox"), TextBox)
            Dim contractorCatalogNumberDropDownList As DropDownList = CType(fvPricelistDetail.Row.FindControl("contractorCatalogNumberDropDownList"), DropDownList)

            If BPAPriceTextBox.Text.Length > 0 Then
                _BPA_PriceParameter.DefaultValue = GetMoneyFromString(BPAPriceTextBox.Text, "BPA Price")
            Else
                _BPA_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If

            _BPADateEffectiveParameter.DefaultValue = BPAEffectiveDateTextBox.Text
            _BPAExpirationDateParameter.DefaultValue = BPAExpirationDateTextBox.Text
            _BPADescriptionParameter.DefaultValue = BPADescriptionTextBox.Text

            _parentLogNumberString = contractorCatalogNumberDropDownList.SelectedValue()
            _fssLogNumberParameter.DefaultValue = _parentLogNumberString
            Session("ParentLogNumberString") = _parentLogNumberString

            _LastModifiedByParameter.DefaultValue = browserSecurity.UserInfo.LoginName

        ElseIf _pricelistType.CompareTo("NB") = 0 Then

            _bpaLogNumberParameter.DefaultValue = _logNumberString

            Dim BPAPriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAPriceTextBox"), TextBox)
            Dim BPAEffectiveDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAEffectiveDateTextBox"), TextBox)
            Dim BPAExpirationDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPAExpirationDateTextBox"), TextBox)
            Dim BPADescriptionTextBox As TextBox = CType(fvPricelistDetail.FindControl("BPADescriptionTextBox"), TextBox)

            If BPAPriceTextBox.Text.Length > 0 Then
                _BPA_PriceParameter.DefaultValue = GetMoneyFromString(BPAPriceTextBox.Text, "BPA Price")
            Else
                _BPA_PriceParameter.DefaultValue = DBNull.Value.ToString()
            End If

            _BPADateEffectiveParameter.DefaultValue = BPAEffectiveDateTextBox.Text
            _BPAExpirationDateParameter.DefaultValue = BPAExpirationDateTextBox.Text
            _BPADescriptionParameter.DefaultValue = BPADescriptionTextBox.Text

            _LastModifiedByParameter.DefaultValue = browserSecurity.UserInfo.LoginName

        End If


    End Sub
    Private Function GetMoneyFromString(ByVal moneyString As String, ByVal fieldName As String) As Decimal
        Dim moneyPart As String
        Dim moneyNumber As Decimal
        If moneyString.Contains("$") Then
            moneyPart = moneyString.Split("$")(1)
        Else
            moneyPart = moneyString
        End If

        If Decimal.TryParse(moneyPart, moneyNumber) Then
            Return moneyNumber
        Else
            Throw New Exception(fieldName + " is not in a correct format.")
        End If
    End Function
    Private Sub RefreshFSSPortion()

        Dim FSSPriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSPriceTextBox"), TextBox)

        Dim packageSizePricedOnContractTextBox As TextBox = CType(fvPricelistDetail.FindControl("packageSizePricedOnContractTextBox"), TextBox)

        Dim FSSEffectiveDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSEffectiveDateTextBox"), TextBox)
        Dim FSSExpirationDateTextBox As TextBox = CType(fvPricelistDetail.FindControl("FSSExpirationDateTextBox"), TextBox)

        Dim SINTextBox As TextBox = CType(fvPricelistDetail.FindControl("SINTextBox"), TextBox)

        Dim productLongDescriptionTextBox As TextBox = CType(fvPricelistDetail.FindControl("productLongDescriptionTextBox"), TextBox)
        Dim tier1PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier1PriceTextBox"), TextBox)
        Dim tier2PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier2PriceTextBox"), TextBox)
        Dim tier3PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier3PriceTextBox"), TextBox)
        Dim tier4PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier4PriceTextBox"), TextBox)
        Dim tier5PriceTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier5PriceTextBox"), TextBox)
        Dim tier1NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier1NotesTextBox"), TextBox)
        Dim tier2NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier2NotesTextBox"), TextBox)
        Dim tier3NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier3NotesTextBox"), TextBox)
        Dim tier4NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier4NotesTextBox"), TextBox)
        Dim tier5NotesTextBox As TextBox = CType(fvPricelistDetail.FindControl("tier5NotesTextBox"), TextBox)
        'Dim basePackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("basePackUOMDropDownList"), DropDownList)
        'Dim basePackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("basePackQuantityTextBox"), TextBox)
        'Dim basePackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("basePackUPNTextBox"), TextBox)
        'Dim basePackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("basePackShippableCheckBox"), CheckBox)
        'Dim innerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("innerPackUOMDropDownList"), DropDownList)
        'Dim innerPackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("innerPackQuantityTextBox"), TextBox)
        'Dim innerPackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("innerPackUPNTextBox"), TextBox)
        'Dim innerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("innerPackShippableCheckBox"), CheckBox)
        'Dim outerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("outerPackUOMDropDownList"), DropDownList)
        'Dim outerPackQuantityTextBox As TextBox = CType(fvPricelistDetail.FindControl("outerPackQuantityTextBox"), TextBox)
        'Dim outerPackUPNTextBox As TextBox = CType(fvPricelistDetail.FindControl("outerPackUPNTextBox"), TextBox)
        'Dim outerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("outerPackShippableCheckBox"), CheckBox)

        Dim resultsIterator As IEnumerable
        resultsIterator = _pricelistDetailNewFSSForBPADataSource.Select(System.Web.UI.DataSourceSelectArguments.Empty)

        'there is only one record
        For Each record As DataRowView In resultsIterator  ' switched code from DbDataRecord on 5/1/2014
            Dim FSSPrice As Decimal
            FSSPrice = Decimal.Parse(record("FSS_Price").ToString())
            FSSPriceTextBox.Text = String.Format("{0:c}", FSSPrice)
            packageSizePricedOnContractTextBox.Text = record("Package_Size_Priced_on_Contract").ToString()

            Dim FSSEffectiveDate As DateTime
            If DateTime.TryParse(record("FSSDateEffective").ToString(), FSSEffectiveDate) = True Then
                FSSEffectiveDateTextBox.Text = FSSEffectiveDate.ToString("d")
            End If

            Dim FSSExpirationDate As DateTime
            If DateTime.TryParse(record("FSSExpirationDate").ToString(), FSSExpirationDate) = True Then
                FSSExpirationDateTextBox.Text = FSSExpirationDate.ToString("d")
            End If

            SINTextBox.Text = record("SIN").ToString()
            productLongDescriptionTextBox.Text = record("Product_Long_Description").ToString()

            tier1PriceTextBox.Text = String.Format("{0:c}", record("Tier_1_Price").ToString())
            tier2PriceTextBox.Text = String.Format("{0:c}", record("Tier_2_Price").ToString())
            tier3PriceTextBox.Text = String.Format("{0:c}", record("Tier_3_Price").ToString())
            tier4PriceTextBox.Text = String.Format("{0:c}", record("Tier_4_Price").ToString())
            tier5PriceTextBox.Text = String.Format("{0:c}", record("Tier_5_Price").ToString())
            tier1NotesTextBox.Text = record("Tier_1_Note").ToString()
            tier2NotesTextBox.Text = record("Tier_2_Note").ToString()
            tier3NotesTextBox.Text = record("Tier_3_Note").ToString()
            tier4NotesTextBox.Text = record("Tier_4_Note").ToString()
            tier5NotesTextBox.Text = record("Tier_5_Note").ToString()

            'basePackUOMDropDownList.SelectedValue = record("Base_Packaging_UOM").ToString()
            'basePackQuantityTextBox.Text = record("Base_Packaging_Unit_of_Conversion_Factor").ToString()
            'basePackUPNTextBox.Text = record("Base_Packaging_UPN").ToString()
            'basePackShippableCheckBox.Checked = record("Base_Packaging_Unit_Shippable")

            'innerPackUOMDropDownList.SelectedValue = record("Intermediate_Pack_UOM").ToString()
            'innerPackQuantityTextBox.Text = record("Intermediate_Pack_Unit_of_Conversion_Factor").ToString()
            'innerPackUPNTextBox.Text = record("Intermediate_Pack_UPN").ToString()
            'innerPackShippableCheckBox.Checked = record("Intermediate_Pack_Shippable")

            'outerPackUOMDropDownList.SelectedValue = record("Outer_Pack_UOM").ToString()
            'outerPackQuantityTextBox.Text = record("Outer_Pack_Unit_of_Conversion_Factor").ToString()
            'outerPackUPNTextBox.Text = record("Outer_Pack_UPN").ToString()
            'outerPackShippableCheckBox.Checked = record("Outer_Pack_Unit_Shippable")

        Next

    End Sub
    'Protected Sub Close_Click(ByVal S As Object, ByVal e As EventArgs)
    '    Response.Write("<script>window.close();</script>")
    'End Sub
    Protected Sub Print_Click(ByVal sender As Object, ByVal e As EventArgs)
        Response.Write("<script language='javascript'>window.print()</script>")
    End Sub
    Protected Sub Save_Click(ByVal sender As Object, ByVal e As EventArgs)
        If Page.IsValid = True Then
            Save()
        Else
            DisplayErrorDialog()
        End If
    End Sub
    ' the service description becomes the item description
    Protected Sub serviceCategoriesDropDownList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim productLongDescriptionTextBox As TextBox = CType(fvPricelistDetail.FindControl("productLongDescriptionTextBox"), TextBox)
        Dim SINTextBox As TextBox = CType(fvPricelistDetail.FindControl("SINTextBox"), TextBox)
        Dim serviceCategoriesDropDownList As DropDownList = CType(sender, DropDownList)
        productLongDescriptionTextBox.Text = serviceCategoriesDropDownList.SelectedItem.Text 'TO DROP SIN FROM DESC CAN USE: GetSINAndDescriptionFromServiceList()(1)
        SINTextBox.Text = GetSINAndDescriptionFromServiceList()(0)
    End Sub
    ' selecting new parent for bpa item
    Protected Sub contractorCatalogNumberDropDownList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim contractorCatalogNumberDropDownList As DropDownList = CType(sender, DropDownList)
        Dim newFSSLogNumber As Integer
        newFSSLogNumber = contractorCatalogNumberDropDownList.SelectedValue
        _fssLogNumberForRefreshParameter.DefaultValue = newFSSLogNumber
        'save for postback
        Session("ParentLogNumberString") = newFSSLogNumber
        RefreshFSSPortion()
    End Sub
    ' selecting new SIN
    'Protected Sub SINDropDownList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    '    Dim SINDropDownList As DropDownList = CType(sender, DropDownList)
    '    Dim newSIN As String
    '    newSIN = SINDropDownList.SelectedValue
    '    _fssLogNumberForRefreshParameter.DefaultValue = newFSSLogNumber
    'End Sub
    Private Function GetIDSINFromServiceList() As String()
        Dim idSIN As String
        Dim idSINArray() As String

        Dim serviceCategoriesDropDownList As DropDownList
        serviceCategoriesDropDownList = CType(fvPricelistDetail.Row.FindControl("serviceCategoriesDropDownList"), DropDownList)
        idSIN = CType(serviceCategoriesDropDownList.SelectedValue, String)
        idSINArray = idSIN.Split("|")

        idSINArray(0) = idSINArray(0).Trim
        idSINArray(1) = idSINArray(1).Trim

        Return idSINArray
    End Function

    Private Function GetSINAndDescriptionFromServiceList() As String()
        Dim sinAndDescription As String
        Dim sinAndDescriptionArray() As String

        Dim serviceCategoriesDropDownList As DropDownList
        serviceCategoriesDropDownList = CType(fvPricelistDetail.Row.FindControl("serviceCategoriesDropDownList"), DropDownList)

        sinAndDescription = serviceCategoriesDropDownList.SelectedItem.Text
        sinAndDescriptionArray = sinAndDescription.Split(":")

        sinAndDescriptionArray(0) = sinAndDescriptionArray(0).Trim
        sinAndDescriptionArray(1) = sinAndDescriptionArray(1).Trim

        Return sinAndDescriptionArray
    End Function

    Private Sub SetServiceListFromId(ByVal serviceId As Integer)
        Dim serviceCategoriesDropDownList As DropDownList
        Dim idSIN As String
        Dim idSINArray() As String
        Dim testId As Integer

        serviceCategoriesDropDownList = CType(fvPricelistDetail.Row.FindControl("serviceCategoriesDropDownList"), DropDownList)

        For Each serviceItem As ListItem In serviceCategoriesDropDownList.Items
            idSIN = CType(serviceItem.Value, String)
            If idSIN.Length > 0 Then
                idSINArray = idSIN.Split("|")
                idSINArray(0) = idSINArray(0).Trim
                testId = Integer.Parse(idSINArray(0))
                If testId = serviceId Then
                    serviceItem.Selected = True
                    Exit For
                End If
            End If
        Next

    End Sub
    Private Function GetServiceDescriptionFromId(ByVal serviceId As Integer) As String
        Dim serviceCategoriesDropDownList As DropDownList
        Dim idSIN As String
        Dim idSINArray() As String
        Dim testId As Integer
        Dim sinAndDescription As String
        Dim sinAndDescriptionArray() As String
        Dim description As String = "not found"

        serviceCategoriesDropDownList = CType(fvPricelistDetail.Row.FindControl("serviceCategoriesDropDownList"), DropDownList)

        For Each serviceItem As ListItem In serviceCategoriesDropDownList.Items
            idSIN = CType(serviceItem.Value, String)
            If idSIN.Length > 0 Then
                idSINArray = idSIN.Split("|")
                idSINArray(0) = idSINArray(0).Trim
                testId = Integer.Parse(idSINArray(0))
                If testId = serviceId Then
                    sinAndDescription = serviceItem.Text
                    sinAndDescriptionArray = sinAndDescription.Split(":")
                    description = sinAndDescriptionArray(1).Trim
                    Exit For
                End If
            End If

        Next
        Return description
    End Function

    Private Sub SetVisibilityOfControlsForPricelistType()
        'all controls are hidden by default
        If _pricelistType.CompareTo("F") = 0 Then
            ShowFSSPortion()

            Dim BPAPriceTable As Table
            BPAPriceTable = CType(fvPricelistDetail.FindControl("BPAPriceTable"), Table)
            BPAPriceTable.Visible = False

        ElseIf _pricelistType.CompareTo("6") = 0 Then
            ShowFSSPortion()

            ShowLabel("serviceCategoryLabel")

            Dim serviceCategoriesDropDownList As DropDownList
            serviceCategoriesDropDownList = CType(fvPricelistDetail.FindControl("serviceCategoriesDropDownList"), DropDownList)
            serviceCategoriesDropDownList.Visible = True
            serviceCategoriesDropDownList.Enabled = True

            'replace sin ddl with textbox
            Dim SINDropDownList As DropDownList
            SINDropDownList = CType(fvPricelistDetail.FindControl("SINDropDownList"), DropDownList)
            SINDropDownList.Visible = False
            SINDropDownList.Enabled = False

            ShowTextBox("SINTextBox")
            MakeTextBoxReadOnly("SINTextBox") 'the sin is updated by the selection of service category

        ElseIf _pricelistType.CompareTo("B") = 0 Then
            ShowFSSPortion()

            Dim BPAPriceTable As Table
            BPAPriceTable = CType(fvPricelistDetail.FindControl("BPAPriceTable"), Table)
            BPAPriceTable.Visible = True

            ShowLabel("BPAPriceLabel")
            ShowTextBox("BPAPriceTextBox")

            Dim BPAPriceEffectiveDateTable As HtmlTable
            BPAPriceEffectiveDateTable = CType(fvPricelistDetail.FindControl("BPAPriceEffectiveDateTable"), HtmlTable)
            BPAPriceEffectiveDateTable.Visible = True

            ShowLabel("BPAPriceEffectiveDateRangeLabel")
            ShowLabel("BPAEffectiveDateLabel")
            ShowTextBox("BPAEffectiveDateTextBox")

            ShowLabel("BPAExpirationDateLabel")
            ShowTextBox("BPAExpirationDateTextBox")

            ShowLabel("BPADescriptionLabel")
            ShowTextBox("BPADescriptionTextBox")

            Dim contractorCatalogNumberDropDownList As DropDownList
            contractorCatalogNumberDropDownList = CType(fvPricelistDetail.FindControl("contractorCatalogNumberDropDownList"), DropDownList)
            contractorCatalogNumberDropDownList.Visible = True
            contractorCatalogNumberDropDownList.Enabled = True

            Dim contractorCatalogNumberTextBox As TextBox
            contractorCatalogNumberTextBox = CType(fvPricelistDetail.FindControl("contractorCatalogNumberTextBox"), TextBox)
            contractorCatalogNumberTextBox.Visible = False
            contractorCatalogNumberTextBox.Enabled = False

            DisableEditingOfFSS()
        ElseIf _pricelistType.CompareTo("NB") = 0 Then
            Dim BPAPriceTable As Table
            BPAPriceTable = CType(fvPricelistDetail.FindControl("BPAPriceTable"), Table)
            BPAPriceTable.Visible = True

            ShowLabel("BPAPriceLabel")
            ShowTextBox("BPAPriceTextBox")

            Dim BPAPriceEffectiveDateTable As HtmlTable
            BPAPriceEffectiveDateTable = CType(fvPricelistDetail.FindControl("BPAPriceEffectiveDateTable"), HtmlTable)
            BPAPriceEffectiveDateTable.Visible = True

            ShowLabel("BPAPriceEffectiveDateRangeLabel")
            ShowLabel("BPAEffectiveDateLabel")
            ShowTextBox("BPAEffectiveDateTextBox")

            ShowLabel("BPAExpirationDateLabel")
            ShowTextBox("BPAExpirationDateTextBox")

            ShowLabel("BPADescriptionLabel")
            ShowTextBox("BPADescriptionTextBox")

        End If
    End Sub

    Private Sub ShowLabel(ByVal labelName As String)
        Dim theLabel As Label
        theLabel = CType(fvPricelistDetail.FindControl(labelName), Label)
        theLabel.Visible = True
        theLabel.Enabled = True
    End Sub

    Private Sub ShowTextBox(ByVal textBoxName As String)
        Dim theTextBox As TextBox
        theTextBox = CType(fvPricelistDetail.FindControl(textBoxName), TextBox)
        theTextBox.Visible = True
        theTextBox.Enabled = True
    End Sub
    Private Sub MakeTextBoxReadOnly(ByVal textBoxName As String)
        Dim theTextBox As TextBox
        theTextBox = CType(fvPricelistDetail.FindControl(textBoxName), TextBox)
        theTextBox.ReadOnly = True
    End Sub
    Private Sub HideLabel(ByVal labelName As String)
        Dim theLabel As Label
        theLabel = CType(fvPricelistDetail.FindControl(labelName), Label)
        theLabel.Visible = True
        theLabel.Enabled = True
    End Sub
    Private Sub HideTextBox(ByVal textBoxName As String)
        Dim theTextBox As TextBox
        theTextBox = CType(fvPricelistDetail.FindControl(textBoxName), TextBox)
        theTextBox.Visible = True
        theTextBox.Enabled = True
    End Sub

    Private Sub DisableEditingOfFSS()

        'replace sin ddl with textbox
        Dim SINDropDownList As DropDownList
        SINDropDownList = CType(fvPricelistDetail.FindControl("SINDropDownList"), DropDownList)
        SINDropDownList.Visible = False
        SINDropDownList.Enabled = False

        ShowTextBox("SINTextBox")

        'replace package size ddl with textbox
        Dim packageSizePricedOnContractDropDownList As DropDownList
        packageSizePricedOnContractDropDownList = CType(fvPricelistDetail.FindControl("packageSizePricedOnContractDropDownList"), DropDownList)
        packageSizePricedOnContractDropDownList.Visible = False
        packageSizePricedOnContractDropDownList.Enabled = False

        ShowTextBox("packageSizePricedOnContractTextBox")

        'make fss textboxes readonly=true
        MakeTextBoxReadOnly("FSSPriceTextBox")
        MakeTextBoxReadOnly("SINTextBox")
        MakeTextBoxReadOnly("packageSizePricedOnContractTextBox")
        MakeTextBoxReadOnly("FSSEffectiveDateTextBox")
        MakeTextBoxReadOnly("FSSExpirationDateTextBox")
        MakeTextBoxReadOnly("contractorCatalogNumberTextBox")
        MakeTextBoxReadOnly("productLongDescriptionTextBox")
        MakeTextBoxReadOnly("tier1PriceTextBox")
        MakeTextBoxReadOnly("tier2PriceTextBox")
        MakeTextBoxReadOnly("tier3PriceTextBox")
        MakeTextBoxReadOnly("tier4PriceTextBox")
        MakeTextBoxReadOnly("tier5PriceTextBox")
        MakeTextBoxReadOnly("tier1NotesTextBox")
        MakeTextBoxReadOnly("tier2NotesTextBox")
        MakeTextBoxReadOnly("tier3NotesTextBox")
        MakeTextBoxReadOnly("tier4NotesTextBox")
        MakeTextBoxReadOnly("tier5NotesTextBox")

        ''replace base pack UOM ddl with textbox
        'Dim basePackUOMDropDownList As DropDownList
        'basePackUOMDropDownList = CType(fvPricelistDetail.FindControl("basePackUOMDropDownList"), DropDownList)
        'basePackUOMDropDownList.Visible = False
        'basePackUOMDropDownList.Enabled = False

        'ShowTextBox("basePackUOMTextBox")
        'MakeTextBoxReadOnly("basePackUOMTextBox")

        'MakeTextBoxReadOnly("basePackQuantityTextBox")
        'MakeTextBoxReadOnly("basePackUPNTextBox")

        'Dim basePackShippableCheckBox As CheckBox
        'basePackShippableCheckBox = CType(fvPricelistDetail.FindControl("basePackShippableCheckBox"), CheckBox)
        'basePackShippableCheckBox.Enabled = False

        ''replace inner pack UOM ddl with textbox
        'Dim innerPackUOMDropDownList As DropDownList
        'innerPackUOMDropDownList = CType(fvPricelistDetail.FindControl("innerPackUOMDropDownList"), DropDownList)
        'innerPackUOMDropDownList.Visible = False
        'innerPackUOMDropDownList.Enabled = False

        'ShowTextBox("innerPackUOMTextBox")
        'MakeTextBoxReadOnly("innerPackUOMTextBox")

        'MakeTextBoxReadOnly("innerPackQuantityTextBox")
        'MakeTextBoxReadOnly("innerPackUPNTextBox")

        'Dim innerPackShippableCheckBox As CheckBox
        'innerPackShippableCheckBox = CType(fvPricelistDetail.FindControl("innerPackShippableCheckBox"), CheckBox)
        'innerPackShippableCheckBox.Enabled = False

        ''replace outer pack UOM ddl with textbox
        'Dim outerPackUOMDropDownList As DropDownList
        'outerPackUOMDropDownList = CType(fvPricelistDetail.FindControl("outerPackUOMDropDownList"), DropDownList)
        'outerPackUOMDropDownList.Visible = False
        'outerPackUOMDropDownList.Enabled = False

        'ShowTextBox("outerPackUOMTextBox")
        'MakeTextBoxReadOnly("outerPackUOMTextBox")

        'MakeTextBoxReadOnly("outerPackQuantityTextBox")
        'MakeTextBoxReadOnly("outerPackUPNTextBox")

        'Dim outerPackShippableCheckBox As CheckBox
        'outerPackShippableCheckBox = CType(fvPricelistDetail.FindControl("outerPackShippableCheckBox"), CheckBox)
        'outerPackShippableCheckBox.Enabled = False

    End Sub

    Private Sub ShowFSSPortion()

        Dim FSSPriceTable As Table = CType(fvPricelistDetail.FindControl("FSSPriceTable"), Table)
        FSSPriceTable.Visible = True
        FSSPriceTable.Enabled = True

        ShowTextBox("FSSPriceTextBox")
        ShowLabel("FSSPriceLabel")
        ShowLabel("packageSizePricedOnContractLabel")

        Dim packageSizePricedOnContractDropDownList As DropDownList
        packageSizePricedOnContractDropDownList = CType(fvPricelistDetail.FindControl("packageSizePricedOnContractDropDownList"), DropDownList)
        packageSizePricedOnContractDropDownList.Visible = True
        packageSizePricedOnContractDropDownList.Enabled = True

        '     ShowTextBox("packageSizePricedOnContractTextBox")

        Dim FSSPriceEffectiveDateTable As Table = CType(fvPricelistDetail.FindControl("FSSPriceEffectiveDateTable"), Table)
        FSSPriceEffectiveDateTable.Visible = True
        FSSPriceEffectiveDateTable.Enabled = True

        ShowLabel("priceEffectiveDateRangeLabel")
        ShowLabel("FSSEffectiveDateLabel")
        ShowTextBox("FSSEffectiveDateTextBox")
        ShowLabel("FSSExpirationDateLabel")
        ShowTextBox("FSSExpirationDateTextBox")

        Dim contractorCatalogNumberLabel As Label
        contractorCatalogNumberLabel = CType(fvPricelistDetail.FindControl("contractorCatalogNumberLabel"), Label)
        contractorCatalogNumberLabel.Visible = True
        contractorCatalogNumberLabel.Enabled = True

        'Dim contractorCatalogNumberDropDownList As DropDownList
        'contractorCatalogNumberDropDownList = CType(fvPricelistDetail.FindControl("contractorCatalogNumberDropDownList"), DropDownList)
        'contractorCatalogNumberDropDownList.Visible = True
        'contractorCatalogNumberDropDownList.Enabled = True

        Dim contractorCatalogNumberTextBox As TextBox
        contractorCatalogNumberTextBox = CType(fvPricelistDetail.FindControl("contractorCatalogNumberTextBox"), TextBox)
        contractorCatalogNumberTextBox.Visible = True
        contractorCatalogNumberTextBox.Enabled = True

        Dim SINLabel As Label
        SINLabel = CType(fvPricelistDetail.FindControl("SINLabel"), Label)
        SINLabel.Visible = True
        SINLabel.Enabled = True

        '     ShowTextBox("SINTextBox")

        Dim SINDropDownList As DropDownList
        SINDropDownList = CType(fvPricelistDetail.FindControl("SINDropDownList"), DropDownList)
        SINDropDownList.Visible = True
        SINDropDownList.Enabled = True

        Dim productLongDescriptionLabel As Label
        productLongDescriptionLabel = CType(fvPricelistDetail.FindControl("productLongDescriptionLabel"), Label)
        productLongDescriptionLabel.Visible = True
        productLongDescriptionLabel.Enabled = True

        ShowTextBox("productLongDescriptionTextBox")

        Dim tieredPricingTable As HtmlTable = CType(fvPricelistDetail.FindControl("tieredPricingTable"), HtmlTable)
        tieredPricingTable.Visible = True
        ' tieredPricingTable.Enabled = True

        ShowLabel("tieredPricingLabel")

        ShowTextBox("tier1PriceTextBox")
        ShowTextBox("tier2PriceTextBox")
        ShowTextBox("tier3PriceTextBox")
        ShowTextBox("tier4PriceTextBox")
        ShowTextBox("tier5PriceTextBox")
        ShowTextBox("tier1NotesTextBox")
        ShowTextBox("tier2NotesTextBox")
        ShowTextBox("tier3NotesTextBox")
        ShowTextBox("tier4NotesTextBox")
        ShowTextBox("tier5NotesTextBox")

        'Dim BasePackTable As HtmlTable = CType(fvPricelistDetail.FindControl("BasePackTable"), HtmlTable)
        'BasePackTable.Visible = True

        'Dim InnerPackTable As HtmlTable = CType(fvPricelistDetail.FindControl("InnerPackTable"), HtmlTable)
        'InnerPackTable.Visible = True

        'Dim OuterPackTable As HtmlTable = CType(fvPricelistDetail.FindControl("OuterPackTable"), HtmlTable)
        'OuterPackTable.Visible = True

        'Dim basePackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("basePackUOMDropDownList"), DropDownList)
        'basePackUOMDropDownList.Visible = True
        'basePackUOMDropDownList.Enabled = True

        'ShowTextBox("basePackQuantityTextBox")
        'ShowTextBox("basePackUPNTextBox")

        'Dim basePackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("basePackShippableCheckBox"), CheckBox)
        'basePackShippableCheckBox.Visible = True
        'basePackShippableCheckBox.Enabled = True

        'Dim innerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("innerPackUOMDropDownList"), DropDownList)
        'innerPackUOMDropDownList.Visible = True
        'innerPackUOMDropDownList.Enabled = True

        'ShowTextBox("innerPackQuantityTextBox")
        'ShowTextBox("innerPackUPNTextBox")

        'Dim innerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("innerPackShippableCheckBox"), CheckBox)
        'innerPackShippableCheckBox.Visible = True
        'innerPackShippableCheckBox.Enabled = True

        'Dim outerPackUOMDropDownList As DropDownList = CType(fvPricelistDetail.FindControl("outerPackUOMDropDownList"), DropDownList)
        'outerPackUOMDropDownList.Visible = True
        'outerPackUOMDropDownList.Enabled = True

        'ShowTextBox("outerPackQuantityTextBox")
        'ShowTextBox("outerPackUPNTextBox")

        'Dim outerPackShippableCheckBox As CheckBox = CType(fvPricelistDetail.FindControl("outerPackShippableCheckBox"), CheckBox)
        'outerPackShippableCheckBox.Visible = True
        'outerPackShippableCheckBox.Enabled = True

    End Sub

    Private Sub SetEditingOfControlsForPricelistTypeAndEditingStatus()
        If _canEditYN.CompareTo("N") = 0 Then
            Dim SaveButton As Button
            SaveButton = CType(fvPricelistDetail.FindControl("SaveButton"), Button)
            SaveButton.Visible = False
            SaveButton.Enabled = False
        End If

        If _pricelistType.CompareTo("F") = 0 Then
            If _canEditYN.CompareTo("N") = 0 Then
                DisableEditingOfFSS()
            End If
        ElseIf _pricelistType.CompareTo("6") = 0 Then
            If _canEditYN.CompareTo("N") = 0 Then
                DisableEditingOfFSS()
                'disable additional service related controls
                Dim serviceCategoriesDropDownList As DropDownList
                serviceCategoriesDropDownList = CType(fvPricelistDetail.FindControl("serviceCategoriesDropDownList"), DropDownList)
                serviceCategoriesDropDownList.Visible = False
                serviceCategoriesDropDownList.Enabled = False

                ShowTextBox("serviceCategoriesTextBox")
                MakeTextBoxReadOnly("serviceCategoriesTextBox")
            End If

        ElseIf _pricelistType.CompareTo("B") = 0 Then
            If _canEditYN.CompareTo("N") = 0 Then
                DisableEditingOfBPA()
                'fss already disabled via visibility function
            End If
        ElseIf _pricelistType.CompareTo("NB") = 0 Then
            If _canEditYN.CompareTo("N") = 0 Then
                DisableEditingOfNonStandardBPA()
                'fss already disabled via visibility function
            End If
        End If

    End Sub
    Private Sub DisableEditingOfBPA()
        MakeTextBoxReadOnly("BPAPriceTextBox")
        MakeTextBoxReadOnly("BPAEffectiveDateTextBox")
        MakeTextBoxReadOnly("BPAExpirationDateTextBox")
        MakeTextBoxReadOnly("BPADescriptionTextBox")

        ShowTextBox("contractorCatalogNumberTextBox")
        MakeTextBoxReadOnly("contractorCatalogNumberTextBox")

        'hide catalog drop down list
        Dim contractorCatalogNumberDropDownList As DropDownList
        contractorCatalogNumberDropDownList = CType(fvPricelistDetail.FindControl("contractorCatalogNumberDropDownList"), DropDownList)
        contractorCatalogNumberDropDownList.Visible = False
        contractorCatalogNumberDropDownList.Enabled = False

    End Sub
    Private Sub DisableEditingOfNonStandardBPA()
        MakeTextBoxReadOnly("BPAPriceTextBox")
        MakeTextBoxReadOnly("BPAEffectiveDateTextBox")
        MakeTextBoxReadOnly("BPAExpirationDateTextBox")
        MakeTextBoxReadOnly("BPADescriptionTextBox")

        'ShowTextBox("contractorCatalogNumberTextBox")
        'MakeTextBoxReadOnly("contractorCatalogNumberTextBox")

        'hide catalog drop down list
        'Dim contractorCatalogNumberDropDownList As DropDownList
        'contractorCatalogNumberDropDownList = CType(fvPricelistDetail.FindControl("contractorCatalogNumberDropDownList"), DropDownList)
        'contractorCatalogNumberDropDownList.Visible = False
        'contractorCatalogNumberDropDownList.Enabled = False

    End Sub
    Protected Sub BPAPriceTextBox_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim BPAPriceTextBox As TextBox = CType(sender, TextBox)

        If _pricelistType.CompareTo("B") = 0 Or _pricelistType.CompareTo("NB") = 0 Then

            Dim fvPricelistDetail As FormView = CType(BPAPriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim bpaPrice As Decimal
            If Decimal.TryParse(currentRow("BPAPrice").ToString(), bpaPrice) = True Then
                BPAPriceTextBox.Text = String.Format("{0:c}", bpaPrice)
            Else
                Throw New Exception("BPA Price was not in the correct format")
            End If

        End If
    End Sub

    Protected Sub BPAEffectiveDateTextBox_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim BPAEffectiveDateTextBox As TextBox = CType(sender, TextBox)

        If _pricelistType.CompareTo("B") = 0 Or _pricelistType.CompareTo("NB") = 0 Then

            Dim fvPricelistDetail As FormView = CType(BPAEffectiveDateTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim bpaEffectiveDate As DateTime
            If DateTime.TryParse(currentRow("BPADateEffective").ToString(), bpaEffectiveDate) = True Then
                BPAEffectiveDateTextBox.Text = MakeNullDateBlank(bpaEffectiveDate)
            Else
                Throw New Exception("BPA Effective Date was not in the correct format")
            End If
        End If
    End Sub

    Protected Sub BPAExpirationDateTextBox_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim BPAExpirationDateTextBox As TextBox = CType(sender, TextBox)

        If _pricelistType.CompareTo("B") = 0 Or _pricelistType.CompareTo("NB") = 0 Then

            Dim fvPricelistDetail As FormView = CType(BPAExpirationDateTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim bpaExpirationDate As DateTime
            If DateTime.TryParse(currentRow("BPAExpirationDate").ToString(), bpaExpirationDate) = True Then
                BPAExpirationDateTextBox.Text = MakeNullDateBlank(bpaExpirationDate)
            Else
                Throw New Exception("BPA Expiration Date was not in the correct format")
            End If

        End If
    End Sub
    Protected Function MakeNullDateBlank(ByVal dateFromDatabase As DateTime) As String
        Dim returnString As String
        If dateFromDatabase.CompareTo(DateTime.MinValue) = 0 Or dateFromDatabase.CompareTo(New DateTime(1900, 1, 1)) = 0 Then
            returnString = ""
        Else
            returnString = String.Format("{0:d}", dateFromDatabase)
        End If
        Return returnString
    End Function

    Protected Sub BPADescriptionTextBox_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim BPADescriptionTextBox As TextBox = CType(sender, TextBox)

        If _pricelistType.CompareTo("B") = 0 Or _pricelistType.CompareTo("NB") = 0 Then

            Dim fvPricelistDetail As FormView = CType(BPADescriptionTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            BPADescriptionTextBox.Text = currentRow("BPADescription").ToString()
        End If
    End Sub

    Protected Sub FSSPriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim FSSPriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(FSSPriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim fssPrice As Decimal
            If Decimal.TryParse(currentRow("FSS_Price").ToString(), fssPrice) = True Then
                FSSPriceTextBox.Text = String.Format("{0:c}", fssPrice)
            Else
                Throw New Exception("FSS Price was not in the correct format")
            End If
        End If
    End Sub

    Protected Sub packageSizePricedOnContractTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim packageSizePricedOnContractTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(packageSizePricedOnContractTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            packageSizePricedOnContractTextBox.Text = currentRow("Package_Size_Priced_on_Contract").ToString()

        End If
    End Sub

    Protected Sub packageSizePricedOnContractDropDownList_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim packageSizePricedOnContractDropDownList As DropDownList = CType(sender, DropDownList)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then

            Dim fvPricelistDetail As FormView = CType(packageSizePricedOnContractDropDownList.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            packageSizePricedOnContractDropDownList.SelectedValue = currentRow("Package_Size_Priced_on_Contract").ToString()

        End If
    End Sub

    Protected Sub FSSEffectiveDateTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim FSSEffectiveDateTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(FSSEffectiveDateTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim FSSEffectiveDate As DateTime
            If DateTime.TryParse(currentRow("FSSDateEffective").ToString(), FSSEffectiveDate) = True Then
                FSSEffectiveDateTextBox.Text = MakeNullDateBlank(FSSEffectiveDate)
            Else
                Throw New Exception("FSS Effective Date was not in the correct format")
            End If


        End If   ' Text='<%#MakeNullDateBlank(Eval("FSSDateEffective","{0:d}")) %>'
    End Sub

    Protected Sub FSSExpirationDateTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim FSSExpirationDateTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(FSSExpirationDateTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim FSSExpirationDate As DateTime
            If DateTime.TryParse(currentRow("FSSExpirationDate").ToString(), FSSExpirationDate) = True Then
                FSSExpirationDateTextBox.Text = MakeNullDateBlank(FSSExpirationDate)
            Else
                Throw New Exception("FSS Expiration Date was not in the correct format")
            End If

        End If   ' Text='<%# MakeNullDateBlank(Eval("FSSExpirationDate","{0:d}")) %>'
    End Sub

    Protected Sub contractorCatalogNumberTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim contractorCatalogNumberTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(contractorCatalogNumberTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            contractorCatalogNumberTextBox.Text = currentRow("Contractor_Catalog_Number").ToString()

        End If  '  Text='<%# Eval("Contractor_Catalog_Number") %>'
    End Sub

    Protected Sub SINTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim SINTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(SINTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            SINTextBox.Text = currentRow("FSS_SIN").ToString()

        End If   ' Text='<%#Eval("FSS_SIN") %>'
    End Sub

    Protected Sub productLongDescriptionTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim productLongDescriptionTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(productLongDescriptionTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            productLongDescriptionTextBox.Text = currentRow("Product_Long_Description").ToString()

        End If  '  Text='<%#Eval("Product_Long_Description") %>'
    End Sub

    Protected Sub tier1PriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier1PriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier1PriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim tierPrice As Decimal
            If Not currentRow.Item("Tier_1_Price").Equals(DBNull.Value) Then
                If Decimal.TryParse(currentRow("Tier_1_Price").ToString(), tierPrice) = True Then
                    tier1PriceTextBox.Text = String.Format("{0:c}", tierPrice)
                Else
                    Throw New Exception("Tier 1 Price was not in the correct format")
                End If
            End If
        End If '   Text='<%#Eval("Tier_1_Price", "{0:c}") %>'
    End Sub

    Protected Sub tier2PriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier2PriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier2PriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim tierPrice As Decimal
            If Not currentRow.Item("Tier_2_Price").Equals(DBNull.Value) Then
                If Decimal.TryParse(currentRow("Tier_2_Price").ToString(), tierPrice) = True Then
                    tier2PriceTextBox.Text = String.Format("{0:c}", tierPrice)
                Else
                    Throw New Exception("Tier 2 Price was not in the correct format")
                End If
            End If
        End If '   Text='<%#Eval("Tier_1_Price", "{0:c}") %>'
    End Sub

    Protected Sub tier3PriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier3PriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier3PriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim tierPrice As Decimal
            If Not currentRow.Item("Tier_3_Price").Equals(DBNull.Value) Then

                If Decimal.TryParse(currentRow("Tier_3_Price").ToString(), tierPrice) = True Then
                    tier3PriceTextBox.Text = String.Format("{0:c}", tierPrice)
                Else
                    Throw New Exception("Tier 3 Price was not in the correct format")
                End If

            End If
        End If '   Text='<%#Eval("Tier_1_Price", "{0:c}") %>'
    End Sub

    Protected Sub tier4PriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier4PriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier4PriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim tierPrice As Decimal

            If Not currentRow.Item("Tier_4_Price").Equals(DBNull.Value) Then

                If Decimal.TryParse(currentRow("Tier_4_Price").ToString(), tierPrice) = True Then
                    tier4PriceTextBox.Text = String.Format("{0:c}", tierPrice)
                Else
                    Throw New Exception("Tier 4 Price was not in the correct format")
                End If
            End If
        End If '   Text='<%#Eval("Tier_1_Price", "{0:c}") %>'
    End Sub

    Protected Sub tier5PriceTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier5PriceTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier5PriceTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)

            Dim tierPrice As Decimal
            If Not currentRow.Item("Tier_5_Price").Equals(DBNull.Value) Then
                If Decimal.TryParse(currentRow("Tier_5_Price").ToString(), tierPrice) = True Then
                    tier5PriceTextBox.Text = String.Format("{0:c}", tierPrice)
                Else
                    Throw New Exception("Tier 5 Price was not in the correct format")
                End If
            End If
        End If '   Text='<%#Eval("Tier_1_Price", "{0:c}") %>'
    End Sub


    Protected Sub tier1NotesTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier1NotesTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier1NotesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            tier1NotesTextBox.Text = currentRow("Tier_1_Note").ToString()

        End If   ' Text='<%#Eval("Tier_1_Note") %>'
    End Sub


    Protected Sub tier2NotesTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier2NotesTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier2NotesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            tier2NotesTextBox.Text = currentRow("Tier_2_Note").ToString()

        End If   ' Text='<%#Eval("Tier_1_Note") %>'
    End Sub

    Protected Sub tier3NotesTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier3NotesTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier3NotesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            tier3NotesTextBox.Text = currentRow("Tier_3_Note").ToString()

        End If   ' Text='<%#Eval("Tier_1_Note") %>'
    End Sub

    Protected Sub tier4NotesTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier4NotesTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier4NotesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            tier4NotesTextBox.Text = currentRow("Tier_4_Note").ToString()

        End If   ' Text='<%#Eval("Tier_1_Note") %>'
    End Sub

    Protected Sub tier5NotesTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim tier5NotesTextBox As TextBox = CType(sender, TextBox)
        Dim temp As String = _pricelistType

        If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

            Dim fvPricelistDetail As FormView = CType(tier5NotesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            tier5NotesTextBox.Text = currentRow("Tier_5_Note").ToString()

        End If   ' Text='<%#Eval("Tier_1_Note") %>'
    End Sub


    'Protected Sub basePackUOMDropDownList_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim basePackUOMDropDownList As DropDownList = CType(sender, DropDownList)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(basePackUOMDropDownList.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        basePackUOMDropDownList.SelectedValue = currentRow("Base_Packaging_UOM").ToString()

    '    End If   '     SelectedValue='<%#Eval("[Base_Packaging_UOM]") %>'>
    'End Sub

    'Protected Sub basePackUOMTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim basePackUOMTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(basePackUOMTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        basePackUOMTextBox.Text = currentRow("Base_Packaging_UOM").ToString()

    '    End If   '     Text='<%#Eval("[Base_Packaging_UOM]") %>'
    'End Sub

    'Protected Sub basePackQuantityTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim basePackQuantityTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(basePackQuantityTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        basePackQuantityTextBox.Text = currentRow("Base_Packaging_Unit_of_Conversion_Factor").ToString()

    '    End If   '     Text='<%#Eval("Base_Packaging_Unit_of_Conversion_Factor") %>'
    'End Sub

    'Protected Sub basePackUPNTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim basePackUPNTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(basePackUPNTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        basePackUPNTextBox.Text = currentRow("Base_Packaging_UPN").ToString()

    '    End If   '     Text='<%#Eval("Base_Packaging_UPN") %>'
    'End Sub

    'Protected Sub basePackShippableCheckBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim basePackShippableCheckBox As CheckBox = CType(sender, CheckBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(basePackShippableCheckBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        basePackShippableCheckBox.Checked = currentRow("Base_Packaging_Unit_Shippable").ToString()

    '    End If   '     Checked='<%#Eval("Base_Packaging_Unit_Shippable") %>'
    'End Sub

    'Protected Sub innerPackUOMDropDownList_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim innerPackUOMDropDownList As DropDownList = CType(sender, DropDownList)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(innerPackUOMDropDownList.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        innerPackUOMDropDownList.SelectedValue = currentRow("Intermediate_Pack_UOM").ToString()

    '    End If   '     SelectedValue='<%#Eval("[Inner_Packaging_UOM]") %>'>
    'End Sub

    'Protected Sub innerPackUOMTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim innerPackUOMTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(innerPackUOMTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        innerPackUOMTextBox.Text = currentRow("Intermediate_Pack_UOM").ToString()

    '    End If   '     Text='<%#Eval("[Inner_Packaging_UOM]") %>'
    'End Sub

    'Protected Sub innerPackQuantityTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim innerPackQuantityTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(innerPackQuantityTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        innerPackQuantityTextBox.Text = currentRow("Intermediate_Pack_Unit_of_Conversion_Factor").ToString()

    '    End If   '     Text='<%#Eval("Inner_Packaging_Unit_of_Conversion_Factor") %>'
    'End Sub

    'Protected Sub innerPackUPNTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim innerPackUPNTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(innerPackUPNTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        innerPackUPNTextBox.Text = currentRow("Intermediate_Pack_UPN").ToString()

    '    End If   '     Text='<%#Eval("Inner_Packaging_UPN") %>'
    'End Sub

    'Protected Sub innerPackShippableCheckBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim innerPackShippableCheckBox As CheckBox = CType(sender, CheckBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(innerPackShippableCheckBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        innerPackShippableCheckBox.Checked = currentRow("Intermediate_Pack_Shippable").ToString()

    '    End If   '     Checked='<%#Eval("Inner_Packaging_Unit_Shippable") %>'
    'End Sub

    'Protected Sub outerPackUOMDropDownList_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim outerPackUOMDropDownList As DropDownList = CType(sender, DropDownList)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(outerPackUOMDropDownList.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        outerPackUOMDropDownList.SelectedValue = currentRow("Outer_Pack_UOM").ToString()

    '    End If   '     SelectedValue='<%#Eval("[Outer_Packaging_UOM]") %>'>
    'End Sub

    'Protected Sub outerPackUOMTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim outerPackUOMTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(outerPackUOMTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        outerPackUOMTextBox.Text = currentRow("Outer_Pack_UOM").ToString()

    '    End If   '     Text='<%#Eval("[Outer_Packaging_UOM]") %>'
    'End Sub

    'Protected Sub outerPackQuantityTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim outerPackQuantityTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(outerPackQuantityTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        outerPackQuantityTextBox.Text = currentRow("Outer_Pack_Unit_of_Conversion_Factor").ToString()

    '    End If   '     Text='<%#Eval("Outer_Packaging_Unit_of_Conversion_Factor") %>'
    'End Sub

    'Protected Sub outerPackUPNTextBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim outerPackUPNTextBox As TextBox = CType(sender, TextBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(outerPackUPNTextBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        outerPackUPNTextBox.Text = currentRow("Outer_Pack_UPN").ToString()

    '    End If   '     Text='<%#Eval("Outer_Packaging_UPN") %>'
    'End Sub

    'Protected Sub outerPackShippableCheckBox_OnDataBinding(ByVal sender As Object, ByVal e As EventArgs)
    '    Dim outerPackShippableCheckBox As CheckBox = CType(sender, CheckBox)
    '    Dim temp As String = _pricelistType

    '    If _pricelistType.CompareTo("F") = 0 Or _pricelistType.CompareTo("6") = 0 Or _pricelistType.CompareTo("B") = 0 Then

    '        Dim fvPricelistDetail As FormView = CType(outerPackShippableCheckBox.NamingContainer, FormView)
    '        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
    '        outerPackShippableCheckBox.Checked = currentRow("Outer_Pack_Unit_Shippable").ToString()

    '    End If   '     Checked='<%#Eval("Outer_Packaging_Unit_Shippable") %>'
    'End Sub


    Protected Sub serviceCategoriesDropDownList_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim serviceCategoriesDropDownList As DropDownList = CType(sender, DropDownList)

        If _pricelistType.CompareTo("6") = 0 Then

            Dim fvPricelistDetail As FormView = CType(serviceCategoriesDropDownList.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            Dim currentServiceCategoryId As Integer
            currentServiceCategoryId = Integer.Parse(currentRow("ServiceCategoryId").ToString())

            SetServiceListFromId(currentServiceCategoryId)
        End If
    End Sub
    Protected Sub serviceCategoriesTextBox_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim serviceCategoriesTextBox As TextBox = CType(sender, TextBox)

        If _pricelistType.CompareTo("6") = 0 Then

            Dim fvPricelistDetail As FormView = CType(serviceCategoriesTextBox.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            Dim currentServiceCategoryId As Integer
            currentServiceCategoryId = Integer.Parse(currentRow("ServiceCategoryId").ToString())

            serviceCategoriesTextBox.Text = GetServiceDescriptionFromId(currentServiceCategoryId)
        End If
    End Sub

    Protected Sub SINDropDownList_DataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim SINDropDownList As DropDownList = CType(sender, DropDownList)

        If _pricelistType.CompareTo("F") = 0 Then

            Dim fvPricelistDetail As FormView = CType(SINDropDownList.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            Dim currentSIN As String
            currentSIN = currentRow("FSS_SIN").ToString()

            Dim selectedItem As ListItem

            If SINDropDownList.Items.Count > 0 Then
                selectedItem = SINDropDownList.Items.FindByText(currentSIN)
                If Not selectedItem Is Nothing Then
                    SINDropDownList.SelectedValue = currentSIN
                Else
                    SINDropDownList.SelectedValue = "Invalid"
                End If
            End If
        End If
    End Sub

    Protected Sub contractorCatalogNumberDropDownList_DataBinding(ByVal sender As Object, ByVal e As EventArgs)
        Dim contractorCatalogNumberDropDownList As DropDownList = CType(sender, DropDownList)

        If _pricelistType.CompareTo("B") = 0 Then
            Dim fvPricelistDetail As FormView = CType(contractorCatalogNumberDropDownList.NamingContainer, FormView)
            Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
            Dim fssLogNumber As String
            fssLogNumber = currentRow("FSSLogNumber").ToString()

            contractorCatalogNumberDropDownList.SelectedValue = fssLogNumber
        End If
    End Sub

    Protected Sub fvPricelistDetail_OnDataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim fvPricelistDetail As FormView = CType(sender, FormView)
        Dim currentRow As DataRowView = CType(fvPricelistDetail.DataItem, DataRowView)
        _OriginalSINValue = currentRow("FSS_SIN").ToString()
        Session("OriginalSINValueForPricelistDetail") = _OriginalSINValue

    End Sub
  
End Class