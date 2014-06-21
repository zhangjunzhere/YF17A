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
    public partial class PageUserPassword : Page, IObserverResult
    {

        public const string TAG = "PageUserPassword.xaml";
        
        public const String PASSWORD_PAGE = "password_page";
        public const int ID_RESET = 0;
        public const int ID_MODIFY = 1;

        private int mPageId; 
        private String mAccount; 
        List<Panel> mListPanel = new List<Panel>();

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


        public PageUserPassword()
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
            }           
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);

            context.CommandObserver(PageLogin.TAG, null, PageLogin.SHOWKEYBOARD);

            mListPanel.Add(this.panel_password_verify);
            mListPanel.Add(this.panel_password_old);

            Dictionary<String, Object> bundle = context.getExtra(TAG);
            object pageid;
            object account;
            bundle.TryGetValue(PASSWORD_PAGE, out pageid);
            bundle.TryGetValue(PageUserManager.UserName, out account);           
            this.tb_user_name.Text = account.ToString();
            mAccount = account.ToString();
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
            if (pageid == ID_MODIFY)
            {
                viz = Visibility.Visible;
            }
            foreach (Panel panel in mListPanel)
            {
                panel.Visibility = viz;
            }            
        }

        private void btn_password_modify_clicked(object sender, RoutedEventArgs e)
        {
            String password0 = this.pb_modify_old.Password;
            String password1 = this.pb_modify_new1.Password;
            String password2 = this.pb_modify_new2.Password;

            if (mPageId == ID_MODIFY)
            {
                if (String.IsNullOrEmpty(mAccount))
                {
                    mAccount = User.GetInstance().GetCurrentUserInfo().Account;
                }

                if (User.GetInstance().GetCurrentUserInfo().IsLogin
                    && !String.IsNullOrEmpty(password0)
                    && !String.IsNullOrEmpty(password1)
                    && password1.Equals(password2))
                {
                    User.GetInstance().ChangePassword(mAccount, password0, password1);
                }
            }
            else if (mPageId == ID_RESET)
            {
                if (!String.IsNullOrWhiteSpace(password1))
                {
                    User.GetInstance().ResetPassword(mAccount, password1);
                }
            }
        }
    }
}
