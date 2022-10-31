using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Formats.Asn1.AsnWriter;

namespace sample
{
    internal class Bot
    {

        private static string token = Strings.Tokens.BotToken;

        private static TelegramBotClient bot = new TelegramBotClient(token);
        private static GoogleHelper googleHelper = new GoogleHelper();

        private static bool isFioValid(string Fio)
        {
            if (Fio.Contains("'\'") || Fio.Contains("/") || Fio.Contains("'"))
                return false;
            if (Fio.Split(' ').Length == 2 || Fio.Split(' ').Length == 3)
            {
                foreach (char el in Fio)
                {
                    if (el != ' ')
                    {
                        if (((int)el >= 97 && (int)el <= 122))
                        {
                            return false;
                        }
                    }

                }
            }
            else
                return false;

            return true;
        }

        private static bool isGroupValid(string group)
        {
            if (group.Contains("'\'") || group.Contains("/") || group.Contains("'"))
                return false;
            try
            {
                string[] parts = group.Split('-');
                if (parts.Length == 2)
                {
                    if (parts[0].Length == 0 && parts[0].Length > 3)
                    {
                        return false;
                    }
                    if (parts[1].Length >= 2)
                    {
                        if (!char.IsDigit(parts[1][0]) && !char.IsDigit(parts[1][1]))
                            return false;
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static bool isContactValid(string contact)
        {
            if (contact.Contains("'\'") || contact.Contains("/") || contact.Contains("'"))
                return false;
            if (contact.Length == 11)
            {
                if (contact.StartsWith("8"))
                {
                    for (int i = 1; i < contact.Length; i++)
                    {
                        if (!char.IsDigit(contact[i]))
                            return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
                return false;

            return true;
        }

        private static bool IsDateValid(string date)
        {
            try
            {
                DateTime output = Convert.ToDateTime(date);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private static bool IsAdult(DateTime birht)
        {
            return ((DateTime.Now - birht).TotalDays / 365) >= 18;
        }

        public static int ConvertCodeToInt(string code)
        {

            string code_no_litres = null;
            for (int i = 0; i < code.Length; i++)
            {
                if (char.IsDigit(code[i]))
                    code_no_litres += code[i];
            }

            return Convert.ToInt32(code_no_litres);
        }

        private static void Logger(Update update)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString());
            Console.WriteLine($"{update.Type}\n");
            if (update.Type == UpdateType.Message)
            {
                if (string.IsNullOrEmpty(update.Message.Text) == false)
                    Console.WriteLine($"Message: {update.Message.Text}\nFrom: {update.Message.From.Id}");

            }
            else
            {
                Console.WriteLine($"Querry: {update.CallbackQuery.Data}\nText: {update.CallbackQuery.Message.Text}\nFrom: {update.CallbackQuery.From.Id}");
            }
            Console.WriteLine();
        }

        private static async Task EndReg(long chatId)
        {

            User activeUser = await User.GetUserByChatId(chatId);
            InlineKeyboardMarkup keyboard;
            string data;



            if (activeUser.University == "BMSTU")
            {
                keyboard = Buttons.RegEndBmstu;
                string scores = activeUser.NeedScores? "Нужны": "Не нужны";

                string university = "МГТУ им. Н.Э. Баумана";

                data = $"🔹 ФИО: {activeUser.Fio}\n🔹 Моб.тел: {activeUser.Contact}\n🔹 Университет: {university}\n🔹 Учебная группа: {activeUser.Group}" +
                    $"\n🔹 Дата Рождения: {activeUser.Birth}\n🔹 Баллы на физкультуру: {scores}";
            }
            else
            {
                keyboard = Buttons.RegEndNotBmstu;
                string university = activeUser.University;

                if (activeUser.University == "Not a student")
                {
                    university = "Не студент";
                }

                data = $"🔹 ФИО: {activeUser.Fio}\n🔹 Моб.тел: {activeUser.Contact}\n🔹 Университет: {university}" +
                    $"\n🔹 Дата Рождения: {activeUser.Birth}";
            }


            


            await bot.SendTextMessageAsync(chatId, $"Анкета подошла к концу. Проверка персональных данных.\n\n{data}\n\nВсё правильно? Нажимай на зелёную кнопку и приходи на батутную ночь. " +
                                                    "Ждём тебя и твоих друзей!", ParseMode.Markdown, replyMarkup: keyboard);

            await activeUser.Update("CommandLine", "", "chatId", chatId);

        }

        private static async Task UserCommandLineHandler(User activeUser, string message_text)
        {
            string commandLine = activeUser.CommandLine;


            if (commandLine.Contains("Input"))
            {
                if (commandLine == "Input_Fio")
                {
                    if (isFioValid(message_text))
                    {
                        await activeUser.Update("Fio", message_text, "chatId", activeUser.ChatId);
                        await activeUser.Update("CommandLine", "_", "chatId", activeUser.ChatId);


                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.IsBmst, replyMarkup: Buttons.IsBmstuKeyBoard);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.FioError);
                    }
                }
                else if (commandLine == "Input_Group")
                {
                    if (isGroupValid(message_text))
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.AskBirth, replyMarkup: Buttons.BackToInputGroup);

                        await activeUser.Update(param: "univer_group", message_text, "chatId", activeUser.ChatId);
                        await activeUser.Update("CommandLine", "Input_Birth", "chatId", activeUser.ChatId);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.GroupError);

                    }
                }
                else if (commandLine == "Input_University")
                {
                    await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.AskBirth, replyMarkup: Buttons.BackToInputUniversity);

                    await activeUser.Update(param: "University", message_text, "chatId", activeUser.ChatId);
                    await activeUser.Update(param: "univer_group", "", "chatId", activeUser.ChatId);


                    await activeUser.Update("CommandLine", "Input_Birth", "chatId", activeUser.ChatId);
                }
                else if (commandLine == "Input_Birth")
                {
                    if (IsDateValid(message_text))
                    {
                        var date = DateTime.Parse(message_text);

                        if (IsAdult(date))
                        {
                            await activeUser.Update("Birth", message_text, "chatId", activeUser.ChatId);

                            await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.AskContact, replyMarkup: Buttons.BackToInputBirth);
                            await activeUser.Update("CommandLine", "Input_Contact", "chatId", activeUser.ChatId);

                        }
                        else
                        {
                            await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.NotAdultErorr);

                            await activeUser.Update("CommandLine", "NOT ADULT", "chatId", activeUser.ChatId);
                            await activeUser.Update("isRegEnd", 1, "chatId", activeUser.ChatId);
                        }
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.DateErorr);
                    }
                }
                else if (commandLine == "Input_Contact")
                {
                    if (isContactValid(message_text))
                    {
                        await activeUser.Update("Contact", message_text, "chatId", activeUser.ChatId);

                        if (activeUser.University == "BMSTU")
                        {
                            await bot.SendTextMessageAsync(activeUser.ChatId, "Хочешь получить доп баллы на физкультуру?", replyMarkup: Buttons.NeedScoresKeyBoard);

                        }
                        else
                        {
                            await EndReg(activeUser.ChatId);
                        }
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.ContactError);
                    }
                }
            }
            else if (commandLine.Contains("Change"))
            {
                if (commandLine == "Change_Fio")
                {
                    if (isFioValid(message_text))
                    {
                        await activeUser.Update("Fio", message_text, "chatId", activeUser.ChatId);
                        await activeUser.Update("CommandLine", "_", "chatId", activeUser.ChatId);
                        await EndReg(activeUser.ChatId);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.FioError);


                    }
                }
                else if (commandLine == "Change_Group")
                {
                    if (isGroupValid(message_text))
                    {
                        await activeUser.Update("univer_group", message_text, "chatId", activeUser.ChatId);
                        await activeUser.Update("CommandLine", "_", "chatId", activeUser.ChatId);
                        await EndReg(activeUser.ChatId);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.GroupError);

                    }
                }
                else if (commandLine == "Change_Contact")
                {
                    if (isContactValid(message_text))
                    {
                        await activeUser.Update("Contact", message_text, "chatId", activeUser.ChatId);
                        await activeUser.Update("CommandLine", "_", "chatId", activeUser.ChatId);
                        await EndReg(activeUser.ChatId);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.ContactError);

                    }
                }
            }
        }
        private static async Task AdminCommandLineHandler(Admin activeAdmin,string message_text)
        {
            string commandLine = activeAdmin.commandLine;

            if(commandLine == "Input_Code")
            {
                int code = ConvertCodeToInt(message_text);

                User get_user = await User.GetUserByCode(code);
                if(get_user != null)
                {
                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Да",$"SendUserToXls {get_user.ExelId} {get_user.NeedScores} {get_user.Code}")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Нет","repete_input_code")

                        }
                    });
                    await bot.SendTextMessageAsync(activeAdmin.chatId, $"Убедись пожалуйста, что это он {get_user.Fio}",replyMarkup: keyboard);
                }
                else
                {
                    await bot.SendTextMessageAsync(activeAdmin.chatId, "Ой, кажется такого пользователя не существует, попробуй ещё раз");
                }
            }
            else if (commandLine.Contains("SendMessage"))
            {
                int param = Convert.ToInt32(commandLine.Split(' ')[1]);

                await bot.SendTextMessageAsync(activeAdmin.chatId, "Рассылка начата");
                await bot.SendTextMessageAsync(activeAdmin.chatId, "Админ-панель", replyMarkup: Buttons.MainAdminKeyBoard);


                List<User> users = await User.GetUsers(param);

               

                foreach(var user in users)
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            await bot.SendTextMessageAsync(user.ChatId, message_text);
                            await Task.Delay(50);
                        });
                      
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                
                }

                await bot.SendTextMessageAsync(activeAdmin.chatId, "Рассылка Закончена");
            }
        }
        public static async Task Start()
        {




            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

      
                try
                {
                    var cancellationToken = CancellationToken.None;
                    var receiverOptions = new ReceiverOptions
                    {
                        AllowedUpdates = { }, // receive all update types
                    };

                    var updateReceiver = new QueuedUpdateReceiver(bot, receiverOptions);

                    

                    try
                    {
                        await foreach (Update update in updateReceiver.WithCancellation(cancellationToken))
                        {

                        _ = Task.Run(() =>
                        {
                            try
                            {
                                _ = HandleUpdateAsync(update);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

                            return Task.CompletedTask;
                        });

                    }
                    }
                    catch (OperationCanceledException exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await Start();
                }

            

        }

        private static async Task HandleUpdateAsync( Update update)
        {
            if (update != null)
            {
                Logger(update);


                if (update.Type == UpdateType.Message)
                {

                    Message message = update.Message;

                    if (message.Type == MessageType.Text)
                    {
                        await TextHandler(message);
                    }

                }
                else if (update.Type == UpdateType.CallbackQuery)
                {

                    await CallBackHandler(update);

                }
            }
        }
        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task TextHandler(Message message)
        {

            User activeUser = null;
            Admin activeAdmin = null;

            long chatId = message.From.Id;
            string message_text = message.Text;

            await bot.SendChatActionAsync(chatId, ChatAction.Typing);


            if (activeUser == null)
            {
                try
                {
                    activeUser = await User.GetUserByChatId(chatId);
                }
                catch (Exception)
                {

                }
               
            }
            if (activeAdmin == null)
            {
                try
                {
                    activeAdmin = await Admin.GetAdminByChatId(chatId);

                }
                catch (Exception)
                {

                   
                }
            }

           

            if (message_text.Contains("start"))
            {
                
                if (message_text == "/start")
                {
                    await bot.SendTextMessageAsync(chatId, Strings.Messages.StartMessage, replyMarkup: Buttons.StartKeyBoard);

                }

                string param = message_text.Split(' ')[1];
                if (param == "Main")
                {
                    await Admin.Registation(chatId, true);
                    await bot.SendTextMessageAsync(chatId, "Админ-панель",replyMarkup:Buttons.MainAdminKeyBoard);
                }
                else if(param == "Admin")
                {
                    await Admin.Registation(chatId, false);
                    await bot.SendTextMessageAsync(chatId, "Админ-панель", replyMarkup: Buttons.AdminKeyBoard);

                }
            }


            if (activeUser != null)
            {
                if (activeUser.IsRegEnd == false)
                {
                    await UserCommandLineHandler(activeUser, message_text);
                }
                else
                {
                    await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.RegEnd);
                }
            }
            else if(activeAdmin != null)
            {
                await AdminCommandLineHandler(activeAdmin, message_text);
            }
            


        }
        private static async Task CallBackHandler(Update update)
        {

            User activeUser = null;
            Admin activeAdmin = null;

            long chatId = update.CallbackQuery.From.Id;
            string query = update.CallbackQuery.Data;

            await bot.SendChatActionAsync(chatId, ChatAction.Typing);

            if (activeUser == null)
            {
                try
                {
                    activeUser = await User.GetUserByChatId(chatId);
                }
                catch (Exception ex)
                {
                    
                }
               
            }
            if (activeAdmin == null)
            {
                try
                {
                    activeAdmin = await Admin.GetAdminByChatId(chatId);

                }
                catch (Exception)
                {

                    
                }
            }

        
            if (query == Strings.Queries.StartQuery)
            {
                if(activeUser == null)
                {
                    await User.Registartion(chatId, update?.CallbackQuery?.From?.Username);
                    await bot.SendTextMessageAsync(chatId, Strings.Messages.AskFio);
                }
                else
                {
                    await bot.SendTextMessageAsync(chatId, Strings.Messages.RegEnd);
                }
            }
            if (activeUser != null)
            {
                if (activeUser.IsRegEnd == false)
                {
                    if (query.Contains(Strings.Queries.IsBmstuQuery))
                    {
                        int value = Convert.ToInt32(query.Split(' ')[1]);


                        //Not bmstu
                        if (value == 0)
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskUniversity, replyMarkup: Buttons.BackToUniversitySelection);
                            await activeUser.Update(param: "CommandLine", value: "Input_University", where: "chatId", key: chatId);

                        }
                        //bmstu
                        else if (value == 1)
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskGroup, replyMarkup: Buttons.BackToUniversitySelection);
                            await activeUser.Update(param: "CommandLine", value: "Input_Group", where: "chatId", key: chatId);
                            await activeUser.Update(param: "University", value: "BMSTU", where: "chatID", key: chatId);
                        }
                        //not student
                        else if (value == 2)
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskBirth, replyMarkup: Buttons.BackToUniversitySelection);
                            await activeUser.Update(param: "CommandLine", value: "Input_Birth", where: "chatId", key: chatId);
                            await activeUser.Update(param: "University", value: "Not a student", where: "chatID", key: chatId);
                            await activeUser.Update(param: "univer_group", "", "chatId", activeUser.ChatId);

                        }
                    }
                    else if (query.Contains("NeedSocres"))
                    {
                        int value = Convert.ToInt32(query.Split(' ')[1]);

                        await activeUser.Update("NeedScores", value, "chatId", chatId);

                        await EndReg(chatId);

                    }
                    else if (query.Contains("Back"))
                    {
                        if (query == "BackToInputFio")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskFio);
                            await activeUser.Update(param: "CommandLine", value: "Input_Fio", where: "chatId", key: chatId);
                        }
                        else if (query == "BackToBirth")
                        {

                            //bmstu: group -> birth
                            //not bmstu: university -> birth
                            //not a student: select not a student -> birth

                            InlineKeyboardMarkup back = null;

                            if (activeUser.University == "BMSTU")
                            {

                                back = Buttons.BackToInputGroup;
                            }
                            else if (activeUser.University == "Not a student")
                            {
                                back = Buttons.BackToUniversitySelection;
                            }
                            else
                            {
                                back = Buttons.BackToInputUniversity;
                            }

                            await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.AskBirth, replyMarkup: back);
                            await activeUser.Update("CommandLine", "Input_Birth", "chatId", activeUser.ChatId);
                        }
                        else if (query == "BackToContact")
                        {
                            //bmstu: group -> birth -> contact
                            //not bmstu: university -> birth -> contact
                            //not a student: select not a student -> birth -> contact
                            await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.AskContact, replyMarkup: Buttons.BackToInputBirth);

                            await activeUser.Update("CommandLine", "Input_Contact", "chatId", activeUser.ChatId);

                        }
                        else if (query == "BackToGroup")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskGroup, replyMarkup: Buttons.BackToUniversitySelection);
                            await activeUser.Update(param: "CommandLine", value: "Input_Group", where: "chatId", key: chatId);


                        }
                        else if (query == "BackToInputUniversity")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskUniversity, replyMarkup: Buttons.BackToUniversitySelection);
                            await activeUser.Update(param: "CommandLine", value: "Input_University", where: "chatId", key: chatId);

                        }
                        else if (query == "BackToUniversitySelection")
                        {
                            await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.IsBmst, replyMarkup: Buttons.IsBmstuKeyBoard);
                            await activeUser.Update("CommandLine", "-", "chatId", chatId);
                        }
                    }
                    else if (query.Contains("Error"))
                    {
                        if (query == "FioError")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskFioAgain);
                            await activeUser.Update("CommandLine", "Change_Fio", "chatId", chatId);
                        }
                        else if (query == "GroupError")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskGroupAgain);
                            await activeUser.Update("CommandLine", "Change_Group", "chatId", chatId);
                        }
                        else if (query == "ConcactError")
                        {
                            await bot.SendTextMessageAsync(chatId, Strings.Messages.AskContactAgain);
                            await activeUser.Update("CommandLine", "Change_Contact", "chatId", chatId);
                        }
                        else if (query == "ScoresError")
                        {
                            await activeUser.Update("NeedScores", Convert.ToInt16(!activeUser.NeedScores), "chatId", chatId);

                            await EndReg(chatId);
                        }

                    }
                    else if (query == "SendToXls")
                    {
                        await activeUser.Update("isRegEnd", 1, "chatId", chatId);

                        var mess = await bot.SendTextMessageAsync(chatId, Strings.Messages.SendUserCode + $"`{activeUser.makeCodeString()}`", ParseMode.Markdown);
                        await bot.PinChatMessageAsync(chatId, mess.MessageId);
                        await bot.SendTextMessageAsync(chatId, Strings.Messages.Info);

                        string exel = await googleHelper.InputUser(activeUser, GoogleHelper.Sheets[0]);
                        await activeUser.Update("Exel_Id", exel, "chatId", chatId);
                    }


                }
                else
                {
                    await bot.SendTextMessageAsync(activeUser.ChatId, Strings.Messages.RegEnd);
                }
            }

            if (activeAdmin != null)
            {
                if(query == "inputCode")
                {
                    await bot.SendTextMessageAsync(chatId, "Введи код, который тебе показал участник");
                    await activeAdmin.Update("CommandLine", "Input_Code", "chatId", chatId);
                }
                else if(query == "sendMessage")
                {
                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Всем участникам","SendMessage 0")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Не закончившим регистрацию","SendMessage 1")
                        }
                        ,
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Физра","SendMessage 2")
                        }
                        ,
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Только зарегистрированным","SendMessage 3")
                        }
                    });
                    await bot.SendTextMessageAsync(chatId, "Выбери тип рассылки", replyMarkup: keyboard);
                }
                else if (query.Contains("SendUserToXls"))
                {
                    await activeAdmin.Update("CommandLine", "-", "chatId", chatId);

                    string exel = query.Split(' ')[1];
                    bool scores = Convert.ToBoolean(query.Split(' ')[2]);

                    await googleHelper.Update(GoogleHelper.Sheets[0], exel, "ДА");

                    if (scores)
                    {
                        int code = Convert.ToInt32(query.Split(' ')[3]);
                        User get = await User.GetUserByCode(code);

                        await googleHelper.InputUser(get,GoogleHelper.Sheets[1]);

                    }

                    if (activeAdmin.isMainAdmin)
                    {
                        await bot.SendTextMessageAsync(chatId, "Админ-панель", replyMarkup: Buttons.MainAdminKeyBoard);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(chatId, "Админ-панель", replyMarkup: Buttons.AdminKeyBoard);
                    }
                }
                else if(query == "repete_input_code")
                {
                    await bot.SendTextMessageAsync(chatId, "Введи код, который тебе показал участник");
                    await activeAdmin.Update("CommandLine", "Input_Code", "chatId", chatId);
                }
                else if (query.Contains("SendMessage"))
                {
                    int param = Convert.ToInt32(query.Split(' ')[1]);

                    await bot.SendTextMessageAsync(chatId, "Введи сообщение для рассылки");
                    await activeAdmin.Update("CommandLine", $"SendMessage {param}", "chatId", chatId);
                }
            }
            try
            {
                await bot.DeleteMessageAsync(chatId, messageId: update.CallbackQuery.Message.MessageId);

            }
            catch (Exception)
            {

            }


        }
    }
}
