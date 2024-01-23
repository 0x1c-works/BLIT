using Autofac;
using BLIT.scripts.Common;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace BLIT.scripts.Services;

public interface IProjectService<T> : INotifyPropertyChanged where T : IProject
{
    T? Current { get; }
    string? Name { get; }
    string? FilePath { get; }

    Task<T> NewProject(Func<T, Task>? onLoad = null);
    Task Save(string filePath);
    Task Open(string filePath);
}

public interface IProject : INotifyPropertyChanged
{
    void AfterLoaded();
    Task Write(Stream s);
    Task Read(Stream s);
}

class ProjectService<T> : BindableBase, IProjectService<T>, IDisposable where T : IProject
{
    ILifetimeScope? _scope;

    T? _project;
    public T? Current
    {
        get => _project;
        set
        {
            T? old = Current;
            var changed = SetProperty(ref _project, value);
        }
    }
    string? _filePath;
    public string? FilePath
    {
        get => _filePath;
        private set
        {
            if (SetProperty(ref _filePath, value))
            {
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public string? Name => string.IsNullOrEmpty(FilePath) ? null : Path.GetFileName(FilePath);

    public ProjectService()
    {
        Log.Information($"ProjectService<{typeof(T).Name}> created");
    }

    public async Task<T> NewProject(Func<T, Task>? onLoad = null)
    {
        // Close the opened project if any
        Dispose();
        _scope = AppService.Container.BeginLifetimeScope(typeof(T).Name);
        T vm = _scope.Resolve<T>();
        if (onLoad != null)
        {
            await onLoad(vm);
        }
        Current = vm;
        vm.AfterLoaded();
        FilePath = null;
        return Current;
    }
    public async Task Save(string filePath)
    {
        if (Current == null)
        {
            Log.Error($"No {typeof(T).Name} to save");
            return;
        }
        using Stream s = File.OpenWrite(filePath);
        await Current.Write(s);
        FilePath = filePath;
    }
    public async Task Open(string filePath)
    {
        using Stream s = File.Open(filePath, FileMode.Open);
        await NewProject(vm => vm.Read(s));
        FilePath = filePath;
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}

