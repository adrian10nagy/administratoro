<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApartmentRegistry.aspx.cs" Inherits="Admin.Reports.ApartmentRegistry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <h3>Registre de apartament</h3>
        <br />
        <div class="form-group">
            <asp:Label runat="server" ID="lblMessage"></asp:Label>
            <asp:Label runat="server" Text="Alege tipul de registru"></asp:Label>
        </div>
        <div class="form-group" runat="server">
            <asp:DropDownList runat="server" ID="drpRegType"></asp:DropDownList><br />

        </div>

        <div class="form-group" runat="server">
            <asp:Label runat="server" Text="Alege apartamentul"></asp:Label>
        </div>
        <div class="form-group" runat="server">
            <asp:DropDownList runat="server" ID="drpApartments"></asp:DropDownList><br />
        </div>

         <div class="form-group" runat="server">
            <asp:Button runat="server" ID="btnGenerate" Text="Arată registrul" OnClick="btnGenerate_Click" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
