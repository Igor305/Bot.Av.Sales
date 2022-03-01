using DataAccessLayer;
using DataAccessLayer.AppContext;
using DataAccessLayer.Repositories.EFRepositories;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly static string idCreator = "309516361";
        private readonly static string connectionStringSQL03 = "Data Source=sql03;Initial Catalog=Avrora;Persist Security Info=True;User ID=j-PlanShops-Reader;Password=AE97rX3j5n";      

        private readonly static string pathLog = "LogAvSalesBot.txt";
        private static Timer aTimer;
        private static TelegramBotClient botClient;

        static async Task Main(string[] args)
        {
            Program program = new Program();
            await program.start();
        }
        
        public async Task start()
        {
            try
            {
                IServiceCollection serviceCollection = new ServiceCollection()

                    .AddLogging()
                    .AddDbContext<AvroraContext>(opts => opts.UseSqlServer(connectionStringSQL03))
                    .AddScoped<IItExecutionPlanShopRepository, ItExecutionPlanShopRepository>()
                    .AddScoped<IItGetCashByStockRepository, ItGetCashByStockRepository>()
                    .AddScoped<ISalesByCategoryManagerRepository, SalesByCategoryManagerRepository>();


                IServiceProvider services = serviceCollection.BuildServiceProvider();

                IItExecutionPlanShopRepository itExecutionPlanShopRepository = services.GetService<IItExecutionPlanShopRepository>();
                ISalesByCategoryManagerRepository salesByCategoryManagerRepository = services.GetService<ISalesByCategoryManagerRepository>();
                IItGetCashByStockRepository itGetCashByStockRepository = services.GetService<IItGetCashByStockRepository>();

                botClient = new TelegramBotClient(token);

                botClient.StartReceiving();

                await getSalesStatistic(itExecutionPlanShopRepository);
                //await getMessageSalesGroup(itExecutionPlanShopRepository);

                botClient.OnMessage += async (s, e) => await Bot_OnMessage(e, itExecutionPlanShopRepository, salesByCategoryManagerRepository, itGetCashByStockRepository);
            }

            catch (Exception ee)
            {
                await botClient.SendTextMessageAsync(
                        chatId: idCreator,
                        text: ee.Message
                        );
            }

            /* aTimer = new Timer(1800000);
             aTimer.Elapsed += async (s, e) => await getSalesStatistic(itExecutionPlanShopRepository);
             aTimer.AutoReset = true;
             aTimer.Start();*/

             //System.Threading.Thread.Sleep(-1);
        }

        private static async Task getSalesStatistic(IItExecutionPlanShopRepository itExecutionPlanShopRepository)
        {
            DateTime now = DateTime.Now;

            if (now.Minute == 0)
            {
                if (now.Hour < 7 || now.Hour > 22)
                {
                    if (File.Exists(pathLog))
                    {
                        File.Delete(pathLog);
                    }
                    return;
                }

                /* Check File
                if (File.Exists(pathLog))
                {
                    var text = File.ReadAllLines(pathLog, Encoding.UTF8);
                    foreach (string str in text)
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
                }*/

                string message = await getMessageSalesStatistic(itExecutionPlanShopRepository);

                string result = $"************************************************\nHour:{now.Hour}\n{message}\n";

                await File.AppendAllTextAsync(pathLog, result);
            }

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
                $"/summ - суммарные продажи по всей сети\n" +
                $"/kmX - где x - номер категорийного менеджера (признака 4);\n" +
                $"отправляет личное сообщение с данными продаж по категорийному менеджеру\n" +
                $"/kmall -  продажи по всем категорийным менеджерам\n" +
                $"/summcash - суммарная выдача налички из касса\n" +
                $"/allcash - выдача налички из кассы в разрезе магазинов";

            await botClient.SendTextMessageAsync(
                chatId: idSalesGroup,
                text: message
                );
        }

        private static async Task Bot_OnMessage(MessageEventArgs e, IItExecutionPlanShopRepository itExecutionPlanShopRepository, ISalesByCategoryManagerRepository salesByCategoryManagerRepository, IItGetCashByStockRepository itGetCashByStockRepository)
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
                                await deleteMessage(e);

                                List<string> messages = await getAllSales(itExecutionPlanShopRepository);

                                foreach (string message in messages)
                                {
                                    await botClient.SendTextMessageAsync(
                                      chatId: e.Message.From.Id,
                                      text: message
                                    );
                                }
                            }; break;

                        case "allcash@Av_Sales_Bot":
                        case "allcash":
                            {
                                await deleteMessage(e);

                                List<string> messages = await getAllCash(itGetCashByStockRepository);

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
                                await deleteMessage(e);

                                string message = "Вместо x, укажите номер магазина (к примеру /5)";

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );
                            }; break;

                        case "summ@Av_Sales_Bot":                  
                        case "summ":
                            {
                                await deleteMessage(e);

                                string message = await stringForMessageSales(itExecutionPlanShopRepository);

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );

                            }; break;

                        case "summcash@Av_Sales_Bot":
                        case "summcash":
                            {
                                await deleteMessage(e);

                                string message = await stringForMessagesCash(itGetCashByStockRepository);

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );

                            };break;

                        case "summChannel@Av_Sales_Bot":
                        case "summChannel":
                            {
                                await deleteMessage(e);

                                string message = await stringForMessageSales(itExecutionPlanShopRepository);

                                await botClient.SendTextMessageAsync(
                                  chatId: idSalesStatistic,
                                  text: message
                                );

                            }; break;

                        case "help@Av_Sales_Bot":
                        case "help":
                            {
                                await deleteMessage(e);

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
                                                $"/summ - суммарные продажи по всей сети\n" +
                                                $"/kmX - где x - номер категорийного менеджера (признака 4);\n" +
                                                $"отправляет личное сообщение с данными продаж по категорийному менеджеру\n" +
                                                $"/kmall -  продажи по всем категорийным менеджерам\n" +
                                                $"/summcash - суммарная выдача налички из касса\n" +
                                                $"/allcash - выдача налички из кассы в разрезе магазинов";

                                await botClient.SendTextMessageAsync(
                                    chatId: e.Message.From.Id,
                                    text: message
                                    );
                            }; break;

                        case "kmall@Av_Sales_Bot":
                        case "kmall":
                            {
                                await deleteMessage(e);

                                List<string> messages = await getAllCategoryManager(salesByCategoryManagerRepository);

                                foreach (string message in messages)
                                {
                                    await botClient.SendTextMessageAsync(
                                      chatId: e.Message.From.Id,
                                      text: message
                                    );
                                }

                            }; break;

                        case "kmx@Av_Sales_Bot":
                        case "kmx":
                        case "kmX@Av_Sales_Bot":
                        case "kmX":
                            {
                                await deleteMessage(e);

                                string message = "Вместо X, укажите номер категорийного менеджера (к примеру /km15)";

                                await botClient.SendTextMessageAsync(
                                  chatId: e.Message.From.Id,
                                  text: message
                                );
                            }; break;                 

                        default:
                            {
                                string temporarily = e.Message.Text.Trim('/');

                                await deleteMessage(e);

                                try
                                {
                                    string km = "";
                                    try
                                    {
                                        if (temporarily.Length > 2)
                                        {
                                            if (temporarily.Substring(0, 2) == "km")
                                            {                                              
                                                km = temporarily.Trim('k', 'm');

                                                SalesByCategoryManager salesByCategoryManager = await salesByCategoryManagerRepository.getCategoryManager(int.Parse(km));

                                                decimal salesByCM = Math.Round(salesByCategoryManager.SalesByCategoryManager1 ?? 0);

                                                string sale = formFact(salesByCM);

                                                string result = $"{sale} - {salesByCategoryManager.CategoryManagerName}";

                                                await botClient.SendTextMessageAsync(
                                                  chatId: e.Message.From.Id,
                                                  text: result
                                                  );
                                                return;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        string result = $"Отсутствуют данные по номеру категорийного менеджера - {km}";

                                        await botClient.SendTextMessageAsync(
                                              chatId: e.Message.From.Id,
                                              text: result
                                              );
                                        return;
                                    }

                                    int num = Int32.Parse(temporarily);

                                    string message = await getOneShopInfo(itExecutionPlanShopRepository, num);

                                    if (message == "")
                                    {
                                        message = $"Отсутствуют данные по номеру магазина - {temporarily}";
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
                            };  break;

                    }
                }
                else
                {
                    await deleteMessage(e);

                    string message = "Вы указали не правильную команду (/help)";

                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.From.Id,
                      text: message
                    );
                }
            }
            catch (Exception except)
            {
            }
        }

        private static async Task deleteMessage(MessageEventArgs e)
        {
            await botClient.DeleteMessageAsync(
                chatId: e.Message.Chat,
                messageId: e.Message.MessageId
             );
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

                                decimal percent = Math.Round(percentPlan ?? 0, 1);

                                decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                                string resultFact = formFact(fact);

                                message = $"{itExecutionPlanShop.StockId} - {resultFact} ({percent}%)\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
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

                            decimal percent = Math.Round(percentPlan ?? 0, 1);
                            decimal fact = Math.Round(itExecutionPlanShop.FactDay ?? 0);

                            string resultFact = formFact(fact);

                            if (sales.Length <= 4050)
                            {
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact} ({percent}%)\n";
                            }

                            if (sales.Length > 4050)
                            {
                                allSales.Add(sales);
                                sales = "";
                                sales += $"{itExecutionPlanShop.StockId} - {resultFact} ({percent}%)\n";
                            }
                        }
                    }
                }
                allSales.Add(sales);
            }
            catch (Exception ee)
            {
            }

            return allSales;
        }

        private static async Task<List<string>> getAllCash(IItGetCashByStockRepository itGetCashByStockRepository)
        {
            List<string> allCashs = new List<string>();

            try
            {
                List<ItGetCashByStock> itGetCashByStocks = await itGetCashByStockRepository.getAll();

                string cash = "";

                foreach (ItGetCashByStock itGetCashByStock in itGetCashByStocks)
                {
                    if (itGetCashByStock != null)
                    {
                        if (cash.Length <= 4050)
                        {
                            string sum = formFact(Math.Round(itGetCashByStock.SumGetCash ?? 0));
                            cash += $"{itGetCashByStock.StockId} - {sum}\n";
                        }
                        else
                        {
                            allCashs.Add(cash);
                            cash = "";
                            string sum = formFact(Math.Round(itGetCashByStock.SumGetCash ?? 0));
                            cash += $"{itGetCashByStock.StockId} - {Math.Round(itGetCashByStock.SumGetCash ?? 0)}\n";
                        }
                    }
                }
                allCashs.Add(cash);
            }
            catch (Exception ee)
            { 
            }

            return allCashs;
        }

        private static async Task<List<string>> getAllCategoryManager(ISalesByCategoryManagerRepository salesByCategoryManagerRepository)
        {
            List<string> allCategoryManager = new List<string>();

            try
            {
                List<SalesByCategoryManager> salesByCategoryManagers = await salesByCategoryManagerRepository.getCategoryManagers();

                string sales = "";

                foreach (SalesByCategoryManager salesByCategoryManager in salesByCategoryManagers)
                {

                    decimal sale = Math.Round(salesByCategoryManager.SalesByCategoryManager1 ?? 0);

                    string result = formFact(sale);

                    if (salesByCategoryManager.SalesByCategoryManager1 == 0)
                    {
                        result = salesByCategoryManager.SalesByCategoryManager1.ToString();
                    }

                    if (sales.Length <= 4050)
                    {
                        sales += $"{result} - {salesByCategoryManager.CategoryManagerName}\n";
                    }

                    if (sales.Length > 4050)
                    {
                        allCategoryManager.Add(sales);
                        sales = "";
                        sales += $"{result} - {salesByCategoryManager.CategoryManagerName}\n";
                    }                    
                }

                allCategoryManager.Add(sales);

            }
            catch (Exception ee)
            {
            }

            return allCategoryManager;
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

                if (maxPlanDay == 0)
                {                 

                    decimal maxFactD = maxFactDay ?? 0;

                    decimal maxFact = Math.Round(maxFactD);

                    string resultMaxFact = formFact(maxFact);

                    DateTime now = DateTime.Now;
                    string date = now.ToShortDateString();
                    string time = now.ToShortTimeString();

                    sales = $"{date} на {time} - {resultMaxFact}";
                }

                if (maxPlanDay != 0)
                {
                    decimal? maxFactDayByPlans = 0;

                    foreach (ItExecutionPlanShop itExecutionPlanShop in itExecutionPlanShops)
                    {
                        if (itExecutionPlanShop.PlanDay != null && itExecutionPlanShop.FactDay != null)
                        {
                            maxFactDayByPlans += itExecutionPlanShop.FactDay;
                        }
                    }

                    decimal? percentPlan = maxFactDayByPlans * 100 / maxPlanDay;

                    decimal maxFactD = maxFactDay ?? 0;
                    decimal percentP = percentPlan ?? 0;

                    decimal maxFact = Math.Round(maxFactD);
                    decimal percent = Math.Round(percentP, 1);

                    string resultMaxFact = formFact(maxFact);

                    DateTime now = DateTime.Now;
                    string date = now.ToShortDateString();
                    string time = now.ToShortTimeString();

                    sales = $"{date} на {time} - {resultMaxFact} ({percent}%)";
                }
            }

            catch (Exception ee)
            { 
            }

            return sales;
        }

        private static async Task<string> stringForMessagesCash(IItGetCashByStockRepository itGetCashByStockRepository)
        {
            string cash = "";
            try
            {
                decimal? summ = await itGetCashByStockRepository.getSumm();

                DateTime now = DateTime.Now;
                string date = now.ToShortDateString();
                string time = now.ToShortTimeString();
                
                string summCash = formFact(Math.Round(summ ?? 0));
              
                cash = $"{date} на {time} - {summCash} грн";
            }
            catch 
            {

            }

            return cash;
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

                    string TmaxFactResult = amountZero(TmaxFact);
                    string maxFactResult = amountZero(maxFact);

                    result = $"{MmaxFact} {TmaxFactResult} {maxFactResult}";
                }

                else
                {
                    string maxFactResult = amountZero(maxFact);
                    result = $"{TmaxFact} {maxFactResult}";                
                }
            }

            else
            {
                result = $"{maxFact}";
            }

            return result;
        }

        private static string amountZero(decimal maxFact)
        {
            string result = "";

            if (maxFact >= 100 )
            {
                result = $"{maxFact}";
            }

            if (maxFact < 100)
            {
                result = $"0{maxFact}";
            }

            if (maxFact < 10)
            {
                result = $"00{maxFact}";
            }

            return result;
        }
    }
}
