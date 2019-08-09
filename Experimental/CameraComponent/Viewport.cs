using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;

namespace CameraComponent
{
    public enum Stretch { Fill, FixX, FixY, Uniform, UniformToFill};
    /// Fill: Stretch the image to the size of the viewport so the same amount of the image is always shown
    /// FixX: Stretch the image to the width of the screen so the same amount of horizontal space in the scene is always shown 
    /// FixY: Stretch the image to the height of the screen so the same amount of vertical space in the scene is always shown 
    /// Uniform: If the screen is wide stretch to the height of the screen, if the screen is tall stretch to the width of the screen 
    /// UniformToFill: If the screen is tall stretch to the height of the screen, if the screen is wide stretch to the width of the screen 

    public sealed class Viewport
    {
        private SceneVisual _sceneVisual;
        private Compositor _compositor;
        private Camera _camera;
        private CompositionPropertySet _propertySet;

        public Viewport(SceneVisual visual)
        {
            _sceneVisual = visual;
            _compositor = _sceneVisual.Compositor;
            _propertySet = _compositor.CreatePropertySet();

            // Create properties of viewport
            _propertySet.InsertVector3("Offset", Vector3.Zero);
            _propertySet.InsertVector2("Size", new Vector2(100, 100));
            _propertySet.InsertScalar("Stretch", (int)Stretch.Uniform);
            _propertySet.InsertMatrix4x4("StretchMatrix", Matrix4x4.Identity);

            StartAnimationsOnStretchMatrix();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        /////////////////////////////////////////////////////////////////////////////////////////////////

        // The visual whose transform matrix we are using to create a camera
        public SceneVisual SceneVisual { get => _sceneVisual; set => _sceneVisual = value; }

        // The size of our viewport in the form of (width, height)
        public Vector2 Size
        {
            get
            {
                Vector2 curr;
                _propertySet.TryGetVector2("Size", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector2("Size", value);
            }
        }

        // The coordinate of the top left corner of our viewport in screen space
        public Vector3 Offset
        {
            get
            {
                Vector3 curr;
                _propertySet.TryGetVector3("Offset", out curr);
                return curr;
            }
            set
            {
                _propertySet.InsertVector3("Offset", value);
            }
        }

        // Governs how our image is stretched to the near plane
        public Stretch Stretch
        {
            get
            {
                float curr;
                _propertySet.TryGetScalar("Stretch", out curr);
                return (Stretch)curr;
            }
            set
            {
                _propertySet.InsertScalar("Stretch", (int)value);
            }
        }

        // The camera object that holds our position in world space
        public Camera Camera
        {
            get => _camera;
            set
            {
                _camera = value;

                // if null, nothing should be displayed
                if (_camera == null)
                {
                    _sceneVisual.StopAnimation("TransfromMatrix");
                }
                else
                {
                    StartAnimationsOnTransformMatrix();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// ANIMATION FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////    

        // Starts animations on _sceneVisual's TransformMatrix property
        private void StartAnimationsOnTransformMatrix()
        {
            // Creates an expression that is represents the product of all our transformations
            var cameraMatExpression = _compositor.CreateExpressionAnimation();
            cameraMatExpression.Expression = "Camera.ModelViewProjectionMatrix * Viewport.StretchMatrix " +
                "* Matrix4x4.CreateTranslation(Vector3(Viewport.Offset.X + Viewport.Size.X / 2f, Viewport.Offset.Y + Viewport.Size.Y / 2f, Viewport.Offset.Z))";
            cameraMatExpression.SetReferenceParameter("Camera", Camera.GetPropertySet());
            cameraMatExpression.SetReferenceParameter("Viewport", _propertySet);

            // Links our product of matrices expression to _sceneVisual's TransformMatrix
            _sceneVisual.StartAnimation("TransformMatrix", cameraMatExpression);
        }

        // Starts an expression animation on StretchMatrix property based on the Stretch 
        private void StartAnimationsOnStretchMatrix()
        {
            // Expression that creates a matrix based on the value of the viewport's stretch property
            string stretchMat =
            // if (Stretch == Fill)
            "(Viewport.Stretch == 0f)?" + 
                "Matrix4x4(" +
                "Viewport.Size.X, 0, 0, 0, " +
                "0, Viewport.Size.Y, 0, 0, " +
                "0, 0, (Viewport.Size.X + Viewport.Size.Y) / 2f, 0, " +
                "0, 0, 0, 1) " +
            ":" +
            // else if (Stretch == FixX)
            "(Viewport.Stretch == 1f)? " +
                "Matrix4x4(" +
                "Viewport.Size.X, 0, 0, 0, " +
                "0, Viewport.Size.X, 0, 0, " +
                "0, 0, Viewport.Size.X, 0, " +
                "0, 0, 0, 1)" +
            ":" +
            // else if (Stretch == FixY)
            "(Viewport.Stretch == 2f)? " +
                "Matrix4x4(" +
                "Viewport.Size.Y, 0, 0, 0, " +
                "0, Viewport.Size.Y, 0, 0, " +
                "0, 0, Viewport.Size.Y, 0, " +
                "0, 0, 0, 1)" +
            ":" +
            // else if (Stretch == Uniform)
            "(Viewport.Stretch == 3f)? " +
                // if (Size.X >= Size.Y)
                "(Viewport.Size.X >= Viewport.Size.Y)? " +
                    "Matrix4x4(" +
                    "Viewport.Size.Y, 0, 0, 0, " +
                    "0, Viewport.Size.Y, 0, 0, " +
                    "0, 0, Viewport.Size.Y, 0, " +
                    "0, 0, 0, 1) " +
                ":" +
                // else 
                    "Matrix4x4(" +
                    "Viewport.Size.X, 0, 0, 0, " +
                    "0, Viewport.Size.X, 0, 0, " +
                    "0, 0, Viewport.Size.X, 0, " +
                    "0, 0, 0, 1)" +
            ":" +
             // else if (Stretch == UniformToFill)
                // if (Size.X < Size.Y)
                "(Viewport.Size.X < Viewport.Size.Y)? " +
                    "Matrix4x4(" +
                    "Viewport.Size.Y, 0, 0, 0, " +
                    "0, Viewport.Size.Y, 0, 0, " +
                    "0, 0, Viewport.Size.Y, 0, " +
                    "0, 0, 0, 1) " +
                ":" +
                // else
                    "Matrix4x4(" +
                    "Viewport.Size.X, 0, 0, 0, " +
                    "0, Viewport.Size.X, 0, 0, " +
                    "0, 0, Viewport.Size.X, 0, " +
                    "0, 0, 0, 1)";

            var stretchExpression = _compositor.CreateExpressionAnimation();
            stretchExpression.Expression = stretchMat;
            stretchExpression.SetReferenceParameter("Viewport", _propertySet);

            _propertySet.StartAnimation("StretchMatrix", stretchExpression);
        }

        // start an animation on the specified property
        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            _propertySet.StartAnimation(propertyName, animation);
        }

        // stop the animation on the given property
        public void StopAnimation(string propertyName)
        {
            _propertySet.StopAnimation(propertyName);
        }
    }
}
