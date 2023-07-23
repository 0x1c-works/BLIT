using BLIT.Helpers;
using ReactiveUI;
using Splat;
using System;
using System.Reactive;
using System.Reflection;

namespace BLIT.ViewModels;
public class SettingsViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "settings";
    public IScreen HostScreen { get; }
    public Language[] SupportedLanguages = new Language[] {
        new("English", "en"),
        new("中文", "zh"),
    };
    public string AppVersion
    {
        get
        {
            Version? ver = Assembly.GetExecutingAssembly().GetName().Version;
            return ver == null ? "-" : $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
        }
    }
    public ReactiveCommand<Unit, Unit> OpenLogFolder { get; }


    public SettingsViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        OpenLogFolder = ReactiveCommand.Create(() => {
            FileSystemHelper.OpenFolderInExplorer(FileSystemHelper.AppLogPath);
        });
    }
}

public record Language(string DisplayName, string Value);
