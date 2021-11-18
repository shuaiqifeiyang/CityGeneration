```C#
// 一些尺寸的说明
width = peopleDensity.width; // 人口密度图的宽度
height = peopleDensity.height; // 人口密度图的长度
swidth = peopleDensity.width*mapScale; // 实际Unity里面整个Plane的宽度
sheight = peopleDensity.height*mapScale; // 实际Unity里面整个Plane的长度
minHighway = swidth / 30; // 所有的HighWay都必须大于swidth/30, 删去零碎的路径
minStreet = swidth / 100; // 所有的street都必须大于swidth/100, 删去零碎的路径

// 根据highway生成的street的长度在以下两个值之间随机取值
spawnStreetLower = swidth / 70;
spawnStreetHigher = swidth / 50;
```

