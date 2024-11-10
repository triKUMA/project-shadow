using Godot;
using System;
using System.Collections.Generic;

public partial class TimeManager : Node3D {
  private ulong year, month, day = 1ul;
  private ulong hour, minute, second = 0ul;
  private double milliseconds = 0d;

  private readonly Dictionary<ulong, ulong> monthLengths = new(){
    {1ul, 31ul},
    {2ul, 28ul},
    {3ul, 31ul},
    {4ul, 30ul},
    {5ul, 31ul},
    {6ul, 30ul},
    {7ul, 31ul},
    {8ul, 31ul},
    {9ul, 30ul},
    {10ul, 31ul},
    {11ul, 30ul},
    {12ul, 31ul},
  };

  private readonly Dictionary<ulong, string> monthNames = new() {
    {1ul, "January"},
    {2ul, "February"},
    {3ul, "March"},
    {4ul, "April"},
    {5ul, "May"},
    {6ul, "June"},
    {7ul, "July"},
    {8ul, "August"},
    {9ul, "September"},
    {10ul, "October"},
    {11ul, "November"},
    {12ul, "December"},
  };

  public override void _Ready() {
  }

  public override void _Process(double delta) {
    milliseconds += delta * 1000d;
    if (milliseconds < 1000) return;

    second += (ulong)(milliseconds / 1000);
    milliseconds %= 1000;

    cascadeTime(ref second, ref minute, 60ul);
    cascadeTime(ref minute, ref hour, 60ul);
    cascadeTime(ref hour, ref day, 24ul);
    cascadeDay(ref day, ref month);
    cascadeTime(ref month, ref year, 12ul, true);
  }

  private void cascadeTime(ref ulong startVal, ref ulong overflowVal, ulong capacity, bool startsAtOne = false) {
    var adjStartVal = startsAtOne ? startVal - 1ul : startVal;

    if (adjStartVal < capacity) return;

    overflowVal += adjStartVal / overflowVal;
    startVal = adjStartVal % capacity + (startsAtOne ? 1ul : 0ul);
  }

  private void cascadeDay(ref ulong day, ref ulong month) {
    bool overflow;
    do {
      overflow = false;
      ulong monthLength = month == 2ul ? (isLeapyear(year) ? 29ul : 28ul) : monthLengths[month];
      if (day > monthLength) {
        overflow = true;
        day -= monthLength;
        month++;
      }
    } while (overflow);
  }

  private bool isLeapyear(ulong year) {
    return (
      year % 4ul == 0ul && (
        year % 100ul != 0ul ||
        year % 400ul == 0ul
      )
    );
  }
}
