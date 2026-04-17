using UnityEngine;
using UnityEngine.UI;
using Game.Game.Nation;

public class CityAttackPanel : MonoBehaviour
{
    public static CityAttackPanel Instance;

    public Button btnAttack;
    public NationCityManager playerNation;

    private CityData _currentTargetCity;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        btnAttack.gameObject.SetActive(false);
        btnAttack.onClick.AddListener(OnAttackClick);

        // 自动找玩家
        if (playerNation == null)
            playerNation = FindObjectOfType<NationCityManager>();
    }

    // 外部调用：显示/隐藏按钮
    public void TryShowAttack(CityData targetCity)
    {
        if (playerNation == null || targetCity == null)
        {
            btnAttack.gameObject.SetActive(false);
            return;
        }

        NationCityManager owner = NationWarManager.Instance.GetNationByCity(targetCity);

        // 敌方城市 → 显示
        if (owner != null && owner != playerNation)
        {
            _currentTargetCity = targetCity;
            btnAttack.gameObject.SetActive(true);
        }
        else
        {
            _currentTargetCity = null;
            btnAttack.gameObject.SetActive(false);
        }
    }

    // 攻击按钮点击
    private void OnAttackClick()
    {
        if (_currentTargetCity == null)
        {
            Debug.Log("未选中敌方城市");
            return;
        }

        if (playerNation.cityList.Count == 0)
        {
            Debug.Log("玩家无城市");
            return;
        }

        CityData fromCity = playerNation.cityList[0];
        NationCityManager targetNation = NationWarManager.Instance.GetNationByCity(_currentTargetCity);

        if (targetNation == null) return;

        // 宣战
        if (playerNation.diplomacy == null)
            playerNation.diplomacy = new NationDiplomacy(playerNation);

        playerNation.diplomacy.DeclareWar(targetNation);

        // 发送军队
        int sendCount = fromCity.armyOut / 2;
        NationWarManager.Instance.SendArmy(
            playerNation,
            fromCity,
            _currentTargetCity,
            sendCount
        );

        Debug.Log("✅ 进攻开始！");

        // 关闭面板
        gameObject.SetActive(false);
    }
}