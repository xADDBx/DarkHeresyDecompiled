using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.EntitySystem;

public class RuntimeAreaSettings
{
	private readonly struct CROverrideEntry
	{
		public readonly CROverrideToken Token;

		public readonly int Cr;

		public CROverrideEntry(CROverrideToken token, int cr)
		{
			Token = token;
			Cr = cr;
		}
	}

	public readonly CountableFlag Peaceful = new CountableFlag();

	public readonly CountableFlag IgnorePartyEncumbrance = new CountableFlag();

	public readonly CountableFlag IgnorePersonalEncumbrance = new CountableFlag();

	public readonly CountableFlag DisableLocalMap = new CountableFlag();

	public readonly CountableFlag DisableDetectiveServoskull = new CountableFlag();

	private readonly List<CROverrideEntry> m_CROverrides = new List<CROverrideEntry>();

	private CROverrideToken m_SingleSlotToken;

	private readonly CountingGuard m_CapitalPartyMode = new CountingGuard(canGoNegative: true);

	private readonly List<EtudeBracketForceInitiativeOrder> m_EtudeBracketForceInitiativeOrders = new List<EtudeBracketForceInitiativeOrder>();

	public int? CROverride
	{
		get
		{
			if (m_CROverrides.Count <= 0)
			{
				return null;
			}
			List<CROverrideEntry> cROverrides = m_CROverrides;
			return cROverrides[cROverrides.Count - 1].Cr;
		}
		set
		{
			if (m_SingleSlotToken != null)
			{
				PopCROverride(m_SingleSlotToken);
				m_SingleSlotToken = null;
			}
			if (value.HasValue)
			{
				m_SingleSlotToken = PushCROverride(value.Value);
			}
		}
	}

	public bool CapitalModeTemporaryDisabled_Hack { get; set; }

	public bool CapitalPartyMode
	{
		get
		{
			if (!CapitalModeTemporaryDisabled_Hack)
			{
				return m_CapitalPartyMode;
			}
			return false;
		}
	}

	[CanBeNull]
	public EtudeBracketForceInitiativeOrder CurrentEtudeBracketForceInitiativeOrder => m_EtudeBracketForceInitiativeOrders.LastOrDefault();

	public CROverrideToken PushCROverride(int cr)
	{
		CROverrideToken cROverrideToken = new CROverrideToken();
		m_CROverrides.Add(new CROverrideEntry(cROverrideToken, cr));
		return cROverrideToken;
	}

	public bool PopCROverride(CROverrideToken token)
	{
		if (token == null)
		{
			return false;
		}
		for (int num = m_CROverrides.Count - 1; num >= 0; num--)
		{
			if (m_CROverrides[num].Token == token)
			{
				m_CROverrides.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public bool SetCapitalMode(bool value)
	{
		if (m_CapitalPartyMode.SetValue(value))
		{
			Game.Instance.Player.InvalidateCharacterLists();
			EventBus.RaiseEvent(delegate(IPartyHandler h)
			{
				h.HandleCapitalModeChanged();
			});
			return true;
		}
		return false;
	}

	public void SetEtudeBracketForceInitiativeOrder([NotNull] EtudeBracketForceInitiativeOrder value)
	{
		if (m_EtudeBracketForceInitiativeOrders.Contains(value))
		{
			PFLog.Default.Error("EtudeBracketForceInitiativeOrders list already contains the order instance! Ignoring...");
		}
		else
		{
			m_EtudeBracketForceInitiativeOrders.Add(value);
		}
	}

	public void RemoveEtudeBracketForceInitiativeOrder([NotNull] EtudeBracketForceInitiativeOrder value)
	{
		if (!m_EtudeBracketForceInitiativeOrders.Remove(value))
		{
			PFLog.Default.Error("EtudeBracketForceInitiativeOrders list didn't contain the order instance!");
		}
	}
}
