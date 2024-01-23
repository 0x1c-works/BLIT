using Autofac;
using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System.ComponentModel;

public partial class BannerIconsEditor : Control {
    private IProjectService<BannerIconsProject> ProjectService => AppService.Container.Resolve<IProjectService<BannerIconsProject>>();

    [Export] public Label? LabelFileName { get; set; }
    [Export] public FileDialog? OpenProjectDialog { get; set; }
    [Export] public FileDialog? SaveProjectDialog { get; set; }

    public override void _Ready() {
        if (ProjectService == null) {
            throw new System.ApplicationException("ProjectService is not injected");
        }
        ProjectService.PropertyChanged += OnProjectServicePropertyChanged;
        UpdateFileName();
    }

    private void OnNewProject() {
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

    private void OnOpenProject() {
        FileDialog dlg = CreateProjectFileDialog(ProjectService.FilePath);
        dlg.FileSelected += (path) => {
            ProjectService.Open(path);
        };
        dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
        dlg.Title = "OPEN_BIP";
        dlg.PopupCentered();
    }

    private FileDialog CreateProjectFileDialog(string? filePath) {
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

    private void OnProjectServicePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ProjectService.Name)) {
            UpdateFileName();
        }
    }

    private void UpdateFileName() {
        if (LabelFileName != null) {
            LabelFileName.Text = ProjectService.Name ?? "NEW_PROJECT";
        }
    }
}
