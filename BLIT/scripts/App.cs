using BLIT.scripts.Common;
using BLIT.scripts.Services;
using Godot;

public partial class App : Control
{
    public App() : base()
    {
        Logging.Initialize();
        AppConfig.Load();
        AppService.Configure();
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
