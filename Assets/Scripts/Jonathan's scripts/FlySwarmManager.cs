using UnityEngine;

public class FlySwarmManager : MonoBehaviour
{
    public GameObject flyPrefab;
    public int swarmSize = 15;
    public float swarmRadius = 1.5f;
    public float flySpeed = 3f;

    private GameObject[] flies;
    private Vector3[] localTargetPositions;
    void Start()
    {
        flies = new GameObject[swarmSize];
        localTargetPositions = new Vector3[swarmSize];

        for (int i = 0; i < swarmSize; i++)
        {
            Vector3 randomLocalPos = Random.insideUnitSphere * swarmRadius;
            flies[i] = Instantiate(flyPrefab, transform);
            flies[i].transform.localPosition = randomLocalPos;
            localTargetPositions[i] = randomLocalPos;
        }
    }
    void Update()
    {
        for (int i = 0; i < swarmSize; i++)
        {
            if (flies[i] == null) continue;

            flies[i].transform.localPosition = Vector3.MoveTowards(flies[i].transform.localPosition, localTargetPositions[i], flySpeed * Time.deltaTime);

            Vector3 direction = localTargetPositions[i] - flies[i].transform.localPosition;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                flies[i].transform.localRotation = Quaternion.Slerp(flies[i].transform.localRotation, lookRotation, Time.deltaTime * 5f);
            }

            if (Vector3.Distance(flies[i].transform.localPosition, localTargetPositions[i]) < 0.1f)
            {
                localTargetPositions[i] = Random.insideUnitSphere * swarmRadius;
            }
        }
    }
}
