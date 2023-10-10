namespace CryptoMethods.CryptoMethods;

public class Clefia
{
    private const int Rounds = 18;
    private const int KeySize = 128;

    private readonly uint[] roundKeys = new uint[36];

    public Clefia(byte[] key)
    {
        GenerateRoundKeys(key);
    }

    private void GenerateRoundKeys(byte[] key)
    {
        uint[] wk = new uint[4];
        uint[] rk = new uint[36];

        for (int i = 0; i < 4; i++)
        {
            wk[i] = BitConverter.ToUInt32(key, i * 4);
        }

        rk[35] = 0xB7E15163;
        rk[34] = 0x9AB20037;

        for (int i = 33; i >= 0; i--)
        {
            rk[i] = wk[0] ^ Constants.RotHi(wk[1], 8) ^ (uint)i;
            wk[0] = wk[1];
            wk[1] = wk[2];
            wk[2] = wk[3];
            wk[3] = rk[i];
        }

        for (int i = 0; i < 36; i += 2)
        {
            rk[i] = WK(rk[i], rk[i + 1]);
            rk[i + 1] = Constants.RotLo(rk[i + 1], 9);
        }

        Array.Copy(rk, roundKeys, 36);
    }

    private uint WK(uint x, uint y)
    {
        x ^= y;
        return Constants.T4(x);
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        if (plaintext.Length != 16)
        {
            throw new ArgumentException("Plaintext length must be 16 bytes.");
        }

        uint[] input = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            input[i] = BitConverter.ToUInt32(plaintext, i * 4);
        }

        ProcessBlocks(input, true);

        byte[] ciphertext = new byte[16];
        for (int i = 0; i < 4; i++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(input[i]), 0, ciphertext, i * 4, 4);
        }

        return ciphertext;
    }

    public byte[] Decrypt(byte[] ciphertext)
    {
        if (ciphertext.Length != 16)
        {
            throw new ArgumentException("Ciphertext length must be 16 bytes.");
        }

        uint[] input = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            input[i] = BitConverter.ToUInt32(ciphertext, i * 4);
        }

        ProcessBlocks(input, false);

        byte[] plaintext = new byte[16];
        for (int i = 0; i < 4; i++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(input[i]), 0, plaintext, i * 4, 4);
        }

        return plaintext;
    }

    private void ProcessBlocks(uint[] data, bool encrypt)
    {
        uint[] whiteKeys = new uint[4];
        Array.Copy(data, whiteKeys, 4);

        uint[] roundKeys = new uint[36];
        Array.Copy(this.roundKeys, roundKeys, 36);

        if (!encrypt)
        {
            Array.Reverse(roundKeys);
        }

        for (int i = 0; i <= Rounds; i++)
        {
            if (i % 2 == 0)
            {
                AddRoundKey(data, roundKeys, i);
                SubCells(data);
                ShiftRows(data);
                MixColumns(data);
            }
            else
            {
                AddRoundKey(data, roundKeys, i);
                MixColumns(data);
                ShiftRows(data);
                SubCells(data);
            }
        }

        AddRoundKey(data, whiteKeys, Rounds + 1);
    }

    private void AddRoundKey(uint[] data, uint[] roundKeys, int round)
    {
        for (int i = 0; i < 4; i++)
        {
            data[i] ^= roundKeys[round * 4 + i];
        }
    }

    private void SubCells(uint[] data)
    {
        for (int i = 0; i < 4; i++)
        {
            data[i] = Constants.U1(data[i]);
        }
    }

    private void ShiftRows(uint[] data)
    {
        data[1] = Constants.RotLo(data[1], 8);
        data[2] = Constants.RotLo(data[2], 16);
        data[3] = Constants.RotLo(data[3], 24);
    }

    private void MixColumns(uint[] data)
    {
        uint[] temp = new uint[4];
        Array.Copy(data, temp, 4);

        data[0] = Constants.Mul2(temp[0]) ^ Constants.Mul3(temp[1]) ^ temp[2] ^ temp[3];
        data[1] = temp[0] ^ Constants.Mul2(temp[1]) ^ Constants.Mul3(temp[2]) ^ temp[3];
        data[2] = temp[0] ^ temp[1] ^ Constants.Mul2(temp[2]) ^ Constants.Mul3(temp[3]);
        data[3] = Constants.Mul3(temp[0]) ^ temp[1] ^ temp[2] ^ Constants.Mul2(temp[3]);
        }
    }

    public static class Constants
    {
        public static uint U1(uint x)
        {
            return x ^ RotHi(x, 8) ^ RotHi(x, 16) ^ RotHi(x, 24);
        }

        public static uint T4(uint x)
        {
            return RotHi(x, 8) ^ RotHi(x, 16) ^ (x << 24);
        }

        public static uint RotHi(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        public static uint RotLo(uint x, int n)
        {
            return (x >> n) | (x << (32 - n));
        }

        public static uint Mul2(uint x)
        {
            return ((x << 1) ^ ((x >> 7) * 0x0000001B)) & 0xFF;
        }

        public static uint Mul3(uint x)
        {
            return ((x << 1) ^ ((x >> 7) * 0x0000001B) ^ x) & 0xFF;
        }
    }

/*byte[] key = // Введите ваш ключ длиной 16 байт

byte[] plaintext = // Введите ваш текст длиной 16 байт

Clefia clefia = new Clefia(key);

byte[] ciphertext = clefia.Encrypt(plaintext);
Console.WriteLine("Ciphertext: " + BitConverter.ToString(ciphertext));

byte[] decrypted = clefia.Decrypt(ciphertext);
Console.WriteLine("Decrypted: " + BitConverter.ToString(decrypted));*/