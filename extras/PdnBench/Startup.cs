#define EFFECTS
#define RESIZE
#define GRADIENT
#define COMPOSITION
#define TRANSFORM
#define BLIT

using Microsoft.Win32;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PdnBench
{
    class Startup
    {
        // various benchmark wide settings
        static bool useTsvOutput = false;
        const string defaultImageName = "PdnBench.cat.jpg";
        static string benchmarkImageName = defaultImageName;

        static void PrintHelp()
        {
            Console.WriteLine("PdnBench command line arguments:");
            Console.WriteLine("    /?             : show this help");
            Console.WriteLine("    /image <file>  : use an alternate image for benchmarking");
            Console.WriteLine("    /proc <N>      : set number of 'logical' processors to use");
            Console.WriteLine("    /tsv           : output in tab-separated-value format, easy to import into excel");
            Console.WriteLine();
        }

        static int GetCpuSpeed()
        {
            int mhz = -1;
            string keyName = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            string valueName = @"~MHz";

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(valueName);
                        mhz = (int)value;
                    }
                }
            }

            catch (Exception)
            {
                mhz = -1;
            }

            return mhz;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/proc":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("Use /proc <N> to specify number of processors");
                            return;
                        }

                        int numProcs;

                        if (Int32.TryParse(args[i + 1], out numProcs))
                        {
                            // only increment i if successful b/c we're going to continue the run
                            // with the default # of processors and don't want to automatically 
                            // eat the next parameter.
                            ++i;
                            Processor.LogicalCpuCount = numProcs;
                        }
                        else
                        {
                            Console.WriteLine("You must specify a integer for /proc <N>, continuing with default");
                        }
                        break;

                    case "/image":
                        if (i + 1 == args.Length)
                        {
                            Console.WriteLine("Use /image <filename> to specify a file to perform benchmark with");
                            return;
                        }

                        ++i;
                        benchmarkImageName = args[i];

                        if (!System.IO.File.Exists(benchmarkImageName))
                        {
                            Console.WriteLine("Specified image doesn't exist");
                            return;
                        }
                        break;

                    case "/tsv":
                        useTsvOutput = true;
                        break;

                    case "/?":
                        PrintHelp();
                        return;

                    default:
                        break;
                }
            }

            //Processor.LogicalCpuCount = 1;
            Console.WriteLine("PdnBench v" + PdnInfo.GetVersion());
            Console.WriteLine("Running in " + (8 * Marshal.SizeOf(typeof(IntPtr))) + "-bit mode on Windows " +
                Environment.OSVersion.Version.ToString() + " " +
                OS.Revision + (OS.Revision.Length > 0 ? " " : string.Empty) +
                OS.Type + " " +
                Processor.NativeArchitecture.ToString().ToLower());

            Console.WriteLine("Processor: " + Processor.LogicalCpuCount + "x \"" + Processor.CpuName + "\" @ ~" + GetCpuSpeed() + " MHz");
            Console.WriteLine("Memory: " + ((Memory.TotalPhysicalBytes / 1024) / 1024) + " MB");
            Console.WriteLine();

            Console.WriteLine("Using " + Processor.LogicalCpuCount + " threads.");

            ArrayList benchmarks = new ArrayList();

            Document document;

            Console.Write("Loading image ... ");

            Stream imageStream = null;
            try
            {
                imageStream = (defaultImageName == benchmarkImageName) ?
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(benchmarkImageName) :
                    new FileStream(benchmarkImageName, FileMode.Open);

                JpegFileType jft = new JpegFileType();
                document = jft.Load(imageStream);
            }

            finally
            {
                if (imageStream != null)
                {
                    imageStream.Dispose();
                }
            }

            Console.WriteLine("(" + document.Width + " x " + document.Height + ") done");

            Surface surface = ((BitmapLayer)document.Layers[0]).Surface;

            Surface dst = new Surface(surface.Width * 4, surface.Height * 4);

#if EFFECTS
            for (double i = 0; i < (2 * Math.PI); i += 70.0 * ((2 * Math.PI) / 360.0))
            {
                benchmarks.Add(
                    new EffectBenchmark("Rotate/Zoom at " + ((i * 180.0) / Math.PI).ToString("F2") + " degrees",
                    3,
                    new PaintDotNet.Effects.RotateZoomEffect(),
                    new PaintDotNet.Effects.RotateZoomEffectConfigToken(
                        true,
                        (float)(Math.PI * 0.3f),
                        (float)((Math.PI * -0.4) + i),
                        50,
                        0.5f,
                        new PointF(-0.2f, 0.3f),
                        false,
                        true),
                        surface));
            }

            for (int i = 1; i <= 4; i += 3)
            {
                for (int j = 10; j < 100; j += 75)
                {
                    OilPaintingEffect e = new OilPaintingEffect();
                    PropertyCollection props = e.CreatePropertyCollection();
                    props[OilPaintingEffect.PropertyNames.BrushSize].Value = i;
                    props[OilPaintingEffect.PropertyNames.Coarseness].Value = j;

                    benchmarks.Add(
                        new EffectBenchmark(
                            "Oil Painting, brush size = " + i + ", coarseness = " + j,
                            1,
                            e,
                            new PropertyBasedEffectConfigToken(props),
                            surface));
                }
            }

            for (int i = 2; i <= 200; i += i)
            {
                GaussianBlurEffect e = new GaussianBlurEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[GaussianBlurEffect.PropertyNames.Radius].Value = i;

                benchmarks.Add(
                    new EffectBenchmark(
                        "Gaussian Blur with radius of " + i,
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            for (int i = 1; i <= 4; i += 3)
            {
                SharpenEffect e = new SharpenEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[SharpenEffect.PropertyNames.Amount].Value = i;

                benchmarks.Add(
                    new EffectBenchmark(
                        "Sharpen with value of " + i,
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            for (int i = 81; i >= 5; i /= 3)
            {
                CloudsEffect e = new CloudsEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props["Scale"].Value = 50;
                props["Power"].Value = (double)i / 100.0;
                props["Seed"].Value = 12345 % 255;
                props["BlendOp"].Value = typeof(UserBlendOps.NormalBlendOp);

                benchmarks.Add(
                    new EffectBenchmark(
                        "Clouds, roughness = " + i,
                        2,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            for (int i = 4; i <= 64; i *= 4)
            {
                MedianEffect e = new MedianEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[MedianEffect.PropertyNames.Radius].Value = i;
                props[MedianEffect.PropertyNames.Percentile].Value = 50;

                benchmarks.Add(
                    new EffectBenchmark(
                        "Median, radius " + i,
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            for (int i = 4; i <= 64; i *= 4)
            {
                UnfocusEffect e = new UnfocusEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[UnfocusEffect.PropertyNames.Radius].Value = i;

                benchmarks.Add(
                   new EffectBenchmark(
                       "Unfocus, radius " + i,
                       1,
                       e,
                       new PropertyBasedEffectConfigToken(props),
                       surface));
            }

            {
                MotionBlurEffect e = new MotionBlurEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[MotionBlurEffect.PropertyNames.Angle].Value = 0.0;
                props[MotionBlurEffect.PropertyNames.Distance].Value = 15;
                props[MotionBlurEffect.PropertyNames.Centered].Value = true;

                benchmarks.Add(
                    new EffectBenchmark(
                        "Motion Blur, Horizontal",
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            {
                MotionBlurEffect e = new MotionBlurEffect();
                PropertyCollection props = e.CreatePropertyCollection();
                props[MotionBlurEffect.PropertyNames.Angle].Value = 90.0;
                props[MotionBlurEffect.PropertyNames.Distance].Value = 15;
                props[MotionBlurEffect.PropertyNames.Centered].Value = true;

                benchmarks.Add(
                    new EffectBenchmark(
                        "Motion Blur, Vertical",
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            {
                ReduceNoiseEffect e = new ReduceNoiseEffect();
                PropertyCollection props = e.CreatePropertyCollection();

                benchmarks.Add(
                    new EffectBenchmark(
                        "Reduce Noise (3x)",
                        3,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        dst));
            }

            {
                MandelbrotFractalEffect e = new MandelbrotFractalEffect();
                PropertyCollection props = e.CreatePropertyCollection();

                benchmarks.Add(
                    new EffectBenchmark(
                        "Mandelbrot Fractal",
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

            {
                JuliaFractalEffect e = new JuliaFractalEffect();
                PropertyCollection props = e.CreatePropertyCollection();

                benchmarks.Add(
                    new EffectBenchmark(
                        "Julia Fractal",
                        1,
                        e,
                        new PropertyBasedEffectConfigToken(props),
                        surface));
            }

#endif

#if RESIZE
            // Resize benchmarks
            for (int i = 1; i < 8; i += 2)
            {
                int newWidth = i * (dst.Width / 8);
                int newHeight = i * (dst.Height / 8);

                Surface dstWindow = dst.CreateWindow(new Rectangle(0, 0, newWidth, newHeight));
                benchmarks.Add(new ResizeBenchmark("Resize from " + surface.Width + "x" + surface.Height + " to " + newWidth + "x" + newHeight, surface, dstWindow));
                benchmarks.Add(new ResizeBenchmark("Resize from " + newWidth + "x" + newHeight + " to " + surface.Width + "x" + surface.Height, dstWindow, surface));
            }
#endif

#if GRADIENT
            // Gradient benchmarks
            benchmarks.Add(new GradientBenchmark(
                "Linear reflected gradient @ " + dst.Width + "x" + dst.Height + " (5x)",
                dst,
                new GradientRenderers.LinearReflected(false, new UserBlendOps.NormalBlendOp()),
                2));

            benchmarks.Add(new GradientBenchmark(
                "Conical gradient @ " + dst.Width + "x" + dst.Height + " (5x)",
                dst,
                new GradientRenderers.Conical(false, new UserBlendOps.NormalBlendOp()),
                2));

            benchmarks.Add(new GradientBenchmark(
                "Radial gradient @ " + dst.Width + "x" + dst.Height + " (5x)",
                dst,
                new GradientRenderers.Radial(false, new UserBlendOps.NormalBlendOp()),
                2));
#endif

#if COMPOSITION
            // Composition benchmarks
            Document doc1 = new Document(surface.Size);
            BitmapLayer layer1 = Layer.CreateBackgroundLayer(doc1.Width, doc1.Height);
            layer1.Surface.CopySurface(surface);
            doc1.Layers.Add(layer1);
            doc1.Layers.Add(layer1.Clone());
            doc1.Layers.Add(layer1.Clone());
            doc1.Layers.Add(layer1.Clone());

            benchmarks.Add(new CompositionBenchmark("Compositing one layer, Normal blend mode, 255 opacity (" + CompositionBenchmark.Iterations + "x)",
                doc1,
                surface,
                delegate(int layerIndex, Layer layer)
                {
                    if (layerIndex == 0)
                    {
                        layer.Visible = true;
                        layer.Opacity = 255;
                        ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                    }
                    else
                    {
                        layer.Visible = false;
                    }
                }));

            benchmarks.Add(new CompositionBenchmark("Compositing one layer, Normal blend mode, 128 opacity (" + CompositionBenchmark.Iterations + "x)",
                doc1,
                surface,
                delegate(int layerIndex, Layer layer)
                {
                    if (layerIndex == 0)
                    {
                        layer.Visible = true;
                        layer.Opacity = 128;
                        ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                    }
                    else
                    {
                        layer.Visible = false;
                    }
                }));

            benchmarks.Add(new CompositionBenchmark("Compositing four layers, Normal blend mode, 255 opacity (" + CompositionBenchmark.Iterations + "x)",
                doc1,
                surface,
                delegate(int layerIndex, Layer layer)
                {
                    layer.Visible = true;
                    layer.Opacity = 255;
                    ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                }));

            benchmarks.Add(new CompositionBenchmark("Compositing four layers, Normal blend mode, 255 (layer 0) and 128 (layer 1-3) opacity (" + CompositionBenchmark.Iterations + "x)", doc1, surface,
                delegate(int layerIndex, Layer layer)
                {
                    layer.Visible = true;
                    layer.Opacity = 128;
                    ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                }));

            benchmarks.Add(new CompositionBenchmark("Compositing four layers, Normal blend mode, 128 opacity (" + CompositionBenchmark.Iterations + "x)", doc1, surface,
                delegate(int layerIndex, Layer layer)
                {
                    layer.Visible = true;
                    layer.Opacity = 128;
                    ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                }));

            benchmarks.Add(new CompositionBenchmark("Compositing three layers, Normal+Multiply+Overlay blending, 150+255+170 opacity (" + CompositionBenchmark.Iterations + "x)", doc1, surface,
                delegate(int layerIndex, Layer layer)
                {
                    if (layerIndex == 0)
                    {
                        layer.Visible = true;
                        layer.Opacity = 150;
                        ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.NormalBlendOp());
                    }
                    else if (layerIndex == 1)
                    {
                        layer.Visible = true;
                        layer.Opacity = 255;
                        ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.MultiplyBlendOp());
                    }
                    else if (layerIndex == 2)
                    {
                        layer.Visible = true;
                        layer.Opacity = 170;
                        ((BitmapLayer)layer).SetBlendOp(new UserBlendOps.OverlayBlendOp());
                    }
                    else
                    {
                        layer.Visible = false;
                    }
                }));
#endif

#if TRANSFORM
            // Transform benchmarks
            Matrix m = new Matrix();
            m.Reset();

            MaskedSurface msSimple = new MaskedSurface(surface, new PdnRegion(surface.Bounds)); // simple masked surface

            PdnRegion complexRegion = new PdnRegion(surface.Bounds);

            // cut 4 holes in region 1 to form a complex clipping surface
            for (int x = -1; x < 3; ++x)
            {
                for (int y = -1; y < 3; ++y)
                {
                    int left = (1 + (x * 3)) * (surface.Width / 6);
                    int top = (1 + (x * 3)) * (surface.Height / 6);
                    int right = (2 + (x * 3)) * (surface.Width / 6);
                    int bottom = (2 + (x * 3)) * (surface.Height / 6);

                    Rectangle rect = Rectangle.FromLTRB(left, top, right, bottom);
                    PdnGraphicsPath path = new PdnGraphicsPath();
                    path.AddEllipse(rect);
                    complexRegion.Exclude(path);
                }
            }

            MaskedSurface msComplex = new MaskedSurface(surface, complexRegion);

            benchmarks.Add(new TransformBenchmark("Transform simple surface, no transform, nearest neighbor resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m,
                false));

            benchmarks.Add(new TransformBenchmark("Transform complex surface, no transform, nearest neighbor resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m,
                false));

            benchmarks.Add(new TransformBenchmark("Transform simple surface, no transform, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m,
                true));

            benchmarks.Add(new TransformBenchmark("Transform complex surface, no transform, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m,
                true));

            Matrix m2 = m.Clone();
            m2.RotateAt(45.0f, new PointF(surface.Width / 2, surface.Height / 2));

            benchmarks.Add(new TransformBenchmark("Transform simple surface, 45 deg. rotation about center, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m2,
                true));

            benchmarks.Add(new TransformBenchmark("Transform complex surface, 45 deg. rotation about center, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m2,
                true));

            Matrix m3 = m.Clone();
            m3.Scale(0.5f, 0.75f);

            benchmarks.Add(new TransformBenchmark("Transform simple surface, 50% x-scaling 75% y-scaling, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m3,
                true));

            benchmarks.Add(new TransformBenchmark("Transform complex surface, 50% x-scaling 75% y-scaling, bilinear resampling (" + TransformBenchmark.Iterations + "x)",
                surface,
                msSimple,
                m3,
                true));
#endif

#if BLIT
            // Blit benchmarks
            benchmarks.Add(new ZoomOutBlitBenchmark("Zoom out, rotated grid multisampling, 66% (" + ZoomOutBlitBenchmark.IterationCount + "x)",
                surface,
                dst,
                new Size((surface.Width * 2) / 3, (surface.Height * 2) / 3)));

            benchmarks.Add(new ZoomOutBlitBenchmark("Zoom out, rotated grid multisampling, 28% (" + ZoomOutBlitBenchmark.IterationCount + "x)",
                surface,
                dst,
                new Size((surface.Width * 28) / 100, (surface.Height * 28) / 100)));

            benchmarks.Add(new ZoomOneToOneBlitBenchmark("Zoom 1:1, straight blit (" + ZoomOneToOneBlitBenchmark.IterationCount + "x)",
                surface,
                dst.CreateWindow(new Rectangle(0, 0, surface.Width, surface.Height))));
#endif

            // Run benchmarks!
            Timing timing = new Timing();
            ulong start = timing.GetTickCount();

            foreach (Benchmark benchmark in benchmarks)
            {
                Console.Write(benchmark.Name + (useTsvOutput ? "\t" : " ... "));
                TimeSpan timeSpan = benchmark.Execute();
                Console.WriteLine(" " + timeSpan.TotalMilliseconds.ToString() + (useTsvOutput ? "\t" : "") + " milliseconds");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            ulong end = timing.GetTickCount();

            Console.WriteLine();
            Console.WriteLine("Total time: " + (useTsvOutput ? "\t" : "") + (end - start).ToString() + (useTsvOutput ? "\t" : "") + " milliseconds");
            Console.WriteLine();
        }
    }
}
