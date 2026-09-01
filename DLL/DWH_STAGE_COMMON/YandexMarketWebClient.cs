using System;
using System.Globalization;
using System.Net;
using System.Threading;

namespace DWH_STAGE_COMMON
{
    public class YandexMarketWebClient : MarketplaceWebClient
    {

        public YandexMarketWebClient(int timeout, int maxRetries, int SleepMilliseconds) : base(timeout, maxRetries, SleepMilliseconds, "Global_DWH_STAGE_COMMON_YandexMarket_16d1dbdd-bb51-4c5b-bd0c-fdfb96d9fa47")
        {

        }

        public byte[] SendRequestWithRateLimit(string url, string ApiKey, byte[] PostData = null)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            int currentRetry = 0;

            while (true)
            {
                // Используем директиву using для автоматического закрытия ресурсов WebClient
                using (var client = new TimeoutWebClient(_Timeout))
                {
                    try
                    {
                        // Устанавливаем обязательный заголовок авторизации для API YandexMarket
                        client.Headers.Add("Api-Key", string.Format("{0}", ApiKey));
                        client.Headers.Add("Content-Type", "application/json");

                        return DownloadData(client, url, PostData);
                    }
                    catch (WebException ex)
                    {
                        currentRetry++;

                        if (currentRetry > _maxRetries)
                        {
                            throw new WebException("Превышено максимальное количество попыток запроса", ex);
                        }

                        if ((currentRetry + 1) == _maxRetries)
                        {
                            //перед последней попыткой ждем 10 мин 
                            Thread.Sleep(600000);
                            continue; // Возвращаемся в начало цикла и повторяем запрос
                        }

                        // Проверяем, пришел ли ответ от сервера (а не ошибка сети/DNS)
                        if (ex.Response is HttpWebResponse response)
                        {
                            // Проверяем статус-код 420 (420 Enhance Your Calm)
                            // В старых версиях .NET может не быть HttpStatusCode.TooManyRequests, тогда используем (HttpStatusCode)429
                            if (response.StatusCode == (HttpStatusCode)420)
                            {
                                int delaySeconds = 1; // Дефолтное время ожидания                             

                                DateTime vDateXRateLimit = DateTime.ParseExact(response.Headers["X-RateLimit-Resource-Until"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                TimeSpan vDifference = vDateXRateLimit.ToLocalTime() - DateTime.Now;

                                int totalSeconds = Convert.ToInt32(vDifference.TotalSeconds);

                                // Блокируем поток на необходимое количество миллисекунд
                                if (totalSeconds > 0)
                                    Thread.Sleep(totalSeconds * 1000 + 2000);

                                continue; // Возвращаемся в начало цикла и повторяем запрос
                            }
                            else if (
                                response.StatusCode == (HttpStatusCode)429
                                || response.StatusCode == (HttpStatusCode)502
                                || response.StatusCode == (HttpStatusCode)503
                                || response.StatusCode == (HttpStatusCode)504
                                || response.StatusCode == (HttpStatusCode)408
                                || response.StatusCode == (HttpStatusCode)500                                
                                )
                            {
                                Thread.Sleep(_SleepMilliseconds * (currentRetry + 1));
                                continue; // Возвращаемся в начало цикла и повторяем запрос
                            }
                        }

                        if (
                            ex.Message.Contains("Время ожидания операции истекло")
                            || ex.Message.Contains("Query timeout expired")
                            )
                        {
                            Thread.Sleep(_SleepMilliseconds * (currentRetry + 1));
                            continue; // Возвращаемся в начало цикла и повторяем запрос
                        }

                        // Если это любая другая ошибка пробрасываем её дальше
                        throw;
                    }
                }
            }
        }
    }
}
