﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.cli;
using fin.exporter.assimp;
using fin.exporter.gltf;
using fin.log;
using fin.util.asserts;

using Microsoft.Extensions.Logging;

using mkds.exporter;

using MKDS_Course_Modifier.GCN;

namespace mkds.cli {
  public class Cli {
    public static int Main(string[] args) {
      Args.PopulateFromArgs(args);

      var logger = Logging.Create<Cli>();
      using var _ = logger.BeginScope("Entry");
      logger.LogInformation(string.Join(" ", args));

      using var _2 = logger.BeginScope("Main");
      logger.LogInformation("Attempting to parse:");
      logger.LogInformation("- model: " + Args.BmdPath);
      logger.LogInformation("- " +
                            Args.BcxPaths.Count +
                            " animations:\n" +
                            string.Join('\n', Args.BcxPaths));
      logger.LogInformation("- " +
                            Args.BtiPaths.Count +
                            " textures:\n" +
                            string.Join('\n', Args.BtiPaths));

      var bmdExists = File.Exists(Args.BmdPath);
      if (!bmdExists) {
        throw new ArgumentException("Model does not exist: " + Args.BmdPath);
      }

      var nonexistentBcxes =
          Args.BcxPaths.Where(bcxPath => !File.Exists(bcxPath));
      var bcxesExist = !nonexistentBcxes.Any();
      if (!bcxesExist) {
        throw new ArgumentException("Some bcxes don't exist: " +
                                    string.Join(' ', nonexistentBcxes));
      }

      var nonexistentBtis =
          Args.BtiPaths.Where(btiPath => !File.Exists(btiPath));
      var btisExist = !nonexistentBtis.Any();
      if (!btisExist) {
        throw new ArgumentException("Some btis don't exist: " +
                                    string.Join(' ', nonexistentBtis));
      }

      BMD bmd;
      try {
        bmd = new BMD(File.ReadAllBytes(Args.BmdPath));
      } catch {
        logger.LogError("Failed to load BMD!");
        throw;
      }

      List<(string, IBcx)> pathsAndBcxs;
      try {
        pathsAndBcxs = Args.BcxPaths
                           .Select(bcxPath => {
                             var extension =
                                 new FileInfo(bcxPath).Extension.ToLower();
                             IBcx bcx = extension switch {
                                 ".bca" =>
                                     new BCA(File.ReadAllBytes(bcxPath)),
                                 ".bck" =>
                                     new BCK(File.ReadAllBytes(bcxPath)),
                                 _ => throw new NotSupportedException(),
                             };
                             return (bcxPath, bcx);
                           })
                           .ToList();
      } catch {
        logger.LogError("Failed to load BCX!");
        throw;
      }

      List<(string, BTI)> pathsAndBtis;
      try {
        pathsAndBtis =
            Args.BtiPaths.Select(btiPath => (btiPath,
                                             new BTI(
                                                 File.ReadAllBytes(btiPath))))
                .ToList();
      } catch {
        logger.LogError("Failed to load BTI!");
        throw;
      }

      BmdDebugHelper.ExportFilesInBmd(bmd, pathsAndBtis);

      var outputFile = Args.OutputFile;
      if (Args.Static) {
        logger.LogInformation("Converting to a static mesh first.");

        outputFile.GetParent().Create();

        var model =
            new ModelConverter().Convert(bmd, pathsAndBcxs, pathsAndBtis);

        //new GltfExporter().Export(outputFile, model);
        new AssimpExporter().Export(outputFile.CloneWithExtension(".fbx"),
                                    model);
      } else {
        logger.LogInformation("Exporting directly.");
        new GltfExporterOld().Export(Args.OutputPath,
                                     bmd,
                                     pathsAndBcxs,
                                     pathsAndBtis);
      }

      return 0;
    }
  }
}