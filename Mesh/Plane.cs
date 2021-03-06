﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mesh
{
    public class Plane
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public Vector3D Normal { get; set; }

        public Plane(Point p1, Point p2, Point p3)
        {
            Vector3D v1 = p1.ToVector3D(), v2 = p2.ToVector3D(), v3 = p3.ToVector3D();
            this.Normal = Vector3D.CrossProduct(v2 - v1, v3 - v1);
            this.A = Normal.X;
            this.B = Normal.Y;
            this.C = Normal.Z;
            this.D = -Normal.X * p1.X - Normal.Y * p1.Y - Normal.Z * p1.Z;
        }

        /// <summary>
        /// Returns that the plane contains the point, if the distance between them is
        /// smaller than Epsilon.
        /// </summary>
        public bool Contains(Point point)
        {
            double distance = Math.Abs(A * point.X + this.B * point.Y + this.C * point.Z + this.D) /
                Math.Sqrt(this.A * this.A + this.B * this.B + this.C * this.C);
            if (distance < Geometry.Epsilon)
            {
                return true;
            }
            return false;
        }
    }
}
