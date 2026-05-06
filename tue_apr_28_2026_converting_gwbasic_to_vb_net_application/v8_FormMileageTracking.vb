Private Sub EnsureLegacyFilesExist()
    If Not File.Exists(MileageDatPath) Then
        File.WriteAllText(MileageDatPath, "", Encoding.ASCII)
    End If

    If Not File.Exists(OdometerNumPath) Then
        File.WriteAllText(OdometerNumPath, "0", Encoding.ASCII)
    End If
End Sub