sealed class ChainMail : Equippable {
  public ChainMail() {
    EquipmentType = EquipmentType.Armor;
    DefenseBonus = 3;
  }

  private void OnValidate() {
    if (gameObject.transform.parent) {
      gameObject.transform.parent.GetComponent<Equipment>().Armor = this;
    }
  }
}