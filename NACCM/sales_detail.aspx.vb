Public Partial Class sales_detail
    Inherits System.Web.UI.Page
    Protected TitleName As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Dim myContractNumber As String = ""
            If Not Request.QueryString("CntrctNum") Is Nothing Then
                myContractNumber = Request.QueryString("CntrctNum").ToString
            End If
            TitleName = "Sales Detail for " & myContractNumber
        End If
    End Sub
    Protected Function CalculateVASales(ByVal PVAQtrSales As Object, ByVal VASales As Object) As String
        Dim myVariance As String = ""
        Dim myVAQtr As Double = 0
        Dim myVASales As Double = 0
        'Dim myLabelName As String = passLabel
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

        'Dim myLabel As Label = CType(fvContractInfo.Row.FindControl(myLabelName), Label)
        Return myVariance
    End Function
    Protected Function CalculateIFF(ByVal First As Object, ByVal Second As Object) As String
        Dim myIFF As String = ""
        Dim myFirst As Double = 0
        Dim mySecond As Double = 0
        If Not First.Equals(DBNull.Value) Then
            myFirst = CType(First, Double)
        End If
        If Not Second.Equals(DBNull.Value) Then
            mySecond = CType(Second, Double)
        End If
        myIFF = FormatCurrency(myFirst * mySecond).ToString
        Return myIFF
    End Function
    Protected Function CalculateIFFAll(ByVal First As Object, ByVal Second As Object, ByVal Third As Object, ByVal Fourth As Object, ByVal Fifth As Object, ByVal Sixth As Object) As String
        Dim myIFF As String = ""
        Dim myFirst As Double = 0
        Dim mySecond As Double = 0
        Dim myThird As Double = 0
        Dim myFourth As Double = 0
        Dim myFifth As Double = 0
        Dim mySixth As Double = 0
        If Not First.Equals(DBNull.Value) Then
            myFirst = CType(First, Double)
        End If
        If Not Second.Equals(DBNull.Value) Then
            mySecond = CType(Second, Double)
        End If
        If Not Third.Equals(DBNull.Value) Then
            myThird = CType(Third, Double)
        End If
        If Not Fourth.Equals(DBNull.Value) Then
            myFourth = CType(Fourth, Double)
        End If
        If Not Fifth.Equals(DBNull.Value) Then
            myFifth = CType(Fifth, Double)
        End If
        If Not Sixth.Equals(DBNull.Value) Then
            mySixth = CType(Sixth, Double)
        End If
        myIFF = FormatCurrency((myFirst * mySecond) + (myThird * myFourth) + (myFifth * mySixth)).ToString
        Return myIFF
    End Function
    Protected Sub btn_Close(ByVal s As Object, ByVal e As EventArgs)
        Response.Write("<script>window.close()</script>")
        'Response.Redirect("NAC_CM_BPA_Edit.aspx?CntrctNum=" & Request.QueryString("CntrctNum"))

    End Sub
End Class