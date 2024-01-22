using Autofac;
using BLIT.scripts.Common;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace BLIT.scripts.Services;

public interface IProjectService<T> where T : IProject
{
    T? Current { get; }
    string Name { get; }

    Task<T> NewProject(Func<T, Task>? onLoad = null);
    Task Save(string filePath);
    Task Load(string filePath);
}

public interface IProject : INotifyPropertyChanged
{
    string FilePath { get; }
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
            if (changed)
            {
                if (old != null) old.PropertyChanged -= OnProjectPropertyChanged;
                if (Current != null) Current.PropertyChanged += OnProjectPropertyChanged;
            }
        }
    }

    void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Current.FilePath))
        {
            OnPropertyChanged(nameof(Name));
        }
    }
    public string Name
    {
        get
        {
            var path = Current?.FilePath;
            return string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
        }
    }

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
    }
    public async Task Load(string filePath)
    {
        using Stream s = File.Open(filePath, FileMode.Open);
        await NewProject(vm => vm.Read(s));
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}

