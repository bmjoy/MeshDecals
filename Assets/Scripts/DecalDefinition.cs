using System;
using UnityEngine;

namespace DecalSystem
{
	[CreateAssetMenu(fileName="DecalDef",menuName="Decal Definition")]
	public class DecalDefinition : ScriptableObject
	{
		public Material material;
		public Sprite sprite;
		public bool randomRotation = true;
		public float rotation;
	}
}