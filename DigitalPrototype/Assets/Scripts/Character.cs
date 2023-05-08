using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public string charName = "blankName";
    // base stats are stats before being affected by weapon stats
    public int baseHP = 10;
    public int baseSTR = 1;
    public int baseMAG = 1;
    public int baseDEF = 1;
    public int baseRES = 1;
    public int baseSPD = 1;
    public int baseMOV = 1;
    public int HPMOD = 0;
    public int STRMOD = 0;
    public int MAGMOD = 0;
    public int DEFMOD = 0;
    public int RESMOD = 0;
    public int SPDMOD = 0;
    public int MOVMOD = 0;
    public int HP = 10;
    public int STR = 1;
    public int MAG = 1;
    public int DEF = 1;
    public int RES = 1;
    public int SPD = 1;
    public int MOV = 1;
    public int hpLeft;
    public int movLeft;
    public bool isDead = false;
    public bool canAttack = true; 

    public int weapon = 1;
    // 1. Sword
    // 2. Bow
    // 3. 
    // 4. 

    // number means able to attack at that range and all lower ranges

    public float attackRange;

    public void changeWeapon(int choice)
    {
        weapon = choice;

        // sword (no stat change)
        if (weapon == 1)
        {
            attackRange = 1;

            // 0 all mods
            HPMOD = 0;
            STRMOD = 0;
            MAGMOD = 0;
            DEFMOD = 0;
            RESMOD = 0;
            SPDMOD = 0;
            MOVMOD = 0;
}
        // bow (no stat change)
        else if (weapon == 2)
        {
            attackRange = 2;

            // 0 all mods
            HPMOD = 0;
            STRMOD = 0;
            MAGMOD = 0;
            DEFMOD = 0;
            RESMOD = 0;
            SPDMOD = 0;
            MOVMOD = 0;
        }
        // axe (+3 STR, -3 SPD)
        else if (weapon == 3)
        {
            attackRange = 2;

            // 
            HPMOD = 0;
            STRMOD = 3;
            MAGMOD = 0;
            DEFMOD = 0;
            RESMOD = 0;
            SPDMOD = -3;
            MOVMOD = 0;
        }

    }

    public void takeDamage(int amount)
    {
        hpLeft = hpLeft - amount;

        if (hpLeft < 0)
            hpLeft = 0;
        if (hpLeft <= 0)
            die();

    }

    public void die()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

    public void resetMove()
    {
        movLeft = MOV;
    }

    public void resetHP()
    {
        hpLeft = HP;
    }

    public void setAttack(bool b)
    {
        canAttack = b;
    }

    // Start is called before the first frame update
    void Start()
    {       
        charName = gameObject.name;
        HP = baseHP + HPMOD;
        STR = baseSTR + STRMOD;
        MAG = baseMAG + MAGMOD;
        DEF = baseDEF + DEFMOD;
        RES = baseRES + RESMOD;
        SPD = baseSPD + SPDMOD;
        MOV = baseMOV + MOVMOD;

        resetHP();
        resetMove();
        setAttack(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
