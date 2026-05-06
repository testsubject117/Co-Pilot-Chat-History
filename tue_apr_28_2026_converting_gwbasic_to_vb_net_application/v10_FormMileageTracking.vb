Private Sub EnsureMileageDatExists()
    If Not File.Exists(MileageDatPath) Then
        File.WriteAllText(MileageDatPath, "", Encoding.ASCII)
    End If
End Sub