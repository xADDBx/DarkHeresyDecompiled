using System;
using System.Linq;
using System.Reflection;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Items;
using Kingmaker.UI;
using Kingmaker.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Init;

internal static class BugReportServiceInit
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeTooltipsHelperService()
	{
		BugReportService.GetAbilityContext = GetAbilityContext;
		BugReportService.HasBark = HasBark;
		BugReportService.GetOtherUIFeatureName = GetOtherUIFeature;
	}

	private static BugContext GetAbilityContext(TooltipBaseTemplate tooltip)
	{
		BugContext bugContext = new BugContext("Ability");
		if (!(tooltip is TooltipTemplateBuff tooltipTemplateBuff))
		{
			if (!(tooltip is TooltipTemplateFeature tooltipTemplateFeature))
			{
				if (!(tooltip is TooltipTemplateUIFeature tooltipTemplateUIFeature))
				{
					if (!(tooltip is TooltipTemplateAbility tooltipTemplateAbility))
					{
						if (!(tooltip is TooltipTemplateToggleAbility tooltipTemplateToggleAbility))
						{
							if (!(tooltip is TooltipTemplateLevelUpModifier tooltipTemplateLevelUpModifier))
							{
								if (!(tooltip is TooltipTemplateLevelUpAbility tooltipTemplateLevelUpAbility))
								{
									if (!(tooltip is TooltipTemplateAbilityTag tooltipTemplateAbilityTag))
									{
										if (tooltip is TooltipTemplateItem tooltipTemplateItem)
										{
											try
											{
												bugContext = new BugContext("Item");
												if (tooltipTemplateItem.Item != null)
												{
													bugContext.ContextObject = tooltipTemplateItem.Item.Blueprint;
												}
												else
												{
													ItemEntity itemEntity = (ItemEntity)(typeof(TooltipTemplateItem).GetField("m_Item", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(tooltipTemplateItem));
													if (itemEntity != null)
													{
														bugContext.ContextObject = itemEntity.Blueprint;
													}
												}
											}
											catch (Exception ex)
											{
												PFLog.Default.Log("BugReport collect  Item tooltip exception: " + ex.Message + "\n" + ex.StackTrace);
											}
										}
									}
									else
									{
										bugContext.ContextObject = tooltipTemplateAbilityTag.AbilityTag;
									}
								}
								else
								{
									bugContext.ContextObject = tooltipTemplateLevelUpAbility.BlueprintAbility;
								}
							}
							else
							{
								bugContext.ContextObject = tooltipTemplateLevelUpModifier.BlueprintModifier;
							}
						}
						else
						{
							bugContext.ContextObject = tooltipTemplateToggleAbility.BlueprintAbility;
						}
					}
					else
					{
						bugContext.ContextObject = tooltipTemplateAbility.BlueprintAbility;
					}
				}
				else
				{
					bugContext.ContextObject = tooltipTemplateUIFeature.UIFeature.Feature;
				}
			}
			else
			{
				bugContext.ContextObject = tooltipTemplateFeature.BlueprintFeatureBase;
			}
		}
		else
		{
			bugContext.ContextObject = tooltipTemplateBuff.BlueprintBuff;
		}
		return bugContext;
	}

	private static bool HasBark()
	{
		foreach (OvertipBarkBlockView item in UnityEngine.Object.FindObjectsByType<OvertipBarkBlockView>(FindObjectsSortMode.None).ToList())
		{
			try
			{
				RectTransform component = item.transform.GetChild(0).GetComponent<RectTransform>();
				if (component != null)
				{
					RectTransformUtility.ScreenPointToLocalPointInRectangle(component, Input.mousePosition, UICamera.Instance, out var localPoint);
					if (Math.Abs(localPoint.x) < component.rect.width && localPoint.y >= 0f && localPoint.y < component.rect.height)
					{
						return true;
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static string GetOtherUIFeature()
	{
		if (RootUIContext.Instance.HasIngameMenu)
		{
			return "Ingame Menu";
		}
		if (!HasBark())
		{
			return string.Empty;
		}
		return "Bark";
	}
}
