using System;
using System.Runtime.InteropServices;

namespace MyGame.Logging
{
    public static class LabStreamingLayer
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        private const string libname = "lsl";

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lsl_push_sample_str(
            IntPtr outlet,
            string[] data
        );

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lsl_destroy_outlet(
            IntPtr outlet
        );

        public const int cf_string = 3;
#endif
    }
}
