using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        Bitmap img;

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


        public Form1()
        {
            InitializeComponent();
        }

        private Page textDetect(Bitmap img)
        {
            var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);

            var page = ocr.Process(img, Tesseract.PageSegMode.SingleChar);
            String iText = page.GetText();
            if(iText != "")
            {
                return page;
            }
           // textBox1.Text = page.GetText();
            return page;
        }
        //test

        private void button2_Click_1(object sender, EventArgs e)
        {
            var ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
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

                 img = new Bitmap(openfile.FileName);
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

       
    }
}
