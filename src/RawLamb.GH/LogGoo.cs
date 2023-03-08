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
using RawLamb;

using DeepSight.RhinoCommon;

namespace DeepSight.GH
{
    public class GH_Log : GH_GeometricGoo<Log>
    {
        #region Constructors
        public GH_Log() : this(null) { }
        public GH_Log(Log native) { this.Value = native; }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Log();
            else
                return this;
        }
        #endregion

        public static Log ParseLog(object obj)
        {
            if (obj is GH_Log)
                return (obj as GH_Log).Value;
            else
                return obj as Log;
        }

        public override string ToString()
        {
            if (Value == null) return "Null Log";
            return Value.ToString();
        }

        public override string TypeName => "LogGoo";
        public override string TypeDescription => "LogGoo";
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
                return Value.BoundingBox;
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
            if (source is Log grid)
            {
                Value = grid;
                return true;
            }
            if (source is GH_Log gh_log)
            {
                Value = gh_log.Value;
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
            return new GH_Log(Value.Duplicate());

            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            var duplicate = Value.Duplicate();
            duplicate.Transform(xform);
            return new GH_Log(duplicate);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotImplementedException();
        }

        #endregion

        
    }
}
