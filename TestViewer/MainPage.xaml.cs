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

namespace TestViewer
{
    /// <summary>
    /// A page that generates a gltf image using the SceneLoaderComponent.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly Compositor _compositor;
        readonly SceneVisual _sceneVisual;

        public MainPage()
        {
            this.InitializeComponent();

            _compositor = Window.Current.Compositor;

            var root = _compositor.CreateContainerVisual();

            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            _sceneVisual = SceneVisual.Create(_compositor);
            root.Children.InsertAtTop(_sceneVisual);

            _sceneVisual.Offset = new Vector3(300, 300, 0);
            _sceneVisual.RotationAxis = new Vector3(0, 1, 0);

            var rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();

            rotationAnimation.InsertKeyFrame(0, 0, _compositor.CreateLinearEasingFunction());
            rotationAnimation.InsertKeyFrame(0.5f, 360, _compositor.CreateLinearEasingFunction());
            rotationAnimation.InsertKeyFrame(1, 0, _compositor.CreateLinearEasingFunction());

            rotationAnimation.Duration = TimeSpan.FromSeconds(8);
            rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _sceneVisual.StartAnimation("RotationAngleInDegrees", rotationAnimation);
        }

        async Task<SceneNode> LoadGLTF(string fileStr)
        {
            string[] parts = fileStr.Split('.');
            string fileExtension = parts[parts.Length - 1];

            Uri uri = new Uri(fileStr);
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            var loader = new SceneLoader();

            if (fileExtension == "glb")
            {
                return loader.Load(buffer, _compositor);
            }
            else if (fileExtension == "gltf")
            {
                return loader.LoadGLB(buffer, _compositor, uri.LocalPath);
            }
            else
            {
                Console.WriteLine("Extension: " + fileExtension);
                return null;
            }
        }

        async void Page_Loaded(object sender, RoutedEventArgs e) => _sceneVisual.Root = await LoadGLTF("ms-appx:///Assets/DamagedHelmet.gltf");
    }
}
