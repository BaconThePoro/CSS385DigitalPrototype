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
    public GameObject playerControllerObj = null;
    private PlayerController playerController = null;

    public GameObject[] enemyUnits;
    public Character[] enemyStats;

    public Tilemap currTilemap = null;
    // dont set in here, they grab value from GameController
    private float tileX = 0;
    private float tileY = 0;

    public float inBetweenDelay = .25f;
    private int aggroRange = 5;
    public bool battleDone = false; 

    private enum direction { left, right, up, down };

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObj.GetComponent<GameController>();
        playerController = playerControllerObj.GetComponent<PlayerController>();
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
            Vector3 startPos = new Vector3(-4f, -2f + i, 0f);
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

    public IEnumerator enemyTurn()
    {
        Debug.Log("Enemy Turn start");

        // find our target (whoever is closest)
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            GameObject target = null;
            Vector3 targetVector = Vector3.zero;
            float targetDistance = 999;

            for (int j = 0; j < playerController.playerUnits.Length; j++)
            {
                Vector3 distanceVector = playerController.playerUnits[j].transform.position - enemyUnits[i].transform.position;
                
                if ( distanceVector.magnitude < targetDistance)
                {
                    target = playerController.playerUnits[j];
                    targetVector = distanceVector;
                    targetDistance = distanceVector.magnitude;
                }
            }

            // if using sword
            if (enemyStats[i].weapon == 1)
            {
                // if in range already
                if ((Mathf.Abs(targetVector.x) == 1 && targetVector.y == 0) || (targetVector.x == 0 && Mathf.Abs(targetVector.y) == 1))
                {
                    // small delay at the start of every units turn
                    yield return new WaitForSeconds(inBetweenDelay * 4);
                    // attack
                    yield return StartCoroutine(beginBattle(i, target));
                }
                else
                {

                    for (enemyStats[i].movLeft = enemyStats[i].movLeft; enemyStats[i].movLeft > 0; enemyStats[i].movLeft--)
                    {
                        Debug.Log("Target Vector: " + targetVector);

                        // small delay at the start of every units turn
                        yield return new WaitForSeconds(inBetweenDelay);

                        // if target is in a straight line
                        if ((Mathf.Abs(targetVector.normalized.x) == 1 || Mathf.Abs(targetVector.normalized.y) == 1) 
                            && playerController.unitHere(Vector3Int.FloorToInt(enemyUnits[i].transform.position + targetVector.normalized)) == false)
                        {
                            enemyUnits[i].transform.position = enemyUnits[i].transform.position + targetVector.normalized;
                        }
                        // target is not in a straight line
                        else
                        {
                            float xMov = 0;
                            float yMov = 0;

                            if (targetVector.x < 0)
                                xMov = -1;
                            else
                                xMov = 1;

                            if (targetVector.y < 0)
                                yMov = -1;
                            else
                                yMov = 1;

                            Vector3 targetXPos = enemyUnits[i].transform.position + new Vector3(xMov, 0, 0);
                            Vector3 targetYPos = enemyUnits[i].transform.position + new Vector3(0, yMov, 0);

                            // try move vertically
                            if (playerController.unitHere(Vector3Int.FloorToInt(targetYPos)) == false)
                            {
                                enemyUnits[i].transform.position = targetYPos;
                            }
                            // try move horizontally
                            else if (playerController.unitHere(Vector3Int.FloorToInt(targetXPos)) == false)
                            {
                                enemyUnits[i].transform.position = targetXPos;
                            }
                            // both sides occupied, cant move
                            else
                            {                       
                                break;
                            }

                        }

                        // recalc target vector since we moved
                        targetVector = target.transform.position - enemyUnits[i].transform.position;

                        // check if in range
                        if ((Mathf.Abs(targetVector.x) == 1 && targetVector.y == 0) || (targetVector.x == 0 && Mathf.Abs(targetVector.y) == 1))
                        {
                            // small delay at the start of every units turn
                            yield return new WaitForSeconds(inBetweenDelay * 4);
                            // attack
                            yield return StartCoroutine(beginBattle(i, target));
                            enemyStats[i].movLeft = 0;
                        }


                    }



                }


            }
            // using bow
            else if (enemyStats[i].weapon == 2)
            {

            }










        }


        // end turn whenever were finished
        //Debug.Log("Enemy turn end");
        //gameController.GetComponent<GameController>().changeTurn(GameController.turnMode.PlayerTurn);
        //yield return StartCoroutine(waitCoroutine());

        yield return new WaitForSeconds(inBetweenDelay);
        resetAllMove();
        gameController.changeTurn(GameController.turnMode.PlayerTurn);
        Debug.Log("Enemy turn end");
    }

    public void resetAllMove()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            enemyStats[i].resetMove();
        }
    }

    IEnumerator beginBattle(int i, GameObject target)
    {
        Debug.Log("battle time");

        gameController.changeMode(GameController.gameMode.BattleMode);

        // figure out which way to face (ally on left or right)
        direction battleDirection = facingWhere(enemyUnits[i].transform.position, target.transform.position);

        // calculate range of this battle
        int battleRange = 0;
        Vector3 distance = target.transform.position - enemyUnits[i].transform.position;
        if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 0)
            || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 1))
        {
            battleRange = 1;
        }
        else if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)
            || (Mathf.Abs(distance.x) == 2 && Mathf.Abs(distance.y) == 0)
            || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 2))
        {
            battleRange = 2;
        }

        // if facing to the right or down then put ally on the left 
        if (battleDirection == direction.right || battleDirection == direction.down)
            yield return StartCoroutine(gameController.startBattle(enemyUnits[i], target, true, false, battleRange));
        // else put ally on the right
        else
            yield return StartCoroutine(gameController.startBattle(target, enemyUnits[i], false, false, battleRange));
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

    direction facingWhere(Vector3 allyPos, Vector3 enemyPos)
    {
        Vector3 difference = enemyPos - allyPos;

        if (difference.x < 0)
            return direction.left;
        else if (difference.x > 0)
            return direction.right;

        if (difference.y < 0)
            return direction.down;
        else if (difference.y > 0)
            return direction.up;

        Debug.Log("facingWhere() is broke, units somehow standing on top of each other");
        return direction.left;
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
