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
using Windows.System;
using Experimental;
using Windows.UI.Core;

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
        private OrbitalCamera _orbital_cam;
        
        public MainPage()
        {
            this.InitializeComponent();

            _compositor = Window.Current.Compositor;

            var root = _compositor.CreateContainerVisual();

            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            _sceneVisual = SceneVisual.Create(_compositor);
            root.Children.InsertAtTop(_sceneVisual);

            _viewport = new Viewport(_sceneVisual);
            _orbital_cam = new OrbitalCamera();
            _viewport.Camera = _orbital_cam;

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            _viewport.Size = e.Size.ToVector2();
        }

        private void Dispatcher_AcceleratorKeyActivated(Windows.UI.Core.CoreDispatcher sender, Windows.UI.Core.AcceleratorKeyEventArgs args)
        {
            if (args.EventType == CoreAcceleratorKeyEventType.KeyDown)
            {
                VirtualKey pressed = args.VirtualKey;
                switch (pressed)
                {
                    case VirtualKey.W:
                        _orbital_cam.Latitude += -MathF.PI / 20;
                        break;
                    case VirtualKey.S:
                        _orbital_cam.Latitude += MathF.PI / 20;
                        break;
                    case VirtualKey.A:
                        _orbital_cam.Longitude += -MathF.PI / 20;
                        break;
                    case VirtualKey.D:
                        _orbital_cam.Longitude += MathF.PI / 20;
                        break;
                    case VirtualKey.Q:
                        _orbital_cam.Radius += -50f;
                        break;
                    case VirtualKey.E:
                        _orbital_cam.Radius += 50f;
                        break;
                }
            }
        }

        async Task<SceneNode> LoadGLTF(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            var loader = new SceneLoader();
            return loader.Load(buffer, _compositor);
        }

        async void Page_Loaded(object sender, RoutedEventArgs e) => _sceneVisual.Root = await LoadGLTF(new Uri("ms-appx:///Assets/OrientationTest.gltf"));
    }
}
