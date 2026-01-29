using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.IO;

namespace BLIT.WPF.Services;

public interface IStreamReadWrite {
    Task Write(Stream s);
    Task Read(Stream s);
}

public interface IProjectService<T> : INotifyPropertyChanged where T : IProject {
    T? Current { get; }
    string? CurrentFile { get; }
    string Name { get; }

    Task<T> NewProject(Func<T, Task>? onLoad = null);
    Task Save(string filePath);
    Task Load(string file);
}

public interface IProject : INotifyPropertyChanged, IStreamReadWrite {
    void AfterLoaded();
}

internal partial class ProjectService<T> : ObservableObject, IProjectService<T>, IDisposable where T : IProject {
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Name))]
    private string? _currentFile;

    private ILifetimeScope? _scope;
    private T? _vm;

    #region IDisposable Members

    public void Dispose() {
        _scope?.Dispose();
    }

    #endregion

    #region IProjectService<T> Members

    public T? Current {
        get => _vm;
        set => SetProperty(ref _vm, value);
    }

    public string Name {
        get {
            var path = CurrentFile;
            return string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
        }
    }

    public async Task<T> NewProject(Func<T, Task>? onLoad = null) {
        Dispose();
        _scope = AppServices.Container.BeginLifetimeScope(typeof(T).Name);
        var vm = _scope.Resolve<T>();
        if (onLoad != null) {
            await onLoad(vm);
        }

        CurrentFile = null;
        Current = vm;
        vm.AfterLoaded();
        return Current;
    }

    public async Task Save(string filePath) {
        if (Current == null) {
            throw new InvalidOperationException("No current project to save");
        }

        using Stream s = File.OpenWrite(filePath);
        await Current.Write(s);
        CurrentFile = filePath;
    }

    public async Task Load(string file) {
        using Stream s = File.OpenRead(file);
        await NewProject(vm => vm.Read(s));
        CurrentFile = file;
    }

    #endregion
}