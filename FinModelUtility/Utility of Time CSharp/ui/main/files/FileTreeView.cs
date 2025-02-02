﻿using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using fin.data.fuzzy;
using fin.data.queue;

using UoT.ui.common.component;
using UoT.util;

#pragma warning disable CS8604


namespace UoT.ui.main.files {
  public interface IUiFile {
    // TODO: Make these nonnull via init setters in C#9.
    string? FileName { get; set; }
    string? BetterFileName { get; set; }
  }

  public abstract partial class FileTreeView<TFile, TFiles> : UserControl
      where TFile : notnull, IUiFile where TFiles : notnull {
    // TODO: Add tests.
    // TODO: Move the fuzzy logic to a separate reusable component.
    // TODO: Add support for different sorting systems.
    // TODO: Add support for different hierarchies.
    // TODO: Clean up the logic here.

    private readonly BetterTreeView<FileNode> betterTreeView_;

    private readonly IFuzzyFilterTree<FileNode> filterImpl_ =
        new FuzzyFilterTree<FileNode>(fileNode => {
          var keywords = new HashSet<string>();

          var file = fileNode.File;
          if (file != null) {
            var fileName = file.FileName;
            keywords.Add(fileName!);

            var betterFileName = file.BetterFileName;
            if (!string.IsNullOrEmpty(betterFileName)) {
              keywords.Add(betterFileName!);
            }
          }

          return keywords;
        });

    public delegate void FileSelectedHandler(TFile file);

    public event FileSelectedHandler FileSelected = delegate { };

    // TODO: Clean this up.
    protected class FileNode {
      private readonly BetterTreeNode<FileNode> treeNode_;
      private readonly IFuzzyNode<FileNode> filterNode_;

      public FileNode(FileTreeView<TFile, TFiles> treeView) {
        this.treeNode_ = treeView.betterTreeView_.Root;
        this.treeNode_.Data = this;

        this.filterNode_ = treeView.filterImpl_.Root.AddChild(this);
      }

      private FileNode(FileNode parent, TFile file) {
        this.File = file;

        this.treeNode_ = parent.treeNode_.Add(file.BetterFileName);
        this.treeNode_.Data = this;

        this.filterNode_ = parent.filterNode_.AddChild(this);
      }

      private FileNode(FileNode parent, string text) {
        this.treeNode_ = parent.treeNode_.Add(text);
        this.treeNode_.Data = this;

        this.filterNode_ = parent.filterNode_.AddChild(this);
      }


      public TFile? File { get; set; }

      public FileNode? Parent => this.filterNode_.Parent?.Data;
      public float MatchPercentage => this.filterNode_.MatchPercentage;
      public IReadOnlySet<string> Keywords => this.filterNode_.Keywords;

      public FileNode AddChild(TFile file) => new(this, file);
      public FileNode AddChild(string text) => new(this, text);
    }

    public FileTreeView() {
      this.InitializeComponent();

      this.filterTextBox_.TextChanged += (_, _) => this.Filter_();

      this.betterTreeView_ = new BetterTreeView<FileNode>(this.fileTreeView_);
      this.betterTreeView_.Selected += betterTreeNode => {
        var associatedData = betterTreeNode.Data;
        if (associatedData == null) {
          return;
        }

        var selectedFile = associatedData.File;
        if (selectedFile != null) {
          this.FileSelected.Invoke(selectedFile);
        }
      };
    }

    public void Populate(TFiles files) {
      this.betterTreeView_.BeginUpdate();

      this.PopulateImpl(files, new FileNode(this));
      this.InitializeAutocomplete_();

      this.betterTreeView_.EndUpdate();
    }

    protected abstract void PopulateImpl(
        TFiles files,
        FileNode root);

    private void InitializeAutocomplete_() {
      var allAutocompleteKeywords = new AutoCompleteStringCollection();

      var queue = new FinQueue<IFuzzyNode<FileNode>>(this.filterImpl_.Root);
      while (queue.TryDequeue(out var filterNode)) {
        foreach (var keyword in filterNode.Keywords) {
          allAutocompleteKeywords.Add(keyword);
        }

        queue.Enqueue(filterNode.Children);
      }

      this.filterTextBox_.AutoCompleteCustomSource = allAutocompleteKeywords;
    }

    // TODO: Debounce this method.
    private void Filter_() {
      var filterText = this.filterTextBox_.Text.ToLower();

      this.filterImpl_.Reset();

      this.betterTreeView_.BeginUpdate();

      if (string.IsNullOrEmpty(filterText)) {
        this.betterTreeView_.Root.ResetChildrenRecursively();

        this.betterTreeView_.Comparer = null;
      } else {
        const float minMatchPercentage = .2f;
        this.filterImpl_.Filter(filterText, minMatchPercentage);

        this.betterTreeView_.Root.ResetChildrenRecursively(
            betterTreeNode
                => Asserts.Assert(betterTreeNode.Data).MatchPercentage >=
                   minMatchPercentage);

        this.betterTreeView_.Comparer ??= new FuzzyTreeComparer();
      }

      this.betterTreeView_.EndUpdate();
    }

    private class FuzzyTreeComparer : IComparer<BetterTreeNode<FileNode>> {
      public int Compare(
          BetterTreeNode<FileNode> lhs,
          BetterTreeNode<FileNode> rhs)
        => -Asserts.Assert(lhs.Data)
                   .MatchPercentage.CompareTo(
                       Asserts.Assert(rhs.Data).MatchPercentage);
    }
  }
}