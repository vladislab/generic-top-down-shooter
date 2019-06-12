using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State{
        Idle,
        Chasing,
        Attacking,
    };
    
    State currentState;
    NavMeshAgent pathFinder;
    Transform target;
    Spawner spawner;
    Material skinMaterial;
    Color originalColor;
    LivingEntity targetEntity;
    float attackDistanceThreshold =.5f;
    float timeBetweenAttacks = 1f;
    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisonRadius;
    float damage = 1f;
    bool hasTarget;
    bool hasAppliedDamage;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        pathFinder = GetComponent<NavMeshAgent>();
        
        if(CheckTarget()){
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            spawner=FindObjectOfType<Spawner>();

            skinMaterial = GetComponent<Renderer>().material;
            originalColor = skinMaterial.color;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisonRadius = GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());        
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(CheckTarget()){
            if(Time.time>nextAttackTime){
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if(sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold+myCollisionRadius+targetCollisonRadius,2)){
                    nextAttackTime = Time.time+timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }

        

    }
    IEnumerator Attack(){
        currentState = State.Attacking;
        pathFinder.enabled=false;

        Vector3 originalPos = transform.position;
        if(hasTarget){
            
        }
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPos = target.position - directionToTarget*(myCollisionRadius);


        float percent =0;
        float attackSpeed=3;

        skinMaterial.color=Color.red;
        hasAppliedDamage = false;

        while(percent <=1){
            if(percent >=.5f && !hasAppliedDamage){
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime *attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2)+percent)*4;
            transform.position = Vector3.Lerp(originalPos,attackPos,interpolation);
            yield return null;
            
        }
        skinMaterial.color=originalColor;
        currentState = State.Chasing;
        pathFinder.enabled=true;
    }

    IEnumerator UpdatePath(){
        float refreshRate = 0.25f;
        while(CheckTarget()){
            if(currentState==State.Chasing){
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget*(myCollisionRadius+targetCollisonRadius+attackDistanceThreshold/2);
            
                if(!dead){
                    pathFinder.SetDestination(targetPosition);
                }
            }    
            
            yield return new WaitForSeconds(refreshRate);   
        }
    }
    protected override void Die(){
        spawner.OnEnemyDeath();
        hasTarget = false;
        currentState = State.Idle;
        base.Die();
        
    }
    private bool CheckTarget(){
        return GameObject.FindGameObjectWithTag("Player");
    }
}
