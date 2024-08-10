using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class PlacementController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject sphereFab;
    [SerializeField]
    private GameObject equipmentFab;

    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private GameObject inputPanel;
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private UnityEngine.UI.Button confirmButton;

    [SerializeField]
    private GameObject onoffPanel;
    [SerializeField]
    private TextMeshProUGUI equiptName;
    [SerializeField]
    private UnityEngine.UI.Button onButton;
    [SerializeField]
    private UnityEngine.UI.Button offButton;

    // Constants
    private float fetchedScaleValue = 1f; // Scale
    private string url = "https://10.5.20.123/val";
    private float fetchInterval = 1f; // Sec

    private ARRaycastManager arRaycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedSpheres = new List<GameObject>();
    private List<GameObject> placedCubes = new List<GameObject>();
    private GameObject currentCube;

    private Vector2 touchPosition;

    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom) ConnectToPhoton();
        StartCoroutine(FetchValueContinuously());
        inputPanel.SetActive(false);
        onoffPanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        onButton.onClick.AddListener(OnOnButtonClick);
        offButton.onClick.AddListener(OnOffButtonClick);
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return; // Ensure we're in a room before allowing placements

        // Object Placement Logic
        if (Input.touchCount > 0 && !inputPanel.activeSelf && !onoffPanel.activeSelf)
        {
            var touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.position.y < Screen.height * 0.9 && touch.phase == TouchPhase.Began)
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    var hitPose = hits[0].pose;
                    if (toggle.isOn)
                    {
                        GameObject newCube = PhotonNetwork.Instantiate(equipmentFab.name, hitPose.position, hitPose.rotation);
                        newCube.GetComponent<cubeBehavior>().createTag().setLoc(hitPose.position).getstr();
                        placedCubes.Add(newCube);
                    }
                    else
                    {
                        GameObject newSphere = PhotonNetwork.Instantiate(sphereFab.name, hitPose.position, hitPose.rotation);
                        newSphere.GetComponent<sphereBehavior>().createTag().setLoc(hitPose.position).setScale(fetchedScaleValue).getstr();
                        placedSpheres.Add(newSphere);
                    }
                }
            }
        }

        // Object Interaction Logic
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1.0f))
        {
            GameObject hitObject = hitInfo.collider.gameObject;

            if (placedSpheres.Contains(hitObject))
            {
                hitObject.GetComponent<sphereBehavior>().setScale(fetchedScaleValue).getstr();
                inputPanel.SetActive(false);
                onoffPanel.SetActive(false);
            }
            else if (placedCubes.Contains(hitObject))
            {
                currentCube = hitObject;
                if (currentCube.GetComponent<cubeBehavior>().nameNotSet())
                {
                    inputPanel.SetActive(true);
                }
                else
                {
                    onoffPanel.SetActive(true);
                    equiptName.text = currentCube.GetComponent<cubeBehavior>().getName();
                    if (currentCube.GetComponent<cubeBehavior>().getStatus())
                    {
                        changeColor(onButton, UnityEngine.Color.green);
                        changeColor(offButton, UnityEngine.Color.white);
                    }
                    else
                    {
                        changeColor(onButton, UnityEngine.Color.white);
                        changeColor(offButton, UnityEngine.Color.red);
                    }
                }
            }
        }
        else
        {
            inputPanel.SetActive(false);
            onoffPanel.SetActive(false);
        }
    }

    public void OnConfirmButtonClick()
    {
        if (currentCube != null)
        {
            string textInput = inputField.text.Clone().ToString();
            currentCube.GetComponent<cubeBehavior>().setName(textInput);
            inputPanel.SetActive(false);
        }
    }

    public void OnOnButtonClick()
    {
        if (currentCube != null)
        {
            currentCube.GetComponent<cubeBehavior>().setStatus(true);
            onoffPanel.SetActive(false);
        }
    }

    public void OnOffButtonClick()
    {
        if (currentCube != null)
        {
            currentCube.GetComponent<cubeBehavior>().setStatus(false);
            onoffPanel.SetActive(false);
        }
    }

    IEnumerator FetchValueContinuously()
    {
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.certificateHandler = new BypassCertificate();
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    try
                    {
                        string jsonResponse = webRequest.downloadHandler.text;
                        ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);
                        fetchedScaleValue = responseData.value;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error parsing JSON: " + e.Message);
                    }
                }
            }

            yield return new WaitForSeconds(fetchInterval);
        }
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    [System.Serializable]
    private class ResponseData
    {
        public float value;
    }

    private void changeColor(Button btn, UnityEngine.Color newColor)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = newColor;
        cb.highlightedColor = newColor;
        cb.pressedColor = newColor;
        cb.selectedColor = newColor;
        btn.colors = cb;
    }

    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu"; // Adjust as needed
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        if (!PhotonNetwork.InRoom)
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 20, IsVisible = true, IsOpen = true };
            PhotonNetwork.JoinOrCreateRoom("Room", roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
        ConnectToPhoton();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Failed: " + message);
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 });
    }
}
