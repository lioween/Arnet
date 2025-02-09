using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;


public class BookmarkLoader : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    [SerializeField]
    public GameObject bookmarkPanel;
    public GameObject buttonPrefab;
    public Transform content; // This is now the Scroll View Content
    public TMP_InputField inpSearch;
    public GameObject loadingPanel;
    public GameObject txtNoResult;
    public GameObject txtNoBookmark;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        FetchAndDisplayBookmarks();
    }

    public void FetchAndDisplayBookmarks()
    {
        StartCoroutine(FetchAndDisplayBookmarksCoroutine());
    }

    private IEnumerator FetchAndDisplayBookmarksCoroutine()
    {
        loadingPanel.SetActive(true);

        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("User not authenticated");
            loadingPanel.SetActive(false);
            yield break;
        }

        Debug.Log($"User ID: {currentUser.UserId}");

        // Clear existing buttons
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        Query bookmarkQuery = db
            .Collection("profile")
            .Document(currentUser.UserId)
            .Collection("bookmark");

        Task<QuerySnapshot> fetchTask = bookmarkQuery.GetSnapshotAsync();

        while (!fetchTask.IsCompleted)
        {
            yield return null;
        }

        if (fetchTask.Exception != null)
        {
            Debug.LogError($"Error fetching bookmarks: {fetchTask.Exception.Message}");
            loadingPanel.SetActive(false);
            yield break;
        }

        QuerySnapshot snapshot = fetchTask.Result;
        Debug.Log($"Number of bookmarks: {snapshot.Count}");

        if (snapshot.Count == 0)
        {
            txtNoBookmark.SetActive(true);
            txtNoResult.SetActive(false);
            bookmarkPanel.SetActive(false);
        }
        else
        {
            txtNoBookmark.SetActive(false);
            txtNoResult.SetActive(false);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string sceneName = document.Id;
                string category = document.ContainsField("category") ? document.GetValue<string>("category") : "No Category";
                string title = document.ContainsField("title") ? document.GetValue<string>("title") : "No Title";

                GameObject button = Instantiate(buttonPrefab, content);
                button.transform.Find("txtCategory").GetComponentInChildren<TMP_Text>().text = category;
                button.transform.Find("txtTitle").GetComponentInChildren<TMP_Text>().text = title;

                button.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(sceneName));
            }

            bookmarkPanel.SetActive(true);
        }

        // Force layout update to adjust scroll view
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        loadingPanel.SetActive(false);
    }
}
