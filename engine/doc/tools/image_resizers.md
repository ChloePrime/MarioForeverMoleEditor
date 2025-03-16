引擎内置了两个用于xbrz缩放贴图的工具场景，分别用于处理单张图片和TileSet。
## 单张图片版
#### 使用方法：
1. 使用godot引擎打开本项目
2. 在左下角的文件窗口里找到并双击打开 `res://engine/tools/sprite_image_resizer.tscn`
3. 按F6运行当前场景
4. 从Windows文件管理器里把你想要处理的图片拖到正在运行的godot窗口里，等待**叮咚**的声音发出后就处理完成了。
5. 放大后的图片会存放在你拖入窗口的原图的目录，以`${拖入的图片文件名}_2x.png`的文件名存放。

## TileSet版
#### 使用方法：
1. 使用godot引擎打开本项目并打开`res://engine/tools/tile_image_resizer.tscn`
2. 将你的16x原始TileSet处理成`res://engine/tools/tile_image_resizer_reference_source_tileset.png`的格式
3. 按F6运行当前场景
4. 将按照格式处理后的16x原始TileSet拖入窗口，等待**叮咚**的声音发出后就处理完成了。
5. 放大后的TileSet会存放在你拖入窗口的原图的目录，以`${拖入的图片文件名}_2x.png`的文件名存放。
