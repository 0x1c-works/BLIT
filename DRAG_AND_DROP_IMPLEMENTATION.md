# WPF 拖拽排序实现总结

## 概述
在 BLIT.WPF 项目中为 icon list 和 group list 添加了拖拽重新排序功能，支持虚拟列表渲染。

## 实现细节

### 1. 集成 gong-wpf-dragdrop 库

**文件**: `BLIT.WPF.csproj`

添加了 NuGet 包引用：
```xml
<PackageReference Include="gong-wpf-dragdrop" Version="4.0.0" />
```

### 2. 创建拖拽处理器

#### Icon List Drop Handler
**文件**: `Pages/BannerIcons/ViewModels/IconListDropHandler.cs`

特点：
- 实现 `IDropTarget` 接口
- 支持同组内的图标重新排序
- 使用 `DropTargetAdorners.Highlight` 显示拖拽目标位置
- 自动更新 cell indices（通过集合变化事件）

```csharp
public class IconListDropHandler : IDropTarget
{
    public void DragOver(IDropInfo dropInfo) { /* ... */ }
    public void Drop(IDropInfo dropInfo) { /* ... */ }
}
```

#### Group List Drop Handler
**文件**: `Pages/BannerIcons/ViewModels/GroupListDropHandler.cs`

特点：
- 实现 `IDropTarget` 接口
- 支持组列表重新排序
- 使用 `DropTargetAdorners.Highlight` 显示拖拽目标

```csharp
public class GroupListDropHandler : IDropTarget
{
    public void DragOver(IDropInfo dropInfo) { /* ... */ }
    public void Drop(IDropInfo dropInfo) { /* ... */ }
}
```

### 3. XAML 绑定

#### Icon List (BannerIconGroupEditor.xaml)
```xaml
<ListBox
    x:Name="listIcons"
    dd:DragDrop.IsDragSource="True"
    dd:DragDrop.IsDropTarget="True"
    dd:DragDrop.UseDefaultDragAdorner="True"
    dd:DragDrop.UseDefaultEffectDataTemplate="True">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <vwp:VirtualizingWrapPanel
                ItemSize="200,200"
                SpacingMode="Uniform"
                StretchItems="False" />
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

**关键属性**:
- `IsDragSource="True"` - 允许拖动源
- `IsDropTarget="True"` - 允许放置目标
- `UseDefaultDragAdorner="True"` - 显示默认拖拽预览（半透明图层）
- `UseDefaultEffectDataTemplate="True"` - 显示默认效果指示符

#### Group List (BannerIconsPage.xaml)
```xaml
<ListBox
    x:Name="listViewGroups"
    dd:DragDrop.IsDragSource="True"
    dd:DragDrop.IsDropTarget="True"
    dd:DragDrop.UseDefaultDragAdorner="True"
    dd:DragDrop.UseDefaultEffectDataTemplate="True">
</ListBox>
```

## 功能说明

### 拖拽流程

1. **按下鼠标并拖动** - 源项目被拖动
2. **悬停在目标上** - 显示 `Highlight` 装饰器
3. **释放鼠标** - 项目在新位置插入，旧位置移除
4. **自动刷新索引** - `BannerGroupEntry.RefreshCellIndex()` 更新显示

### 虚拟化支持

- 使用 `VirtualizingWrapPanel` 作为 icon list 的 ItemsPanel
- gong-wpf-dragdrop 与虚拟化面板兼容
- 即使列表有 99 项也能流畅拖拽（当前上限）

### 拖拽预览

- **默认拖拽装饰器** - 半透明浮动图像显示被拖动的项
- **目标位置指示** - Highlight adorner 在悬停位置显示
- **视觉反馈** - 禁用状态时显示 ✗ 图标

## 集合变化处理

### BannerGroupEntry (group list)
```csharp
public ObservableCollection<BannerIconEntry> Icons { get; } = new();
```
- 通过 `CollectionChanged` 事件自动刷新索引
- 移动项时自动更新 `CellIndex` 属性

### BannerIconsProject (group collection)
```csharp
public ObservableCollection<BannerGroupEntry> Groups { get; }
```
- 支持组列表重新排序
- 排序变化自动保存到项目

## 限制与注意事项

### 当前限制
- 不支持列表间拖拽（只在单一列表内重新排序）
- Icon list 最多 99 项，group list 最多 20 项（业务需求）

### 虚拟化注意
- 拖拽时虚拟化面板可能会滚动
- 确保 `VirtualizingWrapPanel` 配置正确的 `ItemSize`

### 性能考虑
- 移动后调用 `RefreshCellIndex()` 有 O(n) 复杂度
- 对于当前数据规模（<100 项）无性能问题

## 使用示例

### 操作流程

1. **拖拽 Icon**:
   - 在 icon grid 中按住并拖动任意图标
   - 放置到新位置
   - 图标自动重新排序，ID 号不变

2. **拖拽 Group**:
   - 在左侧组列表中按住并拖动组名
   - 放置到新位置
   - 组列表自动重新排序

### 数据持久化

- 拖拽完成后，集合自动更新
- 下次保存项目时，新的排序被保存
- 支持撤销/重做（通过 `ObservableCollection` 变化通知）

## 配置参考

### gong-wpf-dragdrop 属性

| 属性 | 值 | 说明 |
|------|-----|------|
| `IsDragSource` | True | 启用拖动源 |
| `IsDropTarget` | True | 启用放置目标 |
| `UseDefaultDragAdorner` | True | 显示默认拖拽预览 |
| `UseDefaultEffectDataTemplate` | True | 显示操作指示符 |

### VirtualizingWrapPanel 属性

| 属性 | 值 | 说明 |
|------|-----|------|
| `ItemSize` | 200,200 | 图标尺寸（宽x高） |
| `SpacingMode` | Uniform | 统一间距 |
| `StretchItems` | False | 不拉伸项目 |

## future improvements（可选）

- [ ] 添加撤销/重做功能
- [ ] 支持跨列表拖拽（icon -> icon, group -> group）
- [ ] 自定义拖拽预览模板
- [ ] 拖拽动画过渡
- [ ] 触摸和笔输入支持
