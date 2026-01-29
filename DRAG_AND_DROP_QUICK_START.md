# 拖拽排序功能 - 快速使用指南

## 功能介绍

你的 BLIT.WPF 项目现在支持以下拖拽功能：

### ✅ Icon List 拖拽排序
- **位置**: BannerIconGroupEditor（右侧 icon grid）
- **功能**: 在同一组内拖动图标重新排序
- **虚拟化**: 支持 VirtualizingWrapPanel，可高效处理大列表
- **数据量**: 支持最多 99 个图标

### ✅ Group List 拖拽排序
- **位置**: BannerIconsPage（左侧组列表）
- **功能**: 拖动组名重新排序
- **数据量**: 支持最多 20 个组

## 使用方式

### 拖拽图标
```
1. 在右侧 icon grid 中找到要移动的图标
2. 按住鼠标左键并拖动图标
3. 悬停在目标位置（会看到蓝色高亮框）
4. 释放鼠标完成排序
```

### 拖拽组
```
1. 在左侧组列表中找到要移动的组
2. 按住鼠标左键并拖动组名
3. 悬停在目标位置（会看到蓝色高亮框）
4. 释放鼠标完成排序
```

## 视觉反馈

拖拽时你会看到：
- **半透明拖拽图像** - 显示被拖动的项（默认装饰器）
- **蓝色高亮框** - 表示项将被放置的位置（Highlight adorner）
- **无法放置时** - 显示 ✗ 符号

## 技术实现

### 已安装的库
- **gong-wpf-dragdrop** v4.0.0 - MVVM 友好的拖拽框架

### 关键文件

| 文件 | 功能 |
|------|------|
| `BLIT.WPF.csproj` | 添加了 gong-wpf-dragdrop NuGet 包 |
| `IconListDropHandler.cs` | Icon list 拖拽处理逻辑 |
| `GroupListDropHandler.cs` | Group list 拖拽处理逻辑 |
| `BannerIconGroupEditor.xaml` | Icon list 拖拽 UI 绑定 |
| `BannerIconsPage.xaml` | Group list 拖拽 UI 绑定 |

### XAML 关键属性

```xaml
<!-- 启用拖拽 -->
dd:DragDrop.IsDragSource="True"
dd:DragDrop.IsDropTarget="True"

<!-- 显示视觉反馈 -->
dd:DragDrop.UseDefaultDragAdorner="True"
dd:DragDrop.UseDefaultEffectDataTemplate="True"
```

## 数据流向

```
用户拖拽 → Drop Handler 处理
  ↓
从源集合移除项
  ↓
插入到目标位置
  ↓
刷新索引（RefreshCellIndex）
  ↓
UI 自动更新（ObservableCollection 通知）
  ↓
数据持久化（下次保存时）
```

## 常见问题

### Q: 拖拽不起作用怎么办？
**A**: 
- 确保项目编译成功（已验证）
- 检查 ListBox 的 `IsDragSource` 和 `IsDropTarget` 属性是否为 `True`
- 确保 drop handler 被正确绑定（已自动使用默认行为）

### Q: 虚拟列表拖拽性能如何？
**A**: 
- VirtualizingWrapPanel 只渲染可见项，拖拽性能优秀
- 即使有 99 个图标也能流畅操作
- 自动虚拟化，无需手动配置

### Q: 拖拽后数据是否保存？
**A**: 
- 拖拽完成后，数据立即更新
- 下次使用"保存项目"命令时，新排序被保存到文件
- ObservableCollection 自动通知所有 binding 更新

### Q: 支持跨列表拖拽吗？
**A**: 
- 目前不支持（设计限制）
- icon 只能在组内排序
- 组只能在组列表内排序
- 可在将来扩展此功能

## 性能优化建议

虽然当前数据规模不需要额外优化，但如果未来有需求：

1. **批量拖拽**: 可使用 `SelectionMode="Extended"` 进行多选拖拽
2. **虚拟化调整**:
   ```xaml
   <vwp:VirtualizingWrapPanel
       ItemSize="200,200"
       VirtualizingPanel.CacheLength="300"
       VirtualizingPanel.CacheLengthUnit="Pixel" />
   ```
3. **异步刷新**: 对于超大列表，可考虑异步更新索引

## 扩展功能

### 可选实现（如需要）

- **撤销/重做**: 使用 MVVM Toolkit 的 RelayCommand
- **拖拽动画**: 在 drop handler 中添加 Storyboard
- **自定义拖拽预览**: 修改 `DragAdornerTemplate`
- **触摸支持**: gong-wpf-dragdrop 原生支持

## 技术文档

详细的实现细节请参考: `DRAG_AND_DROP_IMPLEMENTATION.md`

## 相关资源

- [gong-wpf-dragdrop GitHub](https://github.com/punker76/gong-wpf-dragdrop)
- [VirtualizingWrapPanel GitHub](https://github.com/sbaeumlisberger/VirtualizingWrapPanel)
- 项目中已有注释代码说明逻辑
