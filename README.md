# cropRemoteSensingImage
C#版的遥感影像剪裁算法，采用gdal库制作，根据shp文件剪裁
配置好c#版的库后，调用SampleMake函数即可，SampleMake(tif影像路径, shp文件路径, 512,512);  最后两个参数是图像的长宽