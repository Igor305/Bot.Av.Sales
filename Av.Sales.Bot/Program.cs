using DataAccessLayer;
using DataAccessLayer.AppContext;
using DataAccessLayer.Repositories.EFRepositories;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace AvSalesBot
{
    public class Program
    {
        private readonly static string token = "1751551484:AAG4EElnp4JasnVeyu8RMJ4_Ckq0GUJ1XTI";
        private readonly static string idSalesStatistic = "-1001248328691";
        private readonly static string idSalesGroup = "-1001373411474";
        private readonly static string connectionStringSQL03 = "Data Source=sql03;Initial Catalog=Avrora;Persist Security Info=True;User ID=j-PlanShops-Reader;Password=AE97rX3j5n";

        private readonly static string pathLog = "Log.txt";
        private static Timer aTimer;
        private static TelegramBotClient botClient;

        static async Task Main(string[] args)
        {
            Program program = new Program();
            await program.start();
        }

        public async Task start()
        {
            IServiceCollection serviceCollection = new ServiceCollection()

                .AddLogging()
                .AddDbContext<AvroraContext>(opts => opts.UseSqlServer(connectionStringSQL03))
                .AddScoped<IItExecutionPlanShopRepository, ItExecutionPlanShopRepository>();

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            IItExecutionPlanShopRepository itExecutionPlanShopRepository = services.GetService<IItExecutionPlanShopRepository>();

            botClient = new TelegramBotClient(token);

            botClient.StartReceiving();

            await getSalesStatistic(itExecutionPlanShopRepository);
            //await getMessageSalesGroup(itExecutionPlanShopRepository);

            botClient.OnMessage += async (s, e) => await Bot_OnMessage(e, itExecutionPlanShopRepository);

            /* aTimer = new Timer(1800000);
             aTimer.Elapsed += async (s, e) => await getSalesStatistic(itExecutionPlanShopRepository);
             aTimer.AutoReset = true;
             aTimer.Start();

             System.Threading.Thread.Sleep(-1);*/
        }

        private static async Task getSalesStatistic(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            DateTime now = DateTime.Now;

            if (now.Hour < 7 || now.Hour > 22)
            {
                return;
            }

            if (now.Hour == 7 && now.Minute <= 30)
            {
                if (File.Exists(pathLog))
                {
                    File.Delete(pathLog);
                }
            }

            if (File.Exists(pathLog))
            {
                var text = File.ReadAllLines(pathLog, Encoding.UTF8);
                foreach(string str in text)
                {
                    if (str.Contains("Hour:"))
                    {
                        string hour = str.Substring(5);
                        if (hour == now.Hour.ToString())
                        {
                            return;
                        }
                    }
                }
            }

            string message = await getMessageSalesStatistic(itExecutionPlanShopRepository);

            string result = $"************************************************\nHour:{now.Hour}\n{message}\n";

            await File.AppendAllTextAsync(pathLog, result);

        }

        private static async Task getMessageSalesGroup(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            string message = $"Для начала работы с ботом необходимо:\n" +
                $"1.Нажать на бота @Av_Sales_Bot\n" +
                $"2.Внизу чата нажать 'Начать'\n" +
                $"3.Теперь бот сможет отправлять Вам личные сообщения.\n" +
                $"Список доступных команд:\n" +
                $"/all - отправляет личное сообщение с данными продаж по всем " +
                $"магазинам(кроме, где продаж за текущий день небыло)\n" +
                $"/x - где x - числовой номер магазина;отправляет личное " +
                $"сообщение с данными продаж по указаному номеру магазина.\n" +
                $"/help - список доступных команд\n" +
                $"/summ - суммарные продажи по всей сети\n";

            await botClient.SendTextMessageAsync(
                chatId: idSalesGroup,
                text: message
                );
        }

        private static async Task Bot_OnMessage(MessageEventArgs e, IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            try
            {
                var members = await botClient.GetChatMemberAsync(idSalesGroup, e.Message.From.Id);
                
                if (members.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Left)
                {
                    return;
                }

                if (e.Message.Text[0] == '/')
                {
                    switch (e.Message.Text.Trim('/'))
                    {
                        case "all@Av_Sales_Bot":
                        case "all":
                            {
                                await botClient.DeleteMessageAsync(
                                     chatId: e.Message.Chat,
                                     messageId: e.Message.MessageId
                                );

                                List<string> messages = await getAllSales(itExecutionPlanShopRepository);

                                foreach (string message in messages)
                                {
                                    await botClient.SendTextMessageAsync(
                                      chatId: e.Message.From.Id,
                                      text: message
                                    );
                                }
                            }; break;

                        case "x@Av_Sales_Bot":
                        case "x":
                            {
                                await botClient.DeleteMessageAsync(
                                        chatId: e.Message.Chat,
                                        messageId: e.Message.MessageId
                                );
                                string message = "Вместо x, укажите номер магазина (к примеру /5)";

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );
                            }; break;

                        case "summ@Av_Sales_Bot":                  
                        case "summ":
                            {
                                await botClient.DeleteMessageAsync(
                                     chatId: e.Message.Chat,
                                     messageId: e.Message.MessageId
                                );
                                string message = await stringForMessageSales(itExecutionPlanShopRepository);

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );

                            }; break;

                        case "help@Av_Sales_Bot":
                        case "help":
                            {
                                await botClient.DeleteMessageAsync(
                                    chatId: e.Message.Chat,
                                    messageId: e.Message.MessageId
                                );
                                string message = $"Список доступных команд:\n" +
                                    $"/all - отправляет личное сообщение с данными продаж по всем " +
                                    $"магазинам(кроме, где продаж за текущий день небыло)\n" +
                                    $"/x - где x - числовой номер магазина;отправляет личное " +
                                    $"сообщение с данными продаж по указаному номеру магазина.\n" +
                                    $"/help - список доступных команд\n" +
                                    $"/summ - суммарные продажи по всей сети\n";

                                await botClient.SendTextMessageAsync(
                                    chatId: e.Message.From.Id,
                                    text: message
                                    );
                            }; break;

                        default:
                            {
                                await botClient.DeleteMessageAsync(
                                   chatId: e.Message.Chat,
                                   messageId: e.Message.MessageId
                                );

                                string number = e.Message.Text.Trim('/');

                                try
                                {
                                    int num = Int32.Parse(number);

                                    string message = await getOneShopInfo(itExecutionPlanShopRepository, num);

                                    if (message == "")
                                    {
                                        message = $"Отсутствуют данные по номеру магазина - {number}";
                                    }

                                    await botClient.SendTextMessageAsync(
                                      chatId: e.Message.From.Id,
                                      text: message
                                    );
                                }

                                catch
                                {
                                    string message = "Вы указали не правильную команду (/help)";

                                    await botClient.SendTextMessageAsync(
                                      chatId: e.Message.From.Id,
                                      text: message
                                    );

                                }
                            }
                            break;

                    }
                }
                else
                {
                    await botClient.DeleteMessageAsync(
                        chatId: e.Message.Chat,
                        messageId: e.Message.MessageId
                    );

                    string message = "Вы указали не правильную команду (/help)";

                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.From.Id,
                      text: message
                    );
                }
            }
            catch
            {

            }
        }

        private static async Task<string> getOneShopInfo(IItExecutionPlanShopRepository itExecutionPlanShopRepository, int number)
        {
            List<ItExecutionPlanShop> itExecutionPlanShops = await itExecutionPlanShopRepository.getSales();

            string message = "";

            try
            { 
                foreach (ItExecutionPlanShop itExecutionPlanShop in itExecutionPlanShops)
                {
                    if (itExecutionPlanShop.StockId == number)
                    {
                        if (itExecutionPlanShop.FactDay != null)
                        {
                            if (itExecutionPlanShop.PlanDay == null)
                            {
                                decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                                string resultFact = formFact(fact);

                                message = $"{itExecutionPlanShop.StockId} - {resultFact}\n";
                            }

                            if (itExecutionPlanShop.PlanDay != null)
                            {
                                decimal? percentPlan = itExecutionPlanShop.FactDay * 100 / itExecutionPlanShop.PlanDay;

                                decimal percent = Math.Round(percentPlan ?? 0, 2);

                                decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                                string resultFact = formFact(fact);

                                message = $"{itExecutionPlanShop.StockId} - {resultFact}({percent}%)\n";
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return message;
        }

        private static async Task<List<string>> getAllSales(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            List<string> allSales = new List<string>();

            try
            {
                List<ItExecutionPlanShop> itExecutionPlanShops = await itExecutionPlanShopRepository.getSales();

                string sales = "";

                foreach (ItExecutionPlanShop itExecutionPlanShop in itExecutionPlanShops)
                {
                    if ( itExecutionPlanShop.FactDay != null)
                    {
                        if (itExecutionPlanShop.PlanDay == null)
                        {
                            decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                            string resultFact = formFact(fact);

                            if (sales.Length <= 4050)
                            {
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact}\n";
                            }

                            if (sales.Length > 4050)
                            {
                                allSales.Add(sales);
                                sales = "";
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact}\n";
                            }
                        }

                        if (itExecutionPlanShop.PlanDay != null)
                        {
                            decimal? percentPlan = itExecutionPlanShop.FactDay * 100 / itExecutionPlanShop.PlanDay;

                            decimal percent = Math.Round(percentPlan ?? 0, 2);
                            decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                            string resultFact = formFact(fact);

                            if (sales.Length <= 4050)
                            {
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact}({percent}%)\n";
                            }

                            if (sales.Length > 4050)
                            {
                                allSales.Add(sales);
                                sales = "";
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact}({percent}%)\n";
                            }
                        }
                    }
                }
                allSales.Add(sales);

            }
            catch
            {

            }

            return allSales;
        }

        private static async Task<string> getMessageSalesStatistic(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            string sales = await stringForMessageSales(itExecutionPlanShopRepository);

            if (sales != "")
            {

                await botClient.SendTextMessageAsync(
                    chatId: idSalesStatistic,
                    text: sales
                    );
            }

            return sales;
        }

        private static async Task<string> stringForMessageSales(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            string sales = "";

            try
            {
                List<ItExecutionPlanShop> itExecutionPlanShops = await itExecutionPlanShopRepository.getSales();

                decimal? maxPlanDay = 0;
                decimal? maxFactDay = 0;
                foreach (ItExecutionPlanShop itExecutionPlanShop in itExecutionPlanShops)
                {
                    if (itExecutionPlanShop.PlanDay != null)
                    {
                        maxPlanDay += itExecutionPlanShop.PlanDay;
                    }

                    if (itExecutionPlanShop.FactDay != null)
                    {
                        maxFactDay += itExecutionPlanShop.FactDay;
                    }
                }
                decimal? percentPlan = maxFactDay * 100 / maxPlanDay;

                decimal maxFactD = maxFactDay ?? 0;
                decimal percentP = percentPlan ?? 0;

                decimal maxFact = Math.Round(maxFactD);
                decimal percent = Math.Round(percentP, 2);

                string resultMaxFact = formFact(maxFact);

                DateTime now = DateTime.Now;
                string date = now.ToShortDateString();
                string time = now.ToShortTimeString();

                sales = $"{date} на {time} - {resultMaxFact}({percent}%)";
            }
            catch
            {

            }

            return sales;
        }

        private static string formFact(decimal maxFact)
        {
            string result = "";

            if (Math.Truncate(maxFact / 1000) > 0)
            {
                decimal TmaxFact = Math.Truncate(maxFact / 1000);

                maxFact = maxFact % 1000;

                if (Math.Truncate(TmaxFact / 1000) > 0)
                {
                    decimal MmaxFact = Math.Truncate(TmaxFact / 1000);
                    TmaxFact = TmaxFact % 1000;

                    if (TmaxFact >= 100)
                    {
                        result = $"{MmaxFact} {TmaxFact} {maxFact}";
                    }

                    if (TmaxFact < 100)
                    {
                        result = $"{MmaxFact} 0{TmaxFact} {maxFact}";
                    }

                    if (TmaxFact < 10)
                    {
                        result = $"{MmaxFact} 00{TmaxFact} {maxFact}";
                    }                 
                }

                else
                {
                    if (maxFact >= 100)
                    {
                        result = $"{TmaxFact} {maxFact}";
                    }

                    if (maxFact < 100)
                    {
                        result = $"{TmaxFact} 0{maxFact}";
                    }

                    if (maxFact < 10)
                    {
                        result = $"{TmaxFact} 00{maxFact}";
                    }
                }
            }

            else
            {
                result = $"{maxFact}";
            }

            return result;
        }
    }
}
