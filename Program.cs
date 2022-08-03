using System;
using OSGeo.GDAL;
using OSGeo.OGR;
using System.IO;
using System.Collections.Generic;



namespace crop
{
    class Program
    {
        static void Main(string[] args)
        {
            

            SampleMake("C:\\Users\\yc\\Desktop\\test\\test1.tif", "C:\\Users\\yc\\Desktop\\test\\test1.shp", 512,512);
        }

        private static  void SampleMake( string img,string  shp,int blocksizeX,int blocksizeY)
        {
           
            

            string outputRasterFile = "buffer.tif";
            

            Gdal.AllRegister();
            Dataset data = Gdal.Open(img, Access.GA_ReadOnly);     // 读取数据到gdal的dataset中

            DataType srcType = data.GetRasterBand(1).DataType;
            double[] geo_transform = new double[6];
            data.GetGeoTransform(geo_transform);
            int bandcount = data.RasterCount;
            string proj = data.GetProjection();
            double x_min = geo_transform[0];
            double y_max = geo_transform[3];
            double pixel_width = geo_transform[1];
            double x_max = x_min + geo_transform[1] * data.RasterXSize;
            double y_min = y_max + geo_transform[5] * data.RasterYSize;
            int x_res = data.RasterXSize;
            int y_res = data.RasterYSize;
            int blockX = x_res / blocksizeX + 1;
            int blockY = y_res / blocksizeY + 1;


            //根据cc码的值建立字典
            SortedDictionary<string, int> sd = new SortedDictionary<string, int>();
            sd.Add("10", 2);
            sd.Add("20", 3);
            sd.Add("30", 4);

            DataSource mb_v = Ogr.Open(shp, 1);
            Layer mb_l = mb_v.GetLayerByIndex(0);

            if (mb_l.FindFieldIndex("pixel", 0) == -1)
            {
                FieldDefn pixel = new FieldDefn("pixel", FieldType.OFTInteger);
                mb_l.CreateField(pixel, 1);
                Feature pFeature = mb_l.GetNextFeature();

                while (pFeature != null)
                {
                    pFeature.SetField("pixel", sd[pFeature.GetFieldAsString("CC")]);
                    mb_l.SetFeature(pFeature);
                    pFeature = mb_l.GetNextFeature();
                }
            }



            Dataset target_ds = Gdal.GetDriverByName("GTiff").Create(outputRasterFile, x_res, y_res, 1, DataType.GDT_Byte, null);
            target_ds.SetGeoTransform(geo_transform);
            target_ds.SetProjection(proj);
            Band band = target_ds.GetRasterBand(1);
            int NoData_value = 0;
            int[] band_list = new int[1] { 1 };
            band.SetNoDataValue(NoData_value);
            band.FlushCache();
            string[] options = new string[2];
            options[1] = "ATTRIBUTE=pixel";
            options[0] = "ALL_TOUCHED=TRUE";
            Gdal.RasterizeLayer(target_ds, 1, band_list, mb_l, IntPtr.Zero, IntPtr.Zero, 1, null, options, null, null);

            int[] imgArray = new int[blocksizeX * blocksizeY * bandcount];
            int[] bandArray = new int[bandcount];

            int[] labelArray = new int[blocksizeX * blocksizeY];
            int[] labelbandArray = new int[1] { 1 };


            for (int i = 0; i < bandcount; i++)
            {
                bandArray[i] = i + 1;
            }



            string filePath = img.Substring(0,img.LastIndexOf("\\")+1);
            string fileName = img.Substring(img.LastIndexOf("\\") + 1,img.LastIndexOf(".")-img.LastIndexOf("\\") - 1);
            string fileExc = img.Substring(img.LastIndexOf(".") + 1,img.Length- img.LastIndexOf(".")-1);

            if (!Directory.Exists(filePath + "\\image"))
            {
                Directory.CreateDirectory(filePath + "\\image");
                
            }
            if (!Directory.Exists(filePath + "\\label"))
            {
                Directory.CreateDirectory(filePath + "\\label");

            }

            for (int j = 0; j < blockY; j++)
            {
                for (int i = 0; i < blockX; i++)
                {
                    string blockimgname = filePath+"image\\" + i.ToString() + "_" + j.ToString() + "." + fileExc;
                    Dataset block = Gdal.GetDriverByName("GTiff").Create(blockimgname, blocksizeX, blocksizeY, bandcount, srcType, null);
                    string blocklabelname = filePath + "label\\" + i.ToString() + "_" + j.ToString() + "." + fileExc;
                    Dataset blocklabel= Gdal.GetDriverByName("GTiff").Create(blocklabelname, blocksizeX, blocksizeY, 1, DataType.GDT_Byte, null);


                    if ((j == blockY - 1) && (i != blockX - 1))
                    {
                        data.ReadRaster(i * blocksizeX, j * blocksizeY, blocksizeX, y_res - j * blocksizeY, imgArray, blocksizeX, blocksizeY, bandcount, bandArray, 0, 0, 0);
                        target_ds.ReadRaster(i * blocksizeX, j * blocksizeY, blocksizeX, y_res - j * blocksizeY, labelArray, blocksizeX, blocksizeY, 1, labelbandArray, 0, 0, 0);
                    }
                    else if ((j == blockY - 1) && (i == blockX - 1))
                    {
                        data.ReadRaster(i * blocksizeX, j * blocksizeY, x_res - i * blocksizeX, y_res - j * blocksizeY, imgArray, blocksizeX, blocksizeY, bandcount, bandArray, 0, 0, 0);
                        target_ds.ReadRaster(i * blocksizeX, j * blocksizeY, x_res - i * blocksizeX, y_res - j * blocksizeY, labelArray, blocksizeX, blocksizeY, 1, labelbandArray, 0, 0, 0);
                    }
                    else if ((i == blockX - 1) && (j != blockY - 1))
                    {
                        data.ReadRaster(i * blocksizeX, j * blocksizeY, x_res - i * blocksizeX, blocksizeY, imgArray, blocksizeX, blocksizeY, bandcount, bandArray, 0, 0, 0);
                        target_ds.ReadRaster(i * blocksizeX, j * blocksizeY, x_res - i * blocksizeX, blocksizeY, labelArray, blocksizeX, blocksizeY, 1, labelbandArray, 0, 0, 0);
                    }
                    else
                    {
                        data.ReadRaster(i * blocksizeX, j * blocksizeY, blocksizeX, blocksizeY, imgArray, blocksizeX, blocksizeY, bandcount, bandArray, 0, 0, 0);
                        target_ds.ReadRaster(i * blocksizeX, j * blocksizeY, blocksizeX, blocksizeY, labelArray, blocksizeX, blocksizeY, 1, labelbandArray, 0, 0, 0);
                    }
                    block.WriteRaster(0, 0, blocksizeX, blocksizeY, imgArray, blocksizeX, blocksizeY, bandcount, bandArray, 0, 0, 0);
                    blocklabel.WriteRaster(0, 0, blocksizeX, blocksizeY, labelArray, blocksizeX, blocksizeY, 1, labelbandArray, 0, 0, 0);
                    block.Dispose();
                    blocklabel.Dispose();
                }
            }
            target_ds.Dispose();
            File.Delete(outputRasterFile);

        }

        
    }
}
