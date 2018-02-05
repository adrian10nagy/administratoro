<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Invoices.aspx.cs" Inherits="Admin.Expenses.Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
    <webopt:BundleReference runat="server" Path="~/Content/Expenses" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h3 runat="server" id="mainHeader">Facturi</h3>
    <asp:LinkButton ID="lblExpenseMeessageConfigure" CssClass="expenseMeessageConfigure" OnClick="lblExpenseMeessageConfigure_Click" runat="server">Configurează(<i class="fa fa-wrench"></i>)</asp:LinkButton>
    <asp:LinkButton ID="lblExpenseMonthlyList" CssClass="expenseMeessageConfigure" OnClick="lblExpenseMonthlyList_Click" runat="server">Vezi lista de plată(<i class="fa fa-table"></i>)</asp:LinkButton>
    <br />
    <br />

    <asp:Panel ID="invoiceRedistribute" runat="server" Visible="false">
        <asp:Button runat="server" Text="Anulează" ID="invoiceRedistributeCancel" OnClick="invoiceRedistributeCancel_Click" />
        <asp:Label runat="server" ID="invoiceRedistributeMessage">
        </asp:Label>
        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Egal per apartament" ID="invoiceRedistributeEqualApartment" OnClick="invoiceRedistributeEqualApartment_Click" />
            <asp:Label runat="server" ID="txtInvoiceRedistributeEqualApartment"></asp:Label>
        </asp:Panel>

        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Per  număr de locatari" ID="invoiceRedistributeEqualTenants" OnClick="invoiceRedistributeEqualTenants_Click" />
            <asp:Label runat="server" ID="txtInvoiceRedistributeEqualTenants"></asp:Label>
        </asp:Panel>

        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Proporțional cu consumul" ID="invoiceRedistributeConsumption" OnClick="invoiceRedistributeConsumption_Click" />
        </asp:Panel>

    </asp:Panel>

    <asp:Panel ID="invoiceMain" runat="server">
    </asp:Panel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
