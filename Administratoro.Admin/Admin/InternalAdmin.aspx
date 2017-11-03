<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InternalAdmin.aspx.cs" Inherits="Admin.Reports.InternalAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%-- Grad acoperire --%>
    <div class="col-md-4 col-sm-4 col-xs-12">
        <div class="x_panel tile fixed_height_320">
            <div class="x_title">
                <h2>Gradul de acoperire al bibliotecilor</h2>
                <div class="clearfix"></div>
            </div>
            <div class="x_content">
                <div class="dashboard-widget-content">
                    <ul class="quick-list">
                        <li>
                            <i class="fa fa-calendar-o"></i> 
                            <asp:LinkButton ID="btnLibraryAdd" Text="Adaugă" runat="server" OnClick="btnLibraryAdd_Click"></asp:LinkButton>
                        </li>
                        <li>
                        </li>
                        <li><i class="fa fa-bar-chart"></i><a href="#">Auto Renewal</a> </li>
                        <li><i class="fa fa-line-chart"></i><a href="#">Achievements</a>
                        </li>
                        <li><i class="fa fa-bar-chart"></i><a href="#">Auto Renewal</a> </li>
                        <li><i class="fa fa-line-chart"></i><a href="#">Achievements</a>
                        </li>
                        <li><i class="fa fa-area-chart"></i><a href="#">Logout</a>
                        </li>
                    </ul>
                    <div class="sidebar-widget">
                        <h4>Grad acoperire</h4>
                        <canvas width="150" height="80" id="canvas-cuverage-id" class="" style="width: 160px; height: 100px;"></canvas>
                        <div class="goal-wrapper">
                            <span class="gauge-value pull-left">$</span>
                            <span id="gauge-current" class="gauge-value pull-left">0</span>
                            <span id="goal-text" class="goal-value pull-right">319</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <asp:Panel class="col-md-4 col-sm-4 col-xs-12" runat="server" visible="false" id="pnlAddPartner">
        <div class="x_panel tile fixed_height_320">
            <div class="x_title">
                <h2>Adaugă o bibliotecă nouă</h2>
                <div class="clearfix"></div>
            </div>
            <div class="x_content">
                <asp:TextBox ID="txtPartenerAddName" runat="server"></asp:TextBox>
                <asp:Button ID="btnPartnerAddSubmit" runat="server" OnClick="btnPartnerAddSubmit_Click" Text="Adaugă" CssClass="" CausesValidation="false"></asp:Button>
            </div>
            </div>

    </asp:Panel>

    <%-- Grad acoperire --%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">

    <script>
        var opts = {
            lines: 12,
            angle: 0,
            lineWidth: 0.4,
            pointer: {
                length: 0.75,
                strokeWidth: 0.042,
                color: '#1D212A'
            },
            limitMax: 'false',
            colorStart: '#1ABC9C',
            colorStop: '#1ABC9C',
            strokeColor: '#F0F3F3',
            generateGradient: true
        };
        var target = document.getElementById('canvas-cuverage-id'),
           gauge = new Gauge(target).setOptions(opts);

        gauge.maxValue = 319;
        gauge.animationSpeed = 32;
        gauge.set(100);
        gauge.setTextField(document.getElementById("gauge-current"));

    </script>
</asp:Content>
