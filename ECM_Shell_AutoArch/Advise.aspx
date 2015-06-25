<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Advise.aspx.cs" Inherits="ECM_Shell_AutoArch.Advise" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <br />
    <h2>
        <asp:Image ID="imgMessage" runat="server" ImageUrl="~/Images/cloud_alert.png" />
        &nbsp;
        <strong><asp:Label ID="lblMessage" runat="server"></asp:Label>
        </strong>
    </h2>
    <p>
    </p>
    <br />
</asp:Content>