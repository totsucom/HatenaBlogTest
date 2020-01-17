Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports AngleSharp.Html.Parser
Imports AsyncOAuth

Public Class Hatena
    '定数
    Const POST_URL = "https://www.hatena.ne.jp/login"
    Const INITIATE_URL = "https://www.hatena.ne.jp/oauth/initiate"
    Const AUTHORIZE_URL = "https://www.hatena.ne.jp/oauth/authorize"
    Const ACCESS_TOKEN_URL = "https://www.hatena.ne.jp/oauth/token"
    Const BLOG_ATOM_URL = "https://blog.hatena.ne.jp/"

    'イベントハンドラ
    Public Shared Event LoginResult(result As Boolean, msg As String) 'ログイン結果
    Public Shared Event OAuthResult(result As Boolean, msg As String) 'OAuth認証結果
    Public Shared Event MyJsonResult(result As Boolean, my As MyJson) 'my.json取得結果
    Public Shared Event ArticlesResult(result As Boolean, articles As Articles) '記事一覧取得結果
    Public Shared Event CategoriesResult(result As Boolean, categories As String()) 'カテゴリー一覧取得結果

    'OAuth
    Private Shared _consumerKey As String = ""
    Private Shared _consumerSecret As String = ""
    Private Shared _accessToken As AccessToken = Nothing
    Private Shared _oauthClient As HttpClient = Nothing

    Private Shared _hatenaId As String = ""
    'Private Shared _dl As HttpDownloader = Nothing


    Public Shared ReadOnly Property ConsumerKey As String
        Get
            Return _consumerKey
        End Get
    End Property

    Public Shared ReadOnly Property ConsumerSecret As String
        Get
            Return _consumerSecret
        End Get
    End Property

    Public Shared ReadOnly Property AccessToken As AccessToken
        Get
            Return _accessToken
        End Get
    End Property

    'はてなブログにログインする
    '結果はイベントハンドラLoginResultで返す
    'HTTPダウンローダーのクッキーは有効にしておくこと
    Public Shared Async Sub Login(hatenaId As String, password As String)

        'ログイン処理
        Try
            Dim content As HttpContent = New FormUrlEncodedContent(
                New Dictionary(Of String, String)() From {{"name", hatenaId}, {"password", password}})

            Dim res As HttpResponseMessage = Await Form1.dl.Client.PostAsync(POST_URL, content)
            If res.StatusCode <> HttpStatusCode.OK Then
                RaiseEvent LoginResult(False, "ログインページを開けません。StatusCode=" & res.StatusCode.ToString)
                Exit Sub
            End If
            Try
                Dim x = res.Headers.GetValues("Set-Cookie") 'ログインが成功したらクッキーがセットされる。なければ例外が発生する
            Catch ex As InvalidOperationException
                RaiseEvent LoginResult(False, "ログインエラー")
                Exit Sub
            End Try
        Catch ex As System.Net.Http.HttpRequestException
            RaiseEvent LoginResult(False, "ネットワークを確認してください" & vbNewLine & ex.Message)
            Exit Sub
        Catch ex As Exception
            'よくわからん例外
            RaiseEvent LoginResult(False, "ログイン中の例外" & vbNewLine & ex.Message)
            Exit Sub
        End Try

        _hatenaId = hatenaId
        '_dl = dl

        '確認処理
        Try
            Dim res As HttpResponseMessage = Await Form1.dl.Client.GetAsync(POST_URL, HttpCompletionOption.ResponseHeadersRead)

            Dim parser = New HtmlParser
            Dim doc = Await parser.ParseDocumentAsync(Await res.Content.ReadAsStreamAsync())

            'htmlの例
            '<div id="body"><div id="container">
            '  <div class="oauth-message">
            '    <p>現在 totsucom(id:mae8bit) でログインしています。</p>

            Dim p = doc.QuerySelector("div.oauth-message p")
            If p IsNot Nothing AndAlso p.TextContent.IndexOf("(id:" & hatenaId & ")") >= 0 Then
                RaiseEvent LoginResult(True, "ログインしました")
            Else
                RaiseEvent LoginResult(False, "ログインを確認できませんでした")
            End If
        Catch ex As Exception
            'よくわからん例外
            RaiseEvent LoginResult(False, "ログイン確認中の例外" & vbNewLine & ex.Message)
        End Try
    End Sub

    'はてなブログのOAuth認証を実行する
    '結果はイベントハンドラOAuthResultで返す
    'ダウンローダーははてなログインを完了させておくこと
    Public Shared Async Sub OAuth(consumerKey As String, consumerSecret As String, scope As String)

        _consumerKey = ""
        _consumerSecret = ""
        _accessToken = Nothing
        _oauthClient = Nothing

        'OAuth用ハッシュ計算関数
        OAuthUtility.ComputeHash = Function(key, buffer)
                                       Using hmac = New System.Security.Cryptography.HMACSHA1(key)
                                           Return hmac.ComputeHash(buffer)
                                       End Using
                                   End Function

        Dim authorizer = New OAuthAuthorizer(consumerKey, consumerSecret)

        'リクエストトークンを取得
        Dim tokenResponse = Await authorizer.GetRequestToken(
            INITIATE_URL,
            New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("oauth_callback", "oob")},
            New FormUrlEncodedContent(New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("scope", scope)}))
        Dim requestToken As RequestToken = tokenResponse.Token()
        Dim pinRequestUrl As String = authorizer.BuildAuthorizeUrl(AUTHORIZE_URL, requestToken)

        'ピンコード取得許可ページを開く
        Dim res As HttpResponseMessage = Await Form1.dl.Client.GetAsync(pinRequestUrl, HttpCompletionOption.ResponseHeadersRead)
        If res.StatusCode <> HttpStatusCode.OK Then
            RaiseEvent OAuthResult(False, "許可ページを開けませんでした。StatusCode=" & res.StatusCode.ToString)
            Exit Sub
        End If

        Dim html As String = Await res.Content.ReadAsStringAsync()
        Dim parser = New HtmlParser
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)
        'Debug.Print("======================================================")
        'Debug.Print(text)

        '許可するボタンの情報を取得
        Dim postUri As Uri = Nothing, rkm As String = "", oauth_token As String = "", errorMessage As String = ""
        If Not GetAcceptInfo(pinRequestUrl, doc, postUri, rkm, oauth_token, errorMessage) Then
            If errorMessage = "" Then
                RaiseEvent OAuthResult(False, "HTMLパース失敗。許可するボタンを取得できませんでした")
            Else
                RaiseEvent OAuthResult(False, errorMessage)
            End If
            Exit Sub
        End If

        '許可するボタンを押す（ポスト）
        Dim content As HttpContent = New FormUrlEncodedContent(
                New Dictionary(Of String, String)() From {{"rkm", rkm}, {"oauth_token", oauth_token}})
        res = Await Form1.dl.Client.PostAsync(postUri, content)
        If res.StatusCode <> HttpStatusCode.OK Then
            RaiseEvent OAuthResult(False, "ピンコードのページを開けませんでした。StatusCode=" & res.StatusCode.ToString)
            Exit Sub
        End If

        '表示されたページからピンコードを取得
        doc = Await parser.ParseDocumentAsync(Await res.Content.ReadAsStreamAsync())
        Dim pinCode As String = getPinCode(doc)
        If pinCode Is Nothing Then
            RaiseEvent OAuthResult(False, "HTMLパース失敗。ピンコードを取得できませんでした")
            Exit Sub
        End If

        'アクセストークンを取得する
        Dim accessTokenResponse = Await authorizer.GetAccessToken(ACCESS_TOKEN_URL, requestToken, pinCode)
        _accessToken = accessTokenResponse.Token
        _consumerKey = consumerKey
        _consumerSecret = consumerSecret
        _oauthClient = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken)

        RaiseEvent OAuthResult(True, "OAuth認証が成功しました")
    End Sub


    '許可する、のページからポストに必要な情報を抜き取る
    Private Shared Function GetAcceptInfo(sourceUri As String, doc As AngleSharp.Html.Dom.IHtmlDocument,
                                      ByRef postUri As Uri, ByRef rkm As String,
                                      ByRef oauth_token As String, ByRef errorMessage As String) As Boolean
        'HTMLの例
        '<ul class="oauth-btn-list column2">
        '  <li>
        '    <form action="/oauth/authorize.deny" method="post">
        '      <input type="hidden" name="rkm" value="gU3ZV6YdgDSY1z5HpPnn6w">
        '      <input type="hidden" name="oauth_token" value="4Rndz3cI/CYdgg==">
        '      <input class="oauth-btn" type="submit" name="name" value="拒否する" />
        '    </form>
        '  </li>
        '  <li>
        '    <form action="/oauth/authorize" method="post">
        '      <input type="hidden" name="rkm" value="gU3ZV6YdgDSY1z5HpPnn6w">
        '      <input type="hidden" name="oauth_token" value="4Rndz3cI/CYdgg==">
        '      <input class="oauth-btn btn-yes" type="submit" name="name" value="許可する" />
        '    </form>
        '  </li>
        '</ul>

        'HTMLの例（失敗時）
        '<div id = "oauth-contents" >
        '  <header id = "title" >
        '    <hgroup>
        '      <h1>認証エラー</h1>
        '      <h2>
        '        <div class="error-message">
        '          <p>トークンが不正です。元のサイトに戻ってもう一度試してみてください。</p>
        '        </div></h2>

        errorMessage = ""

        '<form action= の値からポスト先アドレスを取得
        Dim input = doc.QuerySelector("input[value='許可する']")
        If input Is Nothing Then
            Dim p = doc.QuerySelector("div.error-message p")
            If p IsNot Nothing Then errorMessage = p.TextContent
            Return False
        End If
        Dim form = input.ParentElement
        Dim relUri As String = form.GetAttribute("action")
        If relUri Is Nothing Then Return False
        postUri = New Uri(New Uri(sourceUri), relUri)

        'ポストすべきデータを取得
        rkm = form.QuerySelector("input[name='rkm']").GetAttribute("value")
        oauth_token = form.QuerySelector("input[name='oauth_token']").GetAttribute("value")
        If rkm Is Nothing OrElse oauth_token Is Nothing Then Return False

        Return True
    End Function

    'ピンコードを抜き取る
    Private Shared Function getPinCode(doc As AngleSharp.Html.Dom.IHtmlDocument) As String
        'HTMLの例
        '<div id="oauth-contents">
        '    <header id="title">
        '      <div class="oauth-app-icon" style="background-image: url('');"></div>
        '      <hgroup>
        '        <h1>アクセスを許可しました</h1>
        '        <h2>アプリケーションにアクセスを許可しました。以下のコードをアプリケーションに入力して下さい。
        '           <div Class=verifier><pre>6+XXItabB3tukWyW2SZH2AAq</pre></div>
        '        </h2>
        '      </hgroup>

        Return doc.QuerySelector("div.verifier pre").TextContent
    End Function


    Public Class MyJson
        Public ReadOnly profile_image_url As String = ""
        Public ReadOnly profile_image As Image = Nothing 'result=Trueでも画像が入っているとは限らない
        Public ReadOnly url_name As String = ""
        Public ReadOnly display_name As String = ""
        Public Sub New(profile_image_url As String, url_name As String, display_name As String, profile_image As Image)
            Me.profile_image_url = profile_image_url
            Me.url_name = url_name
            Me.display_name = display_name
            Me.profile_image = profile_image
        End Sub
    End Class

    'My.jsonを取得する
    Public Shared Async Sub GetMyJson()

        '取得HTMLのサンプル http://developer.hatena.ne.jp/ja/documents/auth/apis/oauth/consumer

        Dim url = "http://n.hatena.com/applications/my.json"
        Dim res = Await _oauthClient.GetAsync(url)
        Dim text As String = Await res.Content.ReadAsStringAsync()

        'データの例
        '{"profile_image_url":"https://cdn.profile-image.st-hatena.com/users/mae8bit/profile.gif?1534548810","url_name":"mae8bit","display_name":"totsucom"}

        'これだけにライブラリ使うのもったいないので標準関数でパース
        Dim i = text.IndexOf("{")
        Dim j = text.LastIndexOf("}")
        If i < 0 OrElse j < 0 OrElse i > j Then
            RaiseEvent MyJsonResult(False, Nothing)
            Exit Sub
        End If
        Dim items = text.Substring(i + 1, j - i - 1).Split({","c})
        Dim p = "", u = "", d = "", b = 0
        For Each item In items
            i = item.IndexOf(""":""")
            If i < 0 Then
                RaiseEvent MyJsonResult(False, Nothing)
                Exit Sub
            End If
            Dim n = item.Substring(1, i - 1)
            Dim v = item.Substring(i + 3, item.Length - (i + 4))
            Select Case n
                Case "profile_image_url" : p = v : b = b Or 1
                Case "url_name" : u = v : b = b Or 2
                Case "display_name" : d = v : b = b Or 4
                Case Else
                    RaiseEvent MyJsonResult(False, Nothing)
                    Exit Sub
            End Select
        Next
        If b = 7 Then
            Dim img As Image = Nothing
            If p <> "" Then img = Await Form1.dl.DownloadImageAsync(p)
            RaiseEvent MyJsonResult(True, New MyJson(p, u, d, img))
        Else
            RaiseEvent MyJsonResult(False, Nothing)
        End If
    End Sub


    Public Enum SyntaxMode
        NONE = 0
        MITAMAMA = 1
        HATENA = 2
        MARKDOWN = 3
    End Enum

    Public Class Article
        Public result As Boolean
        Public errorMessage As String = ""
        Public editUrl As String = ""
        Public entryId As String = ""
        Public alternateUrl As String = ""
        Public title As String = ""
        Public updated As DateTime
        Public published As DateTime
        Public edited As DateTime
        Public summary As String = ""
        Public summaryType As String = ""
        Public content As String = ""
        Public contentType As String = ""
        Public categories As String() = Nothing
        Public draft As Boolean
        Public Function GetSyntaxMode() As SyntaxMode
            Select Case contentType.Trim.ToLower
                Case "text/html"
                    Return SyntaxMode.MITAMAMA
                Case "text/x-hatena-syntax"
                    Return SyntaxMode.HATENA
                Case "text/x-markdown"
                    Return SyntaxMode.MARKDOWN
                Case Else
                    Return SyntaxMode.NONE
            End Select
        End Function
    End Class

    Private Shared Function parseArticle(entry As AngleSharp.Dom.IElement) As Article
        Dim a As New Article
        Dim e As AngleSharp.Dom.Element
        a.editUrl = entry.QuerySelector("link[rel='edit']").GetAttribute("href")
        a.entryId = a.editUrl.Substring(a.editUrl.ToLower.IndexOf("/entry/") + 7)
        a.alternateUrl = entry.QuerySelector("link[rel='alternate']").GetAttribute("href")
        a.title = entry.QuerySelector("title").TextContent 'AngleSharpがアンエスケープをやってくれる
        a.updated = DateTime.Parse(entry.QuerySelector("updated").TextContent)
        a.published = DateTime.Parse(entry.QuerySelector("published").TextContent)
        a.edited = DateTime.Parse(entry.QuerySelector("app_edited").TextContent)
        e = entry.QuerySelector("summary")
        a.summary = e.TextContent
        a.summaryType = e.GetAttribute("type")
        e = entry.QuerySelector("content")
        a.content = unescapeEnter(e.TextContent) 'AngleSharpがアンエスケープをやってくれる
        a.contentType = e.GetAttribute("type")
        Dim ar As New List(Of String)
        For Each e In entry.QuerySelectorAll("category") '一覧の場合は存在しない
            ar.Add(e.GetAttribute("term"))
        Next
        a.categories = ar.ToArray
        a.draft = (entry.QuerySelector("app_draft").TextContent.Trim.ToLower = "yes")
        Return a
    End Function


    Public Class Articles
        Public result As Boolean
        Public errorMessage As String = ""
        Public nextUrl As String = ""   '次の一覧を取得するためのURL
        Public articles As Article() = Nothing
    End Class

    '記事の一覧を取得する
    '結果はイベントArticlesResultで返される
    Public Shared Sub GetArticles(blogId As String)
        GetNextArticles(BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/entry")
    End Sub

    '記事の一覧を取得する（２ページ目以降）
    'urlには前の Articles.nextUrl を渡す
    '結果はイベントArticlesResultで返される
    Public Shared Async Sub GetNextArticles(url As String)

        '取得HTMLのサンプル http://developer.hatena.ne.jp/ja/documents/blog/apis/atom

        Dim r As New Articles
        Dim res = Await _oauthClient.GetAsync(url)
        If Not res.IsSuccessStatusCode Then
            r.result = False
            r.errorMessage = "取得失敗。レスポンスコード " & res.StatusCode.ToString
            RaiseEvent ArticlesResult(False, r)
            Exit Sub
        End If

        Dim html As String = (Await res.Content.ReadAsStringAsync) _
            .Replace("app:edited>", "app_edited>") _
            .Replace("app:draft>", "app_draft>")
        Dim parser = New HtmlParser()
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)
        'Debug.Print("======================================================")
        'Debug.Print(html)

        Dim link = doc.QuerySelector("link[rel='next']")
        If link IsNot Nothing Then
            r.nextUrl = link.GetAttribute("href")
        Else
            r.nextUrl = ""
        End If

        Dim ar As New List(Of Article)
        For Each entry As AngleSharp.Dom.IElement In doc.QuerySelectorAll("entry")
            ar.Add(parseArticle(entry))
        Next
        r.articles = ar.ToArray

        RaiseEvent ArticlesResult(True, r)
    End Sub

    'signature_invalidになってしまう
    Public Shared Async Function test() As Task(Of Boolean)

        Dim url = "http://f.hatena.ne.jp/atom/feed"
        Dim res = Await _oauthClient.GetAsync(url)
        Dim html As String = Await res.Content.ReadAsStringAsync()
        Debug.Print("=======================================")
        Debug.Print(html)
        Debug.Print("=======================================")
        Return True
    End Function


    '記事を取得する
    Public Shared Async Function GetArticle(blogId As String, entryId As String) As Task(Of Article)

        '取得HTMLのサンプル http://developer.hatena.ne.jp/ja/documents/blog/apis/atom

        Dim url = BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/entry/" & entryId
        Dim res = Await _oauthClient.GetAsync(url)

        Dim a As Article
        If res.StatusCode <> 200 Then
            a = New Article
            a.result = False
            a.errorMessage = "取得失敗。レスポンスコード " & res.StatusCode.ToString
            Return a
        End If

        Dim html As String = (Await res.Content.ReadAsStringAsync()) _
            .Replace("app:edited>", "app_edited>") _
            .Replace("app:draft>", "app_draft>")
        Dim parser = New HtmlParser()
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)

        Dim entry = doc.QuerySelector("entry")
        If entry IsNot Nothing Then
            a = parseArticle(entry)
            a.result = True
            a.errorMessage = ""
        Else
            a = New Article
            a.result = False
            a.errorMessage = "HTMLパースエラー"
        End If

        Return a
    End Function


    '記事を投稿する
    Public Shared Async Function NewArticle(blogId As String, title As String, content As String,
                                             categories As String(), draft As Boolean) As Task(Of Article)
        'POST
        Dim contentXml As String = createPostContent(title, content, categories, draft)
        Dim url As String = BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/entry"
        Dim res = Await _oauthClient.PostAsync(url, New StringContent(contentXml, Encoding.UTF8))

        Dim a As Article
        If res.StatusCode <> 201 Then
            a = New Article
            a.result = False
            a.errorMessage = "投稿失敗。レスポンスコード " & res.StatusCode.ToString
            Return a
        End If

        Dim html As String = (Await res.Content.ReadAsStringAsync()) _
            .Replace("app:edited>", "app_edited>") _
            .Replace("app:draft>", "app_draft>")
        Dim parser = New HtmlParser()
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)

        Dim entry = doc.QuerySelector("entry")
        If entry IsNot Nothing Then
            a = parseArticle(entry)
            a.result = True
            a.errorMessage = ""
        Else
            a = New Article
            a.result = False
            a.errorMessage = "HTMLパースエラー"
        End If
        Return a
    End Function


    '記事を更新する
    '公開している記事を更新で下書きには戻せないみたい。HTTPレスポンスコード400が返る
    Public Shared Async Function UpdateArticle(blogId As String, entryId As String,
                                               title As String, content As String,
                                               categories As String(), draft As Boolean) As Task(Of Article)
        'PUT
        Dim contentXml As String = createPostContent(title, content, categories, draft)
        Dim url As String = BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/entry/" & entryId
        Dim res = Await _oauthClient.PutAsync(url, New StringContent(contentXml, Encoding.UTF8))

        Dim a As Article
        If res.StatusCode <> 200 Then
            a = New Article
            a.result = False
            a.errorMessage = "更新失敗。レスポンスコード " & res.StatusCode.ToString
            Return a
        End If

        Dim html As String = (Await res.Content.ReadAsStringAsync()) _
            .Replace("app:edited>", "app_edited>") _
            .Replace("app:draft>", "app_draft>")
        Dim parser = New HtmlParser()
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)

        Dim entry = doc.QuerySelector("entry")
        If entry IsNot Nothing Then
            a = parseArticle(entry)
            a.result = True
            a.errorMessage = ""
        Else
            a = New Article
            a.result = False
            a.errorMessage = "HTMLパースエラー"
        End If

        Return a
    End Function

    '投稿用XMLを作成する
    Private Shared Function createPostContent(title As String, content As String,
                                              categories As String(), draft As Boolean) As String

        '設定HTMLのサンプル http://developer.hatena.ne.jp/ja/documents/blog/apis/atom

        Dim sb As New Text.StringBuilder
        sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>")
        sb.Append(vbLf)
        sb.Append("<entry xmlns=""http://www.w3.org/2005/Atom"" xmlns:app=""http://www.w3.org/2007/app"">")
        sb.Append(vbLf)
        sb.Append("<title>")
        sb.Append(Web.HttpUtility.HtmlEncode(title))
        sb.Append("</title>")
        sb.Append(vbLf)
        sb.Append("<author><name>")
        sb.Append(_hatenaId)
        sb.Append("</name></author>")
        sb.Append(vbLf)
        sb.Append("<content type=""text/plain"">")
        sb.Append(Web.HttpUtility.HtmlEncode(content))
        sb.Append("</content>")
        sb.Append(vbLf)
        sb.Append("<updated>")
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd"))
        sb.Append("T")
        sb.Append(DateTime.Now.ToString("HH:mm:ss"))
        sb.Append("</updated>")
        sb.Append(vbLf)
        If categories IsNot Nothing Then
            For Each cat As String In categories
                sb.Append("<category term=""")
                sb.Append(cat)
                sb.Append(""" />")
                sb.Append(vbLf)
            Next
        End If
        sb.Append("<app:control><app:draft>")
        sb.Append(IIf(draft, "yes", "no"))
        sb.Append("</app:draft></app:control>")
        sb.Append(vbLf)
        sb.Append("</entry>")
        sb.Append(vbLf)
        Debug.Print("投稿データ========================")
        Debug.Print(sb.ToString)
        Return sb.ToString
    End Function

    Private Shared Function escapeXml(text As String) As String
        Return text.Replace(vbCrLf, vbLf).Replace("""", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;")
    End Function

    'Private Shared Function unescapeXml(text As String) As String
    '    Dim s = text.Replace("&amp;", "&").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&apos;", "'").Replace("&quot;", """")
    '    If s.IndexOf(vbCrLf) < 0 Then s = s.Replace(vbLf, vbCrLf)
    '    Return s
    'End Function

    Private Shared Function escapeEnter(text As String) As String
        Return text.Replace(vbCrLf, vbLf)
    End Function

    Private Shared Function unescapeEnter(text As String) As String
        'AngleSharpは文字のアンエスケープはやってくれるので
        'ここでは改行コードのアンエスケープのみを行っている
        If text.IndexOf(vbCrLf) < 0 Then
            Return text.Replace(vbLf, vbCrLf)
        Else
            Return text
        End If
    End Function


    '記事を削除する
    Public Shared Async Function DeleteArticle(blogId As String, entryId As String) As Task(Of Boolean)

        Dim url As String = BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/entry/" & entryId

        'DELETE
        Dim res = Await _oauthClient.DeleteAsync(url)
        Return (res.StatusCode = 200)
    End Function

    'カテゴリー一覧を取得する
    'イベントハンドラで結果を返す
    Public Shared Async Sub GetCategories(blogId As String)
        Dim url = BLOG_ATOM_URL & _hatenaId & "/" & blogId & "/atom/category"
        Dim res = Await _oauthClient.GetAsync(url)

        'HTMLの例
        '<app:categories    xmlns:app="http://www.w3.org/2007/app"    xmlns:atom="http://www.w3.org/2005/Atom"    fixed="no">
        '  <atom:category term="basicio.h" />
        '  <atom:category term="TWELITE DIP" />
        '  <atom:category term="サンプルコード" />
        '</app:categories>

        Dim html As String = (Await res.Content.ReadAsStringAsync()) _
            .Replace("app:categories", "app_categories") _
            .Replace("<atom:category", "<atom_category")
        Dim parser = New HtmlParser()
        Dim doc As AngleSharp.Html.Dom.IHtmlDocument = Await parser.ParseDocumentAsync(html)

        Dim cats = doc.QuerySelector("app_categories")
        If cats Is Nothing Then
            RaiseEvent CategoriesResult(False, Nothing)
            Exit Sub
        End If

        Dim ar As New List(Of String)
        For Each c In cats.QuerySelectorAll("atom_category")
            ar.Add(c.GetAttribute("term"))
        Next
        RaiseEvent CategoriesResult(True, ar.ToArray)
    End Sub




    'フォトライフの１つのフォルダのウェブ１ページ分のフォトを処理するクラス
    'このクラスでは一時的なデータしか保持しないので、ユーザー側で保持すること
    Public Class PhotoLifeDirectory
        Public Shared Event ThumbnailsDownloaded(dir As PhotoLifeDirectory, errorMessage As String)

        'サムネイル画像
        Public Class Photo
            Public ReadOnly entry As String
            Public ReadOnly thumbnailUrl As String
            Public ReadOnly title As String
            Public thumbnail As Image = Nothing
            Private _hatenaId As String
            Private _imageUrl As String = ""  'キャッシュ
            Private _typeChar As Char '画像形式を表す文字(はてな準拠)
            Private _image As Image = Nothing 'キャッシュ
            Public Sub New(hatenaId As String, title As String, thumbnailUrl As String)
                _hatenaId = hatenaId
                Me.title = title
                Me.thumbnailUrl = thumbnailUrl
                Dim i = thumbnailUrl.LastIndexOf("/"c)
                Dim j = thumbnailUrl.LastIndexOf("_"c)
                entry = thumbnailUrl.Substring(i + 1, j - i - 1)
            End Sub
            Public ReadOnly Property ImageUrl As String
                Get
                    Return _imageUrl
                End Get
            End Property
            Public ReadOnly Property TypeChar As Char
                Get
                    Return _typeChar
                End Get
            End Property

            '元画像を取得する
            Public Async Function GetImage() As Task(Of Image)
                If _image IsNot Nothing Then Return _image

                If _imageUrl = "" Then
                    Dim url = "https://f.hatena.ne.jp/" & _hatenaId & "/" & entry
                    Dim doc = Await Form1.dl.DownloadParserAsync(url)
                    If doc Is Nothing Then Return Nothing

                    'HTMLの例
                    '<div id="foto-body" name="foto-body" class="foto-body" style="width:1920px;">
                    '<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/mae8bit/20200116/20200116115758.jpg" alt="4バイト送信遅延" title="4バイト送信遅延" width="1920" height="1080" class="foto" style="" />

                    Dim imgt = doc.QuerySelector("#foto-body img")
                    If imgt Is Nothing Then Return Nothing

                    _imageUrl = imgt.GetAttribute("src")
                End If

                Dim img = Await Form1.dl.DownloadImageAsync(_imageUrl)

                'イメージのファイル形式を調べる
                If img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif) Then
                    Debug.Print("GIF")
                    _typeChar = "g"c

                    '動画GIFのときPictureBoxで例外が発生するので(常にかどうかは不明)、形式変換する
                    Try
                        Using ms As New IO.MemoryStream
                            img.Save(ms, Imaging.ImageFormat.Jpeg)
                            img = Image.FromStream(ms)
                        End Using
                    Catch ex As Exception
                        Debug.Print("GIF=>JPG変換失敗")
                        Return Nothing
                    End Try
                ElseIf img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) Then
                    Debug.Print("JPG")
                    _typeChar = "j"c
                ElseIf img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) Then
                    Debug.Print("PNG")
                    _typeChar = "p"c
                Else
                    Debug.Print("対応していない形式です")
                    Return Nothing
                End If

                _image = img
                Return _image
            End Function
        End Class
        Private _photos As Photo() = Nothing

        'サムネイルダウンロードに用いる内部クラス
        Private Class pldDummy
            Public requester As PhotoLifeDirectory
            Public downloadingIndex As Integer
            Public Sub New(requester As PhotoLifeDirectory, downloadingIndex As Integer)
                Me.requester = requester
                Me.downloadingIndex = downloadingIndex
            End Sub
        End Class

        'サムネイル画像のダウンロード処理を管理
        Public nextDownloadIndex As Integer
        Public processCount As Integer

        'Private _dl As HttpDownloader = Nothing
        Private _hatenaId As String = ""
        Private _directory As String = ""
        Private _currentPage As Integer
        Private _nextPageUrl As String

        '取得したサムネイルの一覧を返す
        'Nothingの場合もある
        Public ReadOnly Property Photos As Photo()
            Get
                Return _photos
            End Get
        End Property

        '現在のページ番号を返す。1~
        Public ReadOnly Property CurrentPage As Integer
            Get
                Return _currentPage
            End Get
        End Property

        '次ページのURLを返す。無い場合は空文字を返す
        Public ReadOnly Property NextPageUrl As String
            Get
                Return _nextPageUrl
            End Get
        End Property

        'フォトライフのディレクトリページ("Hatena Blog" など)からサムネイル画像を読み出す
        'ダウンローダーははてなにログインしていおくこと
        '結果は ThumbnailsDownloaded イベントで返す
        Public Sub New(hatenaId As String, directory As String)
            '_dl = dl
            _hatenaId = hatenaId
            _directory = directory
            _currentPage = 0
            _nextPageUrl = ""

            'ダウンロードハンドラを追加
            AddHandler Form1.dl.Downloaded, AddressOf dl_Downloaded

            'ページをダウンロード開始
            Dim uri = "https://f.hatena.ne.jp/" & hatenaId
            If directory = "" OrElse directory(0) <> "/"c Then uri &= "/"
            uri &= System.Uri.EscapeDataString(directory)
            If uri.Last <> "/"c Then uri &= "/"c
            uri &= "?sort=new"
            Form1.dl.DownloadParserAsync(uri, New pldDummy(Me, -1))
        End Sub

        Protected Overrides Sub Finalize()

            'ダウンロードハンドラを削除
            RemoveHandler Form1.dl.Downloaded, AddressOf dl_Downloaded

            MyBase.Finalize()
        End Sub

        '次のページをダウンロードする。無い場合はFalseを返す
        'ダウンロードしていないとき、つまり ThumbnailsDownloadedイベントを受け取った後に実行すること
        '結果は ThumbnailsDownloaded イベントで返す
        Public Function DownloadNextPage() As Boolean
            If _nextPageUrl = "" Then Return False

            '次ページをダウンロード開始
            Form1.dl.DownloadParserAsync(_nextPageUrl & "&sort=new", New pldDummy(Me, -1))

            _nextPageUrl = ""
            Return True
        End Function

        'サムネイルのページまたはサムネイル画像のダウンロード処理が完了した
        Private Sub dl_Downloaded(sender As HttpDownloader, uri As String, output As Object,
                                  res As HttpResponseMessage, tag As Object)

            If TypeName(tag) <> "pldDummy" Then Exit Sub 'PhotoLifeDirectoryのリクエストではない
            Dim pld = DirectCast(tag, pldDummy)
            If pld.requester IsNot Me Then Exit Sub '自身のリクエストではない

            Dim downloadingIndex = pld.downloadingIndex

            If downloadingIndex = -1 Then
                'サムネイル一覧のページをダウンロードした
                _currentPage += 1

                If res IsNot Nothing Then
                    If res.StatusCode = Net.HttpStatusCode.OK Then
                        'フォトライフのページが開けたので
                        'サムネイルのURL一覧を取得する
                        Dim dom As AngleSharp.Html.Dom.IHtmlDocument = DirectCast(output, AngleSharp.Html.Dom.IHtmlDocument)
                        _photos = getPhotoListFromHtml(_hatenaId, dom)

                        If _photos.Count = 0 Then
                            'サムネイルが見つからない
                            RaiseEvent ThumbnailsDownloaded(Me, "No thumbnails")
                        Else
                            nextDownloadIndex = 0   '次にダウンロード開始する_photos()インデックス
                            processCount = 0        'ダウンロード中のアイテム数
                            Do
                                If nextDownloadIndex > UBound(_photos) OrElse processCount = 3 Then Exit Do
                                Form1.dl.DownloadImageAsync(_photos(nextDownloadIndex).thumbnailUrl,
                                                              New pldDummy(Me, nextDownloadIndex))
                                nextDownloadIndex += 1
                                processCount += 1
                            Loop
                        End If

                        '次ページの有無のチェック
                        Dim pagerLinks = dom.QuerySelectorAll("div.pager a")
                        If pagerLinks IsNot Nothing Then
                            Dim nextPage As String = (_currentPage + 1).ToString
                            For Each a In pagerLinks
                                If a.TextContent = nextPage Then
                                    _nextPageUrl = "https://f.hatena.ne.jp" & a.GetAttribute("href")
                                    Exit For
                                End If
                            Next
                        End If
                    Else
                        'HTTPエラー
                        RaiseEvent ThumbnailsDownloaded(Me, "StatusCode=" & res.StatusCode.ToString & " " & uri)
                    End If
                Else
                    'ダウンローダーで例外
                    RaiseEvent ThumbnailsDownloaded(Me, DirectCast(output, String))
                End If
            Else
                'サムネイル画像をダウンロードした

                If res IsNot Nothing Then
                    If res.StatusCode = Net.HttpStatusCode.OK Then
                        'サムネイルをダウンロードできた?(Nothingの場合もある)
                        _photos(downloadingIndex).thumbnail = DirectCast(output, Image)
                    Else
                        'HTTPエラー
                        Debug.Print("HTTPエラー" & res.StatusCode.ToString)
                        _photos(downloadingIndex).thumbnail = Nothing
                    End If
                Else
                    'ダウンローダーで例外
                    Debug.Print("ダウンローダーで例外 " & vbNewLine & DirectCast(output, String))
                    _photos(downloadingIndex).thumbnail = Nothing
                End If
                processCount -= 1

                If nextDownloadIndex <= UBound(_photos) Then
                    '次をダウンロード
                    Form1.dl.DownloadImageAsync(_photos(nextDownloadIndex).thumbnailUrl,
                                                  New pldDummy(Me, nextDownloadIndex))
                    nextDownloadIndex += 1
                    processCount += 1
                End If

                If processCount = 0 Then
                    'すべてのダウンロード処理が終わった
                    RaiseEvent ThumbnailsDownloaded(Me, "")
                End If
            End If
        End Sub

        'フォトライフのHTMLからURLとエントリを読み出す
        Private Shared Function getPhotoListFromHtml(hatenaId As String, doc As AngleSharp.Html.Dom.IHtmlDocument) As Photo()
            '例
            '<ul class="fotolist">
            '    <li><a href="/mae8bit/20200103100144"><img onload="window.$FR ? $FR.centering(this) : '';" class="foto_thumb" src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/mae8bit/20200103/20200103100144_120.jpg" title="20200103100144" alt="20200103100144" /></a></li>
            '    <li><a href="/mae8bit/20200103092704"><img onload="window.$FR ? $FR.centering(this) : '';" class="foto_thumb" src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/mae8bit/20200103/20200103092704_120.jpg" title="20200103092704" alt="20200103092704" /></a></li>

            Dim ar As New List(Of Photo)
            For Each li In doc.QuerySelector("ul.fotolist").Children()
                Dim img = li.QuerySelector("img.foto_thumb")
                Dim url = img.GetAttribute("src")
                Dim title = img.GetAttribute("title")
                If title Is Nothing Then title = img.GetAttribute("alt")
                If url <> "" AndAlso title <> "" Then
                    ar.Add(New Photo(hatenaId, title, url))
                End If
            Next
            Return ar.ToArray
        End Function

        '画像をアップロードする
        'titleが未指定の場合はファイル名が用いられる。
        'photoSizeは長辺の長さを指定できる。未指定の場合はオリジナルの大きさを用いる。
        Public Async Function AddPicture(filePath As String, Optional title As String = "", Optional fotoSize As Integer = 0) As Task(Of Photo)

            Dim img As Image
            Try
                img = Image.FromFile(filePath)
            Catch ex As Exception
                Debug.Print("イメージを開けない")
                Return Nothing
            End Try

            If img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif) Then
            ElseIf img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) Then
            ElseIf img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) Then
            Else
                Debug.Print("未対応の画像形式")
                Return Nothing
            End If

            Dim url = "https://f.hatena.ne.jp/" & _hatenaId & "/up?mode=classic" '"http://localhost/" & _hatenaId & "/up.php?mode=classic"
            Dim dom As AngleSharp.Html.Dom.IHtmlDocument = Await Form1.dl.DownloadParserAsync(url)
            If dom Is Nothing Then
                Debug.Print("ページを開けません " & url)
                Return Nothing
            End If

            'コンテンツを準備
            Dim mode = dom.QuerySelector("input[name='mode']").GetAttribute("value")
            Dim rkm = dom.QuerySelector("input[name='rkm']").GetAttribute("value")
            If title = "" Then title = System.IO.Path.GetFileNameWithoutExtension(filePath)
            If fotoSize <= 0 Then fotoSize = IIf(img.Width > img.Height, img.Width, img.Height)

            'コンテンツを作成
            Dim content = New MultipartFormDataContent
            content.Add(New StringContent(mode), "mode")
            content.Add(New StringContent(rkm), "rkm")

            content.Add(New StringContent(title), "fototitle1")
            content.Add(New StringContent(fotoSize.ToString), "fotosize1")
            content.Add(New ByteArrayContent(System.IO.File.ReadAllBytes(filePath)), "image1", "test.jpg")
            For i = 2 To 5
                content.Add(New StringContent(""), "fototitle" & i)
                content.Add(New StringContent("800"), "fotosize" & i)
                content.Add(New StringContent(""), "image" & i)
            Next

            content.Add(New StringContent(_directory.Replace("/", "")), "folder")
            content.Add(New StringContent(""), "taglist")
            content.Add(New StringContent("800"), "fotosize")

            'POST
            url = "https://f.hatena.ne.jp/" & _hatenaId & "/up?sort=new" '"http://localhost/" & _hatenaId & "/up.php"
            Dim res = Await Form1.dl.Client.PostAsync(url, content)

            If Not res.IsSuccessStatusCode Then
                Debug.Print("送信失敗。ステータスコード" & res.StatusCode.ToString)
                Return Nothing
            End If
            Dim html As String = Await res.Content.ReadAsStringAsync
            Debug.Print("ポストの戻り値 =========================================")
            Debug.Print(html)
            Dim parser = New HtmlParser
            Dim doc = parser.ParseDocument(html)   'Await parser.ParseDocumentAsync(Await res.Content.ReadAsStreamAsync())

            Dim div = doc.QuerySelector("div.errormessage")
            If div IsNot Nothing Then
                'アップロード失敗
                Debug.Print(div.TextContent())
                Return Nothing
            End If

            'HTMLの例
            '<ul class="fotolist">
            '<li><a href="/mae8bit/20200116102239"><img onload="window.$FR ? $FR.centering(this) : '';" class="foto_thumb" src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/mae8bit/20200116/20200116102239_120.jpg" title="AAA" alt="AAA" /></a></li>
            '<li><a href="/mae8bit/20200116102020"><img onload="window.$FR ? $FR.centering(this) : '';" class="foto_thumb" src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/mae8bit/20200116/20200116102020_120.jpg" title="モノスティックのピン配置" alt="モノスティックのピン配置" /></a></li>

            Dim photos As Photo() = getPhotoListFromHtml(_hatenaId, doc)
            If photos Is Nothing OrElse photos.Count = 0 Then
                Debug.Print("アップロードできましたが取得できませんでした")
                Return Nothing
            End If

            'アップロード後に表示されるサムネイル一覧の中から、最初に一致する同一タイトルを探す  
            For Each p In photos
                If p.title = title Then

                    'サムネイルをダウンロードしたらユーザーに返す
                    p.thumbnail = Await Form1.dl.DownloadImageAsync(p.thumbnailUrl)
                    Return p
                End If
            Next

            Debug.Print("アップロードできましたが取得できませんでした")
            Return Nothing
        End Function

    End Class
End Class
