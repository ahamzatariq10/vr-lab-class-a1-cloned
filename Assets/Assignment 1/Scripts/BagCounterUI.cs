using TMPro;
using UnityEngine;

public class BagCounterUI : MonoBehaviour
{
    public static BagCounterUI Instance { get; private set; }

    [SerializeField] private TMP_Text label;
    [SerializeField] private string prefix = "Trash bags collected: ";

    private int count = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateLabel();
    }

    public void AddOne()
    {
        count++;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (label != null)
            label.text = prefix + count;
    }
}