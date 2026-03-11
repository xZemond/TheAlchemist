// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class MarbleScoreEvaluator : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private JarContentsTracker[] jars;

//     [Tooltip("Optional parent containing all marbles. If null, all MarbleColor components in the scene are used.")]
//     [SerializeField] private Transform allMarblesRoot;

//     [Header("Jar Score")]
//     [Tooltip("Temperature/scale for the exponential score. Larger values reduce growth.")]
//     [SerializeField] private float expScale = 8f;

//     [Tooltip("Multiplier for the exponential jar term.")]
//     [SerializeField] private float jarScoreMultiplier = 100f;

//     [Tooltip("Small additional linear reward per dominant-color marble.")]
//     [SerializeField] private float dominantLinearReward = 1f;

//     [Tooltip("Bonus added when a jar contains only one color.")]
//     [SerializeField] private float pureJarBonus = 50f;

//     [Header("Outside / Floor Penalty")]
//     [Tooltip("Linear penalty per marble that is not inside any jar after the grace time.")]
//     [SerializeField] private float penaltyPerOutsideMarble = 30f;

//     [Tooltip("A marble is only penalized if it has been outside all jars for longer than this time.")]
//     [SerializeField] private float outsideGraceSeconds = 1.0f;

//     [Header("Debug Output")]
//     [SerializeField] private bool logEverySecond = true;
//     [SerializeField] private float logIntervalSeconds = 1.0f;

//     private float logTimer;

//     // Tracks how long each marble has continuously been outside all jars
//     private readonly Dictionary<MarbleColor, float> outsideTimers = new Dictionary<MarbleColor, float>();

//     private void Awake()
//     {
//         Debug.Log("[MarbleScoreEvaluator] Awake", this);
//     }

//     private void Start()
//     {
//         Debug.Log("[MarbleScoreEvaluator] Start", this);
//     }

//     private void Update()
//     {
//         UpdateOutsideTimers();

//         if (!logEverySecond) return;

//         logTimer += Time.deltaTime;
//         if (logTimer >= logIntervalSeconds)
//         {
//             logTimer = 0f;
//             Debug.Log(BuildScoreReport(out double score), this);
//         }
//     }

//     [ContextMenu("Print Score Now")]
//     public void PrintScoreNow()
//     {
//         UpdateOutsideTimers();
//         Debug.Log(BuildScoreReport(out double score), this);
//     }

//     private string BuildScoreReport(out double totalScore)
//     {
//         if (jars == null)
//             jars = Array.Empty<JarContentsTracker>();

//         HashSet<MarbleColor> inAnyJar = new HashSet<MarbleColor>();
//         double sumJarScores = 0.0;

//         var report = new System.Text.StringBuilder();
//         report.AppendLine("=== Marble Score Evaluation ===");

//         for (int j = 0; j < jars.Length; j++)
//         {
//             JarContentsTracker jar = jars[j];
//             if (jar == null)
//             {
//                 report.AppendLine($"Jar {j}: <null>");
//                 continue;
//             }

//             jar.CleanupNulls();

//             Dictionary<int, int> counts = new Dictionary<int, int>();
//             int jarTotal = 0;

//             foreach (MarbleColor marble in jar.Inside)
//             {
//                 if (marble == null) continue;

//                 jarTotal++;
//                 inAnyJar.Add(marble);

//                 int colorId = marble.colorId;
//                 counts.TryGetValue(colorId, out int currentCount);
//                 counts[colorId] = currentCount + 1;
//             }

//             int dominantCount = 0;
//             int dominantColorId = -1;

//             foreach (KeyValuePair<int, int> kv in counts)
//             {
//                 if (kv.Value > dominantCount)
//                 {
//                     dominantCount = kv.Value;
//                     dominantColorId = kv.Key;
//                 }
//             }

//             int offColorCount = Mathf.Max(0, jarTotal - dominantCount);

//             double exponentialTerm =
//                 SafeExp(dominantCount / expScale) -
//                 SafeExp(offColorCount / expScale);

//             double jarScore = jarScoreMultiplier * exponentialTerm;
//             jarScore += dominantLinearReward * dominantCount;

//             if (jarTotal > 0 && offColorCount == 0)
//                 jarScore += pureJarBonus;

//             // Keep each jar contribution non-negative
//             jarScore = Math.Max(0.0, jarScore);

//             sumJarScores += jarScore;

//             report.Append($"Jar {j}: total={jarTotal}, dominantColor=c{dominantColorId}, d={dominantCount}, o={offColorCount}, jarScore={jarScore:0.##}");

//             if (counts.Count > 0)
//             {
//                 report.Append(" | ");
//                 bool first = true;
//                 foreach (KeyValuePair<int, int> kv in counts)
//                 {
//                     if (!first) report.Append(", ");
//                     first = false;
//                     report.Append($"c{kv.Key}:{kv.Value}");
//                 }
//             }

//             report.AppendLine();
//         }

//         int totalMarbles = CountTotalMarbles();
//         int outsidePenaltyCount = CountOutsidePenaltyMarbles();

//         double outsidePenalty = outsidePenaltyCount * penaltyPerOutsideMarble;
//         double rawTotal = sumJarScores - outsidePenalty;

//         totalScore = Math.Max(0.0, rawTotal);

//         report.AppendLine("---");
//         report.AppendLine($"TotalMarbles: {totalMarbles}");
//         report.AppendLine($"InAnyJar: {inAnyJar.Count}");
//         report.AppendLine($"OutsidePenaltyCount: {outsidePenaltyCount} => OutsidePenalty: -{outsidePenalty:0.##}");
//         report.AppendLine($"SumJarScores: {sumJarScores:0.##}");
//         report.AppendLine($"RAW TOTAL: {rawTotal:0.##}");
//         report.AppendLine($"TOTAL SCORE (>= 0): {totalScore:0.##}");

//         return report.ToString();
//     }

//     private void UpdateOutsideTimers()
//     {
//         MarbleColor[] allMarbles = GetAllMarbles();
//         HashSet<MarbleColor> inAnyJar = GetMarblesInAnyJar();

//         // Add/update timers
//         for (int i = 0; i < allMarbles.Length; i++)
//         {
//             MarbleColor marble = allMarbles[i];
//             if (marble == null) continue;

//             if (inAnyJar.Contains(marble))
//             {
//                 outsideTimers[marble] = 0f;
//             }
//             else
//             {
//                 if (!outsideTimers.ContainsKey(marble))
//                     outsideTimers[marble] = 0f;

//                 outsideTimers[marble] += Time.deltaTime;
//             }
//         }

//         // Remove destroyed marbles from the dictionary
//         List<MarbleColor> toRemove = null;
//         foreach (KeyValuePair<MarbleColor, float> kv in outsideTimers)
//         {
//             if (kv.Key == null)
//             {
//                 if (toRemove == null) toRemove = new List<MarbleColor>();
//                 toRemove.Add(kv.Key);
//             }
//         }

//         if (toRemove != null)
//         {
//             for (int i = 0; i < toRemove.Count; i++)
//                 outsideTimers.Remove(toRemove[i]);
//         }
//     }

//     private int CountOutsidePenaltyMarbles()
//     {
//         int count = 0;

//         foreach (KeyValuePair<MarbleColor, float> kv in outsideTimers)
//         {
//             if (kv.Key == null) continue;
//             if (kv.Value > outsideGraceSeconds)
//                 count++;
//         }

//         return count;
//     }

//     private HashSet<MarbleColor> GetMarblesInAnyJar()
//     {
//         HashSet<MarbleColor> inAnyJar = new HashSet<MarbleColor>();

//         if (jars == null)
//             return inAnyJar;

//         for (int j = 0; j < jars.Length; j++)
//         {
//             JarContentsTracker jar = jars[j];
//             if (jar == null) continue;

//             jar.CleanupNulls();

//             foreach (MarbleColor marble in jar.Inside)
//             {
//                 if (marble != null)
//                     inAnyJar.Add(marble);
//             }
//         }

//         return inAnyJar;
//     }

//     private MarbleColor[] GetAllMarbles()
//     {
//         if (allMarblesRoot != null)
//             return allMarblesRoot.GetComponentsInChildren<MarbleColor>(true);

//         return FindObjectsOfType<MarbleColor>(true);
//     }

//     private int CountTotalMarbles()
//     {
//         MarbleColor[] marbles = GetAllMarbles();

//         int count = 0;
//         for (int i = 0; i < marbles.Length; i++)
//         {
//             if (marbles[i] != null)
//                 count++;
//         }

//         return count;
//     }

//     private static double SafeExp(float x)
//     {
//         const double maxInput = 80.0;
//         double clamped = Math.Max(-maxInput, Math.Min(maxInput, x));
//         return Math.Exp(clamped);
//     }
// }