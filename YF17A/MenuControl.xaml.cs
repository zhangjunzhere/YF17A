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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    public partial class MenuControl : UserControl, IToolControlObserver
    {
        public const string TAG = "MenuControl.xaml";

        //menu panel name
       // public const string MENU_HELP = "help";
        public const string MENU_DOCUMENT = "menu_document";
        public const string MENU_SYSTEM_SETTING = "menu_system_setting";
        public const string MENU_PARAMETER_SETTING = "menu_parameter_setting"; 
        public const string MENU_WARNING = "menu_warning";  

        //action
        public const String ACTION_MANUAL = "manual";
        public const String ACTION_ELECTRIC = "electric";
        public const String ACTION_EXIT = "exit";
        public const String ACTION_USER_MANAGER = "user_manager";

        public const String ACTION_PARAMETER_MODIFY = "parameter_modify";
        public const String ACTION_PARAMETER_PERSIST = "parameter_persist";

        public const String ACTION_WARNNING_LOG = "warnning_log";
        public const String ACTION_WARNNING_STATISTIC = "warnning_statistic";

        public const String ACTION_CLEAN_SCREEN = "clean_screen";


       
        private IToolControlObserver mToolControl = new ToolControlObserverImpl();             
        private Dictionary<String, StackPanel> mPanelMap = new Dictionary<String, StackPanel>();
             

        public MenuControl()
        {
            InitializeComponent();
           // RegisterObeserver(MainWindow.getProtocalInstance());        
            mPanelMap.Add(MENU_DOCUMENT, this.menu_document);
            mPanelMap.Add(MENU_SYSTEM_SETTING, this.menu_system_setting);
            mPanelMap.Add(MENU_PARAMETER_SETTING, this.menu_parameter_setting);
            mPanelMap.Add(MENU_WARNING, this.menu_warning);
        }

        #region RegisterObservers
        public void RegisterObeserver(IObserverResult observer)
        {
            mToolControl.RegisterObeserver(observer);
        }

        public void Update(Dictionary<String, Object> bundle)
        {
            //mBundle = bundle;
            Object senderName;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderName);

            Dictionary<String, StackPanel>.Enumerator iterator = mPanelMap.GetEnumerator();

            while(iterator.MoveNext())
            {
                KeyValuePair<String, StackPanel> pair =  iterator.Current;
                if (pair.Key.Equals(senderName.ToString()))
                {
                    pair.Value.Visibility = Visibility.Visible;
                }
                else
                {
                    pair.Value.Visibility = Visibility.Collapsed;
                }
            }             
        } 

        public void NotifyObserver(Dictionary<String, Object> bundle)
        {
            mToolControl.NotifyObserver(bundle);
        }
        #endregion              
       
        public double UserControlToolTipX
        {
            get { return this.UserControlToolTipXY.X; }
            set { this.UserControlToolTipXY.X = value; }
        }

        public double UserControlToolTipY
        {
            get { return this.UserControlToolTipXY.Y; }
            set { this.UserControlToolTipXY.Y = value; }
        }
             

        private void on_BtnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            String name = btn.GetValue(Button.NameProperty) as String;
            Dictionary<String, Object> bundle = new Dictionary<string, object>();

            bundle.Add( PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, name);

           if (ACTION_MANUAL.Equals(name) || ACTION_ELECTRIC.Equals(name))
           {
                PageDataExchange sInstance = PageDataExchange.getInstance();
                sInstance.putExtra(PageDocument.TAG, bundle);

                Utils.NavigateToPage(MainWindow.sFrameReportName, PageDocument.TAG);                
            }
           else if(ACTION_PARAMETER_MODIFY.Equals(name))
           {
               Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterMain.TAG);              
            }
           else if (ACTION_PARAMETER_PERSIST.Equals(name))
           {
               Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterBackup.TAG);              
           }
           else if (ACTION_USER_MANAGER.Equals(name))
           {
               int userlevel = User.GetInstance().GetCurrentUserInfo().UserLevel;
               if (userlevel >= User.USER_PREVILIDGE_ADMINISTRATOR)
               {
                   Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserManager.TAG);
               }
               else
               {
                   MessageBox.Show("您需要管理员用户登录才能进行此操作！");
                   //bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
                   //PageDataExchange.getInstance().putExtra(PageUserRegister.TAG, bundle);
                   //Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);     
               }
           }
           else if (ACTION_CLEAN_SCREEN.Equals(name))
           {
               PageDataExchange context = PageDataExchange.getInstance();
               context.CommandObserver(PageCleanScreen.TAG, PageCleanScreen.TAG, Visibility.Visible);
           }
           else
           {
               //forward action to MainWindow
               NotifyObserver(bundle);
           }            
        }
     
    }
}
