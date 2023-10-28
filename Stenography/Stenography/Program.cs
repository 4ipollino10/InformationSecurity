using System.Drawing;
using System.Drawing.Imaging;

namespace Stenography
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Введите текст, который хотите внедрить в картинку");
            var text = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Плохо, давай еще");
                text = Console.ReadLine();
            }
            
            var bmp = new Bitmap("1.bmp");
            
            var encoded = Stenography.CodeText(text, bmp);
            
            encoded.Save("2.bmp", ImageFormat.Bmp);
            
            var bmpEncoded = new Bitmap("2.bmp");
            var decoded = Stenography.DecodeText(bmpEncoded);
            
            Console.WriteLine("Внедренный в картинку текст: \n" + decoded);

            Console.ReadLine();
        }
    }
}