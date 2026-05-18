using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.UI.DollRoom;

internal sealed class DollRoomAvatarManager<TKey>
{
	private readonly Dictionary<TKey, Character> m_Cache = new Dictionary<TKey, Character>();

	private readonly Transform m_Parent;

	private readonly Action<Character> m_OnAvatarUpdated;

	public Character Active { get; private set; }

	public DollRoomAvatarManager([NotNull] Transform parent, [NotNull] Action<Character> onAvatarUpdated)
	{
		m_Parent = parent;
		m_OnAvatarUpdated = onAvatarUpdated;
	}

	[NotNull]
	public Character GetOrCreate(TKey key, [NotNull] Character prefab, string name, Action<Character> onCreated = null)
	{
		if (!m_Cache.TryGetValue(key, out var value))
		{
			value = CreateAvatar(prefab, name);
			m_Cache[key] = value;
			onCreated?.Invoke(value);
		}
		return value;
	}

	public void SetActive(TKey key)
	{
		if (m_Cache.TryGetValue(key, out var value))
		{
			if (Active != null && Active != value)
			{
				Active.gameObject.SetActive(value: false);
			}
			Active = value;
			Active.gameObject.SetActive(value: true);
		}
	}

	public void SetupAnimationManager([NotNull] UnitAnimationManager animationManager, bool hasBallisticMechadendrite = false)
	{
		animationManager.UpdateMode = DirectorUpdateMode.UnscaledGameTime;
		animationManager.IsInCombat = true;
		animationManager.HasBallisticMechadendrite = hasBallisticMechadendrite;
		animationManager.UpdateActiveWeaponStyle();
		animationManager.Tick(RealTimeController.SystemStepDurationSeconds);
	}

	public void Cleanup()
	{
		foreach (Character value in m_Cache.Values)
		{
			if (!(value == null))
			{
				value.OnUpdated -= m_OnAvatarUpdated;
				value.OnOutfitOnlyUpdated -= m_OnAvatarUpdated;
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		m_Cache.Clear();
		Active = null;
	}

	private Character CreateAvatar([NotNull] Character prefab, string dollName)
	{
		Character character = new GameObject("Doll [" + dollName + "]").EnsureComponent<Character>();
		character.GenderAndRace = prefab.GenderAndRace;
		character.CopyEquipmentFrom(prefab);
		character.transform.localScale = prefab.transform.localScale;
		character.DisplayOptions.IsInDollRoom = true;
		character.ForbidBeltItemVisualization = prefab.ForbidBeltItemVisualization;
		character.AnimatorPrefab = prefab.AnimatorPrefab;
		character.Skeleton = prefab.Skeleton;
		character.AnimationSet = prefab.AnimationSet;
		character.OnUpdated += m_OnAvatarUpdated;
		character.OnOutfitOnlyUpdated += m_OnAvatarUpdated;
		character.AtlasData = prefab.AtlasData;
		character.Initialize();
		character.Animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
		character.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		character.transform.SetParent(m_Parent, worldPositionStays: false);
		character.transform.localRotation = Quaternion.Euler(0f, -25f, 0f);
		return character;
	}
}
