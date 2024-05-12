using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Physics_Simulation
{
    public partial class MainWindow : Window
    {
        private static readonly Vector ZeroVector = new Vector(0, 0);
        private Point StartPosition;

        private Ball ball;
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer magneticIndicatorTimer;
        private readonly DispatcherTimer windArrowTimer;

        private Point lastMousePosition;
        private Point magneticCenter;
        private Point canvasCenter;

        private int tickCounter = 0;
        private bool isFrictionEnabled = false;
        private bool isGravityEnabled = false;
        private bool isMagneticAttractionEnabled = false;
        private bool isMagneticRepulsionEnabled = false;
        private bool isWindEnabled = false;

        private double windStrength = 0.5;
        private Vector windForce = ZeroVector;

        private static readonly double Gravity = 3;
        private static readonly double MaxSpeed = 15;

        private static readonly Material metal = new Material("Metal", Colors.Gray, 0.5, 0.05, 7.5);
        private static readonly Material rubber = new Material("Rubber", Colors.Red, 1.1, 0.1, 1.3);
        private static readonly Material ice = new Material("Ice", Colors.White, 0.7, 0.001, 0.9);
        private static readonly Material stone = new Material("Stone", Colors.DarkGray, 0.4, 0.09, 2.5);
        private static readonly Material plastic = new Material("Plastic", Colors.Blue, 0.6, 0.08, 1.07);

        private Material currentMaterial = plastic;

        public MainWindow()
        {
            InitializeComponent();

            magneticIndicatorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            magneticIndicatorTimer.Tick += (s, e) => {
                magneticIndicator.Opacity = 0;
                magneticIndicatorTimer.Stop();
            };

            windArrowTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            windArrowTimer.Tick += (s, e) => {
                windArrow.Opacity = 0;
                windArrowTimer.Stop();
            };

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            canvasCenter = new Point(myCanvas.ActualWidth / 2, myCanvas.ActualHeight / 2);
            StartPosition = new Point(canvasCenter.X - myEllipse.Width / 2, canvasCenter.Y - myEllipse.Width / 2);

            double ballRadius = myEllipse.Width / 2;
            ball = new Ball(currentMaterial, StartPosition, ballRadius);

            ChangeMaterial(currentMaterial);

            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);

            MessageBox.Show(
              "Instrukcja obsługi:\n" +
              "- Użyj przycisków po lewej stronie, aby wybrać, jaka siła ma działać na piłkę.\n" +
              "- Kliknij przyciski z nazwami materiałów, aby zmienić właściwości piłki.\n" +
              "- Użyj myszki do przeciągania piłki.\n" +
              "- W trybie wiatru, steruj kierunkiem wiatru przy pomocy strzałek.\n",
              "Witam w symulacji fizyki!",
              MessageBoxButton.OK,
              MessageBoxImage.Information
            );
        }

        private Vector LimitSpeed(Vector velocity)
        {
            double x = Math.Max(Math.Min(velocity.X, MaxSpeed), -MaxSpeed);
            double y = Math.Max(Math.Min(velocity.Y, MaxSpeed), -MaxSpeed);
            return new Vector(x, y);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ball.IsDragging) return;

            if (ball.Velocity.Length < 0.01)
            {
                ball.Velocity = ZeroVector;
                return;
            }

            ball.Move();
            velocityDisplay.Text = $"Prędkość: {ball.Velocity.Length:F2}";

            if (isGravityEnabled)
            {
                double increment = 0.1 * Gravity * ball.Mass;
                Vector gravityForce = new Vector(0, increment);
                ball.ApplyForce(gravityForce, LimitSpeed);

                if (IsOnGround())
                {
                    Vector groundFriction = -currentMaterial.Friction * ball.Velocity;
                    ball.ApplyForce(groundFriction, LimitSpeed);
                }
            }

            if (isWindEnabled)
            {
                ball.ApplyForce(windForce, LimitSpeed);
            }

            if (isMagneticAttractionEnabled || isMagneticRepulsionEnabled)
            {
                Vector toCenter = new Vector(magneticCenter.X - ball.Position.X, magneticCenter.Y - ball.Position.Y);
                double distance = toCenter.Length;

                if (isMagneticAttractionEnabled)
                {
                    Vector magneticForce = (toCenter / distance) * 0.5;
                    ball.ApplyForce(magneticForce, LimitSpeed);
                }

                if (isMagneticRepulsionEnabled)
                {
                    Vector magneticForce = (toCenter / distance) * -0.3;
                    ball.ApplyForce(magneticForce, LimitSpeed);
                }
            }

            tickCounter++;
            if (tickCounter % 3 == 0)
            {
                CheckForCollision();

                if (isFrictionEnabled)
                {
                    Vector friction = -currentMaterial.Friction * ball.Velocity;
                    ball.ApplyForce(friction, LimitSpeed);
                }
            }

            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
        }

        private bool IsOnGround()
        {
            return ball.Position.Y + myEllipse.Height >= myCanvas.ActualHeight;
        }

        private void CheckForCollision()
        {
            double canvasWidth = myCanvas.ActualWidth;
            double canvasHeight = myCanvas.ActualHeight;

            double restitution = currentMaterial.CoefficientOfRestitution;

            if (ball.Position.X < 0)
            {
                ball.Velocity = LimitSpeed(
                  new Vector(
                    Math.Abs(ball.Velocity.X) * (restitution),
                    ball.Velocity.Y
                  )
                );
                ball.Position = new Point(0, ball.Position.Y);
                return;
            }

            if (ball.Position.X + myEllipse.Width >= canvasWidth)
            {
                ball.Velocity = LimitSpeed(
                  new Vector(
                    -Math.Abs(ball.Velocity.X) * (restitution),
                    ball.Velocity.Y
                  )
                );
                ball.Position = new Point(canvasWidth - myEllipse.Width, ball.Position.Y);
                return;
            }

            if (ball.Position.Y < 0)
            {
                ball.Velocity = LimitSpeed(
                  new Vector(
                    ball.Velocity.X,
                    Math.Abs(ball.Velocity.Y) * (restitution)
                  )
                );
                ball.Position = new Point(ball.Position.X, 0);
                return;
            }

            if (ball.Position.Y + myEllipse.Height >= canvasHeight)
            {
                if (isGravityEnabled)
                {
                    restitution -= 0.1;
                }

                ball.Velocity = LimitSpeed(
                  new Vector(
                    ball.Velocity.X,
                    -Math.Abs(ball.Velocity.Y) * (restitution)
                  )
                );
                ball.Position = new Point(ball.Position.X, canvasHeight - myEllipse.Height);

                if (ball.Velocity.Length < 0.01)
                {
                    ball.Velocity = ZeroVector;
                }
            }
        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMagneticAttractionEnabled || isMagneticRepulsionEnabled)
            {
                Point currentPosition = e.GetPosition(myCanvas);
                magneticCenter = currentPosition;

                magneticIndicator.Margin = new Thickness(currentPosition.X - 5, currentPosition.Y - 5, 0, 0);
                magneticIndicator.Opacity = 1;

                magneticIndicatorTimer.Start();
            }

            if (ball.IsDragging)
            {
                Point currentPosition = e.GetPosition(myCanvas);
                Vector movement = new Vector(currentPosition.X - lastMousePosition.X, currentPosition.Y - lastMousePosition.Y);
                ball.Velocity = movement;
                ball.Position = new Point(currentPosition.X - ball.MouseOffset.X, currentPosition.Y - ball.MouseOffset.Y);
                lastMousePosition = currentPosition;
                myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(myCanvas);
            double radius = myEllipse.Width / 2;
            Point center = new Point(ball.Position.X + radius, ball.Position.Y + radius);
            double distance = Math.Sqrt(Math.Pow(clickPosition.X - center.X, 2) + Math.Pow(clickPosition.Y - center.Y, 2));

            if (distance <= radius)
            {
                ball.IsDragging = true;
                ball.MouseOffset = new Vector(clickPosition.X - ball.Position.X, clickPosition.Y - ball.Position.Y);
                lastMousePosition = clickPosition;
                ball.Velocity = ZeroVector;
            }
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ball.IsDragging = false;
            ball.MouseOffset = ZeroVector;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isWindEnabled)
            {
                canvasCenter = new Point(myCanvas.ActualWidth / 2, myCanvas.ActualHeight / 2);

                double angle = 0;
                Vector windVector = ZeroVector;

                switch (e.Key)
                {
                    case Key.Up:
                        angle = 180;
                        windVector = new Vector(0, -windStrength);
                        break;
                    case Key.Down:
                        angle = 0;
                        windVector = new Vector(0, windStrength);
                        break;
                    case Key.Left:
                        angle = 90;
                        windVector = new Vector(-windStrength, 0);
                        break;
                    case Key.Right:
                        angle = 270;
                        windVector = new Vector(windStrength, 0);
                        break;
                    default:
                        break;
                }

                windArrow.RenderTransform = new RotateTransform(angle, 0, 0);
                windArrow.Margin = new Thickness(canvasCenter.X, canvasCenter.Y, 0, 0);

                windForce = windVector;
                windArrow.Opacity = 1;
                windArrowTimer.Start();
            }
        }

        private void ToggleWind(object sender, RoutedEventArgs e)
        {

            isGravityEnabled = false;
            isFrictionEnabled = false;
            isMagneticAttractionEnabled = false;
            isMagneticRepulsionEnabled = false;

            var windButton = sender as Button;
            if (isWindEnabled)
            {
                isWindEnabled = false;
                windArrow.Opacity = 0;
            }
            else
            {
                isWindEnabled = true;
                windArrow.Opacity = 1;
            }

            UpdateBackgroundColor();
            forceDisplay.Text = "Siła: " + (isWindEnabled ? "Wiatr" : "Brak");
        }

        private void ToggleMagneticAttraction(object sender, RoutedEventArgs e)
        {

            isGravityEnabled = false;
            isFrictionEnabled = false;
            isMagneticRepulsionEnabled = false;
            isWindEnabled = false;

            var attractionButton = sender as Button;
            if (isMagneticAttractionEnabled)
            {
                isMagneticAttractionEnabled = false;
                magneticIndicator.Opacity = 0;
                magneticIndicatorTimer.Stop();
            }
            else
            {
                isMagneticAttractionEnabled = true;
                magneticIndicator.Opacity = 1;
                isMagneticRepulsionEnabled = false;
                magneticIndicatorTimer.Start();
            }

            UpdateBackgroundColor();
            forceDisplay.Text = "Siła: " + (isMagneticAttractionEnabled ? "Przyciąganie" : "Brak");
        }

        private void ToggleMagneticRepulsion(object sender, RoutedEventArgs e)
        {

            isWindEnabled = false;
            isGravityEnabled = false;
            isFrictionEnabled = false;
            isMagneticAttractionEnabled = false;

            var repulsionButton = sender as Button;
            if (isMagneticRepulsionEnabled)
            {
                isMagneticRepulsionEnabled = false;
                magneticIndicator.Opacity = 0;
                magneticIndicatorTimer.Stop();
            }
            else
            {
                isMagneticRepulsionEnabled = true;
                magneticIndicator.Opacity = 1;
                isMagneticAttractionEnabled = false;
                magneticIndicatorTimer.Start();
            }

            UpdateBackgroundColor();
            forceDisplay.Text = "Siła: " + (isMagneticRepulsionEnabled ? "Odpychanie" : "Brak");
        }

        private void Gravity_Click(object sender, RoutedEventArgs e)
        {

            isWindEnabled = false;
            isFrictionEnabled = false;
            isMagneticAttractionEnabled = false;
            isMagneticRepulsionEnabled = false;

            if (isGravityEnabled)
            {
                isGravityEnabled = false;

            }
            else
            {
                isGravityEnabled = true;

            }

            UpdateBackgroundColor();
            forceDisplay.Text = "Siła: " + (isGravityEnabled ? "Grawitacja" : "Brak");
        }

        private void Friction_Click(object sender, RoutedEventArgs e)
        {
            isWindEnabled = false;
            isGravityEnabled = false;
            isMagneticAttractionEnabled = false;
            isMagneticRepulsionEnabled = false;

            var frictionButton = sender as Button;
            if (isFrictionEnabled)
            {
                isFrictionEnabled = false;
            }
            else
            {
                isFrictionEnabled = true;
            }

            UpdateBackgroundColor();
            forceDisplay.Text = "Siła: " + (isWindEnabled ? "Wiatr" : "Brak");
        }

        private void UpdateBackgroundColor()
        {
            if (isWindEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightGreen);
            }
            else if (isGravityEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightGray);
            }
            else if (isFrictionEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.BurlyWood);
            }
            else if (isMagneticAttractionEnabled || isMagneticRepulsionEnabled)
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightYellow);
            }
            else
            {
                myCanvas.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }

        private void ChangeMaterial(Material newMaterial)
        {
            currentMaterial = newMaterial;

            ball.Position = StartPosition;
            ball.Velocity = ZeroVector;
            myEllipse.Fill = new SolidColorBrush(currentMaterial.Color);

            massDisplay.Text = $"Masa: {currentMaterial.Mass} kg";
            myEllipse.Margin = new Thickness(ball.Position.X, ball.Position.Y, 0, 0);
            materialDisplay.Text = $"Materiał:  {currentMaterial.Name}";
        }

        private void Metal_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(metal);
        }

        private void Rubber_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(rubber);
        }

        private void Ice_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(ice);
        }

        private void Stone_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(stone);
        }

        private void Plastic_Click(object sender, RoutedEventArgs e)
        {
            ChangeMaterial(plastic);
        }
    }
}