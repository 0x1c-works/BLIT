using ConsoleAppFramework;

var app = ConsoleApp.Create();
app.Add<BLIT.CLI.Commands.BannerCommands>();
app.Add<BLIT.CLI.Commands.SpriteCommands>();
await app.RunAsync(args);
