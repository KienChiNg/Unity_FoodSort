using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Header : MonoBehaviour
{
    [SerializeField] private Plate _plate;

    public Plate Plate { get => _plate; set => _plate = value; }
}
