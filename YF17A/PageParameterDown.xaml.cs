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
    /// Interaction logic for PageParamDown.xaml
    /// </summary>
    public partial class PageParameterDown : Page, IObserverResult
    {
        public const String TAG = "PageParameterDown.xaml";
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();
        private BeckHoff mBeckHoff;
        private PageParameterHelper mParameterHelper;

        public PageParameterDown()
        {
            InitializeComponent();
            //keyboard.registerObserver(this);

            InitStatusMap();
            mBeckHoff = Utils.GetBeckHoffInstance();

            mParameterHelper = new PageParameterHelper(this);
        }

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            //popup keyboard control
            Button button = sender as Button;
            Point pt1 = button.TranslatePoint(new Point(), this);         
            int X0 = (int)pt1.X;
            int Y0 =  (int)(pt1.Y + button.ActualHeight) ;

            if(X0 + keyboard.ActualWidth > this.ActualWidth)
            {
                X0 = (int)(this.ActualWidth - keyboard.ActualWidth);
            }
            if(Y0 + keyboard.ActualHeight > this.ActualHeight)
            {
                Y0 = Y0 -  (int)(keyboard.ActualHeight +button.ActualHeight) ;
            }

            keyboard.Visibility = Visibility.Visible;
            keyboard.UserControlToolTipX = X0;
            keyboard.UserControlToolTipY = Y0;

                        
            Dictionary<String, Object> bundle = new Dictionary<String, Object>();

            String controlName = button.GetValue(Button.NameProperty).ToString();
            String plcVarName = controlName.Replace("btn_",".");
            BeckHoff.ThresHold limit;
            mBeckHoff.plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
            if (limit != null)
            {
                bundle.Add(PageDataExchange.KEY_THREAD_HOLD, limit);
            }
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, controlName);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, button.Content);
            keyboard.onRecieveResult(bundle);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            keyboard.Visibility = Visibility.Hidden;
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
            bundle.TryGetValue( PageDataExchange.KEY_RESULT_VALUE, out resultValue);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            //ContentControl control = this.MainPanel.FindName(senderName.ToString()) as ContentControl;
            //control.Content = resultValue.ToString();

            //if (KeyboardControl.TAG.Equals(senderName))
            //{   
            //    String sourceControlName = senderValue.ToString();        
            //    String plcVarName = sourceControlName.Replace("btn_", ".");         
            //    mBeckHoff.writeInt(plcVarName, resultValue);
            //}
            //else 
            if (ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarParameter.ACTION_HELP.Equals(senderValue))
                {

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
            Type t;
            mBeckHoff.plcVarTypeMap.TryGetValue(viewTag, out t);
            //float fValue = Convert.ToSingle(value);
            //if (typeof(int) == t && mBeckHoff.plcVarThreadHoldMap.ContainsKey(viewTag))
            //{
            //    BeckHoff.ThresHold limit;
            //    mBeckHoff.plcVarThreadHoldMap.TryGetValue(viewTag, out limit);
            //    if (limit != null)
            //    {
            //        fValue = fValue / limit.ratio;
            //    }
            //}

            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
           

            if (element.GetType().Equals(typeof(Button)))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }
            else if (element.GetType().Equals(typeof(TextBlock)))
            {
                TextBlock tb = element as TextBlock;
                tb.Text = value.ToString();
            }            
        }
        #endregion

        private void InitStatusMap()
        {
            mStatusMap.Add(".Slope_cig_speed", this.tb_Slope_cig_speed);//Slope_cig_speed	Int	斜向电机烟支速度		只读	数值显示	MW1886 
            mStatusMap.Add(".Store_cig_speed", this.tb_Store_cig_speed);//Store_cig_speed	Int	存储电机烟支速度		只读	数值显示	MW1884          
            mStatusMap.Add(".Packer_cig_speed", this.tb_Packer_cig_speed);//Packer_cig_speed	Int	包装机烟支速度		只读	数值显示	MW1310      
            /***to do here  下降口反馈高度 and  存储手动速度 重复***/
            mStatusMap.Add(".DownPort_hight", this.btn_DownPort_hight);//DownPort_hight	Int	下降口反馈高度		只读	数值显示	DB8.DBW26
            mStatusMap.Add(".Downport_CigIn_hight1", this.btn_Downport_CigIn_hight1);//Downport_CigIn_hight1	Int	下降口存烟高度1		读/写	数值显示	DB8.DBW78
            mStatusMap.Add(".Store_CigIn_Comp_speed1", this.btn_Store_CigIn_Comp_speed1);//Store_CigIn_Comp_speed1	Int	存储高度1补偿速度		读/写	数值显示	DB8.DBW52
            mStatusMap.Add(".Downport_CigIn_hight2", this.btn_Downport_CigIn_hight2);//Downport_CigIn_hight2	Int	下降口存烟高度2		读/写	数值显示	DB8.DBW56
            mStatusMap.Add(".Store_CigIn_Comp_speed2", this.btn_Store_CigIn_Comp_speed2);//Store_CigIn_Comp_speed2	Int	存储高度2补偿速度		读/写	数值显示	DB8.DBW54
            mStatusMap.Add(".Downport_CigIn_lowest_hight", this.btn_Downport_CigIn_lowest_hight);//Downport_CigIn_lowest_hight	Int	存烟最低工作高度		读/写	数值显示	DB8.DBW92
            mStatusMap.Add(".Downport_CigOut_hight1", this.btn_Downport_CigOut_hight1);//Downport_CigOut_hight1	Int	下降口排烟高度1		读/写	数值显示	DB8.DBW80

            mStatusMap.Add(".Store_CigOut_Comp_speed1", this.btn_Store_CigOut_Comp_speed1);//Store_CigOut_Comp_speed1	Int	排烟高度1补偿速度		读/写	数值显示	DB8.DBW84
            mStatusMap.Add(".Downport_CigOut_hight2", this.btn_Downport_CigOut_hight2);//Downport_CigOut_hight2	Int	下降口排烟高度2		读/写	数值显示	DB8.DBW82
            mStatusMap.Add(".Store_CigOut_Comp_speed2", this.btn_Store_CigOut_Comp_speed2);//Store_CigOut_Comp_speed2	Int	排烟高度2补偿速度		读/写	数值显示	DB8.DBW86
            mStatusMap.Add(".Downport_comp_output", this.btn_Downport_comp_output);//Downport_comp_output	Int	下降口控制补偿输出		只读	数值显示	MW1250
            mStatusMap.Add(".Downport_Highest_limit", this.btn_Downport_Highest_limit);//Downport_Highest_limit	Int	下降口传感器上限		读/写	数值显示	DB8.DBW88
            mStatusMap.Add(".Downport_Lowest_limit", this.btn_Downport_Lowest_limit);//Downport_Lowest_limit	Int	下降口传感器下限		读/写	数值显示	DB8.DBW90
            mStatusMap.Add(".Downport_sensor_output", this.btn_Downport_sensor_output);//Downport_sensor_output	Int	下降口传感器输出		只读	数值显示	MW182
            mStatusMap.Add(".Store_manual_speed", this.btn_Store_manual_speed);//Store_manual_speed	Int	存储电机手动速度		读/写	数值显示	MW260    
            mStatusMap.Add(".Store_empty_position", this.btn_Store_empty_position);//Store_empty_position	Int	存储空位置		读/写	数值显示	DB8.DBW64
            mStatusMap.Add(".Store_full_position", this.btn_Store_full_position);//Store_full_position	Int	存储满位置		读/写	数值显示	DB8.DBW66

            mStatusMap.Add(".Maker_stop_position", this.btn_Maker_stop_position);//Maker_stop_position	Int	卷烟机停止位置		读/写	数值显示	DB8.DBW72
            mStatusMap.Add(".Packer_enable_position", this.btn_Packer_enable_position);//Packer_enable_position	Int	包装机可启动位置		读/写	数值显示	DB8.DBW70
            mStatusMap.Add(".Packer_LowSpeed_position", this.btn_Packer_LowSpeed_position);//	Packer_LowSpeed_position	Int	包装机低速位置		读/写	数值显示	DB8.DBW68


            FrameworkElement panel_sudu = this.tooltip.FindName("panel_sudu") as Grid;
            panel_sudu.Visibility = Visibility.Hidden;
        }
    }
}
