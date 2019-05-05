using System;
using UnityEngine;

namespace lhlv.VFX.DecalSystem
{
	[CreateAssetMenu(fileName = "DecalDef", menuName = "Decal Definition")]
	public class DecalDefinition : ScriptableObject
	{
		public Color color = Color.white;
		public Material material;
		public Sprite sprite;
		[Range(0.001f, 2f)] public float size = 0.2f;
		[Range(-1, 1)] public float normalFactor = 0;
		[Range(0, 10)] public float depth = 1;
		public float angle = 0;
		public float pointOffset = 0.25f;
		public bool randomAngle = true;
	}
}