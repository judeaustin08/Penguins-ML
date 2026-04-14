using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class PenguinArea : MonoBehaviour
{
    [Tooltip("The agent inside the arena")]
    public PenguinAgent penguinAgent;

    [Tooltip("The baby penguin inside the arena")]
    public GameObject penguinBaby;

    [Tooltip("The size of the penguin area")]
    public float areaSize = 9;

    [Tooltip("The TextMeshPro text object that displays the cumulative reward of the agent")]
    public TextMeshPro cumulativeRewardText;

    [Tooltip("Prefab of a live fish")]
    public Fish fishPrefab;

    private List<GameObject> fishList;

    private void Start()
    {
        // Reset the area
        ResetArea();
    }

    private void Update()
    {
        // Update the cumulative reward text
        cumulativeRewardText.text = penguinAgent.GetCumulativeReward().ToString("0.00");
    }

    /// <summary>
    /// Reset the area entirely so that a new episode can begin
    /// </summary>
    public void ResetArea()
    {
        RemoveAllFish();
        PlacePenguin();
        PlaceBaby();
        SpawnFish(4, 0.5f);
    }

    /// <summary>
    /// Remove a specific fish when eaten
    /// </summary>
    /// <param name="fishObject">The fish to remove</param>
    public void RemoveSpecificFish(GameObject fishObject)
    {
        fishList.Remove(fishObject);
        Destroy(fishObject);
    }

    /// <summary>
    /// The number of fish remaining
    /// </summary>
    public int FishRemaining => fishList.Count;

    /// <summary>
    /// Choose a random position on the XZ plane within a partial donut shape
    /// </summary>
    /// <param name="center">The center of the donut</param>
    /// <param name="minAngle">The minimum angle of the wedge</param>
    /// <param name="maxAngle">The maximum angle of the wedge</param>
    /// <param name="minRadius">The minimum distance from the center</param>
    /// <param name="maxRadius">The maximum distance from the center</param>
    /// <returns>A position falling within the specified region</returns>
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = minRadius;
        float angle = minAngle;

        if (maxRadius > minRadius)
        {
            // Pick a random radius
            radius = Random.Range(minRadius, maxRadius);
        }

        if (maxAngle > minAngle)
        {
            // Pick a random angle
            angle = Random.Range(minAngle, maxAngle);
        }

        // Center position + forward vector rotated around the Y axis by angle degrees, multiplies by radius
        return center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
    }

    /// <summary>
    /// Remove all fish from the area
    /// </summary>
    private void RemoveAllFish()
    {
        for (int i = 0; i < fishList?.Count; i++)
        {
            if (fishList[i] != null)
            {
                Destroy(fishList[i]);
            }
        }

        fishList = new();
    }

    /// <summary>
    /// Place a penguin in the area
    /// </summary>
    private void PlacePenguin()
    {
        Rigidbody rb = penguinAgent.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        penguinAgent.transform.SetPositionAndRotation(
            ChooseRandomPosition(transform.position, 0, 360, 0, areaSize) + Vector3.up * 0.5f,
            Quaternion.Euler(0, Random.Range(0, 360), 0)
        );
    }

    /// <summary>
    /// Place a baby penguin in the area
    /// </summary>
    private void PlaceBaby()
    {
        Rigidbody rb = penguinBaby.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        penguinBaby.transform.SetPositionAndRotation(
            ChooseRandomPosition(transform.position, -45f, 45f, 4, areaSize) + Vector3.up * 0.5f,
            Quaternion.Euler(0, Random.Range(0, 360), 0)
        );
    }

    /// <summary>
    /// Place some number of fish in the area with some speed
    /// </summary>
    /// <param name="count">The number of fish to spawn</param>
    /// <param name="fishSpeed">The speed of each fish</param>
    private void SpawnFish(int count, float fishSpeed)
    {
        for (int i = 0; i < count; i++)
        {
            // Create the fish GameObject
            GameObject fishObj = Instantiate(fishPrefab.gameObject);
            fishObj.transform.SetPositionAndRotation(
                ChooseRandomPosition(transform.position, 100, 260, 2, 13) + Vector3.up * 0.5f,
                Quaternion.Euler(0, Random.Range(0, 360), 0)
            );

            // Set the fish's parent to the area transform
            fishObj.transform.SetParent(transform);

            // Track the fish
            fishList.Add(fishObj);

            // Set the fish speed
            fishObj.GetComponent<Fish>().fishSpeed = fishSpeed;
        }
    }
}