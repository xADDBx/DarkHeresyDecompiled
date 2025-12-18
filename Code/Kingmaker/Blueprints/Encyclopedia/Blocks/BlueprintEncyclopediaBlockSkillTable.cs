namespace Kingmaker.Blueprints.Encyclopedia.Blocks;

public class BlueprintEncyclopediaBlockSkillTable : BlueprintEncyclopediaBlock, IBlockText, IBlockSkillTable, IBlock
{
	public string GetText()
	{
		return string.Empty;
	}

	public override string ToString()
	{
		return "Skill Table";
	}
}
