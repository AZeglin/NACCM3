<%@ Control Language="C#" AutoEventWireup="true"  CodeBehind="CompactDrugItemHeader.ascx.cs" Inherits="VA.NAC.CM.DrugItems.CompactDrugItemHeader" %>

    <asp:Panel ID="ItemDetailsHeaderPanel" runat="server"  CssClass="DrugItemDetails" Width="534px" Height="90px" >
      <div id="ItemDetailsHeaderDiv"  style="width:534px; height:88px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;" >
        <table style="height: 86px; table-layout:fixed; width: 526px; text-align:left;"  >
            <tr>
                <td style="text-align:center; width:100%;">
                    <asp:Label ID="HeaderTitleLabel" runat="server" Text="No Item Selected"  CssClass="DrugItemHeaderText" />
                </td>

            </tr>
            <tr>
               <td style="text-align:center; width:100%;">
                   <asp:Label ID="CombinedNDCLabel" runat="server" Width="100px" CssClass="DrugItemHeaderText"  ></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="1" >
                    <asp:Label ID="GenericNameLabel" runat="server" Width="22%" CssClass="DrugItemSmallText" Text="Generic Name" /> 
                    
                     <asp:Label ID="GenericNameLabelData" runat="server" Width="74%" CssClass="DrugItemRegularText" />
                </td>
            </tr>
            <tr>
                <td colspan="1"> 
                   <asp:Label ID="TradeNameLabel" runat="server" Width="22%" CssClass="DrugItemSmallText" Text="Trade Name" Height="16px" /> 
                    
                   <asp:Label ID="TradeNameLabelData" runat="server" Width="74%" CssClass="DrugItemRegularText" />
                </td>
            </tr>
        </table>
     </div>   
    </asp:Panel>
   

   
    