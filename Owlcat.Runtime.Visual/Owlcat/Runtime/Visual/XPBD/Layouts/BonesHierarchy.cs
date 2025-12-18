using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts;

[Serializable]
public class BonesHierarchy
{
	public GameObject Skeleton;

	public Transform RootBone;

	public List<Mesh> Meshes;

	public List<int> BonesPerVertexInMesh;

	public List<Transform> Bones;

	public List<Matrix4x4> Bindposes;

	public List<Matrix4x4> Boneposes;

	public List<int> ParentMap;

	public List<int> MeshBoneMap;

	public List<int2> MeshBoneMapRanges;

	public bool ContainsData;

	public void Generate()
	{
		ContainsData = false;
		Meshes = new List<Mesh>();
		BonesPerVertexInMesh = new List<int>();
		Bones = new List<Transform>();
		Bindposes = new List<Matrix4x4>();
		Boneposes = new List<Matrix4x4>();
		ParentMap = new List<int>();
		MeshBoneMap = new List<int>();
		MeshBoneMapRanges = new List<int2>();
		Dictionary<Transform, Matrix4x4> dictionary = new Dictionary<Transform, Matrix4x4>();
		SkinnedMeshRenderer[] componentsInChildren = Skeleton.GetComponentsInChildren<SkinnedMeshRenderer>();
		List<Matrix4x4> list = new List<Matrix4x4>();
		RootBone = componentsInChildren[0].rootBone;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			Meshes.Add(skinnedMeshRenderer.sharedMesh);
			BonesPerVertexInMesh.Add(CalculateBonesPerVertex(skinnedMeshRenderer));
			list.Clear();
			Transform[] bones = skinnedMeshRenderer.bones;
			skinnedMeshRenderer.sharedMesh.GetBindposes(list);
			for (int j = 0; j < bones.Length; j++)
			{
				Matrix4x4 m = list[j];
				if (dictionary.TryGetValue(bones[j], out var value))
				{
					if (!CompareSkinningMatrices(ref m, ref value))
					{
						throw new ArgumentException("Bindpose conflicts in some meshes");
					}
				}
				else
				{
					dictionary.Add(bones[j], m);
				}
			}
		}
		foreach (KeyValuePair<Transform, Matrix4x4> item2 in dictionary)
		{
			Bones.Add(item2.Key);
			Bindposes.Add(item2.Value);
			Boneposes.Add(Skeleton.transform.worldToLocalMatrix * item2.Key.localToWorldMatrix);
		}
		Dictionary<Transform, int> dictionary2 = new Dictionary<Transform, int>();
		for (int k = 0; k < Bones.Count; k++)
		{
			dictionary2[Bones[k]] = k;
		}
		for (int l = 0; l < Bones.Count; l++)
		{
			Transform transform = Bones[l];
			if (dictionary2.TryGetValue(transform.parent, out var value2))
			{
				ParentMap.Add(value2);
			}
			else
			{
				ParentMap.Add(-1);
			}
		}
		for (int n = 0; n < componentsInChildren.Length; n++)
		{
			Transform[] bones2 = componentsInChildren[n].bones;
			int count = MeshBoneMap.Count;
			Transform[] array = bones2;
			foreach (Transform key in array)
			{
				int item = dictionary2[key];
				MeshBoneMap.Add(item);
			}
			MeshBoneMapRanges.Add(new int2(count, MeshBoneMap.Count - count));
		}
		ContainsData = true;
	}

	private int CalculateBonesPerVertex(SkinnedMeshRenderer skin)
	{
		BoneWeight[] boneWeights = skin.sharedMesh.boneWeights;
		int num = 0;
		BoneWeight[] array = boneWeights;
		for (int i = 0; i < array.Length; i++)
		{
			BoneWeight boneWeight = array[i];
			if (boneWeight.weight0 > 0f)
			{
				num = math.max(1, num);
			}
			if (boneWeight.weight1 > 0f)
			{
				num = math.max(2, num);
			}
			if (boneWeight.weight2 > 0f)
			{
				num = math.max(3, num);
			}
			if (boneWeight.weight3 > 0f)
			{
				num = math.max(4, num);
			}
		}
		return num;
	}

	private static bool CompareSkinningMatrices(ref Matrix4x4 m1, ref Matrix4x4 m2)
	{
		if ((double)Mathf.Abs(m1.m00 - m2.m00) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m01 - m2.m01) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m02 - m2.m02) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m03 - m2.m03) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m10 - m2.m10) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m11 - m2.m11) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m12 - m2.m12) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m13 - m2.m13) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m20 - m2.m20) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m21 - m2.m21) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m22 - m2.m22) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m23 - m2.m23) > 0.0001)
		{
			return false;
		}
		return true;
	}
}
