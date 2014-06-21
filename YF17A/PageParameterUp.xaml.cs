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
    /// Interaction logic for PageParamUp.xaml
    /// </summary>
    public partial class PageParameterUp : Page,IObserverResult
    {
        public const string TAG = "PageParameterUp.xaml";
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;
        private PageParameterHelper mParameterHelper;
        public PageParameterUp()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName,ToolbarParameter.TAG);
            //keyboard.registerObserver(this);

            InitStatusMap();
            mBeckHoff = Utils.GetBeckHoffInstance();
            mParameterHelper = new PageParameterHelper(this);
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            context.CommandToolbarParamter(TAG, ToolbarParameter.ACTIONBAR_REGISTER);
             //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);
            mParameterHelper.Loaded(sender, e);
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
            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
            mParameterHelper.Unloaded(sender, e);
        }
        #endregion      

        #region IActionBar members
        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object resultValue;
            Object senderValue;

            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_RESULT_VALUE, out resultValue);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

             //beckhoff changed 
            if (BeckHoff.TAG.Equals(senderName))
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
            Type t;
            mBeckHoff.plcVarTypeMap.TryGetValue(viewTag, out t);
            
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);         

            if (element.GetType() == typeof(Button))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }
            else if (element.GetType() == typeof(TextBlock))
            {
                TextBlock tb = element as TextBlock;
                tb.Text = value.ToString();
            }
        }
        #endregion

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
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


          private void InitStatusMap()
          {
              mStatusMap.Add(".Sample_cig_speed", this.tb_Sample_cig_speed); //Sample_cig_speed	Int	取样电机烟支速度		只读	数值显示	MW1302
              mStatusMap.Add(".Corner_cig_speed", this.tb_Corner_cig_speed); //Corner_cig_speed	Int	弯道电机烟支速度		只读	数值显示	MW1304
              mStatusMap.Add(".Maker_cig_speed", this.tb_Maker_cig_speed); //Maker_cig_speed	Int	卷烟机烟支速度		只读	数值显示	MW1300
              mStatusMap.Add(".Life_cig_speed", this.tb_Life_cig_speed); //提升电机烟支速度（MW1400）Life_cig_speed	
              mStatusMap.Add(".Transfer_cig_speed", this.tb_Transfer_cig_speed);//传送电机烟支速度（MW1402）Transfer_cig_speed	
              mStatusMap.Add(".MakerExport_cig_speed", this.tb_MakerExport_cig_speed);//烟机出口电机速度（MW1404）MakerExport_cig_speed
         
              mStatusMap.Add(".Corner_pid_sp", this.btn_Corner_pid_sp); //Corner_pid_sp	Int	弯道高度设定SP		读/写	数值显示	DB8.DBW0
              mStatusMap.Add(".Corner_pid_pv", this.btn_Corner_pid_pv); //Corner_pid_pv	Int	弯道高度反馈		只读	数值显示	DB8.DBW2
              mStatusMap.Add(".Corner_pid_p_gain", this.btn_Corner_pid_p_gain); //Corner_pid_p_gain	Int	弯道控制增益		读/写	数值显示	DB8.DBW4
             mStatusMap.Add(".Corner_pid_i_gain", this.btn_Corner_pid_i_gain); //Corner_pid_i_gain	Int	弯道控制积分时间		读/写	数值显示	DB8.DBW6                   
              mStatusMap.Add(".Corner_pid_deadband", this.btn_Corner_pid_deadband); //Corner_pid_deadband	Int	弯道控制不敏区		读/写	数值显示	DB8.DBW8
              mStatusMap.Add(".Corner_pid_output", this.btn_Corner_pid_output); //Corner_pid_output	Int	弯道控制补偿输出		只读	数值显示	DB8.DBW10
              mStatusMap.Add(".Corner_work_limit", this.btn_Corner_work_limit); //Corner_work_limit	Int	弯道最小烟层高度		读/写	数值显示	DB8.DBW36
              mStatusMap.Add(".Corner_work_off_delay", this.btn_Corner_work_off_delay); //Corner_work_off_delay	Int	最小烟层高度滞后		读/写	数值显示	DB8.DBW38
              mStatusMap.Add(".Corner_p_parameter", this.btn_Corner_p_parameter); //Corner_p_parameter	Int	弯道速度比例系数		读/写	数值显示	DB8.DBW40
              mStatusMap.Add(".Corner_entrance_hight_limit", this.btn_Corner_entrance_hight_limit); //Corner_entrance_hight_limit	Int	弯道入口传感器上限		读/写	数值显示	DB8.DBW74
              mStatusMap.Add(".Corner_entrance_low_limt", this.btn_Corner_entrance_low_limt); //Corner_entrance_low_limt	Int	弯道入口传感器下限		读/写	数值显示	DB8.DBW76
              mStatusMap.Add(".Corner_manual_speed", this.btn_Corner_manual_speed); //Corner_manual_speed	Int	弯道电机手动速度		读/写	数值显示	MW254
              mStatusMap.Add(".Corner_entrance_sensor_output", this.btn_Corner_entrance_sensor_output); //Corner_entrance_sensor_output	Int	弯道入口传感器输出		只读	数值显示	MW180

              mStatusMap.Add(".Lift_p_parameter", this.btn_Lift_p_parameter);//提升速度比例系数（MW270）Lift_p_parameter
              mStatusMap.Add(".Transfer_p_parameter", this.btn_Transfer_p_parameter);//传送速度比例系数（MW272）Transfer_p_parameter
              mStatusMap.Add(".MakerExport_p_parameter", this.btn_MakerExport_p_parameter);//MakerExport_p_parameter	Int	烟机出口速度比例系数

              FrameworkElement panel_sudu = this.tooltip.FindName("panel_sudu") as Grid;
              panel_sudu.Visibility = Visibility.Hidden;
          }
     }
}


