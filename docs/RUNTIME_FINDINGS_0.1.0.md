# Runtime findings — PrayerClarity 0.1.0

User test on Graveyard Keeper 1.407 established:

- the test harness can synthesize and switch prayer families/qualities in the live pulpit UI without granting the prayer to the save;
- the visible `Church quality / Sermon requires / Success chance` block remains vanilla UI;
- PrayerClarity 0.1.0 did not render its forecast because `R.BalanceData` selected an open generic `GameBalance.GetData*` overload and reflection invocation failed with `ContainsGenericParameters=true`;
- the plugin failed safe: vanilla pulpit UI remained usable and sermon mechanics were not changed;
- this is an implementation bug in the reflection resolver, not evidence against the chosen pulpit UI seam or forecast model.

The next candidate must select only a closed, one-string-parameter `GameBalance.GetData` / `GetDataOrNull` method (or otherwise close the verified generic overload explicitly) and retain fail-safe behavior.
