﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    Dictionary<string, int> Skills;

    private List<SkillObject> skillobjects;
    private Player player;
    private int skillamount;

    public SkillManager(Player p)
    {
        player = p;
        Skills = new Dictionary<string, int>();
        skillobjects = new List<SkillObject>();
    }

    /**
     * Adds stats to the player using the data in the SkillObject.
     * 
     */
    public void AddSkill(SkillObject skill)
    {
        if (Skills.ContainsKey(skill.name)) // Adds to the stack of skills
        {
            Skills[skill.name] += 1;
        }
        else
        {
            skillobjects.Add(skill);
            Skills.Add(skill.name, 1);
        }

        AddSkillStats(skill, true);
        skillamount++;
    }

    /**
     * Removes a stack of skill
     */
    public void RemoveSkill(SkillObject skill)
    {
        if (Skills.ContainsKey(skill.name)) // Removes the skill from the dictionary
        {
            Skills[skill.name] -= 1;
            if (Skills[skill.name] <= 0)
            {
                Skills.Remove(skill.name);
            }
            AddSkillStats(skill, false);
            skillamount--;
        }
    }

    /**
     * Adds the stats to the player. If Adding is true
     * Then it is a positive addition, otherwise it is negative
     */
    private void AddSkillStats(SkillObject skill, bool IsAdding)
    {
        int multi = IsAdding ? 1 : -1;
        player.GetHealth().AddMaxValue(skill.health * multi);
        player.GetDamage().AddMaxValue(skill.damage * multi);
        player.GetSpeed().AddMaxValue(skill.speed * multi);
        player.GetAttackSpeed().AddMaxValue(skill.attackspeed * multi);
        player.GetFlatDamageReduction().AddMaxValue(skill.flatdamagereduction * multi);
        player.GetDamageReduction().AddMaxValue(skill.damagereduction * multi);
        player.GetDodgeChance().AddMaxValue(skill.dodgechance * multi);
        player.GetCritChance().AddMaxValue(skill.critchance * multi);
        player.GetCritDamage().AddMaxValue(skill.critdamage * multi);
        player.GetRange().AddMaxValue(skill.range * multi);
        player.GetShield().AddMaxValue(skill.shield * multi);
        player.GetWeight().AddMaxValue(skill.weight * multi);
    }

    /**
     * Returns whether or not the SkillManager contains the skill
     */
    public bool HasSkill(string name)
    {
        return Skills.ContainsKey(name);
    }

    /**
     * Returns a random SkillObject that exists in the resources/skills folder
     */
    public SkillObject GetRandomSkill()
    {
        Object[] skills = Resources.LoadAll("Skills");
        SkillObject skill = (SkillObject)skills[Random.Range(0, skills.Length)];
        //Debug.Log(skill.name);
        return skill;
    }

    /**
    * Returns a random SkillObject that exists in the resources/skills/abilities folder
    */
    public SkillObject GetRandomAbility()
    {
        Object[] skills = Resources.LoadAll("Skills/Abilities");
        SkillObject skill = (SkillObject)skills[Random.Range(0, skills.Length)];
        //Debug.Log(skill.name);
        return skill;
    }

    public List<SkillObject> GetSkillObjects()
    {
        return skillobjects;
    }

    public int GetSkillStack(string name)
    {
        return Skills[name];
    }

    public int GetAmount() {
        return skillamount;
    }

}
