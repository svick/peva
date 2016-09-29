using System;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;

namespace peva
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<FindFirstEqualByte>();
        }
    }

    [Q1Column, Q3Column, Config(typeof(Config))]
    public class FindFirstEqualByte
    {
        public class Config : ManualConfig
        {
            public Config()
            {
                Add(StatisticColumn.CiLower(ConfidenceLevel.L95), StatisticColumn.CiUpper(ConfidenceLevel.L95));
            }
        }

        private readonly Vector<byte>[] inputs;

        public FindFirstEqualByte()
        {
            inputs = new Vector<byte>[Vector<byte>.Count * 2];

            var bytes = Enumerable.Repeat<byte>(0xff, Vector<byte>.Count).ToArray();
            for (int i = 0; i < Vector<byte>.Count; i++)
            {
                Vector<byte> vector = new Vector<byte>(bytes);
                inputs[i] = vector;
                bytes[i] = 0;
            }

            for (int i = 0; i < Vector<byte>.Count; i++)
            {
                bytes[i] = 1;
                Vector<byte> vector = new Vector<byte>(bytes);
                inputs[Vector<byte>.Count + i] = vector;
                bytes[i] = 0;
            }
        }

        [Benchmark]
        public int RunOld()
        {
            int sum = 0;

            var random = new Random(42);

            for (int i = 0; i < inputs.Length * 10; i++)
            {
                int index = random.Next(inputs.Length);

                sum += Old(ref inputs[index]);
            }

            return sum;
        }

        [Benchmark]
        public int RunTernary()
        {
            int sum = 0;

            var random = new Random(42);

            for (int i = 0; i < inputs.Length * 10; i++)
            {
                int index = random.Next(inputs.Length);

                sum += Ternary(ref inputs[index]);
            }

            return sum;
        }

        [Benchmark]
        public int RunIfs()
        {
            int sum = 0;

            var random = new Random(42);

            for (int i = 0; i < inputs.Length * 10; i++)
            {
                int index = random.Next(inputs.Length);

                sum += Ifs(ref inputs[index]);
            }

            return sum;
        }

        private static int Old(ref Vector<byte> byteEquals)
        {
            var vector64 = Vector.AsVectorInt64(byteEquals);
            for (var i = 0; i < Vector<long>.Count; i++)
            {
                var longValue = vector64[i];
                if (longValue == 0) continue;

                return (i << 3) +
                       ((longValue & 0x00000000ffffffff) > 0
                           ? (longValue & 0x000000000000ffff) > 0
                               ? (longValue & 0x00000000000000ff) > 0 ? 0 : 1
                               : (longValue & 0x0000000000ff0000) > 0 ? 2 : 3
                           : (longValue & 0x0000ffff00000000) > 0
                               ? (longValue & 0x000000ff00000000) > 0 ? 4 : 5
                               : (longValue & 0x00ff000000000000) > 0 ? 6 : 7);
            }
            throw new InvalidOperationException();
        }

        private static int Ternary(ref Vector<byte> byteEquals)
        {
            var vector64 = Vector.AsVectorInt64(byteEquals);
            for (var i = 0; i < Vector<long>.Count; i++)
            {
                var longValue = vector64[i];
                if (longValue == 0) continue;

                int result = i << 3;

                var tmp1 = longValue & 0x00000000ffffffff;
                result += tmp1 == 0 ? 4 : 0;
                longValue = tmp1 == 0 ? longValue : tmp1;

                var tmp2 = longValue & 0x0000ffff0000ffff;
                result += tmp2 == 0 ? 2 : 0;
                longValue = tmp2 == 0 ? longValue : tmp2;

                var tmp3 = longValue & 0x00ff00ff00ff00ff;
                result += tmp3 == 0 ? 1 : 0;

                return result;
            }
            throw new InvalidOperationException();
        }

        private static int Ifs(ref Vector<byte> byteEquals)
        {
            var vector64 = Vector.AsVectorInt64(byteEquals);
            for (var i = 0; i < Vector<long>.Count; i++)
            {
                var longValue = vector64[i];
                if (longValue == 0) continue;

                int result = i << 3;

                var tmp1 = longValue & 0x00000000ffffffff;

                if (tmp1 == 0)
                    result += 4;
                else
                    longValue = tmp1;

                var tmp2 = longValue & 0x0000ffff0000ffff;

                if (tmp2 == 0)
                    result += 2;
                else
                    longValue = tmp2;

                var tmp3 = longValue & 0x00ff00ff00ff00ff;

                if (tmp3 == 0)
                    result += 1;

                return result;
            }
            throw new InvalidOperationException();
        }
    }
}
