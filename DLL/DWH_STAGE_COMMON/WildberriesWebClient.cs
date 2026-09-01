using System;
using System.Net;
using System.Threading;

namespace DWH_STAGE_COMMON
{
    public class WildberriesWebClient : MarketplaceWebClient
    {

        public WildberriesWebClient(int timeout, int maxRetries, int SleepMilliseconds) : base(timeout, maxRetries, SleepMilliseconds, "Global_DWH_STAGE_COMMON_WB_318c5f4c-2f5f-49bd-afad-625b0bd54c03")
        {

        }

        public byte[] SendRequestWithRateLimit(string url, string token, byte[] PostData = null)
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
                        // Устанавливаем обязательный заголовок авторизации для API Wildberries
                        client.Headers.Add(HttpRequestHeader.Authorization, token);
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

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
                            // Проверяем статус-код 429 (Too Many Requests)
                            // В старых версиях .NET может не быть HttpStatusCode.TooManyRequests, тогда используем (HttpStatusCode)429
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                int delaySeconds = 1; // Дефолтное время ожидания

                                // 1. Извлекаем X-Ratelimit-Retry
                                string retryHeader = response.Headers["X-Ratelimit-Retry"];
                                // 2. Если его нет, пробуем извлечь X-Ratelimit-Reset
                                string resetHeader = response.Headers["X-Ratelimit-Reset"];

                                if (!string.IsNullOrEmpty(retryHeader) && int.TryParse(retryHeader, out int retryValue))
                                {
                                    delaySeconds = retryValue;
                                }
                                else if (!string.IsNullOrEmpty(resetHeader) && int.TryParse(resetHeader, out int resetValue))
                                {
                                    delaySeconds = resetValue;
                                }
                                else
                                {
                                    // Запасной вариант: экспоненциальное ожидание
                                    delaySeconds = (int)Math.Pow(2, currentRetry);
                                }

                                // Блокируем поток на необходимое количество миллисекунд
                                Thread.Sleep((delaySeconds + 1) * 1000);
                                continue; // Возвращаемся в начало цикла и повторяем запрос
                            }
                            else if (
                                response.StatusCode == (HttpStatusCode)502
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
