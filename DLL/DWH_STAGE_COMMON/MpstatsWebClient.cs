using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DWH_STAGE_COMMON
{
    public class MpstatsWebClient : MarketplaceWebClient
    {
        public MpstatsWebClient(int timeout, int maxRetries, int SleepMilliseconds) : base(timeout, maxRetries, SleepMilliseconds, "Global_DWH_STAGE_COMMON_Mpstats_d24c7d51-a5d3-41bf-89ae-576d2d55fa56")
        {

        }

        public byte[] SendRequestWithRateLimit(string url, string X_Mpstats_TOKEN, byte[] PostData = null)
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
                        client.Headers.Add("X-Mpstats-TOKEN", X_Mpstats_TOKEN);
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
                            if (
                                response.StatusCode == (HttpStatusCode)500
                                || response.StatusCode == (HttpStatusCode)502
                                || response.StatusCode == (HttpStatusCode)504
                                )
                            {
                                Thread.Sleep(_SleepMilliseconds * (currentRetry + 1));
                                continue; // Возвращаемся в начало цикла и повторяем запрос
                            }
                            else if (
                                ex.Message.Contains("Время ожидания операции истекло")
                                || ex.Message.Contains("Базовое соединение закрыто: Соединение было неожиданно закрыто")
                                || ex.Message.Contains("Невозможно соединиться с удаленным сервером")
                                || ex.Message.Contains("Исключение во время запроса WebClient")
                            )
                            {
                                Thread.Sleep(_SleepMilliseconds * (currentRetry + 1));
                                continue; // Возвращаемся в начало цикла и повторяем запрос
                            }

                        }

                        // Если это любая другая ошибка пробрасываем её дальше
                        throw;
                    }
                }
            }
        }
    }
}
