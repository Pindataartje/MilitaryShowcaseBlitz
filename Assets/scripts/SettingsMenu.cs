using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMP_Text resolutionText;
    [SerializeField] Button higherResButton;
    [SerializeField] Button lowerResButton;

    Resolution[] resolutions;
    int currentResIndex;

    void Start()
    {
        resolutions = new Resolution[]
        {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 2560, height = 1440 },
            new Resolution { width = 3840, height = 2160 }
        };

        Resolution current = Screen.currentResolution;
        currentResIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResIndex = i;
                break;
            }
        }

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        higherResButton.onClick.AddListener(HigherResolution);
        lowerResButton.onClick.AddListener(LowerResolution);
        UpdateResolutionText();
    }

    void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    void HigherResolution()
    {
        if (currentResIndex < resolutions.Length - 1)
        {
            currentResIndex++;
            ApplyResolution();
        }
    }

    void LowerResolution()
    {
        if (currentResIndex > 0)
        {
            currentResIndex--;
            ApplyResolution();
        }
    }

    void ApplyResolution()
    {
        Resolution res = resolutions[currentResIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        UpdateResolutionText();
    }

    void UpdateResolutionText()
    {
        Resolution res = resolutions[currentResIndex];
        resolutionText.text = res.width + " x " + res.height;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
