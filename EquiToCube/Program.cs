using System;

namespace Vangaorth.EquiToCube
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Starting");

            var converter = new EquirectangularToCubeMapConverter();
            converter.Convert("Input/equi00.jpg", "Output");
            
            Console.WriteLine($"End");
        }
    }
}