using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Code._Legacy.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Animation;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.Scene.Mechanics.Entities;

[KnowledgeDatabaseID("0c80e47b166948f7ad3a7aea3af3b59a")]
public abstract class AbstractDestructibleEntityView : MapObjectView, IAbstractDestructibleEntityView, IMapObjectView, IMechanicEntityView, IEntityView, IDestructibleEntityConfig, IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig, IDestructionStagesManager, IDamageFXHandler, ISubscriber, IGameModeHandler
{
	internal readonly struct DestructionStageViewSetup
	{
		public readonly DestructionStage Type;

		public readonly GameObject NavmeshModifier;

		public DestructionStageViewSetup(DestructionStage type, GameObject navmeshModifier)
		{
			Type = type;
			NavmeshModifier = navmeshModifier;
		}
	}

	[Serializable]
	private class DestructionStageSettings
	{
		public DestructionStage Type;

		public GameObject NavmeshModifier;

		[NonSerialized]
		public GridObstacle[] GridObstacles = new GridObstacle[0];
	}

	[SerializeField]
	private bool m_UseCustomBlueprint;

	[SerializeField]
	[HideIf("m_UseCustomBlueprint")]
	private StandardDestructibleObjectType m_StandardBlueprintType;

	[SerializeField]
	[ShowIf("m_UseCustomBlueprint")]
	private BlueprintDestructibleObject.Reference m_CustomBlueprint;

	[SerializeField]
	private DestructionStageSettings[] m_DestructionStages = new DestructionStageSettings[0];

	[CanBeNull]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	[ShowIf("VisibleInExploration")]
	public LocalizedString ExplorationBark;

	private new Collider[] m_Colliders;

	protected DestructionStage m_CurrentDestructionStage;

	[CanBeNull]
	private DestructionStageSettings m_CurrentStageSettings;

	private DestructibleAnimationManager m_AnimationManager;

	private Vector3? m_OvertipPosition;

	private Rect m_CachedBounds;

	private Vector3 m_PrevPosition;

	private bool m_ForceRecalculateBounds = true;

	[CanBeNull]
	private GridObstacle[] m_AllStagesGridObstaclesCache;

	[field: SerializeField]
	public bool DisableAutoHit { get; private set; }

	[field: ShowIf("DisableAutoHit")]
	[field: SerializeField]
	public int HitChanceModifier { get; private set; }

	[field: SerializeField]
	public bool VisibleInExploration { get; private set; }

	[field: SerializeField]
	[field: ShowIf("VisibleInExploration")]
	public HighlightType HighlightType { get; private set; } = HighlightType.Always;


	public bool UseCustomBlueprint => m_UseCustomBlueprint;

	public bool HasCustomBlueprint
	{
		get
		{
			if (m_CustomBlueprint != null)
			{
				return !m_CustomBlueprint.IsEmpty();
			}
			return false;
		}
	}

	public BlueprintDestructibleObject Blueprint
	{
		get
		{
			if (!m_UseCustomBlueprint)
			{
				return ConfigRoot.Instance.DestructibleObjectsRoot.GetStandardObject(m_StandardBlueprintType);
			}
			return m_CustomBlueprint?.Get() ?? ConfigRoot.Instance.DestructibleObjectsRoot.GetStandardObject(m_StandardBlueprintType);
		}
	}

	public override BlueprintMechanicEntityFact MechanicFactBlueprint => Blueprint ?? base.MechanicFactBlueprint;

	public Rect Bounds => CalculateBounds();

	public Bounds RenderersBounds { get; private set; }

	public new DestructibleEntity Data => (DestructibleEntity)base.Data;

	public IEnumerable<DestructionStage> Stages => m_DestructionStages.Select((DestructionStageSettings i) => i.Type);

	public GridObstacle[] CurrentStageGridObstacles => m_CurrentStageSettings?.GridObstacles ?? Array.Empty<GridObstacle>();

	public GridObstacle[] WholeStageGridObstacles => m_DestructionStages.FirstItem((DestructionStageSettings i) => i.Type == DestructionStage.Whole)?.GridObstacles ?? Array.Empty<GridObstacle>();

	public GridObstacle[] AllGridObstacles => m_AllStagesGridObstaclesCache ?? (m_AllStagesGridObstaclesCache = CollectAllGridObstacles());

	public Vector3 OvertipPosition
	{
		get
		{
			if (m_OvertipPosition.HasValue)
			{
				return m_OvertipPosition.Value;
			}
			Bounds? bounds = null;
			if (m_Colliders != null)
			{
				Collider[] colliders = m_Colliders;
				foreach (Collider collider in colliders)
				{
					if (collider.enabled && collider.gameObject.activeInHierarchy)
					{
						if (!bounds.HasValue)
						{
							bounds = collider.bounds;
						}
						else
						{
							bounds.Value.Encapsulate(collider.bounds);
						}
					}
				}
			}
			Bounds valueOrDefault = bounds.GetValueOrDefault();
			if (!bounds.HasValue)
			{
				valueOrDefault = new Bounds(base.transform.position, Vector3.one);
				bounds = valueOrDefault;
			}
			m_OvertipPosition = bounds.Value.center + new Vector3(0f, bounds.Value.extents.y, 0f);
			return m_OvertipPosition.Value;
		}
	}

	protected override bool HasHighlight
	{
		get
		{
			if (!CanBeAttackedDirectly && !base.HasHighlight && !VisibleInExploration)
			{
				return Data?.GetOptional<PartAdditionalCombatObjectiveMapObject>() != null;
			}
			return true;
		}
	}

	internal DestructionStageViewSetup[] GetDestructionStageViewSetup()
	{
		if (m_DestructionStages == null || m_DestructionStages.Length == 0)
		{
			return Array.Empty<DestructionStageViewSetup>();
		}
		List<DestructionStageViewSetup> list = new List<DestructionStageViewSetup>(m_DestructionStages.Length);
		for (int i = 0; i < m_DestructionStages.Length; i++)
		{
			DestructionStageSettings destructionStageSettings = m_DestructionStages[i];
			if (destructionStageSettings != null)
			{
				list.Add(new DestructionStageViewSetup(destructionStageSettings.Type, destructionStageSettings.NavmeshModifier));
			}
		}
		return list.ToArray();
	}

	private GridObstacle[] CollectAllGridObstacles()
	{
		if (m_DestructionStages == null || m_DestructionStages.Length == 0)
		{
			return Array.Empty<GridObstacle>();
		}
		int num = 0;
		DestructionStageSettings[] destructionStages = m_DestructionStages;
		foreach (DestructionStageSettings destructionStageSettings in destructionStages)
		{
			if (destructionStageSettings?.GridObstacles != null)
			{
				num += destructionStageSettings.GridObstacles.Length;
			}
		}
		if (num == 0)
		{
			return Array.Empty<GridObstacle>();
		}
		GridObstacle[] array = new GridObstacle[num];
		int num2 = 0;
		destructionStages = m_DestructionStages;
		foreach (DestructionStageSettings destructionStageSettings2 in destructionStages)
		{
			if (destructionStageSettings2?.GridObstacles != null)
			{
				Array.Copy(destructionStageSettings2.GridObstacles, 0, array, num2, destructionStageSettings2.GridObstacles.Length);
				num2 += destructionStageSettings2.GridObstacles.Length;
			}
		}
		return array;
	}

	protected override bool CheckHighlightConditions()
	{
		int num;
		if (!base.MouseHoverHighlighting)
		{
			PartAdditionalCombatObjectiveMapObject additionalCombatObjective = base.AdditionalCombatObjective;
			num = ((additionalCombatObjective != null && additionalCombatObjective.ShowType == AdditionalCombatType.OnceInCombat && additionalCombatObjective.WasHovered) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		PartAdditionalCombatObjectiveMapObject additionalCombatObjective2 = base.AdditionalCombatObjective;
		bool flag2 = additionalCombatObjective2 != null && additionalCombatObjective2.ShouldHighlight() && !flag;
		if (m_CurrentDestructionStage != DestructionStage.Destroyed)
		{
			return (base.CheckHighlightConditions() && CanBeAttackedDirectly) || flag2;
		}
		return false;
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DestructibleEntity(this));
	}

	protected override void Awake()
	{
		base.Awake();
		DestructionStageSettings[] destructionStages = m_DestructionStages;
		foreach (DestructionStageSettings destructionStageSettings in destructionStages)
		{
			if (destructionStageSettings != null)
			{
				GameObject navmeshModifier = destructionStageSettings.NavmeshModifier;
				destructionStageSettings.GridObstacles = ((navmeshModifier != null) ? (navmeshModifier.GetComponentsInChildren<GridObstacle>() ?? Array.Empty<GridObstacle>()) : Array.Empty<GridObstacle>());
			}
		}
		if (!Application.isPlaying)
		{
			return;
		}
		m_Colliders = GetComponentsInChildren<Collider>(includeInactive: true);
		destructionStages = m_DestructionStages;
		foreach (DestructionStageSettings destructionStageSettings2 in destructionStages)
		{
			if (destructionStageSettings2.NavmeshModifier != null)
			{
				destructionStageSettings2.NavmeshModifier.gameObject.SetActive(value: false);
			}
		}
		m_AnimationManager = GetComponent<DestructibleAnimationManager>();
	}

	protected override Color GetHighlightColor()
	{
		ViewHighlightingColors viewHighlightingColors = MapObjectView.UIConfig.ViewHighlightingColors;
		if (base.AdditionalCombatObjective != null)
		{
			if (!base.MouseHoverHighlighting)
			{
				return viewHighlightingColors.AdditionalCombatObjective.HighlightColor;
			}
			return viewHighlightingColors.AdditionalCombatObjective.HoverColor;
		}
		if (!GlobalHighlighting && !base.MouseHoverHighlighting && !base.NoticeHighlightOnReveal && !base.ForcedHighlightExternal)
		{
			return Color.clear;
		}
		if (!base.MouseHoverHighlighting)
		{
			return viewHighlightingColors.DestructableEntity.HighlightColor;
		}
		return viewHighlightingColors.DestructableEntity.HoverColor;
	}

	private Rect CalculateBounds()
	{
		Vector3 position = base.transform.position;
		if (position == m_PrevPosition && !m_ForceRecalculateBounds)
		{
			return m_CachedBounds;
		}
		m_PrevPosition = position;
		m_ForceRecalculateBounds = false;
		Bounds? bounds = null;
		if (m_Colliders != null)
		{
			for (int j = 0; j < m_Colliders.Length; j++)
			{
				Collider collider = m_Colliders[j];
				if (collider.enabled && collider.gameObject.activeInHierarchy)
				{
					if (!bounds.HasValue)
					{
						bounds = collider.bounds;
						continue;
					}
					Bounds value = bounds.Value;
					value.Encapsulate(collider.bounds);
					bounds = value;
				}
			}
		}
		Bounds valueOrDefault = bounds.GetValueOrDefault();
		if (!bounds.HasValue)
		{
			valueOrDefault = new Bounds(base.transform.position, Vector3.one);
			bounds = valueOrDefault;
		}
		RenderersBounds = bounds.Value;
		bool includeOuterNodes = IsWall();
		GridObstacle[] array = m_DestructionStages.FirstItem((DestructionStageSettings i) => i.Type == DestructionStage.Whole)?.GridObstacles;
		if (array != null && array.Length != 0)
		{
			AstarPath active = AstarPath.active;
			if ((object)active != null)
			{
				AstarData data = active.data;
				if (data != null)
				{
					GridGraph gridGraph = data.gridGraph;
					if (gridGraph != null)
					{
						bounds = array[0].GetMechanicBounds(gridGraph.transform, includeOuterNodes);
						for (int k = 1; k < array.Length; k++)
						{
							Bounds value2 = bounds.Value;
							value2.Encapsulate(array[k].GetMechanicBounds(gridGraph.transform, includeOuterNodes));
							bounds = value2;
						}
					}
				}
			}
		}
		m_CachedBounds = new Rect
		{
			xMin = bounds.Value.min.x,
			yMin = bounds.Value.min.z,
			xMax = bounds.Value.max.x,
			yMax = bounds.Value.max.z
		};
		return m_CachedBounds;
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		m_ForceRecalculateBounds = true;
		m_CurrentDestructionStage = stage;
		UpdateHighlight();
		DestructionStageSettings newStageSettings = m_DestructionStages.FirstItem((DestructionStageSettings i) => i.Type == stage);
		m_CurrentStageSettings?.GridObstacles.ForEach(delegate(GridObstacle x)
		{
			x.gameObject.SetActive(value: false);
		});
		newStageSettings?.GridObstacles.ForEach(delegate(GridObstacle x)
		{
			x.gameObject.SetActive(value: true);
		});
		if (!onLoad && stage == DestructionStage.Destroyed && m_AnimationManager != null)
		{
			m_AnimationManager.PlayOnDestroy(delegate
			{
				HandleVisualStageChange(m_CurrentStageSettings, newStageSettings);
			});
		}
		else
		{
			HandleVisualStageChange(m_CurrentStageSettings, newStageSettings);
		}
		m_CurrentStageSettings = newStageSettings;
		EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IDestructibleEntityHandler>)delegate(IDestructibleEntityHandler h)
		{
			h.HandleDestructionStageChanged(stage);
		}, isCheckRuntime: true);
	}

	private void HandleVisualStageChange(DestructionStageSettings prev, DestructionStageSettings next)
	{
		prev?.NavmeshModifier.gameObject.SetActive(value: false);
		next?.NavmeshModifier.gameObject.SetActive(value: true);
	}

	public override void HandleHoverChange(bool isHover)
	{
		base.HandleHoverChange(isHover);
		EventBus.RaiseEvent((IMechanicEntity)Data, (Action<IDestructibleHoverUIHandler>)delegate(IDestructibleHoverUIHandler h)
		{
			h.HandleHoverChange(isHover);
		}, isCheckRuntime: true);
	}

	protected override void OnDrawGizmosSelected()
	{
		if (m_Colliders.Empty())
		{
			m_Colliders = GetComponentsInChildren<Collider>(includeInactive: true);
		}
		base.OnDrawGizmosSelected();
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		if (Data.IsVisibleForPlayer && VisibleInExploration)
		{
			switch (HighlightType)
			{
			case HighlightType.Always:
				ForceHighlightExternal(Data.IsVisibleForPlayer);
				break;
			case HighlightType.Once:
				if (!Data.WasHighlightedOnRevealAndNoticed)
				{
					m_HighlightingTriggers.Value |= HighlightingFlags.NoticeOnReveal;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case HighlightType.Default:
				break;
			}
		}
		UpdateHighlight();
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if (Data.IsViewActive)
		{
			Data.GetDestructionStagesManagerOptional()?.UpdateOnIsInGameTrue();
			UpdateHighlight();
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (dealDamage.Target.View == this)
		{
			StartCoroutine(HighlightOnHit());
			m_AnimationManager?.PlayOnHit();
		}
	}

	private IEnumerator HighlightOnHit()
	{
		base.Highlighter.ConstantOn(ConfigRoot.Instance.HitSystemRoot.CoverHitOutlineColor, 0.05f);
		yield return new WaitForSeconds(ConfigRoot.Instance.HitSystemRoot.CoverHitHighlightTime);
		base.Highlighter.ConstantOff();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Default)
		{
			UpdateHighlight();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public bool IsWall()
	{
		bool flag = false;
		bool flag2 = false;
		Vector3? vector = null;
		GridObstacle[] wholeStageGridObstacles = WholeStageGridObstacles;
		for (int i = 0; i < wholeStageGridObstacles.Length; i++)
		{
			Vector3 position = wholeStageGridObstacles[i].transform.position;
			if (!vector.HasValue)
			{
				vector = position;
				continue;
			}
			flag |= Math.Abs(vector.Value.x - position.x) > 0.1f;
			flag2 |= Math.Abs(vector.Value.z - position.z) > 0.1f;
		}
		if (flag)
		{
			return !flag2;
		}
		return true;
	}

	T IMapObjectView.GetComponent<T>()
	{
		return GetComponent<T>();
	}

	T IMapObjectView.GetComponentInChildren<T>()
	{
		return GetComponentInChildren<T>();
	}

	T[] IMapObjectView.GetComponents<T>()
	{
		return GetComponents<T>();
	}

	GameObject IMechanicEntityView.get_gameObject()
	{
		return base.gameObject;
	}

	T[] IMechanicEntityView.GetComponentsInChildren<T>()
	{
		return GetComponentsInChildren<T>();
	}

	string IEntityView.get_name()
	{
		return base.name;
	}

	string IDestructionStagesManager.get_name()
	{
		return base.name;
	}
}
