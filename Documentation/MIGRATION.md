# 原游戏分析与迁移记录

## 输入与技术栈

本次实际读取 `SavesDir.zip` 和 `textures_desktop.zip`。最初的 `KAMI.zip` 超过附件下载限制，未读取。档案包括 Adobe AIR 4.0 描述文件、Windows 可执行文件、运行时 DLL、`Kami_Steam.swf`、XML 关卡与布局、TexturePacker 图集和 MP3 音频；未发现原 ActionScript 项目源码。

未将 AIR 运行时、EXE、DLL、SWF、Steam 用户保存信息或签名文件加入 Unity 项目。Unity 脚本为本次新实现，并非从编译文件提取的原作者源码。

## 玩法和数据

关卡采用行优先 16×10 网格，每个字符表示一个颜色 ID。一次操作选择目标颜色与任一格子，将其四方向连通同色区域换色；选择原色不消耗步数。全盘同色结束，按 XML gold/silver/bronze 阈值判定奖牌，可撤销或重置。

正式目录为 A、B、C、D、E、5C1、Pat1、Conv，每组 9 关，共 72 关。`!NewLevel.xml` 为非正式数据，保留原文件但不加入选关。原始 XML 中的 `numColours` 与部分实际颜色数不一致，因此色板从实际字符集合生成。Solution 的 length 也存在与 Turn 数目不一致的情况，验证使用实际 Turn 节点，不修改原数据。

四方向规则通过全部 73 份 XML 解法回放；目录内 72 关另验证了主题图集的静态纸片引用。换色只实现即时区域着色，尚未还原原版折纸逐帧过渡、提示购买、Steam 成就、原 UI 布局或云存档。解法回放足以支持核心规则推断，但不是所有原游戏行为的证明。

## 文件映射

- `Assets/Resources/puzzles`：原关卡、章节与分组 XML。
- `Assets/Resources/levels.json`：由章节顺序生成的 Unity 目录，不依赖 Unity 不稳定的资源枚举顺序。
- `Assets/Resources/themeTextures_png`：原主题图集，按静态子纹理坐标运行时切片，转换 TexturePacker 顶部原点到 Unity 底部原点。
- `Assets/Resources/themes`：原主题定义。
- `Assets/Resources/sounds` 与 `Music.mp3`：用户提供的原音频；当前仅使用其中一个换色音效及背景音乐。
- `Assets/Scripts`：独立规则模型与 IMGUI 游戏界面。
- `Assets/Editor`：主题导入设置、打开场景和 C# 解法回放入口。

资源保留其原有权利归属，仓库未为这些商业游戏素材声明新的开源许可证。对外发布或分发前需确认所持素材的使用授权。

## 验证限制

当前机器没有 Unity 编辑器及其授权，未执行 C# 编译、Unity SmokeTest 或打包。该项目采用 Unity 2022.3 LTS；第一次打开时 Unity 会生成其余默认项目设置。已提供固定 GUID 的场景与 meta 文件以及构建场景配置。
