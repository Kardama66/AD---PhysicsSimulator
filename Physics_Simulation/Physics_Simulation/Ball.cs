using System;
using System.Windows;
using System.Windows.Shapes;

namespace Physics_Simulation
{
    public class Ball
    {
        public Vector Velocity { get; set; } // Prędkość piłki
        public Point Position { get; set; } // Aktualna pozycja piłki
        public Point TargetPosition { get; set; } // Cel dla piłki
        public Vector MouseOffset { get; set; } // Przesunięcie myszy względem piłki
        public double Radius { get; set; } // Promień piłki

        public Material CurrentMaterial { get; set; } // Aktualny materiał piłki
        public bool IsDragging { get; set; } // Czy piłka jest przeciągana

        public Ball(Material initialMaterial, Point initialPosition, double radius)
        {
            CurrentMaterial = initialMaterial; // Ustaw materiał
            Position = initialPosition; // Ustaw pozycję
            Velocity = new Vector(0, 0); // Początkowa prędkość
            Radius = radius; // Ustaw promień piłki
        }

        public double Mass
        {
            get { return CurrentMaterial.Mass; } // Pobierz masę z bieżącego materiału
        }

        // Dodawanie siły do prędkości piłki
        public void ApplyForce(Vector force, Func<Vector, Vector> limitSpeedFunc)
        {
            Vector acceleration = force / Mass; // Oblicz przyspieszenie
            Velocity += acceleration; // Dodaj przyspieszenie do prędkości

            if (limitSpeedFunc != null)
            {
                Velocity = limitSpeedFunc(Velocity); // Ogranicz prędkość
            }
        }

        // Przesuwanie piłki
        public void Move()
        {
            Position = new Point(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }
    }
}
