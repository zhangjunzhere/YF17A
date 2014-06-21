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
    public partial class PageDebugUp : Page,IObserverResult
    {
        public const string TAG = "PageDebugUp.xaml";

        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
         private BeckHoff mBeckHoff;
         
       
        public PageDebugUp()
        {
            InitializeComponent();     
            Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);
            InitStatusMap();
            mBeckHoff = Utils.GetBeckHoffInstance();

        }
                
        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_SETTING_SHOW);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);

            //init views
            foreach (String item in mStatusMap.Keys)
            {
                Object value;
                mBeckHoff.plcVarUserdataMap.TryGetValue(item, out value);
                UpdateView(item, value);
            }

          
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_SETTING_HIDE);

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
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarParameter.ACTION_HELP.Equals(senderValue))
                {

                }             
                else if (ToolbarParameter.ACTION_SETTING.Equals(senderValue))
                {
                   // ToolbarParameter.sBackPageStack.Push(TAG);
                    Utils.NavigateToPage(MainWindow.sFrameReportName, PageParameterUp.TAG);
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
        }

        private void UpdateView(String viewTag, Object value)
        {
            
           // mBeckHoff.plcVarTypeMap.TryGetValue(viewTag, out t);

            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
            Type t = element.GetType() ;

            if (t== typeof(Image))
            {
                Boolean status = (Boolean)value;
                Image iv = element as Image;

                if (status)
                {
                    iv.Source = MainWindow.sEnableLight;
                }
                else
                {
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

        private void   InitStatusMap()
        {
            mStatusMap.Add(".Sample_power", this.iv_Sample_power); //Sample_power	Bool	取样伺服主电源	Q06	只读	指示灯/报警条显示	I0.1
            mStatusMap.Add(".Corner_power", this.iv_Corner_power);  //Corner_power	Bool	弯道伺服主电源	Q07	只读	指示灯/报警条显示	I0.2
            mStatusMap.Add(".Lift_power", this.iv_Lift_power); //Lift_power	Bool	提升伺服主电源	Q08	只读	指示灯/报警条显示	I2.0
            mStatusMap.Add(".Transfer_power", this.iv_Transfer_power); //Transfer_power	Bool	传送伺服主电源	Q09	只读	指示灯/报警条显示	I2.7

            mStatusMap.Add(".Sample_servo_fault", this.iv_Sample_servo_fault); //Sample_servo_fault	Bool	取样伺服控制器故障	A16	只读	指示灯/报警条显示	I0.5
            mStatusMap.Add(".Corner_servo_fault", this.iv_Corner_servo_fault); //Corner_servo_fault	Bool	弯道伺服控制器故障	A17-1	只读	指示灯/报警条显示	I0.6
            mStatusMap.Add(".Lift_servo_fault", this.iv_Lift_servo_fault); //Lift_servo_fault	Bool	提升伺服控制器故障	A17-2	只读	指示灯	I4.4
            mStatusMap.Add(".Transfer_servo_fault", this.iv_Transfer_servo_fault); //Transfer_servo_fault	Bool	传送伺服控制器故障	A17-3	只读	指示灯	I4.5

            mStatusMap.Add(".sample_servo_initialized", this.iv_sample_servo_initialized); // sample_servo_initialized	Bool	取样驱动器初始完成		只读	指示灯	M1502.7
            mStatusMap.Add(".Corner_servo_initialized", this.iv_Corner_servo_initialized); //  Corner_servo_initialized	Bool	弯道驱动器初始完成		只读	指示灯	M1602.7  
            mStatusMap.Add(".Lift_servo_initialized", this.iv_Lift_servo_initialized); // Lift_servo_initialized	Bool	提升驱动器初始完成		只读	指示灯	M1902.7
            mStatusMap.Add(".Transfer_servo_initialized", this.iv_Transfer_servo_initialized); // Transfer_servo_initialized	Bool	传送驱动器初始完成		只读	指示灯	M2002.7

            mStatusMap.Add(".sample_servo_enable", this.iv_sample_servo_enable); // sample_servo_enable	Bool	取样驱动器使能		只读	指示灯	M1502.5
            mStatusMap.Add(".Corner_servo_enable", this.iv_Corner_servo_enable); //Corner_servo_enable	Bool	弯道驱动器使能		只读	指示灯	M1602.5
            mStatusMap.Add(".Slope_servo_enable", this.iv_Slope_servo_enable); //Slope_servo_enable	Bool	斜向驱动器使能		只读	指示灯	M1702.5
            mStatusMap.Add(".Transfer_servo_enable", this.iv_Transfer_servo_enable); //Transfer_servo_enable	Bool	传送驱动器使能		只读	指示灯	M2002.5

            mStatusMap.Add(".Sample_speed_rpm", this.tb_Sample_speed_rpm); //Sample_speed_rpm	Int	取样电机转速		只读	数值显示	MW1536
            mStatusMap.Add(".Corner_speed_rpm", this.tb_Corner_speed_rpm); // Corner_speed_rpm	Int	弯道电机转速		只读	数值显示	MW1636
            mStatusMap.Add(".Lift_speed_rpm", this.tb_Lift_speed_rpm); //Lift_speed_rpm	Int	提升电机转速		只读	数值显示	MW1936
            mStatusMap.Add(".Transfer_speed_rpm", this.tb_Transfer_speed_rpm); //Transfer_speed_rpm	Int	传送电机转速		只读	数值显示	MW2036
           
        }
    }
}


