using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfApp1Test
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        private List<PlanetaryObject> spheres = new();
        private double distanceFromCenter = 200;
        private bool orbitObject = false;
        private PlanetaryObject orbitingObject = null;
        private double[] orbitPos = new double[3] { 0,0,0 };

        private Point lastMousePos;
        private Point mouseLockPos;
        private Vector mouseDif;
        private int ScrollSpeed = 20;
        private double theta = 0; // Horizontal angle (Yaw/Y-axis rotation)
        private double phi = 0;   // Vertical angle (Pitch/X-axis rotation)
        private readonly double rotationSpeed = 0.5; // Adjust sensitivity

        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += OnRenderFrame;

            // Add a few spheres
            /* CreateSphere takes 3 parameters: center (Point3D), radius (double), color (Color)
                center - position from center
                radious - size of the sphere
                color - color of the sphere

            */

            var sphereData = new (Point3D position, double radius, Color color)[]
            {
                (new Point3D(0, 0, 0), 64, Colors.Red), //64
                (new Point3D(3844, 0, 0), 17, Colors.Blue),
            };

            foreach (var data in sphereData)
            {
                


                
                var profile = new PlanetaryObject
                {
                    CenterPosition = data.position,
                    Radius = data.radius,
                    
                    Name = "Gerald",
                    Description = "Red"
                };
                var sphereVisual = CreateSphere(data.position, data.radius, data.color, profile);
                viewport.Children.Add(sphereVisual);
                spheres.Add(profile);
                profile.Visual = sphereVisual;
            }


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10); // ~33 FPS
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private bool keyCheck_tab = false;

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            double speed = 1;

            // camera orbit object
            if (orbitObject == true)
            {
                if (orbitingObject != null)
                {
                    Point3D center = orbitingObject.CenterPosition;

                    // 1. Convert angles from degrees to radians
                    double thetaRad = theta * Math.PI / 180.0;
                    double phiRad = phi * Math.PI / 180.0;

                    // 2. Calculate spherical coordinates for the camera's position
                    // Note: WPF's Y-axis is often "up", so Z and Y are typically swapped
                    // compared to standard math notation for (r, theta, phi).

                    // Calculate position in the X-Z plane (horizontal circle)
                    double x_proj = distanceFromCenter * Math.Cos(phiRad);
                    double x_local = x_proj * Math.Sin(thetaRad);
                    double z_local = x_proj * Math.Cos(thetaRad);

                    // Calculate Y position (vertical)
                    double y_local = distanceFromCenter * Math.Sin(phiRad);

                    // 3. Set the new camera position relative to the center
                    Point3D newCameraPosition = new Point3D(
                        center.X + x_local,
                        center.Y - y_local,
                        center.Z - z_local
                    );

                    camera.Position = newCameraPosition;

                    // 4. Update LookDirection to always point back to the center
                    // (Center - Camera Position)
                    camera.LookDirection = new Vector3D(
                        center.X - camera.Position.X,
                        center.Y - camera.Position.Y,
                        center.Z - camera.Position.Z
                    );
                }
            }


            if (Keyboard.IsKeyDown(Key.W))
            {
                if (orbitObject == true)
                {
                    distanceFromCenter -= speed;
                }
                else
                {
                    camera.Position = new Point3D(camera.Position.X - speed, camera.Position.Y - speed, camera.Position.Z - speed);
                }
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                if (orbitObject == true)
                {
                    distanceFromCenter += speed;
                }
                else
                {
                    camera.Position = new Point3D(camera.Position.X + speed, camera.Position.Y + speed, camera.Position.Z + speed);
                }
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                orbitObject = false;
            }

            if (Keyboard.IsKeyDown(Key.Tab) && keyCheck_tab == false)
            {
                keyCheck_tab = true;
                orbitObject = true;
                
                if (orbitingObject == null)
                {
                    orbitingObject = spheres[1];
                }
                
                else if (spheres.Contains(orbitingObject) != false)
                {
                    int index = spheres.IndexOf(orbitingObject);
                    if (index + 1 >= spheres.Count)
                    {
                        index = -1;
                    }
                    orbitingObject = spheres[index+1];
                }
                
                
                
                
            }
            else if (!Keyboard.IsKeyDown(Key.Tab))
            {
                keyCheck_tab = false;
            }

            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                
            }

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePos = e.GetPosition(this);
            mouseDif = currentMousePos - lastMousePos;
            lastMousePos = currentMousePos;

            if (Mouse.RightButton == MouseButtonState.Pressed && orbitObject == true && orbitingObject != null)
            {
                // Convert pixel difference to angular difference
                // mouseDif.X corresponds to rotation around Y-axis (theta)
                // mouseDif.Y corresponds to rotation around X-axis (phi)

                theta += mouseDif.X * rotationSpeed;
                phi -= mouseDif.Y * rotationSpeed; // Subtract because positive Y is usually down

                // Clamp the vertical angle (phi) to prevent the camera from flipping over
                // -89 degrees to +89 degrees is safe
                phi = Math.Clamp(phi, -89, 89);
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // The Delta value is usually +120 for one tick up, and -120 for one tick down.
            if (e.Delta > 0)
            {
                // The scroll wheel was moved UP (forward)
                // Perform action for scroll up, e.g., zoom in or move camera closer

                // Example:
                // distanceFromCenter -= scrollSpeed; 

                if (orbitObject == true)
                {
                    distanceFromCenter -= ScrollSpeed;
                }
                else
                {
                    camera.Position = new Point3D(camera.Position.X - ScrollSpeed, camera.Position.Y - ScrollSpeed, camera.Position.Z - ScrollSpeed);
                }
            }
            else if (e.Delta < 0)
            {
                // The scroll wheel was moved DOWN (backward)
                // Perform action for scroll down, e.g., zoom out or move camera farther

                // Example:
                // distanceFromCenter += scrollSpeed;

                if (orbitObject == true)
                {
                    distanceFromCenter += ScrollSpeed;
                }
                else
                {
                    camera.Position = new Point3D(camera.Position.X + ScrollSpeed, camera.Position.Y + ScrollSpeed, camera.Position.Z + ScrollSpeed);
                }
            }

            // Optional: Mark the event as handled to prevent it from propagating 
            // to parent elements or lower-level controls.
            // e.Handled = true; 
        }



        private class PlanetaryObject
        {
            public ModelVisual3D Visual { get; set; }
            public Point3D CenterPosition { get; set; }
            public double Radius { get; init; }

            public double Mass { get; init; }

            
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private double angle = 0;

        private void OnTimerTick(object? sender, EventArgs e)
        {
            angle += 1;

            // rotate around an axis
            //spheres[1].Visual.Transform = 
        }

        private ModelVisual3D CreateSphere(Point3D center, double radius, Color color, PlanetaryObject profile)
        {
            int slices = 20; // horizontal divisions
            int stacks = 10; // vertical divisions

            var mesh = new MeshGeometry3D();

            // Generate vertices
            for (int stack = 0; stack <= stacks; stack++)
            {
                double phi = Math.PI / 2 - stack * Math.PI / stacks; // from +π/2 to -π/2
                double y = radius * Math.Sin(phi);
                double scale = radius * Math.Cos(phi);

                for (int slice = 0; slice <= slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / slices;
                    double x = scale * Math.Cos(theta);
                    double z = scale * Math.Sin(theta);
                    mesh.Positions.Add(new Point3D(center.X + x, center.Y + y, center.Z + z));
                    var normal = new Vector3D(x, y, z);
                    normal.Normalize();
                    mesh.Normals.Add(normal);

                }
            }

            // Generate triangles
            for (int stack = 0; stack < stacks; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    int n = slices + 1;
                    int i1 = stack * n + slice;
                    int i2 = i1 + n;
                    mesh.TriangleIndices.Add(i1);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i1 + 1);
                    mesh.TriangleIndices.Add(i1 + 1);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i2 + 1);
                }
            }

            var material = new DiffuseMaterial(new SolidColorBrush(color));
            var model = new GeometryModel3D(mesh, material);
            model.BackMaterial = model.Material;
            model.SetValue(FrameworkElement.TagProperty, profile);

            return new ModelVisual3D { Content = model };
        }

        private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(viewport);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);

            VisualTreeHelper.HitTest(viewport, null, HitTestResultCallback, hitParams);
        }

        private HitTestResultBehavior HitTestResultCallback(HitTestResult result)
        {
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                var meshHit = rayResult.ModelHit as GeometryModel3D;

                if (meshHit != null)
                {
                    // Retrieve profile data stored in Tag
                    var profile = meshHit.GetValue(FrameworkElement.TagProperty) as PlanetaryObject;

                    ShowProfile(profile);
                }
                else
                {
                    
                }
            }
            return HitTestResultBehavior.Stop;
        }

        private void ShowProfile(PlanetaryObject profile)
        {

            if (profile == null)
                throw new Exception("Profile is NULL");
            if (ProfileName == null)
                throw new Exception("ProfileName TextBlock is NULL");
            if (ProfileDescription == null)
                throw new Exception("ProfileDescription TextBlock is NULL");
            if (ProfilePanel == null)
                throw new Exception("ProfilePanel is NULL");


            ProfileName.Text = profile.Name;
            ProfileDescription.Text = profile.Description;
            ProfilePanel.Visibility = Visibility.Visible;
        }

    }
}
