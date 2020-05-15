using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public static class SyrfusMath
    {
        public static int Mod(int a, int n)
        {
            int result = a % n;
            if ((result < 0 && n > 0) || (result > 0 && n < 0))
            {
                result += n;
            }
            return result;
        }

        public static float Mod(float a, float n)
        {
            float result = a % n;
            if ((result < 0 && n > 0) || (result > 0 && n < 0))
            {
                result += n;
            }
            return result;
        }
    }
}
