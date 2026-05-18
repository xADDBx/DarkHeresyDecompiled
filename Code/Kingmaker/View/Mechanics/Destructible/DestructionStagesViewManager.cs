using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Code.View.Mechanics.Entities.Covers;
using Kingmaker.Enums;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Scene.Mechanics.Entities;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

public class DestructionStagesViewManager : MonoBehaviour, IDestructionStagesManager
{
	[Serializable]
	private class StageSettings
	{
		public DestructionStage Type;

		public GameObject StaticPrefab;

		public GameObject FXOnEnter;

		[AkEventReference]
		public string SFXOnEnter;
	}

	public enum GridSize
	{
		[InspectorName("null")]
		SizeNull,
		[InspectorName("0x1")]
		Size0x1,
		[InspectorName("0x2")]
		Size0x2,
		[InspectorName("0x3")]
		Size0x3,
		[InspectorName("1x1")]
		Size1x1,
		[InspectorName("1x2")]
		Size1x2,
		[InspectorName("1x3")]
		Size1x3,
		[InspectorName("2x2")]
		Size2x2,
		[InspectorName("2x3")]
		Size2x3,
		[InspectorName("3x3")]
		Size3x3,
		[InspectorName("0x4")]
		Size0x4,
		[InspectorName("1x4")]
		Size1x4,
		[InspectorName("2x4")]
		Size2x4
	}

	private StageSettings m_CurrentStage;

	public float SwitchPrefabsDelaySeconds;

	[SerializeField]
	private StageSettings[] m_Stages = new StageSettings[0];

	[SerializeField]
	[HideInInspector]
	public GridSize gridSize;

	[CanBeNull]
	private MapObjectView m_MapObject;

	[CanBeNull]
	private CancellationTokenSource m_SwitchStagesCancellation;

	public IEnumerable<DestructionStage> Stages => m_Stages.Select((StageSettings i) => i.Type);

	public bool HasStages
	{
		get
		{
			if (m_Stages != null)
			{
				return m_Stages.Length != 0;
			}
			return false;
		}
	}

	public bool HasFxOnEnter
	{
		get
		{
			if (m_Stages != null)
			{
				return m_Stages.Any((StageSettings s) => s != null && s.FXOnEnter != null);
			}
			return false;
		}
	}

	public bool SyncFromCover(bool overwrite)
	{
		if (Application.isPlaying)
		{
			return false;
		}
		if (!overwrite && HasStages)
		{
			return false;
		}
		StageSettings[] stages = m_Stages;
		BaseCoverEntityView componentInParent = GetComponentInParent<BaseCoverEntityView>();
		if (componentInParent == null)
		{
			return false;
		}
		AbstractDestructibleEntityView.DestructionStageViewSetup[] setup = componentInParent.GetDestructionStageViewSetup();
		if (setup.Length == 0)
		{
			return false;
		}
		m_Stages = new StageSettings[setup.Length];
		int i;
		for (i = 0; i < setup.Length; i++)
		{
			StageSettings stageSettings = null;
			if (stages != null)
			{
				stageSettings = stages.FirstOrDefault((StageSettings s) => s != null && s.Type == setup[i].Type);
				if (stageSettings == null && i < stages.Length)
				{
					stageSettings = stages[i];
				}
			}
			m_Stages[i] = new StageSettings
			{
				Type = setup[i].Type,
				StaticPrefab = setup[i].NavmeshModifier,
				FXOnEnter = stageSettings?.FXOnEnter,
				SFXOnEnter = stageSettings?.SFXOnEnter
			};
		}
		return true;
	}

	private void OnValidate()
	{
		SyncFromCover(overwrite: false);
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			m_MapObject = this.GetComponentNonAlloc<MapObjectView>() ?? base.transform.parent.GetComponentNonAlloc<MapObjectView>();
		}
	}

	private void Start()
	{
		StageSettings[] stages = m_Stages;
		foreach (StageSettings stageSettings in stages)
		{
			if (!(stageSettings.StaticPrefab == null))
			{
				stageSettings.StaticPrefab.SetActive(m_CurrentStage == stageSettings);
			}
		}
	}

	private void OnDisable()
	{
		m_SwitchStagesCancellation?.Cancel();
		m_SwitchStagesCancellation = null;
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		StageSettings currentStage = m_CurrentStage;
		if (currentStage == null || currentStage.Type != stage)
		{
			StageSettings stageSettings = ChooseStageSettings(stage);
			if (stageSettings != null)
			{
				m_SwitchStagesCancellation?.Cancel();
				m_SwitchStagesCancellation = new CancellationTokenSource();
				SwitchStages(m_MapObject, m_CurrentStage, stageSettings, onLoad, SwitchPrefabsDelaySeconds.Seconds(), m_SwitchStagesCancellation.Token);
				m_CurrentStage = stageSettings;
			}
		}
	}

	private static async void SwitchStages(MapObjectView mapObject, [CanBeNull] StageSettings prevStage, StageSettings newStage, bool onLoad, TimeSpan switchPrefabDelay, CancellationToken ct)
	{
		if (!onLoad)
		{
			if (newStage.SFXOnEnter != null && mapObject != null && mapObject.gameObject != null)
			{
				SoundEventsManager.PostEvent(newStage.SFXOnEnter, mapObject.gameObject);
			}
			if (newStage.FXOnEnter != null)
			{
				bool flag = newStage.FXOnEnter.GetComponentsInChildren<SoundFx>().Length != 0;
				if (mapObject != null && mapObject.gameObject != null)
				{
					GameObject gameObject = FxHelper.SpawnFxOnGameObject(newStage.FXOnEnter, mapObject.gameObject);
					if (flag && gameObject != null)
					{
						SoundFx[] componentsInChildren = gameObject.GetComponentsInChildren<SoundFx>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].BlockSoundFXPlaying = true;
						}
					}
				}
			}
			await Task.Delay((int)switchPrefabDelay.TotalMilliseconds, ct);
		}
		if (prevStage != null && prevStage.StaticPrefab != null)
		{
			prevStage.StaticPrefab.SetActive(value: false);
		}
		if (newStage == null || newStage.StaticPrefab == null)
		{
			UberDebug.LogError($"Destructible view error! NewStage = {newStage}, static = {newStage?.StaticPrefab}");
		}
		if (newStage != null && newStage.StaticPrefab != null)
		{
			newStage.StaticPrefab.SetActive(value: true);
		}
		mapObject.Or(null)?.ReinitHighlighterMaterials();
	}

	private StageSettings ChooseStageSettings(DestructionStage desiredStage)
	{
		if (m_Stages == null)
		{
			return null;
		}
		if (m_Stages.Length == 0)
		{
			return null;
		}
		StageSettings stageSettings = null;
		StageSettings[] stages = m_Stages;
		foreach (StageSettings stageSettings2 in stages)
		{
			if (stageSettings2.Type == desiredStage)
			{
				return stageSettings2;
			}
			if (stageSettings == null || ((stageSettings2.Type < desiredStage) ? (stageSettings2.Type > stageSettings.Type) : (stageSettings2.Type < stageSettings.Type)))
			{
				stageSettings = stageSettings2;
			}
		}
		return stageSettings;
	}

	string IDestructionStagesManager.get_name()
	{
		return base.name;
	}
}
