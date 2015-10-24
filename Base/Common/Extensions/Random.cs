using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sinan
{
#if mono
    //[Serializable, ComVisible(true)]
    //public sealed class Random
    //{
    //    // Fields
    //    private int inext;
    //    private int inextp;
    //    private const int MBIG = 0x7fffffff;
    //    private const int MSEED = 0x9a4ec86;
    //    private const int MZ = 0;
    //    private readonly int[] SeedArray;

    //    // Methods
    //    public Random()
    //        : this(Environment.TickCount)
    //    {
    //    }

    //    public Random(int Seed)
    //    {
    //        this.SeedArray = new int[0x38];
    //        int num4 = (Seed == -2147483648) ? 0x7fffffff : Math.Abs(Seed);
    //        int num2 = 0x9a4ec86 - num4;
    //        this.SeedArray[0x37] = num2;
    //        int num3 = 1;
    //        for (int i = 1; i < 0x37; i++)
    //        {
    //            int index = (0x15 * i) % 0x37;
    //            this.SeedArray[index] = num3;
    //            num3 = num2 - num3;
    //            if (num3 < 0)
    //            {
    //                num3 += 0x7fffffff;
    //            }
    //            num2 = this.SeedArray[index];
    //        }
    //        for (int j = 1; j < 5; j++)
    //        {
    //            for (int k = 1; k < 0x38; k++)
    //            {
    //                this.SeedArray[k] -= this.SeedArray[1 + ((k + 30) % 0x37)];
    //                if (this.SeedArray[k] < 0)
    //                {
    //                    this.SeedArray[k] += 0x7fffffff;
    //                }
    //            }
    //        }
    //        this.inext = 0;
    //        this.inextp = 0x15;
    //        Seed = 1;
    //    }

    //    private double GetSampleForLargeRange()
    //    {
    //        int num = this.InternalSample();
    //        if ((this.InternalSample() % 2) == 0)
    //        {
    //            num = -num;
    //        }
    //        double num2 = num;
    //        num2 += 2147483646.0;
    //        return (num2 / 4294967293);
    //    }

    //    private int InternalSample()
    //    {
    //        int inext = this.inext;
    //        int inextp = this.inextp;
    //        if (++inext >= 0x38)
    //        {
    //            inext = 1;
    //        }
    //        if (++inextp >= 0x38)
    //        {
    //            inextp = 1;
    //        }
    //        int num = this.SeedArray[inext] - this.SeedArray[inextp];
    //        if (num == 0x7fffffff)
    //        {
    //            num--;
    //        }
    //        if (num < 0)
    //        {
    //            num += 0x7fffffff;
    //        }
    //        this.SeedArray[inext] = num;
    //        this.inext = inext;
    //        this.inextp = inextp;
    //        return num;
    //    }

    //    public int Next()
    //    {
    //        return this.InternalSample();
    //    }

    //    public int Next(int maxValue)
    //    {
    //        if (maxValue < 0)
    //        {
    //            throw new ArgumentOutOfRangeException("maxValue", "Max value is less than min value.");
    //        }
    //        return (int)(this.Sample() * maxValue);
    //    }

    //    public int Next(int minValue, int maxValue)
    //    {
    //        if (minValue > maxValue)
    //        {
    //            throw new ArgumentOutOfRangeException("minValue", "Min value is greater than max value.");
    //        }
    //        long num = maxValue - minValue;
    //        if (num <= 0x7fffffffL)
    //        {
    //            return (((int)(this.Sample() * num)) + minValue);
    //        }
    //        return (((int)((long)(this.GetSampleForLargeRange() * num))) + minValue);
    //    }

    //    public void NextBytes(byte[] buffer)
    //    {
    //        if (buffer == null)
    //        {
    //            throw new ArgumentNullException("buffer");
    //        }
    //        for (int i = 0; i < buffer.Length; i++)
    //        {
    //            buffer[i] = (byte)(this.InternalSample() % 0x100);
    //        }
    //    }

    //    public double NextDouble()
    //    {
    //        return this.Sample();
    //    }

    //    double Sample()
    //    {
    //        return (this.InternalSample() * 4.6566128752457969E-10);
    //    }
    //}


    [Serializable]
    [ComVisible(true)]
    public sealed class Random
    {
        const int MBIG = int.MaxValue;
        const int MSEED = 161803398;

        int inext, inextp;
        int[] SeedArray = new int[56];

        public Random()
            : this(Environment.TickCount)
        {
        }

        public Random(int Seed)
        {
            int ii;
            int mj, mk;

            if (Seed == Int32.MinValue)
                mj = MSEED - Math.Abs(Int32.MinValue + 1);
            else
                mj = MSEED - Math.Abs(Seed);

            SeedArray[55] = mj;
            mk = 1;
            for (int i = 1; i < 55; i++)
            {  //  [1, 55] is special (Knuth)
                ii = (21 * i) % 55;
                SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                    mk += MBIG;
                mj = SeedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                    if (SeedArray[i] < 0)
                        SeedArray[i] += MBIG;
                }
            }
            inext = 0;
            inextp = 31;
        }

        double Sample()
        {
            int retVal;

            int inext = this.inext;
            int inextp = this.inextp;

            if (++inext >= 56) inext = 1;
            if (++inextp >= 56) inextp = 1;

            retVal = SeedArray[inext] - SeedArray[inextp];

            if (retVal < 0)
                retVal += MBIG;

            SeedArray[inext] = retVal;
            this.inext = inext;
            this.inextp = inextp;

            return retVal * (1.0 / MBIG);
        }

        public int Next()
        {
            return (int)(Sample() * int.MaxValue);
        }

        public int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("maxValue", "Max value is less than min value.");

            return (int)(Sample() * maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue", "Min value is greater than max value.");

            // special case: a difference of one (or less) will always return the minimum
            // e.g. -1,-1 or -1,0 will always return -1
            uint diff = (uint)(maxValue - minValue);
            if (diff <= 1)
                return minValue;

            return (int)((uint)(Sample() * diff) + minValue);
        }

        public void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(Sample() * (byte.MaxValue + 1));
            }
        }

        public double NextDouble()
        {
            return this.Sample();
        }
    }
#else
    //public sealed class Random : System.Random
    //{
    //}
#endif
}
