<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NewMonthOpening.aspx.cs" Inherits="Admin.Config.NewMonthOpening" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label runat="server" ID="lblMessage"></asp:Label>
    Anul:
    <asp:DropDownList ID="drpOpeningYear" runat="server"></asp:DropDownList>
    Luna:
    <asp:DropDownList ID="drpOpeningMonth" runat="server"></asp:DropDownList>

    <asp:Table ID="tblMonthlyExpenses" runat="server" class="table table-striped table-bordered dt-responsive nowrap"
        CellSpacing="0" Width="100%" ClientIDMode="Static">
        <asp:TableHeaderRow TableSection="TableHeader">
            <asp:TableHeaderCell>#</asp:TableHeaderCell>
            <asp:TableHeaderCell>Cheltuielă</asp:TableHeaderCell>
            <asp:TableHeaderCell>Tip</asp:TableHeaderCell>
        </asp:TableHeaderRow>
    </asp:Table>
    <br />

    <asp:Button ID="btnOpening" runat="server" Text="Deschide noua lună" OnClick="btnOpening_Click" CssClass="btn btn-primary"/>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
