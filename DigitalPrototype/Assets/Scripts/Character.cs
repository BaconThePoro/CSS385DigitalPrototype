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
    private bool isDead = false;
    private bool canAttack = true;
    private bool isEnemy = false;
    public int HPCost = 0;
    public int STRCost = 0;
    public int MAGCost = 0;
    public int DEFCost = 0;
    public int RESCost = 0;
    public int SPDCost = 0;
    public int MOVCost = 0;
    private int HPMAX = 40;
    private int STRMAX = 20;
    private int MAGMAX = 20;
    private int DEFMAX = 20;
    private int RESMAX = 20;
    private int SPDMAX = 20;
    private int MOVMAX = 6;

    public enum bodyType { Spring, Cog };
    private bodyType currBody;
    public enum weaponType { Sword, Bow, Axe, FireTome, LightningTome };
    private weaponType currWeapon;

    // number means able to attack at that range and all lower ranges
    private float attackRange;
    private GameObject weaponSprites = null;
    private GameObject bodySprites = null;

    public bool getIsDead()
    {
        return isDead;
    }

    public bool getCanAttack()
    {
        return canAttack;
    }

    public bool getIsEnemy()
    {
        return isEnemy;
    }

    public int getHPMAX()
    {
        return HPMAX;
    }

    public int getSTRMAX()
    {
        return STRMAX;
    }

    public int getMAGMAX()
    {
        return MAGMAX;
    }

    public int getDEFMAX()
    {
        return DEFMAX;
    }

    public int getRESMAX()
    {
        return RESMAX;
    }

    public int getSPDMAX()
    {
        return SPDMAX;
    }

    public int getMOVMAX()
    {
        return MOVMAX;
    }

    public void updateCosts()
    {
        HPCost = Mathf.FloorToInt(baseHP / 2) + 1;
        STRCost = Mathf.FloorToInt(baseSTR / 2) + 1;
        MAGCost = Mathf.FloorToInt(baseMAG / 2) + 1;
        DEFCost = Mathf.FloorToInt(baseDEF / 2) + 1;
        RESCost = Mathf.FloorToInt(baseRES / 2) + 1;
        SPDCost = Mathf.FloorToInt(baseSPD / 2) + 1;
        MOVCost = Mathf.FloorToInt(baseMOV / 2) + 1;
    }

    public void setEnemy()
    {
        isEnemy = true;
    }

    public float getAttackRange()
    {
        return attackRange;
    }

    public bodyType GetBodyType()
    {
        return currBody;
    }

    public weaponType GetWeaponType()
    {
        return currWeapon;
    }


    public void changeBody(bodyType choice)
    {
        currBody = choice;
        setBodyVisuals();
    }

    public void changeWeapon(weaponType choice)
    {
        currWeapon = choice;
        setWeaponStats();
    }

    public void setBodyVisuals()
    {
        GetComponent<SpriteRenderer>().sprite = bodySprites.transform.GetChild((int)currBody).GetComponent<SpriteRenderer>().sprite;

        if (isEnemy == true)
            GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void setWeaponVisuals()
    {
        GameObject weapon = transform.GetChild(1).gameObject;
        GameObject weaponPrefab = weaponSprites.transform.GetChild((int)currWeapon).gameObject;

        // set weapon sprite based on currently equiped weapon
        weapon.GetComponent<SpriteRenderer>().sprite = weaponPrefab.GetComponent<SpriteRenderer>().sprite;
        weapon.transform.localScale = weaponPrefab.transform.localScale;
        weapon.transform.localPosition = weaponPrefab.transform.localPosition;
    }

    public void setWeaponStats()
    {
        setWeaponVisuals();

        // sword (no stat change)
        if (currWeapon == weaponType.Sword)
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
        // axe (+3 STR, -3 SPD)
        else if (currWeapon == weaponType.Axe)
        {
            attackRange = 1;

            // (+3 STR, -3 SPD)
            HPMOD = 0;
            STRMOD = 3;
            MAGMOD = 0;
            DEFMOD = 0;
            RESMOD = 0;
            SPDMOD = -3;
            MOVMOD = 0;
        }
        // bow + fire tome + healing tome (no stat change)
        else if (currWeapon == weaponType.Bow || currWeapon == weaponType.FireTome)
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
        else if (currWeapon == weaponType.LightningTome)
        {
            attackRange = 2;

            // -3 STR, +3 SPD
            HPMOD = 0;
            STRMOD = -3;
            MAGMOD = 0;
            DEFMOD = 0;
            RESMOD = 0;
            SPDMOD = 3;
            MOVMOD = 0;
        }


        updateStats();
    }

    public void updateStats()
    {
        HP = baseHP + HPMOD;
        STR = baseSTR + STRMOD;
        MAG = baseMAG + MAGMOD;
        DEF = baseDEF + DEFMOD;
        RES = baseRES + RESMOD;
        SPD = baseSPD + SPDMOD;
        MOV = baseMOV + MOVMOD;
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

    public void undie()
    {
        isDead = false;
        gameObject.SetActive(true);
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

        // do this for allies only
        if (isEnemy == false)
        {
            SpriteRenderer sR = transform.GetComponent<SpriteRenderer>();
            if (canAttack == true)
            {
                sR.color = Color.white;
            }
            else
            {
                sR.color = Color.grey;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {       
        charName = gameObject.name;
        weaponSprites = GameObject.Find("WeaponSprites");
        bodySprites = GameObject.Find("BodySprites");

        updateStats();
        resetHP();
        resetMove();
        setAttack(true);
        setWeaponStats();
        setWeaponVisuals();
        setBodyVisuals();
        updateCosts();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
