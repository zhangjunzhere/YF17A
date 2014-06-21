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
    public partial class PageUserManager : Page, IObserverResult
    {

        public const string TAG = "PageUserManager.xaml";

        public const String UserName = "UserName";
        PageDataExchange mContext = PageDataExchange.getInstance();
       
        //category
        private const int STATE_LOGIN_ENTRY = 0;
        private const int STATE_REGISTER_ENTRY = 1;
        private const int STATE_MODYFY_PASSWORD = 2;
        private const int STATE_ACTION_SUCCESSED = 3;
        private const int STATE_ACTION_FAILED = 4;
        
        //action
        private const int ACTION_SUCCESSED_LOGIN = 3;
        private const int ACTION_FAILED_LOGIN = 4;
        private const int ACTION_SUCCESSED_REGISTER = 5;
        private const int ACTION_FAILED_REGISTER = 6;
        private const int ACTION_SUCCESSED_MODYFY = 7;
        private const int ACTION_FAILED_MODYFY = 8;       

        private List<StackPanel> mPanelList = new List<StackPanel>();
        private List<String> mUserRoleList = new List<String>();


        public PageUserManager()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
            mContext = PageDataExchange.getInstance();           
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            string action = senderValue.ToString();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {    
            mContext.addResultObserver(TAG,this);
            mContext.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
            //check wheather user login or not

            List<User.Info> usersinfo = User.GetInstance().getUsersInfo();
            this.lv_users.ItemsSource = usersinfo;          
        }      

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {                    
            mContext.removeResultObserver(TAG);
            mContext.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);

           // mContext.CommandObserver(PageLogin.TAG, null, PageLogin.KILLKEYBOARD);
        }


        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            String account = btn.Tag.ToString();
            Boolean success = User.GetInstance().deleteUser(account);
            String title = "删除用户";
            String info = "用户" + account + "已删除！";
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageUserActionResult.TITLE, title);
             bundle.Add(PageUserActionResult.INFO, info);
             mContext.putExtra(PageUserActionResult.TAG, bundle);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserActionResult.TAG,false);
        }

        private void btn_reset_password_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            String account = btn.Tag.ToString();

            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageUserManager.UserName, account);
            bundle.Add(PageUserPassword.PASSWORD_PAGE, PageUserPassword.ID_RESET);
            mContext.putExtra(PageUserPassword.TAG, bundle);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserPassword.TAG);
        }

        private void btn_add_user_click(object sender, RoutedEventArgs e)
        {            
              Dictionary<String, Object> bundle = new Dictionary<string, object>();           
              bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_REGISTER);
              mContext.putExtra(PageUserRegister.TAG, bundle);
              Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);
        }      
    }
}
