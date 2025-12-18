using System.Collections.Generic;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneSignalDispatcher
{
	private readonly List<CutscenePlayerBlockData> m_Blocks = new List<CutscenePlayerBlockData>();

	private readonly List<CutscenePlayerGateData> m_Gates = new List<CutscenePlayerGateData>();

	public void RegisterBlock(CutscenePlayerBlockData block)
	{
		m_Blocks.Add(block);
	}

	public void RegisterGate(CutscenePlayerGateData gate)
	{
		m_Gates.Add(gate);
	}

	public void SignalBlock(CutsceneBlock block)
	{
		m_Blocks.FirstOrDefault((CutscenePlayerBlockData bp) => bp.Block == block)?.SignalBlock();
	}

	public void SignalBlock(string blockId)
	{
		m_Blocks.FirstOrDefault((CutscenePlayerBlockData bp) => bp.Block.Guid == blockId)?.SignalBlock();
	}

	public void SignalGate(CutsceneGate gate)
	{
		m_Gates.FirstOrDefault((CutscenePlayerGateData bp) => bp.Gate == gate)?.SignalGate();
	}

	public void SignalGate(string gateId)
	{
		m_Gates.FirstOrDefault((CutscenePlayerGateData bp) => bp.Gate.Guid == gateId)?.SignalGate();
	}

	public void Clear()
	{
		m_Blocks.Clear();
		m_Gates.Clear();
	}
}
