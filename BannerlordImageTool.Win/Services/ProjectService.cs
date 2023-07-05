using Autofac;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace BannerlordImageTool.Win.Services;

public interface IStreamReadWrite
{
    Task Write(Stream s);
    Task Read(Stream s);
}

public interface IProjectService<T> where T : IProject
{
    event Action<T> ProjectChanged;
    T Current { get; }
    StorageFile CurrentFile { get; }
    string Name { get; }

    Task<T> NewProject(Func<T, Task> onLoad = null);
    Task Save(string filePath);
    Task Load(StorageFile file);
}

public interface IProject : INotifyPropertyChanged, IStreamReadWrite
{
    void AfterLoaded();
}

class ProjectService<T> : IProjectService<T>, IDisposable where T : IProject
{
    ILifetimeScope _scope;

    public event Action<T> ProjectChanged;

    T _vm;
    public T Current
    {
        get => _vm;
        set
        {
            _vm = value;
            ProjectChanged?.Invoke(value);
        }
    }
    public StorageFile CurrentFile { get; private set; }
    public string Name
    {
        get
        {
            var path = CurrentFile?.Path;
            if (string.IsNullOrEmpty(path)) return "";
            return Path.GetFileName(path);
        }
    }

    public async Task<T> NewProject(Func<T, Task> onLoad = null)
    {
        Dispose();
        _scope = AppServices.Container.BeginLifetimeScope(typeof(T).Name);
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
        Log.Debug("Project {Project} (scope {Scope}) is disposed", Current, _scope?.Tag ?? "(new)");
        _scope?.Dispose();
    }
}
