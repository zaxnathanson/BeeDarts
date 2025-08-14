using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TypingEasterEgg : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] private float timeWindow = 2f;
    [SerializeField] private string stringToType;

    private List<KeyEntry> keyPresses = new List<KeyEntry>();

    private struct KeyEntry
    {
        public char key;
        public float time;

        public KeyEntry(char k, float t)
        {
            key = k;
            time = t;
        }
    }

    void Update()
    {
        if (Input.inputString.Length > 0)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsLetter(c))
                {
                    ProcessKeyPress(char.ToLower(c));
                }
            }
        }

        // remove old keys
        keyPresses.RemoveAll(entry => Time.time - entry.time > timeWindow);
    }

    private void ProcessKeyPress(char key)
    {
        keyPresses.Add(new KeyEntry(key, Time.time));
        CheckForSequence();
    }

    private void CheckForSequence()
    {
        if (keyPresses.Count < 5) return;

        // get last 5 chars
        string recent = "";

        for (int i = keyPresses.Count - 5; i < keyPresses.Count; i++)
        {
            recent += keyPresses[i].key;
        }

        if (recent == stringToType)
        {
            float timeDiff = keyPresses[keyPresses.Count - 1].time - keyPresses[keyPresses.Count - 5].time;
            if (timeDiff <= timeWindow)
            {
                OnSequenceDetected();
            }
        }
    }

    private void OnSequenceDetected()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            UnlockUltraBeeModeEpicBeeModeWhereTheBeesGoEverywhereAndItsAwesome();
            Debug.Log("Title scene unlocked.");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            UnlockTheEntireMapAndAlsoSpawnInFiveBeesSoThatYouCanTestTheGameMoreEasily();
            Debug.Log("Main scene unlocked.");
        }

            keyPresses.Clear();
    }

    private void UnlockUltraBeeModeEpicBeeModeWhereTheBeesGoEverywhereAndItsAwesome()
    {
        TitleScreenDartController controller = GetComponent<TitleScreenDartController>();

        controller.maxDarts = 100;
        controller.fireDelay = 0f;
    }

    private void UnlockTheEntireMapAndAlsoSpawnInFiveBeesSoThatYouCanTestTheGameMoreEasily()
    {
        // find all dartboard types including inheritors
        var dartboardTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(Dartboard).IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();

        // find all instances and call raise method
        foreach (var type in dartboardTypes)
        {
            var instances = Object.FindObjectsByType(type, FindObjectsSortMode.None) as Dartboard[];
            foreach (var dartboard in instances)
            {
                dartboard.RaiseAffectedHexagons();
            }
        }

        BeeManager.Instance.RespawnBee(transform.position);
        BeeManager.Instance.RespawnBee(transform.position);
        BeeManager.Instance.RespawnBee(transform.position);
        BeeManager.Instance.RespawnBee(transform.position);
        BeeManager.Instance.RespawnBee(transform.position);
    }
}