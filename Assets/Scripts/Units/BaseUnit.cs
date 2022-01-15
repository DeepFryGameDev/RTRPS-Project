using UnityEngine;

public class BaseUnit
{
    int experience, HP, MP, strength, stamina, agility, luck, intelligence, willpower, movement, level;
    int expToNextLevel, maxHP, maxMP;
    Sprite faceGraphic;

    [HideInInspector] public bool usesEnergy = false; // defaults to all units using Magic/MP unless otherwise specified

    public int GetEXP() { return experience; }
    public void SetEXP(int exp) { experience = exp; }
    public int GetHP() { return HP; }
    public void SetHP(int HP) { this.HP = HP; }
    public int GetMP() { return MP; }
    public void SetMP(int MP) { this.MP = MP; }
    public int GetStrength() { return strength; }
    public void SetStrength(int strength) { this.strength = strength; }
    public int GetStamina() { return stamina; }
    public void SetStamina(int stamina) { this.stamina = stamina; }
    public int GetAgility() { return agility; }
    public void SetAgility(int agility) { this.agility = agility; }
    public int GetLuck() { return luck; }
    public void SetLuck(int luck) { this.luck = luck; }
    public int GetIntelligence() { return intelligence; }
    public void SetIntelligence(int intelligence) { this.intelligence = intelligence; }
    public int GetWillpower() { return willpower; }
    public void SetWillpower(int willpower) { this.willpower = willpower; }
    public int GetMovement() { return movement; }
    public void SetMovement(int movement) { this.movement = movement; }

    public int GetLevel() { return level; }
    public void SetLevel(int level) { this.level = level; }

    public int GetExpToNextLevel() { return expToNextLevel; }
    public void SetExpToNextLevel(int expToNextLevel) { this.expToNextLevel = expToNextLevel; }
    public int GetMaxHP() { return maxHP; }
    public void SetMaxHP(int maxHP) { this.maxHP = maxHP; }
    public int GetMaxMP() { return maxMP; }
    public void SetMaxMP(int maxMP) { this.maxMP = maxMP; }

    public Sprite GetFaceGraphic() { return faceGraphic; }

    public void SetFaceGraphic(Sprite graphic) { this.faceGraphic = graphic; }
}
