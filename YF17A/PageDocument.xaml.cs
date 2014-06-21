using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageDocument.xaml
    /// </summary>
    public partial class PageDocument : Page
    {
        public static String TAG = "PageDocument.xaml";
        private string ManualFile = @"操作手册.pdf";
        private string ElectricFile = @"电气原理图.pdf";
        private string mDocumentFile ;


        private System.Windows.Forms.Integration.WindowsFormsHost host;
        private AxAcroPDFLib.AxAcroPDF pdfviewer;
       
        public PageDocument()
        {
            InitializeComponent();
            mDocumentFile = ManualFile;
            Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            pdfviewer = new AxAcroPDFLib.AxAcroPDF();
            host.Child = pdfviewer;
            this.Stage.Children.Add(host);
           
            pdfviewer.setShowToolbar(false);
            pdfviewer.setShowScrollbars(true);

            PageDataExchange sInstance = PageDataExchange.getInstance();

            Dictionary<String,Object> bundle = sInstance.getExtra(PageDocument.TAG);
            Object action;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out action);

           
            if (action != null)
            {
                if (MenuControl.ACTION_MANUAL.Equals(action.ToString()))
                {
                    mDocumentFile = ManualFile;
                }
                else if (MenuControl.ACTION_ELECTRIC.Equals(action.ToString()))
                {
                    mDocumentFile = ElectricFile;
                }
            }

            String filePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            pdfviewer.LoadFile(Path.Combine(filePath, mDocumentFile));
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            this.Stage.Children.Remove(host);
            pdfviewer.Dispose();
            pdfviewer = null;
            host.Dispose();
            host = null;           
        }     
    }
}
