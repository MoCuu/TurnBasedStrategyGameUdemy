using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInputMenu : MonoBehaviour
{
    public static PlayerInputMenu instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject inputMenu, moveMenu, meleeMenu, rangedMenu;

    public TMP_Text actionPointText, errorText;

    public float errorDisplayTime = 2f;
    private float errorCounter;

    public TMP_Text hitChanceText;

    public TMP_Text resultText;
    public GameObject endBattleButton, nextMissionButton;

    public void HideMenus()
    {
        inputMenu.SetActive(false);
        moveMenu.SetActive(false);
        meleeMenu.SetActive(false);
        rangedMenu.SetActive(false);
    }

    public void ShowInputMenu()
    {
        inputMenu.SetActive(true);
    }

    public void ShowMoveMenu()
    {
        HideMenus();
        moveMenu.SetActive(true);

        ShowMove();

        SFXManager.instance.UISelect.Play();
    }

    public void HideMoveMenu()
    {
        HideMenus();
        MoveGrid.instance.HideMovePoints();
        ShowInputMenu();

        SFXManager.instance.UICancel.Play();
    }

    public void ShowMove()
    {
        if (GameManager.instance.actionPointsRemaining >= 1)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.moveRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 1;
        }

        SFXManager.instance.UISelect.Play();
    }

    public void ShowRun()
    {
        if (GameManager.instance.actionPointsRemaining >= 2)
        {
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currentActionCost = 2;
        }

        SFXManager.instance.UISelect.Play();
    }

    public void UpdateActionPointText(int actionPoints)
    {
        actionPointText.text = "Action Points Remaining: " + actionPoints;
    }

    public void SkipTurn()
    {
        GameManager.instance.EndTurn();

        SFXManager.instance.UISelect.Play();
    }

    public void ShowMeleeMenu()
    {
        HideMenus();
        meleeMenu.SetActive(true);

        SFXManager.instance.UISelect.Play();
    }

    public void HideMeleeMenu()
    {
        HideMenus();
        ShowInputMenu();
        GameManager.instance.targetDisplay.SetActive(false);

        SFXManager.instance.UICancel.Play();
    }

    public void CheckMelee()
    {
        GameManager.instance.activePlayer.GetMeleeTargets();

        if (GameManager.instance.activePlayer.meleeTargets.Count > 0)
        {
            ShowMeleeMenu();

            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;

            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);
        } else
        {
            ShowErrorText("No enemies in melee range!");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void MeleeHit()
    {
        GameManager.instance.activePlayer.DoMelee();
        GameManager.instance.currentActionCost = 1;

        HideMenus();

        GameManager.instance.targetDisplay.SetActive(false);

        SFXManager.instance.UISelect.Play();

        StartCoroutine(WaitToEndActionCo(1f));
    }

    public IEnumerator WaitToEndActionCo(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendActionPoint();

        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);
    }

    public void NextMeleeTarget()
    {
        GameManager.instance.activePlayer.currentMeleeTarget++;
        if (GameManager.instance.activePlayer.currentMeleeTarget >= GameManager.instance.activePlayer.meleeTargets.Count)
        {
            GameManager.instance.activePlayer.currentMeleeTarget = 0;
        }

        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;

        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);

        SFXManager.instance.UISelect.Play();
    }

    public void ShowRangedMenu()
    {
        HideMenus();
        rangedMenu.SetActive(true);

        UpdateHitChance();

        SFXManager.instance.UISelect.Play();
    }

    public void HideRangedMenu()
    {
        HideMenus();
        ShowInputMenu();
        GameManager.instance.targetDisplay.SetActive(false);

        CameraController.instance.SetMoveTarget(GameManager.instance.activePlayer.transform.position);

        SFXManager.instance.UICancel.Play();
    }

    public void ShowErrorText(string messageToShow)
    {
        errorText.text = messageToShow;
        errorText.gameObject.SetActive(true);

        errorCounter = errorDisplayTime;
    }

    private void Update()
    {
        if (errorCounter > 0)
        {
            errorCounter -= Time.deltaTime;
            if (errorCounter <= 0)
            {
                errorText.gameObject.SetActive(false);
            }
        }
    }

    public void CheckRanged()
    {
        GameManager.instance.activePlayer.GetRangedTargets();

        if (GameManager.instance.activePlayer.rangedTargets.Count > 0)
        {
            ShowRangedMenu();

            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.rangedTargets[GameManager.instance.activePlayer.currentRangedTarget].transform.position;

            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.rangedTargets[GameManager.instance.activePlayer.currentRangedTarget].transform);

            CameraController.instance.SetFireView();
        } else
        {
            ShowErrorText("No Enemies in range!");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void NextRangedTarget()
    {
        GameManager.instance.activePlayer.currentRangedTarget++;
        if (GameManager.instance.activePlayer.currentRangedTarget >= GameManager.instance.activePlayer.rangedTargets.Count)
        {
            GameManager.instance.activePlayer.currentRangedTarget = 0;
        }

        GameManager.instance.targetDisplay.transform.position = GameManager.instance.activePlayer.rangedTargets[GameManager.instance.activePlayer.currentRangedTarget].transform.position;

        UpdateHitChance();

        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.rangedTargets[GameManager.instance.activePlayer.currentRangedTarget].transform);

        CameraController.instance.SetFireView();

        SFXManager.instance.UISelect.Play();
    }

    public void FireShot()
    {
        GameManager.instance.activePlayer.FireShot();

        GameManager.instance.currentActionCost = 1;
        HideMenus();

        GameManager.instance.targetDisplay.SetActive(false);

        SFXManager.instance.UISelect.Play();

        StartCoroutine(WaitToEndActionCo(1f));
    }

    public void UpdateHitChance()
    {
        hitChanceText.text = "Chance To Hit: " + GameManager.instance.activePlayer.CheckShotChance().ToString("F0") + "%";
    }

    public void Defend()
    {
        GameManager.instance.activePlayer.SetDefending(true);
        GameManager.instance.EndTurn();

        SFXManager.instance.UISelect.Play();
    }

    public void LeaveBattle()
    {
        GameManager.instance.LeaveBattle();
    }

    public void NextMission()
    {
        GameManager.instance.NextMission();
    }
}
