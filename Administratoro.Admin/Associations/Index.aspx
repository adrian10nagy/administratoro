<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Admin.Associations.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1 runat="server" id="estateName" />

    <div>
        <span>Bloc împărțit pe scări:</span>
        <asp:RadioButtonList ID="estateStairs" runat="server" AutoPostBack="true" OnSelectedIndexChanged="estateStairs_SelectedIndexChanged"
            RepeatLayout="Flow" RepeatDirection="Horizontal">
            <asp:ListItem Text="Nu" Value="0" />
            <asp:ListItem Text="Da" Value="1" />
        </asp:RadioButtonList>
        <asp:Panel runat="server" ID="estateStairsAdded" Visible="false">
        </asp:Panel>
         <asp:TextBox runat="server" ID="txtEstateStairsNew" Visible="false"></asp:TextBox>
         <asp:Button runat="server" ID="btnEstateStairsNew" Text="Adaugă scară nouă" OnClick="btnEstateStairsNew_Click"  Visible="false" />
    </div>

    <h3>Indiviză</h3>
    <asp:TextBox runat="server" ID="estateIndiviza" Enabled="false" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
