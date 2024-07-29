<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemHeader.ascx.cs" Inherits="VA.NAC.CM.ApplicationStartup.ItemHeader" %>

    <asp:Panel ID="ItemDetailsHeaderPanel" runat="server"  
    CssClass="ItemDetails" Width="99%"  Height="110px" >
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
                    <asp:Label ID="HeaderTitleLabel" runat="server" Text="No Item Selected"  CssClass="ItemHeaderText" />
                </td>
                <td style="height:20px;">
                </td>

            </tr>           
            <tr  >
                
                <td style="height:40px;">                                     
                </td>
                <td  style="vertical-align:top; height:40px;">
                    <table style="width:100%;" > 
                          <tr>
                            <td style="text-align:left; vertical-align:middle; height:20px;">
                                <asp:Label ID="CatalogNumberLabel" runat="server" Width="180px" CssClass="ItemRegularText"  />                         
                            </td>
                            <td  style="text-align:left; vertical-align:middle; height:20px;">
                                <asp:Label ID="CatalogNumberLabelData" runat="server" Width="460px" CssClass="ItemRegularText" />                          
                            </td>
                         </tr>       
                         <tr>
                            <td style="text-align:left; vertical-align:middle; height:20px;">
                                <asp:Label ID="ItemDescriptionLabel" runat="server" Width="180px" CssClass="ItemRegularText" Text="Item Description" />                         
                            </td>
                            <td  style="text-align:left; vertical-align:middle; height:20px;">
                                <asp:Label ID="ItemDescriptionLabelData" runat="server" Width="460px" CssClass="ItemRegularText" />                          
                            </td>
                         </tr>                        
                    </table>
                    
                </td>
                <td style="height:40px;">                                     
                </td>

            </tr>
        
         </table>
     </div>   
    </asp:Panel>
   

   
    