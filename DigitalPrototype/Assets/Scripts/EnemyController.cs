using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    // most public stuff needs to be connected through unity editor
    public GameObject gameControllerObj = null;
    private GameController gameController = null;
    public GameObject playerControllerObj = null;
    private PlayerController playerController = null;
    [System.NonSerialized]
    public GameObject[] enemyUnits;
    [System.NonSerialized]
    public Character[] enemyStats;
    public Vector3[] enemyStartPos;
    public Character.bodyType[] bodysList;
    public Character.weaponType[] weaponsList;
    public Tilemap currTilemap = null;
    public float inBetweenDelay = .75f;
    public bool battleDone = false;
    private int aggroRange = 10;
    private enum direction { left, right, up, down };
    private List<Vector3> OneRangePath = new List<Vector3>();
    private List<Vector3> TwoRangePath = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObj.GetComponent<GameController>();
        playerController = playerControllerObj.GetComponent<PlayerController>();

        // get a handle on each child for EnemyController
        enemyUnits = new GameObject[transform.childCount];
        enemyStats = new Character[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            enemyUnits[i] = child.gameObject;
            enemyStats[i] = enemyUnits[i].GetComponent<Character>();      
            enemyUnits[i].transform.position = enemyStartPos[i];
            enemyStats[i].setEnemy();
            enemyStats[i].changeBody(bodysList[i]);
            enemyStats[i].changeWeapon(weaponsList[i]);

            i += 1;
        }

        OneRangePath.Add(new Vector3(1, 0, 0));
        OneRangePath.Add(new Vector3(-1, 0, 0));
        OneRangePath.Add(new Vector3(0, 1, 0));
        OneRangePath.Add(new Vector3(0, -1, 0));

        resetDelay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool allDead()
    {
        for (int i = 0; i < enemyStats.Length; i++)
        {
            if (enemyStats[i].getIsDead() == false)
                return false; 
        }

        return true; 
    }

    public void resetDelay()
    {
        inBetweenDelay = 0.3f;
        gameController.resetDelay();
    }

    public GameObject findClosestTarget(GameObject us)
    {
        int shortestVectorHaver = 0;
        Vector3 shortestVector = new Vector3(999, 999, 0);

        // for all of playerUnits
        for (int i = 0; i < playerController.playerUnits.Length; i++)
        {
            Vector3 distanceVector = playerController.playerUnits[i].transform.position - us.transform.position;

            if (distanceVector.magnitude < shortestVector.magnitude)
            {
                shortestVector = distanceVector;
                shortestVectorHaver = i;
            }
                
        }

        return playerController.playerUnits[shortestVectorHaver];
    }

    public IEnumerator enemyTurn()
    {
        Debug.Log("Enemy Turn start");
        resetDelay();

        // for every enemy unit
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            GameObject currEnemy = enemyUnits[i];
            Character currEnemyStats = currEnemy.GetComponent<Character>();

            // if their dead skip them
            if (currEnemyStats.getIsDead() == false)
            {

                // turn on target reticle for this unit
                currEnemy.transform.GetChild(0).gameObject.SetActive(true);

                // find target
                GameObject target = findClosestTarget(currEnemy);
                Debug.Log("target is here " + target.transform.position);

                // if in range already
                if (inAttackRange(Vector3Int.FloorToInt(target.transform.position), currEnemy))
                {
                    // small delay at the start of every units turn
                    yield return new WaitForSeconds(inBetweenDelay * 4);
                    // attack
                    currEnemy.transform.GetChild(0).gameObject.SetActive(false);
                    yield return StartCoroutine(beginBattle(i, target));
                    currEnemy.transform.GetChild(0).gameObject.SetActive(true);
                }
                // have to move
                else
                {
                    List<PathNode> bestPath = findBestOverall(currEnemy);

                    if (bestPath != null)
                    {
                        yield return StartCoroutine(movePath(bestPath, currEnemy));
                        playerController.pathfinding.resetCollision();

                        for (int j = 0; j < bestPath.Count; j++)
                            Debug.Log("BestPath: " + bestPath[j]);
                    }
                    else
                        Debug.Log("best path for enemy is null");
                        

                    // try attack at end of move
                    if (inAttackRange(Vector3Int.FloorToInt(target.transform.position), currEnemy))
                    {
                        // small delay at the start of every units turn
                        yield return new WaitForSeconds(inBetweenDelay * 4);
                        // attack
                        currEnemy.transform.GetChild(0).gameObject.SetActive(false);
                        yield return StartCoroutine(beginBattle(i, target));
                        currEnemy.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
            
            yield return new WaitForSeconds(inBetweenDelay * 2);
            // disable target reticle
            enemyUnits[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        
        // end turn whenever were finished
        //Debug.Log("Enemy turn end");
        //gameController.GetComponent<GameController>().changeTurn(GameController.turnMode.PlayerTurn);
        //yield return StartCoroutine(waitCoroutine());

        yield return new WaitForSeconds(inBetweenDelay);
        resetAllMove();
        gameController.changeTurn(GameController.turnMode.PlayerTurn);
        Debug.Log("Enemy turn end");
        playerController.resetAllAttack();
        playerController.resetAllMove();
    }

    public List<PathNode> findBestForOne(GameObject currEnemy, GameObject target, ref int g)
    {
        Character currEnemyStats = currEnemy.GetComponent<Character>();

        //facingWhere(currEnemy.transform.position, target.transform.position);
        int oldGCost = 0;
        int newGCost = 0;

        List<PathNode> vectorPath = new List<PathNode>();
        vectorPath = playerController.pathfinding.FindEnemyPath((int)currEnemy.transform.position.x, (int)currEnemy.transform.position.y,
           (int)target.transform.position.x, (int)target.transform.position.y, currEnemyStats.movLeft, ref oldGCost);

        List<PathNode> bestPath = new List<PathNode>();
        bestPath = vectorPath;

        for (int j = 0; j < OneRangePath.Count; j++)
        {
            Debug.Log("trying target: " + ((int)target.transform.position.x + (int)OneRangePath[j].x) +
                ", " + ((int)target.transform.position.y + (int)OneRangePath[j].y));

            vectorPath = playerController.pathfinding.FindEnemyPath((int)currEnemy.transform.position.x, (int)currEnemy.transform.position.y,
                ((int)target.transform.position.x + (int)OneRangePath[j].x), ((int)target.transform.position.y + (int)OneRangePath[j].y),
                currEnemyStats.movLeft, ref newGCost);

            if (vectorPath != null && newGCost < oldGCost)
            {
                oldGCost = newGCost;
                bestPath = vectorPath;

                for (int k = 0; k < bestPath.Count; k++)
                    Debug.Log("VectorPath[" + k + "]: " + vectorPath[k]);
            }
            else
                Debug.Log("VectorPath: null");
        }

        g = oldGCost; 
        return bestPath;
    }

    public List<PathNode> findBestOverall (GameObject currEnemy)
    {
        int oldG = 99999;
        int newG = 99999;
        List<PathNode> vectorPath = new List<PathNode>();
        List<PathNode> bestPath = new List<PathNode>();

        for (int i = 0; i < playerController.playerUnits.Length; i++)
        {
            vectorPath = findBestForOne(currEnemy, playerController.playerUnits[i], ref newG);

            if (vectorPath != null && newG < oldG)
            {
                oldG = newG;
                bestPath = vectorPath; 
            }
        }

        return bestPath;
    }

    public IEnumerator movePath(List<PathNode> vectorPath, GameObject currEnemy)
    {
        Character currEnemyStats = currEnemy.GetComponent<Character>();
        // stop player from ending turn during movement
        //gameController.changeMode(GameController.gameMode.MenuMode);
        
        // first Node is its own position must remove
        vectorPath.RemoveAt(0);

        for (int i = 0; i < vectorPath.Count; i++)
        {
            if (vectorPath[i].isWalkable && currEnemyStats.movLeft > -99)
            {
                if ((vectorPath[i].x - currEnemy.transform.position.x) == 1)
                    currEnemy.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
                else if ((vectorPath[i].x - currEnemy.transform.position.x) == -1)
                    currEnemy.transform.rotation = new Quaternion(0f, 180f, 0f, 1f);

                currEnemy.transform.position = new Vector3(vectorPath[i].x, vectorPath[i].y, 0);
                currEnemyStats.movLeft--;
                yield return new WaitForSeconds(inBetweenDelay);
            }
            else
                break;
        }


        //gameController.changeMode(GameController.gameMode.MapMode);
    }

    bool inAttackRange(Vector3Int targetPos, GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        // sword
        if (unitStats.GetWeaponType() == Character.weaponType.Sword || unitStats.GetWeaponType() == Character.weaponType.Axe)
        {
            Vector3Int distance = targetPos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 1 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 1))
                return true;
        }
        // bow
        else if (unitStats.GetWeaponType() == Character.weaponType.Bow)
        {
            Vector3Int distance = targetPos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 2 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 2) || (Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1))
                return true;
        }

        return false;
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
            yield return StartCoroutine(gameController.startBattle(enemyUnits[i], target, false, battleRange));
        // else put ally on the right
        else
            yield return StartCoroutine(gameController.startBattle(target, enemyUnits[i], false, battleRange));
    }

    public bool enemyHere(Vector3Int pos)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (enemyUnits[i].transform.position == pos && enemyStats[i].getIsDead() == false)
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
            if (enemyUnits[i].GetComponent<Character>().getIsDead() == false)
                enemyUnits[i].gameObject.SetActive(true);
        }
    }

    public void comeBackToLife()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            enemyStats[i].undie();
        }
    }
}
