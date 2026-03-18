using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro

public class ScoreEvaluator : MonoBehaviour
{
    [Header("References")]
    public List<JarContentsTracker> jars = new List<JarContentsTracker>();
    public TMP_Text scoreText; // Assign your TextMeshPro - Text (UI) here

    [Header("Scoring Parameters")]
    public float jarScoreConstant = 1f;
    public float floorMarblePenalty = 1f;
    public float evaluationInterval = 2f;

    private void Start()
    {
        StartCoroutine(EvaluateScoreLoop());
    }

    IEnumerator EvaluateScoreLoop()
    {
        while (true)
        {
            EvaluateScore();
            yield return new WaitForSeconds(evaluationInterval);
        }
    }

    void EvaluateScore()
    {
        int totalMarblesInJars = 0;
        int totalMarblesInScene = FindObjectsOfType<MarbleColor>().Length;

        if (totalMarblesInScene == 0) return;

        float sumPurity = 0f;
        int jarsWithMarbles = 0;

        foreach (var jar in jars)
        {
            List<GameObject> marbles = jar.GetAllMarbles();
            int totalMarbles = marbles.Count;

            if (totalMarbles == 0)
                continue;

            jarsWithMarbles++;
            totalMarblesInJars += totalMarbles;

            // Count marbles per color
            Dictionary<int, int> colorCounts = new Dictionary<int, int>();
            foreach (var marble in marbles)
            {
                MarbleColor color = marble.GetComponent<MarbleColor>();
                if (color == null) continue;

                if (!colorCounts.ContainsKey(color.colorId))
                    colorCounts[color.colorId] = 0;

                colorCounts[color.colorId]++;
            }

            // Find dominant color
            int maxSameColor = 0;
            foreach (var kvp in colorCounts)
            {
                if (kvp.Value > maxSameColor)
                    maxSameColor = kvp.Value;
            }

            float jarPurity = (float)maxSameColor / totalMarbles;
            sumPurity += jarPurity;
        }

        // Avoid division by zero
        float averagePurity = (jarsWithMarbles > 0) ? sumPurity / jarsWithMarbles : 0f;

        // Ratio of marbles inside jars
        float insideRatio = (float)totalMarblesInJars / totalMarblesInScene;

        // Target score as percentage
        float targetScore = averagePurity * insideRatio * 100f;

        // Update TextMeshPro UI
        if (scoreText != null)
            scoreText.text = $"Total Score: {targetScore:0.##}%\n" +
                            $"Chemical Purity: {averagePurity * 100f:0.##}%\n" +
                            $"Lost Chemicals: {100f - insideRatio * 100f:0.##}%";
    }
}