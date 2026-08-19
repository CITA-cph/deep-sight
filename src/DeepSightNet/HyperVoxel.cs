using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSight
{
    /// <summary>
    /// A named layer inside a <see cref="HyperVoxel"/>.
    /// </summary>
    public class HyperVoxelLayer
    {
        public string Name { get; set; }
        public GridApi Grid { get; set; }

        public HyperVoxelLayer(string name, GridApi grid)
        {
            Name = name;
            Grid = grid;
        }

        /// <summary>OpenVDB type string of the underlying grid (e.g. "Tree_float_5_4_3").</summary>
        public string GridType
        {
            get { return Grid != null && Grid.IsValid ? Grid.Type : "invalid"; }
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", Name, GridType);
        }
    }

    /// <summary>
    /// An ordered, named stack of voxel grids that share a common index space.
    /// </summary>
    /// <remarks>
    /// DESIGN NOTE - why this is a container rather than a new grid type.
    ///
    /// A "hypervoxel" in the sense of several co-registered volumes addressed as
    /// one object is, in OpenVDB terms, exactly what a .vdb file already is: a
    /// set of named grids. Building on that has three concrete advantages over
    /// inventing a new voxel structure:
    ///
    ///   1. No C++ changes. This is pure C# on top of the existing GridApi, so
    ///      it needs no rebuild of deepsight.dll and no OpenVDB work.
    ///   2. It serialises for free. Saving a HyperVoxel is just writing its
    ///      grids to one .vdb, which Houdini, Blender and every other OpenVDB
    ///      tool can already read.
    ///   3. Sparsity is preserved per layer. Packing channels into a Vec3f grid
    ///      (the obvious alternative) forces a voxel active in ONE channel to
    ///      allocate storage in ALL of them, which on sparse scan data can
    ///      multiply memory use for no benefit. It also caps you at 3-4 layers
    ///      and coerces every layer to float.
    ///
    /// OWNERSHIP: a HyperVoxel does NOT own its grids and does not dispose them.
    /// The same GridApi may sit on a Grasshopper canvas in several places at
    /// once, and disposing it from here would pull the native grid out from
    /// under those other references. Dispose grids where they were created.
    ///
    /// ALIGNMENT: see <see cref="Validate"/>. Layers are only meaningfully
    /// comparable if they share an index-to-world transform.
    /// </remarks>
    public class HyperVoxel
    {
        private readonly List<HyperVoxelLayer> m_layers = new List<HyperVoxelLayer>();

        public string Name { get; set; }

        public HyperVoxel()
        {
            Name = "hypervoxel";
        }

        public HyperVoxel(string name)
        {
            Name = string.IsNullOrEmpty(name) ? "hypervoxel" : name;
        }

        public IList<HyperVoxelLayer> Layers
        {
            get { return m_layers; }
        }

        public int Count
        {
            get { return m_layers.Count; }
        }

        public string[] LayerNames
        {
            get { return m_layers.Select(x => x.Name).ToArray(); }
        }

        /// <summary>Layer by index.</summary>
        public HyperVoxelLayer this[int index]
        {
            get
            {
                if (index < 0 || index >= m_layers.Count)
                    throw new IndexOutOfRangeException(
                        string.Format("Layer index {0} is out of range (0..{1}).", index, m_layers.Count - 1));
                return m_layers[index];
            }
        }

        /// <summary>
        /// Add a grid as a new layer. If <paramref name="name"/> is empty the
        /// grid's own name is used; duplicates get a numeric suffix so that
        /// lookup by name stays unambiguous.
        /// </summary>
        public HyperVoxelLayer Add(GridApi grid, string name = null)
        {
            if (grid == null)
                throw new ArgumentNullException("grid");
            if (!grid.IsValid)
                throw new ArgumentException("Cannot add an invalid or disposed grid.", "grid");

            if (string.IsNullOrEmpty(name))
            {
                try { name = grid.Name; }
                catch { name = null; }
            }

            if (string.IsNullOrEmpty(name))
                name = "layer";

            name = MakeUniqueName(name);

            var layer = new HyperVoxelLayer(name, grid);
            m_layers.Add(layer);
            return layer;
        }

        private string MakeUniqueName(string desired)
        {
            if (!m_layers.Any(x => x.Name == desired))
                return desired;

            for (int i = 1; i < int.MaxValue; i++)
            {
                string candidate = string.Format("{0}_{1}", desired, i);
                if (!m_layers.Any(x => x.Name == candidate))
                    return candidate;
            }
            return desired;
        }

        /// <summary>Find a layer by name. Returns null if not present.</summary>
        public HyperVoxelLayer FindLayer(string name)
        {
            return m_layers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The transform of the first layer, used as the reference. Null if empty.</summary>
        public float[] ReferenceTransform
        {
            get
            {
                if (m_layers.Count == 0) return null;
                return m_layers[0].Grid.Transform;
            }
        }

        /// <summary>
        /// Compare two 4x4 transforms element-wise.
        /// </summary>
        public static bool TransformsMatch(float[] a, float[] b, float tolerance = 1e-5f)
        {
            if (a == null || b == null) return false;
            if (a.Length != 16 || b.Length != 16) return false;

            for (int i = 0; i < 16; i++)
            {
                if (Math.Abs(a[i] - b[i]) > tolerance)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check the stack for problems. Returns an empty list if all is well.
        /// </summary>
        /// <remarks>
        /// THIS IS THE IMPORTANT ONE. Every OpenVDB grid carries its own
        /// index-to-world transform. If two layers have different voxel sizes or
        /// origins, then voxel (10,10,10) in layer A and voxel (10,10,10) in
        /// layer B are at DIFFERENT PLACES IN SPACE. Any later operation that
        /// treats layers as co-registered - extracting a layer to compare, or
        /// adding two layers together - would then be silently, invisibly wrong.
        /// Nothing would crash; you would just get meaningless numbers.
        ///
        /// So mismatched transforms are reported here and the merge component
        /// surfaces them as a warning (or an error in Strict mode). The fix is
        /// to resample the offending grid onto the reference transform first,
        /// which the existing GridResample component already does.
        /// </remarks>
        public IList<string> Validate()
        {
            var problems = new List<string>();

            if (m_layers.Count == 0)
            {
                problems.Add("HyperVoxel contains no layers.");
                return problems;
            }

            for (int i = 0; i < m_layers.Count; i++)
            {
                if (m_layers[i].Grid == null || !m_layers[i].Grid.IsValid)
                    problems.Add(string.Format("Layer {0} ('{1}') is null or disposed.", i, m_layers[i].Name));
            }

            float[] reference = null;
            try { reference = ReferenceTransform; }
            catch { problems.Add("Could not read the reference transform from layer 0."); return problems; }

            for (int i = 1; i < m_layers.Count; i++)
            {
                if (m_layers[i].Grid == null || !m_layers[i].Grid.IsValid) continue;

                float[] t;
                try { t = m_layers[i].Grid.Transform; }
                catch { problems.Add(string.Format("Could not read the transform of layer {0}.", i)); continue; }

                if (!TransformsMatch(reference, t))
                {
                    problems.Add(string.Format(
                        "Layer {0} ('{1}') has a different transform from layer 0 ('{2}'). " +
                        "Voxel coordinates will not line up between these layers; resample it onto " +
                        "the same transform before relying on per-voxel comparisons.",
                        i, m_layers[i].Name, m_layers[0].Name));
                }
            }

            // Mixed value types are allowed, but arithmetic between them is not
            // meaningful without a conversion, so it is worth saying out loud.
            var types = m_layers
                .Where(x => x.Grid != null && x.Grid.IsValid)
                .Select(x => x.GridType)
                .Distinct()
                .ToArray();

            if (types.Length > 1)
            {
                problems.Add(string.Format(
                    "Layers have {0} different grid types ({1}). This is allowed, but arithmetic " +
                    "between layers of different types needs an explicit conversion.",
                    types.Length, string.Join(", ", types)));
            }

            return problems;
        }

        public override string ToString()
        {
            if (m_layers.Count == 0)
                return string.Format("HyperVoxel '{0}' (empty)", Name);

            return string.Format("HyperVoxel '{0}' ({1} layers: {2})",
                Name, m_layers.Count, string.Join(", ", LayerNames));
        }
    }
}
