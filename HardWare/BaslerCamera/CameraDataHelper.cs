using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.HardWare.BaslerCamera
{
    /// <summary>
    /// Camera数据处理器
    /// </summary>
    public class CameraDataHelper
    {
        /// <summary>
        /// 将Bitmap格式转换成BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
            {
                for (j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    Color newColor = Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
                    bitmapSource.SetPixel(i, j, newColor);
                }
            }

            MemoryStream ms = new MemoryStream();
            bitmapSource.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }

        /// <summary>
        /// 把内存里的BitmapImage数据保存到硬盘中
        /// </summary>
        /// <param name="bitmapImage">BitmapImage数据</param>
        /// <param name="filePath">输出的文件路径</param>
        public static void SaveBitmapImageIntoFile(BitmapImage bitmapImage, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public static void CompressImg(string path, Image img, int quality)
        {
            ThreadStart(() =>
            {
                int i = 0;
                SaveImage:
                try
                {
                    if (quality < 0 || quality > 100)
                        throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

                    // Encoder parameter for image quality 
                    EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

                    // JPEG image codec 
                    ImageCodecInfo jpegCodec = GetEncoderInfo(Properties.Settings.Default.SaveImageType);

                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;

                    img.Save(path, jpegCodec, encoderParams);
                }
                catch (Exception e)
                {
                    AddLog("CameraDataHelper.class CompressImg: " + e.Message, -1);
                    if (i < 100)
                    {
                        ThreadSleep(1000);
                        i++;
                        goto SaveImage;
                    }
                }
                img.Dispose();
            });
        }

        /// <summary>
        /// 把内存里的Bitmap数据保存到硬盘中
        /// </summary>
        /// <param name="bitmap">Bitmap数据</param>
        /// <param name="filePath">输出的文件路径</param>
        public static void SaveBitmapIntoFile(Image bitmap, string filepath, JoinMode joinMode, int proportion = 0)
        {
            if (proportion == 0)
            {
                proportion = Properties.Settings.Default.ImageProportion;
            }
            if (filepath.Contains("testdata"))
            {
                filepath.Replace("testdata", "test_data");
            }
            //缩放图片
            Bitmap objNewPic;
            try
            {
                //重新缩放生成生成新的Bitmap
                objNewPic = new Bitmap(bitmap,
                    joinMode == JoinMode.Horizontal ? (proportion * bitmap.Width) / bitmap.Height : proportion,
                    joinMode == JoinMode.Vertical ? (proportion * bitmap.Height) / bitmap.Width : proportion);

                //保存图片
                CompressImg(filepath, objNewPic, 100);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SaveBitmapIntoFile1: " + e.Message, -1);
            }
            finally
            {
                objNewPic = null;
            }
        }

        public static void SaveBitmapIntoFile(Image bitmap, string filepath)
        {
            if (filepath.Contains("testdata"))
            {
                filepath.Replace("testdata", "test_data");
            }
            //缩放图片
            Bitmap objNewPic;
            try
            {
                //重新缩放生成生成新的Bitmap
                objNewPic = new Bitmap(bitmap, bitmap.Width, bitmap.Height);

                //保存图片
                CompressImg(filepath, objNewPic, 100);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SaveBitmapIntoFile2: " + e.Message, -1);
            }
            finally
            {
                objNewPic = null;
            }
        }

        public static void SaveBitmapIntoFileFromSpeed(Image bitmap, string filepath, JoinMode joinMode, int proportion, int start, int end, List<int> speedList)
        {
            if (proportion == 0)
            {
                proportion = Properties.Settings.Default.ImageProportion;
            }
            if (filepath.Contains("taskdata"))
            {
                filepath = filepath.Replace("taskdata", "task_data");
            }
            string orcPath = filepath.Replace("task_data", "orc");
            if (!Directory.Exists(orcPath.Substring(0, orcPath.LastIndexOf('\\'))))
            {
                Directory.CreateDirectory(orcPath.Substring(0, orcPath.LastIndexOf('\\')));
            }
            //缩放图片
            Bitmap objNewPic;
            try
            {
                //重新缩放生成生成新的Bitmap
                objNewPic = new Bitmap(bitmap,
                    joinMode == JoinMode.Horizontal ? (proportion * bitmap.Width) / bitmap.Height : proportion,
                    joinMode == JoinMode.Vertical ? (proportion * bitmap.Height) / bitmap.Width : proportion);
                int speed = 800;
                if (speedList.Count > start && speedList.Count >= end && start < end)
                {
                    int count = 0;
                    double _speed = 0d;
                    for (int i = start; i <= end; i++)
                    {
                        _speed += speedList[i];
                        count++;
                    }
                    speed = (int)(_speed / count);
                }
                //保存图片
                CompressImg(filepath, ZoomPicBySpeed(objNewPic, speed), 100);
                CompressImg(orcPath, objNewPic, 100);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SaveBitmapIntoFileFromSpeed: " + e.Message, -1);
            }
            finally
            {
                objNewPic = null;
            }
        }

        public static byte[] GetBytes(Image image)
        {
            try
            {
                if (image == null) return null;
                using (Bitmap bitmap = new Bitmap(image))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Jpeg);
                        return stream.GetBuffer();
                    }
                }
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
        }

        public enum JoinMode
        {
            /// <summary>
            /// 横向
            /// </summary>
            Horizontal,
            /// <summary>
            /// 纵向
            /// </summary>
            Vertical
        }

        public static Image JoinImage(List<Image> imageList, JoinMode jm)
        {
            //图片列表
            if (imageList.Count <= 0)
            {
                return null;
            }

            //横向拼接
            if (jm == JoinMode.Horizontal)
            {
                int width = 0;

                //计算总长度
                foreach (Image i in imageList)
                {
                    width += i.Width;
                }

                //高度不变
                int height = imageList.Max(x => x.Height);

                //构造最终的图片白板
                Bitmap tableChartImage = new Bitmap(width, height);
                Graphics graph = Graphics.FromImage(tableChartImage);
                graph.DrawImage(tableChartImage, width, height);//初始化这个大图

                //初始化当前宽
                int currentWidth = 0;
                foreach (Image i in imageList)
                {
                    graph.DrawImage(i, currentWidth, 0, i.Width, i.Height);//一张一张的拼图
                    currentWidth += i.Width;//拼接改图后，当前宽度
                }

                return tableChartImage;
            }
            //纵向拼接
            else if (jm == JoinMode.Vertical)
            {
                int height = 0;

                //计算总长度
                foreach (Image i in imageList)
                {
                    height += i.Height;
                }
                
                int width = imageList.Max(x => x.Width);//宽度不变

                //构造最终的图片白板
                Bitmap tableChartImage = new Bitmap(width, height);
                Graphics graph = Graphics.FromImage(tableChartImage);
                graph.DrawImage(tableChartImage, width, height);//初始化这个大图

                //初始化当前宽
                int currentHeight = 0;
                foreach (Image i in imageList)
                {
                    graph.DrawImage(i, 0, currentHeight, i.Width, i.Height);//一张一张的拼图
                    currentHeight += i.Height;//拼接改图后，当前宽度
                }

                return tableChartImage;
            }
            else
            {
                return null;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType == mimeType)
                {
                    return codecs[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 缩小图片
        /// </summary>
        /// <param name="strOldPic">源图文件名(包括路径)</param>
        /// <param name="strNewPic">缩小后保存为文件名(包括路径)</param>
        /// <param name="intWidth">缩小至宽度</param>
        /// <param name="intHeight">缩小至高度</param>
        public void SmallPicByWidthAndHeight(string strOldPic, string strNewPic, int intWidth, int intHeight)
        {
            Bitmap objPic, objNewPic;

            try
            {
                objPic = new Bitmap(strOldPic);

                objNewPic = new Bitmap(objPic, intWidth, intHeight);
                objNewPic.Save(strNewPic);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SmallPicByWidthAndHeight: " + e.Message, -1);
            }
            finally
            {
                objPic = null;
                objNewPic = null;
            }
        }

        /// <summary>
        /// 按比例缩小图片，自动计算高度
        /// </summary>
        /// <param name="strOldPic">源图文件名(包括路径)</param>
        /// <param name="strNewPic">缩小后保存为文件名(包括路径)</param>
        /// <param name="intWidth">缩小至宽度</param>
        public void SmallPicByWidth(string strOldPic, string strNewPic, int intWidth)
        {
            Bitmap objPic, objNewPic;

            try
            {
                objPic = new Bitmap(strOldPic);

                int intHeight = (intWidth / objPic.Width) * objPic.Height;
                objNewPic = new Bitmap(objPic, intWidth, intHeight);
                objNewPic.Save(strNewPic);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SmallPicByWidth: " + e.Message, -1);
            }
            finally
            {
                objPic = null;
                objNewPic = null;
            }
        }

        /// <summary>
        /// 按比例缩小图片，自动计算宽度
        /// </summary>
        /// <param name="strOldPic">源图文件名(包括路径)</param>
        /// <param name="strNewPic">缩小后保存为文件名(包括路径)</param>
        /// <param name="intHeight">缩小至高度</param>
        public void SmallPicByHeight(string strOldPic, string strNewPic, int intHeight)
        {
            Bitmap objPic, objNewPic;

            try
            {
                objPic = new Bitmap(strOldPic);

                int intWidth = (intHeight / objPic.Height) * objPic.Width;
                objNewPic = new Bitmap(objPic, intWidth, intHeight);
                objNewPic.Save(strNewPic);
            }
            catch (Exception e)
            {
                AddLog("CameraDataHelper.class SmallPicByHeight: " + e.Message, -1);
            }
            finally
            {
                objPic = null;
                objNewPic = null;
            }
        }

        /// <summary>
        /// 根据速度缩放图片
        /// </summary>
        /// <param name="threshold">阈值</param>
        public static Bitmap ZoomPicBySpeed(Image image, int speed, int threshold = 800)
        {
            int height = (int)((float)speed / threshold * image.Height);
            Bitmap bitmap = new Bitmap(image, image.Width, height);
            return bitmap;
        }

        /// <summary>
        /// 压缩成zip
        /// </summary>
        /// <param name="filesPath">d:\</param>
        /// <param name="zipFilePath">d:\a.zip</param>
        public static void CreateZipFile(string filesPath, string zipFilePath)
        {

            if (!Directory.Exists(filesPath))
            {
                Console.WriteLine("Cannot find directory '{0}'", filesPath);
                return;
            }

            try
            {
                string[] filenames = Directory.GetFiles(filesPath);
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {

                    s.SetLevel(9); // 压缩级别 0-9
                                   //s.Password = "123"; //Zip压缩文件密码
                    byte[] buffer = new byte[4096]; //缓冲区大小
                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);
            }
        }
    }
}
