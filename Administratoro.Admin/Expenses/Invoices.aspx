<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Invoices.aspx.cs" Inherits="Admin.Expenses.CashBook" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
    <webopt:BundleReference runat="server" Path="~/Content/Expenses" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h3 runat="server" id="mainHeader">Facturi</h3>
    <asp:LinkButton ID="lblExpenseMeessageConfigure" CssClass="expenseMeessageConfigure" OnClick="lblExpenseMeessageConfigure_Click" runat="server">Configurează(<i class="fa fa-wrench"></i>)</asp:LinkButton>
    <br />
    <br />

    <asp:Panel ID="cashBookRedistribute" runat="server" Visible="false">
        <asp:Button runat="server" Text="Anulează" ID="cashBookRedistributeCancel" OnClick="cashBookRedistributeCancel_Click" />
        <asp:Label runat="server" ID="cashBookRedistributeMessage">
        </asp:Label>
        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Egal per apartament" ID="cashBookRedistributeEqualApartment" OnClick="cashBookRedistributeEqualApartment_Click" />
            <asp:Label runat="server" ID="txtCashBookRedistributeEqualApartment"></asp:Label>
        </asp:Panel>

        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Per  număr de locatari" ID="cashBookRedistributeEqualTenants" OnClick="cashBookRedistributeEqualTenants_Click" />
            <asp:Label runat="server" ID="txtCashBookRedistributeEqualTenants"></asp:Label>
        </asp:Panel>

        <asp:Panel runat="server">
            <asp:Button runat="server" Text="Proporțional cu consumul" ID="cashBookRedistributeConsumption" OnClick="cashBookRedistributeConsumption_Click" />
            <asp:Label runat="server" ID="txtCashBookRedistributeConsumption"></asp:Label>
        </asp:Panel>

    </asp:Panel>

    <asp:Panel ID="cashBookMain" runat="server">
    </asp:Panel>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
