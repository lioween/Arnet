using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro; // Import TextMeshPro namespace
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

public class FirebaseBadgeLoader : MonoBehaviour
{
    public string profileCollection = "profile"; // Firestore collection
    [SerializeField] private GameObject badgePrefab; // Prefab for badge images
    [SerializeField] private Transform contentPanel; // Assign `svBadges/Viewport/Content`
    [SerializeField] private GameObject loadingPanel; // ✅ New: Loading Panel

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userID;
    private Dictionary<string, int> badgeCounts = new Dictionary<string, int>(); // Store badge counts per collection

    void Start()
    {
        if (badgePrefab == null)
        {
            Debug.LogError("⚠️ Badge Prefab is not assigned in the Inspector!");
            return;
        }

        if (loadingPanel == null)
        {
            Debug.LogError("⚠️ Loading Panel is not assigned in the Inspector!");
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    userID = auth.CurrentUser.UserId;
                    StartCoroutine(LoadUserBadgesCoroutine()); // ✅ Coroutine to load badges
                }
                else
                {
                    Debug.LogError("User is not logged in.");
                }
            }
            else
            {
                Debug.LogError("Firebase dependencies not available.");
            }
        });
    }

    IEnumerator LoadUserBadgesCoroutine()
    {
        loadingPanel.SetActive(true); // ✅ Show loading panel before fetching data

        List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();

        for (float i = 1.0f; i <= 10.0f; i += 1.0f) // Loop through collections 1.0 to 10.0
        {
            string collectionName = i.ToString("0.0", CultureInfo.InvariantCulture);
            badgeCounts[collectionName] = 0;
            tasks.Add(db.Collection(profileCollection).Document(userID).Collection(collectionName).GetSnapshotAsync());
        }

        yield return new WaitUntil(() => tasks.TrueForAll(t => t.IsCompleted)); // ✅ Wait until all Firebase calls finish

        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].IsCompletedSuccessfully)
            {
                string collectionName = (i + 1.0f).ToString("0.0", CultureInfo.InvariantCulture);
                QuerySnapshot snapshot = tasks[i].Result;
                ProcessCollection(collectionName, snapshot);
            }
            else
            {
                Debug.LogError($"❌ Firebase failed for collection {(i + 1.0f).ToString("0.0", CultureInfo.InvariantCulture)}");
            }
        }

        loadingPanel.SetActive(false); // ✅ Hide loading panel after all collections are checked
    }

    void ProcessCollection(string collectionName, QuerySnapshot snapshot)
    {
        // ✅ Correct way to find inactive objects
        Transform panelTransform = contentPanel.Find(collectionName);
        GameObject panel = panelTransform != null ? panelTransform.gameObject : null;

        Transform scrollViewTransform = contentPanel.Find("sv" + collectionName);
        GameObject scrollView = scrollViewTransform != null ? scrollViewTransform.gameObject : null;

        int badgeCount = 0;

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.ContainsField("status") && doc.GetValue<string>("status") == "passed")
            {
                string imageName = doc.Id;
                AddBadgeToScrollView(collectionName, imageName);
                badgeCount++;
            }
        }

        // ✅ Only show the Panel & ScrollView if at least 1 document has `status="passed"`
        bool hasPassedDocuments = badgeCount > 0;

        if (panel != null)
        {
            panel.SetActive(hasPassedDocuments);
        }
        else
        {
            Debug.LogWarning($"⚠️ Panel {collectionName} not found inside Content.");
        }

        if (scrollView != null)
        {
            scrollView.SetActive(hasPassedDocuments);
        }
        else
        {
            Debug.LogWarning($"⚠️ ScrollView sv{collectionName} not found inside Content.");
        }

        if (hasPassedDocuments)
        {
            Debug.Log($"✅ Collection {collectionName} has `status=passed`, activating Panel & ScrollView.");
        }
        else
        {
            Debug.Log($"❌ No `status=passed` in Collection {collectionName}, keeping Panel & ScrollView inactive.");
        }

        // Store badge count for UI updates
        badgeCounts[collectionName] = badgeCount;
        UpdateBadgeCountUI(collectionName);
    }



    void AddBadgeToScrollView(string collectionName, string imageName)
    {
        // ✅ Find ScrollView inside Content
        Transform scrollViewTransform = contentPanel.Find("sv" + collectionName);
        GameObject scrollView = scrollViewTransform != null ? scrollViewTransform.gameObject : null;

        if (scrollView != null)
        {
            // ✅ Correct way to find Content inside ScrollView
            Transform viewport = scrollView.transform.Find("Viewport");
            Transform contentTransform = viewport != null ? viewport.Find("Content") : null;

            if (contentTransform != null)
            {
                // ✅ Load image from Resources folder
                Sprite badgeSprite = Resources.Load<Sprite>("Badges/" + imageName);

                if (badgeSprite != null)
                {
                    // ✅ Instantiate badge prefab inside ScrollView Content
                    GameObject newBadge = Instantiate(badgePrefab, contentTransform);
                    UnityEngine.UI.Image imageComponent = newBadge.GetComponent<UnityEngine.UI.Image>();

                    if (imageComponent != null)
                    {
                        imageComponent.sprite = badgeSprite;
                        Debug.Log($"✅ Added badge {imageName} to ScrollView: {collectionName}");
                    }
                    else
                    {
                        Debug.LogError("⚠️ Badge prefab is missing an Image component.");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Image {imageName} not found in Resources/Badges/");
                }
            }
            else
            {
                Debug.LogError($"❌ Content not found inside ScrollView: sv{collectionName}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ ScrollView sv{collectionName} not found inside Content.");
        }
    }


    void UpdateBadgeCountUI(string collectionName)
    {
        Transform panel = contentPanel.Find(collectionName);

        if (panel != null)
        {
            Transform txtTotalTransform = panel.Find("txtTotal");

            if (txtTotalTransform != null)
            {
                TMP_Text countText = txtTotalTransform.GetComponent<TMP_Text>();

                if (countText != null)
                {
                    countText.text = $"{badgeCounts[collectionName]}/5";
                    Debug.Log($"🏆 Updated badge count for {collectionName}: {badgeCounts[collectionName]}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ TMP_Text component missing from 'txtTotal' inside Panel {collectionName}.");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ 'txtTotal' not found inside Panel {collectionName}.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Panel {collectionName} not found inside Content.");
        }
    }
}
