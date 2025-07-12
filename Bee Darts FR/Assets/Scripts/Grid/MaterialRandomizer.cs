using UnityEngine;

public class MaterialRandomizer : MonoBehaviour
{
    private Material material;

    [SerializeField] private int randomFactor = 50;

    private byte randomValue;

    private const byte COLOR_MAX = 255;

    void Start()
    {
        material = GetComponent<Renderer>().material;

        randomValue = (byte)Random.Range((COLOR_MAX - randomFactor), COLOR_MAX);

        material.color = new Color32(randomValue, randomValue, randomValue, 255);
    }
}
