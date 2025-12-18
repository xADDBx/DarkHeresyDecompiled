using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public abstract class SnapMapBase : MonoBehaviour
{
	[SerializeField]
	private float m_ParticleSizeScale = 1f;

	[SerializeField]
	private float m_SizeScale = 1f;

	[SerializeField]
	private float m_LifetimeScale = 1f;

	[SerializeField]
	private float m_RateOverTimeScale = 1f;

	[SerializeField]
	private float m_BurstScale = 1f;

	[Tooltip("Размер партиклов массива Bones")]
	public float ParticleSize = 0.35f;

	[Space(10f)]
	[SerializeField]
	public List<FxLocator> FxLocators = new List<FxLocator>();

	[HideInInspector]
	[SerializeField]
	private List<FxBone> m_Bones = new List<FxBone>();

	private readonly Dictionary<string, FxBone> m_BonesByName = new Dictionary<string, FxBone>();

	private readonly Dictionary<BlueprintFxLocatorGroup, List<FxBone>> m_LocatorGroups = new Dictionary<BlueprintFxLocatorGroup, List<FxBone>>();

	public bool Initialized { get; protected set; }

	public float AdditionalScale { get; set; } = 1f;


	public float AdditionalScaleReduced => 1f + (AdditionalScale - 1f) / 2f;

	public List<FxBone> Bones
	{
		get
		{
			return m_Bones;
		}
		set
		{
			m_Bones = value;
		}
	}

	public float ParticleSizeScale
	{
		get
		{
			return m_ParticleSizeScale * AdditionalScale;
		}
		set
		{
			m_ParticleSizeScale = value;
		}
	}

	public float SizeScale
	{
		get
		{
			return m_SizeScale * AdditionalScaleReduced;
		}
		set
		{
			m_SizeScale = value;
		}
	}

	public float LifetimeScale
	{
		get
		{
			return m_LifetimeScale * AdditionalScaleReduced;
		}
		set
		{
			m_LifetimeScale = value;
		}
	}

	public float RateOverTimeScale
	{
		get
		{
			return m_RateOverTimeScale * AdditionalScale;
		}
		set
		{
			m_RateOverTimeScale = value;
		}
	}

	public float BurstScale
	{
		get
		{
			return m_BurstScale * AdditionalScale;
		}
		set
		{
			m_BurstScale = value;
		}
	}

	public FxBone this[string boneName]
	{
		get
		{
			if (!Initialized)
			{
				Init();
			}
			m_BonesByName.TryGetValue(boneName, out var value);
			return value;
		}
	}

	private void Start()
	{
		if (!Initialized)
		{
			Init();
		}
	}

	public void ResetLocList()
	{
		FxLocators.Clear();
		foreach (FxBone bone in m_Bones)
		{
			Transform transform = FindChildRecursive(base.gameObject.transform, bone.Name);
			if ((bool)transform)
			{
				FxLocator fxLocator = transform.GetComponent<FxLocator>();
				if (fxLocator == null)
				{
					fxLocator = transform.gameObject.AddComponent<FxLocator>();
				}
				FxLocators.Add(fxLocator);
			}
		}
	}

	private void GetAllLocatorsFromHierarchy()
	{
		if (FxLocators.Count <= 0)
		{
			FxLocators = GetComponentsInChildren<FxLocator>().ToList();
		}
	}

	private static Transform FindChildRecursive(Transform transform, string name)
	{
		if (transform.name.Equals(name))
		{
			return transform;
		}
		foreach (Transform item in transform)
		{
			Transform transform2 = FindChildRecursive(item, name);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	public virtual void Init()
	{
		using (ProfileScope.New("Initialize SnapMapBase"))
		{
			m_BonesByName.Clear();
			InitGroups();
			OnInitialize();
			foreach (FxBone bone in Bones)
			{
				if (bone != null && bone.Name != null)
				{
					if (bone.Name.Contains("_Special"))
					{
						bone.Flags |= FxBoneFlags.Special;
					}
					else
					{
						bone.Flags &= ~FxBoneFlags.Special;
					}
					m_BonesByName[bone.Name] = bone;
					string[] array = bone.Aliases.EmptyIfNull();
					foreach (string key in array)
					{
						m_BonesByName[key] = bone;
					}
				}
			}
			Initialized = true;
		}
	}

	private void InitGroups()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		using (ProfileScope.New("Initialize Groups"))
		{
			FxLocators.Clear();
			m_Bones.Clear();
			m_LocatorGroups.Clear();
			FxLocators = GetComponentsInChildren<FxLocator>().ToList();
			foreach (FxLocator fxLocator in FxLocators)
			{
				if (fxLocator.Group != null)
				{
					List<FxBone> list = m_LocatorGroups.Get(fxLocator.Group);
					if (list == null)
					{
						m_LocatorGroups.Add(fxLocator.Group, list = new List<FxBone>());
					}
					fxLocator.Data.Name = fxLocator.name;
					fxLocator.Data.Transform = fxLocator.transform;
					list.Add(fxLocator.Data);
				}
			}
			m_Bones = m_LocatorGroups.SelectMany((KeyValuePair<BlueprintFxLocatorGroup, List<FxBone>> i) => i.Value).ToList();
		}
	}

	protected abstract void OnInitialize();

	public void RestoreBoneTransforms(CharacterBonesList bonesList = null)
	{
		using (ProfileScope.New("Restore Bone Transforms"))
		{
			bool flag = !Initialized;
			if (flag)
			{
				Init();
			}
			if (bonesList != null)
			{
				foreach (FxBone bone in Bones)
				{
					bone.Transform = bonesList.GetByName(bone.Name);
				}
				return;
			}
			if (!flag)
			{
				InitGroups();
			}
		}
	}

	public IReadOnlyList<FxBone> GetLocators(BlueprintFxLocatorGroup group)
	{
		if (group == null)
		{
			return null;
		}
		return m_LocatorGroups.Get(group);
	}

	public FxBone GetLocatorFirst(BlueprintFxLocatorGroup group)
	{
		if (group == null)
		{
			return null;
		}
		return m_LocatorGroups.Get(group)?.FirstItem();
	}
}
