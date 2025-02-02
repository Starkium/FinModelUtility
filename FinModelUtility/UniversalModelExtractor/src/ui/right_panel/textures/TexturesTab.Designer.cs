﻿namespace uni.ui.right_panel.textures {
  partial class TexturesTab {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Windows.Forms.SplitContainer splitContainer;
      this.textureSelectorBox_ = new uni.ui.right_panel.textures.TextureSelectorBox();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.texturePanel_ = new uni.ui.right_panel.textures.TexturePanel();
      this.textureInfoSection_ = new uni.ui.right_panel.textures.TextureInfoSection();
      splitContainer = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer
      // 
      splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      splitContainer.Location = new System.Drawing.Point(0, 0);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.textureSelectorBox_);
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.splitContainer1);
      splitContainer.Size = new System.Drawing.Size(218, 366);
      splitContainer.SplitterDistance = 203;
      splitContainer.TabIndex = 0;
      // 
      // textureSelectorBox_
      // 
      this.textureSelectorBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textureSelectorBox_.Location = new System.Drawing.Point(0, 0);
      this.textureSelectorBox_.Name = "textureSelectorBox_";
      this.textureSelectorBox_.SelectedTexture = null;
      this.textureSelectorBox_.Size = new System.Drawing.Size(218, 203);
      this.textureSelectorBox_.TabIndex = 0;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.texturePanel_);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.textureInfoSection_);
      this.splitContainer1.Size = new System.Drawing.Size(218, 159);
      this.splitContainer1.SplitterDistance = 73;
      this.splitContainer1.TabIndex = 1;
      // 
      // texturePanel_
      // 
      this.texturePanel_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.texturePanel_.Location = new System.Drawing.Point(0, 0);
      this.texturePanel_.Name = "texturePanel_";
      this.texturePanel_.Size = new System.Drawing.Size(218, 73);
      this.texturePanel_.TabIndex = 0;
      // 
      // textureInfoSection_
      // 
      this.textureInfoSection_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textureInfoSection_.Location = new System.Drawing.Point(0, 0);
      this.textureInfoSection_.Name = "textureInfoSection_";
      this.textureInfoSection_.Size = new System.Drawing.Size(218, 82);
      this.textureInfoSection_.TabIndex = 0;
      // 
      // TexturesTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(splitContainer);
      this.Name = "TexturesTab";
      this.Size = new System.Drawing.Size(218, 366);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).EndInit();
      splitContainer.ResumeLayout(false);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
    private TexturePanel texturePanel_;
    private TextureSelectorBox textureSelectorBox_;
    private SplitContainer splitContainer1;
    private TextureInfoSection textureInfoSection_;
  }
}
