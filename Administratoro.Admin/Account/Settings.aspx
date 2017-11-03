<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="Admin.Account.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <div class="col-md-6 col-sm-6 col-xs-12">
            <div class="x_panel">
                <div class="x_title">
                    <asp:Label ID="lblStatus" runat="server"></asp:Label>
                    <asp:Label ID="lblUserId" Visible="false" runat="server"></asp:Label>
                    <h2>Schimbare parolă</h2>

                    <div class="clearfix"></div>
                </div>
                <div class="x_content form-horizontal form-label-left">
                    <br />
                        <asp:Label runat="server" ID="txtErrorMessage"></asp:Label>

                    <div class="item form-group">
                        <label runat="server" id="lblUserPasswordOld" class="control-label col-md-3 col-sm-3 col-xs-12">Parolă actuală</label>
                        <div class="col-md-9 col-sm-9 col-xs-12">
                            <input type="password" class="form-control" id="txtUserPasswordOld" runat="server" autocomplete="off">
                        </div>
                    </div>

                    <div class="item form-group">
                        <label runat="server" id="lblUserPasswordNew" class="control-label col-md-3 col-sm-3 col-xs-12">Parolă nouă</label>
                        <div class="col-md-9 col-sm-9 col-xs-12">
                            <input type="password" class="form-control" id="txtUserPasswordNew" runat="server" autocomplete="off">
                        </div>
                    </div>

                    <div class="item form-group">
                        <label runat="server" id="lblUserPasswordNewConfirm" class="control-label col-md-3 col-sm-3 col-xs-12">Confirmă parolă nouă</label>
                        <div class="col-md-9 col-sm-9 col-xs-12">
                            <input type="password" class="form-control" id="txtUserPasswordNewConfirm" runat="server" autocomplete="off">
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="col-md-9 col-sm-9 col-xs-12 col-md-offset-3">
                            <asp:Button ID="btnSave" runat="server" Text="Salvează" class="btn btn-success" OnClick="btnSave_Click" CausesValidation="true" />
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
