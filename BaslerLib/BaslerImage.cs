using AForge.Imaging;
using AForge.Imaging.Filters;
using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Basler
{
    public class BaslerImage : IDisposable
	{
		public enum ImagePixelFormat
		{
			BIN,
			GRAY8,
			GRAY12,
			GRAY16,
			RGB,
			RGBA,
			BAYERBG8,
			BAYERRG8,
			BAYERGB8
		}

		public enum ImageRotation
		{
			None,
			Rotate90Degree,
			Rotate180Degree,
			Rotate270Degree
		}

		private int _objectID;
		private int _cameraID;
		private int _imageNumber;

		private int _width;
		private int _height;
		private ImageFormat _format = ImageFormat.Tiff;
		private PixelFormat _pixelFormat = PixelFormat.Format8bppIndexed;
		private ImagePixelFormat _PixelFormat = ImagePixelFormat.GRAY8;
		private string _filename;
		private string _location;
		private byte[] _rawImage;
		private Bitmap _bitmap;
		private Bitmap _displayBitmap;
		private List<ImageAnnotation> _annotations = new List<ImageAnnotation>();

		[Category("Identifier")]
		public int ObjectID { get => _objectID; set => _objectID = value; }

		[Category("Identifier")]
		public int CameraID { get => _cameraID; set => _cameraID = value; }

		[Category("Identifier")]
		public int ImageNumber { get => _imageNumber; set => _imageNumber = value; }

		[Category("Image properties")]
		public int Width { get => _width; set => _width = value; }

		[Category("Image properties")]
		public PixelFormat PixelFormat { get => _pixelFormat; set => _pixelFormat = value; }

		[Category("Image properties")]
		public ImagePixelFormat BaslerPixelFormat { get => _PixelFormat; set => _PixelFormat = value; }

		[Category("Image properties")]
		public int Height { get => _height; set => _height = value; }

		[Category("Image properties")]
		public ImageFormat Format { get => _format; set => _format = value; }

		[Category("Image properties")]
		public string Filename { get => _filename; set => _filename = value; }

		[Category("Image properties")]
		public string Location { get => _location; set => _location = value; }

		[Category("Image properties")]
		public byte[] RawImage { get => _rawImage; }

		[Category("Image properties")]
		public List<ImageAnnotation> Annotations { get => _annotations; set => _annotations = value; }

		[Category("Image properties")]
		public Bitmap Bitmap { get => _bitmap; }

		[Category("Image properties")]
		public Bitmap DisplayBitmap { get => _displayBitmap; }

		public BaslerImage() { }

		public BaslerImage(string filename)
		{
			CreateImage(filename);
		}

		public BaslerImage(Bitmap bitmap)
		{
			CreateImage(bitmap);
		}

		public void CreateImage(int width, int height, byte[] rawImage, ImageRotation rotation = ImageRotation.None, ImagePixelFormat format = ImagePixelFormat.GRAY8)
		{
			if (rawImage == null || rawImage.Length == 0)
			{
				return;
			}

			Array.Resize<byte>(ref _rawImage, rawImage.Length);
			_PixelFormat = format;
			int bytesPerPixel = 1;

			if (format == ImagePixelFormat.GRAY12 || format == ImagePixelFormat.GRAY16)
			{
				bytesPerPixel = 2;
			}

			switch (rotation)
			{
				case ImageRotation.None:
					_width = width;
					_height = height;
					Array.Copy(rawImage, _rawImage, rawImage.Length);
					break;
				case ImageRotation.Rotate90Degree:
					_width = height;
					_height = width;

					for (int x = 0; x < width; x++)
						for (int y = 0; y < height; y++)
						{
							int yy = x;
							int xx = _width - 1 - y;
							for (int b = 0; b < bytesPerPixel; b++)
								_rawImage[(yy * _width + xx) * bytesPerPixel + b] = rawImage[(y * width + x) * bytesPerPixel + b];
						}
					break;
				case ImageRotation.Rotate180Degree:
					_width = width;
					_height = height;

					for (int x = 0; x < width; x++)
						for (int y = 0; y < height; y++)
						{
							int yy = _height - 1 - y;
							int xx = _width - 1 - x;
							for (int b = 0; b < bytesPerPixel; b++)
								_rawImage[(yy * _width + xx) * bytesPerPixel + b] = rawImage[(y * width + x) * bytesPerPixel + b];
						}
					break;
				case ImageRotation.Rotate270Degree:
					_width = height;
					_height = width;

					for (int x = 0; x < width; x++)
					{
						for (int y = 0; y < height; y++)
						{
							int yy = _height - 1 - x;
							int xx = y;
							for (int b = 0; b < bytesPerPixel; b++)
								_rawImage[(yy * _width + xx) * bytesPerPixel + b] = rawImage[(y * width + x) * bytesPerPixel + b];
						}
					}
					break;
			}

			_bitmap = GetBitmap();

			if (_PixelFormat == ImagePixelFormat.BAYERRG8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.R, RGB.G }, { RGB.G, RGB.B } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERBG8)
			{
				BayerFilter filter = new BayerFilter();
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERGB8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.G, RGB.B }, { RGB.R, RGB.G } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else
				_displayBitmap = _bitmap;

			_annotations.Clear();
		}

		public void CreateImage(string filename)
		{
			_filename = filename;
			if (!File.Exists(filename))
				throw new Exception("未找到图片");

			if (filename.Split('.').Last().ToLower() == "tif" || filename.Split('.').Last().ToLower() == "tiff")
				_bitmap = DecodeTiff(_filename);
			else
			{
				_bitmap = new Bitmap(_filename);
				_width = _bitmap.Width;
				_height = _bitmap.Height;
			}

			if (_PixelFormat == ImagePixelFormat.BAYERRG8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.R, RGB.G }, { RGB.G, RGB.B } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERBG8)
			{
				BayerFilter filter = new BayerFilter();
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERGB8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.G, RGB.B }, { RGB.R, RGB.G } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else
				_displayBitmap = _bitmap;

			_annotations.Clear();
		}

		public void CreateImage(Bitmap bitmap)
		{
			if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
			{
				return;
			}

			_bitmap = bitmap;
			_width = _bitmap.Width;
			_height = _bitmap.Height;

			if (_PixelFormat == ImagePixelFormat.BAYERRG8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.R, RGB.G }, { RGB.G, RGB.B } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERBG8)
			{
				BayerFilter filter = new BayerFilter();
				_displayBitmap = filter.Apply(_bitmap);
			}
			else if (_PixelFormat == ImagePixelFormat.BAYERGB8)
			{
				BayerFilter filter = new BayerFilter();
				filter.BayerPattern = new int[2, 2] { { RGB.G, RGB.B }, { RGB.R, RGB.G } };
				_displayBitmap = filter.Apply(_bitmap);
			}
			else
				_displayBitmap = _bitmap;

			_annotations.Clear();
		}

		private Bitmap DecodeTiff(string filename)
		{
			Bitmap result;

			using (Tiff tif = Tiff.Open(filename, "r"))
			{
				if (tif == null)
				{
					return null;
				}

				FieldValue[] res = tif.GetField(TiffTag.IMAGELENGTH);
				int height = res[0].ToInt();
				_height = height;

				res = tif.GetField(TiffTag.IMAGEWIDTH);
				int width = res[0].ToInt();
				_width = width;

				res = tif.GetField(TiffTag.BITSPERSAMPLE);
				short bitsPerSample = res[0].ToShort();

				res = tif.GetField(TiffTag.SAMPLESPERPIXEL);
				short samplesPerPixel = res == null ? (short)1 : res[0].ToShort();

				int stride = tif.ScanlineSize();
				byte[] buffer = new byte[stride];

				if (samplesPerPixel == 3)
				{
					return new Bitmap(filename);
				}

				_rawImage = new byte[stride * height];
				for (int i = 0; i < height; i++)
				{
					tif.ReadScanline(buffer, i);
					Array.Copy(buffer, 0, _rawImage, i * stride, stride);
				}

				if (bitsPerSample == 8)
				{
					_pixelFormat = PixelFormat.Format8bppIndexed;
					result = GetBitmap();
				}
				else if (bitsPerSample == 16)
				{
					int bytesPerPixel = 2;
					byte[] bytes8 = new byte[width * height];

					bool is16BitImage = false;
					for (int i = 0; i < bytesPerPixel * width * height; i = i + bytesPerPixel)
					{
						if (_rawImage[i + 1] > 16)
						{
							is16BitImage = true;
							break;
						}
						bytes8[i / 2] = (byte)(_rawImage[i + 1] * 16 + _rawImage[i] / 16);
					}

					if (is16BitImage)
					{
						result = new Bitmap(filename);
						_pixelFormat = result.PixelFormat;
					}
					else
					{
						result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
						_pixelFormat = result.PixelFormat;
						
						if (result.PixelFormat == PixelFormat.Format8bppIndexed)
						{
							ColorPalette colorPalette = result.Palette;
							for (int i = 0; i < 256; i++)
							{
								colorPalette.Entries[i] = Color.FromArgb(i, i, i);
							}
							result.Palette = colorPalette;
						}

						BitmapData bmpData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadWrite, result.PixelFormat);
						IntPtr ptrBmp = bmpData.Scan0;
						int imageStride = width;
						
						if (imageStride == bmpData.Stride)
						{
							Marshal.Copy(bytes8, 0, ptrBmp, bmpData.Stride * result.Height);
						}
						else 
						{
							for (int i = 0; i < result.Height; ++i)
							{
								Marshal.Copy(bytes8, i * imageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), width);
							}
						}

						result.UnlockBits(bmpData);
					}
				}
				else
				{
					result = new Bitmap(filename);
					_pixelFormat = result.PixelFormat;
				}
			}

			return result;
		}

		private static bool convertBuffer(byte[] buffer, byte[] buffer8Bit)
		{
			for (int i = 0; i < buffer.Length; i = i + 2)
			{
				if (buffer[i + 1] > 16)
					return true;
				buffer8Bit[i / 2] = (byte)(buffer[i + 1] * 16 + buffer[i] / 16);
			}
			return false;
		}

		private byte[] ClipByteArray(byte[] source, long startIndex, int length, bool reverse = false)
		{
			byte[] array = new byte[length];
			for (int i = 0; i < length; i++)
				array[i] = reverse ? source[startIndex + length - 1 - i] : source[startIndex + i];

			return array;
		}

		public void SaveAsTiff(string filename)
		{
			if (_rawImage == null || _rawImage.Length == 0)
			{
				if (_bitmap == null)
					throw new Exception("图片数据为Null");
				else
				{
					_bitmap.Save(filename, ImageFormat.Tiff);
					_filename = filename;
					return;
				}
			}

			if (_PixelFormat == ImagePixelFormat.GRAY12 || _PixelFormat == ImagePixelFormat.GRAY16)
			{
				using (Tiff output = Tiff.Open(filename, "w"))
				{
					output.SetField(TiffTag.IMAGEWIDTH, _width);
					output.SetField(TiffTag.IMAGELENGTH, _height);
					output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
					output.SetField(TiffTag.BITSPERSAMPLE, 16);
					output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
					output.SetField(TiffTag.ROWSPERSTRIP, _height);
					output.SetField(TiffTag.XRESOLUTION, 88.0);
					output.SetField(TiffTag.YRESOLUTION, 88.0);
					output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
					output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
					output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
					output.SetField(TiffTag.COMPRESSION, Compression.NONE);
					output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
					output.WriteRawStrip(0, _rawImage, _rawImage.Length);
					output.WriteDirectory();
				}
			}
			else
			{
				_bitmap.Save(filename, ImageFormat.Tiff);
			}
			_filename = filename;
		}

		public byte[] GetTiffStream()
		{
			try
			{
				if (_PixelFormat == ImagePixelFormat.GRAY12 || _PixelFormat == ImagePixelFormat.GRAY16)
				{
					using (Tiff output = Tiff.Open("tempfile", "w"))
					{
						output.SetField(TiffTag.IMAGEWIDTH, _width);
						output.SetField(TiffTag.IMAGELENGTH, _height);
						output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
						output.SetField(TiffTag.BITSPERSAMPLE, 16);
						output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
						output.SetField(TiffTag.ROWSPERSTRIP, _height);
						output.SetField(TiffTag.XRESOLUTION, 88.0);
						output.SetField(TiffTag.YRESOLUTION, 88.0);
						output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
						output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
						output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
						output.SetField(TiffTag.COMPRESSION, Compression.NONE);
						output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

						output.WriteRawStrip(0, _rawImage, _rawImage.Length);

						TiffStream obj = new TiffStream();
						byte[] buffer = new byte[obj.Size(output.Clientdata())];
						obj = output.GetStream();
						obj.Read(output.Clientdata(), buffer, 0, buffer.Length);
						output.Close();
						output.Dispose();
						return buffer;
					}
				}
				else
				{
					using (MemoryStream ms = new MemoryStream())
					{
						_bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
						return ms.ToArray();
					}
				}
			}
			catch { }
			return null;
		}

		private Bitmap GetBitmap()
		{
			if (_rawImage == null || _width == 0 || _height == 0)
			{
				return null;
			}

			Bitmap bitmap = new Bitmap(_width, _height, _pixelFormat);

			if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
			{
				ColorPalette colorPalette = bitmap.Palette;
				for (int i = 0; i < 256; i++)
				{
					colorPalette.Entries[i] = Color.FromArgb(i, i, i);
				}
				bitmap.Palette = colorPalette;
			}

			BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			IntPtr ptrBmp = bmpData.Scan0;
			int imageStride;

			switch (_pixelFormat)
			{
				case PixelFormat.Format32bppArgb:
					imageStride = _width * 4;
					break;
				case PixelFormat.Format24bppRgb:
					imageStride = _width * 3;
					break;
				case PixelFormat.Format8bppIndexed:
					imageStride = _width;
					break;
				default:
					imageStride = _width;
					break;
			}

			byte[] rawImage;
			byte[] bytes8 = new byte[_width * _height];
			
			if (_PixelFormat == ImagePixelFormat.GRAY12)
			{
				for (int i = 0; i < 2 * _width * _height; i += 2)
					bytes8[i / 2] = (byte)(_rawImage[i + 1] * 16 + _rawImage[i] / 16);
				rawImage = bytes8;
			}
			else if (_PixelFormat == ImagePixelFormat.GRAY16)
			{
				for (int i = 0; i < 2 * _width * _height; i += 2)
					bytes8[i / 2] = (byte)(_rawImage[i + 1] + _rawImage[i] / 256);
				rawImage = bytes8;
			}
			else
				rawImage = _rawImage;

			if (imageStride == bmpData.Stride)
			{
				Marshal.Copy(rawImage, 0, ptrBmp, bmpData.Stride * bitmap.Height);
			}
			else
			{
				for (int i = 0; i < bitmap.Height; ++i)
				{
					Marshal.Copy(rawImage, i * imageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), _width);
				}
			}

			bitmap.UnlockBits(bmpData);
			return bitmap;
		}

		public void Dispose()
		{
			_rawImage = null;
			_bitmap.Dispose();
			_displayBitmap.Dispose();
			_annotations.Clear();
		}
	}
}
