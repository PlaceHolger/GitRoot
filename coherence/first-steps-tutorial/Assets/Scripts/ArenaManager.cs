using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGridElement
{
    public GameObject groundGameObject;
}

public class ArenaManager : MonoBehaviour
{
    [SerializeField]
    private int arenaGridWidth = 16;

    [SerializeField]
    private int arenaGridLength = 9;

    [SerializeField, Range(0.1f, 3f)]
    private float arenaGridCellSize = 1.0f;

    [SerializeField]
    private GameObject arenaEmptyGround;

    [SerializeField, Tooltip("Order: Top Left, Top Right, Bottom Left, Bottom Right, Straight, Empty")]
    private List<GameObject> arenaBorderPrefabs;

    private List<GameObject> borderElements = new List<GameObject>();

    private Dictionary<int, ArenaGridElement> arenaGridElements = new Dictionary<int, ArenaGridElement>();

    public int IndexAt(int x, int y)
    {
        return (arenaGridWidth * y) + x;
    }

    public (int x, int y) CoordAt ( int index )
    {
        return (index / arenaGridWidth, index % arenaGridWidth);
    }

    public void GenerateArena ()
    {
        for ( int i = -1; i < arenaGridWidth + 1; i++ )
        {
            for ( int j = -1; j < arenaGridLength + 1; j++ )
            {
                ArenaGridElement arenaGridElement = new ArenaGridElement();
                arenaGridElements[IndexAt(i, j)] = arenaGridElement;

                arenaGridElement.groundGameObject = Instantiate(arenaEmptyGround, transform);
                arenaGridElement.groundGameObject.transform.localPosition = new Vector3(i, 0, j);
            }
        }

        Quaternion rotationQuaternion = Quaternion.identity;
        Vector3 borderElementPosition;

        for ( int i = 0; i < arenaGridWidth + 1; i++ )
        {
            borderElementPosition = new Vector3( i - 1, 0, -1 );
            GameObject borderElement = i == 0 ? 
                Instantiate(arenaBorderPrefabs[0], borderElementPosition, rotationQuaternion, transform) : 
                Instantiate(arenaBorderPrefabs[1], borderElementPosition, rotationQuaternion, transform);
        }

        rotationQuaternion = Quaternion.AngleAxis(-90f, Vector3.up);

        for ( int j = 0; j < arenaGridLength + 1; j++  )
        {
            borderElementPosition = new Vector3(arenaGridWidth, 0, j - 1);
            GameObject borderElement = j == 0 ?
                Instantiate(arenaBorderPrefabs[0], borderElementPosition, rotationQuaternion, transform) :
                Instantiate(arenaBorderPrefabs[1], borderElementPosition, rotationQuaternion, transform);
        }

        rotationQuaternion = Quaternion.AngleAxis(180f, Vector3.up);

        for (int i = arenaGridWidth + 1; i > 0; i--)
        {
            borderElementPosition = new Vector3(i - 1, 0, arenaGridLength);
            GameObject borderElement = i == arenaGridWidth + 1 ?
                Instantiate(arenaBorderPrefabs[0], borderElementPosition, rotationQuaternion, transform) :
                Instantiate(arenaBorderPrefabs[1], borderElementPosition, rotationQuaternion, transform);
        }

        rotationQuaternion = Quaternion.AngleAxis(90f, Vector3.up);

        for (int j = arenaGridLength + 1; j > 0; j--)
        {
            borderElementPosition = new Vector3(-1, 0, j - 1);
            GameObject borderElement = j == arenaGridLength + 1 ?
                Instantiate(arenaBorderPrefabs[0], borderElementPosition, rotationQuaternion, transform) :
                Instantiate(arenaBorderPrefabs[1], borderElementPosition, rotationQuaternion, transform);
        }
    }

    private void Awake()
    {
        GenerateArena();
    }
}
