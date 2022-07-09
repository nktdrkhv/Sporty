using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Sporty.Handlers;

var builder = new ConfigurationBuilder().AddJsonFile(Path.Combine(Environment.CurrentDirectory, "config.json"), false, true);
var config = builder.Build();

var bot = new TelegramBotClient(config["TelegramToken"]);
var me = await bot.GetMeAsync();
Console.Title = me.Username ?? "Sporty";

using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions()
{
    AllowedUpdates = Array.Empty<UpdateType>(),
    ThrowPendingUpdates = true,
};

bot.StartReceiving(
    updateHandler: UpdateHandler.HandleUpdateAsync,
    pollingErrorHandler: UpdateHandler.PollingErrorHandler,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token);

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

cts.Cancel();