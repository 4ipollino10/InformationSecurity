using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace CryptoMethods.Sha256
{
    public class HashSha256
    {
        private static readonly uint[] K = new uint[64] {
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };

        private static uint Rotr(uint x, byte n)
        {
            Debug.Assert(n < 32);
            return (x >> n) | (x << (32 - n));
        }

        private static uint Ch(uint x, uint y, uint z)
        {
            return (x & y) ^ ((~x) & z);
        }

        private static uint Maj(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }

        private static uint Sigma0(uint x)
        {
            return Rotr(x, 2) ^ Rotr(x, 13) ^ Rotr(x, 22);
        }

        private static uint Sigma1(uint x)
        {
            return Rotr(x, 6) ^ Rotr(x, 11) ^ Rotr(x, 25);
        }

        private static uint sigma0(uint x)
        {
            return Rotr(x, 7) ^ Rotr(x, 18) ^ (x >> 3);
        }

        private static uint sigma1(uint x)
        {
            return Rotr(x, 17) ^ Rotr(x, 19) ^ (x >> 10);
        }


        private readonly uint[] _h = new uint[8] {
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

        private readonly byte[] _pendingBlock = new byte[64];
        private uint _pendingBlockOff = 0;
        private readonly uint[] _uintBuffer = new uint[16];

        private ulong _bitsProcessed = 0;

        private bool _closed = false;

        private void ProcessBlock(IReadOnlyList<uint> m)
        {
            Debug.Assert(m.Count == 16);

            // 1. Prepare the message schedule (W[t]):
            var W = new uint[64];
            for (var t = 0; t < 16; ++t)
            {
                W[t] = m[t];
            }

            for (var t = 16; t < 64; ++t)
            {
                W[t] = sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16];
            }

            // 2. Initialize the eight working variables with the (i-1)-st hash value:
            uint a = _h[0],
                   b = _h[1],
                   c = _h[2],
                   d = _h[3],
                   e = _h[4],
                   f = _h[5],
                   g = _h[6],
                   h = _h[7];

            // 3. For t=0 to 63:
            for (var t = 0; t < 64; ++t)
            {
                var T1 = h + Sigma1(e) + Ch(e, f, g) + K[t] + W[t];
                var T2 = Sigma0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + T1;
                d = c;
                c = b;
                b = a;
                a = T1 + T2;
            }

            // 4. Compute the intermediate hash value H:
            _h[0] = a + _h[0];
            _h[1] = b + _h[1];
            _h[2] = c + _h[2];
            _h[3] = d + _h[3];
            _h[4] = e + _h[4];
            _h[5] = f + _h[5];
            _h[6] = g + _h[6];
            _h[7] = h + _h[7];
        }

        private void AddData(byte[] data, uint offset, uint len)
        {
            if (_closed)
                throw new InvalidOperationException("Adding data to a closed hasher.");

            if (len == 0)
                return;

            _bitsProcessed += len * 8;

            while (len > 0)
            {
                uint amount_to_copy;

                if (len < 64)
                {
                    if (_pendingBlockOff + len > 64)
                        amount_to_copy = 64 - _pendingBlockOff;
                    else
                        amount_to_copy = len;
                }
                else
                {
                    amount_to_copy = 64 - _pendingBlockOff;
                }

                Array.Copy(data, offset, _pendingBlock, _pendingBlockOff, amount_to_copy);
                len -= amount_to_copy;
                offset += amount_to_copy;
                _pendingBlockOff += amount_to_copy;

                if (_pendingBlockOff == 64)
                {
                    ToUintArray(_pendingBlock, _uintBuffer);
                    ProcessBlock(_uintBuffer);
                    _pendingBlockOff = 0;
                }
            }
        }

        private ReadOnlyCollection<byte> GetHash()
        {
            return ToByteArray(GetHashUInt32());
        }

        private ReadOnlyCollection<uint> GetHashUInt32()
        {
            if (!_closed)
            {
                var sizeTemp = _bitsProcessed;

                AddData(new byte[1] { 0x80 }, 0, 1);

                var availableSpace = 64 - _pendingBlockOff;

                if (availableSpace < 8)
                    availableSpace += 64;

                // 0-initialized
                var padding = new byte[availableSpace];
                // Insert lenght uint64
                for (uint i = 1; i <= 8; ++i)
                {
                    padding[padding.Length - i] = (byte)sizeTemp;
                    sizeTemp >>= 8;
                }

                AddData(padding, 0u, (uint)padding.Length);

                Debug.Assert(_pendingBlockOff == 0);

                _closed = true;
            }

            return Array.AsReadOnly(_h);
        }

        private static void ToUintArray(byte[] src, uint[] dest)
        {
            for (uint i = 0, j = 0; i < dest.Length; ++i, j += 4)
            {
                dest[i] = ((uint)src[j+0] << 24) | ((uint)src[j+1] << 16) | ((uint)src[j+2] << 8) | ((uint)src[j+3]);
            }
        }

        private static ReadOnlyCollection<byte> ToByteArray(ReadOnlyCollection<uint> src)
        {
            var dest = new byte[src.Count * 4];
            var pos = 0;

            foreach (var t in src)
            {
                dest[pos++] = (byte)(t >> 24);
                dest[pos++] = (byte)(t >> 16);
                dest[pos++] = (byte)(t >> 8);
                dest[pos++] = (byte)(t);
            }

            return Array.AsReadOnly(dest);
        }

        public static ReadOnlyCollection<byte> HashFile(Stream fs)
        {
            var sha = new HashSha256();
            var buf = new byte[8196];

            uint bytesRead;
            do
            {
                bytesRead = (uint)fs.Read(buf, 0, buf.Length);
                if (bytesRead == 0)
                    break;

                sha.AddData(buf, 0, bytesRead);
            }
            while (bytesRead == 8196);

            return sha.GetHash();
        }
    }
}