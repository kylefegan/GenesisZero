﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Kenny Doan
 * Skill Display is a script that displays the Skills the player has in a UI. Each element is displayed
 * and can be moused over for it's description. 
 */
public class SkillDisplay : MonoBehaviour
{
    public Vector2 StartPoint;  // Where the first skill starts
    public Vector2 Seperation;  // the x,y seperation of each skill
    public int MaxColumns = 10;
    private Player Player;
    public SkillManager SkillManager;
    private List<GameObject> skilldisplay;
    public GameObject UIElement;
    [Space]
    public Vector2 Ability1Position;
    public Vector2 Ability2Position;

    public int skillnumber;
    // Start is called before the first frame update
    void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }
        SkillManager = Player.GetSkillManager();
        skillnumber = SkillManager.GetAmount();
        skilldisplay = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //SkillManager = Player.GetSkillManager();
        if (skillnumber != SkillManager.GetAmount() || SkillManager.GetUpdated() == true)
        {
            skillnumber = SkillManager.GetAmount();
            SkillManager.SetUpdated(false);
            UpdateDisplay();
        }
    }

    /**
     * Updates the display of the skills.
     */
    public void UpdateDisplay()
    {
        List<SkillObject> skills = SkillManager.GetSkillObjects();
        foreach (GameObject sk in skilldisplay)
        {
            Destroy(sk.gameObject);
        }
        skilldisplay.Clear();
        for (int i = 0; i < skills.Count; i++)
        {
            int col = i % MaxColumns;
            int row = i / MaxColumns;

            GameObject instance = (GameObject)Instantiate(UIElement, (Vector3)transform.position + (Vector3)StartPoint + new Vector3(Seperation.x * col, Seperation.y * row), Quaternion.identity);
            instance.transform.parent = transform;
            instance.transform.localScale = new Vector3(1,1,1);
            instance.GetComponent<SkillUIElement>().SetIcon(skills[i].Icon);
            instance.GetComponent<SkillUIElement>().SetStack(SkillManager.GetSkillStack(skills[i].name));
            instance.GetComponent<SimpleTooltip>().infoLeft = skills[i].Description;
            skilldisplay.Add(instance);

            if (skills[i].IsAbility && skills[i].name == SkillManager.GetAbility1().name)
            {
                GameObject abil1 = (GameObject)Instantiate(UIElement, (Vector3)transform.position + (Vector3)Ability1Position, Quaternion.identity);
                abil1.transform.parent = transform;
                abil1.transform.localScale = new Vector3(1, 1, 1);
                abil1.GetComponent<SkillUIElement>().SetIcon(skills[i].Icon);
                abil1.GetComponent<SimpleTooltip>().infoLeft = skills[i].Description;
                skilldisplay.Add(abil1);
            }
            else if (skills[i].IsAbility && skills[i].name == SkillManager.GetAbility2().name)
            {
                GameObject abil2 = (GameObject)Instantiate(UIElement, (Vector3)transform.position + (Vector3)Ability2Position, Quaternion.identity);
                abil2.transform.parent = transform;
                abil2.transform.localScale = new Vector3(1, 1, 1);
                abil2.GetComponent<SkillUIElement>().SetIcon(skills[i].Icon);
                abil2.GetComponent<SimpleTooltip>().infoLeft = skills[i].Description;
                skilldisplay.Add(abil2);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position + (Vector3)StartPoint, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)Ability1Position, .3f);
        Gizmos.DrawWireSphere(transform.position + (Vector3)Ability2Position, .3f);
    }
}
