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
    public GameObject charInfoPanelL = null;
    public GameObject charInfoPanelR = null;
    public GameObject leftDamageUI = null;
    public GameObject rightDamageUI = null;
    private TMPro.TMP_Text leftDamageTXT = null;
    private TMPro.TMP_Text rightDamageTXT = null;
    public GameObject VictoryScreen = null;
    public GameObject DefeatScreen = null;

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
    private Vector3 leftBattlePos1 = new Vector3(-1.5f, 0, -1);
    private Vector3 rightBattlePos1 = new Vector3(1.5f, 0, -1);
    private Vector3 leftBattlePos2 = new Vector3(-3, 0, -1);
    private Vector3 rightBattlePos2 = new Vector3(3, 0, -1);
    private Vector3 leftTarget = new Vector3(0f, 0, -1);
    private Vector3 rightTarget = new Vector3(0f, 0, -1);
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
    private float inbetweenAttackDelay = 0.5f;
    private float animationDuration = 0.25f;

    // panel stuff
    private GameObject LmovLeftTXT = null;
    private GameObject LmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI LcharNameTXT = null;
    private TMPro.TextMeshProUGUI LhpNUM = null;
    private TMPro.TextMeshProUGUI LstrNUM = null;
    private TMPro.TextMeshProUGUI LmagNUM = null;
    private TMPro.TextMeshProUGUI LspdNUM = null;
    private TMPro.TextMeshProUGUI LdefNUM = null;
    private TMPro.TextMeshProUGUI LresNUM = null;
    private TMPro.TextMeshProUGUI LmovNUM = null;
    private TMPro.TextMeshProUGUI LmovLeftNUM = null;
    //
    private GameObject RmovLeftTXT = null;
    private GameObject RmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI RcharNameTXT = null;
    private TMPro.TextMeshProUGUI RhpNUM = null;
    private TMPro.TextMeshProUGUI RstrNUM = null;
    private TMPro.TextMeshProUGUI RmagNUM = null;
    private TMPro.TextMeshProUGUI RspdNUM = null;
    private TMPro.TextMeshProUGUI RdefNUM = null;
    private TMPro.TextMeshProUGUI RresNUM = null;
    private TMPro.TextMeshProUGUI RmovNUM = null;
    private TMPro.TextMeshProUGUI RmovLeftNUM = null;
    //

    // set these ones 
    public float tileX = 1;
    public float tileY = 1;

    // world limit is limit camera should be movable
    float worldLimX = 18f;
    float worldLimY = 11f;
    float camMoveAmount = 0.02f;
    float panBorderThickness = 30f;


    // Start is called before the first frame update
    void Start()
    {
        playerController = playerControllerObj.GetComponent<PlayerController>();
        enemyController = enemyControllerObj.GetComponent<EnemyController>();
        mainCamera = mainCameraObj.GetComponent<Camera>();
        leftDamageTXT = leftDamageUI.GetComponent<TMPro.TextMeshProUGUI>();
        rightDamageTXT = rightDamageUI.GetComponent<TMPro.TextMeshProUGUI>();

        LcharNameTXT = charInfoPanelL.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftTXT = charInfoPanelL.transform.GetChild(9).gameObject;
        LhpNUM = charInfoPanelL.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        LstrNUM = charInfoPanelL.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        LmagNUM = charInfoPanelL.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        LspdNUM = charInfoPanelL.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        LdefNUM = charInfoPanelL.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        LresNUM = charInfoPanelL.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        LmovNUM = charInfoPanelL.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftNUMObj = charInfoPanelL.transform.GetChild(17).gameObject;
        LmovLeftNUM = LmovLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();

        RcharNameTXT = charInfoPanelR.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftTXT = charInfoPanelR.transform.GetChild(9).gameObject;
        RhpNUM = charInfoPanelR.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        RstrNUM = charInfoPanelR.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        RmagNUM = charInfoPanelR.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        RspdNUM = charInfoPanelR.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        RdefNUM = charInfoPanelR.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        RresNUM = charInfoPanelR.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        RmovNUM = charInfoPanelR.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftNUMObj = charInfoPanelR.transform.GetChild(17).gameObject;
        RmovLeftNUM = RmovLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();

        changeTurn(turnMode.PlayerTurn);
        changeMode(gameMode.MapMode);

        updateTurnText();

    }

    // Update is called once per frame
    void Update()
    {
        if (currGameMode == gameMode.BattleMode)
        {
            // if user left clicks during battle
            if (Input.GetMouseButtonDown(0))
            {
                // set delay to 0 (fast mode)
                inbetweenAttackDelay = 0.1f;
            }
        }

        if (currTurnMode == turnMode.EnemyTurn)
        {
            // if user left clicks during enemy turn
            if (Input.GetMouseButtonDown(0))
            {
                // set delay to 0 (fast mode)
                enemyController.inBetweenDelay = 0.1f;
                inbetweenAttackDelay = 0.1f;
            }
        }

        // Map mode only
        if (currGameMode == gameMode.MapMode)
        {
            // camera move up
            if (Input.GetKey(KeyCode.W) && mainCamera.transform.position.y < worldLimY 
                || Input.mousePosition.y >= Screen.height - panBorderThickness && mainCamera.transform.position.y < worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + camMoveAmount, mainCamera.transform.position.z); 
            }
            // camera move down
            if (Input.GetKey(KeyCode.S) && mainCamera.transform.position.y > -worldLimY
                || Input.mousePosition.y <= panBorderThickness && mainCamera.transform.position.y > -worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move left
            if (Input.GetKey(KeyCode.A) && mainCamera.transform.position.x > -worldLimX
                || Input.mousePosition.x <= panBorderThickness && mainCamera.transform.position.x > -worldLimX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            // camera move right
            if (Input.GetKey(KeyCode.D) && mainCamera.transform.position.x < worldLimX
                || Input.mousePosition.x >= Screen.width - panBorderThickness && mainCamera.transform.position.x < worldLimY)
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

            // if user presses right click, end turn
            if (Input.GetMouseButtonDown(1) && currTurnMode == turnMode.PlayerTurn)
            {
                endTurnButtonPressed();
            }
        }
    }


    // playerSide = FALSE, player is on the left
    // playerSide = TRUE, player is on the right
    // playerTurn = False, this was not a playerTurn battle
    // playerTurn = true, this was a playerTurn battle

    // playerSide and Turn in conjunction tell who should strike first (whoevers turn it is ie. playerTurn attack means player attacks first)
    // if the attack occurs on a playerTurn they need control returned to them as well
    public IEnumerator startBattle(GameObject leftChar, GameObject rightChar, bool playerSide, bool playerTurn, int battleRange)
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
        charInfoPanelL.SetActive(true);
        charInfoPanelR.SetActive(true);
        leftDamageUI.SetActive(true);
        rightDamageUI.SetActive(true);
        updateBattleStats(leftStats, rightStats);
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
        if (battleRange == 1)
        {
            leftChar.transform.position = leftBattlePos1;
            rightChar.transform.position = rightBattlePos1;
        }
        else if (battleRange == 2)
        {
            leftChar.transform.position = leftBattlePos2;
            rightChar.transform.position = rightBattlePos2;
        }

        mainCamera.transform.position = camBattlePos;
        leftChar.transform.rotation = leftBattleQua;
        rightChar.transform.rotation = rightBattleQua;
        //

        // delay for 1.5s so user can see before battle starts
        yield return new WaitForSeconds(inbetweenAttackDelay * 3);

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
                StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                yield return new WaitForSeconds(.5f);
                StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                updateBattleStats(leftStats, rightStats);
                
                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // enemy attack
                if (rightStats.attackRange == battleRange)
                {
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                if (whoDoubles == doubleAttack.leftDoubles && leftStats.isDead == false && rightStats.isDead == false)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                   
                    // player doubles
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                else if (whoDoubles == doubleAttack.rightDoubles && leftStats.isDead == false 
                    && rightStats.isDead == false && rightStats.attackRange == battleRange)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    
                    // enemy doubles
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
                }

            }
            // player is on right
            else
            {
                // player attack             
                StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                yield return new WaitForSeconds(.5f);
                StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                updateBattleStats(leftStats, rightStats);

                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // enemy attack
                if (leftStats.attackRange == battleRange)
                {
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                if (whoDoubles == doubleAttack.leftDoubles && leftStats.isDead == false 
                    && rightStats.isDead == false && leftStats.attackRange == battleRange)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    
                    // enemy doubles
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                else if (whoDoubles == doubleAttack.rightDoubles && leftStats.isDead == false && rightStats.isDead == false)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                   
                    // player doubles
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
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
                StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                yield return new WaitForSeconds(.5f);
                StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                updateBattleStats(leftStats, rightStats);

                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // player attack
                if (rightStats.attackRange == battleRange)
                {
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
                }


                if (whoDoubles == doubleAttack.leftDoubles && leftStats.isDead == false && rightStats.isDead == false)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                   
                    // enemy doubles
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                else if (whoDoubles == doubleAttack.rightDoubles && leftStats.isDead == false
                    && rightStats.isDead == false && rightStats.attackRange == battleRange)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);  
                    
                    // player doubles
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
                }
            }
            // enemy is on right
            else
            {
                // enemy attack               
                StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                yield return new WaitForSeconds(.5f);
                StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                updateBattleStats(leftStats, rightStats);

                // delay
                yield return new WaitForSeconds(inbetweenAttackDelay);

                // player attack
                if (leftStats.attackRange == battleRange)
                {
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }


                if (whoDoubles == doubleAttack.leftDoubles && leftStats.isDead == false 
                    && rightStats.isDead == false && leftStats.attackRange == battleRange)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                   
                    // enemy doubles
                    StartCoroutine(LerpPosition(leftChar, leftTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(true, Attack(leftStats, rightStats)));
                    updateBattleStats(leftStats, rightStats);
                }

                else if (whoDoubles == doubleAttack.rightDoubles && leftStats.isDead == false && rightStats.isDead == false)
                {
                    // delay
                    yield return new WaitForSeconds(inbetweenAttackDelay);
                    
                    // player doubles
                    StartCoroutine(LerpPosition(rightChar, rightTarget, animationDuration));
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(damageTXT(false, Attack(rightStats, leftStats)));
                    updateBattleStats(leftStats, rightStats);
                }
            }
        }

        // delay again for 1.5s so user can see result of battle before leaving battlemode
        yield return new WaitForSeconds(inbetweenAttackDelay * 4);

        // reset inbetweenAttackDelay in case user skipped battle
        if (playerTurn == true)
        {
            resetDelay();
        }
        
        // return them to prior positions
        leftChar.transform.position = savedPosLeft;
        rightChar.transform.position = savedPosRight;
        mainCamera.transform.position = savedPosCam;
        mainCamera.orthographicSize = savedCamSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        //

        // return to mapmode
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        leftDamageTXT.text = "";
        rightDamageTXT.text = "";
        leftDamageUI.SetActive(false);
        rightDamageUI.SetActive(false);
        turnPanel.SetActive(true);
        playerController.activateChildren();
        enemyController.activateChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
        changeMode(gameMode.MapMode);
        //

        // return to either player or enemy turn
        if (playerTurn == true)
        {
            playerController.ourTurn = true;
            changeTurn(turnMode.PlayerTurn);
        }
        else
        {
            playerController.ourTurn = false;
        }
    }

    public void resetDelay()
    {
        inbetweenAttackDelay = 0.5f;
    }
    
    // false == left hurt, true == right hurt
    public IEnumerator damageTXT(bool side, int damageNum)
    {
        // dead char attack number
        if (damageNum == -999)
            yield return null;
        else
        {
            if (!side)
            {
                leftDamageTXT.text = "(-" + damageNum + ")";
            }
            else
            {
                rightDamageTXT.text = "(-" + damageNum + ")";
            }

            yield return new WaitForSeconds(0.75f);

            leftDamageTXT.text = "";
            rightDamageTXT.text = "";
        }
    }

    public IEnumerator waitTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    public IEnumerator LerpPosition(GameObject theObject, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector2 startPosition = theObject.transform.position;
        while (time < duration)
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

        theObject.transform.position = startPosition;
    }
    

    public int Attack(Character attacker, Character damageTaker)
    {
        if (attacker.isDead == true || damageTaker.isDead == true)
            return -999;

        int damageMinusDefense = -1;
        // if attacker has a sword
        if (attacker.weapon == 1 || attacker.weapon == 2)
        {
            damageMinusDefense = attacker.STR - damageTaker.DEF;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;
            
            damageTaker.takeDamage(damageMinusDefense);
        }

        // all player characters dead
        if (playerController.allDead())
        {
            Debug.Log("All allies dead you lose");
            StartCoroutine(defeat());
        }
        // all enemy characters dead
        else if (enemyController.allDead())
        {
            Debug.Log("All enemies dead you win");
            StartCoroutine(victory());
        }

        return damageMinusDefense;
    }

    public IEnumerator victory()
    {
        yield return new WaitForSeconds(2);
        VictoryScreen.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        leftDamageUI.SetActive(false);
        rightDamageUI.SetActive(false);
        playerControllerObj.SetActive(false);
        enemyControllerObj.SetActive(false);
    }

    public IEnumerator defeat()
    {
        yield return new WaitForSeconds(2);
        DefeatScreen.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        leftDamageUI.SetActive(false);
        rightDamageUI.SetActive(false);
        playerControllerObj.SetActive(false);
        enemyControllerObj.SetActive(false);
    }

    public void updateBattleStats(Character leftStats, Character rightStats)
    {
        LcharNameTXT.text = "Name: " + leftStats.charName;
        LhpNUM.text = "" + leftStats.hpLeft + " / " + leftStats.HP;
        LstrNUM.text = "" + leftStats.STR;
        LmagNUM.text = "" + leftStats.MAG;
        LdefNUM.text = "" + leftStats.DEF;
        LresNUM.text = "" + leftStats.RES;
        LspdNUM.text = "" + leftStats.SPD;
        LmovNUM.text = "" + leftStats.MOV;
        LmovLeftTXT.SetActive(false);
        LmovLeftNUMObj.SetActive(false);

        RcharNameTXT.text = "Name: " + rightStats.charName;
        RhpNUM.text = "" + rightStats.hpLeft + " / " + rightStats.HP;
        RstrNUM.text = "" + rightStats.STR;
        RmagNUM.text = "" + rightStats.MAG;
        RdefNUM.text = "" + rightStats.DEF;
        RresNUM.text = "" + rightStats.RES;
        RspdNUM.text = "" + rightStats.SPD;
        RmovNUM.text = "" + rightStats.MOV;
        RmovLeftTXT.SetActive(false);
        RmovLeftNUMObj.SetActive(false);
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
                if (playerController.ourTurn == false)
                {
                    playerController.resetAllMove();
                    playerController.ourTurn = true;
                }

                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
            }
            // if enemy turn
            else
            {
                playerController.ourTurn = false;
                playerController.deselectTarget();

                // turn off end turn button for player since it isnt their turn
                endTurnButton.gameObject.SetActive(false);

                enemyController.resetAllMove();
                StartCoroutine(enemyController.enemyTurn());
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
