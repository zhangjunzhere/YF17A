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
using System.Windows.Media.Animation;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageCleanScreen.xaml
    /// </summary>
    public partial class PageCleanScreen : Page,IObserverResult
    {
        public const String TAG = "PageCleanScreen.xaml";
        DispatcherTimer tm = new DispatcherTimer();
        Storyboard story = new Storyboard();

        private int tick = 1;
        private int span = 10;

        public PageCleanScreen()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);

            tm.Tick += new EventHandler(tm_Tick);
            tm.Interval = TimeSpan.FromSeconds(1);
            this.Visibility = Visibility.Hidden;
            animate();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }

        #region IResultObserver
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            //show menu from toolbar main action
            if (senderName.Equals(TAG))
            {
                this.Visibility = Visibility.Visible;
               // tm.Start();
                story.Begin(this);          
            }          
        }
        #endregion

        private void animate()
        {
            this.anim_cleanscreen.RenderTransform = new TranslateTransform();
                  
            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName(this.anim_cleanscreen.Name,  this.anim_cleanscreen);

            DoubleAnimation xAnimation = new DoubleAnimation();
            xAnimation.From = 0;
            xAnimation.To = 100;
            xAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            DependencyProperty[] propertyChain = new DependencyProperty[]
            {
                Image.RenderTransformProperty,
                TranslateTransform.XProperty
            };

           
            story.AutoReverse = true;
            story.RepeatBehavior = RepeatBehavior.Forever;
            story.Children.Add(xAnimation);

            Storyboard.SetTargetName(xAnimation, this.anim_cleanscreen.Name);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(0).(1)", propertyChain));

            //story.Begin(this);           
        }


        private void tm_Tick(object sender, EventArgs e)
        {
            if (++tick > span)
            {
                //tick = 0;
                //this.Visibility = Visibility.Hidden;
                //tm.Stop();
            }
            else
            {
                Point pt = this.anim_cleanscreen.RenderTransformOrigin;
                this.anim_cleanscreen.RenderTransform.Transform(new Point(pt.X + 100*tick, pt.Y + 100*tick));
               
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tick = 0;
            this.Visibility = Visibility.Hidden;
            tm.Stop();

            story.Stop();
        }


    }
}
