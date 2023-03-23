using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace TelegramBot;

public class Bot : BackgroundService
{
    private readonly ILogger<Bot> logger;
    private readonly IOptions<BotOptions> botOptions;
    
    public Bot(ILogger<Bot> logger, IOptions<BotOptions> botOptions)
    {
        this.logger = logger;
        this.botOptions = botOptions;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting up...");
        
        var botClient = new TelegramBotClient(botOptions.Value.Token);

        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );
        
        await Task.Delay(-1, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken stoppingToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not null)
        {
            if (message.Text.ToLower().Contains("chang") && message.Text.ToLower().Contains("status"))
            {
                await SendStatusUpdate(botClient, message.Chat.Id, stoppingToken);
            }
        }
        
        if (message.Photo is not null)
        {
            await DownloadPhoto(botClient, message, stoppingToken);
        }

        if (message.Video is not null)
        {
            await DownloadVideo(botClient, message, stoppingToken);
        }
    }

    private async Task DownloadPhoto(ITelegramBotClient botClient, Message message, CancellationToken stoppingToken)
    {
        if (message.Photo is null)
            return;
    
        var fileId = message.Photo.Last().FileId;
        var filePath = Path.Combine(botOptions.Value.PhotoPath, GetFileName(".jpg"));
    
        await DownloadFileAsync(botClient, fileId, filePath, stoppingToken);
    }
    
    private async Task DownloadVideo(ITelegramBotClient botClient, Message message, CancellationToken stoppingToken)
    {
        if (message.Video is null)
            return;
    
        var fileId = message.Video.FileId;
        var filePath = Path.Combine(botOptions.Value.VideoPath, GetFileName(".mp4"));
    
        await DownloadFileAsync(botClient, fileId, filePath, stoppingToken);
    }
    
    private async Task SendStatusUpdate(ITelegramBotClient botClient, long chatId, CancellationToken stoppingToken)
    {
        var numPhotos = Directory.GetFiles(botOptions.Value.PhotoPath, "*.jpg").Length;
        var numVideos = Directory.GetFiles(botOptions.Value.VideoPath, "*.mp4").Length;
        
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Hail, warrior! From the dawn of my existence, I have amassed {numPhotos} captured memories and {numVideos} tales of moving imagery. Qapla!",
            cancellationToken: stoppingToken);
    }
    
    private async Task DownloadFileAsync(ITelegramBotClient botClient, string fileId, string filePath, CancellationToken stoppingToken)
    {
        var fileInfo = await botClient.GetFileAsync(fileId, stoppingToken);
    
        logger.LogInformation($"Downloading file {fileId}");
    
        if (fileInfo.FilePath is null)
            return;
    
        await using Stream fileStream = File.OpenWrite(filePath);
        await botClient.DownloadFileAsync(fileInfo.FilePath, fileStream, stoppingToken);
    
        logger.LogInformation($"Downloaded file {fileId} to {filePath}");
    }
    
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken stoppingToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(exception, errorMessage);
        return Task.CompletedTask;
    }
    
    private static string GetFileName(string fileExtension) =>
        DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + fileExtension;
}
