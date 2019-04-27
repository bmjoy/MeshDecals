using System;
using UnityEngine;

namespace DecalSystem
{
	public class Decal : MonoBehaviour
	{
		public DecalDefinition decalDefinition;

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;

		[HideInInspector]
		public bool initialized = false;

		public void Init(DecalDefinition decalDef = null)
		{
			if (initialized)
				Debug.Log("Already initialized");
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();

			if (decalDef != null)
				decalDefinition = decalDef;
			meshRenderer.material = decalDefinition.material;

			initialized = true;
		}

		void Start()
		{
			if (!initialized)
				Init();
		}
	}
}