﻿using UnityEngine;
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

		public static void BuildDecalForObject(Decal decal, GameObject affectedObject)
		{
			Mesh affectedMesh = affectedObject.GetComponent<MeshFilter>().sharedMesh;
			if (affectedMesh == null) return;
			if (!affectedMesh.isReadable) return;

			Vector3[] vertices = affectedMesh.vertices;
			int[] triangles = affectedMesh.GetTriangles(0);
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

				if (Vector3.Angle(-Vector3.forward, normal) >= decal.decalDefinition.normalFactor)
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

			if (decal.decalDefinition.sprite)
				GenerateTexCoords(startVertexCount, decal.decalDefinition.sprite);
			else
			{
				int textureWidth = decal.decalDefinition.material.mainTexture.width;
				int textureHeight = decal.decalDefinition.material.mainTexture.height;
				Texture2D textureData = new Texture2D(textureWidth, textureHeight);
				Sprite fakeSprite = Sprite.Create(textureData, new Rect(0, 0, textureWidth, textureHeight), new Vector2(textureWidth / 2, textureHeight / 2));
				GenerateTexCoords(startVertexCount, decal.decalDefinition.sprite);
			}
		}

		private static void AddPolygon(DecalPolygon poly, Vector3 normal)
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

		private static int AddVertex(Vector3 vertex, Vector3 normal)
		{
			// int index = FindVertex(vertex);
			// if (index == -1)
			// {
			bufVertices.Add(vertex);
			bufNormals.Add(normal);
			var index = bufVertices.Count - 1;
			// }
			// else
			// {
			// Vector3 t = bufNormals[index] + normal;
			// bufNormals[index] = t.normalized;
			// }
			return (int)index;
		}

		private static void GenerateTexCoords(int start, Sprite sprite)
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

		public static void Push(float distance)
		{
			for (int i = 0; i < bufVertices.Count; i++)
			{
				Vector3 normal = bufNormals[i];
				bufVertices[i] += normal * distance;
			}
		}


		public static Mesh CreateMesh()
		{
			if (bufIndices.Count == 0)
			{
				return null;
			}
			Mesh mesh = new Mesh();

			mesh.vertices = bufVertices.ToArray();
			mesh.normals = bufNormals.ToArray();
			mesh.uv = bufTexCoords.ToArray();
			mesh.uv2 = bufTexCoords.ToArray();
			mesh.triangles = bufIndices.ToArray();

			bufVertices.Clear();
			bufNormals.Clear();
			bufTexCoords.Clear();
			bufIndices.Clear();

			return mesh;
		}

	}
}