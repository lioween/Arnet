using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;

public class FirebaseBadgeLoader : MonoBehaviour
{
    public string profileCollection = "profile";
    [SerializeField] private GameObject badgePrefab;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text txtNoBadge;
    [SerializeField] private GameObject badgeDetailsPanel;
    [SerializeField] private Image badgeDetailsImage;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userID;
    private Dictionary<string, int> badgeCounts = new Dictionary<string, int>();
    private int totalBadges = 0;
    private Dictionary<string, List<string>> allBadgesByCollection = new Dictionary<string, List<string>>();

    void Start()
    {
        if (badgePrefab == null || loadingPanel == null || txtNoBadge == null || badgeDetailsPanel == null || badgeDetailsImage == null)
        {
            Debug.LogError("⚠️ One or more UI elements are not assigned in the Inspector!");
            return;
        }

        txtNoBadge.gameObject.SetActive(false);
        badgeDetailsPanel.SetActive(false);

        LoadBadgesFromResources();

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

    void LoadBadgesFromResources()
    {
        allBadgesByCollection.Clear();

        Sprite[] allBadges = Resources.LoadAll<Sprite>("Badges");
        foreach (Sprite badge in allBadges)
        {
            string badgeName = badge.name;
            if (badgeName.Length < 2 || !char.IsDigit(badgeName[0])) continue; // Skip invalid names

            string collectionKey = badgeName[0] + ".0"; // Categorize by first digit (1.0, 2.0, etc.)

            if (!allBadgesByCollection.ContainsKey(collectionKey))
            {
                allBadgesByCollection[collectionKey] = new List<string>();
            }

            allBadgesByCollection[collectionKey].Add(badgeName);
        }
    }

    IEnumerator LoadUserBadgesCoroutine()
    {
        loadingPanel.SetActive(true);
        totalBadges = 0;
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
        txtNoBadge.gameObject.SetActive(totalBadges == 0);
    }

    void ProcessCollection(string collectionName, QuerySnapshot snapshot)
    {
        Transform panelTransform = contentPanel.Find(collectionName);
        GameObject panel = panelTransform != null ? panelTransform.gameObject : null;

        Transform scrollViewTransform = contentPanel.Find("sv" + collectionName);
        GameObject scrollView = scrollViewTransform != null ? scrollViewTransform.gameObject : null;

        HashSet<string> passedBadges = new HashSet<string>();
        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.ContainsField("status") && doc.GetValue<string>("status") == "passed")
            {
                passedBadges.Add(doc.Id);
                totalBadges++;
            }
        }

        if (passedBadges.Count > 0) // ✅ Show collection only if there is at least one "passed" badge
        {
            if (allBadgesByCollection.ContainsKey(collectionName))
            {
                foreach (string badge in allBadgesByCollection[collectionName])
                {
                    bool isUnlocked = passedBadges.Contains(badge);
                    AddBadgeToScrollView(collectionName, badge, isUnlocked);
                }
            }

            if (panel != null) panel.SetActive(true);
            if (scrollView != null) scrollView.SetActive(true);
        }
        else
        {
            if (panel != null) panel.SetActive(false);
            if (scrollView != null) scrollView.SetActive(false);
        }

        badgeCounts[collectionName] = passedBadges.Count;
        UpdateBadgeCountUI(collectionName);
    }

    void AddBadgeToScrollView(string collectionName, string imageName, bool isUnlocked)
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
                    Image imageComponent = newBadge.GetComponent<Image>();

                    if (imageComponent != null)
                    {
                        imageComponent.sprite = badgeSprite;
                        Button badgeButton = newBadge.GetComponent<Button>();

                        if (isUnlocked)
                        {
                            if (badgeButton != null)
                            {
                                badgeButton.onClick.AddListener(() => ShowBadgeDetails(badgeSprite));
                            }
                        }
                        else
                        {
                            if (badgeButton != null)
                            {
                                badgeButton.interactable = false;
                            }
                            imageComponent.color = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f);
                        }
                    }
                }
            }
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
                    countText.text = $"{badgeCounts[collectionName]}/5"; // ✅ Only counting passed badges
                }
            }
        }
    }

    void ShowBadgeDetails(Sprite badgeSprite)
    {
        badgeDetailsImage.sprite = badgeSprite;
        badgeDetailsPanel.SetActive(true);
    }

    public void CloseBadgeDetails()
    {
        badgeDetailsPanel.SetActive(false);
    }
}
