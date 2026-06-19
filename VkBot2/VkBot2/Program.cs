using VkBotFramework;
using VkBotFramework.Models;
using VkNet.Model;
using System;
using System.Collections.Generic;
using System.Threading;

string accessToken = "vk1.a.CO2QQovwEx871nF6fZGFxV75MZsj443nI7og8FsQW8YZLkd0DYMiihKOFj63JD0X3NTEEsaRrc6GiPHtChtlyuPwrg8T5eii4fmKdqoTaYOg8Itj0aZ4K-InHNkAqwZzNC2rzE8JCclq10-3OSER8e1HNtCpH1MkUmeo-rOyHXs4uTyY0-TGXCEKopP_1kUdD9MmFAKx3ywgE1zBBOrjxw";
string groupURL = "https://vk.com/club239161894";
VkBot bot = new VkBot(accessToken, groupURL);

var zagadki = new List<(string vopros, string otvet)>
{
   ("Что исчезает, как только произнесешь его название?", "Тишина"),
   ("Без рук, без ног, а дверь открывает. Что это?", "Ветер"),
   ("Чем больше берёшь, тем больше становится. Что это?", "Яма"),
   ("Висит груша, нельзя скушать. Что это?", "Лампочка"),
   ("Утром на четырёх, днём на двух, вечером на трёх. Кто это?", "Человек"),
   ("Два кольца, два конца, а посередине гвоздик. Что это?", "Ножницы"),
   ("Не огонь, а жжётся. Что это?", "Крапива"),
   ("Что проходит через города и поля, но никогда не двигается с места?","Дорога"),
};
var podpischiki = new List<long>();
var ozhidayutOtveta = new HashSet<long>();
int tekushayaZagadka = 0;
int vcherashnyayaZagadka = -1;

var timer = new Timer(OtpravitZagadku, null, TimeSpan.Zero, TimeSpan.FromHours(24));
bot.OnMessageReceived += BotOnMessageReceived;
Console.WriteLine("VkBot run");
bot.Start();
Console.ReadLine();

void BotOnMessageReceived(object? sender, MessageReceivedEventArgs e)
{
    try
    {
        var message = e.Message.Text?.Trim();
        var peerId = e.Message.PeerId;
        if (string.IsNullOrWhiteSpace(message) || peerId == null)
            return;
        long userId = peerId.Value;
        if (!podpischiki.Contains(userId))
            podpischiki.Add(userId);
        switch (message.ToLower())
        {
            case "привет":
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Привет! Я бот-загадочник!\nКаждый день новая загадка.\n\nНапиши: загадка",
                    PeerId = peerId,
                    RandomId = Environment.TickCount
                });
                return;
            case "загадка":
                var z = zagadki[tekushayaZagadka];
                ozhidayutOtveta.Add(userId);
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Сегодняшняя загадка:\n" + z.vopros + "\n\nНапиши ответ!",
                    PeerId = peerId,
                    RandomId = Environment.TickCount
                });
                return;
            case "ответ":
                var z2 = zagadki[tekushayaZagadka];
                ozhidayutOtveta.Remove(userId);
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Ответ на загадку:\n" + z2.otvet,
                    PeerId = peerId,
                    RandomId = Environment.TickCount
                });
                return;

            case "вчерашняя":
            case "вчера":
                if (vcherashnyayaZagadka == -1)
                {
                    bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        Message = "Вчерашней загадки ещё нет! Загадки начались только сегодня.",
                        PeerId = peerId,
                        RandomId = Environment.TickCount
                    });
                }
                else
                {
                    var vcher = zagadki[vcherashnyayaZagadka];
                    bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        Message = $"Вчерашняя загадка:\n{vcher.vopros}\n\nОтвет: {vcher.otvet}",
                        PeerId = peerId,
                        RandomId = Environment.TickCount
                    });
                }
                return;
        }
        if (ozhidayutOtveta.Contains(userId))
        {
            string pravOtvet = zagadki[tekushayaZagadka].otvet;
            ozhidayutOtveta.Remove(userId);

            if (message.Equals(pravOtvet, StringComparison.OrdinalIgnoreCase))
            {
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Правильно, молодец!",
                    PeerId = peerId,
                    RandomId = Environment.TickCount
                });
            }
            else
            {
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Неверно \nПравильный ответ: " + pravOtvet,
                    PeerId = peerId,
                    RandomId = Environment.TickCount
                });
            }
            return;
        }

        bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            Message = "Команды:\nзагадка — получить загадку\nответ — узнать ответ\nвчерашняя — посмотреть вчерашнюю загадку",
            PeerId = peerId,
            RandomId = Environment.TickCount
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

void OtpravitZagadku(object? state)
{
    try
    {
        if (podpischiki.Count > 0)
        {
            var staraya = zagadki[tekushayaZagadka];
            foreach (var id in podpischiki)
            {
                ozhidayutOtveta.Remove(id);
                bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    Message = "Ответ на вчерашнюю загадку:\n" + staraya.otvet,
                    PeerId = id,
                    RandomId = Environment.TickCount
                });
            }
        }
        vcherashnyayaZagadka = tekushayaZagadka;
        tekushayaZagadka = (tekushayaZagadka + 1) % zagadki.Count;
        var novaya = zagadki[tekushayaZagadka];
        foreach (var id in podpischiki)
        {
            ozhidayutOtveta.Add(id);
            bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                Message = "Загадка дня:\n" + novaya.vopros + "\n\nНапиши ответ!",
                PeerId = id,
                RandomId = Environment.TickCount
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка таймера: {ex.Message}");
    }
}
