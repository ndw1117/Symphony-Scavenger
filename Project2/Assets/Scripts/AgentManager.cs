using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a singleton class. There will only be one of these. Access its fields with .Instance
public class AgentManager : Singleton<AgentManager>
{
    [SerializeField]
    GameObject music;

    // Used to keep track of which instruments are currently playing
    string musicState = "";

    AudioSource[] tracks;

    [SerializeField]
    List<Agent> instrumentPrefabs;

    [SerializeField]
    Agent conductorPrefab;

    public List<Agent> agents = new List<Agent>();

    public Agent conductor;

    
    public List<Obstacle> obstacles;

    // (Optional) Prevents non-singleton constructor use.
    protected AgentManager() { }

    // Start is called before the first frame update
    void Start()
    {
        tracks = music.GetComponents<AudioSource>();

        conductor = Instantiate<Agent>(conductorPrefab);    //Conductor is not added to the list of agents
    }

    // Update is called once per frame
    void Update()
    {
        CollisionCheck();
    }

    public void SpawnInstrument(int instrumentType)
    {
        foreach (Agent instrument in agents)
        {
            if ((int)instrument.GetComponent<Instrument>().instrumentType == instrumentType)
            {
                return;     // If this instrument already exists in the scene, do nothing
            }
        }

        Agent newInstrument = Instantiate<Agent>(instrumentPrefabs[instrumentType], new Vector3(Random.Range(-4f,2.5f), Random.Range(-1f, 1f), 0), Quaternion.identity);
        agents.Add(newInstrument);

        if (musicState.Contains(instrumentType.ToString()))
        {
            newInstrument.GetComponent<Instrument>().currentState = state.Seeking;
        }
        else
        {
            newInstrument.GetComponent<Instrument>().currentState = state.Fleeing;
        }

        if (conductor.GetComponent<Conductor>().currentState == conductorState.Wandering)
        {
            conductor.GetComponent<Conductor>().ChangeState();
        }
    }

    void CollisionCheck()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            //Check for a collision with the conductor
            if (Mathf.Pow(conductor.GetComponent<PhysicsObject>().radius + agents[i].GetComponent<PhysicsObject>().radius, 2) > 
               (Mathf.Pow(conductor.gameObject.transform.position.x - agents[i].gameObject.transform.position.x, 2) +
               Mathf.Pow(conductor.gameObject.transform.position.y - agents[i].gameObject.transform.position.y, 2)))
            {
                //Changes the music to include/exclude the instrument involved in the collision
                int type = (int)agents[i].GetComponent<Instrument>().instrumentType;
                if (musicState.Contains(type.ToString()))
                {
                    musicState = musicState.Replace(type.ToString(), "");
                    tracks[type].Stop();
                }
                else
                {
                    foreach(AudioSource track in tracks)
                    {
                        if (track.isPlaying)
                        {
                            tracks[type].time = track.time;
                            tracks[type].Play();
                            break;
                        }
                    }

                    if (!tracks[type].isPlaying)
                    {
                        tracks[type].time = 0f;
                        tracks[type].Play();
                    }

                    musicState += type.ToString();
                    
                }

                Debug.Log(musicState);

                Destroy(agents[i].gameObject);
                agents.RemoveAt(i);

                if (agents.Count == 0)
                {
                    conductor.GetComponent<Conductor>().ChangeState();
                }
            }
        }
    }
}
