using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateTextfieldFromValue : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_TextfieldToUpdate;

    public void SetValue(float value)
    {
        m_TextfieldToUpdate.text = ((int)value).ToString();
    }
}
