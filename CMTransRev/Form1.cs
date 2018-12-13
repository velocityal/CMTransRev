using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace CMTransRev
{
    public partial class Form1 : Form
    {
        TesseractEngine ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
        Bitmap img;
        int testtop;
        int newtop;
        public static Bitmap GrayScale(Bitmap b)
        {

            var test = MakeGrayscale3(b);
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            //BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //int stride = bmData.Stride;
            //System.IntPtr Scan0 = bmData.Scan0;

            //unsafe
            //{
            //    byte* p = (byte*)(void*)Scan0;

            //    int nOffset = stride - b.Width * 3;

            //    byte red, green, blue;

            //    for (int y = 0; y < b.Height; ++y)
            //    {
            //        for (int x = 0; x < b.Width; ++x)
            //        {
            //            blue = p[0];
            //            green = p[1];
            //            red = p[2];

            //            p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

            //            p += 3;
            //        }
            //        p += nOffset;
            //    }
            //}
            //int bytes = Math.Abs(bmData.Stride) * b.Height;
            //byte[] outPut = new byte[bytes];
            //Bitmap test = new Bitmap(b.Width, b.Height, PixelFormat.Format8bppIndexed);
            //test.Palette = b.Palette;
            //BitmapData testData = test.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            //Marshal.Copy(outPut, 0, testData.Scan0, bytes);
            //test.UnlockBits(bmData);

            //b.UnlockBits(bmData);

            return test;
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();
            
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private Bitmap rescale(Bitmap img, int stride)
        {
           

           
            Bitmap oBitmap = img;
            Graphics oGraphic = Graphics.FromImage(oBitmap);

            // color black pixels (i think the default is black but this helps to explain)
            SolidBrush oBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            oGraphic.FillRectangle(oBrush, 0, 0, 1, 1);
            oGraphic.FillRectangle(oBrush, 1, 1, 1, 1);

            //color white pixels
            oBrush = new SolidBrush(Color.FromArgb(0, 0, 0));
            oGraphic.FillRectangle(oBrush, 0, 1, 1, 1);
            oGraphic.FillRectangle(oBrush, 1, 0, 1, 1);

            // downscale to with 2x2
            Bitmap result = new Bitmap(img.Width/stride, img.Height/stride);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                // I don't know what these settings should be :

                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                //draw the image into the target bitmap 
                graphics.DrawImage(oBitmap, 0, 0, result.Width, result.Height);
            }

            pictureBox1.Height = result.Height;
            pictureBox1.Width = result.Width;
            pictureBox2.Height = result.Height;
            pictureBox2.Width = result.Width;
            pictureBox1.Image = img;
            pictureBox2.Image = result;
            return result;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private Page textDetect(Bitmap pic)
        {
            var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
            //Tesseract recog = new Tesseract(./tessdata", "jpn", Tesseract.OcrEngineMode.OEM_TESSERACT_ONLY);
            var page = ocr.Process(pic, Tesseract.PageSegMode.SingleChar);
            
            String iText = page.GetText();
            Console.WriteLine("Height: " + pic.Height);
            if(page.GetMeanConfidence() > 0.7)
            {
                return page;
            }
           // textBox1.Text = page.GetText();
            return page;
        }
        //test



        private void button2_Click_1(object sender, EventArgs e)
        {
           
            //for(int i = 0; i < img.Width; i++)
            //{
            //    for(int j = 0; j < img.Height; j++)
            //    {
            int i = 0;
            int j = 0;
            int x = 100;
            Recal:

            int top = 10;
          
                Int32.TryParse(textBox2.Text,out x);

            Page page;
            Bitmap cloneBitmap;
            do
            {
                top += x;
                int z = 0;
                if (top > (img.Height - j))
                {
                    top = img.Height - j; // - (img.Height / 10) - j;
                }
                do
                {
                    z += 100;
                    if (z > (img.Width - i ))
                    {
                        z = img.Width - i; // - (img.Width / 10) - i;
                    }


                    // Rectangle cloneRect = new Rectangle(i, j, i + img.Width / 8, j + top + img.Height / 10);
                    //  Rectangle cloneRect = new Rectangle(i, j, 400,364);
                    Rectangle cloneRect = new Rectangle(i, j, z, top);
                    // j += 9;



                    System.Drawing.Imaging.PixelFormat format =
                        img.PixelFormat;
                    cloneBitmap = img.Clone(cloneRect, format);
                    page = textDetect(cloneBitmap);
                    
                } while (page.GetMeanConfidence() * 100 <= 77 && z < (img.Width - i));

                
                
            } while (page.GetMeanConfidence() * 100 <= 77 && top < (img.Height - j));
            textBox1.Text += page.GetText();
            label1.Text = "Confidence: " + page.GetMeanConfidence().ToString();
            if (page.GetMeanConfidence() * 100 >= 77)
            {
                j = top;
                goto Recal;
            }

            
           
            // Draw the cloned portion of the Bitmap object.
            pictureBox1.Image = cloneBitmap;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == DialogResult.OK)
                //Page page = new Page;
            {
                int x = 1;
                x = Int32.Parse(textBox3.Text);
                img = new Bitmap(openfile.FileName);
               // var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
              // img = ocr.Process(img).;
                 img = rescale(img, x);
               // ocr.Process(img).AnalyseLayout();
                label2.Text = img.Height.ToString();
              //  var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
              //  //for(int i = 0; i < img.Width; i++)
              //  //{
              //  //    for(int j = 0; j < img.Height; j++)
              //  //    {
              //  int i = 0;
              //  int j = 0;
              //  int top = 100;
                
              //  Page page;
              //  Bitmap cloneBitmap;
              //  do
              //  {
              //     if (top >(img.Height - (img.Height / 10) - j))
              //     {
              //          top = img.Height - (img.Height / 10) - j;
              //     }
              //  Rectangle cloneRect = new Rectangle(i, j, i + img.Width / 8, j + top + img.Height / 10);
              ////  Rectangle cloneRect = new Rectangle(i, j, 400,364);
              //  // j += 9;



              //  System.Drawing.Imaging.PixelFormat format =
              //      img.PixelFormat;
              //       cloneBitmap = img.Clone(cloneRect, format);
              //      page = textDetect(cloneBitmap);
              //      top += 50;
                    
              // } while (page.GetMeanConfidence() * 100 <= 90);
              //          textBox1.Text += page.GetText();
              //          label1.Text = "Confidence: " + page.GetMeanConfidence().ToString();
              //          // Draw the cloned portion of the Bitmap object.
              //          pictureBox1.Image = cloneBitmap;
              //  //    }
              //  //   i += 9;
              //  //}
                
                
              ////  pictureBox1.Image = img;
              //  textBox1.ReadOnly = true;
              // // pictureBox1.Image = GrayScale(img);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int i = 0;
            int j = 0;
            int x = 100;
            int top = 10;
            button3.Click += (sender2, e2) => button3_Click(sender2, e2, i, x, top);

        }

        private void button3_Click(object sender, EventArgs e, int i, int x, int top)
        {
            int j = testtop;

           
            //var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
            //for(int i = 0; i < img.Width; i++)
            //{
            //    for(int j = 0; j < img.Height; j++)
            //    {
            
            Recal:
            
            

            Int32.TryParse(textBox2.Text, out x);

            Page page;
            Bitmap cloneBitmap;
            Relapse:
           // do
           // {
                newtop += x;
            top = newtop;
            int z = 0;
                if (top > (img.Height - j))
                {
                    top = img.Height - j; // - (img.Height / 10) - j;
                }
               // do
               // {
                    z += 100;
                    if (z > (img.Width - i))
                    {
                        z = img.Width - i; // - (img.Width / 10) - i;
                    }


                    // Rectangle cloneRect = new Rectangle(i, j, i + img.Width / 8, j + top + img.Height / 10);
                    //  Rectangle cloneRect = new Rectangle(i, j, 400,364);
                    Rectangle cloneRect = new Rectangle(i, j, z, top);
                    // j += 9;



                    System.Drawing.Imaging.PixelFormat format =
                        img.PixelFormat;
                    cloneBitmap = img.Clone(cloneRect, format);
            pictureBox1.Image = cloneBitmap;
            if (GetBlackDots(top, x, cloneBitmap) == true)
            {
                goto Relapse;
            }
  
            
                //goto Ext;
                page = textDetect(cloneBitmap);

                // } while (page.GetMeanConfidence() * 100 <= 77 && z < (img.Width - i));



                //} while (page.GetMeanConfidence() * 100 <= 77 && top < (img.Height - j));


                if ((page.GetMeanConfidence() * 100 >= 69) || top >= (img.Height - j))
                {

                    textBox1.Text += page.GetText();
                    testtop += top;
                    newtop = 0;
                    // goto Recal;
                }
                else
                {
                    //        goto Relapse;
                }

            
               
            // Draw the cloned portion of the Bitmap object.
            label1.Text = "Confidence: " + page.GetMeanConfidence().ToString();
           // Ext:;

        }

        public Boolean GetBlackDots(int j, int i, Bitmap pic)
        {
            //if(i > j)
            //{
            //    i = 0;
            //}
            Color pixelColor;
            var list = new List<String>();
            for (int y = 0; y < i; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    pixelColor = img.GetPixel(x, j + y);
                if (pixelColor.R <= 200 && pixelColor.G <= 200 && pixelColor.B <= 200)
                    return true;
                       // list.Add(String.Format("x:{0} y:{1}", x, j));
                }
                return false;
            }
            return false;
        }

        private void test()
        {


        }
    }
}
