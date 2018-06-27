<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Add.aspx.cs" Inherits="Admin.Payment.Add" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="form-group">
            <label>Apartamentul</label>
            <asp:DropDownList runat="server" ID="drpApartaments" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="drpApartament_SelectedIndexChanged">
            </asp:DropDownList>
        </div>
         <div class="form-group">
            <label>Cheltuielile de plată</label>
            <asp:CheckBox runat="server" ID="chbWhatToPay" CssClass="form-control">
            </asp:CheckBox>
        </div>
      
         <div class="form-group" runat="server">
            <asp:Button runat="server" ID="btnCancel" Text="Anulează" OnClick="btnCancel_Click" CausesValidation="false" />
            <asp:Button runat="server" ID="btnSave" Text="Plătește" OnClick="btnSave_Click" Enabled="false"/>
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
