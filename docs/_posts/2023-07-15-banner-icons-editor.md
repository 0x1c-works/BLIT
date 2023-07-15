---
layout: single
title: Banner Icons Editor
permalink: /banner-icons-editor
toc: true
toc_sticky: true
---

## Overview

Adding customer banners to Bannerlord can be quite cumbersome. You'll have to create the 4x4 textures, organize the sprites, edit XML files, and so on. It's a lot of work, and it's easy to make mistakes.

BLIT can manage the config and assets for you, including:

- Banner groups
- Banner icon textures
- Banner icon sprites
- Custom colors

Your work will be saved as **Bannerlord Icon Project**s, or `bip` files, which can be exported with just one-click and then imported in your mod.

---

## [0] First-Run Setup

To better managing your banner assets, you will need to tweak the banner settings on your first run for features like sprite searching as well as start IDs for groups and colors. BLIT will remember your settings.

First, in the navigation pane, click **Settings**.

### Relative Scan Paths

BLIT scans for corresponding sprites for banner textures via *relative* scan paths, which are relative to the location of the texture files.

For instance, if you have banner assets stored in a directory structure like this:

     MyWork
     ├── Textures
     │   └── AwesomeBanner.png
     └── Sprites
         └── AwesomeBanner.png

Then you should add `..\Sprites` as the search path. When importing the texture `AwesomeBanner.png`, BLIT will search for the sprite at `MyWork\Sprites\AwesomeBanner.png`.

**Note that** BLIT will search for the sprite with **the *same* file name of the texture**.

![Relative Scan Paths](/assets/images/banner-editor-tutorial/relative-path-setting.png)

> BLIT DO NOT support scanning for sprites in the directory of textures (i.e. `.\`), because you can't have two files with the same name in the same directory.
{: .notice--warning}

### Start IDs

Each banner icon group must have a unique ID, so do the banner colors. To avoid conflicts with the native game and other mods, you should set a start ID for your mod.

![Start IDs](/assets/images/banner-editor-tutorial/banner-start-ids.png)

For example, if you set the group start ID as `10000`, whenever you create the first group in a new project, its ID will be `10000`.

Please keep an eye on the hints of each setting about the restrictions.

---

## [1] Import Banner Icons

Go to **Banner Icons** via the navigation pane. When opened, it creates an empty project for you, containing no groups or colors.

To import banner icons, you need to create a banner icon group first:

![Add group](/assets/images/banner-editor-tutorial/banner-1-add-group.png)

You'll notice that with the new group selected, the group editor shows up. Click **Add textures** and select your banner textures. You can, of course, import mulitple textures in a batch.

![Add group](/assets/images/banner-editor-tutorial/banner-2-add-textures.png)

The imported icon textures will show in the list, each of which is assigned with an **Icon ID**.

> The Icon IDs are comprised of the group ID and the texture index in the group.

You can also find what the texture's file name would be for each icon at the top of a texture tile. Since every banner texture can hold only 16 icons, there could be more than one texture files (suffixed with `_01`, `_02`, etc.) for an icon group.

![Add group](/assets/images/banner-editor-tutorial/banner-3-textures.png)

When an icon is selected, its detail will show up in the right pane, where you can inspect the texture and the sprite in a larger view. If you have set the [relative scan paths](#relative-scan-paths) correctly and have named the sprite files same as the texture files, BLIT will automatically set the sprite for the icons.

You can always pick sprites and textures manually for each icon by clicking **Change...**. If an image has changed, you'll need to click **Reimport** to refresh it.

> Alternatively, you can add the existing textures again by clicking **Add textures**, if there are many images to be refreshed.

![Icon Details](/assets/images/banner-editor-tutorial/banner-4-icon-details.png)

---

## [2] Manage Custom Color

{% capture color-warning %}

**DO NOT** add any custom color if you:

- Haven't patched the native `BannerManager.GetColorId` method, or;
- Don't use a banner enhancement mod that supports custom colors (such as [Banner Editor](https://www.nexusmods.com/mountandblade2bannerlord/mods/4944))

Otherwise, it will **crash your game and corrupt the saved games**!!!

> I have reported this bug to TaleWorlds but the fix is still in progress.
You can help bump it in this [thread](https://forums.taleworlds.com/index.php?threads/custom-colors-can-be-selected-from-disabled-mods-and-will-corrupt-game-saves.457487/post-9865176).
>
> For more technical details, or if you want to patch it in your own mod, please refer to [How To Support Custom Colors](/how-to-support-custom-colors).

{% endcapture %}

<div class="notice--danger">
  <h4 class="text-center">☣️ DANGER! ☣️</h4><br />
  {{ color-warning | markdownify }}
</div>

To manage custom colors, click **BANNER COLORS** under the banner editor's toolbar:

![Banner colors](/assets/images/banner-editor-tutorial/banner-5-goto-colors.png)

Editing colors should be pretty straightforward: clicking **Add** to add a new color; **Delete** to remove the selected color.

For each color, you can toggle the **Sigil** and **Background** switches to determine whether this color can be used in the sigil or background palettes in game.

You can also change the color ID manually, as long as it's unique. BLIT will prevent color IDs from being duplicated in a single project.

![Color editor](/assets/images/banner-editor-tutorial/banner-6-color-editor.png)

You can delete multiple colors by muti-selection (via <kbd>Shift</kbd> or <kbd>Ctrl</kbd>). When more than one color are selected, however, you won't be able to edit the ID nor the color.

![Color Multi Selection](/assets/images/banner-editor-tutorial/banner-9-multi-colors.png)

---

## [3] Export Banners

### Complete Export

Click the **Export All** button in the toolbar and select a destination folder to export:

![Export](/assets/images/banner-editor-tutorial/banner-7-export-toolbar.png)

A complete export will generate the following outputs:

    (Target Folder)
    ├─banner_icons.xml
    ├─AssetSources
    │  └─BannerIcons
    └─GUI
        └─SpriteParts
            ├─Config.xml
            └─ui_<GROUP ID>

| File/Folder                | Description                                                                                                                                                                                                                                        |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| banner_icons.xml           | Put it in the **ModuleData** folder of your mod.                                                                                                                                                                                                   |
| AssetSources               | The merged banner textures (PSD files), which can be imported by Bannerlord Modding Kit directly.                                                                                                                                                  |
| GUI/SpriteParts/ui_*       | The sprite folders for sprite sheet generation.                                                                                                                                                                                                    |
| GUI/SpriteParts/Config.xml | The config file for setting the banner sprites as `<AlwaysLoad>` so that you don't have to manually load the sprite sheets in code. Use it wisely. <br/><br/> You will probably need to merge its content with your existing `Config.xml`, if any. |

### Export XML Only

If you have only changed the banner colors and won't want to wait for all the textures to be packed again (it could take a long time!), you can click **Export XML**. This will only export the `banner_icons.xml` file.

Note that when exporting XML, you also need to choose a *folder* instead of a file as the destination.

### Texture Resolution

By default, each merged texture will be in 2K resolution, i.e. 2048x2048 pixels. Since each texture is 4x4, the packed icon textures will be 512x512 pixels.

This is the recommended configuration, as banners are typically too small for the resolution to be noticable. Smaller textures will reduce the size of your mod as well as conserve RAM and vRAM.

However, if you do wish a larger texture, you can select the 4K resolution before exporting. This will enlarge the merged textures to 4096x4096.

![Texture resolution](/assets/images/banner-editor-tutorial/banner-8-texture-resolution.png)