# Telegram Bot

Telegram bot to automatically download photos and videos from Telegram.

## Installation

### Raspberry Pi OS (32-bit)

```console
$ dotnet publish --runtime linux-arm --self-contained -c Release
$ rsync -a --delete --exclude 'appsettings.json' ./bin/Release/net7.0/linux-arm/publish/ io.lan:/home/kyle/telegram-bot
$ ssh io.lan chmod +x /home/kyle/telegram-bot/telegram-bot
$ ssh io.lan systemctl --user restart telegram-bot.service
```

`appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Bot": {
    "Token": "",
    "PhotoPath": "",
    "VideoPath": ""
  }
}
```

## Running as a systemd service

### Create Service

To create a systemd service, create a file called `telegram-bot.service` in the `~/.config/systemd/user/` directory:

```console
$ mkdir -p ~/.config/systemd/user
$ nano ~/.config/systemd/user/telegram-bot.service
```

In the file, add the following configuration:

```ini
[Unit]
Description=Telegram Bot

[Service]
Type=simple
Restart=always
RestartSec=10
ExecStart=%h/telegram-bot/telegram-bot
WorkingDirectory=%h/telegram-bot
KillMode=process

[Install]
WantedBy=default.target
```

### Enable and start

Enable and start the service:

```console
$ systemctl --user daemon-reload
$ systemctl --user enable telegram-bot.service
$ systemctl --user start telegram-bot.service
```

### Lingering

By default, systemd user instances are started when a user logs in and stopped when their last session ends. However, to ensure that your service is always running, even when no user is logged in, you can enable "lingering" for your user:

```console
$ loginctl enable-linger $(whoami)
```

### Check status

Check the status of the service:

```console
$ systemctl --user status telegram-bot.service
```

### Logs

View the logs for the service:

```console
$ journalctl -f --user-unit telegram-bot.service
```
