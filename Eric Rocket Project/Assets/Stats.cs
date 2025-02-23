using UnityEngine;
using TMPro;
using System;

public class Stats : MonoBehaviour
{
    // static instance of the Stats class
    public static Stats Instance;

    public TextMeshProUGUI statText;
    public int successCount;
    public int failureCount;
    public int crashCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int totalCount = successCount + failureCount;
        if (totalCount == 0) {
            totalCount = 1; // avoid division by zero
        }
        statText.text = $"Accuracy: {Math.Round((successCount / (totalCount * 1.0f)) * 100, 2)}%\nSuccesses: {successCount}\nFailures: {failureCount}\nCrashes: {crashCount}";
    }
}
