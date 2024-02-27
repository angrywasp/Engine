// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    internal static class ReflectionHelpers
    {
        public static bool IsValueType(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }

            return targetType.IsValueType;
        }

        public static Assembly GetAssembly(Type targetType)
        {
            if (targetType == null)
            {
                throw new NullReferenceException("Must supply the targetType parameter");
            }

            return targetType.Assembly;
        }

        public static int SizeOf<T>() => Marshal.SizeOf(typeof(T));
    }
}
