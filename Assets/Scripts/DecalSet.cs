using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecalSystem
{
    public class DecalSet : MonoBehaviour
    {
		public int m_MaxDecals = 10;
		
		[HideInInspector]
		public List<GameObject> m_DecalList = new List<GameObject>();
		
		protected int m_DecalCount = 0;
		protected Vector3[] m_Vertices;
		protected Matrix4x4 m_VP;
		protected float m_NormalFactor;
		protected float m_Depth;
		protected float m_Offset;
		protected Vector3 m_Dir;
		protected float m_UVRot;
		
		public virtual void AddDecal(Transform origin, Vector3 point, Decal decal, float size = 0.2f, float rotation = 0, float normalFactor = 0, float offset = 0.1f, float depth = 1)
		{
			
		}
    }
}