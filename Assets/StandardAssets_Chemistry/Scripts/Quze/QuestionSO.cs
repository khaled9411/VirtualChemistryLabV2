using UnityEngine;

[CreateAssetMenu(fileName = "NewChemQuestion", menuName = "Chemistry Lab/MCQ Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(3, 5)]
    public string questionText;

    public string[] options = new string[4];

    [Range(0, 3)]
    public int correctOptionIndex;
}