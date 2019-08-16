// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SceneLoaderComponent;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using System.Numerics;
using Windows.UI.Xaml.Hosting;
using Windows.Storage;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CameraComponent
{
    public sealed partial class ModelViewer : UserControl
    {
        private Compositor _compositor;
        private SceneVisual _sceneVisual;
        private Viewport _viewport;
        private OrbitalCamera _camera;

        private bool _mouseDowned;
        private Vector2 _mouseDownLocation;

        public ModelViewer()
        {
            this.InitializeComponent();

            // Create a camera and a ContainerVisual
            _compositor = Window.Current.Compositor;
            var root = _compositor.CreateContainerVisual();
            root.Size = new Vector2(1000, 1000);
            ElementCompositionPreview.SetElementChildVisual(this, root);

            // create a SceneVisual and insert it into the visual tree
            _sceneVisual = SceneVisual.Create(_compositor);
            root.Children.InsertAtTop(_sceneVisual);

            // instantiate viewport and assign it "good" default values
            _viewport = new Viewport(_compositor);
            _viewport.AttachToVisual(_sceneVisual);
            _viewport.Size = Target.ActualSize;
            _viewport.Offset = new Vector3(_viewport.Size / 2f, 0f);

            // instantiate camera and assign it "good" default values
            _camera = new OrbitalCamera(_compositor);
            _camera.Target = new Vector3(0f, 0f, 0f);
            _camera.Radius = 600f;
            _camera.Theta = 0f;
            _camera.Phi = MathF.PI / 4;

            // instantiate projection and assign it "good" default values
            PerspectiveProjection projection = new PerspectiveProjection(_compositor);
            projection.Fov = MathF.PI / 2;

            _camera.Projection = projection;
            _viewport.Camera = _camera;

            // add event handler for chaning target size
            Target.SizeChanged += Target_SizeChanged;

            // event handlers for pointer events
            Target.PointerWheelChanged += Target_PointerWheelChanged;
            Target.PointerPressed += Target_PointerPressed;
            Target.PointerReleased += Target_PointerReleased;
            Target.PointerMoved += Target_PointerMoved;

            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;
        }

        private void CoreWindow_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            _mouseDowned = false;
        }

        private void Target_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _viewport.Size = e.NewSize.ToVector2();
        }

        private void Target_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // positive if scroll away from the user, negative if scroll toward the user
            int scrollSign = Math.Sign(e.GetCurrentPoint(Target).Properties.MouseWheelDelta);

            // determines how much to increase or decrease camera's radius by with each scroll
            // smaller numbers correspond to greater changes in radius
            float sensitivity = 0.95f;

            // scroll away from you
            if (scrollSign > 0)
            {
                _camera.Radius *= sensitivity;
            }
            // scroll towards you
            else if (scrollSign < 0)
            {
                _camera.Radius *= 1 + (1 - sensitivity);
            }
        }

        private void Target_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // use to determine how much to rotate camera by, based on size of change in pointer position
            _mouseDownLocation = e.GetCurrentPoint(Target).Position.ToVector2();

            _mouseDowned = true;
        }

        private void Target_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // prohibit mouse from changing rotation if the mouse button isn't pressed
            _mouseDowned = false;
        }

        private void Target_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_mouseDowned)
            {
                // rotate proportionately to the size of the mouse movement
                Vector2 newPos = e.GetCurrentPoint(Target).Position.ToVector2();
                float thetaDelta = newPos.X - _mouseDownLocation.X;
                float phiDelta = newPos.Y - _mouseDownLocation.Y;

                _mouseDownLocation = newPos;

                // higher number corresponds to faster rotation
                float sensitivity = 0.005f;

                // changes camera's phi and theta based on the sensitivity and size of the mouse movement
                _camera.Theta -= sensitivity * thetaDelta;
                _camera.Phi -= sensitivity * phiDelta;
            }
        }

        public string GltfFile
        {
            get { return (string)GetValue(GltfFileProperty); }
            set
            {
                SetValue(GltfFileProperty, value);
                LoadGltfFile();
            }
        }

        private static readonly DependencyProperty GltfFileProperty =
            DependencyProperty.Register("GltfFile", typeof(string), typeof(ModelViewer), null);

        private async void LoadGltfFile()
        {
            _sceneVisual.Root = await LoadGLTF(new Uri(GltfFile));
        }          

        async Task<SceneNode> LoadGLTF(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            var loader = new SceneLoader();
            return loader.Load(buffer, _compositor);
        }
    }
}
