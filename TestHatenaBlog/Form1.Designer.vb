<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.ListViewPhotos = New System.Windows.Forms.ListView()
        Me.ListViewArticles = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TextBoxContent = New System.Windows.Forms.TextBox()
        Me.PictureBoxPhoto = New System.Windows.Forms.PictureBox()
        Me.ButtonDelete = New System.Windows.Forms.Button()
        Me.ButtonPost = New System.Windows.Forms.Button()
        Me.LabelArticleEntry = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ButtonNew = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPageArticle = New System.Windows.Forms.TabPage()
        Me.LabelSyntax = New System.Windows.Forms.Label()
        Me.CheckedListBoxCategories = New System.Windows.Forms.CheckedListBox()
        Me.CheckBoxDraft = New System.Windows.Forms.CheckBox()
        Me.TextBoxTitle = New System.Windows.Forms.TextBox()
        Me.TabPagePhoto = New System.Windows.Forms.TabPage()
        Me.LabelPhotoEntry = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.StatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ButtonOAuth = New System.Windows.Forms.Button()
        Me.LabelUrlName = New System.Windows.Forms.Label()
        Me.LabelDisplayName = New System.Windows.Forms.Label()
        Me.PictureBoxProfile = New System.Windows.Forms.PictureBox()
        CType(Me.PictureBoxPhoto, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabControl1.SuspendLayout()
        Me.TabPageArticle.SuspendLayout()
        Me.TabPagePhoto.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBoxProfile, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ListViewPhotos
        '
        Me.ListViewPhotos.AllowDrop = True
        Me.ListViewPhotos.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListViewPhotos.HideSelection = False
        Me.ListViewPhotos.Location = New System.Drawing.Point(2, 3)
        Me.ListViewPhotos.MultiSelect = False
        Me.ListViewPhotos.Name = "ListViewPhotos"
        Me.ListViewPhotos.Size = New System.Drawing.Size(195, 432)
        Me.ListViewPhotos.TabIndex = 0
        Me.ListViewPhotos.UseCompatibleStateImageBehavior = False
        '
        'ListViewArticles
        '
        Me.ListViewArticles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.ListViewArticles.Dock = System.Windows.Forms.DockStyle.Top
        Me.ListViewArticles.FullRowSelect = True
        Me.ListViewArticles.GridLines = True
        Me.ListViewArticles.HideSelection = False
        Me.ListViewArticles.Location = New System.Drawing.Point(0, 0)
        Me.ListViewArticles.MultiSelect = False
        Me.ListViewArticles.Name = "ListViewArticles"
        Me.ListViewArticles.Size = New System.Drawing.Size(588, 191)
        Me.ListViewArticles.TabIndex = 1
        Me.ListViewArticles.UseCompatibleStateImageBehavior = False
        Me.ListViewArticles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Title"
        Me.ColumnHeader1.Width = 100
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Entry"
        Me.ColumnHeader2.Width = 100
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Summery"
        Me.ColumnHeader3.Width = 300
        '
        'TextBoxContent
        '
        Me.TextBoxContent.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxContent.Location = New System.Drawing.Point(3, 65)
        Me.TextBoxContent.Multiline = True
        Me.TextBoxContent.Name = "TextBoxContent"
        Me.TextBoxContent.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBoxContent.Size = New System.Drawing.Size(575, 184)
        Me.TextBoxContent.TabIndex = 2
        '
        'PictureBoxPhoto
        '
        Me.PictureBoxPhoto.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PictureBoxPhoto.Location = New System.Drawing.Point(3, 36)
        Me.PictureBoxPhoto.Name = "PictureBoxPhoto"
        Me.PictureBoxPhoto.Size = New System.Drawing.Size(586, 213)
        Me.PictureBoxPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBoxPhoto.TabIndex = 3
        Me.PictureBoxPhoto.TabStop = False
        '
        'ButtonDelete
        '
        Me.ButtonDelete.Enabled = False
        Me.ButtonDelete.Location = New System.Drawing.Point(387, 6)
        Me.ButtonDelete.Name = "ButtonDelete"
        Me.ButtonDelete.Size = New System.Drawing.Size(50, 28)
        Me.ButtonDelete.TabIndex = 7
        Me.ButtonDelete.Text = "削除"
        Me.ButtonDelete.UseVisualStyleBackColor = True
        '
        'ButtonPost
        '
        Me.ButtonPost.Enabled = False
        Me.ButtonPost.Location = New System.Drawing.Point(259, 6)
        Me.ButtonPost.Name = "ButtonPost"
        Me.ButtonPost.Size = New System.Drawing.Size(50, 28)
        Me.ButtonPost.TabIndex = 6
        Me.ButtonPost.Text = "投稿"
        Me.ButtonPost.UseVisualStyleBackColor = True
        '
        'LabelArticleEntry
        '
        Me.LabelArticleEntry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelArticleEntry.Location = New System.Drawing.Point(127, 8)
        Me.LabelArticleEntry.Name = "LabelArticleEntry"
        Me.LabelArticleEntry.Size = New System.Drawing.Size(126, 24)
        Me.LabelArticleEntry.TabIndex = 5
        Me.LabelArticleEntry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(64, 14)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(57, 12)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "ENTRY ID"
        '
        'ButtonNew
        '
        Me.ButtonNew.Location = New System.Drawing.Point(8, 6)
        Me.ButtonNew.Name = "ButtonNew"
        Me.ButtonNew.Size = New System.Drawing.Size(50, 28)
        Me.ButtonNew.TabIndex = 3
        Me.ButtonNew.Text = "新規"
        Me.ButtonNew.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPageArticle)
        Me.TabControl1.Controls.Add(Me.TabPagePhoto)
        Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControl1.Location = New System.Drawing.Point(0, 191)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(588, 278)
        Me.TabControl1.TabIndex = 5
        '
        'TabPageArticle
        '
        Me.TabPageArticle.Controls.Add(Me.LabelSyntax)
        Me.TabPageArticle.Controls.Add(Me.CheckedListBoxCategories)
        Me.TabPageArticle.Controls.Add(Me.CheckBoxDraft)
        Me.TabPageArticle.Controls.Add(Me.TextBoxTitle)
        Me.TabPageArticle.Controls.Add(Me.TextBoxContent)
        Me.TabPageArticle.Controls.Add(Me.ButtonDelete)
        Me.TabPageArticle.Controls.Add(Me.ButtonPost)
        Me.TabPageArticle.Controls.Add(Me.ButtonNew)
        Me.TabPageArticle.Controls.Add(Me.LabelArticleEntry)
        Me.TabPageArticle.Controls.Add(Me.Label1)
        Me.TabPageArticle.Location = New System.Drawing.Point(4, 22)
        Me.TabPageArticle.Name = "TabPageArticle"
        Me.TabPageArticle.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageArticle.Size = New System.Drawing.Size(580, 252)
        Me.TabPageArticle.TabIndex = 0
        Me.TabPageArticle.Text = "記事"
        Me.TabPageArticle.UseVisualStyleBackColor = True
        '
        'LabelSyntax
        '
        Me.LabelSyntax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelSyntax.Location = New System.Drawing.Point(315, 38)
        Me.LabelSyntax.Name = "LabelSyntax"
        Me.LabelSyntax.Size = New System.Drawing.Size(126, 24)
        Me.LabelSyntax.TabIndex = 11
        Me.LabelSyntax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'CheckedListBoxCategories
        '
        Me.CheckedListBoxCategories.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CheckedListBoxCategories.Enabled = False
        Me.CheckedListBoxCategories.FormattingEnabled = True
        Me.CheckedListBoxCategories.Location = New System.Drawing.Point(443, 3)
        Me.CheckedListBoxCategories.MaximumSize = New System.Drawing.Size(200, 200)
        Me.CheckedListBoxCategories.Name = "CheckedListBoxCategories"
        Me.CheckedListBoxCategories.Size = New System.Drawing.Size(131, 60)
        Me.CheckedListBoxCategories.TabIndex = 10
        '
        'CheckBoxDraft
        '
        Me.CheckBoxDraft.AutoSize = True
        Me.CheckBoxDraft.Location = New System.Drawing.Point(315, 18)
        Me.CheckBoxDraft.Name = "CheckBoxDraft"
        Me.CheckBoxDraft.Size = New System.Drawing.Size(57, 16)
        Me.CheckBoxDraft.TabIndex = 9
        Me.CheckBoxDraft.Text = "下書き"
        Me.CheckBoxDraft.UseVisualStyleBackColor = True
        '
        'TextBoxTitle
        '
        Me.TextBoxTitle.Location = New System.Drawing.Point(3, 40)
        Me.TextBoxTitle.Name = "TextBoxTitle"
        Me.TextBoxTitle.Size = New System.Drawing.Size(306, 19)
        Me.TextBoxTitle.TabIndex = 8
        '
        'TabPagePhoto
        '
        Me.TabPagePhoto.Controls.Add(Me.LabelPhotoEntry)
        Me.TabPagePhoto.Controls.Add(Me.Label4)
        Me.TabPagePhoto.Controls.Add(Me.PictureBoxPhoto)
        Me.TabPagePhoto.Location = New System.Drawing.Point(4, 22)
        Me.TabPagePhoto.Name = "TabPagePhoto"
        Me.TabPagePhoto.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPagePhoto.Size = New System.Drawing.Size(580, 252)
        Me.TabPagePhoto.TabIndex = 1
        Me.TabPagePhoto.Text = "フォト"
        Me.TabPagePhoto.UseVisualStyleBackColor = True
        '
        'LabelPhotoEntry
        '
        Me.LabelPhotoEntry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelPhotoEntry.Location = New System.Drawing.Point(70, 9)
        Me.LabelPhotoEntry.Name = "LabelPhotoEntry"
        Me.LabelPhotoEntry.Size = New System.Drawing.Size(95, 24)
        Me.LabelPhotoEntry.TabIndex = 7
        Me.LabelPhotoEntry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(7, 15)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(57, 12)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "ENTRY ID"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 469)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(588, 22)
        Me.StatusStrip1.TabIndex = 6
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'StatusLabel1
        '
        Me.StatusLabel1.Name = "StatusLabel1"
        Me.StatusLabel1.Size = New System.Drawing.Size(119, 17)
        Me.StatusLabel1.Text = "ToolStripStatusLabel1"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.ButtonOAuth)
        Me.Panel1.Controls.Add(Me.LabelUrlName)
        Me.Panel1.Controls.Add(Me.LabelDisplayName)
        Me.Panel1.Controls.Add(Me.PictureBoxProfile)
        Me.Panel1.Controls.Add(Me.ListViewPhotos)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Right
        Me.Panel1.Location = New System.Drawing.Point(588, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(200, 491)
        Me.Panel1.TabIndex = 7
        '
        'ButtonOAuth
        '
        Me.ButtonOAuth.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOAuth.Location = New System.Drawing.Point(3, 441)
        Me.ButtonOAuth.Name = "ButtonOAuth"
        Me.ButtonOAuth.Size = New System.Drawing.Size(194, 47)
        Me.ButtonOAuth.TabIndex = 4
        Me.ButtonOAuth.Text = "リトライOAuth"
        Me.ButtonOAuth.UseVisualStyleBackColor = True
        Me.ButtonOAuth.Visible = False
        '
        'LabelUrlName
        '
        Me.LabelUrlName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LabelUrlName.Location = New System.Drawing.Point(3, 460)
        Me.LabelUrlName.Name = "LabelUrlName"
        Me.LabelUrlName.Size = New System.Drawing.Size(138, 22)
        Me.LabelUrlName.TabIndex = 3
        Me.LabelUrlName.Text = "UrlName"
        Me.LabelUrlName.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LabelDisplayName
        '
        Me.LabelDisplayName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LabelDisplayName.Location = New System.Drawing.Point(3, 438)
        Me.LabelDisplayName.Name = "LabelDisplayName"
        Me.LabelDisplayName.Size = New System.Drawing.Size(138, 22)
        Me.LabelDisplayName.TabIndex = 2
        Me.LabelDisplayName.Text = "DisplayName"
        Me.LabelDisplayName.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'PictureBoxProfile
        '
        Me.PictureBoxProfile.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PictureBoxProfile.Location = New System.Drawing.Point(147, 438)
        Me.PictureBoxProfile.Name = "PictureBoxProfile"
        Me.PictureBoxProfile.Size = New System.Drawing.Size(50, 50)
        Me.PictureBoxProfile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBoxProfile.TabIndex = 1
        Me.PictureBoxProfile.TabStop = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(788, 491)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ListViewArticles)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        CType(Me.PictureBoxPhoto, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPageArticle.ResumeLayout(False)
        Me.TabPageArticle.PerformLayout()
        Me.TabPagePhoto.ResumeLayout(False)
        Me.TabPagePhoto.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        CType(Me.PictureBoxProfile, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListViewPhotos As ListView
    Friend WithEvents ListViewArticles As ListView
    Friend WithEvents TextBoxContent As TextBox
    Friend WithEvents PictureBoxPhoto As PictureBox
    Friend WithEvents ButtonDelete As Button
    Friend WithEvents ButtonPost As Button
    Friend WithEvents LabelArticleEntry As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents ButtonNew As Button
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPageArticle As TabPage
    Friend WithEvents TabPagePhoto As TabPage
    Friend WithEvents LabelPhotoEntry As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents StatusLabel1 As ToolStripStatusLabel
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents TextBoxTitle As TextBox
    Friend WithEvents CheckBoxDraft As CheckBox
    Friend WithEvents CheckedListBoxCategories As CheckedListBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBoxProfile As PictureBox
    Friend WithEvents LabelUrlName As Label
    Friend WithEvents LabelDisplayName As Label
    Friend WithEvents ButtonOAuth As Button
    Friend WithEvents LabelSyntax As Label
End Class
