using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsObject : MonoBehaviour
{
    public Vector3 position = Vector3.zero;
    public Vector3 velocity = Vector3.zero;

    Vector3 acceleration = Vector3.zero;

    [SerializeField]
    float mass = 1f; //Can't have mass of zero or forces won't work

    public float radius = 1f;

    public Vector3 cameraPosition;
    public float cameraHalfHeight;
    public float cameraHalfWidth;

    //This project does not use gravity and friction
    /*public bool useGravity;
    public bool useFriction;

    float gravity = -.981f;

    [SerializeField]
    float coeff = 0.2f;*/

    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        cameraPosition = Camera.main.transform.position;
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
    }

    // Update is called once per frame
    void Update()
    {

        //This project does not use gravity and friction
        /*if (useGravity)
        {
            ApplyGravity(new Vector3(0, gravity, 0));
        }

        if (useFriction)
        {
            ApplyFriction(coeff);
        }*/

        velocity += acceleration * Time.deltaTime; //Must be += because acceleration is change in velocity. We don't want to set velocity equal to acceleration

        position += velocity * Time.deltaTime; 

        //This project does not have agents bounce
        //CheckForBounce();

        transform.position = position;

        //Change the object's rotation to match the direction of its velocity
        transform.rotation = Quaternion.LookRotation(Vector3.forward, velocity.normalized);

        acceleration = Vector3.zero; //Always zero out your acceleration at the end of update! Otherwise you'll be applying forces longer than intended.
    }

    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    //This project does not use gravity and friction
    /*void ApplyFriction(float coeff)
    {
        Vector3 friction = velocity * -1;
        friction.Normalize();
        friction = friction * coeff;
        ApplyForce(friction);
    }

    void ApplyGravity (Vector3 force)
    {
        acceleration += force;
    }*/

    //This project does not have agents bounce
    /*
    void CheckForBounce()
    {
        //Check if past left edge
        if (position.x > cameraPosition.x + cameraHalfWidth)
        {
            velocity.x *= -1;
            position.x = cameraPosition.x + cameraHalfWidth;
        }
        //Check if past right edge
        else if (position.x < cameraPosition.x - cameraHalfWidth)
        {
            velocity.x *= -1;
            position.x = -cameraPosition.x - cameraHalfWidth;
        }

        //Check if past top edge
        if (position.y > cameraPosition.y + cameraHalfHeight)
        {
            velocity.y *= -1;
            position.y = cameraPosition.y + cameraHalfHeight;
        }
        //Check if past bottom edge
        else if (position.y < cameraPosition.y - cameraHalfHeight)
        {
            velocity.y *= -1;
            position.y = -cameraPosition.y - cameraHalfHeight;
        }
    }*/
}
