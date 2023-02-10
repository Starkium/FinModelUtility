﻿using System.Collections.Generic;
using System.IO;

namespace fin.io.bundles {
  public interface IFileBundle : IUiFile {
    IFileHierarchyFile? MainFile { get; }
    IFileHierarchyDirectory Directory => MainFile.Parent!;
    string IUiFile.FileName => this.MainFile?.NameWithoutExtension ?? "(n/a)";

    string FullPath {
      get {
        var mainFile = this.MainFile;
        if (mainFile != null) {
          return Path.Join(mainFile.Root.Name,
                           mainFile.Parent!.LocalPath,
                           mainFile.NameWithoutExtension);
        }

        return FileName;
      }
    }
  }

  public interface IFileBundleGatherer {
    IEnumerable<IFileBundle> GatherFileBundles(bool assert);
  }

  public interface IFileBundleGatherer<TFileBundle> : IFileBundleGatherer
      where TFileBundle : IFileBundle {
    new IEnumerable<TFileBundle> GatherFileBundles(bool assert);

    IEnumerable<IFileBundle> IFileBundleGatherer.
        GatherFileBundles(bool assert) {
      foreach (var fileBundle in this.GatherFileBundles(assert)) {
        yield return fileBundle;
      }
    }
  }

  public interface IFileBundleGathererAccumulator : IFileBundleGatherer {
    IFileBundleGathererAccumulator Add(IFileBundleGatherer gatherer);
  }
}