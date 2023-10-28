using System.Collections.ObjectModel;
using System.Text;

namespace CryptoMethods.Sha256;

public static class Util
{
    public static string ArrayToString(ReadOnlyCollection<byte> arr)
    {
        var s = new StringBuilder(arr.Count * 2);
        foreach (var t in arr)
        {
            s.Append($"{t:x2}");
        }

        return s.ToString();
    }
}