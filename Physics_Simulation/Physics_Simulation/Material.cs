using System.Windows.Media;

namespace Physics_Simulation
{
    public class Material
    {
        public string Name { get; set; } // Nazwa materiału
        public Color Color { get; set; } // Kolor materiału
        public double CoefficientOfRestitution { get; set; } // Współczynnik odbicia
        public double Friction { get; set; } // Współczynnik tarcia
        public double Mass { get; set; } // Masa materiału

        // Konstruktor materiału
        public Material(string name, Color color, double restitution, double friction, double mass)
        {
            Name = name;
            Color = color;
            CoefficientOfRestitution = restitution;
            Friction = friction;
            Mass = mass;
        }
    }
}
