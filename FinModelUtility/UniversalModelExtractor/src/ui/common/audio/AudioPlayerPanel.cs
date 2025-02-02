﻿using fin.audio;


namespace uni.ui.common.audio {
  public partial class AudioPlayerPanel : UserControl, IAudioPlayerPanel {
    public AudioPlayerPanel() {
      this.InitializeComponent();

      this.impl_.OnChange += audioFileBundle => {
        this.Invoke(() => {
          if (audioFileBundle != null) {
            this.groupBox_.Text = audioFileBundle.DisplayFullName;
          } else {
            this.groupBox_.Text = "(Select audio)";
          }
        });

        this.OnChange(audioFileBundle);
      };
    }

    public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
      get => this.impl_.AudioFileBundles;
      set => this.impl_.AudioFileBundles = value;
    }

    public event Action<IAudioFileBundle?> OnChange = delegate { };
  }
}