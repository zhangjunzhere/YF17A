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
    class ToolTipHelper:IObserverResult
    {
        public const string TAG = "ToolTipHelp.cs";

        private Boolean mHelpState ;
        DispatcherTimer tm = new DispatcherTimer();
  
        private BeckHoff mBeckHoff;
        private Dictionary<String, ElementInfo> mStatusMap = new Dictionary<string, ElementInfo>();
        private Dictionary<String, Boolean> mLightMap = new Dictionary<string, Boolean>();
        private List<String> mFlashLights = new List<String>();


        private FrameworkElement panel;
        private TooltipControl tooltip = new TooltipControl();

        private class ElementInfo
        {
            public FrameworkElement Element {set;get;}
            public Boolean Flash { set; get; }
            public BitmapImage Light { set; get; }
            public Boolean Switch { set; get; }
        }

        public ToolTipHelper(FrameworkElement panel)
        {
            this.panel = panel;

            InitStatusMap();            
            //TooltipControl tooltip = new TooltipControl();
            Grid rootView =  panel.FindName("root") as Grid;
            rootView.Children.Add(tooltip);
            tooltip.registerObserver(this);

            panel.PreviewMouseUp +=new MouseButtonEventHandler(panel_PreviewMouseUp);
            tooltip.MouseUp +=new MouseButtonEventHandler(tooltip_MouseUp);
        }

        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (ToolbarMain.TAG.Equals(senderName) || ToolbarParameter.TAG.Equals(senderName))
            {
                if (ToolbarMain.BUTTON_HELP.Equals(senderValue))
                {
                    mHelpState = !mHelpState;
                }else if(ToolbarMain.BUTTON_HOME.Equals(senderValue))
                {
                    mHelpState = false;
                }
                UpdateHelpState();
            }
              //beckhoff changed 
            else if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                Object plcValue ;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);

                if (mStatusMap.ContainsKey(plcVarName))
                {
                    UpdateView(plcVarName, plcValue);
                }
            }     
        }

        public void Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);

           tm.Tick += new EventHandler(tm_Tick);      
           tm.Interval = TimeSpan.FromSeconds(1);
           tm.Start();     

           mHelpState = false;
           UpdateHelpState();

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

        public void Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            tm.Stop();
            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }       
       
    
        private void tm_Tick(object sender, EventArgs e)
       {           
            foreach(String item in mFlashLights)
            {
                ElementInfo info;
                mStatusMap.TryGetValue(item, out info);
                if (info.Element.Visibility == Visibility.Visible)
                {
                    info.Element.Visibility = Visibility.Hidden;
                }
                else
                {
                    info.Element.Visibility = Visibility.Visible;
                }
            }     
        }

        private void help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image helpIcon = sender as Image;
            Point pt1 = helpIcon.TranslatePoint(new Point(), panel);

            int panelWidth = Convert.ToInt32(panel.ActualWidth+panel.Margin.Left);

            int X0 =  (int)(pt1.X );
            int Y0 = (int)(pt1.Y + helpIcon.ActualHeight) ;

            if (X0 + tooltip.ActualWidth > panelWidth)
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
            foreach (KeyValuePair<String, ElementInfo> entry in mStatusMap)
            {
                string key = entry.Key;
                ElementInfo info = entry.Value;

                if (info.Element != null && info.Light != null)
                {
                    String helpIconName = key.Replace(".", "help_");
                    Image img = panel.FindName(helpIconName) as Image;
                    img.Visibility = visibility;
                }
            }

            if(!mHelpState){
                tooltip.Visibility = Visibility.Hidden;
            }            
        }      
      
        private void UpdateView(String viewTag, Object value)
        {
            ElementInfo info;
            mStatusMap.TryGetValue(viewTag, out info);
            FrameworkElement element = info.Element;

            if (element == null)
            {
                return;
            }

            if (element.GetType() == typeof(Image))
            {
                Image iv_light = element as Image;
               Boolean b = (Boolean)value;

               if (info.Switch)
               {
                   info.Element.Visibility = Visibility.Visible;
                   if (b)
                   {
                       info.Light = MainWindow.sGreenLight;
                   }
                   else
                   {
                       info.Light = MainWindow.sRedLight;
                   }
               }
               else
               {
                   info.Light = MainWindow.sYellowLight;
                   if (info.Flash == b)
                   {
                       mFlashLights.Add(viewTag);
                   }
                   else
                   {
                       mFlashLights.Remove(viewTag);
                   }
               }

               iv_light.Source = info.Light;
           
            }
            else if (element.GetType() == typeof(TextBlock))
            {
                int lValue = (int)value;
                TextBlock tv = element as TextBlock;
                tv.Text = lValue.ToString();
            }        
        }


        private void InitStatusMap()
        {
            BitmapImage mRedLight = MainWindow.sDisableLight;
            BitmapImage mYellowLight = MainWindow.sEnableLight;

            mBeckHoff = Utils.GetBeckHoffInstance();

            mStatusMap.Add(".Sample_entrance_sensor", new ElementInfo() { Light = mYellowLight, Flash = false }); // Sample_entrance_sensor	Bool	取样入口有烟传感器	B-PSW101	只读	指示灯	I2.3
            mStatusMap.Add(".Emergency_stop", new ElementInfo() { Light = mRedLight, Flash = false });  //Emergency_stop	Bool	紧急停止继电器	K01	只读	指示灯/报警条显示	I0.0
            mStatusMap.Add(".Elevater_e_stop", new ElementInfo() {  Light = mRedLight, Flash = false }); //Elevater_e_stop	Bool	提升机紧急停止按钮	SB105	只读	指示灯/报警条显示	I1.7
            mStatusMap.Add(".alarm_corner_entrance_jam", new ElementInfo() { Light = mRedLight, Flash = true });//alarm_corner_entrance_jam	Bool	弯道入口堵塞报警指示		只读	指示灯/报警条显示	M50.1

            mStatusMap.Add(".alarm_transfer_overload", new ElementInfo() { Light = mRedLight, Flash=true }); //alarm_transfer_overload	Bool	传送电机过载报警指示		只读	指示灯/报警条显示	M50.3
            mStatusMap.Add(".Transfer_cig_exist", new ElementInfo() { Light = mYellowLight, Flash = false }); //Transfer_cig_exist	Bool	高架烟支传感器	B-PSW201	只读	指示灯	I4.2
            mStatusMap.Add(".alarm_downport_entrance_jam", new ElementInfo() { Light = mRedLight, Flash = true }); //alarm_downport_entrance_jam	Bool	下降口入口堵塞报警指示		只读	指示灯/报警条显示	M50.2
            mStatusMap.Add(".StoreUnit_e_stop_button", new ElementInfo() { Light = mRedLight, Flash = false });  //StoreUnit_e_stop_button	Bool	存储器紧急停止按钮	SB205	只读	指示灯/报警条显示	I3.5

            mStatusMap.Add(".Slope_empty", new ElementInfo() { Light = mYellowLight, Flash = false });//Slope_empty	Bool	斜向通道空	B-PRX202	只读	指示灯	I4.1
            mStatusMap.Add(".Store_entrance_cig_exist", new ElementInfo() { Light = mYellowLight, Flash = true }); //Store_entrance_cig_exist	Bool	存储器入口有烟传感器	B-PSW301	只读	指示灯	I5.3
            mStatusMap.Add(".alarm_store_entrance_jam", new ElementInfo() { Light = mRedLight, Flash = true });  //alarm_store_entrance_jam	Bool	存储器入口堵塞报警指示		只读	指示灯/报警条显示	M50.5
            mStatusMap.Add(".alarm_store_overload", new ElementInfo() { Light = mRedLight, Flash = true }); //alarm_store_overload	Bool	存储器过载报警指示		只读	指示灯/报警条显示	M50.4

            mStatusMap.Add(".Store_full", new ElementInfo() { Light = mRedLight, Flash = true });  //Store_full	Bool	存储器满传感器	B-PRX301	只读	指示灯	I5.0
            mStatusMap.Add(".alarm_store_limit_on", new ElementInfo() { Light = mRedLight, Flash = true });  //alarm_store_limit_on	Bool	存储器极限位置到达报警指示		只读	指示灯	M60.2

            mStatusMap.Add(".Maker_enable_relay_Q", new ElementInfo() { Light = mRedLight, Flash = false, Switch=true }); //<!--Maker_enable_relay_Q	Bool	卷烟机允许(备用)	K101	只读	指示灯	Q5.4-->
            mStatusMap.Add(".Packer_enable_relay_Q", new ElementInfo() { Light = mRedLight, Flash = false, Switch = true }); // <!--Packer_enable_relay_Q	Bool	包装机允许（备用）	K107	只读	指示灯	Q8.2-->

            mStatusMap.Add(".Store_empty", new ElementInfo() { Light = mYellowLight, Flash = false });//Slope_empty	Bool	斜向通道空	B-PRX202	只读	指示灯	I4.1
        

            foreach (KeyValuePair<String, ElementInfo>  entry in mStatusMap)
            {
                String key = entry.Key;                
                String controlName = key.Replace(".", "iv_");
                ElementInfo info = entry.Value;
                info.Element = panel.FindName(controlName) as FrameworkElement;
                if (info.Element != null)
                {
                    info.Element.Visibility = Visibility.Hidden;
                }

                Image helpIcon = panel.FindName(key.Replace(".", "help_")) as Image;
                if (helpIcon != null)
                {
                    helpIcon.MouseUp += new MouseButtonEventHandler(help_MouseUp);
                }
            }
        }

        private void tooltip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mHelpState = false;
            UpdateHelpState();
            PageDataExchange context = PageDataExchange.getInstance();
            context.CommandObserver(PageParameterHelper.TAG, ToolbarParameter.TAG, ToolbarMain.BUTTON_HELP);
        }
    }
}

