using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectiles projectile;
    public float msBetweenShots = 100f;
    public float muzzleVelocity = 35;
    float nextShotTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Shoot(){
        if(Time.time > nextShotTime){
            nextShotTime = Time.time + msBetweenShots/1000;
            Projectiles newProjectile = Instantiate(projectile,muzzle.position,muzzle.rotation) as Projectiles;
            newProjectile.SetSpeed(muzzleVelocity);
        }

    }
}
