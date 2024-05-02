using System.Diagnostics.Metrics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Physics_Simulation
{
    /// <summary>
    /// Główne okno aplikacji symulującej fizykę.
    /// </summary>
    public partial class MainWindow : Window
    {
        private Ball ball; // Instancja piłki
        private Point lastMousePosition; // Ostatnia pozycja myszy
        private DispatcherTimer timer; // Timer do obsługi animacji
        private int tickCounter = 0; // Licznik tików, używany do optymalizacji
        private bool isFrictionEnabled = false; // Flaga kontrolująca, czy tarcie jest włączone
        private const double Gravity = 3; // Stała grawitacji
        private bool isGravityEnabled = false; // Flaga kontrolująca, czy grawitacja jest włączona
        private readonly Point startPosition = new Point(50, 50); // Pozycja startowa piłki

        // Definicje materiałów
        Material metal = new Material("Metal", Colors.Gray, 0.5, 0.05, 7.5);
        Material rubber = new Material("Rubber", Colors.Red, 1.1, 0.1, 1.3);
        Material ice = new Material("Ice", Colors.White, 0.7, 0.001, 0.9);
        Material stone = new Material("Stone", Colors.DarkGray, 0.4, 0.09, 2.5);
        Material plastic = new Material("Plastic", Colors.Blue, 0.6, 0.08, 1.07);

        private Material currentMaterial; // Aktualny materiał używany przez piłkę
        private const double MaxSpeed = 10.0; // Maksymalna prędkość w dowolnym kierunku

        // Ograniczanie prędkości
        private Vector LimitSpeed(Vector velocity)
        {
            double x = Math.Max(Math.Min(velocity.X, MaxSpeed), -MaxSpeed); // Ogranicz prędkość X
            double y = Math.Max(Math.Min(velocity.Y, MaxSpeed), -MaxSpeed); // Ogranicz prędkość Y
            return new Vector(x, y);
        }

        // Obsługa zdarzenia Timer_Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!ball.IsDragging) // Jeśli piłka nie jest przeciągana
            {
                // Jeśli prędkość jest bardzo mała, zatrzymaj piłkę
                if (ball.Velocity.Length < 0.01)
                {
                    ball.Velocity = new Vector(0, 0);
                }
                else
                {
                    // Przesuń piłkę
                    ball.Move();

                    // Aktualizuj wyświetlacz prędkości
                    velocityDisplay.Text = $"Prędkość: {ball.Velocity.Length:F2}";

                    // Zastosuj grawitację, jeśli jest włączona
                    if (isGravityEnabled)
                    {
                        double increment = 0.1 * Gravity * ball.Mass; // Siła grawitacji dodawana stopniowo
                        Vector gravityForce = new Vector(0, increment); // Siła skierowana w dół
                        ball.ApplyForce(gravityForce, LimitSpeed);

                        // Dodaj tarcie, jeśli piłka jest na ziemi
                        if (IsOnGround())
                        {
                            Vector groundFriction = -currentMaterial.Friction * ball.Velocity;
                            ball.ApplyForce(groundFriction, LimitSpeed);
                        }
                    }

                    tickCounter++;
                    if (tickCounter % 3 == 0) // Sprawdź kolizje co trzeci tik
                    {
                        CheckForCollision();

                        // Zastosuj tarcie, jeśli jest włączone
                        if (isFrictionEnabled)
                        {
                            Vector friction = -currentMaterial.Friction * ball.Velocity;
                            ball.ApplyForce(friction, LimitSpeed);
                        }
                    }
                }

                // Aktualizacja pozycji piłki
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
            }
        }

        // Funkcja sprawdzająca, czy piłka jest na ziemi
        private bool IsOnGround()
        {
            return ball.Position.Y + myEllipse.Height >= myCanvas.ActualHeight;
        }

        // Sprawdź kolizje i dostosuj prędkość po odbiciu
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
                if (isGravityEnabled)
                {
                    restitution -= 0.1; // Zmniejsz współczynnik odbicia, aby piłka traciła prędkość
                }
                ball.Velocity = LimitSpeed(new Vector(
                    ball.Velocity.X,
                    -Math.Abs(ball.Velocity.Y) * (restitution)
                ));
                ball.Position = new Point(ball.Position.X, canvasHeight - myEllipse.Height);

                if (ball.Velocity.Length < 0.01)
                {
                    ball.Velocity = new Vector(0, 0); // Zatrzymaj piłkę, jeśli prędkość jest bardzo mała
                }
            }
        }

        // Zdarzenie MouseMove dla przeciągania piłki
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ball.IsDragging)
            {
                Point currentPosition = e.GetPosition(myCanvas);
                Vector movement = currentPosition - lastMousePosition;
                ball.Velocity = movement;
                ball.Position = currentPosition - ball.MouseOffset;
                lastMousePosition = currentPosition;
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
            }
        }

        // Zdarzenie MouseLeftButtonDown dla rozpoczęcia przeciągania
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(myCanvas);
            double radius = myEllipse.Width / 2;
            Point center = new Point(ball.Position.X + radius, ball.Position.Y + radius);
            double distance = Math.Sqrt(Math.Pow(clickPosition.X - center.X, 2) + Math.Pow(clickPosition.Y - center.Y, 2));

            if (distance <= radius)
            {
                ball.IsDragging = true;
                ball.MouseOffset = clickPosition - ball.Position;
                lastMousePosition = clickPosition;
                ball.Velocity = new Vector(0, 0);
            }
        }

        // Zdarzenie MouseLeftButtonUp dla zakończenia przeciągania
        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ball.IsDragging = false;
            ball.MouseOffset = new Vector(0, 0);
        }

        // Przełącznik dla grawitacji
        private void Gravity_Click(object sender, RoutedEventArgs e)
        {
            isGravityEnabled = !isGravityEnabled;

            if (isGravityEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }

        // Przełącznik dla tarcia
        private void Friction_Click(object sender, RoutedEventArgs e)
        {
            isFrictionEnabled = !isFrictionEnabled;

            if (isFrictionEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.BurlyWood);
            }
            else
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }

        // Zmiana materiału piłki
        private void ChangeMaterial(Material newMaterial)
        {
            currentMaterial = newMaterial;

            // Zresetuj pozycję piłki do startowej
            ball.Position = startPosition;
            ball.Velocity = new Vector(0, 0);
            myEllipse.Fill = new SolidColorBrush(currentMaterial.Color);

            // Aktualizuj informację o masie
            massDisplay.Text = $"Masa: {currentMaterial.Mass} kg";

            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
        }

        // Przycisk do zmiany na metal
        private void Metal_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(metal);
        }

        // Przycisk do zmiany na gumę
        private void Rubber_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(rubber);
        }

        // Przycisk do zmiany na lód
        private void Ice_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(ice);
        }

        // Przycisk do zmiany na kamień
        private void Stone_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(stone);
        }

        // Przycisk do zmiany na plastik
        private void Plastic_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(plastic);
        }

        // Konstruktor głównego okna
        public MainWindow()
        {
            InitializeComponent();

            ball = new Ball(plastic, startPosition, myEllipse.Width / 2);
            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10); // Interwał timera
            timer.Tick += Timer_Tick;
            timer.Start();

            ChangeMaterial(plastic); // Ustaw domyślny materiał
        }
    }
}
