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

        public MainWindow()
        {
            InitializeComponent();

            double radius = myEllipse.Width / 2;
            ball = new Ball(1.0, new Point(50, 50), radius);
            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5); // Ustawiamy interwał na 20 milisekund
            timer.Tick += Timer_Tick; // Dodajemy obsługę zdarzenia Tick
            timer.Start(); // Startujemy timer
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!ball.IsDragging)
            {
                ball.Move(); // Przesuń piłkę zgodnie z jej prędkością
                CheckForCollision(); // Sprawdź kolizje
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0); // Aktualizuj pozycję
            }
        }




        private void CheckForCollision()
        {
            double canvasWidth = myCanvas.ActualWidth;
            double canvasHeight = myCanvas.ActualHeight;

            if (ball.Position.X < 0)
            {
                Console.WriteLine("Collision with left wall");
                ball.Velocity = new Vector(Math.Abs(ball.Velocity.X), ball.Velocity.Y);
                ball.Position = new Point(0, ball.Position.Y);
            }
            else if (ball.Position.X + myEllipse.Width >= canvasWidth)
            {
                Console.WriteLine("Collision with right wall");
                ball.Velocity = new Vector(-Math.Abs(ball.Velocity.X), ball.Velocity.Y);
                ball.Position = new Point(canvasWidth - myEllipse.Width, ball.Position.Y);
            }

            if (ball.Position.Y < 0)
            {
                Console.WriteLine("Collision with top wall");
                ball.Velocity = new Vector(ball.Velocity.X, Math.Abs(ball.Velocity.Y));
                ball.Position = new Point(ball.Position.X, 0);
            }
            else if (ball.Position.Y + myEllipse.Height >= canvasHeight)
            {
                Console.WriteLine("Collision with bottom wall");
                ball.Velocity = new Vector(ball.Velocity.X, -Math.Abs(ball.Velocity.Y));
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
            ball.ApplyForce(new Vector(0, 9.8 * ball.Mass));
        }

        private void Friction_Click(object sender, RoutedEventArgs e)
        {
            // Zastosuj siłę tarcia
            Vector friction = 0.1 * ball.Velocity;
            ball.ApplyForce(-friction);
        }

    }
}
