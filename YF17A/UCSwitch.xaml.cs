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
using System.Drawing;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for UCSwitch.xaml
    /// </summary>
    public partial class UCSwitch : UserControl, IObserverResult
    {
        private String TAG;
        private BeckHoff mBeckHoff;
        private Dictionary<String, String> mStatusMap = new Dictionary<String, String>();     //key: plcVarName, value: TextBlock text

        HashSet<String> mDynamicShowList = new HashSet<string>();

        public UCSwitch() 
        {
            InitializeComponent();
            mBeckHoff = Utils.GetBeckHoffInstance();
            
            InitStatusMap();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {            
            TAG = this.Name.Replace("btn_", ".");
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);

            //init views           
            Object value;
            mBeckHoff.plcVarUserdataMap.TryGetValue(TAG, out value);
            UpdateView(TAG, value);            
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {           
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }

        //override
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

                if (TAG.Equals(plcVarName))
                {
                    UpdateView(plcVarName, plcValue);
                }
            }
            else if (User.TAG.Equals(senderName))
            {
                if (mDynamicShowList.Contains(this.TAG))
                {
                    Utils.ShowPrevilageControl(this);
                }        
            }
        }
        private void UpdateView(String viewTag, Object value)
        {
            String  content;
            mStatusMap.TryGetValue(viewTag, out content);
            this.tb_description.Text = content;         
            Boolean b = (Boolean)value;                 
            if (b)
            {
                this.iv_status.Source = MainWindow.sSwitchOn;
                this.tb_description.Foreground = new SolidColorBrush(Colors.Green );
            }
            else
            {
                this.iv_status.Source = MainWindow.sSwitchOff;
                this.tb_description.Foreground = new SolidColorBrush(Colors.Red); ;
            }

            if (mDynamicShowList.Contains(this.TAG))
            {
                Utils.ShowPrevilageControl(this);
            }           
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {                    
            Object value;
            mBeckHoff.plcVarUserdataMap.TryGetValue(TAG, out value);
            mBeckHoff.writeBoolean(TAG, !(Boolean)value);
        }

        private void InitStatusMap()
        {
            mStatusMap.Add(".StoreUnit_discharge_button", "全排空"); //<!--全排空：M2603.6 StoreUnit_discharge_button-->
            mStatusMap.Add(".Elevater_man_paikong", "手动排空");//<!--手动排空：M2603.7 Elevater_man_paikong-->
             mStatusMap.Add(".test_run", "试运行");  //btn_test_run
            mStatusMap.Add(".test_run_unprotected", "强制试运行");  //btn_test_run   
            mStatusMap.Add(".Store_set_zero", "手动校零");  //<!-- 手动校零地址：M1000.0 Store_set_zero -->

          //  mDynamicShowList.Add(".test_run_unprotected");
           // mDynamicShowList.Add(".test_run");
        }
    }
}
