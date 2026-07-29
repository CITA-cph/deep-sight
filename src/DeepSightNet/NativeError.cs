/*
 * DeepSight
 * Copyright 2023 Tom Svilans
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using System;
using System.Runtime.InteropServices;

namespace DeepSight
{
    /// <summary>
    /// Thrown when a call into deepsight.dll fails.
    /// </summary>
    public class DeepSightNativeException : Exception
    {
        public DeepSightNativeException(string message) : base(message) { }
    }

    /// <summary>
    /// Bridge to the native exception barrier.
    ///
    /// The native library no longer lets C++ exceptions escape across the C ABI
    /// (doing so is undefined behaviour and reliably took Rhino down with it).
    /// Instead it records a message per thread, which these helpers read back
    /// and re-raise as a managed exception.
    /// </summary>
    public static class NativeError
    {
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DeepSight_GetLastError();

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DeepSight_HasError();

        /// <summary>
        /// The message from the last failed native call on this thread, or null.
        /// </summary>
        /// <remarks>
        /// Deliberately marshalled by hand rather than declared as a
        /// <c>string</c> return. The default LPStr return marshaller calls
        /// CoTaskMemFree on the pointer it receives, but this buffer is owned by
        /// the native library, so letting the marshaller free it would corrupt
        /// the heap. PtrToStringAnsi copies without taking ownership.
        /// </remarks>
        public static string LastError
        {
            get
            {
                IntPtr ptr = DeepSight_GetLastError();
                if (ptr == IntPtr.Zero) return null;

                string message = Marshal.PtrToStringAnsi(ptr);
                return string.IsNullOrEmpty(message) ? null : message;
            }
        }

        /// <summary>
        /// Throw if the most recent native call on this thread failed.
        /// </summary>
        public static void ThrowIfFailed(string context = null)
        {
            if (DeepSight_HasError() == 0) return;

            string message = LastError ?? "Unknown native error.";
            throw new DeepSightNativeException(
                context == null ? message : context + ": " + message);
        }

        /// <summary>
        /// Validate a handle returned by a native factory function and throw
        /// with the underlying reason if it is null.
        /// </summary>
        public static IntPtr Check(IntPtr handle, string context)
        {
            if (handle == IntPtr.Zero)
            {
                throw new DeepSightNativeException(
                    context + ": " + (LastError ?? "native call returned a null handle."));
            }
            return handle;
        }
    }
}
