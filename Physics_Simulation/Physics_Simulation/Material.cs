using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Physics_Simulation
{
    public class Material
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public double CoefficientOfRestitution { get; set; } // Współczynnik odbicia
        public double Friction { get; set; } // Współczynnik tarcia

        public Material(string name, Color color, double restitution, double friction)
        {
            Name = name;
            Color = color;
            CoefficientOfRestitution = restitution;
            Friction = friction;
        }
    }

}
