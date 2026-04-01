using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UIMainMenu mainMenu;
    private MapDrawUINavigator mapNav;
    private Button btn;

    void Awake()
    {
        // 自动找当前场景的动画管理者
        mainMenu = FindObjectOfType<UIMainMenu>();
        mapNav = FindObjectOfType<MapDrawUINavigator>();
        btn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 空判断保护，不报错！
        if (btn == null) return;

        if (mainMenu != null)
            mainMenu.OnPointerEnter(btn);
        
        if (mapNav != null)
            mapNav.OnPointerEnter(btn);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 空判断保护，不报错！
        if (btn == null) return;

        if (mainMenu != null)
            mainMenu.OnPointerExit(btn);
        
        if (mapNav != null)
            mapNav.OnPointerExit(btn);
    }
}