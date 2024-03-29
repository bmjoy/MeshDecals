using UnityEngine;
using System.Collections.Generic;

namespace lhlv.VFX.DecalSystem
{
	public class DecalPolygon
	{

		public List<Vector3> vertices = new List<Vector3>(9);
		public static Plane right = new Plane(Vector3.right, Vector3.right / 2f);
		public static Plane left = new Plane(-Vector3.right, -Vector3.right / 2f);
		public static Plane top = new Plane(Vector3.up, Vector3.up / 2f);
		public static Plane bottom = new Plane(-Vector3.up, -Vector3.up / 2f);
		public static Plane front = new Plane(Vector3.forward, Vector3.forward / 2f);
		public static Plane back = new Plane(-Vector3.forward, -Vector3.forward / 2f);

		public DecalPolygon(params Vector3[] vts)
		{
			vertices.AddRange(vts);
		}

		public static DecalPolygon ClipPolygon(DecalPolygon polygon, Plane plane)
		{
			bool[] positive = new bool[9];
			int positiveCount = 0;

			for (int i = 0; i < polygon.vertices.Count; i++)
			{
				positive[i] = !plane.GetSide(polygon.vertices[i]);
				if (positive[i]) positiveCount++;
			}

			if (positiveCount == 0) return null; // полностью за плоскостью
			if (positiveCount == polygon.vertices.Count) return polygon; // полностью перед плоскостью

			DecalPolygon tempPolygon = new DecalPolygon();

			for (int i = 0; i < polygon.vertices.Count; i++)
			{
				int next = i + 1;
				next %= polygon.vertices.Count;

				if (positive[i])
				{
					tempPolygon.vertices.Add(polygon.vertices[i]);
				}

				if (positive[i] != positive[next])
				{
					Vector3 v1 = polygon.vertices[next];
					Vector3 v2 = polygon.vertices[i];

					Vector3 v = LineCast(plane, v1, v2);
					tempPolygon.vertices.Add(v);
				}
			}

			return tempPolygon;
		}

		private static Vector3 LineCast(Plane plane, Vector3 a, Vector3 b)
		{
			float dis;
			Ray ray = new Ray(a, b - a);
			plane.Raycast(ray, out dis);
			return ray.GetPoint(dis);
		}

	}
}