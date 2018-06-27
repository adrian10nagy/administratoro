<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AddEditIndividual.aspx.cs" Inherits="Admin.Expenses.AddEditHeatHelp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblExpenseMeessage" runat="server"></asp:Label><br />
    <asp:Label ID="lblExpenseMeessageInfo" runat="server"></asp:Label><br />
    <asp:Button runat="server" ID="btnRedirect" Text="Revino la Facturi" />
    <br />
    <br />

    <div id="divExpensesPerIndex" runat="server">
        <br />
        <asp:GridView ID="gvExpensesPerIndex" runat="server" AutoGenerateColumns="true" AutoGenerateEditButton="true"
            OnRowEditing="gvExpensesPerIndex_RowEditing" OnRowUpdating="gvExpensesPerIndex_RowUpdating"
            OnRowCancelingEdit="gvExpensesPerIndex_RowCancelingEdit" OnRowDataBound="gvExpensesPerIndex_RowDataBound"
            CssClass="table table-striped table-bordered dt-responsive nowrap">
        </asp:GridView>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
