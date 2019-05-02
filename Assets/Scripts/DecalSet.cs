using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DecalSystem
{
	public enum DecalSetType
	{
		Static,
		Skinned,
	}

	public class DecalSet : MonoBehaviour
	{
		public int MaxDecals = 10;
		[Header("Skinned")]
		public SkinQuality DecalQuality = SkinQuality.Bone4;

		[HideInInspector]
		public List<Decal> DecalList = new List<Decal>();

		// privates
		int DecalCount = 0;
		SkinnedMeshRenderer SkiMesh;
		MeshFilter MFilter;
		Vector3[] Vertices;
		bool isSkinned = false;

		// builder vars
		Matrix4x4 VP;
		Vector3 Direction;
		Vector3 Point;
		DecalDefinition DecalDef;
		float Size;
		Quaternion Rotation;
		float NormalFactor;
		float PointBackwardOffset;
		float Depth;
		Plane[] Planes;

		void Init()
		{
			SkiMesh = GetComponent<SkinnedMeshRenderer>();
			MFilter = GetComponent<MeshFilter>();

			if (SkiMesh != null)
			{
				Vertices = SkiMesh.sharedMesh.vertices;
				SkiMesh.sharedMesh.MarkDynamic();
				isSkinned = true;
			}
			else if (MFilter != null)
			{
				Vertices = MFilter.sharedMesh.vertices;
			}
		}

		void Awake()
		{
			Init();
		}

		public void AddDecal(Vector3 direction, Vector3 point, DecalDefinition decalDefinition, float size, Quaternion rotation, float normalFactor = 0, float pointBackwardOffset = 0.25f, float depth = 1)
		{
			// set globals
			SetUp(direction,
				point,
				decalDefinition,
				size,
				rotation,
				normalFactor,
				pointBackwardOffset,
				depth);

			Process();
		}

		public void AddDecal(Vector3 direction, Vector3 point, DecalDefinition decalDefinition)
		{
			// set globals
			SetUp(direction,
				point,
				decalDefinition,
				decalDefinition.size,
				decalDefinition.randomRotation ? Quaternion.Euler(0, 0, Random.Range(0, 360)) : Quaternion.Euler(0, 0, decalDefinition.rotation),
				decalDefinition.normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		public void AddDecal(Vector3 direction, Vector3 point, DecalDefinition decalDefinition, float size)
		{
			// set globals
			SetUp(direction,
				point,
				decalDefinition,
				size,
				decalDefinition.randomRotation ? Quaternion.Euler(0, 0, Random.Range(0, 360)) : Quaternion.Euler(0, 0, decalDefinition.rotation),
				decalDefinition.normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		public void AddDecal(Vector3 direction, Vector3 point, DecalDefinition decalDefinition, float size, Quaternion rotation)
		{
			// set globals
			SetUp(direction,
				point,
				decalDefinition,
				size,
				rotation,
				decalDefinition.normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		public void AddDecal(Vector3 direction, Vector3 point, DecalDefinition decalDefinition, float size, Quaternion rotation, float normalFactor = 0)
		{
			// set globals
			SetUp(direction,
				point,
				decalDefinition,
				size,
				rotation,
				normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		void SetUp(Vector3 direction, Vector3 point, DecalDefinition decalDefinition, float size, Quaternion rotation, float normalFactor = 0, float pointBackwardOffset = 0.25f, float depth = 1)
		{
			// set globals
			Direction = direction;
			Point = point;
			DecalDef = decalDefinition;
			Size = size;
			Rotation = rotation;
			NormalFactor = normalFactor;
			PointBackwardOffset = pointBackwardOffset;
			Depth = depth;
		}

		void CalculateMatrixAndPlanes()
		{
			// project from a close point from the hit point
			Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(Point - Direction * PointBackwardOffset, Quaternion.LookRotation(Direction, Vector3.up) * Rotation, new Vector3(1, 1, -1)));
			// project from origin (need a high depth value)
			// Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(origin.position, origin.rotation, new Vector3(1, 1, -1)));

			Matrix4x4 p;
			if (DecalDef.sprite == null)
				p = Matrix4x4.Ortho(-Size, Size, -Size, Size, 0.0001f, Depth);
			else
				p = Matrix4x4.Ortho(-(DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width) * Size,
									(DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width) * Size,
									-(DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height) * Size,
									(DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height) * Size,
									0.0001f, Depth);

			VP = p * v;

			Planes = GeometryUtility.CalculateFrustumPlanes(VP);
		}

		void Process()
		{
			// calculate matrix and frustum planes
			CalculateMatrixAndPlanes();

			// choose which type of decal
			if (isSkinned)
				AddDecalSkinned();
			else
				AddDecalStatic();

			// increase counter
			DecalCount++;

			// check for limits 
			while (DecalCount > MaxDecals)
			{
				Destroy(DecalList[0].gameObject);
				DecalList.RemoveAt(0);
				DecalCount--;
			}
		}

		void AddDecalStatic()
		{
			// get decalmesh
			Mesh decalMesh = Mesh.Instantiate(MFilter.sharedMesh);

			// uvs
			Vector2[] uvs = decalMesh.uv;

			// process each submesh
			for (int subMesh = 0; subMesh < decalMesh.subMeshCount; subMesh++)
			{
				List<int> triangleList = new List<int>();
				int[] triangles = decalMesh.GetTriangles(subMesh);

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

			// create go
			GameObject decalGO = new GameObject("decalSkinned");
			decalGO.transform.parent = MFilter.transform;
			decalGO.transform.localPosition = Vector3.zero;
			decalGO.transform.localRotation = Quaternion.identity;
			decalGO.transform.localScale = Vector3.one;

			// meshfilter
			MeshFilter decalMf = decalGO.AddComponent<MeshFilter>();
			decalMf.sharedMesh = decalMesh;

			// create mesh renderer compnent
			MeshRenderer decalSkinRend = decalGO.AddComponent<MeshRenderer>();
			decalSkinRend.shadowCastingMode = ShadowCastingMode.Off;
			decalSkinRend.sharedMaterial = DecalDef.material;

			// create decal component
			Decal decal = decalGO.AddComponent<Decal>();
			decal.Init(DecalDef, decalMesh);

			DecalList.Add(decal);
		}

		void AddDecalSkinned()
		{
			// get decalmesh
			Mesh decalMesh = Mesh.Instantiate(SkiMesh.sharedMesh);

			// get a snapshot of the mesh to test against
			Mesh bakeMesh = new Mesh();
			SkiMesh.BakeMesh(bakeMesh);

			// get vertices that are going to be tested
			Vertices = bakeMesh.vertices;

			// uvs
			Vector2[] uvs = decalMesh.uv;

			// process each submesh
			for (int subMesh = 0; subMesh < decalMesh.subMeshCount; subMesh++)
			{
				List<int> triangleList = new List<int>();
				int[] triangles = decalMesh.GetTriangles(subMesh);

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

			// create go
			GameObject decalGO = new GameObject("decalSkinned");
			decalGO.transform.parent = SkiMesh.transform;

			// create skinned mesh compnent
			SkinnedMeshRenderer decalSkinRend = decalGO.AddComponent<SkinnedMeshRenderer>();
			decalSkinRend.quality = DecalQuality;
			decalSkinRend.shadowCastingMode = ShadowCastingMode.Off;
			decalSkinRend.sharedMesh = decalMesh;
			decalSkinRend.bones = SkiMesh.bones;
			decalSkinRend.rootBone = SkiMesh.rootBone;
			decalSkinRend.sharedMaterial = DecalDef.material;

			// create decal component
			Decal decal = decalGO.AddComponent<Decal>();
			decal.Init(DecalDef, decalMesh);

			DecalList.Add(decal);

			Destroy(bakeMesh);
		}

		Vector3[] vecs = new Vector3[3];
		bool isInsideFrustum(int t1, int t2, int t3)
		{
			vecs[0] = Vertices[t1];
			vecs[1] = Vertices[t2];
			vecs[2] = Vertices[t3];

			if (!FacingNormal(t1, t2, t3))
				return false;

			if (!GeometryUtility.TestPlanesAABB(Planes, GeometryUtility.CalculateBounds(vecs, transform.localToWorldMatrix)))
				return false;

			return true;
		}

		void GenerateUVs(int t1, int t2, int t3, ref Vector2[] uvs)
		{
			uvs[t1] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t1].x, Vertices[t1].y, Vertices[t1].z, 1);
			uvs[t2] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t2].x, Vertices[t2].y, Vertices[t2].z, 1);
			uvs[t3] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t3].x, Vertices[t3].y, Vertices[t3].z, 1);

			if (DecalDef.sprite == null)
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
				Vector2 aspect = new Vector2((DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width), (DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height));
				uvs[t1] *= aspect;
				uvs[t2] *= aspect;
				uvs[t3] *= aspect;

				//scale more
				uvs[t1] *= 0.5f;
				uvs[t2] *= 0.5f;
				uvs[t3] *= 0.5f;

				// TODO: Fix rotation offset error
				// move to sprite pos
				Vector2 pos = new Vector2(DecalDef.sprite.rect.center.x / DecalDef.sprite.texture.width, DecalDef.sprite.rect.center.y / DecalDef.sprite.texture.height);
				uvs[t1] += pos;
				uvs[t2] += pos;
				uvs[t3] += pos;
			}
		}

		bool FacingNormal(int t1, int t2, int t3)
		{
			// Plane plane = new Plane(v1, v2, v3);
			Vector3 vec1 = Vertices[t2] - Vertices[t1];
			Vector3 vec2 = Vertices[3] - Vertices[t1];
			Vector3 norm = Vector3.Cross(vec1, vec2);
			norm.Normalize();

			if (Vector3.Dot(-Direction.normalized, norm) < NormalFactor)
				return false;

			return true;
		}

	}
}