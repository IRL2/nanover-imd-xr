using System;
using Nanover.Frontend.UI;
using UnityEngine;

public class ReplicateLocation : MonoBehaviour
{
    [SerializeField]
    public Transform source;

    void Start()
    {
        transform.position = source.position;
        transform.rotation = source.rotation;

        Debug.Log("placing panel at source location");
    }


}
