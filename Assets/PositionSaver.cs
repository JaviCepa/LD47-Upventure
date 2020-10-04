using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionSaver : MonoBehaviour
{
    Vector3 startPosition;

    [Button]
    void SaveStartPosition()
    {
        startPosition = transform.position;
    }

    [Button]
    void GoToStartPosition()
    {
        transform.position = startPosition;
    }
}
