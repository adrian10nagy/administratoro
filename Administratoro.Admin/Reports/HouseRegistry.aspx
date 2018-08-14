<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HouseRegistry.aspx.cs" Inherits="Admin.Reports.HouseRegistry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="form-group">
            <label>Dată chitanță</label>
            <asp:TextBox runat="server" ID="txtDate" CssClass="form-control datepicker" AutoCompleteType="Disabled"></asp:TextBox>
        </div>
        <div class="form-group" runat="server">

            <asp:Button runat="server" ID="btnGenerate" Text="Arată jurnalul de casă" OnClick="btnGenerate_OnClick" />
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
