Private Sub MenuButton_Click(sender As Object, e As EventArgs)
    Dim btn = TryCast(sender, Button)
    Dim key = TryCast(btn?.Tag, String)

    Select Case key
        Case "C"
            Using f As New FormLedgerView()
                f.ShowDialog(Me)
            End Using

        Case "?"
            Using f As New FormAbout()
                f.ShowDialog(Me)
            End Using

        Case Else
            MessageBox.Show(Me, $"Menu item ({key}) not implemented yet.", "AMiOffice")
    End Select
End Sub