sealed class Sword : Equippable {
  public Sword() {
    EquipmentType = EquipmentType.Weapon;
    PowerBonus = 4;
  }

  private void OnValidate() {
    if (gameObject.transform.parent) {
      gameObject.transform.parent.GetComponent<Equipment>().Weapon = this;
    }
  }
}