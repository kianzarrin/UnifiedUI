using System;
using UnityEngine;

namespace KianCommons.Math {
    public static class MathUtil {
        public const float Epsilon = 0.001f;
        public static bool EqualAprox(float a, float b, float error = Epsilon) {
            float diff = a - b;
            return (diff > -error) & (diff < error);
        }

        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
    }
}
