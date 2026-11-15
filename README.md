# KAMI Unity 迁移版

基于用户提供的 KAMI 桌面版文件，将关卡与资源迁移到 Unity，并用 C# 重新实现核心玩法。这不是原 ActionScript 源码转换，也尚未达到原版动画和界面的完整复刻。

## 启动

1. 用 Unity Hub 安装 **Unity 2022.3.62f1 LTS**，将本目录作为项目打开。
2. 等待资源导入与脚本编译，选择菜单 **KAMI → Open Game**，点击 Play。
3. 选择底部颜色，再点击棋盘中的区域；四方向相连的同色纸片会一起换色，将全盘变为同一种颜色即完成。

包含 72 个正式关卡、选关、步数与奖牌、撤销、重置、下一关、PlayerPrefs 本地最佳成绩、原纸张静态纹理、背景音乐与一次换色音效。全部关卡可直接选择，不复刻原版付费解锁。

## 验证

无需 Unity 的数据检查：

```sh
python3 Tools/validate_assets.py
```

Unity 内 C# 规则回放（需要已授权的编辑器）：

```sh
Unity -batchmode -nographics -projectPath "$PWD" \
  -executeMethod KamiUnity.Editor.KamiProject.SmokeTest -quit -logFile /tmp/kami-unity.log
```

成功日志应包含 `KAMI_SMOKE_PASSED levels=72`。打包时确认 `Assets/Scenes/Kami.unity` 在 Build Settings 中（已预配置），选择本机已安装的目标平台模块。

## 当前验证范围

云端已通过 72 个正式关卡的数据、贴图引用与原解法回放。云端未安装 Unity 编辑器，因此 Unity 编译、实际游玩、声音播放和目标平台打包仍待本地编辑器验证。Python 回放验证数据和规则推断，不代表 C# 已编译通过。

详细分析和资源来源见 [迁移说明](Documentation/MIGRATION.md)。
