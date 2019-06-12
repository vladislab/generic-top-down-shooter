﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public LayerMask collisionMask;
    float speed = 10f;
    float damage = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveDistance = speed*Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward*moveDistance);
    }
    public void SetSpeed(float newSpeed){
        speed=newSpeed;
    }
    private void CheckCollisions(float moveDistance){
        Ray ray = new Ray(transform.position,transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit, moveDistance,collisionMask,QueryTriggerInteraction.Collide)){
            OnHitObject(hit);
        }
    }

    private void OnHitObject(RaycastHit hit){
        IDamagable damagableObject = hit.collider.GetComponent<IDamagable>();
        if(damagableObject!=null){
            damagableObject.TakeHit(damage,hit);
        }
        GameObject.Destroy(gameObject);
    }
}
