﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using fin.io;
using fin.util.asserts;
using fin.util.data;

namespace uni.util.io {
  public interface IFileHierarchy {
    IFileHierarchyDirectory Root { get; }

    void ForEach(Action<IFileHierarchyDirectory> action);
  }

  public interface IFileHierarchyInstance {
    bool Exists { get; }

    string FullName { get; }
    string Name { get; }

    string LocalPath { get; }
  }

  public interface IFileHierarchyDirectory : IFileHierarchyInstance {
    public IDirectory Impl { get; }

    public IReadOnlyList<IFileHierarchyDirectory> Subdirs { get; }
    public IReadOnlyList<IFileHierarchyFile> Files { get; }

    public bool Refresh(bool recursive = false);

    public IFileHierarchyDirectory TryToGetSubdir(string localPath);
  }

  public interface IFileHierarchyFile : IFileHierarchyInstance {
    public IFile Impl { get; }
    public string Extension { get; }
  }


  public class FileHierarchy : IFileHierarchy {
    public FileHierarchy(IDirectory directory) {
      this.Root = new FileHierarchyDirectory(directory, directory);
    }

    public IFileHierarchyDirectory Root { get; }

    private class FileHierarchyDirectory : IFileHierarchyDirectory {
      private readonly IDirectory baseDirectory_;

      private List<IFileHierarchyDirectory> subdirs_ = new();
      private List<IFileHierarchyFile> files_ = new();

      public FileHierarchyDirectory(
          IDirectory directory,
          IDirectory baseDirectory) {
        this.baseDirectory_ = baseDirectory;

        this.Impl = directory;
        this.LocalPath =
            directory.FullName.Substring(baseDirectory.FullName.Length);

        this.Subdirs =
            new ReadOnlyCollection<IFileHierarchyDirectory>(this.subdirs_);
        this.Files =
            new ReadOnlyCollection<IFileHierarchyFile>(this.files_);

        this.Refresh();
      }

      public IDirectory Impl { get; }


      public bool Exists => this.Impl.Exists;

      public string FullName => this.Impl.FullName;
      public string Name => this.Impl.Name;

      public string LocalPath { get; }


      public IReadOnlyList<IFileHierarchyDirectory> Subdirs { get; }
      public IReadOnlyList<IFileHierarchyFile> Files { get; }

      public bool Refresh(bool recursive = false) {
        var didChange = false;

        var actualSubdirs = this.Impl.GetExistingSubdirs().ToArray();
        didChange |=
            ListUtil.RemoveWhere(this.subdirs_,
                                 subdir => !actualSubdirs
                                               .Contains(subdir.Impl));
        foreach (var actualSubdir in actualSubdirs) {
          if (this.subdirs_.All(subdir => !subdir.Impl.Equals(actualSubdir))) {
            this.subdirs_.Add(
                new FileHierarchyDirectory(actualSubdir,
                                           this.baseDirectory_));
            didChange = true;
          }
        }

        var actualFiles = this.Impl.GetExistingFiles().ToList();
        didChange |=
            ListUtil.RemoveWhere(this.files_,
                                 file => !actualFiles.Contains(file.Impl));
        foreach (var actualFile in actualFiles) {
          if (this.files_.All(file => !file.Impl.Equals(actualFile))) {
            this.files_.Add(
                new FileHierarchyFile(actualFile, this.baseDirectory_));
            didChange = true;
          }
        }

        if (recursive) {
          foreach (var subdir in this.subdirs_) {
            didChange |= subdir.Refresh(true);
          }
        }

        return didChange;
      }

      public IFileHierarchyDirectory TryToGetSubdir(string relativePath)
        => this.GetSubdirImpl_(relativePath.Split('/', '\\'));

      private IFileHierarchyDirectory GetSubdirImpl_(
          IEnumerable<string> subdirs) {
        IFileHierarchyDirectory current = this;

        foreach (var subdir in subdirs) {
          if (subdir == "") {
            continue;
          }

          if (subdir == "..") {
            Asserts.Fail();
            continue;
          }

          current = current.Subdirs
                           .Single(dir => dir.Name == subdir);
        }

        return current;
      }
    }

    private class FileHierarchyFile : IFileHierarchyFile {
      public FileHierarchyFile(IFile file, IDirectory baseDirectory) {
        this.Impl = file;
        this.LocalPath =
            file.FullName.Substring(baseDirectory.FullName.Length);
      }

      public IFile Impl { get; }

      public bool Exists => this.Impl.Exists;

      public string FullName => this.Impl.FullName;
      public string Name => this.Impl.Name;
      public string Extension => this.Impl.Extension;

      public string LocalPath { get; }
    }

    public void ForEach(Action<IFileHierarchyDirectory> action) {
      var directoryQueue = new Queue<IFileHierarchyDirectory>();
      directoryQueue.Enqueue(this.Root);
      while (directoryQueue.Count > 0) {
        var directory = directoryQueue.Dequeue();

        action(directory);

        foreach (var subdir in directory.Subdirs) {
          directoryQueue.Enqueue(subdir);
        }
      }
    }
  }
}