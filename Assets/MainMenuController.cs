using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Linq; // Para ordenar la lista

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject rankingsPanel;

    [Header("Main Menu Elements")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button playButton;
    [SerializeField] private Button rankingsButton;
    [SerializeField] private Button settingsButton;

    [Header("Settings Panel")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeSettingsButton;

    [Header("Rankings Panel")]
    [SerializeField] private Transform contentParent; // El objeto "Content" del Scroll View
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Button closeRankingsButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;
    // NUEVO: Variables para asignar los componentes y clips específicos
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip buttonClickSound;

    void Start()
    {
        // --- Reproducir Música de Fondo ---
        if (musicSource != null && menuMusicClip != null)
        {
            musicSource.clip = menuMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
        // --- Conectar Listeners ---
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        rankingsButton.onClick.AddListener(ShowRankings);
        closeSettingsButton.onClick.AddListener(() => HidePanel(settingsPanel));
        closeRankingsButton.onClick.AddListener(() => HidePanel(rankingsPanel));
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);

        // NUEVO: Añadimos el sonido de clic a todos los botones
        playButton.onClick.AddListener(PlayClickSound);
        settingsButton.onClick.AddListener(PlayClickSound);
        rankingsButton.onClick.AddListener(PlayClickSound);
        closeSettingsButton.onClick.AddListener(PlayClickSound);
        closeRankingsButton.onClick.AddListener(PlayClickSound);

        // --- Inicialización ---
        settingsPanel.SetActive(false);
        rankingsPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panel) => panel.SetActive(true);
    private void HidePanel(GameObject panel) => panel.SetActive(false);

    public void PlayGame()
    {
        string nickname = nicknameInput.text;
        if (string.IsNullOrWhiteSpace(nickname))
        {
            // Opcional: mostrar un aviso si el nombre está vacío
            nickname = "Player";
        }

        // Guardamos el nombre del jugador para usarlo en la escena de juego.
        PlayerPrefs.SetString("CurrentPlayerName", nickname);
        SceneManager.LoadScene("GameScene"); // Asegúrate de que tu escena de juego se llame así
    }

    public void ShowRankings()
    {
        // Limpiamos la lista anterior
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        RankingsData data = SaveSystem.LoadRankings();

        // Ordenamos la lista de mayor a menor puntuación
        var sortedScores = data.scores.OrderByDescending(s => s.score).ToList();

        // Creamos una fila en la UI por cada puntuación guardada
        foreach (var entry in sortedScores)
        {
            GameObject newEntry = Instantiate(scoreEntryPrefab, contentParent);
            newEntry.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = entry.playerName;
            newEntry.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = entry.score.ToString();
        }

        ShowPanel(rankingsPanel);
    }

    // --- Funciones de Volumen (iguales que en GameManager) ---
    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSfxVolume(float volume)
    {
        mainMixer.SetFloat("SfxVolume", Mathf.Log10(volume) * 20);
    }
    // NUEVO: Una función pública para reproducir el sonido del clic
    public void PlayClickSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }
}