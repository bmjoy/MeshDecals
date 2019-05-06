using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace lhlv.VFX.DecalSystem
{
	public static class DecalBuilder
	{
		// builder vars
		static List<Vector3> bufVertices = new List<Vector3>();
		static List<Vector3> bufNormals = new List<Vector3>();
		static List<Vector2> bufTexCoords = new List<Vector2>();
		static List<int> bufIndices = new List<int>();

		// some globals
		public static Decal decal;
		public static GameObject affectedObject;
		public static Mesh decalMesh;
		public static Vector3[] vertices;
		public static int[] triangles;
		public static Mesh quadMesh = CreateQuadMesh();

		// isInsidefrustum aux vars
		static Vector3[] vecs = new Vector3[3];

		public static Vector3 direction;
		public static Vector3 point;
		public static Matrix4x4 vP;
		public static Plane[] planes;

		// custom vals
		public static float size;
		public static float angle;
		public static float normalFactor;
		public static float pointBackwardOffset;
		public static float depth;

		public static void SetUp(DecalType decType, GameObject targetObj, ref Vector3[] verts, ref int[] tris, DecalDefinition decalDef, Vector3 dir, Vector3 p, float si, float ang, float normFac, float pBack, float de)
		{
			// builder vars
			affectedObject = targetObj;
			vertices = verts;
			triangles = tris;
			direction = dir;
			point = p;
			size = si;
			angle = ang;
			normalFactor = normFac;
			pointBackwardOffset = pBack;
			depth = de;

			// create decal GO
			GameObject decalGO = new GameObject("decalSkinned");
			decalGO.transform.parent = affectedObject.transform;

			// create decal component
			decal = decalGO.AddComponent<Decal>();
			decal.Init(decalDef, decType, affectedObject);

			if (decType == DecalType.Skinned)
			{
				CalculateMatrixAndPlanes();
				decalGO.transform.localPosition = Vector3.zero;
				decalGO.transform.localRotation = Quaternion.identity;
				decalGO.transform.localScale = Vector3.one;
			}
			else
			{
				decalGO.transform.position = point;
				decalGO.transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, 0, angle);
			}
		}

		public static void CreateDecalMeshStatic()
		{
			int startVertexCount = bufVertices.Count;

			Matrix4x4 matrix = decal.transform.worldToLocalMatrix * affectedObject.transform.localToWorldMatrix;

			for (int i = 0; i < triangles.Length; i += 3)
			{
				Vector3 v1 = matrix.MultiplyPoint(vertices[triangles[i]]);
				Vector3 v2 = matrix.MultiplyPoint(vertices[triangles[i + 1]]);
				Vector3 v3 = matrix.MultiplyPoint(vertices[triangles[i + 2]]);

				Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
				if (!FacingNormal(triangles[i], triangles[i + 1], triangles[i + 2]))
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

			GenerateTexCoords(startVertexCount, decal.decalDefinition.sprite);

			decalMesh = StaticCreateMesh();

			decal.SetMesh(decalMesh);
		}

		public static void CreateDecalMeshSkinned(SkinnedMeshRenderer smr)
		{

			// get a snapshot of the mesh to test against
			Mesh bakedMesh = new Mesh();
			smr.BakeMesh(bakedMesh);

			decalMesh = Mesh.Instantiate(smr.sharedMesh);

			// get vertices that are going to be tested
			vertices = bakedMesh.vertices;

			// uvs
			Vector2[] uvs = bakedMesh.uv;

			// process each submesh
			for (int subMesh = 0; subMesh < bakedMesh.subMeshCount; subMesh++)
			{
				List<int> triangleList = new List<int>();
				triangles = bakedMesh.GetTriangles(subMesh);

				// check each triangle against view Frustum
				for (int i = 0; i < triangles.Length; i += 3)
				{
					if (isInsideFrustum(triangles[i], triangles[i + 1], triangles[i + 2]))
					{
						// TODO: need to clip decals to frustum, otherwise sprite decals leak
						triangleList.Add(triangles[i]);
						triangleList.Add(triangles[i + 1]);
						triangleList.Add(triangles[i + 2]);

						GenerateUVs(triangles[i], triangles[i + 1], triangles[i + 2], ref uvs);
					}
				}

				decalMesh.SetTriangles(triangleList.ToArray(), subMesh);
			}
			decalMesh.uv = uvs;

			decal.SetMesh(decalMesh);
		}

		public static void CreateDecalMeshQuad()
		{
			decal.SetMesh(quadMesh);
		}

		static Mesh CreateQuadMesh()
		{
			var mesh = new Mesh();

			var vertices = new Vector3[4];

			vertices[0] = new Vector3(-0.5f, -0.5f, 0);
			vertices[1] = new Vector3(0.5f, -0.5f, 0);
			vertices[2] = new Vector3(-0.5f, 0.5f, 0);
			vertices[3] = new Vector3(0.5f, 0.5f, 0);

			mesh.vertices = vertices;

			var tris = new int[6];

			tris[0] = 0;
			tris[1] = 2;
			tris[2] = 1;

			tris[3] = 2;
			tris[4] = 3;
			tris[5] = 1;

			mesh.triangles = tris;

			var normals = new Vector3[4];

			normals[0] = -Vector3.forward;
			normals[1] = -Vector3.forward;
			normals[2] = -Vector3.forward;
			normals[3] = -Vector3.forward;

			mesh.normals = normals;

			var uvs = new Vector2[4];

			uvs[0] = new Vector2(0, 0);
			uvs[1] = new Vector2(1, 0);
			uvs[2] = new Vector2(0, 1);
			uvs[3] = new Vector2(1, 1);

			mesh.uv = uvs;
			
			return mesh;
		}

		static void CalculateMatrixAndPlanes()
		{
			// project from a close point from the hit point
			Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(point - direction * pointBackwardOffset, Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, 0, angle), new Vector3(1, 1, -1)));

			Matrix4x4 p;
			if (decal.decalDefinition.sprite == null)
				p = Matrix4x4.Ortho(-size / 2, size / 2, -size / 2, size / 2, 0.0001f, depth);
			else
				p = Matrix4x4.Ortho(-(decal.decalDefinition.sprite.rect.size.x / decal.decalDefinition.sprite.texture.width) * 4 * size,
									(decal.decalDefinition.sprite.rect.size.x / decal.decalDefinition.sprite.texture.width) * 4 * size,
									-(decal.decalDefinition.sprite.rect.size.y / decal.decalDefinition.sprite.texture.height) * 4 * size,
									(decal.decalDefinition.sprite.rect.size.y / decal.decalDefinition.sprite.texture.height) * 4 * size,
									0.0001f, depth);

			vP = p * v;

			planes = GeometryUtility.CalculateFrustumPlanes(vP);
		}

		static bool isInsideFrustum(int t1, int t2, int t3)
		{
			vecs[0] = vertices[t1];
			vecs[1] = vertices[t2];
			vecs[2] = vertices[t3];

			if (!FacingNormal(t1, t2, t3))
				return false;

			if (!GeometryUtility.TestPlanesAABB(planes, GeometryUtility.CalculateBounds(vecs, affectedObject.transform.localToWorldMatrix)))
				return false;

			return true;
		}

		static void GenerateUVs(int t1, int t2, int t3, ref Vector2[] uvs)
		{
			uvs[t1] = vP * decal.transform.localToWorldMatrix * new Vector4(vertices[t1].x, vertices[t1].y, vertices[t1].z, 1);
			uvs[t2] = vP * decal.transform.localToWorldMatrix * new Vector4(vertices[t2].x, vertices[t2].y, vertices[t2].z, 1);
			uvs[t3] = vP * decal.transform.localToWorldMatrix * new Vector4(vertices[t3].x, vertices[t3].y, vertices[t3].z, 1);

			if (decal.decalDefinition.sprite == null)
			{
				uvs[t1] *= 0.5f;
				uvs[t2] *= 0.5f;
				uvs[t3] *= 0.5f;

				Vector2 offset = Vector2.one * 0.5f;
				uvs[t1] += offset;
				uvs[t2] += offset;
				uvs[t3] += offset;
			}
			else
			{
				// scale to fit
				Vector2 aspect = new Vector2((decal.decalDefinition.sprite.rect.size.x / decal.decalDefinition.sprite.texture.width), (decal.decalDefinition.sprite.rect.size.y / decal.decalDefinition.sprite.texture.height));
				uvs[t1] *= aspect;
				uvs[t2] *= aspect;
				uvs[t3] *= aspect;

				//scale more
				uvs[t1] *= 0.5f;
				uvs[t2] *= 0.5f;
				uvs[t3] *= 0.5f;

				// move to sprite pos
				Vector2 pos = new Vector2(decal.decalDefinition.sprite.rect.center.x / decal.decalDefinition.sprite.texture.width, decal.decalDefinition.sprite.rect.center.y / decal.decalDefinition.sprite.texture.height);
				uvs[t1] += pos;
				uvs[t2] += pos;
				uvs[t3] += pos;
			}
		}

		static bool FacingNormal(int t1, int t2, int t3)
		{
			Vector3 vec1 = vertices[t2] - vertices[t1];
			Vector3 vec2 = vertices[t3] - vertices[t1];
			Vector3 norm = Vector3.Cross(vec1, vec2);
			norm.Normalize();

			if (Vector3.Dot(-direction.normalized, norm) < normalFactor)
				return false;

			return true;
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
			Rect rect;
			if (sprite)
			{
				rect = sprite.rect;
				rect.x /= sprite.texture.width;
				rect.y /= sprite.texture.height;
				rect.width /= sprite.texture.width;
				rect.height /= sprite.texture.height;
			}
			else
			{
				rect = new Rect();
				rect.xMin = rect.yMin = 0;
				rect.xMax = rect.yMax = 1;
			}

			for (int i = start; i < bufVertices.Count; i++)
			{
				Vector3 vertex = bufVertices[i];

				Vector2 uv = new Vector2(vertex.x + 0.5f, vertex.y + 0.5f);
				uv.x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
				uv.y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);

				bufTexCoords.Add(uv);
			}
		}

		static Mesh StaticCreateMesh()
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