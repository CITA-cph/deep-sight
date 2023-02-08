using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSight
{
    public class LayeredGrid
    {
        public Dictionary<string, Grid> Layers;

        public LayeredGrid()
        {
            Layers = new Dictionary<string, Grid>();
        }

        public Grid this[string key]
        {
            get
            {
                return Layers[key];
            }
            set
            {
                value.Name = key;
                Layers[key] = value;
            }
        }

        public float[] SampleAll(float x, float y, float z)
        {
            var samples = new float[Layers.Count];
            int i = 0;
            foreach (var grid in Layers)
            {
                samples[i] = grid.Value[x, y, z];
                i++;
            }
            return samples;
        }
    }
}
