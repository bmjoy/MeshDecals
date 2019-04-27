using System;
using UnityEngine;

namespace DecalSystem
{
	public class Decal : MonoBehaviour
	{
		public DecalDefinition decalDefinition;
		public Mesh mesh;

		public void Init(DecalDefinition decalDef, Mesh mesh)
		{
			decalDefinition = decalDef;
			this.mesh = mesh;
		}
	}
}