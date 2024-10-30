public class Champion
{
    public string Version { get; set; }
    public string Id { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Blurb { get; set; }
    public Info Info { get; set; }
    public Image Image { get; set; }
    public List<string> Tags { get; set; }
    public string Partype { get; set; }
    public Stats Stats { get; set; }
}

public class Info
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Magic { get; set; }
    public int Difficulty { get; set; }
}

public class Image
{
    public string Full { get; set; }
    public string Sprite { get; set; }
    public string Group { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}

public class Stats
{
    public float Hp { get; set; }
    public float HpPerLevel { get; set; }
    public float Mp { get; set; }
    public float MpPerLevel { get; set; }
    public int MoveSpeed { get; set; }
    public float Armor { get; set; }
    public float ArmorPerLevel { get; set; }
    public float SpellBlock { get; set; }
    public float SpellBlockPerLevel { get; set; }
    public int AttackRange { get; set; }
    public float HpRegen { get; set; }
    public float HpRegenPerLevel { get; set; }
    public float MpRegen { get; set; }
    public float MpRegenPerLevel { get; set; }
    public int Crit { get; set; }
    public int CritPerLevel { get; set; }
    public float AttackDamage { get; set; }
    public float AttackDamagePerLevel { get; set; }
    public float AttackSpeedPerLevel { get; set; }
    public float AttackSpeed { get; set; }
}