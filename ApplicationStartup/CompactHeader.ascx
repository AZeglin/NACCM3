<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CompactHeader.ascx.cs" Inherits="VA.NAC.CM.ApplicationStartup.CompactHeader" %>

    <asp:Panel ID="ItemDetailsHeaderPanel" runat="server"  
    CssClass="ItemDetails" Width="488px" Height="78px" >
      <div id="ItemDetailsHeaderDiv"  style="width:486px; height:76px; top:0px;  left:0px; border:solid 1px black; background-color:White; margin:1px;" >
        <table style="height: 76px; table-layout:fixed; width: 484px; text-align:left;"  >
            <tr>
                <td style="text-align:center; width:100%;" >
                    <asp:Label ID="HeaderTitleLabel" runat="server" Text="No Item Selected"  CssClass="HeaderText" />
                </td>

            </tr>
            <tr>
               <td style="text-align:center; width:100%;" >
                   <asp:Label ID="ContractNumberLabelData" runat="server" Width="100px" CssClass="HeaderText"  ></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="1" >
                    <asp:Label ID="ContractorNameLabel" runat="server" Width="30%" CssClass="SmallText" Text="Contractor Name" /> 
                    
                     <asp:Label ID="ContractorNameLabelData" runat="server" Width="66%" CssClass="SmallText" />
                </td>
            </tr>
            <tr>
                <td colspan="1"> 
                   <asp:Label ID="CommodityCoveredLabel" runat="server" Width="30%" CssClass="SmallText" Text="Commodity Covered"  /> 
                    
                   <asp:Label ID="CommodityCoveredLabelData" runat="server" Width="66%" CssClass="SmallText" />
                </td>
            </tr>
        </table>
     </div>   
    </asp:Panel>
   

   
    