using Autofac;
using BLIT.Win.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace BLIT.Win.Services;

public interface IStreamReadWrite {
    Task Write(Stream s);
    Task Read(Stream s);
}

public interface IProjectService<T> : INotifyPropertyChanged where T : IProject {
    T Current { get; }
    StorageFile CurrentFile { get; }
    string Name { get; }

    Task<T> NewProject(Func<T, Task> onLoad = null);
    Task Save(string filePath);
    Task Load(StorageFile file);
}

public interface IProject : INotifyPropertyChanged, IStreamReadWrite {
    void AfterLoaded();
}

internal class ProjectService<T> : BindableBase, IProjectService<T>, IDisposable where T : IProject {
    private ILifetimeScope _scope;
    private T _vm;
    public T Current {
        get => _vm;
        set => SetProperty(ref _vm, value);
    }

    private StorageFile _file;
    public StorageFile CurrentFile {
        get => _file;
        private set {
            SetProperty(ref _file, value);
            OnPropertyChanged(nameof(Name));
        }
    }
    public string Name {
        get {
            var path = CurrentFile?.Path;
            return string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
        }
    }

    public async Task<T> NewProject(Func<T, Task> onLoad = null) {
        Dispose();
        _scope = AppServices.Container.BeginLifetimeScope(typeof(T).Name);
        T vm = _scope.Resolve<T>();
        if (onLoad != null) {
            await onLoad(vm);
        }
        Current = vm;
        vm.AfterLoaded();
        return Current;
    }
    public async Task Save(string filePath) {
        using Stream s = File.OpenWrite(filePath);
        await Current.Write(s);
        CurrentFile = await StorageFile.GetFileFromPathAsync(filePath);
    }
    public async Task Load(StorageFile file) {
        using Stream s = await file.OpenStreamForReadAsync();
        await NewProject(vm => vm.Read(s));
        CurrentFile = file;
    }

    public void Dispose() {
        _scope?.Dispose();
    }
}
