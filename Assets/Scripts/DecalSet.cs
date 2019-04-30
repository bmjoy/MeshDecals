using System;
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
		public DecalSetType SetType = DecalSetType.Static;
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

		// builder vars
		Matrix4x4 VP;
		Transform Origin;
		Vector3 Point;
		DecalDefinition DecalDef;
		float Size;
		float Rotation;
		float NormalFactor;
		float PointBackwardOffset;
		float Depth;
		Plane[] Planes;

		void Init()
		{
			SkiMesh = GetComponent<SkinnedMeshRenderer>();
			MFilter = GetComponent<MeshFilter>();

			if (SetType == DecalSetType.Static)
				Vertices = MFilter.sharedMesh.vertices;
		}

		void Start()
		{
			Init();
		}

		void CalculateMatrixAndPlanes()
		{
			// project from a close point from the hit point
			// Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(Point - Origin.forward * PointBackwardOffset, Quaternion.Euler(0, 0, Rotation) *Origin.transform.rotation, new Vector3(1, 1, -1)));
			Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(Point - Origin.forward * PointBackwardOffset, Quaternion.Euler(0, 0, DecalDef.rotation) * Origin.transform.rotation, new Vector3(1, 1, -1)));
			// project from origin (need a high depth value)
			// Matrix4x4 v = Matrix4x4.Inverse(Matrix4x4.TRS(origin.position, origin.rotation, new Vector3(1, 1, -1)));
			Matrix4x4 p = Matrix4x4.Ortho(-(DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width) * Size,
											(DecalDef.sprite.rect.size.x / DecalDef.sprite.texture.width) * Size,
											-(DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height) * Size,
											(DecalDef.sprite.rect.size.y / DecalDef.sprite.texture.height) * Size,
											0.0001f, Depth);
			VP = p * v;

			Planes = GeometryUtility.CalculateFrustumPlanes(VP);
		}

		public void AddDecal(Transform origin, Vector3 point, DecalDefinition decalDefinition, float size = 0.2f, float rotation = 0, float normalFactor = 0, float pointBackwardOffset = 0.1f, float depth = 1)
		{
			// set globals
			Origin = origin;
			Point = point;
			DecalDef = decalDefinition;
			Size = size;
			Rotation = decalDefinition.randomRotation ? rotation : 0;
			NormalFactor = normalFactor;
			PointBackwardOffset = pointBackwardOffset;
			Depth = depth;

			// calculate matrix and frustum planes
			CalculateMatrixAndPlanes();

			// choose which type of decal
			if (SetType == DecalSetType.Static)
				AddDecalStatic();
			else if (SetType == DecalSetType.Skinned)
				AddDecalSkinned();

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

		bool isInsideFrustum(int t1, int t2, int t3)
		{


			// check against the 6 planes
			for (int i = 0; i < 6; i++)
			{
				if (!Planes[i].GetSide(transform.TransformPoint(Vertices[t1])) && !Planes[i].GetSide(transform.TransformPoint(Vertices[t2])) && !Planes[i].GetSide(transform.TransformPoint(Vertices[t3])))
					return false;
				if (!FacingNormal(Vertices[t1], Vertices[t2], Vertices[t3]))
					return false;
			}
			return true;
		}

		void GenerateUVs(int t1, int t2, int t3, ref Vector2[] uvs)
		{
			uvs[t1] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t1].x, Vertices[t1].y, Vertices[t1].z, 1);
			uvs[t2] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t2].x, Vertices[t2].y, Vertices[t2].z, 1);
			uvs[t3] = VP * transform.localToWorldMatrix * new Vector4(Vertices[t3].x, Vertices[t3].y, Vertices[t3].z, 1);

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


		bool FacingNormal(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Plane plane = new Plane(v1, v2, v3);

			if (Vector3.Dot(-Origin.forward.normalized, plane.normal) < NormalFactor)
				return false;

			return true;
		}

	}
}