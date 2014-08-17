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
using System.Windows.Shapes;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageWarningDetail.xaml
    /// </summary>
    public partial class PageWarningDetail : Page,IObserverResult
    {
        public const String TAG = "PageWarningDetail.xaml";

        private  WarnningDataSource mWarnningDataSource;
        private Dictionary<String, WarnningDataSource.ErrorInfo> mStatusMap ; //plcvarname, errorinfo      
        private List<WarnningDataSource.ErrorInfo> mListError = new List<WarnningDataSource.ErrorInfo>();

                
        public PageWarningDetail()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);

            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {            
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);          

            //init views
            Dictionary<String, Object> bundle = context.getExtra(TAG);
            onRecieveResult(bundle);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override beckhoff changed       
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;          
            Object senderValue;

            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);          
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (WarnningDataSource.TAG.Equals(senderName))
            {
                WarnningDataSource.ErrorInfo info = (WarnningDataSource.ErrorInfo)senderValue;
                this.tb_code.Text = info.code;
                this.tb_solution.Text = info.solution;

                PageDataExchange context = PageDataExchange.getInstance();
                context.NotifyObserverChanged(PageStatus.TAG, PageStatus.CODE, info.code);
            }
        }
        #endregion

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            mWarnningDataSource = WarnningDataSource.GetInstance();
            mStatusMap = mWarnningDataSource.mStatusMap;


            WarnningDataSource.ErrorInfo info = new WarnningDataSource.ErrorInfo() { level = 202, description = "add test" };
            
            int count = mWarnningDataSource.GetWarnningCount();
            //test add to list
            mListError.Add(null);
            mWarnningDataSource.AddWarningItem(info);
            count = mWarnningDataSource.GetWarnningCount();

            PageDataExchange context = PageDataExchange.getInstance();
            context.NotifyObserverChanged(PageWarnningHeader.TAG, WarnningDataSource.TAG, info);
         
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            mWarnningDataSource = WarnningDataSource.GetInstance();
            mStatusMap = mWarnningDataSource.mStatusMap;
            WarnningDataSource.ErrorInfo info = new WarnningDataSource.ErrorInfo() { level = 202, description  = "add test" };
            //test delete to list 
            mWarnningDataSource.RemoveWarningItem(info);
            int count = mWarnningDataSource.GetWarnningCount();

            PageDataExchange context = PageDataExchange.getInstance();
            context.NotifyObserverChanged(PageWarnningHeader.TAG, WarnningDataSource.TAG, info);
        }
    }
}

