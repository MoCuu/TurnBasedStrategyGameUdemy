using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Before Start is called
    private void Awake()
    {
        instance = this;
    }

    public CharacterController activePlayer;

    public List<CharacterController> allChars = new List<CharacterController>();
    public List<CharacterController> playerTeam = new List<CharacterController>(), enemyTeam = new List<CharacterController>();

    private int currentChar;

    public int totalActionPoints = 2;
    [HideInInspector]
    public int actionPointsRemaining;

    public int currentActionCost = 1;

    public GameObject targetDisplay;

    public bool shouldSpawnAtRandomPoints;
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public List<Transform> enemySpawnPoints = new List<Transform>();

    public bool matchEnded;

    public string levelToLoad;
    public string nextLevel;

    // Start is called before the first frame update
    void Start()
    {
        List<CharacterController> tempList = new List<CharacterController>();

        tempList.AddRange(FindObjectsOfType<CharacterController>());

        int iterations = tempList.Count + 50; // overlooping security, DO NOT REMOVE!
        while (tempList.Count > 0 && iterations > 0)
        {
            int randomPick = Random.Range(0, tempList.Count);
            allChars.Add(tempList[randomPick]);

            tempList.RemoveAt(randomPick);

            iterations--; // overlooping security, DO NOT REMOVE!
        }

        foreach (CharacterController cc in allChars)
        {
            if (cc.isEnemy == false)
            {
                playerTeam.Add(cc);
            } else
            {
                enemyTeam.Add(cc);
            }
        }

        allChars.Clear();
        int playerCount = playerTeam.Count-1;
        int enemyCount = enemyTeam.Count-1;
        while (playerCount >= 0 && enemyCount >= 0)
        {
            if (playerCount >= 0)
            {
                allChars.Add(playerTeam[playerCount]);
                playerCount--;
            }
            if (enemyCount >= 0)
            {
                allChars.Add(enemyTeam[enemyCount]);
                enemyCount--;
            }
        }

        /*if (Random.value > .5f)
        {
            allChars.AddRange(playerTeam);
            allChars.AddRange(enemyTeam);
        } else
        {
            allChars.AddRange(enemyTeam);
            allChars.AddRange(playerTeam);
        }*/
        


        activePlayer = allChars[0];

        if (shouldSpawnAtRandomPoints)
        {
            foreach (CharacterController cc in playerTeam)
            {
                if (playerSpawnPoints.Count > 0)
                {
                    int pos = Random.Range(0, playerSpawnPoints.Count);

                    cc.transform.position = playerSpawnPoints[pos].position;
                    playerSpawnPoints.RemoveAt(pos);
                }
            }

            foreach (CharacterController cc in enemyTeam)
            {
                if (enemySpawnPoints.Count > 0)
                {
                    int pos = Random.Range(0, enemySpawnPoints.Count);

                    cc.transform.position = enemySpawnPoints[pos].position;
                    enemySpawnPoints.RemoveAt(pos);
                }
            }
        }

        CameraController.instance.SetMoveTarget(activePlayer.transform.position);

        currentChar = -1;
        EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FinishedMovement()
    {
        SpendActionPoint();
    }

    public void SpendActionPoint()
    {
        actionPointsRemaining -= currentActionCost;

        CheckForVictory();

        if (matchEnded == false)
        {

            if (actionPointsRemaining <= 0)
            {
                EndTurn();
            }
            else
            {
                if (activePlayer.isEnemy == false)
                {
                    //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

                    PlayerInputMenu.instance.ShowInputMenu();
                }
                else
                {
                    PlayerInputMenu.instance.HideMenus();

                    activePlayer.brain.ChooseAction();
                }
            }
        }
        PlayerInputMenu.instance.UpdateActionPointText(actionPointsRemaining);
    }

    public void EndTurn()
    {
        CheckForVictory();

        if (matchEnded == false)
        {

            currentChar++;

            if (currentChar >= allChars.Count)
            {
                currentChar = 0;
            }

            activePlayer = allChars[currentChar];

            CameraController.instance.SetMoveTarget(activePlayer.transform.position);

            actionPointsRemaining = totalActionPoints;

            if (activePlayer.isEnemy == false)
            {
                //MoveGrid.instance.ShowPointsInRange(activePlayer.moveRange, activePlayer.transform.position);

                PlayerInputMenu.instance.ShowInputMenu();
                PlayerInputMenu.instance.actionPointText.gameObject.SetActive(true);
            }
            else
            {
                PlayerInputMenu.instance.HideMenus();
                PlayerInputMenu.instance.actionPointText.gameObject.SetActive(false);

                //StartCoroutine(AISkipCo());
                activePlayer.brain.ChooseAction();
            }

            currentActionCost = 1;

            PlayerInputMenu.instance.UpdateActionPointText(actionPointsRemaining);
        

            activePlayer.SetDefending(false);
        }
    }

    public IEnumerator AISkipCo()
    {
        yield return new WaitForSeconds(1f);
        EndTurn();
    }

    public void CheckForVictory()
    {
        bool allDead = true;

        foreach (CharacterController cc in playerTeam)
        {
            if (cc.currentHealth > 0)
            {
                allDead = false;
            }
        }

        if (allDead)
        {
            PlayerLoses();

        } else
        {
            allDead = true;
            foreach (CharacterController cc in enemyTeam)
            {
                if (cc.currentHealth > 0)
                {
                    allDead = false;
                }
            }

            if (allDead)
            {
                PlayerWins();
            }
        }
    }

    public void PlayerWins()
    {
        Debug.Log("Victory!");

        matchEnded = true;

        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);
        PlayerInputMenu.instance.resultText.text = "Victory!";

        PlayerInputMenu.instance.nextMissionButton.SetActive(true);

        PlayerInputMenu.instance.actionPointText.gameObject.SetActive(false);
    }

    public void PlayerLoses()
    {
        Debug.Log("Defeat!");

        matchEnded = true;

        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);
        PlayerInputMenu.instance.resultText.text = "Defeat!";

        PlayerInputMenu.instance.endBattleButton.SetActive(true);

        PlayerInputMenu.instance.actionPointText.gameObject.SetActive(false);
    }

    public void LeaveBattle()
    {
        SceneManager.LoadScene(levelToLoad);
    }

    public void NextMission()
    {
        SceneManager.LoadScene(nextLevel);
    }
}
