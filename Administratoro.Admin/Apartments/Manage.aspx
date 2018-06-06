<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="Admin.Tenants.Manage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
    <link href="/Content/dataTables.bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="/Content/responsive.bootstrap.min.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="preLoadIcon"></div>
    <div class="col-md-12 col-sm-12 col-xs-12">
        <asp:Label ID="lblMessage" runat="server"></asp:Label>
        <span id="apartmentsManageAddNew" class="btn">
            <i class="fa fa-plus"></i><a href="~/Apartments/Add.aspx" runat="server">Adaugă apartament nou</a>
        </span>
        <br />
        <br />
        <div class="x_panel">
            <div class="x_title">
                <h2>Apartamente</h2>

                <div class="clearfix"></div>
            </div>
            <div class="x_content">
                <asp:Table ID="datatableResponsive" runat="server" class="table table-striped table-bordered dt-responsive nowrap" CellSpacing="0" Width="100%" ClientIDMode="Static">
                    <asp:TableHeaderRow TableSection="TableHeader">
                        <asp:TableHeaderCell>#</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Număr</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Nume</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Telefon</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Email</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Pers. în întreținere</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Cota Indiviză</asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                </asp:Table>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
    <script src='<%= ResolveUrl("~/Scripts/Datatables/jquery.dataTables.min.js")%>'></script>
    <script src='<%= ResolveUrl("~/Scripts/Datatables/dataTables.bootstrap.min.js")%>'></script>
    <script src='<%= ResolveUrl("~/Scripts/Datatables/dataTables.responsive.min.js")%>'></script>
    <script src='<%= ResolveUrl("~/Scripts/Datatables/responsive.bootstrap.js")%>'></script>

    <script>
        $(window).load(function () {
            // Animate loader off screen
            $(".preLoadIcon").fadeOut("slow");
        });

        $('#datatableResponsive').DataTable();
    </script>

</asp:Content>
