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
    public partial class PageUser : Page, IObserverResult
    {
            
        public const string TAG = "PageUser.xaml";

        private int mState;
      
       
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

        
        public PageUser()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);

            mPanelList.Add(this.panel_login_entry);            
            mPanelList.Add(this.panel_register_entry);
            mPanelList.Add(this.panel_modify_password);

            mPanelList.Add(this.panel_action_successed);
            mPanelList.Add(this.panel_action_failed);
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
                mState = STATE_LOGIN_ENTRY;
                SwitchToPanel(mState); 
            }           
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
            //check wheather user login or not

            mState = STATE_LOGIN_ENTRY;
            if (User.GetInstance().GetCurrentUserInfo().IsLogin)
            {
                mState = ACTION_SUCCESSED_LOGIN;
            }        

            this.register_role_list.ItemsSource = User.GetInstance().getUserRoleList();
            this.register_role_list.DisplayMemberPath = "description";
            this.register_role_list.SelectedValuePath = "id";
            this.register_role_list.SelectedIndex = 0;           
            SwitchToPanel(mState); 
        }      

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {         
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);
        }

        private void SwitchToPanel( int state)
        {           
            this.btn_action_success2.Visibility = Visibility.Collapsed;
            this.btn_action_success3.Visibility = Visibility.Collapsed;          
            this.btn_action_failed2.Visibility = Visibility.Collapsed;

            if (state == STATE_LOGIN_ENTRY)
            {
                if (User.GetInstance().GetCurrentUserInfo().IsLogin)
                {
                    state = ACTION_SUCCESSED_LOGIN;
                    mState = state;
                 }
            }

            switch (state)
            {
              case ACTION_SUCCESSED_LOGIN:
                    this.tb_action_success_title.Text = @"登陆成功";
                    this.tb_action_success_info.Text = @"用户" + User.GetInstance().GetCurrentUserInfo().Account + @"已登陆";
                    this.btn_action_success1.Content = @"注销";
                    this.btn_action_success2.Content = @"修改密码";     
                    this.btn_action_success2.Visibility = Visibility.Visible;
                    if (User.GetInstance().GetCurrentUserInfo().UserLevel >= User.USER_PREVILIDGE_DEBUGGER)
                    {
                        this.btn_action_success3.Visibility = Visibility.Visible;
                    }
                    break;
              case ACTION_FAILED_LOGIN:
                    this.tb_action_failed_title.Text = @"登陆失败";
                    this.tb_action_failed_info.Text = @"错误的用户名或密码";       
                    this.btn_action_failed1.Content = @"重试";                         
                    break;
               case ACTION_SUCCESSED_REGISTER:
                    this.tb_action_success_title.Text = @"注册成功";
                    this.tb_action_success_info.Text = @"请返回登陆窗口登陆";                      
                    this.btn_action_success1.Content = @"确定";               
                    break;
              case ACTION_FAILED_REGISTER:
                    this.tb_action_failed_title.Text = @"注册失败";
                    this.tb_action_failed_info.Text = @"用户名错误或者两次输入密码不一致";
                    this.btn_action_failed1.Content = @"重试";
                    this.btn_action_failed2.Content = @"取消";
                    this.btn_action_failed2.Visibility = Visibility.Visible;  
                    break;
              case ACTION_SUCCESSED_MODYFY:
                    this.tb_action_success_title.Text = @"修改密码成功";
                    this.tb_action_success_info.Text = @"如果密码遗忘请联系管理员";                      
                    this.btn_action_success1.Content = @"确定";                   
                    break;
              case ACTION_FAILED_MODYFY:
                     this.tb_action_failed_title.Text = @"修改密码失败";
                    this.tb_action_failed_info.Text = @"原密码错误或者两次输入密码不一致";                      
                    this.btn_action_failed1.Content = @"重试";
                    this.btn_action_failed2.Content = @"取消";
                    this.btn_action_failed2.Visibility = Visibility.Visible;  
                    break;
                default:
                    break;
            }

            int mCurrentPanelIndex = state;           
            if (mState >= ACTION_SUCCESSED_LOGIN)
            {
                mCurrentPanelIndex = STATE_ACTION_SUCCESSED + (mState+1) % 2;
            }

            //clear viriable
            this.entry_account.Clear();
            this.entry_password.Clear();

            this.pb_modify_old.Clear();
            this.pb_modify_new1.Clear();
            this.pb_modify_new2.Clear();

            this.register_account.Clear();
            this.register_password1.Clear();
            this.register_password2.Clear();

            for (int i = 0; i < mPanelList.Count; i++)
            {
                StackPanel panel = mPanelList[i];
                if (i == mCurrentPanelIndex)
                {
                    panel.Visibility = Visibility.Visible;
                }
                else
                {
                    panel.Visibility = Visibility.Collapsed;
                }
            }         
        }

       
        //login page
        private void entry_login_Click(object sender, RoutedEventArgs e)
        {
            String username = entry_account.Text;
            String password = entry_password.Password;

            mState = ACTION_FAILED_LOGIN;
            Boolean success = User.GetInstance().Login(username, password);
            if (success)
            {
                mState = ACTION_SUCCESSED_LOGIN;
                //notifyPageLogin(PageLogin.LOGIN);                
            }

            SwitchToPanel(mState);
        }
        private void entry_register_Click(object sender, RoutedEventArgs e)
        {         
            mState = STATE_REGISTER_ENTRY;
            SwitchToPanel(mState);
        }      
      
        //register page
         private void onRegisterConfirmClicked(object sender, RoutedEventArgs e)
         {
             String name = this.register_account.Text ;
             String password1 = this.register_password1.Password;
             String password2 = this.register_password2.Password;
             int roleIndex = (int)this.register_role_list.SelectedValue;

             mState = ACTION_FAILED_REGISTER;
             
             if (!String.IsNullOrEmpty(name)
                 && !String.IsNullOrEmpty(password1)
                 &&  password1.Equals(password2)
                 && User.GetInstance().registerUser(name, password1, roleIndex))
             {
                 mState = ACTION_SUCCESSED_REGISTER;
             }

             SwitchToPanel(mState); 
         }
         private void onRegisterCancelClicked(object sender, RoutedEventArgs e)
         {
             onModifyCancelClicked(sender,e);
         }

         //modify password 
         private void onPasswordModifyClicked(object sender, RoutedEventArgs e)
         {
             String password0 = this.pb_modify_old.Password;
             String password1 = this.pb_modify_new1.Password;
             String password2 = this.pb_modify_new2.Password;
             int roleIndex = this.register_role_list.SelectedIndex + 1;

             mState = ACTION_FAILED_MODYFY;

             User.Info info = User.GetInstance().GetCurrentUserInfo();

             if (info.IsLogin
                 && !String.IsNullOrEmpty(password0)
                 && !String.IsNullOrEmpty(password1)
                 && password1.Equals(password2)
                 && User.GetInstance().ChangePassword(info.Account,password0, password1))
             {
                 mState = ACTION_SUCCESSED_MODYFY;
             }
             SwitchToPanel(mState);
         }

         private void onModifyCancelClicked(object sender, RoutedEventArgs e)
         {
             mState = STATE_LOGIN_ENTRY;          
             SwitchToPanel(mState);
         }
        

        private void action_success1_Click(object sender, RoutedEventArgs e)
        {
            switch (mState)
            {
                case ACTION_SUCCESSED_LOGIN:    
                    User.GetInstance().Logout();
                    mState = STATE_LOGIN_ENTRY;                    
                    //notifyPageLogin(PageLogin.LOGOUT);
                    break;            
                case ACTION_SUCCESSED_REGISTER:                 
                  //  this.btn_action_success1.Content = @"确定";
                    mState = STATE_LOGIN_ENTRY;                 
                    break;               
                case ACTION_SUCCESSED_MODYFY:
                    mState = STATE_LOGIN_ENTRY;
                    if (User.GetInstance().GetCurrentUserInfo().IsLogin)
                    {
                        mState = ACTION_SUCCESSED_LOGIN;
                    }                   
                    break;          
                default:
                    break;
            }

            SwitchToPanel(mState);
        }       

        private void action_success2_Click(object sender, RoutedEventArgs e)
        {
            switch (mState)
            {
                case ACTION_SUCCESSED_LOGIN:                 
                    //this.btn_action_success2.Content = @"修改密码";
                    mState = STATE_MODYFY_PASSWORD ;
                    break;
             
                case ACTION_SUCCESSED_REGISTER:                 
                    break;
          
                case ACTION_SUCCESSED_MODYFY:                 
                    break;           
            }
            SwitchToPanel(mState);
        }

        private void action_failed1_Click(object sender, RoutedEventArgs e)
        {
            switch (mState)
            {
                case ACTION_FAILED_LOGIN:
                    mState = STATE_LOGIN_ENTRY;
                    break;

                case ACTION_FAILED_REGISTER: //retry
                    mState = STATE_REGISTER_ENTRY;                   
                    break;
          
                case ACTION_FAILED_MODYFY:
                    mState = STATE_LOGIN_ENTRY;
                    break;
                default:
                    break;                 
            }
            SwitchToPanel(mState);
        }

        
        private void action_failed2_Click(object sender, RoutedEventArgs e)
        {
            switch (mState)
            {              
                case ACTION_FAILED_LOGIN:            
                    break;
        
                case ACTION_FAILED_REGISTER:
                    mState = STATE_LOGIN_ENTRY;                  
                    break;
          
                case ACTION_FAILED_MODYFY:
                    mState = STATE_LOGIN_ENTRY;                   
                    break;       
            }
            SwitchToPanel(mState);
        }
        
    }
}
