// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SceneLoaderComponent;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Compositor compositor;
        SceneVisual sceneVisual;

        public MainPage()
        {
            this.InitializeComponent();

            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            var root = compositor.CreateContainerVisual();

            root.Size = new System.Numerics.Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            sceneVisual = SceneVisual.Create(compositor);
            root.Children.InsertAtTop(sceneVisual);

            sceneVisual.Offset = new System.Numerics.Vector3(300, 300, 0);
            sceneVisual.RotationAxis = new System.Numerics.Vector3(0, 1, 0);

            var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();

            rotationAnimation.InsertKeyFrame(0f, 0.0f, compositor.CreateLinearEasingFunction());
            rotationAnimation.InsertKeyFrame(0.5f, 360.0f, compositor.CreateLinearEasingFunction());
            rotationAnimation.InsertKeyFrame(1f, 0.0f, compositor.CreateLinearEasingFunction());

            rotationAnimation.Duration = TimeSpan.FromSeconds(8);
            rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            sceneVisual.StartAnimation("RotationAngleInDegrees", rotationAnimation);
        }
        async Task<SceneNode> LoadGLTF(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            IBuffer buffer = await FileIO.ReadBufferAsync(storageFile);

            SceneLoader loader = new SceneLoader();
            return loader.Load(buffer, compositor);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var sceneNode = await LoadGLTF(new Uri("ms-appx:///Assets/DamagedHelmet.gltf"));

            sceneVisual.Root = sceneNode;
        }
    }
}
