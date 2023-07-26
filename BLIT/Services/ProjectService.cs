using Autofac;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BLIT.Services;

public interface IStreamReadWrite
{
    Task Write(Stream s);
    Task Read(Stream s);
}

public interface IProjectService<T> : INotifyPropertyChanged where T : IProject
{
    T? Current { get; }
    StorageFile? CurrentFile { get; }
    string Name { get; }

    Task<T> NewProject(Func<T, Task>? onLoad = null);
    Task Save(string filePath);
    Task Load(StorageFile file);
}

public interface IProject : INotifyPropertyChanged, IStreamReadWrite
{
    void AfterLoaded();
}

class ProjectService<T> : ReactiveObject, IProjectService<T>, IDisposable where T : IProject
{
    ILifetimeScope? _scope;
    readonly CompositeDisposable _disposables = new();

    [Reactive] public T? Current { get; private set; }
    [Reactive] public StorageFile? CurrentFile { get; private set; }
    [ObservableAsProperty] public string Name { get; } = string.Empty;

    public ProjectService()
    {
        this.WhenAnyValue(x => x.CurrentFile)
            .Select(file => {
                var path = CurrentFile?.Path;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path);
            })
            .ToPropertyEx(this, x => x.Name)
            .DisposeWith(_disposables);
    }

    public async Task<T> NewProject(Func<T, Task>? onLoad = null)
    {
        Dispose();
        _scope = App.Container.BeginLifetimeScope(typeof(T).Name);
        T vm = _scope.Resolve<T>();
        if (onLoad != null)
        {
            await onLoad(vm);
        }
        Current = vm;
        vm.AfterLoaded();
        return Current;
    }
    public async Task Save(string filePath)
    {
        if (Current == null)
        {
            throw new InvalidOperationException("No project loaded");
        }
        using Stream s = File.OpenWrite(filePath);
        await Current.Write(s);
        CurrentFile = await StorageFile.GetFileFromPathAsync(filePath);
    }
    public async Task Load(StorageFile file)
    {
        using Stream s = await file.OpenStreamForReadAsync();
        await NewProject(vm => vm.Read(s));
        CurrentFile = file;
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _disposables.Dispose();
    }
}
