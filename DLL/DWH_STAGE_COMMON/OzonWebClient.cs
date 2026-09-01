using System;
using System.Globalization;
using System.Net;
using System.Threading;

namespace DWH_STAGE_COMMON
{
    public class OzonWebClient : MarketplaceWebClient
    {

        public OzonWebClient(int timeout, int maxRetries, int SleepMilliseconds) : base(timeout, maxRetries, SleepMilliseconds, "Global_DWH_STAGE_COMMON_Ozon_ad14b6cb-f77f-429f-8250-1300cadc66cc")
        {

        }

        public byte[] SendRequestWithRateLimit(string url, string ClientId, string ApiKey, byte[] PostData = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            int currentRetry = 0;

            while (true)
            {
                // Используем директиву using для автоматического закрытия ресурсов WebClient
                using (var client = new TimeoutWebClient(_Timeout))
                {
                    try
                    {
                        // Устанавливаем обязательный заголовок авторизации для API Wildberries
                        client.Headers.Add("Client-Id", string.Format("{0}", ClientId));
                        client.Headers.Add("Api-Key", ApiKey);

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
                        // В старых версиях .NET может не быть HttpStatusCode.TooManyRequests, тогда используем (HttpStatusCode)429
                        if (ex.Response is HttpWebResponse response)
                        {
                            if (
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
