<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="sba_projections.aspx.vb" Inherits="NACCM.sba_projectons" %>

<!DOCTYPE html />

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SBA Projections</title>
</head>
<body style="background-color:#ECE9D8">
    <form id="form1" runat="server">
    <asp:Button id="btnClose" runat="server" Text="Close" />
    <asp:Panel ID="mainPanel" runat="server" ScrollBars="Vertical" Height="425">
    <asp:FormView ID="fvSBAProjections"  runat="server" DataSourceID="SBAProjectionsDataSource" DataKeyNames="ProjectionID,SBAPlanID" Font-Names="Arial" HeaderStyle-Font-Size="Large" HeaderStyle-ForeColor="#000099" HeaderStyle-HorizontalAlign="Center">
    <HeaderTemplate>
    View/Edit SBA Projection
    </HeaderTemplate>
    <EditItemTemplate>
    <table>
    <tr><td>StartDate</td><td><asp:TextBox runat="server" ID="tbStateDate" Text='<%#Bind("StartDate","{0:d}") %>' /></td><td>EndDate</td><td><asp:TextBox runat="server" ID="tbEnddate" Text='<%#Bind("EndDate","{0:d}") %>' /></td></tr>
    <tr><td>Total Subcontracting Dollars</td><td><asp:TextBox runat="server" ID="TextBox1" Text='<%#Bind("TotalSubConDollars","{0:n2}") %>' /></td><td></td><td></td></tr>
    <tr><td>SB Dollars:</td><td><asp:TextBox runat="server" ID="TextBox2" Text='<%#Bind("SBDollars","{0:n2}") %>' /></td><td>SB Percent:</td><td><asp:TextBox runat="server" ID="TextBox4" Text='<%#CalPercentage(Eval("SBDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true" /></td></tr>
    <tr><td>Veteran Owned:</td><td><asp:TextBox runat="server" ID="TextBox10" Text='<%#Bind("VeteranOwnedDollars","{0:n2}") %>' /></td><td>Veteran Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox16" Text='<%#CalPercentage(Eval("VeteranOwnedDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true"/></td></tr>
    <tr><td>Disabled Vet:</td><td><asp:TextBox runat="server" ID="TextBox7" Text='<%#Bind("DisabledVetDollars","{0:n2}") %>' /></td><td>Disabled Vet Percent:</td><td><asp:TextBox runat="server" ID="TextBox13" Text='<%#CalPercentage(Eval("DisabledVetDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true"/></td></tr>
    <tr><td>SDB Dollars:</td><td><asp:TextBox runat="server" ID="TextBox5" Text='<%#Bind("SDBDollars","{0:n2}") %>' /></td><td>SDB Percent:</td><td><asp:TextBox runat="server" ID="TextBox3" Text='<%#CalPercentage(Eval("SDBDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true" /></td></tr>
    <tr><td>Women Owned:</td><td><asp:TextBox runat="server" ID="TextBox6" Text='<%#Bind("WomenOwnedDollars","{0:n2}") %>' /></td><td>Women Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox12" Text='<%#CalPercentage(Eval("WomenOwnedDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true"/></td></tr>
    <tr><td>Hub Zone:</td><td><asp:TextBox runat="server" ID="TextBox8" Text='<%#Bind("HubZoneDollars","{0:n2}") %>' /></td><td>Hub Zone Percent:</td><td><asp:TextBox runat="server" ID="TextBox14" Text='<%#CalPercentage(Eval("HubZoneDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true" /></td></tr>
    <tr><td>HBCU Dollars:</td><td><asp:TextBox runat="server" ID="TextBox9" Text='<%#Bind("HBCUDollars","{0:n2}") %>' /></td><td>HBCU Percent:</td><td><asp:TextBox runat="server" ID="TextBox15" Text='<%#CalPercentage(Eval("HBCUDollars"),Eval("TotalSubConDollars")) %>' ReadOnly="true" /></td></tr>
    <tr valign="top"><td>Comments</td><td colspan="3"><asp:TextBox runat="server" ID="TextBox11" Text='<%#Bind("Comments") %>' TextMode="MultiLine" Rows="5" Columns="50" /></td></tr>
    </table>
  
    <asp:Button ID="updateProjectionButton" runat="server" CommandName="Update" text="Save"/>
    </EditItemTemplate>
    <ItemTemplate>
    <table>
    <tr><td>StartDate</td><td><asp:TextBox runat="server" ID="tbStateDate" Text='<%#Eval("StartDate","{0:d}") %>' ReadOnly="true" /></td><td>EndDate</td><td><asp:TextBox runat="server" ID="tbEnddate" Text='<%#Eval("EndDate","{0:d}") %>' ReadOnly="true" /></td></tr>
    <tr><td>Total Subcontracting Dollars</td><td><asp:TextBox runat="server" ID="TextBox1" Text='<%#Eval("TotalSubConDollars","{0:c}") %>' /></td><td></td><td></td></tr>
    <tr><td>SB Dollars:</td><td><asp:TextBox runat="server" ID="TextBox2" Text='<%#Eval("SBDollars","{0:c}") %>' ReadOnly="true"/></td><td>SB Percent:</td><td><asp:TextBox runat="server" ID="TextBox4" Text='<%#CalPercentage(Eval("SBDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>Veteran Owned:</td><td><asp:TextBox runat="server" ID="TextBox10" Text='<%#Eval("VeteranOwnedDollars","{0:c}") %>' ReadOnly="true" /></td><td>Veteran Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox16" Text='<%#CalPercentage(Eval("VeteranOwnedDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>Disabled Vet:</td><td><asp:TextBox runat="server" ID="TextBox7" Text='<%#Eval("DisabledVetDollars","{0:c}") %>' ReadOnly="true"/></td><td>Disabled Vet Percent:</td><td><asp:TextBox runat="server" ID="TextBox13" Text='<%#CalPercentage(Eval("DisabledVetDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>SDB Dollars:</td><td><asp:TextBox runat="server" ID="TextBox5" Text='<%#Eval("SDBDollars","{0:c}") %>' ReadOnly="true" /></td><td>SDB Percent:</td><td><asp:TextBox runat="server" ID="TextBox3" Text='<%#CalPercentage(Eval("SBDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>Women Owned:</td><td><asp:TextBox runat="server" ID="TextBox6" Text='<%#Eval("WomenOwnedDollars","{0:c}") %>' ReadOnly="true"/></td><td>Women Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox12" Text='<%#CalPercentage(Eval("WomenOwnedDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>Hub Zone:</td><td><asp:TextBox runat="server" ID="TextBox8" Text='<%#Eval("HubZoneDollars","{0:c}") %>' ReadOnly="true" /></td><td>Hub Zone Percent:</td><td><asp:TextBox runat="server" ID="TextBox14" Text='<%#CalPercentage(Eval("HubZoneDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr><td>HBCU Dollars:</td><td><asp:TextBox runat="server" ID="TextBox9" Text='<%#Eval("HBCUDollars","{0:c}") %>' ReadOnly="true" /></td><td>HBCU Percent:</td><td><asp:TextBox runat="server" ID="TextBox15" Text='<%#CalPercentage(Eval("HBCUDollars"),Eval("TotalSubConDollars")) %>' /></td></tr>
    <tr valign="top"><td>Comments</td><td colspan="3"><asp:TextBox runat="server" ID="TextBox11" Text='<%#Eval("Comments") %>' ReadOnly="true" TextMode="MultiLine" Rows="5" Columns="50" /></td></tr>
    </table>
    </ItemTemplate>
    <InsertItemTemplate>
      <table>
    <tr><td>StartDate</td><td><asp:TextBox runat="server" ID="tbStateDate" Text='<%#Bind("StartDate","{0:d}") %>' /></td><td>EndDate</td><td><asp:TextBox runat="server" ID="tbEnddate" Text='<%#Bind("EndDate","{0:d}") %>' /></td></tr>
    <tr><td>Total Subcontracting Dollars</td><td><asp:TextBox runat="server" ID="tbInsertTotalDollars" Text='<%#Bind("TotalSubConDollars","{0:n2}") %>'  /></td><td></td><td></td></tr>
    <tr><td>SB Dollars:</td><td><asp:TextBox runat="server" ID="tbInsertDollars" Text='<%#Bind("SBDollars","{0:n2}") %>' OnTextChanged="CalInsertPercentage" /></td><td>SB Percent:</td><td>
    <asp:Label ID="lbTest" runat="server" Text="Test" />
    <asp:TextBox runat="server" ID="tbInsertSBPercent"  ReadOnly="true" /></td></tr>
    <tr><td>Veteran Owned:</td><td><asp:TextBox runat="server" ID="TextBox10" Text='<%#Bind("VeteranOwnedDollars","{0:n2}") %>' /></td><td>Veteran Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox16"  ReadOnly="true"/></td></tr>
    <tr><td>Disabled Vet:</td><td><asp:TextBox runat="server" ID="TextBox7" Text='<%#Bind("DisabledVetDollars","{0:n2}") %>' /></td><td>Disabled Vet Percent:</td><td><asp:TextBox runat="server" ID="TextBox13"  ReadOnly="true"/></td></tr>
    <tr><td>SDB Dollars:</td><td><asp:TextBox runat="server" ID="TextBox5" Text='<%#Bind("SDBDollars","{0:n2}") %>' /></td><td>SDB Percent:</td><td><asp:TextBox runat="server" ID="TextBox3"  ReadOnly="true" /></td></tr>
    <tr><td>Women Owned:</td><td><asp:TextBox runat="server" ID="TextBox6" Text='<%#Bind("WomenOwnedDollars","{0:n2}") %>' /></td><td>Women Owned Percent:</td><td><asp:TextBox runat="server" ID="TextBox12"  ReadOnly="true"/></td></tr>
    <tr><td>Hub Zone:</td><td><asp:TextBox runat="server" ID="TextBox8" Text='<%#Bind("HubZoneDollars","{0:n2}") %>' /></td><td>Hub Zone Percent:</td><td><asp:TextBox runat="server" ID="TextBox14"  ReadOnly="true" /></td></tr>
    <tr><td>HBCU Dollars:</td><td><asp:TextBox runat="server" ID="TextBox9" Text='<%#Bind("HBCUDollars","{0:n2}") %>' /></td><td>HBCU Percent:</td><td><asp:TextBox runat="server" ID="TextBox15"  ReadOnly="true" /></td></tr>
    <tr valign="top"><td>Comments</td><td colspan="3"><asp:TextBox runat="server" ID="TextBox11" Text='<%#Bind("Comments") %>' TextMode="MultiLine" Rows="5" Columns="50" /></td></tr>
    </table>
    <asp:Button ID="insertProjectionButton" runat="server" Text="Save" CommandName="Insert" />
    </InsertItemTemplate>
    </asp:FormView>
    </asp:Panel>
    <asp:SqlDataSource ID="SBAProjectionsDataSource" runat="server" 
        ConnectionString="<%$ ConnectionStrings:CM %>" 
        DeleteCommand="DELETE FROM [tbl_sba_Projection] WHERE [ProjectionID] = @ProjectionID" 
        InsertCommand="INSERT INTO [tbl_sba_Projection] ([SBAPlanID], [SBDollars], [SDBDollars], [WomenOwnedDollars], [DisabledVetDollars], [HubZoneDollars], [HBCUDollars], [VeteranOwnedDollars], [TotalSubConDollars], [Comments], [EndDate], [StartDate], CreatedBy, CreationDate, LastModifiedBy, LastModificationDate) VALUES (@SBAPlanID, @SBDollars, @SDBDollars, @WomenOwnedDollars, @DisabledVetDollars, @HubZoneDollars, @HBCUDollars, @VeteranOwnedDollars, @TotalSubConDollars, @Comments, @EndDate, @StartDate, @CreatedBy, @CreationDate, @LastModifiedBy, @LastModificationDate)" 
        SelectCommand="SELECT * FROM [tbl_sba_Projection] WHERE [SBAPlanID] = @SBAPlanID AND [ProjectionID] = @ProjectionID" 
        UpdateCommand="UPDATE [tbl_sba_Projection] SET [SBDollars] = @SBDollars, [SDBDollars] = @SDBDollars, [WomenOwnedDollars] = @WomenOwnedDollars, [DisabledVetDollars] = @DisabledVetDollars, [HubZoneDollars] = @HubZoneDollars, [HBCUDollars] = @HBCUDollars, [VeteranOwnedDollars] = @VeteranOwnedDollars, [TotalSubConDollars] = @TotalSubConDollars, [Comments] = @Comments, [EndDate] = CONVERT(Datetime,@EndDate), [StartDate] = CONVERT(datetime,@StartDate), LastModifiedBy = @LastModifiedBy, LastModificationDate = @LastModificationDate WHERE [ProjectionID] = @ProjectionID">
        <SelectParameters>
            <asp:QueryStringParameter Name="SBAPlanID" QueryStringField="SBAPlanID" 
                Type="Int32" />
            <asp:QueryStringParameter Name="ProjectionID" QueryStringField="ProjID" 
                Type="Int32" />
        </SelectParameters>
        <DeleteParameters>
            <asp:Parameter Name="ProjectionID" Type="Int32" />
        </DeleteParameters>
        <UpdateParameters>
             <asp:Parameter Name="SBDollars" Type="Decimal" />
            <asp:Parameter Name="SDBDollars" Type="Decimal" />
            <asp:Parameter Name="WomenOwnedDollars" Type="Decimal" />
            <asp:Parameter Name="DisabledVetDollars" Type="Decimal" />
            <asp:Parameter Name="HubZoneDollars" Type="Decimal" />
            <asp:Parameter Name="HBCUDollars" Type="Decimal" />
            <asp:Parameter Name="VeteranOwnedDollars" Type="Decimal" />
            <asp:Parameter Name="TotalSubConDollars" Type="Decimal" />
            <asp:Parameter Name="Comments" Type="String" />
            <asp:Parameter Name="EndDate" Type="DateTime" />
            <asp:Parameter Name="StartDate" Type="DateTime" />
           
        </UpdateParameters>
        <InsertParameters>
            <asp:Parameter Name="SBAPlanID" Type="Int32" />
            <asp:Parameter Name="SBDollars" Type="Decimal" />
            <asp:Parameter Name="SDBDollars" Type="Decimal" />
            <asp:Parameter Name="WomenOwnedDollars" Type="Decimal" />
            <asp:Parameter Name="DisabledVetDollars" Type="Decimal" />
            <asp:Parameter Name="HubZoneDollars" Type="Decimal" />
            <asp:Parameter Name="HBCUDollars" Type="Decimal" />
            <asp:Parameter Name="VeteranOwnedDollars" Type="Decimal" />
            <asp:Parameter Name="TotalSubConDollars" Type="Decimal" />
            <asp:Parameter Name="Comments" Type="String" />
            <asp:Parameter Name="EndDate" Type="DateTime" />
            <asp:Parameter Name="StartDate" Type="DateTime" />
 
        </InsertParameters>
    </asp:SqlDataSource>
    </form>
</body>
</html>
