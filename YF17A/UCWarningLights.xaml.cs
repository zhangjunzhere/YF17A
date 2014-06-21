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
    /// <summary>
    /// Interaction logic for UCWarningLights.xaml
    /// </summary>
    public partial class UCWarningLights : UserControl,IObserverResult
    {
        public const String TAG = "UCWarningLights.xaml";
        DispatcherTimer tm = new DispatcherTimer();
        private ToolTipHelper mTooltipHelper ;
        BeckHoff mBeckHoff;
        private Dictionary<String, FrameworkElement> mStatusMap = new Dictionary<string, FrameworkElement>();

        private String AniatiomImageUriPattern = "/image/animation/donghua{0}.png";
        private const int ANIMATION_INDEX_SHUIPING = 10001;
        private const int ANIMATION_INDEX_TISHENG1 = 20001;
        private const int ANIMATION_INDEX_CHUIZHI = 30001;
        private const int ANIMATION_INDEX_TISHENG0 = 40001;
        private const int ANIMATION_INDEX_XIAJIANG = 50001;
        private const int ANIMATION_INDEX_SLOPE = 60001;
        private const int ANIMATION_INDEX_BUCKET0 = 70001;
        private const int ANIMATION_INDEX_BUCKET1 = 80001;
        private const int ANIMATION_INDEX_XIAJIANG1 = 90001;

        private enum FlowDynamic { STILL = 0, FORWORD = 1, BACKWORD = 2 };      
        private const int INDEX_QUYANG = 0;
        private const int INDEX_TISHENG = 1;
        private const int INDEX_GAOJIA = 2;
        private const int INDEX_XIAJIANG = 3;
        private const int INDEX_SLOPE = 4;
        private const int INDEX_STORE = 5;
        private const int COUNT_INDEX = 6;

        private int[] mCommonIdx = new int[COUNT_INDEX];
        private FlowDynamic[] mDynamics = new FlowDynamic[COUNT_INDEX];

        private const int COMMON_START = 0;
        private const int COMMON_STOP = 5;

        private const int BUCKET_START = 0;
        private const int BUCKET_STOP = 56;
        private const int BUCKET_FULL = 100;
        private const int BUCKET_LEVEL = 20;
        private const int BUCKET_CAPCITY_PER_LEVEL = 500;
              
        private int[] mBucketPercents = new int[BUCKET_LEVEL];
        private List<Image> mBucketElements = new List<Image>(BUCKET_LEVEL);

        public UCWarningLights()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);
            mTooltipHelper = new ToolTipHelper(this);
            mBeckHoff = Utils.GetBeckHoffInstance();
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);

            mTooltipHelper.Loaded(sender, e);
            tm.Tick += new EventHandler(tm_Tick);
            tm.Interval = TimeSpan.FromSeconds(0.5);
            tm.Start();

            updateBucket();
            initStatusMap();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);

            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
            mTooltipHelper.Unloaded(sender, e);
            tm.Stop();    
        }

        private void initStatusMap()
        {            
            for(int i = 0; i < BUCKET_LEVEL; i++){
                Image iv = FindName(String.Format("iv_flow_bucket{0}",i)) as Image;                
                iv.Margin = new Thickness(0, 18 * i, 0, 0);
                mBucketElements.Add(iv);
            }
            mStatusMap.Add(".Store_percent",null);            
            mStatusMap.Add(".Sample_entrance_sensor", null);           
            mStatusMap.Add(".Corner_pid_sp", null);
            mStatusMap.Add(".Transfer_cig_exist", null);           
            mStatusMap.Add(".DownPort_hight", null);
          
            mStatusMap.Add(".Store_entrance_cig_exist", null);          
            mStatusMap.Add(".Corner_pid_pv", null);

            mStatusMap.Add(".Slope_cig_speed", this.tb_Slope_cig_speed);//斜向电机烟支速度（MW1886）Slope_cig_speed	
            mStatusMap.Add(".Store_cig_speed", this.tb_Store_cig_speed);//存储电机烟支速度（MW1884）Store_cig_speed	
            mStatusMap.Add(".Packer_cig_speed", this.tb_Packer_cig_speed);//包装机烟支速度（MW1310）Packer_cig_speed

            mStatusMap.Add(".Corner_cig_speed", this.tb_Corner_cig_speed);//弯道电机烟支速度（MW1304）Corner_cig_speed	
            mStatusMap.Add(".Life_cig_speed", this.tb_Life_cig_speed);//提升电机烟支速度（MW1400）Life_cig_speed	
            mStatusMap.Add(".Transfer_cig_speed", this.tb_Transfer_cig_speed);//传送电机烟支速度（MW1402）Transfer_cig_speed

            mStatusMap.Add(".Sample_cig_speed", this.tb_Sample_cig_speed);//取样电机烟支速度（MW1302）Sample_cig_speed		
            mStatusMap.Add(".Maker_cig_speed", this.tb_Maker_cig_speed);//卷烟机烟支速度（MW1300）Maker_cig_speed
            mStatusMap.Add(".MakerExport_cig_speed", this.tb_MakerExport_cig_speed);//烟机出口电机速度：MW1404 MakerExport_cig_speed

            foreach (String item in mStatusMap.Keys)
            {              
                updateStatus(item);
            }
        }

        private void updateBucket()
        {
            Object plcValue;
            mBeckHoff.plcVarUserdataMap.TryGetValue(".Store_percent", out plcValue);
            int capacity =(int) ((double)plcValue * 100);

          

            int n = capacity / BUCKET_CAPCITY_PER_LEVEL;
            float percent = (float)capacity % BUCKET_CAPCITY_PER_LEVEL / BUCKET_CAPCITY_PER_LEVEL;
            int level = (int)(BUCKET_STOP * percent);
            for (int i = 0; i < BUCKET_LEVEL; i++)
            {
                if (i < n)
                {
                    mBucketPercents[i] = BUCKET_FULL;
                }
                else if (i == n)
                {
                    mBucketPercents[i] = level;
                }
                else
                {
                    mBucketPercents[i] = BUCKET_START;
                }
            }
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
            
                if (mStatusMap.ContainsKey(plcVarName))
                {
                    updateStatus(plcVarName);
                }
            }
        }

        private void UpdateView(String viewTag, Object value)
        {
            FrameworkElement element;
            mStatusMap.TryGetValue(viewTag, out element);
            if(null == element){
                return;
            }
            Type t = element.GetType();
            if (t == typeof(Image))
            {
                Boolean status = (Boolean)value;
                Image iv = element as Image;
                if (status)
                {
                    iv.Source = MainWindow.sEnableLight;
                }
                else
                {
                    iv.Source = MainWindow.sDisableLight;
                }
            }
            else if (t == typeof(Button))
            {
                Button tv = element as Button;
                tv.Content = Utils.GetFormatedPlcValue(viewTag);
            }
        }

        private void updateStatus(String plcVarName)
        {
            Object plcValue;
            mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);
            UpdateView(plcVarName, plcValue);

            if (".Store_percent".Equals(plcVarName))
            {
                updateBucket();
            }      
    
            //提升机部分显示
            if (".Corner_pid_sp".Equals(plcVarName) )
            {
                Object Corner_pid_sp;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out Corner_pid_sp);                        
                
                Visibility vis = Visibility.Hidden;
                if (Convert.ToInt32(Corner_pid_sp) > 10 )
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_tisheng0.Visibility = vis;
                this.iv_flow_tisheng1.Visibility = vis;
                this.iv_flow_chuizhi.Visibility = vis;               
            }
            //提升 速度
            else if (".Corner_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                if (speed > 100)
                {
                    mDynamics[INDEX_TISHENG] = FlowDynamic.FORWORD;
                    mDynamics[INDEX_GAOJIA] = FlowDynamic.FORWORD;
                    mDynamics[INDEX_XIAJIANG] = FlowDynamic.FORWORD;
                }
                else
                {
                    mDynamics[INDEX_TISHENG] = FlowDynamic.STILL;
                    mDynamics[INDEX_GAOJIA] = FlowDynamic.STILL;
                    mDynamics[INDEX_XIAJIANG] = FlowDynamic.STILL;
                }
            }
            //取样部分显示
            else if (".Sample_entrance_sensor".Equals(plcVarName))
            {
                Boolean hasCigerate = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (hasCigerate)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_sample.Visibility = vis;               
            }
            //高架显示
            else if (".Transfer_cig_exist".Equals(plcVarName))
            {
                Boolean hasCigerate = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (hasCigerate)
                {
                    vis = Visibility.Visible;
                }              
                this.iv_flow_shuiping0.Visibility = vis;              
            }   
            //下降口部分显示
            else if (".DownPort_hight".Equals(plcVarName))
            {
                int height = Convert.ToInt32(plcValue);
                Visibility vis = Visibility.Hidden;
                if (height > 10)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_xiajiang0.Visibility = vis;              
            }
            else if (".Packer_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                Visibility vis = Visibility.Hidden;
                if (speed > 100)
                {
                    vis = Visibility.Visible;
                }               
                this.iv_flow_xiajiang1.Visibility = vis;              
            }
            //存储器入口部分（即斜向部分）显示
            else if (".Store_entrance_cig_exist".Equals(plcVarName))
            {
                Boolean exist = (Boolean)plcValue;
                Visibility vis = Visibility.Hidden;
                if (exist)
                {
                    vis = Visibility.Visible;
                }
                this.iv_flow_slope.Visibility = vis;
            }
           
             //取样速度
            else if (".Sample_cig_speed".Equals(plcVarName))
            {
                int speed = (int)plcValue;
                if (speed > 100)
                {
                    mDynamics[INDEX_QUYANG] = FlowDynamic.FORWORD;
                }
                else
                {
                    mDynamics[INDEX_QUYANG] = FlowDynamic.STILL;
                }
            }
           
            
           //斜向速度
            else if (".Slope_cig_speed".Equals(plcVarName))
            {
                int speed = Convert.ToInt32(plcValue.ToString());
                if (speed > 100)
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.FORWORD;
                }
                else if (speed < -100)
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.BACKWORD;
                }
                else
                {
                    mDynamics[INDEX_SLOPE] = FlowDynamic.STILL;
                }
            }
            //存储速度
            else if (".Store_cig_speed".Equals(plcVarName))
            {
                int speed =Convert.ToInt32(plcValue.ToString());
                if (speed > 100)
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.FORWORD;
                }
                else if (speed < -100)
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.BACKWORD;
                }
                else
                {
                    mDynamics[INDEX_STORE] = FlowDynamic.STILL;
                }
            }
          
        }
        #endregion

        private void tm_Tick(object sender, EventArgs e)
        {
            animateSample();
            animateShuiping();
            animateTisheng();
            animateXiajiang();
            animateSlope();
            animateBucket();

            for (int i = INDEX_QUYANG; i <= INDEX_STORE; i++)
            {
                if (mDynamics[i] == FlowDynamic.STILL)
                {
                    continue;
                }
                else if (mDynamics[i] == FlowDynamic.FORWORD)
                {
                    if (++mCommonIdx[i] > COMMON_STOP)
                    {
                        mCommonIdx[i] = COMMON_START;
                    }
                }
                else if (mDynamics[i] == FlowDynamic.BACKWORD)
                {
                    if (--mCommonIdx[i] < COMMON_START )
                    {
                        mCommonIdx[i] = COMMON_STOP;
                    }
                }
            }
        }

        private void animateSample()
        {
            String path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_SHUIPING + mCommonIdx[INDEX_QUYANG]); 
            this.iv_flow_sample.Source  = new BitmapImage(new Uri(path, UriKind.Relative));           
        }

        private void  animateBucket()
        {               
            for (int i =0; i < BUCKET_LEVEL; i++)
            {
                int percent = mBucketPercents[i];               
                Image image  = mBucketElements[i];             
                String path = null;

                if (percent == 0)
                {
                    image.Source = null;
                    continue;
                }
                else if (percent == BUCKET_FULL)
                {
                    //full
                    path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_BUCKET1 + mCommonIdx[INDEX_STORE]);                    
                }
                else
                {
                    path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_BUCKET0 + percent) ;    
                }
                BitmapImage bm = new BitmapImage(new Uri(path, UriKind.Relative));
                image.Source = bm;
            }
        }

        private void animateShuiping()
        {
            String path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_SHUIPING + mCommonIdx[INDEX_GAOJIA]);
            BitmapImage bm = new BitmapImage(new Uri(String.Format(path), UriKind.Relative));
            this.iv_flow_shuiping0.Source = bm;          
        }

        private void animateTisheng()
        {
            String path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_TISHENG0 + mCommonIdx[INDEX_TISHENG]);            
            this.iv_flow_tisheng0.Source = new BitmapImage(new Uri(path, UriKind.Relative));

            path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_TISHENG1 + mCommonIdx[INDEX_TISHENG]);   
            this.iv_flow_tisheng1.Source  = new BitmapImage(new Uri(path, UriKind.Relative));

            path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_CHUIZHI + mCommonIdx[INDEX_TISHENG]);
            this.iv_flow_chuizhi.Source = new BitmapImage(new Uri(path, UriKind.Relative));
       }

        private void animateXiajiang()
        {
            String path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_XIAJIANG + mCommonIdx[INDEX_XIAJIANG]);            
            this.iv_flow_xiajiang0.Source = new BitmapImage(new Uri(path, UriKind.Relative));

            path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_XIAJIANG1 + mCommonIdx[INDEX_XIAJIANG]);
            BitmapImage bm2 = new BitmapImage(new Uri(path, UriKind.Relative));
       
            this.iv_flow_xiajiang1.Source = bm2;         
        }

        private void animateSlope()
        {
            String path = String.Format(AniatiomImageUriPattern, ANIMATION_INDEX_SLOPE + mCommonIdx[INDEX_SLOPE]);
            this.iv_flow_slope.Source = new BitmapImage(new Uri(path, UriKind.Relative));          
        }
    }
}
