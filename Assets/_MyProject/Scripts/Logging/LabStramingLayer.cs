using System;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

namespace MyGame.Logging
{
    public static class LabStreamingLayer
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        private const string libname = "lsl";

        public static readonly bool IsAvailable;

        static LabStreamingLayer()
        {
            try
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                IsAvailable = LoadLibrary("lsl.dll") != IntPtr.Zero || LoadLibrary("liblsl.dll") != IntPtr.Zero || LoadLibrary("lsl") != IntPtr.Zero;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                IsAvailable = dlopen("liblsl.dylib", RTLD_NOW) != IntPtr.Zero || dlopen("lsl.dylib", RTLD_NOW) != IntPtr.Zero;
#else
                IsAvailable = dlopen("liblsl.so", RTLD_NOW) != IntPtr.Zero || dlopen("lsl.so", RTLD_NOW) != IntPtr.Zero;
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LabStreamingLayer: native availability check failed: {e.Message}");
                IsAvailable = false;
            }

            if (!IsAvailable)
            {
                Debug.LogWarning("LabStreamingLayer: native LSL library not found. LSL logging will be disabled.");
            }
        }

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr lsl_create_streaminfo(
            string name,
            string type,
            int channel_count,
            double nominal_srate,
            int channel_format,
            string source_id
        );

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lsl_create_outlet(
            IntPtr info,
            int chunk_size,
            int max_buffered
        );

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int lsl_push_sample_str(
            IntPtr outlet,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] data,
            double timestamp,
            int pushthrough
        );

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lsl_destroy_outlet(
            IntPtr outlet
        );

        public const int cf_string = 3;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX
        private const int RTLD_NOW = 2;
        [DllImport("libdl")]
        private static extern IntPtr dlopen(string fileName, int flags);
#endif

#else
        public static readonly bool IsAvailable = false;
        public const int cf_string = 3;
#endif
    }
}
