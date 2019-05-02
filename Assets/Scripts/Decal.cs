using System;
using UnityEngine;

namespace DecalSystem
{
	public class Decal : MonoBehaviour
	{
		public DecalDefinition decalDefinition;
		public Mesh mesh;

		Vector3 oldScale;

		public void Init(DecalDefinition decalDef, Mesh mesh)
		{
			oldScale = transform.localScale;
			decalDefinition = decalDef;
			this.mesh = mesh;
			SetScale(decalDefinition.size);
		}

		public void SetScale(float size)
		{
			Vector3 scale = transform.localScale;
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
				scale.z = decalDefinition.depth;
				transform.localScale = scale * size;
			}
		}
	}
}