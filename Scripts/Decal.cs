using System;
using UnityEngine;

namespace lhlv.VFX.DecalSystem
{
	public enum DecalType
	{
		Static,
		Skinned,
		Quad,
	}
	public class Decal : MonoBehaviour
	{

		public DecalDefinition decalDefinition;

		public DecalType decalT;

		public SkinnedMeshRenderer smr;
		public MeshFilter mf;
		public MeshRenderer mr;
		public new BoxCollider collider;

		Vector3 oldScale;
		Vector2[] originalUvs;
		Vector2[] uvs;
		Mesh decalMesh;

		public void Init(DecalDefinition decalDef, DecalType decType, GameObject affectedObj)
		{
			decalT = decType;
			if (decalT == DecalType.Skinned)
			{
				smr = gameObject.AddComponent<SkinnedMeshRenderer>();
				if (affectedObj)
				{
					smr.rootBone = affectedObj.GetComponent<SkinnedMeshRenderer>().rootBone;
					smr.bones = affectedObj.GetComponent<SkinnedMeshRenderer>().bones;
					smr.sharedMaterial = decalDef.material;
				}
			}
			else
			{
				mf = gameObject.AddComponent<MeshFilter>();
				mr = gameObject.AddComponent<MeshRenderer>();
				mr.sharedMaterial = decalDef.material;
			}

			oldScale = transform.localScale;
			decalDefinition = decalDef;
			SetScale(decalDefinition.size);
		}

		public void SetScale(float size)
		{
			Vector3 scale = Vector3.one;
			if (decalDefinition.sprite != null)
			{
				float ratio = (float)decalDefinition.sprite.rect.width / decalDefinition.sprite.rect.height;
				if (oldScale.x != scale.x)
				{
					scale.y = scale.x / ratio;
				}
				else
				if (oldScale.y != scale.y)
				{
					scale.x = scale.y * ratio;
				}
				else
				if (scale.x != scale.y * ratio)
				{
					scale.x = scale.y * ratio;
				}
			}
			scale.z = decalDefinition.depth;
			transform.localScale = scale * size;
		}

		public void SetMesh(Mesh mesh)
		{
			if (decalT == DecalType.Skinned)
				smr.sharedMesh = mesh;
			else
				mf.sharedMesh = mesh;
			decalMesh = mesh;

			// if can uv expand, set decal to small size, to then grow if necessary
			if (decalDefinition.canExpand)
			{
				// create a collider, because we need it to know when to grow
				collider = gameObject.AddComponent<BoxCollider>();
				// we need to downsize collider too
				collider.size = new Vector3(decalDefinition.minSize, decalDefinition.minSize, decalDefinition.minSize / 2);

				if (decalT != DecalType.Quad)
				{
					originalUvs = mesh.uv;
					uvs = new Vector2[originalUvs.Length];

					// center uvs, and make the image small
					for (int i = 0; i < uvs.Length; i++)
					{
						uvs[i] = (originalUvs[i] * 10f) - (Vector2.one * 5f);
					}

					decalMesh.uv = uvs;
				}
				else
				{
					transform.localScale = Vector3.one * decalDefinition.minSize;
				}
			}
		}

		public Mesh GetMesh()
		{
			return decalMesh;
		}


		public void Expand()
		{
			if (decalDefinition.canExpand)
			{
				if (decalT != DecalType.Quad)
				{
					for (int i = 0; i < originalUvs.Length; i++)
					{
						uvs[i] = Vector2.Lerp(uvs[i], originalUvs[i], decalDefinition.expandFactor);
					}
					decalMesh.uv = uvs;
				}
				else
					transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * decalDefinition.size, decalDefinition.expandFactor);

				collider.size = Vector3.Lerp(collider.size, new Vector3(1, 1, decalDefinition.minSize / 2) / 4, decalDefinition.expandFactor);
			}

		}
	}
}