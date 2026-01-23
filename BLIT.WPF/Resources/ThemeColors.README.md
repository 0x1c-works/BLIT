# WPF-UI 主题颜色 IntelliSense 参考指南

## 概述

此项目包含 `ThemeColors.reference.xaml` 文件，它为 Rider IDE 提供 WPF-UI 主题颜色的 IntelliSense 支持。

## 文件信息

- **位置**: `BLIT.WPF/Resources/ThemeColors.reference.xaml`
- **作用**: 仅用于 IDE 智能提示，不参与运行时执行
- **来源**: WPF-UI 官方 Dark.xaml 主题
- **许可**: MIT

## 使用方式

### 1. 在 XAML 中使用颜色

编辑器输入时，按 `Ctrl+Space` 会自动显示所有可用的主题颜色。

```xaml
<!-- 背景颜色 -->
<Border Background="{DynamicResource ApplicationBackgroundBrush}"/>

<!-- 文本颜色 -->
<TextBlock Foreground="{DynamicResource TextFillColorPrimaryBrush}"/>

<!-- 控件填充色 -->
<Button Background="{DynamicResource ControlFillColorDefaultBrush}"/>

<!-- 边框颜色 -->
<Border BorderBrush="{DynamicResource DividerStrokeColorDefaultBrush}"/>
```

### 2. 主要颜色分类

#### 背景颜色
- `ApplicationBackgroundBrush` - 应用主背景
- `SolidBackgroundFillColorBaseBrush` - 实心背景基础色
- `SolidBackgroundFillColorSecondaryBrush` - 实心背景次级色
- `SolidBackgroundFillColorTertiaryBrush` - 实心背景三级色

#### 文本颜色
- `TextFillColorPrimaryBrush` - 主文本（高对比度）
- `TextFillColorSecondaryBrush` - 副文本
- `TextFillColorTertiaryBrush` - 三级文本
- `TextFillColorDisabledBrush` - 禁用文本

#### 控件填充色
- `ControlFillColorDefaultBrush` - 标准控件填充
- `ControlFillColorSecondaryBrush` - 二级控件填充
- `ControlFillColorTertiaryBrush` - 三级控件填充
- `ControlFillColorDisabledBrush` - 禁用状态

#### 卡片和容器
- `CardBackgroundFillColorDefaultBrush` - 卡片背景
- `CardStrokeColorDefaultBrush` - 卡片边框
- `LayerFillColorDefaultBrush` - 图层填充

#### 边框和分隔符
- `ControlStrokeColorDefaultBrush` - 标准边框
- `DividerStrokeColorDefaultBrush` - 分隔线
- `SurfaceStrokeColorDefaultBrush` - 表面边框

#### 系统状态颜色
- `SystemFillColorSuccessBrush` - 成功状态
- `SystemFillColorCautionBrush` - 警告状态
- `SystemFillColorCriticalBrush` - 错误/关键状态

### 3. IntelliSense 提示

在 XAML 编辑器中输入 `DynamicResource` 时：

```xaml
<Border Background="{DynamicResource |" 
```

按 `Ctrl+Space` 会显示所有可用的主题颜色资源，包括：
- 所有 `*Brush` 资源（推荐使用）
- 所有 `*Color` 资源（用于高级场景）

## 常见用途示例

### 工具栏背景
```xaml
<Border Background="{DynamicResource ApplicationBackgroundBrush}" 
        BorderBrush="{DynamicResource DividerStrokeColorDefaultBrush}"
        BorderThickness="0,0,0,1"/>
```

### 按钮样式
```xaml
<Button Background="{DynamicResource ControlFillColorDefaultBrush}"
        Foreground="{DynamicResource TextFillColorPrimaryBrush}"/>
```

### 卡片容器
```xaml
<Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"/>
```

### 禁用状态
```xaml
<TextBlock Foreground="{DynamicResource TextFillColorDisabledBrush}"/>
<Button Background="{DynamicResource ControlFillColorDisabledBrush}"/>
```

## 重要说明

⚠️ **此文件仅供 IDE 参考！**

- 参考文件会通过 `App.xaml` 合并到资源字典中
- 但实际的主题颜色是由 WPF-UI 的 `ui:ThemesDictionary` 提供的
- 如果需要修改主题颜色，应该在 `App.xaml` 中自定义，而不是修改此参考文件

## 更新主题颜色

如果你需要自定义主题颜色，在 `App.xaml` 中添加：

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:ThemesDictionary Theme="Dark" />
            <ui:ControlsDictionary />
            <ResourceDictionary Source="Resources/ThemeColors.reference.xaml" />
        </ResourceDictionary.MergedDictionaries>
        
        <!-- 自定义颜色覆盖 -->
        <SolidColorBrush x:Key="ApplicationBackgroundBrush" Color="#1E1E1E"/>
    </ResourceDictionary>
</Application.Resources>
```

## 相关资源

- [WPF-UI GitHub Repository](https://github.com/lepoco/wpfui)
- [WPF-UI Dark Theme Source](https://github.com/lepoco/wpfui/blob/main/src/Wpf.Ui/Resources/Theme/Dark.xaml)
- [Microsoft XAML Theming Guide](https://docs.microsoft.com/en-us/windows/apps/design/style/xaml-theme-resources)

## 许可证

此参考文件来自 WPF-UI 项目，遵循 MIT 许可证。
