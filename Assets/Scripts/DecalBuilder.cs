using UnityEngine;
using System.Collections.Generic;
using DecalSystem;

namespace DecalSystem
{
	public class DecalBuilder
	{
		private static List<Vector3> bufVertices = new List<Vector3>();
		private static List<Vector3> bufNormals = new List<Vector3>();
		private static List<Vector2> bufTexCoords = new List<Vector2>();
		private static List<int> bufIndices = new List<int>();

		public static Mesh CreateDecalMesh(Decal decal, GameObject affectedObject, Mesh mesh)
		{
			Vector3[] vertices = mesh.vertices;
			int[] triangles = mesh.GetTriangles(0);
			int startVertexCount = bufVertices.Count;

			Matrix4x4 matrix = decal.transform.worldToLocalMatrix * affectedObject.transform.localToWorldMatrix;

			for (int i = 0; i < triangles.Length; i += 3)
			{
				int i1 = triangles[i];
				int i2 = triangles[i + 1];
				int i3 = triangles[i + 2];

				Vector3 v1 = matrix.MultiplyPoint(vertices[i1]);
				Vector3 v2 = matrix.MultiplyPoint(vertices[i2]);
				Vector3 v3 = matrix.MultiplyPoint(vertices[i3]);

				Vector3 side1 = v2 - v1;
				Vector3 side2 = v3 - v1;
				Vector3 normal = Vector3.Cross(side1, side2).normalized;
				
				if (Vector3.Dot(decal.transform.forward, normal) < decal.decalDefinition.normalFactor)
					continue;

				DecalPolygon poly = new DecalPolygon(v1, v2, v3);

				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.right);
				if (poly == null)
					continue;
				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.left);
				if (poly == null)
					continue;

				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.top);
				if (poly == null)
					continue;
				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.bottom);
				if (poly == null)
					continue;

				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.front);
				if (poly == null)
					continue;
				poly = DecalPolygon.ClipPolygon(poly, DecalPolygon.back);
				if (poly == null)
					continue;

				AddPolygon(poly, normal);
			}

			if (decal.decalDefinition.sprite != null)
				GenerateTexCoords(startVertexCount, decal.decalDefinition.sprite);
			else
			{
				GenerateTexCoords(startVertexCount, decal.decalDefinition.sprite);
			}

			return CreateMesh();
		}

		static void AddPolygon(DecalPolygon poly, Vector3 normal)
		{
			// int ind1 = AddVertex(poly.vertices[0], normal);
			for (int i = 1; i < poly.vertices.Count - 1; i++)
			{
				int ind2 = AddVertex(poly.vertices[i], normal);
				int ind3 = AddVertex(poly.vertices[i + 1], normal);
				int ind1 = AddVertex(poly.vertices[0], normal);

				bufIndices.Add(ind1);
				bufIndices.Add(ind2);
				bufIndices.Add(ind3);
			}
		}

		static int AddVertex(Vector3 vertex, Vector3 normal)
		{
			bufVertices.Add(vertex);
			bufNormals.Add(normal);
			var index = bufVertices.Count - 1;
			return (int)index;
		}

		static void GenerateTexCoords(int start, Sprite sprite)
		{
			Rect rect = sprite.rect;
			rect.x /= sprite.texture.width;
			rect.y /= sprite.texture.height;
			rect.width /= sprite.texture.width;
			rect.height /= sprite.texture.height;

			for (int i = start; i < bufVertices.Count; i++)
			{
				Vector3 vertex = bufVertices[i];

				Vector2 uv = new Vector2(vertex.x + 0.5f, vertex.y + 0.5f);
				uv.x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
				uv.y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);

				bufTexCoords.Add(uv);
			}
		}

		// void GenerateUVs(int t1, int t2, int t3, ref Vector2[] uvs)
		// {
		// 	uvs[t1] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t1].x, Vertices[t1].y, Vertices[t1].z, 1);
		// 	uvs[t2] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t2].x, Vertices[t2].y, Vertices[t2].z, 1);
		// 	uvs[t3] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t3].x, Vertices[t3].y, Vertices[t3].z, 1);

		// 	if (DecalDef.sprite == null)
		// 	{
		// 		uvs[t1] *= 0.5f;
		// 		uvs[t2] *= 0.5f;
		// 		uvs[t3] *= 0.5f;

		// 		Vector2 offset = Vector2.one * 0.5f;
		// 		uvs[t1] += offset;
		// 		uvs[t2] += offset;
		// 		uvs[t3] += offset;
		// 	}
		// 	else
		// 	{
		// 		// scale to fit
		// 		Vector2 aspect = new Vector2((DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width), (DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height));
		// 		uvs[t1] *= aspect;
		// 		uvs[t2] *= aspect;
		// 		uvs[t3] *= aspect;

		// 		//scale more
		// 		uvs[t1] *= 0.5f;
		// 		uvs[t2] *= 0.5f;
		// 		uvs[t3] *= 0.5f;

		// 		// TODO: Fix rotation offset error
		// 		// move to sprite pos
		// 		Vector2 pos = new Vector2(DecalDef.sprite.rect.center.x / DecalDef.sprite.texture.width, DecalDef.sprite.rect.center.y / DecalDef.sprite.texture.height);
		// 		uvs[t1] += pos;
		// 		uvs[t2] += pos;
		// 		uvs[t3] += pos;
		// 	}
		// }

		static Mesh CreateMesh()
		{
			if (bufIndices.Count == 0)
			{
				return null;
			}
			Mesh mesh = new Mesh();

			mesh.vertices = bufVertices.ToArray();
			mesh.normals = bufNormals.ToArray();
			mesh.uv = bufTexCoords.ToArray();
			mesh.triangles = bufIndices.ToArray();

			bufVertices.Clear();
			bufNormals.Clear();
			bufTexCoords.Clear();
			bufIndices.Clear();

			return mesh;
		}

	}
}