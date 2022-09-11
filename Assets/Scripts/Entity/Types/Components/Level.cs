using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Level : MonoBehaviour {
  [SerializeField] private int currentLevel = 1, currentXp, levelUpBase = 200, levelUpFactor = 150, xpGiven;

  public int XPGiven { get => xpGiven; set => xpGiven = value; }

  public int ExperienceToNextLevel() => levelUpBase + currentLevel * levelUpFactor;

  public bool RequiresLevelUp() => currentXp >= ExperienceToNextLevel();

  public void AddExperience(int xp) {
    if (xp == 0 || levelUpBase == 0) return;

    currentXp += xp;

    UIManager.instance.AddMessage($"You gain {xp} experience points.", "#FFFFFF");

    if (RequiresLevelUp()) UIManager.instance.AddMessage($"You advance to level {++currentLevel}!", "#00FF00"); //Green
  }

  public void IncreaseLevel() {
    currentXp -= ExperienceToNextLevel();
    currentLevel++;
  }

  public void IncreaseMaxHp(int amount = 20) {
    GetComponent<Actor>().Fighter.MaxHp += amount;
    GetComponent<Actor>().Fighter.Hp += amount;

    UIManager.instance.AddMessage($"Your health improves!", "#00FF00"); //Green

    IncreaseLevel();
  }

  public void IncreasePower(int amount = 1) {
    GetComponent<Actor>().Fighter.Power += amount;

    UIManager.instance.AddMessage($"You feel stronger!", "#00FF00"); //Green

    IncreaseLevel();
  }

  public void IncreaseDefense(int amount = 1) {
    GetComponent<Actor>().Fighter.Defense += amount;

    UIManager.instance.AddMessage($"Your movements are getting swifter!", "#00FF00"); //Green

    IncreaseLevel();
  }

  public LevelState SaveState() => new LevelState(
    currentLevel: currentLevel,
    currentXp: currentXp,
    levelUpBase: levelUpBase
  );

  public void LoadState(LevelState state) {
    currentLevel = state.CurrentLevel;
    currentXp = state.CurrentXp;
    levelUpBase = state.LevelUpBase;
  }
}

public class LevelState {
  [SerializeField] private int currentLevel = 1, currentXp, levelUpBase = 200;

  public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
  public int CurrentXp { get => currentXp; set => currentXp = value; }
  public int LevelUpBase { get => levelUpBase; set => levelUpBase = value; }

  public LevelState(int currentLevel, int currentXp, int levelUpBase) {
    this.currentLevel = currentLevel;
    this.currentXp = currentXp;
    this.levelUpBase = levelUpBase;
  }
}
