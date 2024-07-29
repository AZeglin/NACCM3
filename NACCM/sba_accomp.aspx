<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sba_accomp.aspx.vb" Inherits="NACCM.sba_accomp" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SBA Accomplishments</title>
</head>
<body style="background-color: #ece9d8">
    <form id="form1" runat="server">
    <asp:Label ID="lbTitle" runat="server" Text="294/295 Information" Font-Size="X-Large" ForeColor="DarkBlue" />
   <asp:FormView ID="fvSBAAccomplish" runat="server" DataKeyNames="Acc_Record_ID">
   <ItemTemplate>
   <asp:Label ID="lbFiscalYear" runat="server" Text='<%#Eval("Fiscal_Year") %>' />&nbsp;
   <asp:Label ID="lbAccompPeriod" runat="server" Text='<%#Eval("Accomplishment_Period") %>' />
   <table>
   <tr><td></td><td>VA Attributable Percentage</td><td><asp:Label ID="lbVARatio" runat="server" Text='<%#Eval("VA_Ratio") %>' /></td><td></td></tr>
   <tr><td></td><td>Whole Dollars</td><td>Percentage</td><td>Applicable to VA</td></tr>
   <tr><td>Small Business Concerns:</td><td><asp:Label ID="lbSBConDollars" runat="server" Text='<%#Eval("SmallBusDollars", "{0:c}") %>' /></td><td><asp:Label ID="lbPerSB" runat="server" Text='<%#CalPercentDollars(Eval("SmallBusDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="lbSmallVA" runat="server" Text='<%#CalPercentToVA(Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Large Business Concerns:</td><td><asp:Label ID="lbLBConDollars" runat="server" Text='<%#Eval("LargeBusinessDollars","{0:c}") %>' /></td><td><asp:Label ID="Label9" runat="server" Text='<%#CalPercentDollars(Eval("LargeBusinessDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="lbLargeVA" runat="server" Text='<%#CalPercentToVA(Eval("LargeBusinessDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Total:</td><td><asp:Label ID="Label1" runat="server" Text='<%#CalTotalDollars(Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td></td><td><asp:Label ID="Label10" runat="server" Text='<%#CalTotalToVA(Eval("LargeBusinessDollars"),Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>SDB Concerns:</td><td><asp:Label ID="Label2" runat="server" Text='<%#Eval("SDBDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerrSB" runat="server" Text='<%#CalPercentDollars(Eval("SDBDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label11" runat="server" Text='<%#CalPercentToVA(Eval("SDBDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Woman Owned Concerns:</td><td><asp:Label ID="Label3" runat="server" Text='<%#Eval("WomanOwnedDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerWO" runat="server" Text='<%#CalPercentDollars(Eval("WomanOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label12" runat="server" Text='<%#CalPercentToVA(Eval("WomanOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Vet Owned Concerns:</td><td><asp:Label ID="Label4" runat="server" Text='<%#Eval("VetOwnedDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerVet" runat="server" Text='<%#CalPercentDollars(Eval("VetOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label13" runat="server" Text='<%#CalPercentToVA(Eval("VetOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Disabled Vet Concerns:</td><td><asp:Label ID="Label5" runat="server" Text='<%#Eval("DisabledVetDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerDVet" runat="server" Text='<%#CalPercentDollars(Eval("DisabledVetDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label14" runat="server" Text='<%#CalPercentToVA(Eval("DisabledVetDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Hub Zone Concerns:</td><td><asp:Label ID="Label6" runat="server" Text='<%#Eval("HubZoneDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerHub" runat="server" Text='<%#CalPercentDollars(Eval("HubZoneDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label15" runat="server" Text='<%#CalPercentToVA(Eval("HubZoneDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>HBDC Concerns</td><td><asp:Label ID="Label7" runat="server" Text='<%#Eval("HBDCDollars","{0:c}") %>' /></td><td><asp:Label ID="lbPerHBDC" runat="server" Text='<%#CalPercentDollars(Eval("HBDCDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label ID="Label16" runat="server" Text='<%#CalPercentToVA(Eval("HBDCDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr><td>Comments:</td><td colspan="3"><asp:Label ID="Label8" runat="server" Text='<%#Eval("Comments") %>' /></td></tr>
   <tr><td></td><td></td><td></td><td></td></tr>
   
   </table>
   
   </ItemTemplate>
   <EditItemTemplate>
   
     <table>
     <tr><td>Fiscal Year</td><td><asp:DropDownList id="dlFiscalYear" runat="server" SelectedValue='<%#Bind("Fiscal_Year") %>'><asp:ListItem Text="2001" Value="2001" />
     <asp:ListItem Text="2002" Value="2002" />
     <asp:ListItem Text="2003" Value="2003" />
     <asp:ListItem Text="2004" Value="2004" />
     <asp:ListItem Text="2005" Value="2005" />
     <asp:ListItem Text="2006" Value="2006" />
     <asp:ListItem Text="2007" Value="2007" />
     <asp:ListItem Text="2008" Value="2008" />
     <asp:ListItem Text="2009" Value="2009" />
     </asp:DropDownList></td><td><asp:DropDownList ID="dlAccompPeriod" runat="server" SelectedValue='<%#Bind("Accomplishment_Period") %>'>
     <asp:ListItem Text="294 Report 1" Value="294 Report 1" />
     <asp:ListItem Text="294 Report 2" Value="294 Report 2" />
     <asp:ListItem Text="295" Value="295" />
     </asp:DropDownList></td><td></td></tr>
   <tr><td></td><td>VA Attributable Percentage</td><td><asp:TextBox ID="tbVARatio" runat="server" Text='<%#Bind("VA_Ratio") %>' /></td><td></td></tr>
   <tr><td></td><td>Whole Dollars</td><td>Percentage</td><td>Applicable to VA</td></tr>
   <tr valign="bottom"><td style="width:200">Small Business Concerns:</td><td><asp:TextBox ID="tbSBConDollars" runat="server" Text='<%#Bind("SmallBusDollars", "{0:c}") %>' /></td><td><asp:Label ID="lbPerSB" runat="server" Text='<%#CalPercentDollars(Eval("SmallBusDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="lbSmallVA" runat="server" Text='<%#CalPercentToVA(Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Large Business Concerns:</td><td><asp:TextBox ID="tbLBConDollars" runat="server" Text='<%#Bind("LargeBusinessDollars","{0:c}") %>' /></td><td><asp:Label  ID="Label9" runat="server" Text='<%#CalPercentDollars(Eval("LargeBusinessDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="lbLargeVA" runat="server" Text='<%#CalPercentToVA(Eval("LargeBusinessDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Total:</td><td><asp:Label ID="lbTotal" runat="server" Text='<%#CalTotalDollars(Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td></td><td><asp:Label  ID="Label10" runat="server" Text='<%#CalTotalToVA(Eval("LargeBusinessDollars"),Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>SDB Concerns:</td><td><asp:TextBox ID="tbSDBDollars" runat="server" Text='<%#Bind("SDBDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerrSB" runat="server" Text='<%#CalPercentDollars(Eval("SDBDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label11" runat="server" Text='<%#CalPercentToVA(Eval("SDBDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Woman Owned Concerns:</td><td><asp:TextBox ID="tbWODollars" runat="server" Text='<%#Bind("WomanOwnedDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerWO" runat="server" Text='<%#CalPercentDollars(Eval("WomanOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label12" runat="server" Text='<%#CalPercentToVA(Eval("WomanOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Vet Owned Concerns:</td><td><asp:TextBox ID="tbVetDollars" runat="server" Text='<%#Bind("VetOwnedDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerVet" runat="server" Text='<%#CalPercentDollars(Eval("VetOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label13" runat="server" Text='<%#CalPercentToVA(Eval("VetOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Disabled Vet Concerns:</td><td><asp:TextBox ID="tbDVetDollars" runat="server" Text='<%#Bind("DisabledVetDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerDVet" runat="server" Text='<%#CalPercentDollars(Eval("DisabledVetDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label14" runat="server" Text='<%#CalPercentToVA(Eval("DisabledVetDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Hub Zone Concerns:</td><td><asp:TextBox ID="tbHubDollars" runat="server" Text='<%#Bind("HubZoneDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerHub" runat="server" Text='<%#CalPercentDollars(Eval("HubZoneDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label15" runat="server" Text='<%#CalPercentToVA(Eval("HubZoneDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>HBDC Concerns</td><td><asp:TextBox ID="tbHBDCDollars" runat="server" Text='<%#Bind("HBDCDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerHBDC" runat="server" Text='<%#CalPercentDollars(Eval("HBDCDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label16" runat="server" Text='<%#CalPercentToVA(Eval("HBDCDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="top"><td>Comments:</td><td colspan="3"><asp:TextBox ID="tbCommentsEdit" runat="server" Text='<%#Bind("Comments") %>' TextMode="MultiLine" Rows="6" Width="500" /></td></tr>
   <tr><td></td><td></td><td></td><td></td></tr>
   
   </table>
   <asp:Button ID="btnSaveEdit" runat="server" Text="Save" CommandName="Update" 
           onclick="btnSaveEdit_Click"/>
   <asp:Button ID="btnCloseEdit" runat="server" Text="Close" OnClick="CloseWindow" />
   </EditItemTemplate>
   <InsertItemTemplate>
     <table>
     <tr><td>Fiscal Year</td><td><asp:DropDownList id="dlFiscalYear" runat="server" SelectedValue='<%#Bind("Fiscal_Year") %>'><asp:ListItem Text="2001" Value="2001" />
     <asp:ListItem Text="2002" Value="2002" />
     <asp:ListItem Text="2003" Value="2003" />
     <asp:ListItem Text="2004" Value="2004" />
     <asp:ListItem Text="2005" Value="2005" />
     <asp:ListItem Text="2006" Value="2006" />
     <asp:ListItem Text="2007" Value="2007" />
     <asp:ListItem Text="2008" Value="2008" />
     <asp:ListItem Text="2009" Value="2009" />
     </asp:DropDownList></td><td><asp:DropDownList ID="dlAccompPeriod" runat="server" SelectedValue='<%#Bind("Accomplishment_Period") %>'>
     <asp:ListItem Text="294 Report 1" Value="294 Report 1" />
     <asp:ListItem Text="294 Report 2" Value="294 Report 2" />
     <asp:ListItem Text="295" Value="295" />
     </asp:DropDownList></td><td></td></tr>
   <tr><td></td><td>VA Attributable Percentage</td><td><asp:TextBox ID="tbVARatio" runat="server" Text='<%#Bind("VA_Ratio") %>' /></td><td></td></tr>
   <tr><td></td><td>Whole Dollars</td><td>Percentage</td><td>Applicable to VA</td></tr>
   <tr valign="bottom"><td style="width:200">Small Business Concerns:</td><td><asp:TextBox ID="tbSBConDollars" runat="server" Text='<%#Bind("SmallBusDollars", "{0:c}") %>' /></td><td><asp:Label ID="lbPerSB" runat="server" Text='<%#CalPercentDollars(Eval("SmallBusDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="lbSmallVA" runat="server" Text='<%#CalPercentToVA(Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Large Business Concerns:</td><td><asp:TextBox ID="tbLBConDollars" runat="server" Text='<%#Bind("LargeBusinessDollars","{0:c}") %>' /></td><td><asp:Label  ID="Label9" runat="server" Text='<%#CalPercentDollars(Eval("LargeBusinessDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="lbLargeVA" runat="server" Text='<%#CalPercentToVA(Eval("LargeBusinessDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Total:</td><td><asp:Label ID="lbTotal" runat="server" Text='<%#CalTotalDollars(Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td></td><td><asp:Label  ID="Label10" runat="server" Text='<%#CalTotalToVA(Eval("LargeBusinessDollars"),Eval("SmallBusDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>SDB Concerns:</td><td><asp:TextBox ID="tbSDBDollars" runat="server" Text='<%#Bind("SDBDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerrSB" runat="server" Text='<%#CalPercentDollars(Eval("SDBDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label11" runat="server" Text='<%#CalPercentToVA(Eval("SDBDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Woman Owned Concerns:</td><td><asp:TextBox ID="tbWODollars" runat="server" Text='<%#Bind("WomanOwnedDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerWO" runat="server" Text='<%#CalPercentDollars(Eval("WomanOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label12" runat="server" Text='<%#CalPercentToVA(Eval("WomanOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Vet Owned Concerns:</td><td><asp:TextBox ID="tbVetDollars" runat="server" Text='<%#Bind("VetOwnedDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerVet" runat="server" Text='<%#CalPercentDollars(Eval("VetOwnedDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label13" runat="server" Text='<%#CalPercentToVA(Eval("VetOwnedDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Disabled Vet Concerns:</td><td><asp:TextBox ID="tbDVetDollars" runat="server" Text='<%#Bind("DisabledVetDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerDVet" runat="server" Text='<%#CalPercentDollars(Eval("DisabledVetDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label14" runat="server" Text='<%#CalPercentToVA(Eval("DisabledVetDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>Hub Zone Concerns:</td><td><asp:TextBox ID="tbHubDollars" runat="server" Text='<%#Bind("HubZoneDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerHub" runat="server" Text='<%#CalPercentDollars(Eval("HubZoneDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label15" runat="server" Text='<%#CalPercentToVA(Eval("HubZoneDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="bottom"><td>HBDC Concerns</td><td><asp:TextBox ID="tbHBDCDollars" runat="server" Text='<%#Bind("HBDCDollars","{0:c}") %>' /></td><td><asp:Label  ID="lbPerHBDC" runat="server" Text='<%#CalPercentDollars(Eval("HBDCDollars"),Eval("LargeBusinessDollars"),Eval("SmallBusDollars")) %>' /></td><td><asp:Label  ID="Label16" runat="server" Text='<%#CalPercentToVA(Eval("HBDCDollars"),Eval("VA_Ratio")) %>' /></td></tr>
   <tr valign="top"><td>Comments:</td><td colspan="3"><asp:TextBox ID="tbCommentsEdit" runat="server" Text='<%#Bind("Comments") %>' TextMode="MultiLine" Rows="6" Width="500" /></td></tr>
   <tr><td></td><td></td><td></td><td></td></tr>
   
   </table>
   <asp:Button ID="btnSaveInsert" runat="server" Text="Save" CommandName="Insert" 
           onclick="btnSaveInsert_Click"/>
   <asp:Button ID="btnCloseInsert" runat="server" Text="Close" OnClick="CloseWindow" />
  
   </InsertItemTemplate>
   </asp:FormView>
   
    </form>
</body>
</html>
