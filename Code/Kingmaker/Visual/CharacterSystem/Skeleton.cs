using System;
using System.Collections.Generic;
using Kingmaker.UI.DollRoom;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class Skeleton : ScriptableObject
{
	[Serializable]
	public class Bone
	{
		[ArrayElementNameProvider]
		public string Name;

		public Vector3 Scale = Vector3.one;

		public Vector3 Offset = Vector3.zero;

		public bool ApplyOffset;
	}

	public struct BoneData
	{
		public Vector3 Scale;

		public Vector3 Offset;

		public bool ApplyOffset;
	}

	[Serializable]
	public struct BoneType
	{
		[SerializeField]
		public string BoneName;
	}

	[Serializable]
	public class BoneModifier
	{
		[SerializeField]
		private BoneType m_BoneType;

		[SerializeField]
		private bool m_ApplyScale;

		[ShowIf("m_ApplyScale")]
		[SerializeField]
		private Vector3 m_Scale = Vector3.one;

		[SerializeField]
		private bool m_ApplyOffset;

		[ShowIf("m_ApplyOffset")]
		[SerializeField]
		private Vector3 m_Offset = Vector3.zero;

		[Tooltip("Модификатор не будет применяться, если на персонаже есть хотя бы одно из ЕЕ из списка")]
		[SerializeField]
		private bool m_IgnoreModifierOnCharacterWithSpecificEE;

		[ShowIf("m_IgnoreModifierOnCharacterWithSpecificEE")]
		[SerializeField]
		private List<EquipmentEntity> m_ConflictEE;

		public BoneType BoneType => m_BoneType;

		public bool IsValid()
		{
			if (!m_ApplyOffset || !(m_Offset != Vector3.zero))
			{
				if (m_ApplyScale)
				{
					return m_Scale != Vector3.one;
				}
				return false;
			}
			return true;
		}

		public bool HasConflictsWithEquipment(List<EquipmentEntity> equipmentEntities)
		{
			if (!m_IgnoreModifierOnCharacterWithSpecificEE)
			{
				return false;
			}
			using (List<EquipmentEntity>.Enumerator enumerator = m_ConflictEE.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					EquipmentEntity current = enumerator.Current;
					return equipmentEntities.Contains(current);
				}
			}
			return false;
		}

		public void Apply(Transform boneTransform)
		{
			if (m_ApplyScale)
			{
				Vector3 localScale = Vector3.Scale(boneTransform.localScale, m_Scale);
				boneTransform.localScale = localScale;
			}
			if (m_ApplyOffset)
			{
				boneTransform.localPosition = m_Offset;
			}
		}
	}

	private bool m_IsDirty;

	public CharacterFxBonesMap CharacterFxBonesMap;

	[Tooltip("Required only for copying list of bones in list below")]
	public GameObject RaceBoneHierarchyObject;

	public AnimationSet AnimationSetOverride;

	public List<Bone> Bones;

	[SerializeField]
	private DollRoomCameraZoomPreset m_DollRoomZoomPreset;

	private NativeArray<BoneData> m_BoneDataForJob;

	public DollRoomCameraZoomPreset DollRoomZoomPreset => m_DollRoomZoomPreset;

	private void CreateBoneData()
	{
		if (m_BoneDataForJob.IsCreated)
		{
			m_BoneDataForJob.Dispose();
		}
		m_BoneDataForJob = new NativeArray<BoneData>(Bones.Count, Allocator.Persistent);
		for (int i = 0; i < Bones.Count; i++)
		{
			m_BoneDataForJob[i] = new BoneData
			{
				ApplyOffset = Bones[i].ApplyOffset,
				Offset = Bones[i].Offset,
				Scale = Bones[i].Scale
			};
		}
	}

	public NativeArray<BoneData> GetBoneData()
	{
		if (!m_BoneDataForJob.IsCreated)
		{
			CreateBoneData();
		}
		return m_BoneDataForJob;
	}

	public void SetSkeletonDirty()
	{
		m_IsDirty = true;
	}

	public bool IsDirty()
	{
		return m_IsDirty;
	}

	public void ResetDirty()
	{
		m_IsDirty = false;
	}

	private void OnDisable()
	{
		if (m_BoneDataForJob.IsCreated)
		{
			m_BoneDataForJob.Dispose();
		}
	}
}
