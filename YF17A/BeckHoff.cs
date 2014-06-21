using System;
using System.Collections.Generic;
using System.Text;
using TwinCAT.Ads;
using System.Collections;
using System.Windows.Forms;
using YF17A.AccessDatabase;
using System.Threading;

namespace YF17A
{
    public  class BeckHoff
    {
         public const string TAG = "BeckHoff";

        private TcAdsClient adsClient;
        private int mWriteFlagHandle;

        private Dictionary<String, IObserverResult> mObserverCache = new Dictionary<string, IObserverResult>(); //key : page tag, value: page who implemet IObserverResult
        private List<String> plcVariableNames = new List<String>();
        private ArrayList notificationHandles = new ArrayList();
        private Dictionary<int, String> plcHNotifyVarMap = new Dictionary<int, String>(); //notificationHandle,varname;

        public Dictionary<String, int> plcVarHAccessMap = new Dictionary<string, int>(); //varname, rwHandle;
        public Dictionary<String, int> plcVarHNotifyMap = new Dictionary<string, int>(); //varname, notifyHandle;  
        public Dictionary<String, Type> plcVarTypeMap = new Dictionary<string, Type>(); //varname, type;       
        public Dictionary<String, Object> plcVarUserdataMap = new Dictionary<String, Object>(); //varname,value;
        public Dictionary<String, String> plcVarDescriptionMap = new Dictionary<String, String>(); //varname,description;


        private const String INT_TOKON = "I";
        private const String BOOL_TOKON = "B";

        public class ThresHold
        {
            private double _ratio = 1.0F;
            public int max {get;set;}
            public int min { get; set; }
            public double ratio
            {
                get { return _ratio; }
                set {_ratio = value;}
            }
       }

        public Dictionary<String, ThresHold> plcVarThreadHoldMap = new Dictionary<String, ThresHold>(); //varname,value;

        private HashSet<String> mvPlcVarSets = new HashSet<string>();
      
        public  BeckHoff(TcAdsClient adsClient)
        {
            this.adsClient = adsClient;
            mWriteFlagHandle = adsClient.CreateVariableHandle(".WriteFlag");

            foreach (String name in plcVariableNameTypes)
            {
                Type t = typeof(int);              
                if(name.StartsWith(BOOL_TOKON)){
                    t = typeof(Boolean);
                }

                String varName = name.Substring(1);
                plcVariableNames.Add(varName);
                plcVarTypeMap.Add(varName,t);
                createPlcAccessHandle(varName);              
            }
           
            initThreadHold();
            initVarDescription();

            //init mv plc var
            mvPlcVarSets.Add(".Corner_entrance_hight_limit");
            mvPlcVarSets.Add(".Corner_entrance_low_limt");
            mvPlcVarSets.Add(".Downport_Highest_limit");
            mvPlcVarSets.Add(".Downport_Lowest_limit");
            mvPlcVarSets.Add(".Downport_sensor_output");
            mvPlcVarSets.Add(".Corner_entrance_sensor_output");
        }

      

        public void RegisterObserver(String pageTag, IObserverResult observer)
        {
            mObserverCache.Add(pageTag, observer);
        }
        public void UnregisterObserver(String pageTag)
        {
            mObserverCache.Remove(pageTag);
        }


        private int createPlcAccessHandle(String plcVarName)
        {  
            try
            {
                int handle = adsClient.CreateVariableHandle(plcVarName);
                plcVarHAccessMap.Add(plcVarName, handle);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                System.Console.WriteLine(ex.Message);
            }

            return 0;
        }

         //add notifac
        public void addNotification(String plcVarName, Object userdata)
		{		
		   Type t ;
            plcVarTypeMap.TryGetValue(plcVarName, out t);              
            int NotificationHandle = adsClient.AddDeviceNotificationEx(plcVarName, AdsTransMode.OnChange, 100, 0, userdata, t);
            plcHNotifyVarMap.Add(NotificationHandle, plcVarName);

           // notificationHandles.Add(notifyHandle);
        }
         

        public void callback_Notification(AdsNotificationExEventArgs e)
        {        
            String plcVarName;
            plcHNotifyVarMap.TryGetValue(e.NotificationHandle, out plcVarName);
            Type t;
            plcVarTypeMap.TryGetValue(plcVarName, out t);

           
            Object value = e.Value;
            Console.WriteLine(plcVarName + "   " + e.Value.ToString());
            if (t.Equals(typeof(int)))
            {
                ThresHold limit;
                plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
                if (limit != null)
                {
                    double fValue = Convert.ToSingle(value);
                    fValue = fValue / limit.ratio ;

                    if (mvPlcVarSets.Contains(plcVarName))
                    {
                        fValue = (int)(fValue + 0.99);
                    }                    
                    value = fValue;
                }

                if (plcVarName.Equals(".Store_cig_speed") || plcVarName.Equals(".Slope_cig_speed"))
                {
                    //convert unsigned integer into integer
                    int lValue = (int)value;
                    value = lValue - ((lValue <= 32767) ? 0 : 65536); 
                }             
            }

            if (plcVarUserdataMap.ContainsKey(plcVarName))
            {
                plcVarUserdataMap.Remove(plcVarName);
            }           
            plcVarUserdataMap.Add(plcVarName,value);
            DispatchChanges(plcVarName);           
        }

        private void DispatchChanges(String plcVarName)
        {
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, TAG);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, plcVarName);

            //notify observers
            foreach (KeyValuePair<String, IObserverResult> item in mObserverCache)
            {               
                IObserverResult observer = item.Value;
                observer.onRecieveResult(bundle);
            }
        }


            //add notifac
        public void removeNotifications()
		{		
			try
			{
				foreach(int handle in notificationHandles)
					adsClient.DeleteDeviceNotification(handle);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			notificationHandles.Clear();
        }


        //write
        public object read(String plcVarName)
        {    
            int handle;            
            plcVarHAccessMap.TryGetValue(plcVarName, out handle);

            Type t;
            plcVarTypeMap.TryGetValue(plcVarName,out t);

            return  adsClient.ReadAny(handle, t).ToString();
        }

        public void writeAny(String plcVarName, Object value)
        {
            if (!User.GetInstance().GetCurrentUserInfo().IsLogin)
            {
                MessageBox.Show("您需要登录才能修改！");
                Dictionary<String, Object> bundle = new Dictionary<string, object>();
                bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
                PageDataExchange.getInstance().putExtra(PageUserRegister.TAG, bundle);
                Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);               
                return;
            }
            else if (".test_run_unprotected".Equals(plcVarName) && !(User.GetInstance().GetCurrentUserInfo().UserLevel >= User.USER_PREVILIDGE_DEBUGGER))
            {
                MessageBox.Show("您需要更高权限才能修改！");
                return;
            }
           
            try
            {              
                Object originalValue;
                int handle;
                plcVarUserdataMap.TryGetValue(plcVarName, out originalValue);              
                plcVarHAccessMap.TryGetValue(plcVarName, out handle);

                //begin write
                adsClient.WriteAny(mWriteFlagHandle, true);
                Thread.Sleep(10);
                adsClient.WriteAny(handle, value);
                Thread.Sleep(10);
                adsClient.WriteAny(mWriteFlagHandle, false);
                //end write

                String description;
                plcVarDescriptionMap.TryGetValue(plcVarName, out description);
                String content = String.Format("参数 {0} 被用户 {1} 从 {2} 修改为 {3}", description, User.GetInstance().GetCurrentUserInfo().Account, originalValue, value); 
                Log.write(Log.CATEGOTY_PARAMETER, content);                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        public void writeBoolean(String plcVarName, Object value)
        {
            Boolean bValue = (Boolean)value;
            writeAny(plcVarName, bValue);
        }

        public   void writeInt(String plcVarName, Object value)
        {
            try
            {
                double fValue =Convert.ToSingle(value);
                
                ThresHold limit;                
                plcVarThreadHoldMap.TryGetValue(plcVarName, out limit);
                if (limit != null  )
                {
                    fValue = fValue * limit.ratio;
                    if (fValue < limit.min || fValue > limit.max)
                    {
                        MessageBox.Show("参数超出范围!!!");
                        return;
                    } 
                }
                writeAny(plcVarName, (short)(fValue));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }           
        }

        public void readAll()
        {
            foreach (String name in plcVariableNames)
            {
                object obj = read(name);
                MessageBox.Show(name + "  " + obj.ToString());
            }
        }

        public void addAllNotifacations()
        {
            foreach (String name in plcVariableNames)
            {
                Type t;
                plcVarTypeMap.TryGetValue(name, out t);   

                Object userdata = null;
                int NotificationHandle = adsClient.AddDeviceNotificationEx(name, AdsTransMode.OnChange, 100, 0, userdata, t);

                plcHNotifyVarMap.Add(NotificationHandle, name);

                Console.WriteLine(name);
            }
        }

        public void AddUserData(String plcVarName, Object userdata)
        {
            plcVarUserdataMap.Add(plcVarName, userdata);
        }

        private void initThreadHold()
        {
            plcVarThreadHoldMap.Add(".Corner_pid_sp", new ThresHold() { max = 9000, min = 3000, ratio=100 });  //Int	弯道高度设定SP		读/写	数值显示	DB8.DBW0	Corner_pid_sp	%	90.0 	30.0 
            plcVarThreadHoldMap.Add(".Corner_work_limit", new ThresHold() { max = 7000, min = 2000,ratio=100 }); //Int	弯道最小烟层高度		读/写	数值显示	DB8.DBW36	Corner_work_limit	%	70.0 	20.0 
            plcVarThreadHoldMap.Add(".Corner_work_off_delay", new ThresHold() { max = 1000, min = 50, ratio = 100 }); //Int	最小烟层高度滞后		读/写	数值显示	DB8.DBW38	Corner_work_off_delay	%	10.0 	0.5 
            plcVarThreadHoldMap.Add(".Corner_pid_p_gain", new ThresHold() { max = 500, min = 15, ratio = 100 }); //Int	弯道控制增益		读/写	数值显示	DB8.DBW4	Corner_pid_p_gain	%	5.0 	0.15 
            plcVarThreadHoldMap.Add(".Corner_p_parameter", new ThresHold() { max = 110, min = 60, ratio = 100 });  //Int	弯道速度比例系数		读/写	数值显示	DB8.DBW40	Corner_p_parameter		1.1 	0.6
            plcVarThreadHoldMap.Add(".cig_dim", new ThresHold() { max =900, min = 500, ratio = 100 });  //Int	烟支直径		读/写	数值显示	DB8.DBW46	cig_dim	mm	9.5 	9.0 
            plcVarThreadHoldMap.Add(".Store_CigIn_Comp_speed1", new ThresHold() { max = 4000, min = 100 });  //Int	存储高度1补偿速度		读/写	数值显示	DB8.DBW52	Store_CigIn_Comp_speed1	支/分	4000 	100 
            plcVarThreadHoldMap.Add(".Store_CigIn_Comp_speed2", new ThresHold() { max = 4000, min = 0 }); //Int	存储高度2补偿速度		读/写	数值显示	DB8.DBW54	Store_CigIn_Comp_speed2	支/分	4000 	0 
            plcVarThreadHoldMap.Add(".Downport_CigIn_hight2", new ThresHold() { max = 9000, min = 7000,ratio=100 }); //Int	下降口存烟高度2		读/写	数值显示	DB8.DBW56	Downport_CigIn_hight2	%	90.0 	70.0 
            plcVarThreadHoldMap.Add(".Corner_pid_i_gain", new ThresHold() { max = 10000, min = 0,ratio=100 }); //Int	弯道控制积分时间		读/写	数值显示	DB8.DBW6	Corner_pid_i_gain	%	100.0 	0.0 
            plcVarThreadHoldMap.Add(".Store_empty_position", new ThresHold() { max = 1000, min = 100 ,ratio=100});  //存储空位置		读/写	数值显示	DB8.DBW64	Store_empty_position	%	10.0 	1.0 
            plcVarThreadHoldMap.Add(".Store_percent", new ThresHold() { max = 10000, min = 0, ratio = 100 });  //

            plcVarThreadHoldMap.Add(".Store_full_position", new ThresHold() { max = 9900, min = 9000,ratio=100 }); //Int	存储满位置		读/写	数值显示	DB8.DBW66	Store_full_position	%	99.0 	90.0 
            plcVarThreadHoldMap.Add(".Packer_LowSpeed_position", new ThresHold() { max = 1500, min = 500,ratio=100 }); //Int	包装机低速位置		读/写	数值显示	DB8.DBW68	Packer_LowSpeed_position	%	15.0 	0.0 
            plcVarThreadHoldMap.Add(".Packer_enable_position", new ThresHold() { max = 1000, min = 200, ratio = 100 }); //Int	包装机可启动位置		读/写	数值显示	DB8.DBW70	Packer_enable_position	%	10.0 	2.0 
            plcVarThreadHoldMap.Add(".Corner_entrance_hight_limit", new ThresHold() { max = 27850, min = 2785, ratio = 2.785 });  //Int	弯道入口传感器上限		读/写	数值显示	DB8.DBW74	Corner_entrance_hight_limit	mv	10000 	1000 
            plcVarThreadHoldMap.Add(".Corner_entrance_low_limt", new ThresHold() { max = 13925, min = 0, ratio = 2.785 }); //Int	弯道入口传感器下限		读/写	数值显示	DB8.DBW76	Corner_entrance_low_limt	mv	5000 	0 
            plcVarThreadHoldMap.Add(".Downport_CigIn_hight1", new ThresHold() { max = 6500, min = 4000,ratio=100 }); //Int	下降口存烟高度1		读/写	数值显示	DB8.DBW78	Downport_CigIn_hight1	%	65.0 	40.0 
            plcVarThreadHoldMap.Add(".Corner_pid_deadband", new ThresHold() { max = 300, min = 50, ratio = 100 }); //Int	弯道控制不敏区		读/写	数值显示	DB8.DBW8	Corner_pid_deadband	%	3.0 	0.5 
            plcVarThreadHoldMap.Add(".Downport_CigOut_hight1", new ThresHold() { max = 4000, min = 2500,ratio=100 }); //Int	下降口排烟高度1		读/写	数值显示	DB8.DBW80	Downport_CigOut_hight1	%	40.0 	25.0 
            plcVarThreadHoldMap.Add(".Downport_CigOut_hight2", new ThresHold() { max = 3500, min = 1000 ,ratio=100}); //Int	下降口排烟高度2		读/写	数值显示	DB8.DBW82	Downport_CigOut_hight2	%	35.0 	10.0 
            plcVarThreadHoldMap.Add(".Store_CigOut_Comp_speed1", new ThresHold() { max = 3000, min = 200 }); //Int	排烟高度1补偿速度		读/写	数值显示	DB8.DBW84	Store_CigOut_Comp_speed1	支/分	3000 	200
            plcVarThreadHoldMap.Add(".Store_CigOut_Comp_speed2", new ThresHold() { max = 4500, min = 0 });  //Int	排烟高度2补偿速度		读/写	数值显示	DB8.DBW86	Store_CigOut_Comp_speed2	支/分	4500 	0 
            plcVarThreadHoldMap.Add(".Downport_Highest_limit", new ThresHold() { max = 27850, min = 2785, ratio = 2.785 }); //Int	下降口传感器上限		读/写	数值显示	DB8.DBW88	Downport_Highest_limit	mv	10000 	1000 
            plcVarThreadHoldMap.Add(".Downport_Lowest_limit", new ThresHold() { max = 25065, min = 0, ratio = 2.785 });  //Int	下降口传感器下限		读/写	数值显示	DB8.DBW90	Downport_Lowest_limit	mv	9000 	0 
            plcVarThreadHoldMap.Add(".Downport_sensor_output", new ThresHold() { max = 27850, min = 0, ratio = 2.785 });  //Int	下降口传感器输出		读/写	数值显示	DB8.DBW90	Downport_Lowest_limit	mv	9000 	0 
            plcVarThreadHoldMap.Add(".Corner_entrance_sensor_output", new ThresHold() { max = 27850, min = 0, ratio = 2.785 });  //Int	弯道入口传感器输出		读/写	数值显示	DB8.DBW90	Downport_Lowest_limit	mv	9000 	0 
       
            plcVarThreadHoldMap.Add(".Downport_CigIn_lowest_hight", new ThresHold() { max = 4500, min = 3500 ,ratio=100});  //Int	存烟最低工作高度		读/写	数值显示	DB8.DBW92	Downport_CigIn_lowest_hight	%	45.0 	35.0
            plcVarThreadHoldMap.Add(".Corner_manual_speed", new ThresHold() { max = 9000, min = 100 }); //Corner_manual_speed	支/分	9000 	100
            plcVarThreadHoldMap.Add(".Store_manual_speed", new ThresHold() { max = 4000, min = -4000 }); //Store_manual_speed		9000 	100 	
            plcVarThreadHoldMap.Add(".Lift_p_parameter", new ThresHold() { max = 120, min = 60, ratio = 100 });//Lift_p_parameter		1.2 	0.6 	缩100倍
            plcVarThreadHoldMap.Add(".Transfer_p_parameter", new ThresHold() { max = 120, min = 60, ratio = 100 }); //Transfer_p_parameter		1.2 	0.6 	缩100倍
            plcVarThreadHoldMap.Add(".MakerExport_p_parameter", new ThresHold() { max = 120, min = 60, ratio = 100 });  //MakerExport_p_parameter		1.2 	0.6 	缩100倍
            plcVarThreadHoldMap.Add(".Maker_MaxSpeedLimit", new ThresHold() { max = 16000, min = 1000 }); //Maker_MaxSpeedLimit	支/分	16000 	1000 	
            plcVarThreadHoldMap.Add(".Packer_MaxSpeedLimit", new ThresHold() { max = 16000, min = 1000 });  //Packer_MaxSpeedLimit	支/分	16000 	1000
          //  plcVarThreadHoldMap.Add(".Maker_stop_position", new ThresHold() { max = 9800, min = 9000 }); //Maker_stop_position		9800.0 	9000.0 	            

            plcVarThreadHoldMap.Add(".Corner_pid_pv", new ThresHold() { max = 10000, min = 0, ratio = 100 }); //弯道高度反馈（DB8.DBW2）Corner_pid_pv
            plcVarThreadHoldMap.Add(".Corner_pid_output", new ThresHold() { max = 10000, min = 0, ratio = 100 }); //弯道控制补偿输出（DB8.DBW10）Corner_pid_output
            plcVarThreadHoldMap.Add(".DownPort_hight", new ThresHold() { max = 10000, min = 0, ratio = 100 });//下降口反馈高度（DB8.DBW26）DownPort_hight
            plcVarThreadHoldMap.Add(".Maker_stop_position", new ThresHold() { max = 10000, min = 0, ratio = 100 });//卷烟机停机位置（DB8.DBW72）Maker_stop_position

            plcVarThreadHoldMap.Add(".test_maker_speed", new ThresHold() { max = 12000, min = 100 }); //test_maker_speed	Int	卷烟机试运转速度		读/写	数值显示	MW1950
            plcVarThreadHoldMap.Add(".test_packer_speed", new ThresHold() { max = 12000, min = 100 }); //test_packer_speed	Int	包装机试运转速度		读/写	数值显示	MW1952
       
        }

        private void initVarDescription()
        {
            plcVarDescriptionMap.Add(".Corner_pid_sp", "弯道高度设定SP");
            plcVarDescriptionMap.Add(".Corner_pid_output", "弯道控制补偿输出");
            plcVarDescriptionMap.Add(".Corner_pid_pv", "弯道高度反馈");
            plcVarDescriptionMap.Add(".DownPort_hight", "下降口反馈高度");
            plcVarDescriptionMap.Add(".Corner_work_limit", "弯道最小烟层高度");
            plcVarDescriptionMap.Add(".Corner_work_off_delay", "最小烟层高度滞后");
            plcVarDescriptionMap.Add(".Corner_pid_p_gain", "弯道控制增益");
            plcVarDescriptionMap.Add(".Corner_p_parameter", "弯道速度比例系数");
            plcVarDescriptionMap.Add(".cig_dim", "烟支直径");
            plcVarDescriptionMap.Add(".Store_percent", "存储位置百分比");
            plcVarDescriptionMap.Add(".Store_CigIn_Comp_speed1", "存储高度1补偿速度");
            plcVarDescriptionMap.Add(".Store_CigIn_Comp_speed2", "存储高度2补偿速度");
            plcVarDescriptionMap.Add(".Downport_CigIn_hight2", "下降口存烟高度2");
            plcVarDescriptionMap.Add(".Corner_lowlimit", "备用");
            plcVarDescriptionMap.Add(".Corner_pid_i_gain", "弯道控制积分时间");
            plcVarDescriptionMap.Add(".Maker_MaxSpeedLimit", "卷烟机最大速度设定");
            plcVarDescriptionMap.Add(".Packer_MaxSpeedLimit", "包装机最大速度设定");
            plcVarDescriptionMap.Add(".Store_empty_position", "存储空位置");
            plcVarDescriptionMap.Add(".Store_full_position", "存储满位置");
            plcVarDescriptionMap.Add(".Packer_LowSpeed_position", "包装机低速位置");
            plcVarDescriptionMap.Add(".Packer_enable_position", "包装机可启动位置");
            plcVarDescriptionMap.Add(".Maker_stop_position", "卷烟机停止位置");
            plcVarDescriptionMap.Add(".Corner_entrance_hight_limit", "弯道入口传感器上限");
            plcVarDescriptionMap.Add(".Corner_entrance_low_limt", "弯道入口传感器下限");
            plcVarDescriptionMap.Add(".Downport_CigIn_hight1", "下降口存烟高度1");
            plcVarDescriptionMap.Add(".Corner_pid_deadband", "弯道控制不敏区");
            plcVarDescriptionMap.Add(".Downport_CigOut_hight1", "下降口排烟高度1");
            plcVarDescriptionMap.Add(".Downport_CigOut_hight2", "下降口排烟高度2");
            plcVarDescriptionMap.Add(".Store_CigOut_Comp_speed1", "排烟高度1补偿速度");
            plcVarDescriptionMap.Add(".Store_CigOut_Comp_speed2", "排烟高度2补偿速度");
            plcVarDescriptionMap.Add(".Downport_Highest_limit", "下降口传感器上限");
            plcVarDescriptionMap.Add(".Downport_Lowest_limit", "下降口传感器下限");
            plcVarDescriptionMap.Add(".Downport_CigIn_lowest_hight", "存烟最低工作高度");
            plcVarDescriptionMap.Add(".Emergency_stop", "紧急停止继电器");
            plcVarDescriptionMap.Add(".Sample_power", "取样伺服主电源");
            plcVarDescriptionMap.Add(".Corner_power", "弯道伺服主电源");
            plcVarDescriptionMap.Add(".Slope_power", "斜向伺服主电源");
            plcVarDescriptionMap.Add(".Store_power", "存储伺服主电源");
            plcVarDescriptionMap.Add(".Sample_servo_fault", "取样伺服控制器故障");
            plcVarDescriptionMap.Add(".Corner_servo_fault", "弯道伺服控制器故障");
            plcVarDescriptionMap.Add(".Slope_servo_fault", "斜向伺服控制器故障");
            plcVarDescriptionMap.Add(".Store_servo_fault", "存储伺服控制器故障");
            plcVarDescriptionMap.Add(".Spare11", "备用");
            plcVarDescriptionMap.Add(".Elevater_man_auto_sw", "提升机手动/自动方式");
            plcVarDescriptionMap.Add(".Elevater_man_paikong", "手动排空");
            plcVarDescriptionMap.Add(".Elevater_start_pb", "提升机启动按钮");
            plcVarDescriptionMap.Add(".DropTransLowLevel", "下降口输送低料位");
            plcVarDescriptionMap.Add(".DropTransHighLevel", "下降口输送高料位");
            plcVarDescriptionMap.Add(".DropTransLimitLevel", "下降口输送极限料位");
            plcVarDescriptionMap.Add(".Lift_power", "提升伺服主电源");
            plcVarDescriptionMap.Add(".DropTrans_servo_fault", "备用");
            plcVarDescriptionMap.Add(".Spare22", "备用");
            plcVarDescriptionMap.Add(".Sample_entrance_sensor", "取样入口有烟传感器");
            plcVarDescriptionMap.Add(".Sample_entrance_jam_sensor", "取样入口堵塞传感器（备用）");
            plcVarDescriptionMap.Add(".Corner_entrance_jam_sensor", "弯道入口堵塞传感器");
            plcVarDescriptionMap.Add(".Spare26", "备用");
            plcVarDescriptionMap.Add(".Transfer_power", "传送伺服主电源");
            //plcVarDescriptionMap.Add(".StoreUnit_man_auto_sw", "存储器手动/自动方式");
            plcVarDescriptionMap.Add(".StoreUnit_discharge_button", "全排空");
           // plcVarDescriptionMap.Add(".StoreUnit_start_button", "存储器启动按钮");
          //  plcVarDescriptionMap.Add(".StoreUnit_reset_button", "存储器复位按钮");
         //   plcVarDescriptionMap.Add(".StoreUnit_stop_button", "存储器停机按钮");
          //  plcVarDescriptionMap.Add(".StoreUnit_e_stop_button", "存储器紧急停止按钮");
            plcVarDescriptionMap.Add(".Spare36", "包装机运行（备用）");
            plcVarDescriptionMap.Add(".Spare37", "备用");
            plcVarDescriptionMap.Add(".Downport_jam_sensor", "下降口堵塞传感器");
            plcVarDescriptionMap.Add(".Slope_empty", "斜向通道空");
            plcVarDescriptionMap.Add(".Transfer_cig_exist", "高架烟支传感器");
            plcVarDescriptionMap.Add(".Transfer_overload_sensor", "传送过载传感器");
            plcVarDescriptionMap.Add(".Lift_servo_fault", "提升伺服控制器故障");
            plcVarDescriptionMap.Add(".Transfer_servo_fault", "传送伺服控制器故障");
            plcVarDescriptionMap.Add(".Spare46", "备用");
            plcVarDescriptionMap.Add(".Spare47", "备用");
            plcVarDescriptionMap.Add(".Store_full", "存储器满传感器");
            plcVarDescriptionMap.Add(".Store_empty", "存储器空传感器");
            plcVarDescriptionMap.Add(".Store_overload", "存储器过载传感器");
            plcVarDescriptionMap.Add(".Store_entrance_cig_exist", "存储器入口有烟传感器");
            plcVarDescriptionMap.Add(".Store_entrance_jam", "存储器入口堵塞");
            plcVarDescriptionMap.Add(".Store_overlimit", "存储器极限开关");
            plcVarDescriptionMap.Add(".Store_running", "存储器运行中");
            plcVarDescriptionMap.Add(".Store_enabled", "存储器使能");
            plcVarDescriptionMap.Add(".Store_set_zero", "存储手动校零");
            plcVarDescriptionMap.Add(".test_run", "机器试运转");
            plcVarDescriptionMap.Add(".test_run_unprotected", "机器强制试运转");
            plcVarDescriptionMap.Add(".test_run_light", "备用");
            plcVarDescriptionMap.Add(".sample_servo_enable", "取样驱动器使能");
            plcVarDescriptionMap.Add(".sample_servo_initialized", "取样驱动器初始完成");
            plcVarDescriptionMap.Add(".Corner_servo_enable", "弯道驱动器使能");
            plcVarDescriptionMap.Add(".Corner_servo_initialized", "弯道驱动器初始完成");
            plcVarDescriptionMap.Add(".Slope_servo_enable", "斜向驱动器使能");
            plcVarDescriptionMap.Add(".Slope_servo_initialized", "斜向驱动器初始完成");
            plcVarDescriptionMap.Add(".Store_servo_enable", "存储驱动器使能");
            plcVarDescriptionMap.Add(".Lift_servo_enable", "提升驱动器使能");
            plcVarDescriptionMap.Add(".Lift_servo_initialized", "提升驱动器初始完成");
            plcVarDescriptionMap.Add(".Transfer_servo_enable", "传送驱动器使能");
            plcVarDescriptionMap.Add(".Transfer_servo_initialized", "传送驱动器初始完成");
            plcVarDescriptionMap.Add(".alarm_sample_entrance_jam", "取样入口堵塞报警指示");
            plcVarDescriptionMap.Add(".alarm_corner_entrance_jam", "弯道入口堵塞报警指示");
            plcVarDescriptionMap.Add(".alarm_downport_entrance_jam", "下降口入口堵塞报警指示");
            plcVarDescriptionMap.Add(".alarm_transfer_overload", "传送电机过载报警指示");
            plcVarDescriptionMap.Add(".alarm_store_overload", "存储器过载报警指示");
            plcVarDescriptionMap.Add(".alarm_store_entrance_jam", "存储器入口堵塞报警指示");
            plcVarDescriptionMap.Add(".Elevater_manual_discharge", "提升机手动排空运转");
            plcVarDescriptionMap.Add(".Elevater_auto_run", "提升机自动运转");
            plcVarDescriptionMap.Add(".StoreUnit_man_run", "存储器手动运转");
            plcVarDescriptionMap.Add(".StoreUnit_auto_run", "存储器自动运转");
            plcVarDescriptionMap.Add(".StoreUnit_discharge_run", "存储器全排空运转");
            plcVarDescriptionMap.Add(".alarm_store_limit_on", "存储器极限位置到达报警指示");
            plcVarDescriptionMap.Add(".alarm_store_full", "存储器满报警指示");
            plcVarDescriptionMap.Add(".alarm_encoder_fault", "存储编码器故障报警");
            plcVarDescriptionMap.Add(".Maker_run", "卷烟机运转");
            plcVarDescriptionMap.Add(".packer_run", "包装机运转");
            plcVarDescriptionMap.Add(".Downport_comp_output", "下降口控制补偿输出");
            plcVarDescriptionMap.Add(".Maker_cig_speed", "卷烟机烟支速度");
            plcVarDescriptionMap.Add(".Sample_cig_speed", "取样电机烟支速度");
            plcVarDescriptionMap.Add(".Corner_cig_speed", "弯道电机烟支速度");
            plcVarDescriptionMap.Add(".Packer_cig_speed", "包装机烟支速度");
            plcVarDescriptionMap.Add(".Life_cig_speed", "提升电机烟支速度");
            plcVarDescriptionMap.Add(".Transfer_cig_speed", "传送电机烟支速度");

            plcVarDescriptionMap.Add(".Sample_speed_rpm", "取样电机转速");           
            plcVarDescriptionMap.Add(".Corner_speed_rpm", "弯道电机转速");
          
            plcVarDescriptionMap.Add(".Slope_speed_rpm", "斜向电机转速");
            plcVarDescriptionMap.Add(".Store_speed_rpm", "存储电机转速");

            plcVarDescriptionMap.Add(".Corner_entrance_sensor_output", "弯道入口传感器输出");
            plcVarDescriptionMap.Add(".Downport_sensor_output", "下降口传感器输出");
            plcVarDescriptionMap.Add(".Store_CigNum", "存储烟支数");
            plcVarDescriptionMap.Add(".Store_cig_speed", "存储电机烟支速度");
            plcVarDescriptionMap.Add(".Slope_cig_speed", "斜向电机烟支速度");
            plcVarDescriptionMap.Add(".Lift_speed_rpm", "提升电机转速");
            plcVarDescriptionMap.Add(".test_maker_speed", "卷烟机试运转速度");
            plcVarDescriptionMap.Add(".test_packer_speed", "包装机试运转速度");
            plcVarDescriptionMap.Add(".Transfer_speed_rpm", "传送电机转速");
            plcVarDescriptionMap.Add(".Corner_manual_speed", "弯道电机手动速度");
            plcVarDescriptionMap.Add(".Store_manual_speed", "存储电机手动速度");
            plcVarDescriptionMap.Add(".Lift_p_parameter", "提升速度比例系数");
            plcVarDescriptionMap.Add(".Transfer_p_parameter", "传送速度比例系数");
            plcVarDescriptionMap.Add(".sample_servo_enable_Q", "取样伺服驱动器使能");
            plcVarDescriptionMap.Add(".Corner_servo_enable_Q", "弯道伺服驱动器使能");
            plcVarDescriptionMap.Add(".Slope_servo_enable_Q", "斜向伺服驱动器使能");
            plcVarDescriptionMap.Add(".Store_servo_enable_Q", "存储伺服驱动器使能");
            plcVarDescriptionMap.Add(".SpareOutput44", "备用");
            plcVarDescriptionMap.Add(".Elevater_manual_discharge_Q", "提升机手动排空指示");
            plcVarDescriptionMap.Add(".Elevater_start_Q", "提升机启动指示");
            plcVarDescriptionMap.Add(".Elevater_reset_Q", "提升机复位指示");
            plcVarDescriptionMap.Add(".Elevater_stop_Q", "提升机停机指示");
            plcVarDescriptionMap.Add(".Store_FaultReset_Q", "存储器故障复位");
            plcVarDescriptionMap.Add(".SpareOutput52", "备用");
            plcVarDescriptionMap.Add(".SpareOutput53", "备用");
            plcVarDescriptionMap.Add(".Maker_enable_relay_Q", "卷烟机允许(备用)");
            plcVarDescriptionMap.Add(".SpareOutput55", "备用");
            plcVarDescriptionMap.Add(".SpareOutput56", "备用");
            plcVarDescriptionMap.Add(".Maker_QuickStop_Q", "卷烟机快停（备用）");
            plcVarDescriptionMap.Add(".SpareOutput58", "备用");
            plcVarDescriptionMap.Add(".SpareOutput59", "备用");
            plcVarDescriptionMap.Add(".Packer_enable_relay_Q", "包装机允许（备用）");
            plcVarDescriptionMap.Add(".Packer_LowSpeed_request_Q", "包装机低速请求（备用）");
            plcVarDescriptionMap.Add(".Lift_servo_enable_Q", "提升伺服驱动使能");
            plcVarDescriptionMap.Add(".Transfer_servo_enable_Q", "传送伺服驱动使能");
            plcVarDescriptionMap.Add(".StoreUnit_discharge_Q", "存储器全排空指示");
            plcVarDescriptionMap.Add(".StoreUnit_start_Q", "存储器启动指示");
            plcVarDescriptionMap.Add(".StoreUnit_reset_Q", "存储器复位指示");
            plcVarDescriptionMap.Add(".StoreUnit_stop_Q", "存储器停机指示");
            plcVarDescriptionMap.Add(".StoreUnit_e_stop_Q", "存储器急停指示");
            plcVarDescriptionMap.Add(".Store_full_Q", "存储器满指示");
            plcVarDescriptionMap.Add(".Store_empty_Q", "存储器空指示");
            plcVarDescriptionMap.Add(".Store_overload_Q", "存储器过载指示");
            plcVarDescriptionMap.Add(".Store_limitOn_Q", "存储器极限指示");
            plcVarDescriptionMap.Add(".Store_entrance_jam_Q", "存储器入口堵塞指示");
            plcVarDescriptionMap.Add(".Downport_jam_Q", "下降口堵塞指示");
            plcVarDescriptionMap.Add(".Packer_enable_Q", "包装机允许指示");
            plcVarDescriptionMap.Add(".Slope_empty_Q", "斜向通道空指示");
            plcVarDescriptionMap.Add(".Sample_entrance_jam_Q", "取样入口堵塞指示");
            plcVarDescriptionMap.Add(".Corner_entrance_jam_Q", "弯道入口堵塞指示");
            plcVarDescriptionMap.Add(".Lift_overload_Q", "提升机过载指示");
            plcVarDescriptionMap.Add(".Maker_enable_Q", "卷烟机允许指示");
            plcVarDescriptionMap.Add(".SpareOutput107", "备用");
            plcVarDescriptionMap.Add(".Sample_servo_ethercat_fault", "取样伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Corner_servo_ethercat_fault", "弯道伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Lift_servo_ethercat_fault", "提升伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Transfer_servo_ethercat_fault", "传送伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Slope_servo_ethercat_fault", "斜向伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Store_servo_ethercat_fault", "存储伺服通讯卡故障");
            plcVarDescriptionMap.Add(".Digital_Input1_ethercat_fault", "数字量输入模块1通讯故障");
            plcVarDescriptionMap.Add(".Digital_Input2_ethercat_fault", "数字量输入模块2通讯故障");
            plcVarDescriptionMap.Add(".Digital_Input3_ethercat_fault", "数字量输入模块3通讯故障");
            plcVarDescriptionMap.Add(".Digital_Input4_ethercat_fault", "数字量输入模块4通讯故障");
            plcVarDescriptionMap.Add(".Digital_Input5_ethercat_fault", "数字量输入模块5通讯故障");
            plcVarDescriptionMap.Add(".Digital_Input6_ethercat_fault", "数字量输入模块6通讯故障");
            plcVarDescriptionMap.Add(".Digital_Output1_ethercat_fault", "数字量输出模块1通讯故障");
            plcVarDescriptionMap.Add(".Digital_Output2_ethercat_fault", "数字量输出模块2通讯故障");
            plcVarDescriptionMap.Add(".Digital_Output3_ethercat_fault", "数字量输出模块3通讯故障");
            plcVarDescriptionMap.Add(".Digital_Output4_ethercat_fault", "数字量输出模块4通讯故障");
            plcVarDescriptionMap.Add(".Digital_Output5_ethercat_fault", "数字量输出模块5通讯故障");
            plcVarDescriptionMap.Add(".Analog_Input1_ethercat_fault", "模拟量输入模块1通讯故障");
            plcVarDescriptionMap.Add(".Analog_Input2_ethercat_fault", "模拟量输入模块2通讯故障");
            plcVarDescriptionMap.Add(".Sample_Servo_FaultNum", "取样伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".Corner_Servo_FaultNum", "弯道伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".Lift_Servo_FaultNum", "提升伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".Transfer_Servo_FaultNum", "传送伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".Slope_Servo_FaultNum", "斜向伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".Store_Servo_FaultNum", "存储伺服驱动器故障代码");
            plcVarDescriptionMap.Add(".StoreUnit_Stop", "斜向存储停机");
            plcVarDescriptionMap.Add(".Elevater_Stop", "提升机停机");
            plcVarDescriptionMap.Add(".Manual_Run", "机器手动运行");
            plcVarDescriptionMap.Add(".Auto_Run", "机器自动运行");
            plcVarDescriptionMap.Add(".Elevater_Manual_Run", "提升机手动运行");
            
            plcVarDescriptionMap.Add(".MakerExport_cig_speed", "烟机出口电机速度");
            plcVarDescriptionMap.Add(".MakerExport_p_parameter", "烟机出口比例系数");

        }

       public String[] plcVariableNameTypes = new String[] {
                                                "I.CigMakingSpeed",// AT %IW330",//:INT;
                                                "I.PackingSpeed",// AT %IW332",//:INT;
                                                "I.PotenSpeed",// AT %IW328",//:INT;
                                                "I.ElevatorLevel",// AT %IW324",//:INT;
                                                "I.DropLevel",// AT %IW326",//:INT;
                                                "B.Store_set_zero",// AT %MX1000.0 : BOOL;
                                                "B.test_run",// AT %MX1000.6 : BOOL;
                                                "B.StoreUnit_discharge_button",// AT %MX2603.6 : BOOL;
                                                "B.Elevater_man_paikong",// AT %MX2603.7 :BOOL;
                                                "B.test_run_unprotected",// AT %MX1000.7 : BOOL;
                                                "I.test_maker_speed",// AT %MW1950 : INT;
                                                "I.test_packer_speed",// AT %MW1952 : INT;
                                                "I.Corner_manual_speed",// AT %MW254 : INT;
                                                "I.Store_manual_speed",// AT %MW260 : INT;
                                                "I.DropTrans_cig_speed",// AT %MW2320 : INT;(*下降口输送烟支速度*)(*******7Axis**********)
                                                "I.DropTrans_speed_rpm",// AT %MW2340 : INT;(*下降口输送电机转速*)(******7Axis***********)

                                                "I.Lift_p_parameter",// AT %MW270 : INT;
                                                "I.Transfer_p_parameter",// AT %MW272 : INT;
                                                "I.MakerExport_p_parameter",// AT %MW274 : INT;

                                                "B.Emergency_stop",// AT %IX0.0:BOOL;(*紧急停止继电器*)
                                                "B.MakerExit_power",// AT %IX0.1:BOOL;(*备用*)
                                                "B.Sample_power",// AT %IX0.2:BOOL;(*取样伺服主电源*)
                                                "B.Corner_power",// AT %IX0.3:BOOL;(*弯道伺服主电源*)
                                                "B.Lift_power",// AT %IX0.4:BOOL;(*提升伺服主电源*)
                                                "B.Transfer_power",// AT %IX0.5:BOOL;(*传送伺服主电源*)
                                                "B.Slope_power",// AT %IX0.6:BOOL;(*斜向伺服主电源*)
                                                "B.Store_power",// AT %IX0.7:BOOL;(*存储伺服主电源*)
                                                "B.DropTrans_power",// AT %IX1.0:BOOL;(*下降口输送伺服主电源*)(****************************************************7Axis中用DropTrans_power替换原Spare10*************)

                                                "B.Spare11",// AT %IX1.1:BOOL;(*备用*)
                                                "B.MakerExit_servo_fault",// AT %IX1.2:BOOL;(*备用*)
                                                "B.Sample_servo_fault",// AT %IX1.3:BOOL;(*取样伺服控制器故障*)
                                                "B.Corner_servo_fault",// AT %IX1.4:BOOL;(*弯道伺服控制器故障*)
                                                "B.Lift_servo_fault",// AT %IX1.5:BOOL;(*提升伺服控制器故障*)
                                                "B.Transfer_servo_fault",// AT %IX1.6:BOOL;(*传送伺服控制器故障*)
                                                "B.Slope_servo_fault",// AT %IX1.7:BOOL;(*斜向伺服控制器故障*)
                                                "B.Store_servo_fault",// AT %IX2.0:BOOL;(*存储伺服控制器故障*)
                                                "B.DropTrans_servo_fault",// AT %IX2.1:BOOL;(*备用*)(********************************************************************7Axis中用DropTrans_servo_fault替换原Spare21*************)
                                                "B.Spare22",// AT %IX2.2:BOOL;(*备用*)
                                                "B.Elevater_man_auto_sw",// AT %IX2.3:BOOL;(*备用*)
                                                "B.Elevater_start_pb",// AT %IX2.4:BOOL;(*备用*)
                                                "B.DropTransLowLevel",// AT %IX2.5:BOOL;(*下降口输送低料位*)(********************************************************7Axis中用DropTransLowLevel替换原Elevater_reset_pb*************)
                                                "B.DropTransHighLevel",// AT %IX2.6:BOOL;(*下降口输送高料位*)(******************************************************7Axis中用DropTransHighLevel替换原Elevater_stop_pb*************)
                                                "B.DropTransLimitLevel",// AT %IX2.7:BOOL;(*下降口输送极限料位*)(*****************************************************7Axis中用DropTransLimitLevel替换原Elevater_e_stop*************)
                                                "B.Spare30",// AT %IX3.0:BOOL;(*备用*)
                                                "B.Spare31",// AT %IX3.1:BOOL;(*备用*)
                                                "B.MakerExit_sensor",// AT %IX3.2:BOOL;(*卷烟机出口有烟(备用)*)
                                                "B.Sample_entrance_sensor",// AT %IX3.3:BOOL;(*取样入口有烟传感器*)
                                                "B.Sample_entrance_jam_sensor",// AT %IX3.4:BOOL;(*取样入口堵塞传感器（备用）*)
                                                "B.Corner_entrance_jam_sensor",// AT %IX3.5:BOOL;(*弯道入口堵塞传感器*)
                                                "B.MakerExit_jam_sensor",// AT %IX3.6:BOOL;(*卷烟机出口堵塞（备用）*)
                                                "B.Spare37",// AT %IX3.7:BOOL;(*备用*)
                                                "B.Downport_jam_sensor",// AT %IX4.0:BOOL;(*下降口堵塞传感器*)
                                                "B.Slope_empty",// AT %IX4.1:BOOL;(*斜向通道空*)
                                                "B.Transfer_cig_exist",// AT %IX4.2:BOOL;(*高架烟支传感器*)
                                                "B.Transfer_overload_sensor",// AT %IX4.3:BOOL;(*传送过载传感器*)
                                                "B.Spare44",// AT %IX4.4:BOOL;(*备用*)
                                                "B.Spare45",// AT %IX4.5:BOOL;(*备用*)
                                                "B.StoreUnit_e_stop_button",//:BOOL;
                                                "B.Spare46",// AT %IX4.6:BOOL;(*备用*)
                                                "B.Spare47",// AT %IX4.7:BOOL;(*备用*)
                                                "B.Store_full",// AT %IX5.0:BOOL;(*存储器满传感器*)
                                                "B.Store_empty",// AT %IX5.1:BOOL;(*存储器空传感器*)
                                                "B.Store_overload",// AT %IX5.2:BOOL;(*存储器过载传感器*)
                                                "B.Store_entrance_cig_exist",// AT %IX5.3:BOOL;(*存储器入口有烟传感器*)
                                                "B.Store_entrance_jam",// AT %IX5.4:BOOL;(*存储器入口堵塞*)
                                                "B.Store_overlimit",// AT %IX5.5:BOOL;(*存储器极限开关*)
                                                "B.Store_running",// AT %IX5.6:BOOL;(*存储器运行中*)
                                                "B.Store_enabled",// AT %IX5.7:BOOL;(*存储器使能*)
                                                "B.sample_servo_enable_Q",// AT %QX4.0:BOOL;(*取样伺服驱动器使能*)
                                                "B.Corner_servo_enable_Q",// AT %QX4.1:BOOL;(*弯道伺服驱动器使能*)
                                                "B.Lift_servo_enable_Q",// AT %QX4.2:BOOL;(*提升伺服驱动器使能*)
                                                "B.Transfer_servo_enable_Q",// AT %QX4.3:BOOL;(*传送伺服驱动器使能*)
                                                "B.Slope_servo_enable_Q",// AT %QX4.4:BOOL;(*斜向伺服驱动器使能*)
                                                "B.Store_servo_enable_Q",// AT %QX4.5:BOOL;(*存储伺服驱动器使能*)
                                                "B.DropTrans_servo_enable_Q",// AT %QX4.6:BOOL;(*下降口输送伺服驱动器使能*)(*******************************7Axis中用DropTrans_servo_enable_Q替换原SpareOutput46*************)
                                                "B.SpareOutput47",// AT %QX4.7:BOOL;(*备用*)
                                                "B.Elevater_start_Q",// AT %QX5.0:BOOL;(*备用*)
                                                "B.Elevater_reset_Q",// AT %QX5.1:BOOL;(*备用*)
                                                "B.Elevater_stop_Q",// AT %QX5.2:BOOL;(*备用*)
                                                "B.Store_FaultReset_Q",// AT %QX5.3:BOOL;(*存储器故障复位*)
                                                "B.Maker_enable_relay_Q",// AT %QX5.4:BOOL;(*卷烟机允许(备用)*)
                                                "B.SpareOutput55",// AT %QX5.5:BOOL;(*下降口急停复位*)
                                                "B.SpareOutput56",// AT %QX5.6:BOOL;(*备用*)
                                                "B.Maker_QuickStop_Q",// AT %QX5.7:BOOL;(*卷烟机快停（备用）*)
                                                "B.SpareOutput80",// AT %QX8.0:BOOL;(*存储振动检测*)
                                                "B.SpareOutput81",// AT %QX8.1:BOOL;(*备用*)
                                                "B.Packer_enable_relay_Q",// AT %QX8.2:BOOL;(*包装机允许（备用）*)
                                                "B.Packer_LowSpeed_request_Q",// AT %QX8.3:BOOL;(*包装机低速请求（备用）*)
                                                "B.SpareOutput84",// AT %QX8.4:BOOL;(*备用*)
                                                "B.SpareOutput85",// AT %QX8.5:BOOL;(*系统主电源*)
                                                "B.SpareOutput86",// AT %QX8.6:BOOL;(*备用*)
                                                "B.StoreUnit_start_Q",// AT %QX8.7:BOOL;(*备用*)
                                                "B.StoreUnit_reset_Q",// AT %QX9.0:BOOL;(*备用*)
                                                "B.StoreUnit_stop_Q",// AT %QX9.1:BOOL;(*备用*)
                                                "B.SpareOutput92",// AT %QX9.2:BOOL;(*备用*)
                                                "B.SpareOutput93",// AT %QX9.3:BOOL;(*备用*)
                                                "B.SpareOutput94",// AT %QX9.4:BOOL;(*备用*)
                                                "B.SpareOutput95",// AT %QX9.5:BOOL;(*备用*)
                                                "B.SpareOutput96",// AT %QX9.6:BOOL;(*备用*)
                                                "B.SpareOutput97",// AT %QX9.7:BOOL;(*备用*)

                                                "B.DropTrans_servo_enable",// AT %MX3102.5:BOOL;(*下降口输送驱动器使能*)(*******************7Axis用*********************************)
                                                "B.DropTrans_servo_initialized",// AT %MX3102.7:BOOL;(*下降口输送初始完成*)(*******************7Axis用*********************************)

                                                "B.MakerExit_servo_enable",// AT %MX2102.5 : BOOL;
                                                "B.MakerExit_servo_initialized",// AT %MX2102.7 : BOOL;
                                                "B.sample_servo_enable",// AT %MX1502.5 : BOOL;
                                                "B.sample_servo_initialized",// AT %MX1502.7 : BOOL;
                                                "B.Corner_servo_enable",// AT %MX1602.5 : BOOL;
                                                "B.Corner_servo_initialized",// AT %MX1602.7 : BOOL;
                                                "B.Slope_servo_enable",// AT %MX1702.5 : BOOL;
                                                "B.Slope_servo_initialized",// AT %MX1702.7 : BOOL;
                                                "B.Store_servo_enable",// AT %MX1802.5 : BOOL;
                                                "B.Lift_servo_enable",// AT %MX1902.5 : BOOL;
                                                "B.Lift_servo_initialized",// AT %MX1902.7 : BOOL;
                                                "B.Transfer_servo_enable",// AT %MX2002.5 : BOOL;
                                                "B.Transfer_servo_initialized",// AT %MX2002.7 : BOOL;
                                                "B.alarm_sample_entrance_jam",// AT %MX50.0 : BOOL;
                                                "B.alarm_corner_entrance_jam",// AT %MX50.1 : BOOL;
                                                "B.alarm_downport_entrance_jam",// AT %MX50.2 : BOOL;

                                                "B.alarm_droptrans_jam",// AT %MX920.8 : BOOL;(*下降口输送堵塞*)(*******************7Axis用*********************************)
                                                "B.alarm_transfer_overload",// AT %MX50.3 : BOOL;
                                                "B.alarm_store_overload",// AT %MX50.4 : BOOL;
                                                "B.alarm_store_entrance_jam",// AT %MX50.5 : BOOL;
                                                "B.Elevater_manual_discharge",// AT %MX501.0 : BOOL;
                                                "B.Elevater_auto_run",// AT %MX502.1 : BOOL;
                                                "B.StoreUnit_man_run",// AT %MX550.1 : BOOL;
                                                "B.StoreUnit_auto_run",// AT %MX552.1 : BOOL;
                                                "B.StoreUnit_discharge_run",// AT %MX553.7 : BOOL;
                                                "B.alarm_store_limit_on",// AT %MX60.2 : BOOL;
                                                "B.alarm_store_full",// AT %MX71.2 : BOOL;
                                                "B.alarm_encoder_fault",// AT %MX71.3 : BOOL;
                                                "B.Maker_run",// AT %MX80.0 : BOOL;
                                                "B.packer_run",// AT %MX80.1 : BOOL;
                                                "B.MakerExit_servo_ethercat_fault",// AT %MX2600.0 : BOOL;
                                                "B.Sample_servo_ethercat_fault",// AT %MX2600.1 : BOOL;
                                                "B.Corner_servo_ethercat_fault",// AT %MX2600.2 : BOOL;
                                                "B.Lift_servo_ethercat_fault",// AT %MX2600.3 : BOOL;
                                                "B.Transfer_servo_ethercat_fault",// AT %MX2600.4 : BOOL;
                                                "B.Slope_servo_ethercat_fault",// AT %MX2600.5 : BOOL;
                                                "B.Store_servo_ethercat_fault",// AT %MX2600.6 : BOOL;
                                                "B.DropTrans_servo_ethercat_fault",// AT %MX2600.7 : BOOL;(*下降口输送伺服通讯卡故障*)(*************************7Axis中用DropTrans_servo_ethercat_fault替换原Spare26007*************)
                                                "B.Spare26010",// AT %MX2601.0 : BOOL;
                                                "B.Digital_Input1_ethercat_fault",// AT %MX2601.1 : BOOL;
                                                "B.Digital_Input2_ethercat_fault",// AT %MX2601.2 : BOOL;
                                                "B.Digital_Input3_ethercat_fault",// AT %MX2601.3 : BOOL;
                                                "B.Digital_Input4_ethercat_fault",// AT %MX2601.4 : BOOL;
                                                "B.Digital_Input5_ethercat_fault",// AT %MX2601.5 : BOOL;
                                                "B.Digital_Input6_ethercat_fault",// AT %MX2601.6 : BOOL;
                                                "B.Digital_Input7_ethercat_fault",// AT %MX2601.7 : BOOL;
                                                "B.Digital_Output1_ethercat_fault",// AT %MX2602.0 : BOOL;
                                                "B.Digital_Output2_ethercat_fault",// AT %MX2602.1 : BOOL;
                                                "B.Digital_Output3_ethercat_fault",// AT %MX2602.2 : BOOL;
                                                "B.Digital_Output4_ethercat_fault",// AT %MX2602.3 : BOOL;
                                                "B.Analog_Input1_ethercat_fault",// AT %MX2602.4 : BOOL;
                                                "B.Analog_Input2_ethercat_fault",// AT %MX2602.5 : BOOL;
                                                "B.StoreUnit_Stop",// AT %MX2603.0 : BOOL;
                                                "B.Elevater_Stop",// AT %MX2603.1 : BOOL;
                                                "B.Manual_Run",// AT %MX2603.2 : BOOL;
                                                "B.Auto_Run",// AT %MX2603.3 : BOOL;
                                                "B.Elevater_Manual_Run",// AT %MX2603.4 : BOOL;
                                                "B.test_run_light",// AT %MX1002.0 : BOOL;
                                                "I.Downport_comp_output",// AT %MW1250 : INT;
                                                "I.Maker_cig_speed",// AT %MW1300 : INT;
                                                "I.Sample_cig_speed",// AT %MW1302 : INT;
                                                "I.Corner_cig_speed",// AT %MW1304 : INT;
                                                "I.Packer_cig_speed",// AT %MW1310 : INT;
                                                "I.Life_cig_speed",// AT %MW1400 : INT;
                                                "I.Transfer_cig_speed",// AT %MW1402 : INT;
                                                "I.MakerExport_cig_speed",// AT %MW1404 : INT;
                                                "I.MakerExport_speed_rpm",// AT %MW2236 : INT;
                                                "I.Sample_speed_rpm",// AT %MW1536 : INT;
                                                "I.Corner_speed_rpm",// AT %MW1636 : INT;
                                                "I.Slope_speed_rpm",// AT %MW1736 : INT;
                                                "I.Store_speed_rpm",// AT %MW1738 : INT;
                                                "I.Corner_entrance_sensor_output",// AT %MW180 : INT;
                                                "I.Downport_sensor_output",// AT %MW182 : INT;
                                                "I.Store_CigNum",// AT %MW1880 : INT;
                                                "I.Store_CigNum2",// AT %MW1882 : INT;
                                                "I.Store_cig_speed",// AT %MW1884 : INT;
                                                "I.Slope_cig_speed",// AT %MW1886 : INT;
                                                "I.Lift_speed_rpm",// AT %MW1936 : INT;
                                                "I.Transfer_speed_rpm",// AT %MW2036 : INT;
                                                "I.MakerExport_Servo_FaultNum",// AT %MW2800 : DWORD;
                                                "I.Sample_Servo_FaultNum",// AT %MW2804 : DWORD;
                                                "I.Corner_Servo_FaultNum",// AT %MW2808 : DWORD;
                                                "I.Lift_Servo_FaultNum",// AT %MW2812 : DWORD;

                                                "I.Transfer_Servo_FaultNum",// AT %MW2816 : DWORD;
                                                "I.Slope_Servo_FaultNum",// AT %MW2820 : DWORD;
                                                "I.Store_Servo_FaultNum",// AT %MW2824 : DWORD;
                                                "I.DropTrans_Servo_FaultNum",// AT %MW2828 : DWORD;(*下降口输送伺服驱动器故障代码*)(***********7Axis增加下降口输送电机故障代码**************)

                                                "I.Corner_pid_sp",//:INT;
                                                "I.Corner_work_limit",//:INT;
                                                "I.Corner_work_off_delay",//:INT;
                                                "I.Corner_pid_p_gain",//:INT;
                                                "I.Corner_p_parameter",//:INT;
                                                "I.cig_dim",//:INT;
                                                "I.Store_CigIn_Comp_speed1",//:INT;
                                                "I.Store_CigIn_Comp_speed2",//:INT;
                                                "I.Downport_CigIn_hight2",//:INT;
                                                "I.Corner_pid_i_gain",//:INT;
                                                "I.Maker_MaxSpeedLimit",//:INT;
                                                "I.Packer_MaxSpeedLimit",//:INT;
                                                "I.Store_empty_position",//:INT;
                                                "I.Store_full_position",//:INT;
                                                "I.Packer_LowSpeed_position",//:INT;
                                                "I.Packer_enable_position",//:INT;
                                                "I.Maker_stop_position",//:INT;
                                                "I.Corner_entrance_hight_limit",//:INT;
                                                "I.Corner_entrance_low_limt",//:INT;
                                                "I.Downport_CigIn_hight1",//:INT;
                                                "I.Corner_pid_deadband",//:INT;
                                                "I.Downport_CigOut_hight1",//:INT;
                                                "I.Downport_CigOut_hight2",//:INT;
                                                "I.Store_CigOut_Comp_speed1",//:INT;
                                                "I.Store_CigOut_Comp_speed2",//:INT;
                                                "I.Downport_Highest_limit",//:INT;
                                                "I.Downport_Lowest_limit",//:INT;
                                                "I.Downport_CigIn_lowest_hight",//:INT;
                                                "I.Corner_pid_output",//:INT;
                                                "I.Corner_pid_pv",//:INT;
                                                "I.DownPort_hight",//:INT;
                                                "I.Store_percent",//:INT;
                                                "I.Corner_lowlimit",//:INT;
                                        };


    }
}
