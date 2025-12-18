using System;
using System.Text;
using Kingmaker.ElementsSystem;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Serializable]
public class DamageSettings
{
	[JsonProperty]
	public int Bonus;

	[JsonProperty]
	public DamageTypeSettings TypeDescription = new DamageTypeSettings();

	[SerializeReference]
	public IntEvaluator EvaluatedBonus;

	public bool CausedByCheckFail;

	private int m_BonusWithSource;

	public IntermediateDamage CreateDamage()
	{
		int num = Bonus - m_BonusWithSource;
		if (EvaluatedBonus != null)
		{
			num += EvaluatedBonus.GetValue();
		}
		IntermediateDamage intermediateDamage = TypeDescription.CreateDamage(num);
		intermediateDamage.CausedByCheckFail = CausedByCheckFail;
		return intermediateDamage;
	}

	public string GetReadableFormula()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Bonus != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" + ");
			}
			stringBuilder.Append(Bonus);
		}
		if (EvaluatedBonus != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" + ");
			}
			stringBuilder.Append(EvaluatedBonus.GetCaption());
		}
		return stringBuilder.ToString();
	}
}
