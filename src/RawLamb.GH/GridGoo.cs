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
using System.Linq;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Drawing;
using GH_IO.Serialization;
using GH_IO;

using DeepSight;
using RawLamb;

namespace RawLamb.GH
{
    public class GH_Grid : GH_GeometricGoo<Grid>
    {
        #region Constructors
        public GH_Grid() : this(null) { }
        public GH_Grid(Grid native) { this.Value = native; }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Grid();
            else
                return this;
        }
        #endregion

        public static Grid ParseStructure(object obj)
        {
            if (obj is GH_Grid)
                return (obj as GH_Grid).Value;
            else
                return obj as Grid;
        }
        public override string ToString()
        {
            if (Value == null) return "Null Grid";
            return Value.ToString();
        }

        public override string TypeName => "GridGoo";
        public override string TypeDescription => "GridGoo";
        public override object ScriptVariable() => Value;

        public override bool IsValid
        {
            get
            {
                if (Value == null) return false;
                return true;
            }
        }
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) return "No data";
                return string.Empty;
            }
        }

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }

                int[] min, max;
                Value.BoundingBox(out min, out max);

                var pMin = new Point3d(min[0], min[1], min[2]);
                var pMax = new Point3d(max[0], max[1], max[2]);

                var grid_xform = Value.Transform.ToRhinoTransform();
                pMin.Transform(grid_xform);
                pMax.Transform(grid_xform);

                return new BoundingBox(pMin, pMax);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var bb = Boundingbox;
            if (bb.IsValid) bb.Transform(xform);
            return bb;

        }

        #region Casting
        public override bool CastFrom(object source)
        {
            if (source == null) return false;
            if (source is Grid grid)
            {
                Value = grid;
                return true;
            }
            if (source is GH_Grid gh_grid)
            {
                Value = gh_grid.Value;
                return true;
            }
            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null) return false;

            return false;
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_Grid(Value.Duplicate());

            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            var grid_xform = Value.Transform.ToRhinoTransform();
            var new_xform = Rhino.Geometry.Transform.Multiply(xform, grid_xform);

            Value.Transform(new_xform);

            return new GH_Grid(Value);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
