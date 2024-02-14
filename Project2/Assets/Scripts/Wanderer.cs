using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wanderer : Agent
{
    float delay = 0f;
    float wanderAngle = 0f;

    [SerializeField]
    float boundsTime = 1f; //The time to use when calculating future position in the StayInBounds method

    Vector3 wanderForce;
    Vector3 boundsForce;
    Vector3 centerForce;
    Vector3 separateForce;
    Vector3 avoidForce;

    [SerializeField]
    float wanderScalar = 3f;

    [SerializeField]
    float boundsScalar = 1f;

    [SerializeField]
    float centerScalar = 0.2f;

    [SerializeField]
    float separateScalar = 1.3f;

    [SerializeField]
    float avoidScalar = 1f;

    [SerializeField]
    float avoidTime = 1f;   // The amount of time to look ahead when drawing the "safe space" box for obstacle avoidance

    //Child classes don't have update and start because that would override the update/start in the parent class

    public override void CalculateSteeringForces()
    {
        if ( delay <= 0)
        {
            wanderAngle = Random.Range(-6f, 6f);    //This is in radians!
            delay = Random.Range(2f, 4f);
        }
        else
        {
            delay -= Time.deltaTime;
        }

        boundsForce = StayInBounds(boundsTime) * boundsScalar;     //Made it a variable so we can draw a Gizmo for it
        totalForces += boundsForce;

        if (boundsForce != Vector3.zero)
        {
            wanderAngle = Random.Range(-6f, 6f);  //This is in radians!
        }

        //Wander towards a target
        wanderForce = Wander(delay, wanderAngle) * wanderScalar;   //Made it a variable so we can draw a Gizmo for it
        totalForces += wanderForce;

        if (boundsForce != Vector3.zero)
        {
            wanderAngle = Random.Range(-6f, 6f);    //This is in radians!
        }

        centerForce = Seek(Vector3.zero) * centerScalar; //if you want the agent to really stay away from bounds
        totalForces += centerForce;

        //Add this if you want Wanderer to separate from other agents
        separateForce = Separate() * separateScalar;
        totalForces += separateForce;

        //Add a force avoiding obstacles
        avoidForce = AvoidObstacles(avoidTime) * avoidScalar;
        totalForces += avoidForce;
        
        
    }

    private void OnDrawGizmosSelected()
    {
        //Green = Velocity
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, physicsObject.velocity);

        //Red = WanderForce
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, wanderForce);

        //Yellow = BoundsForce
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, boundsForce);

        //White = CenterForce
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, centerForce);

        //Black = SeparateForce
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, separateForce);

        //Magenta = AvoidForce
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, avoidForce);
    }

    private void OnDrawGizmos()
    {
        //Draw "safe space" box

        //Calculate how far we want the "safe space" box to stretch based on future position
        Vector3 futurePos = CalculateFuturePosition(avoidTime, physicsObject.velocity);
        float dist = Vector3.Distance(transform.position, futurePos) + physicsObject.radius;

        //Size of the box
        Vector3 boxSize = new Vector3(physicsObject.radius * 2f, dist, physicsObject.radius * 2f);

        //Calculate the position of the "safe space" box
        Vector3 boxCenter = Vector3.zero;
        boxCenter.y += dist / 2f;

        Gizmos.color = Color.green;

        //Allows the gizmo to adjust to the object's local space instead of world space (with rotation, for example)
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(boxCenter, boxSize);

        //We need this after drawing the "safe space" box to make sure that the other gizmos don't get drawn in local space
        Gizmos.matrix = Matrix4x4.identity;


        //Draw a line to all found obstacles in the "safe space" box. (Used for debugging obstacle avoidance)
        Gizmos.color = Color.blue;
        foreach(Vector3 pos in foundObstacles)
        {
            Gizmos.DrawLine(transform.position, pos);
        }

    }
}
