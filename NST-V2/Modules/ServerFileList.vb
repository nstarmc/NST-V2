Module ServerFileList
    Function get_server_domain()
        Return ("nstv2.nstarmc.cn")
    End Function

    Function get_server_domain_https()
        Return "https://" & get_server_domain()
    End Function

    Function get_json_main()
        Return get_server_domain_https() & "/main.json"
    End Function

    Function get_json_nstarmcpacks()
        Return get_server_domain_https() & "/packs/nstarmc.json"
    End Function
    Function get_json_javas()
        Return get_server_domain_https() & "/java.json"
    End Function
End Module
