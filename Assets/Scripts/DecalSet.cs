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
		bool isSkinned;

		// cache vertices if it is static obj (unchanging)
		Vector3[] vertices;
		int[] triangles;

		void Init()
		{
			SkiMesh = GetComponent<SkinnedMeshRenderer>();
			MFilter = GetComponent<MeshFilter>();

			if (SkiMesh != null)
			{
				SkiMesh.sharedMesh.MarkDynamic();
				isSkinned = true;
			}
			else if (MFilter != null)
			{
				vertices = MFilter.sharedMesh.vertices;
				triangles = MFilter.sharedMesh.GetTriangles(0);
			}
		}

		void Awake()
		{
			Init();
		}

		public void AddDecal(DecalDefinition decalDefinition, Vector3 direction, Vector3 point, float size, Quaternion rotation, float normalFactor = 0, float pointBackwardOffset = 0.25f, float depth = 1)
		{
			// set globals
			DecalBuilder.SetUp(isSkinned,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				size,
				rotation,
				normalFactor,
				pointBackwardOffset,
				depth);

			Process();
		}

		public void AddDecal(DecalDefinition decalDefinition, Vector3 direction, Vector3 point)
		{
			// set globals
			DecalBuilder.SetUp(isSkinned,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				decalDefinition.size,
				decalDefinition.randomRotation ? Quaternion.Euler(0, 0, Random.Range(0, 360)) : Quaternion.Euler(0, 0, decalDefinition.rotation),
				decalDefinition.normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		void Process()
		{
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
			// create a mesh
			DecalBuilder.CreateDecalMeshStatic();
			// get decal
			DecalList.Add(DecalBuilder.decal);
		}

		void AddDecalSkinned()
		{
			// create mesh
			DecalBuilder.CreateDecalMeshSkinned(SkiMesh);
			// get decal
			DecalList.Add(DecalBuilder.decal);
		}

	}
}