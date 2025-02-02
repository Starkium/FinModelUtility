﻿using fin.config;
using uni.platforms;


namespace uni.config {
  public class Config {
    public static Config Instance { get; } =
      DirectoryConstants.CONFIG_FILE.Deserialize<Config>();

    public bool ExportAllTextures { get; set; }
    public string[] ExportedFormats { get; set; } = Array.Empty<string>();
    public bool AutomaticallyPlayGameAudioForModel { get; set; }

    public bool ShowSkeleton {
      get => FinConfig.ShowSkeleton;
      set => FinConfig.ShowSkeleton = value;
    }

    public bool ShowGrid { get; set; }

    public ThirdPartyConfig ThirdParty { get; } = new();

    public bool UseMultithreadingToExtractRoms { get; set; }
    public bool VerboseConsole { get; set; }

    public void SaveSettings()
      => DirectoryConstants.CONFIG_FILE.Serialize(Config.Instance);
  }

  public class ThirdPartyConfig {
    public bool ExportBoneScaleAnimationsSeparately { get; set; }
  }
}