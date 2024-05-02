using System.Diagnostics.Metrics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Physics_Simulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Ball ball;
        private Point lastMousePosition;
        private DispatcherTimer timer; // Nowy timer
        private int tickCounter = 0;
        private readonly Point startPosition = new Point(50, 50); // Pozycja startowa piłki
        Material metal = new Material("Metal", Colors.Gray, 0.5, 0.2);
        Material rubber = new Material("Rubber", Colors.Red, 1.2, 0.8);
        Material wood = new Material("Wood", Colors.Brown, 0.7, 0.5);
        Material stone = new Material("Stone", Colors.Gray, 0.4, 0.3);
        Material plastic = new Material("Plastic", Colors.Blue, 0.6, 0.4);
        private Material currentMaterial;
        private const double MaxSpeed = 10.0; // Maksymalna prędkość w dowolnym kierunku


        private Vector LimitSpeed(Vector velocity)
        {
            double x = Math.Max(Math.Min(velocity.X, MaxSpeed), -MaxSpeed); // Ogranicz prędkość X
            double y = Math.Max(Math.Min(velocity.Y, MaxSpeed), -MaxSpeed); // Ogranicz prędkość Y
            return new Vector(x, y);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!ball.IsDragging)
            {
                ball.Move(); // Przesuń piłkę zgodnie z jej prędkością

                tickCounter++;
                if (tickCounter % 3 == 0) // Sprawdź kolizje co drugi tik
                {
                    CheckForCollision();
                }
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
            }
        }





        private void CheckForCollision()
        {
            double canvasWidth = myCanvas.ActualWidth;
            double canvasHeight = myCanvas.ActualHeight;

            double restitution = currentMaterial.CoefficientOfRestitution;

            if (ball.Position.X < 0)
            {
                ball.Velocity = LimitSpeed(new Vector(
                    Math.Abs(ball.Velocity.X) * restitution,
                    ball.Velocity.Y
                ));
                ball.Position = new Point(0, ball.Position.Y);
                return;
            }

            if (ball.Position.X + myEllipse.Width >= canvasWidth)
            {
                ball.Velocity = LimitSpeed(new Vector(
                    -Math.Abs(ball.Velocity.X) * restitution,
                    ball.Velocity.Y
                ));
                ball.Position = new Point(canvasWidth - myEllipse.Width, ball.Position.Y);
                return;
            }

            if (ball.Position.Y < 0)
            {
                ball.Velocity = LimitSpeed(new Vector(
                    ball.Velocity.X,
                    Math.Abs(ball.Velocity.Y) * restitution
                ));
                ball.Position = new Point(ball.Position.X, 0);
                return;
            }

            if (ball.Position.Y + myEllipse.Height >= canvasHeight)
            {
                ball.Velocity = LimitSpeed(new Vector(
                    ball.Velocity.X,
                    -Math.Abs(ball.Velocity.Y) * restitution
                ));
                ball.Position = new Point(ball.Position.X, canvasHeight - myEllipse.Height);
            }
        }




        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ball.IsDragging)
            {
                Point currentPosition = e.GetPosition(myCanvas);
                // Oblicz prędkość na podstawie różnicy między obecnym a ostatnim położeniem
                Vector movement = currentPosition - lastMousePosition;
                ball.Velocity = movement; // Ustaw prędkość piłki
                ball.Position = currentPosition - ball.MouseOffset; // Ustaw pozycję piłki
                lastMousePosition = currentPosition; // Zaktualizuj ostatnią pozycję myszy
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0); // Zaktualizuj pozycję na ekranie
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(myCanvas);
            double radius = myEllipse.Width / 2;
            Point center = new Point(ball.Position.X + radius, ball.Position.Y + radius);
            double distance = Math.Sqrt(Math.Pow(clickPosition.X - center.X, 2) + Math.Pow(clickPosition.Y - center.Y, 2));

            if (distance <= radius) // Jeśli kliknięcie jest wewnątrz piłki
            {
                ball.IsDragging = true;
                ball.MouseOffset = clickPosition - ball.Position; // Oblicz przesunięcie względem piłki
                lastMousePosition = clickPosition; // Zapisz początkową pozycję
                ball.Velocity = new Vector(0, 0); // Zresetuj prędkość podczas przeciągania
            }
        }


        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ball.IsDragging = false;
            // Gdy puszczamy przycisk myszy, zerujemy przesunięcie myszy
            ball.MouseOffset = new Vector(0, 0);
        }

        private void Gravity_Click(object sender, RoutedEventArgs e)
        {
            // Zastosuj siłę grawitacji
            ball.ApplyForce(new Vector(0, 9.8 * ball.Mass), LimitSpeed);
        }

        private void Friction_Click(object sender, RoutedEventArgs e)
        {
            // Zastosuj siłę tarcia
            Vector friction = 0.1 * ball.Velocity;
            ball.ApplyForce(-friction, LimitSpeed);
        }

        private void ChangeMaterial(Material newMaterial)
        {
            currentMaterial = newMaterial;

            // Zresetuj pozycję piłki do startowej
            ball.Position = startPosition;
            ball.Velocity = new Vector(0, 0); // Zresetuj prędkość
            myEllipse.Fill = new SolidColorBrush(currentMaterial.Color); // Zmień kolor piłki

            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
        }
        private void Metal_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(metal);
        }

        private void Rubber_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(rubber);
        }

        private void Wood_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(wood);
        }

        private void Stone_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(stone);
        }

        private void Plastic_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(plastic);
        }

        public MainWindow()
        {
            InitializeComponent();
            currentMaterial = plastic;

            double radius = myEllipse.Width / 2;
            ball = new Ball(1.0, new Point(50, 50), radius);
            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10); // Zwiększ interwał, aby zmniejszyć obciążenie
                                                            // Ustawiamy interwał na 20 milisekund
            timer.Tick += Timer_Tick; // Dodajemy obsługę zdarzenia Tick
            timer.Start(); // Startujemy timer

            

        }






    }
}
