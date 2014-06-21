using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace YF17A
{
    public interface IProtocal : IObserverResult
    {
        //String FrameParameterName = "reportPanel"; 
        void navigateToPage(String panelName, String pageName);

        BeckHoff GetBeckHoff();
    }

    public interface IObserverResult
    {
        void onRecieveResult(Dictionary<String, Object> bundle);
    }

    public interface IActionBarObserver
    {
        void Back();
        void Home();
        void Help();
        void Option();

    }


    public interface IToolControlObserver
    {
        void RegisterObeserver(IObserverResult observer);
        void Update(Dictionary<String, Object> bundle);
        void NotifyObserver(Dictionary<String, Object> bundle);
    }

    public class ToolControlObserverImpl : IToolControlObserver
    {
        public const String KEY_MENU_INDEX = "MenuIndex";

        private IObserverResult mObserver;

        public void RegisterObeserver(IObserverResult observer)
        {
            mObserver = observer;
        }

        public void Update(Dictionary<String, Object> bundle) { }

        public void NotifyObserver(Dictionary<String, Object> bundle)
        {
            if (mObserver != null)
            {
                mObserver.onRecieveResult(bundle);
            }
        }
    }

    public class PageDataExchange
    {
        //public const string 
        public const String KEY_SENDER_NAME = "senderName";
        public const String KEY_SENDER_VALUE = "senderValue";
        public const String KEY_RESULT_VALUE = "resultValue";
        public const String KEY_THREAD_HOLD = "threadHold";

        private Dictionary<String, Dictionary<String, Object>> mPageDataCache = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<String, IObserverResult> mResultObserverCache = new Dictionary<String, IObserverResult>();

        private static PageDataExchange sPageDataExchange;
        private PageDataExchange()
        {
        }

        public static PageDataExchange getInstance()
        {
            if (sPageDataExchange == null)
            {
                sPageDataExchange = new PageDataExchange();
            }
            return sPageDataExchange;
        }

        public void putExtra(String pageTage, Dictionary<String, Object> pageData)
        {
            Dictionary<String, Object> dataCached;
            mPageDataCache.TryGetValue(pageTage, out dataCached);

            if (dataCached != null)
            {
                dataCached.Clear();
                mPageDataCache.Remove(pageTage);
            }

            mPageDataCache.Add(pageTage, pageData);
        }

        public Dictionary<String, Object> getExtra(String pageTage)
        {
            Dictionary<String, Object> dataCached;
            mPageDataCache.TryGetValue(pageTage, out dataCached);

            return dataCached;
        }

        public void addResultObserver(string pageTag, IObserverResult observer)
        {
            mResultObserverCache.Add(pageTag, observer);      
        }
        public void removeResultObserver(string pageTag)
        {
            mResultObserverCache.Remove(pageTag);
        }
        public IObserverResult getResultObserverByTag(string pageTag)
        {
            IObserverResult observer;
            mResultObserverCache.TryGetValue(pageTag, out observer);
            return observer;
        }

      
        public void CommandToolbarParamter(String senderTag, String command)
        {
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add( PageDataExchange.KEY_SENDER_NAME, senderTag);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, command);

            IObserverResult observer = this.getResultObserverByTag(ToolbarParameter.TAG);
            if (observer != null)
            {
                observer.onRecieveResult(bundle);
            }
        }

        public void CommandObserver(String ObserverTag, String senderKey, Object command)
        {
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add( PageDataExchange.KEY_SENDER_NAME, senderKey);
            bundle.Add( PageDataExchange.KEY_SENDER_VALUE, command);

            IObserverResult observer = this.getResultObserverByTag(ObserverTag);
            if (observer != null)
            {
                observer.onRecieveResult(bundle);
            }
        }

        public void NotifyObserverChanged(String ObserverName, String senderName, Object value)
        {
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, senderName);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, value);

            IObserverResult observer = this.getResultObserverByTag(ObserverName);
            if (observer != null)
            {
                observer.onRecieveResult(bundle);
            }
        }

        public Boolean IsPageLoaded(String pageTag)
        {
            return null != getResultObserverByTag(pageTag);
        }

        public void NotifyAllObeservers( String senderKey, Object command)
        {
            Dictionary<String, Object> bundle = new Dictionary<string, object>();
            bundle.Add(PageDataExchange.KEY_SENDER_NAME, senderKey);
            bundle.Add(PageDataExchange.KEY_SENDER_VALUE, command);

            foreach (KeyValuePair<String, IObserverResult> entry in mResultObserverCache)
            {
                IObserverResult observer = entry.Value;
                if (observer != null)
                {
                    observer.onRecieveResult(bundle);
                }
            }
        }
    }
}
