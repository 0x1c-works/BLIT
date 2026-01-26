using BLIT.WPF.Helpers;
using BLIT.WPF.Pages.BannerIcons.Models;
using BLIT.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLIT.WPF.Pages.BannerIcons.ViewModels;

public partial class BannerColorsEditorViewModel : ObservableObject {
    private const int TITLE_MAX_COUNT = 3;

    // 项目数据引用
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Colors))]
    private BannerIconsProject? projectData;

    // UI 状态属性
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FirstSelectedColor))]
    [NotifyPropertyChangedFor(nameof(HasSelectedColor))]
    [NotifyPropertyChangedFor(nameof(IsSingleSelected))]
    [NotifyPropertyChangedFor(nameof(IsMultipleSelection))]
    [NotifyPropertyChangedFor(nameof(SelectedColorIDs))]
    [NotifyPropertyChangedFor(nameof(MoreColorsText))]
    [NotifyPropertyChangedFor(nameof(IsForSigil))]
    [NotifyPropertyChangedFor(nameof(IsForBackground))]
    private IEnumerable<BannerColorEntry> _selectedColors = [];

    // 计算属性
    public BannerColorEntry? FirstSelectedColor => SelectedColors.FirstOrDefault();
    public bool HasSelectedColor => SelectedColors.Any();
    
    /// <summary>
    /// 颜色列表，从 ProjectData 中暴露
    /// </summary>
    public IEnumerable<BannerColorEntry>? Colors => ProjectData?.Colors;
    public bool IsSingleSelected => SelectedColors.Count() == 1;
    public bool IsMultipleSelection => SelectedColors.Count() > 1;

    public string SelectedColorIDs =>
        string.Join(", ", SelectedColors.Take(TITLE_MAX_COUNT).Select(c => c.ID));

    public string MoreColorsText {
        get {
            var moreCount = SelectedColors.Count() - TITLE_MAX_COUNT;
            return moreCount > 0 ? string.Format(I18n.Current.GetString("AndMore"), moreCount) : string.Empty;
        }
    }

    public bool IsForSigil {
        get => GetMultiSelectionFlag(c => c.IsForSigil);
        set => SetMultiSelectionFlag((c, v) => c.IsForSigil = v, value);
    }

    public bool IsForBackground {
        get => GetMultiSelectionFlag(c => c.IsForBackground);
        set => SetMultiSelectionFlag((c, v) => c.IsForBackground = v, value);
    }

    // ============ RelayCommand 们 ============

    [RelayCommand]
    public void AddColor() {
        if (ProjectData == null) return;
        ProjectData.AddColor();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedColor))]
    public async Task DeleteColors() {
        if (!SelectedColors.Any() || ProjectData == null) {
            return;
        }

        var confirmDialog = AppServices.Get<IConfirmDialogService>();
        if (confirmDialog == null) return;

        var result = await confirmDialog.ShowDanger(
            I18n.Current.GetString("DialogDeleteColor/Title"),
            string.Format(I18n.Current.GetString("DialogDeleteColor/Content"), SelectedColors.Count()));

        if (result != ContentDialogResult.Primary) {
            return;
        }

        ProjectData.DeleteColors(SelectedColors);
    }

    [RelayCommand]
    public void SortColors() {
        ProjectData?.SortColors();
    }

    // ============ 辅助方法 ============

    private bool GetMultiSelectionFlag(Func<BannerColorEntry, bool> getter) {
        if (!HasSelectedColor) return false;
        return SelectedColors.All(c => getter?.Invoke(c) ?? false);
    }

    private void SetMultiSelectionFlag(Action<BannerColorEntry, bool> setter, bool value) {
        foreach (BannerColorEntry item in SelectedColors) {
            setter?.Invoke(item, value);
        }
        OnPropertyChanged(nameof(IsForSigil));
        OnPropertyChanged(nameof(IsForBackground));
    }

    // ============ 事件处理 ============

    public void OnSelectionChanged(IEnumerable<BannerColorEntry> colors) {
        SelectedColors = colors;
    }
}
