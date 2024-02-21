sealed class Rod : Equippable
{
  public Rod()
  {
    EquipmentType = EquipmentType.Weapon;
    MagicBonus = 1;
  }

  private void OnValidate()
  {
    if (gameObject.transform.parent)
    {
      gameObject.transform.parent.GetComponent<Equipment>().Weapon = this;
    }
  }
}