using Godot;

namespace GoBox;

public static class Util {
    public static string FormatStopwatch(double duration) {
        if (double.IsNaN(duration))
            return "NaN";
        if (duration < 0)
            return "-" + FormatStopwatch(-duration);
        if (double.IsInfinity(duration))
            return "∞";

        var hh = (int)(duration / 3600);
        duration -= hh * 3600;
        var mm = (int)(duration / 60);
        duration -= mm * 60;
        var ss = (int)duration;
        duration -= ss;
        var uu = (int)(duration * 1000);

        return hh > 0
            ? $"{hh}h{mm:00}′{ss:00}″{uu:000}"
            : $"{mm:00}′{ss:00}″{uu:000}";
    }
}