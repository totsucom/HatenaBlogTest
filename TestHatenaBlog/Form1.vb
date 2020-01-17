Public Class Form1
    Const HATENA_ID = "XXXX" 'はてなユーザー名。メアドではなく、はてなにログインしたときに上部に表示されている短い名前。はてなID。
    Const HATENA_PASSWORD = "XXXXX" 'はてなログインパスワード
    Const HATENA_BLOG_ID = "mae8bit-twe.hateblo.jp" 'ブログのID。ドメインも選択できるため、hateblo.jpとは限りません
    Const CONSUMER_KEY = "xxxxxxxxxxxx" 'このページの、http://developer.hatena.ne.jp/ アプリケーション登録で得られるコンシューマーキー
    Const CONSUMER_SECRET = "xxxxxxxxxxxxx" '同上。コンシューマーシークレット
    Const OAUTH_SCOPE = "read_public,write_public,read_private,write_private" '全ての操作を許可するならこれでいい
    Const DEFAULT_SYNTAX As Hatena.SyntaxMode = Hatena.SyntaxMode.HATENA 'はてなブログで設定している入力書式設定
    Const FOTOLIFE_DIR = "Hatena Blog" 'フォト一覧に表示するフォトライフのディレクトリ

    Const IMAGE_SIZE = 96 'フォト一覧のサムネイルの大きさ

    'この中のHttpClientを使いまわす
    'HttpClientは再利用すべきだが、Async関数にはByrefでdlを渡せない。MSDNでは静的変数で定義するようなことを書いているみたい
    '（受け売り）なので、このような方法にした。
    Public Shared dl As HttpDownloader

    Private dir As Hatena.PhotoLifeDirectory

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'はてなブログへのログイン結果を返す
        AddHandler Hatena.LoginResult, AddressOf hatena_LoginResult

        'はてなフォトライフのサムネイル画像一覧のダウンロード結果を返す
        AddHandler Hatena.PhotoLifeDirectory.ThumbnailsDownloaded, AddressOf photolife_ThumbnailsDownloaded

        AddHandler Hatena.OAuthResult, AddressOf hatena_OAuthResult
        AddHandler Hatena.MyJsonResult, AddressOf hatena_MyJsonResult
        AddHandler Hatena.ArticlesResult, AddressOf hatena_ArticlesResult
        AddHandler Hatena.CategoriesResult, AddressOf hatena_CategoriesResult

        'ダウンローダーを初期化。Cookieを使用する
        dl = New HttpDownloader(True)
        dl.Client.Timeout = New TimeSpan(0, 1, 0) '1分(デバッグするので)

        'ダウンローダーをつかってはてなブログにログインする
        Hatena.Login(HATENA_ID, HATENA_PASSWORD)
        StatusLabel1.Text = "はてなにログインしています...。"

        setSyntaxMode(DEFAULT_SYNTAX)
    End Sub

    'はてなブログのログイン結果
    Private Sub hatena_LoginResult(result As Boolean, msg As String)
        StatusLabel1.Text = msg

        If result Then
            'ログインに成功したのでフォトライフのサムネイルをダウンロードする
            ListViewPhotos.ListViewItemSorter = New ListViewIndexComparer 'LargeIconでは自動でインデックス順に表示してくれないのでソーターを設定
            ListViewPhotos.LargeImageList = New ImageList
            ListViewPhotos.LargeImageList.ImageSize = New Size(IMAGE_SIZE, IMAGE_SIZE)
            ListViewPhotos.Items.Clear()
            dir = New Hatena.PhotoLifeDirectory(HATENA_ID, FOTOLIFE_DIR)

            'OAuth認証を行う
            Hatena.OAuth(CONSUMER_KEY, CONSUMER_SECRET, OAUTH_SCOPE)
            StatusLabel1.Text = "OAuth認証を行っています...。"
        End If
    End Sub

    'OAuth認証の結果
    Private Sub hatena_OAuthResult(result As Boolean, msg As String)
        StatusLabel1.Text = msg

        If result Then

            'プロファイル情報を取得する
            Hatena.GetMyJson()

            'はてなブログの記事一覧を取得
            ListViewArticles.Items.Clear()
            Hatena.GetArticles(HATENA_BLOG_ID)

            'はてなブログのカテゴリー一覧を取得
            Hatena.GetCategories(HATENA_BLOG_ID)

            '投稿ボタンを有効にする
            ButtonPost.Enabled = True
        Else
            'リトライ用ボタンを表示
            ButtonOAuth.Visible = True
        End If
    End Sub

    'OAuthリトライボタンがクリックされた
    Private Sub ButtonOAuth_Click(sender As Object, e As EventArgs) Handles ButtonOAuth.Click
        'OAuth認証を行う
        Hatena.OAuth(CONSUMER_KEY, CONSUMER_SECRET, OAUTH_SCOPE)
        StatusLabel1.Text = "OAuth認証を行っています...。"

        '結果が出るまでボタンは非表示
        ButtonOAuth.Visible = False
    End Sub

    'my.jsonを取得した
    Private Sub hatena_MyJsonResult(result As Boolean, my As Hatena.MyJson)
        If result Then
            PictureBoxProfile.Image = my.profile_image
            LabelDisplayName.Text = my.display_name
            LabelUrlName.Text = "(id:" & my.url_name & ")"
        Else
            PictureBoxProfile.Image = Nothing
            LabelDisplayName.Text = ""
            LabelUrlName.Text = ""
        End If
    End Sub

    'はてなブログの記事一覧を取得した
    Private Sub hatena_ArticlesResult(result As Boolean, articles As Hatena.Articles)
        If Not result Then Exit Sub
        ListViewArticles.BeginUpdate()
        For Each a In articles.articles
            ListViewArticles.Items.Add(createListViewItem(a))
        Next
        ListViewArticles.EndUpdate()

        '全ての記事を取得するには次のコードを有効にする
        'If articles.nextUrl <> "" Then
        '    '次の記事一覧を取得
        '    Hatena.GetNextArticles(articles.nextUrl)
        'End If
    End Sub

    Private Function createListViewItem(a As Hatena.Article) As ListViewItem
        Dim lvi As New ListViewItem(a.title)
        lvi.SubItems.Add(a.entryId)
        lvi.SubItems.Add(a.summary)
        lvi.Tag = a.entryId
        Return lvi
    End Function

    Private Sub updateListViewItem(lvi As ListViewItem, a As Hatena.Article)
        lvi.Text = a.title
        lvi.SubItems(1).Text = a.entryId
        lvi.SubItems(2).Text = a.summary
        lvi.Tag = a.entryId
    End Sub

    'はてなブログのカテゴリー一覧を取得した
    Private Sub hatena_CategoriesResult(result As Boolean, categories As String())
        With CheckedListBoxCategories
            .Items.Clear()
            If result Then
                For Each c In categories
                    .Items.Add(c)
                Next
                .Enabled = True
            Else
                .Enabled = False
            End If
        End With
    End Sub

    'はてなフォトライフのサムネイル一覧を取得した
    Private Sub photolife_ThumbnailsDownloaded(dir As Hatena.PhotoLifeDirectory, errorMessage As String)
        If errorMessage.Length > 0 Then Debug.Print(errorMessage)
        If dir.Photos IsNot Nothing AndAlso dir.Photos.Count > 0 Then

            'リストビューに表示
            With ListViewPhotos
                .BeginUpdate()
                For Each p In dir.Photos
                    If p.thumbnail IsNot Nothing Then
                        .LargeImageList.Images.Add(p.entry, ResizeImage(p.thumbnail, New Drawing.Size(IMAGE_SIZE, IMAGE_SIZE)))
                    End If
                    .Items.Add(createListViewItem(p))
                Next
                .EndUpdate()
            End With
        End If

        '全ての画像を読み込む場合は、下記をコメントアウトする
        'If dir.NextPageUrl <> "" Then
        '    '次ページを取得する
        '    dir.DownloadNextPage()
        'End If
    End Sub

    Private Function createListViewItem(photo As Hatena.PhotoLifeDirectory.Photo) As ListViewItem
        Dim lvi As New ListViewItem
        lvi.ImageKey = photo.entry 'エントリ番号をイメージキーとする
        lvi.Text = photo.title
        lvi.Tag = photo
        Return lvi
    End Function

    'イメージのアスペクト比を維持しながら縮小する。余白はWhite
    Private Function ResizeImage(img As Image, size As Size) As Bitmap
        Dim bmp As New Bitmap(size.Width, size.Height)
        Dim g As Graphics = Graphics.FromImage(bmp)
        g.FillRectangle(Brushes.White, 0, 0, size.Width, size.Height)

        Dim sx As Single = size.Width / img.Width
        Dim sy As Single = size.Height / img.Height
        If sx < sy Then
            '横幅に合わせる
            g.DrawImage(img, 0, (size.Height - img.Height * sx) / 2, size.Width, img.Height * sx)
        Else
            '高さに合わせる
            g.DrawImage(img, (size.Width - img.Width * sy) / 2, 0, img.Width * sy, size.Height)
        End If
        Return bmp
    End Function

    'フォトが選択された
    Private Async Sub ListViewPhotos_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListViewPhotos.SelectedIndexChanged
        PictureBoxPhoto.Image = Nothing
        LabelPhotoEntry.Text = ""
        StatusLabel1.Text = ""

        If ListViewPhotos.SelectedItems.Count <> 1 Then Exit Sub

        Dim p As Hatena.PhotoLifeDirectory.Photo = ListViewPhotos.SelectedItems(0).Tag

        Dim img = Await p.GetImage()
        If img IsNot Nothing Then
            LabelPhotoEntry.Text = p.entry & p.TypeChar
            PictureBoxPhoto.Image = img
            StatusLabel1.Text = p.ImageUrl
            TabControl1.SelectedTab = TabPagePhoto 'タブを選択
        Else
            LabelPhotoEntry.Text = p.entry
            PictureBoxPhoto.Image = Nothing
            StatusLabel1.Text = "ダウンロードできませんでした"
        End If
    End Sub

    'フォトがダブルクリックされた
    Private Async Sub ListViewPhotos_DoubleClick(sender As Object, e As EventArgs) Handles ListViewPhotos.DoubleClick
        If ListViewPhotos.SelectedItems.Count <> 1 Then Exit Sub

        TabControl1.SelectedTab = TabPageArticle '記事タブを選択

        Dim p As Hatena.PhotoLifeDirectory.Photo = ListViewPhotos.SelectedItems(0).Tag
        Dim mode As Hatena.SyntaxMode = DirectCast(LabelSyntax.Tag, Hatena.SyntaxMode)

        Dim img As Image = Await p.GetImage()
        If img Is Nothing Then
            MsgBox("元画像を取得できていません")
            Exit Sub
        End If

        Dim caption As String = InputBox("キャプション")
        Dim insText As String
        If caption = "" Then
            'キャプションなし。書式モードによって挿入内容が変わる
            If mode = Hatena.SyntaxMode.HATENA OrElse mode = Hatena.SyntaxMode.MARKDOWN Then
                insText = "[f:id:" & HATENA_ID & ":" & p.entry & p.TypeChar & ":plain]" & vbNewLine
            Else
                Dim s = "f:id:" & HATENA_ID & ":" & p.entry & p.TypeChar & ":plain"
                insText = "<p><img class=""hatena-fotolife"" title=""" & s & """ src=""" & p.ImageUrl & """ alt=""" & s & """ /></p>"
            End If
        Else
            'キャプションあり。書式モードによって挿入内容が変わる
            If mode = Hatena.SyntaxMode.HATENA OrElse mode = Hatena.SyntaxMode.MARKDOWN Then
                insText = "<figure class=""figure-image figure-image-fotolife"" title=""" &
                    caption & """>[f:id:" & HATENA_ID & ":" & p.entry &
                     p.TypeChar & ":plain]<figcaption>" & caption & "</figcaption></figure>" & vbNewLine
            Else
                Dim s = "f:id:" & HATENA_ID & ":" & p.entry & p.TypeChar & ":plain"
                insText = "<figure class=""figure-image figure-image-fotolife mceNonEditable"" title=""" & caption & """>" & vbNewLine _
                    & "<p><img class=""hatena-fotolife"" title=""" & s & """ src=""" & p.ImageUrl & """ alt=""" & s & """ /></p>" & vbNewLine _
                    & "</figure>" & vbNewLine
            End If
        End If

        Dim selectPos As Integer = TextBoxContent.SelectionStart
        TextBoxContent.Text = TextBoxContent.Text.Insert(selectPos, insText)
        TextBoxContent.SelectionStart = selectPos
        TabControl1.SelectedTab = TabPageArticle 'タブを選択
    End Sub

    Private Sub setSyntaxMode(mode As Hatena.SyntaxMode)
        Select Case mode
            Case Hatena.SyntaxMode.HATENA
                LabelSyntax.Text = "はてな記法"
            Case Hatena.SyntaxMode.MARKDOWN
                LabelSyntax.Text = "マークダウン記法"
            Case Else
                LabelSyntax.Text = "見たままモード(HTML)"
        End Select
        LabelSyntax.Tag = mode
    End Sub

    '記事が選択された
    Private Async Sub ListViewArticle_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListViewArticles.SelectedIndexChanged
        TextBoxTitle.Text = ""
        CheckBoxDraft.Enabled = True
        With CheckedListBoxCategories
            For i As Integer = 0 To .Items.Count - 1
                .SetItemChecked(i, False)
            Next
        End With
        TextBoxContent.Text = ""
        LabelArticleEntry.Text = ""
        StatusLabel1.Text = ""
        ButtonDelete.Enabled = False

        If ListViewArticles.SelectedItems.Count = 1 Then

            TabControl1.SelectedTab = TabPageArticle '記事タブを選択

            '記事をダウンロード
            Dim entry As String = ListViewArticles.SelectedItems(0).SubItems(1).Text
            Dim a As Hatena.Article = Await Hatena.GetArticle(HATENA_BLOG_ID, entry)

            If a.result Then
                '成功時の処理
                TextBoxTitle.Text = a.title
                setSyntaxMode(a.GetSyntaxMode)
                CheckBoxDraft.Checked = a.draft
                CheckBoxDraft.Enabled = a.draft '公開済みは下書きに戻せない
                With CheckedListBoxCategories
                    For Each c In a.categories
                        Dim i = .Items.IndexOf(c)
                        If i < 0 Then i = .Items.Add(c)
                        .SetItemChecked(i, True)
                    Next
                End With
                TextBoxContent.Text = a.content
                LabelArticleEntry.Text = a.entryId
                StatusLabel1.Text = "記事をダウンロードしました"
                ButtonDelete.Enabled = True
            Else
                StatusLabel1.Text = a.errorMessage
            End If
        End If
    End Sub

    '新規ボタンがクリックされた
    Private Sub ButtonNew_Click(sender As Object, e As EventArgs) Handles ButtonNew.Click
        TextBoxTitle.Text = ""
        setSyntaxMode(DEFAULT_SYNTAX)
        CheckBoxDraft.Checked = True
        CheckBoxDraft.Enabled = True
        With CheckedListBoxCategories
            For i As Integer = 0 To .Items.Count - 1
                .SetItemChecked(i, False)
            Next
        End With
        TextBoxContent.Text = ""
        LabelArticleEntry.Text = ""
        StatusLabel1.Text = ""
        ButtonDelete.Enabled = False
    End Sub

    '投稿ボタンがクリックされた
    Private Async Sub ButtonPost_Click(sender As Object, e As EventArgs) Handles ButtonPost.Click

        Dim mode As Hatena.SyntaxMode = DirectCast(LabelSyntax.Tag, Hatena.SyntaxMode)
        If mode <> DEFAULT_SYNTAX Then
            Dim modeText As String
            Select Case DEFAULT_SYNTAX
                Case Hatena.SyntaxMode.HATENA
                    modeText = "はてな記法"
                Case Hatena.SyntaxMode.MARKDOWN
                    modeText = "マークダウン記法"
                Case Else
                    modeText = "見たままモード(HTML)"
            End Select
            If MsgBox("記事は'" & modeText & "'で投稿されます" & vbNewLine & "よろしいですか？",
                      MsgBoxStyle.OkCancel) <> MsgBoxResult.Ok Then Exit Sub
        End If

        Dim ar As New List(Of String)
        For Each c In CheckedListBoxCategories.CheckedItems
            ar.Add(c)
        Next

        If LabelArticleEntry.Text = "" Then
            '新規
            Dim a = Await Hatena.NewArticle(HATENA_BLOG_ID, TextBoxTitle.Text, TextBoxContent.Text,
                                            ar.ToArray, CheckBoxDraft.Checked)
            If a.result Then
                StatusLabel1.Text = "記事を投稿しました。" & a.entryId
                MsgBox("投稿しました")

                Dim lvi As ListViewItem = createListViewItem(a)
                ListViewArticles.Items.Insert(0, lvi)
                lvi.Selected = True
            Else
                StatusLabel1.Text = a.errorMessage
                MsgBox("投稿できませんでした")
            End If
        Else
            '更新
            Dim entryId As String = LabelArticleEntry.Text
            Dim a = Await Hatena.UpdateArticle(HATENA_BLOG_ID, entryId, TextBoxTitle.Text,
                                               TextBoxContent.Text, ar.ToArray, CheckBoxDraft.Checked)

            If a.result Then
                StatusLabel1.Text = "記事を更新しました。" & a.entryId
                MsgBox("更新しました")

                For Each lvi In ListViewArticles.Items
                    If DirectCast(lvi.tag, String) = entryId Then
                        updateListViewItem(lvi, a)
                        lvi.Selected = True
                        Exit For
                    End If
                Next
            Else
                StatusLabel1.Text = a.errorMessage
                MsgBox("更新できませんでした")
            End If

        End If
    End Sub

    '削除ボタンがクリックされた
    Private Async Sub ButtonDelete_Click(sender As Object, e As EventArgs) Handles ButtonDelete.Click

        Dim b = Await Hatena.test()
        Exit Sub

        If LabelArticleEntry.Text = "" Then Exit Sub
        If MsgBox("本当に削除しますか？", MsgBoxStyle.OkCancel) <> MsgBoxResult.Ok Then Exit Sub

        Dim entryId As String = LabelArticleEntry.Text
        Dim result As Boolean = Await Hatena.DeleteArticle(HATENA_BLOG_ID, entryId)

        If result Then
            StatusLabel1.Text = "記事を削除しました。" & entryId
            MsgBox("削除しました")

            For Each lvi In ListViewArticles.Items
                If DirectCast(lvi.tag, String) = entryId Then
                    ListViewArticles.Items.Remove(lvi)
                    Exit For
                End If
            Next
            ButtonNew.PerformClick()
        End If
    End Sub

    Private Sub ListViewPhotos_DragEnter(sender As Object, e As DragEventArgs) Handles ListViewPhotos.DragEnter
        'コントロール内にドラッグされたとき実行される
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            'ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
            e.Effect = DragDropEffects.Copy
        Else
            'ファイル以外は受け付けない
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Async Sub ListViewPhotos_DragDrop(sender As Object, e As DragEventArgs) Handles ListViewPhotos.DragDrop
        'コントロール内にドロップされたとき実行される
        'ドロップされたすべてのファイル名を取得する
        Dim fileNames As String() = CType(
            e.Data.GetData(DataFormats.FileDrop, False),
            String())

        StatusLabel1.Text = "画像をアップロードしています..."

        Dim count As Integer = 0
        For Each fileName In fileNames

            'ファイル毎にアップロード
            Dim p As Hatena.PhotoLifeDirectory.Photo = Await dir.AddPicture(fileName)

            If p IsNot Nothing Then

                'リストビューを更新
                If p.thumbnail IsNot Nothing Then
                    ListViewPhotos.LargeImageList.Images.Add(p.entry, ResizeImage(p.thumbnail, New Drawing.Size(IMAGE_SIZE, IMAGE_SIZE)))
                End If
                Dim lvi = createListViewItem(p)
                ListViewPhotos.Items.Insert(0, lvi)
                lvi.Selected = True
                count += 1
            End If
        Next

        MsgBox(count & "件アップロードしました")
        StatusLabel1.Text = count & "件の画像をアップロードしました"
    End Sub

    Private Class ListViewIndexComparer
        Implements IComparer
        Private Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Return CType(x, ListViewItem).Index - CType(y, ListViewItem).Index
        End Function
    End Class

End Class
