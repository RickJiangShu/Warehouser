关于名称：

PathPair：路径映射对
PathParis：PathPairs[]


Warehouser		库管
Warehouse		仓库
Instance		指实例
Asset			指资源（既包括UnityEngine.Resources 和 AssetBundles）


path 既包括Resources的完整路径，又指AssetBundle的包名。

提供接口：
GetInstance		任意Asset的实例
Recycle			回收实例

GetAsset									获取Asset
UnloadAsset(Object asset)					销毁Asset
UnloadAssetBundle(string bundleName, bool)	销毁AssetBundle



关于Load
无论是Resources.Load，还是AssetBundle.Load
它们都是将数据加载到内存中，无论你是否建立缓存引用它依旧存在。
换句话说，建立字典缓存Asset是没有必要的，因为Unity已经做了。


所以，


实验1：
Resources.Load 内部有缓存机制，重复调用Load不会持续增加内存。
在没有引用的情况下，调用Unload可以卸载掉Asset。

实验2：
当重复调用 AssetBundle.Load 会提示“The AssetBundle 'XXX' can't be loaded because another AssetBundle with the same files is already loaded.”
由此可见，AssetBundle也有缓存机制。

实验3：
在Profiler里，AssetBundle在Not Saved节点下，占用内存很小。
由此可见，Bundle相当于一个压缩包。

实验4：
a.ab 有一张贴图 aaa
b.ab 有一个preab bbb
bbb 引用了 aaa

结果1：单独加载b.ab并加载bbb，不会报错，但贴图引用丢失。
结果2：先加载a.ab，再加载b.ab，贴图引用正常。
结果3：销毁掉bbb，但aaa依旧在内存中。
结果4：从a.ab中再次LoadAsset出aaa，并Resources.Unload它便从内存中消失了。

实验5：
在有引用AssetBundle中Asset的情况下，调用AssetBundle.Unload(true)，依然可以把Asset销毁掉。

实验6：
我通过AssetBundle加载了一个Asset，我没有引用Asset，并调用Unload(false)把Bundle销毁了。
可以通过Resources.UnloadUnusedAssets() 将其销毁。

综上所述，Asset是没有必要缓存的，原因有三：
1、Unity自身就已经做了缓存机制；
2、通过AssetBundle依赖关系加载的Asset，你没办法引用；
3、由2产生的引用混乱会导致无法卸载Asset。


