// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using SceneLoaderComponent;
using CameraComponent;
using Windows.ApplicationModel.Appointments.DataProvider;
using Windows.UI;
using Windows.UI.Core;
using Windows.System;
using System.Diagnostics;

namespace TestViewer
{
    /// <summary>
    /// A page that generates a gltf image using the SceneLoaderComponent.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly Compositor _compositor;

        readonly SceneVisual _sceneVisual1;
        readonly SceneVisual _sceneVisual2;
        private Viewport _viewport1;
        private Viewport _viewport2;
        private SpriteVisual v1;
        private SpriteVisual v2;
        private OrbitalCamera cam1;
        private OrbitalCamera cam2;
        private Vector2 mouseDownLocation;
        private bool mouseDowned = false;

        private ScalarKeyFrameAnimation latitudeAnimation;
        private ScalarKeyFrameAnimation longitudeAnimation;
        private ScalarKeyFrameAnimation radiusAnimation;
        private ScalarKeyFrameAnimation projectionAnimation;

        public MainPage()
        {
            this.InitializeComponent();

            _compositor = Window.Current.Compositor;

            var root = _compositor.CreateContainerVisual();

            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(ModelHost, root);

            // Should the sprite visual sizes change with screen size change?
            v1 = _compositor.CreateSpriteVisual();
            v1.Size = new Vector2((2f / 3f) * (float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);
            v1.Offset = new Vector3(0, 0, 0);
            v1.Brush = _compositor.CreateColorBrush(Colors.SeaGreen);
            root.Children.InsertAtTop(v1);

            v2 = _compositor.CreateSpriteVisual();
            v2.Size = new Vector2((1f / 3f) * (float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);
            v2.Offset = new Vector3(v1.Size.X, 0, 0);

            v2.Brush = _compositor.CreateColorBrush(Colors.Black);
            v2.Clip = _compositor.CreateInsetClip();
            root.Children.InsertAtTop(v2);
            //root.Children.InsertBelow(v2, v1);

            _sceneVisual1 = SceneVisual.Create(_compositor);
            v1.Children.InsertAtTop(_sceneVisual1);
            _sceneVisual1.Size = v1.Size;
            // viewport & camera
            _viewport1 = new Viewport(_sceneVisual1);
            cam1 = new OrbitalCamera();
            _viewport1.Camera = cam1;
            _viewport1.Size = _sceneVisual1.Size;

            // scene visual
            _sceneVisual2 = SceneVisual.Create(_compositor);

            v2.Children.InsertAtTop(_sceneVisual2);
            _sceneVisual2.Size = v2.Size;
            // viewport & camera
            _viewport2 = new Viewport(_sceneVisual2);
            cam2 = new OrbitalCamera();
            _viewport2.Camera = cam2;
            _viewport2.Size = _sceneVisual2.Size;


            cam1.UseAnimations = cam2.UseAnimations = true;

            // inital positions for cameras
            cam1.Latitude = MathF.PI / 2;
            cam2.Latitude = MathF.PI / 4;
            cam2.Longitude = MathF.PI / 4;
            cam2.Radius += 1100;

            





            _viewport1.Camera.UseAnimations = true;

            Home(null, null);

            // Keyboard Handler
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            // Pointer Handler
            Window.Current.CoreWindow.PointerWheelChanged += CoreWindow_PointerWheelChanged;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;
            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;


            //_sceneVisual = SceneVisual.Create(_compositor);
            //root.Children.InsertAtTop(_sceneVisual);


            //_viewport = new Viewport(_sceneVisual);

            Window.Current.SizeChanged += Current_SizeChanged;

            //OrbitalCamera camera = new OrbitalCamera();
            //_viewport.Camera = camera;

            //camera.Longitude = MathF.PI / 2f;
            //camera.Latitude = MathF.PI / 2f;
            //camera.Projection = new PerspectiveProjection();

            //camera.UseAnimations = false;




            //var longitudeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            //longitudeAnimation.InsertExpressionKeyFrame(0.0f, "This.StartingValue");
            //longitudeAnimation.InsertExpressionKeyFrame(0.33f, "This.StartingValue + Pi", _compositor.CreateLinearEasingFunction());
            //longitudeAnimation.InsertExpressionKeyFrame(0.66f, "This.StartingValue + (2 * Pi)", _compositor.CreateLinearEasingFunction());
            //longitudeAnimation.InsertExpressionKeyFrame(1f, "This.StartingValue", _compositor.CreateLinearEasingFunction());
            ////longitudeAnimation.InsertKeyFrame(0.0f, 0f);
            ////longitudeAnimation.InsertKeyFrame(1f, MathF.PI, _compositor.CreateLinearEasingFunction());
            //longitudeAnimation.Duration = TimeSpan.FromSeconds(12);
            //longitudeAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            //camera.StartAnimation("Longitude", longitudeAnimation);

            //var radiusAnimation = _compositor.CreateScalarKeyFrameAnimation();
            //radiusAnimation.InsertExpressionKeyFrame(0.0f, "This.StartingValue");
            //radiusAnimation.InsertExpressionKeyFrame(0.5f, "This.StartingValue + 600f", _compositor.CreateLinearEasingFunction());
            //radiusAnimation.InsertKeyFrame(1f, 600f, _compositor.CreateLinearEasingFunction());
            //radiusAnimation.Duration = TimeSpan.FromSeconds(3);
            //radiusAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            //camera.StartAnimation("Radius", radiusAnimation);
        }

        private void CoreWindow_PointerWheelChanged(CoreWindow sender, PointerEventArgs args)
        {
            RadiusAnimation.IsChecked = false;

            // positive if scroll away from the user, negative if scroll toward the user
            int scrollSign = Math.Sign(args.CurrentPoint.Properties.MouseWheelDelta);

            // determines how much to increase or decrease camera's radius by with each scroll
            // smaller numbers correspond to greater changes in radius
            float sensitivity = 0.95f;

            // scroll away from you

            if (scrollSign > 0)
            {
                cam1.Radius *= sensitivity;
            }
            // scroll towards you
            else if (scrollSign < 0)
            {
                cam1.Radius *= 1 + (1 - sensitivity);
            }
        }
        private void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            if (mouseDowned)
            {
                LatitudeAnimation.IsChecked = false;
                LongitudeAnimation.IsChecked = false;

                // rotates proportionately to size of mouse movement
                Vector2 newPos = args.CurrentPoint.Position.ToVector2();
                float longitudeDelta = newPos.X - mouseDownLocation.X;
                float latitudeDelta = newPos.Y - mouseDownLocation.Y;

                mouseDownLocation = newPos;

                // higher number corresponds to faster rotation
                float sensitivity = 0.005f;

                // changes camera's latitude and longitude based on the sensitivity and size of mouse movement
                cam1.Longitude -= sensitivity * longitudeDelta;
                cam1.Latitude -= sensitivity * latitudeDelta;
            }
        }
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            // use to determine how much to rotate camera by, based on size of change in pointer position
            mouseDownLocation = args.CurrentPoint.Position.ToVector2();
            // indicate that the mouse has been pressed
            mouseDowned = true;
        }
        private void CoreWindow_PointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            // prohibit mouse from changing rotation if the mouse button isn't pressed
            mouseDowned = false;
        }

        private void Dispatcher_AcceleratorKeyActivated(Windows.UI.Core.CoreDispatcher sender, Windows.UI.Core.AcceleratorKeyEventArgs args)
        {
            if (args.EventType == CoreAcceleratorKeyEventType.KeyDown)
            {
                VirtualKey pressed = args.VirtualKey;

                switch (pressed)
                {
                    case VirtualKey.W:
                        cam1.Latitude += -MathF.PI / 20;
                        break;
                    case VirtualKey.S:
                        cam1.Latitude += MathF.PI / 20;
                        break;
                    case VirtualKey.A:
                        cam1.Longitude += -MathF.PI / 20;
                        break;
                    case VirtualKey.D:
                        cam1.Longitude += MathF.PI / 20;
                        break;
                    case VirtualKey.Q:
                        cam1.Radius += -50f;
                        break;
                    case VirtualKey.E:
                        cam1.Radius += 50f;
                        break;
                }
            }
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Vector2 newSize = e.Size.ToVector2(); ;
            v1.Size = new Vector2((2f / 3f) * newSize.X, newSize.Y);
            _viewport1.Size = v1.Size;
            v2.Size = new Vector2((1f / 3f) * newSize.X, newSize.Y);
            _viewport2.Size = v2.Size;
            v2.Offset = new Vector3(v1.Size.X, 0, 0);
            
        }

        async Task<SceneNode> LoadGLTF(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            var loader = new SceneLoader();
            return loader.Load(buffer, _compositor);
        }

        //async void Page_Loaded(object sender, RoutedEventArgs e) => _sceneVisual.Root = await LoadGLTF(new Uri("ms-appx:///Assets/DamagedHelmet.gltf"));
        async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _sceneVisual1.Root = await LoadGLTF(new Uri("ms-appx:///Assets/OrientationTest.gltf"));
            _sceneVisual2.Root = SceneNode.Create(_compositor);
            SceneNode orientateModel = await LoadGLTF(new Uri("ms-appx:///Assets/OrientationTest.gltf"));
            SceneNode camModel = await LoadGLTF(new Uri("ms-appx:///Assets/cam.gltf"));
            camModel.Transform.Scale *= 3*orientateModel.Transform.Scale;
            _sceneVisual2.Root.Children.Insert(0, orientateModel);

            SceneNode yawNode = SceneNode.Create(_compositor);
            SceneNode pitchNode = SceneNode.Create(_compositor);
            SceneNode posNode = SceneNode.Create(_compositor);

            _sceneVisual2.Root.Children.Insert(1, posNode);
            posNode.Children.Insert(0, yawNode);
            yawNode.Children.Insert(0, pitchNode);
            pitchNode.Children.Insert(0, camModel);

            //_sceneVisual2.Root.Children.Insert(1, camModel);


            //camModel.Transform.Translation = new Vector3(0, 0, 500);
            camModel.Transform.RotationAxis = new Vector3(0, 1, 0);
            camModel.Transform.RotationAngle = MathF.PI / 2;

            // set up animation
            var fp_cam1PropertySet = cam1.GetCartesianPropertySet();

            var yawExpression = _compositor.CreateExpressionAnimation();
            yawExpression.Expression = "FPCam1.Yaw";
            yawExpression.SetReferenceParameter("FPCam1", fp_cam1PropertySet);
            yawNode.Transform.RotationAxis = new Vector3(0, 1, 0);
            yawNode.Transform.StartAnimation("RotationAngle", yawExpression);

            var pitchExpression = _compositor.CreateExpressionAnimation();
            pitchExpression.Expression = "-FPCam1.Pitch";
            pitchExpression.SetReferenceParameter("FPCam1", fp_cam1PropertySet);
            pitchNode.Transform.RotationAxis = new Vector3(1, 0, 0);
            pitchNode.Transform.StartAnimation("RotationAngle", pitchExpression);


            var posExpression = _compositor.CreateExpressionAnimation();
            posExpression.Expression = "Vector3(FPCam1.Position.X, -FPCam1.Position.Y, FPCam1.Position.Z)";
            posExpression.SetReferenceParameter("FPCam1", fp_cam1PropertySet);
            posNode.Transform.StartAnimation("Translation", posExpression);
        }

        private void LatitudeAnimation_Checked(object sender, RoutedEventArgs e)
        {
            float min = 0;
            float max = MathF.PI;
            float start = cam1.Latitude;
            float p = (start - min) / (max - min);

            Debug.Assert(latitudeAnimation == null);
            latitudeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            latitudeAnimation.InsertKeyFrame(0, start, _compositor.CreateLinearEasingFunction());
            latitudeAnimation.InsertKeyFrame((1 - p) / 2, max, _compositor.CreateLinearEasingFunction());
            latitudeAnimation.InsertKeyFrame((1 - p) / 2 + .5f, min, _compositor.CreateLinearEasingFunction());
            latitudeAnimation.InsertKeyFrame(1f, start, _compositor.CreateLinearEasingFunction());
            latitudeAnimation.Duration = TimeSpan.FromSeconds(5);
            latitudeAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            cam1.StartAnimation("Latitude", latitudeAnimation);
        }

        private void LatitudeAnimation_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.Assert(latitudeAnimation != null);
            cam1.StopAnimation("Latitude");
            latitudeAnimation = null;
        }

        private void LongitudeAnimation_Checked(object sender, RoutedEventArgs e)
        {
            float start = cam1.Longitude;

            Debug.Assert(longitudeAnimation == null);
            longitudeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            longitudeAnimation.InsertKeyFrame(0, start, _compositor.CreateLinearEasingFunction());
            longitudeAnimation.InsertKeyFrame(1, start + MathF.PI * 2, _compositor.CreateLinearEasingFunction());
            longitudeAnimation.Duration = TimeSpan.FromSeconds(8);
            longitudeAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            cam1.StartAnimation("Longitude", longitudeAnimation);
        }

        private void LongitudeAnimation_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.Assert(longitudeAnimation != null);
            cam1.StopAnimation("Longitude");
            longitudeAnimation = null;
        }

        private void RadiusAnimation_Checked(object sender, RoutedEventArgs e)
        {
            float min = 500;
            float max = 1500;
            float start = cam1.Radius;
            float p = (start - min) / (max - min);

            Debug.Assert(radiusAnimation == null);
            radiusAnimation = _compositor.CreateScalarKeyFrameAnimation();
            radiusAnimation.InsertKeyFrame(0, start, _compositor.CreateLinearEasingFunction());
            radiusAnimation.InsertKeyFrame((1 - p) / 2, max, _compositor.CreateLinearEasingFunction());
            radiusAnimation.InsertKeyFrame((1 - p) / 2 + .5f, min, _compositor.CreateLinearEasingFunction());
            radiusAnimation.InsertKeyFrame(1f, start, _compositor.CreateLinearEasingFunction());
            radiusAnimation.Duration = TimeSpan.FromSeconds(14);
            radiusAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            cam1.StartAnimation("Radius", radiusAnimation);
        }

        private void RadiusAnimation_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.Assert(radiusAnimation != null);
            cam1.StopAnimation("Radius");
            radiusAnimation = null;
        }

        private void RadiusAnimation_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (LatitudeAnimation.IsEnabled)
            {
                Debug.Assert(radiusAnimation == null);
                radiusAnimation = _compositor.CreateScalarKeyFrameAnimation();
                radiusAnimation.InsertKeyFrame(0, 500);
                radiusAnimation.InsertKeyFrame(.5f, 1000);
                radiusAnimation.InsertKeyFrame(1, 500);
                radiusAnimation.Duration = TimeSpan.FromSeconds(5);
                radiusAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                cam1.StartAnimation("Radius", radiusAnimation);
            }
            else
            {
                Debug.Assert(radiusAnimation != null);
                cam1.StopAnimation("Radius");
                radiusAnimation = null;
            }
        }

        private void Projection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cam1 == null) return;
            if ((String)((ComboBoxItem)(Projection.SelectedValue)).Content == "Perspective")
            {
                cam1.Projection = new PerspectiveProjection();
            }
            else
            {
                cam1.Projection = new OrthographicProjection();
            }
        }

        private void FOV_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            //if(cam1 != null && cam1.Projection is PerspectiveProjection)
            //{
            //    float fov = (float) FOVSlider.Value / 360 * MathF.PI * 2;
            //    ((PerspectiveProjection)cam1.Projection).XFov = fov;
            //    ((PerspectiveProjection)cam1.Projection).YFov = fov;
            //}
        }

        private void Stretch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cam1 == null) return;
            if ((String)((ComboBoxItem)(Stretch.SelectedValue)).Content == "Fill")
            {
                _viewport1.Stretch = CameraComponent.Stretch.Fill;
            }
            else if ((String)((ComboBoxItem)(Stretch.SelectedValue)).Content == "Fixed X FOV")
            {
                _viewport1.Stretch = CameraComponent.Stretch.FixX;
            }
            else if ((String)((ComboBoxItem)(Stretch.SelectedValue)).Content == "Fixed Y FOV")
            {
                _viewport1.Stretch = CameraComponent.Stretch.FixY;
            }
            else if ((String)((ComboBoxItem)(Stretch.SelectedValue)).Content == "Uniform")
            {
                _viewport1.Stretch = CameraComponent.Stretch.Uniform;
            }
            else if ((String)((ComboBoxItem)(Stretch.SelectedValue)).Content == "Uniform To Fill")
            {
                _viewport1.Stretch = CameraComponent.Stretch.UniformToFill;
            }
        }

        private void Home(object sender, RoutedEventArgs e)
        {
            LongitudeAnimation.IsChecked = false;
            LatitudeAnimation.IsChecked = false;
            RadiusAnimation.IsChecked = false;

            UniformStretch.IsSelected = true;
            PerspectiveProjection.IsSelected = true;

            cam1.Latitude = MathF.PI / 3;
            cam1.Longitude = MathF.PI / 3;
            cam1.Radius = 800;
            cam1.Projection = new PerspectiveProjection();
            _viewport1.Stretch = CameraComponent.Stretch.Uniform;
        }
    }
}
