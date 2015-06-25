<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Operation.aspx.cs" Inherits="ECM_Shell_AutoArch.Operation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <h2>
        Operation - Archiving Orders Generation
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
			                <asp:Label 
				            ID="Label1" runat="server" 
				            Text="List of Archiving Orders (Pre-Analysis)" Font-Bold="True">
			                </asp:Label>
			                <br />
			                <asp:ListBox 
				            ID="lstAOs" runat="server" Height="64px" Width="95%" 
				            AutoPostBack="True" OnSelectedIndexChanged="lstAOs_SelectedIndexChanged" EnableViewState="True">
			                </asp:ListBox>
			                <br />
			                <br />
			                <asp:Label 
				            ID="Label2" runat="server"
				            Text="List of Archiving Requests Form (Server)" Font-Bold="True">
			                </asp:Label>
			                <br />
			                <div id='div1' style="Z-INDEX: 101; OVERFLOW: auto; WIDTH: 95%; HEIGHT: 200px" >
			                    <asp:ListBox 
				                ID="lstReqPlan" runat="server" Height="100%" Width="150%"
				                AutoPostBack="True" OnSelectedIndexChanged="lstReqPlan_SelectedIndexChanged" EnableViewState="True">
			                    </asp:ListBox>
                            </div>
                            <asp:FileUpload 
                                ToolTip="Archiving Request Form..."
                                id="FileUpload1" runat="server" Width="94%">
                            </asp:FileUpload>
                            <asp:Button 
                                CssClass="myButton" ID="btn_Upload" runat="server" 
                                Text="Upload Request Form..." onclick="btn_Upload_Click" Width="94%" Visible="True" />
                            <br />
                            <br />
                            <br />
			                <asp:Label 
				            ID="Label3" runat="server" 
				            Text="List of SQL's created (Server)" Font-Bold="True">
			                </asp:Label>
			                <br />
			                <div id='div2' style="Z-INDEX: 102; OVERFLOW: auto; WIDTH: 95%; HEIGHT: 200px" >
			                    <asp:ListBox 
				                ID="lstSQLs" runat="server" Height="100%" Width="150%"
				                AutoPostBack="True" OnSelectedIndexChanged="lstSQLs_SelectedIndexChanged" EnableViewState="True">
			                    </asp:ListBox>
                            </div>
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
                            <asp:Label ID="dtSel3" runat="server" Text="-" BackColor="#CCCCCC" ForeColor="#993300" Font-Bold="True" Font-Size="Small"></asp:Label>
                            <asp:TreeView ID="TreeView1" runat="server" 
                                                    Font-Size="Small"
                                                    Font-Names="Courier New" Font-Bold="True">
                                <NodeStyle          ForeColor="#993300"></NodeStyle>
                                <ParentNodeStyle    ForeColor="#993300" />
                                <RootNodeStyle      ForeColor="#993300" BackColor="#CCCCCC"></RootNodeStyle>
                                <SelectedNodeStyle  ForeColor="#993300" BackColor="#CCCCCC"/>
                            </asp:TreeView>
                            <br />
                            <asp:Label ID="dtSel1" runat="server" Text="-" BackColor="Black" ForeColor="White" Font-Bold="True" Font-Size="Small"></asp:Label>
                            <br />
                            <asp:GridView ID="GridView1" runat="server" CssClass="grid" CellPadding="4" 
                                ForeColor="#333333" GridLines="None" Font-Names="Verdana" Font-Size="8pt" >
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#000000" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
                                <sortedascendingcellstyle backcolor="#E9E7E2" />
                                <sortedascendingheaderstyle backcolor="#506C8C" />
                                <sorteddescendingcellstyle backcolor="#FFFDF8" />
                                <sorteddescendingheaderstyle backcolor="#6F8DAE" />
                            </asp:GridView>
                            <br />
                            <asp:Label ID="dtSel2" runat="server" Text="-" BackColor="#1C5E55" ForeColor="White" Font-Bold="True" Font-Size="Small"></asp:Label>
                            <br />
                            <asp:GridView ID="GridView2" runat="server" CssClass="grid" CellPadding="4" 
                                ForeColor="#333333" GridLines="None" Font-Names="Verdana" Font-Size="8pt" >
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                <EditRowStyle BackColor="#999999" />
                                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#E2DED6" ForeColor="#333333" Font-Bold="True" />
                                <sortedascendingcellstyle backcolor="#E9E7E2" />
                                <sortedascendingheaderstyle backcolor="#506C8C" />
                                <sorteddescendingcellstyle backcolor="#FFFDF8" />
                                <sorteddescendingheaderstyle backcolor="#6F8DAE" />
                            </asp:GridView>
                        </asp:Panel>
                        <asp:Button 
                            CssClass="myButton" ID="btnExport" runat="server" 
                            Text="Preview Orders... (Revised)" onclick="btnExport_Click" Width="48%" Visible="True" />
                        <asp:Button 
                            CssClass="myButton" ID="btnGeraSQL" runat="server" 
                            Text="Generate SQL Script (Insert DB)" onclick="btnGeraSQL_Click" Width="48%" Visible="True" />
                    </asp:Panel>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <br />
    <asp:Panel ID="panAviso" runat="server" Height="75px" ScrollBars="None" Width="100%" Visible="True">
        <div 
            ID="divMessage" runat="server" >
        </div>
    </asp:Panel>
    <br />

</asp:Content>
