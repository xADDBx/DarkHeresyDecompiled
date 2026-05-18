using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.Controllers;

public class BarkController : IController
{
	public class Data
	{
		public Entity Entity;

		public IBarkHandle Handle;

		public ActionExecutionContextData.Type ContextType;
	}

	private Dictionary<ActionExecutionContextData.Type, List<Data>> m_ContextTypeToDataMap = new Dictionary<ActionExecutionContextData.Type, List<Data>>();

	private readonly List<IBarkHandle> m_AllHandles = new List<IBarkHandle>();

	private readonly CountableFlag m_SuppressFlag = new CountableFlag();

	private readonly CountableFlag m_CutsceneContextFlag = new CountableFlag();

	public bool IsSuppressed
	{
		get
		{
			if ((bool)m_SuppressFlag)
			{
				return !m_CutsceneContextFlag;
			}
			return false;
		}
	}

	public void EnterSuppression()
	{
		m_SuppressFlag.Retain();
	}

	public void LeaveSuppression()
	{
		m_SuppressFlag.Release();
	}

	public void EnterCutsceneContext()
	{
		m_CutsceneContextFlag.Retain();
	}

	public void LeaveCutsceneContext()
	{
		m_CutsceneContextFlag.Release();
	}

	public void TrackHandle(IBarkHandle handle)
	{
		m_AllHandles.Add(handle);
	}

	public void UntrackHandle(IBarkHandle handle)
	{
		m_AllHandles.Remove(handle);
	}

	public void StopAll()
	{
		List<IBarkHandle> list = new List<IBarkHandle>(m_AllHandles);
		m_AllHandles.Clear();
		foreach (IBarkHandle item in list)
		{
			item.InterruptBark();
		}
	}

	public void Add(Entity entity, BarkHandle barkHandle)
	{
		ActionExecutionContextData.Type type = ((ContextData<ActionExecutionContextData>.Current != null) ? ContextData<ActionExecutionContextData>.Current.ContextType : ActionExecutionContextData.Type.None);
		if (type == ActionExecutionContextData.Type.Interaction)
		{
			Data item = new Data
			{
				Entity = entity,
				Handle = barkHandle,
				ContextType = type
			};
			if (type == ActionExecutionContextData.Type.Interaction)
			{
				StopAllBarksFromInteractions();
			}
			m_ContextTypeToDataMap.TryAdd(type, new List<Data>());
			m_ContextTypeToDataMap[type].Add(item);
		}
	}

	private void StopAllBarksFromInteractions()
	{
		if (!m_ContextTypeToDataMap.TryGetValue(ActionExecutionContextData.Type.Interaction, out var value))
		{
			return;
		}
		foreach (Data item in value)
		{
			item.Handle.InterruptBark();
		}
		m_ContextTypeToDataMap[ActionExecutionContextData.Type.Interaction].Clear();
	}
}
