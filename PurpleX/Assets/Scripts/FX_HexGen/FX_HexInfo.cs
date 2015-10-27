/*
 * This script holds a Hexes position value that is used for distance calculations.
 * & its current amount of units per player 
 */

using UnityEngine;
using System.Collections;

public class FX_HexInfo : MonoBehaviour {
	public Vector3 HexPosition;
    // reference to all the units located at this hex
    public GameObject[] units = new GameObject[10];
    // whether units belong to player 1 or 2
    // both is not possible -> fight (till only one remains)
    public bool player1;
}
