<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sales_detail.aspx.vb"
    Inherits="NACCM.sales_detail" %>

<!DOCTYPE html />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Sales Detail</title>
</head>
<body style="background-color:#ECE9D8">
    <form id="form1" runat="server">
    <asp:FormView ID="fvGeneral" runat="server" DataSourceID="ContractDataSource">
        <ItemTemplate>
            <asp:Table ID="tblGeneralInfo" runat="server" Width="876px">
                <asp:TableRow>
                    <asp:TableCell>NAC CM<br />Detailed Sales<br />Analysis</asp:TableCell><asp:TableCell>
                        <asp:Label ID="lbContractorNumber" runat="server" Text='<%#Eval("CntrctNum") %>' /></asp:TableCell><asp:TableCell>
                            <asp:Label ID="lbContractor" runat="server" Text='<%#Eval("Contractor_Name") %>' /></asp:TableCell><asp:TableCell HorizontalAlign="Right">
                                <asp:Button ID="btnClose" runat="server" Text="Close" OnClick="btn_Close" /></asp:TableCell></asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell Wrap="false">
                        <asp:Label ID="lbTitle" runat="server" Text='<%#Eval("Title") %>' Width="100" /></asp:TableCell><asp:TableCell>
                            <asp:Table ID="tblVASales" runat="server" GridLines="Both">
                                <asp:TableHeaderRow>
                                    <asp:TableCell RowSpan="2" BackColor="Aqua">VA Sales</asp:TableCell><asp:TableHeaderCell
                                        ColumnSpan="2">Previous</asp:TableHeaderCell><asp:TableHeaderCell RowSpan="2">IFF<br />Amount</asp:TableHeaderCell></asp:TableHeaderRow>
                                <asp:TableHeaderRow>
                                    <asp:TableHeaderCell>Qtr</asp:TableHeaderCell><asp:TableHeaderCell>Year</asp:TableHeaderCell></asp:TableHeaderRow>
                                <asp:TableRow>
                                    <asp:TableCell BackColor="Aqua">
                                        <asp:Label ID="lbVASalesSum" runat="server" Text='<%#Eval("VA_Sales_Sum","{0:c}") %>' Width="70" /></asp:TableCell><asp:TableCell>
                                            <asp:Label ID="lbVASalesQtr" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_VA_Sales_Sum"),Eval("VA_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                                <asp:Label ID="lbVASalesYear" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_VA_Sales_Sum"),Eval("VA_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                                    <asp:Label ID="lbVAIFF" runat="server" Text='<%# CalculateIFF(Eval("VA_Sales_Sum"),Eval("VA_IFF")) %>' /></asp:TableCell></asp:TableRow>
                            </asp:Table>
                        </asp:TableCell>
                    <asp:TableCell>
                        <asp:Table ID="tblOGASales" runat="server" GridLines="Both">
                            <asp:TableHeaderRow>
                                <asp:TableCell RowSpan="2" BackColor="Aqua">OGA Sales</asp:TableCell><asp:TableHeaderCell
                                    ColumnSpan="2">Previous</asp:TableHeaderCell><asp:TableHeaderCell RowSpan="2">IFF<br />Amount</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>Qtr</asp:TableHeaderCell><asp:TableHeaderCell>Year</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Aqua">
                                    <asp:Label ID="lbOGASales" runat="server" Text='<%#Eval("OGA_Sales_Sum","{0:c}") %>' Width="70" /></asp:TableCell><asp:TableCell>
                                        <asp:Label ID="lbOGAQtr" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_OGA_Sales_Sum"),Eval("OGA_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                            <asp:Label ID="lbOGAYear" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_OGA_Sales_Sum"),Eval("OGA_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                                <asp:Label ID="Label4" runat="server" Text='<%# CalculateIFF(Eval("OGA_Sales_Sum"),Eval("OGA_IFF")) %>' /></asp:TableCell></asp:TableRow>
                        </asp:Table>
                    </asp:TableCell>
                     <asp:TableCell>
                        <asp:Table ID="tblSLGSales" runat="server" GridLines="Both">
                            <asp:TableHeaderRow>
                                <asp:TableCell RowSpan="2" BackColor="Aqua">S/C/L Govt. Sales</asp:TableCell><asp:TableHeaderCell
                                    ColumnSpan="2">Previous</asp:TableHeaderCell><asp:TableHeaderCell RowSpan="2">IFF<br />Amount</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>Qtr</asp:TableHeaderCell><asp:TableHeaderCell>Year</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Aqua">
                                    <asp:Label ID="Label12" runat="server" Text='<%#Eval("SLG_Sales_Sum","{0:c}") %>' Width="70" /></asp:TableCell><asp:TableCell>
                                        <asp:Label ID="Label13" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_SLG_Sales_Sum"),Eval("SLG_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                            <asp:Label ID="Label14" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_SLG_Sales_Sum"),Eval("SLG_Sales_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                                <asp:Label ID="Label15" runat="server" Text='<%# CalculateIFF(Eval("SLG_Sales_Sum"),Eval("SLG_IFF")) %>' /></asp:TableCell></asp:TableRow>
                        </asp:Table>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:Table ID="tblTotalSales" runat="server" GridLines="Both">
                            <asp:TableHeaderRow>
                                <asp:TableCell RowSpan="2" BackColor="Aqua">Total Sales</asp:TableCell><asp:TableHeaderCell
                                    ColumnSpan="2">Previous</asp:TableHeaderCell><asp:TableHeaderCell RowSpan="2">IFF<br />Amount</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>Qtr</asp:TableHeaderCell><asp:TableHeaderCell>Year</asp:TableHeaderCell></asp:TableHeaderRow>
                            <asp:TableRow>
                                <asp:TableCell BackColor="Aqua">
                                    <asp:Label ID="lbTotalSales" runat="server" Text='<%#Eval("Total_Sum","{0:c}") %>' Width="70" /></asp:TableCell><asp:TableCell>
                                        <asp:Label ID="lbTotalQtr" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_Total_Sales"),Eval("Total_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                            <asp:Label ID="lbTotalYear" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_Total_Sales"),Eval("Total_Sum")) %>' /></asp:TableCell><asp:TableCell>
                                                <asp:Label ID="Label5" runat="server" Text='<%# CalculateIFFAll(Eval("VA_Sales_Sum"),Eval("VA_IFF"),Eval("OGA_Sales_Sum"),Eval("OGA_IFF"),Eval("SLG_Sales_Sum"),Eval("SLG_IFF")) %>' />
                                </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </asp:TableCell></asp:TableRow>
            </asp:Table>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ContractDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
        
        SelectCommand="SELECT CntrctNum, Contract_Record_ID, Quarter_ID, Title, Year, Qtr, VA_Sales_Sum, OGA_Sales_Sum, Total_Sum, Previous_Qtr_VA_Sales_Sum, Previous_Qtr_OGA_Sales_Sum, Previous_Qtr_Total_Sales, Previous_Year_VA_Sales_Sum, Previous_Year_OGA_Sales_Sum, Previous_Year_Total_Sales, Contractor_Name, Schedule_Name, Schedule_Number, CO_Name, Logon_Name, Total_IFF_Amount, OGA_IFF_Amount, VA_IFF_Amount, OGA_IFF, VA_IFF, SLG_Sales_Sum, Previous_Qtr_SLG_Sales_Sum, Previous_Year_SLG_Sales_Sum, SLG_IFF, SLG_IFF_Amount FROM dbo.view_Sales_Variance_By_Year_C WHERE (CntrctNum = @CntrctNum) AND (Quarter_ID = @Quarter_ID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            <asp:QueryStringParameter Name="Quarter_ID" QueryStringField="QtrID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:DataList ID="dataListSINDetail" runat="server" DataSourceID="SINDataSource" CellPadding="4">
        <ItemTemplate>
            <tr>
                <td>
                    <asp:Label ID="lbSIN" runat="server" Text='<%#Eval("SIN") %>' Width="100" />
                </td>
                <td>
                    <asp:Label ID="lbSINVASales" runat="server" Text='<%#Eval("VA_Sales","{0:c}") %>'  />
                </td>
                <td>
                    <asp:Label ID="lbTotalQtr" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_VA_Sales"),Eval("VA_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label1" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_VA_Sales"),Eval("VA_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label4" runat="server" Text='<%# CalculateIFF(Eval("VA_Sales"),Eval("VA_IFF")) %>' Width="80" />
                </td>
                <td>
                    <asp:Label ID="Label2" runat="server" Text='<%#Eval("OGA_Sales","{0:c}") %>' Width="75" />
                </td>
                <td>
                    <asp:Label ID="Label3" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_OGA_Sales"),Eval("OGA_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label6" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_OGA_Sales"),Eval("OGA_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label7" runat="server" Text='<%# CalculateIFF(Eval("OGA_Sales"),Eval("OGA_IFF")) %>' Width="70" />
                </td>
                <td>
                    <asp:Label ID="Label16" runat="server" Text='<%#Eval("SLG_Sales","{0:c}") %>' Width="75" />
                </td>
                <td>
                    <asp:Label ID="Label17" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_SLG_Sales"),Eval("SLG_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label18" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_SLG_Sales"),Eval("SLG_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label19" runat="server" Text='<%# CalculateIFF(Eval("SLG_Sales"),Eval("SLG_IFF")) %>' Width="70" />
                </td>
                <td>
                    <asp:Label ID="Label8" runat="server" Text='<%#Eval("Total_Sales","{0:c}") %>' Width="70"/>
                </td>
                <td>
                    <asp:Label ID="Label9" runat="server" Text='<%# CalculateVASales(Eval("Previous_Qtr_Total_Sales"),Eval("Total_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label10" runat="server" Text='<%# CalculateVASales(Eval("Previous_Year_Total_Sales"),Eval("Total_Sales")) %>' />
                </td>
                <td>
                    <asp:Label ID="Label11" runat="server" Text='<%# CalculateIFFAll(Eval("VA_Sales"),Eval("VA_IFF"),Eval("OGA_Sales"),Eval("OGA_IFF"),Eval("SLG_Sales"),Eval("SLG_IFF")) %>' />
                </td>
            </tr>
        </ItemTemplate>
    </asp:DataList>
    <asp:SqlDataSource ID="SINDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:CM %>"
        
        SelectCommand="SELECT CntrctNum, ID, Quarter_ID, Title, SIN, Year, Qtr, VA_Sales, OGA_Sales, Total_Sales, Previous_Qtr_VA_Sales, Previous_Year_OGA_Sales, Previous_Year_Total_Sales, VA_IFF, OGA_IFF, Contract_Record_ID, Previous_Year_VA_Sales, Previous_Qtr_OGA_Sales, Previous_Qtr_Total_Sales, SLG_Sales, Previous_Qtr_SLG_Sales, Previous_Year_SLG_Sales, SLG_IFF FROM dbo.view_Sales_Variance_by_Sin_B WHERE (CntrctNum = @CntrctNum) AND (Quarter_ID = @Quarter_ID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="CntrctNum" QueryStringField="CntrctNum" Type="String" />
            <asp:QueryStringParameter Name="Quarter_ID" QueryStringField="QtrID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    </form>
</body>
</html>
