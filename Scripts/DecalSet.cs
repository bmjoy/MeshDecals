using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace lhlv.VFX.DecalSystem
{
	public class DecalSet : MonoBehaviour
	{
		[SerializeField]
		public int MaxDecals = 10;

		[Header("Skinned")]
		[SerializeField]
		public SkinQuality DecalQuality = SkinQuality.Bone4;

		[HideInInspector]
		public List<Decal> DecalList = new List<Decal>();

		// privates
		int DecalCount = 0;
		SkinnedMeshRenderer SkiMesh;
		MeshFilter MFilter;
		DecalType decType;

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
				decType = DecalType.Skinned;
			}
			else if (MFilter != null)
			{
				vertices = MFilter.sharedMesh.vertices;
				triangles = MFilter.sharedMesh.GetTriangles(0);
				decType = DecalType.Static;
			}
		}

		void Awake()
		{
			Init();
		}

		public void AddDecal(DecalDefinition decalDefinition, Vector3 direction, Vector3 point, float size, float angle, float normalFactor = 0, float pointBackwardOffset = 0.25f, float depth = 1)
		{
			// set globals
			DecalBuilder.SetUp(decType,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				size,
				angle,
				normalFactor,
				pointBackwardOffset,
				depth);

			Process();
		}

		public void AddDecal(DecalDefinition decalDefinition, Vector3 direction, Vector3 point)
		{
			// set globals
			DecalBuilder.SetUp(decType,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				decalDefinition.size,
				decalDefinition.randomAngle ? Random.Range(0, 360) : decalDefinition.angle,
				decalDefinition.normalFactor,
				decalDefinition.pointOffset,
				decalDefinition.depth);

			Process();
		}

		public void AddDecalQuad(DecalDefinition decalDefinition, Vector3 direction, Vector3 point, float size, float angle)
		{
			// set globals
			DecalBuilder.SetUp(DecalType.Quad,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				size,
				angle,
				-1,
				0,
				1);

			Process(true);
		}

		public void AddDecalQuad(DecalDefinition decalDefinition, Vector3 direction, Vector3 point)
		{
			// set globals
			DecalBuilder.SetUp(DecalType.Quad,
				gameObject,
				ref vertices,
				ref triangles,
				decalDefinition,
				direction,
				point,
				decalDefinition.size,
				decalDefinition.randomAngle ? Random.Range(0, 360) : decalDefinition.angle,
				-1,
				0,
				1);

			Process(true);
		}

		void Process(bool quad = false)
		{
			// choose which type of decal
			if (quad)
				AddDecalQuad();
			else if (decType == DecalType.Static)
				AddDecalStatic();
			else
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

		void AddDecalQuad()
		{
			// create mesh
			DecalBuilder.CreateDecalMeshQuad();
			// get decal
			DecalList.Add(DecalBuilder.decal);
		}

	}
}