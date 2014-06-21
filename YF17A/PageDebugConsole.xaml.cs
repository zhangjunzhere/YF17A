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
    /// 
    public partial class PageDebugConsole : Page, IObserverResult
    {
        public const string TAG = "PageDebugConsole.xaml";
        private Dictionary<String, StatusInfo> mStatusMap = new Dictionary<string, StatusInfo>();
        
        private int mPageIndex = 0;
        StatusInfo[][] mStatusInfosAllPage = new StatusInfo[][] { 
                                                                                                StatusInfo.IQStatusInfos0, 
                                                                                                StatusInfo.IQStatusInfos1, 
                                                                                                StatusInfo.IQStatusInfos2, 
                                                                                                StatusInfo.IQStatusInfos3,                                                                                                 
                                                                                                StatusInfo.IQStatusInfos4
                                                                                            };

        private BeckHoff mBeckHoff;

        public PageDebugConsole()
        {
            InitializeComponent();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
            InitStatusMap();
            UpdateStatusLayout();
            mBeckHoff = Utils.GetBeckHoffInstance();
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
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);


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
            Boolean status = (Boolean)value;         
            StatusInfo info;
            mStatusMap.TryGetValue(viewTag, out info);
            info.Enabled = status;            
            io_VariableList.Items.Refresh();
        }
        #endregion


        private void btn_part2_Click(object sender, RoutedEventArgs e)
        {
            if (mPageIndex < mStatusInfosAllPage.Length - 1)
            {
                mPageIndex++;
            }
            UpdateStatusLayout();
        }

        private void btn_part1_Click(object sender, RoutedEventArgs e)
        {
            if (mPageIndex > 0)
            {
                mPageIndex--;
            }
            UpdateStatusLayout();
        }

        private void UpdateStatusLayout()
        {
            this.btn_part1.IsEnabled = true;
            this.btn_part2.IsEnabled = true;

            if (mPageIndex == 0)
            {
                this.btn_part1.IsEnabled = false;
            }
            else if (mPageIndex == mStatusInfosAllPage.Length - 1)
            {
                this.btn_part2.IsEnabled = false;
            }

            io_VariableList.ItemsSource = mStatusInfosAllPage[mPageIndex];
            io_VariableList.Items.Refresh();
            // String pageIndictor =
            String status = "输入";
            if(mPageIndex >= 3){
                status = "输出";
            }

            String title = String.Format("YF17 控制柜{0}状态", status);
            this.tb_console_input_status.Text = title;
            this.tb_page_indictor.Text = (mPageIndex + 1) + "/" + mStatusInfosAllPage.Length;
        }

   

        public class StatusInfo
        {           
            public String Title { get; set; }
            public String PlcName { get; set; }
            public Boolean Enabled { get; set; }

            public static StatusInfo[] IQStatusInfos0 = new StatusInfo[]
            {
                ///******  I0.0- I0.7 *************/
                new StatusInfo(){PlcName=".Emergency_stop",  Title="I0.0 紧急停止"},
                new StatusInfo(){PlcName=".MakerExit_power", Title= "I0.1 备用"},
                new StatusInfo(){PlcName=".Sample_power", Title= "I0.2 取样伺服主电源 "},
                new StatusInfo(){PlcName=".Corner_power",  Title="I0.3 弯道伺服主电源 "},
                new StatusInfo(){PlcName=".Lift_power",  Title="I0.4 提升伺服主电源"},
                new StatusInfo(){PlcName=".Transfer_power", Title="I0.5 传送伺服主电源"},
                new StatusInfo(){PlcName=".Slope_power", Title="I0.6 斜向伺服主电源 "},
                new StatusInfo(){PlcName=".Store_power", Title="I0.7 存储伺服主电源 "},      
                //Emergency_stop AT %IX0.0:BOOL;(*紧急停止继电器*)
                //MakerExit_power AT %IX0.1:BOOL;(*备用*)
                //Sample_power AT %IX0.2:BOOL;(*取样伺服主电源*)
                //Corner_power AT %IX0.3:BOOL;(*弯道伺服主电源*)
                //Lift_power AT %IX0.4:BOOL;(*提升伺服主电源*)
                //Transfer_power AT %IX0.5:BOOL;(*传送伺服主电源*)
                //Slope_power AT %IX0.6:BOOL;(*斜向伺服主电源*)
                //Store_power AT %IX0.7:BOOL;(*存储伺服主电源*)
            
                  // /******  I1.0- I1.7 *************/
                new StatusInfo(){PlcName=".DropTrans_power", Title="I1.0 下降口输送伺服主电源"},
                new StatusInfo(){PlcName=".Spare11", Title="I1.1 备用"},
                new StatusInfo(){PlcName=".MakerExit_servo_fault", Title="I1.2 备用"},
                new StatusInfo(){PlcName=".Sample_servo_fault", Title="I1.3 取样伺服驱动异常"},
                new StatusInfo(){PlcName=".Corner_servo_fault", Title="I1.4 弯道伺服驱动异常"},
                new StatusInfo(){PlcName=".Lift_servo_fault", Title="I1.5 提升伺服驱动异常"},
                new StatusInfo(){PlcName=".Transfer_servo_fault", Title="I1.6 传送伺服驱动异常"},
                new StatusInfo(){PlcName=".Slope_servo_fault", Title="I1.7 斜向伺服驱动异常"},       
                    //Spare10 AT %IX1.0:BOOL;(*备用*)
                //Spare11 AT %IX1.1:BOOL;(*备用*)
                //MakerExit_servo_fault AT %IX1.2:BOOL;(*备用*)
                //Sample_servo_fault AT %IX1.3:BOOL;(*取样伺服控制器故障*)
                //Corner_servo_fault AT %IX1.4:BOOL;(*弯道伺服控制器故障*)
                //Lift_servo_fault AT %IX1.5:BOOL;(*提升伺服控制器故障*)
                //Transfer_servo_fault AT %IX1.6:BOOL;(*传送伺服控制器故障*)
                //Slope_servo_fault AT %IX1.7:BOOL;(*斜向伺服控制器故障*)
                // /******  I2.0- I2.7 *************/
                new StatusInfo(){PlcName=".Store_servo_fault", Title="I2.0 存储伺服控制器故障"},
                new StatusInfo(){PlcName=".DropTrans_servo_fault", Title="I2.1 下降口输送驱动器准备好"},  
                new StatusInfo(){PlcName=".Spare22", Title="I2.2 备用"}, 
                new StatusInfo(){PlcName=".Elevater_man_auto_sw", Title="I2.3 备用"},	  
                //Store_servo_fault AT %IX2.0:BOOL;(*存储伺服控制器故障*)
                //Spare21 AT %IX2.1:BOOL;(*备用*)
                //Spare22 AT %IX2.2:BOOL;(*备用*)
                //Elevater_man_auto_sw AT %IX2.3:BOOL;(*备用*)                
             };

            public static StatusInfo[] IQStatusInfos1 = new StatusInfo[]{
                new StatusInfo(){PlcName=".Elevater_start_pb", Title="I2.4 备用"},	
                new StatusInfo(){PlcName=".DropTransLowLevel",Title="I2.5 下降口输送低料位"},	
                new StatusInfo(){PlcName=".DropTransHighLevel", Title="I2.6 下降口输送高料位"},
                new StatusInfo(){PlcName=".DropTransLimitLevel", Title="I2.7 下降口输送极限料位"},
               
                // /******  I3.0- I3.7 *************/          
                new StatusInfo(){PlcName=".Spare30", Title="I3.0 备用"},  
                new StatusInfo(){PlcName=".Spare31", Title="I3.1 备用"}, 
                new StatusInfo(){PlcName=".MakerExit_sensor", Title="I3.2 卷烟机出口有烟(备用)"}, 
                new StatusInfo(){PlcName=".Sample_entrance_sensor", Title="I3.3 取样入口有烟传感器"},
                new StatusInfo(){PlcName=".Sample_entrance_jam_sensor",Title="I3.4 取样入口堵塞传感器(备用)"},
                new StatusInfo(){PlcName=".Corner_entrance_jam_sensor", Title="I3.5 弯道入口堵塞传感器"},
                new StatusInfo(){PlcName=".MakerExit_jam_sensor", Title="I3.6 卷烟机出口堵塞(备用)"},
                new StatusInfo(){PlcName=".Spare37", Title="I3.7 备用"},          
                //Spare30 AT %IX3.0:BOOL;(*备用*)
                //Spare31 AT %IX3.1:BOOL;(*备用*)
                //MakerExit_sensor AT %IX3.2:BOOL;(*卷烟机出口有烟(备用)*)
                //Sample_entrance_sensor AT %IX3.3:BOOL;(*取样入口有烟传感器*)
                //Sample_entrance_jam_sensor AT %IX3.4:BOOL;(*取样入口堵塞传感器（备用）*)
                //Corner_entrance_jam_sensor AT %IX3.5:BOOL;(*弯道入口堵塞传感器*)
                //MakerExit_jam_sensor AT %IX3.6:BOOL;(*卷烟机出口堵塞（备用）*)
                //Spare37 AT %IX3.7:BOOL;(*备用*)
                 
                // /******  I4.0- I4.7 *************/
                new StatusInfo(){PlcName=".Downport_jam_sensor", Title="I4.0 下降口堵塞传感器"},
                new StatusInfo(){PlcName=".Slope_empty", Title="I4.1 斜向通道空"}, 
                new StatusInfo(){PlcName=".Transfer_cig_exist", Title="I4.2 高架烟支传感器"},
                new StatusInfo(){PlcName=".Transfer_overload_sensor",Title="I4.3 传送过载传感器"},
                new StatusInfo(){PlcName=".Spare44", Title="I4.4 备用"},
                new StatusInfo(){PlcName=".Spare45",Title="I4.5 备用"},
                new StatusInfo(){PlcName=".Spare46", Title="I4.6 备用"}, 
                new StatusInfo(){PlcName=".Spare47", Title="I4.7 备用"}, 
                 //Downport_jam_sensor AT %IX4.0:BOOL;(*下降口堵塞传感器*)
                //Slope_empty AT %IX4.1:BOOL;(*斜向通道空*)
                //Transfer_cig_exist AT %IX4.2:BOOL;(*高架烟支传感器*)
                //Transfer_overload_sensor AT %IX4.3:BOOL;(*传送过载传感器*)
                //Spare44 AT %IX4.4:BOOL;(*备用*)
                //Spare45 AT %IX4.5:BOOL;(*备用*)
                //Spare46 AT %IX4.6:BOOL;(*备用*)
                //Spare47 AT %IX4.7:BOOL;(*备用*)
            };


            public static StatusInfo[] IQStatusInfos2 = new StatusInfo[]{           
                //  /******  I5.0- I5.7 *************/
                new StatusInfo(){PlcName=".Store_full", Title="I5.0 存储器满传感器"},
                new StatusInfo(){PlcName=".Store_empty", Title="I5.1 存储器空传感器"},
                new StatusInfo(){PlcName=".Store_overload", Title="I5.2 存储器过载传感器"},
                new StatusInfo(){PlcName=".Store_entrance_cig_exist", Title="I5.3 存储器入口有烟传感器"},
                new StatusInfo(){PlcName=".Store_entrance_jam", Title="I5.4 存储器入口堵塞"}, 
                new StatusInfo(){PlcName=".Store_overlimit", Title="I5.5 存储器极限开关"}, 
                new StatusInfo(){PlcName=".Store_running", Title="I5.6 存储器运行中"}, 
                new StatusInfo(){PlcName=".Store_enabled", Title="I5.7 存储器使能"},    
                //Store_full AT %IX5.0:BOOL;(*存储器满传感器*)
                //Store_empty AT %IX5.1:BOOL;(*存储器空传感器*)
                //Store_overload AT %IX5.2:BOOL;(*存储器过载传感器*)
                //Store_entrance_cig_exist AT %IX5.3:BOOL;(*存储器入口有烟传感器*)
                //Store_entrance_jam AT %IX5.4:BOOL;(*存储器入口堵塞*)
                //Store_overlimit AT %IX5.5:BOOL;(*存储器极限开关*)
                //Store_running AT %IX5.6:BOOL;(*存储器运行中*)
                //Store_enabled AT %IX5.7:BOOL;(*存储器使能*)             
            };

            public static StatusInfo[] IQStatusInfos3 = new StatusInfo[]{   
                // /******  Q4.0- Q4.7 *************/
                  new StatusInfo(){PlcName=".sample_servo_enable_Q", Title="Q4.0 取样伺服驱动器使能"},
                  new StatusInfo(){PlcName=".Corner_servo_enable_Q", Title="Q4.1 弯道伺服驱动器使能"},
                  new StatusInfo(){PlcName=".Lift_servo_enable_Q", Title="Q4.2 提升伺服驱动器使能"},
                  new StatusInfo(){PlcName=".Transfer_servo_enable_Q", Title="Q4.3 传送伺服驱动器使能"},
                  new StatusInfo(){PlcName=".Slope_servo_enable_Q",Title="Q4.4 斜向伺服驱动器使能"},
                  new StatusInfo(){PlcName=".Store_servo_enable_Q", Title="Q4.5 存储伺服驱动器使能"},                  
                  new StatusInfo(){PlcName=".DropTrans_servo_enable_Q", Title="Q4.6 下降口输送伺服驱动器使能"}, 
                  new StatusInfo(){PlcName=".SpareOutput47", Title="Q4.7 备用"}, 
                     //sample_servo_enable_Q AT %QX4.0:BOOL;(*取样伺服驱动器使能*)
                //Corner_servo_enable_Q AT %QX4.1:BOOL;(*弯道伺服驱动器使能*)
                //Lift_servo_enable_Q AT %QX4.2:BOOL;(*提升伺服驱动器使能*)
                //Transfer_servo_enable_Q AT %QX4.3:BOOL;(*传送伺服驱动器使能*)
                //Slope_servo_enable_Q AT %QX4.4:BOOL;(*斜向伺服驱动器使能*)
                //Store_servo_enable_Q AT %QX4.5:BOOL;(*存储伺服驱动器使能*)
                //SpareOutput46 AT %QX4.6:BOOL;(*备用*)
                //SpareOutput47 AT %QX4.7:BOOL;(*备用*)               
             
                  // /******  Q5.0- Q5.7 *************/
                  new StatusInfo(){PlcName=".Elevater_start_Q", Title="Q5.0 备用"},
                  new StatusInfo(){PlcName=".Elevater_reset_Q", Title="Q5.1 备用"},
                  new StatusInfo(){PlcName=".Elevater_stop_Q", Title="Q5.2 备用"},
                  new StatusInfo(){PlcName=".Store_FaultReset_Q", Title="Q5.3 存储器故障复位"},
                  new StatusInfo(){PlcName=".Maker_enable_relay_Q",Title="Q5.4 卷烟机允许(备用)"},
                  new StatusInfo(){PlcName=".SpareOutput55", Title="Q5.5 下降口急停复位"}, 
                  new StatusInfo(){PlcName=".SpareOutput56", Title="Q5.6 备用"},                   
                  new StatusInfo(){PlcName=".Maker_QuickStop_Q",Title="Q5.7 卷烟机快停(备用)"},
                   //Elevater_start_Q AT %QX5.0:BOOL;(*备用*)
                //Elevater_reset_Q AT %QX5.1:BOOL;(*备用*)
                //Elevater_stop_Q AT %QX5.2:BOOL;(*备用*)
                //Store_FaultReset_Q AT %QX5.3:BOOL;(*存储器故障复位*)
                //Maker_enable_relay_Q AT %QX5.4:BOOL;(*卷烟机允许(备用)*)
                //SpareOutput55 AT %QX5.5:BOOL;(*下降口急停复位*)
                //SpareOutput56 AT %QX5.6:BOOL;(*备用*)
                //Maker_QuickStop_Q AT %QX5.7:BOOL;(*卷烟机快停（备用）*)                

                   // /******  Q80- Q8.7 *************/
                  new StatusInfo(){PlcName=".SpareOutput80", Title="Q8.0 存储振动检测"},    
                  new StatusInfo(){PlcName=".SpareOutput81", Title="Q8.1 备用"},    
                  new StatusInfo(){PlcName=".Packer_enable_relay_Q", Title="Q8.2 包装机允许(备用)"},
                  new StatusInfo(){PlcName=".Packer_LowSpeed_request_Q",Title="Q8.3 包装机低速请求(备用) "},
                  //SpareOutput80 AT %QX8.0:BOOL;(*存储振动检测*)
                //SpareOutput81 AT %QX8.1:BOOL;(*备用*)
                //Packer_enable_relay_Q AT %QX8.2:BOOL;(*包装机允许(备用)*)
                //Packer_LowSpeed_request_Q AT %QX8.3:BOOL;(*包装机低速请求（备用）*)               
            };

            public static StatusInfo[] IQStatusInfos4 = new StatusInfo[]{                   
                  new StatusInfo(){PlcName=".SpareOutput84", Title="Q8.4 备用"},    
                  new StatusInfo(){PlcName=".SpareOutput85", Title="Q8.5 系统主电源"},
                  new StatusInfo(){PlcName=".SpareOutput86", Title="Q8.6 备用"},                     
                  new StatusInfo(){PlcName=".StoreUnit_start_Q", Title="Q8.7 备用 "},
                 //SpareOutput84 AT %QX8.4:BOOL;(*备用*)
                //SpareOutput85 AT %QX8.5:BOOL;(*系统主电源*)
                //SpareOutput86 AT %QX8.6:BOOL;(*备用*)
                //StoreUnit_start_Q AT %QX8.7:BOOL;(*备用*)
               
                   // /******  Q9.0- Q9.7 *************/ 
                  new StatusInfo(){PlcName=".StoreUnit_reset_Q", Title="Q9.0 备用 "},
                  new StatusInfo(){PlcName=".StoreUnit_stop_Q", Title="Q9.1 备用 "},
                  new StatusInfo(){PlcName=".SpareOutput92", Title="Q9.2 备用"}, 
                  new StatusInfo(){PlcName=".SpareOutput93", Title="Q9.3 备用"},    
                  new StatusInfo(){PlcName=".SpareOutput94", Title="Q9.4 备用"},
                  new StatusInfo(){PlcName=".SpareOutput95", Title="Q9.5 备用"}, 
                  new StatusInfo(){PlcName=".SpareOutput96", Title="Q9.6 备用"},    
                  new StatusInfo(){PlcName=".SpareOutput97", Title="Q9.7 备用"},
                   //StoreUnit_reset_Q AT %QX9.0:BOOL;(*备用*)
                //StoreUnit_stop_Q AT %QX9.1:BOOL;(*备用*)
                //SpareOutput92 AT %QX9.2:BOOL;(*备用*)
                //SpareOutput93 AT %QX9.3:BOOL;(*备用*)
                //SpareOutput94 AT %QX9.4:BOOL;(*备用*)
                //SpareOutput95 AT %QX9.5:BOOL;(*备用*)
                //SpareOutput96 AT %QX9.6:BOOL;(*备用*)
                //SpareOutput97 AT %QX9.7:BOOL;(*备用*)
            };
        }
        
        private void InitStatusMap()
        {
           foreach (StatusInfo[] infos in mStatusInfosAllPage)
            {
                foreach (StatusInfo item in infos)
                {
                    item.Enabled = true;
                    mStatusMap.Add(item.PlcName, item);
                }
            }                  
        }
    }


    public class VBool2Image : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean enabled = (Boolean)value;
            BitmapImage bitmap = MainWindow.sDisableLight;
            if (enabled)
            {
                bitmap = MainWindow.sEnableLight;
            }

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }  
}

