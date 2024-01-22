using BLIT.scripts.Common;
using BLIT.scripts.Services;
using Godot;

public partial class App : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Logging.Initialize();
        AppConfig.Load();
        AppService.Configure();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
