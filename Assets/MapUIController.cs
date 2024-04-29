using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    public int currentFloor = 0;
    private int currentPlayerFloor = 0;
    private bool syncPlayerPosition = true;

    [Header("UI Elements")]
    public TextMeshProUGUI currentFloorText;
    public RawImage mapImage;
    public Image playerImage;
    public Button[] floorButtons;
    public ScrollRect scrollView;
    public GameObject[] floorIconsParents;
    [Header("References")]
    public GameObject mapIcon;
    private Transform playerPosition;
    public GameObject mapUI;

    // Singleton
    public static MapUIController Instance;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // assign button click events
        for (int i = 0; i < floorButtons.Length; i++)
        {
            int floor = i;
            floorButtons[i].onClick.AddListener(() => SetFloor(floor));
        }
        playerPosition = GameManager.Instance.player.transform;
    }

    public void Update()
    {
        if(!mapUI.activeSelf)
        {
            return;
        }
        Vector3 normalizedPlayerPos = new Vector3(playerPosition.position.x / Generator3D.gridSize, playerPosition.position.y / Generator3D.gridSize, playerPosition.position.z / Generator3D.gridSize);
        float scaleFactor = 853.3333f / Generator3D.size.x; // 853.3333 is the size of the map image in pixels also asuming the map is square
        playerImage.rectTransform.anchoredPosition = new Vector3(normalizedPlayerPos.x * scaleFactor, normalizedPlayerPos.z * scaleFactor, 0);
        currentPlayerFloor = Mathf.CeilToInt(normalizedPlayerPos.y)-1;

        if (syncPlayerPosition)
        {
            SetFloor(currentPlayerFloor);
            // Center Scrollview onto player image
            scrollView.verticalNormalizedPosition =  playerImage.rectTransform.anchoredPosition.y / scrollView.content.rect.size.y;
            scrollView.horizontalNormalizedPosition = playerImage.rectTransform.anchoredPosition.x / scrollView.content.rect.size.x;
        }
    }

    public void GenerateMapIcon(Sprite sprite,Vector3 position, int[] floors)
    {
        foreach (int floor in floors)
        {
            GameObject icon = Instantiate(mapIcon, floorIconsParents[floor].transform);

            Vector3 normalizedPosition = new Vector3(position.x / Generator3D.gridSize, position.y / Generator3D.gridSize, position.z / Generator3D.gridSize);
            float scaleFactor = 853.3333f / Generator3D.size.x;
            normalizedPosition *= scaleFactor;
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector3(normalizedPosition.x, normalizedPosition.z, 0);
            icon.GetComponent<Image>().sprite = sprite;

        }
    }

    public void SetFloor(int floor)
    {
        currentFloor = floor;
        string floorLetter = "";
        switch (floor)
        {
            case 0:
                floorLetter = "U2";
                break;
            case 1:
                floorLetter = "U1";
                break;
            case 2:
                floorLetter = "E0";
                break;
            case 3:
                floorLetter = "E1";
                break;
            case 4:
                floorLetter = "E2";
                break;
            default:
                floorLetter = "???";
                break;
        }
        foreach (GameObject floorIconsParent in floorIconsParents)
        {
            floorIconsParent.SetActive(false);
        }
        floorIconsParents[currentFloor].SetActive(true);
        currentFloorText.text = floorLetter;
        mapImage.texture = MapGenerator.Instance.mapTextures[currentFloor];
    }

}
