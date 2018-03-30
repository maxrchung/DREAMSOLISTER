﻿using System.Numerics;
using System.Windows.Media;

namespace ShapeAnimation {
    class Shape {
        public Vector2 position { get; set; }
        public Angle rotation { get; set; }
        public Vector2 scaleVector { get; set; }
        public float fade { get; set; }
        public Color color { get; set; }

        public Shape(Vector2 pPosition, Angle pRotation, Vector2 pScaleVector, float pFade, Color pColor) {
            position = pPosition;
            rotation = pRotation;
            scaleVector = pScaleVector;
            fade = pFade;
            color = pColor;
        }
    }
}
