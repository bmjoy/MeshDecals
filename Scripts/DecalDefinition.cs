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
		[Range(0.001f, 10f)] public float size = 0.2f;
		[Range(-1, 1)] public float normalFactor = 0;
		[Range(0, 20)] public float depth = 1;
		[Range(0, 360)] public float angle = 0;
		public bool randomAngle = true;
		[Range(0, 2)] public float pointOffset = 0.25f;
		public bool canExpand = false;
		[Range(0.01f, 1)] public float expandFactor = 0.1f;
		[Range(0.01f, 1)] public float minSize = 0.01f;
	}
}