using System;
using System.Net;
using System.Security.Policy;
using System.Threading;

namespace DWH_STAGE_COMMON
{
    public class MarketplaceWebClient
    {
        protected int _Timeout { get; set; }

        protected Mutex _mutex { get; set; }

        protected int _maxRetries { get; set; }

        protected int _SleepMilliseconds { get; set; }

        public MarketplaceWebClient()
        {
            _Timeout = 30000;
            _maxRetries = 6;
            _SleepMilliseconds = 26000;
        }

        public MarketplaceWebClient(int timeout, int maxRetries, int SleepMilliseconds, string MutexName)
        {
            _Timeout = timeout;
            _maxRetries = maxRetries;
            _SleepMilliseconds = SleepMilliseconds;
            _mutex = new Mutex(false, MutexName);
        }

        protected byte[] DownloadData(TimeoutWebClient TWC, string URL, byte[] PostData = null)
        {
            byte[] vBuffer = null;
            // Захватываем мьютекс (аналог WaitOne)
            _mutex.WaitOne();

            try
            {
                if (PostData == null)
                {
                    vBuffer = TWC.DownloadData(URL);
                }
                else
                {
                    vBuffer = TWC.UploadData(URL, "post", PostData);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Обязательно освобождаем мьютекс в блоке finally
                _mutex.ReleaseMutex();
            }

            return vBuffer;
        }

        #region TimeoutWebClient
        public class TimeoutWebClient : WebClient
        {
            /// <summary>
            /// Default constructor (30000 ms timeout)
            /// NOTE: timeout can be changed later on using the [Timeout] property.
            /// </summary>
            public TimeoutWebClient() : this(30000) { }

            /// <summary>
            /// Constructor with customizable timeout
            /// </summary>
            /// <param name="timeout">
            /// Web request timeout (in milliseconds)
            /// </param>
            public TimeoutWebClient(int timeout)
            {
                Timeout = timeout;
            }

            #region Methods
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = Timeout;
                ((HttpWebRequest)w).ReadWriteTimeout = Timeout;
                return w;
            }
            #endregion

            /// <summary>
            /// Web request timeout (in milliseconds)
            /// </summary>
            public int Timeout { get; set; }
        }
        #endregion

    }
}
