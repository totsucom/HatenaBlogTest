'イベントハンドラを使ったHTTPダウンロード

'2020/1/11
'HttpClientは使いまわすべきだそうで、クラス変数とした
'クッキーに対応させた

Imports System.IO
Imports System.Net.Http
Imports AngleSharp.Html.Parser

Public Class HttpDownloader

    'イベントハンドラ(res.StatusCodeで成功の有無を確認すること)
    '例外の場合はres=Nothingでoutputにメッセージが格納される
    Public Event Downloaded(sender As HttpDownloader, Uri As String, output As Object, res As HttpResponseMessage, tag As Object)

    Private _client As HttpClient = Nothing
    Private _clientHandler As HttpClientHandler = Nothing
    Private _cookie As System.Net.CookieContainer = Nothing

    Public ReadOnly Property Client As HttpClient
        Get
            Return _client
        End Get
    End Property


    'クッキーを渡して初期化
    Sub New(cookie As System.Net.CookieContainer)
        If cookie Is Nothing Then
            _client = New HttpClient()
            _clientHandler = Nothing
            _cookie = Nothing
        Else
            _cookie = cookie
            _clientHandler = New HttpClientHandler
            _clientHandler.CookieContainer = _cookie
            _clientHandler.UseCookies = True
            _client = New HttpClient(_clientHandler)
        End If
    End Sub

    '一般的な初期化。クッキーを使うかどうか指定できる
    Sub New(bUseCookies As Boolean)
        If Not bUseCookies Then
            _client = New HttpClient()
            _clientHandler = Nothing
            _cookie = Nothing
        Else
            _clientHandler = New HttpClientHandler
            _cookie = New System.Net.CookieContainer
            _clientHandler.CookieContainer = _cookie
            _clientHandler.UseCookies = True
            _client = New HttpClient(_clientHandler)
        End If
    End Sub


    'tag は管理するクラスなどを渡しておけば、複数のダウンロードを実行したときに
    '何のダウンロードだったか容易に識別できる

    '================================================================================================================
    'ファイルにダウンロードする。イベントハンドラ版
    Public Async Sub DownloadFileAsync(ByVal uri As String, ByVal outputPath As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            Using fileStream = File.Create(outputPath)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(fileStream)
                    fileStream.Flush()
                End Using
            End Using
            RaiseEvent Downloaded(Me, uri, outputPath, res, tag) '成功
        Catch ex As Exception
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub

    'ファイルにダウンロードする。Await版
    Public Async Function DownloadFileAsync(ByVal uri As String, ByVal outputPath As String) As Task(Of Boolean)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then Return False
            Using fileStream = File.Create(outputPath)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(fileStream)
                    fileStream.Flush()
                End Using
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    '================================================================================================================
    'メモリストリームにダウンロードする。イベントハンドラ版
    Public Async Sub DownloadMemoryStreamAsync(ByVal uri As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then
                Debug.Print("DownloadMemoryStreamAsync(イベント版) ステータスコード" & res.StatusCode.ToString)
                RaiseEvent Downloaded(Me, uri, "ステータスコード" & res.StatusCode.ToString, Nothing, tag) '失敗
            Else
                Dim memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(memoryStream)
                End Using
                RaiseEvent Downloaded(Me, uri, memoryStream, res, tag) '成功
            End If
        Catch ex As Exception
            Debug.Print("DownloadMemoryStreamAsync(イベント版)の例外")
            Debug.Print(ex.Message)
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub

    'メモリストリームにダウンロードする。Await版
    Public Async Function DownloadMemoryStreamAsync(ByVal uri As String) As Task(Of MemoryStream)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then
                Debug.Print("DownloadMemoryStreamAsync(関数版) ステータスコード" & res.StatusCode.ToString)
                Return Nothing
            End If
            Dim memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
            Using httpStream = Await res.Content.ReadAsStreamAsync()
                httpStream.CopyTo(memoryStream)
            End Using
            Return memoryStream
        Catch ex As Exception
            Debug.Print("DownloadMemoryStreamAsync(関数版)の例外")
            Debug.Print(ex.Message)
            Return Nothing
        End Try
    End Function

    '================================================================================================================
    'イメージでダウンロードする。イベントハンドラ版
    Public Async Sub DownloadImageAsync(ByVal uri As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then
                Debug.Print("DownloadImageAsync(イベント版) ステータスコード" & res.StatusCode.ToString)
                RaiseEvent Downloaded(Me, uri, "ステータスコード" & res.StatusCode.ToString, Nothing, tag) '失敗
            Else
                Dim img As Image = Nothing
                Using memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
                    Using httpStream = Await res.Content.ReadAsStreamAsync()
                        httpStream.CopyTo(memoryStream)
                        img = Image.FromStream(memoryStream)
                    End Using
                End Using
                RaiseEvent Downloaded(Me, uri, img, res, tag) '成功
            End If
        Catch ex As Exception
            Debug.Print("DownloadMemoryStreamAsync(イベント版)の例外")
            Debug.Print(ex.Message)
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub

    'イメージでダウンロードする。Await版
    Public Async Function DownloadImageAsync(ByVal uri As String) As Task(Of Image)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then
                Debug.Print("DownloadMemoryStreamAsync(関数版) ステータスコード" & res.StatusCode.ToString)
                Return Nothing
            End If
            Dim img As Image = Nothing
            Using memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(memoryStream)
                    img = Image.FromStream(memoryStream)
                End Using
            End Using
            Return img
        Catch ex As Exception
            Debug.Print("DownloadMemoryStreamAsync(関数版)の例外")
            Debug.Print(ex.Message)
            Return Nothing
        End Try
    End Function

    '================================================================================================================
    'ダウンロードしてテキストで返す。イベントハンドラ版
    Public Async Sub DownloadTextAsync(ByVal uri As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            Dim text As String = Await res.Content.ReadAsStringAsync
            RaiseEvent Downloaded(Me, uri, text, res, tag) '成功
        Catch ex As Exception
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub
    'ダウンロードしてテキストで返す。Await版
    Public Async Function DownloadTextAsync(ByVal uri As String) As Task(Of String)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then Return ""
            Dim text As String = Await res.Content.ReadAsStringAsync
            Return text
        Catch ex As Exception
            Return ""
        End Try
    End Function

    '================================================================================================================
    'ダウンロードしてバイト配列で返す。イベントハンドラ版
    Public Async Sub DownloadBytesAsync(ByVal uri As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            Dim bs As Byte()
            Using memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(memoryStream)
                End Using
                bs = memoryStream.ToArray()
            End Using
            RaiseEvent Downloaded(Me, uri, bs, res, tag) '成功
        Catch ex As Exception
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub

    'ダウンロードしてバイト配列で返す。Await版
    Public Async Function DownloadBytesAsync(ByVal uri As String) As Task(Of Byte())
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then Return Nothing
            Dim bs As Byte()
            Using memoryStream = New MemoryStream(res.Content.Headers.ContentLength)
                Using httpStream = Await res.Content.ReadAsStreamAsync()
                    httpStream.CopyTo(memoryStream)
                End Using
                bs = memoryStream.ToArray()
            End Using
            Return bs
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    '================================================================================================================
    'HTMLパーサーにダウンロードする。イベントハンドラ版
    'イベントハンドラで返される値は AngleSharp.Html.Dom.IHtmlDocument 型
    Public Async Sub DownloadParserAsync(ByVal uri As String, Optional tag As Object = Nothing)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            Dim parser = New HtmlParser
            Dim doc = Await parser.ParseDocumentAsync(Await res.Content.ReadAsStreamAsync())
            RaiseEvent Downloaded(Me, uri, doc, res, tag) '成功
        Catch ex As Exception
            RaiseEvent Downloaded(Me, uri, ex.Message, Nothing, tag) '失敗
        End Try
    End Sub

    'HTMLパーサーにダウンロードする。Await版
    'イベントハンドラで返される値は AngleSharp.Html.Dom.IHtmlDocument 型
    Public Async Function DownloadParserAsync(ByVal uri As String) As Task(Of AngleSharp.Html.Dom.IHtmlDocument)
        Try
            Dim res As HttpResponseMessage = Await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
            If Not res.IsSuccessStatusCode Then Return Nothing
            Dim parser = New HtmlParser
            Dim doc = Await parser.ParseDocumentAsync(Await res.Content.ReadAsStreamAsync())
            Return doc
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    '================================================================================================================
End Class
