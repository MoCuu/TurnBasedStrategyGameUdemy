using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed;
    private Vector3 moveTarget;

    public NavMeshAgent navAgent;
    private bool isMoving;

    public bool isEnemy;
    public AIBrain brain;

    public float moveRange = 3.5f, runRange = 7f;

    public float meleeRange = 1.5f;
    [HideInInspector]
    public List<CharacterController> meleeTargets = new List<CharacterController>();
    [HideInInspector]
    public int currentMeleeTarget;
    public float meleeDamage = 5f;

    public float maxHealth = 10f;
    [HideInInspector]
    public float currentHealth;

    public TMP_Text healthText, nameText;
    public Slider healthSlider;

    public float rangedRange, rangedDamage;
    [HideInInspector]
    public List<CharacterController> rangedTargets = new List<CharacterController>();
    [HideInInspector]
    public int currentRangedTarget;
    public Transform shootPoint;
    public Vector3 rangedMissRange;

    public LineRenderer shootLine;
    public float shotRemainTime = .5f;
    private float shotRemainCounter;

    public GameObject shotHitEffect, shotMissEffect;

    public GameObject defendObject;
    public bool isDefending;

    public Animator anim;

    private void Awake()
    {
        moveTarget = transform.position;

        navAgent.speed = moveSpeed;

        currentHealth = maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateHealthDisplay();

        nameText.text = name;
        if (isEnemy)
        {
            nameText.color = Color.red;
        } else
        {
            nameText.color = Color.green;
        }

        shootLine.transform.position = Vector3.zero;
        shootLine.transform.rotation = Quaternion.identity;
        shootLine.transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving == true)
        {
            //transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

            if (GameManager.instance.activePlayer == this)
            {
                CameraController.instance.SetMoveTarget(transform.position);

                if (Vector3.Distance(transform.position, moveTarget) < .2f)
                {
                    isMoving = false;

                    GameManager.instance.FinishedMovement();

                    anim.SetBool("isWalking", false);
                }
            }
        }

        if (shotRemainCounter > 0)
        {
            shotRemainCounter -= Time.deltaTime;

            if (shotRemainCounter <= 0)
            {
                shootLine.gameObject.SetActive(false);
            }
        }
    }

    public void MoveToPoint(Vector3 pointToMoveTo)
    {
        moveTarget = pointToMoveTo;

        navAgent.SetDestination(moveTarget);
        isMoving = true;

        anim.SetBool("isWalking", true);
    }

    public void GetMeleeTargets()
    {
        meleeTargets.Clear();

        if (isEnemy == false)
        {
            foreach (CharacterController cc in GameManager.instance.enemyTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < meleeRange)
                {
                    meleeTargets.Add(cc);
                }
            }
        } else
        {
            foreach (CharacterController cc in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < meleeRange)
                {
                    meleeTargets.Add(cc);
                }
            }
        }

        if (currentMeleeTarget >= meleeTargets.Count)
        {
            currentMeleeTarget = 0;
        }
    }

    public void DoMelee()
    {
        meleeTargets[currentMeleeTarget].TakeDamage(meleeDamage);

        //meleeTargets[currentMeleeTarget].gameObject.SetActive(false);

        anim.SetTrigger("doMelee");

        SFXManager.instance.meleeHit.Play();
    }

    public void TakeDamage(float damageToTake)
    {
        if (isDefending == true)
        {
            damageToTake *= .5f;
        }

        currentHealth -= damageToTake;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            navAgent.enabled = false;

            //transform.rotation = Quaternion.Euler(-70f, transform.rotation.eulerAngles.y, 0f);

            GameManager.instance.allChars.Remove(this);
            if (GameManager.instance.playerTeam.Contains(this))
            {
                GameManager.instance.playerTeam.Remove(this);
            }
            if (GameManager.instance.enemyTeam.Contains(this))
            {
                GameManager.instance.enemyTeam.Remove(this);
            }

            anim.SetTrigger("die");

            if (isEnemy == false)
            {
                SFXManager.instance.deathHuman.Play();
            } else
            {
                SFXManager.instance.deathRobot.Play();
            }

            GetComponent<Collider>().enabled = false;

        } else
        {
            anim.SetTrigger("takeHit");

            SFXManager.instance.takeDamage.Play();
        }

        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            healthText.text = "";
            nameText.text = "";
        }
    }

    public void GetRangedTargets()
    {
        rangedTargets.Clear();

        if (isEnemy == false)
        {
            foreach (CharacterController cc in GameManager.instance.enemyTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < rangedRange)
                {
                    rangedTargets.Add(cc);
                }
            }
        } else
        {
            foreach (CharacterController cc in GameManager.instance.playerTeam)
            {
                if (Vector3.Distance(transform.position, cc.transform.position) < rangedRange)
                {
                    rangedTargets.Add(cc);
                }
            }
        }

        if (currentRangedTarget >= rangedTargets.Count)
        {
            currentRangedTarget = 0;
        }
    }

    public void FireShot()
    {
        Vector3 targetPoint = new Vector3(rangedTargets[currentRangedTarget].transform.position.x, rangedTargets[currentRangedTarget].shootPoint.position.y, rangedTargets[currentRangedTarget].transform.position.z);
        targetPoint.y = Random.Range(targetPoint.y, rangedTargets[currentRangedTarget].transform.position.y + .25f);

        Vector3 targetOffset = new Vector3(Random.Range(-rangedMissRange.x, rangedMissRange.x),
            Random.Range(-rangedMissRange.y, rangedMissRange.y), 
            Random.Range(-rangedMissRange.z, rangedMissRange.z));
        targetOffset = targetOffset * (Vector3.Distance(rangedTargets[currentRangedTarget].transform.position, transform.position) / rangedRange);
        targetPoint += targetOffset;

        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;

        Debug.DrawRay(shootPoint.position, shootDirection * rangedRange, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, rangedRange))
        {
            if (hit.collider.gameObject == rangedTargets[currentRangedTarget].gameObject)
            {
                Debug.Log(name + " shot target " + rangedTargets[currentRangedTarget].name);
                rangedTargets[currentRangedTarget].TakeDamage(rangedDamage);

                Instantiate(shotHitEffect, hit.point, Quaternion.identity);
            } else
            {
                Debug.Log(name + " missed " + rangedTargets[currentRangedTarget].name + "!");

                PlayerInputMenu.instance.ShowErrorText("Shot Missed!");

                Instantiate(shotMissEffect, hit.point, Quaternion.identity);
            }

            shootLine.SetPosition(0, shootPoint.position);
            shootLine.SetPosition(1, hit.point);

            SFXManager.instance.impact.Play();

        } else
        {
            Debug.Log(name + " missed " + rangedTargets[currentRangedTarget].name + "!");

            PlayerInputMenu.instance.ShowErrorText("Shot Missed!");

            shootLine.SetPosition(0, shootPoint.position);
            shootLine.SetPosition(1, shootPoint.position + (shootDirection * rangedRange));
        }

        shootLine.gameObject.SetActive(true);
        shotRemainCounter = shotRemainTime;

        SFXManager.instance.PlayShoot();
    }

    public float CheckShotChance()
    {
        float shotChance = 0f;

        RaycastHit hit;

        Vector3 targetPoint = new Vector3(rangedTargets[currentRangedTarget].transform.position.x, rangedTargets[currentRangedTarget].shootPoint.position.y, rangedTargets[currentRangedTarget].transform.position.z);

        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;
        Debug.DrawRay(shootPoint.position, shootDirection * rangedRange, Color.red, 1f);
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, rangedRange))
        {
            if (hit.collider.gameObject == rangedTargets[currentRangedTarget].gameObject)
            {
                shotChance += 50f;
            }
        }

        targetPoint.y = rangedTargets[currentRangedTarget].transform.position.y + .25f;
        shootDirection = (targetPoint - shootPoint.position).normalized;
        Debug.DrawRay(shootPoint.position, shootDirection * rangedRange, Color.red, 1f);
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, rangedRange))
        {
            if (hit.collider.gameObject == rangedTargets[currentRangedTarget].gameObject)
            {
                shotChance += 50f;
            }
        }

        //shotChance = shotChance * .95f;
        shotChance *= 1f - Vector3.Distance(rangedTargets[currentRangedTarget].transform.position, transform.position) / rangedRange;

        return shotChance;
    }

    public void LookAtTarget(Transform target)
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z), Vector3.up);
    }

    public void SetDefending(bool shouldDefend)
    {
        isDefending = shouldDefend;

        defendObject.SetActive(isDefending);
    }
}
