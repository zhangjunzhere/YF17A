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
        private HashSet<String> mNagtiveVariables = new HashSet<String>();
        private HashSet<String> mNoUserVariables = new HashSet<String>();

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

            //init nagtive plc var            
            mNagtiveVariables.Add(".Store_cig_speed");
            mNagtiveVariables.Add(".Slope_cig_speed");
            mNagtiveVariables.Add(".Store_speed_rpm");
            mNagtiveVariables.Add(".Slope_speed_rpm");
            mNagtiveVariables.Add(".Store_manual_speed");
            mNagtiveVariables.Add(".Corner_pid_output");
            mNagtiveVariables.Add(".Downport_comp_output");      
      
            //operation no user needed
            mNoUserVariables.Add(".StoreUnit_discharge_button");
            mNoUserVariables.Add(".Elevater_man_paikong");
            mNoUserVariables.Add(".Store_manual_speed");
            mNoUserVariables.Add(".Corner_manual_speed");
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
                if (mNagtiveVariables.Contains(plcVarName))
                {
                    //convert unsigned integer into integer
                    int lValue = Convert.ToInt32(value);
                    value = lValue - ((lValue <= 32767) ? 0 : 65536);
                }             

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
            if (!User.GetInstance().GetCurrentUserInfo().IsLogin && !mNoUserVariables.Contains(plcVarName))
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
                    if (fValue < limit.min || fValue >(limit.max + 0.01))
                    {
                        MessageBox.Show("参数超出范围!!!");
                        return;
                    } 
                }
                writeAny(plcVarName, Convert.ToInt16(fValue));
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
            plcVarThreadHoldMap.Add(".Corner_pid_output", new ThresHold() { max = 10000, min = -10000, ratio = 100 }); //弯道控制补偿输出（DB8.DBW10）Corner_pid_output
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
            plcVarDescriptionMap.Add(".Elevater_reset_pb", "提升机复位按钮");
            plcVarDescriptionMap.Add(".Elevater_stop_pb", "提升机停机按钮");
            plcVarDescriptionMap.Add(".Elevater_e_stop", "提升机紧急停止按钮");
            plcVarDescriptionMap.Add(".Lift_power", "提升伺服主电源");
            plcVarDescriptionMap.Add(".Spare21", "备用");
            plcVarDescriptionMap.Add(".Spare22", "备用");
            plcVarDescriptionMap.Add(".Sample_entrance_sensor", "取样入口有烟传感器");
            plcVarDescriptionMap.Add(".Sample_entrance_jam_sensor", "取样入口堵塞传感器（备用）");
            plcVarDescriptionMap.Add(".Corner_entrance_jam_sensor", "弯道入口堵塞传感器");
            plcVarDescriptionMap.Add(".Spare26", "备用");
            plcVarDescriptionMap.Add(".Transfer_power", "传送伺服主电源");
            plcVarDescriptionMap.Add(".StoreUnit_man_auto_sw", "存储器手动/自动方式");
            plcVarDescriptionMap.Add(".StoreUnit_discharge_button", "全排空");
            plcVarDescriptionMap.Add(".StoreUnit_start_button", "存储器启动按钮");
            plcVarDescriptionMap.Add(".StoreUnit_reset_button", "存储器复位按钮");
            plcVarDescriptionMap.Add(".StoreUnit_stop_button", "存储器停机按钮");
            plcVarDescriptionMap.Add(".StoreUnit_e_stop_button", "存储器紧急停止按钮");
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
                                                "I.Corner_pid_sp",
                                                "I.Corner_work_limit",
                                                "I.Corner_work_off_delay",
                                                "I.Corner_pid_p_gain",
                                                "I.Corner_p_parameter",
                                                "I.cig_dim",
                                                "I.Store_CigIn_Comp_speed1",
                                                "I.Store_CigIn_Comp_speed2",
                                                "I.Downport_CigIn_hight2",
                                                "I.Corner_pid_i_gain",
                                                "I.Maker_MaxSpeedLimit",
                                                "I.Packer_MaxSpeedLimit",
                                                "I.Store_empty_position",
                                                "I.Store_full_position",
                                                "I.Packer_LowSpeed_position",
                                                "I.Packer_enable_position",
                                                "I.Maker_stop_position",
                                                "I.Corner_entrance_hight_limit",
                                                "I.Corner_entrance_low_limt",
                                                "I.Downport_CigIn_hight1",
                                                "I.Corner_pid_deadband",
                                                "I.Downport_CigOut_hight1",
                                                "I.Downport_CigOut_hight2",
                                                "I.Store_CigOut_Comp_speed1",
                                                "I.Store_CigOut_Comp_speed2",
                                                "I.Downport_Highest_limit",
                                                "I.Downport_Lowest_limit",
                                                "I.Downport_CigIn_lowest_hight",
                                                "B.Store_set_zero",
                                                "B.test_run",
                                                "B.StoreUnit_discharge_button",
                                                "B.Elevater_man_paikong",
                                                "B.test_run_unprotected",
                                                "I.test_maker_speed",
                                                "I.test_packer_speed",
                                                "I.Corner_manual_speed",
                                                "I.Store_manual_speed",
                                                "I.Lift_p_parameter",
                                                "I.Transfer_p_parameter",
                                                "I.MakerExport_p_parameter",
                                                "B.Emergency_stop",
                                                "B.MakerExit_power",
                                                "B.Sample_power",
                                                "B.Corner_power",
                                                "B.Lift_power",
                                                "B.Transfer_power",
                                                "B.Slope_power",
                                                "B.Store_power",
                                                "B.Spare10",
                                                "B.Spare11",
                                                "B.MakerExit_servo_fault",
                                                "B.Sample_servo_fault",
                                                "B.Corner_servo_fault",
                                                "B.Lift_servo_fault",
                                                "B.Transfer_servo_fault",
                                                "B.Slope_servo_fault",
                                                "B.Store_servo_fault",
                                                "B.Spare21",
                                                "B.Spare22",
                                                "B.Elevater_man_auto_sw",
                                                "B.Elevater_start_pb",
                                                "B.Elevater_reset_pb",
                                                "B.Elevater_stop_pb",
                                                "B.Elevater_e_stop",
                                                "B.Spare30",
                                                "B.Spare31",
                                                "B.MakerExit_sensor",
                                                "B.Sample_entrance_sensor",
                                                "B.Sample_entrance_jam_sensor",
                                                "B.Corner_entrance_jam_sensor",
                                                "B.MakerExit_jam_sensor",
                                                "B.Spare37",
                                                //"B.StoreUnit_man_auto_sw",
                                                //"B.Spare41",
                                                //"B.StoreUnit_start_button",
                                                //"B.StoreUnit_reset_button",
                                                //"B.StoreUnit_stop_button",
                                                //"B.StoreUnit_e_stop_button",
                                                //"B.Spare46",
                                                //"B.Spare47",
                                                "B.Downport_jam_sensor",
                                                "B.Slope_empty",
                                                "B.Transfer_cig_exist",
                                                "B.Transfer_overload_sensor",
                                                "B.Spare44",
												"B.Spare45",
                                                "B.StoreUnit_e_stop_button",
                                                "B.Spare46",
                                                "B.Spare47",
                                                "B.Store_full",
                                                "B.Store_empty",
                                                "B.Store_overload",
                                                "B.Store_entrance_cig_exist",
                                                "B.Store_entrance_jam",
                                                "B.Store_overlimit",
                                                "B.Store_running",
                                                "B.Store_enabled",
                                                "B.sample_servo_enable_Q",
                                                "B.Corner_servo_enable_Q",
                                                "B.Lift_servo_enable_Q",
                                                "B.Transfer_servo_enable_Q",
                                                "B.Slope_servo_enable_Q",
                                                "B.Store_servo_enable_Q",
                                                "B.SpareOutput46",
                                                "B.SpareOutput47",
                                                "B.Elevater_start_Q",
                                                "B.Elevater_reset_Q",
                                                "B.Elevater_stop_Q",
                                                "B.Store_FaultReset_Q",
                                                "B.Maker_enable_relay_Q",
                                                "B.SpareOutput55",
                                                "B.SpareOutput56",
                                                "B.Maker_QuickStop_Q",
                                                "B.SpareOutput80",
                                                "B.SpareOutput81",
                                                "B.Packer_enable_relay_Q",
                                                "B.Packer_LowSpeed_request_Q",
                                                "B.SpareOutput84",
                                                "B.SpareOutput85",
                                                "B.SpareOutput86",
                                                "B.StoreUnit_start_Q",
                                                "B.StoreUnit_reset_Q",
                                                "B.StoreUnit_stop_Q",
                                                "B.SpareOutput92",
                                                "B.SpareOutput93",
                                                "B.SpareOutput94",
                                                "B.SpareOutput95",
                                                "B.SpareOutput96",
                                                "B.SpareOutput97",
                                                "B.MakerExit_servo_enable",
                                                "B.MakerExit_servo_initialized",
                                                "B.sample_servo_enable",
                                                "B.sample_servo_initialized",
                                                "B.Corner_servo_enable",
                                                "B.Corner_servo_initialized",
                                                "B.Slope_servo_enable",
                                                "B.Slope_servo_initialized",
                                                "B.Store_servo_enable",
                                                "B.Lift_servo_enable",
                                                "B.Lift_servo_initialized",
                                                "B.Transfer_servo_enable",
                                                "B.Transfer_servo_initialized",
                                                "B.alarm_sample_entrance_jam",
                                                "B.alarm_corner_entrance_jam",
                                                "B.alarm_downport_entrance_jam",
                                                "B.alarm_transfer_overload",
                                                "B.alarm_store_overload",
                                                "B.alarm_store_entrance_jam",
                                                "B.Elevater_manual_discharge",
                                                "B.Elevater_auto_run",
                                                "B.StoreUnit_man_run",
                                                "B.StoreUnit_auto_run",
                                                "B.StoreUnit_discharge_run",
                                                "B.alarm_store_limit_on",
                                                "B.alarm_store_full",
                                                "B.alarm_encoder_fault",
                                                "B.Maker_run",
                                                "B.packer_run",
                                                "B.MakerExit_servo_ethercat_fault",
                                                "B.Sample_servo_ethercat_fault",
                                                "B.Corner_servo_ethercat_fault",
                                                "B.Lift_servo_ethercat_fault",
                                                "B.Transfer_servo_ethercat_fault",
                                                "B.Slope_servo_ethercat_fault",
                                                "B.Store_servo_ethercat_fault",
                                                "B.Spare26007",
                                                "B.Spare26010",
                                                "B.Digital_Input1_ethercat_fault",
                                                "B.Digital_Input2_ethercat_fault",
                                                "B.Digital_Input3_ethercat_fault",
                                                "B.Digital_Input4_ethercat_fault",
                                                "B.Digital_Input5_ethercat_fault",
                                                "B.Digital_Input6_ethercat_fault",
                                                "B.Digital_Input7_ethercat_fault",
                                                "B.Digital_Output1_ethercat_fault",
                                                "B.Digital_Output2_ethercat_fault",
                                                "B.Digital_Output3_ethercat_fault",
                                                "B.Digital_Output4_ethercat_fault",
                                                "B.Analog_Input1_ethercat_fault",
                                                "B.Analog_Input2_ethercat_fault",
                                                "B.StoreUnit_Stop",
                                                "B.Elevater_Stop",
                                                "B.Manual_Run",
                                                "B.Auto_Run",
                                                "B.Elevater_Manual_Run",
                                                "B.test_run_light",
                                                "I.Corner_pid_output",
                                                "I.Corner_pid_pv",
                                                "I.DownPort_hight",
                                                "I.Store_percent",
                                                "I.Downport_comp_output",
                                                "I.Maker_cig_speed",
                                                "I.Sample_cig_speed",
                                                "I.Corner_cig_speed",
                                                "I.Packer_cig_speed",
                                                "I.Life_cig_speed",
                                                "I.Transfer_cig_speed",
                                                "I.MakerExport_cig_speed",
                                                "I.MakerExport_speed_rpm",
                                                "I.Sample_speed_rpm",
                                                "I.Corner_speed_rpm",
                                                "I.Slope_speed_rpm",
                                                "I.Store_speed_rpm",
                                                "I.Corner_entrance_sensor_output",
                                                "I.Downport_sensor_output",
                                                "I.Store_CigNum",
                                                "I.Store_CigNum2",
                                                "I.Store_cig_speed",
                                                "I.Slope_cig_speed",
                                                "I.Lift_speed_rpm",
                                                "I.Transfer_speed_rpm",

                                                "I.MakerExport_Servo_FaultNum",
                                                "I.Sample_Servo_FaultNum",
                                                "I.Corner_Servo_FaultNum",
                                                "I.Lift_Servo_FaultNum",
                                                "I.Transfer_Servo_FaultNum",
                                                "I.Slope_Servo_FaultNum",
                                                "I.Store_Servo_FaultNum",
                                                "I.Corner_lowlimit",   
                                                "I.CigMakingSpeed",
	                                            "I.PackingSpeed",      
	                                            "I.PotenSpeed",     
	                                            "I.ElevatorLevel",    
	                                            "I.DropLevel",      
                                        };


    }
}
