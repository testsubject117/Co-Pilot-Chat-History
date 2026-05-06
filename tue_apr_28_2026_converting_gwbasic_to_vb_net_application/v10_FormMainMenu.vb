' ... inside HandleMenuKey(ch As String)

        Select Case up
            Case "A" : NotYet("Shop Card Generator")
            Case "B" : NotYet("Invoice Generator")
            Case "C"
                Using f As New FormLedgerMenu()
                    f.ShowDialog(Me)
                End Using
            Case "D" : NotYet("View Sales Journal (SALES)")
            Case "E" : NotYet("View Log Book (LOGBOOK)")
            Case "F" : NotYet("Price List Program (plist)")
            Case "G" : NotYet("Print/Void Invoices (BOOT)")
            Case "H" : NotYet("Quick Message Flashing")

            Case "I"
                Try
                    Cursor = Cursors.WaitCursor
                    MigrationService.EnsureFoldersAndMigrateOnce()
                    BackupService.RunBackupPriceListAndRolodex()
                    MessageBox.Show("Backup complete." & Environment.NewLine & AppPaths.BackupDir,
                                    "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show("Backup failed: " & ex.Message,
                                    "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    Cursor = Cursors.Default
                End Try

            Case "J" : NotYet("Print Out Customers Actual Names (spool real names)")
            Case "K" : NotYet("Cash Disbursements (BILL)")
            Case "L" : NotYet("Business Expenses Account (password)")
            Case "M" : NotYet("Quotation Form Generator (QUOTE)")
            Case "N" : NotYet("Rolodex (PHONE)")

            Case "O" : NotYet("Copy Spec Index")
            Case "P" : NotYet("Entire Ledger Viewing (ENTIRE)")
            Case "Q" : NotYet("Word Processor")
            Case "R" : NotYet("Find Word Processor Text")

            Case "T" : NotYet("Change Date or Time")
            Case "X" : NotYet("Typewriter Mode")
            Case "Y" : NotYet("Ed Dean's Personal Backup")

            Case "Z"
                Using f As New FormPersonalCalendar()
                    f.ShowDialog(Me)
                End Using

            Case "1"
                Using f As New FormMileageTracking()
                    f.ShowDialog(Me)
                End Using

            Case "2" : NotYet("Product Purchasing")
            Case "3" : NotYet("Miscellaneous Menu")
            Case "4" : NotYet("Add Entries to Log Book")
            Case "6" : NotYet("Cadmium Cards")
            Case "7" : NotYet("Emergency PAYROLL System")

            Case "?"
                Using f As New FormAbout()
                    f.ShowDialog(Me)
                End Using

            Case Else
                ' ignore unknown keys
        End Select