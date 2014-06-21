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
using YF17A.AccessDatabase;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageStatus.xaml
    /// </summary>
    public partial class PageStatus : Page,IObserverResult
    {
        public const string TAG = "PageStatus.xaml";
        public const string PATH = "PATH";
        public const string CODE = "CODE";
        DispatcherTimer tm = new DispatcherTimer();

        public PageStatus()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {          
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            tm.Tick += new EventHandler(tm_Tick);
            tm.Interval = TimeSpan.FromSeconds(1);
            tm.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            tm.Stop();
        }

        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;

            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (PATH.Equals(senderName))
            {              
                this.lb_path.Content = senderValue.ToString();
            }
            else if (CODE.Equals(senderName))
            {
                if (senderValue == null)
                {
                    this.lb_code.Content = "";
                }
                else
                {
                    this.lb_code.Content = String.Format("报警器件代码   {0}", senderValue.ToString());
                }
            }
            else if (PageLogin.LOGIN.Equals(senderValue) || PageLogin.LOGOUT.Equals(senderValue))
            {            
                User.Info usr = User.GetInstance().GetCurrentUserInfo();
                this.user.Text = usr.Account;
                this.role.Text = usr.Role;
            }          
        }

      
        #endregion

        private void tm_Tick(object sender, EventArgs e)
        {           
            this.lb_time.Content = DateTime.Now.ToString();
        }
    }
}
