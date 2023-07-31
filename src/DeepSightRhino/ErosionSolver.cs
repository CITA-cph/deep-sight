using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grid = DeepSight.FloatGrid;

namespace DeepSight.Rhino
{

    public abstract class ConditionBase
    {
        public abstract float CalculateDamage(Grid grid, Point3d pt, float[] neighbours, float threshold);

    }

    public class VectorCondition : ConditionBase
    {
        Vector3d[] Compass;
        Vector3d Direction;
        public VectorCondition(Vector3d vec)
        {
            Direction = vec;
        }
        public override float CalculateDamage(Grid grid, Point3d pt, float[] neighbours, float threshold)
        {
            float exp = 0;
            for (int i = 0; i < 27; ++i)
            {
                if (i == 13) continue;
                if (neighbours[i] < threshold)
                    exp += (float)Math.Max(0, -Direction * Compass[i]);
            }
            return exp / 26.0F;
        }
    }

    public class PlaneCondition : ConditionBase
    {
        Plane Plane;
        Transform Transform;
        public PlaneCondition(Plane plane)
        {
            this.Plane = plane;
        }
        public override float CalculateDamage(Grid grid, Point3d pt, float[] neighbours, float threshold)
        {
            pt.Transform(Transform);

            Vector3d vv = pt - this.Plane.Origin;
            if (vv * this.Plane.ZAxis < 0)
                return 1.0f;
            return 0.0f;
        }
    }



    public class ErosionSolver
    {
        List<ConditionBase> Conditions;
        public void Solve()
        {
            float damage = 0.0f;

            Conditions = new List<ConditionBase>();

            Conditions.Add(new VectorCondition(Vector3d.YAxis));
            Conditions.Add(new PlaneCondition(Plane.WorldXY));

            foreach(ConditionBase c in Conditions)
            {
                //damage += c.CalculateDamage(grid, pt, nbrs, threshold);
            }
        }
    }
}
