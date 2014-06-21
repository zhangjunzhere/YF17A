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
    public partial class PageUserRegister : Page, IObserverResult
    {

        public const string TAG = "PageUserRegister.xaml";

        public const String USER_PAGE = "userpage";
        public const int ID_LOGIN = 0;
        public const int ID_REGISTER = 1;
        private int mPageId;
        private int mState;
        List<Panel> mListPanel = new List<Panel>();
        //category
        private const int STATE_LOGIN_ENTRY = 0;
        private const int STATE_REGISTER_ENTRY = 1;
        private const int STATE_MODYFY_PASSWORD = 2;
        private const int STATE_ACTION_SUCCESSED = 3;
        private const int STATE_ACTION_FAILED = 4;
        
        //action
        public const int ACTION_SUCCESSED_LOGIN = 3;
        private const int ACTION_FAILED_LOGIN = 4;
        private const int ACTION_SUCCESSED_REGISTER = 5;
        private const int ACTION_FAILED_REGISTER = 6;
        private const int ACTION_SUCCESSED_MODYFY = 7;
        private const int ACTION_FAILED_MODYFY = 8;       

        private List<StackPanel> mPanelList = new List<StackPanel>();
        private List<String> mUserRoleList = new List<String>();


        public PageUserRegister()
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

            if (PageLogin.LOGIN.Equals(action) || PageLogin.LOGOUT.Equals(action))
            {
                String title = String.Empty;
                String info = String.Empty;
                Dictionary<String, Object> pagedata = new Dictionary<string, object>();
                PageDataExchange context = PageDataExchange.getInstance();
                if (PageLogin.LOGIN.Equals(action))
                {
                    bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
                    context.putExtra(PageUserRegister.TAG, bundle);
                    //Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);                  
                }
                else if (PageLogin.LOGOUT.Equals(action))
                {
                    bundle.Add(PageUserActionResult.TITLE, "用户登出");
                    bundle.Add(PageUserActionResult.INFO, "用户" + User.GetInstance().GetCurrentUserInfo().Account + "已登出！");
                    context.putExtra(PageUserActionResult.TAG, bundle);
                  //  Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserActionResult.TAG,false);
                }                
            }            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
            context.CommandObserver(PageLogin.TAG, null, PageLogin.SHOWKEYBOARD);

            mListPanel.Add(this.panel_password_verify);
            mListPanel.Add(this.panel_role);
            this.register_role_list.ItemsSource = User.GetInstance().getUserRoleList();
            this.register_role_list.DisplayMemberPath = "description";
            this.register_role_list.SelectedValuePath = "id";
            this.register_role_list.SelectedIndex = 0;           

            Dictionary<String, Object> bundle = context.getExtra(TAG);
            object pageid;
            bundle.TryGetValue(USER_PAGE, out pageid);
            mPageId = (int)pageid;
            SwitchToPage(mPageId);
        }      

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {         
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);
            context.CommandObserver(PageLogin.TAG, null, PageLogin.KILLKEYBOARD);
        }

        public void SwitchToPage(int pageid)
        {
            Visibility viz = Visibility.Collapsed;
            if (pageid == ID_REGISTER)
            {
                viz = Visibility.Visible;
                this.tb_title.Text = "用户注册";               
            }
            else if (pageid == ID_LOGIN)
            {
                this.tb_title.Text = "用户登录";
            }
         
            foreach (Panel panel in mListPanel) 
            {
                panel.Visibility = viz;
            }
        }

        private void btn_action_verify_click(object sender, RoutedEventArgs e)
        {
            String username = this.tb_account.Text;
            String password1 = this.pb_password0.Password;
            String password2 = this.pb_password1.Password;
            int roleIndex = (int)this.register_role_list.SelectedValue;

            String title = String.Empty;
            String info = String.Empty;
            int status = ACTION_FAILED_LOGIN;
            Boolean success = false;
            
            if (mPageId == ID_LOGIN)
            {
                 success = User.GetInstance().Login(username, password1);
                if (success)
                {
                    title = "登录成功！";
                    info = "用户" + username + "已登陆！";
                    status = ACTION_SUCCESSED_LOGIN;
                }
                else
                {
                    title = "登录失败！";
                    info = "错误的用户名或密码！";
                }
            }
            else if (mPageId == ID_REGISTER)
            {
                if (!String.IsNullOrEmpty(username)
                    && !String.IsNullOrEmpty(password1)
                    && password1.Equals(password2))                     
                {
                     success  = User.GetInstance().registerUser(username, password1, roleIndex);
                }

                if (success)
                {
                    title = "注册成功！";
                    info = "用户" + username + "已注册！";
                }
                else
                {
                    title = "注册失败！";
                    info = "已存在的用户名或两次密码输入不一致！";
                }
            }
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageUserActionResult.TITLE, title);
            bundle.Add(PageUserActionResult.INFO, info);
            bundle.Add(PageUserActionResult.KEY_STATUS, status);
            PageDataExchange context = PageDataExchange.getInstance();
            context.putExtra(PageUserActionResult.TAG, bundle);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserActionResult.TAG,false);
        }      
    }
}
