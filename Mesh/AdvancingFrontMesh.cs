﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mesh
{
    public class AdvancingFrontMesh : Mesh2D
    {
        private double radiusMultiplier;
        private double idealDistance;

        public AdvancingFrontMesh(Region region)
            : base(region)
        {
            this.radiusMultiplier = 0.8;
            this.idealDistance = Double.MaxValue;
        }

        /// <summary>
        /// Creates triangle meshes from the Geometry object using the
        /// Advancing Front Triangulation method.
        /// </summary>
        public override void BuildMesh()
        {
            // Check whether the contours of the geometry are closed.

            // Check whether the points of the contours are in the same plane.
            CheckPlane();

            // Check inner/outer contour numbers.

            // Divide contours into segments.
            this.Fronts = this.OwnerRegion.DivideContours();

            // Calculate min ideal point distance from base segment.
            CalculateIdealDistance();

            // TEST: form triangles
            ProcessFronts();
        }

        // temporary solution, needs to be rewrited
        private void CalculateIdealDistance()
        {
            foreach (Front f in this.Fronts)
            {
                List<Segment> segments = f.GetSegmentsUnordered();
                foreach (Segment segment in segments)
                {
                    if (segment.GetLength() < this.idealDistance)
                    {
                        this.idealDistance = segment.GetLength();
                    }
                }
            }
        }

        // Refinement needed (3 points on the same straight).
        /// <summary>
        /// Checks that all input points in the owner region are in the same plane.
        /// </summary>
        private void CheckPlane()
        {
            List<Point> regionPoints = new List<Point>();
            foreach (Contour contour in this.OwnerRegion.Contours)
            {
                regionPoints.AddRange(contour.GetContourPoints());
            }
            Plane plane = new Plane(regionPoints[0], regionPoints[1], regionPoints[2]);
            foreach (Point point in regionPoints)
            {
                if (!plane.Contains(point))
                {
                    throw new ApplicationException(
                        "Not all points are in the same plane in some regions");
                }
            }
        }

        private void ProcessFronts()
        {
            foreach (Front front in this.Fronts)
            {
                // Add front points to output points.
                this.Points.AddRange(front.Points);

                while (front.Segments.Count > 0)
                {
                    Segment shortestSegment = front.GetShortestUncheckedSegment();
                    shortestSegment.Checked = true;

                    // Find ideal point.
                    //Point idealPoint = shortestSegment.RotateInward(60.0);
                    Point idealPoint = shortestSegment.GetIdealPoint(idealDistance);

                    // Search for nearby points.
                    double radius = idealDistance * radiusMultiplier;
                    List<Point> nearbyPoints = GetNearbyPoints(idealPoint, radius, front);
                    Point.SortByDistance(nearbyPoints, idealPoint);
                    nearbyPoints.Add(idealPoint);

                    while (nearbyPoints.Count > 0)
                    {
                        // Processing ideal point
                        //if (nearbyPoints.Count == 1)
                        //{
                        //    if (!front.IsPointInside(nearbyPoints[0]))
                        //        return;
                        //}
                        bool formed = FormTriangle(front, shortestSegment, nearbyPoints[0],
                            nearbyPoints.Count != 1);
                        if (formed)
                        {
                            foreach (Segment seg in front.GetSegmentsUnordered())
                            {
                                seg.Checked = false;
                            }
                            break;
                        }

                        nearbyPoints.RemoveAt(0);
                    }


                    //// No nearby point.
                    //if (nearbyPoints.Count == 0)
                    //{
                    //    if (!front.IsPointInside(idealPoint))
                    //        break;
                    //    FormTriangle(front, shortestSegment, idealPoint, false);
                    //    continue;
                    //}

                    //// One nearby point.
                    //if (nearbyPoints.Count == 1)
                    //{
                    //    Point p = nearbyPoints[0];
                    //    FormTriangle(front, shortestSegment, p, true);
                    //    continue;
                    //}

                    //// Multiple nearby points.
                    //if (nearbyPoints.Count > 1)
                    //{
                    //    Point p = GetClosestPoint(idealPoint, nearbyPoints);
                    //    FormTriangle(front, shortestSegment, p, true);
                    //}
                }
            }
        }

        private bool FormTriangle(Front front, Segment shortestSegment, Point p, bool existingPoint)
        {
            Segment s1 = new Segment(shortestSegment.Start, p);
            Segment s2 = new Segment(p, shortestSegment.End);

            List<Segment> SegmentsToAdd = new List<Segment>();
            List<Segment> SegmentsToRemove = new List<Segment>();

            // Check if triangle can be formed
            if (!front.Contains(s1))
            {
                // Test if triangle candidate intersects with existing elements
                if (CheckIntersection(s1) == false) return false;
                SegmentsToAdd.Add(s1);
            }
            else SegmentsToRemove.Add(s1);

            if (!front.Contains(s2))
            {
                // Test if triangle candidate intersects with existing elements
                if (CheckIntersection(s2) == false) return false;
                SegmentsToAdd.Add(s2);
            }
            else SegmentsToRemove.Add(s2);

            // update front and form triangle

            // Remove current segment from the front.
            front.RemoveSegment(shortestSegment);

            // Update front with new segments.
            if (!existingPoint)
            {
                this.Points.Add(p);
                front.Points.Add(p);
            }
            foreach (Segment s in SegmentsToAdd)
            {
                front.AddSegment(s);
            }
            foreach (Segment s in SegmentsToRemove)
            {
                front.RemoveSegment(s);
            }

            this.Triangles.Add(new Triangle(shortestSegment.Start, shortestSegment.End, p));
            return true;
        }

        private bool CheckIntersection(Segment s)
        {
            foreach (Triangle triangle in this.Triangles)
            {
                Point p1 = s.Intersection(new Segment(triangle.Points[0], triangle.Points[1]));
                Point p2 = s.Intersection(new Segment(triangle.Points[1], triangle.Points[2]));
                Point p3 = s.Intersection(new Segment(triangle.Points[2], triangle.Points[0]));
                if (p1 != null || p2 != null || p3 != null) return false;
            }
            return true;
        }

        // to be improved (store points in tree structure)
        private List<Point> GetNearbyPoints(Point idealPoint, double r, Front f)
        {
            List<Point> pts = new List<Point>();
            foreach (Point p in f.Points)
            {
                if (Point.Distance(idealPoint, p) <= r)
                {
                    pts.Add(p);
                }
            }
            return pts;
        }
    }
}
