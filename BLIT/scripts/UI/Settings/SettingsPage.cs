using BLIT.scripts.Common;
using Godot;

public partial class SettingsPage : Control {
    public static readonly (string, string)[] LANGS = [
        ("en", "English"),
        ("zh", "中文"),
    ];
    [Export] public OptionButton? LanguageSelect { get; set; }
    [Export] public Label? VersionLabel { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (LanguageSelect != null) {
            var currentLocale = TranslationServer.GetLocale();
            LanguageSelect.Clear();
            var selectedIndex = -1;
            for (var i = 0; i < LANGS.Length; i++) {
                (string, string) lang = LANGS[i];
                LanguageSelect.AddItem(lang.Item2, i);
                LanguageSelect.SetItemMetadata(i, lang.Item1);
                if (currentLocale.StartsWith(lang.Item1)) {
                    selectedIndex = i;
                }
            }
            LanguageSelect.Select(selectedIndex);
            LanguageSelect.ItemSelected += OnLanguageSelected;
        }
        if (VersionLabel != null) {
            VersionLabel.Text = (string)ProjectSettings.GetSetting("application/config/version");
        }
    }

    private void OnLanguageSelected(long index) {
        TranslationServer.SetLocale((string)(LanguageSelect?.GetItemMetadata((int)index))!);
    }
    private void OnOpenLog() {
        OS.ShellShowInFileManager(Logging.Folder, true);
    }

}
