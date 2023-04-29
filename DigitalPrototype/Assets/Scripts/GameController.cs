using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GameController : MonoBehaviour
{
    // we need a pointer to both Player and Enemy controller. Must be connected via unity editor. 
    public GameObject playerControllerObj = null;
    private PlayerController playerController = null;
    public GameObject enemyControllerObj = null;
    private EnemyController enemyController = null;
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public GameObject Mapmode = null;
    public GameObject Battlemode = null;
    public GameObject leftHPUI = null;
    public GameObject rightHPUI = null;

    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { PlayerTurn, EnemyTurn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    // Must be connected via unity editor
    public GameObject turnPanel = null;
    public TMPro.TMP_Text turnModeTXT = null;
    public Button endTurnButton = null;

    // Must be connected via unity editor
    public Grid currGrid = null;
    public Tilemap currTilemap = null;
    public Tile hoverTile = null;

    // defaulted to this so the hover doesnt overwrite an existing tile on first deselection
    private Vector3Int previousMousePos = new Vector3Int(0, 0, -999);

    // stuff for battlemode
    private static Vector3 leftBattlePos = new Vector3(-2, 0, -1);
    private static Vector3 rightBattlePos = new Vector3(2, 0, -1);
    private Vector3 camBattlePos = new Vector3(0, 0.5f, -50);
    private float camBattleSize = 2;
    private Quaternion leftBattleQua = new Quaternion();
    private Quaternion rightBattleQua = new Quaternion(0, 180, 0, 1);
    private Vector3 savedPosLeft;
    private Vector3 savedPosRight;
    private Vector3 savedPosCam;
    private Quaternion savedQuaLeft;
    private Quaternion savedQuaRight;
    private float savedCamSize;
    private enum doubleAttack { neitherDouble, leftDoubles, rightDoubles };
    private int doubleRequirement = 4;
    private float inbetweenAttackDelay = 1.5f;
    private bool attackOrDefend;
    bool theKobeWaitBool;
    //Target Position for the orbs to move too in battle
    public Vector3 leftTarget = leftBattlePos + new Vector3(0.1f,0f,0f);
    public Vector3 rightKobeTarget = rightBattlePos + new Vector3(-0.1f,0f,0f);
    //Duration for Animation
    public float KobeDuration;

    //

    // set these ones 
    public float tileX = 1;
    public float tileY = 1;

    // world limit is limit camera should be movable
    float worldLimX = 10f;
    float worldLimY = 5f;
    float camMoveAmount = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        playerController = playerControllerObj.GetComponent<PlayerController>();
        enemyController = enemyControllerObj.GetComponent<EnemyController>();
        mainCamera = mainCameraObj.GetComponent<Camera>();

        changeTurn(turnMode.PlayerTurn);
        changeMode(gameMode.MapMode);

        updateTurnText();
    }

    // Update is called once per frame
    void Update()
    {
        // Map mode only
        if (currGameMode == gameMode.MapMode)
        {
            // camera move up
            if (Input.GetKey(KeyCode.W) && mainCamera.transform.position.y < worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + camMoveAmount, mainCamera.transform.position.z); 
            }
            // camera move down
            if (Input.GetKey(KeyCode.S) && mainCamera.transform.position.y > -worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move left
            if (Input.GetKey(KeyCode.A) && mainCamera.transform.position.x > -worldLimX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            // camera move right
            if (Input.GetKey(KeyCode.D) && mainCamera.transform.position.x < worldLimX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }

            // mouse hover over effect
            Vector3Int mousePos = GetMousePosition();

            // adjust pos to z -5, see below
            mousePos = new Vector3Int(mousePos.x, mousePos.y, -6);

            if (!mousePos.Equals(previousMousePos))
            {
                // select layer is z: -6 (cause I randomly decided)
                Vector3Int mousePosZHover = new Vector3Int(mousePos.x, mousePos.y, -6);
                currTilemap.SetTile(previousMousePos, null); // Remove old hoverTile
                currTilemap.SetTile(mousePosZHover, hoverTile);
                previousMousePos = mousePos;
            }
        }
    }

    // playerSide = FALSE, player is on the left
    // playerSide = TRUE, player is on the right
    // playerTurn = False, this was not a playerTurn battle
    // playerTurn = true, this was a playerTurn battle

    // playerSide and Turn in conjunction tell who should strike first (whoevers turn it is ie. playerTurn attack means player attacks first)
    // if the attack occurs on a playerTurn they need control returned to them as well
    public IEnumerator startBattle(GameObject leftChar, GameObject rightChar, bool playerSide, bool playerTurn)
    {
        Debug.Log("starting battle");

        Character leftStats = leftChar.GetComponent<Character>();
        Character rightStats = rightChar.GetComponent<Character>();

        // go to battlemode
        turnPanel.SetActive(false);
        playerController.deselectTarget();
        playerController.deactivateChildren();
        enemyController.deactivateChildren();
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        savedCamSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize = camBattleSize;
        leftHPUI.SetActive(true);
        rightHPUI.SetActive(true);
        updateBattleHP(leftStats, rightStats);
        //

        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);

        // save position and rotation for both participants (and camera) before we move them
        savedPosLeft = leftChar.transform.position;
        savedPosRight = rightChar.transform.position;
        savedPosCam = mainCamera.transform.position;
        savedQuaLeft = leftChar.transform.rotation;
        savedQuaRight = rightChar.transform.rotation;
        //

        // move both participants (and camera) to position for battle
        leftChar.transform.position = leftBattlePos;
        rightChar.transform.position = rightBattlePos;
        mainCamera.transform.position = camBattlePos;
        leftChar.transform.rotation = leftBattleQua;
        rightChar.transform.rotation = rightBattleQua;
        //

        // delay for 1.5s so user can see before battle starts
        yield return new WaitForSeconds(2f);

        // figure out if one battler is double attacking or not
        doubleAttack whoDoubles;
        if (leftStats.SPD >= rightStats.SPD + doubleRequirement)
        {
            whoDoubles = doubleAttack.leftDoubles;
        }
        else if (rightStats.SPD >= leftStats.SPD + doubleRequirement)
        {
            whoDoubles = doubleAttack.rightDoubles;
        }
        else
            whoDoubles = doubleAttack.neitherDouble;


        // player initiated battle, they attack first
        if (playerTurn == true)
        {
            // player is on left
            if (playerSide == false)
            {
                // player attack
                Attack(leftStats, rightStats);
                updateBattleHP(leftStats, rightStats);

                // start animation coroutine
                StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.5f));
                yield return new WaitForSeconds(.5f);

                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // enemy attack
                Attack(rightStats, leftStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(rightChar,leftTarget, .5f));
                yield return new WaitForSeconds(.5f);


                if (whoDoubles == doubleAttack.leftDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(leftStats, rightStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.5f));
                    yield return new WaitForSeconds(.5f);
    
                }

                else if (whoDoubles == doubleAttack.rightDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(rightStats, leftStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(rightChar,leftTarget,.5f));
                    yield return new WaitForSeconds(.5f);

                }

            }
            // player is on right
            else
            {
                // player attack
                Attack(rightStats, leftStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                yield return new WaitForSeconds(.5f);

                
                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // enemy attack
                Attack(leftStats, rightStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                yield return new WaitForSeconds(.5f);



                if (whoDoubles == doubleAttack.leftDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(leftStats, rightStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                    yield return new WaitForSeconds(.5f);



                }

                else if (whoDoubles == doubleAttack.rightDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(rightStats, leftStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                    yield return new WaitForSeconds(.5f);

                    
                }
            }
        }

        // enemy initiated battle, they attack first
        else
        {
            // enemy is on left
            if (playerSide == true)
            {
                // enemy attack
                Attack(leftStats, rightStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                yield return new WaitForSeconds(.5f);



                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // player attack
                Attack(rightStats, leftStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                yield return new WaitForSeconds(.5f);



                if (whoDoubles == doubleAttack.leftDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(leftStats, rightStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                    yield return new WaitForSeconds(.5f);



                }

                else if (whoDoubles == doubleAttack.rightDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(rightStats, leftStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                    yield return new WaitForSeconds(.5f);



                }
            }
            // enemy is on right
            else
            {
                // enemy attack
                Attack(rightStats, leftStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                yield return new WaitForSeconds(.5f);




                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // player attack
                Attack(leftStats, rightStats);
                updateBattleHP(leftStats, rightStats);
                StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                yield return new WaitForSeconds(.5f);




                if (whoDoubles == doubleAttack.leftDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(leftStats, rightStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(leftChar,rightKobeTarget,.25f));
                    yield return new WaitForSeconds(.5f);

                }

                else if (whoDoubles == doubleAttack.rightDoubles)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    Attack(rightStats, leftStats);
                    updateBattleHP(leftStats, rightStats);
                    StartCoroutine(LerpPosition(rightChar,leftTarget,.25f));
                    yield return new WaitForSeconds(.5f);


                }
            }
        }

        // delay again for 1.5s so user can see result of battle before leaving battlemode
        yield return new WaitForSeconds(2f);

        // return them to prior positions
        leftChar.transform.position = savedPosLeft;
        rightChar.transform.position = savedPosRight;
        mainCamera.transform.position = savedPosCam;
        mainCamera.orthographicSize = savedCamSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        //

        // return to mapmode
        leftHPUI.SetActive(false);
        rightHPUI.SetActive(false);
        turnPanel.SetActive(true);
        playerController.activateChildren();
        enemyController.activateChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
        changeMode(gameMode.MapMode);
        //

        // return to either player or enemy turn
        if (playerTurn == true)
            playerController.ourTurn = true;
        else
            playerController.ourTurn = false;
    }
    

    public IEnumerator waitTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    public IEnumerator LerpPosition(GameObject theObject, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector2 startPosition = theObject.transform.position;
        while (time < duration )
        {
            theObject.transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        
        theObject.transform.position = targetPosition;
        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(targetPosition, startPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
 
        }
     

    }
    

    public void Attack(Character attacker, Character damageTaker)
    {
        if (attacker.isDead == true || damageTaker.isDead == true)
            return;

        // if attacker has a sword
        if (attacker.weapon == 1)
        {
            int damageMinusDefense = attacker.STR - damageTaker.DEF;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;
            
            damageTaker.takeDamage(damageMinusDefense);
        }
    }

    public void updateBattleHP(Character leftStats, Character rightStats)
    {
        leftHPUI.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "HP: " + leftStats.hpLeft + " / " + leftStats.HP;
        rightHPUI.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "HP: " + rightStats.hpLeft + " / " + rightStats.HP;
    }

    // for hovering effect
    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    public void changeTurn(turnMode newTurn)
    {
        turnMode prevTurnMode = currTurnMode;
        currTurnMode = newTurn;

        // make sure to update turn text as well
        updateTurnText();

        if (currTurnMode == newTurn)
        {
            Debug.Log("turnMode changed from " + prevTurnMode + " to " + newTurn);
           
            // if player turn
            if (currTurnMode == turnMode.PlayerTurn)
            {
                playerController.resetAllMove();
                playerController.ourTurn = true;

                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
            }
            // if enemy turn
            else
            {
                playerController.ourTurn = false;

                // turn off end turn button for player since it isnt their turn
                endTurnButton.gameObject.SetActive(false);

                enemyController.enemyTurn();
            }
        }
        else
        {
            Debug.Log("!!! Failed to change turnMode from " + prevTurnMode + " to " + newTurn);
            
        }
    }

    public void changeMode(gameMode newMode)
    {
        gameMode prevGameMode = currGameMode; 
        currGameMode = newMode;

        if (currGameMode == newMode)
        {
            Debug.Log("gameMode changed from " + prevGameMode + " to " + newMode);
        }
        else
        {
            Debug.Log("!!! Failed to change gameMode from " + prevGameMode + " to " + newMode);
        }
    }

    public void updateTurnText()
    {
        if (currTurnMode == turnMode.PlayerTurn)
        {
            turnModeTXT.text = "Player Turn";    
        }
        else 
        {
            turnModeTXT.text = "Enemy Turn";
        }
    }

    public void endTurnButtonPressed()
    {
        if (currTurnMode == turnMode.PlayerTurn)
            changeTurn(turnMode.EnemyTurn);
        else
            Debug.Log("!!! The end turn button was pressed BUT it isnt currently the players turn");
    }
}
