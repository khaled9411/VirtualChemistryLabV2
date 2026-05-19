using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuizManager : MonoBehaviour
{
    [Header("Questions Data")]
    public List<QuestionSO> questions;
    private int currentQuestionIndex = 0;
    private int score = 0;

    [Header("UI References")]
    public RectTransform quizPanel;
    public Text questionTextUI;
    public Button[] optionButtons;
    public Text[] optionTextsUI;
    public Text scoreTextUI;

    [Header("Feedback Colors")]
    public Sprite normalColor;
    public Sprite correctColor;
    public Sprite wrongColor;

    private bool isAnswering = false;

    void Start()
    {
        quizPanel.anchoredPosition = new Vector2(0, 1000);
        quizPanel.DOAnchorPosY(0, 0.6f).SetEase(Ease.OutBack).OnComplete(StartQuiz);
    }

    void StartQuiz()
    {
        score = 0;
        currentQuestionIndex = 0;
        LoadQuestion();
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            EndQuiz();
            return;
        }

        isAnswering = false;
        QuestionSO currentQ = questions[currentQuestionIndex];
        questionTextUI.text = currentQ.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionTextsUI[i].text = currentQ.options[i];
            optionButtons[i].GetComponent<Image>().sprite = normalColor;

            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
        }
    }

    public void OnOptionSelected(int selectedIndex)
    {
        if (isAnswering) return;
        isAnswering = true;

        QuestionSO currentQ = questions[currentQuestionIndex];

        if (selectedIndex == currentQ.correctOptionIndex)
        {
            score++;
            if (selectedIndex >= 0)
            {
                Image btnImage = optionButtons[selectedIndex].GetComponent<Image>();
                btnImage.sprite = correctColor;
            }
        }
        else
        {
            if (selectedIndex >= 0)
            {
                Image wrongBtnImage = optionButtons[selectedIndex].GetComponent<Image>();
                wrongBtnImage.sprite = wrongColor;

                wrongBtnImage.transform.DOShakePosition(0.4f, new Vector3(15, 0, 0), 10, 90, false, true);
            }

            Image correctBtnImage = optionButtons[currentQ.correctOptionIndex].GetComponent<Image>();
            correctBtnImage.sprite = correctColor;
        }

        StartCoroutine(NextQuestionRoutine());
    }

    IEnumerator NextQuestionRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        currentQuestionIndex++;
        LoadQuestion();
    }

    void EndQuiz()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.gameObject.SetActive(true);
            scoreTextUI.text = $"Score: {score} / {questions.Count}";
        }
    }
}