using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[ComponentName("Root/UnitTemplateRoot")]
[TypeId("9975b0f67d8243d7b9c0612a207afcfe")]
public class UnitTemplateRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<UnitTemplateRoot>
	{
	}

	public class UnitTemplateRootReference : BlueprintReference<UnitTemplateRoot>
	{
		public UnitTemplateRootReference()
		{
			guid = "355d6a206371431b8d6407ae1105ab9c";
		}
	}

	public List<BlueprintUnitTemplateReference> Templates = new List<BlueprintUnitTemplateReference>();

	private static readonly UnitTemplateRootReference s_Instance = new UnitTemplateRootReference();

	public static UnitTemplateRoot Instance => s_Instance;

	public static List<BlueprintUnitTemplateReference> GetTemplates()
	{
		return Instance.Templates.Where((BlueprintUnitTemplateReference t) => t.Blueprint != null).ToList();
	}

	public static void AddTemplate(BlueprintUnitTemplateReference blueprintUnitTemplate)
	{
		if (!Instance.Templates.Contains(blueprintUnitTemplate))
		{
			Instance.Templates.Add(blueprintUnitTemplate);
		}
		DeleteMissingTemplates();
		Instance.SetDirty();
	}

	private static void DeleteMissingTemplates()
	{
		for (int num = Instance.Templates.Count - 1; num >= 0; num--)
		{
			if (Instance.Templates[num].Blueprint == null)
			{
				Instance.Templates.RemoveAt(num);
			}
		}
	}
}
