using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PenguinAgent : Agent
{
    private InputActions input;

    [Tooltip("How fast the agent moves")]
    public float moveSpeed = 5f;

    [Tooltip("How fast the agent turns")]
    public float turnSpeed = 180f;

    [Tooltip("Prefab of the heart that appears when the baby is fed")]
    public GameObject heartPrefab;

    [Tooltip("Prefab of the regurgitated fish that appears when the baby is fed")]
    public GameObject regurgitatedFishPrefab;

    private PenguinArea penguinArea;
    new private Rigidbody rigidbody;
    private GameObject baby;
    private bool isFull; // If true, penguin has a full stomach

    private void Start()
    {
        input = new();
        input.Enable();
    }

    private void OnDestroy()
    {
        input.Disable();
    }

    /// <summary>
    /// Initial setup; called when the agent is initialized
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        penguinArea = GetComponentInParent<PenguinArea>();
        baby = penguinArea.penguinBaby;
        rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Perform actions based on a vector of numbers
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float forwardAmount = actionBuffers.DiscreteActions[0];

        float turnAmount = 0;
        switch (actionBuffers.DiscreteActions[1])
        {
            case 1:
                turnAmount = -1;
                break;
            case 2:
                turnAmount = 1;
                break;
        }

        // Apply movement
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Apply a small negative reward to encourage shorter strategies
        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    /// <summary>
    /// Reads input from the keyboard and converts it into a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behaviour Type to "Heuristic Only" in the Behaviour parameters inspector.
    /// </summary>
    /// <returns>A vectorAction array of floats that will be passed to <see cref="AgentAction(float[])"/></returns>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;

        if (input.Player.Forward.IsPressed())
        {
            forwardAction = 1;
        }

        float turnAmount = input.Player.Turn.ReadValue<float>();
        if (turnAmount < 0) turnAction = 1;
        if (turnAmount > 0) turnAction = 2;

        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }

    /// <summary>
    /// When a new episode begins, reset the agent and the area
    /// </summary>
    public override void OnEpisodeBegin()
    {
        isFull = false;
        penguinArea.ResetArea();
    }

    /// <summary>
    /// Collect all non-raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Whether the penguin has eaten a fish (1 bool = 1 value)
        sensor.AddObservation(isFull);

        // Distance to the baby (1 float = 1 value)
        sensor.AddObservation(Vector3.Distance(transform.position, baby.transform.position));

        // Direction to the baby (1 Vector3 = 3 floats = 3 values)
        sensor.AddObservation((baby.transform.position - transform.position).normalized);

        // Direction penguin is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        // 1 + 1 + 3 + 3 = 8 total values
    }

    /// <summary>
    /// When the agent collides with something, take action
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("fish"))
        {
            EatFish(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("baby"))
        {
            RegurgitateFish();
        }
    }

    /// <summary>
    /// Check if the agent is full; if not, eat a fish and get a reward
    /// </summary>
    /// <param name="fishObject">The fish to eat</param>
    private void EatFish(GameObject fishObject)
    {
        if (isFull) return;

        isFull = true;
        penguinArea.RemoveSpecificFish(fishObject);
        AddReward(1);
    }

    /// <summary>
    /// Check if agent is full; if yes, feed the baby
    /// </summary>
    private void RegurgitateFish()
    {
        if (!isFull) return;

        isFull = false;

        // Spawn regurgitated fish
        GameObject regurgitatedFishObj = Instantiate(regurgitatedFishPrefab);
        regurgitatedFishObj.transform.parent = transform.parent;
        regurgitatedFishObj.transform.position = baby.transform.position;
        Destroy(regurgitatedFishObj, 4);

        // Spawn heart
        GameObject heartObj = Instantiate(heartPrefab);
        heartObj.transform.parent = transform.parent;
        heartObj.transform.position = baby.transform.position + Vector3.up;
        Destroy(heartObj, 4);

        AddReward(1);

        if (penguinArea.FishRemaining <= 0)
            EndEpisode();
    }
}
