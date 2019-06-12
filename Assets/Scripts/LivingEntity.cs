using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class LivingEntity : MonoBehaviour, IDamagable
{
    protected float health;
    protected bool dead;
    public float startingHealth;

    public void TakeHit(float damage,RaycastHit hit){
        health -= damage;
        if(health<=0 && !dead){
            Die();
        }
    }
    protected virtual void Die(){
        dead = true;
        GameObject.Destroy(gameObject);
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = startingHealth;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
