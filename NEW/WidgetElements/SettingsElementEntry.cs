//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsElementEntry : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public string cachedDescription;
    public Sprite cachedReferenceImage;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_Text text;
    public Button descriptionButton;
    public Slider slider;
    public Toggle checkbox;
    public TMP_Dropdown dropdown;
    public Button_Selector selector;
    



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetInfo(string _name, string _cachedDescription, Sprite _cachedReferenceImage, bool _hidden, float _sliderValue, float _sliderMinValue, float _sliderMaxValue)
    {
        text.text = _name;
        cachedDescription = _cachedDescription;
        cachedReferenceImage = _cachedReferenceImage;
        gameObject.SetActive(!_hidden);
        slider.value = _sliderValue;
        slider.minValue = _sliderMinValue;
        slider.maxValue = _sliderMaxValue;
    }
    
    public void SetInfo(string _name, string _cachedDescription, Sprite _cachedReferenceImage, bool _hidden, bool _checkboxValue)
    {
        text.text = _name;
        cachedDescription = _cachedDescription;
        cachedReferenceImage = _cachedReferenceImage;
        gameObject.SetActive(!_hidden);
        checkbox.isOn = _checkboxValue;
    }
    
    public void SetInfo(string _name, string _cachedDescription, Sprite _cachedReferenceImage, bool _hidden, int _dropdownValue, List<TMP_Dropdown.OptionData> _dropdownOptions)
    {
        text.text = _name;
        cachedDescription = _cachedDescription;
        cachedReferenceImage = _cachedReferenceImage;
        gameObject.SetActive(!_hidden);
        dropdown.value = _dropdownValue;
        dropdown.options = _dropdownOptions;
    }
    
    public void SetInfo(string _name, string _cachedDescription, Sprite _cachedReferenceImage, bool _hidden, int _selectorCurrentIndex, List<string> _selectorOptions)
    {
        text.text = _name;
        cachedDescription = _cachedDescription;
        cachedReferenceImage = _cachedReferenceImage;
        gameObject.SetActive(!_hidden);
        selector.currentIndex = _selectorCurrentIndex;
        selector.selectorOptions = _selectorOptions;
    }


    #endregion
}
