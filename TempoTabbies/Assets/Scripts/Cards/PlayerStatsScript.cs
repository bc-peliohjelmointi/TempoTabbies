using UnityEngine;

public class PlayerStatsScript : MonoBehaviour
{
    [System.Serializable]
    public class PlayerStats
    {
        public string PlayerName;
        public int Combo;
        public int Score;
        public float Accuracy;

        public void AddCombo(int amount)
        {
            Combo = Mathf.Max(0, Combo + amount);
        }

        public void ResetCombo()
        {
            Combo = 0;
        }
        public void AddScore(int amount)
        {
            Combo = Mathf.Max(0, Combo + amount);
        }
        public void DebugStats()
        {
            Debug.Log($"[{PlayerName}] Combo: {Combo}, Score: {Score}, Accuracy: {Accuracy}");
        }
    }
}
