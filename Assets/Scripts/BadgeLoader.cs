using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

public class FirebaseBadgeLoader : MonoBehaviour
{
    public string profileCollection = "profile";
    [SerializeField] private GameObject badgePrefab;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text txtNoBadge; // ✅ UI Text for No Badges Available

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userID;
    private Dictionary<string, int> badgeCounts = new Dictionary<string, int>();
    private int totalBadges = 0; // ✅ Track total badges across collections

    void Start()
    {
        if (badgePrefab == null || loadingPanel == null || txtNoBadge == null)
        {
            Debug.LogError("⚠️ One or more UI elements are not assigned in the Inspector!");
            return;
        }

        txtNoBadge.gameObject.SetActive(false); // ✅ Hide initially

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    userID = auth.CurrentUser.UserId;
                    StartCoroutine(LoadUserBadgesCoroutine());
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
        loadingPanel.SetActive(true);
        totalBadges = 0; // ✅ Reset badge count before loading

        List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();

        for (float i = 1.0f; i <= 10.0f; i += 1.0f)
        {
            string collectionName = i.ToString("0.0", CultureInfo.InvariantCulture);
            badgeCounts[collectionName] = 0;
            tasks.Add(db.Collection(profileCollection).Document(userID).Collection(collectionName).GetSnapshotAsync());
        }

        yield return new WaitUntil(() => tasks.TrueForAll(t => t.IsCompleted));

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

        loadingPanel.SetActive(false);

        // ✅ If no badges exist, show "No badges available" text
        txtNoBadge.gameObject.SetActive(totalBadges == 0);
    }

    void ProcessCollection(string collectionName, QuerySnapshot snapshot)
    {
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
                totalBadges++; // ✅ Increase global badge count
            }
        }

        bool hasPassedDocuments = badgeCount > 0;

        if (panel != null)
        {
            panel.SetActive(hasPassedDocuments);
        }

        if (scrollView != null)
        {
            scrollView.SetActive(hasPassedDocuments);
        }

        badgeCounts[collectionName] = badgeCount;
        UpdateBadgeCountUI(collectionName);
    }

    void AddBadgeToScrollView(string collectionName, string imageName)
    {
        Transform scrollViewTransform = contentPanel.Find("sv" + collectionName);
        GameObject scrollView = scrollViewTransform != null ? scrollViewTransform.gameObject : null;

        if (scrollView != null)
        {
            Transform viewport = scrollView.transform.Find("Viewport");
            Transform contentTransform = viewport != null ? viewport.Find("Content") : null;

            if (contentTransform != null)
            {
                Sprite badgeSprite = Resources.Load<Sprite>("Badges/" + imageName);

                if (badgeSprite != null)
                {
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
