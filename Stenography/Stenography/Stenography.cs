using System.Drawing;
using System.Text;

namespace Stenography;

public static  class Stenography
{
    public static Bitmap CodeText(string text, Bitmap bmp)
    {
        var length = text.Length;
        var idx = 0;
        
        text = text.ToLower();
        bmp = EncodeLength(bmp, length);
        
        for (var i = 0; i < bmp.Width; i++)
        {
            for (var j = 0; j < bmp.Height; j++)
            {
                if (i == 0 && j is 0 or 1 or 2 or 3) 
                {
                    continue;
                }
                if (idx == length)
                {
                    return bmp;
                }
                var pixel = bmp.GetPixel(i, j);
                bmp.SetPixel(i, j, Encode(text[idx], pixel));
                
                idx++;
            }
        }

        return bmp;
    }

    static Color Encode(char symbol, Color pixel)
    {
        var red = (byte) (pixel.R & 0xFC);
        var green = (byte) (pixel.G & 0xF8);
        var blue = (byte) (pixel.B & 0xF8);
        
        red += (byte) (symbol & 3);
        green += (byte) ((symbol & 28) >> 2);
        blue += (byte) ((symbol & 224) >> 5);
        
        var color = Color.FromArgb(red, green, blue);
        
        return color;
    }

    private static Bitmap EncodeLength(Bitmap bmp, int length)
    {
        var a = 255;
        var part = 0;
        part = length & a;
        
        for (var y = 0; y < 4; y++)
        {
            var color = bmp.GetPixel(0, y);
            var newColor = Encode((char) part, color);
            
            bmp.SetPixel(0, y, newColor);
            length >>= 8;
            part = length & a;
        }

        return bmp;
    }
    
    private static char Decode(Color pixel)
    {
        var red = (byte) (pixel.R & 0x3);
        var green = (byte) (pixel.G & 0x7);
        var blue = (byte) (pixel.B & 0x7);
        
        var a = (byte) ((((blue << 3) + green) << 2) + red);
        
        return (char) a;
    }

    private static int DecodeLength(Bitmap bmp)
    {
        var part = 0;
        var interval = 0;
        for (var y = 0; y < 4; y++)
        {
            var color = bmp.GetPixel(0, y);
            int a = Decode(color);
            
            part = (a << interval) + part;
            interval += 8;
        }

        return part;
    }

    public static string DecodeText(Bitmap bmp)
    {
        var idx = 0;
        var length = DecodeLength(bmp);
        var stringBuilder = new StringBuilder();
        
        for (var i = 0; i < bmp.Width; i++)
        {
            for (var j = 0; j < bmp.Height; j++)
            {
                if (i == 0 && j is 0 or 1 or 2 or 3)
                {
                    continue;
                }

                if (idx == length)
                {
                    return stringBuilder.ToString();
                }
                
                var color = bmp.GetPixel(i, j);
                
                stringBuilder.Append(Decode(color));
                idx++;
            }
        }

        return stringBuilder.ToString();
    }
}