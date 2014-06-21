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
    public partial class PageDebugBucket : Page, IObserverResult
    {
        public const string TAG = "PageDebugBucket.xaml";
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;
        
        public PageDebugBucket()
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
            mBeckHoff.UnregisterObserver(TAG);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_UNREGISTER);
            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_SETTING_HIDE);

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
                    //ToolbarParameter.sBackPageStack.Push(TAG);
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
        }
        #endregion

        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
            Type t = element.GetType();

            if (t == typeof(Image))
            {
                Boolean status = (Boolean)value;
                Image iv = element as Image;
                BitmapImage bmTrue = MainWindow.sEnableLight;
                BitmapImage bmFalse = MainWindow.sDisableLight;
                
                if(status) {
                    iv.Source = bmTrue;
                } else {
                    iv.Source = bmFalse;
                }               
            }
            else if (t == typeof(Button))
            {
                int lValue = (int)value;
                Button tv = element as Button;
                tv.Content = lValue.ToString();
            }
        }

        private void InitStatusMap()
        {
           //<!--驱动器主电源	I0.3   指示灯	I0.4  指示灯-->
            mStatusMap.Add(".Slope_power", this.iv_Slope_power); //<!-- Slope_power	Bool	斜向伺服主电源	Q10	只读	指示灯/报警条显示	I0.3-->
            mStatusMap.Add(".Store_power", this.iv_Store_power); //<!--Store_power	Bool	存储伺服主电源	Q11	只读	指示灯/报警条显示	I0.4-->               
            //<!--驱动器准备好	I0.7  指示灯	I1.0  指示灯-->     
            mStatusMap.Add(".Slope_servo_fault", this.iv_Slope_servo_fault);//<!-- Slope_servo_fault	Bool	斜向伺服控制器故障	A18	只读	指示灯/报警条显示	I0.7-->
            mStatusMap.Add(".Store_servo_fault", this.iv_Store_servo_fault);//<!--Store_servo_fault	Bool	存储伺服控制器故障	A19	只读	指示灯/报警条显示	I1.0-->           
            //<!--初始化完成	M1702.7 指示灯	M1702.7 指示灯-->
            mStatusMap.Add(".Slope_servo_initialized", this.iv_Slope_servo_initialized);//<!--Slope_servo_initialized	Bool	斜向驱动器初始完成		只读	指示灯	M1702.7-->          
            //<!--驱动器使能	M1702.5 指示灯	M1802.5 指示灯-->
            mStatusMap.Add(".Slope_servo_enable", this.iv_Slope_servo_enable);//<!--Slope_servo_enable	Bool	斜向驱动器使能		只读	指示灯	M1702.5-->
            mStatusMap.Add(".Store_servo_enable", this.iv_Store_servo_enable);//<!--Store_servo_enable	Bool	存储驱动器使能		只读	指示灯	M1802.5-->              
            //<!--伺服电机转速	MW1736  数值显示	MW1738  数值显示-->
            mStatusMap.Add(".Slope_speed_rpm", this.tb_Slope_speed_rpm);//<!--Slope_speed_rpm	Int	斜向电机转速		只读	数值显示	MW1736-->
            mStatusMap.Add(".Store_speed_rpm", this.tb_Store_speed_rpm);//<!--Store_speed_rpm	Int	存储电机转速		只读	数值显示	MW1738-->            

           // mStatusMap.Add(".Store_set_zero", this.iv_Store_set_zero); //<!-- 手动校零地址：M1000.0 Store_set_zero -->
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            String plcVarName = ".Store_set_zero";
            Boolean lValue = true;
            mBeckHoff.writeBoolean(plcVarName, lValue);  //<!-- 手动校零           
        }      
    }
}
