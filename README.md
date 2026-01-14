# XdbLoader
生存战争Xdb自动装载工具（支持多Mod共存）

## 集成到自己的Mod里面
将XdbLoaderUtil复制粘贴到自己的项目里面或者生成dll到自己的Mod里面，按照XdbLoader示例ModLoader那样配置即可。
注意：千万不要保留自动置尾的部分，如果碰到XdbLoader的Mod会出现无限循环。
不要用下面的代码：
if (ModsManager.ModList.Last() != Entity)
{
  ModsManager.ModList.Add(Entity);
  return;
}
