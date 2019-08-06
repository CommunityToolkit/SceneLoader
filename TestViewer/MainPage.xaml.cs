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

namespace TestViewer
{
    /// <summary>
    /// A page that generates a gltf image using the SceneLoaderComponent.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly Compositor _compositor;
        readonly SceneVisual _sceneVisual;
        private Viewport _viewport;

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

        public MainPage()
        {
            this.InitializeComponent();

            _compositor = Window.Current.Compositor;

            var root = _compositor.CreateContainerVisual();

            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            // Should the sprite visual sizes change with screen size change?
            v1 = _compositor.CreateSpriteVisual();
            v1.Size = new Vector2((2f / 3f) * (float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);
            v1.Offset = new Vector3(0, 0, -100);
            v1.Brush = _compositor.CreateColorBrush(Colors.SeaGreen);
            root.Children.InsertAtTop(v1);

            v2 = _compositor.CreateSpriteVisual();
            v2.Size = new Vector2((1f / 3f) * (float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);
            v2.Offset = new Vector3(v1.Size.X, 0, -100);
            v2.Brush = _compositor.CreateColorBrush(Colors.Black);
            root.Children.InsertAtTop(v2);
            //root.Children.InsertBelow(v2, v1);

            // scene visual
            _sceneVisual1 = SceneVisual.Create(_compositor);
            v1.Children.InsertAtTop(_sceneVisual1);
            _sceneVisual1.Size = v1.Size;
            // viewport & camera
            _viewport1 = new Viewport(_sceneVisual1);
            cam1 = new OrbitalCamera();
            _viewport1.Camera = cam1;
            cam1.Latitude = MathF.PI / 2;
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
                // rotates proportionately to size of mouse movement
                Vector2 newPos = args.CurrentPoint.Position.ToVector2();
                float longitudeDelta = newPos.X - mouseDownLocation.X;
                float latitudeDelta = newPos.Y - mouseDownLocation.Y;

                // higher number corresponds to faster rotation
                float sensitivity = 0.0002f;

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
            _sceneVisual2.Root = await LoadGLTF(new Uri("ms-appx:///Assets/OrientationTest.gltf"));
        }
    }
}
