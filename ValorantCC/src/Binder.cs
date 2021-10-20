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
        public Primary Primary { get; set; }
        public string ProfileName { get; set; }
    }

    public partial class Primary
    {
        public CrosshairColor Color { get; set; }
    }
    public partial class CrosshairColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
    }

    public partial class ProfileList
    {
        public int CurrentProfile { get; set; }
        public List<CrosshairProfile> Profiles { get; set; }
    }

    public partial class Data
    {
        public Actionmapping[] actionMappings { get; set; }
        public object[] axisMappings { get; set; }
        public Boolsetting[] boolSettings { get; set; }
        public Floatsetting[] floatSettings { get; set; }
        public Intsetting[] intSettings { get; set; }
        public int roamingSetttingsVersion { get; set; }
        public Stringsetting[] stringSettings { get; set; }
    }

}
