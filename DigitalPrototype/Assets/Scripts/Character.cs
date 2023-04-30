using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public string charName = "blankName";
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

    public int weapon = 1;
    // 1. Sword
    // 2. 
    // 3. 
    // 4. 

    // number means able to attack at that range and all lower ranges

    public float attackRange;

    public void changeWeapon(int choice)
    {
        weapon = choice;

        if (weapon == 1)
            attackRange = 1;
        else if (weapon == 2)
            attackRange = 2;
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

    // Start is called before the first frame update
    void Start()
    {       
        charName = gameObject.name;
        resetHP();
        resetMove();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
