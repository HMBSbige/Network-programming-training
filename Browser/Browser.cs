using System;
using System.Windows.Forms;
using CefSharp; 
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;

namespace Browser
{
    public partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();
        }

        private ChromiumWebBrowser browser;

        private void Form1_Load(object sender, EventArgs e)
        {
            ResizeURlTextBox();
            var settings = new CefSettings();
            Cef.Initialize(settings);
            browser = new ChromiumWebBrowser(@"ie.icoa.cn")
            {
                Dock = DockStyle.Fill
            };
            toolStripContainer1.ContentPanel.Controls.Add(browser);
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;
            browser.LifeSpanHandler = new OpenPageSelf();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            LoadUrl(urlTextbox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }

        private void urlTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadUrl(urlTextbox.Text);
            }
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextbox.Text = args.Address);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            LoadUrl(@"ie.icoa.cn");
        }

        private void ResizeURlTextBox()
        {
            urlTextbox.Width = toolStrip1.Width - toolStripButton1.Width - toolStripButton2.Width - toolStripButton3.Width - toolStripButton4.Width - toolStripButton5.Width- toolStripButton5.Width;
        }

        private void Browser_Resize(object sender, EventArgs e)
        {
            ResizeURlTextBox();
        }

        private class OpenPageSelf : ILifeSpanHandler
        {
            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return false;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {

            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {

            }

            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
                string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
                IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                newBrowser = null;
                var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
                chromiumWebBrowser.Load(targetUrl);
                return true;
            }
        }
    }
}
