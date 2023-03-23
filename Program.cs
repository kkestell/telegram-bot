using TelegramBot;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<BotOptions>(hostContext.Configuration.GetSection("Bot"));
        services.AddHostedService<Bot>();
    })
    .Build();

host.Run();
