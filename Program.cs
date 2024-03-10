using System.Text;

public static class HammingAlgm
{
    private static int GetCountOfControlBits(int m)
    {
        for (int p = 1; ; p++)
        {
            if ((1 << p) >= (m + p + 1))
            {
                return p;
            }
        }
    }

    private static List<int> GetPositionsForContolBitCalculation(int p, int l)
    {
        var array = new List<int>();
        int maxK = ((l + 1) / (2 * p)) + 1;
        for (int k = 1; k <= maxK; k++)
        {
            int _2k1 = (2 * k) - 1;
            for (int t = _2k1 * p; t < (_2k1 + 1) * p && t <= l; t++)
            {
                array.Add(t - 1);
            }
        }

        return array;
    }

    private static string GetBinary(int m)
    {
        int[] bin = new int[8];
        int binLen = bin.Length;

        for (int i = 0; i < binLen; i++)
        {
            int degOfTwo = 1 << (binLen - i - 1);
            if (m >= degOfTwo)
            {
                m -= degOfTwo;
                bin[i] = 1;
                if (m == 0)
                {
                    return string.Join("", bin);
                }
            }
        }

        return string.Join("", bin);
    }

    public static string EncodeASCII(string source)
    {
        string sourceASCII = string.Join("", Encoding.ASCII.GetBytes(source).Select(x => GetBinary(x)));

        return Encode(sourceASCII);
    }

    private static byte[] GetByteArray(string bin)
    {
        byte[] result = new byte[bin.Length / 8];

        for (int i = 0; i < bin.Length / 8; i++)
        {
            result[i] = Convert.ToByte(bin.Substring(i * 8, 8), 2);
        }

        return result;
    }

    public static string DecodeASCII(string source)
    {
        var restored = Decode(source);

        return Encoding.ASCII.GetString(GetByteArray(restored));
    }

    public static string Encode(string source)
    {
        int cntOfContolBits = GetCountOfControlBits(source.Length);
        int dataLen = source.Length + cntOfContolBits;

        int[] DataArray = new int[dataLen];

        for (int i = dataLen - 1, j = source.Length - 1; i > 1; i--)
        {
            if ((i + 1) > 0 && (((i + 1) & i) == 0))
            {
                continue;
            }

            DataArray[i] = source[j--] - '0';
            if (DataArray[i] is not 0 and not 1)
            {
                throw new ArgumentException("The number must consist of 0 and 1");
            }
        }

        for (int i = 0; i < cntOfContolBits; i++)
        {
            int contolBit = (1 << i) - 1;
            var positions = GetPositionsForContolBitCalculation(1 << i, dataLen);
            DataArray[contolBit] = positions.Select(p => DataArray[p]).Sum() % 2;
        }

        return string.Join("", DataArray);
    }

    public static string Decode(string encodedWithOneError)
    {
        int dataLen = encodedWithOneError.Length;
        int cntOfContolBits = GetCountOfControlBits(dataLen);
        var DataArray = new int[dataLen];
        for (int i = 0; i < dataLen; i++)
        {
            DataArray[i] = encodedWithOneError[i] - '0';

            if (DataArray[i] is not 0 and not 1)
            {
                throw new ArgumentException("The number must consist of 0 and 1");
            }
        }

        int brakePositions = 0;
        for (int i = 0; i < cntOfContolBits; i++)
        { 
            var positions = GetPositionsForContolBitCalculation(1 << i, dataLen);

            if (positions.Select(p => DataArray[p]).Sum() % 2 == 1)
            {
                brakePositions += 1 << i;
            }
        }

        StringBuilder encoded = new StringBuilder(encodedWithOneError);
        if (brakePositions != 0)
        {
            encoded[brakePositions - 1] = encoded[brakePositions - 1] == '0' ? '1' : '0';
        }

        StringBuilder result = new StringBuilder(string.Empty);
        for (int i = 0; i < dataLen; i++)
        {
            if ((i + 1) > 0 && (((i + 1) & i) == 0))
            {
                continue;
            }

            result.Append(encoded[i]);
        }

        return result.ToString();
    }
}

class Program
{
    static void Main()
    {
        var encodedContentFilePath = "content.txt";
        var content = File.ReadAllText(encodedContentFilePath);

        string encoded = HammingAlgm.EncodeASCII(content);
        string restored = HammingAlgm.DecodeASCII(encoded);

        Console.WriteLine("Source: " + content);
        Console.WriteLine("Encoded: " + encoded);
        Console.WriteLine("Restored: " + restored);
    }
}
