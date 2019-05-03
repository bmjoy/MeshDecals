using System;
using UnityEngine;

namespace DecalSystem
{
	public class Decal : MonoBehaviour
	{
		public DecalDefinition decalDefinition;

		public bool isSkinned;

		public SkinnedMeshRenderer smr;
		public MeshFilter mf;
		public MeshRenderer mr;

		Vector3 oldScale;

		public void Init(DecalDefinition decalDef, bool isSkinned, GameObject affectedObj)
		{
			this.isSkinned = isSkinned;
			if (isSkinned)
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
			if (isSkinned)
				smr.sharedMesh = mesh;
			else
				mf.sharedMesh = mesh;
		}

		public Mesh GetMesh()
		{
			if (isSkinned)
				return smr.sharedMesh;
			else
				return mf.sharedMesh;
		}
	}
}