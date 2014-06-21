using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Media;

namespace YF17A
{
    public class WarnningDataSource : IObserverResult
    {
        public const String TAG = "WarnningDataSource.xaml";
        public  Dictionary<String, ErrorInfo> mStatusMap = new Dictionary<string, ErrorInfo>(); //plcvarname, errorinfo
        private BeckHoff mBeckHoff;
        private static WarnningDataSource sInstance;
        private SortedDictionary<int, ErrorInfo> mSortedWarnningMap = new SortedDictionary<int, ErrorInfo>(); //errorinfo level, plcvarname
        private ErrorInfo ERROEINFO_NORMAL = new ErrorInfo() { level = 9999, description = "设备运转正常 " }; // 强制试运转中,注意设备人身安全!!
       // public List<ErrorInfo> mWarnningList = new List<ErrorInfo>();


        public class ErrorInfo
        {
            public int level { set; get; }
            public String description { set; get; }
            public String address { set; get; }
            public String code { set; get; }
            public String solution { set; get; }
            public String whenhappened { set; get; }
            public String whenresolved { set; get; } //could be empty
            public String category { set; get; }
            public Boolean trigger { set;get;}//if true, trigger

          //  public String resolvedSerial { get {  if() return} }

            public int category_id
            {
                get
                {
                    int category = Warnning.ERROR;
                    if (level > 100 && level < 200)
                    {
                        category = Warnning.WANNING;
                    }
                    else if (level > 200 && level < 300)
                    {
                        category = Warnning.INFO;
                    }
                    return category;
                }
            }

            public String foreground
            {
                get
                {
                    Color c = Colors.Black;
                    if (level > 100 && level < 200)
                    {
                        c = Colors.Red;
                    }
                    else if (level > 200 && level < 300)
                    {
                        c = Colors.DarkOrange;
                    }
                    return c.ToString();
                }
            }         
        }

        private WarnningDataSource()
        {
            mBeckHoff = Utils.GetBeckHoffInstance();
            mBeckHoff.RegisterObserver(TAG, this);

            InitStatusMap();
            InitWarnningList();   
        }

        public static WarnningDataSource GetInstance()
        {
            if(sInstance == null){
                sInstance = new WarnningDataSource();
              //  sInstance.RegisterObserver();
            }
            return sInstance;
        }

        public Dictionary<String, ErrorInfo> GetWarnningMap()
        {
            return mStatusMap;
        }

        #region toolbarparameter
        public void RegisterObserver()
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            //beck hoff register
               
           
        }

        public void UnregisterObserver()
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }
        #endregion

        #region IActionBar members       
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object resultValue;
            Object senderValue;

            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_RESULT_VALUE, out resultValue);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
         
            //beckhoff changed 
            if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                if (mStatusMap.ContainsKey(plcVarName))
                {
                    ErrorInfo info;
                    mStatusMap.TryGetValue(plcVarName, out info);            
                    Boolean expected = ExpectStatus( plcVarName,  info);
                    PageDataExchange context = PageDataExchange.getInstance();

                    if (expected)
                    {
                        AddWarningItem(info);  
                    }
                    else
                    {
                        RemoveWarningItem(info);                   
                    }
                    context.NotifyObserverChanged(PageWarnningHeader.TAG, TAG, info);
                   
                }
            }
        }
        #endregion

        public int GetWarnningCount()
        {
            int count = mSortedWarnningMap.Count;
            if (mSortedWarnningMap.ContainsKey(ERROEINFO_NORMAL.level))
            {
                count--;
            }
            return count;
        }

        public List<ErrorInfo> GetWarnningList()
        {
            return mSortedWarnningMap.Values.ToList();
        }
        public void AddWarningItem(ErrorInfo info)
        {         
            int key = info.level;
            if (ERROEINFO_NORMAL != info)
            {
                mSortedWarnningMap.Remove(ERROEINFO_NORMAL.level);
            }
            mSortedWarnningMap.Add(key, info);
            Warnning.write(info);
        }

        public void RemoveWarningItem( ErrorInfo info)
        {    

            foreach (KeyValuePair<int, ErrorInfo> entry in  mSortedWarnningMap)
            {
                if (info.level.Equals(entry.Key))
                {
                    mSortedWarnningMap.Remove(entry.Key);                 
                    Warnning.resolve(info);
                    break;
                }
            }  
            if(mSortedWarnningMap.Count == 0)
            {
                mSortedWarnningMap.Add(ERROEINFO_NORMAL.level, ERROEINFO_NORMAL);
            }             
        }       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plcVarName"></param>
        /// <returns>ture added, false removed</returns>
        public Boolean IsWarnningAdded( ErrorInfo info)
        {
            Boolean result = false;
            if(info != null){
                result = mSortedWarnningMap.ContainsKey(info.level);
            }
            return  result;
        }

        private void InitWarnningList()
        {
            foreach (KeyValuePair<String, ErrorInfo> entry in mStatusMap)
            {
                String plcVarName = entry.Key;
                ErrorInfo info = entry.Value;
                Boolean expected = ExpectStatus(plcVarName, info);
                if (expected)
                {
                    mSortedWarnningMap.Add(info.level, info);
                }
            }          
        }

        private Boolean ExpectStatus(String plcVarName, ErrorInfo info)
        {         
            Object plcValue;
            mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);
          
            Boolean expected = info.trigger ;
            return expected.Equals(plcValue);
        }

        private void InitStatusMap()
        {
            mStatusMap.Add(".StoreUnit_e_stop_button", new ErrorInfo() { level = 101, description =  "下降口急停按钮按下", address = "I4.5",trigger=false,  code = "S205", solution = "请松开急停按钮并按压复位按钮" });
            mStatusMap.Add(".DropTransLimitLevel", new ErrorInfo() { level = 102, description = "下降口输送极限料位", address = "I2.7", trigger = false, code = "S105", solution = "请松开急停按钮并按压复位按钮" });
            mStatusMap.Add(".Emergency_stop", new ErrorInfo() { level = 103, description =  "紧急停止回路断开", address = "I0.0", trigger=false, code = "K01", solution = "检查紧急停止按钮或请维修人员检修" });
            mStatusMap.Add(".MakerExit_power", new ErrorInfo() { level = 104, description =  "烟机出口伺服主电源断开", address = "I0.1", trigger=false, code = "Q06", solution = "请维修人员检修。" });
            mStatusMap.Add(".Sample_power", new ErrorInfo() { level = 105, description =  "取样伺服主电源断开", address = "I0.2", trigger=false, code = "Q07", solution = "请维修人员检修。" });
            mStatusMap.Add(".Corner_power", new ErrorInfo() { level = 106, description =  "弯道伺服主电源断开", address = "I0.3", trigger=false,  code = "Q08", solution = "请维修人员检修。" });
            mStatusMap.Add(".Lift_power", new ErrorInfo() { level = 107, description =  "提升伺服主电源断开", address = "I0.4", trigger=false,  code = "Q09", solution = "请维修人员检修。" });
            mStatusMap.Add(".Transfer_power", new ErrorInfo() { level = 108, description =  "传送伺服主电源断开", address = "I0.5", trigger=false,  code = "Q10", solution = "请维修人员检修。" });
            mStatusMap.Add(".Slope_power", new ErrorInfo() { level = 109, description =  "斜向伺服主电源断开", address = "I0.6", trigger=false,  code = "Q11", solution = "请维修人员检修。" });
            mStatusMap.Add(".Store_power", new ErrorInfo() { level = 110, description =  "存储伺服主电源断开", address = "I0.7", trigger=false,  code = "Q12", solution = "请维修人员检修。" });
            mStatusMap.Add(".MakerExit_servo_ethercat_fault", new ErrorInfo() { level = 111, description = "烟机出口伺服通信卡故障", address = "M2600.0", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Sample_servo_ethercat_fault", new ErrorInfo() { level = 112, description = "取样伺服通讯卡故障", address = "M2600.1", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Corner_servo_ethercat_fault", new ErrorInfo() { level = 113, description = "弯道伺服通讯卡故障", address = "M2600.2", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Lift_servo_ethercat_fault", new ErrorInfo() { level = 114, description = "提升伺服通讯卡故障", address = "M2600.3", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Transfer_servo_ethercat_fault", new ErrorInfo() { level = 115, description = "传送伺服通讯卡故障", address = "M2600.4", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Slope_servo_ethercat_fault", new ErrorInfo() { level = 116, description = "斜向伺服通讯卡故障", address = "M2600.5", trigger = true, code = "", solution = "请断电重启PLC" });
            mStatusMap.Add(".Store_servo_ethercat_fault", new ErrorInfo() { level = 117, description = "存储伺服通讯卡故障", address = "M2600.6", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input1_ethercat_fault", new ErrorInfo() { level = 118, description = "数字量输入模块1通讯故障", address = "M2601.1", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input2_ethercat_fault", new ErrorInfo() { level = 119, description = "数字量输入模块2通讯故障", address = "M2601.2", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input3_ethercat_fault", new ErrorInfo() { level = 120, description = "数字量输入模块3通讯故障", address = "M2601.3", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input4_ethercat_fault", new ErrorInfo() { level = 121, description = "数字量输入模块4通讯故障", address = "M2601.4", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input5_ethercat_fault", new ErrorInfo() { level = 122, description = "数字量输入模块5通讯故障", address = "M2601.5", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input6_ethercat_fault", new ErrorInfo() { level = 123, description = "数字量输入模块6通讯故障", address = "M2601.6", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Input7_ethercat_fault", new ErrorInfo() { level = 124, description = "数字量输入模块7通讯故障", address = "M2601.7", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Output1_ethercat_fault", new ErrorInfo() { level = 125, description = "数字量输出模块1通讯故障", address = "M2602.0", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Output2_ethercat_fault", new ErrorInfo() { level = 126, description = "数字量输出模块2通讯故障", address = "M2602.1", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Output3_ethercat_fault", new ErrorInfo() { level = 127, description = "数字量输出模块3通讯故障", address = "M2602.2", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Digital_Output4_ethercat_fault", new ErrorInfo() { level = 128, description = "数字量输出模块4通讯故障", address = "M2602.3", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Analog_Input1_ethercat_fault", new ErrorInfo() { level = 129, description = "模拟量输入模块1通讯故障", address = "M2602.4", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".Analog_Input2_ethercat_fault", new ErrorInfo() { level = 130, description = "模拟量输入模块2通讯故障", address = "M2602.5", trigger = true, code = "", solution = "请断电重启设备" });
            mStatusMap.Add(".alarm_encoder_fault", new ErrorInfo() { level = 131, description = "存储编码器故障", address = "M71.3", code = "", trigger = true, solution = "请维修人员检修。" });
            mStatusMap.Add(".MakerExit_servo_fault", new ErrorInfo() { level = 132, description =  "烟机出口伺服故障", address = "I1.2", trigger=false, code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Sample_servo_fault", new ErrorInfo() { level = 133, description =  "取样伺服故障", address = "I1.3", trigger=false, code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Corner_servo_fault", new ErrorInfo() { level = 134, description =  "弯道伺服故障", address = "I1.4", trigger=false, code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Lift_servo_fault", new ErrorInfo() { level = 135, description =  "提升伺服故障", address = "I1.5", trigger=false,  code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Transfer_servo_fault", new ErrorInfo() { level = 136, description =  "传送伺服故障", address = "I1.6", trigger=false,  code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Slope_servo_fault", new ErrorInfo() { level = 137, description =  "斜向伺服故障", address = "I1.7", trigger=false, code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".Store_servo_fault", new ErrorInfo() { level = 138, description =  "存储器伺服故障", address = "I1.8", trigger=false,  code = "", solution = "按压控制盘上复位按钮或请维修人员检修。" });
            mStatusMap.Add(".alarm_store_limit_on", new ErrorInfo() { level = 139, description = "存储器超越极限", address = "M60.2", trigger = true, code = "", solution = "请维修人员维修!" });
            mStatusMap.Add(".alarm_transfer_overload", new ErrorInfo() { level = 140, description = "提升机过载", address = "M50.3", trigger = true, code = "", solution = "请维修人员维修!" });
            mStatusMap.Add(".alarm_store_overload", new ErrorInfo() { level = 141, description = "存储器过载", address = "M50.4", trigger = true, code = "", solution = "请维修人员维修!" });
            mStatusMap.Add(".alarm_sample_entrance_jam", new ErrorInfo() { level = 142, description = "取样入口堵塞", address = "M50.0", trigger = true, code = "", solution = "处理乱烟后复位开机。" });
            mStatusMap.Add(".alarm_corner_entrance_jam", new ErrorInfo() { level = 143, description = "提升机入口堵塞", address = "M50.1", trigger = true, code = "", solution = "处理乱烟后复位开机。" });
            mStatusMap.Add(".alarm_downport_entrance_jam", new ErrorInfo() { level = 144, description = "下降口堵塞", address = "M50.2", trigger = true, code = "", solution = "处理乱烟后复位开机。" });
            mStatusMap.Add(".alarm_store_entrance_jam", new ErrorInfo() { level = 145, description = "存储器入口堵塞", address = "M50.5", trigger = true, code = "", solution = "处理乱烟后复位开机。" });
            mStatusMap.Add(".alarm_store_full", new ErrorInfo() { level = 146, description = "存储器满", address = "M71.2", code = "", trigger = true, solution = "" });

            mStatusMap.Add(".test_run_unprotected", new ErrorInfo() { level = 201, description = "强制试运转中,注意设备人身安全!!", address = "M1000.7", trigger = true, code = "", solution = "" });

            mStatusMap.Add(".Elevater_manual_discharge", new ErrorInfo() { level = 301, description = "提升机手动排空中", address = "M501.0", trigger = true, code = "", solution = "" });
            mStatusMap.Add(".StoreUnit_discharge_run", new ErrorInfo() { level = 302, description = "存储器全排空中", address = "M553.7", trigger = true, code = "", solution = "" });
            mStatusMap.Add(".StoreUnit_Stop", new ErrorInfo() { level = 303, description = "斜向存储停机", address = "M2603.0", code = "", trigger = true, solution = "" });
            mStatusMap.Add(".Elevater_Stop", new ErrorInfo() { level = 304, description = "提升机停机", address = "M2603.1", code = "", trigger = true, solution = "" });
            mStatusMap.Add(".Manual_Run", new ErrorInfo() { level = 305, description = "机器手动运行", address = "M2603.2", code = "", trigger = true, solution = "" });
            mStatusMap.Add(".StoreUnit_man_run", new ErrorInfo() { level = 306, description = "斜向存储手动运行", address = "M550.1", trigger = true, code = "", solution = "" });
            mStatusMap.Add(".Elevater_Manual_Run", new ErrorInfo() { level = 307, description = "提升机手动运行", address = "M2603.4", trigger = true, code = "", solution = "" });
            mStatusMap.Add(".Maker_run", new ErrorInfo() { level = 308, description =  "卷烟机停机", address = "M80.0", trigger=false, code = "", solution = "" });
            mStatusMap.Add(".packer_run", new ErrorInfo() { level = 309, description =  "包装机停机", address = "M80.1", trigger=false, code = "", solution = "" });
            mStatusMap.Add(".test_run", new ErrorInfo() { level = 310, description = "机器试运转中", address = "M1000.6", code = "", trigger = true, solution = "" });
            mStatusMap.Add(".Auto_Run", new ErrorInfo() { level = 311, description = "机器自动运行", address = "M2603.3", code = "", trigger = true, solution = "" });

        }
    }
}


   

