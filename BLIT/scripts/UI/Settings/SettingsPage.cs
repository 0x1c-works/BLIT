using Godot;

public partial class SettingsPage : Control {
    public static readonly (string, string)[] LANGS = [
        ("en", "English"),
        ("zh", "中文"),
    ];
    [Export] public OptionButton? Language { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (Language != null) {
            var currentLocale = TranslationServer.GetLocale();
            Language.Clear();
            var selectedIndex = -1;
            for (var i = 0; i < LANGS.Length; i++) {
                (string, string) lang = LANGS[i];
                Language.AddItem(lang.Item2, i);
                Language.SetItemMetadata(i, lang.Item1);
                if (currentLocale.StartsWith(lang.Item1)) {
                    selectedIndex = i;
                }
            }
            Language.Select(selectedIndex);
            Language.ItemSelected += OnLanguageSelected;
        }
    }

    private void OnLanguageSelected(long index) {
        TranslationServer.SetLocale((string)(Language?.GetItemMetadata((int)index))!);
    }

}
