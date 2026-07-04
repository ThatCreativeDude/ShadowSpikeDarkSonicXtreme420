using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int weakCount;
        public int chargerCount;
        public float spawnInterval = 0.4f;
    }


    public Wave[] waves;
    public GameObject weakEnemyPrefab;
    public GameObject chargerEnemyPrefab;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 3f;

    public int CurrentWave = -1;
    public bool AllWavesComplete = false;
    public int AliveEnemyCount => aliveEnemies.Count;

    public event System.Action<int> OnWaveStart;
    public event System.Action OnAllWavesComplete;

  [SerializeField] private TextMeshProUGUI remainingEnemiesText; 
  [SerializeField] private TextMeshProUGUI waveText; 
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private List<Transform> spawnPointBag = new List<Transform>();
    private int spawnPointIndex;


    void Start()
    {            
        waveText.gameObject.SetActive(false);
        if (waves == null || waves.Length == 0)
        {
            return;
        }

        StartCoroutine(RunWaves());
    }
    void Update()
    {
        if(!AllWavesComplete)
        {
        remainingEnemiesText.text = "Remaining Enemies: " + AliveEnemyCount;
        waveText.text = "Wave: " + (CurrentWave + 1) + "/" + waves.Length;
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
    

    IEnumerator RunWaves()
    {
        while (CurrentWave < waves.Length - 1)
        {
            CurrentWave++;
            waveText.gameObject.SetActive(true);
       //     waveText.gameObject.GetComponent<Animator>().Play("pop");
            yield return new WaitForSeconds(3f);
            waveText.gameObject.SetActive(false);
            OnWaveStart?.Invoke(CurrentWave);

            yield return StartCoroutine(SpawnWave(waves[CurrentWave]));

            yield return new WaitUntil(AllEnemiesDead);

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        AllWavesComplete = true;
        OnAllWavesComplete?.Invoke();
        remainingEnemiesText.text = "Press 'R' to Restart";
        waveText.gameObject.SetActive(true);
        waveText.text = "Hooray!\nAll waves complete!";
        yield return new WaitForSeconds(.4f);
        waveText.gameObject.GetComponent<Animator>().Play("Hooray");

    }

    IEnumerator SpawnWave(Wave wave)
    {
        List<GameObject> spawnList = new List<GameObject>();

        for (int i = 0; i < wave.weakCount; i++)
            spawnList.Add(weakEnemyPrefab);

        for (int i = 0; i < wave.chargerCount; i++)
            spawnList.Add(chargerEnemyPrefab);

        Shuffle(spawnList);

        foreach (GameObject prefab in spawnList)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || spawnPoints.Length == 0) return;

        Transform point = GetNextSpawnPoint();
        GameObject enemy = Instantiate(prefab, point.position, point.rotation);
        aliveEnemies.Add(enemy);
    }

    Transform GetNextSpawnPoint()
    {
        if (spawnPointIndex >= spawnPointBag.Count)
        {
            spawnPointBag = new List<Transform>(spawnPoints);
            Shuffle(spawnPointBag);
            spawnPointIndex = 0;
        }

        Transform point = spawnPointBag[spawnPointIndex];
        spawnPointIndex++;
        return point;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    bool AllEnemiesDead()
    {
        aliveEnemies.RemoveAll(e => e == null);
        return aliveEnemies.Count == 0;
    }
}