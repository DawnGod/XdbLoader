# XdbLoader
生存战争Xdb自动装载工具（支持多Mod共存）

## 集成到自定义Mod中
### 集成步骤
1. 将`XdbLoaderUtil`复制粘贴到你的项目中，或生成dll文件并引入到你的Mod项目里；
2. 参照`XdbLoader`示例`ModLoader`的配置方式完成集成配置。

### 重要警告
**不要保留自动置尾相关代码段**，否则当加载包含XdbLoader的Mod时，会触发无限循环问题。

#### 不要使用的代码部分：
```csharp
if (ModsManager.ModList.Last() != Entity)
{
  ModsManager.ModList.Add(Entity);
  return;
}
