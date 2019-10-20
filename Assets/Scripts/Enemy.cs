using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State {Idle, Chasing, Attacking};
    State currentEnemyState;

    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;
    Color originlColor;


    float attackDistanceThreshold = 0.5f;
    float timeBetweenAttacks = 1f;
    float nextAttackTime;

    float targetCollisionRadius;
    float enemyCollisionRadius;

    //keeps if the target is availabe to be chased & ...
    LivingEntity targetEntity;
    bool hasTarget;

    //enemy's damage power
    int damage = 1;


    public ParticleSystem deathEffect;

    public static event System.Action OnDeathStatic;


    //it will run before Start()
    public void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            //who the enemy is going to chase
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            //get the radius sizes
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            enemyCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        // run Start() in `LivingEntity` first, then this Start()
        base.Start(); 
        
        if (hasTarget)
        {
            targetEntity.OnDeath += OnTargetDeath;

            //set the default state of enemy
            currentEnemyState = State.Chasing;

            //run only 4 times in a second, so optimized
            StartCoroutine(UpdateFixed());
        }
    }


    //when the target (player) dies
    void OnTargetDeath() {
        hasTarget = false;

        currentEnemyState = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                // this is very expensive, so let's use a less costy way
                //Vector3.Distance(target.position, transform.position);

                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + targetCollisionRadius + enemyCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }

    }

    // when an enemy gets close to the target
    IEnumerator Attack() {
        currentEnemyState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 enemyPosition = transform.position;
        Vector3 direction = (target.position - enemyPosition).normalized;
        Vector3 targetPosition = target.position - direction * targetCollisionRadius;

        float attackSpeed = 3f;
        float percent = 0;
        bool isDamageApplied = false;

        //SM-1
        //skinMaterial.color = Color.red;

        while (percent <= 1) {

            if (percent >= 0.5 && !isDamageApplied) {
                isDamageApplied = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;

            //based on the formula:  y=4(-x^2 + x)
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(enemyPosition, targetPosition, interpolation);

            //instead of SM-1 and SM-2 I used this one
            skinMaterial.color = Color.Lerp(originlColor, Color.red, interpolation);

            yield return null;
        }

        //SM-2
        //skinMaterial.color = originlColor;

        currentEnemyState = State.Chasing;
        pathfinder.enabled = true;
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
            damage = Mathf.CeilToInt(targetEntity.startingHealth / hitsToKillPlayer);

        startingHealth = enemyHealth;

        //the enemy's base color which changes while attacking
        deathEffect.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originlColor = skinMaterial.color;
    }

    IEnumerator UpdateFixed()
    {
        float refreshRate = 0.25f;

        while(hasTarget)
        {
            if (currentEnemyState == State.Chasing)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - direction * (targetCollisionRadius + enemyCollisionRadius + attackDistanceThreshold /2);

                if (!dead)
                    pathfinder.SetDestination(targetPosition);

            }
            yield return new WaitForSeconds(refreshRate);
        }
    }


    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);

        //dies only when the health is <1
        if (damage >= health) {
            if(OnDeathStatic != null)
            {
                OnDeathStatic();
            }

            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.startLifetime);

            AudioManager.instance.PlaySound("Enemy Death", transform.position);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }
}
