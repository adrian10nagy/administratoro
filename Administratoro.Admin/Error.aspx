<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Admin.Error" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-1">
        </div>
        <div class="col-md-3">
            <h1>:-(</h1>
            <h2>oops.</h2>
        </div>
        <div class="col-md-6">
            <h1>Ceva nu a mers bine</h1>
            <h3>am notat eroarea și vom încerca să o reparăm cât de repede putem</h3>
            <h4>Pentru a ne ajuta vă rugăm contactează-ne și descrie problema, mulțumim!</h4>
        </div>
        <div class="col-md-2">
        </div>
    </div>
    <br />
    <br />
    <asp:Label ID="lblMessage" runat="server"></asp:Label>
    <asp:Label ID="lblMoreInfo" runat="server"></asp:Label>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
