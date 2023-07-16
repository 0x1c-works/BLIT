---
layout: single
title: Get Started
permalink: /get-started/
---

## Requirements

- Windows 10 version 1809 (10.0.17763.0) or later
- Windows App Runtime 1.2

## Install

1. Download the latest release from [here](https://github.com/0x1c-works/BLIT/releases).
2. Extract the zip file.
3. Run the `Install.ps1` in PowerShell with the following commands:

   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ./Install.ps1
   Set-ExecutionPolicy -ExecutionPolicy AllSigned -Scope CurrentUser
   ```

   This will add the certificate to your system and install the app.
4. **Alternatively**, you can install it manually without PowerShell:
   1. Right click on the `.cer` file and "Install Certificate".
   2. Run the `BLIT.Win_<VERSION>_x64.msix` file.

## Next

- [Generate custom banner assets](/banner-icons-editor/)