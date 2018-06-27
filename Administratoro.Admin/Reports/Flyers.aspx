<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Flyers.aspx.cs" Inherits="Admin.Reports.Flyers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel runat="server">
        <h3>Fluturași</h3><br />
        <asp:Label runat="server" ID="lblMessage"></asp:Label>
        <asp:Label runat="server" Text="Alege luna"></asp:Label>
        <asp:DropDownList runat="server" ID="drpAvailableMonths"></asp:DropDownList>
        <asp:Button runat="server" ID="btnPreview" OnClick="btnPreview_Click" Text="Vizualizare"/>
        <asp:Button runat="server" ID="btnDownload" OnClick="btnDownload_Click" Text="Descarcă"/>
        <asp:Button runat="server" ID="btnEmail" OnClick="btnEmail_Click" Text="Trimite email"/>
        <asp:Button runat="server" ID="btnEmail_Confirm" OnClick="btnEmail_Confirm_Click" Text="Confirm Trimitere email" Visible="false"/>
    </asp:Panel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
