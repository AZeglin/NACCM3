<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DrugItemHeader.ascx.cs" Inherits="VA.NAC.CM.DrugItems.DrugItemHeader" %>

    <asp:Panel ID="ItemDetailsHeaderPanel" runat="server"  
    CssClass="DrugItemDetails" Width="99%"  Height="110px" >
      <div id="ItemDetailsHeaderDiv"  style="width:100%; height:98%; top:0px;  left:0px; 
            border:solid 1px; border-color:Black; background-color:White; text-align:center; margin-bottom:auto;" >
        <table style="height: 98%; width:100%; table-layout:fixed; margin-bottom:auto; margin-left:auto; margin-right:auto; margin-top:auto;" align="left"  >
            <colgroup>
                <col style="width:190px;" />
                <col style="width:100%;" />
                <col style="width:190px;" />
            </colgroup>
            <tr >
                <td style="height:20px;">
                </td>
                <td style="text-align:center; height:20px;">
                    <asp:Label ID="HeaderTitleLabel" runat="server" Text="No Item Selected"  CssClass="DrugItemHeaderText" />
                </td>
                <td style="height:20px;">
                </td>

            </tr>
            <tr >
                <td style="text-align:center; vertical-align:middle; height:24px;">
                    <asp:Label ID="CoveredLabel" runat="server" Width="80px" Text="Covered" CssClass="DrugItemRegularText" ForeColor="Coral"/>                    
                </td>
                <td  style="text-align:center; height:24px;">
                   <asp:Label ID="CombinedNDCLabel" runat="server" Width="100px" CssClass="DrugItemHeaderText"  ></asp:Label>
                </td>
                <td style="text-align:center; vertical-align:middle; height:24px;">
                    <asp:Label ID="FETAmountLabel" runat="server" Width="80px" Text="FET" CssClass="DrugItemRegularText" ForeColor="Crimson"/>
                </td>
            </tr>
            <tr  >
                
                <td style="text-align:center; vertical-align:middle; height:40px;">
                    <asp:Label ID="SingleDualLabel" runat="server" Width="80px" Text="Single Pricer" CssClass="DrugItemRegularText"/>                    
                </td>
                <td  style="vertical-align:top; height:40px;">
                    <table style="width:100%;" > 
                         <tr>
                            <td style="text-align:left; vertical-align:top; height:20px;">
                                <asp:Label ID="GenericNameLabel" runat="server" Width="120px" CssClass="DrugItemRegularText" Text="Generic Name" /> 
                        
                            </td>
                            <td  style="text-align:left; vertical-align:top; height:20px;">
                                <asp:Label ID="GenericNameLabelData" runat="server" Width="520px" CssClass="DrugItemRegularText" />
                          
                            </td>
                         </tr>
                         <tr>
                            <td style="text-align:left; vertical-align:top; height:20px;">
                                <asp:Label ID="TradeNameLabel" runat="server" Width="120px" CssClass="DrugItemRegularText" Text="Trade Name" /> 
                       
                            </td>
                            <td style="text-align:left; vertical-align:top; height:20px;">
                                <asp:Label ID="TradeNameLabelData" runat="server" Width="520px" CssClass="DrugItemRegularText" />
                   
                            </td>
                         </tr>
                    </table>
                    
                </td>
            </tr>
        
         </table>
     </div>   
    </asp:Panel>
   

   
    