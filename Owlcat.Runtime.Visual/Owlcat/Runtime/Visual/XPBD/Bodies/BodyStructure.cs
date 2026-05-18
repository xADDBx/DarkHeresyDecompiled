using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

[Serializable]
public class BodyStructure
{
	public List<Particle> Particles = new List<Particle>();

	public List<Constraint> Constraints = new List<Constraint>();

	public List<ConstraintBatch> ConstraintBatches = new List<ConstraintBatch>();

	public int2 SimplexOffsetCount = -1;

	public uint UsedConstraints;

	public List<int> VertexToParticlesMap;

	public List<int> ParticleToVertexMap;

	public List<int> Triangles;

	public List<int> VertexTrianglesMap;

	public List<int2> VertexTrianglesMapRanges;

	public List<float3> RestNormals;

	public bool UseSkin;

	public int2 SkinBufferRange;

	public List<BodyBone> Bones;

	public List<string> BoneNames;

	public List<string> SkeletonMeshNames;

	public List<int> MeshBonesMap;

	public List<int2> MeshBonesMapRanges;

	public List<ParticleGroup> Groups = new List<ParticleGroup>();

	public Bounds Bounds;

	public bool AllParticlesInUse;

	internal void RecalculateBounds()
	{
		if (Particles.Count > 0)
		{
			Bounds = new Bounds((Vector3)Particles[0].BasePosition, Vector3.zero);
			for (int i = 1; i < Particles.Count; i++)
			{
				Bounds.Encapsulate((Vector3)Particles[i].BasePosition);
			}
		}
		else
		{
			Bounds = default(Bounds);
		}
	}

	internal void CreateVertexTrianglesMap()
	{
		List<HashSet<int>> list = new List<HashSet<int>>();
		for (int i = 0; i < Particles.Count; i++)
		{
			list.Add(new HashSet<int>());
		}
		int num = Triangles.Count / 3;
		for (int j = 0; j < num; j++)
		{
			int index = Triangles[j * 3];
			int index2 = Triangles[j * 3 + 1];
			int index3 = Triangles[j * 3 + 2];
			list[index].Add(j);
			list[index2].Add(j);
			list[index3].Add(j);
		}
		VertexTrianglesMap = new List<int>();
		VertexTrianglesMapRanges = new List<int2>();
		for (int k = 0; k < list.Count; k++)
		{
			HashSet<int> hashSet = list[k];
			VertexTrianglesMapRanges.Add(new int2(VertexTrianglesMap.Count, hashSet.Count));
			VertexTrianglesMap.AddRange(hashSet);
		}
	}

	internal void Clear()
	{
		Particles.Clear();
		Constraints.Clear();
		ConstraintBatches.Clear();
		SimplexOffsetCount = -1;
		UsedConstraints = 0u;
		VertexToParticlesMap = null;
		ParticleToVertexMap = null;
		Triangles = null;
		RestNormals = null;
		VertexTrianglesMap = null;
		VertexTrianglesMapRanges = null;
		UseSkin = false;
		SkinBufferRange = -1;
		Bones = null;
		BoneNames = null;
		SkeletonMeshNames = null;
		MeshBonesMap = null;
		MeshBonesMapRanges = null;
		AllParticlesInUse = false;
	}

	public void CopyFrom(BodyStructure other)
	{
		Clear();
		Particles = new List<Particle>(other.Particles);
		Constraints = new List<Constraint>(other.Constraints);
		ConstraintBatches = new List<ConstraintBatch>(other.ConstraintBatches);
		SimplexOffsetCount = other.SimplexOffsetCount;
		UsedConstraints = other.UsedConstraints;
		if (other.VertexToParticlesMap != null && other.VertexToParticlesMap.Count > 0)
		{
			VertexToParticlesMap = new List<int>(other.VertexToParticlesMap);
		}
		if (other.ParticleToVertexMap != null && other.ParticleToVertexMap.Count > 0)
		{
			ParticleToVertexMap = new List<int>(other.ParticleToVertexMap);
		}
		if (other.Triangles != null && other.Triangles.Count > 0)
		{
			Triangles = new List<int>(other.Triangles);
		}
		if (other.VertexTrianglesMap != null && other.VertexTrianglesMap.Count > 0)
		{
			VertexTrianglesMap = new List<int>(other.VertexTrianglesMap);
		}
		if (other.VertexTrianglesMapRanges != null && other.VertexTrianglesMapRanges.Count > 0)
		{
			VertexTrianglesMapRanges = new List<int2>(other.VertexTrianglesMapRanges);
		}
		if (other.RestNormals != null && other.RestNormals.Count > 0)
		{
			RestNormals = new List<float3>(other.RestNormals);
		}
		UseSkin = other.UseSkin;
		SkinBufferRange = other.SkinBufferRange;
		if (other.Bones != null && other.Bones.Count > 0)
		{
			Bones = new List<BodyBone>(other.Bones);
		}
		if (other.BoneNames != null && other.BoneNames.Count > 0)
		{
			BoneNames = new List<string>(other.BoneNames);
		}
		if (other.SkeletonMeshNames != null && other.SkeletonMeshNames.Count > 0)
		{
			SkeletonMeshNames = new List<string>(other.SkeletonMeshNames);
		}
		if (other.MeshBonesMap != null && other.MeshBonesMap.Count > 0)
		{
			MeshBonesMap = new List<int>(other.MeshBonesMap);
		}
		if (other.MeshBonesMapRanges != null && other.MeshBonesMapRanges.Count > 0)
		{
			MeshBonesMapRanges = new List<int2>(other.MeshBonesMapRanges);
		}
		Groups.Clear();
		foreach (ParticleGroup group in other.Groups)
		{
			ParticleGroup particleGroup = ScriptableObject.CreateInstance<ParticleGroup>();
			particleGroup.name = group.name;
			particleGroup.ParticleIndices.AddRange(group.ParticleIndices);
			Groups.Add(particleGroup);
		}
		AllParticlesInUse = other.AllParticlesInUse;
		RecalculateBounds();
	}
}
