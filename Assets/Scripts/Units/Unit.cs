using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    int experience, HP, MP, strength, stamina, agility, luck, intelligence, willpower, movement, level;
    int expToNextLevel, maxHP, maxMP;
    BiomeTile onBiome;

    [HideInInspector] public bool usesEnergy = false;
    [HideInInspector] public bool isSelected;

    protected UnitProcessing up;

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

    public BiomeTile GetBiome() { return onBiome; }
    public void SetBiome(BiomeTile onBiome) { this.onBiome = onBiome; }

    public Unit()
    {
        SetEXP(0);
        SetLevel(1);
    }

    private void Awake()
    {
        up = FindObjectOfType<UnitProcessing>();
    }

    protected virtual void SetUnitProcessingVars()
    {
        SetExpToNextLevel(Mathf.RoundToInt(level * up.toNextLevelFactor));

        GetCurrentBiomeTile();
    }

    void GetCurrentBiomeTile()
    {
        RaycastHit[] hits;
        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), -transform.up);
        hits = Physics.RaycastAll(ray, 1000);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("BiomeTile"))
            {
                SetBiome(hit.transform.GetComponent<BiomeTile>());
            }
        }
    }

    protected void UnitAwake()
    {
        GetComponent<Outline>().OutlineWidth = up.highlightWidth;
        GetComponent<Outline>().enabled = false;
    }
}
