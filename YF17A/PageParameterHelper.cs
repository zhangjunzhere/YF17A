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

namespace YF17A
{
    class PageParameterHelper:IObserverResult
    {
        public const string TAG = "PageParameterHelper.cs";

        private Boolean mHelpState ;         
        private Dictionary<String, String> mStatusMap = new Dictionary<string, String>(); //key: plcVarName,  value: help info
        private TooltipControl tooltip = new TooltipControl();
        private FrameworkElement panel;

        public PageParameterHelper(FrameworkElement panel)
        {
            this.panel = panel;

            InitStatusMap();            
            //TooltipControl tooltip = new TooltipControl();
            Grid rootView =  panel.FindName("root") as Grid;
            rootView.Children.Add(tooltip);
            tooltip.registerObserver(this);
            tooltip.Visibility = Visibility.Hidden;
            panel.PreviewMouseUp +=new MouseButtonEventHandler(panel_PreviewMouseUp);
            tooltip.MouseUp +=new MouseButtonEventHandler(tooltip_MouseUp);            
        }

        public void Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);
            mHelpState = false;     
        }

        public void Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }       

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (ToolbarMain.TAG.Equals(senderName) || ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarMain.BUTTON_HELP.Equals(senderValue))
                {
                    mHelpState = !mHelpState;
                }
                //else if (ToolbarMain.BUTTON_HOME.Equals(senderValue))
                //{
                //    mHelpState = false;
                //}
                UpdateHelpState();
            }           
        }

        private void help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image helpIcon = sender as Image;
            Point pt1 = helpIcon.TranslatePoint(new Point(), panel);

            int X0 =  (int)(pt1.X );
            int Y0 = (int)(pt1.Y + helpIcon.ActualHeight) ;

            if (X0 + tooltip.ActualWidth > panel.ActualWidth)
            {
                X0 = (int)(panel.ActualWidth - tooltip.ActualWidth - helpIcon.ActualWidth);
            }
            if (Y0 + tooltip.ActualHeight > panel.ActualHeight)
            {
                Y0 = Y0 - (int)(tooltip.ActualHeight + helpIcon.ActualHeight);
            }

            tooltip.Visibility = Visibility.Visible;

            tooltip.UserControlToolTipX = X0;
            tooltip.UserControlToolTipY = Y0;

            Dictionary<String, Object> bundle = new Dictionary<String, Object>();
            //bundle.Add( PageDataExchange.KEY_SENDER_NAME, helpIcon.GetValue(Image.NameProperty).ToString());
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, helpIcon.ToolTip);
            tooltip.putExtra(bundle);
        }


        private void panel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tooltip.Visibility = Visibility.Hidden;
        }

        private void UpdateHelpState( )
        {
            Visibility visibility = Visibility.Hidden;
            if (mHelpState)
            {
                visibility = Visibility.Visible;
            }
            foreach (String entry in mStatusMap.Keys)
            {
                String helpIconName = entry.Replace(".", "iv_");
                Image img = panel.FindName(helpIconName) as Image;
                if (img != null)
                {
                    img.Visibility = visibility;
                }
            }

            if(!mHelpState){
                tooltip.Visibility = Visibility.Hidden;
            }            
        }      
      
       
        private void InitStatusMap()
        {	
        //烟支直径：	根据烟支规格指示书进行调整，设定范围为5.0mm到9.0mm。
            //上升口
            mStatusMap.Add(".Sample_cig_speed", "this.tb_Sample_cig_speed"); //Sample_cig_speed	Int	取样电机烟支速度		只读	数值显示	MW1302
            mStatusMap.Add(".Corner_cig_speed", "this.tb_Corner_cig_speed"); //Corner_cig_speed	Int	弯道电机烟支速度		只读	数值显示	MW1304
            mStatusMap.Add(".Maker_cig_speed", "this.tb_Maker_cig_speed"); //Maker_cig_speed	Int	卷烟机烟支速度		只读	数值显示	MW1300
            mStatusMap.Add(".Life_cig_speed", "this.tb_Life_cig_speed"); //提升电机烟支速度（MW1400）Life_cig_speed	
            mStatusMap.Add(".Transfer_cig_speed", "this.tb_Transfer_cig_speed");//传送电机烟支速度（MW1402）Transfer_cig_speed	
            mStatusMap.Add(".MakerExport_cig_speed", "this.tb_MakerExport_cig_speed");//烟机出口电机速度（MW1404）MakerExport_cig_speed

            mStatusMap.Add(".Corner_pid_sp", "提升高度设定值，正常工作设定范围30.0%到90.0%之间。"); //Corner_pid_sp	Int	弯道高度设定SP		读/写	数值显示	DB8.DBW0
            mStatusMap.Add(".Corner_pid_pv", "提升机入口传感器高度反馈值，0.0%对应传感器测量下限。100.00%对应传感器测量上限。"); //Corner_pid_pv	Int	弯道高度反馈		只读	数值显示	DB8.DBW2
            mStatusMap.Add(".Corner_pid_p_gain", "提升机高度补偿调节增益，增益越大反应越灵敏，增益越小反应越迟钝。增益太大或太小，系统都工作不稳定，一般设定工作范围0.15到5.00之间。"); //Corner_pid_p_gain	Int	弯道控制增益		读/写	数值显示	DB8.DBW4
            mStatusMap.Add(".Corner_pid_i_gain", "	提升机高度补偿调节积分参数，数值越大系统越稳定，但反应越迟缓，数值越小反应快，系统越不容易稳定。工作范围一般设定在范围10.00到100.00之间。"); //Corner_pid_i_gain	Int	弯道控制积分时间		读/写	数值显示	DB8.DBW6                   
            mStatusMap.Add(".Corner_pid_deadband", "控制不敏区，也叫控制死区，当设定高度和反馈高度之间的差小于死区时，系统不进行调整。一般设定工作范围0.15到5.00之间。"); //Corner_pid_deadband	Int	弯道控制不敏区		读/写	数值显示	DB8.DBW8
            mStatusMap.Add(".Corner_pid_output", "暂无帮助信息"); //Corner_pid_output	Int	弯道控制补偿输出		只读	数值显示	DB8.DBW10
            mStatusMap.Add(".Corner_work_limit", "	提升机在自动运转情况下，跟踪取样电机运转的最小高度。当烟层高度低于提最小高度值时取样电机运转但提升电机不运转。当烟层高度高于最小烟层高度时，提升电机根据入口高度传感器自动调节提升电机的运转速度，以稳定烟层高度。"); //Corner_work_limit	Int	弯道最小烟层高度		读/写	数值显示	DB8.DBW36
            mStatusMap.Add(".Corner_work_off_delay", "	提升电机在自动跟踪取样电机运转情况下，烟层高度低于最小烟层高度减去最小烟层高度滞后值时，提升电机停止运转。避免电机频繁的起停。"); //Corner_work_off_delay	Int	最小烟层高度滞后		读/写	数值显示	DB8.DBW38
            mStatusMap.Add(".Corner_p_parameter", "提升电机与取样电机速度之间的基本速度比率。提升电机在此基础上进行微量调整，来控制提升机入口烟层的高度。"); //Corner_p_parameter	Int	弯道速度比例系数		读/写	数值显示	DB8.DBW40
            mStatusMap.Add(".Corner_entrance_hight_limit", "提升入口传感器达到最大高度时模拟电压值。相对应传感器反馈高度值100.00%。"); //Corner_entrance_hight_limit	Int	弯道入口传感器上限		读/写	数值显示	DB8.DBW74
            mStatusMap.Add(".Corner_entrance_low_limt", "提升入口传感器达到最低高度时模拟电压值。相对应传感器反馈高度值0.00%。"); //Corner_entrance_low_limt	Int	弯道入口传感器下限		读/写	数值显示	DB8.DBW76
            mStatusMap.Add(".Corner_manual_speed", "暂无帮助信息"); //Corner_manual_speed	Int	弯道电机手动速度		读/写	数值显示	MW254
            mStatusMap.Add(".Corner_entrance_sensor_output", " 提升入口传感器的实际模拟电压输出值，一般最低位置模拟电压值为1000mV左右。最高位置值在3000mV到10000mV之间。在试车或大修时需要进行机械校对调整。"); //Corner_entrance_sensor_output	Int	弯道入口传感器输出		只读	数值显示	MW180

            mStatusMap.Add(".Lift_p_parameter", "暂无帮助信息");//提升速度比例系数（MW270）Lift_p_parameter
            mStatusMap.Add(".Transfer_p_parameter", "暂无帮助信息");//传送速度比例系数（MW272）Transfer_p_parameter
            mStatusMap.Add(".MakerExport_p_parameter", "暂无帮助信息");//MakerExport_p_parameter	Int	烟机出口速度比例系数

           
            /**  下降口参数帮助 ***/
            mStatusMap.Add(".DownPort_hight", "下降口高度传感器反馈值，0.00%对应传感器测量下限。100.00%对应传感器测量上限。");//DownPort_hight	Int	下降口反馈高度		只读	数值显示	DB8.DBW26
            mStatusMap.Add(".Downport_CigIn_hight1", "下降口高度传感器反馈值，0.00%对应传感器测量下限。100.00%对应传感器测量上限。");//Downport_CigIn_hight1	Int	下降口存烟高度1		读/写	数值显示	DB8.DBW78
            mStatusMap.Add(".Store_CigIn_Comp_speed1", "存储器正常存烟补偿速度，当下降口高度高于\"下降口存烟高度1\"时，控制器根据下降口高度进行自动调节存烟补偿速度。");//Store_CigIn_Comp_speed1	Int	存储高度1补偿速度		读/写	数值显示	DB8.DBW52
            mStatusMap.Add(".Downport_CigIn_hight2","当下降口高度大于该设定值时，存烟高度2补偿速度开始工作，主要用来提高系统的灵敏度。");//Downport_CigIn_hight2	Int	下降口存烟高度2		读/写	数值显示	DB8.DBW56
            mStatusMap.Add(".Store_CigIn_Comp_speed2", "当下降口高度大于下降口高度2设定值，存烟高度2补偿速度跟存烟高度1补偿速度一起对下降口高度进行控制。快速存储下降口处多余的烟支。");//Store_CigIn_Comp_speed2	Int	存储高度2补偿速度		读/写	数值显示	DB8.DBW54
            mStatusMap.Add(".Downport_CigIn_lowest_hight","低于该设定高度时，存储器禁止存储烟支，防止烟层高度过低时跌落烟支。");//Downport_CigIn_lowest_hight	Int	存烟最低工作高度		读/写	数值显示	DB8.DBW92
            mStatusMap.Add(".Downport_CigOut_hight1", "当包装机和卷烟机同时运转时，下降口高度低于该设定值时，存储器开始排烟速度补偿，高于该设定值时，存储器禁止排出烟支，防止斜向排烟不畅造成堵烟。");//Downport_CigOut_hight1	Int	下降口排烟高度1		读/写	数值显示	DB8.DBW80

            mStatusMap.Add(".Store_CigOut_Comp_speed1", "包装机和卷烟机同时运行时，对存储器排烟速度进行调节，增大该数值使反应灵敏度提高，但速度过大容易造成存储器入口堵烟。");//Store_CigOut_Comp_speed1	Int	排烟高度1补偿速度		读/写	数值显示	DB8.DBW84
            mStatusMap.Add(".Downport_CigOut_hight2","当只有包装机运转时，下降口高度低于该设定值时，存储器开始排烟速度补偿。高于该设定值时，存储器禁止排出烟支，防止斜向排烟不畅造成堵烟。");//Downport_CigOut_hight2	Int	下降口排烟高度2		读/写	数值显示	DB8.DBW82
            mStatusMap.Add(".Store_CigOut_Comp_speed2", "当只有包装机运转时的排烟补偿速度，补偿速度太大会因斜向排烟不畅造成堵烟。");//Store_CigOut_Comp_speed2	Int	排烟高度2补偿速度		读/写	数值显示	DB8.DBW86
            mStatusMap.Add(".Downport_comp_output", "控制器对下降口高度进行调节控制的输出值，正值为存烟补偿，负值为排烟补偿。");//Downport_comp_output	Int	下降口控制补偿输出		只读	数值显示	MW1250
            mStatusMap.Add(".Downport_Highest_limit", "存储器下降口最大高度时传感器的模拟电压值。相对应传感器反馈高度值100.00%。");//Downport_Highest_limit	Int	下降口传感器上限		读/写	数值显示	DB8.DBW88
            mStatusMap.Add(".Downport_Lowest_limit", "存储器下降口最低高度时传感器的模拟电压值。相对应传感器反馈高度值0.00%。");//Downport_Lowest_limit	Int	下降口传感器下限		读/写	数值显示	DB8.DBW90
            mStatusMap.Add(".Downport_sensor_output", "存储器下降口传感器的实际模拟电压输出值，一般最低位置模拟电压值为1000mV左右。最高位置值在3000mV到10000mv之间。在试车或大修时需要进行机械校对调整。");//Downport_sensor_output	Int	下降口传感器输出		只读	数值显示	MW182
            mStatusMap.Add(".Store_manual_speed", "暂无帮助信息");//Store_manual_speed	Int	存储电机手动速度		读/写	数值显示	MW260    
            mStatusMap.Add(".Store_empty_position", "当存储器的烟支存储量%低于设定数值，存储器空信号发出。设定范围：1.0%到10.0%之间");//Store_empty_position	Int	存储空位置		读/写	数值显示	DB8.DBW64
            mStatusMap.Add(".Store_full_position", "当存储器的烟支存储量%高于设定数值，存储器满信号发出。设定范围：90.0%到99.0%之间。");//Store_full_position	Int	存储满位置		读/写	数值显示	DB8.DBW66

            mStatusMap.Add(".Maker_stop_position", "当存储器的烟支存储量%高于设定数值，卷烟机停机信号发出。设定范围：90.0%到98.0%之间。");//Maker_stop_position	Int	卷烟机停止位置		读/写	数值显示	DB8.DBW72
            mStatusMap.Add(".Packer_enable_position","当存储器的烟支存储量%高于设定数值，包装机可启动信号发出。设定范围：2.0%到10.0%之间。");//Packer_enable_position	Int	包装机可启动位置		读/写	数值显示	DB8.DBW70
            mStatusMap.Add(".Packer_LowSpeed_position", "当存储器的烟支存储量%高于设定数值，包装机低速信号发出。设定范围：5.0%到15.0%之间。");//	Packer_LowSpeed_position	Int	包装机低速位置		读/写	数值显示	DB8.DBW68

            foreach (KeyValuePair<String, String>  entry in mStatusMap)
            {
                String key = entry.Key;                
                String controlName = key.Replace(".", "iv_");

                Image helpIcon = panel.FindName(controlName) as Image;
                if (helpIcon != null)
                {
                    helpIcon.ToolTip = entry.Value;
                    helpIcon.Visibility = Visibility.Hidden;
                    helpIcon.MouseUp += new MouseButtonEventHandler(help_MouseUp);
                }
            }
        }

        private void tooltip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mHelpState = false;
            UpdateHelpState();
            PageDataExchange context = PageDataExchange.getInstance();
            context.CommandObserver(ToolTipHelper.TAG, ToolbarParameter.TAG, ToolbarMain.BUTTON_HELP);
        }
    }
}

