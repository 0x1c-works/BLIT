using BLIT.scripts.Models.BannerIcons;
using BLIT.scripts.Services;
using Godot;
using System.ComponentModel;

public partial class BannerIconsEditor : Control {
    private IProjectService<BannerIconsProject> ProjectService => AppService.Get<IProjectService<BannerIconsProject>>();

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

    private async void OnNewProject() {

        ConfirmDialogService service = AppService.Get<ConfirmDialogService>();
        var ok = await service.Ask(this,
                    title: "CONFIRMATION/TITLE/CREATE_PROJECT",
                    text: "CONFIRMATION/TEXT/CREATE_PROJECT");
        if (ok) {
            await ProjectService.NewProject();
        }
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

    private async void OnSaveProject() {
        if (string.IsNullOrEmpty(ProjectService.FilePath)) {
            OnSaveProjectAs();
        } else {
            await ProjectService.Save(ProjectService.FilePath);
        }
    }
    private void OnSaveProjectAs() {
        FileDialog dlg = CreateProjectFileDialog(ProjectService.FilePath);
        dlg.FileSelected += async (path) => {
            if (!path.EndsWith(BannerIconsProject.FILE_EXTENSION)) {
                path += BannerIconsProject.FILE_EXTENSION;
            }
            await ProjectService.Save(path);
        };
        dlg.FileMode = FileDialog.FileModeEnum.SaveFile;
        dlg.Title = "SAVE_BIP";
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
