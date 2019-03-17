using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Vangaorth.EquiToCube
{

    /// <summary>
    /// C# conversion of Equirectangular image to cubemap algorithm on
    /// https://stackoverflow.com/questions/29678510/convert-21-equirectangular-panorama-to-cube-map
    /// </summary>
    public class EquirectangularToCubeMapConverter
    {
        public virtual void Convert(string inImagePath, string outPath)
        {
            using (var imgIn = Image.Load<Rgb24>(inImagePath))
            {
                var inSize = imgIn.Size();
                var edge = (int)(0.25f * inSize.Width);
                for (var face = Face.Back; face <= Face.Bottom; face++)
                {
                    var faceString = face.ToFriendlyString();

                    Console.WriteLine($"Processing {faceString}");

                    using (var imgOut = new Image<Rgb24>(Configuration.Default, edge, edge, new Rgb24(0, 0, 0)))
                    {
                        ConvertFace(face, imgIn, imgOut);

                        var outFilePath = Path.Combine(outPath, $"{faceString}.jpg");
                        imgOut.Save(outFilePath);
                    }
                }
            }
        }

        protected virtual void ConvertFace(Face face, Image<Rgb24> imgIn, Image<Rgb24> imgOut)
        {
            var inSize = imgIn.Size();

            // The length of each edge in pixels
            var edge = 0.25f * inSize.Width;

            // Back face
            var iStart = 0;
            var iEnd = edge;
            switch (face)
            {
                case Face.Left:
                    iStart = (int)edge;
                    iEnd = (int)(2.0f * edge);
                    break;
                case Face.Front:
                case Face.Top:
                case Face.Bottom:
                    iStart = (int)(2.0f * edge);
                    iEnd = (int)(3.0f * edge);
                    break;
                case Face.Right:
                    iStart = (int)(3.0f * edge);
                    iEnd = inSize.Width;
                    break;
            }

            for (var i = iStart; i < iEnd; i++)
            {
                // Back, Left, Front and Right faces
                var jStart = (int)edge;
                var jEnd = (int)(2.0f * edge);
                switch (face)
                {
                    case Face.Top:
                        jStart = 0;
                        jEnd = (int)edge;
                        break;
                    case Face.Bottom:
                        jStart = (int)(2.0f * edge);
                        jEnd = (int)(3.0f * edge);
                        break;
                }

                for (var j = jStart; j < jEnd; j++)
                {
                    var xyz = OutImgToXYZ(i, j, face, edge);

                    var theta = MathF.Atan2(xyz.Y, xyz.X);  // Range -pi to pi
                    var r = MathF.Sqrt(xyz.X * xyz.X + xyz.Y * xyz.Y);
                    var phi = MathF.Atan2(xyz.Z, r);    // Range -pi/2 to pi/2

                    // Source img coords
                    var uf = 2.0f * edge * (theta + MathF.PI) / MathF.PI;
                    var vf = 2.0f * edge * (0.5f * MathF.PI - phi) / MathF.PI;

                    // Use bilinear interpolation between the four surrounding pixels
                    var ui = MathF.Floor(uf);
                    var vi = MathF.Floor(vf);
                    var u2 = ui + 1.0f;
                    var v2 = vi + 1.0f;
                    var mu = uf - ui;
                    var nu = vf - vi;

                    // Pixel values of four corners
                    var a = imgIn[(int)(ui % inSize.Width), (int)Math.Clamp(vi, 0, inSize.Height - 1)];
                    var b = imgIn[(int)(u2 % inSize.Width), (int)Math.Clamp(vi, 0, inSize.Height - 1)];
                    var c = imgIn[(int)(ui % inSize.Width), (int)Math.Clamp(v2, 0, inSize.Height - 1)];
                    var d = imgIn[(int)(u2 % inSize.Width), (int)Math.Clamp(v2, 0, inSize.Height - 1)];

                    // Interpolate
                    var rgb = new Triple<float>(
                        a.R * (1 - mu) * (1 - nu) + b.R * mu * (1 - nu) + c.R * (1 - mu) * nu + d.R * mu * nu,
                        a.G * (1 - mu) * (1 - nu) + b.G * mu * (1 - nu) + c.G * (1 - mu) * nu + d.G * mu * nu,
                        a.B * (1 - mu) * (1 - nu) + b.B * mu * (1 - nu) + c.B * (1 - mu) * nu + d.B * mu * nu);

                    imgOut[(int)(i % edge), (int)(j % edge)] = new Rgb24((byte)MathF.Round(rgb.X), (byte)MathF.Round(rgb.Y), (byte)MathF.Round(rgb.Z));
                }
            }
        }


        protected virtual Triple<float> OutImgToXYZ(int i, int j, Face face, float edge)
        {
            var a = 2.0f * i / edge;
            var b = 2.0f * j / edge;

            switch (face)
            {
                case Face.Back:
                    return new Triple<float>(-1.0f, 1.0f - a, 3.0f - b);
                case Face.Left:
                    return new Triple<float>(a - 3.0f, -1.0f, 3.0f - b);
                case Face.Front:
                    return new Triple<float>(1.0f, a - 5.0f, 3.0f - b);
                case Face.Right:
                    return new Triple<float>(7.0f - a, 1.0f, 3.0f - b);
                case Face.Top:
                    return new Triple<float>(b - 1.0f, a - 5.0f, 1.0f);
                case Face.Bottom:
                    return new Triple<float>(5.0f - b, a - 5.0f, -1.0f);
            }

            throw new Exception($"Unexpected {nameof(face)} value ({face})");
        }
    }
}
