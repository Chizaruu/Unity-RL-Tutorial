using System.Collections.Generic;
using UnityEngine;
abstract class Visibility {
  /// <param name="origin">The location of the monster whose field of view will be calculated.</param>
  /// <param name="rangeLimit">The maximum distance from the origin that tiles will be lit.
  /// If equal to -1, no limit will be applied.
  /// </param>
  /// <param name="visibleTiles">The list of tiles that are visible to the monster.</param>
  public abstract void Compute(Vector3Int origin, int rangeLimit, List<Vector3Int> visibleTiles);
}