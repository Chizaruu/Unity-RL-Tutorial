//Credits: Anon Coward
//Source: https://stackoverflow.com/questions/66298705/does-c-sharp-have-something-equivalent-to-pythons-random-choices
using System;
using System.Collections.Generic;

public static class RandomUtils {
  public static string Choice(this Random rnd, IEnumerable<string> choices, IEnumerable<int> weights) {
    var cumulativeWeight = new List<int>();
    int last = 0;
    foreach (var cur in weights) {
      last += cur;
      cumulativeWeight.Add(last);
    }
    int choice = rnd.Next(last);
    int i = 0;
    foreach (var cur in choices) {
      if (choice < cumulativeWeight[i]) {
        return cur;
      }
      i++;
    }
    return null;
  }

  public static List<string> Choices(this Random rnd, IEnumerable<string> choices, IEnumerable<int> weights, int maxChoices) {
    var result = new List<string>();
    for (int i = 0; i < maxChoices; i++) {
      result.Add(rnd.Choice(choices, weights));
    }
    return result;
  }
}