using Autofac;
using BannerlordImageTool.Win.Helpers;
using Serilog;
using System;

namespace BannerlordImageTool.Win.Services;

public interface IProjectService<T> where T : BindableBase
{
    T ViewModel { get; }
    T NewProject();

    event Action<T> ProjectChanged;
}

class ProjectService<T> : BindableBase, IProjectService<T>, IDisposable where T : BindableBase
{
    ILifetimeScope _scope;

    T _vm;
    public event Action<T> ProjectChanged;

    public T ViewModel
    {
        get => _vm;
        set
        {
            if (_vm == value) return;
            SetProperty(ref _vm, value);
        }
    }

    public ProjectService()
    {
        PropertyChanged += OnPropertyChanged;
        NewProject();
    }

    void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel))
        {
            ProjectChanged?.Invoke(ViewModel);
        }
    }

    public T NewProject()
    {
        Dispose();
        _scope = AppServices.Container.BeginLifetimeScope(typeof(T).Name);
        ViewModel = _scope.Resolve<T>();
        return ViewModel;
    }

    public void Dispose()
    {
        Log.Debug("Project {Project} (scope {Scope}) is disposed", ViewModel, _scope?.Tag ?? "(new)");
        _scope?.Dispose();
    }
}
