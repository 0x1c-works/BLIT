# Microsoft Store 应用打包和提交 - 最终执行指南

## ✅ 当前状态

所有项目配置已经准备就绪：

| 配置项 | 状态 | 值 |
|------|------|-----|
| 应用包名称 | ✓ | `ZoroWizcas.350BF036A26` |
| Publisher | ✓ | `CN=AD42E090-F4E2-47FD-BAF5-866425B8F937` |
| 版本号 | ✓ | `0.1.22.0` (自动递增) |
| 签名启用 | ✓ | `AppxPackageSigningEnabled=True` |
| 测试工件 | ✓ | `GenerateTestArtifacts=False` |
| 打包模式 | ✓ | `UapAppxPackageBuildMode=StoreOnly` |
| Bundle | ✓ | `AppxBundle=Always` |

**VS Associate 已完成**，VS 向导将能够自动获取 Partner Center 的证书。

---

## 🚀 执行步骤 - 生成可提交的包

### 步骤 1：清理项目 (5 秒)

在 Visual Studio 2022 中：

```
1. 在解决方案浏览器中右键 BLIT.Win 项目
2. 点击"清理"
3. 等待完成
```

**目的**：清除之前的构建文件，确保全新构建

---

### 步骤 2：打开"创建应用包"向导 (1 分钟)

```
1. 在解决方案浏览器中右键 BLIT.Win 项目
2. 选择"打包和发布" → "创建应用包..."
3. 向导窗口会打开
```

---

### 步骤 3：选择分发方法 (1 分钟)

在向导的第一个页面：

```
【分发方法】
选择：● Microsoft Store  ← 选这个

点击【下一步】
```

**预期**：向导会加载你的 Partner Center 应用信息

---

### 步骤 4：确认包身份 (1 分钟)

向导会显示：

```
【选择应用身份】
应用名称：Bannerlord.BLIT
包名称：ZoroWizcas.350BF036A26
发布者：CN=AD42E090-F4E2-47FD-BAF5-866425B8F937

✓ 验证这些信息与 Partner Center 中的一致
```

**这里是关键**：如果信息匹配 Partner Center，就是正确的。

点击【下一步】

---

### 步骤 5：选择要生成的包 (1 分钟)

```
【选择包配置】
架构：
  ☑ x64  ← 已勾选（因为 AppxBundlePlatforms=x64）

生成选项：
  ☑ 为 Microsoft Store 上传创建应用包
  这会生成 .msixupload 文件

点击【下一步】
```

**重要**：确保选中了"为 Microsoft Store 上传创建应用包"

---

### 步骤 6：配置生成设置 (1 分钟)

```
【输出位置】
输出目录：E:\gamework\mb2\BLIT\BLIT.Win\dist

【配置】
选择：Release  ← 必须是 Release

【平台】
x64  ← 与之前选择对应

点击【下一步】
```

**重要**：必须选择 Release 配置，Debug 不会生成有效的 Store 包

---

### 步骤 7：签名配置 (1-2 分钟)

向导会来到签名步骤：

```
【签名】
VS 会自动使用与你的 Partner Center 账户关联的证书
（这是因为你已完成了"关联应用与 Microsoft Store"）

注意：可能会要求输入 Microsoft 账户密码以验证证书
```

**如果 VS 要求选择证书**：
- 确保选择的是与 Partner Center 应用关联的证书
- 通常会显示与你的 Microsoft 账户关联的证书

点击【创建】

---

### 步骤 8：等待打包完成 (2-5 分钟)

```
VS 会进行以下操作：
1. 编译应用（Release 模式）
2. 创建 MSIX 包
3. 用 Partner Center 证书签名
4. 生成 .msixupload 包装文件
5. 生成符号包（.msixsym）

进度条会显示完成状态
```

**预期输出** (在 `E:\gamework\mb2\BLIT\BLIT.Win\dist\` 中)：

```
BLIT.Win_0.1.22.0_x64_bundle.msixupload
    ↓
这个文件直接上传到 Partner Center！

其他文件（辅助）：
BLIT.Win_0.1.22.0_x64.msix           (应用包本身)
BLIT.Win_0.1.22.0_x64.msixsym        (符号信息)
BLIT.Win_0.1.22.0_x64.cer            (证书)
```

---

## 📤 步骤 9：上传到 Microsoft Partner Center

打包完成后：

### 9.1 登录 Partner Center

```
访问：https://partner.microsoft.com/dashboard
使用你的 Microsoft 账户登录
进入 Bannerlord.BLIT 应用
```

### 9.2 创建新的提交或继续现有提交

```
点击"创建新的提交"或继续未完成的提交
```

### 9.3 上传包

```
在"包"部分：
1. 点击"上传"或类似按钮
2. 选择文件：BLIT.Win_0.1.22.0_x64_bundle.msixupload
3. 等待 Partner Center 扫描和验证包

Partner Center 会检查：
  ✓ 包签名有效性
  ✓ 包身份与应用匹配
  ✓ 版本号递增正确
  ✓ 内容有效性
```

### 9.4 继续提交流程

```
填写必要的应用信息：
- 描述
- 屏幕截图
- 内容评级等

完成后点"提交"
```

**预期结果**：
- ✓ 应用进入验证队列
- ✓ Microsoft 将自动检查应用
- ✓ 检查通过后，应用发布到 Store

---

## ⚠️ 常见问题和解决方案

### Q1: 打包时提示"找不到证书"
**原因**：VS 向导无法访问 Partner Center 证书

**解决**：
1. 确保使用的 Microsoft 账户与 Partner Center 账户相同
2. 检查网络连接
3. 尝试在 VS 中重新登录 Microsoft 账户
   - 文件 → 账户设置 → 登出 → 重新登录

### Q2: 生成的包太大了（几百 MB）
**这是正常的** - WinUI 3 应用包含完整的运行时
- x64 应用包通常在 50-150 MB 之间
- 符号包可能很大（用于调试）

### Q3: Partner Center 拒绝了上传的包，说"包身份不匹配"
**解决步骤**：
1. 检查 Partner Center 中显示的包身份（FQBN）
2. 对比 Package.appxmanifest 中的信息
3. 如果不匹配，可能是证书签名失败
4. 重新执行打包步骤，确保选择了正确的签名证书

### Q4: 为什么 dist 文件夹中仍有 `_Test` 文件夹？
**这是正常的** - `_Test` 文件夹包含本地测试用的包，不影响 Store 提交
- 重要的是 `.msixupload` 文件，不是 `_Test` 文件夹
- `_Test` 用于本地开发/测试，可以忽略

### Q5: Partner Center 提交后多久能发布？
**处理时间**：
- 自动验证：1-2 小时
- 内容审查：1-3 天
- 认证完成后自动发布

可以在 Partner Center 的"提交"页面实时查看状态

---

## ✅ 验证清单

在上传到 Partner Center 前，检查以下项目：

- [ ] 在 Visual Studio 中完成了"关联应用与 Microsoft Store"
- [ ] Package.appxmanifest 中的 Publisher 是 `CN=AD42E090-F4E2-47FD-BAF5-866425B8F937`
- [ ] 使用 Release 配置进行打包
- [ ] 在向导中选择了"为 Microsoft Store 上传创建应用包"
- [ ] 成功生成了 `.msixupload` 文件
- [ ] Partner Center 中应用身份与本地配置匹配
- [ ] 版本号递增（当前：0.1.22.0）

---

## 📝 总结

1. **VS 已经完全配置好了** ✓
   - 包身份正确
   - 签名已启用
   - Build 模式设置为 StoreOnly

2. **只需执行打包向导** ← 你现在的位置
   - 向导会自动处理证书
   - 自动生成 .msixupload 文件

3. **然后上传到 Partner Center**
   - 选择 .msixupload 文件
   - 填写应用信息
   - 点击提交

**预计总耗时**：5-10 分钟

---

## 🎯 下一步

现在就打开 Visual Studio，按照上面的步骤执行"创建应用包"向导！

有问题？确认：
1. 使用了正确的 Microsoft 账户（与 Partner Center 相同）
2. 选择了 Release 配置
3. 向导中选择了"为 Microsoft Store 上传"选项

Good luck! 祝提交顺利！ 🚀
