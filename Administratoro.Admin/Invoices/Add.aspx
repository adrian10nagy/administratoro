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
        <div class="form-group" runat="server" id="pnlInvoiceBody">
            <div class="col-md-3 col-sm-6 col-xs-12 form-group has-feedback" runat="server" id="divInvoiceMainValue">
                <label>Valoare</label>
                <asp:TextBox runat="server" ID="txtInvoiceValue" AutoCompleteType="Disabled"></asp:TextBox>
            </div>
            <div class="col-md-3 col-sm-6 col-xs-12 form-group has-feedback">
                <label>Denumire factură</label>
                <asp:TextBox runat="server" ID="txtInvoiceDescription" AutoCompleteType="Disabled"></asp:TextBox>
            </div>

            <div class="col-md-3 col-sm-6 col-xs-12 form-group has-feedback">
                <label>Data emiterii</label>
                <asp:TextBox runat="server" ID="txtInvoiceDate" CssClass="datepicker" AutoCompleteType="Disabled"></asp:TextBox>
            </div>
            <div class="col-md-3 col-sm-6 col-xs-12 form-group has-feedback">
                <label>Număr</label>
                <asp:TextBox runat="server" ID="txtInvoiceNumber" AutoCompleteType="Disabled"></asp:TextBox>
            </div>
        </div>
        <asp:Panel runat="server" ID="pnInvoiceSubcategories" EnableViewState="false" Visible="false">
        </asp:Panel>
        <asp:Panel runat="server" ID="pnInvoiceValues" EnableViewState="false">
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlInvoiceDiverseValues" EnableViewState="false">
        </asp:Panel>
        <div class="form-group" runat="server">
            <asp:Button runat="server" ID="btnCancel" Text="Anulează" OnClick="btnCancel_Click" CausesValidation="false" />
            <asp:Button runat="server" ID="btnSave" Text="Salvează" OnClick="btnSave_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
