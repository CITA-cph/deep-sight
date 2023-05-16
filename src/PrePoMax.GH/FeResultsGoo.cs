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
using CaeResults;


namespace PrePoMax.GH
{
    public class GH_FeResults : GH_Goo<FeResults>
    {
        #region Constructors
        public GH_FeResults() : this(null) { }
        public GH_FeResults(FeResults native) { this.Value = native; }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_FeResults();
            else
                return this;
        }
        #endregion

        public static FeResults Parse(object obj)
        {
            if (obj is GH_FeResults)
                return (obj as GH_FeResults).Value;
            else if (obj is FeResults)
                return obj as CaeResults.FeResults;
            else
            {
                string msg = "";
                msg += string.Format("type: {0}\n", obj.GetType());
                msg += string.Format("reflected type: {0}\n", obj.GetType().ReflectedType);
                msg += string.Format("equals: {0}\n", obj.GetType() == typeof(FeResults));
                msg += string.Format("assembly: {0}\n", typeof(FeResults).AssemblyQualifiedName);
                msg += string.Format("assembly: {0}\n", obj.GetType().AssemblyQualifiedName);

                throw new Exception(msg);
            }
        }

        public override string ToString()
        {
            if (Value == null) return "Null FeResults";
            return Value.ToString();
        }

        public override string TypeName => "FeResultsGoo";
        public override string TypeDescription => "FeResultsGoo";
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
            if (source is FeResults fres)
            {
                Value = fres;
                return true;
            }
            if (source is GH_FeResults gh_fres)
            {
                Value = gh_fres.Value;
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
