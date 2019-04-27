using System;
using UnityEngine;

namespace DecalSystem
{
	public class Decal : MonoBehaviour
	{
		public DecalDefinition decalDefinition;
		public Mesh mesh;

		public Decal(DecalDefinition decalDef)
		{
			decalDefinition = decalDef;
		}
	}
}