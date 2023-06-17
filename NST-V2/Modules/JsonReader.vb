Imports System.IO
Imports System.Net
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module JsonReader
    '读取本地版本信息函数
    Function read_localvinfo(text1 As String)
        Dim json As JObject = CType(JsonConvert.DeserializeObject(text1), JObject)
        log_record("JsonReader Module", "获取本地整合包信息成功！" & vbCrLf & text1)
        Return json
    End Function

    Function lc_json(text) As JObject
        Dim json As JObject = CType(JsonConvert.DeserializeObject(text.ToString), JObject)
        Return json
    End Function

    'main.json
    Function json_main()
        Try
            Dim request As HttpWebRequest = WebRequest.Create(get_json_main())
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim jsonback = sr.ReadToEnd '储存返回的json信息
            '处理json
            Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            log_record("Json-Reader Module", "获取json信息成功：" & get_json_main().ToString)
            Return json
        Catch ex As Exception
            log_record("Json-Reader Module", "获取json信息失败：" & get_json_main().ToString & vbCrLf & ex.ToString)
        End Try
    End Function

    'java.json
    Function json_java()
        Try
            Dim request As HttpWebRequest = WebRequest.Create(get_json_javas())
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim jsonback = sr.ReadToEnd '储存返回的json信息
            '处理json
            Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            log_record("Json-Reader Module", "获取json信息成功：" & get_json_javas().ToString)
            Return json
        Catch ex As Exception
            log_record("Json-Reader Module", "获取json信息失败：" & get_json_javas().ToString & vbCrLf & ex.ToString)
        End Try
    End Function

    'packs/nstarmc.json
    Function json_packs_nstarmc()
        Try
            Dim request As HttpWebRequest = WebRequest.Create(get_json_nstarmcpacks())
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim jsonback = sr.ReadToEnd '储存返回的json信息
            '处理json
            Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            log_record("Json-Reader Module", "获取json信息成功：" & get_json_nstarmcpacks().ToString)
            Return json
        Catch ex As Exception
            log_record("Json-Reader Module", "获取json信息失败：" & get_json_nstarmcpacks().ToString & vbCrLf & ex.ToString)
        End Try
    End Function

    'mcv
    Function json_mcv()
        Try
            Dim request As HttpWebRequest = WebRequest.Create("http://launchermeta.mojang.com/mc/game/version_manifest_v2.json")
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim jsonback = sr.ReadToEnd '储存返回的json信息
            '处理json
            Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            log_record("Json-Reader Module", "获取json信息成功：http://launchermeta.mojang.com/mc/game/version_manifest_v2.json")
            Return json
        Catch ex As Exception
            log_record("Json-Reader Module", "获取json信息失败：http://launchermeta.mojang.com/mc/game/version_manifest_v2.json")
        End Try
    End Function
End Module
