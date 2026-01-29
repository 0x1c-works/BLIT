# BLIT.WPF 实现计划文档

> **生成日期**: 2026-01-21  
> **状态**: 已审核，准备实施  
> **目标**: 基于 BLIT.Win (WinUI 3) 创建 BLIT.WPF 版本

---

## 📋 目录

1. [项目概述](#1-项目概述)
2. [现有系统分析](#2-现有系统分析)
3. [关键决策](#3-关键决策)
4. [版本升级方案](#4-版本升级方案)
5. [技术架构](#5-技术架构)
6. [实施阶段详解](#6-实施阶段详解)
7. [代码迁移对照表](#7-代码迁移对照表)
8. [本地化迁移](#8-本地化迁移)
9. [虚拟列表优化](#9-虚拟列表优化)
10. [UI/UX 迁移](#10-uiux-迁移)
11. [依赖项变更](#11-依赖项变更)
12. [测试清单](#12-测试清单)
13. [风险与注意事项](#13-风险与注意事项)

---

## 1. 项目概述

### 1.1 目标

创建一个基于 **WPF UI** 库的桌面应用程序，保持与现有 **BLIT.Win** (WinUI 3) 一致的 UI/UX 和业务逻辑。

### 1.2 核心要求

- ✅ 使用 WPF UI 库 (https://github.com/lepoco/wpfui)
- ✅ 保持一致的 UI/UX 和操作逻辑
- ✅ 图片列表使用 virtualization 优化性能
- ✅ 升级到 .NET 9.0 和最新依赖库（仅 WPF 项目）
- ✅ 保留所有现有功能（项目操作、图标编辑、导出等）

### 1.3 约束条件

- ❌ **不修改现有的 BLIT.Win 项目**
- ✅ 仅在新建的 BLIT.WPF 中使用升级的依赖版本
- ✅ 保持简单背景效果（无 Mica/Acrylic）

---

## 2. 现有系统分析

### 2.1 BLIT.Win 项目结构

```
BLIT.Win/
├── Assets/                    # 图片资源
├── Controls/                  # 自定义控件
│   ├── LoadingOverlay.xaml    # 加载覆盖层
│   ├── Toast.xaml             # Toast 通知
│   └── ToastPanel.xaml        # Toast 面板
├── Helpers/                   # 工具类
│   ├── BindableBase.cs        # MVVM 基类
│   ├── BindingConverters.cs   # 值转换器
│   ├── FileHelpers.cs         # 文件操作
│   ├── I18nHelper.cs          # 国际化
│   ├── ImageHelper.cs         # 图片处理
│   ├── Logging.cs             # 日志
│   ├── NativeHelpers.cs       # 原生 API
│   ├── ThemeHelper.cs         # 主题管理
│   └── WinUIColorFormatter.cs # 颜色格式化
├── Pages/                     # 页面
│   ├── BannerIcons/           # Banner 图标编辑
│   │   ├── BannerIconsPage.xaml
│   │   ├── BannerColorsEditor.xaml
│   │   ├── BannerIconGroupEditor.xaml
│   │   └── Models/
│   │       ├── BannerIconsProject.cs
│   │       ├── BannerGroupEntry.cs
│   │       ├── BannerColorEntry.cs
│   │       └── BannerIconEntry.cs
│   └── Settings/              # 设置页面
│       ├── SettingsPage.xaml
│       └── BannerSpriteScanFoldersEditor.xaml
├── Services/                  # 服务层
│   ├── AppServices.cs         # DI 容器配置
│   ├── ConfirmDialogService.cs
│   ├── FileDialogService.cs
│   ├── LoadingService.cs
│   ├── NotificationService.cs
│   ├── ProjectService.cs
│   └── SettingsService.cs
├── Settings/                  # 设置数据
│   ├── BannerSettings.cs
│   └── GlobalSettings.cs
├── Theming/                   # 主题系统
│   ├── ThemedWindow.cs        # 主题窗口基类
│   └── WindowsSystemDispatcherQueueHelper.cs
├── App.xaml                   # 应用定义
├── App.xaml.cs                # 应用入口
├── MainWindow.xaml            # 主窗口
└── MainWindow.xaml.cs         # 主窗口逻辑
```

### 2.2 技术栈

| 组件 | 当前版本 (BLIT.Win) |
|------|---------------------|
| 框架 | WinUI 3 (Windows App SDK 1.4) |
| .NET | net6.0-windows10.0.19041.0 |
| DI | Autofac 8.0.0 |
| 日志 | Serilog 3.1.1 |
| 错误追踪 | Sentry 6.0.0 |
| 图片处理 | Magick.NET 13.5.0 |
| 序列化 | MessagePack 2.5.140 |

### 2.3 核心功能模块

1. **项目管理**: 新建/打开/保存项目文件
2. **Banner Icons 编辑**:
   - 图标组管理 (Group)
   - 图标管理 (Icon)
   - 颜色管理 (Color)
   - 纹理导出
3. **设置系统**: 扫描文件夹、分辨率配置等
4. **通知系统**: Toast 通知、加载状态

---

## 3. 关键决策

### 3.1 版本升级策略

| 项目 | 决策 |
|------|------|
| BLIT.Win | **保持不变** - 不修改现有项目 |
| BLIT.WPF | **全新升级** - 使用最新依赖版本 |

### 3.2 本地化方案

- ❌ 继续使用 WinUI 的 `.resw` 格式（不兼容 WPF）
- ✅ 转换为标准 `.resx` 格式（WPF 原生支持）
- 详情见[第 8 节](#8-本地化迁移)

### 3.3 背景效果

- ❌ 不实现 Mica/Acrylic（WinUI 特有）
- ✅ 使用纯色/渐变背景（简单方案）

### 3.4 虚拟列表

- 仅针对 **图标列表** (BannerIconEntry) 优化
- 使用 `VirtualizingStackPanel`
- 详情见[第 9 节](#9-虚拟列表优化)

---

## 4. 版本升级方案

### 4.1 BLIT.WPF 目标版本

| 组件 | 升级前 | 升级后 |
|------|--------|--------|
| **.NET** | net6.0 | **net9.0-windows** |
| **Autofac** | 8.0.0 | 8.1.5 |
| **MessagePack** | 2.5.140 | 2.6.120 |
| **Microsoft.Extensions.DI** | 8.0.0 | 9.0.0 |
| **Serilog** | 3.1.1 | 4.1.0 |
| **CsvHelper** | 30.0.1 | 33.0.1 |
| **Magick.NET** | 13.5.0 | 14.10.2 |
| **Sentry** | 6.0.0 | 保持不变 |
| **WinUI 控件** | CommunityToolkit | **Wpf.Ui 3.0.5** |

### 4.2 WPF UI 库选择

**Wpf.Ui** (https://github.com/lepoco/wpfui)
- 版本: 3.0.5
- 优点: Fluent Design 风格，与 WinUI 视觉一致
- 包含: NavigationView、Button、Dialog、Snackbar 等

---

## 5. 技术架构

### 5.1 项目结构

```
BLIT.WPF/
├── Assets/                     # 图片资源（复用）
├── Controls/                   # 自定义控件
│   ├── LoadingOverlay.xaml     # 加载覆盖层
│   └── ToastPanel.xaml         # Toast 通知（改用 Wpf.Ui）
├── Helpers/                    # 工具类（需适配）
│   ├── BindableBase.cs         # MVVM 基类
│   ├── BindingConverters.cs    # 值转换器
│   ├── FileHelpers.cs          # 文件操作
│   ├── I18nHelper.cs           # 国际化（重写）
│   ├── ImageHelper.cs          # 图片处理
│   ├── Logging.cs              # 日志
│   ├── NativeHelpers.cs        # 原生 API
│   └── ThemeHelper.cs          # 主题管理（重写）
├── Pages/                      # 页面
│   ├── BannerIcons/            # Banner 图标编辑
│   │   ├── BannerIconsPage.xaml（需适配）
│   │   ├── BannerColorsEditor.xaml
│   │   ├── BannerIconGroupEditor.xaml
│   │   └── Models/
│   │       ├── BannerIconsProject.cs（复用）
│   │       ├── BannerGroupEntry.cs（复用）
│   │       ├── BannerColorEntry.cs（复用）
│   │       └── BannerIconEntry.cs（复用）
│   └── Settings/               # 设置页面
│       ├── SettingsPage.xaml
│       └── BannerSpriteScanFoldersEditor.xaml
├── Services/                   # 服务层
│   ├── AppServices.cs          # DI 容器配置
│   ├── ConfirmDialogService.cs # 对话框（改用 Wpf.Ui）
│   ├── FileDialogService.cs    # 文件对话框
│   ├── LoadingService.cs       # 加载状态
│   ├── NotificationService.cs  # 通知（改用 Wpf.Ui）
│   ├── ProjectService.cs       # 项目操作
│   └── SettingsService.cs      # 设置
├── Settings/                   # 设置数据
│   ├── BannerSettings.cs       # 需适配（WinRT → System.Text.Json）
│   └── GlobalSettings.cs       # 需适配
├── Theming/                    # 主题系统
│   ├── ThemeDictionary.xaml    # 新建：主题资源
│   └── ThemeHelper.cs          # 主题切换
├── Properties/
│   └── Resources.resx          # 新建：本地化资源
│   └── Resources.zh-CN.resx    # 新建：中文资源
├── App.xaml                    # 应用定义
├── App.xaml.cs                 # 应用入口
├── MainWindow.xaml             # 主窗口
└── MainWindow.xaml.cs          # 主窗口逻辑
```

### 5.2 依赖关系

```
BLIT.WPF
    ├── BLIT.Banner (项目引用)
    ├── Wpf.Ui (NuGet)
    ├── Autofac (NuGet)
    ├── Serilog (NuGet)
    └── 其他工具库
```

---

## 6. 实施阶段详解

### 阶段 1: 项目骨架

**任务**: 创建基础项目结构和配置

1. **创建项目文件** `BLIT.WPF.csproj`
   - TargetFramework: `net9.0-windows`
   - UseWPF: `true`
   - 配置所有 NuGet 依赖

2. **创建目录结构**
   ```
   BLIT.WPF/
   ├── Assets/
   ├── Controls/
   ├── Helpers/
   ├── Pages/
   ├── Services/
   ├── Settings/
   ├── Theming/
   └── Properties/
   ```

3. **复制资源文件**
   - 从 `BLIT.Win/Assets/` 复制图片资源
   - 保留 `icon.ico`, `empty-asset.png`, `transparent-bg.png`

### 阶段 2: 核心基础

**任务**: 实现应用基础结构和依赖注入

1. **App.xaml / App.xaml.cs**
   - 应用生命周期管理
   - DI 容器初始化
   - Sentry 集成
   - 主题初始化

2. **AppServices.cs**
   - Autofac 容器配置
   - 服务注册（Singleton / InstancePerDependency）

3. **Settings 迁移**
   - `BannerSettings.cs`: WinRT API → System.Text.Json
   - `GlobalSettings.cs`: 同上

### 阶段 3: Helpers 迁移

**任务**: 迁移和适配工具类

| 文件 | 迁移方法 | 备注 |
|------|----------|------|
| `BindableBase.cs` | 直接迁移 | INotifyPropertyChanged 实现 |
| `BindingConverters.cs` | 直接迁移 | 值转换器 |
| `FileHelpers.cs` | 直接迁移 | 文件操作 |
| `ImageHelper.cs` | 直接迁移 | 图片处理 |
| `Logging.cs` | 直接迁移 | Serilog 配置 |
| `NativeHelpers.cs` | 评估迁移 | Win32 API 可能需调整 |
| **ThemeHelper.cs** | **重写** | WinUI ElementTheme → WPF ResourceDictionary |
| **I18nHelper.cs** | **重写** | WinUI ResourceLoader → .resx ResourceManager |

### 阶段 4: 主题系统

**任务**: 实现 WPF 主题切换

1. **创建主题资源文件**
   ```
   Theming/
   ├── ThemeDictionary.xaml      # 基础样式
   ├── DarkTheme.xaml            # 深色主题
   └── LightTheme.xaml           # 浅色主题
   ```

2. **实现 ThemeHelper**
   ```csharp
   public static class ThemeHelper {
       public static void SetTheme(ElementTheme theme) {
           // 根据 theme 切换 ResourceDictionary
       }
   }
   ```

### 阶段 5: 主窗口和导航

**任务**: 创建主窗口框架

**MainWindow.xaml**:
```xml
<Window x:Class="BLIT.WPF.MainWindow"
        xmlns:ui="http://schemas.lepo.co/wpfui/v2024/xaml">
    <Grid>
        <ui:NavigationView x:Name="AppNav">
            <!-- 导航菜单项 -->
        </ui:NavigationView>
        <controls:LoadingOverlay />
        <!-- Toast/通知区域 -->
    </Grid>
</Window>
```

**关键差异**:
| WinUI | WPF |
|-------|-----|
| `Microsoft.UI.Xaml.Controls.NavigationView` | `Wpf.Ui.Controls.NavigationView` |
| `Window` 扩展标题栏 | 标准 `Window` |

### 阶段 6: Controls 迁移

**任务**: 迁移自定义控件

1. **LoadingOverlay**
   - WinUI: `CommunityToolkit.WinUI.UI.Controls.Loading`
   - WPF: 使用 `ProgressRing` 或自定义实现

2. **Toast/通知**
   - WinUI: 自定义 ToastPanel + InfoBar
   - WPF: `Wpf.Ui.Controls.Snackbar` 或 `InfoBar`

### 阶段 7: Services 迁移

**任务**: 迁移服务层

| 服务 | 迁移方法 | 关键变化 |
|------|----------|----------|
| `FileDialogService` | 适配 | WinUI → `System.Windows.Forms` 或 `Ookii.Dialogs` |
| `ConfirmDialogService` | 重写 | WinUI `ContentDialog` → `Wpf.Ui.Controls.Dialog` |
| `NotificationService` | 重写 | Toast → `Wpf.Ui.Snackbar` |
| `LoadingService` | 适配 | 加载状态管理 |
| `ProjectService<T>` | 复用 | 核心逻辑保持 |

### 阶段 8: 本地化迁移

**任务**: WinUI `.resw` → WPF `.resx`

详情见[第 8 节](#8-本地化迁移)

### 阶段 9: 页面迁移

**任务**: 迁移业务页面

1. **BannerIconsPage** (重点)
   - 迁移 XAML 和 C#
   - 实现虚拟列表优化
   - 适配 Wpf.Ui 控件

2. **BannerColorsEditor**
3. **BannerIconGroupEditor**
4. **SettingsPage**

### 阶段 10: 虚拟列表优化

**任务**: 为图标列表实现虚拟化

详情见[第 9 节](#9-虚拟列表优化)

### 阶段 11: 测试和调整

**任务**: 验证功能完整性

- 编译检查
- 功能测试
- UI 一致性验证

---

## 7. 代码迁移对照表

### 7.1 WinUI → WPF API 对照

| 功能 | WinUI (BLIT.Win) | WPF (BLIT.WPF) |
|------|------------------|----------------|
| **窗口基类** | `ThemedWindow : Window` | `Window` |
| **导航** | `NavigationView` | `Wpf.Ui.NavigationView` |
| **框架** | `Frame` | `Frame` |
| **按钮** | `AppBarButton` | `Wpf.Ui.Button` |
| **列表** | `ListView` | `ListBox` / `DataGrid` |
| **图片** | `Image` | `Image` |
| **进度环** | `ProgressRing` | `ProgressBar` 或自定义 |
| **对话框** | `ContentDialog` | `Wpf.Ui.Dialog` |
| **信息栏** | `InfoBar` | `Wpf.Ui.InfoBar` |
| **主题** | `ElementTheme` | `ResourceDictionary` |
| **应用资源** | `Application.Current.Resources` | 同上 |
| **值转换** | `IValueConverter` | 同上 |

### 7.2 WinRT → .NET API 对照

| 功能 | WinRT (BLIT.Win) | .NET 9 (BLIT.WPF) |
|------|------------------|-------------------|
| **文件选取** | `Windows.Storage.Pickers` | `System.Windows.Forms` / `Ookii.Dialogs` |
| **存储** | `ApplicationData` | `Environment.GetFolderPath` / `Configuration` |
| **资源** | `ResourceLoader` | `ResourceManager` |
| **颜色** | `Windows.UI.Color` | `System.Windows.Media.Color` |
| **画笔** | `AcrylicBrush` | `SolidColorBrush` / `LinearGradientBrush` |
| **字体** | `FontIcon` | `TextBlock` |

### 7.3 需要重写的组件

| 组件 | 原因 | 建议方案 |
|------|------|----------|
| `ThemedWindow` | WinUI 特有背景效果 | 简化为标准 `Window` |
| `ThemeHelper` | WinUI 主题 API | 重写为 WPF ResourceDictionary 切换 |
| `I18nHelper` | WinUI ResourceLoader | 重写为 .resx ResourceManager |
| `ToastPanel` | WinUI InfoBar | 重写为 Wpf.Ui Snackbar/InfoBar |
| `LoadingOverlay` | WinUI Loading 控件 | 重写为自定义 XAML |
| `FileDialogService` | WinUI API | 重写为 `OpenFileDialog` / `SaveFileDialog` |
| `ConfirmDialogService` | WinUI ContentDialog | 重写为 `Wpf.Ui.Dialog` |

### 7.4 可直接复用的组件

| 组件 | 原因 |
|------|------|
| `BindableBase.cs` | 标准 INotifyPropertyChanged |
| `BindingConverters.cs` | 标准 IValueConverter |
| `BannerIconsProject.cs` | 纯 C# 业务逻辑 |
| `BannerGroupEntry.cs` | 纯 C# 数据模型 |
| `BannerColorEntry.cs` | 纯 C# 数据模型 |
| `BannerIconEntry.cs` | 纯 C# 数据模型 |
| `AppServices.cs` | Autofac 配置（需调整命名空间） |
| `Logging.cs` | Serilog 配置 |
| `SettingsService.cs` | 纯 C# 逻辑 |

---

## 8. 本地化迁移

### 8.1 迁移概述

| 项目 | BLIT.Win (WinUI) | BLIT.WPF |
|------|------------------|----------|
| **资源格式** | `.resw` | `.resx` |
| **读取 API** | `ResourceLoader` | `ResourceManager` |
| **XAML 绑定** | `x:Uid="Key"` | `I18nExtension` 或绑定 |
| **字符串数量** | 103 | 103 |
| **语言** | en-US, zh-CN | en-US, zh-CN |

### 8.2 资源文件结构

```
Properties/
├── Resources.resx              # 默认（英文）
└── Resources.zh-CN.resx        # 中文
```

### 8.3 I18nHelper 实现

```csharp
using System.Resources;

namespace BLIT.WPF.Helpers;

public class I18n {
    private static readonly ResourceManager _resManager =
        new("BLIT.WPF.Properties.Resources", typeof(I18n).Assembly);

    public static I18n Current => _current ??= new I18n();
    private static I18n? _current;

    public string GetString(string id) {
        return _resManager.GetString(id) ?? id;
    }
}
```

### 8.4 XAML 绑定方式

**方式 1: MarkupExtension（推荐）**

```csharp
[MarkupExtensionReturnType(typeof(string))]
public class I18nExtension : MarkupExtension {
    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        return I18n.Current.GetString(Key);
    }
}
```

**XAML 使用**:
```xml
<TextBlock Text="{local:I18n AppTitle}" />
<Button Content="{local:I18n ButtonOK}" />
```

**方式 2: 代码绑定**

```xml
<TextBlock Text="{Binding Source={x:Static local:I18n.Current}, 
                  Path=GetString('AppTitle')}" />
```

### 8.5 字符串键命名规则

**WinUI 格式** (`.resw`):
```
AppButtonNewProject.Label
AppNavMenuBanner.Content
AppTitle.Text
```

**WPF 格式** (`.resx`):
```
AppButtonNewProject_Label
AppNavMenuBanner_Content
AppTitle_Text
```

或保持原样，代码中处理：
```csharp
// 统一转换：WinUI "Key.Property" → WPF "Key_Property"
string ConvertKey(string winuiKey) {
    return winuiKey.Replace(".", "_");
}
```

### 8.6 资源键对照表

| 原 WinUI 键 | 新 WPF 键 |
|-------------|-----------|
| `AppTitle.Text` | `AppTitle_Text` |
| `AppNavMenuBanner.Content` | `AppNavMenuBanner_Content` |
| `AppNavMenuHelp.Content` | `AppNavMenuHelp_Content` |
| `AppButtonNewProject.Label` | `AppButtonNewProject_Label` |
| `AppButtonSaveProject.Label` | `AppButtonSaveProject_Label` |
| ... | ... |

---

## 9. 虚拟列表优化

### 9.1 优化目标

仅针对 **BannerIconsPage** 中的图标列表（`BannerIconEntry` 列表）实现虚拟化。

### 9.2 实现方案

**XAML 结构**:

```xml
<ListBox ItemsSource="{Binding ViewModel.Icons}"
         VirtualizingStackPanel.IsVirtualizing="True"
         VirtualizingStackPanel.VirtualizationMode="Recycling">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel />
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
    <ListBox.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Image Source="{Binding PreviewImage}"
                       Width="64" Height="64"
                       Stretch="Uniform" />
                <TextBlock Text="{Binding IconID}" />
            </Grid>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### 9.3 性能优化配置

| 配置项 | 值 | 说明 |
|--------|-----|------|
| `IsVirtualizing` | `True` | 启用虚拟化 |
| `VirtualizationMode` | `Recycling` | 回收项目容器 |
| `ScrollUnit` | `Item` | 按项滚动 |
| `CacheLength` | `10` | 前后缓存 10 项 |

### 9.4 异步图片加载

```csharp
public class AsyncImageConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var path = value as string;
        if (string.IsNullOrEmpty(path)) return null;

        // 异步加载图片
        return Task.Run(() => {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.DecodePixelWidth = 64; // 减小内存占用
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        });
    }
}
```

### 9.5 注意事项

1. **图片缓存**: 实现 LRU 缓存，避免重复加载
2. **缩略图**: 使用缩略图而非原图
3. **延迟加载**: 滚动时延迟加载非可见区域
4. **内存管理**: 及时释放不再可见的图片

---

## 10. UI/UX 迁移

### 10.1 主窗口布局

**BLIT.Win (WinUI)**:

```xml
<theming:ThemedWindow>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="32" /> <!-- 标题栏 -->
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel x:Name="AppTitleBar" Grid.Row="0">
            <Image Source="Assets/Square44x44Logo.scale-100.png" />
            <TextBlock x:Uid="AppTitle" />
        </StackPanel>

        <NavigationView x:Name="AppNav" Grid.Row="1">
            <NavigationView.MenuItems>
                <NavigationViewItem x:Uid="AppNavMenuBanner" Icon="Flag" />
            </NavigationView.MenuItems>
            <Frame x:Name="AppContent" />
        </NavigationView>

        <controls:LoadingOverlay Grid.Row="1" />
        <controls:ToastPanel Grid.Row="1" />
    </Grid>
</theming:ThemedWindow>
```

**BLIT.WPF (新)**:

```xml
<Window x:Class="BLIT.WPF.MainWindow"
        xmlns:ui="http://schemas.lepo.co/wpfui/v2024/xaml">
    <Grid>
        <ui:NavigationView x:Name="AppNav"
                           IsBackButtonVisible="Collapsed">
            <ui:NavigationView.MenuItems>
                <ui:NavigationViewItem Content="{local:I18n AppNavMenuBanner}"
                                       Icon="Flag"
                                       Tag="BannerIcons" />
            </ui:NavigationView.MenuItems>
            <ui:NavigationView.FooterMenuItems>
                <ui:NavigationViewItem Content="{local:I18n AppNavMenuHelp}"
                                       Icon="Help"
                                       SelectsOnInvoked="False" />
            </ui:NavigationView.FooterMenuItems>
            <Frame x:Name="AppContent" />
        </ui:NavigationView>

        <controls:LoadingOverlay />
    </Grid>
</Window>
```

### 10.2 样式统一

**App.xaml 资源**:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:Themes.XamlDarkThemeDictionary />
            <!-- 自定义样式 -->
        </ResourceDictionary.MergedDictionaries>

        <SolidColorBrush x:Key="Danger" Color="#AA3232" />
        <SolidColorBrush x:Key="Warn" Color="#FFBB00" />

        <Style x:Key="DangerButton" TargetType="Button">
            <Setter Property="Background" Value="{StaticResource Danger}" />
            <Setter Property="Foreground" Value="White" />
        </Style>
    </ResourceDictionary>
</Application.Resources>
```

### 10.3 主题切换

```csharp
public static class ThemeHelper {
    public static void SetTheme(bool isDark) {
        var uri = isDark
            ? "pack://application:,,,/Wpf.Ui;component/Themes/Dark.xaml"
            : "pack://application:,,,/Wpf.Ui;component/Themes/Light.xaml";

        var themeDict = new ResourceDictionary { Source = uri };
        Application.Current.Resources.MergedDictionaries.Add(themeDict);
    }
}
```

---

## 11. 依赖项变更

### 11.1 NuGet 包变更

**新增依赖 (WPF 特有)**:
- `Wpf.Ui` 3.0.5 - 主要 UI 库

**移除依赖 (WinUI 特有)**:
- `Microsoft.WindowsAppSDK`
- `Microsoft.Windows.SDK.BuildTools`
- `CommunityToolkit.WinUI.UI.Controls`
- `CommunityToolkit.WinUI.UI.Media`
- `Vanara.PInvoke.Shell32` (可能保留用于原生 API 调用)

**版本升级**:
| 包 | 旧版本 | 新版本 |
|----|--------|--------|
| Autofac | 8.0.0 | 8.1.5 |
| Autofac.Extensions.DependencyInjection | 9.0.0 | 9.0.0 |
| MessagePack | 2.5.140 | 2.6.120 |
| Microsoft.Extensions.DependencyInjection | 8.0.0 | 9.0.0 |
| Serilog | 3.1.1 | 4.1.0 |
| Serilog.Sinks.Console | 5.0.1 | 5.0.1 |
| Serilog.Sinks.Debug | 2.0.0 | 2.0.0 |
| Serilog.Sinks.File | 5.0.0 | 5.0.0 |
| Magick.NET-Q16-AnyCPU | 13.5.0 | 14.10.2 |
| CsvHelper | 30.0.1 | 33.0.1 |
| Sentry | 6.0.0 | 6.0.0 (保持) |
| Sentry.Serilog | 6.0.0 | 6.0.0 (保持) |

### 11.2 项目引用

```xml
<ItemGroup>
    <ProjectReference Include="..\BLIT.Banner\BLIT.Banner.csproj" />
</ItemGroup>
```

---

## 12. 测试清单

### 12.1 编译检查

- [ ] 项目成功编译
- [ ] 无编译警告
- [ ] 所有 NuGet 包正确还原
- [ ] 项目引用正确

### 12.2 功能测试

- [ ] 应用启动正常
- [ ] 主题切换正常（深色/浅色）
- [ ] 导航正常（Banner Icons / Settings）
- [ ] 新建项目功能
- [ ] 打开项目功能
- [ ] 保存项目功能
- [ ] 导出功能
- [ ] 添加图标
- [ ] 编辑图标组
- [ ] 编辑颜色
- [ ] 设置保存/加载
- [ ] 本地化切换（英文/中文）

### 12.3 性能测试

- [ ] 虚拟列表滚动流畅
- [ ] 大数量图标加载性能
- [ ] 内存占用正常
- [ ] 图片加载无卡顿

### 12.4 UI 一致性检查

- [ ] 布局与 BLIT.Win 一致
- [ ] 字体显示正确
- [ ] 颜色主题正确
- [ ] 图标显示正确
- [ ] 按钮样式一致

---

## 13. 风险与注意事项

### 13.1 已知风险

| 风险 | 等级 | 缓解措施 |
|------|------|----------|
| Magick.NET 14.x API 变更 | 中 | 编译时检查，必要时调整调用 |
| Wpf.Ui 版本兼容性 | 低 | 使用稳定版本 3.0.5 |
| 本地化格式转换错误 | 低 | 双重验证资源文件 |
| 虚拟列表性能问题 | 低 | 充分测试和优化 |

### 13.2 注意事项

1. **不修改 BLIT.Win**
   - 确保所有修改都在 `BLIT.WPF` 目录
   - 不要触碰现有的 `BLIT.Win` 项目

2. **资源文件路径**
   - `.resx` 文件必须在 `Properties` 目录
   - 文件名必须符合 WPF 约定

3. **Wpf.Ui 版本**
   - 使用版本 3.0.5（文档中指定）
   - 注意 API 可能随版本变化

4. **图片资源**
   - 复用 `BLIT.Win/Assets/` 中的文件
   - 确保 `icon.ico` 存在

### 13.3 回滚计划

如果遇到重大问题：
1. 保留 `BLIT.Win` 作为备份
2. 使用 Git 分支进行 WPF 开发
3. 必要时可回退到 WinUI 版本

---

## 附录 A: 常用命令

```bash
# 编译项目
dotnet build BLIT.WPF/BLIT.WPF.csproj

# 发布项目
dotnet publish BLIT.WPF/BLIT.WPF.csproj -c Release

# 还原依赖
dotnet restore BLIT.WPF/BLIT.WPF.csproj

# 检查依赖更新
dotnet outdated BLIT.WPF/BLIT.WPF.csproj
```

## 附录 B: 参考资源

- **Wpf.Ui**: https://github.com/lepoco/wpfui
- **Wpf.Ui NuGet**: https://www.nuget.org/packages/Wpf.Ui
- **Magick.NET**: https://github.com/dlemstra/Magick.NET
- **Autofac**: https://autofac.org/
- **Serilog**: https://serilog.net/

---

## 变更日志

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-01-21 | 1.0 | 初始版本，完成需求分析和计划制定 |
