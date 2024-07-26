<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="CMStandard" CodeBehind="Calendar.aspx.cs" Inherits="VA.NAC.CM.ApplicationStartup.Calendar" %>

<!DOCTYPE html>
<html>

<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=EDGE" />
    <title>Select Date</title>
    <link href="./App_Themes/CMStandard/CMStandard.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">

        window.focus();
        window.onblur = () => window.focus();

        function CloseWindow( dateType, refresh )
        {
            window.opener.document.forms[0].RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].RefreshDateValueOnSubmit.value = dateType;
            window.opener.document.forms[0].submit();

            top.window.opener = top;
            top.window.open('','_parent','');
            top.window.close();
        }

        function CloseWindowFromContractInfo(dateType, refresh) {
            window.opener.document.forms[0].fvContractInfo$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].fvContractInfo$RefreshDateValueOnSubmit.value = dateType;
            window.opener.document.forms[0].submit();

            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromContractInfo2(dateType, refresh) {           
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ContractGeneralContractDatesFormView$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ContractGeneralContractDatesFormView$RefreshDateValueOnSubmit.value = dateType;
            
            /* this works on v2, consider adding it to other close functions here */
            if (refresh == "True") {
                window.opener.document.forms[0].submit();
            }
          
            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromItemInfo2(dateType, refresh) {
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$PricelistVerificationFormView$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$PricelistVerificationFormView$RefreshDateValueOnSubmit.value = dateType;
            window.opener.document.forms[0].submit();
            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromOfferRecord(dateType, refresh) {
            window.opener.document.forms[0].fvOfferRecord$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].fvOfferRecord$RefreshDateValueOnSubmit.value = dateType;
            window.opener.document.forms[0].submit();

            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromOfferRecord2(dateType, refresh) {
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$CommonContentPlaceHolder$OfferAttributesFormView$RefreshOfferAttributesOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$CommonContentPlaceHolder$OfferAttributesFormView$RefreshOfferAttributesDateValueOnSubmit.value = dateType;
            window.opener.document.forms[0].submit();

            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromContractCreation2(dateType, refresh) {
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$CommonContentPlaceHolder$CreateContractFormView$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$CommonContentPlaceHolder$CreateContractFormView$RefreshDateValueOnSubmit.value = dateType;
            /* this works on v2, consider adding it to other close functions here */
            if (refresh == "True") {
                window.opener.document.forms[0].submit();
            }

            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        function CloseWindowFromContractVendor(dateType, refresh) {
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ContractVendorInsuranceDatesFormView$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ContractVendorInsuranceDatesFormView$RefreshDateValueOnSubmit.value = dateType;
            /* this works on v2, consider adding it to other close functions here */
            if (refresh == "True") {
                window.opener.document.forms[0].submit();
            }

            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }

        /* the date control is in the sba projection grid and sets the value immediately into the corresponding text box */
        function CloseWindowFromSBAProjection(dateType, receivingControlClientId, refresh) {
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$RefreshOrNotOnSubmit.value = refresh;
            window.opener.document.forms[0].ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$RefreshDateValueOnSubmit.value = dateType;

            if (refresh == "True") {
                               
                var selectedDate = document.getElementById('SelectedDate').value;
              
                /* UniqueID = "ctl00$ctl00$ContentPlaceHolderMain$ContractViewContentPlaceHolder$ProjectionGridView$ctl02$endDateTextBox" */
                window.opener.document.forms[0].elements[receivingControlClientId].value = selectedDate;               
            }
            
            top.window.opener = top;
            top.window.open('', '_parent', '');
            top.window.close();
        }
    </script>
        <style type="text/css">
            #form1
            {
                width: 200px;
                height: 300px;
                margin-bottom: 5px;
                margin-left: 15px;
            }
            .style7
            {
                height: 33px;
            }
            .style8
            {
                width: 90px;
            }
            .style9
            {
                height: 198px;
            }
            .style10
            {
                height: 43px;
            }
        </style>
   </head>
<body>
    <form id="form1" runat="server" >
        <div style="text-align:center;" >
         <table style="height: 270px" >
             <tr>
                 <td style="text-align:center; height:24px;" >
                     <asp:Label ID="CalendarCaption" runat="server"  ></asp:Label>
                 </td>
             </tr>
             <tr>
                <td style="height:24px;" >
                    <table > 
                        <tr> 
                            <td class="style8" >
                                <asp:DropDownList ID="cbMonths" runat="server" AutoPostBack="True" 
                                    Font-Bold="True" Font-Names="Verdana" Font-Size="8pt" 
                                    ontextchanged="cbMonths_TextChanged" ToolTip="Month Quick Select">
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:DropDownList ID="cbYears" runat="server" 
                                    Font-Names="Verdana" 
                                    Font-Size="8pt" ontextchanged="cbYears_TextChanged" AutoPostBack="True" 
                                    Font-Bold="True" ToolTip="Year Quick Select">
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                </td>
             </tr>
             <tr>
                 <td class="style9" >
       
    
                    <asp:Calendar ID="calendar1" runat="server" BackColor="White" 
                        BorderColor="#999999" CellPadding="4" DayNameFormat="Shortest" 
                        Font-Names="Verdana" Font-Size="8pt" ForeColor="Black" Height="207px" 
                        Width="192px" onselectionchanged="calendar1_SelectionChanged" 
                                     onvisiblemonthchanged="calendar1_VisibleMonthChanged" 
                                       OnDayRender="calendar1_DayRender"
                                         OnPreRender="calendar1_OnPreRender"
                                     ToolTip="Click on desired date. Use scroll arrows to browse months." >
                        <SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
                        <SelectorStyle BackColor="#CCCCCC" />
                        <WeekendDayStyle BackColor="#FFFFCC" />
                        <TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
                        <OtherMonthDayStyle ForeColor="#808080" />
                        <NextPrevStyle VerticalAlign="Bottom" />
                        <DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
                        <TitleStyle BackColor="#999999" BorderColor="Black" Font-Bold="True" />
                    </asp:Calendar>
    
                 </td>
             </tr>
             <tr>
                 <td class="style10">
                             <asp:Button ID="FormCancelButton" runat="server" Style="z-index: 100; left: 41px; position: absolute;
            top: 280px" Text="Cancel" Width="60px" ToolTip="Cancel without changing selected date." CausesValidation="false" />
            
         <asp:Button ID="FormOkCloseButton" runat="server" Style="z-index: 100; left: 145px; position: absolute;
            top: 280px" Text="Ok" Width="60px" ToolTip="Accept selected date." CausesValidation="false" />
            
            </td>
             </tr>
         </table>
         <asp:HiddenField ID="SelectedDate" runat="server" ClientIDMode="Static" Value="0"  EnableViewState="true" />
    </div>
    
    </form>
</body>
</html>
