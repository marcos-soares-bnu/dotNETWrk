<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="ECM_Shell_AutoArch.Reports" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <h2>
        Reports - Customer Validation Report
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
				            Text="List of Archiving Orders (In Progress)" Font-Bold="True">
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
			                <div id='div1' style="Z-INDEX: 101; OVERFLOW: auto; WIDTH: 95%; HEIGHT: 170px" >
			                    <asp:ListBox 
				                ID="lstReqPlan" runat="server" Height="100%" Width="150%"
				                AutoPostBack="True" OnSelectedIndexChanged="lstReqPlan_SelectedIndexChanged" EnableViewState="True">
			                    </asp:ListBox>
                            </div>
			                <br />
			                <asp:Label 
				            ID="Label4" runat="server"
				            Text="List of CSV Reports (Server)" Font-Bold="True">
			                </asp:Label>
			                <div id='div9' style="Z-INDEX: 101; OVERFLOW: auto; WIDTH: 95%; HEIGHT: 90px" >
			                    <asp:ListBox 
				                ID="lstCsvRpt" runat="server" Height="100%" Width="150%"
				                AutoPostBack="True" OnSelectedIndexChanged="lstCsvRpt_SelectedIndexChanged" EnableViewState="True">
			                    </asp:ListBox>
                            </div>
                            <asp:FileUpload
                                id="FileUpload1" runat="server" Width="94%" Visible="true">
                            </asp:FileUpload>
                            <asp:Button 
                                CssClass="myButton" ID="btn_Upload" runat="server" 
                                Text="Upload CSV Reports..." onclick="btn_Upload_Click" Width="94%" Visible="True" />
			                <br />
                            <br />
			                <asp:Label 
				            ID="Label3" runat="server" 
				            Text="List of CVR's created (Server)" Font-Bold="True">
			                </asp:Label>
			                <br />
			                <div id='div2' style="Z-INDEX: 102; OVERFLOW: auto; WIDTH: 95%; HEIGHT: 123px" >
			                    <asp:ListBox 
				                ID="lstCVRs" runat="server" Height="100%" Width="150%"
				                AutoPostBack="True" EnableViewState="True">
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
                            CssClass="myButton" ID="btnGeraCVR" runat="server" 
                            Text="Generate CVR Document" onclick="btnGeraCVR_Click" Width="96%" Visible="True" />
                        <asp:Button 
                            CssClass="myButton" ID="btnDownload" runat="server" Enabled="False" 
                            Text="Download CVR Document" onclick="btnDownload_Click" Width="48%" Visible="False" />
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
