using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Physics_Simulation
{
    public class Ball
    {
        public double Mass { get; set; }
        public Vector Velocity { get; set; }
        public Point Position { get; set; }
        public Point TargetPosition { get; set; }
        public Vector MouseOffset { get; set; }
        public Vector LastForce { get; set; }
        public double Radius { get; private set; }

        public Vector LastVelocity { get; set; }

        public bool IsDragging { get; set; }
        public Ball(double mass, Point initialPosition, double radius)
        {
            Mass = mass;
            Position = initialPosition;
            Velocity = new Vector(0, 0);
            TargetPosition = initialPosition;
            Radius = radius; // Inicjalizujemy promień
        }

        public void ApplyForce(Vector force)
        {
            if (IsDragging)
            {
                Vector acceleration = force / Mass;
                Velocity += acceleration;
            }
        }

        public void Move()
        {
            Position = new Point(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }
    }
}

