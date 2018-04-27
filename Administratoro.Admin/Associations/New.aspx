<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="New.aspx.cs" Inherits="Admin.Associations.New" EnableEventValidation="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="x_panel">
        <asp:Panel ID="step1" runat="server">
            <div class="x_title">
                <asp:Label ID="lblStatus" runat="server"></asp:Label>
                <asp:Label ID="lblUserId" Visible="false" runat="server"></asp:Label>
                <h2>Adaugă asociație </h2>
                <small>Pasul 1 din 3</small>
                <div class="clearfix"></div>
            </div>
            <div class="x_content form-horizontal form-label-left">
                <br />
                <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                    <label class="control-label col-md-3 col-sm-3 col-xs-12">Denumire asociație</label>
                    <div class="col-md-9 col-sm-9 col-xs-12">
                        <input type="text" class="form-control has-feedback-left" id="associationName" runat="server" autocomplete="off">
                        <span class="fa fa-list-alt form-control-feedback left" aria-hidden="true"></span>
                        <asp:RequiredFieldValidator ControlToValidate="associationName" runat="server" ErrorMessage="Numele proprietății este obligatoriu" CssClass="requiredField"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                    <label class="control-label col-md-3 col-sm-3 col-xs-12">Adresă</label>
                    <div class="col-md-9 col-sm-9 col-xs-12">
                        <input type="text" class="form-control has-feedback-left" id="associationAddress" runat="server" autocomplete="off" />
                        <span class="fa fa-user form-control-feedback left" aria-hidden="true"></span>
                        <asp:RequiredFieldValidator ControlToValidate="associationAddress" runat="server" ErrorMessage="Adresa proprietatății este obligatorie" CssClass="requiredField"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                    <label class="control-label col-md-3 col-sm-3 col-xs-12">Cod Fiscal</label>
                    <div class="col-md-9 col-sm-9 col-xs-12">
                        <input type="text" class="form-control has-feedback-left" id="associationFiscalCode" runat="server" autocomplete="off">
                        <span class="fa fa-group form-control-feedback left" aria-hidden="true"></span>
                    </div>
                </div>

                <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                    <br />
                    <br />
                </div>

                <div class="col-md-6 col-sm-6 col-xs-12 form-group has-feedback">
                    <div>
                        <asp:RadioButtonList ID="associationEqualIndiviza" CssClass="AssociationsNewRadioBtns" runat="server" AutoPostBack="true" RepeatLayout="Flow" OnSelectedIndexChanged="associationEqualIndiviza_SelectedIndexChanged" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Nu" Value="0" Selected="True" />
                            <asp:ListItem Text="Da" Value="1" />
                        </asp:RadioButtonList>
                        <span>Cotă de indiviză egală la toate apatamentele</span>
                        <asp:TextBox runat="server" ID="associationCotaIndivizaApartments" Visible="false" placeholder="ex: 1,16"></asp:TextBox> %
                    </div>
                    <div>
                        <asp:UpdatePanel runat="server" ID="UpdatePanel1">
                            <ContentTemplate>
                                <asp:RadioButtonList ID="associationStairs" runat="server" AutoPostBack="true" CssClass="AssociationsNewRadioBtns"
                                    RepeatLayout="Flow" OnSelectedIndexChanged="associationStairs_SelectedIndexChanged" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="Nu" Value="0" Selected="True" />
                                    <asp:ListItem Text="Da" Value="1" />
                                </asp:RadioButtonList>
                                <span>Bloc împărțit pe scări</span>
                                <asp:Panel runat="server" ID="associationStairsStairsAdded" CssClass="associationsStairCases" Visible="false">
                                </asp:Panel>
                                <asp:PlaceHolder ID="PlaceholderControls" runat="server"></asp:PlaceHolder>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="associationStairs" EventName="SelectedIndexChanged" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </div>

                <div class="form-group">
                    <div class="col-md-9 col-sm-9 col-xs-12 col-md-offset-3">
                        <asp:Button ID="btnCancel" runat="server" Text="Anulează" class="btn btn-primary" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:Button ID="btnSave" runat="server" Text="Salvează" class="btn btn-success" OnClick="btnSave_Click" CausesValidation="true" />
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="step2" runat="server" Visible="false">
            <h2>Configurează cheltuielile de afișat </h2>
            <small>Pasul 2 din 3</small>
            <asp:Panel runat="server" ID="expensesDefault" CssClass="row">
            </asp:Panel>
            <div class="form-group">
                <div class="col-md-9 col-sm-9 col-xs-12 col-md-offset-3">
                    <asp:Button ID="btnCancel2" runat="server" Text="Configurează mai tărziu" class="btn btn-primary" OnClick="btnCancel2_Click" CausesValidation="false" />
                    <asp:Button ID="btnSave2" runat="server" Text="Salvează pasul 2/3" class="btn btn-success" OnClick="btnSave2_Click" CausesValidation="true" />
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="step3" runat="server" Visible="false">
            <h2>Adaugă și configurează <u>contoarele</u> de cheltuieli </h2>
            <small>Pasul 3 din 3</small>
            <asp:Panel runat="server" ID="countersConfiguration" CssClass="row">
            </asp:Panel>
            <asp:Button runat="server" ID="btnCounterAddNew" OnClick="btnCounterAddNew_Click" Text="Adaugă contor nou" />
            <div class="form-group">
                <div class="col-md-9 col-sm-9 col-xs-12 col-md-offset-3">
                    <asp:Button ID="btnCancel3" runat="server" Text="Configurează mai tărziu" class="btn btn-primary" OnClick="btnCancel3_Click" CausesValidation="false" />
                    <asp:Button ID="btnSave3" runat="server" Text="Salvează pasul 3/3" class="btn btn-success" OnClick="btnSave3_Click" CausesValidation="true" />
                </div>
            </div>
        </asp:Panel>
    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
