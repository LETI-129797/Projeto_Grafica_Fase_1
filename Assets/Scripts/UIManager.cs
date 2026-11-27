using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Motor L-System")]
    public LSystemEngine lSystem;

    [Header("Botões")]
    public Button restartButton;
    public Button backButton;
    public Button forwardButton;
    public Button autoPlayButton;

    [Header("Inputs")]
    public TMP_InputField radiusInput;
    public TMP_InputField intervalInput;

    [Header("Textos")]
    public TextMeshProUGUI stepText;
    public TextMeshProUGUI wordText;
    public TextMeshProUGUI endText;

    private Coroutine autoPlayCoroutine;
    private bool isAutoPlaying = false;

    void Start()
    {
        radiusInput.text = lSystem.orbitRadiusIncrement.ToString();
        intervalInput.text = "2";

        restartButton.onClick.AddListener(OnRestartClicked);
        backButton.onClick.AddListener(OnBackClicked);
        forwardButton.onClick.AddListener(OnForwardClicked);
        autoPlayButton.onClick.AddListener(OnAutoPlayClicked);

        UpdateUI();
    }

    void Update()
    {
        // Garante que a UI atualiza mesmo se avançares/recuares com as setas do teclado
        UpdateUI();
    }

    public void OnRestartClicked()
    {
        StopAutoPlay();

        if (float.TryParse(radiusInput.text, out float newRadius) && newRadius > 0)
        {
            lSystem.RestartWithRadius(newRadius);
        }
        else
        {
            lSystem.RestartWithRadius(5f);
            radiusInput.text = "5";
        }

        UpdateUI();
    }

    public void OnForwardClicked()
    {
        if (!lSystem.isEvolutionFinished)
        {
            lSystem.StepForward();
            UpdateUI();
        }
    }

    public void OnBackClicked()
    {
        StopAutoPlay();
        lSystem.StepBackward();
        UpdateUI();
    }

    public void OnAutoPlayClicked()
    {
        if (isAutoPlaying)
        {
            StopAutoPlay();
        }
        else
        {
            isAutoPlaying = true;
            autoPlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Parar Auto";
            autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
        }
    }

    private void StopAutoPlay()
    {
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
        isAutoPlaying = false;
        autoPlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Autoplay";
    }

    private IEnumerator AutoPlayRoutine()
    {
        while (!lSystem.isEvolutionFinished)
        {
            float waitTime = 2f;
            if (float.TryParse(intervalInput.text, out float parsedInterval) && parsedInterval > 0.1f)
            {
                waitTime = parsedInterval;
            }

            yield return new WaitForSeconds(waitTime);

            lSystem.StepForward();
            UpdateUI();
        }

        StopAutoPlay();
    }

    public void UpdateUI()
    {
        stepText.text = "Passo: " + lSystem.GetCurrentStep();
        wordText.text = "Palavra: " + lSystem.GetCurrentWordString();

        bool finished = lSystem.isEvolutionFinished;
        endText.gameObject.SetActive(finished);
        forwardButton.interactable = !finished;
        autoPlayButton.interactable = !finished; 
        backButton.interactable = lSystem.GetCurrentStep() > 0;
    }
}