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
    /// Interaction logic for PageWarnningHeader.xaml
    /// </summary>
    public partial class PageWarnningHeader : Page,IObserverResult
    {
        public const String TAG = "PageWarnningHeader.xaml";
        List<WarnningDataSource.ErrorInfo> listError = new List<WarnningDataSource.ErrorInfo>();

        public PageWarnningHeader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);

            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);    

            WarnningDataSource data = WarnningDataSource.GetInstance();
              
            this.cb_info.ItemsSource = data.GetWarnningList();
            this.cb_info.SelectedIndex = 0;      
           // this.cb_info.DisplayMemberPath = "description";
          
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {         
            //PageDataExchange context = PageDataExchange.getInstance();
            //context.addResultObserver(TAG, this);           
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {        
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object resultValue;
            Object senderValue;

            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_RESULT_VALUE, out resultValue);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            //beckhoff changed 
            if (WarnningDataSource.TAG.Equals(senderName) )
            {
                WarnningDataSource.ErrorInfo infoChanged = (WarnningDataSource.ErrorInfo)senderValue;
                WarnningDataSource.ErrorInfo infoCurrent = (WarnningDataSource.ErrorInfo)this.cb_info.SelectedItem;
                WarnningDataSource data = WarnningDataSource.GetInstance();
                              

                //if (data.IsWarnningAdded(infoChanged))
                //{
                //    //this.cb_info.SelectedItem = infoChanged;
                    
                //}
                //else if (infoCurrent != null && infoChanged.level == infoCurrent.level)// item was removed
                //{
                //    if (this.cb_info.SelectedIndex > 0)
                //    {
                //        this.cb_info.SelectedIndex = 0;
                //    }
                //}

                PageDataExchange context = PageDataExchange.getInstance();
                WarnningDataSource dataSource = WarnningDataSource.GetInstance();
                if (dataSource.GetWarnningCount() == 0 && context.IsPageLoaded(PageWarningDetail.TAG))
                {
                    Utils.NavigatePageBack();
                }

                this.cb_info.ItemsSource = data.GetWarnningList();
                this.cb_info.Items.Refresh();
                this.cb_info.SelectedIndex = 0;
            }
        }
      
        #endregion

        private void infoPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            onSelected();   
        }
        private void cb_info_DropDownClosed(object sender, EventArgs e)
        {
            onSelected();
        }

        private void onSelected()
        {
            if (this.cb_info.SelectedIndex < 0)
            {
                this.cb_info.SelectedIndex = 0;
            }
            WarnningDataSource.ErrorInfo info = (WarnningDataSource.ErrorInfo)this.cb_info.SelectedItem;
            PageDataExchange context = PageDataExchange.getInstance();
            if ( info.level < 200)
            { 
                Dictionary<String, Object> bundle = new Dictionary<string, object>();
                bundle.Add(PageDataExchange.KEY_SENDER_NAME, WarnningDataSource.TAG);
                bundle.Add(PageDataExchange.KEY_SENDER_VALUE, info);
                context.putExtra(PageWarningDetail.TAG, bundle);
                Utils.NavigateToPage(MainWindow.sFrameReportName, PageWarningDetail.TAG,false);
            }

            context.NotifyObserverChanged(PageWarningDetail.TAG, WarnningDataSource.TAG, info);            
            context.NotifyObserverChanged(PageStatus.TAG, PageStatus.CODE, info.code);
            context.NotifyObserverChanged(PageStatus.TAG, PageStatus.PATH, Utils.getPageDescByTag(PageWarningDetail.TAG));
        }      
    }
}
