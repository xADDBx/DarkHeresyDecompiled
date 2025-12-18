using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.BloodMask;

public class BloodyFaceController : IBloodSettingsHandler, ISubscriber, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitLifeStateChanged, EntitySubscriber>
{
	private const float HP_RATIO_DIFFERENCE_THRESHOLD = 0.01f;

	private readonly AbstractUnitEntity m_EntityData;

	private readonly BloodMaskController m_BloodMaskController;

	private readonly BloodMaskSettings m_BloodMaskSettings;

	private readonly Color m_BloodColorAlive = Color.clear;

	private readonly Color m_BloodColorDead = Color.clear;

	private bool m_BloodIsVisibleCache;

	private float m_HpRatioCache = 1f;

	private bool m_IsDeadCache;

	public bool IsDisposed { get; private set; }

	private bool IsBloodAllowed => (SettingsRoot.Game?.Main?.BloodOnCharacters?.GetValue()).GetValueOrDefault();

	public BloodyFaceController(AbstractUnitEntity entityData, BloodMaskController bloodMaskController)
	{
		if (entityData == null)
		{
			UnityEngine.Debug.LogError("BloodyFaceController: entityData for constructor should not be null!");
		}
		if (bloodMaskController == null)
		{
			UnityEngine.Debug.LogError("BloodyFaceController: bloodMaskController for constructor should not be null!");
		}
		m_EntityData = entityData;
		m_BloodMaskController = bloodMaskController;
		m_BloodMaskSettings = CreateBloodMaskSettings(m_EntityData, isDead: false);
		m_BloodColorAlive = ConfigRoot.Instance.HitSystemRoot.GetBloodTypeColor(entityData.SurfaceType);
		m_BloodColorDead = ConfigRoot.Instance.HitSystemRoot.GetBloodTypeColor(entityData.SurfaceType, isDead: true);
		EventBus.Subscribe(this);
		InvalidateState();
		IsDisposed = false;
	}

	public void InvalidateState()
	{
		if (IsBloodAllowed)
		{
			if (!m_BloodMaskController.SettingsEntries.Contains(m_BloodMaskSettings))
			{
				m_BloodMaskController.SettingsEntries.Add(m_BloodMaskSettings);
			}
		}
		else if (m_BloodMaskController.SettingsEntries.Contains(m_BloodMaskSettings))
		{
			m_BloodMaskController.SettingsEntries.Remove(m_BloodMaskSettings);
		}
	}

	private static BloodMaskSettings CreateBloodMaskSettings(AbstractUnitEntity entityData, bool isDead)
	{
		Texture2D bloodTexture = ConfigRoot.Instance.HitSystemRoot.GetBloodTexture(entityData.SurfaceType);
		Vector2 bloodTextureTilingSize = ConfigRoot.Instance.HitSystemRoot.GetBloodTextureTilingSize(entityData.SurfaceType);
		Color bloodTypeColor = ConfigRoot.Instance.HitSystemRoot.GetBloodTypeColor(entityData.SurfaceType, isDead);
		AnimationCurve bloodFadeoutCurve = ConfigRoot.Instance.HitSystemRoot.GetBloodFadeoutCurve(entityData.SurfaceType);
		return new BloodMaskSettings
		{
			BloodColor = bloodTypeColor,
			BloodTexture = bloodTexture,
			DefaultTileSize = bloodTextureTilingSize,
			UnitSizeMultiplier = 1f / GetSizeModifier(entityData.Blueprint),
			BloodFadeoutControl = bloodFadeoutCurve,
			HPRatio = 1f
		};
	}

	public void UpdateBloodValues(bool force = false)
	{
		if (!IsBloodAllowed || m_EntityData.IsDisposed)
		{
			return;
		}
		PartHealth healthOptional = m_EntityData.GetHealthOptional();
		if (healthOptional != null)
		{
			int hitPointsLeft = healthOptional.HitPointsLeft;
			int maxHitPoints = healthOptional.MaxHitPoints;
			float num = Mathf.Max(hitPointsLeft, 0f) / (float)maxHitPoints;
			bool isDead = m_EntityData.IsDead;
			bool isBloodAllowed = IsBloodAllowed;
			if (force || m_BloodIsVisibleCache != isBloodAllowed || Math.Abs(num - m_HpRatioCache) > 0.01f || m_IsDeadCache != isDead)
			{
				m_BloodIsVisibleCache = isBloodAllowed;
				m_HpRatioCache = num;
				m_IsDeadCache = isDead;
				m_BloodMaskSettings.HPRatio = num;
				m_BloodMaskSettings.BloodColor = (m_IsDeadCache ? m_BloodColorDead : m_BloodColorAlive);
				m_BloodMaskSettings.IsNeedUpdate = true;
			}
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		UpdateBloodValues();
	}

	public void HandleBloodSettingChanged()
	{
		InvalidateState();
		UpdateBloodValues(force: true);
	}

	private static float GetSizeModifier(BlueprintUnit unit)
	{
		return unit.Size switch
		{
			Size.Tiny => 1f, 
			Size.Small => 1f, 
			Size.Medium => 1f, 
			Size.Large => 0.75f, 
			Size.Huge => 0.6f, 
			Size.Gargantuan => 0.5f, 
			Size.Colossal => 0.5f, 
			_ => 1f, 
		};
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
		if (m_BloodMaskController.SettingsEntries.Contains(m_BloodMaskSettings))
		{
			m_BloodMaskController.SettingsEntries.Remove(m_BloodMaskSettings);
		}
		IsDisposed = true;
	}
}
