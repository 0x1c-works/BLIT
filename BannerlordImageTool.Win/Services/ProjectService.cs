using Autofac;
using BannerlordImageTool.Win.Helpers;
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
    ILifetimeScope Scope { get; }
    T ViewModel { get; }
    StorageFile CurrentFile { get; }

    Task<T> NewProject(Func<T, Task> onLoad = null);
    Task Save(string filePath);
    Task Load(StorageFile file);
}

public interface IProject : INotifyPropertyChanged, IStreamReadWrite
{
    void AfterLoaded();
}

class ProjectService<T> : BindableBase, IProjectService<T>, IDisposable where T : IProject
{
    public ILifetimeScope Scope { get; private set; }

    public event Action<T> ProjectChanged;

    T _vm;
    public T ViewModel
    {
        get => _vm;
        set => SetProperty(ref _vm, value);
    }
    public StorageFile CurrentFile { get; private set; }

    public ProjectService()
    {
        PropertyChanged += OnPropertyChanged;
    }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel))
        {
            ProjectChanged?.Invoke(ViewModel);
        }
    }

    public async Task<T> NewProject(Func<T, Task> onLoad = null)
    {
        Dispose();
        Scope = AppServices.Container.BeginLifetimeScope(typeof(T).Name);
        T vm = Scope.Resolve<T>();
        if (onLoad != null)
        {
            await onLoad(vm);
        }
        ViewModel = vm;
        vm.AfterLoaded();
        return ViewModel;
    }
    public async Task Save(string filePath)
    {
        using Stream s = File.OpenWrite(filePath);
        await ViewModel.Write(s);
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
        Log.Debug("Project {Project} (scope {Scope}) is disposed", ViewModel, Scope?.Tag ?? "(new)");
        Scope?.Dispose();
    }
}
