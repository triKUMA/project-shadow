using Godot;
using System;
using System.Collections.Generic;
using static TMConstants;

public partial class TimeManager : Node3D {
  [Export] public double dayLength = 30d;

  public static TimeManager Instance { get; private set; }

  public event Action<ulong> OnSecondUpdated;
  public event Action<ulong> OnMinuteUpdated;
  public event Action<ulong> OnHourUpdated;
  public event Action<ulong> OnDayUpdated;
  public event Action<ulong> OnMonthUpdated;
  public event Action<ulong> OnYearUpdated;

  private ulong year = 1ul;
  private ulong month = 1ul;
  private ulong day = 1ul;
  private ulong hour = 0ul;
  private ulong minute = 0ul;
  private ulong second = 0ul;
  private double milliseconds = 0d;

  private double timeSpeed => (24 * 60) / dayLength;

  private Hemisphere hemisphere = Hemisphere.South;

  public override void _Ready() {
    if (Instance == null) {
      Instance = this;
    } else {
      QueueFree();
    }
  }

  public override void _Process(double delta) {
    milliseconds += delta * 1000d * timeSpeed;
    if (milliseconds < 1000) return;

    second += (ulong)(milliseconds / 1000);
    milliseconds %= 1000;
    OnSecondUpdated?.Invoke(second);

    CascadeTime(ref second, ref minute, 60ul, OnMinuteUpdated);
    CascadeTime(ref minute, ref hour, 60ul, OnHourUpdated);
    CascadeTime(ref hour, ref day, 24ul, OnDayUpdated);
    CascadeDay(ref day, ref month, OnMonthUpdated);
    CascadeTime(ref month, ref year, 12ul, OnYearUpdated, true);
  }

  public TMDateTime GetDateTime() {
    return new TMDateTime {
      year = year,
      month = month,
      day = day,
      hour = hour,
      minute = minute,
      second = second,
    };
  }

  private void CascadeTime(ref ulong startVal, ref ulong overflowVal, ulong capacity, Action<ulong> overflowCallback, bool startsAtOne = false) {
    var adjStartVal = startsAtOne ? startVal - 1ul : startVal;

    if (adjStartVal < capacity) return;

    overflowVal += adjStartVal / capacity;
    startVal = adjStartVal % capacity + (startsAtOne ? 1ul : 0ul);

    overflowCallback?.Invoke(overflowVal);
  }

  private void CascadeDay(ref ulong day, ref ulong month, Action<ulong> overflowCallback) {
    bool overflow, monthUpdated = false;
    do {
      overflow = false;
      ulong monthLength = month == 2ul ? (IsLeapyear(year) ? 29ul : 28ul) : MonthLengths[(Month)((month - 1) % 12 + 1)];
      if (day > monthLength) {
        overflow = true;
        day -= monthLength;
        month++;
        monthUpdated = true;
      }
    } while (overflow);

    if (monthUpdated) overflowCallback?.Invoke(month);
  }

  private bool IsLeapyear(ulong year) {
    return (
      year % 4ul == 0ul && (
      year % 100ul != 0ul ||
      year % 400ul == 0ul
      )
    );
  }
}
