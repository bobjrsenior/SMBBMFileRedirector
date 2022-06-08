﻿using System.Collections.Generic;

namespace SMBBMFileRedirector
{
    internal class ReplacementDef
    {
        public string name;
        public string description;
        public string author;
        public int file_format_version;

        public Dictionary<string, string> asset_bundles;
        public Dictionary<string, CueSheetDef> cue_sheets;
        public Dictionary<string, string> usm_audio_files;
        public Dictionary<string, string> cue_to_cue_sheet;
    }
}