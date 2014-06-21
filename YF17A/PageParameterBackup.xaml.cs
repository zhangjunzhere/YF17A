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
    public partial class PageParameterBackup : Page, IObserverResult
    {
        public const String TAG = "PageParameterBackup.xaml";

        private Dictionary<String, Parameter.Record> mStatusMap = new Dictionary<String, Parameter.Record>(); //plcvarname, description    
     
        private List<Parameter.Record> mCurrentParameters ;
        private List<Parameter.Record> mHistoryParameters;
        private String mHistoryQuality;
        private List<Parameter.Record> mHistoryGoodParameters;
        private List<Parameter.Record> mHistoryBetterParameters;
        private List<Parameter.Record> mHistoryBestParameters;

        BeckHoff mBeckHoff = Utils.GetBeckHoffInstance();

        public PageParameterBackup()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);

            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
           
            initView();
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);      
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {          
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override beckhoff changed       
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;          
            Object senderValue;

            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);

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
        #endregion

      
        private void initView()
        {
            InitStatusMap();

            foreach (KeyValuePair<String, Parameter.Record> item in mStatusMap)
            {
                String plcVarName = item.Key;
                Object value;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out value);
                Parameter.Record record = item.Value;
                record.Value =value.ToString();
            }

            this.lv_curparam.ItemsSource = mCurrentParameters;           
            this.tabControl1.SelectedIndex = 0;
        }

        private void UpdateView(String plcVarName, Object plcValue)
        {                
            Object value;
            mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out value);

            Parameter.Record record;
            mStatusMap.TryGetValue(plcVarName, out record);

            record.Value = value.ToString();
            this.lv_curparam.Items.Refresh();
        }
        
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tab = (TabControl)sender;
            switch (tab.SelectedIndex)
            {
                case 0 :
                     mHistoryQuality = Parameter.GOOD;
                    mHistoryParameters = mHistoryGoodParameters;
                    break;
                case 1:
                     mHistoryQuality = Parameter.BETTER;
                     mHistoryParameters = mHistoryBetterParameters;
                    break;
                case 2:
                    mHistoryQuality = Parameter.BEST;
                    mHistoryParameters = mHistoryBestParameters;
                    break;
                default:
                    break;
            }
            RefreshHistoryParams(false);
        }
        
        private void RefreshHistoryParams(Boolean forced)
        {
            if (mHistoryParameters == null || forced)
            {
                mHistoryParameters = Parameter.getParameterList(mHistoryQuality);
            }
            this.lv_historyparams.ItemsSource = mHistoryParameters;
            this.lv_historyparams.Items.Refresh();
        }

        private void btn_backup_Click(object sender, RoutedEventArgs e)
        {
            Parameter.backup(mCurrentParameters,mHistoryQuality);
            RefreshHistoryParams(true);
        }

        private void btn_restore_Click(object sender, RoutedEventArgs e)
        {
            Parameter.restore(mHistoryQuality);
        }

        public void InitStatusMap()
        {
            String[]  parameters = new String[]{
                    ".Corner_pid_sp",
                    ".Corner_pid_output",
                    ".Corner_pid_pv",
                    ".DownPort_hight",
                    ".Corner_work_limit",
                    ".Corner_work_off_delay",
                    ".Corner_pid_p_gain",
                    ".Corner_p_parameter",
                    ".cig_dim",
                    ".Store_percent",
                    ".Store_CigIn_Comp_speed1",
                    ".Store_CigIn_Comp_speed2",
                    ".Downport_CigIn_hight2",
                    ".Corner_lowlimit",
                    ".Corner_pid_i_gain",
                    ".Maker_MaxSpeedLimit",
                    ".Packer_MaxSpeedLimit",
                    ".Store_empty_position",
                    ".Store_full_position",
                    ".Packer_LowSpeed_position",
                    ".Packer_enable_position",
                    ".Maker_stop_position",
                    ".Corner_entrance_hight_limit",
                    ".Corner_entrance_low_limt",
                    ".Downport_CigIn_hight1",
                    ".Corner_pid_deadband",
                    ".Downport_CigOut_hight1",
                    ".Downport_CigOut_hight2",
                    ".Store_CigOut_Comp_speed1",
                    ".Store_CigOut_Comp_speed2",
                    ".Downport_Highest_limit",
                    ".Downport_Lowest_limit",
                    ".Downport_CigIn_lowest_hight",
                    ".Downport_comp_output",
                    ".Maker_cig_speed",
                    ".Sample_cig_speed",
                    ".Corner_cig_speed",
                    ".Packer_cig_speed",
                    ".Life_cig_speed",
                    ".Transfer_cig_speed",                                                
                    ".Sample_speed_rpm",                                              
                    ".Corner_speed_rpm",                                               
                    ".Slope_speed_rpm",
                    ".Store_speed_rpm",                                                
                    ".Corner_entrance_sensor_output",
                    ".Downport_sensor_output",
                    ".Store_CigNum",
                    ".Store_CigNum2",
                    ".Store_cig_speed",
                    ".Slope_cig_speed",
                    ".Lift_speed_rpm",
                    ".test_maker_speed",
                    ".test_packer_speed",
                    ".Transfer_speed_rpm",
                    ".Corner_manual_speed",
                    ".Store_manual_speed",
                    ".Lift_p_parameter",
                    ".Transfer_p_parameter",
                    ".MakerExport_cig_speed",
                    ".MakerExport_p_parameter",
            };
            mCurrentParameters = new List<Parameter.Record>();  
            foreach (String name in parameters)
            {
                String description;
                mBeckHoff.plcVarDescriptionMap.TryGetValue(name, out description);

                Parameter.Record record = new Parameter.Record();
                record.Name = name;
                record.Description = description;
                mCurrentParameters.Add(record);
                mStatusMap.Add(record.Name, record);
            }
        }
    }
}

