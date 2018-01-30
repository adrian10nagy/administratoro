<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Monthly.aspx.cs" Inherits="Admin.Expenses.CurrentMonth" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
    <webopt:BundleReference runat="server" Path="~/Content/Expenses" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <asp:Label ID="lblExpenseMeessage" runat="server"></asp:Label>
    <asp:LinkButton ID="lblExpenseMeessageDownload" CssClass="expenseMeessageConfigure" OnClick="lblExpenseMeessageDownload_Click" runat="server">Salvează raportul(<i class="fa fa-book"></i>)</asp:LinkButton>
    <asp:DropDownList ID="drpDisplayMode" runat="server" CssClass="drpDisplayMode" AutoPostBack="true" OnSelectedIndexChanged="drpDisplayMode_SelectedIndexChanged"></asp:DropDownList>
    <br /><br />
    <small>* Sumele sunt calculate la momentul adăugării facturilor, modificările/adăugările ulterioare de  apartamente/persoane în întreținere etc nu vor fi considerate</small>

    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true" CssClass="table table-striped table-bordered dt-responsive nowrap">
    </asp:GridView>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
