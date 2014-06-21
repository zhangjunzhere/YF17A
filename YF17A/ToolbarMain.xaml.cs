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
    public partial class ToolbarMain : Page,IToolControlObserver
    {
        public const string TAG = "toolbarmain.xaml";

        private IToolControlObserver mToolControlObserver = new ToolControlObserverImpl();
        public const string BUTTON_HELP = "help";
        public const string BUTTON_HOME = "home";

        Dictionary<String, int> mMenuIndexMap = new Dictionary<string, int>();

        public ToolbarMain()
        {
            InitializeComponent();
            RegisterObeserver( MainWindow.getProtocalInstance());
            
            mMenuIndexMap.Add(MenuControl.MENU_SYSTEM_SETTING, 1);
            mMenuIndexMap.Add(MenuControl.MENU_DOCUMENT, 2);
            mMenuIndexMap.Add(MenuControl.MENU_PARAMETER_SETTING, 3);          
        }

        #region  IToolControlObserver
        public void RegisterObeserver(IObserverResult observer)
        {
            mToolControlObserver.RegisterObeserver(observer);
        }

        public void Update(Dictionary<String, Object> bundle)
        {
        }

        public void NotifyObserver(Dictionary<String, Object> bundle)
        {
            mToolControlObserver.NotifyObserver(bundle);
        }
        # endregion 
        
        private void help_Click(object sender, RoutedEventArgs e)
        {
            home_Click(sender, e);
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            //Utils.NavigateToPage(MainWindow.sFrameReportName, Utils.uriPagePopup);  
            Button image = sender as Button;
            String name = image.GetValue(Button.NameProperty).ToString();
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add( PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE,name);

            PageDataExchange context = PageDataExchange.getInstance();
            IObserverResult pageHome = context.getResultObserverByTag(ToolTipHelper.TAG);

            if (pageHome != null)
            {
                pageHome.onRecieveResult(bundle);
            }

            if (name.Equals("home"))
            {
                Utils.NavigateToPage(MainWindow.sFrameReportName, PageHome.TAG);
            }
        }

         private void parameter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterMain.TAG);
            //Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);
        }

        private void popup_Menu_Click(object sender, RoutedEventArgs e)
        {
            Button image = sender as Button;
            String name = image.GetValue(Button.NameProperty).ToString();
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add( PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, name);

            int index;
            mMenuIndexMap.TryGetValue(name, out index);
            bundle.Add(ToolControlObserverImpl.KEY_MENU_INDEX, index);

            NotifyObserver(bundle);            
        }

        private void debug_Click(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageDebugMain.TAG);
        }

        private void menu_warning_Click(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageWarningRecord.TAG);           
        }
        private void menu_log_Click(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageRuntimeLog.TAG);
        }  
      
    }
}
