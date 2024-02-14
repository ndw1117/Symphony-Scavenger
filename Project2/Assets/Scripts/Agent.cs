using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhysicsObject))] //Not required, but ensures that any object with an Agent component will have a PhysicsObject component
public abstract class Agent : MonoBehaviour
{
    [SerializeField]
    protected PhysicsObject physicsObject;

    [SerializeField]
    public float maxSpeed = 2f;

    [SerializeField]
    float maxForce = 2f;

    [SerializeField]
    float separateRadius = 1.3f;

    protected Vector3 totalForces = Vector3.zero;

    protected List<Vector3> foundObstacles = new List<Vector3>();

    public Vector3 cameraPosition;
    public float cameraHalfHeight;
    public float cameraHalfWidth;

    // Start is called before the first frame update
    void Start()
    {
        //physicsObject = GetComponent<PhysicsObject>();        Could set phys object this way if we didn't serialize field
        cameraPosition = Camera.main.transform.position;
        cameraPosition.z = transform.position.z;
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
    }

    // Update is called once per frame
    void Update()
    {

        CalculateSteeringForces();

        //This makes sure total force isn't too large
        totalForces = Vector3.ClampMagnitude(totalForces, maxForce);    //If total force > maxForce, Clamp will reduce magnitude to maxForce, else it will leave it alone

        physicsObject.ApplyForce(totalForces);

        totalForces = Vector3.zero;
    }

    public abstract void CalculateSteeringForces();     //Makes it so child classes must have some implementation of this method
    //Must set value for total forces in this method

    public Vector3 Seek(Vector3 targetPos)
    {
        //Calculate desired velocity
        Vector3 desiredVelocity = targetPos - transform.position;

        //Apply max speed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        //Calculate the steering force
        Vector3 seekingForce = desiredVelocity - physicsObject.velocity;

        return seekingForce;
    }

    public Vector3 Flee(Vector3 targetPos)
    {
        //Calculate desired velocity
        Vector3 desiredVelocity = transform.position - targetPos;       //Desired Velocity is the only thing different from Seek

        //Apply max speed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        //Calculate the steering force
        Vector3 fleeingForce = desiredVelocity - physicsObject.velocity;

        return fleeingForce;
    }

    //Pursue and evade are not used in this project
    /*public Vector3 Pursue(Agent target)
    {
        return (Seek(target.CalculateFuturePosition(2f, physicsObject.velocity)));
    }

    public Vector3 Evade(Agent target)
    {
        return (Flee(target.CalculateFuturePosition(2f, physicsObject.velocity)));
    }*/

    public Vector3 Wander(float time, float wanderAngle)
    {
        Vector3 direction = Vector3.zero;

        if (physicsObject.velocity != Vector3.zero)
        {
            direction = physicsObject.velocity / maxSpeed;
        }
        else
        {
            direction = new Vector3(1, 0, 0);
        }

        direction = Quaternion.Euler(0, 0, wanderAngle) * direction;
        return (Seek(CalculateFuturePosition(time, direction * maxSpeed)));
    }

    public Vector3 Separate()
    {
        float closestDistance = Mathf.Infinity;
        Agent closestAgent = null;

        //Loop through all agents
        foreach (Agent agent in AgentManager.Instance.agents)
        {
            //Calculate the distance between this Agent and the one being compared to
            float distance = Vector3.Distance(transform.position, agent.transform.position);    //Note: this uses square root, so not the best for performance

            //Check to see if the agent being compared to is itself
            if (distance <= Mathf.Epsilon)      //Mathf.Epsilon = a very small number, close to zero
            {
                //If it's comparing itself, skip to next iteration of loop
                continue;
            }

            //Check to see if there is a closest Agent within your space "bubble"
            if (distance < closestDistance && distance <= separateRadius)     //If closer than anything else found, and within the Agent's radius, or "bubble"
            {
                closestAgent = agent;
                closestDistance = distance;
            }
        }

        if (closestAgent != null)
        {
            return Flee(closestAgent.transform.position);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 StayInBounds(float time)
    {
        Vector3 futurePos = CalculateFuturePosition(time, physicsObject.velocity);

        //Check if past left edge, right edge, top edge, bottom edge
        if (futurePos.x > cameraPosition.x + cameraHalfWidth ||
            futurePos.x < cameraPosition.x - cameraHalfWidth ||
            futurePos.y > cameraPosition.y + cameraHalfHeight ||
            futurePos.y < cameraPosition.y - cameraHalfHeight)
        {
            return Seek(cameraPosition); //If out of bounds, seek the center of the world
        }

        return Vector3.zero;

    }

    public Vector3 AvoidObstacles(float avoidTime)
    {
        //Draw "safe space" box

        //Calculate how far we want the "safe space" box to stretch based on future position
        Vector3 futurePos = CalculateFuturePosition(avoidTime, physicsObject.velocity);
        float dist = Vector3.Distance(transform.position, futurePos) + physicsObject.radius;

        //Size of the box
        Vector3 boxSize = new Vector3(physicsObject.radius * 2f, dist, physicsObject.radius * 2f);


        //A vector from the obstacle to the transform position
        Vector3 AtoO = Vector3.zero;

        //Variables to store the dot products
        float forwardDot;
        float rightDot;

        Vector3 desiredVelocity = Vector3.zero;

        //Keep track of closest obstacle's AtoO vector magnitude so that we only avoid closest obstacle
        float closestMagnitude = Mathf.Infinity;

        //We must clear the obstacles from the last check
        foundObstacles.Clear();

        foreach (Obstacle obstacle in AgentManager.Instance.obstacles)
        {
            AtoO = obstacle.transform.position - transform.position;

            if (AtoO.magnitude > closestMagnitude)
            {
                continue;   //Skip this obstacle if it is not the closest one
            }
            else
            {
                closestMagnitude = AtoO.magnitude;
            }

            forwardDot = Vector3.Dot(AtoO, physicsObject.velocity.normalized);
            rightDot = Vector3.Dot(AtoO, physicsObject.transform.right);

            if (forwardDot >= -obstacle.radius) //If the obstacle is in front of the agent (using -radius instead of zero to accound for the circle and not just center of obstacle)
            {
                if (forwardDot <= dist)
                {
                    if (Mathf.Abs(rightDot) <= physicsObject.radius + obstacle.radius) //Obstacle is in "safe space"
                    {
                        foundObstacles.Add(obstacle.transform.position);

                        //Calculate desired velocity
                        if (rightDot < 0) //Obstacle is on the left, so turn right
                        {
                            desiredVelocity = transform.right;
                        }
                        else if (rightDot > 0) //Obstacle is on thr right, so turn left
                        {
                            desiredVelocity = transform.right * -1;
                        }
                        else if (rightDot == 0)
                        {
                            switch (Random.Range(1f, 2f))
                            {
                                case 1f:
                                    desiredVelocity = transform.right * -1;
                                    break;

                                case 2f:
                                    desiredVelocity = transform.right;
                                    break;
                            }
                        }

                    }
                }

            }
        }

        //Apply max speed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        //Calculate the steering force
        Vector3 avoidingForce = Vector3.zero;

        if (desiredVelocity != Vector3.zero)
        {
            avoidingForce = desiredVelocity - physicsObject.velocity;
        }

        return avoidingForce;

    }

    public Vector3 CalculateFuturePosition(float time, Vector3 velocity)
    {
        return transform.position + (velocity * time);    //Don't need delta time because this isn't called every frame
    }

}
