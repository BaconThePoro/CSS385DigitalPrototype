using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public string charName = "blankName";
    public int STR = 1;
    public int MAG = 1;
    public int DEF = 1;
    public int RES = 1;
    public int SPD = 1;
    public int MOV = 1; 

    public int movLeft;

    public void resetMove()
    {
        movLeft = MOV;
    }

    // Start is called before the first frame update
    void Start()
    {       
        charName = gameObject.name;
        resetMove();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
