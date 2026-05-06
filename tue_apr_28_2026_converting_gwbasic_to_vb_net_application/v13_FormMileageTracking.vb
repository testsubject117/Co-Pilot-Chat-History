Private Function EnsureDataDirExists() As Boolean
    Try
        If Directory.Exists(DataDir) Then Return True

        MessageBox.Show("Mileage data folder not found:" & Environment.NewLine &
                        DataDir & Environment.NewLine & Environment.NewLine &
                        "Fix the network path or connect to the share, then try again.",
                        "Mileage Tracking",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        Return False
    Catch ex As Exception
        MessageBox.Show("Unable to access mileage data folder:" & Environment.NewLine &
                        DataDir & Environment.NewLine & Environment.NewLine &
                        ex.Message,
                        "Mileage Tracking",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        Return False
    End Try
End Function