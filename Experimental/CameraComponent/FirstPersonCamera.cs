using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CameraComponent
{
    public sealed class FirstPersonCamera : Camera
    {
        private Compositor compositor;

        private Vector3 position;
        private float yaw, pitch, roll;
        private Projection projection;

        private CompositionPropertySet propertySet;

        private bool useAnimations;
       
        public FirstPersonCamera()
        {
            this.compositor = Window.Current.Compositor;

            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            Position = Vector3.Zero;
            Projection = new PerspectiveProjection();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        public float Yaw
        {
            get
            {
                if (UseAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Yaw", out curr) == CompositionGetValueStatus.Succeeded) ? curr : yaw;
                }
                else
                {
                    return yaw;
                }
            }
            set
            {
                if (UseAnimations)
                {
                    propertySet.InsertScalar("Yaw", value);
                }
                else
                { 
                    yaw = value;
                    RaisePropertyChanged("Yaw");
                }
            }
        }
        public float Pitch 
        {
            get
            {
                if (UseAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Pitch", out curr) == CompositionGetValueStatus.Succeeded) ? curr : pitch;
                }
                else
                {
                    return pitch;
                }
            }
            set
            {
                if (UseAnimations)
                {
                    propertySet.InsertScalar("Pitch", value);
                }
                else
                {
                    pitch = value;
                    RaisePropertyChanged("Pitch");
                }
            }
        }
        public float Roll
        {
            get
            {
                if (UseAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Roll", out curr) == CompositionGetValueStatus.Succeeded) ? curr : roll;
                }
                else
                {
                    return roll;
                }
            }
            set
            {
                if (UseAnimations)
                {
                    propertySet.InsertScalar("Roll", value);
                }
                else
                {
                    roll = value;
                    RaisePropertyChanged("Roll");
                }
            }
        }
        public Vector3 Position
        {
            get
            {
                if(UseAnimations)
                {
                    Vector3 curr = Vector3.Zero;
                    return (propertySet.TryGetVector3("Position", out curr) == CompositionGetValueStatus.Succeeded) ? curr : position;
                }
                else
                {
                    return position;
                }
            }
            set
            {
                if (UseAnimations)
                {
                    propertySet.InsertVector3("Position", value);
                }
                else
                {
                    position = value;
                    RaisePropertyChanged("Position");
                }
            }
        }
        public Vector3 LookDirection
        {
            get => Vector3.Zero;
            set
            {
                Vector3 direction = value;
                if (value != Vector3.Zero)
                {
                    direction = Vector3.Normalize(value);
                }
                
                direction.X *= -1; 
                Yaw = (MathF.PI / 2f) + MathF.Atan2(direction.Z, direction.X);
                Pitch = MathF.Asin(direction.Y);                    
            }
        }
        public bool UseAnimations
        {
            get => useAnimations;
            set
            {
                if(value != useAnimations)
                {
                    useAnimations = value;
                    // turn animations off
                    if (!value)
                    {
                        propertySet.TryGetVector3("Position", out position);
                        propertySet.TryGetScalar("Yaw", out yaw);
                        propertySet.TryGetScalar("Pitch", out pitch);
                        propertySet.TryGetScalar("Roll", out roll);
                        RaisePropertyChanged("ViewMatrix");
                    }
                    // turn animations on
                    else
                    {
                        RaisePropertyChanged("ViewMatrix");
                    }
                }
            }
        }
        public Projection Projection
        {
            get => projection;
            set
            {
                projection = value;
                projection.PropertyChanged += PropertyChanged;
                RaisePropertyChanged("Projection");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Matrix4x4 CreateViewMatrix()
        {
            Matrix4x4 transformation = Matrix4x4.Identity;
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), -roll);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -pitch);
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -yaw);

            return matPos * matYaw * matPitch * matRoll;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// ANIMATION FUNCTIONS
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        public void CreateExpressionAnimation(CompositionPropertySet toAnimate, string propertyName)
        {
            propertySet = compositor.CreatePropertySet();
            propertySet.InsertVector3("Position", Position);
            propertySet.InsertScalar("Yaw", Yaw);
            propertySet.InsertScalar("Pitch", Pitch);
            propertySet.InsertScalar("Roll", Roll);

            var matPos = "Matrix4x4.CreateTranslation(-FPCamera.Position)";
            var matRoll = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 0, 1), -FPCamera.Roll)";
            var matPitch = "Matrix4x4.CreateFromAxisAngle(Vector3(1, 0, 0), -FPCamera.Pitch)";
            var matYaw = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 1, 0), -FPCamera.Yaw)";
            var viewMat = matPos + "*" + matYaw + "*" + matPitch + "*" + matRoll;

            var camExpression = compositor.CreateExpressionAnimation();
            camExpression.Expression = viewMat;
            camExpression.SetReferenceParameter("FPCamera", propertySet);

            toAnimate.StartAnimation(propertyName, camExpression);
        }
        public CompositionPropertySet GetPropertySet()
        {
            return propertySet;
        }
        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            propertySet.StartAnimation(propertyName, animation);
        }
        public void StopAnimation(string propertyName)
        {
            propertySet.StopAnimation(propertyName);
        }
    }
}
