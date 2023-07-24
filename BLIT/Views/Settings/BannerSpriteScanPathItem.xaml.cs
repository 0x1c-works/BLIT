﻿using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BLIT.Views.Settings;

/// <summary>
/// Interaction logic for BannerSpriteScanPathItem.xaml
/// </summary>
public partial class BannerSpriteScanPathItem
{
    public bool IsEditing
    {
        get => ViewModel!.IsEditing;
        set => ViewModel!.IsEditing = value;
    }

    public BannerSpriteScanPathItem()
    {
        InitializeComponent();

        this.WhenActivated((disposables) => {
            this.OneWayBind(ViewModel, x => x.DisplayVisibility, x => x.blockDisplay.Visibility).DisposeWith(disposables);
            this.OneWayBind(ViewModel, x => x.EditorVisibility, x => x.blockEditor.Visibility).DisposeWith(disposables);

            // Display bindings
            this.OneWayBind(ViewModel, x => x.DisplayPath, x => x.txtPath.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.StartEdit, x => x.btnEdit).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.Delete, x => x.btnDelete).DisposeWith(disposables);

            // Editor bindings
            this.Bind(ViewModel, x => x.Path, x => x.inputPath.Text, signalViewUpdate: btnSave.Events().Click).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.QuitEdit, x => x.btnSave, Observable.Return(true)).DisposeWith(disposables);
            this.BindCommand(ViewModel, x => x.QuitEdit, x => x.btnCancel, Observable.Return(false)).DisposeWith(disposables);

            ViewModel?.EditStateChanged.Subscribe((x) => {
                if (x)
                {
                    // FIXME: focus the textbox
                    var succ = inputPath.Focus();
                    Keyboard.Focus(inputPath);
                    inputPath.SelectAll();
                }
            }).DisposeWith(disposables);
        });
    }
}