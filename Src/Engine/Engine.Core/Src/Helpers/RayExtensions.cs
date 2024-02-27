using System;
using System.Numerics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Extensions to provide necessary methods to BEPU
    /// </summary>
	public static class RayExtensions
    {
        public static float Epsilon = 1E-7f;

        public static bool Intersects(this Ray ray, ref BoundingBox boundingBox, out float t)
        {
			if (Math.Abs(ray.Direction.X) < Epsilon && (ray.Position.X < boundingBox.Min.X || ray.Position.X > boundingBox.Max.X))
            {
                //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                //can't be intersecting.
                t = 0;
                return false;
            }
            float tmin = 0, tmax = float.MaxValue;
			float inverseDirection = 1 / ray.Direction.X;
			float t1 = (boundingBox.Min.X - ray.Position.X) * inverseDirection;
			float t2 = (boundingBox.Max.X - ray.Position.X) * inverseDirection;
            if (t1 > t2)
            {
                float temp = t1;
                t1 = t2;
                t2 = temp;
            }
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            if (tmin > tmax)
            {
                t = 0;
                return false;
            }
			if (Math.Abs(ray.Direction.Y) < Epsilon && (ray.Position.Y < boundingBox.Min.Y || ray.Position.Y > boundingBox.Max.Y))
            {
                //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                //can't be intersecting.
                t = 0;
                return false;
            }
			inverseDirection = 1 / ray.Direction.Y;
			t1 = (boundingBox.Min.Y - ray.Position.Y) * inverseDirection;
			t2 = (boundingBox.Max.Y - ray.Position.Y) * inverseDirection;
            if (t1 > t2)
            {
                float temp = t1;
                t1 = t2;
                t2 = temp;
            }
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            if (tmin > tmax)
            {
                t = 0;
                return false;
            }
			if (Math.Abs(ray.Direction.Z) < Epsilon && (ray.Position.Z < boundingBox.Min.Z || ray.Position.Z > boundingBox.Max.Z))
            {
                //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                //can't be intersecting.
                t = 0;
                return false;
            }
			inverseDirection = 1 / ray.Direction.Z;
			t1 = (boundingBox.Min.Z - ray.Position.Z) * inverseDirection;
			t2 = (boundingBox.Max.Z - ray.Position.Z) * inverseDirection;
            if (t1 > t2)
            {
                float temp = t1;
                t1 = t2;
                t2 = temp;
            }
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            if (tmin > tmax)
            {
                t = 0;
                return false;
            }
            t = tmin;
            return true;
        }
			
		public static bool Intersects(this Ray ray, ref BoundingSphere boundingSphere, out float t)
        {
            t = 0;

            // Find the vector between where the ray starts the the sphere's centre
            Vector3 difference = boundingSphere.Center - ray.Position;

            float differenceLengthSquared = difference.LengthSquared();
            float sphereRadiusSquared = boundingSphere.Radius * boundingSphere.Radius;

            // If the distance between the ray start and the sphere's centre is less than
            // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
            if (differenceLengthSquared < sphereRadiusSquared)
                return true;

			float distanceAlongRay = Vector3.Dot(ray.Direction, difference);
            // If the ray is pointing away from the sphere then we don't ever intersect
            if (distanceAlongRay < 0)
                return false;

            // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
            // if x = radius of sphere
            // if y = distance between ray position and sphere centre
            // if z = the distance we've travelled along the ray
            // if x^2 + z^2 - y^2 < 0, we do not intersect
            float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

            //result = (dist < 0) ? null : distanceAlongRay - (float?)Math.Sqrt(dist);

            if (dist < 0)
                return false;

            t = distanceAlongRay;
            return true;
        }

        /// <summary>
        /// Determines if and when the ray intersects the plane.
        /// </summary>
        /// <param name="plane">Plane to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public static bool Intersects(this Ray ray, ref Plane plane, out float t)
        {
            float velocity = Vector3.Dot(ray.Direction, plane.Normal);
            if (Math.Abs(velocity) < Epsilon)
            {
                t = 0;
                return false;
            }
            float distanceAlongNormal = Vector3.Dot(ray.Position, plane.Normal);
            distanceAlongNormal += plane.D;
            t = -distanceAlongNormal / velocity;
            return t >= -Epsilon;
        }

        /// <summary>
        /// Computes a point along a ray given the length along the ray from the ray position.
        /// </summary>
        /// <param name="t">Length along the ray from the ray position in terms of the ray's direction.</param>
        /// <param name="v">Point along the ray at the given location.</param>
		public static void GetPointOnRay(this Ray ray, float t, out Vector3 v)
        {
            v = Vector3.Multiply(ray.Direction, t);
            v = Vector3.Add(v, ray.Position);
        }
    }
}
