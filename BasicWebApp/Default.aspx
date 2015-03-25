<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BasicWebApp._Default" ValidateRequest="false" EnableEventValidation="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h2>Enter Website url!</h2>
        <div class="textbox">
            <asp:TextBox ID="TextBox1" runat="server" Width="100%" TextMode="SingleLine" BorderStyle="Solid" EnableTheming="True"></asp:TextBox>
        </div>
        <div class="button">
            <asp:Button ID="Button1" runat="server" Text="Get Source" OnClick="HandleUrlEntry" /><br />
        </div>
        <asp:Label ID="ErrorLabel" runat="server" Text="Error" Visible="false"></asp:Label>
    </div>
    <div class="row">
        <div class="col-md-3">
            <asp:Label ID="TagsLable" runat="server" Text="Summary of Tags" Visible="false" CssClass="headingAreaTags"></asp:Label>
            <asp:GridView ID="GridView1" runat="server" Visible="false" AutoGenerateColumns="false" OnSelectedIndexChanged="GVSelectionChanged" OnRowDataBound="GV_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="Key" HeaderText="Tag Name" InsertVisible="False" ReadOnly="True" />
                    <asp:BoundField DataField="Value" HeaderText="Occurance count" />
                </Columns>
                <SelectedRowStyle BackColor="LightCyan"
                    ForeColor="DarkBlue"
                    Font-Bold="true" />
            </asp:GridView>
        </div>
        <div class="textarea">
            <asp:Label ID="SourceLable" runat="server" Text="Source" Visible="false" CssClass="headingAreaSource"></asp:Label>
            <textarea id="TextArea1" runat="server" rows="60" visible="false" name="S1" style="width: 880px;" readonly></textarea>
        </div>
    </div>
    <script type="text/javascript" src="Content/components/jquery/jquery.min.js"></script>
    <link type="text/css" rel="stylesheet" href="Content/components/jquery-highlighttextarea/jquery.highlighttextarea.css" />
    <script type="text/javascript" src="Content/components/jquery-highlighttextarea/jquery.highlighttextarea.js"></script>
    <script type="text/javascript">
        function highlightText(text) {
            if (!text) {
                return;
            }
            $("textarea").highlightTextarea({
                words: text
            });
        }
    </script>
</asp:Content>
