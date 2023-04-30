using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    // must be connected via unity editor
    public GameObject gameControllerObj = null;
    private GameController gameController = null;

    public GameObject[] enemyUnits;
    public Character[] enemyStats;

    public Tilemap currTilemap = null;
    // dont set in here, they grab value from GameController
    private float tileX = 0;
    private float tileY = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObj.GetComponent<GameController>();
        tileX = gameController.tileX;
        tileY = gameController.tileY;

        // get a handle on each child for EnemyController
        enemyUnits = new GameObject[transform.childCount];
        enemyStats = new Character[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            enemyUnits[i] = child.gameObject;
            enemyStats[i] = enemyUnits[i].GetComponent<Character>();
          
            // enemy units go on -5
            Vector3 startPos = new Vector3(-4f, -2f + i, -1f);
            enemyUnits[i].transform.position = startPos;

            i += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool allDead()
    {
        for (int i = 0; i < enemyStats.Length; i++)
        {
            if (enemyStats[i].isDead == false)
                return false; 
        }

        return true; 
    }

    public void enemyTurn()
    {
        Debug.Log("Enemy Turn start");            
        // do stuff
        //
        //

        // end turn whenever were finished
        //Debug.Log("Enemy turn end");
        //gameController.GetComponent<GameController>().changeTurn(GameController.turnMode.PlayerTurn);
        StartCoroutine(waitCoroutine());
    }

    public bool enemyHere(Vector3Int pos)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (enemyUnits[i].transform.position == pos && enemyStats[i].isDead == false)
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator waitCoroutine()
    {
        yield return new WaitForSeconds(1);
        gameController.changeTurn(GameController.turnMode.PlayerTurn);
        Debug.Log("Enemy turn end");
    }

    public void deactivateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            enemyUnits[i].gameObject.SetActive(false);
        }
    }

    public void activateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (enemyUnits[i].GetComponent<Character>().isDead == false)
                enemyUnits[i].gameObject.SetActive(true);
        }
    }

    public void comeBackToLife()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            enemyStats[i].isDead = false;
        }
    }
}
