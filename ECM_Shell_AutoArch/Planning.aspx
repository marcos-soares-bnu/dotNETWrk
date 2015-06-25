<%@ Page    Title="Planning" 
            Language="C#" 
            MasterPageFile="~/Site.Master" 
            AutoEventWireup="true"
            EnableEventValidation="false"     
            CodeBehind="Planning.aspx.cs" 
            Inherits="ECM_Shell_AutoArch.Planning" 
%>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <br />
    <h2>
        Planning - Archiving Orders Management
    </h2>
    <br />
    <asp:Panel 
        ID="panExterno" 
        runat="server" 
        Height="75%" 
        ScrollBars="None" 
        Width="100%">
        <asp:Table 
            ID="tabInterno" 
            runat="server" 
            BorderStyle="Ridge" 
            BorderWidth="1px"
            Height="100%" 
            Width="100%">
            <asp:TableRow ID="TableRow0" runat="server">
                <asp:TableCell ID="c1l1" runat="server" Width="30%">
                    <asp:Panel 
                        ID="panMenuInt" 
                        runat="server" 
                        Height="640px" 
                        ScrollBars="None" 
                        Width="300px">
                        <asp:Panel 
                            ID="Panel2" 
                            runat="server" 
                            Height="97%" 
                            ScrollBars="Vertical" 
                            Width="100%">
                            <br />
                            <asp:Button 
                                CssClass="myButton" ID="btn_Clean" runat="server" 
                                Text="Clear" onclick="btn_Clean_Click" Width="31%" Visible="True" />
                            <asp:Button 
                                CssClass="myButton" ID="btn_Filter" runat="server"
                                Text="Filter" onclick="btn_Filter_Click" Width="31%" Visible="True" />
                            <asp:Button 
                                CssClass="myButton" ID="btn_Save" runat="server" 
                                Text="Save" onclick="btn_Save_Click" Width="31%" Visible="True" />
                            <br />
                            <br />
			                <asp:Label ID="lbl1_1" runat="server" Text="Col.Site" Width="30%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl1_2" runat="server" Text="Operator" Width="55%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <br />
			                <asp:TextBox ID="txt_colsitenum" runat="server" Width="25%"></asp:TextBox>
			                <asp:Label ID="spc1" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_operatorname" runat="server" Width="65%" Text=""></asp:TextBox>
                            <br />
                            <br />
			                <asp:Label ID="lbl2_1" runat="server" Text="Unique Id" Width="40%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl2_2" runat="server" Text="Source Name" Width="45%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
                            <asp:ImageButton ID="imgCheckSource" OnClick="imgCheckSource_Click" ImageUrl="~/Images/exe.ico" ToolTip="Click to check if Source Name exists!" Height="18px" Width="18px" runat="server" />
                            <br />
			                <asp:TextBox ID="txt_uniqueid" runat="server" Width="35%"></asp:TextBox>
			                <asp:Label ID="spc2" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_instanceid" runat="server" Width="55%" Text=""></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl3_1" runat="server" Text="OU/DU Name" Font-Bold="True"></asp:Label>
			                <br />
                            <asp:TextBox ID="txt_duname" runat="server" Width="94%"></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl4_1" runat="server" Text="Data?" Width="20%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl4_2" runat="server" Text="Waiv?" Width="20%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl4_3" runat="server" Text="Vol.  (GB)" Width="30%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl4_4" runat="server" Text="WkDays" Width="15%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <br />
			                <asp:CheckBox ID="chk_dataexpctrchk" runat="server" Checked="False" Width="20%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt" />
			                <asp:CheckBox ID="chk_waiverchk" runat="server" Checked="False" Width="20%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt" />
			                <asp:TextBox ID="txt_volumegb" runat="server" Width="25%"></asp:TextBox>
			                <asp:Label ID="spc3" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_durationworkdays" runat="server" Width="21%" Text=""></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl5_1" runat="server" Text="Rec. Date (Plan)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl5_2" runat="server" Text="Rec. Date (Act)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <br />
			                <asp:TextBox ID="txt_planaoreceivedate" runat="server" Width="44%" AutoPostBack="true"></asp:TextBox>
                            <asp:Label ID="spc4" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_actaoreceivedate" runat="server" Width="44%" Text=""></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl6_1" runat="server" Text="Start Date (Plan)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl6_2" runat="server" Text="Start Date (Act)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <br />
			                <asp:TextBox ID="txt_planaostartdate" runat="server" Width="44%"></asp:TextBox>
			                <asp:Label ID="spc5" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_actaostartdate" runat="server" Width="44%" Text=""></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl7_1" runat="server" Text="End Date (Plan)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <asp:Label ID="lbl7_2" runat="server" Text="End Date (Act)" Width="47%" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:Label>
			                <br />
			                <asp:TextBox ID="txt_planaoenddate" runat="server" Width="44%"></asp:TextBox>
			                <asp:Label ID="spc6" runat="server" Width="1%" Text=""></asp:Label>
			                <asp:TextBox ID="txt_actaoenddate" runat="server" Width="44%" Text=""></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl8_1" runat="server" Text="Status" Font-Bold="True"></asp:Label>
			                <br />
                            <asp:TextBox ID="txt_status" runat="server" Width="94%"></asp:TextBox>
			                <br />
			                <br />
			                <asp:Label ID="lbl9_1" runat="server" Text="Comment" Font-Bold="True"></asp:Label>
			                <br />
                            <asp:TextBox ID="txt_comment" runat="server" Width="94%" TextMode="MultiLine" Rows="7"></asp:TextBox>
                        </asp:Panel>
                    </asp:Panel>
                </asp:TableCell>
                <asp:TableCell ID="c2l1" runat="server" Width="1%">
                    <asp:Panel 
                        ID="panInterno" 
                        runat="server" 
                        Height="640px" 
                        ScrollBars="None" 
                        Width="600px">
                        <asp:Panel 
                            ID="panGrids" 
                            runat="server" 
                            Height="97%" 
                            ScrollBars="Both" 
                            Width="100%">
                            <br />
                            <asp:TreeView ID="TreeView1" runat="server" 
                                                    Font-Size="Small"
                                                    Font-Names="Courier New" Font-Bold="True">
                                <NodeStyle          ForeColor="#993300"></NodeStyle>
                                <ParentNodeStyle    ForeColor="#993300" />
                                <RootNodeStyle      ForeColor="#993300" BackColor="#CCCCCC"></RootNodeStyle>
                                <SelectedNodeStyle  ForeColor="#993300" BackColor="#CCCCCC"/>
                            </asp:TreeView>
                            <br />
                            <asp:GridView ID="GridView1" runat="server" CssClass="grid" CellPadding="4" 
                                onrowcommand="GridView1_RowCommand" onrowdatabound="GridView1_RowDataBound" 
                                ForeColor="#333333" GridLines="None" Font-Names="Verdana" Font-Size="8pt" >
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="Silver" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
                                <sortedascendingcellstyle backcolor="#E9E7E2" />
                                <sortedascendingheaderstyle backcolor="#506C8C" />
                                <sorteddescendingcellstyle backcolor="#FFFDF8" />
                                <sorteddescendingheaderstyle backcolor="#6F8DAE" />
                                <Columns>
                                    <asp:TemplateField>
                                      <ItemTemplate>
                                        <asp:ImageButton 
                                                ImageUrl="~/Images/sel.ico" 
                                                ID="bntVer" 
                                                runat="server" 
                                                CommandName="View" 
                                                CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                                ToolTip="Select a Row to Edit!" Height="24px" Width="24px" />
                                      </ItemTemplate> 
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                      <ItemTemplate>
                                        <asp:ImageButton
                                                ImageUrl="~/Images/del.ico" 
                                                ID="bntDel" 
                                                runat="server" 
                                                CommandName="Del" 
                                                CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                                ToolTip="Select a Row to Delete!" Height="24px" Width="24px" />
                                      </ItemTemplate> 
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>
                        <asp:Button 
                            CssClass="myButton" ID="btnGeraClasse" runat="server" 
                            Text="Export Planning" onclick="btnExport_Click" Width="96%" Visible="True" />
                    </asp:Panel>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <br />
    <asp:Panel ID="panAviso" runat="server" Height="100%" ScrollBars="Vertical" Width="100%" Visible="True">
        <div 
            ID="divMessage" runat="server" >
        </div>
    </asp:Panel>
    <br />

</asp:Content>
