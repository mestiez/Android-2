# Android 2

Android 2 is a complete rewrite of Android 1, a Discord bot written for the People Playground Discord server.

![android preview](https://i.imgur.com/n3oKwy9.png)

## Instructions

There are two main components to the bot: the server and the client.

### Server

The server runs on .NET 5.0 so can be built for whatever platform you want to run it on. Read the [dotnet documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) to learn more about publishing.

After it is built, create a file called `bot_token` in the output directory and paste your bot token inside. The file name should match `bot_token` exactly, no extension.
Also create `listening_address` with the target listener address, and `client_address` with the target client address.

Then, you can simply run

`dotnet ./Backend.dll`

This has to be run directly from inside the output folder to ensure the files you created are found.

**IMPORTANT: THERE IS NO SECURITY IN PLACE YET. Anyone can access the control panel if they have the address.
Host it locally or implement some sort of authentication**

### Client

The client is built using Angular (I am sorry) so you need NodeJS and the Angular CLI to build it. Before building it, edit the `src/app/models.ts` file.
Replace the value assignment in the first line with the address of your server.

To build it, run

`npm i`

and then

`ng build --prod`

It will generate a static website in the `dist` directory, which can be served normally. Though, now that you already have the nightmare NodeJS thing on your computer, you could just use [http-server](https://github.com/http-party/http-server).

When all this is done, you should be able to navigate to wherever your client is listening and every guild your bot is in will show up.

## Using the interface

This section is a work-in-progress. It is pretty intuitive so you shouldn't have much trouble. If you know C# then the source code should make perfect sense to you.
