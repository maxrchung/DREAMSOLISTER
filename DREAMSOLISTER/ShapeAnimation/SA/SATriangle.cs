﻿using System.Runtime.Serialization;
using System.Windows.Media;

namespace ShapeAnimation {
    [DataContract]
    class SATriangle : SAShape {
        public PointCollection points {
            get {
                var half = size / 2;
                var collection = new PointCollection() {
                    new Vector(position.x - half.x, position.y + half.y).rotateFrom(rotation.radian, position).toPoint(),
                    new Vector(position.x, position.y - half.y).rotateFrom(rotation.radian, position).toPoint(),
                    new Vector(position.x + half.x, position.y + half.y).rotateFrom(rotation.radian, position).toPoint()
                };
                return collection;
            }
        }

        public SATriangle(Vector position, Angle rotation, Vector scaleVector, Color color)
            : base(position, rotation, scaleVector, color) {
            type = SAShapeType.Triangle;
        }

        public SATriangle()
            : base() {
            type = SAShapeType.Triangle;
        }

        public override SAShape copy() {
            return new SATriangle(position, rotation, scaleVector, color);
        }
    }
}
