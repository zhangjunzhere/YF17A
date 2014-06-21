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
    /// Interaction logic for ToolbarMain.xaml
    /// </summary>
    public partial class ToolbarParameter : Page,IObserverResult
    {
        public const string TAG = "ToolbarParameter.xaml";

        public const string ACTION_HELP = "help";
        public const string ACTION_BACK = "back";
        public const string ACTION_SETTING = "setting";

        public const string ACTIONBAR_REGISTER = "actionbar_register";
        public const string ACTIONBAR_UNREGISTER = "actionbar_unregister";
        public const string ACTIONBAR_SETTING_SHOW = "actionbar_setting_show";
        public const string ACTIONBAR_SETTING_HIDE = "actionbar_setting_hide";

        public const string ACTIONBAR_BACK_TO_PAGE = "actionbar_back_to_page";

        public static  Stack<String> sBackPageStack = new Stack<String>(); //page name
       // public static Boolean IsLoaded = false;

        private IObserverResult mActoinbarObserver;
        public ToolbarParameter()
        {
            InitializeComponent();            
        }
        
        #region notify observers
        public void RegisterActionBarObserver(IObserverResult observer)
        {
            mActoinbarObserver = observer;           
        }

        public void UnregisterActionBarObserver()
        {
            mActoinbarObserver = null;
        }

        private void help_MouseUp(object sender, RoutedEventArgs e)
        {
            //Utils.NavigateToPage(MainWindow.sFrameReportName, Utils.uriPagePopup);  

            String name = ToolbarMain.BUTTON_HELP;
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, name);

            PageDataExchange context = PageDataExchange.getInstance();
            String[] observerTags = new String[] { ToolTipHelper.TAG, PageParameterHelper.TAG };

            foreach (String tag in observerTags)
            {
                IObserverResult tooltiphelper = context.getResultObserverByTag(tag);

                if (tooltiphelper != null)
                {
                    tooltiphelper.onRecieveResult(bundle);
                }
            }
        }

        private void back_MouseUp(object sender, RoutedEventArgs e)
        {
            String uriPage = PageHome.TAG;
            if (sBackPageStack.Count > 0)
            {
                 uriPage = sBackPageStack.Pop();              
            }
            Utils.NavigateToPage(MainWindow.sFrameReportName, uriPage,false);     
        }

        private void home_MouseUp(object sender, RoutedEventArgs e)
        {
            sBackPageStack.Clear();
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageHome.TAG);          
        }

        private void setting_MouseUp(object sender, RoutedEventArgs e)
        {
            notifyObserver(ACTION_SETTING);
        }

        private void notifyObserver(String action)
        { 
            Dictionary<String, Object> bundle = new Dictionary<String, Object>();
            bundle.Add( PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, action);           
            if (mActoinbarObserver != null)
            {
                mActoinbarObserver.onRecieveResult(bundle);
            }
        }
        #endregion

        #region IObseverResult
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //IsLoaded = true;
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);            
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //IsLoaded = false;
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            PageDataExchange context = PageDataExchange.getInstance();
            String observerName = senderName.ToString();

            if (ACTIONBAR_REGISTER.Equals(senderValue))
            {
                mActoinbarObserver =  context.getResultObserverByTag(observerName);
            }
            else if (ACTIONBAR_UNREGISTER.Equals(senderValue))
            {
                mActoinbarObserver = null;
            }
            else if (ACTIONBAR_SETTING_SHOW.Equals(senderValue))
            {
                this.btn_setting.Visibility = Visibility.Visible;                   
            }
            else if (ACTIONBAR_SETTING_HIDE.Equals(senderValue))
            {
                this.btn_setting.Visibility = Visibility.Hidden;
            }
            //UpdateHelpState();
        }
        #endregion 
    }
}
