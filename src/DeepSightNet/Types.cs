using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    using Vec3f = Vec3<float>;
    using Vec3d = Vec3<double>;
    using Vec3i = Vec3<int>;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3<T>
    {
        public T[] Data;

        public T X
        {
            get { return Data[0]; }
            set { Data[0] = value; }
        }
        public T Y
        {
            get { return Data[1]; }
            set { Data[1] = value; }
        }
        public T Z
        {
            get { return Data[2]; }
            set { Data[2] = value; }
        }

        public Vec3(T x, T y, T z)
        {
            Data = new T[] { x, y, z };
        }

        public Vec3(T[] xyz)
        {
            Data = new T[3];
            Array.Copy(xyz, Data, Math.Min(3, xyz.Length));
        }

        public override string ToString()
        {
            return $"[{Data[0]}, {Data[1]}, {Data[2]}]";
        }
    }

}
