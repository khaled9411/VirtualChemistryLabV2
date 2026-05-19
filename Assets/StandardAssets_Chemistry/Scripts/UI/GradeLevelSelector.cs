using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public string sceneName;
}

[System.Serializable]
public class GradeData
{
    public string gradeName;
    public List<LevelData> levels;
}

public class GradeLevelSelector : MonoBehaviour
{
    [Header("Dat")]
    public List<GradeData> grades;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Grade Buttons")]
    public List<Button> gradeButtons;

    [Header("Level Buttons")]
    public List<Button> levelButtons;

    [Header("Continue Button")]
    public Button continueButton;
    public SceneLoader sceneLoader;

    private int selectedGradeIndex = -1;
    private string selectedSceneName = "";

    void Start()
    {
        for (int i = 0; i < gradeButtons.Count; i++)
        {
            int idx = i;
            gradeButtons[i].onClick.AddListener(() => OnGradeSelected(idx));
        }

        for (int i = 0; i < levelButtons.Count; i++)
        {
            int idx = i;
            levelButtons[i].onClick.AddListener(() => OnLevelSelected(idx));
        }

        continueButton.onClick.AddListener(OnContinue);

        SetAllLevelButtonsActive(false);
        continueButton.interactable = false;

        if (grades.Count > 0)
            OnGradeSelected(0);
    }

    void OnGradeSelected(int index)
    {
        if (index >= grades.Count) return;

        selectedGradeIndex = index;
        selectedSceneName = "";
        continueButton.interactable = false;

        for (int i = 0; i < gradeButtons.Count; i++)
        {
            SetButtonSprite(gradeButtons[i], i == index);
        }

        GradeData grade = grades[index];
        for (int i = 0; i < levelButtons.Count; i++)
        {
            bool hasLevel = i < grade.levels.Count;
            levelButtons[i].gameObject.SetActive(hasLevel);

            if (hasLevel)
            {
                Text txt = levelButtons[i].GetComponentInChildren<Text>();
                if (txt != null) txt.text = grade.levels[i].levelName;

                SetButtonSprite(levelButtons[i], false);
            }
        }
    }

    void OnLevelSelected(int index)
    {
        if (selectedGradeIndex < 0) return;
        GradeData grade = grades[selectedGradeIndex];
        if (index >= grade.levels.Count) return;

        selectedSceneName = grade.levels[index].sceneName;
        continueButton.interactable = true;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (levelButtons[i].gameObject.activeSelf)
                SetButtonSprite(levelButtons[i], i == index);
        }
    }

    void OnContinue()
    {
        if (string.IsNullOrEmpty(selectedSceneName)) return;
        sceneLoader.LoadScene(selectedSceneName);
    }

    void SetButtonSprite(Button btn, bool isSelected)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = isSelected ? selectedSprite : normalSprite;
            img.GetComponentInChildren<Text>().color = isSelected ? Color.white : Color.black;
        }
    }

    void SetAllLevelButtonsActive(bool active)
    {
        foreach (var btn in levelButtons)
            btn.gameObject.SetActive(active);
    }
}