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
using System.Threading;
using System.Runtime.InteropServices;
using YF17A.AccessDatabase;
using TwinCAT.Ads;
using System.Windows.Threading;

namespace YF17A
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IProtocal
    {
        public const String TAG = "MainWindow.xaml";
        private static IProtocal mGlobalWindow;
        //ePageIndex mPageIndex;
        private Dictionary<String, Page> mPageCache = new Dictionary<String, Page>();


        private TcAdsClient mAdsClient;
        private  BeckHoff mBeckHoff;
        public static String sFrameReportName = "reportPanel";
        public static String sFrameToolbarName = "toolbarPanel";
        public static String sFrameCleanScreen = "cleanScreenPanel"; 

        public  static BitmapImage sRedLight;
        public static BitmapImage sGreenLight;
        public static BitmapImage sYellowLight;
        public static BitmapImage sDisableLight;
        public static BitmapImage sEnableLight;
        public static BitmapImage sSwitchOn;
        public static BitmapImage sSwitchOff;
        private DispatcherTimer tmUserLogout = new DispatcherTimer(); 
       
        public MainWindow()
        {
            InitializeComponent();

            

            mGlobalWindow = this;
            mAdsClient = new TcAdsClient();

            menu.RegisterObeserver(this);
            
            sRedLight = new BitmapImage(new Uri("/image/debug_light_red.png", UriKind.Relative));
            sGreenLight = new BitmapImage(new Uri("/image/debug_light_green.png", UriKind.Relative));
            sYellowLight = new BitmapImage(new Uri("/image/debug_light_yellow.png", UriKind.Relative));
            sDisableLight = new BitmapImage(new Uri("/image/debug_light_disable.png", UriKind.Relative));
            sEnableLight = new BitmapImage(new Uri("/image/debug_light_enable.png", UriKind.Relative));
            sSwitchOn = new BitmapImage(new Uri("/image/SwitchOn.png", UriKind.Relative));
            sSwitchOff = new BitmapImage(new Uri("/image/SwitchOff.png", UriKind.Relative));
       
            tmUserLogout.Tick += new EventHandler(tmLogout_Tick);
            tmUserLogout.Interval = TimeSpan.FromSeconds(300);

            InitBeckHoff();
           
        }

     

        public static IProtocal getProtocalInstance()
        {
            return mGlobalWindow;
        }

         //override
        public BeckHoff GetBeckHoff()
        {
            if (mBeckHoff == null)
            {
                MessageBox.Show("设备未运行!");
            }
            return mBeckHoff;
        }


        #region IProtocal 
        public void navigateToPage(String panelName, String pageName)
        {
            Frame frame = rootPanel.FindName(panelName) as Frame;
            Page p;
            if (mPageCache.ContainsKey(pageName))
            {
                mPageCache.TryGetValue(pageName, out p);
            }
            else
            {
                p = new Page();
            }

            frame.Navigate(new Uri(pageName, UriKind.Relative));
        }       

        #endregion

        #region IResultObserver 
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            //show menu from toolbar main action
            if (senderName.Equals(ToolbarMain.TAG))
            {
                Object menuIndex;
                bundle.TryGetValue(ToolControlObserverImpl.KEY_MENU_INDEX, out menuIndex);

                int index = (int)menuIndex;

                int offsetX = (int)(index * toolbarPanel.ActualWidth / 7);
                int offSetY = (int)(rootPanel.ActualHeight - toolbarPanel.Height * 2);
               
                menu.UserControlToolTipX = offsetX;
                menu.UserControlToolTipY = offSetY;
                menu.Visibility = Visibility.Visible;   
     
                menu.Update(bundle);
            }
            // responce to menu action
            else if (senderName.Equals(MenuControl.TAG))
            {
               // this.Close();
                Object value;
                bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out value);
                String action = value.ToString();

                if (MenuControl.ACTION_EXIT.Equals(action))
                {
                    int userlevel = User.GetInstance().GetCurrentUserInfo().UserLevel;
                    if (userlevel < User.USER_PREVILIDGE_ADMINISTRATOR)
                    {
                        MessageBox.Show("您需要管理员用户登录才能进行此操作！");
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("您确定要关机吗？", "关机", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {                           
                            this.Close();
                        }
                    }
                                  
                }               
            } 
            //user log in
            else if (PageUserRegister.TAG.Equals(senderName))
            {
                if (PageLogin.LOGIN.Equals(senderValue))
                {                   
                    tmUserLogout.Start();
                }
                //user logout
                else if (PageLogin.LOGOUT.Equals(senderValue))
                {                    
                    tmUserLogout.Stop();
                }
            }             
        }
        #endregion

        private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
           menu.Visibility = Visibility.Hidden;

           if (User.GetInstance().GetCurrentUserInfo().IsLogin && tmUserLogout.IsEnabled)
           {
               // restart timer               
               tmUserLogout.Stop();
               tmUserLogout.Start();
           }

        }

        private void tmLogout_Tick(object sender, EventArgs e)
        {
            //notify user logout
            if (User.GetInstance().GetCurrentUserInfo().IsLogin)
            {
                User.GetInstance().Logout();    
            }
        }
         
        private void adsClient_AdsNotificationEx(object sender, AdsNotificationExEventArgs e)
        {           
            mBeckHoff.callback_Notification(e);
        }

        private void mAdsClient_AdsNotificationError(object sender, AdsNotificationErrorEventArgs e)
        {            
            System.Console.WriteLine(e.ToString());
            //   mBeckHoff.callback_Notification(e);
        }
        private void adsClient_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {           
            System.Console.WriteLine(e.ToString());
            //   mBeckHoff.callback_Notification(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);


            String description = @"进入系统";
            Log.write(Log.CATEGOTY_RUN, description);

            WarnningDataSource observer = WarnningDataSource.GetInstance();
            observer.RegisterObserver();
        }

      
        private void Window_Closed(object sender, EventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            WarnningDataSource observer = WarnningDataSource.GetInstance();
            observer.UnregisterObserver();

            // http://forums.adobe.com/thread/487023
            Win32Helper.CoFreeUnusedLibraries();
            //  Win32Helper.CoUninitialize();

            mAdsClient.Dispose();
            String description = @"退出系统";
            Log.write(Log.CATEGOTY_RUN, description);
        }     

        private void InitBeckHoff()
        {            
            try
            {
                mAdsClient.Connect(801);
               mAdsClient.AdsNotificationEx += new AdsNotificationExEventHandler(adsClient_AdsNotificationEx);
               mAdsClient.AdsStateChanged += new AdsStateChangedEventHandler(adsClient_AdsStateChanged);
               mAdsClient.AdsNotificationError += new AdsNotificationErrorEventHandler(mAdsClient_AdsNotificationError);
                mBeckHoff = new BeckHoff(mAdsClient);
                mBeckHoff.addAllNotifacations();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "设备未运行!");
                Close();
            }
        }

      
    }
}
