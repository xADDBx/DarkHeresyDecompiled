using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Controllers;

public class InteractionGlobalCooldownController : IController, IControllerTick, IAreaHandler, ISubscriber, IAreaActivationHandler
{
	private struct ClusterEntry
	{
		public string UniqueId;

		public Vector3 Position;

		public int Radius;
	}

	private struct ClusterCandidate
	{
		public AbstractUnitEntity Unit;

		public BaseUnitEntity User;

		public IUnitInteraction Interaction;

		public int InteractionIndex;

		public float Distance;
	}

	private IEntity m_CooldownSource;

	private readonly Dictionary<string, int> m_EntityClusterIndex = new Dictionary<string, int>();

	private readonly List<List<string>> m_Clusters = new List<List<string>>();

	private readonly Dictionary<string, ClusterCandidate> m_AccumulatedCandidates = new Dictionary<string, ClusterCandidate>();

	private float m_ClusterTimerRemaining = -1f;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public bool CheckGlobalCooldown()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState?.Blueprint == null || !loadedAreaState.Blueprint.HasInteractionGlobalCooldown)
		{
			return true;
		}
		return Game.Instance.Controllers.TimeController.GameTime >= loadedAreaState.InteractionGlobalCooldownExpiry;
	}

	public void UpdateGlobalCooldown()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		BlueprintArea blueprintArea = loadedAreaState?.Blueprint;
		if (blueprintArea != null && blueprintArea.HasInteractionGlobalCooldown)
		{
			UpdateGlobalCooldown(loadedAreaState.Blueprint.GetInteractionGlobalCooldown());
		}
	}

	public void UpdateGlobalCooldown(IEntity source)
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		BlueprintArea blueprintArea = loadedAreaState?.Blueprint;
		if (blueprintArea != null && blueprintArea.HasInteractionGlobalCooldown)
		{
			UpdateGlobalCooldown(loadedAreaState.Blueprint.GetInteractionGlobalCooldown(), source);
		}
	}

	public void UpdateGlobalCooldown(float cooldownSeconds, IEntity source = null)
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null)
		{
			m_CooldownSource = source;
			loadedAreaState.InteractionGlobalCooldownExpiry = Game.Instance.Controllers.TimeController.GameTime + TimeSpan.FromSeconds(cooldownSeconds);
			if (source != null)
			{
				loadedAreaState.InteractionActivationCounts.TryGetValue(source.UniqueId, out var value);
				loadedAreaState.InteractionActivationCounts[source.UniqueId] = value + 1;
				PFLog.GlobalCooldown.Log($"GlobalCooldown was set to {cooldownSeconds}s, count: {value} => {value + 1}");
			}
			else
			{
				PFLog.GlobalCooldown.Log($"GlobalCooldown was set to {cooldownSeconds}s");
			}
		}
	}

	public bool IsInCluster(string entityUniqueId)
	{
		return m_EntityClusterIndex.ContainsKey(entityUniqueId);
	}

	public void AddClusterCandidate(AbstractUnitEntity unit, BaseUnitEntity user, IUnitInteraction interaction, int interactionIndex, float distance)
	{
		if (!CheckGlobalCooldown())
		{
			return;
		}
		if (!m_AccumulatedCandidates.ContainsKey(unit.UniqueId))
		{
			m_AccumulatedCandidates[unit.UniqueId] = new ClusterCandidate
			{
				Unit = unit,
				User = user,
				Interaction = interaction,
				InteractionIndex = interactionIndex,
				Distance = distance
			};
		}
		if (!(m_ClusterTimerRemaining >= 0f))
		{
			if (HasMinActivationCount(unit.UniqueId))
			{
				PFLog.GlobalCooldown.Log($"Cluster: immediate fire for {unit} (already has min activations)");
				FireCandidate(m_AccumulatedCandidates[unit.UniqueId]);
			}
			else
			{
				m_ClusterTimerRemaining = (Game.Instance.LoadedAreaState?.Blueprint)?.ClusterInteractionInitialDelay ?? 0f;
				PFLog.GlobalCooldown.Log($"Cluster timer started: {m_ClusterTimerRemaining}s");
			}
		}
	}

	private bool HasMinActivationCount(string entityUniqueId)
	{
		if (!m_EntityClusterIndex.TryGetValue(entityUniqueId, out var value))
		{
			return false;
		}
		int activationCount = GetActivationCount(entityUniqueId);
		foreach (string item in m_Clusters[value])
		{
			if (!(item == entityUniqueId) && GetActivationCount(item) < activationCount)
			{
				return false;
			}
		}
		return true;
	}

	public void Tick()
	{
		TickOutOfRangeReset();
		TickClusterTimer();
	}

	private void TickOutOfRangeReset()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		BlueprintArea blueprintArea = loadedAreaState?.Blueprint;
		if (blueprintArea == null || !blueprintArea.HasInteractionGlobalCooldown || blueprintArea.DistanceFromCooldownSourceToReset <= 0f)
		{
			return;
		}
		if (m_CooldownSource == null || m_CooldownSource.IsDisposed || m_CooldownSource.Destroyed)
		{
			m_CooldownSource = null;
			return;
		}
		TimeSpan timeSpan = loadedAreaState.InteractionGlobalCooldownExpiry - Game.Instance.Controllers.TimeController.GameTime;
		if (timeSpan <= TimeSpan.Zero || timeSpan <= TimeSpan.FromSeconds(blueprintArea.OutOfRangeTimerResetTo))
		{
			return;
		}
		Vector3 position = m_CooldownSource.Position;
		float distanceFromCooldownSourceToReset = blueprintArea.DistanceFromCooldownSourceToReset;
		UnitGroup party = Game.Instance.UnitGroups.Party;
		for (int i = 0; i < party.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = party[i];
			if (baseUnitEntity != null && Vector3.Distance(baseUnitEntity.Position, position) <= distanceFromCooldownSourceToReset)
			{
				return;
			}
		}
		float outOfRangeTimerResetTo = blueprintArea.OutOfRangeTimerResetTo;
		loadedAreaState.InteractionGlobalCooldownExpiry = Game.Instance.Controllers.TimeController.GameTime + TimeSpan.FromSeconds(outOfRangeTimerResetTo);
		PFLog.GlobalCooldown.Log($"GlobalCooldown reset to {outOfRangeTimerResetTo}s: party left range of {m_CooldownSource}");
		m_CooldownSource = null;
	}

	private void TickClusterTimer()
	{
		if (!(m_ClusterTimerRemaining < 0f))
		{
			m_ClusterTimerRemaining -= Game.Instance.Controllers.TimeController.DeltaTime;
			if (!(m_ClusterTimerRemaining > 0f))
			{
				m_ClusterTimerRemaining = -1f;
				FireBestClusterCandidate();
			}
		}
	}

	private void FireBestClusterCandidate()
	{
		if (m_AccumulatedCandidates.Count == 0)
		{
			return;
		}
		bool flag = false;
		ClusterCandidate candidate = default(ClusterCandidate);
		int num = 0;
		foreach (ClusterCandidate value in m_AccumulatedCandidates.Values)
		{
			int activationCount = GetActivationCount(value.Unit.UniqueId);
			if (!flag || activationCount < num || (activationCount == num && value.Distance < candidate.Distance))
			{
				candidate = value;
				num = activationCount;
				flag = true;
			}
		}
		if (flag)
		{
			FireCandidate(candidate);
		}
	}

	private void FireCandidate(ClusterCandidate candidate)
	{
		m_AccumulatedCandidates.Clear();
		PFLog.GlobalCooldown.Log($"Cluster: firing {candidate.Unit} (activations={GetActivationCount(candidate.Unit.UniqueId)}, dist={candidate.Distance:F1})");
		candidate.Interaction.Interact(candidate.User, candidate.Unit);
		PartUnitInteractions optional = candidate.Unit.GetOptional<PartUnitInteractions>();
		if (optional != null)
		{
			optional.Cooldowns[candidate.InteractionIndex] = candidate.Interaction.ApproachCooldown;
		}
	}

	private int GetActivationCount(string entityUniqueId)
	{
		Dictionary<string, int> dictionary = Game.Instance.LoadedAreaState?.InteractionActivationCounts;
		if (dictionary == null || !dictionary.TryGetValue(entityUniqueId, out var value))
		{
			return 0;
		}
		return value;
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAreaActivated()
	{
		BuildClusters();
	}

	public void OnAreaBeginUnloading()
	{
		m_EntityClusterIndex.Clear();
		m_Clusters.Clear();
		m_AccumulatedCandidates.Clear();
		m_ClusterTimerRemaining = -1f;
		m_CooldownSource = null;
	}

	private void BuildClusters()
	{
		m_EntityClusterIndex.Clear();
		m_Clusters.Clear();
		BlueprintArea blueprintArea = Game.Instance.LoadedAreaState?.Blueprint;
		if (blueprintArea == null || !blueprintArea.HasInteractionGlobalCooldown || blueprintArea.ClusterOverlap <= 0f)
		{
			return;
		}
		List<ClusterEntry> list = new List<ClusterEntry>();
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			PartUnitInteractions optional = allUnit.GetOptional<PartUnitInteractions>();
			if (optional == null)
			{
				continue;
			}
			int num = -1;
			for (int i = 0; i < optional.Interactions.Count; i++)
			{
				IUnitInteraction unitInteraction = optional.Interactions[i];
				if (unitInteraction is IGlobalCooldownUser { UseGlobalCooldown: not false, CanCluster: not false } && unitInteraction.IsApproach && unitInteraction.Distance > num)
				{
					num = unitInteraction.Distance;
				}
			}
			if (num >= 0)
			{
				list.Add(new ClusterEntry
				{
					UniqueId = allUnit.UniqueId,
					Position = allUnit.Position,
					Radius = num
				});
			}
		}
		if (list.Count < 2)
		{
			return;
		}
		int[] array = new int[list.Count];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = j;
		}
		float clusterOverlap = blueprintArea.ClusterOverlap;
		for (int k = 0; k < list.Count; k++)
		{
			for (int l = k + 1; l < list.Count; l++)
			{
				float num2 = clusterOverlap * (float)(list[k].Radius + list[l].Radius);
				if (Vector3.Distance(list[k].Position, list[l].Position) <= num2)
				{
					UnionFind_Union(array, k, l);
				}
			}
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int m = 0; m < list.Count; m++)
		{
			int key = UnionFind_Find(array, m);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = m_Clusters.Count;
				m_Clusters.Add(new List<string>());
				dictionary[key] = value;
			}
			m_Clusters[value].Add(list[m].UniqueId);
		}
		foreach (List<string> cluster in m_Clusters)
		{
			if (cluster.Count < 2)
			{
				continue;
			}
			int num3 = m_Clusters.IndexOf(cluster);
			foreach (string item in cluster)
			{
				m_EntityClusterIndex[item] = num3;
			}
			PFLog.GlobalCooldown.Log(string.Format("Cluster {0}: {1}", num3, string.Join(", ", cluster)));
		}
	}

	private static int UnionFind_Find(int[] parent, int i)
	{
		while (parent[i] != i)
		{
			parent[i] = parent[parent[i]];
			i = parent[i];
		}
		return i;
	}

	private static void UnionFind_Union(int[] parent, int a, int b)
	{
		int num = UnionFind_Find(parent, a);
		int num2 = UnionFind_Find(parent, b);
		if (num != num2)
		{
			parent[num] = num2;
		}
	}
}
