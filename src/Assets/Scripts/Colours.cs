using UnityEngine;
using System.Collections.Generic;

public static class Colours {
    // The custom colour scheme I chose to fit the visual aesthetic
    private static Dictionary<string, Color> COLOUR_MAP = new Dictionary<string, Color> {
        {"White", Color.white },
        {"Red", new Color(0.9764706f,0.254902f,0.2666667f) },
        {"Blue", new Color(0.1529412f,0.4901961f,0.6313726f) },
        {"Green", new Color(0.5647059f,0.7450981f,0.427451f) },
        {"Cyan", new Color(0.5294118f,0.9647059f,1f) },
        {"Orange", new Color(0.972549f,0.5882353f,0.1176471f) },
        {"Pink", new Color(1f,0.759434f,0.9223981f) },
        {"Purple", new Color(0.6666667f,0.2431373f,0.5960785f) },
        {"Yellow", new Color(1, 0.9215686f, 0.2078431f) }
    };

    public static Color getColour(string colourName) {
        return COLOUR_MAP[colourName];
    }
}
