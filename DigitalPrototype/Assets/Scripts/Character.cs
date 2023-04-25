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

    public void takeDamage(int amount)
    {
        hpLeft = hpLeft - amount; 
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
