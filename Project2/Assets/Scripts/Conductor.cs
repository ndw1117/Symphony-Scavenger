using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum conductorState
{
    SeekandFlee,
    Wandering
}

public class Conductor : Agent
{
    [SerializeField]
    public conductorState currentState = conductorState.Wandering;

    [SerializeField]
    float boundsTime = 1f; //The time to use when calculating future position in the StayInBounds method

    float delay = 0f;
    float wanderAngle = 0f;

    Vector3 seekingForce;
    Vector3 fleeingForce;
    Vector3 boundsForce;
    Vector3 avoidForce;
    Vector3 wanderForce;

    [SerializeField]
    float seekScalar = 1.3f;

    [SerializeField]
    float fleeScalar = 0.12f;

    [SerializeField]
    float wanderScalar = 3f;

    [SerializeField]
    float boundsScalar = 2f;

    [SerializeField]
    float avoidScalar = 3f;

    [SerializeField]
    float avoidTime = 1f;   // The amount of time to look ahead when drawing the "safe space" box for obstacle avoidance

    //Child classes don't have update and start because that would override the update/start in the parent class

    public override void CalculateSteeringForces()
    {
        float dist = Mathf.Infinity;
        fleeingForce = Vector3.zero;
        seekingForce = Vector3.zero;

        if (currentState == conductorState.SeekandFlee)
        {
            foreach (Agent instrument in AgentManager.Instance.agents)
            {
                if (instrument.GetComponent<Instrument>().currentState == state.Fleeing)
                {
                    if (Vector3.Distance(transform.position, instrument.transform.position) < dist)
                    {
                        dist = Vector3.Distance(transform.position, instrument.transform.position);
                        seekingForce = Seek(instrument.transform.position) * seekScalar; //Seek closest fleeing instrument
                    }
                }
                else if (instrument.GetComponent<Instrument>().currentState == state.Seeking)
                {
                    fleeingForce += Flee(instrument.transform.position) * fleeScalar; //Flee all seeking instruments
                }
            }

            totalForces += seekingForce;
            totalForces += fleeingForce;
        }

        if (delay <= 0)
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
        wanderForce = Wander(delay, wanderAngle) * wanderScalar;   
        totalForces += wanderForce;

        if (boundsForce != Vector3.zero)
        {
            wanderAngle = Random.Range(-6f, 6f);    //This is in radians!
        }

        //Add a force avoiding obstacles
        avoidForce = AvoidObstacles(avoidTime) * avoidScalar;
        totalForces += avoidForce;


    }

    public void ChangeState()
    {
        if (currentState == conductorState.SeekandFlee)
        {
            transform.GetComponent<Agent>().maxSpeed = 1.5f;
            wanderScalar = 3f;
            currentState = conductorState.Wandering;
        }
        else
        {
            transform.GetComponent<Agent>().maxSpeed = 2.1f;
            wanderScalar = 2f;
            currentState = conductorState.SeekandFlee;
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Green = Velocity
        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(transform.position, physicsObject.velocity);

        //Yellow = BoundsForce
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, boundsForce);

        //Magenta = AvoidForce
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, avoidForce);

        //Cyan = SeekingForce
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, seekingForce);

        //Red = FleeingForce
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, fleeingForce);

        //White = WanderForce
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, wanderForce);
    }

    private void OnDrawGizmos()
    {
        //Draw Collision Circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, transform.GetComponent<PhysicsObject>().radius);

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
        foreach (Vector3 pos in foundObstacles)
        {
            Gizmos.DrawLine(transform.position, pos);
        }

    }
}
