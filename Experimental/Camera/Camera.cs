using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml.Media;


namespace Experimental
{
    public interface Camera : INotifyPropertyChanged
    {
        Projection Projection { get; set; }
        Matrix4x4 CreateTransformationMatrix();
        bool UseAnimations { get; set; }
        void StartAnimation(string propertyName, CompositionAnimation animation);
        void StopAnimation(string propertyName);
    }
}