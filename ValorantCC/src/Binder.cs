using System;
using System.Collections.Generic;
namespace ValorantCC
{
    public partial class Actionmapping
    {
        public bool alt { get; set; }
        public int bindIndex { get; set; }
        public string characterName { get; set; }
        public bool cmd { get; set; }
        public bool ctrl { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public bool shift { get; set; }
    }

    public partial class Boolsetting
    {
        public string settingEnum { get; set; }
        public bool value { get; set; }
    }

    public partial class Floatsetting
    {
        public string settingEnum { get; set; }
        public float value { get; set; }
    }

    public partial class Intsetting
    {
        public string settingEnum { get; set; }
        public int value { get; set; }
    }

    public partial class Stringsetting
    {
        public string settingEnum { get; set; }
        public string value { get; set; }
    }

    public partial class CrosshairProfile
    {
        public ProfileSettings Primary { get; set; }
        public ProfileSettings aDS { get; set; }
        public SniperSettings Sniper { get; set; }
        public bool bUsePrimaryCrosshairForADS { get; set; }
        public bool bUseCustomCrosshairOnAllPrimary { get; set; }
        public bool bUseAdvancedOptions { get; set; }

        public string ProfileName { get; set; }
    }

    public partial class ProfileSettings
    {
        public CrosshairColor Color { get; set; } = new CrosshairColor();
        public bool bHasOutline { get; set; } = false;
        public float OutlineThickness { get; set; } = 1;
        public CrosshairColor OutlineColor { get; set; } = new CrosshairColor();
        public float OutlineOpacity { get; set; } = 0;
        public float CenterDotSize { get; set; } = 1;
        public float CenterDotOpacity { get; set; } = 0;
        public bool bDisplayCenterDot { get; set; } = false;
        public bool bFixMinErrorAcrossWeapons { get; set; } = false;
        public LineSettings InnerLines { get; set; } = new LineSettings();
        public LineSettings OuterLines { get; set; } = new LineSettings();

    }

    public partial class SniperSettings
    {
        public CrosshairColor CenterDotColor { get; set; } = new CrosshairColor();
        public float CenterDotSize { get; set; } = 0;
        public float CenterDotOpacity { get; set; } = 0;
        public bool bDisplayCenterDot { get; set; } = false;
    }
    public partial class CrosshairColor
    {
        public byte R { get; set; } = 0;
        public byte G { get; set; } = 0;
        public byte B { get; set; } = 0;
        public byte A { get; set; } = 0;
    }

    public partial class LineSettings
    {
        public float LineThickness { get; set; } = 2;
        public float LineLength { get; set; } = 4;
        public float LineOffset { get; set; } = 2;
        public bool bShowMovementError { get; set; } = false;
        public bool bShowShootingError { get; set; } = false;
        public bool bShowMinError { get; set; } = false;
        public float Opacity { get; set; } = 1;
        public bool bShowLines { get; set; } = true;
        public float firingErrorScale { get; set; }
        public float movementErrorScale { get; set; }
    }

    public partial class ProfileList
    {
        public int CurrentProfile { get; set; }
        public List<CrosshairProfile> Profiles { get; set; }
    }

    public partial class Data
    {
        public List<Actionmapping> actionMappings { get; set; }
        public List<object> axisMappings { get; set; }
        public List<Boolsetting> boolSettings { get; set; }
        public List<Floatsetting> floatSettings { get; set; }
        public List<Intsetting> intSettings { get; set; }
        public int roamingSetttingsVersion { get; set; }
        public List<Stringsetting> stringSettings { get; set; }
        public List<string> settingsProfiles { get; set; }
    }

}
