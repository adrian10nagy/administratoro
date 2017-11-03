<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AddEditExpense.aspx.cs" Inherits="Admin.Expenses.AddEditExpense" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblExpenseMeessage" runat="server"></asp:Label><br />
    <asp:Label ID="lblExpenseMeessageInfo" runat="server"></asp:Label><br />
    <asp:Button runat="server" ID="btnRedirect" Text="Revino la Registru de casă" />
    <br />
    <br />

    <asp:GridView ID="gvExpenses" runat="server" AutoGenerateColumns="true" AutoGenerateEditButton="true"
        OnRowEditing="gvExpenses_RowEditing" OnRowUpdating="gvExpenses_RowUpdating" Visible="false"
        OnRowCancelingEdit="gvExpenses_RowCancelingEdit" OnRowDataBound="gvExpenses_RowDataBound"
        DataKeyNames="Id, Apartament" CssClass="table table-striped table-bordered dt-responsive nowrap">
    </asp:GridView>
    <div runat="server" id="txtExpensePerCotaIndiviza" visible="false">
        Cheltuiala totală (valoare factură):<asp:TextBox ID="txtExpensePerCotaIndivizaAll" runat="server"></asp:TextBox><br />
        <asp:Button ID="btnExpensePerProperyAll" runat="server" Text="Salvează cheltuiala" OnClick="btnExpensePerCotaIndivizaAll_Click" />
    </div>
    <div runat="server" id="txtExpensePerTenants" visible="false">
        Cheltuiala totală (valoare factură):
        <asp:TextBox ID="txtExpensePerTenantAll" runat="server"></asp:TextBox><br />
        <asp:Button ID="btnExpensePertenantAll" runat="server" Text="Calculează suma per locatar" OnClick="btnExpensePertenantAll_Click" />
        <hr />
        Suma per locatar:
        <asp:TextBox ID="txtExpensePerTenantEach" runat="server"></asp:TextBox><br />
        <asp:Label ID="txtExpensePerTenantEachInfo" runat="server"></asp:Label><br />
        <asp:Button ID="btnExpensePerTenantEach" runat="server" Text="Salvează cheltuiala" OnClick="btnExpensePerTenantEach_Click" />
    </div>
    <div id="divExpensesPerIndex" runat="server" visible="false">
         Costul per unitate:
        <asp:TextBox ID="txtExpensesPerIndexValue" runat="server" Enabled="false"></asp:TextBox>
        <asp:Button ID="btnExpensesPerIndexValue" runat="server" Text="Modifică" OnClick="btnExpensesPerIndexValue_Click" /><br /><br />
        <asp:GridView ID="gvExpensesPerIndex" runat="server" AutoGenerateColumns="true" AutoGenerateEditButton="true"
            OnRowEditing="gvExpensesPerIndex_RowEditing" OnRowUpdating="gvExpensesPerIndex_RowUpdating"
            OnRowCancelingEdit="gvExpensesPerIndex_RowCancelingEdit" OnRowDataBound="gvExpensesPerIndex_RowDataBound"
            DataKeyNames="Id, Apartament, Index vechi, Valoare" CssClass="table table-striped table-bordered dt-responsive nowrap">
        </asp:GridView>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
