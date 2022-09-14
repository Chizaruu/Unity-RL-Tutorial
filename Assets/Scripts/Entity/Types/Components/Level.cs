using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Level : MonoBehaviour {
  [SerializeField] private int currentLevel = 1, currentXp, xpToNextLevel, levelUpBase = 200, levelUpFactor = 150, xpGiven;

  public int CurrentLevel { get => currentLevel; }
  public int CurrentXp { get => currentXp; }
  public int XpToNextLevel { get => xpToNextLevel; }
  public int XPGiven { get => xpGiven; set => xpGiven = value; }

  private void OnValidate() => xpToNextLevel = ExperienceToNextLevel();
  private int ExperienceToNextLevel() => levelUpBase + currentLevel * levelUpFactor;
  private bool RequiresLevelUp() => currentXp >= xpToNextLevel;

  public void AddExperience(int xp) {
    if (xp == 0 || levelUpBase == 0) return;

    currentXp += xp;

    UIManager.instance.AddMessage($"You gain {xp} experience points.", "#FFFFFF");

    if (RequiresLevelUp()) {
      UIManager.instance.ToggleLevelUpMenu(GetComponent<Actor>());
      UIManager.instance.AddMessage($"You advance to level {currentLevel + 1}!", "#00FF00"); //Green
    }
  }

  private void IncreaseLevel() {
    currentXp -= xpToNextLevel;
    currentLevel++;
    xpToNextLevel = ExperienceToNextLevel();
  }

  public void IncreaseMaxHp(int amount = 20) {
    GetComponent<Fighter>().MaxHp += amount;
    GetComponent<Fighter>().Hp += amount;

    UIManager.instance.AddMessage($"Your health improves!", "#00FF00"); //Green
    IncreaseLevel();
  }

  public void IncreasePower(int amount = 1) {
    GetComponent<Fighter>().BasePower += amount;

    UIManager.instance.AddMessage($"You feel stronger!", "#00FF00"); //Green
    IncreaseLevel();
  }

  public void IncreaseDefense(int amount = 1) {
    GetComponent<Fighter>().BaseDefense += amount;

    UIManager.instance.AddMessage($"Your movements are getting swifter!", "#00FF00"); //Green
    IncreaseLevel();
  }

  public LevelState SaveState() => new LevelState(
    currentLevel: currentLevel,
    currentXp: currentXp,
    xpToNextLevel: xpToNextLevel
  );

  public void LoadState(LevelState state) {
    currentLevel = state.CurrentLevel;
    currentXp = state.CurrentXp;
    xpToNextLevel = state.XpToNextLevel;
  }
}

public class LevelState {
  [SerializeField] private int currentLevel = 1, currentXp, xpToNextLevel;

  public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
  public int CurrentXp { get => currentXp; set => currentXp = value; }
  public int XpToNextLevel { get => xpToNextLevel; set => xpToNextLevel = value; }

  public LevelState(int currentLevel, int currentXp, int xpToNextLevel) {
    this.currentLevel = currentLevel;
    this.currentXp = currentXp;
    this.xpToNextLevel = xpToNextLevel;
  }
}
