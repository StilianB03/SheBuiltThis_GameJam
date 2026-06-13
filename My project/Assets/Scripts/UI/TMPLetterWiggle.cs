using UnityEngine;
using TMPro;

public class TMPLetterWiggle : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float speed = 3.0f;     // How fast the letters move
    [SerializeField] private float height = 5.0f;    // How high they bounce
    [SerializeField] private float spacing = 0.5f;   // The delay between each letter

    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // 1. Force the text to refresh its base vertex data each frame
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // 2. Loop through every character in the text
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Skip spaces or invisible characters
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // Get the vertices array for this character's mesh
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Calculate a unique sine wave offset based on time and character index
            float waveOffset = Mathf.Sin(Time.time * speed + i * spacing) * height;

            // A character is a quad (4 vertices). Apply the vertical offset to all 4 corners.
            vertices[vertexIndex + 0].y += waveOffset;
            vertices[vertexIndex + 1].y += waveOffset;
            vertices[vertexIndex + 2].y += waveOffset;
            vertices[vertexIndex + 3].y += waveOffset;
        }

        // 3. Push the modified vertex data back into the TextMeshPro geometry
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}