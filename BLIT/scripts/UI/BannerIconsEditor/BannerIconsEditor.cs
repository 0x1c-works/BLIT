using Autofac;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System.ComponentModel;

public partial class BannerIconsEditor : Control
{
    IProjectService<BannerIconsProject>? _projectService;
    IProjectService<BannerIconsProject> ProjectService => _projectService ?? AppService.Container.Resolve<IProjectService<BannerIconsProject>>();

    [Export] public Label? LabelFileName { get; set; }
    [Export] public FileDialog? OpenProjectDialog { get; set; }
    [Export] public FileDialog? SaveProjectDialog { get; set; }

    public override void _Ready()
    {
        if (ProjectService == null)
        {
            throw new System.ApplicationException("ProjectService is not injected");
        }
        ProjectService.PropertyChanged += OnProjectServicePropertyChanged;
        UpdateFileName();
    }

    void OnNewProject()
    {
        var confirm = new ConfirmationDialog() {
            Title = "CONFIRMATION/TITLE/CREATE_PROJECT",
            DialogText = "CONFIRMATION/TEXT/CREATE_PROJECT",
            OkButtonText = "YES",
            CancelButtonText = "NO",
            DialogAutowrap = true,
            MinSize = new Vector2I(320, 100),
        };
        //confirm.Theme = ThemeDB.GetProjectTheme();
        confirm.GetOkButton().CustomMinimumSize = new Vector2I(60, 0);
        confirm.GetCancelButton().CustomMinimumSize = new Vector2I(60, 0);
        confirm.GetOkButton().Pressed += () => {
            ProjectService.NewProject();
        };
        AddChild(confirm);
        confirm.PopupCentered();
    }
    void OnOpenProject()
    {
        FileDialog dlg = CreateProjectFileDialog(ProjectService.FilePath);
        dlg.FileSelected += (path) => {
            ProjectService.Open(path);
        };
        dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
        dlg.Title = "OPEN_BIP";
        dlg.PopupCentered();
    }

    FileDialog CreateProjectFileDialog(string? filePath)
    {
        var dlg = new FileDialog() {
            Access = FileDialog.AccessEnum.Filesystem,
            UseNativeDialog = true,
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            CurrentPath = filePath ?? string.Empty,
            Filters = [$"*{BannerIconsProject.FILE_EXTENSION} ; Banner Icons Project",],
            CancelButtonText = "CANCEL",
        };
        AddChild(dlg);
        return dlg;
    }

    void OnProjectServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectService.Name))
        {
            UpdateFileName();
        }
    }

    void UpdateFileName()
    {
        if (LabelFileName != null)
        {
            LabelFileName.Text = ProjectService.Name ?? "NEW_PROJECT";
        }
    }
}
