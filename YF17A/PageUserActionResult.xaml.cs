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
using System.Windows.Threading;
using  YF17A.AccessDatabase;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageHelp.xaml
    /// </summary>
    public partial class PageUserActionResult : Page, IObserverResult
    {
            
        public const string TAG = "PageUserActionResult.xaml";

        public const String TITLE = "title";
        public const String INFO = "info";
        public const String KEY_STATUS = "status";
       
      
        public PageUserActionResult()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            string action = senderValue.ToString();
            PageDataExchange context = PageDataExchange.getInstance();
            //if (PageLogin.LOGIN.Equals(action))
            //{
            //    bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
            //    context.putExtra(PageUserRegister.TAG, bundle);
            //    Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);
            //}
            //else if (PageLogin.LOGOUT.Equals(action))
            //{
            //    bundle.Add(PageUserActionResult.TITLE, "用户登出");
            //    bundle.Add(PageUserActionResult.INFO, "用户" + User.GetInstance().GetCurrentUserInfo().Account + "已登出！");
            //    context.putExtra(PageUserActionResult.TAG, bundle);
            //    Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserActionResult.TAG,false););
            //}       
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
            //check wheather user login or not
            Dictionary<String, Object> bundle = context.getExtra(PageUserActionResult.TAG);
            Object title;
            Object info;
            Object status;
            bundle.TryGetValue(TITLE, out title);
            bundle.TryGetValue(INFO, out info);
            bundle.TryGetValue(KEY_STATUS, out status);

            this.tb_title.Text = title.ToString();
            this.tb_info.Text = info.ToString();

            if ((int)status == PageUserRegister.ACTION_SUCCESSED_LOGIN)
            {
                Utils.NavigatePageBack();
            }
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);
        }
   }    
}
