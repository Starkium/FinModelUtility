﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.util.asserts;
using fin.util.json;

using schema.binary;


namespace fin.io {
  public interface IGenericFile {
    string DisplayPath { get; }

    FileSystemStream OpenRead();
    FileSystemStream OpenWrite();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    StreamReader OpenReadAsText() => new(this.OpenRead());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    StreamWriter OpenWriteAsText() => new(this.OpenWrite());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T ReadNew<T>() where T : IBinaryDeserializable, new() {
      using var er = new EndianBinaryReader(this.OpenRead());
      return er.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T ReadNew<T>(Endianness endianness)
        where T : IBinaryDeserializable, new() {
      using var er = new EndianBinaryReader(this.OpenRead(), endianness);
      return er.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    byte[] ReadAllBytes() {
      using var s = this.OpenRead();
      using var ms = new MemoryStream();
      s.CopyTo(ms);
      return ms.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    string ReadAllText() {
      using var sr = this.OpenReadAsText();
      return sr.ReadToEnd();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteAllBytes(byte[] bytes) {
      using var s = this.OpenWrite();
      using var ms = new MemoryStream(bytes);
      ms.CopyTo(s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteAllText(string text) {
      using var sw = this.OpenWriteAsText();
      sw.Write(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T Deserialize<T>() => JsonUtil.Deserialize<T>(this.ReadAllText());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Serialize<T>(T instance) where T : notnull
      => this.WriteAllText(JsonUtil.Serialize(instance));
  }

  public interface IIoObject<TSelf, TFile, TDirectory> : IEquatable<TSelf>
      where TSelf : IIoObject<TSelf, TFile, TDirectory>
      where TFile : IFile<TFile, TDirectory>
      where TDirectory : IDirectory<TFile, TDirectory> {
    static abstract TSelf FromFullName(string fullName);

    string Name => FinIoStatic.GetName(this.FullName);
    string FullName { get; }

    bool Exists { get; }

    string? GetParentFullName() => FinIoStatic.GetParentFullName(this.FullName);

    TDirectory GetParent() {
      if (this.TryGetParent(out var parent)) {
        return parent;
      }

      throw new Exception("Expected parent directory to exist!");
    }

    bool TryGetParent(out TDirectory parent) {
      var parentName = this.GetParentFullName();
      if (parentName != null) {
        parent = TDirectory.FromFullName(parentName);
        return true;
      }

      parent = default;
      return false;
    }

    TDirectory[] GetAncestry() {
      if (!this.TryGetParent(out var firstParent)) {
        return Array.Empty<TDirectory>();
      }

      var parents = new LinkedList<TDirectory>();
      var current = firstParent;
      while (current.TryGetParent(out var parent)) {
        parents.AddLast(parent);
        current = parent;
      }

      return parents.ToArray();
    }

    string ToString() => this.FullName;

    bool Equals(object? other) {
      if (object.ReferenceEquals(this, other)) {
        return true;
      }

      if (other is not TSelf otherSelf) {
        return false;
      }

      return this.Equals(otherSelf);
    }

    bool IEquatable<TSelf>.Equals(TSelf? other)
      => this.FullName == other?.FullName;
  }


  // Directory

  public interface IDirectory : IDirectory<FinFile, FinDirectory> {
    static FinDirectory
        IIoObject<FinDirectory, FinFile, FinDirectory>.
        FromFullName(string fullName) => new(fullName);
  }

  public interface IDirectory<TFile, TDirectory>
      : IIoObject<TDirectory, TFile, TDirectory>
      where TFile : IFile<TFile, TDirectory>
      where TDirectory : IDirectory<TFile, TDirectory> {
    bool IIoObject<TDirectory, TFile, TDirectory>.Exists
      => FinDirectoryStatic.Exists(this.FullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Create() => FinDirectoryStatic.Create(this.FullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Delete(bool recursive = false)
      => FinDirectoryStatic.Delete(this.FullName, recursive);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void MoveTo(string path) => FinDirectoryStatic.MoveTo(this.FullName, path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<TDirectory> GetExistingSubdirs()
      => FinDirectoryStatic.GetExistingSubdirs(this.FullName)
                           .Select(TDirectory.FromFullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TDirectory GetSubdir(string relativePath, bool create = false)
      => TDirectory.FromFullName(
          FinDirectoryStatic.GetSubdir(this.FullName, relativePath, create));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<TFile> GetExistingFiles()
      => FinDirectoryStatic.GetExistingFiles(this.FullName)
                           .Select(TFile.FromFullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<TFile> SearchForFiles(
        string searchPattern,
        bool includeSubdirs = false)
      => FinDirectoryStatic
         .SearchForFiles(this.FullName, searchPattern, includeSubdirs)
         .Select(TFile.FromFullName);

    bool TryToGetExistingFile(string path, out TFile outFile) {
      if (FinDirectoryStatic.TryToGetExistingFile(
              this.FullName,
              path,
              out var fileFullName)) {
        outFile = TFile.FromFullName(fileFullName);
        return true;
      }

      outFile = default;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TFile GetExistingFile(string path)
      => TFile.FromFullName(
          FinDirectoryStatic.GetExistingFile(this.FullName, path));

    bool PossiblyAssertExistingFile(string relativePath,
                                    bool assert,
                                    out TFile outFile) {
      var fileFullName =
          FinDirectoryStatic.PossiblyAssertExistingFile(
              this.FullName,
              relativePath,
              assert);
      if (fileFullName != null) {
        outFile = TFile.FromFullName(fileFullName);
        return true;
      }

      outFile = default;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<TFile> GetFilesWithExtension(
        string extension,
        bool includeSubdirs = false)
      => FinDirectoryStatic
         .GetFilesWithExtension(this.FullName, extension, includeSubdirs)
         .Select(TFile.FromFullName);
  }


  // File 
  public interface IFile : IFile<FinFile, FinDirectory> {
    static FinFile IIoObject<FinFile, FinFile, FinDirectory>.FromFullName(
        string fullName) => new(fullName);
  }

  public interface IFile<TFile, TDirectory>
      : IIoObject<TFile, TFile, TDirectory>, IGenericFile
      where TFile : IFile<TFile, TDirectory>
      where TDirectory : IDirectory<TFile, TDirectory> {
    bool IIoObject<TFile, TFile, TDirectory>.Exists
      => FinFileStatic.Exists(this.FullName);

    string IGenericFile.DisplayPath => this.FullName;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Delete() => FinFileStatic.Delete(FullName);

    string Extension => FinFileStatic.GetExtension(FullName);

    string FullNameWithoutExtension
      => FinFileStatic.GetNameWithoutExtension(this.FullName);

    string NameWithoutExtension
      => FinFileStatic.GetNameWithoutExtension(this.Name);

    TFile CloneWithExtension(string newExtension) {
      Asserts.True(newExtension.StartsWith("."),
                   $"'{newExtension}' is not a valid extension!");
      return TFile.FromFullName(this.FullNameWithoutExtension + newExtension);
    }
  }
}