using UnityEngine;

[RequireComponent(typeof(Actor), typeof(AStar))]
public class AI : MonoBehaviour {
  [SerializeField] private AStar aStar;

  public AStar AStar { get => aStar; set => aStar = value; }

  private void OnValidate() => aStar = GetComponent<AStar>();
}
