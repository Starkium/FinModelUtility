﻿using System.Collections.Generic;

using CommandLine;

namespace j3d.cli {
  public abstract class BBasicOptions {
    [Option("verbose",
            HelpText = "Whether to print verbose log output.",
            Required = false)]
    public bool Verbose { get; set; }

    [Option("framerate",
            HelpText = "The frame rate of the animations. If not provided, assumed to be 30fps.",
            Required = false)]
    public float? FrameRate { get; set; }
  }

  public abstract class BConversionOptions : BBasicOptions {
    [Option("out",
            HelpText =
                "Path to an output directory that the model(s) will be generated within.",
            Required = true)]
    public string OutputPath { get; set; }
  }

  [Verb("automatic",
        HelpText =
            "Convert GCN model with automatically-determined input files.")]
  public class AutomaticOptions : BConversionOptions {}

  [Verb("manual",
        HelpText = "Convert GCN model with manually-specified input files.")]
  public class ManualOptions : BConversionOptions {
    [Option("bmd",
            HelpText = "Path(s) to input .bmd models.",
            Required = true,
            Min = 1)]
    public IEnumerable<string> BmdPaths { get; set; }

    [Option("bcx", HelpText = "Path(s) to input .bca/.bck animations.")]
    public IEnumerable<string> BcxPaths { get; set; }

    [Option("bti", HelpText = "Path(s) to input .bti textures.")]
    public IEnumerable<string> BtiPaths { get; set; }
  }

  [Verb("debug",
        HelpText =
            "Convert GCN model with hardcoded input files. Not meant for general use.")]
  public class DebugOptions : BBasicOptions {}
}