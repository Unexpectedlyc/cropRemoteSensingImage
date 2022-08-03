# cropRemoteSensingImage
C#版的遥感影像剪裁算法，用于制作深度学习样本，跟shp文件的标注来制作标签，采用gdal库制作，根据shp文件剪裁
配置好c#版的库后，调用SampleMake函数即可，SampleMake(tif影像路径, shp文件路径, 512,512);  最后两个参数是图像的长宽
python版的在另一个项目中
