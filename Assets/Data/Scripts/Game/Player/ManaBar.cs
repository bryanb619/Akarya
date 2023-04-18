using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;



    public void SetMana(float mana)
    {
        _slider.value = mana;
  
    }

    internal void SetNewMax(int add)
    {
        _slider.maxValue += add; 
    }
}