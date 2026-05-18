Case "C"
    Using f As New FormAddChecks()
        If f.ShowDialog(Me) = DialogResult.OK Then
            If f.SelectedCompany IsNot Nothing Then
                MessageBox.Show("Selected company: " &
                                f.SelectedCompany.CompanyCode & " - " &
                                f.SelectedCompany.CompanyName,
                                "Add Checks",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
            End If
        End If
    End Using