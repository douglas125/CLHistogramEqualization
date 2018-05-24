using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

using OpenCLTemplate;

namespace CLHistogramEqualization
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap bmp;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                CLCalc.InitCL();
            }
            catch
            {
            }
            CLSrc src=new CLSrc();
            string s = src.src;

            try
            {
                CLCalc.Program.Compile(s);
            }
            catch
            {
            }
        }

        private void btnEqualize_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            //for (int i = 0; i < 100; i++)
            {
                EqualizeHistogram(ref bmp);
            }
            sw.Stop();
            this.Text = sw.Elapsed.ToString();
            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
        }

        private void btnEqua_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            //for (int i = 0; i < 100; i++)
            {
                CLEqualizeHistogram(ref bmp);
            }
            sw.Stop();
            this.Text = sw.Elapsed.ToString();
            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.bmp;*.jpg;*.tiff;*.jps";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bmp = new Bitmap(ofd.FileName);

                //if (bmp.Width > 4096)
                //{
                //    Bitmap bmpNew = new Bitmap(bmp, new Size(4096, (int)((float)bmp.Height * 4096.0f / (float)bmp.Width)));

                //    bmp = bmpNew;
                //}

                pictureBox1.Image = bmp;
                pictureBox1.Refresh();
            }
        }

        private void btnHue_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = CLHSL(bmp)[0];
        }
        private void btnSat_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = CLHSL(bmp)[1];
        }
        private void btnLum_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = CLHSL(bmp)[2];
        }


        #region Image manipulation

        /// <summary>Equalize RGB histogram in conventional way</summary>
        /// <param name="bmp">Bitmap to get equalized. Gets replaced by the equalized version</param>
        private void EqualizeHistogram(ref Bitmap bmp)
        {
            BitmapData bmdbmp = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            //Computes histogram
            int N = 1024;
            float[] histLuminance = new float[N];

            unsafe
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    byte* rowBmp = (byte*)bmdbmp.Scan0 + (y * bmdbmp.Stride);

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int xPix2 = x * PIXELSIZE;

                        float B = rowBmp[xPix2];
                        float G = rowBmp[xPix2 + 1];
                        float R = rowBmp[xPix2 + 2];

                        float[] hsl = RGBtoHSL(R * 0.00392156862745098f, G * 0.00392156862745098f, B * 0.00392156862745098f);

                        histLuminance[(int)((N - 1) * hsl[2])]++;
                    }
                }
            }

            //Compute histogram integrals in-place
            for (int i = 1; i < N; i++)
            {
                histLuminance[i] += histLuminance[i - 1];
            }

            float scale = 0.9f / histLuminance[N - 1];

            //Scales histograms
            for (int i = 0; i < N; i++)
            {
                histLuminance[i] *= scale;
            }

            //Transformation: intensity I becomes histX[I]*scaleX
            unsafe
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    byte* rowBmp = (byte*)bmdbmp.Scan0 + (y * bmdbmp.Stride);

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int xPix2 = x * PIXELSIZE;

                        float B = rowBmp[xPix2];
                        float G = rowBmp[xPix2 + 1];
                        float R = rowBmp[xPix2 + 2];

                        float[] hsl = RGBtoHSL(R * 0.00392156862745098f, G * 0.00392156862745098f, B * 0.00392156862745098f);

                        hsl[2] = histLuminance[(int)((N - 1) * hsl[2])];

                        //hsl[1] *= 2.5f;

                        float[] rgb = HSLtoRGB(hsl[0], hsl[1], hsl[2]);

                        if (rgb[0] < 0) rgb[0] = 0;
                        if (rgb[1] < 0) rgb[1] = 0;
                        if (rgb[2] < 0) rgb[2] = 0;
                        if (rgb[0] > 1) rgb[0] = 1;
                        if (rgb[1] > 1) rgb[1] = 1;
                        if (rgb[2] > 1) rgb[2] = 1;

                        //if (rgb[0] < 0 || rgb[1] < 0 || rgb[2] < 0)
                        //{
                        //    rgb[0] = 0;
                        //    rgb[1] = 0;
                        //    rgb[2] = 0;
                        //}
                        //else if (rgb[0] > 1 || rgb[1] > 1 || rgb[2] > 1)
                        //{
                        //}
                        //else
                        {
                            rgb[0] *= 255.0f;
                            rgb[1] *= 255.0f;
                            rgb[2] *= 255.0f;
                        }

                        rowBmp[xPix2] = (byte)(rgb[2]);
                        rowBmp[1 + xPix2] = (byte)(rgb[1]);
                        rowBmp[2 + xPix2] = (byte)(rgb[0]);
                    }
                }
            }

            bmp.UnlockBits(bmdbmp);
        }

        private float[] RGBtoHSL(float r, float g, float b)
        {
            // r,b and b are assumed to be in the range 0...1
            float luminance = r * 0.299f + g * 0.587f + b * 0.114f;
            float u = -r * 0.1471376975169300226f - g * 0.2888623024830699774f + b * 0.436f;
            float v = r * 0.615f - g * 0.514985734664764622f - b * 0.100014265335235378f;
            float hue = (float)Math.Atan2(v, u);
            float saturation = (float)Math.Sqrt(u * u + v * v);

            return new float[] { hue, saturation, luminance };
        }

        private float[] HSLtoRGB(float hue, float saturation, float luminance)
        {
            // hue is an angle in radians (-Pi...Pi)
            // for saturation the range 0...1/sqrt(2) equals 0% ... 100%
            // luminance is in the range 0...1
            float u = (float)(Math.Cos(hue) * saturation);
            float v = (float)(Math.Sin(hue) * saturation);
            float r = luminance + 1.139837398373983740f * v;
            float g = luminance - 0.3946517043589703515f * u - 0.5805986066674976801f * v;
            float b = luminance + 2.03211091743119266f * u;

            return new float[] { r, g, b };
        }

        /// <summary>Pixel size</summary>
        private const int PIXELSIZE = 4;

        /// <summary>Converts bitmap to grayscale in-place.</summary>
        /// <param name="bmp">Bitmap to be converted</param>
        public static void ConvertToBlackWhite(Bitmap bmp)
        {
            BitmapData bmdbmp = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);


            unsafe
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    byte* rowBmp = (byte*)bmdbmp.Scan0 + (y * bmdbmp.Stride);

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int xPix2 = x * PIXELSIZE;

                        float val = (rowBmp[xPix2] + rowBmp[xPix2 + 1] + rowBmp[xPix2 + 2]) * 0.333333332f;

                        rowBmp[xPix2] = (byte)val;
                        rowBmp[xPix2 + 1] = (byte)val;
                        rowBmp[xPix2 + 2] = (byte)val;
                    }
                }
            }

            bmp.UnlockBits(bmdbmp);


        }
        #endregion

        #region Image manipulation using OpenCL

        /// <summary>Number of luminance intensities</summary>
        private const int NLumIntens = 1024;

        /// <summary>Bitmap in OpenCL memory</summary>
        CLCalc.Program.Image2D CLbmp, CLNewBmp;

        /// <summary>HSL Conversion. CLNewBmp holds Hue</summary>
        CLCalc.Program.Image2D CLBmpSaturation, CLBmpIntens;

        /// <summary>Partial histograms</summary>
        CLCalc.Program.Variable CLPartialHistograms;
        /// <summary>Miscellaneous info</summary>
        CLCalc.Program.Variable CLN, CLWidth, CLHeight;

        
        /// <summary>Final histogram</summary>
        CLCalc.Program.Variable CLHistogram;
        /// <summary>Compute histograms kernel</summary>
        CLCalc.Program.Kernel kernelComputeHistograms;
        /// <summary>Consolidate histogram from partials</summary>
        CLCalc.Program.Kernel kernelConsolidateHist;
        /// <summary>Kernel to perform normalization</summary>
        CLCalc.Program.Kernel kernelPerformNormalization;

        /// <summary>Kernel to compute Hue</summary>
        CLCalc.Program.Kernel kernelComputeHue;

        /// <summary>Initialize kernels</summary>
        private void InitKernels()
        {
            if (kernelComputeHistograms == null || CLN == null)
            {
                CLSrc src = new CLSrc();
                CLCalc.Program.Compile(src.src);
                kernelComputeHistograms = new CLCalc.Program.Kernel("ComputeHistogram");
                kernelConsolidateHist = new CLCalc.Program.Kernel("ConsolidateHist");
                kernelPerformNormalization = new CLCalc.Program.Kernel("PerformNormalization");
                kernelComputeHue = new CLCalc.Program.Kernel("ComputeHue");

                CLN = new CLCalc.Program.Variable(new int[] { NLumIntens });
                CLWidth = new CLCalc.Program.Variable(new int[] { bmp.Width });
                CLHeight = new CLCalc.Program.Variable(new int[] { bmp.Height });

                CLbmp = new CLCalc.Program.Image2D(bmp);
                CLNewBmp = new CLCalc.Program.Image2D(bmp);
            }
        }

        /// <summary>Equalizes image histogram using OpenCL</summary>
        private void CLEqualizeHistogram(ref Bitmap bmp)
        {
            if (CLCalc.CLAcceleration == CLCalc.CLAccelerationType.Unknown) CLCalc.InitCL();
            if (CLCalc.CLAcceleration != CLCalc.CLAccelerationType.UsingCL) return;

            float[] PartialHistograms = new float[NLumIntens * bmp.Width];
            float[] histLuminance = new float[NLumIntens];

            if (kernelComputeHistograms == null || CLN == null || CLHistogram == null)
            {
                CLHistogram = new CLCalc.Program.Variable(histLuminance);
                CLPartialHistograms = new CLCalc.Program.Variable(PartialHistograms);
            }
            InitKernels();


            System.Diagnostics.Stopwatch swTotal = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swCopyBmp = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swRescaling = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swComputeHistPartial = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swComputeHistConsolid = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch swHistIntegral = new System.Diagnostics.Stopwatch();

            swTotal.Start();

            swCopyBmp.Start();
            if (CLbmp == null || CLbmp.Height != bmp.Height || CLbmp.Width != bmp.Width)
            {
                CLbmp = new CLCalc.Program.Image2D(bmp);
                CLNewBmp = new CLCalc.Program.Image2D(bmp);
                CLPartialHistograms = new CLCalc.Program.Variable(PartialHistograms);
            }
            else
            {
                CLbmp.WriteBitmap(bmp);
                CLN.WriteToDevice(new int[] { NLumIntens });
                CLWidth.WriteToDevice(new int[] { bmp.Width });
                CLHeight.WriteToDevice(new int[] { bmp.Height });
            }
            swCopyBmp.Stop();

            swComputeHistPartial.Start();

            //Partial histograms
            CLCalc.Program.MemoryObject[] args = new CLCalc.Program.MemoryObject[] { CLbmp, CLPartialHistograms, CLHeight, CLN };

            kernelComputeHistograms.Execute(args, bmp.Width);
            CLCalc.Program.Sync();
            swComputeHistPartial.Stop();



            swComputeHistConsolid.Start();

            args = new CLCalc.Program.MemoryObject[] { CLPartialHistograms,CLHistogram,CLHeight,CLN };
            kernelConsolidateHist.Execute(args, NLumIntens);

            CLHistogram.ReadFromDeviceTo(histLuminance);
            
            swComputeHistConsolid.Stop();


            swHistIntegral.Start();
            //Perform histogram integration - better performance in CPU
            //Compute histogram integrals in-place
            for (int i = 1; i < NLumIntens; i++)
            {
                histLuminance[i] += histLuminance[i - 1];
            }

            float scale = 0.9f / histLuminance[NLumIntens - 1];

            //Scales histograms
            for (int i = 0; i < NLumIntens; i++)
            {
                histLuminance[i] *= scale;
            }

            //Writes histogram integral
            CLHistogram.WriteToDevice(histLuminance);
            swHistIntegral.Stop();

            swRescaling.Start();
            //Computes equalized image
            args = new CLCalc.Program.MemoryObject[] { CLbmp, CLNewBmp, CLHistogram, CLN };
            kernelPerformNormalization.Execute(args, new int [] { bmp.Width, bmp.Height });

            bmp = CLNewBmp.ReadBitmap();
            swRescaling.Stop();


            swTotal.Stop();
        }
        
        
        private List<Bitmap> CLHSL(Bitmap bmp)
        {
            if (CLCalc.CLAcceleration == CLCalc.CLAccelerationType.Unknown) CLCalc.InitCL();
            if (CLCalc.CLAcceleration != CLCalc.CLAccelerationType.UsingCL) return null;

            InitKernels();

            if (CLBmpSaturation == null)
            {
                CLBmpSaturation = new CLCalc.Program.Image2D(bmp);
                CLBmpIntens = new CLCalc.Program.Image2D(bmp);
            }


            if (CLbmp == null || CLbmp.Height != bmp.Height || CLbmp.Width != bmp.Width)
            {
                CLbmp = new CLCalc.Program.Image2D(bmp);
                CLNewBmp = new CLCalc.Program.Image2D(bmp);
                CLBmpSaturation = new CLCalc.Program.Image2D(bmp);
                CLBmpIntens = new CLCalc.Program.Image2D(bmp);
            }
            else
            {
                CLbmp.WriteBitmap(bmp);
                CLN.WriteToDevice(new int[] { NLumIntens });
                CLWidth.WriteToDevice(new int[] { bmp.Width });
                CLHeight.WriteToDevice(new int[] { bmp.Height });
            }

            kernelComputeHue.Execute(new CLCalc.Program.MemoryObject[] { CLbmp, CLNewBmp, CLBmpSaturation, CLBmpIntens }, new int[] { bmp.Width, bmp.Height });

            return new List<Bitmap>() { CLNewBmp.ReadBitmap(), CLBmpSaturation.ReadBitmap(), CLBmpIntens.ReadBitmap() };
        }



        private class CLSrc
        {


            public string src = @"

__constant float DivBy255[256] =
{
  0.0f, 0.00392156862745098f,  0.00784313725490196f,  0.0117647058823529f,  0.0156862745098039f,  0.0196078431372549f,
  0.0235294117647059f, 0.0274509803921569f,  0.0313725490196078f,  0.0352941176470588f,  0.0392156862745098f,  0.0431372549019608f,
  0.0470588235294118f, 0.0509803921568627f,  0.0549019607843137f,  0.0588235294117647f,  0.0627450980392157f,  0.0666666666666667f,
  0.0705882352941176f, 0.0745098039215686f,  0.0784313725490196f,  0.0823529411764706f,  0.0862745098039216f,  0.0901960784313725f,
  0.0941176470588235f, 0.0980392156862745f,  0.101960784313725f,  0.105882352941176f,  0.109803921568627f,  0.113725490196078f,
  0.117647058823529f,  0.12156862745098f,   0.125490196078431f,  0.129411764705882f,  0.133333333333333f,  0.137254901960784f,  
  0.141176470588235f,  0.145098039215686f,  0.149019607843137f,  0.152941176470588f,  0.156862745098039f,  0.16078431372549f,  
  0.164705882352941f,  0.168627450980392f,  0.172549019607843f,  0.176470588235294f,  0.180392156862745f,  0.184313725490196f,  
  0.188235294117647f,  0.192156862745098f,  0.196078431372549f,  0.2f,                0.203921568627451f,  0.207843137254902f,  
  0.211764705882353f,  0.215686274509804f,  0.219607843137255f,  0.223529411764706f,  0.227450980392157f,  0.231372549019608f,
  0.235294117647059f,  0.23921568627451f,   0.243137254901961f,  0.247058823529412f,  0.250980392156863f,  0.254901960784314f,  
  0.258823529411765f,  0.262745098039216f,  0.266666666666667f,  0.270588235294118f,  0.274509803921569f,  0.27843137254902f,  
  0.282352941176471f,  0.286274509803922f,  0.290196078431373f,  0.294117647058824f,  0.298039215686275f,  0.301960784313725f,  
  0.305882352941176f,  0.309803921568627f,  0.313725490196078f,  0.317647058823529f,  0.32156862745098f,  0.325490196078431f,  
  0.329411764705882f,  0.333333333333333f,  0.337254901960784f,  0.341176470588235f,  0.345098039215686f,  0.349019607843137f,  
  0.352941176470588f,  0.356862745098039f,  0.36078431372549f,   0.364705882352941f,  0.368627450980392f,  0.372549019607843f,  
  0.376470588235294f,  0.380392156862745f,  0.384313725490196f,  0.388235294117647f,  0.392156862745098f,  0.396078431372549f,  
  0.4f,                0.403921568627451f,  0.407843137254902f,  0.411764705882353f,  0.415686274509804f,  
  0.419607843137255f,  0.423529411764706f,  0.427450980392157f,  0.431372549019608f,  0.435294117647059f,  0.43921568627451f,  
  0.443137254901961f,  0.447058823529412f,  0.450980392156863f,  0.454901960784314f,  0.458823529411765f,  0.462745098039216f,  
  0.466666666666667f,  0.470588235294118f,  0.474509803921569f,  0.47843137254902f,   0.482352941176471f,  0.486274509803922f,  
  0.490196078431373f,  0.494117647058824f,  0.498039215686275f,  0.501960784313725f,  0.505882352941176f,  0.509803921568627f,  
  0.513725490196078f,  0.517647058823529f,  0.52156862745098f,   0.525490196078431f,  0.529411764705882f,  0.533333333333333f,  
  0.537254901960784f,  0.541176470588235f,  0.545098039215686f,  0.549019607843137f,  0.552941176470588f,  0.556862745098039f,  
  0.56078431372549f,   0.564705882352941f,  0.568627450980392f,  0.572549019607843f,  0.576470588235294f,  0.580392156862745f,  
  0.584313725490196f,  0.588235294117647f,  0.592156862745098f,  0.596078431372549f,  0.6f,                0.603921568627451f,  
  0.607843137254902f,  0.611764705882353f,  0.615686274509804f,  0.619607843137255f,  0.623529411764706f,  0.627450980392157f,  
  0.631372549019608f,  0.635294117647059f,  0.63921568627451f,   0.643137254901961f,  0.647058823529412f,  0.650980392156863f,  
  0.654901960784314f,  0.658823529411765f,  0.662745098039216f,  0.666666666666667f,  0.670588235294118f,  0.674509803921569f,  
  0.67843137254902f,   0.682352941176471f,  0.686274509803922f,  0.690196078431373f,  0.694117647058824f,  0.698039215686274f,  
  0.701960784313725f,  0.705882352941177f,  0.709803921568627f,  0.713725490196078f,  0.717647058823529f,  0.72156862745098f,  
  0.725490196078431f,  0.729411764705882f,  0.733333333333333f,  0.737254901960784f,  0.741176470588235f,  0.745098039215686f,  
  0.749019607843137f,  0.752941176470588f,  0.756862745098039f,  0.76078431372549f,   0.764705882352941f,  0.768627450980392f,  
  0.772549019607843f,  0.776470588235294f,  0.780392156862745f,  0.784313725490196f,  0.788235294117647f,  0.792156862745098f,  
  0.796078431372549f,  0.8f,                0.803921568627451f,  0.807843137254902f,  0.811764705882353f,  0.815686274509804f,  
  0.819607843137255f,  0.823529411764706f,  0.827450980392157f,  0.831372549019608f,  0.835294117647059f,  
  0.83921568627451f,   0.843137254901961f,  0.847058823529412f,  0.850980392156863f,  0.854901960784314f,  0.858823529411765f,  
  0.862745098039216f,  0.866666666666667f,  0.870588235294118f,  0.874509803921569f,  0.87843137254902f,   0.882352941176471f,  
  0.886274509803922f,  0.890196078431373f,  0.894117647058824f,  0.898039215686275f,  0.901960784313726f,  0.905882352941176f,  
  0.909803921568627f,  0.913725490196078f,  0.917647058823529f,  0.92156862745098f,   0.925490196078431f,  0.929411764705882f, 
  0.933333333333333f,  0.937254901960784f,  0.941176470588235f,  0.945098039215686f,  0.949019607843137f,  0.952941176470588f,  
  0.956862745098039f,  0.96078431372549f,   0.964705882352941f,  0.968627450980392f,  0.972549019607843f,  0.976470588235294f,  
  0.980392156862745f,  0.984313725490196f,  0.988235294117647f,  0.992156862745098f,  0.996078431372549f,  1.0f
};



float4 RGBtoHSL(float r, float g, float b)
{
    // r,b and b are assumed to be in the range 0...1
    float luminance = mad(r, 0.299f, mad(g, 0.587f, b * 0.114f));
    float u = -r * 0.1471376975169300226f - g * 0.2888623024830699774f + b * 0.436f;
    float v = r * 0.615f - g * 0.514985734664764622f - b * 0.100014265335235378f;
    float hue = atan2(v, u);
    float saturation = native_sqrt(mad(u, u, v * v));

    return (float4)(hue, saturation, luminance, 0.0f);
}

private float4 HSLtoRGB(float hue, float saturation, float luminance)
{
    // hue is an angle in radians (-Pi...Pi)
    // for saturation the range 0...1/sqrt(2) equals 0% ... 100%
    // luminance is in the range 0...1
    float u = native_cos(hue) * saturation;
    float v = native_sin(hue) * saturation;
    float r = luminance + 1.139837398373983740f * v;
    float g = luminance - 0.3946517043589703515f * u - 0.5805986066674976801f * v;
    float b = luminance + 2.03211091743119266f * u;

    return (float4)(r, g, b, 1.0f);
}

__kernel void ComputeHue (__read_only  image2d_t bmp,
                          __write_only image2d_t bmpHue,
                          __write_only image2d_t bmpSat,
                          __write_only image2d_t bmpLum)

{ 
  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

   int2 coords = (int2)(get_global_id(0), get_global_id(1));
   uint4 pix = read_imageui(bmp, smp, coords);
   int R,G,B;

   B = (int)pix.x;
   G = (int)pix.y;
   R = (int)pix.z;

   float4 hsl = RGBtoHSL(DivBy255[R], DivBy255[G], DivBy255[B]);

   hsl.x = clamp((hsl.x+3.1415926f)*40.5845f,0.0f,255.0f);
   //hsl.x = hsl.x > 190.0f && hsl.x < 220.0f ? 0.0f : 255.0f;
   pix = (uint4)((uint)hsl.x, (uint)hsl.x,(uint)hsl.x,(uint)255);
   write_imageui(bmpHue, coords, pix);

   hsl.y = clamp(hsl.y*360.62445f,0.0f,255.0f);

   pix = (uint4)((uint)hsl.y, (uint)hsl.y,(uint)hsl.y,(uint)255);
   write_imageui(bmpSat, coords, pix);

   hsl.z = clamp(hsl.z*255.0f,0.0f,255.0f);
   pix = (uint4)((uint)hsl.z, (uint)hsl.z,(uint)hsl.z,(uint)255);
   write_imageui(bmpLum, coords, pix);

}



__kernel void ComputeHistogram (__read_only image2d_t bmp,
                                __global float * partHist, 
                                __constant int * imgHeight,
                                __constant int * NLevels)

{
  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

  int x = get_global_id(0);                      
  
  int h = imgHeight[0];
  //Intensity levels
  int N = NLevels[0];

  int2 coord = (int2)(x, 0);
  uint4 pix;
  int R,G,B;
  float4 hsl;  

  int localHist[1024];
  for (int i = 0; i < N; i++) localHist[i] = 0.0f;
  
  for (int y = 0; y < h; y++)
  {
     coord.y = y;
     pix = read_imageui(bmp, smp, coord);
     
     B = (int)pix.x;
     G = (int)pix.y;
     R = (int)pix.z;

     hsl = RGBtoHSL(DivBy255[R], DivBy255[G], DivBy255[B]);
     localHist[(int)((N - 1) * hsl.z)]++;
  }
  
  for (int i = 0; i < N; i++) partHist[i + N*x] = (float)localHist[i];
}

__kernel void ComputeHistogramY(__read_only image2d_t bmp,
                                __global float * partHist, 
                                __constant int * imgWidth,
                                __constant int * NLevels)

{
  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

  int y = get_global_id(0);                      
  
  int w = imgWidth[0];
  //Intensity levels
  int N = NLevels[0];

  int2 coord = (int2)(0, y);
  uint4 pix;
  float R,G,B;
  
  float localHist[1024];
  for (int i = 0; i < N; i++) localHist[i] = 0.0f;
  
  for (int x = 0; x < w; x++)
  {
     coord.x = x;
     pix = read_imageui(bmp, smp, coord);
     
     int B = (float)pix.x;
     int G = (float)pix.y;
     int R = (float)pix.z;

     float4 hsl = RGBtoHSL(DivBy255[R], DivBy255[G], DivBy255[B]);
     localHist[(int)((N - 1) * hsl.z)] += 1.0f;
  }
  
  for (int i = 0; i < N; i++) partHist[i + N*y] = localHist[i];
}

kernel void ConsolidateHist(__global const float* PartialHistograms,
                            __global       float* Histograms,
                            __constant      int* Height,
                            __constant      int* NLevels)

{
   int N = NLevels[0];
   int h = Height[0];
   int i = get_global_id(0);
   
   float val = 0;
   int yN = 0;
   for (int y = 0; y < h; y++)
   {
      val += PartialHistograms[i + yN];
      yN += N;
   }
   
   Histograms[i] = val;
}

__kernel void PerformNormalization(read_only  image2d_t   bmp,
                                   write_only image2d_t   bmpNew,
                                   __global const float * histLuminance,
                                   __constant     int *   NLevels)
{

  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

  int N = NLevels[0];
                      
  int2 coord = (int2)(get_global_id(0), get_global_id(1));
  uint4 pix = read_imageui(bmp, smp, coord);

  float B = (float)pix.x;
  float G = (float)pix.y;
  float R = (float)pix.z;

  float4 hsl = RGBtoHSL(R * 0.00392156862745098f, G * 0.00392156862745098f, B * 0.00392156862745098f);
  
  hsl.z = histLuminance[(int)((N - 1) * hsl.z)];

  float4 rgb = HSLtoRGB(hsl.x, hsl.y, hsl.z);

  rgb = clamp(rgb, 0.0f, 1.0f);

  rgb *= 255.0f;

  pix = (uint4)((uint)rgb.z, (uint)rgb.y, (uint)rgb.x, (uint)rgb.w);
  
  write_imageui(bmpNew, coord, pix);
}
";

            public string srcSemOptim = @"
float4 RGBtoHSL(float r, float g, float b)
{
    // r,b and b are assumed to be in the range 0...1
    float luminance = r * 0.299f + g * 0.587f + b * 0.114f;
    float u = -r * 0.1471376975169300226f - g * 0.2888623024830699774f + b * 0.436f;
    float v = r * 0.615f - g * 0.514985734664764622f - b * 0.100014265335235378f;
    float hue = atan2(v, u);
    float saturation = sqrt(u * u + v * v);

    return (float4)(hue, saturation, luminance, 0.0f);
}

private float4 HSLtoRGB(float hue, float saturation, float luminance)
{
    // hue is an angle in radians (-Pi...Pi)
    // for saturation the range 0...1/sqrt(2) equals 0% ... 100%
    // luminance is in the range 0...1
    float u = cos(hue) * saturation;
    float v = sin(hue) * saturation;
    float r = luminance + 1.139837398373983740f * v;
    float g = luminance - 0.3946517043589703515f * u - 0.5805986066674976801f * v;
    float b = luminance + 2.03211091743119266f * u;

    return (float4)(r, g, b, 1.0f);
}

__kernel void ComputeHistogram(__read_only image2d_t bmp,
                               __global float * partHist, 
                               __constant int * imgWidth,
                               __constant int * NLevels)

{
  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

  int y = get_global_id(0);                      
  
  int w = imgWidth[0];
  //Intensity levels
  int N = NLevels[0];

  int2 coord = (int2)(0, y);
  uint4 pix;
  float R,G,B;
  
  float localHist[1024];
  for (int i = 0; i < N; i++) localHist[i] = 0.0f;
  
  for (int x = 0; x < w; x++)
  {
     coord.x = x;
     pix = read_imageui(bmp, smp, coord);
     
     float B = (float)pix.x;
     float G = (float)pix.y;
     float R = (float)pix.z;

     float4 hsl = RGBtoHSL(R * 0.00392156862745098f, G * 0.00392156862745098f, B * 0.00392156862745098f);
     localHist[(int)((N - 1) * hsl.z)] += 1.0f;
  }
  
    for (int i = 0; i < N; i++) partHist[i + N*y] = localHist[i];
}

kernel void ConsolidateHist(__global const float* PartialHistograms,
                            __global       float* Histograms,
                            __constant      int* Height,
                            __constant      int* NLevels)

{
   int N = NLevels[0];
   int h = Height[0];
   int i = get_global_id(0);
   
   float val = 0;
   int yN = 0;
   for (int y = 0; y < h; y++)
   {
      val += PartialHistograms[i + yN];
      yN += N;
   }
   
   Histograms[i] = val;
}

__kernel void PerformNormalization(read_only  image2d_t   bmp,
                                   write_only image2d_t   bmpNew,
                                   __global const float * histLuminance,
                                   __constant     int *   NLevels)
{

  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | //Natural coordinates
                      CLK_ADDRESS_CLAMP | //Clamp to zeros
                      CLK_FILTER_NEAREST; //Don't interpolate

  int N = NLevels[0];
                      
  int2 coord = (int2)(get_global_id(0), get_global_id(1));
  uint4 pix = read_imageui(bmp, smp, coord);

  float B = (float)pix.x;
  float G = (float)pix.y;
  float R = (float)pix.z;

  float4 hsl = RGBtoHSL(R * 0.00392156862745098f, G * 0.00392156862745098f, B * 0.00392156862745098f);
  
  hsl.z = histLuminance[(int)((N - 1) * hsl.z)];

  float4 rgb = HSLtoRGB(hsl.x, hsl.y, hsl.z);

  rgb = clamp(rgb, 0.0f, 1.0f);


  rgb *= 255.0f;

  pix = (uint4)((uint)rgb.z, (uint)rgb.y, (uint)rgb.x, (uint)rgb.w);
  
  write_imageui(bmpNew, coord, pix);
}
";
        }



        #endregion

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //this.Text = e.X.ToString() + " " + e.Y.ToString();
            try
            {
                Color c = ((Bitmap)pictureBox1.Image).GetPixel(e.X, e.Y);
                this.Text = c.R.ToString();
            }
            catch { }
        }







    }
}
