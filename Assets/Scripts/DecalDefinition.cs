using System;
using UnityEngine;

namespace DecalSystem
{
	public class DecalDefinition : ScriptableObject
	{
		public Material material;
		public Sprite sprite;
		public bool randomRotation = true;
	}
}