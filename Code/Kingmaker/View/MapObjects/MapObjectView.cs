using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.Gameplay.Parts.ViewBased;
using Kingmaker.Code.View.Scene;
using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using R3;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[AddComponentMenu("Map Object View")]
[KnowledgeDatabaseID("037fe06a751be534fa04d8b0764331d1")]
public class MapObjectView : MechanicEntityView, IDetectHover, IEntitySubscriber, IAwarenessHandler<EntitySubscriber>, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IEventTag<IAwarenessHandler, EntitySubscriber>, IAreaHandler, IResource
{
	[Flags]
	protected enum HighlightingFlags
	{
		None = 0,
		Global = 1,
		MouseHover = 2,
		NoticeOnReveal = 4,
		ForcedExternal = 8,
		Silent = 0x10
	}

	public bool NeedsVoiceOver;

	[ShowIf("NeedsVoiceOver")]
	public VoIdField VoId = new VoIdField();

	[SerializeField]
	private List<Renderer> m_HideRenderers;

	private Highlighter m_Highlighter;

	private FactHolder m_FactHolder;

	private bool m_Highlighted;

	private IDisposable m_HighlightingTriggersDisposable;

	protected ReactiveProperty<HighlightingFlags> m_HighlightingTriggers;

	public override bool CreatesDataOnLoad => true;

	protected static UIConfig UIConfig => ConfigRoot.Instance.UIConfig;

	public FactHolder FactHolder => m_FactHolder ?? (m_FactHolder = GetComponent<FactHolder>());

	public new MapObjectEntity Data => (MapObjectEntity)base.Data;

	public virtual bool CanBeAttackedDirectly => false;

	public override List<Renderer> Renderers => m_HideRenderers ?? (m_HideRenderers = new List<Renderer>());

	public override bool IsSelectableInFogOfWar => true;

	public virtual float FogOfWarFudgeRadius { get; set; }

	public bool Highlighted => m_Highlighted;

	protected Highlighter Highlighter => m_Highlighter;

	protected virtual bool HasHighlight
	{
		get
		{
			MapObjectEntity data = Data;
			if (data == null)
			{
				AbstractEntityPartComponent[] components = GetComponents<AbstractEntityPartComponent>();
				if (components == null)
				{
					PartDetectiveObject partDetectiveObject = Data?.GetOptional<PartDetectiveObject>();
					if (partDetectiveObject == null)
					{
						return false;
					}
					return partDetectiveObject;
				}
				return components.OfType<IInteractionComponent>().Any();
			}
			return data.Parts.GetAll<AbstractInteractionPart>().Any();
		}
	}

	private bool HasNoticeableInteractions => Data.Parts.GetAll<AbstractInteractionPart>().Any((AbstractInteractionPart ic) => ic.Enabled && (ic.Type == InteractionType.Approach || ic.Type == InteractionType.Direct));

	private bool HasVisibleTrap
	{
		get
		{
			if (HasHighlight)
			{
				MapObjectEntity data = Data;
				if (data == null)
				{
					return false;
				}
				return data.Parts.GetAll<AbstractInteractionPart>().Any((AbstractInteractionPart i) => i.HasVisibleTrap());
			}
			return false;
		}
	}

	private bool HasLootInteractions
	{
		get
		{
			if (!HasHighlight)
			{
				return false;
			}
			return Data?.GetOptional<InteractionLootPart>();
		}
	}

	private bool IsLootViewed
	{
		get
		{
			if (HasLootInteractions)
			{
				return (Data?.GetOptional<InteractionLootPart>())?.LootViewed ?? false;
			}
			return false;
		}
	}

	private bool HasDetectiveInteractions
	{
		get
		{
			if (!HasHighlight)
			{
				return false;
			}
			return Data?.GetOptional<PartDetectiveObject>();
		}
	}

	protected PartAdditionalCombatObjectiveMapObject AdditionalCombatObjective => Data?.GetOptional<PartAdditionalCombatObjectiveMapObject>();

	private bool HasAreaTransition
	{
		get
		{
			AreaTransitionPart areaTransitionPart = Data?.GetOptional<AreaTransitionPart>();
			if (areaTransitionPart == null)
			{
				return false;
			}
			return areaTransitionPart;
		}
	}

	protected bool IsRevealed => Data?.IsRevealed ?? false;

	protected bool IsDetectiveObjectRevealed => (Data?.DetectiveObject?.IsRevealed).GetValueOrDefault();

	protected bool HasAwarnessCheck => Data?.AwarenessCheck != null;

	protected bool IsAwarenessCheckPassed => Data?.IsAwarenessCheckPassed ?? false;

	protected bool IsInFogOfWar => Data?.IsInFogOfWar ?? false;

	private bool IsInCombat => Game.Instance.Controllers.TurnController.TurnBasedModeActive;

	private bool IsLootInCombat
	{
		get
		{
			if (HasLootInteractions)
			{
				return IsInCombat;
			}
			return false;
		}
	}

	private bool HasHighlightingTriggers
	{
		get
		{
			if (m_HighlightingTriggers != null)
			{
				ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
				if (highlightingTriggers == null)
				{
					return true;
				}
				return highlightingTriggers.Value != HighlightingFlags.None;
			}
			return false;
		}
	}

	protected bool NoticeHighlightOnReveal
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers == null)
			{
				return false;
			}
			return highlightingTriggers.Value.HasFlag(HighlightingFlags.NoticeOnReveal);
		}
	}

	protected bool ForcedHighlightExternal
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers == null)
			{
				return false;
			}
			return highlightingTriggers.Value.HasFlag(HighlightingFlags.ForcedExternal);
		}
	}

	protected virtual bool GlobalHighlighting
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers == null)
			{
				return false;
			}
			return highlightingTriggers.Value.HasFlag(HighlightingFlags.Global);
		}
	}

	public bool MouseHoverHighlighting
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers == null)
			{
				return false;
			}
			return highlightingTriggers.Value.HasFlag(HighlightingFlags.MouseHover);
		}
	}

	protected bool SilentHighlighting
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers == null)
			{
				return false;
			}
			return highlightingTriggers.Value.HasFlag(HighlightingFlags.Silent);
		}
	}

	public bool OnlySilentHighlighting
	{
		get
		{
			ReactiveProperty<HighlightingFlags> highlightingTriggers = m_HighlightingTriggers;
			if (highlightingTriggers != null)
			{
				return highlightingTriggers.Value == HighlightingFlags.Silent;
			}
			return false;
		}
	}

	protected bool InteractionConditions
	{
		get
		{
			if (Data.IsRevealed)
			{
				if (!CanBeAttackedDirectly && !Data.Parts.GetAll<AbstractInteractionPart>().Any(ShouldHighlightInteraction))
				{
					return GetComponents<AbstractEntityPartComponent>().OfType<DetectiveObjectComponent>().Any();
				}
				return true;
			}
			return false;
			static bool ShouldHighlightInteraction(AbstractInteractionPart i)
			{
				InteractionType type = i.Type;
				if ((type == InteractionType.Approach || type == InteractionType.Direct) && i.Enabled)
				{
					if (i.ShowOvertip)
					{
						return i.ShowHighlight;
					}
					return true;
				}
				return false;
			}
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return Data;
	}

	public bool IsOwnerOf(EntityFactComponent component)
	{
		if ((bool)FactHolder && component.Fact != null)
		{
			return component.Fact == FactHolder.GetFact();
		}
		return false;
	}

	public virtual bool SupportBlueprint(BlueprintMapObject blueprint)
	{
		return blueprint != null;
	}

	public virtual void ApplyBlueprint(BlueprintMapObject blueprint)
	{
		if (!SupportBlueprint(blueprint))
		{
			throw new Exception("Blueprint is not supported");
		}
		base.gameObject.EnsureComponent<FactHolder>().Blueprint = blueprint;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!Application.isPlaying)
		{
			return;
		}
		List<Transform> list = ListPool<Transform>.Claim();
		GetComponentsInChildren(list);
		foreach (Transform item in list)
		{
			if (item.gameObject.layer == 9)
			{
				item.gameObject.layer = 10;
			}
		}
		ListPool<Transform>.Release(list);
		SetupHighlight();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_HighlightingTriggersDisposable?.Dispose();
		m_HighlightingTriggersDisposable = null;
	}

	private void SetupHighlight()
	{
		m_HighlightingTriggersDisposable?.Dispose();
		m_HighlightingTriggersDisposable = null;
		if (HasHighlight)
		{
			m_Highlighter = this.EnsureComponent<Highlighter>();
			if (m_HighlightingTriggers == null)
			{
				m_HighlightingTriggers = new ReactiveProperty<HighlightingFlags>().AddTo(this);
			}
			m_HighlightingTriggersDisposable = m_HighlightingTriggers.Skip(1).Subscribe(delegate
			{
				UpdateHighlight();
			}).AddTo(this);
		}
		UpdateHighlight();
	}

	public override Entity CreateEntityData(bool load)
	{
		return CreateMapObjectEntityData(load);
	}

	protected virtual MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new MapObjectEntity(this));
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		UpdateHighlight();
	}

	public override void OnInFogOfWarChanged()
	{
		base.OnInFogOfWarChanged();
		UpdateHighlight();
	}

	public void ReinitHighlighterMaterials()
	{
		m_Highlighter.Or(null)?.ReinitMaterials();
	}

	public virtual void OnAreaBeginUnloading()
	{
	}

	public virtual void OnAreaDidLoad()
	{
		UpdateHighlight();
	}

	public void AddHideRenderer(Renderer newRenderer)
	{
		if ((bool)newRenderer)
		{
			m_HideRenderers = m_HideRenderers ?? new List<Renderer>();
			m_HideRenderers.Add(newRenderer);
			SetVisible(base.IsVisible, force: true);
		}
	}

	protected void AddHideRenderers(IEnumerable<Renderer> renderers)
	{
		if (!renderers.Empty())
		{
			m_HideRenderers = m_HideRenderers ?? new List<Renderer>();
			m_HideRenderers.AddRange(renderers);
			SetVisible(base.IsVisible, force: true);
		}
	}

	public virtual void HandleHoverChange(bool isHover)
	{
		AdditionalCombatObjective?.SetHovered(isHover);
		if (HasAreaTransition)
		{
			EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectHighlightChange();
			}, isCheckRuntime: true);
		}
		else if (HasHighlight && m_HighlightingTriggers != null)
		{
			if (NoticeHighlightOnReveal && !Data.WasHighlightedOnRevealAndNoticed)
			{
				Data.WasHighlightedOnRevealAndNoticed = true;
				m_HighlightingTriggers.Value &= ~HighlightingFlags.NoticeOnReveal;
			}
			if (isHover)
			{
				m_HighlightingTriggers.Value |= HighlightingFlags.MouseHover;
			}
			else
			{
				m_HighlightingTriggers.Value &= ~HighlightingFlags.MouseHover;
			}
			Game.Instance.CursorController.SetMapObjectCursor(this, isHover && CheckHighlightConditions());
		}
	}

	public virtual void OnEntityNoticed(BaseUnitEntity character)
	{
		if ((bool)m_Highlighter)
		{
			if (HasVisibleTrap || (HasAwarnessCheck && IsAwarenessCheckPassed) || (HasNoticeableInteractions && !HasAwarnessCheck))
			{
				m_HighlightingTriggers.Value |= HighlightingFlags.NoticeOnReveal;
			}
			UpdateHighlight();
		}
	}

	public void ForceHighlightExternal(bool value)
	{
		if (m_HighlightingTriggers == null)
		{
			if (!HasHighlight)
			{
				return;
			}
			SetupHighlight();
		}
		if (value)
		{
			m_HighlightingTriggers.Value |= HighlightingFlags.ForcedExternal;
		}
		else
		{
			m_HighlightingTriggers.Value &= ~HighlightingFlags.ForcedExternal;
		}
		UpdateHighlight();
	}

	public void UpdateHighlight()
	{
		if ((bool)m_Highlighter)
		{
			SetHighlight(CheckHighlightConditions());
		}
	}

	protected void SetHighlight(bool value)
	{
		if ((bool)m_Highlighter)
		{
			if (value)
			{
				Color highlightColor = GetHighlightColor();
				m_Highlighter.ConstantOn(highlightColor, 0f);
			}
			else
			{
				m_Highlighter.ConstantOff(0f);
			}
			bool highlighted = m_Highlighted;
			m_Highlighted = value;
			if (highlighted != value)
			{
				OnHighlightUpdated();
			}
		}
	}

	public void SetGlobalHighlight(bool value)
	{
		if (m_HighlightingTriggers != null)
		{
			if (value)
			{
				m_HighlightingTriggers.Value |= HighlightingFlags.Global;
			}
			else
			{
				m_HighlightingTriggers.Value &= ~HighlightingFlags.Global;
			}
		}
	}

	public void SetSilentHighlight(bool value)
	{
		if (m_HighlightingTriggers != null)
		{
			if (value)
			{
				m_HighlightingTriggers.Value |= HighlightingFlags.Silent;
			}
			else
			{
				m_HighlightingTriggers.Value &= ~HighlightingFlags.Silent;
			}
		}
	}

	protected virtual bool CheckHighlightConditions()
	{
		if (!HasHighlightingTriggers)
		{
			return false;
		}
		if (AdditionalCombatObjective != null)
		{
			return AdditionalCombatObjective.ShouldHighlight();
		}
		if (IsInCombat && IsLootInCombat)
		{
			return false;
		}
		if (m_HighlightingTriggers.Value == HighlightingFlags.Global && IsInFogOfWar)
		{
			return false;
		}
		if (HasVisibleTrap)
		{
			return true;
		}
		if (IsDetectiveObjectRevealed)
		{
			return IsRevealed;
		}
		if (Data.VisibilitySuppressedByFlashlight())
		{
			return false;
		}
		if (IsRevealed && InteractionConditions)
		{
			return IsAwarenessCheckPassed;
		}
		return false;
	}

	protected virtual Color GetHighlightColor()
	{
		ViewHighlightingColors viewHighlightingColors = UIConfig.ViewHighlightingColors;
		if (!HasHighlight)
		{
			return Color.clear;
		}
		if (!GlobalHighlighting && !MouseHoverHighlighting && !NoticeHighlightOnReveal && !ForcedHighlightExternal && !SilentHighlighting)
		{
			return Color.clear;
		}
		if (NoticeHighlightOnReveal || ForcedHighlightExternal)
		{
			if (HasVisibleTrap)
			{
				return viewHighlightingColors.TrapedLoot.HighlightColor;
			}
			if (HasAwarnessCheck && IsAwarenessCheckPassed)
			{
				return viewHighlightingColors.PerceptedLoot.HighlightColor;
			}
			if (HasDetectiveInteractions)
			{
				if (!ForcedHighlightExternal || MouseHoverHighlighting)
				{
					return viewHighlightingColors.Detective.HoverColor;
				}
				return viewHighlightingColors.Detective.HighlightColor;
			}
			if (HasLootInteractions)
			{
				return viewHighlightingColors.DefualtLoot.HighlightColor;
			}
		}
		if (AdditionalCombatObjective != null)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.AdditionalCombatObjective.HighlightColor;
			}
			return viewHighlightingColors.AdditionalCombatObjective.HoverColor;
		}
		if (HasVisibleTrap)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.TrapedLoot.HighlightColor;
			}
			return viewHighlightingColors.TrapedLoot.HoverColor;
		}
		if (HasDetectiveInteractions)
		{
			PartDetectiveObject partDetectiveObject = Data?.GetOptional<PartDetectiveObject>();
			if (partDetectiveObject != null && !partDetectiveObject.IsRevealed)
			{
				return Color.clear;
			}
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.Detective.HighlightColor;
			}
			return viewHighlightingColors.Detective.HoverColor;
		}
		if (HasAwarnessCheck)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.PerceptedLoot.HighlightColor;
			}
			return viewHighlightingColors.PerceptedLoot.HoverColor;
		}
		if (HasLootInteractions && IsLootViewed)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.DefualtLoot.HighlightColor;
			}
			return viewHighlightingColors.DefualtLoot.HoverColor;
		}
		if (HasLootInteractions)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.DefualtLoot.HighlightColor;
			}
			return viewHighlightingColors.DefualtLoot.HoverColor;
		}
		if (!MouseHoverHighlighting)
		{
			return viewHighlightingColors.Default.HighlightColor;
		}
		return viewHighlightingColors.Default.HoverColor;
	}

	protected virtual void OnHighlightUpdated()
	{
		if (Data != null)
		{
			EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectHighlightChange();
			}, isCheckRuntime: true);
		}
	}
}
