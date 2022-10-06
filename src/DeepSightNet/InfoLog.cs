/*
 * RawLamb
 * Copyright 2022 Tom Svilans
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class InfoLog
    {
        public float[] Pith = null;
        public float[] Knots = null;

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr InfoLog_Load(string filepath, out int n_pith, out IntPtr pith, out int n_knots, out IntPtr knots);

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InfoLog_free(ref IntPtr ptr);
        public InfoLog()
        {
            Pith = new float[0];
            Knots = new float[0];
        }

        public InfoLog(string filepath)
        {
            Pith = new float[0];
            Knots = new float[0];
            Read(filepath);
        }

        public void Read(string filepath)
        {
            int n_pith = 0, n_knots = 0;
            IntPtr pith, knots;
            InfoLog_Load(filepath, out n_pith, out pith, out n_knots, out knots);

            //return;

            Pith = new float[n_pith * 2];
            Marshal.Copy(pith, Pith, 0, n_pith * 2);

            Knots = new float[n_knots * 11];
            Marshal.Copy(knots, Knots, 0, n_knots * 11);

            InfoLog_free(ref pith);
            InfoLog_free(ref knots);

        }
    }
}
