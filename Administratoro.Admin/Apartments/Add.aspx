<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Add.aspx.cs" Inherits="Admin.Tenants.Add" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="x_panel">
        <div class="x_title">
            <asp:Label ID="lblStatus" runat="server"></asp:Label>
            <asp:Label ID="lblUserId" Visible="false" runat="server"></asp:Label>
            <h2>Adaugă apartament <small>[poate fii modificat după salvare]</small></h2>

            <div class="clearfix"></div>
        </div>
        <div class="x_content form-horizontal form-label-left">
            <br />

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Număr apartament</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="number" class="form-control has-feedback-left" id="userNr" runat="server" autocomplete="off">
                    <span class="fa fa-list-alt form-control-feedback left" aria-hidden="true"></span>
                    <asp:RequiredFieldValidator ControlToValidate="userNr" runat="server" ErrorMessage="Numărul apartamentului este obligatoriu" CssClass="requiredField"></asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Nume</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="text" class="form-control has-feedback-left" id="userName" runat="server" autocomplete="off" />
                    <span class="fa fa-user form-control-feedback left" aria-hidden="true"></span>
                    <asp:RequiredFieldValidator ControlToValidate="userName" runat="server" ErrorMessage="Numele este obligatoriu" CssClass="requiredField"></asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Număr locatari</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="number" class="form-control has-feedback-left" id="userDependents" runat="server" autocomplete="off">
                    <span class="fa fa-group form-control-feedback left" aria-hidden="true"></span>
                    <asp:RequiredFieldValidator ControlToValidate="userDependents" runat="server" ErrorMessage="Numărul locatarilor este obligatoriu" CssClass="requiredField"></asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Cota indiviza de proprietate</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="text" class="form-control has-feedback-left" id="usercota" runat="server" autocomplete="off">
                    <span class="fa fa-percent form-control-feedback left" aria-hidden="true"></span>
                    <asp:RequiredFieldValidator ControlToValidate="usercota" runat="server" ErrorMessage="Cota indiviza de proprietate este obligatorie" CssClass="requiredField"></asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback" runat="server" id="divStaircase" visible="false">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Scară</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <asp:DropDownList ID="userStairCase" runat="server" class="form-control has-feedback-left"></asp:DropDownList>
                    <span class="fa fa-phone form-control-feedback left" aria-hidden="true"></span>
                </div>
            </div>

            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Email (opțional)</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="text" class="form-control has-feedback-left" id="userEmail" runat="server" autocomplete="off">
                    <span class="fa fa-envelope form-control-feedback left" aria-hidden="true"></span>
                </div>

            </div>
            <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                <label class="control-label col-md-3 col-sm-3 col-xs-12">Telefon (opțional)</label>
                <div class="col-md-9 col-sm-9 col-xs-12">
                    <input type="text" class="form-control  has-feedback-left" id="userPhone" runat="server" autocomplete="off">
                    <span class="fa fa-phone form-control-feedback left" aria-hidden="true"></span>
                </div>
            </div>

            <div class="item form-group col-md-12">
                <label class="control-label col-md-2 col-sm-2 col-xs-12">Extra informații (opțional)</label>
                <div class="col-md-6 col-sm-6 col-xs-12">
                    <textarea class="form-control" id="userExtraInfo" runat="server" autocomplete="off"></textarea>
                </div>
            </div>


            <div class="form-group">
                <div class="col-md-9 col-sm-9 col-xs-12 col-md-offset-3">
                    <asp:Button ID="btnSave" runat="server" Text="Salvează" class="btn btn-success" OnClick="btnSave_Click" CausesValidation="true" />
                    <asp:Button ID="btnCancel" runat="server" Text="Anulează" class="btn btn-primary" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </div>


</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
    <script>

        $(window).load(function () {
            autoHideStatusLabel('<%=lblStatus.ClientID%>', 10000)
        });

    </script>
</asp:Content>
