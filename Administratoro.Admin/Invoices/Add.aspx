<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Add.aspx.cs" Inherits="Admin.Invoices.Add" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="form-group">
            <label>Luna</label>
            <asp:DropDownList runat="server" ID="drpInvoiceYearMonth" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="drpInvoiceYearMonth_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Cheltuială</label>
            <asp:DropDownList runat="server" ID="drpInvoiceExpenses" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="drpInvoiceExpenses_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Valoare</label>
            <asp:Panel runat="server" ID="pnInvoiceValues">
            </asp:Panel>          
        </div>
        <div class="form-group" runat="server" id="expensesOnIndex">
            <label>Index nou</label>
            <asp:TextBox runat="server" ID="txtInvoiceIndex" Visible="false" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group" runat="server">
            <asp:Button runat="server" ID="btnCancel" Text="Anulează" OnClick="btnCancel_Click" CausesValidation="false"/>
            <asp:Button runat="server" ID="btnSave" Text="Salvează" OnClick="btnSave_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
