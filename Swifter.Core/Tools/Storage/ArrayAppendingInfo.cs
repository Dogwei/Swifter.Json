using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 为集合计算常用的长度，以获取在动态长度数组最佳的性能。
    /// </summary>
    public struct ArrayAppendingInfo
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static void Swap<TValue>(ref TValue val1, ref TValue val2)
        {
            var val = val1;
            val1 = val2;
            val2 = val;
        }

        /// <summary>
        ///  第一常用长度。
        /// </summary>
        public int FirstCommonlyUsedLength;

        /// <summary>
        /// 第一常用长度使用次数。
        /// </summary>
        public long FirstCommonlyUsedNumber;

        /// <summary>
        /// 第二常用长度
        /// </summary>
        public int SecondCommonlyUsedLength;

        /// <summary>
        /// 第二常用长度使用次数。
        /// </summary>
        public long SecondCommonlyUsedNumber;

        /// <summary>
        /// 第三常用长度
        /// </summary>
        public int ThirdCommonlyUsedLength;

        /// <summary>
        /// 第三常用长度使用次数。
        /// </summary>
        public long ThirdCommonlyUsedNumber;

        /// <summary>
        /// 第四常用长度
        /// </summary>
        public int FourthCommonlyUsedLength;

        /// <summary>
        /// 第四常用长度使用次数。
        /// </summary>
        public long FourthCommonlyUsedNumber;

        /// <summary>
        /// 第五常用长度（非前四类的最后一次使用长度）
        /// </summary>
        public int FifthCommonlyUsedLength;

        /// <summary>
        /// 第五常用长度使用次数。
        /// </summary>
        public long FifthCommonlyUsedNumber;

        /// <summary>
        /// 最接近平均值的常用长度。
        /// </summary>
        public int MostClosestMeanCommonlyUsedLength;

        [MethodImpl(MethodImplOptions.NoInlining)]
        void Calculation()
        {
            var totalNumber =
                FirstCommonlyUsedNumber +
                SecondCommonlyUsedNumber +
                ThirdCommonlyUsedNumber +
                FourthCommonlyUsedNumber +
                FifthCommonlyUsedNumber;

            if (totalNumber == 0)
            {
                return;
            }

            int meanLength;

            if (totalNumber > uint.MaxValue)
            {
                var totalLength =
                    (double)FirstCommonlyUsedLength * FirstCommonlyUsedNumber +
                    (double)SecondCommonlyUsedLength * SecondCommonlyUsedNumber +
                    (double)ThirdCommonlyUsedLength * ThirdCommonlyUsedNumber +
                    (double)FourthCommonlyUsedLength * FourthCommonlyUsedNumber +
                    (double)FifthCommonlyUsedLength * FifthCommonlyUsedNumber;

                meanLength = (int)(totalLength / totalNumber);

                if (totalNumber >= 0xffffffffffff)
                {
                    FirstCommonlyUsedNumber /= 0xfffff;
                    SecondCommonlyUsedNumber /= 0xfffff;
                    ThirdCommonlyUsedNumber /= 0xfffff;
                    FourthCommonlyUsedNumber /= 0xfffff;
                    FifthCommonlyUsedNumber /= 0xfffff;
                }
            }
            else
            {
                var totalLength =
                    FirstCommonlyUsedLength * FirstCommonlyUsedNumber +
                    SecondCommonlyUsedLength * SecondCommonlyUsedNumber +
                    ThirdCommonlyUsedLength * ThirdCommonlyUsedNumber +
                    FourthCommonlyUsedLength * FourthCommonlyUsedNumber +
                    FifthCommonlyUsedLength * FifthCommonlyUsedNumber;

                meanLength = (int)(totalLength / totalNumber);
            }


            var abs1 = Math.Abs(meanLength - FirstCommonlyUsedLength);
            var abs2 = Math.Abs(meanLength - SecondCommonlyUsedLength);
            var abs3 = Math.Abs(meanLength - ThirdCommonlyUsedLength);
            var abs4 = Math.Abs(meanLength - FourthCommonlyUsedLength);
            var abs5 = Math.Abs(meanLength - FifthCommonlyUsedLength);

            var minAbs = Math.Min(Math.Min(Math.Min(abs1, abs2), abs3), Math.Min(abs4, abs5));

            if (minAbs == abs1)
            {
                MostClosestMeanCommonlyUsedLength = FirstCommonlyUsedLength;
            }
            else if (minAbs == abs2)
            {
                MostClosestMeanCommonlyUsedLength = SecondCommonlyUsedLength;
            }
            else if (minAbs == abs3)
            {
                MostClosestMeanCommonlyUsedLength = ThirdCommonlyUsedLength;
            }
            else if (minAbs == abs4)
            {
                MostClosestMeanCommonlyUsedLength = FourthCommonlyUsedLength;
            }
            else
            {
                MostClosestMeanCommonlyUsedLength = FifthCommonlyUsedLength;
            }
        }

        /// <summary>
        /// 添加使用长度计数。
        /// </summary>
        /// <param name="length">使用的长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AddUsedLength(int length)
        {
            if (length == FirstCommonlyUsedLength)
            {
                ++FirstCommonlyUsedNumber;

                if (MostClosestMeanCommonlyUsedLength != FirstCommonlyUsedLength)
                {
                    Calculation();
                }
            }
            else if (length == SecondCommonlyUsedLength)
            {
                ++SecondCommonlyUsedNumber;

                if (MostClosestMeanCommonlyUsedLength != SecondCommonlyUsedLength)
                {
                    FirstCommonlyUsedNumber -= FirstCommonlyUsedNumber / 4;

                    Calculation();
                }

                if (SecondCommonlyUsedNumber > FirstCommonlyUsedNumber)
                {
                    Swap(ref FirstCommonlyUsedLength, ref SecondCommonlyUsedLength);
                    Swap(ref FirstCommonlyUsedNumber, ref SecondCommonlyUsedNumber);
                }
            }
            else if (length == ThirdCommonlyUsedLength)
            {
                ++ThirdCommonlyUsedNumber;

                if (MostClosestMeanCommonlyUsedLength != ThirdCommonlyUsedLength)
                {
                    FirstCommonlyUsedNumber -= FirstCommonlyUsedNumber / 4;
                    SecondCommonlyUsedNumber -= SecondCommonlyUsedNumber / 8;

                    Calculation();
                }

                if (ThirdCommonlyUsedNumber > SecondCommonlyUsedNumber)
                {
                    Swap(ref SecondCommonlyUsedLength, ref ThirdCommonlyUsedLength);
                    Swap(ref SecondCommonlyUsedNumber, ref ThirdCommonlyUsedNumber);
                }
            }
            else if (length == FourthCommonlyUsedLength)
            {
                ++FourthCommonlyUsedNumber;

                if (MostClosestMeanCommonlyUsedLength != FourthCommonlyUsedLength)
                {
                    FirstCommonlyUsedNumber -= FirstCommonlyUsedNumber / 4;
                    SecondCommonlyUsedNumber -= SecondCommonlyUsedNumber / 8;
                    ThirdCommonlyUsedNumber -= ThirdCommonlyUsedNumber / 8;

                    Calculation();
                }

                if (FourthCommonlyUsedNumber > ThirdCommonlyUsedNumber)
                {
                    Swap(ref ThirdCommonlyUsedLength, ref FourthCommonlyUsedLength);
                    Swap(ref ThirdCommonlyUsedNumber, ref FourthCommonlyUsedNumber);
                }
            }
            else if (length == FifthCommonlyUsedLength)
            {
                ++FifthCommonlyUsedNumber;

                if (MostClosestMeanCommonlyUsedLength != FifthCommonlyUsedLength)
                {
                    FirstCommonlyUsedNumber -= FirstCommonlyUsedNumber / 4;
                    SecondCommonlyUsedNumber -= SecondCommonlyUsedNumber / 4;
                    ThirdCommonlyUsedNumber -= ThirdCommonlyUsedNumber / 8;
                    FourthCommonlyUsedNumber -= FourthCommonlyUsedNumber / 8;

                    Calculation();
                }

                if (FifthCommonlyUsedNumber > FourthCommonlyUsedNumber)
                {
                    Swap(ref FourthCommonlyUsedLength, ref FifthCommonlyUsedLength);
                    Swap(ref FourthCommonlyUsedNumber, ref FifthCommonlyUsedNumber);
                }
            }
            else
            {
                FifthCommonlyUsedLength = length;
                FifthCommonlyUsedNumber = 1;
            }
        }
    }
}