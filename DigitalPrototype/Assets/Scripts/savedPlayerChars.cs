using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class savedPlayerChars : MonoBehaviour
{
    private PlayerController playerController; 

    public Character.bodyType[] bodysList;
    public Character.weaponType[] weaponsList;
    public string[] charNames;
    public int[] baseHPs;
    public int[] baseSTRs;
    public int[] baseMAGs;
    public int[] baseDEFs;
    public int[] baseRESs;
    public int[] baseSPDs;
    public int[] baseMOVs;
    public int gearNum; 


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        playerController = GameObject.FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void saveChars()
    {
        gearNum = playerController.getGearNum();
        bodysList = new Character.bodyType[playerController.playerStats.Length];
        weaponsList = new Character.weaponType[playerController.playerStats.Length];
        charNames = new string[playerController.playerStats.Length];
        baseHPs = new int[playerController.playerStats.Length];
        baseSTRs = new int[playerController.playerStats.Length];
        baseMAGs = new int[playerController.playerStats.Length];
        baseDEFs = new int[playerController.playerStats.Length];
        baseRESs = new int[playerController.playerStats.Length];
        baseSPDs = new int[playerController.playerStats.Length];
        baseMOVs = new int[playerController.playerStats.Length];

        for (int i = 0; i < playerController.playerStats.Length; i++)
        {
            Debug.Log("playerStats at " + i);
            bodysList[i] = playerController.playerStats[i].GetBodyType();
            weaponsList[i] = playerController.playerStats[i].GetWeaponType();

            charNames[i] = playerController.playerStats[i].name;
            baseHPs[i] = playerController.playerStats[i].baseHP;
            baseSTRs[i] = playerController.playerStats[i].baseSTR;
            baseMAGs[i] = playerController.playerStats[i].baseMAG;
            baseDEFs[i] = playerController.playerStats[i].baseDEF;
            baseRESs[i] = playerController.playerStats[i].baseRES;
            baseSPDs[i] = playerController.playerStats[i].baseSPD;
            baseMOVs[i] = playerController.playerStats[i].baseMOV;
        }
    }

    public void loadChars()
    {
        playerController = GameObject.FindObjectOfType<PlayerController>();

        playerController.setGearNum(gearNum);

        for (int i = 0; i < bodysList.Length; i++)
        {
            playerController.bodysList[i] = bodysList[i];
        }

        for (int i = 0; i < weaponsList.Length; i++)
        {
            playerController.weaponsList[i] = weaponsList[i];
        }

        for (int i = 0; i < charNames.Length; i++)
        {
            playerController.playerStats[i].changeBody(bodysList[i]);
            playerController.playerStats[i].changeWeapon(weaponsList[i]);
            playerController.playerStats[i].undie();
            playerController.playerStats[i].name = charNames[i];
            playerController.playerStats[i].baseHP = baseHPs[i];
            playerController.playerStats[i].baseSTR = baseSTRs[i];
            playerController.playerStats[i].baseMAG = baseMAGs[i];
            playerController.playerStats[i].baseDEF = baseDEFs[i];
            playerController.playerStats[i].baseRES = baseRESs[i];
            playerController.playerStats[i].baseSPD = baseSPDs[i];
            playerController.playerStats[i].baseMOV = baseMOVs[i];
            playerController.playerStats[i].updateStats();
            playerController.playerStats[i].updateCosts();
            playerController.playerStats[i].resetHP();
            playerController.playerStats[i].resetMove();
        }
    }
}
