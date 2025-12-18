using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitUIStateHolder : ViewModel
{
	private static UnitUIStateHolder s_Instance;

	private readonly Dictionary<MechanicEntity, MechanicEntityUIState> m_UnitStates = new Dictionary<MechanicEntity, MechanicEntityUIState>();

	public static UnitUIStateHolder Instance => s_Instance ?? Initialize();

	private static UnitUIStateHolder Initialize()
	{
		return s_Instance = new UnitUIStateHolder();
	}

	protected override void OnDispose()
	{
		Clear();
	}

	public MechanicEntityUIState GetOrCreateUnitState(MechanicEntity unitEntity)
	{
		if (unitEntity == null)
		{
			UberDebug.LogError("UnitState: Unit is null");
			return null;
		}
		if (!m_UnitStates.TryGetValue(unitEntity, out var value))
		{
			value = new MechanicEntityUIState(unitEntity).AddTo(this);
			m_UnitStates[unitEntity] = value;
		}
		return value;
	}

	public void RemoveUnitState(MechanicEntity unitEntity)
	{
		if (unitEntity != null && !m_UnitStates.Empty() && m_UnitStates.TryGetValue(unitEntity, out var value))
		{
			value.Dispose();
			m_UnitStates.Remove(unitEntity);
		}
	}

	public void RemoveUnitState(string uniqueId)
	{
		if (string.IsNullOrEmpty(uniqueId) || m_UnitStates.Empty())
		{
			return;
		}
		foreach (KeyValuePair<MechanicEntity, MechanicEntityUIState> unitState in m_UnitStates)
		{
			if (unitState.Key.UniqueId == uniqueId)
			{
				unitState.Value.Dispose();
				m_UnitStates.Remove(unitState.Key);
				break;
			}
		}
	}

	public void Clear()
	{
		m_UnitStates.Values.ForEach(delegate(MechanicEntityUIState unitState)
		{
			unitState.Dispose();
		});
		m_UnitStates.Clear();
	}
}
