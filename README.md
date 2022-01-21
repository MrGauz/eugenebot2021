# 🤖 EugeneBot2021

A C#-based Telegram bot for interactive communication with a soulless machine.

As is, it was used for creating a series of quizes for my best friend as a challenge for his Birthday. However, since it can send messages based on pre-defined triggers, it can be used for other human-machine communicative purposes.

The bot can send messages to the user in two ways: automatically and manually.
- For automatic communication, fill in the ``messages.json`` file with the text you want the user to receive. Format your messages text in [HTML format](https://core.telegram.org/api/entities).
- To send messages manually, send them directly to the bot via ``/forward`` command — bot will forward them to the user. E.g. ``/forward hello, stupid``. Format your message text as plain text, or as RTF.


## Preparation
- Clone project from GitHub:
	- ``git clone https://github.com/MrGauz/eugenebot2021.git``;
- In ``Bot.cs``, fill in the Bot token and Telegram IDs;
- Rename ``messages.json.example`` to ``messages.json`` and fill out the file with the messages you would want the user to receive;
- Launch the project and be amazed.
	

## Message triggers
- ``sendAt`` triggers the message at a specific time, defined in 24h format;
- ``sendAtLocation`` triggers the message when the user sends a specific geolocation to the bot;
	- please be advised: the message will be triggered within a 100m radius of the specified location.
- ``sendOnText`` triggers the message after the bot receives a specific text from the user;
- ``nextAfter`` triggers the message after a time given in seconds;
	- ``nextName`` defines the ID of the message that needs to be sent after.

## License
You can obviously use and re-use the code in any way that's granted by the [MIT License](https://github.com/MrGauz/eugenebot2021/blob/master/LICENSE.md).
