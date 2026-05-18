using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[Serializable]
public class PhenomenaListOverride
{
	public PhenomenaOverrideMode Mode;

	[ShowIf("IsReplace")]
	public BlueprintPsykerRoot.PhenomenaData[] ReplacementList = new BlueprintPsykerRoot.PhenomenaData[0];

	[ShowIf("IsModify")]
	public PhenomenaModificationEntry[] Modifications = new PhenomenaModificationEntry[0];

	private bool IsReplace => Mode == PhenomenaOverrideMode.Replace;

	private bool IsModify => Mode == PhenomenaOverrideMode.Modify;

	public List<BlueprintPsykerRoot.PhenomenaData> Resolve(List<BlueprintPsykerRoot.PhenomenaData> input)
	{
		switch (Mode)
		{
		case PhenomenaOverrideMode.Inherit:
			return input;
		case PhenomenaOverrideMode.Replace:
		{
			List<BlueprintPsykerRoot.PhenomenaData> list = new List<BlueprintPsykerRoot.PhenomenaData>(ReplacementList.Length);
			list.AddRange(ReplacementList);
			return list;
		}
		case PhenomenaOverrideMode.Modify:
			return ApplyModifications(input);
		default:
			return input;
		}
	}

	private List<BlueprintPsykerRoot.PhenomenaData> ApplyModifications(List<BlueprintPsykerRoot.PhenomenaData> input)
	{
		List<BlueprintPsykerRoot.PhenomenaData> list = new List<BlueprintPsykerRoot.PhenomenaData>(input);
		PhenomenaModificationEntry[] modifications = Modifications;
		foreach (PhenomenaModificationEntry mod in modifications)
		{
			switch (mod.Type)
			{
			case PhenomenaModificationType.Ban:
				list.RemoveAll((BlueprintPsykerRoot.PhenomenaData p) => p.Ability.Equals(mod.Ability));
				break;
			case PhenomenaModificationType.Reweight:
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].Ability.Equals(mod.Ability))
					{
						BlueprintPsykerRoot.PhenomenaData value = new BlueprintPsykerRoot.PhenomenaData
						{
							Bark = list[j].Bark,
							Ability = list[j].Ability,
							OptionalMinorFX = list[j].OptionalMinorFX,
							Weight = mod.NewWeight
						};
						list[j] = value;
					}
				}
				break;
			}
			case PhenomenaModificationType.Add:
				if (mod.AddData != null)
				{
					list.Add(mod.AddData);
				}
				break;
			}
		}
		return list;
	}
}
