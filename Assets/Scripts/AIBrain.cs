using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public CharacterController charCon;

    public float waitBeforeActing = 1f, waitAfterActing = 1f, waitBeforeShooting = .5f;

    public float moveChance = 60f, defendChance = 25f, skipChance = 15f;

    [Range(0f, 100f)]
    public float ignoreShootChance = 20f, moveRandomChance = 50f;

    public void ChooseAction()
    {
        StartCoroutine(ChooseCo());
    }

    public IEnumerator ChooseCo()
    {
        Debug.Log(name + " is choosing and action...");

        yield return new WaitForSeconds(waitBeforeActing);

        bool actionTaken = false;

        charCon.GetMeleeTargets(); // is melee available?
        if (charCon.meleeTargets.Count > 0)
        {
            Debug.Log("Is Meleeing...");

            int meleeTargetRandom = Random.Range(0, charCon.meleeTargets.Count);
            charCon.currentMeleeTarget = meleeTargetRandom;
            charCon.LookAtTarget(charCon.meleeTargets[meleeTargetRandom].transform);

            GameManager.instance.currentActionCost = 1;

            StartCoroutine(WaitToEndAction(waitAfterActing));

            charCon.DoMelee();

            actionTaken = true;
        } // melee end

        charCon.GetRangedTargets(); // is ranged available?
        if (actionTaken == false && charCon.rangedTargets.Count > 0)
        {
            if (Random.Range(0f, 100f) > ignoreShootChance)
            {
                List<float> hitChances = new List<float>();

                for (int i = 0; i < charCon.rangedTargets.Count; i++)
                {
                    charCon.currentRangedTarget = i;
                    charCon.LookAtTarget(charCon.rangedTargets[i].transform);
                    hitChances.Add(charCon.CheckShotChance());
                }

                float highestChance = 0f;
                for (int i = 0; i < hitChances.Count; i++)
                {
                    if (hitChances[i] > highestChance)
                    {
                        highestChance = hitChances[i];
                        charCon.currentRangedTarget = i;
                    }
                    else if (hitChances[i] == highestChance)
                    {
                        if (Random.value > .5f)
                        {
                            charCon.currentRangedTarget = i;
                        }
                    }
                }

                if (highestChance > 0f)
                {
                    Debug.Log(name + " is Shooting at " + charCon.rangedTargets[charCon.currentRangedTarget].name);

                    charCon.LookAtTarget(charCon.rangedTargets[charCon.currentRangedTarget].transform);
                    CameraController.instance.SetFireView();

                    actionTaken = true;

                    StartCoroutine(WaitToShoot());
                }
            }
        } // ranged end

        if (actionTaken == false) // moving
        {
            float actionDecision = Random.Range(0f, moveChance + defendChance + skipChance);

            if (actionDecision < moveChance)
            {
                float moveRandom = Random.Range(0f, 100f);

                List<MovePoint> potentialMovePoints = new List<MovePoint>();
                int selectedPoint = 0;

                if (moveRandom > moveRandomChance)
                {
                    int nearestPlayer = 0;

                    for (int i = 1; i < GameManager.instance.playerTeam.Count; i++)
                    {
                        if (Vector3.Distance(transform.position, GameManager.instance.playerTeam[nearestPlayer].transform.position)
                            > Vector3.Distance(transform.position, GameManager.instance.playerTeam[i].transform.position))
                        {
                            nearestPlayer = i;
                        }
                    }

                    if (Vector3.Distance(transform.position, GameManager.instance.playerTeam[nearestPlayer].transform.position) > charCon.moveRange + 2f && GameManager.instance.actionPointsRemaining >= 2)
                    {
                        potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.runRange, transform.position);

                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedPoint = i;
                            }
                        }

                        GameManager.instance.currentActionCost = 2;

                        Debug.Log(name + " is running towards " + GameManager.instance.playerTeam[nearestPlayer].name);
                    }
                    else
                    {
                        potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.moveRange, transform.position);

                        float closestDistance = 1000f;
                        for (int i = 0; i < potentialMovePoints.Count; i++)
                        {
                            if (Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position) < closestDistance)
                            {
                                closestDistance = Vector3.Distance(GameManager.instance.playerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedPoint = i;
                            }
                        }

                        GameManager.instance.currentActionCost = 1;

                        Debug.Log(name + " is moving towards " + GameManager.instance.playerTeam[nearestPlayer].name);
                    }


                }
                else
                {
                    potentialMovePoints = MoveGrid.instance.GetMovePointsInRange(charCon.moveRange, transform.position);

                    selectedPoint = Random.Range(0, potentialMovePoints.Count);

                    GameManager.instance.currentActionCost = 1;

                    Debug.Log(name + " is repositioning.");
                }

                charCon.MoveToPoint(potentialMovePoints[selectedPoint].transform.position);
            }
            else if (actionDecision < moveChance + defendChance)
            {
                Debug.Log(name + " is defending.");

                charCon.SetDefending(true);

                GameManager.instance.currentActionCost = GameManager.instance.actionPointsRemaining;

                StartCoroutine(WaitToEndAction(waitAfterActing));
            }
            else
            {
                GameManager.instance.EndTurn();

                Debug.Log(name + " skipped turn.");
            }
        } // moving/defending/skipping end
    }

    IEnumerator WaitToEndAction(float timeToWait)
    {
        Debug.Log("Waiting to end an action...");
        yield return new WaitForSeconds(timeToWait);
        GameManager.instance.SpendActionPoint();
    }

    IEnumerator WaitToShoot()
    {
        yield return new WaitForSeconds(waitBeforeShooting);

        charCon.FireShot();

        GameManager.instance.currentActionCost = 1;

        StartCoroutine(WaitToEndAction(waitAfterActing));
    }
}
