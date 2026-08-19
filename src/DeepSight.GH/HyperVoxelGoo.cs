using System;
using System.Linq;
using Grasshopper.Kernel.Types;

namespace DeepSight.GH
{
    /// <summary>
    /// Grasshopper wrapper so a HyperVoxel can travel along wires.
    /// </summary>
    /// <remarks>
    /// Deliberately derives from GH_Goo rather than GH_GeometricGoo (which is
    /// what GH_Grid uses). GH_GeometricGoo requires implementing transform,
    /// morph, bounding box and duplication members, and a HyperVoxel has no
    /// single sensible geometric identity anyway - it is a stack of volumes, and
    /// the individual grids are already previewable via GH_Grid.
    ///
    /// The smaller base class also means far fewer abstract members to get
    /// wrong, which matters because this file has not been through a compiler.
    /// If viewport preview of the whole stack is wanted later, this can be
    /// promoted to GH_GeometricGoo by adding a union bounding box.
    /// </remarks>
    public class GH_HyperVoxel : GH_Goo<HyperVoxel>
    {
        #region Constructors

        public GH_HyperVoxel() : this(null) { }

        public GH_HyperVoxel(HyperVoxel native)
        {
            this.Value = native;
        }

        public override IGH_Goo Duplicate()
        {
            // Shallow copy on purpose: the layers reference native grids that
            // are shared with GH_Grid instances elsewhere on the canvas, and
            // duplicating those would either be expensive (deep copying whole
            // volumes) or wrong (double ownership of the same native pointer).
            if (Value == null) return new GH_HyperVoxel();
            return new GH_HyperVoxel(Value);
        }

        #endregion

        /// <summary>Unwrap a HyperVoxel from a Goo or a raw object.</summary>
        public static HyperVoxel ParseStructure(object obj)
        {
            if (obj is GH_HyperVoxel)
                return (obj as GH_HyperVoxel).Value;
            return obj as HyperVoxel;
        }

        public override bool IsValid
        {
            get { return Value != null && Value.Count > 0; }
        }

        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) return "No data";
                if (Value.Count == 0) return "HyperVoxel has no layers";
                return string.Empty;
            }
        }

        public override string TypeName
        {
            get { return "HyperVoxel"; }
        }

        public override string TypeDescription
        {
            get { return "A stack of co-registered voxel grids."; }
        }

        public override object ScriptVariable()
        {
            return Value;
        }

        public override string ToString()
        {
            if (Value == null) return "Null HyperVoxel";
            return Value.ToString();
        }

        #region Casting

        public override bool CastFrom(object source)
        {
            if (source == null) return false;

            if (source is HyperVoxel)
            {
                Value = source as HyperVoxel;
                return true;
            }

            if (source is GH_HyperVoxel)
            {
                Value = (source as GH_HyperVoxel).Value;
                return true;
            }

            // A single grid is a perfectly good one-layer stack. This makes the
            // component usable without a separate "promote" step.
            GridApi grid = GH_Grid.ParseStructure(source);
            if (grid != null)
            {
                var hv = new HyperVoxel();
                hv.Add(grid);
                Value = hv;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null) return false;

            // Casting a single-layer stack back down to a grid is unambiguous
            // and convenient; anything more would have to pick a layer, which
            // is what the layer-extraction component is for.
            if (Value.Count == 1 && typeof(Q).IsAssignableFrom(typeof(GH_Grid)))
            {
                object goo = new GH_Grid(Value[0].Grid);
                target = (Q)goo;
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(GH_String)))
            {
                object goo = new GH_String(Value.ToString());
                target = (Q)goo;
                return true;
            }

            return false;
        }

        #endregion
    }
}
