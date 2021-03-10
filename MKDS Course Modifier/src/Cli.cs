﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using mkds.exporter;

using MKDS_Course_Modifier.GCN;

public class Cli {
  public static void Main(string[] args) {
    string outputPath = "";
    string bmdPath = "";
    IList<string> bcxPaths = new List<string>();
    IList<string> btiPaths = new List<string>();

    if (args.Length >= 2) {
      outputPath = args[0];
      bmdPath = args[1];
      bcxPaths = args.Skip(2).ToList();
    } else {
      GetForEnemy("Queen",
                  out outputPath,
                  out bmdPath,
                  out bcxPaths,
                  out btiPaths);
      //GetForPikmin(out outputPath, out bmdPath);
      //GetForTesting(out outputPath, out bmdPath, out bcxPaths);
    }

    var bmd = new BMD(File.ReadAllBytes(bmdPath));
    var pathsAndBcxs = bcxPaths
                       .Select(bcxPath => {
                         var extension =
                             new FileInfo(bcxPath).Extension.ToLower();
                         IBcx bcx = extension switch {
                             ".bca" => new BCA(File.ReadAllBytes(bcxPath)),
                             ".bck" => new BCK(File.ReadAllBytes(bcxPath)),
                             _      => throw new NotSupportedException(),
                         };
                         return (bcxPath, bcx);
                       })
                       .ToList();
    var pathsAndBtis =
        btiPaths.Select(btiPath => (btiPath,
                                    new BTI(File.ReadAllBytes(btiPath))))
                .ToList();

    GltfExporter.Export(outputPath, bmd, pathsAndBcxs, pathsAndBtis);
  }

  private static void GetForTesting(
      out string outputPath,
      out string bmdPath,
      out IList<string> bcxPaths) {
    outputPath =
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/out.glb";
    bmdPath =
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/enemy.bmd";
    bcxPaths = new[] {
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/attack0.bca",
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/attack1.bca",
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/attack4.bca",
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/flick.bca",
        "R:/Documents/CSharpWorkspace/Pikmin2Utility/cli/out/move.bca"
    };
  }

  private static void GetForEnemy(
      string name,
      out string outputPath,
      out string bmdPath,
      out IList<string> bcxPaths,
      out IList<string> btiPaths) {
    var basePath = "R:/Documents/CSharpWorkspace/Pikmin2Utility/";
    outputPath = $"{basePath}cli/out/{name}/{name}.glb";

    var enemyBasePath = $"{basePath}cli/roms/pkmn2.gcm_dir/enemy/data/{name}/";
    bmdPath = $"{enemyBasePath}model.szs 0.rarc_dir/model/enemy.bmd";

    var bcxFiles =
        new DirectoryInfo($"{enemyBasePath}anim.szs 0.rarc_dir/anim/");
    bcxPaths = bcxFiles.GetFiles().Select(file => file.FullName).ToList();

    btiPaths = new DirectoryInfo(enemyBasePath)
               .GetFiles()
               .SkipWhile(file => file.Extension != ".bti")
               .Select(file => file.FullName)
               .ToList();
  }

  private static void GetForPikmin(out string outputPath,
                                   out string bmdPath) {
    var basePath = "R:/Documents/CSharpWorkspace/Pikmin2Utility/";
    outputPath = $"{basePath}cli/out/Pikmin/Red/red.glb";

    var pikminBasePath = $"{basePath}cli/roms/pkmn2.gcm_dir/user/Kando/piki/pikis.szs 0.rarc_dir/designer/piki_model/";
    bmdPath = $"{pikminBasePath}piki_p2_red.bmd";

    /*var bcxFiles =
        new DirectoryInfo($"{enemyBasePath}anim.szs 0.rarc_dir/anim/");
    bcxPaths = bcxFiles.GetFiles().Select(file => file.FullName).ToList();

    btiPaths = new DirectoryInfo(enemyBasePath)
               .GetFiles()
               .SkipWhile(file => file.Extension != ".bti")
               .Select(file => file.FullName)
               .ToList();*/
  }
}