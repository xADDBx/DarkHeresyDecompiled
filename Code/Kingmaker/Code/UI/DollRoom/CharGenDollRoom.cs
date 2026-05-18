using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.DollRoom;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Code.UI.DollRoom;

public class CharGenDollRoom : DollRoomBase, ILevelUpDollHandler, ISubscriber
{
	[SerializeField]
	private AnimationClipWrapper m_RightHandedAnimationWrapper;

	private DollRoomAvatarManager<Gender> m_AvatarManager;

	private DollState m_DollState;

	private bool m_IsDirty;

	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: true);
	}

	protected override void Cleanup()
	{
		m_TargetPlaceholder.rotation = Quaternion.identity;
		m_AvatarManager?.Cleanup();
		m_AvatarManager = null;
		m_DollState = null;
		base.Cleanup();
	}

	private void LateUpdate()
	{
		if (m_IsDirty)
		{
			UpdateDoll(m_DollState);
			m_IsDirty = false;
		}
		Services.GetInstance<CharacterAtlasService>().Update();
	}

	[UsedImplicitly]
	private void Update()
	{
		Character character = m_AvatarManager?.Active;
		if (!(character == null) && !(character.AnimationManager == null))
		{
			character.AnimationManager.CustomUpdate(RealTimeController.SystemStepDurationSeconds);
		}
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		m_DollState = dollState;
		m_IsDirty = true;
	}

	public void BindDollState(DollState dollState)
	{
		if (m_DollState != dollState)
		{
			Cleanup();
			m_DollState = dollState;
			m_IsDirty = true;
		}
	}

	private void OnCharacterAvatarUpdated(Character character)
	{
		Renderer[] componentsInChildren = character.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 15;
		}
	}

	private void UpdateDoll(DollState dollState)
	{
		if (m_AvatarManager == null)
		{
			m_AvatarManager = new DollRoomAvatarManager<Gender>(m_TargetPlaceholder, OnCharacterAvatarUpdated);
		}
		BlueprintCharGenRoot instance = BlueprintCharGenRoot.Instance;
		Gender gender = ((dollState.Gender != 0) ? Gender.Female : Gender.Male);
		Character doll = instance.GetDollConfig(gender).Doll;
		string text = ((gender == Gender.Male) ? "CharGen Male" : "CharGen Female");
		m_AvatarManager.GetOrCreate(gender, doll, text, delegate(Character a)
		{
			m_AvatarManager.SetupAnimationManager(a.AnimationManager);
			a.AnimationManager.IsInCombat = false;
		});
		m_AvatarManager.SetActive(gender);
		Character active = m_AvatarManager.Active;
		if ((bool)m_Camera)
		{
			Transform targetTransform = ObjectExtensions.FindChildRecursive(name: active.Skeleton?.DollRoomZoomPreset.TargetBoneName ?? "Head", transform: active.transform);
			m_Camera.LookAt(targetTransform, active.Skeleton?.DollRoomZoomPreset);
		}
		Skeleton skeleton = dollState.GetSkeleton();
		if (skeleton != null && active.Skeleton != skeleton)
		{
			active.Skeleton = skeleton;
		}
		active.RemoveAllEquipmentEntities();
		List<EquipmentEntityLink> list = dollState.CollectEntities();
		EquipmentEntityLink[] clothes = instance.GetDollConfig(gender).Clothes;
		list.AddRange(clothes);
		active.AddEquipmentEntities(list);
		dollState.ApplyRamps(active);
		active.DisplayOptions.ShowHelmet = dollState.ShowHelm;
		active.DisplayOptions.ShowArmor = dollState.ShowArmor;
		active.DisplayOptions.ShowBackpack = dollState.ShowBackpack;
		active.DisplayOptions.ShowCloak = dollState.ShowCloak;
		active.DisplayOptions.ShowGloves = dollState.ShowGloves;
		active.DisplayOptions.ShowBoots = dollState.ShowBoots;
		active.DisplayOptions.ShowCloth = dollState.ShowCloth;
		AnimationClipWrapper rightHandedAnimationWrapper = m_RightHandedAnimationWrapper;
		if (rightHandedAnimationWrapper != null)
		{
			UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(rightHandedAnimationWrapper, "UpdateDoll");
			unitAnimationActionClip.TransitionIn = 0f;
			unitAnimationActionClip.TransitionOut = 0f;
			active.AnimationManager.TryExecute(unitAnimationActionClip);
		}
	}
}
