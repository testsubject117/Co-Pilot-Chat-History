AddMenuButton(p, "1", "Add Checks to ledger & cash receipts",
    Sub()
        Using f As New FormAddChecks()
            If f.ShowDialog(Me) = DialogResult.OK Then
                If f.SelectedCompany IsNot Nothing Then
                    MessageBox.Show(
                        "Company selected:" & Environment.NewLine &
                        f.SelectedCompany.CompanyCode & " - " & f.SelectedCompany.CompanyName,
                        "Add Checks",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
                End If
            End If
        End Using
    End Sub)