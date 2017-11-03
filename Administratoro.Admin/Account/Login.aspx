<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Admin.Account.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="/Content/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="/Content/font-awesome.min.css" rel="stylesheet" type="text/css" />
    <link href="/Content/nprogress.css" rel="stylesheet" type="text/css" />
    <link href="/Content/animate.min.css" rel="stylesheet" type="text/css" />
    <link href="/Content/custom.min.css" rel="stylesheet" type="text/css" />

    <title>Biblioteca- Logare</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body class="login">
    <form runat="server">
        <div>
            <a class="hiddenanchor" id="signup"></a>
            <a class="hiddenanchor" id="signin"></a>

            <div class="login_wrapper">
                <div class="animate form login_form">
                    <section class="login_content">
                        <div class="formDiv">
                            <h1>Intră în cont</h1>
                            <asp:Label ID="lblMessageLogin" runat="server" Visible="false"></asp:Label>
                            <div>
                                <input type="text" class="form-control has-feedback-left" id="txtUserName" placeholder="Utilizator" runat="server" autocomplete="off" />
                            </div>
                            <div>
                                <input type="password" class="form-control has-feedback-left" id="txtUserPassword" placeholder="Parolă" runat="server" />
                            </div>
                            <div>
                                <asp:Button ID="btnAccountLogin" runat="server" Text="Intră în cont" CssClass="btn btn-default submit" OnClick="btnAccountLogin_Click" />
                                <%--<a class="reset_pass" href="#">Am uitat parola</a>--%>
                            </div>
                            <div class="clearfix"></div>
                            <div class="separator">
                                <%--  <p class="change_link">
                                    Nou pe site?
                               
                                <a href="#signup" class="to_register">Crează-ți cont </a>
                                </p>--%>
                                <div class="clearfix"></div>
                                <br />
                                <div>
                                    <h1><i class="fa fa-paw"></i>BiblioSofia</h1>
                                    <p>©<%= DateTime.Now.Year.ToString() %>. Toate drepturile rezervate.</p>
                                </div>
                            </div>
                        </div>
                    </section>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
