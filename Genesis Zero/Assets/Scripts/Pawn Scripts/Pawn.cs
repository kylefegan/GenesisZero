﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Kenny Doan
 * Pawn is a base class for entities that contain statistics, status and can take damage
 * It can handle taking damage and maintaining the statuses/statistics it contains.
 * To use the pawn class, one should extend the class and call the base function Update()
 */
public class Pawn : MonoBehaviour
{
    private Statistic health, damage, speed, attackspeed, flatdamagereduction, damagereduction, dodgechance, critchance, critdamage, range, shield, weight;
    private List<Statistic> statistics;
    public StatObject Stats;

    private Status invunerable, stunned, burning, slowed;
    private List<Status> statuses;

    private float burntime, burndamage; //burndamage is damage per second
    private float slowtime, knockbackforce;
    private Vector3 knockbackvector;

    protected void Start()
    {
        if (Stats != null)
        {
            AddStats();
        }
        else
        {
            Debug.LogError("No statistics have been assigned to " + transform.parent.name);
        }

        InitializeStatuses();
    }

    /**
     * The function that handles damage taken. Does not handle things like burning and knockback
     */
    public float TakeDamage(float amount)
    {
        int chance = Random.Range(0, 100);
        if (chance > GetDodgeChance().GetValue() * 100) // Dodging will ignore damage
        {
            float finaldamage = amount;
            if (invunerable.IsActive() == true)
            {
                finaldamage = 0;
            }
            finaldamage -= GetFlatDamageReduction().GetValue();
            finaldamage = finaldamage - finaldamage * GetDamageReduction().GetValue();

            // Shield Damage
            if (GetShield().GetValue() > 0)
            {
                float diff = GetShield().GetValue() - finaldamage;
                if (diff > 0)
                {
                    GetShield().AddValue(-finaldamage);
                    finaldamage = 0;
                }
                else
                {
                    GetShield().SetValue(0);
                    finaldamage = -diff;
                }
            }

            // Damage
            GetHealth().AddValue(-finaldamage);
            return finaldamage;
            //Debug.Log("Health Left:"+GetHealth().GetValue());
        }
        else
        {
            //Debug.Log(transform.root.name + " Dodged!");
            return 0;
            //Dodge Effect
        }
    }

    /**
     * Restores health to the pawn
     */
    public void Heal(float amount)
    {
        //Heal effect
        GetHealth().AddValue(amount);
    }

    protected void Update()
    {
        foreach (Statistic stat in statistics)
        {
            stat.UpdateStatistics();
        }
        foreach (Status status in statuses)
        {
            status.UpdateStatus();
        }



        if (IsBurning())
        {
            GetHealth().AddValue(-burndamage * Time.deltaTime);
            burntime -= Time.deltaTime;
        }
        if (IsSlowed())
        {
            speed.AddBonus(-GetSpeed().GetMaxValue() * GetSlowedStatus().GetFactor(), Time.deltaTime);
            slowtime -= Time.deltaTime;
        }

        if (GetHealth().GetValue() <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    protected void FixedUpdate()
    {
        if (knockbackforce > 0)
        {
            Translate(knockbackvector.normalized * knockbackforce); // Move the pawn a certain distance
            knockbackforce *= Mathf.Clamp(8f / GetWeight().GetValue(), 0, .95f);
        }
    }

        // ------------------------- ACCESS FUNCTIONS ------------------------------//
        public Statistic GetHealth()
    {
        return health;
    }

    public Statistic GetDamage()
    {
        return damage;
    }

    public Statistic GetSpeed()
    {
        return speed;
    }

    public Statistic GetAttackSpeed()
    {
        return attackspeed;
    }

    public Statistic GetFlatDamageReduction()
    {
        return flatdamagereduction;
    }

    public Statistic GetDamageReduction()
    {
        return damagereduction;
    }

    public Statistic GetDodgeChance()
    {
        return dodgechance;
    }

    public Statistic GetCritChance()
    {
        return critchance;
    }

    public Statistic GetCritDamage()
    {
        return critdamage;
    }

    public Statistic GetRange()
    {
        return range;
    }

    public Statistic GetShield()
    {
        return shield;
    }

    public Statistic GetWeight()
    {
        return weight;
    }

    public bool IsStunned()
    {
        return stunned.IsTrue();
    }

    public Status GetStunnedStatus()
    {
        return stunned;
    }

    public bool IsInvunerable()
    {
        return invunerable.IsTrue();
    }

    public void SetInvunerable(float time)
    {
        invunerable.SetTime(time);
    }

    public Status GetInvunerableStatus()
    {
        return invunerable;
    }

    public bool IsBurning()
    {
        return burning.IsTrue();
    }

    public void Burn(float time, float damage)
    {
        burntime = time;
        burndamage = damage;
        burning.SetTime(time);
    }

    public void KnockBack(Vector3 direction, float force)
    {
        knockbackvector = direction;
        knockbackforce = force;
    }

    public bool IsSlowed()
    {
        return slowed.IsTrue();
    }

    public Status GetSlowedStatus()
    {
        return slowed;
    }

    public void Slow(float factor, float time){
        slowed.SetFactor(factor);
        slowtime = time;
    }
    // ------------------------- ---------------- ------------------------------//

    // ------------------------ General Functions ------------------------------//
    public Vector3 Position()
    {
        return transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Translate(Vector3 translation)
    {
        transform.position += translation;
    }

    // ------------------------- ---------------- ------------------------------//

    private void AddStats()
    {
        statistics = new List<Statistic>();
        health = new Statistic(Stats.health); statistics.Add(health);
        damage = new Statistic(Stats.damage); statistics.Add(damage);
        speed = new Statistic(Stats.speed); statistics.Add(speed);
        attackspeed = new Statistic(Stats.attackspeed); statistics.Add(attackspeed);
        flatdamagereduction = new Statistic(Stats.flatdamagereduction); statistics.Add(flatdamagereduction);
        damagereduction = new Statistic(Stats.damagereduction); statistics.Add(damagereduction);
        dodgechance = new Statistic(Stats.dodgechance); statistics.Add(dodgechance);
        critchance = new Statistic(Stats.critchance); statistics.Add(critchance);
        critdamage = new Statistic(Stats.critdamage); statistics.Add(critdamage);
        range = new Statistic(Stats.range); statistics.Add(range);
        shield = new Statistic(Stats.shield); statistics.Add(shield);
        weight = new Statistic(Stats.weight); statistics.Add(weight);
    }

    private void InitializeStatuses()
    {
        statuses = new List<Status>();
        invunerable = new Status(0); statuses.Add(invunerable);
        stunned = new Status(0); statuses.Add(stunned);
        burning = new Status(0); statuses.Add(burning);
        slowed = new Status(0); statuses.Add(slowed);
    }

}
