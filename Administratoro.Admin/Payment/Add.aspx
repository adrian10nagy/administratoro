<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Add.aspx.cs" Inherits="Admin.Payment.Add" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="form-group">
            <label>Dată chitanță</label>
            <asp:TextBox runat="server" ID="txtAddedDate" CssClass="form-control datepicker" AutoCompleteType="Disabled"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Număr chitanță</label>
            <asp:TextBox runat="server" ID="tbNr" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Apartamentul</label>
            <asp:DropDownList runat="server" ID="drpApartaments" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="drpApartament_SelectedIndexChanged">
            </asp:DropDownList>
        </div>

        <asp:UpdatePanel runat="server" ID="upMain" UpdateMode="Conditional" Visible="False">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="chbWhatToPay" EventName="SelectedIndexChanged" />
            </Triggers>
            <ContentTemplate>
                <div class="form-group " runat="server" id="pnlWhatToPay" visible="false">
                    <label>Cheltuielile de plată</label>
                    <asp:CheckBoxList runat="server" ID="chbWhatToPay" CssClass="form-control pnlWhatToPay" OnSelectedIndexChanged="chbWhatToPay_OnSelectedIndexChanged" AutoPostBack="True">
                    </asp:CheckBoxList>
                </div>
                Total selectat<asp:TextBox runat="server" ID="tbSumOfChecked" CssClass="form-control" Enabled="False" Text="0"></asp:TextBox>
                Total pe chitanță<asp:TextBox runat="server" ID="tbTotalToPay" CssClass="form-control"></asp:TextBox>
                Explicații<asp:TextBox runat="server" ID="tbExplanations" CssClass="form-control"></asp:TextBox>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:Label runat="server" ID="lblValidationMessage"></asp:Label>
        <asp:Panel runat="server" ID="pnlSendEmail" Visible="False">
            <asp:CheckBox runat="server" ID="chbsendEmail" />
            Trimite chitanța pe email
        </asp:Panel>
        <asp:DropDownList runat="server" ID="drpWhatTpPayPartially" CssClass="form-control" Visible="False">
        </asp:DropDownList>

        <div class="form-group" runat="server">
            <asp:Button runat="server" ID="btnCancel" Text="Anulează" OnClick="btnCancel_Click" CausesValidation="false" />
            <asp:Button runat="server" ID="btnConfirm" Text="Confirmă și Plătește" OnClick="bnConfirm_OnClick" Visible="false" />
            <asp:Button runat="server" ID="btnSave" Text="Plătește" OnClick="btnSave_Click" Enabled="false" />
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
