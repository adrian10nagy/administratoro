<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Admin._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <div id="dashboard" class="col-md-10 col-sm-12 col-xs-12 col-md-offset-1">
            <div runat="server" id="dashboardMessage" class="col-md-12 col-xs-12">
            </div>
            <div class="col-md-4 col-sm-4 col-xs-6">
                <div id="dashboardItemMain">
                    <h2>Notițe, atenționări</h2>
                    <br />
                    <asp:Label runat="server" ID="lblNotesWarnings"></asp:Label>
                </div>
            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Associations/Index.aspx" runat="server" class="dashboardItemA"><i class="fa fa-cog  dashboardItemI"></i>
                        <br />
                        Setări Asociație
                    </a>
                </div>
            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Apartments/Manage.aspx" runat="server" class="dashboardItemA"><i class="fa fa-home dashboardItemI"></i>
                        <br />
                        Apartamente
                    </a>
                </div>
            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Expenses/Dashboard.aspx" runat="server" class="dashboardItemA"><i class="fa fa-file-o dashboardItemI"></i>
                        <br />
                        Facturi lunare</a>
                </div>
            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Reports/Index.aspx" runat="server" class="dashboardItemA"><i class="fa fa-table dashboardItemI"></i>
                        <br />
                        Liste de plată</a>
                </div>
            </div>

            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Invoices/Add.aspx" runat="server" class="dashboardItemA"><i class="fa fa-plus dashboardItemI"></i>
                        <br />
                        Adaugă factură</a>
                </div>

            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                    <a href="~/Payment/Add.aspx" runat="server" class="dashboardItemA"><i class="fa fa-plus dashboardItemI"></i>
                        <br />
                        Plătește cheltuială</a>

                </div>

            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem">
                </div>
            </div>
            <div class="col-md-2 col-sm-2 col-xs-2">
                <div class="dashboardItem"></div>

            </div>
        </div>
    </div>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>


