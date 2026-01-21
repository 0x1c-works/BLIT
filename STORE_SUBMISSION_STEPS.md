# Microsoft Store 应用提交完整指南

## 当前状态
- 应用已在 Partner Center 注册：`ZoroWizcas.350BF036A26`
- 应用身份已配置正确
- **问题**：使用了错误的签名证书导致 Store 验证失败

## 根本原因分析
你上传的 `.msix` 文件用以下证书签名：
```
CN=Wizcas (来自临时密钥 BLIT.Win_TemporaryKey.pfx)
```

但 Microsoft Store 期望：
```
CN=AD42E090-F4E2-47FD-BAF5-866425B8F937 (来自 Partner Center)
包族名称（FQBN）：ZoroWizcas.350BF036A26_qja9fms15r7g8
```

## 解决步骤（推荐方法）

### 步骤 1：在 Visual Studio 中重新关联应用
```
1. 在 Visual Studio 2022 中打开 BLIT.Win 项目
2. 右键项目 → 选择 "关联应用与 Microsoft Store..."
3. 使用你的 Microsoft 账户登录
4. 选择你的应用：ZoroWizcas.350BF036A26
5. 点击 "关联"
6. VS 会自动下载并配置正确的签名证书
```

**这一步非常关键**，VS 会：
- 自动下载 Partner Center 提供的签名证书
- 将其存储在项目中或临时位置
- 更新 Package.appxmanifest 中的发布者信息
- 配置 .csproj 使用正确的证书

### 步骤 2：验证证书配置
完成关联后，检查以下内容：

**在 Package.appxmanifest 中验证：**
```xml
<Identity
  Name="ZoroWizcas.350BF036A26"
  Publisher="CN=AD42E090-F4E2-47FD-BAF5-866425B8F937"
  Version="0.1.21.0" />
```

**在 BLIT.Win.csproj 中验证：**
- `<AppxPackageSigningEnabled>True</AppxPackageSigningEnabled>`
- `<PackageCertificateKeyFile>` 指向正确的证书文件

### 步骤 3：清理并重新生成包

```
1. 右键项目 → 清理
2. 右键项目 → 打包和发布 → 创建应用包
3. 选择 "Microsoft Store"
4. 下一步直到完成
5. 在 dist 文件夹中查找：
   - BLIT.Win_0.1.21.0_x64.msix （用于 Store 验证）
   - BLIT.Win_0.1.21.0_x64_bundle.msixupload （用于 Store 上传）
```

### 步骤 4：上传到 Partner Center

```
1. 登录 https://partner.microsoft.com/dashboard
2. 进入你的应用
3. 创建新的提交或继续现有提交
4. 在 "包" 部分上传 .msixupload 文件
5. 系统应该接受它（验证通过）
```

## 替代方法：手动配置证书

如果 VS 的关联不起作用，可以手动操作：

### 从 Partner Center 获取证书

1. 登录 https://partner.microsoft.com/dashboard
2. 进入应用 → "应用身份"
3. 在 "包身份" 部分查找证书下载链接
4. 下载 `.pfx` 文件
5. 将其放到项目目录，重命名为 `StoreCertificate.pfx`

### 更新项目配置

项目已配置为自动检测 `StoreCertificate.pfx` 文件。只需将证书放在项目目录，VS 会自动使用它。

## 验证清单

在提交到 Store 之前，确认：

- [ ] `Package.appxmanifest` 中的 Publisher 是正确的 GUID
- [ ] 使用了 Partner Center 的证书而非临时密钥
- [ ] `AppxPackageSigningEnabled` 是 `True`
- [ ] 生成的 `.msix` 文件大小合理（几十到几百 MB）
- [ ] 有 `.msixupload` 文件可供上传
- [ ] Version 号正确（目前是 0.1.21.0）

## 常见问题

### Q: 为什么用临时密钥会失败？
A: Microsoft Store 要求应用必须用官方的、与你的 Partner Center 账户相关联的证书签名。临时密钥是为本地开发/测试用的。

### Q: 如何重新生成 Partner Center 证书？
A: 通常 Partner Center 会自动生成并保存证书。如果丢失，可以在 Partner Center 中请求重新生成。

### Q: _Test 文件夹是什么？
A: 这是本地测试包，包含 `.msix` 文件和安装脚本。对 Store 提交无害，忽略即可。重要的是 `.msixupload` 文件。

### Q: 为什么需要 .msixupload 而不是 .msix？
A: `.msixupload` 是一个包装格式，包含验证 Store 提交所需的所有信息（包含包本身、符号、证书哈希等）。

## 下一步

1. 按上述步骤 1-4 操作
2. 上传到 Partner Center
3. 等待 Microsoft 的自动验证和审核
4. 应用发布！

有任何问题，参考这个文档或查看 Partner Center 的帮助文档。
