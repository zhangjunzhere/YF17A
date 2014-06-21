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
    /// Interaction logic for PageDebug.xaml
    /// </summary>
    public partial class PageDebugMain : Page, IObserverResult
    {
        public const string TAG = "PageDebugMain.xaml" ;
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;

       
        public PageDebugMain()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);

            InitStatusMap();
            mBeckHoff = Utils.GetBeckHoffInstance();
        }

        private void bucket_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName,PageDebugBucket.TAG);
        }

        private void elevator_Click(object sender, RoutedEventArgs e)
        {
            //ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageDebugUp.TAG);
        }

        private void console_Click(object sender, RoutedEventArgs e)
        {
            ToolbarParameter.sBackPageStack.Push(TAG);
            Utils.NavigateToPage(MainWindow.sFrameReportName, PageDebugConsole.TAG);
        }


        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);
            //init views
            foreach (String item in mStatusMap.Keys)
            {
                Object value;
                mBeckHoff.plcVarUserdataMap.TryGetValue(item, out value);
                UpdateView(item, value);
            }

            //FrameworkElement panel_test_run = this.tooltip.panel_test_run") as FrameworkElement;
            Utils.ShowPrevilageControl(this.panel_test_run);
            Utils.ShowPrevilageControl(btn_test_run_unprotected);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarParameter.ACTION_HELP.Equals(senderValue))
                {

                }             
                else if (ToolbarParameter.ACTION_SETTING.Equals(senderValue))
                {
                    Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterDown.TAG);
                }
            }
            //beckhoff changed 
            else if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                Object plcValue;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);

                if (mStatusMap.ContainsKey(plcVarName))
                {
                    UpdateView(plcVarName, plcValue);
                }
            }
            else if (User.TAG.Equals(senderName))
            {
                Utils.ShowPrevilageControl(this.panel_test_run);
                Utils.ShowPrevilageControl(btn_test_run_unprotected);
            }        
        }

        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
            Type t = element.GetType();

            if (t== typeof(Image))
            {
                Boolean status = (Boolean)value;       
                Image iv = element as Image;
                if (status) {
                    iv.Source = MainWindow.sEnableLight ;
                } else{
                    iv.Source = MainWindow.sDisableLight;
                }               
            }
            else if (t == typeof(Button))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }      
        }
        #endregion

        private void InitStatusMap()
        {
            mStatusMap.Add(".test_run", this.btn_test_run); //test_run	Bool	机器试运转		读/写	按钮/指示灯	M1000.6
            mStatusMap.Add(".test_maker_speed", this.btn_test_maker_speed); //test_maker_speed	Int	卷烟机试运转速度		读/写	数值显示	MW1950
            mStatusMap.Add(".test_packer_speed", this.btn_test_packer_speed); //test_packer_speed	Int	包装机试运转速度		读/写	数值显示	MW1952

            mStatusMap.Add(".Maker_MaxSpeedLimit", this.btn_Maker_MaxSpeedLimit);  //<!--卷烟机最大速度设定（DB8.DBW60）Maker_MaxSpeedLimit-->
            mStatusMap.Add(".Packer_MaxSpeedLimit", this.btn_Packer_MaxSpeedLimit);  //<!--包装机最大速度设定（DB8.DBW62）Packer_MaxSpeedLimit-->

            mStatusMap.Add(".cig_dim", this.btn_cig_dim);  //烟支直径（DB8.DBW46 cig_dim           
            mStatusMap.Add(".test_run_unprotected", this.btn_test_run_unprotected);  // test_run_unprotected	Bool	机器强制试运转		读/写	按钮/指示灯	M1000.7
        }

        private void btn_test_run_Click(object sender, RoutedEventArgs e)
        {            
            Button btn = sender as Button;
            String sourceControlName = btn.GetValue(Button.NameProperty).ToString();
            String plcVarName = sourceControlName.Replace("btn_", ".");
            Boolean bValue = true;
            mBeckHoff.writeBoolean(plcVarName, bValue);
        }

        private void btn_setting_click(object sender, RoutedEventArgs e)
        {
            //popup keyboard control
            Button button = sender as Button;
            Point pt1 = button.TranslatePoint(new Point(), this);
            int X0 = (int)pt1.X;
            int Y0 = (int)(pt1.Y + button.ActualHeight);

            if (X0 + keyboard.ActualWidth > this.ActualWidth)
            {
                X0 = (int)(this.ActualWidth - keyboard.ActualWidth);
            }
            if (Y0 + keyboard.ActualHeight > this.ActualHeight)
            {
                Y0 = Y0 - (int)(keyboard.ActualHeight + button.ActualHeight);
            }

            keyboard.Visibility = Visibility.Visible;
            keyboard.UserControlToolTipX = X0;
            keyboard.UserControlToolTipY = Y0;


            Dictionary<String, Object> bundle = new Dictionary<String, Object>();

            String controlName = button.GetValue(Button.NameProperty).ToString();
            String plcVarName = controlName.Replace("btn_", ".");
            BeckHoff.ThresHold limit;
            mBeckHoff.plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
            if (limit != null)
            {
                bundle.Add(PageDataExchange.KEY_THREAD_HOLD, limit);
            }
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, controlName);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, button.Content);
            keyboard.onRecieveResult(bundle);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            keyboard.Visibility = Visibility.Hidden;
        }

    }
}


