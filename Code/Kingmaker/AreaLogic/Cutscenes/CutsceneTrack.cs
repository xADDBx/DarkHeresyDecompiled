using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[Serializable]
public class CutsceneTrack : IEvaluationErrorHandlingPolicyHolder
{
	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	[SerializeField]
	private List<CommandReference> m_Commands = new List<CommandReference>();

	[SerializeField]
	private List<string> m_EndGateIds = new List<string>();

	[SerializeField]
	private bool m_Repeat;

	[SerializeField]
	private int m_RandomWeight = 1;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public List<CommandReference> Commands => m_Commands;

	public List<string> EndGateIds => m_EndGateIds;

	public bool Repeat => m_Repeat;

	public bool IsContinuous
	{
		get
		{
			if (!m_Repeat)
			{
				if (Commands != null && Commands.Count > 0)
				{
					CommandReference commandReference = Commands.Last();
					if (commandReference == null)
					{
						return false;
					}
					return commandReference.Get()?.IsContinuous == true;
				}
				return false;
			}
			return true;
		}
	}

	public int RandomWeight => m_RandomWeight;

	public int GetHashMD5()
	{
		using (ProfileScope.New("Get Track Hash"))
		{
			using MD5 mD = MD5.Create();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CommandReference command in Commands)
			{
				if (command?.Get() != null)
				{
					stringBuilder.Append(command.Guid);
				}
			}
			foreach (string endGateId in EndGateIds)
			{
				stringBuilder.Append(endGateId);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			byte[] array = mD.ComputeHash(bytes);
			int num = 0;
			for (int i = 0; i < 16; i += 4)
			{
				num ^= array[i] | (array[i + 1] << 8) | (array[i + 2] << 16) | (array[i + 3] << 24);
			}
			return num;
		}
	}
}
