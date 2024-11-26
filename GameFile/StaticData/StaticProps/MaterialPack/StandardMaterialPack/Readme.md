# Standard Material Pack

## 介绍

Standard Material Pack 是原版游戏中包含的基准材料包。此包储存了一些基础的材料信息，用于定义游戏中的基本材质。

## 材料包介绍

材料包必须包含以下三个文件：

- **原始材质（Original Texture）**：原始材质是材质的基础。在没有任何附加条件下，物体将以这种材质存在。其他一切均为变体继承自 `GameMaterial` 类，或者你可以自己编写的抽象类。
- **次级材质（Secondary Texture）**：次级材质是基础材质的进阶版。次级材质继承自原始材质，并且拥有自己的属性，允许重写原始材质的参数。次级材质通过继承和重写来实现自己的定义，继承时不继续抽象。

### 示例

以下是如何定义和使用这些材质的示例代码：

```csharp
// 定义 GameMaterial 抽象类
public abstract class GameMaterial
{
    // 基础属性和方法
}

// 继承自 GameMaterial 的原始材质类
public class OriginalTexture : GameMaterial
{
    // 原始材质的具体实现
}

// 继承自 OriginalTexture 的次级材质类
public class SecondaryTexture : OriginalTexture
{
    // 重写原始材质的属性和方法
}
