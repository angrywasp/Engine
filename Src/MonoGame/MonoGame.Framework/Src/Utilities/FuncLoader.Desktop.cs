using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    internal class FuncLoader
    {
        private class Windows
        {
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string lpszLib);
        }

        private class Linux
        {
            [DllImport("libdl.so.2")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("libdl.so.2")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        private class OSX
        {
            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }
        
        private const int RTLD_LAZY = 0x0001;

        public static IntPtr LoadLibraryExt(string libname)
        {
            var ret = IntPtr.Zero;
            var assemblyLocation = Path.GetDirectoryName(typeof(FuncLoader).Assembly.Location) ?? "./";

            ret = LoadLibrary(Path.Combine(assemblyLocation, libname));

            if (ret == IntPtr.Zero && CurrentPlatform.OS == OS.MacOSX)
                ret = LoadLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libname));

            // Try .NET Core development locations
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(assemblyLocation, "runtimes", CurrentPlatform.Rid, "native", libname));

            // Try current folder (.NET Core will copy it there after publish)
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(assemblyLocation, libname));

            // Try alternate way of checking current folder
            // assemblyLocation is null if we are inside macOS app bundle
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, libname));

            // Try loading system library
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(libname);

            // Welp, all failed, PANIC!!!
            if (ret == IntPtr.Zero)
                throw new Exception("Failed to load library: " + libname);

            return ret;
        }

        public static IntPtr LoadLibrary(string libname)
        {
            switch (CurrentPlatform.OS)
            {
                case OS.Linux: return Linux.dlopen(libname, RTLD_LAZY);
                case OS.MacOSX: return OSX.dlopen(libname, RTLD_LAZY);
                case OS.Windows: return Windows.LoadLibraryW(libname);
                default: throw new PlatformNotSupportedException();
            }
        }

        public static T LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
        {
            var ret = IntPtr.Zero;

            switch (CurrentPlatform.OS)
            {
                case OS.Linux:
                    ret = Linux.dlsym(library, function);
                    break;
                case OS.MacOSX:
                    ret = OSX.dlsym(library, function);
                    break;
                case OS.Windows:
                    ret = Windows.GetProcAddress(library, function);
                    break;
                default: throw new PlatformNotSupportedException();
            }

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default(T);
            }

            return Marshal.GetDelegateForFunctionPointer<T>(ret);
        }
    }
}