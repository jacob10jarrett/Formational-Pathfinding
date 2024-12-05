using UnityEngine;
using System.Collections.Generic; // Needed for List<Vector3>

[RequireComponent(typeof(Pathfinding))]
public class FormationManager : MonoBehaviour
{
    public GameObject characterPrefab;
    public GameObject obstaclePrefab;

    // Movement parameters
    public float maxSpeed = 5f;
    public float rotationSpeed = 2f;

    private GameObject[] characters;
    private Vector3 targetPosition;

    // Formation offsets for finger-four pattern
    private readonly Vector3[] formationOffsets = {
        new Vector3(-3f, 0, -1.5f), // Character 1 (left)
        new Vector3(0, 0, 0),       // Leader (front center)
        new Vector3(3f, 0, -1.5f),  // Character 3 (right)
        new Vector3(4f, 0, -3.5f)   // Character 4 (back right)
    };

    private Pathfinding pathfinding; // Reference to Pathfinding component

    void Start()
    {
        characters = new GameObject[4];
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i] = Instantiate(characterPrefab, transform.position + formationOffsets[i], Quaternion.identity);
            characters[i].AddComponent<CharacterMovement>().Initialize(this, i == 1);
        }

        pathfinding = GetComponent<Pathfinding>();
    }

    void Update()
    {
        // Left-click to set a new target position for the leader character
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPosition = hit.point;

                // Compute path using A*
                List<Vector3> path = pathfinding.FindPath(characters[1].transform.position, targetPosition);
                characters[1].GetComponent<CharacterMovement>().SetPath(path);
            }
        }

        // Right-click to place an obstacle
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Instantiate(obstaclePrefab, hit.point, Quaternion.identity);
                // Update the grid to include the new obstacle
                pathfinding.grid.UpdateGrid();
            }
        }
    }

    public GameObject[] GetCharacters()
    {
        return characters;
    }

    public Vector3 GetFormationTargetPosition(int index)
	{
	    return characters[1].transform.position + formationOffsets[index];
	}

    public Quaternion GetFormationTargetRotation(int index)
    {
            switch (index)
        {
            case 0: // Left Character
                return Quaternion.LookRotation(Vector3.left); // Directly left
            case 1: // Leader
                return Quaternion.LookRotation(Vector3.forward);
            case 2: // Right Character
                return Quaternion.LookRotation(new Vector3(1, 0, 1)); // Diagonally right-forward
            case 3: // Back-right Character
                return Quaternion.LookRotation(Vector3.back); // Directly backward
            default:
                return Quaternion.LookRotation(GetFormationTargetPosition(index) - characters[index].transform.position);
        }
    }
}
