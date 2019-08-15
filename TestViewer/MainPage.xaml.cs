// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using SceneLoaderComponent;
using CameraComponent;

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

        public MainPage()
        {
            this.InitializeComponent();

            // Create a camera and a ContainerVisual
            _compositor = Window.Current.Compositor;
            var root = _compositor.CreateContainerVisual();
            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            // create a SceneVisual
            _sceneVisual = SceneVisual.Create(_compositor);
            root.Children.InsertAtTop(_sceneVisual);

            // instantiate a viewport and a camera
            _viewport = new Viewport(_compositor);
            _viewport.AttachToVisual(_sceneVisual);
            OrbitalCamera camera = new OrbitalCamera(_compositor);
            _viewport.Camera = camera;

            // set the viewport's size to the size of the window
            _viewport.Size = new Vector2((float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);

            // change the camera's properties 
            camera.Longitude = MathF.PI / 2f;
            camera.Latitude = MathF.PI / 2f;
            camera.Projection = new PerspectiveProjection(_compositor);

            // add event handler for window size change
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
        }

        private void CoreWindow_SizeChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowSizeChangedEventArgs args)
        {
            _viewport.Size = args.Size.ToVector2();
        }

        async Task<SceneNode> LoadGLTF(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            var loader = new SceneLoader();
            return loader.Load(buffer, _compositor);
        }

        async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadGLTF(new Uri("ms-appx:///Assets/DamagedHelmet.gltf"));
    }
}
