using Godot;
using System;
using System.Collections.Generic;

public struct TMDateTime {
  public ulong year;
  public ulong month;
  public ulong day;
  public ulong hour;
  public ulong minute;
  public ulong second;
}

public partial class TimeManager : Node3D {
  [Export] public double dayLength = 30d;

  public static TimeManager Instance { get; private set; }

  public event Action<ulong> OnSecondUpdated;
  public event Action<ulong> OnMinuteUpdated;
  public event Action<ulong> OnHourUpdated;
  public event Action<ulong> OnDayUpdated;
  public event Action<ulong> OnMonthUpdated;
  public event Action<ulong> OnYearUpdated;

  public enum Month {
    January = 1,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
  }

  public enum Season {
    Summer,
    Autumn,
    Winter,
    Spring
  }

  public enum Hemisphere {
    North,
    South
  }

  public readonly Dictionary<Month, string> monthNames = new() {
    {Month.January, "January"},
    {Month.February, "February"},
    {Month.March, "March"},
    {Month.April, "April"},
    {Month.March, "May"},
    {Month.June, "June"},
    {Month.July, "July"},
    {Month.August, "August"},
    {Month.September, "September"},
    {Month.October, "October"},
    {Month.November, "November"},
    {Month.December, "December"},
  };

  public readonly Dictionary<Season, string> seasonNames = new() {
    {Season.Summer, "Summer"},
    {Season.Autumn, "Autumn"},
    {Season.Winter, "Winter"},
    {Season.Spring, "Spring"},
  };

  private ulong year = 1ul;
  private ulong month = 1ul;
  private ulong day = 1ul;
  private ulong hour = 0ul;
  private ulong minute = 0ul;
  private ulong second = 0ul;
  private double milliseconds = 0d;

  private double timeSpeed => (24 * 60) / dayLength;

  // 0u = northern hemisphere, 1u = southern hemisphere
  private Hemisphere hemisphere = Hemisphere.South;

  private readonly Dictionary<Month, ulong> monthLengths = new(){
    {Month.January, 31ul},
    {Month.February, 28ul},
    {Month.March, 31ul},
    {Month.April, 30ul},
    {Month.May, 31ul},
    {Month.June, 30ul},
    {Month.July, 31ul},
    {Month.August, 31ul},
    {Month.September, 30ul},
    {Month.October, 31ul},
    {Month.November, 30ul},
    {Month.December, 31ul},
  };

  private readonly Dictionary<Month, (Season, Season)> monthToSeason = new() {
    {Month.January, (Season.Winter, Season.Summer)},
    {Month.February, (Season.Winter, Season.Summer)},
    {Month.March, (Season.Spring, Season.Autumn)},
    {Month.April, (Season.Spring, Season.Autumn)},
    {Month.March, (Season.Spring, Season.Autumn)},
    {Month.June, (Season.Summer, Season.Winter)},
    {Month.July, (Season.Summer, Season.Winter)},
    {Month.August, (Season.Summer, Season.Winter)},
    {Month.September, (Season.Autumn, Season.Spring)},
    {Month.October, (Season.Autumn, Season.Spring)},
    {Month.November, (Season.Autumn, Season.Spring)},
    {Month.December, (Season.Winter, Season.Summer)},
  };

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
      ulong monthLength = month == 2ul ? (IsLeapyear(year) ? 29ul : 28ul) : monthLengths[(Month)((month - 1) % 12 + 1)];
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
