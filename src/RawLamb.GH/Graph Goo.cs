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

namespace RawLamb.GH
{
    public class GH_N4jGraph : GH_Goo<N4jGraph>
    {
        #region Constructors
        public GH_N4jGraph() : this(null) { }
        public GH_N4jGraph(N4jGraph native) { this.Value = native; }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_N4jGraph();
            else
                return this;
        }
        #endregion

        public static N4jGraph ParseStructure(object obj)
        {
            if (obj is GH_N4jGraph)
                return (obj as GH_N4jGraph).Value;
            else
                return obj as N4jGraph;
        }
        public override string ToString()
        {
            if (Value == null) return "Null glulam structure";
            return Value.ToString();
        }

        public override string TypeName => "N4jGraphGoo";
        public override string TypeDescription => "N4jGraphGoo";
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

        #region Casting
        public override bool CastFrom(object source)
        {
            if (source == null) return false;
            if (source is N4jGraph structure)
            {
                Value = structure;
                return true;
            }
            if (source is GH_N4jGraph ghStructure)
            {
                Value = ghStructure.Value;
                return true;
            }
            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null) return false;

            return false;
        }

        #endregion
    }
}
