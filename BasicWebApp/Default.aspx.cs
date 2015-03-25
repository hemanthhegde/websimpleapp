using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BasicWebApp
{
    public partial class _Default : Page
    {
        private string _previousUri;

        // Load the page and add the button click handler.
        protected void Page_Load(object sender, EventArgs e)
        {
            _previousUri = null;
            Button1.Click += new EventHandler(this.HandleUrlEntry);
        }

        /// <summary>
        /// Handles the gridView Selection changed event and invokes the JQuery to highlight text in the text area.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GVSelectionChanged(Object sender, EventArgs e)
        {
            // Get the currently selected row using the SelectedRow property.
            GridViewRow row = this.GridView1.SelectedRow;

            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "highlightText", "highlightText([\"<" + row.Cells[0].Text + "\", \"" + row.Cells[0].Text + ">\"])", true);
            SetTextArea(this.TextArea1.InnerText);
        }

        /// <summary>
        /// Databinding of the row of gridView in order to attach the selection changed handler when the gridView row selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GV_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onmouseover"] = "this.style.cursor='hand';this.style.textDecoration='underline';";
                e.Row.Attributes["onmouseout"] = "this.style.textDecoration='none';";

                e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(this.GridView1, "Select$" + e.Row.RowIndex);
            }
        }

        /// <summary>
        /// This is the main handler which handles the onclick of button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleUrlEntry(Object sender, EventArgs e)
        {
            // When the button is clicked,
            var inputText = this.TextBox1.Text;

            // Handle double clicks.
            if (_previousUri == inputText)
                return;

            OnEnterButtonClick();
            _previousUri = inputText;
            if (string.IsNullOrEmpty(inputText))
            {
                // The strings have to be localized for production use.
                SetErrorText("Please Enter the URL in the text box.");
                return;
            }

            // Parse the uri and ensure that its properly formatted.
            Uri url = CreateUrl(inputText);

            // Sometimes the user might have entered thr url without the http:// or https://.
            // In that case System.Uri constructor throws an exception. So, append http:// to the passed in uri.
            // Huristic way of fixing user entry. Might not be correct 100% of the time.
            if (url == null && !(inputText.StartsWith("http://") || inputText.StartsWith("https://")))
            {
                inputText = "http://" + inputText;
                url = CreateUrl(inputText);
            }

            var html = GetHTMLFromUri(url);
            if (html == null)
            {
                // The strings have to be localized for production use.
                SetErrorText("The Url entered is invalid");
                return;
            }

            SetGridView(ExtractAllTags(html));
            SetTextArea(html);
        }

        /// <summary>
        /// Takes in a string and returns System.Uri object.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Takes in a string and returns System.Uri object. Returns null if its an invalid string.</returns>
        private Uri CreateUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Uri retVal = null;
            try
            {
                retVal = new Uri(url);
            }
            catch (UriFormatException)
            {
                return null;
            }

            return retVal;
        }

        /// <summary>
        /// Sets the error text when invalid url is passed in.
        /// </summary>
        /// <param name="errMsg"></param>
        private void SetErrorText(string errMsg)
        {
            this.ErrorLabel.Text = errMsg;
            this.ErrorLabel.ForeColor = Color.Red;
            this.ErrorLabel.Font.Bold = true;
            this.ErrorLabel.Visible = true;
        }

        /// <summary>
        /// Sets the source text area and source label.
        /// </summary>
        /// <param name="text"></param>
        private void SetTextArea(string text)
        {
            this.TextArea1.InnerText = text;
            this.TextArea1.Visible = true;
            this.SourceLable.Visible = true;
        }

        /// <summary>
        /// Sets all the values nessary to make gridview visible with correct data.
        /// </summary>
        /// <param name="datasource"></param>
        private void SetGridView(Dictionary<string, int> datasource)
        {
            bool visibility = datasource != null;
            this.GridView1.Visible = visibility;
            this.TagsLable.Visible = visibility;
            this.GridView1.DataSource = datasource;
            this.GridView1.DataBind();
        }

        /// <summary>
        /// Handling the logic of hiding various controls when the users requests for a new url.
        /// </summary>
        private void OnEnterButtonClick()
        {
            this.ErrorLabel.Text = string.Empty;
            this.ErrorLabel.Visible = false;
            this.SourceLable.Visible = false;
            this.TextArea1.InnerText = string.Empty;
            this.TextArea1.Visible = false;
            this.TagsLable.Visible = false;
            this.GridView1.Visible = false;
            this.GridView1.DataSource = null;
            this.GridView1.DataBind();
        }

        /// <summary>
        /// Takes in a System.Uri object and returns the entire page as a string.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetHTMLFromUri(Uri url)
        {
            if (url == null)
                return null;

            string result = null;
            try
            {
                var hwr = (HttpWebRequest)HttpWebRequest.Create(url);
                using (var r = hwr.GetResponse())
                using (var s = new StreamReader(r.GetResponseStream()))
                {
                    result = s.ReadToEnd();
                }
            }
            catch (Exception)
            {
                // invalid url.
                result = null;
            }

            return result;
        }

        /// <summary>
        /// This is the core method that extracts all the tags in the string representation of the website
        /// and returns a dictionary object with tags mapping to their count.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private Dictionary<string, int> ExtractAllTags(string htmlString)
        {
            // The first few characters till the first html tag are not properly formed XML.
            if (string.IsNullOrEmpty(htmlString))
                return null;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            Dictionary<string, int> nodelist = new Dictionary<string, int>();

            foreach (var node in doc.DocumentNode.DescendantsAndSelf())
            {
                if (node != null)
                {
                    // Skip the nodes starting with '#'
                    if (node.Name.StartsWith("#"))
                        continue;

                    if (nodelist.ContainsKey(node.Name))
                        nodelist[node.Name] = nodelist[node.Name] + 1;
                    else
                        nodelist[node.Name] = 1;
                }
            }
            return nodelist;
        }
    }
}