using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    // Start is called before the first frame update
    Rigidbody rb2d;     
    void Start()
    {
        rb2d = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 _velocity){
        velocity = _velocity;
    }
    private void FixedUpdate() {
        rb2d.MovePosition(rb2d.position+velocity*Time.fixedDeltaTime);
    }
    public void LookAt(Vector3 lookPoint){
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x,transform.position.y,lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
   
}
