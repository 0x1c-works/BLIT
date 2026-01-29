using BLIT.CLI.Commands;
using ConsoleAppFramework;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
app.Add<BannerCommands>();
app.Add<SpriteCommands>();
await app.RunAsync(args);